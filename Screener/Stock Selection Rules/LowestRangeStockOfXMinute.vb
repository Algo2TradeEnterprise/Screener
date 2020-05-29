Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class LowestRangeStockOfXMinute
    Inherits StockSelection

    Private ReadOnly _checkingTime As Date
    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer,
                   ByVal checkingTime As Date)
        MyBase.New(canceller, cmn, stockType)
        _checkingTime = checkingTime
    End Sub

    Public Overrides Async Function GetStockDataAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Lot Size")
        ret.Columns.Add("ATR %")
        ret.Columns.Add("Blank Candle %")
        ret.Columns.Add("Day ATR")
        ret.Columns.Add("Previous Day Open")
        ret.Columns.Add("Previous Day Low")
        ret.Columns.Add("Previous Day High")
        ret.Columns.Add("Previous Day Close")
        ret.Columns.Add("Current Day Close")
        ret.Columns.Add("Slab")
        ret.Columns.Add("Range %")
        ret.Columns.Add("ATR Range")
        ret.Columns.Add("Volume Per Range")
        ret.Columns.Add("Time")

        Using atrStock As New ATRStockSelection(_canceller)
            AddHandler atrStock.Heartbeat, AddressOf OnHeartbeat

            Dim tradingDate As Date = startDate
            While tradingDate <= endDate
                _bannedStockFileName = Path.Combine(My.Application.Info.DirectoryPath, String.Format("Bannned Stocks {0}.csv", tradingDate.ToString("ddMMyyyy")))
                For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "Bannned Stocks *.csv")
                    If Not runningFile.Contains(tradingDate.ToString("ddMMyyyy")) Then File.Delete(runningFile)
                Next
                Dim bannedStockList As List(Of String) = Nothing
                Using bannedStock As New BannedStockDataFetcher(_bannedStockFileName, _canceller)
                    AddHandler bannedStock.Heartbeat, AddressOf OnHeartbeat
                    bannedStockList = Await bannedStock.GetBannedStocksData(tradingDate).ConfigureAwait(False)
                End Using

                Dim atrStockList As Dictionary(Of String, InstrumentDetails) = Await atrStock.GetATRStockData(_eodTable, tradingDate, bannedStockList, False).ConfigureAwait(False)
                If atrStockList IsNot Nothing AndAlso atrStockList.Count > 0 Then
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim tempStockList As Dictionary(Of String, String()) = Nothing
                    Dim stockData As Dictionary(Of String, Dictionary(Of Date, Payload)) = Nothing
                    For Each runningStock In atrStockList.Keys
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, runningStock, tradingDate.AddDays(-8), tradingDate)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 100 Then
                            If stockData Is Nothing Then stockData = New Dictionary(Of String, Dictionary(Of Date, Payload))
                            stockData.Add(runningStock, intradayPayload)
                        End If
                    Next
                    If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
                        Dim checkingTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, _checkingTime.Hour, _checkingTime.Minute, 0)

                        For Each runningStock In atrStockList.Keys
                            If stockData.ContainsKey(runningStock) AndAlso stockData(runningStock).ContainsKey(checkingTime) Then
                                Dim checkPayload As Payload = stockData(runningStock)(checkingTime)
                                If checkPayload IsNot Nothing Then
                                    Dim highestATR As Decimal = Decimal.MinValue
                                    Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                                    Indicator.ATR.CalculateATR(14, stockData(runningStock), atrPayload)
                                    If atrPayload IsNot Nothing AndAlso atrPayload.Count > 0 Then
                                        Dim firstCandle As Payload = Nothing
                                        For Each runningPayload In stockData(runningStock)
                                            If runningPayload.Key.Date = tradingDate.Date Then
                                                If runningPayload.Value.PreviousCandlePayload.PayloadDate.Date <> tradingDate.Date Then
                                                    firstCandle = runningPayload.Value
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                        If firstCandle IsNot Nothing AndAlso firstCandle.PreviousCandlePayload IsNot Nothing Then
                                            highestATR = atrPayload.Max(Function(x)
                                                                            If x.Key.Date = firstCandle.PreviousCandlePayload.PayloadDate.Date Then
                                                                                Return x.Value
                                                                            Else
                                                                                Return Decimal.MinValue
                                                                            End If
                                                                        End Function)
                                        End If
                                    End If
                                    If highestATR <> Decimal.MinValue Then
                                        Dim range As Decimal = ((checkPayload.High - checkPayload.Low) / checkPayload.Close) * 100
                                        Dim atrRange As Decimal = ((checkPayload.High - checkPayload.Low) / highestATR)
                                        Dim volumePerRange As Decimal = checkPayload.Volume / (checkPayload.High - checkPayload.Low)

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock, {Math.Round(range, 4), Math.Round(atrRange, 4), Math.Round(volumePerRange, 4), checkingTime.ToString("HH:mm:ss")})
                                    End If
                                End If
                            End If
                        Next
                    End If
                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList.OrderByDescending(Function(x)
                                                                                     Return CDec(x.Value(2))
                                                                                 End Function)
                            _canceller.Token.ThrowIfCancellationRequested()
                            Dim row As DataRow = ret.NewRow
                            row("Date") = tradingDate.ToString("dd-MM-yyyy")
                            row("Trading Symbol") = atrStockList(runningStock.Key).TradingSymbol
                            row("Lot Size") = atrStockList(runningStock.Key).LotSize
                            row("ATR %") = Math.Round(atrStockList(runningStock.Key).ATRPercentage, 4)
                            row("Blank Candle %") = atrStockList(runningStock.Key).BlankCandlePercentage
                            row("Day ATR") = Math.Round(atrStockList(runningStock.Key).DayATR, 4)
                            row("Previous Day Open") = atrStockList(runningStock.Key).PreviousDayOpen
                            row("Previous Day Low") = atrStockList(runningStock.Key).PreviousDayLow
                            row("Previous Day High") = atrStockList(runningStock.Key).PreviousDayHigh
                            row("Previous Day Close") = atrStockList(runningStock.Key).PreviousDayClose
                            row("Current Day Close") = atrStockList(runningStock.Key).CurrentDayClose
                            row("Slab") = atrStockList(runningStock.Key).Slab
                            row("Range %") = runningStock.Value(0)
                            row("ATR Range") = runningStock.Value(1)
                            row("Volume Per Range") = runningStock.Value(2)
                            row("Time") = runningStock.Value(3)

                            ret.Rows.Add(row)
                            stockCounter += 1
                            If stockCounter = My.Settings.NumberOfStockPerDay Then Exit For
                        Next
                    End If
                End If

                tradingDate = tradingDate.AddDays(1)
            End While
        End Using
        Return ret
    End Function
End Class