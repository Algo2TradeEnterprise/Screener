Imports System.Threading
Imports Algo2TradeBLL

Public Class DayOpenAtTheMoneyOptions
    Inherits StockSelection

    Private ReadOnly _stockName As String = "NIFTY"
    Private ReadOnly _strikePriceGap As Decimal = 50

    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer)
        MyBase.New(canceller, cmn, stockType)
    End Sub

    Public Overrides Async Function GetStockDataAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Future Trading Symbol")
        ret.Columns.Add("Lot Size")
        ret.Columns.Add("Future Open")
        ret.Columns.Add("Option Trading Symbol")
        ret.Columns.Add("Puts_Calls")
        ret.Columns.Add("Entry Price")
        ret.Columns.Add("Entry Time")
        ret.Columns.Add("Higest Price")
        ret.Columns.Add("Higest Price Time")
        ret.Columns.Add("Lowest Price")
        ret.Columns.Add("Lowest Price Time")
        ret.Columns.Add("EOD Price")

        Dim tradingDate As Date = startDate
        While tradingDate <= endDate
            _canceller.Token.ThrowIfCancellationRequested()
            Dim tradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
            If tradingDay Then
                Dim previousTradingDay As Date = _cmn.GetPreviousTradingDay(Common.DataBaseTable.EOD_Futures, tradingDate)
                If previousTradingDay <> Date.MinValue Then
                    Dim startDateOfWeek As Date = Common.GetStartDateOfTheWeek(tradingDate, DayOfWeek.Monday)
                    Dim thursdayOfWeek As Date = startDateOfWeek.AddDays(3)
                    If tradingDate.DayOfWeek = DayOfWeek.Friday Then
                        thursdayOfWeek = tradingDate.AddDays(6)
                    ElseIf tradingDate.DayOfWeek = DayOfWeek.Saturday Then
                        thursdayOfWeek = tradingDate.AddDays(5)
                    End If
                    Dim expiry As Date = thursdayOfWeek
                    Dim optionTradingSymbol As String = ""
                    Dim lastDayOfTheMonth As Date = New Date(tradingDate.Year, tradingDate.Month, Date.DaysInMonth(tradingDate.Year, tradingDate.Month))
                    Dim lastThursDayOfTheMonth As Date = lastDayOfTheMonth
                    While lastThursDayOfTheMonth.DayOfWeek <> DayOfWeek.Thursday
                        lastThursDayOfTheMonth = lastThursDayOfTheMonth.AddDays(-1)
                    End While
                    If expiry = lastThursDayOfTheMonth Then
                        optionTradingSymbol = String.Format("{0}{1}", _stockName.ToUpper, expiry.ToString("yyMMM"))
                    Else
                        Dim dateString As String = ""
                        If expiry.Month > 9 Then
                            dateString = String.Format("{0}{1}{2}", expiry.ToString("yy"), Microsoft.VisualBasic.Left(expiry.ToString("MMM"), 1), expiry.ToString("dd"))
                        Else
                            dateString = expiry.ToString("yyMdd")
                        End If
                        optionTradingSymbol = String.Format("{0}{1}", _stockName.ToUpper, dateString)
                    End If

                    Dim futureTradingSymbol As String = _cmn.GetCurrentTradingSymbol(Common.DataBaseTable.EOD_Futures, tradingDate, _stockName)
                    If futureTradingSymbol IsNot Nothing Then
                        Dim lotSize As Integer = _cmn.GetLotSize(Common.DataBaseTable.EOD_Futures, futureTradingSymbol, tradingDate)
                        Dim futureEODPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Futures, futureTradingSymbol, previousTradingDay, tradingDate)
                        If futureEODPayload IsNot Nothing AndAlso futureEODPayload.ContainsKey(tradingDate.Date) AndAlso futureEODPayload.ContainsKey(previousTradingDay.Date) Then
                            Dim currentDayFuturePayload As Payload = futureEODPayload(tradingDate.Date)
                            If currentDayFuturePayload.PreviousCandlePayload IsNot Nothing Then
                                Dim instrumentType As String = Nothing
                                If currentDayFuturePayload.PreviousCandlePayload.Close > currentDayFuturePayload.PreviousCandlePayload.Open Then
                                    instrumentType = "CE"
                                Else
                                    instrumentType = "PE"
                                End If
                                If instrumentType IsNot Nothing AndAlso instrumentType.Trim <> "" Then
                                    Dim price As Decimal = currentDayFuturePayload.Open
                                    Dim immediateStrike As Decimal = Decimal.MinValue
                                    If price Mod _strikePriceGap = 0 Then
                                        immediateStrike = price
                                    Else
                                        Dim immediateUpperStrike As Decimal = Math.Ceiling(price / _strikePriceGap) * _strikePriceGap
                                        Dim immediateLowerStrike As Decimal = Math.Floor(price / _strikePriceGap) * _strikePriceGap
                                        If immediateUpperStrike - price > price - immediateLowerStrike Then
                                            immediateStrike = immediateLowerStrike
                                        Else
                                            immediateStrike = immediateUpperStrike
                                        End If
                                    End If
                                    If immediateStrike <> Decimal.MinValue Then
                                        Dim strikePrice As Decimal = immediateStrike
                                        If instrumentType = "CE" Then
                                            strikePrice = immediateStrike + _strikePriceGap
                                        ElseIf instrumentType = "PE" Then
                                            strikePrice = immediateStrike - _strikePriceGap
                                        End If
                                        If strikePrice <> Decimal.MinValue Then
                                            optionTradingSymbol = String.Format("{0}{1}{2}", optionTradingSymbol, strikePrice, instrumentType)
                                            optionTradingSymbol = optionTradingSymbol.ToUpper
                                            Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures_Options, optionTradingSymbol, tradingDate.AddDays(-7), tradingDate)
                                            If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                                                Dim currentDayPayload As Dictionary(Of Date, Payload) = Nothing
                                                For Each runningPayload In intradayPayload
                                                    If runningPayload.Key.Date = tradingDate.Date Then
                                                        If currentDayPayload Is Nothing Then currentDayPayload = New Dictionary(Of Date, Payload)
                                                        currentDayPayload.Add(runningPayload.Key, runningPayload.Value)
                                                    End If
                                                Next
                                                If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 Then
                                                    Dim tradeStartTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 16, 0)
                                                    Dim eodExitTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 15, 15, 0)
                                                    Dim openPrice As Decimal = currentDayPayload.FirstOrDefault.Value.Open
                                                    'Dim smiPayload As Dictionary(Of Date, Decimal) = Nothing
                                                    'Indicator.SMI.CalculateSMI(10, 3, 3, 10, intradayPayload, smiPayload, Nothing)
                                                    'Dim bollingerHighPayload As Dictionary(Of Date, Decimal) = Nothing
                                                    'Indicator.BollingerBands.CalculateBollingerBands(20, Payload.PayloadFields.Close, 2, intradayPayload, bollingerHighPayload, Nothing, Nothing)

                                                    Dim entryPrice As Decimal = Decimal.MinValue
                                                    Dim entryTime As Date = Date.MinValue
                                                    Dim highestPrice As Decimal = Decimal.MinValue
                                                    Dim highestPriceTime As Date = Date.MinValue
                                                    Dim lowestPrice As Decimal = Decimal.MaxValue
                                                    Dim lowestPriceTime As Date = Date.MinValue
                                                    Dim eodPrice As Decimal = Decimal.MinValue

                                                    For Each runningPayload In currentDayPayload
                                                        If runningPayload.Key >= tradeStartTime AndAlso runningPayload.Key <= eodExitTime Then
                                                            If entryTime = Date.MinValue AndAlso runningPayload.Value.High >= openPrice Then
                                                                entryPrice = openPrice
                                                                entryTime = runningPayload.Key
                                                            End If
                                                            If entryTime <> Date.MinValue Then
                                                                If runningPayload.Value.High > highestPrice Then
                                                                    highestPrice = runningPayload.Value.High
                                                                    highestPriceTime = runningPayload.Key
                                                                End If
                                                                If runningPayload.Value.Low < lowestPrice Then
                                                                    lowestPrice = runningPayload.Value.Low
                                                                    lowestPriceTime = runningPayload.Key
                                                                End If
                                                                If runningPayload.Key >= eodExitTime Then
                                                                    eodPrice = runningPayload.Value.Open
                                                                    Exit For
                                                                End If
                                                            End If
                                                        End If
                                                    Next

                                                    If entryTime <> Date.MinValue Then
                                                        Dim row As DataRow = ret.NewRow
                                                        row("Date") = tradingDate.ToString("dd-MMM-yyyy")
                                                        row("Future Trading Symbol") = futureTradingSymbol
                                                        row("Lot Size") = lotSize
                                                        row("Future Open") = price
                                                        row("Option Trading Symbol") = optionTradingSymbol
                                                        row("Puts_Calls") = instrumentType
                                                        row("Entry Price") = entryPrice
                                                        row("Entry Time") = entryTime.ToString("dd-MMM-yyyy HH:mm:ss")
                                                        row("Higest Price") = highestPrice
                                                        row("Higest Price Time") = highestPriceTime.ToString("dd-MMM-yyyy HH:mm:ss")
                                                        row("Lowest Price") = lowestPrice
                                                        row("Lowest Price Time") = lowestPriceTime.ToString("dd-MMM-yyyy HH:mm:ss")
                                                        row("EOD Price") = eodPrice
                                                        ret.Rows.Add(row)
                                                    End If
                                                End If
                                            End If
                                        End If
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