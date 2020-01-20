Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class IntradayVolumeSpike
    Inherits StockSelection

    Private _checkingTime As Date
    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer,
                   ByVal checkingTime As Date)
        MyBase.New(canceller, cmn, stockType)
        _checkingTime = checkingTime
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
        ret.Columns.Add("Volume Change %")

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
                    Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
                    For Each runningStock In atrStockList.Keys
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(Common.DataBaseTable.Intraday_Cash, runningStock, tradingDate.AddDays(-15), tradingDate)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                            Dim signalCheckStartTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0)
                            Dim signalCheckEndTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, _checkingTime.Hour, _checkingTime.Minute, _checkingTime.Second)
                            Dim currentDayVolumeSum As Long = 0
                            Dim previousDaysVolumeSum As Long = 0
                            Dim counter As Integer = 0
                            Dim lastCalculatedDate As Date = Date.MinValue
                            For Each runningPayload In intradayPayload.Keys.OrderByDescending(Function(x)
                                                                                                  Return x
                                                                                              End Function)
                                Dim signalStart As Date = New Date(runningPayload.Year, runningPayload.Month, runningPayload.Day, signalCheckStartTime.Hour, signalCheckStartTime.Minute, signalCheckStartTime.Second)
                                Dim signalEnd As Date = New Date(runningPayload.Year, runningPayload.Month, runningPayload.Day, signalCheckEndTime.Hour, signalCheckEndTime.Minute, signalCheckEndTime.Second)
                                If runningPayload.Date = tradingDate.Date Then
                                    If runningPayload >= signalStart AndAlso runningPayload <= signalEnd Then
                                        currentDayVolumeSum += intradayPayload(runningPayload).Volume
                                    End If
                                ElseIf runningPayload.Date < tradingDate.Date Then
                                    If runningPayload >= signalStart AndAlso runningPayload <= signalEnd Then
                                        If lastCalculatedDate.Date <> runningPayload.Date Then
                                            lastCalculatedDate = runningPayload
                                            counter += 1
                                            If counter = 5 + 1 Then Exit For
                                        End If
                                        previousDaysVolumeSum += intradayPayload(runningPayload).Volume
                                    End If
                                End If
                            Next
                            If currentDayVolumeSum <> 0 AndAlso previousDaysVolumeSum <> 0 Then
                                Dim changePer As Decimal = ((currentDayVolumeSum / (previousDaysVolumeSum / 5)) - 1) * 100
                                If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
                                tempStockList.Add(runningStock, {Math.Round(changePer, 4)})
                            End If
                        End If
                    Next
                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList.OrderByDescending(Function(x)
                                                                                     Return Math.Abs(x.Value(0))
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
                            row("Volume Change %") = runningStock.Value(0)

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
End Class
