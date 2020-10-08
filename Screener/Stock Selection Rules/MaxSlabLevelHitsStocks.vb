Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class MaxSlabLevelHitsStocks
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
        ret.Columns.Add("Max Level")
        ret.Columns.Add("Max Level Hits")
        ret.Columns.Add("Open Upper Level")
        ret.Columns.Add("Open Upper Level Hits")
        ret.Columns.Add("Open Lower Level")
        ret.Columns.Add("Open Lower Level Hits")

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
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, runningStock, tradingDate.AddDays(-10), tradingDate)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                            Dim inputPayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(intradayPayload, 5, New Date(2020, 10, 15, 9, 15, 0))

                            Dim currentdayPayload As Dictionary(Of Date, Payload) = Nothing
                            For Each runningPayload In inputPayload
                                If runningPayload.Key.Date = tradingDate.Date Then
                                    If currentdayPayload Is Nothing Then currentdayPayload = New Dictionary(Of Date, Payload)
                                    currentdayPayload.Add(runningPayload.Key, runningPayload.Value)
                                End If
                            Next
                            If currentdayPayload IsNot Nothing AndAlso currentdayPayload.Count > 0 Then
                                Dim previousTradingDay As Date = currentdayPayload.FirstOrDefault.Value.PreviousCandlePayload.PayloadDate
                                Dim previousDayHigh As Decimal = inputPayload.Max(Function(x)
                                                                                      If x.Key.Date = previousTradingDay.Date Then
                                                                                          Return x.Value.High
                                                                                      Else
                                                                                          Return Decimal.MinValue
                                                                                      End If
                                                                                  End Function)
                                Dim previousDayLow As Decimal = inputPayload.Min(Function(x)
                                                                                     If x.Key.Date = previousTradingDay.Date Then
                                                                                         Return x.Value.Low
                                                                                     Else
                                                                                         Return Decimal.MaxValue
                                                                                     End If
                                                                                 End Function)

                                Dim slab As Decimal = atrStockList(runningStock).Slab
                                Dim firstSlabAboveLow As Decimal = GetSlabBasedLevel(previousDayLow, 1, slab)
                                Dim slabData As Dictionary(Of Decimal, Integer) = New Dictionary(Of Decimal, Integer) From {{firstSlabAboveLow, 0}}
                                Dim slabLevel As Decimal = firstSlabAboveLow
                                While slabLevel <= previousDayHigh
                                    slabLevel = GetSlabBasedLevel(slabLevel, 1, slab)
                                    If slabLevel <= previousDayHigh Then
                                        slabData.Add(slabLevel, 0)
                                    End If
                                End While
                                Dim upperLevelBasedOnOpen As Decimal = GetSlabBasedLevel(currentdayPayload.FirstOrDefault.Value.Open, 1, slab)
                                Dim lowerLevelBasedOnOpen As Decimal = GetSlabBasedLevel(currentdayPayload.FirstOrDefault.Value.Open, -1, slab)
                                If Not slabData.ContainsKey(upperLevelBasedOnOpen) Then slabData.Add(upperLevelBasedOnOpen, 0)
                                If Not slabData.ContainsKey(lowerLevelBasedOnOpen) Then slabData.Add(lowerLevelBasedOnOpen, 0)
                                Dim slabList As List(Of Decimal) = slabData.Keys.ToList
                                For Each runningPayload In inputPayload
                                    If runningPayload.Key.Date = previousTradingDay.Date Then
                                        For Each runningSlab In slabList
                                            If runningPayload.Value.High >= runningSlab AndAlso
                                                runningPayload.Value.Low <= runningSlab Then
                                                slabData(runningSlab) = slabData(runningSlab) + 1
                                            End If
                                        Next
                                    End If
                                Next
                                Dim maxHit As Integer = slabData.Values.Max
                                Dim maxHitLevels As String = Nothing
                                For Each runningSlab In slabData
                                    If runningSlab.Value = maxHit Then
                                        maxHitLevels = String.Format("{0} {1};", maxHitLevels, runningSlab.Key)
                                    End If
                                Next
                                maxHitLevels = maxHitLevels.Substring(0, maxHitLevels.Count - 1).Trim

                                If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                tempStockList.Add(runningStock, {maxHitLevels, maxHit, upperLevelBasedOnOpen, slabData(upperLevelBasedOnOpen), lowerLevelBasedOnOpen, slabData(lowerLevelBasedOnOpen)})
                            End If
                        End If
                    Next
                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList
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
                            row("Max Level") = runningStock.Value(0)
                            row("Max Level Hits") = runningStock.Value(1)
                            row("Open Upper Level") = runningStock.Value(2)
                            row("Open Upper Level Hits") = runningStock.Value(3)
                            row("Open Lower Level") = runningStock.Value(4)
                            row("Open Lower Level Hits") = runningStock.Value(5)

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

    Private Function GetSlabBasedLevel(ByVal price As Decimal, ByVal direction As Integer, ByVal slab As Decimal) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If direction > 0 Then
            ret = Math.Ceiling(price / slab) * slab
            If ret = price Then ret = price + slab
        ElseIf direction < 0 Then
            ret = Math.Floor(price / slab) * slab
            If ret = price Then ret = price - slab
        End If
        Return ret
    End Function
End Class