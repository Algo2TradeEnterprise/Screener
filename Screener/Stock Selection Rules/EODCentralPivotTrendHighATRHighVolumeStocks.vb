Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class EODCentralPivotTrendHighATRHighVolumeStocks
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
        ret.Columns.Add("Volume Per Price")
        ret.Columns.Add("Target Left %")
        ret.Columns.Add("Direction")

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
                    Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
                    For Each runningStock In atrStockList
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_eodTable, runningStock.Value.TradingSymbol, tradingDate.AddDays(-300), tradingDate)
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_intradayTable, runningStock.Value.TradingSymbol, tradingDate.AddDays(-300), tradingDate)
                        If eodPayload IsNot Nothing AndAlso eodPayload.Count > 100 AndAlso eodPayload.ContainsKey(tradingDate.Date) AndAlso
                            intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                            Dim candle As Payload = eodPayload(tradingDate.Date)
                            If candle.Volume >= 1000000 Then
                                Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                                Dim pivotPayload As Dictionary(Of Date, PivotPoints) = Nothing
                                Indicator.ATR.CalculateATR(14, eodPayload, atrPayload)
                                CalculatePivotPoints(eodPayload, intradayPayload, pivotPayload)

                                Dim trendRolloverDate As Date = Date.MinValue
                                Dim direction As Integer = 0
                                Dim pivot As PivotPoints = pivotPayload(tradingDate.Date)
                                Dim previousPivot As PivotPoints = pivotPayload(eodPayload(tradingDate.Date).PreviousCandlePayload.PayloadDate)
                                If pivot.Pivot > previousPivot.Pivot Then
                                    If pivot.Pivot > previousPivot.Resistance1 Then
                                        trendRolloverDate = tradingDate.Date
                                        direction = 1
                                    Else
                                        Dim rolloverDay As Date = GetRolloverDay(tradingDate, eodPayload, pivotPayload, 1)
                                        If rolloverDay <> Date.MinValue Then
                                            trendRolloverDate = rolloverDay.Date
                                            direction = 1
                                        End If
                                    End If
                                ElseIf pivot.Pivot < previousPivot.Pivot Then
                                    If pivot.Pivot < previousPivot.Support1 Then
                                        trendRolloverDate = tradingDate.Date
                                        direction = -1
                                    Else
                                        Dim rolloverDay As Date = GetRolloverDay(tradingDate, eodPayload, pivotPayload, -1)
                                        If rolloverDay <> Date.MinValue Then
                                            trendRolloverDate = rolloverDay.Date
                                            direction = -1
                                        End If
                                    End If
                                End If

                                If trendRolloverDate <> Date.MinValue AndAlso direction <> 0 Then
                                    Dim targetReached As Boolean = True
                                    Dim targetLeftPercentage As Decimal = 0
                                    If direction = 1 Then
                                        Dim highestHigh As Decimal = eodPayload.Max(Function(x)
                                                                                        If x.Key > trendRolloverDate AndAlso x.Key <= tradingDate Then
                                                                                            Return x.Value.High
                                                                                        Else
                                                                                            Return Decimal.MinValue
                                                                                        End If
                                                                                    End Function)
                                        Dim atr As Decimal = atrPayload(trendRolloverDate)
                                        If highestHigh < eodPayload(trendRolloverDate).Close + atr Then
                                            targetReached = False
                                            If highestHigh <> Decimal.MinValue Then
                                                targetLeftPercentage = ((atr - (highestHigh - eodPayload(trendRolloverDate).Close)) / atr) * 100
                                            Else
                                                targetLeftPercentage = 100
                                            End If
                                        End If
                                    ElseIf direction = -1 Then
                                        Dim lowestLow As Decimal = eodPayload.Min(Function(x)
                                                                                      If x.Key > trendRolloverDate AndAlso x.Key <= tradingDate Then
                                                                                          Return x.Value.Low
                                                                                      Else
                                                                                          Return Decimal.MaxValue
                                                                                      End If
                                                                                  End Function)
                                        Dim atr As Decimal = atrPayload(trendRolloverDate)
                                        If lowestLow > eodPayload(trendRolloverDate).Close - atr Then
                                            targetReached = False
                                            If lowestLow <> Decimal.MaxValue Then
                                                targetLeftPercentage = ((atr - (eodPayload(trendRolloverDate).Close - lowestLow)) / atr) * 100
                                            Else
                                                targetLeftPercentage = 100
                                            End If
                                        End If
                                    End If

                                    If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
                                    tempStockList.Add(runningStock.Key, {Math.Ceiling(candle.Volume / candle.Close), Math.Round(targetLeftPercentage, 2), direction})
                                End If
                            End If
                        End If
                    Next

                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList.OrderByDescending(Function(x)
                                                                                     Return x.Value(0)
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
                            row("Volume Per Price") = runningStock.Value(0)
                            row("Target Left %") = runningStock.Value(1)
                            row("Direction") = If(runningStock.Value(2) = 1, "BUY", "SELL")

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

    Private Function GetRolloverDay(ByVal currentTime As Date, ByVal eodPayload As Dictionary(Of Date, Payload),
                                    ByVal pivotTrendPayload As Dictionary(Of Date, PivotPoints), ByVal direction As Integer) As Date
        Dim ret As Date = Date.MinValue
        For Each runningPayload In eodPayload.OrderByDescending(Function(x)
                                                                    Return x.Key
                                                                End Function)
            If runningPayload.Value.PreviousCandlePayload IsNot Nothing AndAlso
                runningPayload.Value.PreviousCandlePayload.PayloadDate < currentTime Then
                Dim pivot As PivotPoints = pivotTrendPayload(runningPayload.Value.PayloadDate)
                Dim previousPivot As PivotPoints = pivotTrendPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate)
                If direction > 0 Then
                    If pivot.Pivot > previousPivot.Resistance1 Then
                        ret = runningPayload.Key
                        Exit For
                    ElseIf pivot.Pivot < previousPivot.Pivot Then
                        Exit For
                    End If
                ElseIf direction < 0 Then
                    If pivot.Pivot < previousPivot.Support1 Then
                        ret = runningPayload.Key
                        Exit For
                    ElseIf pivot.Pivot > previousPivot.Pivot Then
                        Exit For
                    End If
                End If
            End If
        Next
        Return ret
    End Function

