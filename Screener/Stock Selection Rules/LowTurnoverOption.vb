Imports System.Threading
Imports Algo2TradeBLL

Public Class LowTurnoverOption
    Inherits StockSelection

    Private ReadOnly _stockName As String = "BANKNIFTY"
    Private ReadOnly _fetchDataFromLive As Boolean = True
    Private ReadOnly _timeframe As Integer = 1
    Private ReadOnly _maxBlankCandlePer As Decimal = 20
    Private ReadOnly _minTotalCandlePer As Decimal = 80
    Private ReadOnly _endTime As Date = New Date(Now.Year, Now.Month, Now.Day, 11, 15, 0)
    Private ReadOnly _strikePriceRangePer As Decimal = 10
    Private ReadOnly _maxFracatalDiffPer As Decimal = 33
    Private ReadOnly _minTargetPerTrade As Decimal = 500
    Private ReadOnly _minTurnover As Decimal = 2000
    Private ReadOnly _maxTurnover As Decimal = 10000
    Private ReadOnly _minVolumePer As Decimal = 10

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
        ret.Columns.Add("Puts_Calls")
        ret.Columns.Add("Time")
        ret.Columns.Add("Spot Price")
        ret.Columns.Add("Entry Price")
        ret.Columns.Add("Target Price")
        ret.Columns.Add("Quantity")
        ret.Columns.Add("Turnover")
        ret.Columns.Add("Total Volume")
        ret.Columns.Add("Previous Day Highest Volume")
        ret.Columns.Add("Volume %")

        Dim tradingDate As Date = startDate
        While tradingDate <= endDate
            _canceller.Token.ThrowIfCancellationRequested()
            Dim endTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, _endTime.Hour, _endTime.Minute, _endTime.Second)
            Dim tradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
            If tradingDay OrElse tradingDate.Date = Now.Date Then
                Dim previousTradingDay As Date = _cmn.GetPreviousTradingDay(Common.DataBaseTable.EOD_Futures, tradingDate)
                If previousTradingDay <> Date.MinValue Then
                    Dim futureTradingSymbol As String = _cmn.GetCurrentTradingSymbol(Common.DataBaseTable.Intraday_Futures, tradingDate, _stockName)
                    If futureTradingSymbol IsNot Nothing Then
                        Dim lotSize As Integer = _cmn.GetLotSize(Common.DataBaseTable.EOD_Futures, futureTradingSymbol, tradingDate)
                        If lotSize <> Integer.MinValue Then
                            Dim intradayPayload As Dictionary(Of Date, Payload) = Nothing
                            Dim eodPayload As Dictionary(Of Date, Payload) = Nothing
                            If _fetchDataFromLive Then
                                intradayPayload = Await _cmn.GetHistoricalDataAsync(Common.DataBaseTable.Intraday_Cash, "NIFTY BANK", tradingDate, tradingDate).ConfigureAwait(False)
                                eodPayload = Await _cmn.GetHistoricalDataAsync(Common.DataBaseTable.EOD_Cash, "NIFTY BANK", tradingDate.AddYears(-1), tradingDate).ConfigureAwait(False)
                            Else
                                intradayPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Cash, "NIFTY BANK", tradingDate, tradingDate)
                                eodPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Cash, "NIFTY BANK", tradingDate.AddYears(-1), tradingDate)
                            End If

                            If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 AndAlso intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 AndAlso
                                eodPayload.ContainsKey(tradingDate.Date) AndAlso eodPayload.ContainsKey(previousTradingDay.Date) Then
                                Dim spotXMinPayload As Dictionary(Of Date, Payload) = Nothing
                                If _timeframe > 1 Then
                                    spotXMinPayload = intradayPayload
                                Else
                                    Dim exchangeStartTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0)
                                    spotXMinPayload = Common.ConvertPayloadsToXMinutes(intradayPayload, _timeframe, exchangeStartTime)
                                End If

                                Dim hkPayload As Dictionary(Of Date, Payload) = Nothing
                                Indicator.HeikenAshi.ConvertToHeikenAshi(eodPayload, hkPayload)

                                Dim instrumentType As String = Nothing
                                If hkPayload(previousTradingDay.Date).CandleColor = Color.Green Then
                                    instrumentType = "CE"
                                ElseIf hkPayload(previousTradingDay.Date).CandleColor = Color.Red Then
                                    instrumentType = "PE"
                                End If

                                If instrumentType IsNot Nothing Then
                                    Dim optionContracts As Dictionary(Of Decimal, String) = Nothing
                                    Dim currentOptionContracts As Dictionary(Of Decimal, String) = Await GetOptionTradingSymbols(_stockName, instrumentType, tradingDate)
                                    Dim volumeCheckOptionContracts As Dictionary(Of Decimal, String) = Nothing
                                    If tradingDate.DayOfWeek = DayOfWeek.Thursday Then
                                        volumeCheckOptionContracts = Await GetPreviousOptionTradingSymbols(_stockName, instrumentType, tradingDate)
                                    Else
                                        volumeCheckOptionContracts = currentOptionContracts
                                    End If
                                    If volumeCheckOptionContracts IsNot Nothing AndAlso volumeCheckOptionContracts.Count > 0 Then
                                        For Each runningContract In volumeCheckOptionContracts
                                            Dim optionPayload As Dictionary(Of Date, Payload) = Nothing
                                            'If _fetchDataFromLive Then
                                            '    optionPayload = Await _cmn.GetHistoricalDataAsync(Common.DataBaseTable.Intraday_Futures_Options, runningContract.Value, previousTradingDay, previousTradingDay).ConfigureAwait(False)
                                            'Else
                                            optionPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures_Options, runningContract.Value, previousTradingDay, previousTradingDay)
                                            'End If
                                            If optionPayload IsNot Nothing AndAlso optionPayload.Count > 0 Then
                                                Dim numberOfBlankCandle As Integer = optionPayload.Where(Function(x)
                                                                                                             Return x.Value.Volume = 0 OrElse (x.Value.High = x.Value.Low)
                                                                                                         End Function).Count
                                                If (optionPayload.Count / 375) * 100 >= _minTotalCandlePer AndAlso
                                                    (numberOfBlankCandle / 375) * 100 <= _maxBlankCandlePer Then
                                                    If optionContracts Is Nothing Then optionContracts = New Dictionary(Of Decimal, String)
                                                    If tradingDate.DayOfWeek = DayOfWeek.Thursday Then
                                                        If currentOptionContracts.ContainsKey(runningContract.Key) Then
                                                            optionContracts.Add(runningContract.Key, currentOptionContracts(runningContract.Key))
                                                        End If
                                                    Else
                                                        optionContracts.Add(runningContract.Key, runningContract.Value)
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If

                                    If optionContracts IsNot Nothing AndAlso optionContracts.Count > 0 Then
                                        Dim optionData As Dictionary(Of String, Dictionary(Of Date, Payload)) = Nothing
                                        For Each runningContract In optionContracts.Values
                                            Dim optionPayload As Dictionary(Of Date, Payload) = Nothing
                                            If _fetchDataFromLive Then
                                                optionPayload = Await _cmn.GetHistoricalDataAsync(Common.DataBaseTable.Intraday_Futures_Options, runningContract, tradingDate.AddDays(-7), tradingDate).ConfigureAwait(False)
                                            Else
                                                optionPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures_Options, runningContract, tradingDate.AddDays(-7), tradingDate)
                                            End If

                                            Dim optionXMinPayload As Dictionary(Of Date, Payload) = Nothing
                                            If _timeframe > 1 Then
                                                optionXMinPayload = optionPayload
                                            Else
                                                Dim exchangeStartTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0)
                                                optionXMinPayload = Common.ConvertPayloadsToXMinutes(optionPayload, _timeframe, exchangeStartTime)
                                            End If

                                            If optionData Is Nothing Then optionData = New Dictionary(Of String, Dictionary(Of Date, Payload))
                                            optionData.Add(runningContract, optionXMinPayload)
                                        Next


                                        Dim previousDayMaxVolumePayloaod As Dictionary(Of Date, Long) = Nothing
                                        For Each runningCandle In spotXMinPayload
                                            Dim signalCandleTime As Date = runningCandle.Key
                                            If signalCandleTime <= endTime Then
                                                OnHeartbeat(String.Format("Checking previous day highest volume for {0}", signalCandleTime.ToString("HH:mm:ss")))
                                                Dim maxVolume As Long = Long.MinValue
                                                Dim previousDaySignalTime As Date = New Date(previousTradingDay.Year, previousTradingDay.Month, previousTradingDay.Day, signalCandleTime.Hour, signalCandleTime.Minute, signalCandleTime.Second)
                                                For Each runningContract In optionContracts.Values
                                                    Dim optionPayload As Dictionary(Of Date, Payload) = optionData(runningContract)
                                                    If optionPayload IsNot Nothing AndAlso optionPayload.ContainsKey(signalCandleTime) Then
                                                        Dim totalVolume As Long = optionPayload.Sum(Function(x)
                                                                                                        If x.Key.Date = previousTradingDay.Date AndAlso x.Key <= previousDaySignalTime Then
                                                                                                            Return x.Value.Volume
                                                                                                        Else
                                                                                                            Return 0
                                                                                                        End If
                                                                                                    End Function)
                                                        maxVolume = Math.Max(maxVolume, totalVolume)
                                                    End If
                                                Next
                                                If previousDayMaxVolumePayloaod Is Nothing Then previousDayMaxVolumePayloaod = New Dictionary(Of Date, Long)
                                                previousDayMaxVolumePayloaod.Add(signalCandleTime, maxVolume)
                                            End If
                                        Next

                                        For Each runningCandle In spotXMinPayload
                                            Dim signalCandleTime As Date = runningCandle.Key
                                            If signalCandleTime <= endTime Then
                                                OnHeartbeat(String.Format("Checking signal for {0}", signalCandleTime.ToString("dd-MMM-yyyy HH:mm:ss")))
                                                If intradayPayload.ContainsKey(signalCandleTime) Then
                                                    Dim spotPrice As Decimal = intradayPayload(signalCandleTime).Close
                                                    Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
                                                    For Each runningContract In optionContracts
                                                        If Math.Abs(runningContract.Key - spotPrice) <= spotPrice * _strikePriceRangePer / 100 Then
                                                            Dim optionPayload As Dictionary(Of Date, Payload) = optionData(runningContract.Value)
                                                            If optionPayload IsNot Nothing AndAlso optionPayload.ContainsKey(signalCandleTime) Then
                                                                Dim optionFractalHighPayload As Dictionary(Of Date, Decimal) = Nothing
                                                                Dim optionFractalLowPayload As Dictionary(Of Date, Decimal) = Nothing
                                                                Indicator.FractalBands.CalculateFractal(optionPayload, optionFractalHighPayload, optionFractalLowPayload)
                                                                Dim optionFractalHigh As Decimal = optionFractalHighPayload(signalCandleTime)
                                                                Dim optionFractalLow As Decimal = optionFractalLowPayload(signalCandleTime)
                                                                Dim optionSignalCandle As Payload = optionPayload(signalCandleTime)
                                                                If optionSignalCandle.Close < optionFractalLow AndAlso
                                                                    optionFractalHigh > optionFractalLow AndAlso
                                                                    (optionFractalHigh - optionFractalLow) <= optionFractalLow * _maxFracatalDiffPer / 100 AndAlso
                                                                    IsFractalChanged(optionFractalLowPayload, signalCandleTime) Then
                                                                    Dim potentialQuantity As Integer = Math.Ceiling((_minTurnover / optionFractalLow) / lotSize) * lotSize
                                                                    Dim potentialPL As Decimal = CalculatePL(optionFractalLow, optionFractalHigh, potentialQuantity)
                                                                    If potentialPL > 0 Then
                                                                        Dim quantity As Integer = CalculateQuantityFromTarget(optionFractalLow, optionFractalHigh, _minTargetPerTrade, lotSize)
                                                                        Dim turnover As Decimal = optionFractalLow * quantity
                                                                        If turnover >= _minTurnover AndAlso turnover <= _maxTurnover Then
                                                                            Dim totalVolume As Long = optionPayload.Sum(Function(x)
                                                                                                                            If x.Key.Date = tradingDate.Date AndAlso x.Key <= signalCandleTime Then
                                                                                                                                Return x.Value.Volume
                                                                                                                            Else
                                                                                                                                Return 0
                                                                                                                            End If
                                                                                                                        End Function)

                                                                            Dim volumePer As Decimal = Math.Round(totalVolume * 100 / previousDayMaxVolumePayloaod(signalCandleTime), 2)
                                                                            If volumePer >= _minVolumePer Then
                                                                                If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
                                                                                tempStockList.Add(runningContract.Value, {optionFractalLow, optionFractalHigh, quantity, turnover, totalVolume, previousDayMaxVolumePayloaod(signalCandleTime), volumePer})
                                                                            End If
                                                                        End If
                                                                    End If
                                                                End If
                                                            End If
                                                        End If
                                                    Next
                                                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                                                        For Each runningStock In tempStockList.OrderBy(Function(x)
                                                                                                           Return x.Value(3)
                                                                                                       End Function)
                                                            Dim row As DataRow = ret.NewRow
                                                            row("Date") = tradingDate.ToString("dd-MMM-yyyy")
                                                            row("Trading Symbol") = runningStock.Key
                                                            row("Lot Size") = lotSize
                                                            row("Puts_Calls") = instrumentType
                                                            row("Time") = signalCandleTime.ToString("HH:mm:ss")
                                                            row("Spot Price") = spotPrice
                                                            row("Entry Price") = runningStock.Value(0)
                                                            row("Target Price") = runningStock.Value(1)
                                                            row("Quantity") = runningStock.Value(2)
                                                            row("Turnover") = runningStock.Value(3)
                                                            row("Total Volume") = runningStock.Value(4)
                                                            row("Previous Day Highest Volume") = runningStock.Value(5)
                                                            row("Volume %") = runningStock.Value(6)
                                                            ret.Rows.Add(row)

                                                            Exit For
                                                        Next
                                                        Exit For
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If

            tradingDate = tradingDate.AddDays(1)
        End While
        Return ret
    End Function

    Private Function IsFractalChanged(ByVal fractalPayload As Dictionary(Of Date, Decimal), ByVal signalCandle As Date) As Boolean
        Dim ret As Boolean = False
        If fractalPayload IsNot Nothing AndAlso fractalPayload.Count > 0 Then
            Dim currentFractal As Decimal = fractalPayload(signalCandle)
            For Each runningPayload In fractalPayload.OrderByDescending(Function(x)
                                                                            Return x.Key
                                                                        End Function)
                If runningPayload.Key < signalCandle Then
                    Dim fractal As Decimal = fractalPayload(runningPayload.Key)
                    If fractal <> currentFractal Then
                        If runningPayload.Key.Date = signalCandle.Date Then ret = True
                        Exit For
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Private Async Function GetOptionTradingSymbols(ByVal stockName As String, ByVal instrumentType As String, ByVal tradingDate As Date) As Task(Of Dictionary(Of Decimal, String))
        Dim ret As Dictionary(Of Decimal, String) = Nothing
        Dim optionTradingSymbol As String = Nothing
        Dim startDateOfWeek As Date = Common.GetStartDateOfTheWeek(tradingDate, DayOfWeek.Monday)
        Dim nextThursday As Date = startDateOfWeek.AddDays(3)
        If tradingDate.DayOfWeek = DayOfWeek.Thursday Then
            nextThursday = tradingDate.AddDays(7)
        ElseIf tradingDate.DayOfWeek = DayOfWeek.Friday Then
            nextThursday = tradingDate.AddDays(6)
        ElseIf tradingDate.DayOfWeek = DayOfWeek.Saturday Then
            nextThursday = tradingDate.AddDays(5)
        End If
        Dim lastDayOfTheMonth As Date = New Date(tradingDate.Year, tradingDate.Month, Date.DaysInMonth(tradingDate.Year, tradingDate.Month))
        Dim lastThursDayOfTheMonth As Date = lastDayOfTheMonth
        While lastThursDayOfTheMonth.DayOfWeek <> DayOfWeek.Thursday
            lastThursDayOfTheMonth = lastThursDayOfTheMonth.AddDays(-1)
        End While
        If nextThursday = lastThursDayOfTheMonth Then
            optionTradingSymbol = String.Format("{0}{1}%{2}", stockName.ToUpper, nextThursday.ToString("yyMMM").ToUpper, instrumentType)
        Else
            Dim dateString As String = ""
            If nextThursday.Month > 9 Then
                dateString = String.Format("{0}{1}{2}", nextThursday.ToString("yy"), Microsoft.VisualBasic.Left(nextThursday.ToString("MMM"), 1), nextThursday.ToString("dd"))
            Else
                dateString = nextThursday.ToString("yyMdd")
            End If
            optionTradingSymbol = String.Format("{0}{1}%{2}", stockName.ToUpper, dateString.ToUpper, instrumentType)
        End If

        'Dim query As String = "SELECT DISTINCT(`TradingSymbol`) FROM `eod_prices_opt_futures` WHERE `TradingSymbol` LIKE '{0}' AND `SnapshotDate`='{1}'"
        Dim query As String = "SELECT DISTINCT(`TRADING_SYMBOL`) FROM `active_instruments_futures` WHERE `TRADING_SYMBOL` LIKE '{0}' AND `AS_ON_DATE`='{1}'"
        query = String.Format(query, optionTradingSymbol, tradingDate.ToString("yyyy-MM-dd"))
        Dim dt As DataTable = Await _cmn.RunSelectAsync(query).ConfigureAwait(False)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim i As Integer = 0
            While Not i = dt.Rows.Count()
                'If Not IsDBNull(dt.Rows(i).Item("TradingSymbol")) Then
                If Not IsDBNull(dt.Rows(i).Item("TRADING_SYMBOL")) Then
                    'Dim tradingSymbol As String = dt.Rows(i).Item("TradingSymbol")
                    Dim tradingSymbol As String = dt.Rows(i).Item("TRADING_SYMBOL")
                    Dim strikePrice As String = Utilities.Strings.GetTextBetween(stockName, instrumentType, tradingSymbol)
                    strikePrice = strikePrice.Substring(5)
                    If strikePrice IsNot Nothing AndAlso IsNumeric(strikePrice) Then
                        If ret Is Nothing Then ret = New Dictionary(Of Decimal, String)
                        ret.Add(Val(strikePrice), tradingSymbol)
                    End If
                End If
                i += 1
            End While
        End If
        Return ret
    End Function

    Private Async Function GetPreviousOptionTradingSymbols(ByVal stockName As String, ByVal instrumentType As String, ByVal tradingDate As Date) As Task(Of Dictionary(Of Decimal, String))
        Dim ret As Dictionary(Of Decimal, String) = Nothing
        Dim optionTradingSymbol As String = Nothing
        Dim startDateOfWeek As Date = Common.GetStartDateOfTheWeek(tradingDate, DayOfWeek.Monday)
        Dim nextThursday As Date = startDateOfWeek.AddDays(3)
        If tradingDate.DayOfWeek = DayOfWeek.Friday Then
            nextThursday = tradingDate.AddDays(6)
        ElseIf tradingDate.DayOfWeek = DayOfWeek.Saturday Then
            nextThursday = tradingDate.AddDays(5)
        End If
        Dim lastDayOfTheMonth As Date = New Date(tradingDate.Year, tradingDate.Month, Date.DaysInMonth(tradingDate.Year, tradingDate.Month))
        Dim lastThursDayOfTheMonth As Date = lastDayOfTheMonth
        While lastThursDayOfTheMonth.DayOfWeek <> DayOfWeek.Thursday
            lastThursDayOfTheMonth = lastThursDayOfTheMonth.AddDays(-1)
        End While
        If nextThursday = lastThursDayOfTheMonth Then
            optionTradingSymbol = String.Format("{0}{1}%{2}", stockName.ToUpper, nextThursday.ToString("yyMMM").ToUpper, instrumentType)
        Else
            Dim dateString As String = ""
            If nextThursday.Month > 9 Then
                dateString = String.Format("{0}{1}{2}", nextThursday.ToString("yy"), Microsoft.VisualBasic.Left(nextThursday.ToString("MMM"), 1), nextThursday.ToString("dd"))
            Else
                dateString = nextThursday.ToString("yyMdd")
            End If
            optionTradingSymbol = String.Format("{0}{1}%{2}", stockName.ToUpper, dateString.ToUpper, instrumentType)
        End If

        'Dim query As String = "SELECT DISTINCT(`TradingSymbol`) FROM `eod_prices_opt_futures` WHERE `TradingSymbol` LIKE '{0}' AND `SnapshotDate`='{1}'"
        Dim query As String = "SELECT DISTINCT(`TRADING_SYMBOL`) FROM `active_instruments_futures` WHERE `TRADING_SYMBOL` LIKE '{0}' AND `AS_ON_DATE`='{1}'"
        query = String.Format(query, optionTradingSymbol, tradingDate.ToString("yyyy-MM-dd"))
        Dim dt As DataTable = Await _cmn.RunSelectAsync(query).ConfigureAwait(False)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim i As Integer = 0
            While Not i = dt.Rows.Count()
                'If Not IsDBNull(dt.Rows(i).Item("TradingSymbol")) Then
                If Not IsDBNull(dt.Rows(i).Item("TRADING_SYMBOL")) Then
                    'Dim tradingSymbol As String = dt.Rows(i).Item("TradingSymbol")
                    Dim tradingSymbol As String = dt.Rows(i).Item("TRADING_SYMBOL")
                    Dim strikePrice As String = Utilities.Strings.GetTextBetween(stockName, instrumentType, tradingSymbol)
                    strikePrice = strikePrice.Substring(5)
                    If strikePrice IsNot Nothing AndAlso IsNumeric(strikePrice) Then
                        If ret Is Nothing Then ret = New Dictionary(Of Decimal, String)
                        ret.Add(Val(strikePrice), tradingSymbol)
                    End If
                End If
                i += 1
            End While
        End If
        Return ret
    End Function

    Public Function CalculateQuantityFromTarget(ByVal buyPrice As Decimal, ByVal targetPrice As Decimal, ByVal NetProfitOfTrade As Decimal, ByVal lotSize As Integer) As Integer
        Dim potentialBrokerage As Calculator.BrokerageAttributes = Nothing
        Dim calculator As New Calculator.BrokerageCalculator(_canceller)

        Dim quantity As Integer = 1
        For quantity = 1 To Integer.MaxValue
            calculator.FO_Options(buyPrice, targetPrice, quantity, potentialBrokerage)

            If potentialBrokerage IsNot Nothing AndAlso potentialBrokerage.NetProfitLoss > NetProfitOfTrade Then
                Exit For
            End If
        Next
        Return Math.Ceiling(quantity / lotSize) * lotSize
    End Function

    Public Function CalculatePL(ByVal buyPrice As Decimal, ByVal sellPrice As Decimal, ByVal quantity As Long) As Decimal
        Dim potentialBrokerage As Calculator.BrokerageAttributes = Nothing
        Dim calculator As New Calculator.BrokerageCalculator(_canceller)
        calculator.FO_Options(buyPrice, sellPrice, quantity, potentialBrokerage)
        Return potentialBrokerage.NetProfitLoss
    End Function

    Private Async Function IsTradableDay(ByVal tradingDate As Date) As Task(Of Boolean)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As Boolean = False
        Dim historicalData As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(Common.DataBaseTable.EOD_POSITIONAL, "JINDALSTEL", tradingDate, tradingDate)
        If historicalData IsNot Nothing AndAlso historicalData.Count > 0 Then
            ret = True
        End If
        Return ret
    End Function
End Class