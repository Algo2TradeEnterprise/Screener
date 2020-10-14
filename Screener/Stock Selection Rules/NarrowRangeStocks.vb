Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class NarrowRangeStocks
    Inherits StockSelection

    Private ReadOnly _numberOfDays As Integer
    Private ReadOnly _checkDowntrendNR As Boolean

    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer,
                   ByVal numberOfDays As Integer,
                   ByVal chkDwnTrndNR As Boolean)
        MyBase.New(canceller, cmn, stockType)
        _numberOfDays = numberOfDays
        _checkDowntrendNR = chkDwnTrndNR
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
        ret.Columns.Add("Slab")
        ret.Columns.Add("Inside Bar")
        ret.Columns.Add("Direction")
        ret.Columns.Add("NR")

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
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_eodTable, runningStock, tradingDate.AddDays(Math.Max(_numberOfDays * 2, 300) * -1), tradingDate.AddDays(-1))
                        If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
                            Dim currentDayCandle As Payload = eodPayload.LastOrDefault.Value
                            'If currentDayCandle.PayloadDate.Date = tradingDate.Date Then
                            Dim lowestNumberOfDaysToCheck As Integer = _numberOfDays
                            If _checkDowntrendNR Then lowestNumberOfDaysToCheck = 1
                            For nmbrOfDaysCtr As Integer = _numberOfDays To lowestNumberOfDaysToCheck Step -1
                                If tempStockList Is Nothing OrElse Not tempStockList.ContainsKey(runningStock) Then
                                    Dim ctr As Integer = 0
                                    For Each runningPayload In eodPayload.OrderByDescending(Function(x)
                                                                                                Return x.Key
                                                                                            End Function)
                                        If runningPayload.Value.PayloadDate <> currentDayCandle.PayloadDate Then
                                            If currentDayCandle.CandleRange < runningPayload.Value.CandleRange Then
                                                ctr += 1
                                                If ctr >= nmbrOfDaysCtr - 1 Then
                                                    Dim insideBar As Boolean = currentDayCandle.High < currentDayCandle.PreviousCandlePayload.High AndAlso
                                                                            currentDayCandle.Low > currentDayCandle.PreviousCandlePayload.Low

                                                    Dim sma10Payloads As Dictionary(Of Date, Decimal) = Nothing
                                                    Indicator.SMA.CalculateSMA(10, Payload.PayloadFields.Close, eodPayload, sma10Payloads)
                                                    Dim sma50Payloads As Dictionary(Of Date, Decimal) = Nothing
                                                    Indicator.SMA.CalculateSMA(50, Payload.PayloadFields.Close, eodPayload, sma50Payloads)
                                                    Dim sma200Payloads As Dictionary(Of Date, Decimal) = Nothing
                                                    Indicator.SMA.CalculateSMA(200, Payload.PayloadFields.Close, eodPayload, sma200Payloads)

                                                    Dim direction As String = ""
                                                    If sma10Payloads(currentDayCandle.PayloadDate) > sma50Payloads(currentDayCandle.PayloadDate) AndAlso
                                                        sma50Payloads(currentDayCandle.PayloadDate) > sma200Payloads(currentDayCandle.PayloadDate) Then
                                                        direction = "BUY"
                                                    End If
                                                    If sma10Payloads(currentDayCandle.PayloadDate) < sma50Payloads(currentDayCandle.PayloadDate) AndAlso
                                                        sma50Payloads(currentDayCandle.PayloadDate) < sma200Payloads(currentDayCandle.PayloadDate) Then
                                                        direction = "SELL"
                                                    End If
                                                    If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                                    tempStockList.Add(runningStock, {insideBar, direction, nmbrOfDaysCtr})
                                                    Exit For
                                                End If
                                            Else
                                                Exit For
                                            End If
                                        End If
                                    Next
                                End If
                            Next
                            'End If
                        End If
                    Next
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
                            row("Slab") = atrStockList(runningStock.Key).Slab
                            row("Inside Bar") = runningStock.Value(0)
                            row("Direction") = runningStock.Value(1)
                            row("NR") = runningStock.Value(2)

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
