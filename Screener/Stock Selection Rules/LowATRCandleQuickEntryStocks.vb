Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class LowATRCandleQuickEntryStocks
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
                    Dim tempStockList As Dictionary(Of String, Date) = Nothing
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
                        Dim startTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 16, 0)
                        Dim endTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 10, 0, 0)

                        For Each runningStock In atrStockList.Keys
                            If stockData.ContainsKey(runningStock) Then
                                Dim signalPayload As Dictionary(Of Date, Payload) = stockData(runningStock)
                                Dim potentialBuyEntryPrice As Decimal = Decimal.MinValue
                                Dim potentialSellEntryPrice As Decimal = Decimal.MinValue
                                Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                                Indicator.ATR.CalculateATR(14, signalPayload, atrPayload)
                                For Each runningPayload In signalPayload
                                    If runningPayload.Key >= startTime AndAlso runningPayload.Key <= endTime Then
                                        Dim lowestATR As Decimal = Utilities.Numbers.ConvertFloorCeling(GetLowestATR(atrPayload, runningPayload.Value), 0.05, Utilities.Numbers.NumberManipulation.RoundOfType.Celing)
                                        If runningPayload.Value.CandleRange >= lowestATR * 0.5 AndAlso runningPayload.Value.CandleRange < lowestATR Then
                                            Dim buffer As Decimal = CalculateBuffer(runningPayload.Value.Close, Utilities.Numbers.NumberManipulation.RoundOfType.Floor)
                                            potentialBuyEntryPrice = runningPayload.Value.High + buffer
                                            potentialSellEntryPrice = runningPayload.Value.Low - buffer
                                        End If
                                    End If
                                    If potentialBuyEntryPrice <> Decimal.MinValue AndAlso potentialSellEntryPrice <> Decimal.MinValue Then
                                        If runningPayload.Value.High >= potentialBuyEntryPrice OrElse runningPayload.Value.Low <= potentialSellEntryPrice Then
                                            Dim entryTime As Date = Date.MinValue
                                            For Each runningTick In runningPayload.Value.Ticks
                                                If runningTick.High >= potentialBuyEntryPrice OrElse runningTick.Low <= potentialSellEntryPrice Then
                                                    entryTime = runningTick.PayloadDate
                                                    Exit For
                                                End If
                                            Next

                                            If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Date)
                                            tempStockList.Add(runningStock, entryTime)
                                            Exit For
                                        End If
                                    End If
                                Next
                            End If
                        Next
                    End If

                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList.OrderBy(Function(x)
                                                                           Return x.Value
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
                            row("Time") = runningStock.Value.ToString("HH:mm:ss")

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

    Private Function GetLowestATR(ByVal atrPayload As Dictionary(Of Date, Decimal), ByVal signalCandle As Payload) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If atrPayload IsNot Nothing AndAlso atrPayload.Count > 0 Then
            ret = atrPayload.Min(Function(x)
                                     If x.Key.Date = signalCandle.PayloadDate.Date AndAlso x.Key <= signalCandle.PayloadDate Then
                                         Return x.Value
                                     Else
                                         Return Decimal.MaxValue
                                     End If
                                 End Function)
        End If
        Return ret
    End Function
End Class