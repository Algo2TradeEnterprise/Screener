Imports System.Threading
Imports Algo2TradeBLL

Public Class AjitJhaMultiIndicatorStocks
    Inherits StockSelection

    Private ReadOnly _stockList As List(Of String) = New List(Of String) From {"3MINDIA", "ABB", "ACC", "AIAENG", "APLAPOLLO", "AUBANK", "AARTIIND", "AAVAS", "ABBOTINDIA", "ADANIGAS", "ADANIGREEN", "ADANIPORTS", "ADANIPOWER", "ADANITRANS", "ABCAPITAL", "ABFRL", "ADVENZYMES", "AEGISCHEM", "AFFLE", "AJANTPHARM", "AKZOINDIA", "APLLTD", "ALKEM", "ALKYLAMINE", "ALLCARGO", "AMARAJABAT", "AMBER", "AMBUJACEM", "APOLLOHOSP", "APOLLOTYRE", "ARVINDFASN", "ASAHIINDIA", "ASHOKLEY", "ASHOKA", "ASIANPAINT", "ASTERDM", "ASTRAZEN", "ASTRAL", "ATUL", "AUROPHARMA", "AVANTIFEED", "DMART", "AXISBANK", "BASF", "BEML", "BSE", "BAJAJ-AUTO", "BAJAJCON", "BAJAJELEC", "BAJFINANCE", "BAJAJFINSV", "BAJAJHLDNG", "BALKRISIND", "BALMLAWRIE", "BALRAMCHIN", "BANDHANBNK", "BANKBARODA", "BANKINDIA", "MAHABANK", "BATAINDIA", "BAYERCROP", "BERGEPAINT", "BDL", "BEL", "BHARATFORG", "BHEL", "BPCL", "BHARATRAS", "BHARTIARTL", "INFRATEL", "BIOCON", "BIRLACORPN", "BSOFT", "BLISSGVS", "BLUEDART", "BLUESTARCO", "BBTC", "BOMDYEING", "BOSCHLTD", "BRIGADE", "BRITANNIA", "CARERATING", "CCL", "CESC", "CRISIL", "CSBBANK", "CADILAHC", "CANFINHOME", "CANBK", "CAPLIPOINT", "CGCL", "CARBORUNIV", "CASTROLIND", "CEATLTD", "CENTRALBK", "CDSL", "CENTURYPLY", "CENTURYTEX", "CERA", "CHAMBLFERT", "CHENNPETRO", "CHOLAHLDNG", "CHOLAFIN", "CIPLA", "CUB", "COALINDIA", "COCHINSHIP", "COFORGE", "COLPAL", "CONCOR", "COROMANDEL", "CREDITACC", "CROMPTON", "CUMMINSIND", "CYIENT", "DBCORP", "DCBBANK", "DCMSHRIRAM", "DLF", "DABUR", "DALBHARAT", "DEEPAKNTR", "DELTACORP", "DHANUKA", "DBL", "DISHTV", "DCAL", "DIVISLAB", "DIXON", "LALPATHLAB", "DRREDDY", "EIDPARRY", "EIHOTEL", "ESABINDIA", "EDELWEISS", "EICHERMOT", "ELGIEQUIP", "EMAMILTD", "ENDURANCE", "ENGINERSIN", "EQUITAS", "ERIS", "ESCORTS", "ESSELPACK", "EXIDEIND", "FDC", "FEDERALBNK", "FINEORG", "FINCABLES", "FINPIPE", "FSL", "FORTIS", "FCONSUMER", "FRETAIL", "GAIL", "GEPIL", "GET&D", "GHCL", "GMMPFAUDLR", "GMRINFRA", "GALAXYSURF", "GRSE", "GARFIBRES", "GICRE", "GILLETTE", "GLAXO", "GLENMARK", "GODFRYPHLP", "GODREJAGRO", "GODREJCP", "GODREJIND", "GODREJPROP", "GRANULES", "GRAPHITE", "GRASIM", "GESHIP", "GREAVESCOT", "GRINDWELL", "GUJALKALI", "FLUOROCHEM", "GUJGASLTD", "GMDCLTD", "GNFC", "GPPL", "GSFC", "GSPL", "GULFOILLUB", "HEG", "HCLTECH", "HDFCAMC", "HDFCBANK", "HDFCLIFE", "HFCL", "HATHWAY", "HATSUN", "HAVELLS", "HEIDELBERG", "HERITGFOOD", "HEROMOTOCO", "HEXAWARE", "HSCL", "HIMATSEIDE", "HINDALCO", "HAL", "HINDCOPPER", "HINDPETRO", "HINDUNILVR", "HINDZINC", "HONAUT", "HUDCO", "HDFC", "ICICIBANK", "ICICIGI", "ICICIPRULI", "ISEC", "ICRA", "IDBI", "IDFCFIRSTB", "IDFC", "IFBIND", "IFCI", "IIFL", "IIFLWAM", "IRB", "IRCON", "ITC", "ITI", "INDIACEM", "ITDC", "IBULHSGFIN", "IBREALEST", "IBVENTURES", "INDIAMART", "INDIANB", "IEX", "INDHOTEL", "IOC", "IOB", "IRCTC", "INDOSTAR", "INDOCO", "IGL", "INDUSINDBK", "INFIBEAM", "NAUKRI", "INFY", "INGERRAND", "INOXLEISUR", "INTELLECT", "INDIGO", "IPCALAB", "JBCHEPHARM", "JKCEMENT", "JKLAKSHMI", "JKPAPER", "JKTYRE", "JMFINANCIL", "JSWENERGY", "JSWSTEEL", "JAGRAN", "JAICORPLTD", "J&KBANK", "JAMNAAUTO", "JINDALSAW", "JSLHISAR", "JSL", "JINDALSTEL", "JCHAC", "JUBLFOOD", "JUBILANT", "JUSTDIAL", "JYOTHYLAB", "KPRMILL", "KEI", "KNRCON", "KPITTECH", "KRBL", "KSB", "KAJARIACER", "KALPATPOWR", "KANSAINER", "KTKBANK", "KARURVYSYA", "KSCL", "KEC", "KOLTEPATIL", "KOTAKBANK", "L&TFH", "LTTS", "LICHSGFIN", "LAOPALA", "LAXMIMACH", "LTI", "LT", "LAURUSLABS", "LEMONTREE", "LINDEINDIA", "LUPIN", "LUXIND", "MASFIN", "MMTC", "MOIL", "MRF", "MGL", "MAHSCOOTER", "MAHSEAMLES", "M&MFIN", "M&M", "MAHINDCIE", "MHRIL", "MAHLOG", "MANAPPURAM", "MRPL", "MARICO", "MARUTI", "MFSL", "METROPOLIS", "MINDTREE", "MINDACORP", "MINDAIND", "MIDHANI", "MOTHERSUMI", "MOTILALOFS", "MPHASIS", "MCX", "MUTHOOTFIN", "NATCOPHARM", "NBCC", "NCC", "NESCO", "NHPC", "NLCINDIA", "NMDC", "NTPC", "NH", "NATIONALUM", "NFL", "NBVENTURES", "NAVINFLUOR", "NESTLEIND", "NILKAMAL", "NAM-INDIA", "OBEROIRLTY", "ONGC", "OIL", "OMAXE", "OFSS", "ORIENTCEM", "ORIENTELEC", "ORIENTREF", "PIIND", "PNBHOUSING", "PNCINFRA", "PTC", "PVR", "PAGEIND", "PERSISTENT", "PETRONET", "PFIZER", "PHILIPCARB", "PHOENIXLTD", "PIDILITIND", "PEL", "POLYMED", "POLYCAB", "PFC", "POWERGRID", "PRAJIND", "PRESTIGE", "PRSMJOHNSN", "PGHL", "PGHH", "PNB", "QUESS", "RBLBANK", "RECLTD", "RITES", "RADICO", "RVNL", "RAIN", "RAJESHEXPO", "RALLIS", "RCF", "RATNAMANI", "RAYMOND", "REDINGTON", "RELAXO", "RELIANCE", "REPCOHOME", "SBICARD", "SBILIFE", "SJVN", "SKFINDIA", "SRF", "SADBHAV", "SANOFI", "SCHAEFFLER", "SCHNEIDER", "SIS", "SEQUENT", "SFL", "SCI", "SHOPERSTOP", "SHREECEM", "RENUKA", "SHRIRAMCIT", "SRTRANSFIN", "SIEMENS", "SOBHA", "SOLARINDS", "SONATSOFTW", "SOUTHBANK", "SPANDANA", "SPICEJET", "STARCEMENT", "SBIN", "SAIL", "SWSOLAR", "STRTECH", "STAR", "SUDARSCHEM", "SUMICHEM", "SPARC", "SUNPHARMA", "SUNTV", "SUNDARMFIN", "SUNDRMFAST", "SUNTECK", "SUPRAJIT", "SUPREMEIND", "SUZLON", "SWANENERGY", "SYMPHONY", "SYNGENE", "TCIEXP", "TCNSBRANDS", "TTKPRESTIG", "TVTODAY", "TV18BRDCST", "TVSMOTOR", "TAKE", "TASTYBITE", "TATACOMM", "TCS", "TATACONSUM", "TATAELXSI", "TATAINVEST", "TATAMTRDVR", "TATAMOTORS", "TATAPOWER", "TATASTLBSL", "TATASTEEL", "TEAMLEASE", "TECHM", "NIACL", "RAMCOCEM", "THERMAX", "THYROCARE", "TIMETECHNO", "TIMKEN", "TITAN", "TORNTPHARM", "TORNTPOWER", "TRENT", "TRIDENT", "TIINDIA", "UCOBANK", "UFLEX", "UPL", "UJJIVAN", "UJJIVANSFB", "ULTRACEMCO", "UNIONBANK", "UBL", "MCDOWELL-N", "VGUARD", "VMART", "VIPIND", "VRLLOG", "VSTIND", "VAIBHAVGBL", "VAKRANGEE", "VTL", "VARROC", "VBL", "VENKEYS", "VESUVIUS", "VINATIORGA", "IDEA", "VOLTAS", "WABCOINDIA", "WELCORP", "WELSPUNIND", "WESTLIFE", "WHIRLPOOL", "WIPRO", "WOCKPHARMA", "ZEEL", "ZENSARTECH", "ZYDUSWELL", "ECLERX"}

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

        Dim tradingDate As Date = startDate
        While tradingDate <= endDate
            _canceller.Token.ThrowIfCancellationRequested()
            Dim tradingDay As Boolean = Await IsTradableDay(tradingDate).ConfigureAwait(False)
            If tradingDay Then
                If _stockList IsNot Nothing AndAlso _stockList.Count > 0 Then
                    Dim stkCtr As Integer = 0
                    For Each runningStock In _stockList
                        stkCtr += 1
                        OnHeartbeat(String.Format("Running for {0} #{1}/{2}", runningStock, stkCtr, _stockList.Count))
                        Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_intradayTable, runningStock, tradingDate.AddDays(-20), tradingDate)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                            Dim exchangeStartTime As Date = New Date(tradingDate.Year, tradingDate.Month, tradingDate.Day, 9, 15, 0)
                            Dim xMinutePayload As Dictionary(Of Date, Payload) = Common.ConvertPayloadsToXMinutes(intradayPayload, 5, exchangeStartTime)
                            Dim currentDayPayload As Dictionary(Of Date, Payload) = Nothing
                            For Each runningPayload In xMinutePayload
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

                                    Dim emaPayload As Dictionary(Of Date, Decimal) = Nothing
                                    Dim rsiPayload As Dictionary(Of Date, Decimal) = Nothing
                                    Dim vwapPayload As Dictionary(Of Date, Decimal) = Nothing
                                    Indicator.EMA.CalculateEMA(20, Payload.PayloadFields.Close, xMinutePayload, emaPayload)
                                    Indicator.RSI.CalculateRSI(14, xMinutePayload, rsiPayload)
                                    Indicator.VWAP.CalculateVWAP(xMinutePayload, vwapPayload)

                                    For Each runningPayload In currentDayPayload
                                        Dim signalCandle As Payload = runningPayload.Value
                                        If signalCandle.High > weeklyHigh Then
                                            Dim lastestCandle As Payload = GetCurrentDayCandle(currentDayPayload, signalCandle, eodPayload.LastOrDefault.Value, tradingDate, runningStock)
                                            If signalCandle.Close > lastestCandle.PreviousCandlePayload.High Then
                                                Dim lastestPayload As Dictionary(Of Date, Payload) = Utilities.Strings.DeepClone(Of Dictionary(Of Date, Payload))(eodPayload)
                                                lastestPayload.Add(lastestCandle.PayloadDate, lastestCandle)

                                                Dim smaVol20 As Decimal = GetIndicatorLatestValue(lastestPayload, IndicatorType.SMA_Volume_20).Item1
                                                'If lastestCandle.Volume > smaVol20 + 500000 Then
                                                Dim emaCls20 As Decimal = GetIndicatorLatestValue(lastestPayload, IndicatorType.EMA_Close_20).Item1
                                                If signalCandle.Close > emaCls20 Then
                                                    Dim emaCls50 As Decimal = GetIndicatorLatestValue(lastestPayload, IndicatorType.EMA_Close_50).Item1
                                                    If emaCls20 > emaCls50 Then
                                                        Dim macd As Tuple(Of Decimal, Decimal) = GetIndicatorLatestValue(lastestPayload, IndicatorType.MACD_26_12_9)
                                                        If macd.Item1 > macd.Item2 Then
                                                            Dim cci As Decimal = GetIndicatorLatestValue(lastestPayload, IndicatorType.CCI_20).Item1
                                                            If cci > 100 Then
                                                                If rsiPayload(signalCandle.PayloadDate) > 60 Then
                                                                    If signalCandle.High / lastestCandle.Low <= 1.015 Then
                                                                        If lastestCandle.Close >= 100 Then
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
                                                'End If
                                            End If
                                        ElseIf signalCandle.Low < weeklyLow Then
                                            Dim lastestCandle As Payload = GetCurrentDayCandle(currentDayPayload, signalCandle, eodPayload.LastOrDefault.Value, tradingDate, runningStock)
                                            If signalCandle.Close < lastestCandle.PreviousCandlePayload.Low Then
                                                Dim lastestPayload As Dictionary(Of Date, Payload) = Utilities.Strings.DeepClone(Of Dictionary(Of Date, Payload))(eodPayload)
                                                lastestPayload.Add(lastestCandle.PayloadDate, lastestCandle)

                                                Dim smaVol20 As Decimal = GetIndicatorLatestValue(lastestPayload, IndicatorType.SMA_Volume_20).Item1
                                                'If lastestCandle.Volume > smaVol20 + 500000 Then
                                                Dim emaCls20 As Decimal = GetIndicatorLatestValue(lastestPayload, IndicatorType.EMA_Close_20).Item1
                                                If signalCandle.Close < emaCls20 Then
                                                    Dim emaCls50 As Decimal = GetIndicatorLatestValue(lastestPayload, IndicatorType.EMA_Close_50).Item1
                                                    If emaCls20 < emaCls50 Then
                                                        Dim macd As Tuple(Of Decimal, Decimal) = GetIndicatorLatestValue(lastestPayload, IndicatorType.MACD_26_12_9)
                                                        If macd.Item1 < macd.Item2 Then
                                                            Dim cci As Decimal = GetIndicatorLatestValue(lastestPayload, IndicatorType.CCI_20).Item1
                                                            If cci < -100 Then
                                                                If rsiPayload(signalCandle.PayloadDate) < 40 Then
                                                                    If lastestCandle.High / signalCandle.Low <= 1.015 Then
                                                                        If lastestCandle.Close >= 100 Then
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
                                                'End If
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
            Case IndicatorType.SMA_Volume_20
                Dim smaPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(20, Payload.PayloadFields.Volume, inputPayload, smaPayload)
                ret = New Tuple(Of Decimal, Decimal)(smaPayload.LastOrDefault.Value, Decimal.MinValue)
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
        SMA_Volume_20
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
