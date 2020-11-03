Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class FirstFavourableFractalTopGainerLooser
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
        ret.Columns.Add("Breakout Time")
        ret.Columns.Add("Favourable Fractal Time")
        ret.Columns.Add("Gainer_Looser")
        ret.Columns.Add("Change %")
        ret.Columns.Add("Position")

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
                If atrStockList IsNot Nothing AndAlso atrStockList.Count > 0 AndAlso
                    atrStock.AllStocks IsNot Nothing AndAlso atrStock.AllStocks.Count > 0 Then
                    Dim stockData As Dictionary(Of String, Dictionary(Of Date, Payload)) = Nothing
                    For Each runningStock In atrStock.AllStocks
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, runningStock.Key, tradingDate.AddDays(-8), tradingDate)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                            If stockData Is Nothing Then stockData = New Dictionary(Of String, Dictionary(Of Date, Payload))
                            stockData.Add(runningStock.Key, intradayPayload)
                        End If
                    Next
                    If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
                        Dim topFractalBreakoutData As Dictionary(Of Date, Dictionary(Of String, Date)) = Nothing
                        Dim bottomFractalBreakoutData As Dictionary(Of Date, Dictionary(Of String, Date)) = Nothing
                        For Each runningStock In atrStockList.Keys
                            If stockData.ContainsKey(runningStock) Then
                                Dim inputPayload As Dictionary(Of Date, Payload) = stockData(runningStock)
                                If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                                    Dim fractalHighPayload As Dictionary(Of Date, Decimal) = Nothing
                                    Dim fractalLowPayload As Dictionary(Of Date, Decimal) = Nothing
                                    Indicator.FractalBands.CalculateFractal(inputPayload, fractalHighPayload, fractalLowPayload)

                                    Dim lastFavourableFractalHigh As Date = Date.MinValue
                                    Dim lastFavourableFractalLow As Date = Date.MinValue
                                    For Each runningPayload In inputPayload
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        If runningPayload.Value.PreviousCandlePayload IsNot Nothing AndAlso
                                            runningPayload.Value.PreviousCandlePayload.PayloadDate.Date = tradingDate.Date Then
                                            If fractalHighPayload(runningPayload.Key) < fractalHighPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                                lastFavourableFractalHigh = runningPayload.Key
                                            End If
                                            If fractalLowPayload(runningPayload.Key) > fractalLowPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate) Then
                                                lastFavourableFractalLow = runningPayload.Key
                                            End If
                                            If lastFavourableFractalHigh <> Date.MinValue Then
                                                If runningPayload.Value.High > fractalHighPayload(lastFavourableFractalHigh) Then
                                                    If topFractalBreakoutData Is Nothing Then topFractalBreakoutData = New Dictionary(Of Date, Dictionary(Of String, Date))
                                                    If topFractalBreakoutData.ContainsKey(runningPayload.Key) Then
                                                        topFractalBreakoutData(runningPayload.Key).Add(runningStock, lastFavourableFractalHigh)
                                                    Else
                                                        topFractalBreakoutData.Add(runningPayload.Key, New Dictionary(Of String, Date) From {{runningStock, lastFavourableFractalHigh}})
                                                    End If
                                                    Exit For
                                                End If
                                            End If
                                            If lastFavourableFractalLow <> Date.MinValue Then
                                                If runningPayload.Value.Low < fractalLowPayload(lastFavourableFractalLow) Then
                                                    If bottomFractalBreakoutData Is Nothing Then bottomFractalBreakoutData = New Dictionary(Of Date, Dictionary(Of String, Date))
                                                    If bottomFractalBreakoutData.ContainsKey(runningPayload.Key) Then
                                                        bottomFractalBreakoutData(runningPayload.Key).Add(runningStock, lastFavourableFractalLow)
                                                    Else
                                                        bottomFractalBreakoutData.Add(runningPayload.Key, New Dictionary(Of String, Date) From {{runningStock, lastFavourableFractalLow}})
                                                    End If
                                                    Exit For
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        Next
                        If topFractalBreakoutData IsNot Nothing AndAlso topFractalBreakoutData.Count > 0 AndAlso
                            bottomFractalBreakoutData IsNot Nothing AndAlso bottomFractalBreakoutData.Count > 0 Then
                            For Each runningTime In topFractalBreakoutData.OrderBy(Function(x)
                                                                                       Return x.Key
                                                                                   End Function)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim topLooser As Dictionary(Of String, Tuple(Of Decimal, Integer)) = GetTop10LosserData(runningTime.Key, atrStock.AllStocks, stockData)
                                If topLooser IsNot Nothing Then
                                    For Each runningStock In runningTime.Value
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        Dim chngPer As Decimal = Decimal.MinValue
                                        Dim pos As Integer = Integer.MinValue
                                        Dim remarks As String = ""
                                        If topLooser IsNot Nothing AndAlso topLooser.ContainsKey(runningStock.Key) Then
                                            chngPer = topLooser(runningStock.Key).Item1
                                            pos = topLooser(runningStock.Key).Item2
                                            remarks = "Looser"
                                        End If
                                        If chngPer <> Decimal.MinValue AndAlso pos <> Integer.MinValue AndAlso remarks <> "" Then
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
                                            row("Breakout Time") = runningTime.Key.ToString("dd-MMM-yyyy HH:mm:ss")
                                            row("Favourable Fractal Time") = runningStock.Value.ToString("dd-MMM-yyyy HH:mm:ss")
                                            row("Gainer_Looser") = remarks
                                            row("Change %") = Math.Round(chngPer, 2)
                                            row("Position") = pos

                                            ret.Rows.Add(row)
                                        End If
                                    Next
                                End If
                            Next
                            For Each runningTime In bottomFractalBreakoutData.OrderBy(Function(x)
                                                                                          Return x.Key
                                                                                      End Function)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim topGainer As Dictionary(Of String, Tuple(Of Decimal, Integer)) = GetTop10GainerData(runningTime.Key, atrStock.AllStocks, stockData)
                                If topGainer IsNot Nothing Then
                                    For Each runningStock In runningTime.Value
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        Dim chngPer As Decimal = Decimal.MinValue
                                        Dim pos As Integer = Integer.MinValue
                                        Dim remarks As String = ""
                                        If topGainer IsNot Nothing AndAlso topGainer.ContainsKey(runningStock.Key) Then
                                            chngPer = topGainer(runningStock.Key).Item1
                                            pos = topGainer(runningStock.Key).Item2
                                            remarks = "Gainer"
                                        End If
                                        If chngPer <> Decimal.MinValue AndAlso pos <> Integer.MinValue AndAlso remarks <> "" Then
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
                                            row("Breakout Time") = runningTime.Key.ToString("dd-MMM-yyyy HH:mm:ss")
                                            row("Favourable Fractal Time") = runningStock.Value.ToString("dd-MMM-yyyy HH:mm:ss")
                                            row("Gainer_Looser") = remarks
                                            row("Change %") = Math.Round(chngPer, 2)
                                            row("Position") = pos

                                            ret.Rows.Add(row)
                                        End If
                                    Next
                                End If
                            Next
                        End If
                    End If
                End If

                tradingDate = tradingDate.AddDays(1)
            End While
        End Using
        Return ret
    End Function

    Private Function GetTop10GainerData(ByVal checkingTime As Date, ByVal stockDetails As Dictionary(Of String, ActiveInstrumentData),
                                        ByVal stockData As Dictionary(Of String, Dictionary(Of Date, Payload))) As Dictionary(Of String, Tuple(Of Decimal, Integer))
        Dim ret As Dictionary(Of String, Tuple(Of Decimal, Integer)) = Nothing
        Dim chngPerData As Dictionary(Of String, Decimal) = Nothing
        For Each runningStock In stockDetails
            If stockData.ContainsKey(runningStock.Key) Then
                Dim intradayPayload As Dictionary(Of Date, Payload) = stockData(runningStock.Key)
                Dim candleToCheck As Payload = intradayPayload.Values.Where(Function(x)
                                                                                Return x.PayloadDate < checkingTime
                                                                            End Function).LastOrDefault
                If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
                    Dim chngPer As Decimal = ((candleToCheck.Close - runningStock.Value.LastDayClose) / runningStock.Value.LastDayClose) * 100
                    If chngPerData Is Nothing Then chngPerData = New Dictionary(Of String, Decimal)
                    chngPerData.Add(runningStock.Key, chngPer)
                End If
            End If
        Next
        If chngPerData IsNot Nothing AndAlso chngPerData.Count > 0 Then
            Dim counter As Integer = 0
            For Each runningStock In chngPerData.OrderByDescending(Function(x)
                                                                       Return x.Value
                                                                   End Function)
                counter += 1
                If ret Is Nothing Then ret = New Dictionary(Of String, Tuple(Of Decimal, Integer))
                ret.Add(runningStock.Key, New Tuple(Of Decimal, Integer)(runningStock.Value, counter))

                If counter >= 10 Then Exit For
            Next
        End If
        Return ret
    End Function

    Private Function GetTop10LosserData(ByVal checkingTime As Date, ByVal stockDetails As Dictionary(Of String, ActiveInstrumentData),
                                        ByVal stockData As Dictionary(Of String, Dictionary(Of Date, Payload))) As Dictionary(Of String, Tuple(Of Decimal, Integer))
        Dim ret As Dictionary(Of String, Tuple(Of Decimal, Integer)) = Nothing
        Dim chngPerData As Dictionary(Of String, Decimal) = Nothing
        For Each runningStock In stockDetails
            If stockData.ContainsKey(runningStock.Key) Then
                Dim intradayPayload As Dictionary(Of Date, Payload) = stockData(runningStock.Key)
                Dim candleToCheck As Payload = intradayPayload.Values.Where(Function(x)
                                                                                Return x.PayloadDate < checkingTime
                                                                            End Function).LastOrDefault
                If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
                    Dim chngPer As Decimal = ((candleToCheck.Close - runningStock.Value.LastDayClose) / runningStock.Value.LastDayClose) * 100
                    If chngPerData Is Nothing Then chngPerData = New Dictionary(Of String, Decimal)
                    chngPerData.Add(runningStock.Key, chngPer)
                End If
            End If
        Next
        If chngPerData IsNot Nothing AndAlso chngPerData.Count > 0 Then
            Dim counter As Integer = 0
            For Each runningStock In chngPerData.OrderBy(Function(x)
                                                             Return x.Value
                                                         End Function)
                counter += 1
                If ret Is Nothing Then ret = New Dictionary(Of String, Tuple(Of Decimal, Integer))
                ret.Add(runningStock.Key, New Tuple(Of Decimal, Integer)(runningStock.Value, counter))

                If counter >= 10 Then Exit For
            Next
        End If
        Return ret
    End Function

End Class