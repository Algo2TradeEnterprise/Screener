Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class HighSlabLevelMovedStocks
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
        ret.Columns.Add("Level Moved")

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
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, runningStock, tradingDate, tradingDate)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                            Dim levelCtr As Integer = 0
                            Dim slab As Decimal = atrStockList(runningStock).Slab

                            Dim buffer As Decimal = CalculateBuffer(intradayPayload.FirstOrDefault.Value.Open, Utilities.Numbers.NumberManipulation.RoundOfType.Floor)
                            Dim upperLevel As Decimal = GetSlabBasedLevel(intradayPayload.FirstOrDefault.Value.Open, 1, slab) + buffer
                            Dim lowerLevel As Decimal = GetSlabBasedLevel(intradayPayload.FirstOrDefault.Value.Open, -1, slab) - buffer
                            Dim enterd As Decimal = Decimal.MinValue
                            For Each runningPayload In intradayPayload
                                If enterd <> Decimal.MinValue Then
                                    If runningPayload.Value.High >= enterd + slab Then
                                        levelCtr += 1
                                        enterd = Decimal.MinValue
                                    ElseIf runningPayload.Value.Low <= enterd - slab Then
                                        levelCtr += 1
                                        enterd = Decimal.MinValue
                                    End If
                                Else
                                    upperLevel = GetSlabBasedLevel(runningPayload.Value.Open, 1, slab) + buffer
                                    lowerLevel = GetSlabBasedLevel(runningPayload.Value.Open, -1, slab) - buffer
                                    If runningPayload.Value.High >= upperLevel AndAlso runningPayload.Value.Low <= lowerLevel Then
                                        levelCtr += 1
                                    ElseIf runningPayload.Value.High >= upperLevel Then
                                        enterd = upperLevel
                                    ElseIf runningPayload.Value.Low <= lowerLevel Then
                                        enterd = lowerLevel
                                    End If
                                End If
                            Next

                            If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                            tempStockList.Add(runningStock, {levelCtr})
                        End If
                    Next
                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList.OrderByDescending(Function(x)
                                                                                     Return CDec(x.Value(0))
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
                            row("Level Moved") = runningStock.Value(0)

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
        ElseIf direction < 0 Then
            ret = Math.Floor(price / slab) * slab
        End If
        Return ret
    End Function
End Class
