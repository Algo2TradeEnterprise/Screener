Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class TopGainerTopLosserOptions
    Inherits StockSelection

    Private ReadOnly _checkingTime As Date = New Date(Now.Year, Now.Month, Now.Day, 9, 29, 0)

    'Public Sub New(ByVal canceller As CancellationTokenSource,
    '               ByVal cmn As Common,
    '               ByVal stockType As Integer,
    '               ByVal checkingTime As Date)
    '    MyBase.New(canceller, cmn, stockType)
    '    _checkingTime = checkingTime
    'End Sub
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
        ret.Columns.Add("Previous Day Open")
        ret.Columns.Add("Previous Day Low")
        ret.Columns.Add("Previous Day High")
        ret.Columns.Add("Previous Day Close")
        ret.Columns.Add("Current Day Close")
        ret.Columns.Add("Slab")
        ret.Columns.Add("Gain Loss %")
        ret.Columns.Add("Close Price")
        ret.Columns.Add("Option Trading Symbol")

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
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim payloadTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, _checkingTime.Hour, _checkingTime.Minute, 0)
                    Dim tempStockList As Dictionary(Of String, String()) = Nothing
                    For Each runningStock In atrStockList.Keys
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim currentDayGainLossPercentage As Tuple(Of Decimal, Decimal) = GetGainLossPercentage(tradingDate, runningStock, payloadTime, atrStockList(runningStock).PreviousDayClose)
                        If currentDayGainLossPercentage IsNot Nothing AndAlso currentDayGainLossPercentage.Item1 <> Decimal.MinValue Then
                            If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                            tempStockList.Add(runningStock, {Math.Round(currentDayGainLossPercentage.Item1, 4), Math.Round(currentDayGainLossPercentage.Item2, 4)})
                        End If
                    Next
                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList.OrderByDescending(Function(x)
                                                                                     Return CDec(x.Value(0))
                                                                                 End Function)
                            _canceller.Token.ThrowIfCancellationRequested()
                            Dim optionStocks As Dictionary(Of Decimal, String) = Await GetCurrentOptionContractsAsync(runningStock.Key, tradingDate, "CE")
                            If optionStocks IsNot Nothing AndAlso optionStocks.Count > 0 Then
                                Dim closePrice As Decimal = runningStock.Value(1)
                                Dim potentialStrike As Decimal = closePrice + closePrice * 1 / 100

                                Dim strikePrice As Decimal = optionStocks.Where(Function(x)
                                                                                    Return x.Key >= potentialStrike
                                                                                End Function).OrderBy(Function(y)
                                                                                                          Return y.Key
                                                                                                      End Function).FirstOrDefault.Key

                                Dim optionSymbol As String = optionStocks(strikePrice)

                                Dim row As DataRow = ret.NewRow
                                row("Date") = tradingDate.ToString("dd-MM-yyyy")
                                row("Trading Symbol") = atrStockList(runningStock.Key).TradingSymbol
                                row("Lot Size") = atrStockList(runningStock.Key).LotSize
                                row("ATR %") = Math.Round(atrStockList(runningStock.Key).ATRPercentage, 4)
                                row("Blank Candle %") = atrStockList(runningStock.Key).BlankCandlePercentage
                                row("Day ATR") = Math.Round(atrStockList(runningStock.Key).DayATR, 4)
                                row("Previous Day Open") = atrStockList(runningStock.Key).PreviousDayOpen
                                row("Previous Day Low") = atrStockList(runningStock.Key).PreviousDayLow
                                row("Previous Day High") = atrStockList(runningStock.Key).PreviousDayHigh
                                row("Previous Day Close") = atrStockList(runningStock.Key).PreviousDayClose
                                row("Current Day Close") = atrStockList(runningStock.Key).CurrentDayClose
                                row("Slab") = atrStockList(runningStock.Key).Slab
                                row("Gain Loss %") = runningStock.Value(0)
                                row("Close Price") = runningStock.Value(1)
                                row("Option Trading Symbol") = optionSymbol


                                ret.Rows.Add(row)
                                stockCounter += 1
                            End If
                            If stockCounter = My.Settings.NumberOfStockPerDay Then Exit For
                        Next
                        If My.Settings.NumberOfStockPerDay < tempStockList.Count Then
                            stockCounter = 0
                            For Each runningStock In tempStockList.OrderBy(Function(x)
                                                                               Return CDec(x.Value(0))
                                                                           End Function)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim optionStocks As Dictionary(Of Decimal, String) = Await GetCurrentOptionContractsAsync(runningStock.Key, tradingDate, "PE")
                                If optionStocks IsNot Nothing AndAlso optionStocks.Count > 0 Then
                                    Dim closePrice As Decimal = runningStock.Value(1)
                                    Dim potentialStrike As Decimal = closePrice - closePrice * 1 / 100

                                    Dim strikePrice As Decimal = optionStocks.Where(Function(x)
                                                                                        Return x.Key <= potentialStrike
                                                                                    End Function).OrderBy(Function(y)
                                                                                                              Return y.Key
                                                                                                          End Function).LastOrDefault.Key

                                    Dim optionSymbol As String = optionStocks(strikePrice)

                                    Dim row As DataRow = ret.NewRow
                                    row("Date") = tradingDate.ToString("dd-MM-yyyy")
                                    row("Trading Symbol") = atrStockList(runningStock.Key).TradingSymbol
                                    row("Lot Size") = atrStockList(runningStock.Key).LotSize
                                    row("ATR %") = Math.Round(atrStockList(runningStock.Key).ATRPercentage, 4)
                                    row("Blank Candle %") = atrStockList(runningStock.Key).BlankCandlePercentage
                                    row("Day ATR") = Math.Round(atrStockList(runningStock.Key).DayATR, 4)
                                    row("Previous Day Open") = atrStockList(runningStock.Key).PreviousDayOpen
                                    row("Previous Day Low") = atrStockList(runningStock.Key).PreviousDayLow
                                    row("Previous Day High") = atrStockList(runningStock.Key).PreviousDayHigh
                                    row("Previous Day Close") = atrStockList(runningStock.Key).PreviousDayClose
                                    row("Current Day Close") = atrStockList(runningStock.Key).CurrentDayClose
                                    row("Slab") = atrStockList(runningStock.Key).Slab
                                    row("Gain Loss %") = runningStock.Value(0)
                                    row("Close Price") = runningStock.Value(1)
                                    row("Option Trading Symbol") = optionSymbol


                                    ret.Rows.Add(row)
                                    stockCounter += 1
                                End If
                                If stockCounter = My.Settings.NumberOfStockPerDay Then Exit For
                            Next

                            ret = ret.DefaultView.ToTable(True, "Date", "Trading Symbol", "Lot Size", "ATR %", "Blank Candle %", "Day ATR", "Previous Day Open", "Previous Day Low", "Previous Day High", "Previous Day Close", "Slab", "Gain Loss %", "Close Price", "Option Trading Symbol")
                        End If
                    End If
                End If

                tradingDate = tradingDate.AddDays(1)
            End While
        End Using
        Return ret
    End Function

    Private Function GetGainLossPercentage(ByVal tradingDate As Date, ByVal stock As String, ByVal time As Date, ByVal previousClose As Decimal) As Tuple(Of Decimal, Decimal)
        Dim ret As Tuple(Of Decimal, Decimal) = Nothing
        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, stock, tradingDate.AddDays(-3), tradingDate)
        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
            Dim payloadTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, time.Hour, time.Minute, 0)
            Dim candleToCheck As Payload = intradayPayload.Values.Where(Function(x)
                                                                            Return x.PayloadDate <= payloadTime
                                                                        End Function).LastOrDefault
            If candleToCheck IsNot Nothing AndAlso candleToCheck.PreviousCandlePayload IsNot Nothing Then
                ret = New Tuple(Of Decimal, Decimal)(((candleToCheck.Close - previousClose) / previousClose) * 100, candleToCheck.Close)
            End If
        End If
        Return ret
    End Function

    Private Async Function GetCurrentOptionContractsAsync(ByVal rawInstrumentName As String, ByVal tradingDate As Date, ByVal optionType As String) As Task(Of Dictionary(Of Decimal, String))
        Dim ret As Dictionary(Of Decimal, String) = Nothing
        Dim tableName As String = "active_instruments_futures"
        Dim queryString As String = String.Format("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `{0}` WHERE `AS_ON_DATE`='{1}' AND `TRADING_SYMBOL` REGEXP '^{2}[0-9][0-9]*' AND `SEGMENT`='NFO-OPT'",
                                                   tableName, tradingDate.ToString("yyyy-MM-dd"), rawInstrumentName)
        _canceller.Token.ThrowIfCancellationRequested()
        Dim dt As DataTable = Await _cmn.RunSelectAsync(queryString).ConfigureAwait(False)
        _canceller.Token.ThrowIfCancellationRequested()
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim activeInstruments As List(Of ActiveInstrumentData) = Nothing
            For i = 0 To dt.Rows.Count - 1
                _canceller.Token.ThrowIfCancellationRequested()
                Dim tradingSymbol As String = dt.Rows(i).Item(1).ToString.ToUpper
                If tradingSymbol.EndsWith(optionType) Then
                    Dim instrumentData As New ActiveInstrumentData With
                    {.Token = dt.Rows(i).Item(0),
                     .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
                     .Expiry = If(IsDBNull(dt.Rows(i).Item(2)), Date.MaxValue, dt.Rows(i).Item(2))}
                    If activeInstruments Is Nothing Then activeInstruments = New List(Of ActiveInstrumentData)
                    activeInstruments.Add(instrumentData)
                End If
            Next
            If activeInstruments IsNot Nothing AndAlso activeInstruments.Count > 0 Then
                _canceller.Token.ThrowIfCancellationRequested()
                Dim minExpiry As Date = activeInstruments.Min(Function(x)
                                                                  If x.Expiry.Date >= tradingDate.Date Then
                                                                      Return x.Expiry
                                                                  Else
                                                                      Return Date.MaxValue
                                                                  End If
                                                              End Function)
                If minExpiry <> Date.MinValue Then
                    For Each runningOptStock In activeInstruments
                        _canceller.Token.ThrowIfCancellationRequested()
                        If runningOptStock.Expiry.Date = minExpiry.Date Then
                            If ret Is Nothing Then ret = New Dictionary(Of Decimal, String)
                            Dim strikeExpiryString As String = Utilities.Strings.GetTextBetween(rawInstrumentName, optionType, runningOptStock.TradingSymbol)
                            Dim strike As String = strikeExpiryString.Substring(5)
                            If strike IsNot Nothing AndAlso strike.Trim <> "" AndAlso IsNumeric(strike) Then
                                ret.Add(strike, runningOptStock.TradingSymbol)
                            End If
                        End If
                    Next
                End If
            End If
        End If
        Return ret
    End Function
End Class