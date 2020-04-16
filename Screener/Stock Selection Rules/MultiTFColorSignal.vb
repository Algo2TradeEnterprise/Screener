Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class MultiTFColorSignal
    Inherits StockSelection

    Enum TypeOfData
        COLOR = 1
        SUPERTREND
        TII
        EMA_13
        'SWING_HIGH_LOW
        'FRACTAL
        AROON
    End Enum

    Private _dataType As TypeOfData
    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer,
                   ByVal dataType As Integer)
        MyBase.New(canceller, cmn, stockType)
        _dataType = dataType + 1
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

                            If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                                Dim hourlyPayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(intradayPayload, 60, New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0))
                                Dim xMinutePayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(intradayPayload, 15, New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0))
                                If weeklyPayload IsNot Nothing AndAlso weeklyPayload.Count > 0 AndAlso hourlyPayload IsNot Nothing AndAlso hourlyPayload.Count > 0 Then
                                    Dim lastWeekPayload As Payload = weeklyPayload.LastOrDefault.Value
                                    Dim currentWeek As Date = Common.GetStartDateOfTheWeek(tradingDate, DayOfWeek.Monday)
                                    If lastWeekPayload.PayloadDate = currentWeek Then lastWeekPayload = weeklyPayload.LastOrDefault.Value.PreviousCandlePayload
                                    Dim lastDayPayload As Payload = eodPayload.LastOrDefault.Value
                                    Dim lastHourPayload As Payload = hourlyPayload.LastOrDefault.Value.PreviousCandlePayload
                                    Dim lastMinPayload As Payload = xMinutePayload.LastOrDefault.Value

                                    If _dataType = TypeOfData.COLOR Then
                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock,
                                                          {lastWeekPayload.CandleColor.Name,
                                                           lastDayPayload.CandleColor.Name,
                                                           lastHourPayload.CandleColor.Name,
                                                           lastMinPayload.CandleColor.Name})
                                    ElseIf _dataType = TypeOfData.SUPERTREND Then
                                        Dim weeklySupertrend As Dictionary(Of Date, Color) = Nothing
                                        Dim dailySupertrend As Dictionary(Of Date, Color) = Nothing
                                        Dim hourlySupertrend As Dictionary(Of Date, Color) = Nothing
                                        Dim xMinuteSupertrend As Dictionary(Of Date, Color) = Nothing

                                        Indicator.Supertrend.CalculateSupertrend(7, 3, weeklyPayload, Nothing, weeklySupertrend)
                                        Indicator.Supertrend.CalculateSupertrend(7, 3, eodPayload, Nothing, dailySupertrend)
                                        Indicator.Supertrend.CalculateSupertrend(7, 3, hourlyPayload, Nothing, hourlySupertrend)
                                        Indicator.Supertrend.CalculateSupertrend(7, 3, xMinutePayload, Nothing, xMinuteSupertrend)

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock,
                                                          {weeklySupertrend(lastWeekPayload.PayloadDate).Name,
                                                           dailySupertrend(lastDayPayload.PayloadDate).Name,
                                                           hourlySupertrend(lastHourPayload.PayloadDate).Name,
                                                           xMinuteSupertrend(lastMinPayload.PayloadDate).Name})
                                    ElseIf _dataType = TypeOfData.TII Then
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
                                        If weeklyTII(lastWeekPayload.PayloadDate) >= 80 Then
                                            weeklyTrend = Color.Green
                                        ElseIf weeklyTII(lastWeekPayload.PayloadDate) <= 20 Then
                                            weeklyTrend = Color.Red
                                        End If
                                        If dailyTII(lastDayPayload.PayloadDate) >= 80 Then
                                            dailyTrend = Color.Green
                                        ElseIf dailyTII(lastDayPayload.PayloadDate) <= 20 Then
                                            dailyTrend = Color.Red
                                        End If
                                        If hourlyTII(lastHourPayload.PayloadDate) >= 80 Then
                                            hourlyTrend = Color.Green
                                        ElseIf hourlyTII(lastHourPayload.PayloadDate) <= 20 Then
                                            hourlyTrend = Color.Red
                                        End If
                                        If xMinuteTII(lastMinPayload.PayloadDate) >= 80 Then
                                            xMinuteTrend = Color.Green
                                        ElseIf xMinuteTII(lastMinPayload.PayloadDate) <= 20 Then
                                            xMinuteTrend = Color.Red
                                        End If

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(runningStock, {weeklyTrend.Name, dailyTrend.Name, hourlyTrend.Name, xMinuteTrend.Name})
                                    ElseIf _dataType = TypeOfData.EMA_13 Then
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
                                    ElseIf _dataType = TypeOfData.AROON Then
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
                                        'ElseIf _dataType = TypeOfData.FRACTAL Then
                                        '    Dim weeklyHighFractal As Dictionary(Of Date, Decimal) = Nothing
                                        '    Dim weeklyLowFractal As Dictionary(Of Date, Decimal) = Nothing
                                        '    Dim dailyHighFractal As Dictionary(Of Date, Decimal) = Nothing
                                        '    Dim dailyLowFractal As Dictionary(Of Date, Decimal) = Nothing
                                        '    Dim hourlyHighFractal As Dictionary(Of Date, Decimal) = Nothing
                                        '    Dim hourlyLowFractal As Dictionary(Of Date, Decimal) = Nothing
                                        '    Dim xMinuteHighFractal As Dictionary(Of Date, Decimal) = Nothing
                                        '    Dim xMinuteLowFractal As Dictionary(Of Date, Decimal) = Nothing

                                        '    Indicator.FractalBands.CalculateFractal(weeklyPayload, weeklyHighFractal, weeklyLowFractal)
                                        '    Indicator.FractalBands.CalculateFractal(eodPayload, dailyHighFractal, dailyLowFractal)
                                        '    Indicator.FractalBands.CalculateFractal(hourlyPayload, hourlyHighFractal, hourlyLowFractal)
                                        '    Indicator.FractalBands.CalculateFractal(xMinutePayload, xMinuteHighFractal, xMinuteLowFractal)

                                        '    Dim weeklyTrend As Color = Color.White
                                        '    Dim dailyTrend As Color = Color.White
                                        '    Dim hourlyTrend As Color = Color.White
                                        '    Dim xMinuteTrend As Color = Color.White

                                        '    For Each runningPayload In weeklyPayload.OrderByDescending(Function(x)
                                        '                                                                   Return x.Key
                                        '                                                               End Function)
                                        '        If runningPayload.Key <= lastWeekPayload.PayloadDate Then
                                        '            If runningPayload.Value.High > weeklyHighFractal(runningPayload.Value.PayloadDate) Then
                                        '                weeklyTrend = Color.Green
                                        '                Exit For
                                        '            ElseIf runningPayload.Value.Low < weeklyLowFractal(runningPayload.Value.PayloadDate) Then
                                        '                weeklyTrend = Color.Red
                                        '                Exit For
                                        '            End If
                                        '        End If
                                        '    Next
                                        '    For Each runningPayload In eodPayload.OrderByDescending(Function(x)
                                        '                                                                Return x.Key
                                        '                                                            End Function)
                                        '        If runningPayload.Key <= lastDayPayload.PayloadDate Then
                                        '            If runningPayload.Value.High > dailyHighFractal(runningPayload.Value.PayloadDate) Then
                                        '                dailyTrend = Color.Green
                                        '                Exit For
                                        '            ElseIf runningPayload.Value.Low < dailyLowFractal(runningPayload.Value.PayloadDate) Then
                                        '                dailyTrend = Color.Red
                                        '                Exit For
                                        '            End If
                                        '        End If
                                        '    Next
                                        '    For Each runningPayload In hourlyPayload.OrderByDescending(Function(x)
                                        '                                                                   Return x.Key
                                        '                                                               End Function)
                                        '        If runningPayload.Key <= lastHourPayload.PayloadDate Then
                                        '            If runningPayload.Value.High > hourlyHighFractal(runningPayload.Value.PayloadDate) Then
                                        '                hourlyTrend = Color.Green
                                        '                Exit For
                                        '            ElseIf runningPayload.Value.Low < hourlyLowFractal(runningPayload.Value.PayloadDate) Then
                                        '                hourlyTrend = Color.Red
                                        '                Exit For
                                        '            End If
                                        '        End If
                                        '    Next
                                        '    For Each runningPayload In xMinutePayload.OrderByDescending(Function(x)
                                        '                                                                    Return x.Key
                                        '                                                                End Function)
                                        '        If runningPayload.Key <= lastMinPayload.PayloadDate Then
                                        '            If runningPayload.Value.High > xMinuteHighFractal(runningPayload.Value.PayloadDate) Then
                                        '                xMinuteTrend = Color.Green
                                        '                Exit For
                                        '            ElseIf runningPayload.Value.Low < xMinuteLowFractal(runningPayload.Value.PayloadDate) Then
                                        '                xMinuteTrend = Color.Red
                                        '                Exit For
                                        '            End If
                                        '        End If
                                        '    Next

                                        '    If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        '    tempStockList.Add(runningStock, {weeklyTrend.Name, dailyTrend.Name, hourlyTrend.Name, xMinuteTrend.Name})
                                    End If
                                End If
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
                            row("Weekly") = runningStock.Value(0)
                            row("Daily") = runningStock.Value(1)
                            row("Hourly") = runningStock.Value(2)
                            row("15 Minutes") = runningStock.Value(3)

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
