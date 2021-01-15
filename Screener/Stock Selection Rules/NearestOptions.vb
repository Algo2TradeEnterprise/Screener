Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Algo2TradeBLL
Imports Utilities.Network

Public Class NearestOptions
    Inherits StockSelection

    Private ReadOnly _stockList As List(Of String) = Nothing

    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer,
                   ByVal instrumentList As List(Of String))
        MyBase.New(canceller, cmn, stockType)
        _stockList = instrumentList
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
        ret.Columns.Add("Cash Neutral")

        Dim commodityMultiplierMap As Dictionary(Of String, Object) = Nothing
        If _eodTable = Common.DataBaseTable.EOD_Commodity Then
            commodityMultiplierMap = Await GetCommodityMultiplier().ConfigureAwait(False)
        End If

        Dim tradingDate As Date = startDate
        While tradingDate <= endDate
            If _stockList IsNot Nothing AndAlso _stockList.Count > 0 Then
                _canceller.Token.ThrowIfCancellationRequested()
                Dim isTradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
                If isTradingDay OrElse tradingDate.Date = Now.Date Then
                    Dim previousTradingDay As Date = _cmn.GetPreviousTradingDay(_eodTable, tradingDate)
                    If previousTradingDay <> Date.MinValue Then
                        Dim tempStockList As Dictionary(Of String, String()) = Nothing
                        For Each runningStock In _stockList
                            _canceller.Token.ThrowIfCancellationRequested()
                            Dim currentTradingSymbol As String = _cmn.GetCurrentTradingSymbol(_eodTable, tradingDate, runningStock)
                            If _eodTable = Common.DataBaseTable.EOD_Cash Then
                                currentTradingSymbol = _cmn.GetCurrentTradingSymbol(Common.DataBaseTable.EOD_Futures, tradingDate, runningStock)
                            End If
                            If currentTradingSymbol IsNot Nothing AndAlso currentTradingSymbol.Trim <> "" Then
                                Dim lotSize As Integer = _cmn.GetLotSize(_eodTable, currentTradingSymbol, tradingDate)
                                If _eodTable = Common.DataBaseTable.EOD_Cash Then
                                    lotSize = _cmn.GetLotSize(Common.DataBaseTable.EOD_Futures, currentTradingSymbol, tradingDate)
                                End If
                                If lotSize <> Integer.MinValue Then
                                    If _eodTable = Common.DataBaseTable.EOD_Currency Then
                                        lotSize = lotSize * 1000
                                    ElseIf _eodTable = Common.DataBaseTable.EOD_Commodity Then
                                        If commodityMultiplierMap IsNot Nothing AndAlso commodityMultiplierMap.ContainsKey(runningStock) Then
                                            Dim multiplier As Long = commodityMultiplierMap(runningStock).ToString.Substring(0, commodityMultiplierMap(runningStock).ToString.Length - 1)
                                            lotSize = lotSize * multiplier
                                        End If
                                    End If
                                    Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_eodTable, currentTradingSymbol, previousTradingDay.AddDays(-200), previousTradingDay)
                                    If _eodTable = Common.DataBaseTable.EOD_Cash Then
                                        Dim instrumentName As String = runningStock.ToUpper.Trim
                                        If runningStock.ToUpper.Trim = "NIFTY" Then
                                            instrumentName = "NIFTY 50"
                                        ElseIf runningStock.ToUpper.Trim = "BANKNIFTY" Then
                                            instrumentName = "NIFTY BANK"
                                        End If
                                        eodPayload = _cmn.GetRawPayloadForSpecificTradingSymbol(_eodTable, instrumentName, previousTradingDay.AddDays(-200), previousTradingDay)
                                    End If
                                    If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
                                        If eodPayload.LastOrDefault.Value.PayloadDate.Date = previousTradingDay.Date Then
                                            Dim lastDayPayload As Payload = eodPayload.LastOrDefault.Value
                                            Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                                            Indicator.ATR.CalculateATR(14, eodPayload, atrPayload, True)
                                            Dim atr As Decimal = atrPayload(lastDayPayload.PayloadDate)
                                            Dim atrPer As Decimal = (atr / lastDayPayload.Close) * 100
                                            Dim slab As Decimal = CalculateSlab(lastDayPayload.Close, atrPer)

                                            Dim optionContracts As List(Of String) = Await GetCurrentOptionContractsAsync(runningStock.Split("-")(0).Trim, tradingDate).ConfigureAwait(False)
                                            If optionContracts IsNot Nothing AndAlso optionContracts.Count > 0 Then
                                                Dim ceOption As Dictionary(Of Decimal, String) = Nothing
                                                Dim peOption As Dictionary(Of Decimal, String) = Nothing
                                                For Each runningOption In optionContracts
                                                    Dim strikeOption As String = runningOption.Substring(runningStock.Count + 5)
                                                    Dim strike As String = strikeOption.Substring(0, strikeOption.Count - 2)
                                                    If strike IsNot Nothing AndAlso strike.Trim <> "" AndAlso IsNumeric(strike) Then
                                                        If runningOption.EndsWith("CE") Then
                                                            If ceOption Is Nothing Then ceOption = New Dictionary(Of Decimal, String)
                                                            ceOption.Add(Val(strike), runningOption)
                                                        ElseIf runningOption.EndsWith("PE") Then
                                                            If peOption Is Nothing Then peOption = New Dictionary(Of Decimal, String)
                                                            peOption.Add(Val(strike), runningOption)
                                                        End If
                                                    End If
                                                Next
                                                If ceOption IsNot Nothing AndAlso ceOption.Count > 0 AndAlso
                                                    peOption IsNot Nothing AndAlso peOption.Count > 0 Then
                                                    Dim nearestCE As Decimal = ceOption.Where(Function(x)
                                                                                                  Return x.Key >= lastDayPayload.Close
                                                                                              End Function).OrderBy(Function(y)
                                                                                                                        Return y.Key
                                                                                                                    End Function).FirstOrDefault.Key
                                                    Dim nearestPE As Decimal = ceOption.Where(Function(x)
                                                                                                  Return x.Key <= lastDayPayload.Close
                                                                                              End Function).OrderBy(Function(y)
                                                                                                                        Return y.Key
                                                                                                                    End Function).LastOrDefault.Key

                                                    If nearestCE <> Decimal.MinValue AndAlso nearestCE <> Decimal.MaxValue AndAlso nearestCE <> 0 AndAlso
                                                        nearestPE <> Decimal.MinValue AndAlso nearestPE <> Decimal.MaxValue AndAlso nearestPE <> 0 AndAlso
                                                        ceOption.ContainsKey(nearestCE) AndAlso peOption.ContainsKey(nearestPE) Then
                                                        Dim eodCEPayload As Dictionary(Of Date, Payload) = Await GetRawPayloadForOptionsAsync(ceOption(nearestCE), previousTradingDay.AddDays(-200), previousTradingDay).ConfigureAwait(False)
                                                        Dim eodPEPayload As Dictionary(Of Date, Payload) = Await GetRawPayloadForOptionsAsync(peOption(nearestPE), previousTradingDay.AddDays(-200), previousTradingDay).ConfigureAwait(False)

                                                        If eodCEPayload IsNot Nothing AndAlso eodCEPayload.Count > 0 AndAlso
                                                            eodPEPayload IsNot Nothing AndAlso eodPEPayload.Count > 0 Then
                                                            Dim lastDayCEPayload As Payload = eodCEPayload.LastOrDefault.Value
                                                            Dim lastDayPEPayload As Payload = eodPEPayload.LastOrDefault.Value

                                                            If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                                            tempStockList.Add(ceOption(nearestCE), {lotSize, atrPer, atr, lastDayCEPayload.Open, lastDayCEPayload.Low, lastDayCEPayload.High, lastDayCEPayload.Close, slab, Math.Round((lastDayCEPayload.Close / (lastDayCEPayload.Close + lastDayPEPayload.Close)) * 100, 2)})
                                                            tempStockList.Add(peOption(nearestPE), {lotSize, atrPer, atr, lastDayPEPayload.Open, lastDayPEPayload.Low, lastDayPEPayload.High, lastDayPEPayload.Close, slab, Math.Round((lastDayPEPayload.Close / (lastDayCEPayload.Close + lastDayPEPayload.Close)) * 100, 2)})
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next
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
                                row("Cash Neutral") = runningStock.Value(8)
                                ret.Rows.Add(row)
                            Next
                        End If
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

    Private Async Function GetCommodityMultiplier() As Task(Of Dictionary(Of String, Object))
        Dim proxyToBeUsed As HttpProxy = Nothing
        Dim ret As Dictionary(Of String, Object) = Nothing

        Using browser As New HttpBrowser(proxyToBeUsed, Net.DecompressionMethods.GZip, New TimeSpan(0, 1, 0), _canceller)
            AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            AddHandler browser.Heartbeat, AddressOf OnHeartbeat
            AddHandler browser.WaitingFor, AddressOf OnWaitingFor
            AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
            Dim l As Tuple(Of Uri, Object) = Await browser.NonPOSTRequestAsync("https://zerodha.com/static/js/brokerage.min.js",
                                                                                 HttpMethod.Get,
                                                                                 Nothing,
                                                                                 True,
                                                                                 Nothing,
                                                                                 False,
                                                                                 Nothing).ConfigureAwait(False)
            _canceller.Token.ThrowIfCancellationRequested()
            If l Is Nothing OrElse l.Item2 Is Nothing Then
                Throw New ApplicationException(String.Format("No response in the additional site's to fetch commodity multiplier and group map: {0}",
                                                             "https://zerodha.com/static/js/brokerage.min.js"))
            End If
            _canceller.Token.ThrowIfCancellationRequested()
            If l IsNot Nothing AndAlso l.Item2 IsNot Nothing Then
                Dim jString As String = l.Item2
                If jString IsNot Nothing Then
                    Dim multiplierMap As String = Utilities.Strings.GetTextBetween("COMMODITY_MULTIPLIER_MAP=", "}", jString)
                    If multiplierMap IsNot Nothing Then
                        If multiplierMap.EndsWith(",") Then
                            multiplierMap = multiplierMap.Substring(0, multiplierMap.Count - 1)
                        End If
                        multiplierMap = multiplierMap & "}"
                        ret = Utilities.Strings.JsonDeserialize(multiplierMap)
                    End If

                    'Dim groupMap As String = Utilities.Strings.GetTextBetween("COMMODITY_GROUP_MAP=", "}", jString)
                    'If groupMap IsNot Nothing Then
                    '    If groupMap.EndsWith(",") Then
                    '        groupMap = groupMap.Substring(0, groupMap.Count - 1)
                    '    End If
                    '    groupMap = groupMap & "}"
                    '    GlobalVar.GroupMap = Utilities.Strings.JsonDeserialize(groupMap)
                    'End If
                End If
            End If
        End Using
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
            Case Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_Cash
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
                                                                      If x.Expiry.Date.AddDays(-2) > tradingDate.Date Then
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
            Case Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.EOD_Cash
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