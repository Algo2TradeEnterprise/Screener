Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class TopGainerTopLosser
    Inherits StockSelection

    Private ReadOnly _checkingTime As Date
    Private ReadOnly _niftyChangePercentage As Decimal
    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer,
                   ByVal checkingTime As Date,
                   ByVal niftyChangePercentage As Decimal)
        MyBase.New(canceller, cmn, stockType)
        _checkingTime = checkingTime
        _niftyChangePercentage = niftyChangePercentage
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
        ret.Columns.Add("Nifty Gain Loss %")

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
                    Dim payloadTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, _checkingTime.Hour, _checkingTime.Minute, 0)
                    Dim niftyGainLossPercentage As Decimal = Decimal.MinValue
                    Dim niftyEODPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(Common.DataBaseTable.EOD_Futures, "NIFTY", tradingDate.AddDays(-15), tradingDate)
                    If niftyEODPayload IsNot Nothing AndAlso niftyEODPayload.Count > 0 AndAlso niftyEODPayload.LastOrDefault.Key.Date = tradingDate.Date Then
                        If niftyEODPayload.LastOrDefault.Value.PreviousCandlePayload IsNot Nothing Then
                            Dim previousDayPayload As Payload = niftyEODPayload.LastOrDefault.Value.PreviousCandlePayload
                            Dim niftyIntradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(Common.DataBaseTable.Intraday_Futures, "NIFTY", tradingDate.AddDays(-15), tradingDate)
                            If niftyIntradayPayload IsNot Nothing AndAlso niftyIntradayPayload.Count > 0 Then
                                If _niftyChangePercentage = 0 Then
                                    Dim candleToCheck As Payload = Nothing
                                    If niftyIntradayPayload.ContainsKey(payloadTime) Then
                                        candleToCheck = niftyIntradayPayload(payloadTime)
                                    End If
                                    If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
                                        niftyGainLossPercentage = Math.Round(((candleToCheck.Close - previousDayPayload.Close) / previousDayPayload.Close) * 100, 4)
                                    End If
                                Else
                                    For Each runningPaylod In niftyIntradayPayload.Keys
                                        If runningPaylod >= payloadTime Then
                                            Dim candleToCheck As Payload = niftyIntradayPayload(runningPaylod)
                                            If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
                                                niftyGainLossPercentage = Math.Round(((candleToCheck.Close - previousDayPayload.Close) / previousDayPayload.Close) * 100, 4)
                                                If Math.Abs(niftyGainLossPercentage) >= _niftyChangePercentage Then
                                                    payloadTime = runningPaylod
                                                    Exit For
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    End If
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim tempStockList As Dictionary(Of String, String()) = Nothing
                    For Each runningStock In atrStockList.Keys
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, runningStock, tradingDate.AddDays(-15), tradingDate)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                            Dim candleToCheck As Payload = Nothing
                            If intradayPayload.ContainsKey(payloadTime) Then
                                candleToCheck = intradayPayload(payloadTime)
                            End If
                            If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
                                Dim previousClose As Decimal = atrStockList(runningStock).PreviousDayClose
                                Dim gainLossPercentage As Decimal = ((candleToCheck.Close - previousClose) / previousClose) * 100
                                If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                tempStockList.Add(runningStock, {Math.Round(gainLossPercentage, 4), Math.Round(niftyGainLossPercentage, 4)})
                            End If
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
                            row("Gain Loss %") = runningStock.Value(0)
                            row("Nifty Gain Loss %") = runningStock.Value(1)

                            ret.Rows.Add(row)
                            stockCounter += 1
                            If stockCounter = My.Settings.NumberOfStockPerDay Then Exit For
                        Next
                        If My.Settings.NumberOfStockPerDay < tempStockList.Count Then
                            stockCounter = 0
                            For Each runningStock In tempStockList.OrderBy(Function(x)
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
                                row("Gain Loss %") = runningStock.Value(0)
                                row("Nifty Gain Loss %") = runningStock.Value(1)

                                ret.Rows.Add(row)
                                stockCounter += 1
                                If stockCounter = My.Settings.NumberOfStockPerDay Then Exit For
                            Next
                        End If
                    End If
                End If

                tradingDate = tradingDate.AddDays(1)
            End While
        End Using
        Return ret
    End Function
End Class
