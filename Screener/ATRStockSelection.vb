Imports Algo2TradeBLL
Imports MySql.Data.MySqlClient
Imports System.Text.RegularExpressions
Imports System.Threading

Public Class ATRStockSelection
    Implements IDisposable

#Region "Events/Event handlers"
    Public Event DocumentDownloadComplete()
    Public Event DocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
    Public Event Heartbeat(ByVal msg As String)
    Public Event WaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
    'The below functions are needed to allow the derived classes to raise the above two events
    Protected Overridable Sub OnDocumentDownloadComplete()
        RaiseEvent DocumentDownloadComplete()
    End Sub
    Protected Overridable Sub OnDocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
        RaiseEvent DocumentRetryStatus(currentTry, totalTries)
    End Sub
    Protected Overridable Sub OnHeartbeat(ByVal msg As String)
        RaiseEvent Heartbeat(msg)
    End Sub
    Protected Overridable Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        RaiseEvent WaitingFor(elapsedSecs, totalSecs, msg)
    End Sub
#End Region

    Private ReadOnly _cts As CancellationTokenSource
    Private ReadOnly _common As Common
    Private _conn As MySqlConnection

    Public Sub New(ByVal canceller As CancellationTokenSource)
        _cts = canceller
        _common = New Common(_cts)
        AddHandler _common.Heartbeat, AddressOf OnHeartbeat
    End Sub

#Region "Private Class & Enum"
    Private Class ActiveInstrumentData
        Public Property Token As Integer
        Public Property TradingSymbol As String
        Public Property Expiry As Date
        Public Property LastDayOpen As Decimal
        Public Property LastDayLow As Decimal
        Public Property LastDayHigh As Decimal
        Public Property LastDayClose As Decimal
        Public ReadOnly Property RawInstrumentName As String
            Get
                If TradingSymbol.Contains("FUT") Then
                    Return Me.TradingSymbol.Remove(Me.TradingSymbol.Count - 8)
                Else
                    Return TradingSymbol
                End If
            End Get
        End Property
        Public Property CashInstrumentName As String
        Public Property CashInstrumentToken As String
    End Class
