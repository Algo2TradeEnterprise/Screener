Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class TopGainerTopLosserEveryMinute
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
        ret.Columns.Add("Slab")
        ret.Columns.Add("Gain Loss %")
        ret.Columns.Add("Time")
        ret.Columns.Add("Remarks")

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
                    Dim startTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 19, 0)
                    Dim endTime As Date = startTime.AddHours(2)
                    Dim payloadTime As Date = startTime
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim stockData As Dictionary(Of String, Dictionary(Of Date, Payload)) = Nothing
                    For Each runningStock In atrStockList.Keys
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, runningStock, tradingDate, tradingDate)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                            If stockData Is Nothing Then stockData = New Dictionary(Of String, Dictionary(Of Date, Payload))
                            stockData.Add(runningStock, intradayPayload)
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
                                    Dim intradayPayload As Dictionary(Of Date, Payload) = stockData(runningStock)
                                    If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                                        Dim candleToCheck As Payload = Nothing
                                        If intradayPayload.ContainsKey(payloadTime) Then
                                            candleToCheck = intradayPayload(payloadTime)
                                        End If
                                        If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
                                            Dim previousClose As Decimal = atrStockList(runningStock).PreviousDayClose
                                            Dim gainLossPercentage As Decimal = ((candleToCheck.Close - previousClose) / previousClose) * 100
                                            If tempCloseStockList Is Nothing Then tempCloseStockList = New Dictionary(Of String, String())
                                            tempCloseStockList.Add(runningStock, {Math.Round(gainLossPercentage, 4), payloadTime.ToString("HH:mm:ss"), "Previous Close"})
                                        End If
                                        If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
                                            Dim currentOpen As Decimal = intradayPayload.FirstOrDefault.Value.Open
                                            Dim gainLossPercentage As Decimal = ((candleToCheck.Close - currentOpen) / currentOpen) * 100
                                            If tempOpenStockList Is Nothing Then tempOpenStockList = New Dictionary(Of String, String())
                                            tempOpenStockList.Add(runningStock, {Math.Round(gainLossPercentage, 4), payloadTime.ToString("HH:mm:ss"), "Current Open"})
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
                                row1("Blank Candle %") = atrStockList(topGainer.Key).BlankCandlePercentage
                                row1("Day ATR") = Math.Round(atrStockList(topGainer.Key).DayATR, 4)
                                row1("Previous Day Open") = atrStockList(topGainer.Key).PreviousDayOpen
                                row1("Previous Day Low") = atrStockList(topGainer.Key).PreviousDayLow
                                row1("Previous Day High") = atrStockList(topGainer.Key).PreviousDayHigh
                                row1("Previous Day Close") = atrStockList(topGainer.Key).PreviousDayClose
                                row1("Slab") = atrStockList(topGainer.Key).Slab
                                row1("Gain Loss %") = topGainer.Value(0)
                                row1("Time") = topGainer.Value(1)
                                row1("Remarks") = topGainer.Value(2)

                                ret.Rows.Add(row1)

                                Dim topLosser As KeyValuePair(Of String, String()) = tempCloseStockList.OrderByDescending(Function(x)
                                                                                                                              Return CDec(x.Value(0))
                                                                                                                          End Function).LastOrDefault
                                Dim row2 As DataRow = ret.NewRow
                                row2("Date") = tradingDate.ToString("dd-MM-yyyy")
                                row2("Trading Symbol") = atrStockList(topLosser.Key).TradingSymbol
                                row2("Lot Size") = atrStockList(topLosser.Key).LotSize
                                row2("ATR %") = Math.Round(atrStockList(topLosser.Key).ATRPercentage, 4)
                                row2("Blank Candle %") = atrStockList(topLosser.Key).BlankCandlePercentage
                                row2("Day ATR") = Math.Round(atrStockList(topLosser.Key).DayATR, 4)
                                row2("Previous Day Open") = atrStockList(topLosser.Key).PreviousDayOpen
                                row2("Previous Day Low") = atrStockList(topLosser.Key).PreviousDayLow
                                row2("Previous Day High") = atrStockList(topLosser.Key).PreviousDayHigh
                                row2("Previous Day Close") = atrStockList(topLosser.Key).PreviousDayClose
                                row2("Slab") = atrStockList(topLosser.Key).Slab
                                row2("Gain Loss %") = topLosser.Value(0)
                                row2("Time") = topLosser.Value(1)
                                row2("Remarks") = topLosser.Value(2)

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
                                row1("Blank Candle %") = atrStockList(topGainer.Key).BlankCandlePercentage
                                row1("Day ATR") = Math.Round(atrStockList(topGainer.Key).DayATR, 4)
                                row1("Previous Day Open") = atrStockList(topGainer.Key).PreviousDayOpen
                                row1("Previous Day Low") = atrStockList(topGainer.Key).PreviousDayLow
                                row1("Previous Day High") = atrStockList(topGainer.Key).PreviousDayHigh
                                row1("Previous Day Close") = atrStockList(topGainer.Key).PreviousDayClose
                                row1("Slab") = atrStockList(topGainer.Key).Slab
                                row1("Gain Loss %") = topGainer.Value(0)
                                row1("Time") = topGainer.Value(1)
                                row1("Remarks") = topGainer.Value(2)

                                ret.Rows.Add(row1)

                                Dim topLosser As KeyValuePair(Of String, String()) = tempOpenStockList.OrderByDescending(Function(x)
                                                                                                                             Return CDec(x.Value(0))
                                                                                                                         End Function).LastOrDefault
                                Dim row2 As DataRow = ret.NewRow
                                row2("Date") = tradingDate.ToString("dd-MM-yyyy")
                                row2("Trading Symbol") = atrStockList(topLosser.Key).TradingSymbol
                                row2("Lot Size") = atrStockList(topLosser.Key).LotSize
                                row2("ATR %") = Math.Round(atrStockList(topLosser.Key).ATRPercentage, 4)
                                row2("Blank Candle %") = atrStockList(topLosser.Key).BlankCandlePercentage
                                row2("Day ATR") = Math.Round(atrStockList(topLosser.Key).DayATR, 4)
                                row2("Previous Day Open") = atrStockList(topLosser.Key).PreviousDayOpen
                                row2("Previous Day Low") = atrStockList(topLosser.Key).PreviousDayLow
                                row2("Previous Day High") = atrStockList(topLosser.Key).PreviousDayHigh
                                row2("Previous Day Close") = atrStockList(topLosser.Key).PreviousDayClose
                                row2("Slab") = atrStockList(topLosser.Key).Slab
                                row2("Gain Loss %") = topLosser.Value(0)
                                row2("Time") = topLosser.Value(1)
                                row2("Remarks") = topLosser.Value(2)

                                ret.Rows.Add(row2)
                            End If

                            payloadTime = payloadTime.AddMinutes(1)
                        End While
                    End If
                End If

                tradingDate = tradingDate.AddDays(1)
            End While
        End Using
        Return ret
    End Function
End Class