#Region "Pivot Points Trend Calculation"
    Private Sub CalculatePivotPoints(ByVal inputPayload As Dictionary(Of Date, Payload), ByVal minPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, PivotPoints))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            For Each runningPayload In inputPayload
                Dim pivotPointsData As PivotPoints = New PivotPoints
                Dim curHigh As Decimal = minPayload.Max(Function(x)
                                                            If x.Key.Date = runningPayload.Key.Date Then
                                                                Return x.Value.High
                                                            Else
                                                                Return Decimal.MinValue
                                                            End If
                                                        End Function)
                If curHigh <> Decimal.MinValue Then
                    Dim curLow As Decimal = minPayload.Min(Function(x)
                                                               If x.Key.Date = runningPayload.Key.Date Then
                                                                   Return x.Value.Low
                                                               Else
                                                                   Return Decimal.MaxValue
                                                               End If
                                                           End Function)
                    Dim curClose As Decimal = minPayload.Where(Function(x)
                                                                   Return x.Key.Date = runningPayload.Key.Date
                                                               End Function).LastOrDefault.Value.Close

                    pivotPointsData.Pivot = (curHigh + curLow + curClose) / 3
                    pivotPointsData.Support1 = (2 * pivotPointsData.Pivot) - curHigh
                    pivotPointsData.Resistance1 = (2 * pivotPointsData.Pivot) - curLow
                    pivotPointsData.Support2 = pivotPointsData.Pivot - (curHigh - curLow)
                    pivotPointsData.Resistance2 = pivotPointsData.Pivot + (curHigh - curLow)
                    pivotPointsData.Support3 = pivotPointsData.Support2 - (curHigh - curLow)
                    pivotPointsData.Resistance3 = pivotPointsData.Resistance2 + (curHigh - curLow)

                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, PivotPoints)
                    outputPayload.Add(runningPayload.Key, pivotPointsData)
                End If
            Next
        End If
    End Sub
#End Region
End Class