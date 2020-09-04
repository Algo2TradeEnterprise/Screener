Imports System.Threading
Imports Algo2TradeBLL

Public Class LowestPriceAtTheMoneyOptions
    Inherits StockSelection

    Private ReadOnly _stockName As String = "BANKNIFTY"
    Private ReadOnly _strikePriceGap As Decimal = 100
    Private ReadOnly _maximumPremium As Decimal = 500
    Private ReadOnly _numberOfStrikes As Integer = 5

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
        ret.Columns.Add("Price")
        ret.Columns.Add("Time")

        Dim tradingDate As Date = startDate
        While tradingDate <= endDate
            _canceller.Token.ThrowIfCancellationRequested()
            Dim tradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
            If tradingDay Then
                Dim previousTradingDay As Date = _cmn.GetPreviousTradingDay(Common.DataBaseTable.EOD_Futures, tradingDate)
                If previousTradingDay <> Date.MinValue Then
                    Dim startDateOfWeek As Date = Common.GetStartDateOfTheWeek(tradingDate, DayOfWeek.Monday)
                    Dim thursdayOfWeek As Date = startDateOfWeek.AddDays(3)
                    If tradingDate.DayOfWeek = DayOfWeek.Thursday Then
                        thursdayOfWeek = tradingDate.AddDays(7)
                    ElseIf tradingDate.DayOfWeek = DayOfWeek.Friday Then
                        thursdayOfWeek = tradingDate.AddDays(6)
                    ElseIf tradingDate.DayOfWeek = DayOfWeek.Saturday Then
                        thursdayOfWeek = tradingDate.AddDays(5)
                    End If

                    OnHeartbeat(String.Format("Getting option data for {0}", tradingDate.ToString("dd-MM-yyyy")))

                    Dim futureTradingSymbol As String = _cmn.GetCurrentTradingSymbol(Common.DataBaseTable.Intraday_Futures, tradingDate, _stockName)
                    If futureTradingSymbol IsNot Nothing Then
                        Dim lotSize As Integer = _cmn.GetLotSize(Common.DataBaseTable.Intraday_Futures, futureTradingSymbol, tradingDate)
                        Dim futureIntradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures, futureTradingSymbol, tradingDate, tradingDate)
                        If futureIntradayPayload IsNot Nothing AndAlso futureIntradayPayload.Count > 0 Then
                            Dim optionData As Dictionary(Of String, Payload) = Await GetOptionStockData(thursdayOfWeek, _stockName, previousTradingDay).ConfigureAwait(False)
                            If optionData IsNot Nothing AndAlso optionData.Count > 0 Then
                                Dim stockData As Dictionary(Of Decimal, Dictionary(Of String, Dictionary(Of Date, Payload))) = Nothing
                                For Each runningStock In optionData.Values
                                    Dim strikePrice As String = runningStock.TradingSymbol.Substring(_stockName.Count + 5, runningStock.TradingSymbol.Count - _stockName.Count - 7)
                                    Dim intrumentType As String = runningStock.TradingSymbol.Substring(runningStock.TradingSymbol.Count - 2).Trim

                                    Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures_Options, runningStock.TradingSymbol, tradingDate, tradingDate)
                                    If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                                        If stockData Is Nothing Then stockData = New Dictionary(Of Decimal, Dictionary(Of String, Dictionary(Of Date, Payload)))
                                        If Not stockData.ContainsKey(strikePrice) Then stockData.Add(strikePrice, New Dictionary(Of String, Dictionary(Of Date, Payload)))
                                        stockData(strikePrice).Add(intrumentType, intradayPayload)
                                    End If
                                Next
                                If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
                                    Dim usedStocklist As Dictionary(Of String, Payload) = Nothing
                                    For Each runningPayload In futureIntradayPayload
                                        Dim price As Decimal = runningPayload.Value.Close
                                        Dim strikePricesToCheck As List(Of Decimal) = New List(Of Decimal)
                                        If _numberOfStrikes Mod 2 = 0 Then
                                            Dim immediateUpperStrike As Decimal = Math.Ceiling(price / _strikePriceGap) * _strikePriceGap
                                            Dim immediateLowerStrike As Decimal = Math.Floor(price / _strikePriceGap) * _strikePriceGap
                                            strikePricesToCheck.Add(immediateUpperStrike)
                                            strikePricesToCheck.Add(immediateLowerStrike)
                                            For counter = 1 To (_numberOfStrikes / 2) - 1
                                                immediateUpperStrike = immediateUpperStrike + counter * _strikePriceGap
                                                immediateLowerStrike = immediateLowerStrike - counter * _strikePriceGap
                                                strikePricesToCheck.Add(immediateUpperStrike)
                                                strikePricesToCheck.Add(immediateLowerStrike)
                                            Next
                                        Else
                                            Dim immediateUpperStrike As Decimal = Math.Ceiling(price / _strikePriceGap) * _strikePriceGap
                                            Dim immediateLowerStrike As Decimal = Math.Floor(price / _strikePriceGap) * _strikePriceGap
                                            Dim intermidiateStrike As Decimal = Decimal.MinValue
                                            If immediateUpperStrike - price > price - immediateLowerStrike Then
                                                intermidiateStrike = immediateLowerStrike
                                            Else
                                                intermidiateStrike = immediateUpperStrike
                                            End If
                                            strikePricesToCheck.Add(intermidiateStrike)
                                            immediateUpperStrike = intermidiateStrike
                                            immediateLowerStrike = intermidiateStrike
                                            For counter = 1 To (_numberOfStrikes - 1) / 2
                                                immediateUpperStrike = immediateUpperStrike + counter * _strikePriceGap
                                                immediateLowerStrike = immediateLowerStrike - counter * _strikePriceGap
                                                strikePricesToCheck.Add(immediateUpperStrike)
                                                strikePricesToCheck.Add(immediateLowerStrike)
                                            Next
                                        End If
                                        If strikePricesToCheck IsNot Nothing AndAlso strikePricesToCheck.Count > 0 Then
                                            Dim strikePayloads As List(Of Payload) = New List(Of Payload)
                                            For Each runningStrike In strikePricesToCheck
                                                If stockData.ContainsKey(runningStrike) Then
                                                    For Each instrumentType In stockData(runningStrike).Values
                                                        If instrumentType.ContainsKey(runningPayload.Key) Then
                                                            strikePayloads.Add(instrumentType(runningPayload.Key))
                                                        End If
                                                    Next
                                                End If
                                            Next
                                            If strikePayloads IsNot Nothing AndAlso strikePayloads.Count > 0 Then
                                                Dim lowestPayload As Payload = Nothing
                                                For Each optionPayload In strikePayloads.OrderBy(Function(x)
                                                                                                     Return x.Close
                                                                                                 End Function)
                                                    lowestPayload = optionPayload
                                                    Exit For
                                                Next
                                                If usedStocklist Is Nothing Then usedStocklist = New Dictionary(Of String, Payload)
                                                If lowestPayload.Close <= _maximumPremium AndAlso Not usedStocklist.ContainsKey(lowestPayload.TradingSymbol) Then
                                                    usedStocklist.Add(lowestPayload.TradingSymbol, lowestPayload)
                                                End If
                                            End If
                                        End If
                                    Next
                                    If usedStocklist IsNot Nothing AndAlso usedStocklist.Count > 0 Then
                                        For Each runningPayload In usedStocklist
                                            Dim intrumentType As String = runningPayload.Value.TradingSymbol.Substring(runningPayload.Value.TradingSymbol.Count - 2).Trim
                                            Dim row As DataRow = ret.NewRow
                                            row("Date") = tradingDate.ToString("dd-MM-yyyy")
                                            row("Trading Symbol") = runningPayload.Value.TradingSymbol
                                            row("Lot Size") = lotSize
                                            row("Puts_Calls") = intrumentType
                                            row("Price") = runningPayload.Value.Close
                                            row("Time") = runningPayload.Value.PayloadDate.ToString("HH:mm:ss")
                                            ret.Rows.Add(row)
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

    Private Async Function GetOptionStockData(ByVal expiry As Date, ByVal rawInstrumentName As String, ByVal tradingDate As Date) As Task(Of Dictionary(Of String, Payload))
        Dim ret As Dictionary(Of String, Payload) = Nothing
        Dim tradingSymbol As String = ""
        Dim lastDayOfTheMonth As Date = New Date(tradingDate.Year, tradingDate.Month, Date.DaysInMonth(tradingDate.Year, tradingDate.Month))
        Dim lastThursDayOfTheMonth As Date = lastDayOfTheMonth
        While lastThursDayOfTheMonth.DayOfWeek <> DayOfWeek.Thursday
            lastThursDayOfTheMonth = lastThursDayOfTheMonth.AddDays(-1)
        End While
        If expiry = lastThursDayOfTheMonth Then
            tradingSymbol = String.Format("{0}{1}%", rawInstrumentName.ToUpper, expiry.ToString("yyMMM"))
        Else
            Dim dateString As String = ""
            If expiry.Month > 9 Then
                dateString = String.Format("{0}{1}{2}", expiry.ToString("yy"), Microsoft.VisualBasic.Left(expiry.ToString("MMM"), 1), expiry.ToString("dd"))
            Else
                dateString = expiry.ToString("yyMdd")
            End If
            tradingSymbol = String.Format("{0}{1}%", rawInstrumentName.ToUpper, dateString)
        End If

        Dim queryString As String = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol`,`OI`
                                                   FROM `eod_prices_opt_futures`
                                                   WHERE `TradingSymbol` LIKE '{0}'
                                                   AND `SnapshotDate`='{1}'",
                                                   tradingSymbol, tradingDate.ToString("yyyy-MM-dd"))

        Dim dt As DataTable = Await _cmn.RunSelectAsync(queryString).ConfigureAwait(False)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim i As Integer = 0
            ret = New Dictionary(Of String, Payload)
            While Not i = dt.Rows.Count()
                Dim tempPayload As Payload = New Payload(Payload.CandleDataSource.Chart)
                tempPayload.Open = dt.Rows(i).Item(0)
                tempPayload.Low = dt.Rows(i).Item(1)
                tempPayload.High = dt.Rows(i).Item(2)
                tempPayload.Close = dt.Rows(i).Item(3)
                tempPayload.Volume = dt.Rows(i).Item(4)
                tempPayload.PayloadDate = dt.Rows(i).Item(5)
                tempPayload.TradingSymbol = dt.Rows(i).Item(6)
                tempPayload.OI = dt.Rows(i).Item(7)

                ret.Add(tempPayload.TradingSymbol, tempPayload)
                i += 1
            End While
        End If
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