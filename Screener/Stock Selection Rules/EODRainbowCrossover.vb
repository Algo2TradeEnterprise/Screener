Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class EODRainbowCrossover
    Inherits StockSelection

    Private _nifty50Stocks As List(Of String) = New List(Of String) From {"ADANIPORTS", "ASIANPAINT", "AXISBANK", "BAJAJ-AUTO", "BAJFINANCE", "BAJAJFINSV", "BPCL", "BHARTIARTL", "BRITANNIA", "CIPLA", "COALINDIA", "DIVISLAB", "DRREDDY", "EICHERMOT", "GAIL", "GRASIM", "HCLTECH", "HDFCBANK", "HDFCLIFE", "HEROMOTOCO", "HINDALCO", "HINDUNILVR", "HDFC", "ICICIBANK", "ITC", "IOC", "INDUSINDBK", "INFY", "JSWSTEEL", "KOTAKBANK", "LT", "M&M", "MARUTI", "NTPC", "NESTLEIND", "ONGC", "POWERGRID", "RELIANCE", "SBILIFE", "SHREECEM", "SBIN", "SUNPHARMA", "TCS", "TATAMOTORS", "TATASTEEL", "TECHM", "TITAN", "UPL", "ULTRACEMCO", "WIPRO"}

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
        ret.Columns.Add("Closed Below Rainbow")
        ret.Columns.Add("Closed Above Rainbow")
        ret.Columns.Add("NIFTY 50")

        Using atrStock As New ATRStockSelection(_canceller)
            AddHandler atrStock.Heartbeat, AddressOf OnHeartbeat

            Dim tradingDate As Date = startDate
            While tradingDate <= endDate
                _bannedStockFileName = Path.Combine(My.Application.Info.DirectoryPath, String.Format("Bannned Stocks {0}.csv", tradingDate.ToString("ddMMyyyy")))
                For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "Bannned Stocks *.csv")
                    If Not runningFile.Contains(tradingDate.ToString("ddMMyyyy")) Then File.Delete(runningFile)
                Next
                Dim bannedStockList As List(Of String) = Nothing
                'Using bannedStock As New BannedStockDataFetcher(_bannedStockFileName, _canceller)
                '    AddHandler bannedStock.Heartbeat, AddressOf OnHeartbeat
                '    bannedStockList = Await bannedStock.GetBannedStocksData(tradingDate).ConfigureAwait(False)
                'End Using

                Dim atrStockList As Dictionary(Of String, InstrumentDetails) = Await atrStock.GetATRStockData(_eodTable, tradingDate, bannedStockList, False).ConfigureAwait(False)
                If atrStockList IsNot Nothing AndAlso atrStockList.Count > 0 Then
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim tempStockList As Dictionary(Of String, String()) = Nothing
                    For Each runningStock In atrStockList.Keys
                        Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(Common.DataBaseTable.EOD_POSITIONAL, runningStock, tradingDate.AddYears(-2), tradingDate)
                        If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 AndAlso eodPayload.ContainsKey(tradingDate.Date) Then
                            Dim currentDayCandle As Payload = eodPayload(tradingDate.Date)

                            Dim rainbowPayload As Dictionary(Of Date, Indicator.RainbowMA) = Nothing
                            Indicator.RainbowMovingAverage.CalculateRainbowMovingAverage(7, eodPayload, rainbowPayload)

                            Dim lastOutsideRainbow As Tuple(Of Integer, Date) = GetLastOutsideRainbow(currentDayCandle, eodPayload, rainbowPayload)
                            If lastOutsideRainbow IsNot Nothing AndAlso lastOutsideRainbow.Item1 = -1 Then
                                Dim belowRainbow As String = lastOutsideRainbow.Item2.ToString("dd-MMM-yyyy")
                                Dim aboveRainbow As String = Nothing
                                If currentDayCandle.CandleColor = Color.Green Then
                                    Dim rainbow As Indicator.RainbowMA = rainbowPayload(currentDayCandle.PayloadDate)
                                    If currentDayCandle.Close > Math.Max(rainbow.SMA1, Math.Max(rainbow.SMA2, Math.Max(rainbow.SMA3, Math.Max(rainbow.SMA4, Math.Max(rainbow.SMA5, Math.Max(rainbow.SMA6, Math.Max(rainbow.SMA7, Math.Max(rainbow.SMA8, Math.Max(rainbow.SMA9, rainbow.SMA10))))))))) Then
                                        aboveRainbow = currentDayCandle.PayloadDate.ToString("dd-MMM-yyyy")
                                    End If
                                End If

                                Dim nifty50 As String = "N"
                                If _nifty50Stocks.Contains(runningStock) Then nifty50 = "Y"

                                If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                tempStockList.Add(runningStock, {belowRainbow, aboveRainbow, nifty50})
                            End If
                        End If
                    Next
                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList
                            _canceller.Token.ThrowIfCancellationRequested()
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
                            row("Closed Below Rainbow") = runningStock.Value(0)
                            row("Closed Above Rainbow") = runningStock.Value(1)
                            row("NIFTY 50") = runningStock.Value(2)

                            ret.Rows.Add(row)
                            stockCounter += 1
                            If stockCounter = My.Settings.NumberOfStockPerDay Then Exit For
                        Next
                    End If
                End If

                tradingDate = tradingDate.AddDays(1)
            End While
        End Using
        Return ret
    End Function

    Private Function GetLastOutsideRainbow(ByVal candle As Payload, ByVal signalPayload As Dictionary(Of Date, Payload), ByVal rainbowPayload As Dictionary(Of Date, Indicator.RainbowMA)) As Tuple(Of Integer, Date)
        Dim ret As Tuple(Of Integer, Date) = Nothing
        For Each runningPayload In signalPayload.OrderByDescending(Function(x)
                                                                       Return x.Key
                                                                   End Function)
            If runningPayload.Key <= candle.PreviousCandlePayload.PayloadDate Then
                Dim rainbow As Indicator.RainbowMA = rainbowPayload(runningPayload.Key)
                If runningPayload.Value.Close > Math.Max(rainbow.SMA1, Math.Max(rainbow.SMA2, Math.Max(rainbow.SMA3, Math.Max(rainbow.SMA4, Math.Max(rainbow.SMA5, Math.Max(rainbow.SMA6, Math.Max(rainbow.SMA7, Math.Max(rainbow.SMA8, Math.Max(rainbow.SMA9, rainbow.SMA10))))))))) Then
                    If runningPayload.Value.CandleColor = Color.Green Then
                        ret = New Tuple(Of Integer, Date)(1, runningPayload.Key)
                        Exit For
                    End If
                ElseIf runningPayload.Value.Close < Math.Min(rainbow.SMA1, Math.Min(rainbow.SMA2, Math.Min(rainbow.SMA3, Math.Min(rainbow.SMA4, Math.Min(rainbow.SMA5, Math.Min(rainbow.SMA6, Math.Min(rainbow.SMA7, Math.Min(rainbow.SMA8, Math.Min(rainbow.SMA9, rainbow.SMA10))))))))) Then
                    If runningPayload.Value.CandleColor = Color.Red Then
                        ret = New Tuple(Of Integer, Date)(-1, runningPayload.Key)
                        Exit For
                    End If
                End If
            End If
        Next
        Return ret
    End Function
End Class