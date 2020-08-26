Imports System.Threading
Imports Algo2TradeBLL

Public Class AjitJhaMultiIndicatorStocks
    Inherits StockSelection

    Private ReadOnly _stockList As List(Of String) = New List(Of String) From {"APOLLOHOSP", "AMBUJACEM", "BANDHANBNK", "AUROPHARMA", "BEL", "FEDERALBNK", "BPCL", "CADILAHC", "ADANIENT", "CUMMINSIND", "EICHERMOT", "CIPLA", "GAIL", "GODREJCP", "DABUR", "GRASIM", "HAVELLS", "HCLTECH", "DIVISLAB", "HDFC", "ADANIPORTS", "HDFCBANK", "GODREJPROP", "HINDALCO", "INFY", "BRITANNIA", "JINDALSTEL", "JUBLFOOD", "LT", "SUNPHARMA", "LUPIN", "TORNTPHARM", "MGL", "MINDTREE", "ASHOKLEY", "MRF", "UPL", "ASIANPAINT", "COFORGE", "INDUSINDBK", "LICHSGFIN", "ONGC", "AXISBANK", "PETRONET", "SHREECEM", "BAJAJ-AUTO", "BAJAJFINSV", "BALKRISIND", "SUNTV", "BATAINDIA", "BERGEPAINT", "TATACONSUM", "DRREDDY", "TATAPOWER", "TCS", "TITAN", "UJJIVAN", "BHARATFORG", "EQUITAS", "IGL", "BHARTIARTL", "MCDOWELL-N", "ICICIBANK", "JSWSTEEL", "NTPC", "AMARAJABAT", "ESCORTS", "BHEL", "BIOCON", "NMDC", "TATAMOTORS", "PEL", "ZEEL", "CANBK", "ACC", "CENTURYTEX", "CHOLAFIN", "COALINDIA", "COLPAL", "CONCOR", "M&MFIN", "MARICO", "HDFCLIFE", "MARUTI", "DLF", "VEDL", "VOLTAS", "EXIDEIND", "TATASTEEL", "GLENMARK", "GMRINFRA", "NAUKRI", "PIDILITIND", "APOLLOTYRE", "MANAPPURAM", "SRTRANSFIN", "HEROMOTOCO", "HINDPETRO", "HINDUNILVR", "ICICIPRULI", "IDEA", "IDFCFIRSTB", "INDIGO", "INFRATEL", "IOC", "ITC", "BOSCHLTD", "MFSL", "IBULHSGFIN", "MOTHERSUMI", "NESTLEIND", "RELIANCE", "M&M", "TATACHEM", "PAGEIND", "SAIL", "PFC", "PVR", "TVSMOTOR", "RAMCOCEM", "RBLBANK", "BANKBARODA", "SBILIFE", "SBIN", "TECHM", "SRF", "TORNTPOWER", "UBL", "ULTRACEMCO", "WIPRO", "L&TFH", "NATIONALUM", "PNB", "POWERGRID", "BAJFINANCE", "RECLTD", "MUTHOOTFIN", "KOTAKBANK", "SIEMENS"}

    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer)
        MyBase.New(canceller, cmn, stockType)
    End Sub

    Public Overrides Async Function GetStockDataAsync(startDate As Date, endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")

        Dim dtCtr As Integer = 0
        Dim tradingDate As Date = startDate
        While tradingDate <= endDate
            _canceller.Token.ThrowIfCancellationRequested()
            dtCtr += 1
            Dim tradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
            If tradingDay Then
                Dim exchangeStartTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0)
                Dim lastSignalTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 11, 55, 0)

                If _stockList IsNot Nothing AndAlso _stockList.Count > 0 Then
                    Dim stkCtr As Integer = 0
                    For Each runningStock In _stockList
                        _canceller.Token.ThrowIfCancellationRequested()
                        stkCtr += 1
                        OnHeartbeat(String.Format("Running for {0} #{1}/{2} #{3}/{4}", runningStock, stkCtr, _stockList.Count, dtCtr, DateDiff(DateInterval.Day, startDate, endDate) + 1))
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_intradayTable, runningStock, tradingDate.AddDays(-20), tradingDate)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                            Dim xMinutePayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(intradayPayload, 5, exchangeStartTime)
                            Dim currentDayPayload As Dictionary(Of Date, Payload) = Nothing
                            For Each runningPayload In xMinutePayload
                                _canceller.Token.ThrowIfCancellationRequested()
                                If runningPayload.Key.Date = tradingDate.Date Then
                                    If currentDayPayload Is Nothing Then currentDayPayload = New Dictionary(Of Date, Payload)
                                    currentDayPayload.Add(runningPayload.Key, runningPayload.Value)
                                End If
                            Next
                            If currentDayPayload IsNot Nothing AndAlso currentDayPayload.Count > 0 Then
                                Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_eodTable, runningStock, tradingDate.AddDays(-300), tradingDate.AddDays(-1))
                                If eodPayload IsNot Nothing AndAlso eodPayload.Count >= 150 Then
                                    Dim weeklyPayload As Dictionary(Of Date, Payload) = Common.ConvertDayPayloadsToWeek(eodPayload)
                                    Dim weeklySubPayload As Dictionary(Of Date, Payload) = Nothing
                                    Dim counter As Integer = 0
                                    For Each runningPayload In weeklyPayload.OrderByDescending(Function(x)
                                                                                                   Return x.Key
                                                                                               End Function)
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        If weeklySubPayload Is Nothing Then weeklySubPayload = New Dictionary(Of Date, Payload)
                                        weeklySubPayload.Add(runningPayload.Key, runningPayload.Value)

                                        counter += 1
                                        If counter >= 10 Then Exit For
                                    Next
                                    Dim weeklyHigh As Decimal = weeklySubPayload.Min(Function(x)
                                                                                         Return x.Value.High
                                                                                     End Function)
                                    Dim weeklyLow As Decimal = weeklySubPayload.Max(Function(x)
                                                                                        Return x.Value.Low
                                                                                    End Function)

                                    Dim smaPayload As Dictionary(Of Date, Decimal) = Nothing
                                    Dim emaPayload As Dictionary(Of Date, Decimal) = Nothing
                                    Dim rsiPayload As Dictionary(Of Date, Decimal) = Nothing
                                    Dim vwapPayload As Dictionary(Of Date, Decimal) = Nothing
                                    Indicator.SMA.CalculateSMA(20, Payload.PayloadFields.Volume, xMinutePayload, smaPayload)
                                    Indicator.EMA.CalculateEMA(20, Payload.PayloadFields.Close, xMinutePayload, emaPayload)
                                    Indicator.RSI.CalculateRSI(14, xMinutePayload, rsiPayload)
                                    Indicator.VWAP.CalculateVWAP(xMinutePayload, vwapPayload)

                                    For Each runningPayload In currentDayPayload
                                        _canceller.Token.ThrowIfCancellationRequested()
                                        Dim signalCandle As Payload = runningPayload.Value
                                        If signalCandle.PayloadDate <= lastSignalTime Then
                                            If signalCandle.High > weeklyHigh Then
                                                Dim lastestCandle As Payload = GetCurrentDayCandle(currentDayPayload, signalCandle, eodPayload.LastOrDefault.Value, tradingDate, runningStock)
                                                If signalCandle.Close > lastestCandle.PreviousCandlePayload.High Then
                                                    Dim latestPayload As Dictionary(Of Date, Payload) = Utilities.Strings.DeepClone(Of Dictionary(Of Date, Payload))(eodPayload)
                                                    latestPayload.Add(lastestCandle.PayloadDate, lastestCandle)

                                                    If signalCandle.Volume > smaPayload(signalCandle.PayloadDate) Then
                                                        Dim emaCls20 As Decimal = GetIndicatorLatestValue(latestPayload, IndicatorType.EMA_Close_20).Item1
                                                        If signalCandle.Close > emaCls20 Then
                                                            Dim emaCls50 As Decimal = GetIndicatorLatestValue(latestPayload, IndicatorType.EMA_Close_50).Item1
                                                            If emaPayload(signalCandle.PayloadDate) > emaCls50 Then
                                                                Dim macd As Tuple(Of Decimal, Decimal) = GetIndicatorLatestValue(latestPayload, IndicatorType.MACD_26_12_9)
                                                                If macd.Item1 > macd.Item2 Then
                                                                    Dim cci As Decimal = GetIndicatorLatestValue(latestPayload, IndicatorType.CCI_20).Item1
                                                                    If cci > 100 Then
                                                                        If rsiPayload(signalCandle.PayloadDate) > 60 Then
                                                                            If ((signalCandle.High / lastestCandle.Low) - 1) * 100 <= 1 Then
                                                                                If signalCandle.Close >= 100 Then
                                                                                    If signalCandle.Close > vwapPayload(signalCandle.PayloadDate) Then
                                                                                        If signalCandle.Close > signalCandle.Open Then
                                                                                            Dim row As DataRow = ret.NewRow
                                                                                            row("Date") = tradingDate.ToString("dd-MM-yyyy")
                                                                                            row("Trading Symbol") = runningStock
                                                                                            ret.Rows.Add(row)
                                                                                            Exit For
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
                                            ElseIf signalCandle.Low < weeklyLow Then
                                                Dim lastestCandle As Payload = GetCurrentDayCandle(currentDayPayload, signalCandle, eodPayload.LastOrDefault.Value, tradingDate, runningStock)
                                                If signalCandle.Close < lastestCandle.PreviousCandlePayload.Low Then
                                                    Dim latestPayload As Dictionary(Of Date, Payload) = Utilities.Strings.DeepClone(Of Dictionary(Of Date, Payload))(eodPayload)
                                                    latestPayload.Add(lastestCandle.PayloadDate, lastestCandle)

                                                    If signalCandle.Volume > smaPayload(signalCandle.PayloadDate) Then
                                                        Dim emaCls20 As Decimal = GetIndicatorLatestValue(latestPayload, IndicatorType.EMA_Close_20).Item1
                                                        If signalCandle.Close < emaCls20 Then
                                                            Dim emaCls50 As Decimal = GetIndicatorLatestValue(latestPayload, IndicatorType.EMA_Close_50).Item1
                                                            If emaPayload(signalCandle.PayloadDate) < emaCls50 Then
                                                                Dim macd As Tuple(Of Decimal, Decimal) = GetIndicatorLatestValue(latestPayload, IndicatorType.MACD_26_12_9)
                                                                If macd.Item1 < macd.Item2 Then
                                                                    Dim cci As Decimal = GetIndicatorLatestValue(latestPayload, IndicatorType.CCI_20).Item1
                                                                    If cci < -100 Then
                                                                        If rsiPayload(signalCandle.PayloadDate) < 40 Then
                                                                            If ((lastestCandle.High / signalCandle.Low) - 1) * 100 <= 1 Then
                                                                                If signalCandle.Close >= 100 Then
                                                                                    If signalCandle.Close < vwapPayload(signalCandle.PayloadDate) Then
                                                                                        If signalCandle.Close < signalCandle.Open Then
                                                                                            Dim row As DataRow = ret.NewRow
                                                                                            row("Date") = tradingDate.ToString("dd-MM-yyyy")
                                                                                            row("Trading Symbol") = runningStock
                                                                                            ret.Rows.Add(row)
                                                                                            Exit For
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
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    Next
                End If
            End If

            tradingDate = tradingDate.AddDays(1)
        End While
        Return ret
    End Function

    Private Function GetCurrentDayCandle(ByVal currentDayPayload As Dictionary(Of Date, Payload), ByVal signalCandle As Payload, ByVal lastDayPayload As Payload, ByVal tradingDate As Date, ByVal tradingSymbol As String) As Payload
        Dim ret As Payload = Nothing

        Dim open As Decimal = currentDayPayload.FirstOrDefault.Value.Open
        Dim low As Decimal = currentDayPayload.Min(Function(x)
                                                       If x.Key <= signalCandle.PayloadDate Then
                                                           Return x.Value.Low
                                                       Else
                                                           Return Decimal.MaxValue
                                                       End If
                                                   End Function)
        Dim high As Decimal = currentDayPayload.Max(Function(x)
                                                        If x.Key <= signalCandle.PayloadDate Then
                                                            Return x.Value.High
                                                        Else
                                                            Return Decimal.MinValue
                                                        End If
                                                    End Function)
        Dim close As Decimal = signalCandle.Close
        Dim volume As Long = currentDayPayload.Sum(Function(x)
                                                       If x.Key <= signalCandle.PayloadDate Then
                                                           Return x.Value.Volume
                                                       Else
                                                           Return 0
                                                       End If
                                                   End Function)


        ret = New Payload(Payload.CandleDataSource.Calculated) With {
            .Open = open,
            .Low = low,
            .High = high,
            .Close = close,
            .Volume = volume,
            .CumulativeVolume = volume,
            .TradingSymbol = tradingSymbol,
            .PayloadDate = tradingDate.Date,
            .PreviousCandlePayload = lastDayPayload
        }

        Return ret
    End Function

    Private Function GetIndicatorLatestValue(ByVal inputPayload As Dictionary(Of Date, Payload), ByVal typeOfIndicator As IndicatorType) As Tuple(Of Decimal, Decimal)
        Dim ret As Tuple(Of Decimal, Decimal) = Nothing
        Select Case typeOfIndicator
            Case IndicatorType.EMA_Close_20
                Dim emaPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.EMA.CalculateEMA(20, Payload.PayloadFields.Close, inputPayload, emaPayload)
                ret = New Tuple(Of Decimal, Decimal)(emaPayload.LastOrDefault.Value, Decimal.MinValue)
            Case IndicatorType.EMA_Close_50
                Dim emaPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.EMA.CalculateEMA(50, Payload.PayloadFields.Close, inputPayload, emaPayload)
                ret = New Tuple(Of Decimal, Decimal)(emaPayload.LastOrDefault.Value, Decimal.MinValue)
            Case IndicatorType.MACD_26_12_9
                Dim macdPayload As Dictionary(Of Date, Decimal) = Nothing
                Dim macdSignalPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.MACD.CalculateMACD(12, 26, 9, inputPayload, macdPayload, macdSignalPayload, Nothing)
                ret = New Tuple(Of Decimal, Decimal)(macdPayload.LastOrDefault.Value, macdSignalPayload.LastOrDefault.Value)
            Case IndicatorType.CCI_20
                Dim cciPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.CCI.CalculateCCI(20, inputPayload, cciPayload)
                ret = New Tuple(Of Decimal, Decimal)(cciPayload.LastOrDefault.Value, Decimal.MinValue)
            Case Else
                Throw New NotImplementedException
        End Select
        Return ret
    End Function

    Enum IndicatorType
        EMA_Close_20
        EMA_Close_50
        MACD_26_12_9
        CCI_20
    End Enum

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
