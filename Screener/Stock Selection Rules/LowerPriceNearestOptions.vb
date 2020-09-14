Imports System.Threading
Imports Algo2TradeBLL

Public Class LowerPriceNearestOptions
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
        ret.Columns.Add("Time")
        ret.Columns.Add("Turnover")
        ret.Columns.Add("Turnover Ratio")

        Dim tradingDate As Date = startDate
        While tradingDate <= endDate
            _canceller.Token.ThrowIfCancellationRequested()
            Dim tradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
            If tradingDay Then
                Dim previousTradingDay As Date = _cmn.GetPreviousTradingDay(Common.DataBaseTable.EOD_Cash, tradingDate)
                If previousTradingDay <> Date.MinValue Then
                    Dim startDateOfWeek As Date = Common.GetStartDateOfTheWeek(tradingDate, DayOfWeek.Monday)
                    Dim expiry As Date = startDateOfWeek.AddDays(3)
                    If tradingDate.DayOfWeek = DayOfWeek.Thursday Then
                        expiry = tradingDate.AddDays(7)
                    ElseIf tradingDate.DayOfWeek = DayOfWeek.Friday Then
                        expiry = tradingDate.AddDays(6)
                    ElseIf tradingDate.DayOfWeek = DayOfWeek.Saturday Then
                        expiry = tradingDate.AddDays(5)
                    End If

                    Dim lastDayOfTheMonth As Date = New Date(tradingDate.Year, tradingDate.Month, Date.DaysInMonth(tradingDate.Year, tradingDate.Month))
                    Dim lastThursDayOfTheMonth As Date = lastDayOfTheMonth
                    While lastThursDayOfTheMonth.DayOfWeek <> DayOfWeek.Thursday
                        lastThursDayOfTheMonth = lastThursDayOfTheMonth.AddDays(-1)
                    End While


                    Dim nifty50Payload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Cash, "NIFTY 50", previousTradingDay, tradingDate)
                    Dim niftyBankPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Cash, "NIFTY BANK", previousTradingDay, tradingDate)
                    If nifty50Payload IsNot Nothing AndAlso nifty50Payload.Count AndAlso niftyBankPayload IsNot Nothing AndAlso niftyBankPayload.Count > 0 Then
                        Dim nifty50ChangePayload As Dictionary(Of Date, Decimal) = Nothing
                        Dim lastNifty50Open As Decimal = Decimal.MinValue
                        Dim lastNifty50Date As Date = Date.MinValue
                        For Each runningPayload In nifty50Payload
                            If lastNifty50Date = Date.MinValue OrElse lastNifty50Date.Date <> runningPayload.Key.Date Then
                                lastNifty50Date = runningPayload.Key.Date
                                lastNifty50Open = runningPayload.Value.Open
                            End If
                            If lastNifty50Date <> Date.MinValue AndAlso lastNifty50Open <> Decimal.MinValue Then
                                If nifty50ChangePayload Is Nothing Then nifty50ChangePayload = New Dictionary(Of Date, Decimal)
                                nifty50ChangePayload.Add(runningPayload.Key, ((runningPayload.Value.Close / lastNifty50Open) - 1) * 100)
                            End If
                        Next

                        Dim niftyBankChangePayload As Dictionary(Of Date, Decimal) = Nothing
                        Dim lastNiftyBankOpen As Decimal = Decimal.MinValue
                        Dim lastNiftyBankDate As Date = Date.MinValue
                        For Each runningPayload In niftyBankPayload
                            If lastNiftyBankDate = Date.MinValue OrElse lastNiftyBankDate.Date <> runningPayload.Key.Date Then
                                lastNiftyBankDate = runningPayload.Key.Date
                                lastNiftyBankOpen = runningPayload.Value.Open
                            End If
                            If lastNiftyBankDate <> Date.MinValue AndAlso lastNiftyBankOpen <> Decimal.MinValue Then
                                If niftyBankChangePayload Is Nothing Then niftyBankChangePayload = New Dictionary(Of Date, Decimal)
                                niftyBankChangePayload.Add(runningPayload.Key, ((runningPayload.Value.Close / lastNiftyBankOpen) - 1) * 100)
                            End If
                        Next

                        If nifty50ChangePayload IsNot Nothing AndAlso nifty50ChangePayload.Count > 0 Then
                            If niftyBankChangePayload IsNot Nothing AndAlso niftyBankChangePayload.Count > 0 Then
                                Dim diffPayload As Dictionary(Of Date, Decimal) = Nothing
                                For Each runningPayload In nifty50ChangePayload
                                    If diffPayload Is Nothing Then diffPayload = New Dictionary(Of Date, Decimal)
                                    diffPayload.Add(runningPayload.Key, (runningPayload.Value - niftyBankChangePayload(runningPayload.Key)))
                                Next
                                If diffPayload IsNot Nothing AndAlso diffPayload.Count > 0 Then
                                    Dim maxDiff As Decimal = Decimal.MinValue
                                    Dim maxDiffDate As Date = Date.MinValue
                                    For Each runningPayload In diffPayload.Where(Function(x)
                                                                                     Return x.Key.Date = previousTradingDay.Date
                                                                                 End Function)
                                        If Math.Abs(runningPayload.Value) > maxDiff Then
                                            maxDiff = Math.Abs(runningPayload.Value)
                                            maxDiffDate = runningPayload.Key
                                        End If
                                    Next
                                    If maxDiffDate <> Date.MinValue Then
                                        Dim nifty50ModifiedChangePayload As Dictionary(Of Date, Decimal) = Nothing
                                        Dim nifty50MaxDiffClose As Decimal = nifty50Payload(maxDiffDate).Close
                                        For Each runningPayload In nifty50Payload
                                            If runningPayload.Key > maxDiffDate Then
                                                If nifty50ModifiedChangePayload Is Nothing Then nifty50ModifiedChangePayload = New Dictionary(Of Date, Decimal)
                                                nifty50ModifiedChangePayload.Add(runningPayload.Key, ((runningPayload.Value.Close / nifty50MaxDiffClose) - 1) * 100)
                                            End If
                                        Next

                                        Dim niftyBankModifiedChangePayload As Dictionary(Of Date, Decimal) = Nothing
                                        Dim niftyBankMaxDiffClose As Decimal = niftyBankPayload(maxDiffDate).Close
                                        For Each runningPayload In niftyBankPayload
                                            If runningPayload.Key > maxDiffDate Then
                                                If niftyBankModifiedChangePayload Is Nothing Then niftyBankModifiedChangePayload = New Dictionary(Of Date, Decimal)
                                                niftyBankModifiedChangePayload.Add(runningPayload.Key, ((runningPayload.Value.Close / niftyBankMaxDiffClose) - 1) * 100)
                                            End If
                                        Next

                                        Dim diffModifiedPayload As Dictionary(Of Date, Decimal) = Nothing
                                        For Each runningPayload In nifty50ModifiedChangePayload
                                            If diffModifiedPayload Is Nothing Then diffModifiedPayload = New Dictionary(Of Date, Decimal)
                                            diffModifiedPayload.Add(runningPayload.Key, (runningPayload.Value - niftyBankModifiedChangePayload(runningPayload.Key)))
                                        Next

                                        If diffModifiedPayload IsNot Nothing AndAlso diffModifiedPayload.Count > 0 Then
                                            Dim convertedDiffModifiedPayload As Dictionary(Of Date, Payload) = Nothing
                                            Common.ConvertDecimalToPayload(Payload.PayloadFields.Close, diffModifiedPayload, convertedDiffModifiedPayload)

                                            Dim smaPayload As Dictionary(Of Date, Decimal) = Nothing
                                            Indicator.SMA.CalculateSMA(15, Payload.PayloadFields.Close, convertedDiffModifiedPayload, smaPayload)

                                            Dim sdPayload As Dictionary(Of Date, Decimal) = Nothing
                                            Indicator.StandardDeviation.CalculateSD(15, Payload.PayloadFields.Close, convertedDiffModifiedPayload, sdPayload)

                                            Dim tradeEntryTime As Date = Date.MinValue
                                            Dim counter As Integer = 0
                                            For Each runningPayload In diffModifiedPayload
                                                counter += 1
                                                If counter >= 15 AndAlso runningPayload.Key.Date = tradingDate.Date Then
                                                    Dim sd As Decimal = (runningPayload.Value - smaPayload(runningPayload.Key)) / sdPayload(runningPayload.Key)
                                                    If Math.Abs(sd) >= 2.5 Then
                                                        tradeEntryTime = runningPayload.Key
                                                        Exit For
                                                    End If
                                                End If
                                            Next
                                            If tradeEntryTime <> Date.MinValue Then
                                                Dim nifty50Close As Decimal = nifty50Payload(tradeEntryTime).Close
                                                Dim nifty50StrikePrice As Decimal = Decimal.MinValue
                                                If (Math.Ceiling(nifty50Close / 50) * 50) - nifty50Close > nifty50Close - (Math.Floor(nifty50Close / 50) * 50) Then
                                                    nifty50StrikePrice = Math.Floor(nifty50Close / 50) * 50
                                                Else
                                                    nifty50StrikePrice = Math.Ceiling(nifty50Close / 50) * 50
                                                End If

                                                Dim niftyBankClose As Decimal = niftyBankPayload(tradeEntryTime).Close
                                                Dim niftyBankStrikePrice As Decimal = Decimal.MinValue
                                                If (Math.Ceiling(niftyBankClose / 100) * 100) - niftyBankClose > niftyBankClose - (Math.Floor(niftyBankClose / 100) * 100) Then
                                                    niftyBankStrikePrice = Math.Floor(niftyBankClose / 100) * 100
                                                Else
                                                    niftyBankStrikePrice = Math.Ceiling(niftyBankClose / 100) * 100
                                                End If

                                                Dim nifty50TradingSymbol As String = Nothing
                                                Dim niftyBankTradingSymbol As String = Nothing
                                                If expiry = lastThursDayOfTheMonth Then
                                                    nifty50TradingSymbol = String.Format("{0}{1}{2}[TYPE]", "NIFTY", expiry.ToString("yyMMM"), nifty50StrikePrice)
                                                    niftyBankTradingSymbol = String.Format("{0}{1}{2}[TYPE]", "BANKNIFTY", expiry.ToString("yyMMM"), niftyBankStrikePrice)
                                                Else
                                                    Dim dateString As String = ""
                                                    If expiry.Month > 9 Then
                                                        dateString = String.Format("{0}{1}{2}", expiry.ToString("yy"), Microsoft.VisualBasic.Left(expiry.ToString("MMM"), 1), expiry.ToString("dd"))
                                                    Else
                                                        dateString = expiry.ToString("yyMdd")
                                                    End If
                                                    nifty50TradingSymbol = String.Format("{0}{1}{2}[TYPE]", "NIFTY", dateString, nifty50StrikePrice)
                                                    niftyBankTradingSymbol = String.Format("{0}{1}{2}[TYPE]", "BANKNIFTY", dateString, niftyBankStrikePrice)
                                                End If

                                                Dim nifty50ChangePer As Decimal = nifty50ModifiedChangePayload(tradeEntryTime)
                                                Dim niftyBankChangePer As Decimal = niftyBankModifiedChangePayload(tradeEntryTime)
                                                If nifty50ChangePer > niftyBankChangePer Then
                                                    nifty50TradingSymbol = nifty50TradingSymbol.Replace("[TYPE]", "PE")
                                                    niftyBankTradingSymbol = niftyBankTradingSymbol.Replace("[TYPE]", "CE")
                                                Else
                                                    nifty50TradingSymbol = nifty50TradingSymbol.Replace("[TYPE]", "CE")
                                                    niftyBankTradingSymbol = niftyBankTradingSymbol.Replace("[TYPE]", "PE")
                                                End If

                                                Dim nifty50Turnover As Decimal = Decimal.MinValue
                                                Dim nifty50LotSize As Integer = Integer.MinValue
                                                Dim niftyBankTurnover As Decimal = Decimal.MinValue
                                                Dim niftyBankLotSize As Integer = Integer.MinValue
                                                If nifty50TradingSymbol IsNot Nothing Then
                                                    Dim optionPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures_Options, nifty50TradingSymbol, tradingDate, tradingDate)
                                                    If optionPayload IsNot Nothing AndAlso optionPayload.ContainsKey(tradeEntryTime) Then
                                                        nifty50LotSize = _cmn.GetLotSize(Common.DataBaseTable.EOD_Futures_Options, nifty50TradingSymbol, tradingDate)
                                                        nifty50Turnover = optionPayload(tradeEntryTime).Close * nifty50LotSize
                                                    End If
                                                End If
                                                If niftyBankTradingSymbol IsNot Nothing Then
                                                    Dim optionPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.Intraday_Futures_Options, niftyBankTradingSymbol, tradingDate, tradingDate)
                                                    If optionPayload IsNot Nothing AndAlso optionPayload.ContainsKey(tradeEntryTime) Then
                                                        niftyBankLotSize = _cmn.GetLotSize(Common.DataBaseTable.EOD_Futures_Options, niftyBankTradingSymbol, tradingDate)
                                                        niftyBankTurnover = optionPayload(tradeEntryTime).Close * niftyBankLotSize
                                                    End If
                                                End If

                                                If nifty50TradingSymbol IsNot Nothing AndAlso nifty50Turnover <> Decimal.MinValue AndAlso niftyBankTurnover <> Decimal.MinValue Then
                                                    nifty50TradingSymbol = nifty50TradingSymbol.ToUpper
                                                    Dim row As DataRow = ret.NewRow
                                                    row("Date") = tradingDate.ToString("dd-MMM-yyyy")
                                                    row("Trading Symbol") = nifty50TradingSymbol
                                                    row("Lot Size") = nifty50LotSize
                                                    row("Time") = tradeEntryTime.ToString("dd-MM-yyyy HH:mm:ss")
                                                    row("Turnover") = nifty50Turnover
                                                    row("Turnover Ratio") = nifty50Turnover / niftyBankTurnover
                                                    ret.Rows.Add(row)
                                                End If
                                                If niftyBankTradingSymbol IsNot Nothing AndAlso nifty50Turnover <> Decimal.MinValue AndAlso niftyBankTurnover <> Decimal.MinValue Then
                                                    niftyBankTradingSymbol = niftyBankTradingSymbol.ToUpper
                                                    Dim row As DataRow = ret.NewRow
                                                    row("Date") = tradingDate.ToString("dd-MMM-yyyy")
                                                    row("Trading Symbol") = niftyBankTradingSymbol
                                                    row("Lot Size") = niftyBankLotSize
                                                    row("Time") = tradeEntryTime.ToString("dd-MM-yyyy HH:mm:ss")
                                                    row("Turnover") = niftyBankTurnover
                                                    row("Turnover Ratio") = niftyBankTurnover / nifty50Turnover
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

        Dim queryString As String = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol`
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