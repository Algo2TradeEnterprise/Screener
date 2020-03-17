Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class TopGainerTopLosserOfEverySlab
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
                    Dim endTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 10, 59, 0)
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
                        Dim gainerLooserStockList As Dictionary(Of String, String()) = Nothing
                        Dim previousGainerLooserStockList As Dictionary(Of String, String()) = Nothing
                        While payloadTime <= endTime
                            Dim tempGainerLooserStockList As Dictionary(Of String, String()) = Nothing
                            For Each runningStock In atrStockList.Keys
                                _canceller.Token.ThrowIfCancellationRequested()
                                If stockData.ContainsKey(runningStock) Then
                                    Dim intradayPayload As Dictionary(Of Date, Payload) = stockData(runningStock)
                                    If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                                        Dim candleToCheck As Payload = intradayPayload.Values.Where(Function(x)
                                                                                                        Return x.PayloadDate <= payloadTime
                                                                                                    End Function).LastOrDefault
                                        'If intradayPayload.ContainsKey(payloadTime) Then
                                        '    candleToCheck = intradayPayload(payloadTime)
                                        'End If
                                        If candleToCheck IsNot Nothing Then
                                            Dim previousClose As Decimal = atrStockList(runningStock).PreviousDayClose
                                            Dim gainLossPercentage As Decimal = ((candleToCheck.Close - previousClose) / previousClose) * 100
                                            If tempGainerLooserStockList Is Nothing Then tempGainerLooserStockList = New Dictionary(Of String, String())
                                            tempGainerLooserStockList.Add(runningStock, {Math.Round(gainLossPercentage, 4), payloadTime.ToString("HH:mm:ss")})
                                        End If
                                    End If
                                End If
                            Next

                            Dim todayGainerLooserStockList As Dictionary(Of String, String()) = Nothing
                            If tempGainerLooserStockList IsNot Nothing AndAlso tempGainerLooserStockList.Count > 0 Then
                                Dim counter As Integer = 0
                                For Each runningStock In tempGainerLooserStockList.OrderByDescending(Function(x)
                                                                                                         Return CDec(x.Value(0))
                                                                                                     End Function)
                                    If todayGainerLooserStockList Is Nothing Then todayGainerLooserStockList = New Dictionary(Of String, String())
                                    todayGainerLooserStockList.Add(runningStock.Key, runningStock.Value)
                                    counter += 1
                                    If counter = 5 Then Exit For
                                Next

                                counter = 0
                                For Each runningStock In tempGainerLooserStockList.OrderBy(Function(x)
                                                                                               Return CDec(x.Value(0))
                                                                                           End Function)
                                    If todayGainerLooserStockList Is Nothing Then todayGainerLooserStockList = New Dictionary(Of String, String())
                                    If Not todayGainerLooserStockList.ContainsKey(runningStock.Key) Then
                                        todayGainerLooserStockList.Add(runningStock.Key, runningStock.Value)
                                    End If
                                    counter += 1
                                    If counter = 5 Then Exit For
                                Next

                                If payloadTime <> startTime AndAlso todayGainerLooserStockList IsNot Nothing AndAlso todayGainerLooserStockList.Count > 0 Then
                                    For Each runningStock In todayGainerLooserStockList
                                        If previousGainerLooserStockList Is Nothing OrElse Not previousGainerLooserStockList.ContainsKey(runningStock.Key) Then
                                            If gainerLooserStockList Is Nothing Then gainerLooserStockList = New Dictionary(Of String, String())
                                            If Not gainerLooserStockList.ContainsKey(runningStock.Key) Then
                                                gainerLooserStockList.Add(runningStock.Key, runningStock.Value)
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                            previousGainerLooserStockList = todayGainerLooserStockList

                            payloadTime = payloadTime.AddMinutes(5)
                        End While

                        If gainerLooserStockList IsNot Nothing AndAlso gainerLooserStockList.Count Then
                            For Each runningStock In gainerLooserStockList
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
                                row("Time") = runningStock.Value(1)

                                ret.Rows.Add(row)
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