#End Region

    Public Async Function GetATRStockData(ByVal eodTableType As Common.DataBaseTable,
                                          ByVal tradingDate As Date,
                                          ByVal bannedStocks As List(Of String),
                                          ByVal immediatePreviousDay As Boolean) As Task(Of Dictionary(Of String, InstrumentDetails))
        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
        Dim isTradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
        If isTradingDay OrElse tradingDate.Date = Now.Date Then
            Dim stockList As Dictionary(Of String, InstrumentDetails) = Nothing
            If My.Settings.OnlyFOStocks Then
                stockList = Await GetATRBasedFOStockDataAsync(tradingDate, eodTableType).ConfigureAwait(False)
            Else
                stockList = Await GetATRBasedCashStockDataAsync(tradingDate, eodTableType).ConfigureAwait(False)
            End If
            _cts.Token.ThrowIfCancellationRequested()
            If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
                Dim atrStockList As Dictionary(Of String, InstrumentDetails) = Nothing
                For Each stock In stockList.Keys
                    _cts.Token.ThrowIfCancellationRequested()
                    If bannedStocks Is Nothing OrElse
                    (bannedStocks IsNot Nothing AndAlso bannedStocks.Count > 0 AndAlso Not bannedStocks.Contains(stock.ToUpper)) Then
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim tradingSymbol As String = _common.GetCurrentTradingSymbol(eodTableType, tradingDate, stock)
                        If tradingSymbol IsNot Nothing Then
                            If tradingSymbol IsNot Nothing AndAlso tradingSymbol <> "" Then
                                _cts.Token.ThrowIfCancellationRequested()
                                If atrStockList Is Nothing Then atrStockList = New Dictionary(Of String, InstrumentDetails)
                                Dim instrumentData As New InstrumentDetails With
                                {.TradingSymbol = tradingSymbol,
                                 .ATRPercentage = stockList(stock).ATRPercentage,
                                 .LotSize = stockList(stock).LotSize,
                                 .DayATR = stockList(stock).DayATR,
                                 .PreviousDayOpen = stockList(stock).PreviousDayOpen,
                                 .PreviousDayLow = stockList(stock).PreviousDayLow,
                                 .PreviousDayHigh = stockList(stock).PreviousDayHigh,
                                 .PreviousDayClose = stockList(stock).PreviousDayClose,
                                 .Slab = CalculateSlab(.PreviousDayClose, .ATRPercentage)}
                                atrStockList.Add(stock, instrumentData)
                            End If
                        End If
                    End If
                Next
                If atrStockList IsNot Nothing AndAlso atrStockList.Count > 0 Then
                    Dim activeInstrumentData As Dictionary(Of String, InstrumentDetails) = GetActiveInstrumentData(eodTableType, tradingDate, atrStockList)
                    If activeInstrumentData IsNot Nothing AndAlso activeInstrumentData.Count > 0 Then
                        Dim filteredInstruments As IEnumerable(Of KeyValuePair(Of String, InstrumentDetails)) = activeInstrumentData.Where(Function(x)
                                                                                                                                               Return x.Value.IsTradable = True
                                                                                                                                           End Function)
                        If filteredInstruments IsNot Nothing AndAlso filteredInstruments.Count > 0 Then
                            For Each stockData In filteredInstruments
                                _cts.Token.ThrowIfCancellationRequested()
                                Dim stockPayload As Dictionary(Of Date, Payload) = GetStockPayload(eodTableType, tradingDate, stockData.Value, immediatePreviousDay)
                                If stockPayload IsNot Nothing AndAlso stockPayload.Count > 0 Then
                                    stockData.Value.BlankCandlePercentage = CalculateBlankVolumePercentage(stockPayload)
                                Else
                                    stockData.Value.IsTradable = False
                                    stockData.Value.BlankCandlePercentage = Decimal.MinValue
                                End If
                            Next
                            Dim stocksLessThanMaxBlankCandlePercentage As IEnumerable(Of KeyValuePair(Of String, InstrumentDetails)) =
                                filteredInstruments.Where(Function(x)
                                                              Return x.Value.BlankCandlePercentage <> Decimal.MinValue AndAlso
                                                              x.Value.BlankCandlePercentage <= My.Settings.MaxBlankCandlePercentage AndAlso
                                                              x.Value.IsTradable = True
                                                          End Function)
                            If stocksLessThanMaxBlankCandlePercentage IsNot Nothing AndAlso stocksLessThanMaxBlankCandlePercentage.Count > 0 Then
                                For Each runningStock In stocksLessThanMaxBlankCandlePercentage
                                    If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
                                    ret.Add(runningStock.Key, runningStock.Value)
                                Next
                            End If
                        End If
                    End If
                End If
            End If
        End If
        Return ret
    End Function


    Private Async Function GetATRBasedFOStockDataAsync(ByVal tradingDate As Date, ByVal tableType As Common.DataBaseTable) As Task(Of Dictionary(Of String, InstrumentDetails))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
            _cts.Token.ThrowIfCancellationRequested()
            _conn = _common.OpenDBConnection()
        End If
        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim previousTradingDay As Date = _common.GetPreviousTradingDay(Common.DataBaseTable.EOD_Futures, tradingDate)
        If previousTradingDay <> Date.MinValue Then
            If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
                _cts.Token.ThrowIfCancellationRequested()
                _conn = _common.OpenDBConnection()
            End If
            _cts.Token.ThrowIfCancellationRequested()
            OnHeartbeat("Fetching all future instrument")
            Dim cm As MySqlCommand = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_futures` WHERE `AS_ON_DATE`=@sd AND `SEGMENT`='NFO-FUT'", _conn)
            cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
            _cts.Token.ThrowIfCancellationRequested()
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            _cts.Token.ThrowIfCancellationRequested()
            Dim dt As DataTable = New DataTable
            adapter.Fill(dt)
            _cts.Token.ThrowIfCancellationRequested()
            Dim nfoInstruments As List(Of ActiveInstrumentData) = Nothing
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                For i = 0 To dt.Rows.Count - 1
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim instrumentData As New ActiveInstrumentData With
                    {.Token = dt.Rows(i).Item(0),
                     .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
                     .Expiry = dt.Rows(i).Item(2)}

                    Dim pattern As String = "([0-9][0-9]JAN)|([0-9][0-9]FEB)|([0-9][0-9]MAR)|([0-9][0-9]APR)|([0-9][0-9]MAY)|([0-9][0-9]JUN)|([0-9][0-9]JUL)|([0-9][0-9]AUG)|([0-9][0-9]SEP)|([0-9][0-9]OCT)|([0-9][0-9]NOV)|([0-9][0-9]DEC)"
                    If Regex.Matches(instrumentData.TradingSymbol, pattern).Count <= 1 Then
                        If nfoInstruments Is Nothing Then nfoInstruments = New List(Of ActiveInstrumentData)
                        nfoInstruments.Add(instrumentData)
                    End If
                Next
            End If
            If nfoInstruments IsNot Nothing AndAlso nfoInstruments.Count > 0 Then
                Dim lastTradingDay As Date = Date.MinValue
                Dim currentNFOInstruments As List(Of ActiveInstrumentData) = Nothing
                For Each runningInstrument In nfoInstruments
                    If currentNFOInstruments IsNot Nothing AndAlso currentNFOInstruments.Count > 0 Then
                        Dim availableInstrument As IEnumerable(Of ActiveInstrumentData) = currentNFOInstruments.FindAll(Function(z)
                                                                                                                            Return z.RawInstrumentName = runningInstrument.RawInstrumentName
                                                                                                                        End Function)
                        If availableInstrument IsNot Nothing AndAlso availableInstrument.Count > 0 Then
                            Continue For
                        End If
                    End If
                    Dim runningIntruments As IEnumerable(Of ActiveInstrumentData) = nfoInstruments.Where(Function(x)
                                                                                                             Return x.RawInstrumentName = runningInstrument.RawInstrumentName
                                                                                                         End Function)
                    Dim minExpiry As Date = runningIntruments.Min(Function(x)
                                                                      If x.Expiry.Date <= tradingDate.Date Then
                                                                          Return Date.MaxValue
                                                                      Else
                                                                          Return x.Expiry
                                                                      End If
                                                                  End Function)
                    Dim currentIntrument As ActiveInstrumentData = runningIntruments.ToList.Find(Function(y)
                                                                                                     Return y.Expiry.Date = minExpiry.Date
                                                                                                 End Function)
                    If currentIntrument IsNot Nothing Then
                        If currentNFOInstruments Is Nothing Then currentNFOInstruments = New List(Of ActiveInstrumentData)
                        currentNFOInstruments.Add(currentIntrument)
                    End If
                Next
                If currentNFOInstruments IsNot Nothing AndAlso currentNFOInstruments.Count > 0 Then
                    Dim priceFilterdCurrentNFOInstruments As List(Of ActiveInstrumentData) = Nothing
                    For Each runningInstrument In currentNFOInstruments
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim previousDayPayloads As Dictionary(Of Date, Payload) = _common.GetRawPayload(tableType, runningInstrument.RawInstrumentName, previousTradingDay.AddDays(-10), previousTradingDay)
                        Dim lastDayPayload As Payload = Nothing
                        If previousDayPayloads IsNot Nothing AndAlso previousDayPayloads.Count > 0 Then
                            lastDayPayload = previousDayPayloads.LastOrDefault.Value
                        End If
                        If lastDayPayload IsNot Nothing AndAlso lastDayPayload.Close >= My.Settings.MinClose AndAlso lastDayPayload.Close <= My.Settings.MaxClose Then
                            Dim rawCashInstrument As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.EOD_Cash, previousTradingDay, runningInstrument.RawInstrumentName)
                            If rawCashInstrument IsNot Nothing Then
                                runningInstrument.CashInstrumentToken = rawCashInstrument.Item2
                                runningInstrument.CashInstrumentName = rawCashInstrument.Item1
                                runningInstrument.LastDayOpen = lastDayPayload.Open
                                runningInstrument.LastDayLow = lastDayPayload.Low
                                runningInstrument.LastDayHigh = lastDayPayload.High
                                runningInstrument.LastDayClose = lastDayPayload.Close
                                If priceFilterdCurrentNFOInstruments Is Nothing Then priceFilterdCurrentNFOInstruments = New List(Of ActiveInstrumentData)
                                priceFilterdCurrentNFOInstruments.Add(runningInstrument)
                            End If
                        End If
                    Next
                    Dim highATRStocks As Concurrent.ConcurrentDictionary(Of String, Decimal()) = Nothing
                    Try
                        If priceFilterdCurrentNFOInstruments IsNot Nothing AndAlso priceFilterdCurrentNFOInstruments.Count > 0 Then
                            For i As Integer = 0 To priceFilterdCurrentNFOInstruments.Count - 1 Step 20
                                Dim numberOfData As Integer = If(priceFilterdCurrentNFOInstruments.Count - i > 20, 20, priceFilterdCurrentNFOInstruments.Count - i)
                                Dim tasks As IEnumerable(Of Task(Of Boolean)) = Nothing
                                tasks = priceFilterdCurrentNFOInstruments.GetRange(i, numberOfData).Select(Async Function(x)
                                                                                                               Try
                                                                                                                   _cts.Token.ThrowIfCancellationRequested()
                                                                                                                   Dim eodHistoricalData As Dictionary(Of Date, Payload) = Await _common.GetHistoricalDataAsync(Common.DataBaseTable.EOD_Cash, x.CashInstrumentName, previousTradingDay.AddDays(-300), previousTradingDay).ConfigureAwait(False)
                                                                                                                   _cts.Token.ThrowIfCancellationRequested()
                                                                                                                   If eodHistoricalData IsNot Nothing AndAlso eodHistoricalData.Count > 100 Then
                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
                                                                                                                       Dim ATRPayload As Dictionary(Of Date, Decimal) = Nothing
                                                                                                                       Indicator.ATR.CalculateATR(14, eodHistoricalData, ATRPayload)
                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
                                                                                                                       If ATRPayload IsNot Nothing AndAlso ATRPayload.Count > 0 Then
                                                                                                                           Dim lastDayClosePrice As Decimal = eodHistoricalData.LastOrDefault.Value.Close
                                                                                                                           Dim atrPercentage As Decimal = (ATRPayload(eodHistoricalData.LastOrDefault.Key) / lastDayClosePrice) * 100
                                                                                                                           If atrPercentage >= My.Settings.ATRPercentage Then
                                                                                                                               _cts.Token.ThrowIfCancellationRequested()
                                                                                                                               'Dim volumePayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = eodHistoricalData.OrderByDescending(Function(z)
                                                                                                                               '                                                                                                              Return z.Key
                                                                                                                               '                                                                                                          End Function).Take(5)
                                                                                                                               '_cts.Token.ThrowIfCancellationRequested()
                                                                                                                               'If volumePayload IsNot Nothing AndAlso volumePayload.Count > 0 Then
                                                                                                                               '    _cts.Token.ThrowIfCancellationRequested()
                                                                                                                               '    Dim avgVolume As Decimal = volumePayload.Average(Function(z)
                                                                                                                               '                                                         Return z.Value.Volume
                                                                                                                               '                                                     End Function)
                                                                                                                               '    _cts.Token.ThrowIfCancellationRequested()
                                                                                                                               '    If avgVolume >= (My.Settings.PotentialAmount / 100) * lastDayClosePrice Then
                                                                                                                               If highATRStocks Is Nothing Then highATRStocks = New Concurrent.ConcurrentDictionary(Of String, Decimal())
                                                                                                                               highATRStocks.TryAdd(x.CashInstrumentName, {atrPercentage, ATRPayload(eodHistoricalData.LastOrDefault.Key), x.LastDayOpen, x.LastDayLow, x.LastDayHigh, x.LastDayClose})
                                                                                                                               '    End If
                                                                                                                               'End If
                                                                                                                           End If
                                                                                                                       End If
                                                                                                                   End If
                                                                                                               Catch ex As Exception
                                                                                                                   Console.WriteLine(String.Format("{0}:{1}", x.TradingSymbol, ex.ToString))
                                                                                                                   Throw ex
                                                                                                               End Try
                                                                                                               Return True
                                                                                                           End Function)

                                Dim mainTask As Task = Task.WhenAll(tasks)
                                Await mainTask.ConfigureAwait(False)
                                If mainTask.Exception IsNot Nothing Then
                                    Throw mainTask.Exception
                                End If
                            Next
                        End If
                    Catch cex As TaskCanceledException
                        Throw cex
                    Catch aex As AggregateException
                        Throw aex
                    Catch ex As Exception
                        Throw ex
                    End Try

                    If highATRStocks IsNot Nothing AndAlso highATRStocks.Count > 0 Then
                        For Each runningStock In highATRStocks.OrderByDescending(Function(x)
                                                                                     Return x.Value(0)
                                                                                 End Function)
                            _cts.Token.ThrowIfCancellationRequested()
                            Dim currentTradingSymbol As String = _common.GetCurrentTradingSymbol(Common.DataBaseTable.EOD_Futures, tradingDate, runningStock.Key)
                            If currentTradingSymbol IsNot Nothing Then
                                Dim lotSize As Integer = _common.GetLotSize(Common.DataBaseTable.EOD_Futures, currentTradingSymbol, tradingDate)
                                If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
                                ret.Add(runningStock.Key, New InstrumentDetails With
                                        {.ATRPercentage = runningStock.Value(0),
                                         .LotSize = lotSize,
                                         .DayATR = runningStock.Value(1),
                                         .PreviousDayOpen = runningStock.Value(2),
                                         .PreviousDayLow = runningStock.Value(3),
                                         .PreviousDayHigh = runningStock.Value(4),
                                         .PreviousDayClose = runningStock.Value(5)})
                            End If
                        Next
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Async Function GetATRBasedCashStockDataAsync(ByVal tradingDate As Date, ByVal tableType As Common.DataBaseTable) As Task(Of Dictionary(Of String, InstrumentDetails))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
            _cts.Token.ThrowIfCancellationRequested()
            _conn = _common.OpenDBConnection()
        End If
        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim previousTradingDay As Date = _common.GetPreviousTradingDay(Common.DataBaseTable.EOD_Cash, tradingDate)
        If previousTradingDay <> Date.MinValue Then
            If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
                _cts.Token.ThrowIfCancellationRequested()
                _conn = _common.OpenDBConnection()
            End If
            _cts.Token.ThrowIfCancellationRequested()
            OnHeartbeat("Fetching all cash instrument")
            Dim cm As MySqlCommand = New MySqlCommand("SELECT DISTINCT(`INSTRUMENT_TOKEN`),`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_cash` WHERE `AS_ON_DATE`=@sd AND `SEGMENT`<>'INDICES'", _conn)
            cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
            _cts.Token.ThrowIfCancellationRequested()
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            _cts.Token.ThrowIfCancellationRequested()
            Dim dt As DataTable = New DataTable
            adapter.Fill(dt)
            _cts.Token.ThrowIfCancellationRequested()
            Dim cashInstruments As List(Of ActiveInstrumentData) = Nothing
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                For i = 0 To dt.Rows.Count - 1
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim instrumentData As New ActiveInstrumentData With
                    {.Token = dt.Rows(i).Item(0),
                     .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
                     .Expiry = Date.MaxValue}

                    Dim pattern As String = "([0-9][0-9]JAN)|([0-9][0-9]FEB)|([0-9][0-9]MAR)|([0-9][0-9]APR)|([0-9][0-9]MAY)|([0-9][0-9]JUN)|([0-9][0-9]JUL)|([0-9][0-9]AUG)|([0-9][0-9]SEP)|([0-9][0-9]OCT)|([0-9][0-9]NOV)|([0-9][0-9]DEC)"
                    If Regex.Matches(instrumentData.TradingSymbol, pattern).Count <= 1 Then
                        If cashInstruments Is Nothing Then cashInstruments = New List(Of ActiveInstrumentData)
                        cashInstruments.Add(instrumentData)
                    End If
                Next
            End If
            If cashInstruments IsNot Nothing AndAlso cashInstruments.Count > 0 Then
                Dim lastTradingDay As Date = Date.MinValue
                Dim currentInstruments As List(Of ActiveInstrumentData) = cashInstruments
                If currentInstruments IsNot Nothing AndAlso currentInstruments.Count > 0 Then
                    Dim priceFilterdCurrentNFOInstruments As List(Of ActiveInstrumentData) = Nothing
                    For Each runningInstrument In currentInstruments
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim previousDayPayloads As Dictionary(Of Date, Payload) = _common.GetRawPayload(Common.DataBaseTable.EOD_Cash, runningInstrument.RawInstrumentName, previousTradingDay.AddDays(-10), previousTradingDay)
                        Dim lastDayPayload As Payload = Nothing
                        If previousDayPayloads IsNot Nothing AndAlso previousDayPayloads.Count > 0 Then
                            lastDayPayload = previousDayPayloads.LastOrDefault.Value
                        End If
                        If lastDayPayload IsNot Nothing AndAlso lastDayPayload.Close >= My.Settings.MinClose AndAlso lastDayPayload.Close <= My.Settings.MaxClose Then
                            Dim rawCashInstrument As Tuple(Of String, String) = _common.GetCurrentTradingSymbolWithInstrumentToken(Common.DataBaseTable.EOD_Cash, previousTradingDay, runningInstrument.RawInstrumentName)
                            If rawCashInstrument IsNot Nothing Then
                                runningInstrument.CashInstrumentToken = rawCashInstrument.Item2
                                runningInstrument.CashInstrumentName = rawCashInstrument.Item1
                                runningInstrument.LastDayOpen = lastDayPayload.Open
                                runningInstrument.LastDayLow = lastDayPayload.Low
                                runningInstrument.LastDayHigh = lastDayPayload.High
                                runningInstrument.LastDayClose = lastDayPayload.Close
                                If priceFilterdCurrentNFOInstruments Is Nothing Then priceFilterdCurrentNFOInstruments = New List(Of ActiveInstrumentData)
                                priceFilterdCurrentNFOInstruments.Add(runningInstrument)
                            End If
                        End If
                    Next
                    Dim highATRStocks As Concurrent.ConcurrentDictionary(Of String, Decimal()) = Nothing
                    Try
                        If priceFilterdCurrentNFOInstruments IsNot Nothing AndAlso priceFilterdCurrentNFOInstruments.Count > 0 Then
                            For i As Integer = 0 To priceFilterdCurrentNFOInstruments.Count - 1 Step 20
                                Dim numberOfData As Integer = If(priceFilterdCurrentNFOInstruments.Count - i > 20, 20, priceFilterdCurrentNFOInstruments.Count - i)
                                Dim tasks As IEnumerable(Of Task(Of Boolean)) = Nothing
                                tasks = priceFilterdCurrentNFOInstruments.GetRange(i, numberOfData).Select(Async Function(x)
                                                                                                               Try
                                                                                                                   _cts.Token.ThrowIfCancellationRequested()
                                                                                                                   Dim eodHistoricalData As Dictionary(Of Date, Payload) = Await _common.GetHistoricalDataAsync(Common.DataBaseTable.EOD_Cash, x.CashInstrumentName, previousTradingDay.AddDays(-300), previousTradingDay).ConfigureAwait(False)
                                                                                                                   _cts.Token.ThrowIfCancellationRequested()
                                                                                                                   If eodHistoricalData IsNot Nothing AndAlso eodHistoricalData.Count > 100 Then
                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
                                                                                                                       Dim ATRPayload As Dictionary(Of Date, Decimal) = Nothing
                                                                                                                       Indicator.ATR.CalculateATR(14, eodHistoricalData, ATRPayload)
                                                                                                                       _cts.Token.ThrowIfCancellationRequested()
                                                                                                                       If ATRPayload IsNot Nothing AndAlso ATRPayload.Count > 0 Then
                                                                                                                           Dim lastDayClosePrice As Decimal = eodHistoricalData.LastOrDefault.Value.Close
                                                                                                                           Dim atrPercentage As Decimal = (ATRPayload(eodHistoricalData.LastOrDefault.Key) / lastDayClosePrice) * 100
                                                                                                                           If atrPercentage >= My.Settings.ATRPercentage Then
                                                                                                                               _cts.Token.ThrowIfCancellationRequested()
                                                                                                                               'Dim volumePayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = eodHistoricalData.OrderByDescending(Function(z)
                                                                                                                               '                                                                                                              Return z.Key
                                                                                                                               '                                                                                                          End Function).Take(5)
                                                                                                                               '_cts.Token.ThrowIfCancellationRequested()
                                                                                                                               'If volumePayload IsNot Nothing AndAlso volumePayload.Count > 0 Then
                                                                                                                               '    _cts.Token.ThrowIfCancellationRequested()
                                                                                                                               '    Dim avgVolume As Decimal = volumePayload.Average(Function(z)
                                                                                                                               '                                                         Return z.Value.Volume
                                                                                                                               '                                                     End Function)
                                                                                                                               '    _cts.Token.ThrowIfCancellationRequested()
                                                                                                                               '    If avgVolume >= (My.Settings.PotentialAmount / 100) * lastDayClosePrice Then
                                                                                                                               If highATRStocks Is Nothing Then highATRStocks = New Concurrent.ConcurrentDictionary(Of String, Decimal())
                                                                                                                               highATRStocks.TryAdd(x.CashInstrumentName, {atrPercentage, ATRPayload(eodHistoricalData.LastOrDefault.Key), x.LastDayOpen, x.LastDayLow, x.LastDayHigh, x.LastDayClose})
                                                                                                                               '    End If
                                                                                                                               'End If
                                                                                                                           End If
                                                                                                                           'End If
                                                                                                                       End If
                                                                                                                   End If
                                                                                                               Catch ex As Exception
                                                                                                                   Console.WriteLine(String.Format("{0}:{1}", x.TradingSymbol, ex.ToString))
                                                                                                                   Throw ex
                                                                                                               End Try
                                                                                                               Return True
                                                                                                           End Function)

                                Dim mainTask As Task = Task.WhenAll(tasks)
                                Await mainTask.ConfigureAwait(False)
                                If mainTask.Exception IsNot Nothing Then
                                    Throw mainTask.Exception
                                End If
                            Next
                        End If
                    Catch cex As TaskCanceledException
                        Throw cex
                    Catch aex As AggregateException
                        Throw aex
                    Catch ex As Exception
                        Throw ex
                    End Try

                    If highATRStocks IsNot Nothing AndAlso highATRStocks.Count > 0 Then
                        For Each runningStock In highATRStocks.OrderByDescending(Function(x)
                                                                                     Return x.Value(0)
                                                                                 End Function)
                            _cts.Token.ThrowIfCancellationRequested()
                            Dim currentTradingSymbol As String = _common.GetCurrentTradingSymbol(Common.DataBaseTable.EOD_Cash, tradingDate, runningStock.Key)
                            If currentTradingSymbol IsNot Nothing Then
                                Dim lotSize As Integer = _common.GetLotSize(Common.DataBaseTable.EOD_Cash, currentTradingSymbol, tradingDate)
                                If ret Is Nothing Then ret = New Dictionary(Of String, InstrumentDetails)
                                ret.Add(runningStock.Key, New InstrumentDetails With
                                        {.ATRPercentage = runningStock.Value(0),
                                         .LotSize = lotSize,
                                         .DayATR = runningStock.Value(1),
                                         .PreviousDayOpen = runningStock.Value(2),
                                         .PreviousDayLow = runningStock.Value(3),
                                         .PreviousDayHigh = runningStock.Value(4),
                                         .PreviousDayClose = runningStock.Value(5)})
                            End If
                        Next
                    End If
                End If
            End If
        End If
        Return ret
    End Function



    Public Async Function IsTradableDay(ByVal tradingDate As Date) As Task(Of Boolean)
        Dim ret As Boolean = False
        Dim intradayHistoricalData As Dictionary(Of Date, Payload) = Await _common.GetHistoricalDataAsync(Common.DataBaseTable.Intraday_Cash, "JINDALSTEL", tradingDate, tradingDate).ConfigureAwait(False)
        If intradayHistoricalData IsNot Nothing AndAlso intradayHistoricalData.Count > 0 Then
            ret = True
        End If
        Return ret
    End Function

    Private Function CalculateSlab(ByVal price As Decimal, ByVal atrPer As Decimal) As Decimal
        Dim ret As Decimal = 0.25
        Dim slabList As List(Of Decimal) = New List(Of Decimal) From {0.25, 0.5, 1, 2.5, 5, 10, 15}
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

    Private Function GetActiveInstrumentData(ByVal tableType As Common.DataBaseTable, ByVal tradingDate As Date, ByVal stockList As Dictionary(Of String, InstrumentDetails)) As Dictionary(Of String, InstrumentDetails)
        Dim ret As Dictionary(Of String, InstrumentDetails) = Nothing
        If stockList IsNot Nothing AndAlso stockList.Count > 0 Then
            For Each stock In stockList.Keys
                _cts.Token.ThrowIfCancellationRequested()
                If _conn Is Nothing OrElse _conn.State <> ConnectionState.Open Then
                    _cts.Token.ThrowIfCancellationRequested()
                    _conn = _common.OpenDBConnection()
                End If
                _cts.Token.ThrowIfCancellationRequested()
                Dim rawInstrumentName As String = stock
                _cts.Token.ThrowIfCancellationRequested()
                Dim cm As MySqlCommand = Nothing
                Select Case tableType
                    Case Common.DataBaseTable.EOD_Cash, Common.DataBaseTable.Intraday_Cash
                        cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_cash` WHERE `TRADING_SYMBOL` LIKE @trd AND `AS_ON_DATE`=@sd", _conn)
                        cm.Parameters.AddWithValue("@trd", String.Format("{0}", rawInstrumentName))
                    Case Common.DataBaseTable.EOD_Commodity, Common.DataBaseTable.Intraday_Commodity
                        cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_commodity` WHERE `TRADING_SYMBOL` REGEXP @trd AND `SEGMENT`='MCX' AND `AS_ON_DATE`=@sd", _conn)
                        cm.Parameters.AddWithValue("@trd", String.Format("{0}[0-9][0-9]*", rawInstrumentName))
                    Case Common.DataBaseTable.EOD_Currency, Common.DataBaseTable.Intraday_Currency
                        cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_currency` WHERE `TRADING_SYMBOL` REGEXP @trd AND `SEGMENT`='CDS-FUT' AND `AS_ON_DATE`=@sd", _conn)
                        cm.Parameters.AddWithValue("@trd", String.Format("{0}[0-9][0-9]*", rawInstrumentName))
                    Case Common.DataBaseTable.EOD_Futures, Common.DataBaseTable.Intraday_Futures
                        cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_futures` WHERE `TRADING_SYMBOL` REGEXP @trd AND `SEGMENT`='NFO-FUT' AND `AS_ON_DATE`=@sd", _conn)
                        cm.Parameters.AddWithValue("@trd", String.Format("{0}[0-9][0-9]*", rawInstrumentName))
                End Select
                cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
                _cts.Token.ThrowIfCancellationRequested()
                Dim adapter As New MySqlDataAdapter(cm)
                adapter.SelectCommand.CommandTimeout = 300
                Dim dt As New DataTable
                _cts.Token.ThrowIfCancellationRequested()
                adapter.Fill(dt)
                _cts.Token.ThrowIfCancellationRequested()
                Dim activeInstruments As List(Of ActiveInstrumentData) = Nothing
                If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                    For i = 0 To dt.Rows.Count - 1
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim instrumentData As New ActiveInstrumentData With
                        {.Token = dt.Rows(i).Item(0),
                         .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
                         .Expiry = If(IsDBNull(dt.Rows(i).Item(2)), Date.MaxValue, dt.Rows(i).Item(2))}

                        Dim pattern As String = "([0-9][0-9]JAN)|([0-9][0-9]FEB)|([0-9][0-9]MAR)|([0-9][0-9]APR)|([0-9][0-9]MAY)|([0-9][0-9]JUN)|([0-9][0-9]JUL)|([0-9][0-9]AUG)|([0-9][0-9]SEP)|([0-9][0-9]OCT)|([0-9][0-9]NOV)|([0-9][0-9]DEC)"
                        If Regex.Matches(instrumentData.TradingSymbol, pattern).Count <= 1 Then
                            If activeInstruments Is Nothing Then activeInstruments = New List(Of ActiveInstrumentData)
                            activeInstruments.Add(instrumentData)
                            If instrumentData.TradingSymbol = stockList(stock).TradingSymbol Then
                                stockList(stock).InstrumentIdentifier = instrumentData.Token
                                stockList(stock).CurrentContractExpiry = instrumentData.Expiry
                            End If
                        End If
                    Next
                End If
                If activeInstruments IsNot Nothing AndAlso activeInstruments.Count > 0 Then
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim minExpiry As Date = activeInstruments.Min(Function(x)
                                                                      Return x.Expiry
                                                                  End Function)
                    If minExpiry.Date = stockList(stock).CurrentContractExpiry.Date Then
                        stockList(stock).IsTradable = True
                        _cts.Token.ThrowIfCancellationRequested()
                    Else
                        _cts.Token.ThrowIfCancellationRequested()
                        If minExpiry.Date < stockList(stock).CurrentContractExpiry.Date AndAlso minExpiry.Date > tradingDate.Date Then
                            _cts.Token.ThrowIfCancellationRequested()
                            Throw New ApplicationException(String.Format("Check stock {0} on {1}", stockList(stock).TradingSymbol, tradingDate))
                            stockList(stock).IsTradable = False
                            stockList(stock).PreviousContractTradingSymbol = activeInstruments.Find(Function(x)
                                                                                                        Return x.Expiry.Date = minExpiry.Date
                                                                                                    End Function).TradingSymbol
                            stockList(stock).PreviousContractExpiry = minExpiry
                        ElseIf minExpiry.Date < stockList(stock).CurrentContractExpiry.Date AndAlso minExpiry.Date = tradingDate.Date Then
                            _cts.Token.ThrowIfCancellationRequested()
                            stockList(stock).IsTradable = True
                            stockList(stock).PreviousContractTradingSymbol = activeInstruments.Find(Function(x)
                                                                                                        Return x.Expiry.Date = minExpiry.Date
                                                                                                    End Function).TradingSymbol
                            stockList(stock).PreviousContractExpiry = minExpiry
                        ElseIf minExpiry.Date < stockList(stock).CurrentContractExpiry.Date AndAlso minExpiry.Date < tradingDate.Date Then
                            stockList(stock).IsTradable = True
                            _cts.Token.ThrowIfCancellationRequested()
                        End If
                    End If
                    ret = stockList
                End If
            Next
        End If
        Return ret
    End Function

    Private Function GetStockPayload(ByVal tableType As Common.DataBaseTable, ByVal tradingDate As Date, ByVal instrumentData As InstrumentDetails, ByVal immediatePreviousDay As Boolean) As Dictionary(Of Date, Payload)
        Dim ret As Dictionary(Of Date, Payload) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim intradayTable As Common.DataBaseTable = Common.DataBaseTable.Intraday_Cash
        Select Case tableType
            Case Common.DataBaseTable.Intraday_Cash, Common.DataBaseTable.EOD_Cash
                intradayTable = Common.DataBaseTable.Intraday_Cash
            Case Common.DataBaseTable.Intraday_Commodity, Common.DataBaseTable.EOD_Commodity
                intradayTable = Common.DataBaseTable.Intraday_Commodity
            Case Common.DataBaseTable.Intraday_Currency, Common.DataBaseTable.EOD_Currency
                intradayTable = Common.DataBaseTable.Intraday_Currency
            Case Common.DataBaseTable.Intraday_Futures, Common.DataBaseTable.EOD_Futures
                intradayTable = Common.DataBaseTable.Intraday_Futures
        End Select
        _cts.Token.ThrowIfCancellationRequested()
        If instrumentData IsNot Nothing AndAlso instrumentData.IsTradable Then
            If instrumentData.PreviousContractTradingSymbol Is Nothing Then
                Dim previousTradingDate As Date = Date.MinValue
                If immediatePreviousDay Then
                    previousTradingDate = _common.GetPreviousTradingDay(intradayTable, tradingDate)
                Else
                    previousTradingDate = _common.GetPreviousTradingDay(intradayTable, instrumentData.TradingSymbol, tradingDate)
                End If
                If previousTradingDate <> Date.MinValue Then
                    ret = _common.GetRawPayloadForSpecificTradingSymbol(intradayTable, instrumentData.TradingSymbol, previousTradingDate, previousTradingDate)
                End If
            Else
                Dim previousTradingDate As Date = Date.MinValue
                If immediatePreviousDay Then
                    previousTradingDate = _common.GetPreviousTradingDay(intradayTable, tradingDate)
                Else
                    previousTradingDate = _common.GetPreviousTradingDay(intradayTable, instrumentData.PreviousContractTradingSymbol, tradingDate)
                End If
                If previousTradingDate <> Date.MinValue Then
                    ret = _common.GetRawPayloadForSpecificTradingSymbol(intradayTable, instrumentData.PreviousContractTradingSymbol, previousTradingDate, previousTradingDate)
                End If
            End If
        End If
        Return ret
    End Function

    Private Function CalculateBlankVolumePercentage(ByVal inputPayload As Dictionary(Of Date, Payload)) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim blankCandlePayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = inputPayload.Where(Function(x)
                                                                                                              Return x.Value.Open = x.Value.Low AndAlso
                                                                                                              x.Value.Low = x.Value.High AndAlso
                                                                                                              x.Value.High = x.Value.Close
                                                                                                          End Function)
            If blankCandlePayload IsNot Nothing AndAlso blankCandlePayload.Count > 0 Then
                ret = Math.Round((blankCandlePayload.Count / inputPayload.Count) * 100, 2)
            Else
                ret = 0
            End If
        End If
        Return ret
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
