Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Algo2TradeBLL
Imports Utilities.Network

Public Class OpeningPriceOptions
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
        ret.Columns.Add("Day ATR")
        ret.Columns.Add("Previous Day Open")
        ret.Columns.Add("Previous Day Low")
        ret.Columns.Add("Previous Day High")
        ret.Columns.Add("Previous Day Close")
        ret.Columns.Add("Slab")

        Dim tradingDate As Date = startDate
        While tradingDate <= endDate
            _canceller.Token.ThrowIfCancellationRequested()
            Dim isTradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
            If isTradingDay OrElse tradingDate.Date = Now.Date Then
                Dim previousTradingDay As Date = _cmn.GetPreviousTradingDay(_eodTable, tradingDate)
                If previousTradingDay <> Date.MinValue Then
                    Dim tempStockList As Dictionary(Of String, String()) = Nothing
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim currentTradingSymbol As String = _cmn.GetCurrentTradingSymbol(_eodTable, tradingDate, "NIFTY")
                    If currentTradingSymbol IsNot Nothing AndAlso currentTradingSymbol.Trim <> "" Then
                        Dim lotSize As Integer = _cmn.GetLotSize(_eodTable, currentTradingSymbol, tradingDate)
                        If lotSize <> Integer.MinValue Then
                            Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_eodTable, currentTradingSymbol, previousTradingDay.AddDays(-200), tradingDate)
                            If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
                                If eodPayload.LastOrDefault.Value.PayloadDate.Date = tradingDate.Date Then
                                    Dim currentDayPayload As Payload = eodPayload.LastOrDefault.Value
                                    Dim lastDayPayload As Payload = currentDayPayload.PreviousCandlePayload
                                    Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                                    Indicator.ATR.CalculateATR(14, eodPayload, atrPayload, True)
                                    Dim atr As Decimal = atrPayload(lastDayPayload.PayloadDate)
                                    Dim atrPer As Decimal = (atr / lastDayPayload.Close) * 100
                                    Dim slab As Decimal = CalculateSlab(lastDayPayload.Close, atrPer)

                                    Dim optionContracts As List(Of String) = Await GetCurrentOptionContractsAsync("NIFTY", tradingDate).ConfigureAwait(False)
                                    If optionContracts IsNot Nothing AndAlso optionContracts.Count > 0 Then
                                        Dim upperStrike As Decimal = Math.Ceiling(currentDayPayload.Open / 50) * 50
                                        Dim lowerStrike As Decimal = Math.Floor(currentDayPayload.Open / 50) * 50
                                        Dim strikePrice As Decimal = Decimal.MinValue
                                        If upperStrike - currentDayPayload.Open > currentDayPayload.Open - lowerStrike Then
                                            strikePrice = lowerStrike
                                        Else
                                            strikePrice = upperStrike
                                        End If

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(currentTradingSymbol, {lotSize, atrPer, atr, lastDayPayload.Open, lastDayPayload.Low, lastDayPayload.High, lastDayPayload.Close, slab})

                                        For Each runningOption In optionContracts
                                            Dim strike As String = runningOption.Substring(10)
                                            strike = strike.Substring(0, strike.Count - 2)
                                            If IsNumeric(strike) AndAlso Val(strike) = strikePrice Then
                                                Dim eodOptPayload As Dictionary(Of Date, Payload) = Await GetRawPayloadForOptionsAsync(runningOption, previousTradingDay.AddDays(-200), previousTradingDay).ConfigureAwait(False)
                                                If eodOptPayload IsNot Nothing AndAlso eodOptPayload.Count > 0 Then
                                                    Dim lastDayOptPayload As Payload = eodOptPayload.LastOrDefault.Value
                                                    If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                                    tempStockList.Add(runningOption, {lotSize, atrPer, atr, lastDayOptPayload.Open, lastDayOptPayload.Low, lastDayOptPayload.High, lastDayOptPayload.Close, slab})
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                        End If
                    End If
                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        For Each runningStock In tempStockList
                            _canceller.Token.ThrowIfCancellationRequested()
                            Dim row As DataRow = ret.NewRow
                            row("Date") = tradingDate.ToString("dd-MM-yyyy")
                            row("Trading Symbol") = runningStock.Key
                            row("Lot Size") = runningStock.Value(0)
                            row("ATR %") = Math.Round(Val(runningStock.Value(1)), 4)
                            row("Day ATR") = Math.Round(Val(runningStock.Value(2)), 4)
                            row("Previous Day Open") = runningStock.Value(3)
                            row("Previous Day Low") = runningStock.Value(4)
                            row("Previous Day High") = runningStock.Value(5)
                            row("Previous Day Close") = runningStock.Value(6)
                            row("Slab") = runningStock.Value(7)
                            ret.Rows.Add(row)
                        Next
                    End If
                End If
            End If
            tradingDate = tradingDate.AddDays(1)
        End While

        Return ret
    End Function

    Public Async Function IsTradableDay(ByVal tradingDate As Date) As Task(Of Boolean)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As Boolean = False
        Dim historicalData As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(Common.DataBaseTable.EOD_POSITIONAL, "JINDALSTEL", tradingDate, tradingDate)
        If historicalData IsNot Nothing AndAlso historicalData.Count > 0 Then
            ret = True
        End If
        Return ret
    End Function

    Private Function CalculateSlab(ByVal price As Decimal, ByVal atrPer As Decimal) As Decimal
        Dim ret As Decimal = 0
        Dim slabList As List(Of Decimal) = Nothing
        Select Case _eodTable
            Case Common.DataBaseTable.EOD_Currency
                slabList = New List(Of Decimal) From {0.025, 0.05, 0.1, 0.25}
            Case Else
                slabList = New List(Of Decimal) From {0.25, 0.5, 1, 2.5, 5, 10, 15}
        End Select
        Dim atr As Decimal = (atrPer / 100) * price
        Dim supportedSlabList As List(Of Decimal) = slabList.FindAll(Function(x)
                                                                         Return x <= atr / 8
                                                                     End Function)
        If supportedSlabList IsNot Nothing AndAlso supportedSlabList.Count > 0 Then
            ret = supportedSlabList.Max
            If price * 1 / 100 < ret Then
                Dim newSupportedSlabList As List(Of Decimal) = supportedSlabList.FindAll(Function(x)
                                                                                             Return x <= price * 1 / 100
                                                                                         End Function)
                If newSupportedSlabList IsNot Nothing AndAlso newSupportedSlabList.Count > 0 Then
                    ret = newSupportedSlabList.Max
                End If
            End If
        End If
        Return ret
    End Function

    Private Async Function GetCurrentOptionContractsAsync(ByVal rawInstrumentName As String, ByVal tradingDate As Date) As Task(Of List(Of String))
        Dim ret As List(Of String) = Nothing
        Dim tableName As String = "active_instruments_futures"
        Select Case _eodTable
            Case Common.DataBaseTable.EOD_Commodity
                tableName = "active_instruments_commodity"
            Case Common.DataBaseTable.EOD_Currency
                tableName = "active_instruments_currency"
            Case Common.DataBaseTable.EOD_Futures
                tableName = "active_instruments_futures"
            Case Else
                Throw New NotImplementedException
        End Select
        Dim queryString As String = String.Format("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `{0}` WHERE `AS_ON_DATE`='{1}' AND `TRADING_SYMBOL` REGEXP '^{2}[0-9][0-9]*' AND `SEGMENT`='NFO-OPT'",
                                                   tableName, tradingDate.ToString("yyyy-MM-dd"), rawInstrumentName)
        _canceller.Token.ThrowIfCancellationRequested()
        Dim dt As DataTable = Await _cmn.RunSelectAsync(queryString).ConfigureAwait(False)
        _canceller.Token.ThrowIfCancellationRequested()
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                Dim activeInstruments As List(Of ActiveInstrumentData) = Nothing
                For i = 0 To dt.Rows.Count - 1
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim instrumentData As New ActiveInstrumentData With
                        {.Token = dt.Rows(i).Item(0),
                         .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
                         .Expiry = If(IsDBNull(dt.Rows(i).Item(2)), Date.MaxValue, dt.Rows(i).Item(2))}
                    If activeInstruments Is Nothing Then activeInstruments = New List(Of ActiveInstrumentData)
                    activeInstruments.Add(instrumentData)
                Next
                If activeInstruments IsNot Nothing AndAlso activeInstruments.Count > 0 Then
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim minExpiry As Date = activeInstruments.Min(Function(x)
                                                                      If x.Expiry.Date > tradingDate.Date Then
                                                                          Return x.Expiry
                                                                      Else
                                                                          Return Date.MaxValue
                                                                      End If
                                                                  End Function)
                    If minExpiry <> Date.MinValue Then
                        For Each runningOptStock In activeInstruments
                            _canceller.Token.ThrowIfCancellationRequested()
                            If runningOptStock.Expiry.Date = minExpiry.Date Then
                                If ret Is Nothing Then ret = New List(Of String)
                                ret.Add(runningOptStock.TradingSymbol)
                            End If
                        Next
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Class ActiveInstrumentData
        Public Token As String
        Public TradingSymbol As String
        Public Expiry As Date
    End Class

    Private Async Function GetRawPayloadForOptionsAsync(ByVal tradingSymbol As String, ByVal startDate As Date, ByVal endDate As Date) As Task(Of Dictionary(Of Date, Payload))
        Dim ret As Dictionary(Of Date, Payload) = Nothing
        _canceller.Token.ThrowIfCancellationRequested()
        Dim queryString As String = Nothing
        Select Case _eodTable
            Case Common.DataBaseTable.EOD_Currency
                queryString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_opt_currency` WHERE `TradingSymbol`='{0}' AND `SnapshotDate`>={1} AND `SnapshotDate`<='{2}'", tradingSymbol, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"))
            Case Common.DataBaseTable.EOD_Commodity
                queryString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_opt_commodity` WHERE `TradingSymbol`='{0}' AND `SnapshotDate`>={1} AND `SnapshotDate`<='{2}'", tradingSymbol, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"))
            Case Common.DataBaseTable.EOD_Futures
                queryString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_opt_futures` WHERE `TradingSymbol`='{0}' AND `SnapshotDate`>={1} AND `SnapshotDate`<='{2}'", tradingSymbol, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"))
            Case Else
                Throw New NotImplementedException
        End Select

        _canceller.Token.ThrowIfCancellationRequested()
        If tradingSymbol IsNot Nothing Then
            OnHeartbeat(String.Format("Fetching raw candle data from DataBase for {0} on {1}", tradingSymbol, endDate.ToShortDateString))
            Dim dt As DataTable = Await _cmn.RunSelectAsync(queryString).ConfigureAwait(False)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                _canceller.Token.ThrowIfCancellationRequested()
                ret = Common.ConvertDataTableToPayload(dt, 0, 1, 2, 3, 4, 5, 6)
            End If
        End If
        Return ret
    End Function
End Class