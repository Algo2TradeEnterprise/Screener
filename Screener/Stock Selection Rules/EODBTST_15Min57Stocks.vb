Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class EODBTST_15Min57Stocks
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
        ret.Columns.Add("Supporting 1")

        Using atrStock As New ATRStockSelection(_canceller)
            AddHandler atrStock.Heartbeat, AddressOf OnHeartbeat

            Dim tradingDate As Date = startDate
            While tradingDate <= endDate
                _bannedStockFileName = Path.Combine(My.Application.Info.DirectoryPath, String.Format("Bannned Stocks {0}.csv", tradingDate.ToString("ddMMyyyy")))
                For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "Bannned Stocks *.csv")
                    If Not runningFile.Contains(tradingDate.ToString("ddMMyyyy")) Then File.Delete(runningFile)
                Next
                Dim bannedStockList As List(Of String) = Nothing
                'Using bannedStock As New BannedStockDataFetcher(_bannedStockFileName, _canceller)
                '    AddHandler bannedStock.Heartbeat, AddressOf OnHeartbeat
                '    bannedStockList = Await bannedStock.GetBannedStocksData(tradingDate).ConfigureAwait(False)
                'End Using

                Dim atrStockList As Dictionary(Of String, InstrumentDetails) = Await atrStock.GetATRStockData(_eodTable, tradingDate, bannedStockList, False).ConfigureAwait(False)
                If atrStockList IsNot Nothing AndAlso atrStockList.Count > 0 Then
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim tempStockList As Dictionary(Of String, String()) = Nothing
                    For Each runningStock In atrStockList.Keys
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(Common.DataBaseTable.Intraday_Cash, runningStock, tradingDate.AddDays(-8), tradingDate)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 15 AndAlso intradayPayload.LastOrDefault.Key.Date = tradingDate.Date Then
                            Dim xMinPayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(intradayPayload, 15, New Date(Now.Year, Now.Month, Now.Day, 9, 15, 0))

                            Dim smaVolPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.SMA.CalculateSMA(15, Payload.PayloadFields.Volume, xMinPayload, smaVolPayload)

                            Dim currentCandle As Payload = xMinPayload.LastOrDefault.Value
                            Dim condition As String = Nothing
                            If currentCandle.Close > currentCandle.PreviousCandlePayload.Close AndAlso
                                currentCandle.PreviousCandlePayload.Close > currentCandle.PreviousCandlePayload.PreviousCandlePayload.Close AndAlso
                                currentCandle.PreviousCandlePayload.PreviousCandlePayload.Close > currentCandle.PreviousCandlePayload.PreviousCandlePayload.PreviousCandlePayload.Close Then
                                If currentCandle.Volume > smaVolPayload(currentCandle.PayloadDate) AndAlso
                                    currentCandle.PreviousCandlePayload.Volume > smaVolPayload(currentCandle.PreviousCandlePayload.PayloadDate) AndAlso
                                    currentCandle.PreviousCandlePayload.PreviousCandlePayload.Volume > smaVolPayload(currentCandle.PreviousCandlePayload.PreviousCandlePayload.PayloadDate) Then
                                    condition = "Basic"
                                End If
                            End If
                            If condition IsNot Nothing Then
                                Dim sma10Payload As Dictionary(Of Date, Decimal) = Nothing
                                Indicator.SMA.CalculateSMA(10, Payload.PayloadFields.Close, xMinPayload, sma10Payload)
                                Dim sma50Payload As Dictionary(Of Date, Decimal) = Nothing
                                Indicator.SMA.CalculateSMA(50, Payload.PayloadFields.Close, xMinPayload, sma50Payload)
                                Dim rsiPayload As Dictionary(Of Date, Decimal) = Nothing
                                Indicator.RSI.CalculateRSI(14, xMinPayload, rsiPayload)
                                Dim adxPayload As Dictionary(Of Date, Decimal) = Nothing
                                Indicator.ADX.CalculateADX(14, 14, xMinPayload, adxPayload, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
                                Dim macdPayload As Dictionary(Of Date, Decimal) = Nothing
                                Dim signalPayload As Dictionary(Of Date, Decimal) = Nothing
                                Indicator.MACD.CalculateMACD(12, 26, 9, xMinPayload, macdPayload, signalPayload, Nothing)

                                If rsiPayload(currentCandle.PayloadDate) > 50 Then
                                    If macdPayload(currentCandle.PayloadDate) > signalPayload(currentCandle.PayloadDate) Then
                                        If adxPayload(currentCandle.PayloadDate) > 25 Then
                                            If currentCandle.Close > sma10Payload(currentCandle.PayloadDate) Then
                                                If currentCandle.Close > sma50Payload(currentCandle.PayloadDate) Then
                                                    condition = "Advanced"
                                                End If
                                            End If
                                        End If
                                    End If
                                End If

                                If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                tempStockList.Add(runningStock, {condition})
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
                            row("Current Day Close") = atrStockList(runningStock.Key).CurrentDayClose
                            row("Slab") = atrStockList(runningStock.Key).Slab
                            row("Supporting 1") = runningStock.Value(0)

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