Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class TopGainerTopLosserEveryMinute
    Inherits StockSelection

    Private ReadOnly _timeframe As Integer = 15

    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer)
        MyBase.New(canceller, cmn, stockType)
    End Sub

    Public Overrides Async Function GetStockDataAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Time")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Lot Size")
        ret.Columns.Add("ATR %")
        ret.Columns.Add("Avg Volume")
        ret.Columns.Add("Blank Candle %")
        ret.Columns.Add("Day ATR")
        'ret.Columns.Add("Previous Day Open")
        'ret.Columns.Add("Previous Day Low")
        'ret.Columns.Add("Previous Day High")
        'ret.Columns.Add("Previous Day Close")
        ret.Columns.Add("Slab")
        ret.Columns.Add("Gain Loss %")
        ret.Columns.Add("Remarks")
        ret.Columns.Add("Next Open")
        ret.Columns.Add("Next Close")

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
                    Dim startTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0)
                    Dim endTime As Date = startTime.AddHours(5)
                    Dim payloadTime As Date = startTime
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim stockData As Dictionary(Of String, Dictionary(Of Date, Payload)) = Nothing
                    For Each runningStock In atrStockList.Keys
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, runningStock, tradingDate.AddDays(-8), tradingDate)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                            If stockData Is Nothing Then stockData = New Dictionary(Of String, Dictionary(Of Date, Payload))
                            stockData.Add(runningStock, Common.ConvertPayloadsToXMinutes(intradayPayload, _timeframe, startTime))
                        End If
                    Next
                    Dim topGainerLosserStockList As Dictionary(Of String, String()) = Nothing
                    If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
                        While payloadTime <= endTime
                            Dim tempCloseStockList As Dictionary(Of String, String()) = Nothing
                            Dim tempOpenStockList As Dictionary(Of String, String()) = Nothing
                            For Each runningStock In atrStockList.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                If stockData.ContainsKey(runningStock) Then
                                    If stockData(runningStock) IsNot Nothing AndAlso stockData(runningStock).Count > 0 Then
                                        If stockData(runningStock).ContainsKey(payloadTime) Then
                                            Dim candleToCheck As Payload = stockData(runningStock)(payloadTime)
                                            Dim nextCandle As Payload = stockData(runningStock).Where(Function(x)
                                                                                                          Return x.Key = candleToCheck.PayloadDate
                                                                                                      End Function).OrderBy(Function(y)
                                                                                                                                Return y.Key
                                                                                                                            End Function).FirstOrDefault.Value

                                            If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
                                                Dim closeGainLossPercentage As Decimal = ((candleToCheck.Close - candleToCheck.PreviousCandlePayload.Close) / candleToCheck.PreviousCandlePayload.Close) * 100
                                                If tempCloseStockList Is Nothing Then tempCloseStockList = New Dictionary(Of String, String())
                                                tempCloseStockList.Add(runningStock, {Math.Round(closeGainLossPercentage, 4), payloadTime.ToString("HH:mm:ss"), "Previous Close", nextCandle.Open, nextCandle.Close})

                                                Dim openGainLossPercentage As Decimal = ((candleToCheck.Close - candleToCheck.Open) / candleToCheck.Open) * 100
                                                If tempOpenStockList Is Nothing Then tempOpenStockList = New Dictionary(Of String, String())
                                                tempOpenStockList.Add(runningStock, {Math.Round(openGainLossPercentage, 4), payloadTime.ToString("HH:mm:ss"), "Current Open", nextCandle.Open, nextCandle.Close})
                                            End If
                                        End If
                                    End If
                                End If
                            Next
                            If tempCloseStockList IsNot Nothing AndAlso tempCloseStockList.Count > 0 Then
                                Dim topGainer As KeyValuePair(Of String, String()) = tempCloseStockList.OrderByDescending(Function(x)
                                                                                                                              Return CDec(x.Value(0))
                                                                                                                          End Function).FirstOrDefault
                                Dim row1 As DataRow = ret.NewRow
                                row1("Date") = tradingDate.ToString("dd-MM-yyyy")
                                row1("Trading Symbol") = atrStockList(topGainer.Key).TradingSymbol
                                row1("Lot Size") = atrStockList(topGainer.Key).LotSize
                                row1("ATR %") = Math.Round(atrStockList(topGainer.Key).ATRPercentage, 4)
                                row1("Avg Volume") = Math.Round(atrStockList(topGainer.Key).AverageVolume, 4)
                                row1("Blank Candle %") = atrStockList(topGainer.Key).BlankCandlePercentage
                                row1("Day ATR") = Math.Round(atrStockList(topGainer.Key).DayATR, 4)
                                'row1("Previous Day Open") = atrStockList(topGainer.Key).PreviousDayOpen
                                'row1("Previous Day Low") = atrStockList(topGainer.Key).PreviousDayLow
                                'row1("Previous Day High") = atrStockList(topGainer.Key).PreviousDayHigh
                                'row1("Previous Day Close") = atrStockList(topGainer.Key).PreviousDayClose
                                row1("Slab") = atrStockList(topGainer.Key).Slab
                                row1("Gain Loss %") = topGainer.Value(0)
                                row1("Time") = topGainer.Value(1)
                                row1("Remarks") = topGainer.Value(2)
                                row1("Next Open") = topGainer.Value(3)
                                row1("Next Close") = topGainer.Value(4)

                                ret.Rows.Add(row1)

                                Dim topLosser As KeyValuePair(Of String, String()) = tempCloseStockList.OrderByDescending(Function(x)
                                                                                                                              Return CDec(x.Value(0))
                                                                                                                          End Function).LastOrDefault
                                Dim row2 As DataRow = ret.NewRow
                                row2("Date") = tradingDate.ToString("dd-MM-yyyy")
                                row2("Trading Symbol") = atrStockList(topLosser.Key).TradingSymbol
                                row2("Lot Size") = atrStockList(topLosser.Key).LotSize
                                row2("ATR %") = Math.Round(atrStockList(topLosser.Key).ATRPercentage, 4)
                                row2("Avg Volume") = Math.Round(atrStockList(topLosser.Key).AverageVolume, 4)
                                row2("Blank Candle %") = atrStockList(topLosser.Key).BlankCandlePercentage
                                row2("Day ATR") = Math.Round(atrStockList(topLosser.Key).DayATR, 4)
                                'row2("Previous Day Open") = atrStockList(topLosser.Key).PreviousDayOpen
                                'row2("Previous Day Low") = atrStockList(topLosser.Key).PreviousDayLow
                                'row2("Previous Day High") = atrStockList(topLosser.Key).PreviousDayHigh
                                'row2("Previous Day Close") = atrStockList(topLosser.Key).PreviousDayClose
                                row2("Slab") = atrStockList(topLosser.Key).Slab
                                row2("Gain Loss %") = topLosser.Value(0)
                                row2("Time") = topLosser.Value(1)
                                row2("Remarks") = topLosser.Value(2)
                                row2("Next Open") = topLosser.Value(3)
                                row2("Next Close") = topLosser.Value(4)

                                ret.Rows.Add(row2)
                            End If
                            If tempOpenStockList IsNot Nothing AndAlso tempOpenStockList.Count > 0 Then
                                Dim topGainer As KeyValuePair(Of String, String()) = tempOpenStockList.OrderByDescending(Function(x)
                                                                                                                             Return CDec(x.Value(0))
                                                                                                                         End Function).FirstOrDefault
                                Dim row1 As DataRow = ret.NewRow
                                row1("Date") = tradingDate.ToString("dd-MM-yyyy")
                                row1("Trading Symbol") = atrStockList(topGainer.Key).TradingSymbol
                                row1("Lot Size") = atrStockList(topGainer.Key).LotSize
                                row1("ATR %") = Math.Round(atrStockList(topGainer.Key).ATRPercentage, 4)
                                row1("Avg Volume") = Math.Round(atrStockList(topGainer.Key).AverageVolume, 4)
                                row1("Blank Candle %") = atrStockList(topGainer.Key).BlankCandlePercentage
                                row1("Day ATR") = Math.Round(atrStockList(topGainer.Key).DayATR, 4)
                                'row1("Previous Day Open") = atrStockList(topGainer.Key).PreviousDayOpen
                                'row1("Previous Day Low") = atrStockList(topGainer.Key).PreviousDayLow
                                'row1("Previous Day High") = atrStockList(topGainer.Key).PreviousDayHigh
                                'row1("Previous Day Close") = atrStockList(topGainer.Key).PreviousDayClose
                                row1("Slab") = atrStockList(topGainer.Key).Slab
                                row1("Gain Loss %") = topGainer.Value(0)
                                row1("Time") = topGainer.Value(1)
                                row1("Remarks") = topGainer.Value(2)
                                row1("Next Open") = topGainer.Value(3)
                                row1("Next Close") = topGainer.Value(4)

                                ret.Rows.Add(row1)

                                Dim topLosser As KeyValuePair(Of String, String()) = tempOpenStockList.OrderByDescending(Function(x)
                                                                                                                             Return CDec(x.Value(0))
                                                                                                                         End Function).LastOrDefault
                                Dim row2 As DataRow = ret.NewRow
                                row2("Date") = tradingDate.ToString("dd-MM-yyyy")
                                row2("Trading Symbol") = atrStockList(topLosser.Key).TradingSymbol
                                row2("Lot Size") = atrStockList(topLosser.Key).LotSize
                                row2("ATR %") = Math.Round(atrStockList(topLosser.Key).ATRPercentage, 4)
                                row2("Avg Volume") = Math.Round(atrStockList(topLosser.Key).AverageVolume, 4)
                                row2("Blank Candle %") = atrStockList(topLosser.Key).BlankCandlePercentage
                                row2("Day ATR") = Math.Round(atrStockList(topLosser.Key).DayATR, 4)
                                'row2("Previous Day Open") = atrStockList(topLosser.Key).PreviousDayOpen
                                'row2("Previous Day Low") = atrStockList(topLosser.Key).PreviousDayLow
                                'row2("Previous Day High") = atrStockList(topLosser.Key).PreviousDayHigh
                                'row2("Previous Day Close") = atrStockList(topLosser.Key).PreviousDayClose
                                row2("Slab") = atrStockList(topLosser.Key).Slab
                                row2("Gain Loss %") = topLosser.Value(0)
                                row2("Time") = topLosser.Value(1)
                                row2("Remarks") = topLosser.Value(2)
                                row2("Next Open") = topLosser.Value(3)
                                row2("Next Close") = topLosser.Value(4)

                                ret.Rows.Add(row2)
                            End If

                            payloadTime = payloadTime.AddMinutes(_timeframe)
                        End While
                    End If
                End If

                tradingDate = tradingDate.AddDays(1)
            End While
        End Using
        Return ret
    End Function
End Class
