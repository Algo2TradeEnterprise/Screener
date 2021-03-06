﻿Imports System.Threading
Imports Algo2TradeBLL

Public Class VolumeSortCurrentDayOpenFilterTop2Options
    Inherits StockSelection

    Private ReadOnly _stockName As String = "BANKNIFTY"

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
        ret.Columns.Add("Previous Day Volume")
        ret.Columns.Add("Current Day Open")

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

                        Dim optionData As Dictionary(Of String, Payload) = Await GetOptionStockData(thursdayOfWeek, _stockName, previousTradingDay).ConfigureAwait(False)

                        If optionData IsNot Nothing AndAlso optionData.Count > 0 Then
                            Dim ceDone As Boolean = False
                            Dim peDone As Boolean = False
                            Dim counter As Integer = 0
                            For Each runningStock In optionData.Values.OrderByDescending(Function(x)
                                                                                             Return x.Volume
                                                                                         End Function)
                                Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(Common.DataBaseTable.EOD_Futures_Options, runningStock.TradingSymbol, tradingDate, tradingDate)
                                If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 AndAlso intradayPayload.ContainsKey(tradingDate.Date) Then
                                    Dim firstCandleOfDay As Payload = intradayPayload(tradingDate.Date)

                                    If firstCandleOfDay.Open < 30 Then
                                        Dim intrumentType As String = runningStock.TradingSymbol.Substring(runningStock.TradingSymbol.Count - 2).Trim
                                        If Not ceDone AndAlso intrumentType.ToUpper = "CE" Then
                                            Dim row As DataRow = ret.NewRow
                                            row("Date") = tradingDate.ToString("dd-MM-yyyy")
                                            row("Trading Symbol") = runningStock.TradingSymbol
                                            row("Lot Size") = lotSize
                                            row("Puts_Calls") = intrumentType
                                            row("Previous Day Volume") = runningStock.Volume
                                            row("Current Day Open") = firstCandleOfDay.Open
                                            ret.Rows.Add(row)

                                            counter += 1
                                            'ceDone = True
                                        ElseIf Not peDone AndAlso intrumentType.ToUpper = "PE" Then
                                            Dim row As DataRow = ret.NewRow
                                            row("Date") = tradingDate.ToString("dd-MM-yyyy")
                                            row("Trading Symbol") = runningStock.TradingSymbol
                                            row("Lot Size") = lotSize
                                            row("Puts_Calls") = intrumentType
                                            row("Previous Day Volume") = runningStock.Volume
                                            row("Current Day Open") = firstCandleOfDay.Open
                                            ret.Rows.Add(row)

                                            counter += 1
                                            'peDone = True
                                        End If
                                    End If
                                End If
                                If counter >= 2 Then Exit For
                            Next
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