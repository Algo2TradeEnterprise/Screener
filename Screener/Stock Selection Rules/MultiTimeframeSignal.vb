Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class MultiTimeframeSignal
    Inherits StockSelection

    Enum TypeOfIndicator
        TII = 1
        COLOR
        AROON
        EMA_13
        FRACTAL
        SUPERTREND
        SWING_HIGH_LOW
    End Enum

    Private ReadOnly _indicatorType As TypeOfIndicator
    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer,
                   ByVal indicatorType As Integer)
        MyBase.New(canceller, cmn, stockType)
        _indicatorType = indicatorType + 1
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
        ret.Columns.Add("Weekly")
        ret.Columns.Add("Daily")
        ret.Columns.Add("Hourly")
        ret.Columns.Add("15 Minutes")
        ret.Columns.Add("Overall")

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
                        Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_eodTable, runningStock, tradingDate.AddMonths(-30), tradingDate.AddDays(-1))
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, runningStock, tradingDate.AddDays(-50), tradingDate.AddDays(-1))
                        If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
                            Dim weeklyPayload As Dictionary(Of Date, Payload) = Common.ConvertDayPayloadsToWeek(eodPayload)

                            If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 700 AndAlso weeklyPayload IsNot Nothing AndAlso weeklyPayload.Count > 100 Then
                                Dim hourlyPayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(intradayPayload, 60, New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0))
                                Dim xMinutePayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(intradayPayload, 15, New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0))
                                If weeklyPayload IsNot Nothing AndAlso weeklyPayload.Count > 0 AndAlso hourlyPayload IsNot Nothing AndAlso hourlyPayload.Count > 0 Then
                                    Dim lastWeekPayload As Payload = weeklyPayload.LastOrDefault.Value
                                    Dim currentWeek As Date = Common.GetStartDateOfTheWeek(tradingDate, DayOfWeek.Monday)
                                    If lastWeekPayload.PayloadDate = currentWeek Then lastWeekPayload = weeklyPayload.LastOrDefault.Value.PreviousCandlePayload
                                    Dim lastDayPayload As Payload = eodPayload.LastOrDefault.Value
                                    Dim lastHourPayload As Payload = hourlyPayload.LastOrDefault.Value
                                    Dim lastMinPayload As Payload = xMinutePayload.LastOrDefault.Value

                                    If _indicatorType = TypeOfIndicator.COLOR Then
                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock,
                                                          {lastWeekPayload.CandleColor.Name,
                                                           lastDayPayload.CandleColor.Name,
                                                           lastHourPayload.PreviousCandlePayload.CandleColor.Name,
                                                           lastMinPayload.CandleColor.Name})
                                    ElseIf _indicatorType = TypeOfIndicator.SUPERTREND Then
                                        Dim weeklySupertrend As Dictionary(Of Date, Color) = Nothing
                                        Dim dailySupertrend As Dictionary(Of Date, Color) = Nothing
                                        Dim hourlySupertrend As Dictionary(Of Date, Color) = Nothing
                                        Dim xMinuteSupertrend As Dictionary(Of Date, Color) = Nothing

                                        Indicator.Supertrend.CalculateSupertrend(7, 3, weeklyPayload, Nothing, weeklySupertrend)
                                        Indicator.Supertrend.CalculateSupertrend(7, 3, eodPayload, Nothing, dailySupertrend)
                                        Indicator.Supertrend.CalculateSupertrend(7, 3, hourlyPayload, Nothing, hourlySupertrend)
                                        Indicator.Supertrend.CalculateSupertrend(7, 3, xMinutePayload, Nothing, xMinuteSupertrend)

                                        Dim weeklyTrend As Color = weeklySupertrend(lastWeekPayload.PayloadDate)
                                        Dim dailyTrend As Color = dailySupertrend(lastDayPayload.PayloadDate)
                                        Dim hourlyTrend As Color = hourlySupertrend(lastHourPayload.PayloadDate)
                                        Dim xMinuteTrend As Color = xMinuteSupertrend(lastMinPayload.PayloadDate)

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock, {weeklyTrend.Name, dailyTrend.Name, hourlyTrend.Name, xMinuteTrend.Name})
                                    ElseIf _indicatorType = TypeOfIndicator.TII Then
                                        Dim weeklyTII As Dictionary(Of Date, Decimal) = Nothing
                                        Dim weeklySignal As Dictionary(Of Date, Decimal) = Nothing
                                        Dim dailyTII As Dictionary(Of Date, Decimal) = Nothing
                                        Dim dailySignal As Dictionary(Of Date, Decimal) = Nothing
                                        Dim hourlyTII As Dictionary(Of Date, Decimal) = Nothing
                                        Dim hourlySignal As Dictionary(Of Date, Decimal) = Nothing
                                        Dim xMinuteTII As Dictionary(Of Date, Decimal) = Nothing
                                        Dim xMinuteSignal As Dictionary(Of Date, Decimal) = Nothing

                                        Indicator.TrendIntensityIndex.CalculateTII(Payload.PayloadFields.Close, 14, 9, weeklyPayload, weeklyTII, weeklySignal)
                                        Indicator.TrendIntensityIndex.CalculateTII(Payload.PayloadFields.Close, 14, 9, eodPayload, dailyTII, dailySignal)
                                        Indicator.TrendIntensityIndex.CalculateTII(Payload.PayloadFields.Close, 14, 9, hourlyPayload, hourlyTII, hourlySignal)
                                        Indicator.TrendIntensityIndex.CalculateTII(Payload.PayloadFields.Close, 14, 9, xMinutePayload, xMinuteTII, xMinuteSignal)

                                        Dim weeklyTrend As Color = Color.White
                                        Dim dailyTrend As Color = Color.White
                                        Dim hourlyTrend As Color = Color.White
                                        Dim xMinuteTrend As Color = Color.White
                                        If weeklyTII(lastWeekPayload.PayloadDate) >= 100 Then
                                            weeklyTrend = Color.Green
                                        ElseIf weeklyTII(lastWeekPayload.PayloadDate) <= 0 Then
                                            weeklyTrend = Color.Red
                                        End If
                                        If dailyTII(lastDayPayload.PayloadDate) >= 100 Then
                                            dailyTrend = Color.Green
                                        ElseIf dailyTII(lastDayPayload.PayloadDate) <= 0 Then
                                            dailyTrend = Color.Red
                                        End If
                                        If hourlyTII(lastHourPayload.PayloadDate) >= 100 Then
                                            hourlyTrend = Color.Green
                                        ElseIf hourlyTII(lastHourPayload.PayloadDate) <= 0 Then
                                            hourlyTrend = Color.Red
                                        End If
                                        If xMinuteTII(lastMinPayload.PayloadDate) >= 100 Then
                                            xMinuteTrend = Color.Green
                                        ElseIf xMinuteTII(lastMinPayload.PayloadDate) <= 0 Then
                                            xMinuteTrend = Color.Red
                                        End If

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock, {weeklyTrend.Name, dailyTrend.Name, hourlyTrend.Name, xMinuteTrend.Name})
                                    ElseIf _indicatorType = TypeOfIndicator.EMA_13 Then
                                        Dim weeklyEMA As Dictionary(Of Date, Decimal) = Nothing
                                        Dim dailyEMA As Dictionary(Of Date, Decimal) = Nothing
                                        Dim hourlyEMA As Dictionary(Of Date, Decimal) = Nothing
                                        Dim xMinuteEMA As Dictionary(Of Date, Decimal) = Nothing

                                        Indicator.EMA.CalculateEMA(13, Payload.PayloadFields.Close, weeklyPayload, weeklyEMA)
                                        Indicator.EMA.CalculateEMA(13, Payload.PayloadFields.Close, eodPayload, dailyEMA)
                                        Indicator.EMA.CalculateEMA(13, Payload.PayloadFields.Close, hourlyPayload, hourlyEMA)
                                        Indicator.EMA.CalculateEMA(13, Payload.PayloadFields.Close, xMinutePayload, xMinuteEMA)

                                        Dim weeklyTrend As Color = Color.White
                                        Dim dailyTrend As Color = Color.White
                                        Dim hourlyTrend As Color = Color.White
                                        Dim xMinuteTrend As Color = Color.White
                                        If lastWeekPayload.Close > weeklyEMA(lastWeekPayload.PayloadDate) Then
                                            weeklyTrend = Color.Green
                                        ElseIf lastWeekPayload.Close < weeklyEMA(lastWeekPayload.PayloadDate) Then
                                            weeklyTrend = Color.Red
                                        End If
                                        If lastDayPayload.Close > dailyEMA(lastDayPayload.PayloadDate) Then
                                            dailyTrend = Color.Green
                                        ElseIf lastDayPayload.Close < dailyEMA(lastDayPayload.PayloadDate) Then
                                            dailyTrend = Color.Red
                                        End If
                                        If lastHourPayload.Close > hourlyEMA(lastHourPayload.PayloadDate) Then
                                            hourlyTrend = Color.Green
                                        ElseIf lastHourPayload.Close < hourlyEMA(lastHourPayload.PayloadDate) Then
                                            hourlyTrend = Color.Red
                                        End If
                                        If lastMinPayload.Close > xMinuteEMA(lastMinPayload.PayloadDate) Then
                                            xMinuteTrend = Color.Green
                                        ElseIf lastMinPayload.Close < xMinuteEMA(lastMinPayload.PayloadDate) Then
                                            xMinuteTrend = Color.Red
                                        End If

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock, {weeklyTrend.Name, dailyTrend.Name, hourlyTrend.Name, xMinuteTrend.Name})
                                    ElseIf _indicatorType = TypeOfIndicator.AROON Then
                                        Dim weeklyHighDonchian As Dictionary(Of Date, Decimal) = Nothing
                                        Dim weeklyLowDonchian As Dictionary(Of Date, Decimal) = Nothing
                                        Dim dailyHighDonchian As Dictionary(Of Date, Decimal) = Nothing
                                        Dim dailyLowDonchian As Dictionary(Of Date, Decimal) = Nothing
                                        Dim hourlyHighDonchian As Dictionary(Of Date, Decimal) = Nothing
                                        Dim hourlyLowDonchian As Dictionary(Of Date, Decimal) = Nothing
                                        Dim xMinuteHighDonchian As Dictionary(Of Date, Decimal) = Nothing
                                        Dim xMinuteLowDonchian As Dictionary(Of Date, Decimal) = Nothing

                                        Indicator.DonchianChannel.CalculateDonchianChannel(14, 14, weeklyPayload, weeklyHighDonchian, weeklyLowDonchian, Nothing)
                                        Indicator.DonchianChannel.CalculateDonchianChannel(14, 14, eodPayload, dailyHighDonchian, dailyLowDonchian, Nothing)
                                        Indicator.DonchianChannel.CalculateDonchianChannel(14, 14, hourlyPayload, hourlyHighDonchian, hourlyLowDonchian, Nothing)
                                        Indicator.DonchianChannel.CalculateDonchianChannel(14, 14, xMinutePayload, xMinuteHighDonchian, xMinuteLowDonchian, Nothing)

                                        Dim weeklyTrend As Color = Color.White
                                        Dim dailyTrend As Color = Color.White
                                        Dim hourlyTrend As Color = Color.White
                                        Dim xMinuteTrend As Color = Color.White
                                        For Each runningPayload In weeklyPayload.OrderByDescending(Function(x)
                                                                                                       Return x.Key
                                                                                                   End Function)
                                            If runningPayload.Key <= lastWeekPayload.PayloadDate Then
                                                If runningPayload.Value.High > weeklyHighDonchian(runningPayload.Value.PayloadDate) Then
                                                    weeklyTrend = Color.Green
                                                    Exit For
                                                ElseIf runningPayload.Value.Low < weeklyLowDonchian(runningPayload.Value.PayloadDate) Then
                                                    weeklyTrend = Color.Red
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                        For Each runningPayload In eodPayload.OrderByDescending(Function(x)
                                                                                                    Return x.Key
                                                                                                End Function)
                                            If runningPayload.Key <= lastDayPayload.PayloadDate Then
                                                If runningPayload.Value.High > dailyHighDonchian(runningPayload.Value.PayloadDate) Then
                                                    dailyTrend = Color.Green
                                                    Exit For
                                                ElseIf runningPayload.Value.Low < dailyLowDonchian(runningPayload.Value.PayloadDate) Then
                                                    dailyTrend = Color.Red
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                        For Each runningPayload In hourlyPayload.OrderByDescending(Function(x)
                                                                                                       Return x.Key
                                                                                                   End Function)
                                            If runningPayload.Key <= lastHourPayload.PayloadDate Then
                                                If runningPayload.Value.High > hourlyHighDonchian(runningPayload.Value.PayloadDate) Then
                                                    hourlyTrend = Color.Green
                                                    Exit For
                                                ElseIf runningPayload.Value.Low < hourlyLowDonchian(runningPayload.Value.PayloadDate) Then
                                                    hourlyTrend = Color.Red
                                                    Exit For
                                                End If
                                            End If
                                        Next
                                        For Each runningPayload In xMinutePayload.OrderByDescending(Function(x)
                                                                                                        Return x.Key
                                                                                                    End Function)
                                            If runningPayload.Key <= lastMinPayload.PayloadDate Then
                                                If runningPayload.Value.High > xMinuteHighDonchian(runningPayload.Value.PayloadDate) Then
                                                    xMinuteTrend = Color.Green
                                                    Exit For
                                                ElseIf runningPayload.Value.Low < xMinuteLowDonchian(runningPayload.Value.PayloadDate) Then
                                                    xMinuteTrend = Color.Red
                                                    Exit For
                                                End If
                                            End If
                                        Next

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock, {weeklyTrend.Name, dailyTrend.Name, hourlyTrend.Name, xMinuteTrend.Name})
                                    ElseIf _indicatorType = TypeOfIndicator.FRACTAL Then
                                        Dim weeklyHighFractal As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim weeklyLowFractal As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim dailyHighFractal As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim dailyLowFractal As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim hourlyHighFractal As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim hourlyLowFractal As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim xMinuteHighFractal As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim xMinuteLowFractal As Dictionary(Of Date, TrendLineVeriables) = Nothing

                                        Indicator.FractalUTrendLine.CalculateFractalUTrendLine(weeklyPayload, weeklyHighFractal, weeklyLowFractal, Nothing, Nothing)
                                        Indicator.FractalUTrendLine.CalculateFractalUTrendLine(eodPayload, dailyHighFractal, dailyLowFractal, Nothing, Nothing)
                                        Indicator.FractalUTrendLine.CalculateFractalUTrendLine(hourlyPayload, hourlyHighFractal, hourlyLowFractal, Nothing, Nothing)
                                        Indicator.FractalUTrendLine.CalculateFractalUTrendLine(xMinutePayload, xMinuteHighFractal, xMinuteLowFractal, Nothing, Nothing)

                                        Dim weeklyTrend As Color = Color.White
                                        Dim dailyTrend As Color = Color.White
                                        Dim hourlyTrend As Color = Color.White
                                        Dim xMinuteTrend As Color = Color.White

                                        If weeklyHighFractal(lastWeekPayload.PayloadDate) IsNot Nothing AndAlso
                                            weeklyHighFractal(lastWeekPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastWeekPayload.Close > weeklyHighFractal(lastWeekPayload.PayloadDate).CurrentValue Then
                                            weeklyTrend = Color.Green
                                        ElseIf weeklyLowFractal(lastWeekPayload.PayloadDate) IsNot Nothing AndAlso
                                            weeklyLowFractal(lastWeekPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastWeekPayload.Close < weeklyLowFractal(lastWeekPayload.PayloadDate).CurrentValue Then
                                            weeklyTrend = Color.Red
                                        End If
                                        If dailyHighFractal(lastDayPayload.PayloadDate) IsNot Nothing AndAlso
                                            dailyHighFractal(lastDayPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastDayPayload.Close > dailyHighFractal(lastDayPayload.PayloadDate).CurrentValue Then
                                            dailyTrend = Color.Green
                                        ElseIf dailyLowFractal(lastDayPayload.PayloadDate) IsNot Nothing AndAlso
                                            dailyLowFractal(lastDayPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastDayPayload.Close < dailyLowFractal(lastDayPayload.PayloadDate).CurrentValue Then
                                            dailyTrend = Color.Red
                                        End If
                                        If hourlyHighFractal(lastHourPayload.PayloadDate) IsNot Nothing AndAlso
                                            hourlyHighFractal(lastHourPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastHourPayload.Close > hourlyHighFractal(lastHourPayload.PayloadDate).CurrentValue Then
                                            hourlyTrend = Color.Green
                                        ElseIf hourlyLowFractal(lastHourPayload.PayloadDate) IsNot Nothing AndAlso
                                            hourlyLowFractal(lastHourPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastHourPayload.Close < hourlyLowFractal(lastHourPayload.PayloadDate).CurrentValue Then
                                            hourlyTrend = Color.Red
                                        End If
                                        If xMinuteHighFractal(lastMinPayload.PayloadDate) IsNot Nothing AndAlso
                                            xMinuteHighFractal(lastMinPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastMinPayload.Close > xMinuteHighFractal(lastMinPayload.PayloadDate).CurrentValue Then
                                            xMinuteTrend = Color.Green
                                        ElseIf xMinuteLowFractal(lastMinPayload.PayloadDate) IsNot Nothing AndAlso
                                            xMinuteLowFractal(lastMinPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastMinPayload.Close < xMinuteLowFractal(lastMinPayload.PayloadDate).CurrentValue Then
                                            xMinuteTrend = Color.Red
                                        End If

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock, {weeklyTrend.Name, dailyTrend.Name, hourlyTrend.Name, xMinuteTrend.Name})
                                    ElseIf _indicatorType = TypeOfIndicator.SWING_HIGH_LOW Then
                                        Dim weeklyHighSwing As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim weeklyLowSwing As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim dailyHighSwing As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim dailyLowSwing As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim hourlyHighSwing As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim hourlyLowSwing As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim xMinuteHighSwing As Dictionary(Of Date, TrendLineVeriables) = Nothing
                                        Dim xMinuteLowSwing As Dictionary(Of Date, TrendLineVeriables) = Nothing

                                        Indicator.SwingHighLowTrendLine.CalculateSwingHighLowTrendLine(weeklyPayload, weeklyHighSwing, weeklyLowSwing, Nothing, Nothing)
                                        Indicator.SwingHighLowTrendLine.CalculateSwingHighLowTrendLine(eodPayload, dailyHighSwing, dailyLowSwing, Nothing, Nothing)
                                        Indicator.SwingHighLowTrendLine.CalculateSwingHighLowTrendLine(hourlyPayload, hourlyHighSwing, hourlyLowSwing, Nothing, Nothing)
                                        Indicator.SwingHighLowTrendLine.CalculateSwingHighLowTrendLine(xMinutePayload, xMinuteHighSwing, xMinuteLowSwing, Nothing, Nothing)

                                        Dim weeklyTrend As Color = Color.White
                                        Dim dailyTrend As Color = Color.White
                                        Dim hourlyTrend As Color = Color.White
                                        Dim xMinuteTrend As Color = Color.White

                                        If weeklyHighSwing(lastWeekPayload.PayloadDate) IsNot Nothing AndAlso
                                            weeklyHighSwing(lastWeekPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastWeekPayload.Close > weeklyHighSwing(lastWeekPayload.PayloadDate).CurrentValue Then
                                            weeklyTrend = Color.Green
                                        ElseIf weeklyLowSwing(lastWeekPayload.PayloadDate) IsNot Nothing AndAlso
                                            weeklyLowSwing(lastWeekPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastWeekPayload.Close < weeklyLowSwing(lastWeekPayload.PayloadDate).CurrentValue Then
                                            weeklyTrend = Color.Red
                                        End If
                                        If dailyHighSwing(lastDayPayload.PayloadDate) IsNot Nothing AndAlso
                                            dailyHighSwing(lastDayPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastDayPayload.Close > dailyHighSwing(lastDayPayload.PayloadDate).CurrentValue Then
                                            dailyTrend = Color.Green
                                        ElseIf dailyLowSwing(lastDayPayload.PayloadDate) IsNot Nothing AndAlso
                                            dailyLowSwing(lastDayPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastDayPayload.Close < dailyLowSwing(lastDayPayload.PayloadDate).CurrentValue Then
                                            dailyTrend = Color.Red
                                        End If
                                        If hourlyHighSwing(lastHourPayload.PayloadDate) IsNot Nothing AndAlso
                                            hourlyHighSwing(lastHourPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastHourPayload.Close > hourlyHighSwing(lastHourPayload.PayloadDate).CurrentValue Then
                                            hourlyTrend = Color.Green
                                        ElseIf hourlyLowSwing(lastHourPayload.PayloadDate) IsNot Nothing AndAlso
                                            hourlyLowSwing(lastHourPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastHourPayload.Close < hourlyLowSwing(lastHourPayload.PayloadDate).CurrentValue Then
                                            hourlyTrend = Color.Red
                                        End If
                                        If xMinuteHighSwing(lastMinPayload.PayloadDate) IsNot Nothing AndAlso
                                            xMinuteHighSwing(lastMinPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastMinPayload.Close > xMinuteHighSwing(lastMinPayload.PayloadDate).CurrentValue Then
                                            xMinuteTrend = Color.Green
                                        ElseIf xMinuteLowSwing(lastMinPayload.PayloadDate) IsNot Nothing AndAlso
                                            xMinuteLowSwing(lastMinPayload.PayloadDate).CurrentValue <> Decimal.MinValue AndAlso
                                            lastMinPayload.Close < xMinuteLowSwing(lastMinPayload.PayloadDate).CurrentValue Then
                                            xMinuteTrend = Color.Red
                                        End If

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock, {weeklyTrend.Name, dailyTrend.Name, hourlyTrend.Name, xMinuteTrend.Name})
                                    End If
                                End If
                            End If
                        End If
                    Next
                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList
                            _canceller.Token.ThrowIfCancellationRequested()

                            Dim weeklyTrend As String = runningStock.Value(0)
                            Dim dailyTrend As String = runningStock.Value(1)
                            Dim hourlyTrend As String = runningStock.Value(2)
                            Dim xMinuteTrend As String = runningStock.Value(3)
                            Dim overallTrend As String = "White"
                            If weeklyTrend.Trim.ToUpper = dailyTrend.Trim.ToUpper AndAlso
                                dailyTrend.Trim.ToUpper = hourlyTrend.Trim.ToUpper AndAlso
                                hourlyTrend.Trim.ToUpper = xMinuteTrend.Trim.ToUpper Then
                                overallTrend = xMinuteTrend
                            End If

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
                            row("Weekly") = weeklyTrend
                            row("Daily") = dailyTrend
                            row("Hourly") = hourlyTrend
                            row("15 Minutes") = xMinuteTrend
                            row("Overall") = overallTrend

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
