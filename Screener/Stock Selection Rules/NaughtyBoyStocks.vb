﻿Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class NaughtyBoyStocks
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
        ret.Columns.Add("Change %")
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
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim niftyEODPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_POSITIONAL, "NIFTY 50", tradingDate.AddDays(-8), tradingDate)
                    If niftyEODPayload IsNot Nothing AndAlso niftyEODPayload.Count > 0 Then
                        Dim niftyCandle As Payload = niftyEODPayload.LastOrDefault.Value
                        If niftyEODPayload.LastOrDefault.Value.PayloadDate.Date = tradingDate.Date Then
                            niftyCandle = niftyEODPayload.LastOrDefault.Value.PreviousCandlePayload
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim tempStockList As Dictionary(Of String, String()) = Nothing
                        For Each runningStock In atrStockList.Keys
                            Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_POSITIONAL, runningStock, niftyCandle.PayloadDate.AddDays(-7), niftyCandle.PayloadDate)
                            If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
                                Dim candle As Payload = eodPayload.LastOrDefault.Value
                                If niftyCandle.Close > niftyCandle.PreviousCandlePayload.Close Then
                                    If candle.Close < candle.PreviousCandlePayload.Close Then
                                        Dim chng As Decimal = ((candle.PreviousCandlePayload.Close / candle.Close) - 1) * 100

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock, {Math.Round(chng, 2), "Buy"})
                                    End If
                                ElseIf niftyCandle.Close < niftyCandle.PreviousCandlePayload.Close Then
                                    If candle.Close > candle.PreviousCandlePayload.Close Then
                                        Dim chng As Decimal = (1 - (candle.PreviousCandlePayload.Close / candle.Close)) * 100

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock, {Math.Round(chng, 2), "Sell"})
                                    End If
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
                                row("Current Day Close") = atrStockList(runningStock.Key).CurrentDayClose
                                row("Slab") = atrStockList(runningStock.Key).Slab
                                row("Change %") = runningStock.Value(0)
                                row("Direction") = runningStock.Value(1)

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