Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class MultiTimeframeHKSignal
    Inherits StockSelection

    Private _ltf As String
    Private _mtf As String
    Private _htf As String
    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer,
                   ByVal ltf As String,
                   ByVal mtf As String,
                   ByVal htf As String)
        MyBase.New(canceller, cmn, stockType)
        _ltf = ltf
        _mtf = mtf
        _htf = htf
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
        ret.Columns.Add("Signal Time")
        ret.Columns.Add("LTF Signal")
        ret.Columns.Add("MTF SIgnal")
        ret.Columns.Add("HTF Signal")

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
                    Dim tempStockList As List(Of Tuple(Of String, Date, String, String, String)) = Nothing
                    Dim counter As Integer = 0
                    For Each runningStock In atrStockList.Keys
                        _canceller.Token.ThrowIfCancellationRequested()
                        counter += 1
                        Dim ltfPayload As Dictionary(Of Date, Payload) = Nothing
                        Dim mtfPayload As Dictionary(Of Date, Payload) = Nothing
                        Dim htfPayload As Dictionary(Of Date, Payload) = Nothing
                        If _ltf.Contains("Min") OrElse _mtf.Contains("Min") OrElse _htf.Contains("Min") Then
                            Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, runningStock, tradingDate.AddDays(-25), tradingDate)
                            If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                                Dim exchangeStartTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0)
                                If _ltf.Contains("Min") Then
                                    Dim ltf As Integer = CInt(_ltf.Split(" ")(0).Trim)
                                    Indicator.HeikenAshi.ConvertToHeikenAshi(Common.ConvertPayloadsToXMinutes(intradayPayload, ltf, exchangeStartTime), ltfPayload)
                                End If
                                If _mtf.Contains("Min") Then
                                    Dim mtf As Integer = CInt(_mtf.Split(" ")(0).Trim)
                                    Indicator.HeikenAshi.ConvertToHeikenAshi(Common.ConvertPayloadsToXMinutes(intradayPayload, mtf, exchangeStartTime), mtfPayload)
                                End If
                                If _htf.Contains("Min") Then
                                    Dim htf As Integer = CInt(_htf.Split(" ")(0).Trim)
                                    Indicator.HeikenAshi.ConvertToHeikenAshi(Common.ConvertPayloadsToXMinutes(intradayPayload, htf, exchangeStartTime), htfPayload)
                                End If
                            End If
                        End If
                        If Not _ltf.Contains("Min") OrElse Not _mtf.Contains("Min") OrElse Not _htf.Contains("Min") Then
                            Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_eodTable, runningStock, tradingDate.AddDays(-1000), tradingDate)
                            If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
                                If _ltf.Contains("Day") Then Indicator.HeikenAshi.ConvertToHeikenAshi(eodPayload, ltfPayload)
                                If _ltf.Contains("Week") Then Indicator.HeikenAshi.ConvertToHeikenAshi(Common.ConvertDayPayloadsToWeek(eodPayload), ltfPayload)
                                If _ltf.Contains("Month") Then Indicator.HeikenAshi.ConvertToHeikenAshi(Common.ConvertDayPayloadsToMonth(eodPayload), ltfPayload)
                                If _mtf.Contains("Day") Then Indicator.HeikenAshi.ConvertToHeikenAshi(eodPayload, mtfPayload)
                                If _mtf.Contains("Week") Then Indicator.HeikenAshi.ConvertToHeikenAshi(Common.ConvertDayPayloadsToWeek(eodPayload), mtfPayload)
                                If _mtf.Contains("Month") Then Indicator.HeikenAshi.ConvertToHeikenAshi(Common.ConvertDayPayloadsToMonth(eodPayload), mtfPayload)
                                If _htf.Contains("Day") Then Indicator.HeikenAshi.ConvertToHeikenAshi(eodPayload, htfPayload)
                                If _htf.Contains("Week") Then Indicator.HeikenAshi.ConvertToHeikenAshi(Common.ConvertDayPayloadsToWeek(eodPayload), htfPayload)
                                If _htf.Contains("Month") Then Indicator.HeikenAshi.ConvertToHeikenAshi(Common.ConvertDayPayloadsToMonth(eodPayload), htfPayload)
                            End If
                        End If

                        OnHeartbeat(String.Format("Getting signal for {0} #{1}/{2}", runningStock, counter, atrStockList.Count))
                        If ltfPayload IsNot Nothing AndAlso ltfPayload.Count > 0 AndAlso
                            mtfPayload IsNot Nothing AndAlso mtfPayload.Count > 0 AndAlso
                            htfPayload IsNot Nothing AndAlso htfPayload.Count > 0 Then
                            Dim currentDayPayload As Dictionary(Of Date, Payload) = Nothing
                            For Each runningPayload In ltfPayload
                                If runningPayload.Value.PayloadDate.Date = tradingDate.Date Then
                                    If currentDayPayload Is Nothing Then currentDayPayload = New Dictionary(Of Date, Payload)
                                    currentDayPayload.Add(runningPayload.Key, runningPayload.Value)
                                End If
                            Next
                            If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 Then
                                For Each runningPayload In currentDayPayload
                                    Dim signal As String = Nothing
                                    If runningPayload.Value.CandleStrengthHeikenAshi = Payload.StrongCandle.Bullish AndAlso
                                        runningPayload.Value.PreviousCandlePayload.CandleStrengthHeikenAshi = Payload.StrongCandle.Bearish Then
                                        signal = "Buy"
                                    ElseIf runningPayload.Value.CandleStrengthHeikenAshi = Payload.StrongCandle.Bearish AndAlso
                                        runningPayload.Value.PreviousCandlePayload.CandleStrengthHeikenAshi = Payload.StrongCandle.Bullish Then
                                        signal = "Sell"
                                    End If
                                    If signal IsNot Nothing Then
                                        Dim mtfCandle As Payload = Nothing
                                        Dim htfCandle As Payload = Nothing
                                        If _mtf.Contains("Min") Then
                                            Dim mtf As Integer = CInt(_mtf.Split(" ")(0).Trim)
                                            mtfCandle = mtfPayload(GetCurrentXMinuteCandleTime(runningPayload.Key, mtf))
                                        Else
                                            mtfCandle = mtfPayload.LastOrDefault.Value
                                        End If
                                        If _htf.Contains("Min") Then
                                            Dim htf As Integer = CInt(_htf.Split(" ")(0).Trim)
                                            htfCandle = htfPayload(GetCurrentXMinuteCandleTime(runningPayload.Key, htf))
                                        Else
                                            htfCandle = htfPayload.LastOrDefault.Value
                                        End If
                                        If mtfPayload IsNot Nothing AndAlso htfCandle IsNot Nothing Then
                                            If tempStockList Is Nothing Then tempStockList = New List(Of Tuple(Of String, Date, String, String, String))
                                            tempStockList.Add(New Tuple(Of String, Date, String, String, String)(runningStock, runningPayload.Key, signal, GetSignalName(mtfCandle), GetSignalName(htfCandle)))
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    Next
                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList
                            _canceller.Token.ThrowIfCancellationRequested()

                            Dim stockName As String = runningStock.Item1
                            Dim row As DataRow = ret.NewRow
                            row("Date") = tradingDate.ToString("dd-MM-yyyy")
                            row("Trading Symbol") = atrStockList(stockName).TradingSymbol
                            row("Lot Size") = atrStockList(stockName).LotSize
                            row("ATR %") = Math.Round(atrStockList(stockName).ATRPercentage, 4)
                            row("Blank Candle %") = atrStockList(stockName).BlankCandlePercentage
                            row("Day ATR") = Math.Round(atrStockList(stockName).DayATR, 4)
                            row("Previous Day Open") = atrStockList(stockName).PreviousDayOpen
                            row("Previous Day Low") = atrStockList(stockName).PreviousDayLow
                            row("Previous Day High") = atrStockList(stockName).PreviousDayHigh
                            row("Previous Day Close") = atrStockList(stockName).PreviousDayClose
                            row("Slab") = atrStockList(stockName).Slab
                            row("Signal Time") = runningStock.Item2.ToString("HH:mm:ss")
                            row("LTF Signal") = runningStock.Item3
                            row("MTF SIgnal") = runningStock.Item4
                            row("HTF Signal") = runningStock.Item5

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

    Public Function GetSignalName(ByVal hkCandle As Payload) As String
        If hkCandle.CandleStrengthHeikenAshi = Payload.StrongCandle.Bullish Then
            Return "Strong Buy"
        ElseIf hkCandle.CandleStrengthHeikenAshi = Payload.StrongCandle.Bearish Then
            Return "Strong Sell"
        Else
            If hkCandle.CandleColor = Color.Green Then
                Return "Weak Buy"
            Else
                Return "Weak Sell"
            End If
        End If
    End Function

    Public Function GetCurrentXMinuteCandleTime(ByVal lowerTFTime As Date, timeframe As Integer) As Date
        Dim ret As Date = Nothing
        Dim exchangeStartTime As Date = New Date(Now.Year, Now.Month, Now.Day, 9, 15, 0)
        Dim timeframeToCheck As Integer = timeframe
        If exchangeStartTime.Minute Mod timeframeToCheck = 0 Then
            ret = New Date(lowerTFTime.Year, lowerTFTime.Month, lowerTFTime.Day, lowerTFTime.Hour, Math.Floor(lowerTFTime.Minute / timeframeToCheck) * timeframeToCheck, 0)
        Else
            Dim exchangeTime As Date = New Date(lowerTFTime.Year, lowerTFTime.Month, lowerTFTime.Day, exchangeStartTime.Hour, exchangeStartTime.Minute, 0)
            Dim currentTime As Date = New Date(lowerTFTime.Year, lowerTFTime.Month, lowerTFTime.Day, lowerTFTime.Hour, lowerTFTime.Minute, 0)
            Dim timeDifference As Double = currentTime.Subtract(exchangeTime).TotalMinutes
            Dim adjustedTimeDifference As Integer = Math.Floor(timeDifference / timeframeToCheck) * timeframeToCheck
            ret = exchangeTime.AddMinutes(adjustedTimeDifference)
        End If
        Return ret
    End Function
End Class
