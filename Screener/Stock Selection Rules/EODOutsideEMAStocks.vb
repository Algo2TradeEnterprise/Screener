Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class EODOutsideEMAStocks
    Inherits StockSelection

    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer)
        MyBase.New(canceller, cmn, stockType)
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
        ret.Columns.Add("Direction")
        ret.Columns.Add("Hammer Candle Time")

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
                    For Each runningStock In atrStockList.Keys
                        Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_eodTable, runningStock, tradingDate.AddDays(-200), tradingDate)
                        If eodPayload IsNot Nothing AndAlso eodPayload.Count > 30 AndAlso eodPayload.ContainsKey(tradingDate.Date) Then
                            Dim emaPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.EMA.CalculateEMA(20, Payload.PayloadFields.Close, eodPayload, emaPayload)

                            Dim direction As String = Nothing
                            Dim currentDayCandle As Payload = eodPayload(tradingDate.Date)
                            If currentDayCandle.PreviousCandlePayload.Low > emaPayload(currentDayCandle.PreviousCandlePayload.PayloadDate) Then
                                direction = "BUY"
                            ElseIf currentDayCandle.PreviousCandlePayload.High < emaPayload(currentDayCandle.PreviousCandlePayload.PayloadDate) Then
                                direction = "SELL"
                            End If
                            If direction IsNot Nothing AndAlso direction.Trim <> "" Then
                                Dim intrdayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, runningStock, tradingDate, tradingDate)
                                If intrdayPayload IsNot Nothing AndAlso intrdayPayload.Count > 0 Then
                                    Dim exchangeStartTime As Date = New Date(Now.Year, Now.Month, Now.Day, 9, 15, 0)
                                    Dim xMinPayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(intrdayPayload, 15, exchangeStartTime)
                                    If xMinPayload IsNot Nothing AndAlso xMinPayload.Count > 0 Then
                                        Dim hammerCandleTime As Date = Date.MinValue
                                        For Each runningPayload In xMinPayload
                                            If direction.ToUpper = "BUY" Then
                                                If runningPayload.Value.CandleColor = Color.Green Then
                                                    If runningPayload.Value.CandleWicks.Top > 0 AndAlso
                                                        runningPayload.Value.CandleWicks.Bottom > runningPayload.Value.CandleWicks.Top Then
                                                        If runningPayload.Value.CandleBody <= runningPayload.Value.CandleRange * 0.25 Then
                                                            If runningPayload.Value.Open >= runningPayload.Value.Low + runningPayload.Value.CandleRange * 0.6 Then
                                                                hammerCandleTime = runningPayload.Key
                                                                Exit For
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            ElseIf direction.ToUpper = "SELL" Then
                                                If runningPayload.Value.CandleColor = Color.Red Then
                                                    If runningPayload.Value.CandleWicks.Bottom > 0 AndAlso
                                                        runningPayload.Value.CandleWicks.Bottom < runningPayload.Value.CandleWicks.Top Then
                                                        If runningPayload.Value.CandleBody <= runningPayload.Value.CandleRange * 0.25 Then
                                                            If runningPayload.Value.Open <= runningPayload.Value.High - runningPayload.Value.CandleRange * 0.6 Then
                                                                hammerCandleTime = runningPayload.Key
                                                                Exit For
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Next
                                        If hammerCandleTime <> Date.MinValue Then
                                            If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                            tempStockList.Add(runningStock, {direction, hammerCandleTime.ToString("dd-MM-yyyy HH:mm:ss")})
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next
                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList.OrderBy(Function(x)
                                                                           Return Date.ParseExact(x.Value(1), "dd-MM-yyyy HH:mm:ss", Nothing)
                                                                       End Function).ThenByDescending(Function(y)
                                                                                                          Return atrStockList(y.Key).ATRPercentage
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
                            row("Direction") = runningStock.Value(0)
                            row("Hammer Candle Time") = runningStock.Value(1)

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