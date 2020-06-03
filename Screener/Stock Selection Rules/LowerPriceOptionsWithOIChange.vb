Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class LowerPriceOptionsWithOIChange
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
        ret.Columns.Add("Instrument Type")
        ret.Columns.Add("Previous Volume")
        ret.Columns.Add("Previous OI Change%")

        Dim tradingDate As Date = startDate
        While tradingDate <= endDate
            _canceller.Token.ThrowIfCancellationRequested()
            Dim tradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
            If tradingDay Then
                Dim previousTradingDay As Date = _cmn.GetPreviousTradingDay(Common.DataBaseTable.EOD_Futures, tradingDate)
                If previousTradingDay <> Date.MinValue Then
                    Dim prePreviousTradingDay As Date = _cmn.GetPreviousTradingDay(Common.DataBaseTable.EOD_Futures, previousTradingDay)
                    If prePreviousTradingDay <> Date.MinValue Then
                        Dim startDateOfWeek As Date = Common.GetStartDateOfTheWeek(tradingDate, DayOfWeek.Monday)
                        Dim thursdayOfWeek As Date = startDateOfWeek.AddDays(3)
                        If tradingDate.DayOfWeek = DayOfWeek.Friday Then
                            thursdayOfWeek = tradingDate.AddDays(6)
                        ElseIf tradingDate.DayOfWeek = DayOfWeek.Saturday Then
                            thursdayOfWeek = tradingDate.AddDays(5)
                        End If

                        OnHeartbeat(String.Format("Getting option data for {0}", tradingDate.ToString("dd-MM-yyyy")))
                        Dim optionData As List(Of OptionData) = Await GetOptionStockData(thursdayOfWeek, "NIFTY", tradingDate, previousTradingDay, prePreviousTradingDay).ConfigureAwait(False)
                        If optionData IsNot Nothing AndAlso optionData.Count > 0 Then
                            Dim peStockList As List(Of OptionData) = optionData.FindAll(Function(x)
                                                                                            Return x.InstrumentType = "PE"
                                                                                        End Function)

                            Dim ceStockList As List(Of OptionData) = optionData.FindAll(Function(x)
                                                                                            Return x.InstrumentType = "CE"
                                                                                        End Function)

                            If peStockList IsNot Nothing AndAlso peStockList.Count > 0 AndAlso
                                ceStockList IsNot Nothing AndAlso ceStockList.Count > 0 Then
                                Dim peSelectedStocks As List(Of OptionData) = peStockList.FindAll(Function(x)
                                                                                                      Return x.PreviousOIChange > 0
                                                                                                  End Function).OrderByDescending(Function(y)
                                                                                                                                      Return y.PreviousVolume
                                                                                                                                  End Function).Take(My.Settings.NumberOfStockPerDay).ToList

                                Dim ceSelectedStocks As List(Of OptionData) = ceStockList.FindAll(Function(x)
                                                                                                      Return x.PreviousOIChange > 0
                                                                                                  End Function).OrderByDescending(Function(y)
                                                                                                                                      Return y.PreviousVolume
                                                                                                                                  End Function).Take(My.Settings.NumberOfStockPerDay).ToList

                                If peSelectedStocks IsNot Nothing AndAlso peSelectedStocks.Count > 0 Then
                                    For Each runningStock In peSelectedStocks
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        Dim row As DataRow = ret.NewRow
                                        row("Date") = tradingDate.ToString("dd-MM-yyyy")
                                        row("Trading Symbol") = runningStock.TradingSymbol
                                        row("Lot Size") = 75
                                        row("Instrument Type") = runningStock.InstrumentType
                                        row("Previous Volume") = runningStock.PreviousVolume
                                        row("Previous OI Change%") = Math.Round(runningStock.PreviousOIChange, 4)
                                        ret.Rows.Add(row)
                                    Next
                                End If
                                If ceSelectedStocks IsNot Nothing AndAlso ceSelectedStocks.Count > 0 Then
                                    For Each runningStock In ceSelectedStocks
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        Dim row As DataRow = ret.NewRow
                                        row("Date") = tradingDate.ToString("dd-MM-yyyy")
                                        row("Trading Symbol") = runningStock.TradingSymbol
                                        row("Lot Size") = 75
                                        row("Instrument Type") = runningStock.InstrumentType
                                        row("Previous Volume") = runningStock.PreviousVolume
                                        row("Previous OI Change%") = Math.Round(runningStock.PreviousOIChange, 4)
                                        ret.Rows.Add(row)
                                    Next
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

    Private Async Function GetOptionStockData(ByVal expiry As Date,
                                              ByVal rawInstrumentName As String,
                                              ByVal tradingDate As Date,
                                              ByVal previousTradingDate As Date,
                                              ByVal prePreviousTradingDate As Date) As Task(Of List(Of OptionData))
        Dim ret As List(Of OptionData) = Nothing
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
        Dim queryString As String = String.Format("SELECT C.`TradingSymbol`,B.`Volume` `Previous Day Volume`,((B.`OI`-A.`OI`)/A.`OI`)*100 `Previous OI Change`
                                                    FROM (`eod_prices_opt_futures` A JOIN `eod_prices_opt_futures` B JOIN `eod_prices_opt_futures` C)
                                                    WHERE A.`SnapshotDate`='{0}' 
                                                    AND B.`SnapshotDate`='{1}'
                                                    AND C.`SnapshotDate`='{2}'
                                                    AND C.`TradingSymbol` LIKE '{3}' 
                                                    AND A.`TradingSymbol`=C.`TradingSymbol`
                                                    AND B.`TradingSymbol`=C.`TradingSymbol`
                                                    AND C.`Open` < 10
                                                    ORDER BY `Previous OI Change` DESC",
                                                    prePreviousTradingDate.ToString("yyyy-MM-dd"),
                                                    previousTradingDate.ToString("yyyy-MM-dd"),
                                                    tradingDate.ToString("yyyy-MM-dd"),
                                                    tradingSymbol)
        Dim dt As DataTable = Await _cmn.RunSelectAsync(queryString).ConfigureAwait(False)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim i As Integer = 0
            ret = New List(Of OptionData)
            While Not i = dt.Rows.Count()
                If Not IsDBNull(dt.Rows(i).Item(2)) Then
                    Dim tempPayload As OptionData = New OptionData With {
                        .TradingSymbol = dt.Rows(i).Item(0),
                        .PreviousVolume = dt.Rows(i).Item(1),
                        .PreviousOIChange = dt.Rows(i).Item(2)
                    }
                    ret.Add(tempPayload)
                End If
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

    Private Class OptionData
        Public Property TradingSymbol As String
        Public ReadOnly Property InstrumentType As String
            Get
                Return Me.TradingSymbol.Substring(Me.TradingSymbol.Count - 2).Trim
            End Get
        End Property
        Public Property PreviousVolume As Decimal
        Public Property PreviousOIChange As Decimal
    End Class
End Class