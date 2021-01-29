Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class HighATRHighVolumeStocks
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
                        If eodPayload IsNot Nothing AndAlso eodPayload.Count > 100 AndAlso eodPayload.ContainsKey(tradingDate.Date) Then
                            Dim candle As Payload = eodPayload(tradingDate.Date)
                            If candle.Volume >= 1000000 Then
                                Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                                Dim pivotTrendPayload As Dictionary(Of Date, Color) = Nothing
                                Indicator.ATR.CalculateATR(14, eodPayload, atrPayload)
                                Indicator.PivotHighLow.CalculatePivotHighLowTrend(4, 3, eodPayload, Nothing, Nothing, pivotTrendPayload)

                                Dim trendRolloverDate As Date = Date.MinValue
                                Dim direction As Integer = 0
                                Dim trend As Color = pivotTrendPayload(tradingDate.Date)
                                Dim previousTrend As Color = pivotTrendPayload(eodPayload(tradingDate.Date).PreviousCandlePayload.PayloadDate)
                                If trend = Color.Green Then
                                    If previousTrend = Color.Red Then
                                        trendRolloverDate = tradingDate.Date
                                        direction = 1
                                    Else
                                        Dim rolloverDay As Date = GetRolloverDay(trend, eodPayload, pivotTrendPayload)
                                        If rolloverDay <> Date.MinValue Then
                                            trendRolloverDate = rolloverDay.Date
                                            direction = 1
                                        End If
                                    End If
                                ElseIf trend = Color.Red Then
                                    If previousTrend = Color.Green Then
                                        trendRolloverDate = tradingDate.Date
                                        direction = -1
                                    Else
                                        Dim rolloverDay As Date = GetRolloverDay(trend, eodPayload, pivotTrendPayload)
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
                                                targetLeftPercentage = ((highestHigh - eodPayload(trendRolloverDate).Close) / atr) * 100
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
                                                targetLeftPercentage = ((eodPayload(trendRolloverDate).Close - lowestLow) / atr) * 100
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

    Private Function GetRolloverDay(ByVal currentTrend As Color,
                                    ByVal eodPayload As Dictionary(Of Date, Payload),
                                    ByVal pivotTrendPayload As Dictionary(Of Date, Color)) As Date
        Dim ret As Date = Date.MinValue
        For Each runningPayload In eodPayload.OrderByDescending(Function(x)
                                                                    Return x.Key
                                                                End Function)
            If runningPayload.Value.PreviousCandlePayload IsNot Nothing Then
                Dim trend As Color = pivotTrendPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate)
                If trend <> currentTrend Then
                    ret = runningPayload.Key
                    Exit For
                End If
            End If
        Next
        Return ret
    End Function
End Class