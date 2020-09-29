Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL
Imports MySql.Data.MySqlClient

Public Class PreMarketOptions
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
        ret.Columns.Add("Blank Candle %")
        ret.Columns.Add("Day ATR")
        ret.Columns.Add("Slab")

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
                    Dim conn As MySqlConnection = Nothing
                    If conn Is Nothing OrElse conn.State <> ConnectionState.Open Then
                        _canceller.Token.ThrowIfCancellationRequested()
                        conn = _cmn.OpenDBConnection()
                    End If
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim cm As MySqlCommand = New MySqlCommand("SELECT * FROM `v_pre_market` WHERE `APPLICABLE_DATE`=@sd", conn)
                    cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim adapter As New MySqlDataAdapter(cm)
                    adapter.SelectCommand.CommandTimeout = 300
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim dt As DataTable = New DataTable
                    adapter.Fill(dt)
                    _canceller.Token.ThrowIfCancellationRequested()
                    If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                        Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
                        For i = 0 To dt.Rows.Count - 1
                            _canceller.Token.ThrowIfCancellationRequested()
                            Dim instrumentName As String = dt.Rows(i).Item("SYMBOL").ToString.ToUpper
                            Dim valueInLakhs As Double = dt.Rows(i).Item("Value")
                            Dim quantity As Double = dt.Rows(i).Item("Volume")
                            Dim changePer As String = Math.Round(((dt.Rows(i).Item("Price") / dt.Rows(i).Item("Prev Close")) - 1) * 100, 2)
                            If atrStockList.ContainsKey(instrumentName) Then
                                If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
                                tempStockList.Add(instrumentName, {changePer, dt.Rows(i).Item("Prev Close"), valueInLakhs, quantity})
                            End If
                        Next
                        If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                            Dim stockCounter As Integer = 0
                            For Each runningStock In tempStockList.OrderByDescending(Function(x)
                                                                                         Return Math.Abs(x.Value(3))
                                                                                     End Function)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim peTradingSymbols As Dictionary(Of Decimal, String) = Await GetOptionTradingSymbolsAsync(runningStock.Key, "PE", tradingDate).ConfigureAwait(False)
                                Dim ceTradingSymbols As Dictionary(Of Decimal, String) = Await GetOptionTradingSymbolsAsync(runningStock.Key, "CE", tradingDate).ConfigureAwait(False)
                                If peTradingSymbols IsNot Nothing AndAlso peTradingSymbols.Count > 0 AndAlso ceTradingSymbols IsNot Nothing AndAlso ceTradingSymbols.Count > 0 Then
                                    Dim currentDayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_eodTable, runningStock.Key, tradingDate, tradingDate)
                                    If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 Then
                                        Dim open As Decimal = currentDayPayload.LastOrDefault.Value.Open
                                        Dim strikePrice As Decimal = peTradingSymbols.Keys.Where(Function(x)
                                                                                                     Return x >= open AndAlso CInt(x) = x
                                                                                                 End Function).OrderBy(Function(y)
                                                                                                                           Return y
                                                                                                                       End Function).FirstOrDefault
                                        If peTradingSymbols.ContainsKey(strikePrice) AndAlso ceTradingSymbols.ContainsKey(strikePrice) Then
                                            Dim row1 As DataRow = ret.NewRow
                                            row1("Date") = tradingDate.ToString("dd-MM-yyyy")
                                            row1("Trading Symbol") = ceTradingSymbols(strikePrice)
                                            row1("Lot Size") = atrStockList(runningStock.Key).LotSize
                                            row1("ATR %") = Math.Round(atrStockList(runningStock.Key).ATRPercentage, 4)
                                            row1("Blank Candle %") = atrStockList(runningStock.Key).BlankCandlePercentage
                                            row1("Day ATR") = Math.Round(atrStockList(runningStock.Key).DayATR, 4)
                                            row1("Slab") = atrStockList(runningStock.Key).Slab

                                            Dim row2 As DataRow = ret.NewRow
                                            row2("Date") = tradingDate.ToString("dd-MM-yyyy")
                                            row2("Trading Symbol") = peTradingSymbols(strikePrice)
                                            row2("Lot Size") = atrStockList(runningStock.Key).LotSize
                                            row2("ATR %") = Math.Round(atrStockList(runningStock.Key).ATRPercentage, 4)
                                            row2("Blank Candle %") = atrStockList(runningStock.Key).BlankCandlePercentage
                                            row2("Day ATR") = Math.Round(atrStockList(runningStock.Key).DayATR, 4)
                                            row2("Slab") = atrStockList(runningStock.Key).Slab

                                            ret.Rows.Add(row1)
                                            ret.Rows.Add(row2)
                                        End If
                                    End If
                                End If

                                stockCounter += 1
                                If stockCounter = My.Settings.NumberOfStockPerDay Then Exit For
                            Next
                        End If
                    End If
                End If

                tradingDate = tradingDate.AddDays(1)
            End While
        End Using
        Return ret
    End Function

    Private Async Function GetOptionTradingSymbolsAsync(ByVal stockName As String, ByVal instrumentType As String, ByVal tradingDate As Date) As Task(Of Dictionary(Of Decimal, String))
        Dim ret As Dictionary(Of Decimal, String) = Nothing
        Dim futureTradingSymbol As String = _cmn.GetCurrentTradingSymbol(Common.DataBaseTable.EOD_Futures, tradingDate, stockName)
        If futureTradingSymbol IsNot Nothing Then
            Dim optionTradingSymbol As String = futureTradingSymbol.Replace("FUT", String.Format("%{0}", instrumentType))
            Dim query As String = "SELECT DISTINCT(`TRADING_SYMBOL`) FROM `active_instruments_futures` WHERE `TRADING_SYMBOL` LIKE '{0}' AND `AS_ON_DATE`='{1}'"
            query = String.Format(query, optionTradingSymbol, tradingDate.ToString("yyyy-MM-dd"))
            Dim dt As DataTable = Await _cmn.RunSelectAsync(query).ConfigureAwait(False)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                Dim firstPart As String = futureTradingSymbol.Substring(0, futureTradingSymbol.Count - 3)
                Dim i As Integer = 0
                While Not i = dt.Rows.Count()
                    If Not IsDBNull(dt.Rows(i).Item("TRADING_SYMBOL")) Then
                        Dim tradingSymbol As String = dt.Rows(i).Item("TRADING_SYMBOL")
                        Dim strikePrice As String = Utilities.Strings.GetTextBetween(firstPart, instrumentType, tradingSymbol)
                        If strikePrice IsNot Nothing AndAlso IsNumeric(strikePrice) Then
                            If ret Is Nothing Then ret = New Dictionary(Of Decimal, String)
                            ret.Add(Val(strikePrice), tradingSymbol)
                        End If
                    End If
                    i += 1
                End While
            End If
        End If
        Return ret
    End Function
End Class