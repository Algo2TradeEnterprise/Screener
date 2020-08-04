Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class PreviousDayStrongHKStocks
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
        ret.Columns.Add("Previous Day Open")
        ret.Columns.Add("Previous Day Low")
        ret.Columns.Add("Previous Day High")
        ret.Columns.Add("Previous Day Close")
        ret.Columns.Add("Slab")
        ret.Columns.Add("Target To Stoploss Multiplier")
        ret.Columns.Add("Direction")

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
                    Dim tempStockList As Dictionary(Of String, String()) = Nothing
                    For Each runningStock In atrStockList.Keys
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_eodTable, runningStock, tradingDate.AddDays(-50), tradingDate.AddDays(-1))
                        If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
                            Dim eodHKPayload As Dictionary(Of Date, Payload) = Nothing
                            Indicator.HeikenAshi.ConvertToHeikenAshi(eodPayload, eodHKPayload)
                            Dim lastDayHKPayload As Payload = eodHKPayload.LastOrDefault.Value
                            Dim direction As String = Nothing
                            If Math.Round(lastDayHKPayload.Open, 2) = Math.Round(lastDayHKPayload.Low, 2) Then
                                direction = "BULLISH"
                            ElseIf Math.Round(lastDayHKPayload.Open, 2) = Math.Round(lastDayHKPayload.High, 2) Then
                                direction = "BEARISH"
                            End If
                            If direction IsNot Nothing AndAlso direction.Trim <> "" Then
                                Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayload(_intradayTable, runningStock, tradingDate.AddDays(-15), tradingDate.AddDays(-1))
                                If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 100 Then
                                    Dim multiplier As Decimal = CalculateTargetToStoplossMultiplier(intradayPayload, atrStockList(runningStock).PreviousDayClose)

                                    If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                    tempStockList.Add(runningStock, {multiplier, direction})
                                End If
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
                            row("Slab") = atrStockList(runningStock.Key).Slab
                            row("Target To Stoploss Multiplier") = runningStock.Value(0)
                            row("Direction") = runningStock.Value(1)

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

    Private Function CalculateTargetToStoplossMultiplier(ByVal inputPayload As Dictionary(Of Date, Payload), ByVal price As Decimal) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim hkPayload As Dictionary(Of Date, Payload) = Nothing
            Indicator.HeikenAshi.ConvertToHeikenAshi(inputPayload, hkPayload)

            Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
            Indicator.ATR.CalculateATR(14, hkPayload, atrPayload, True)
            Dim lastTradingDay As Date = inputPayload.LastOrDefault.Key.Date
            Dim highestATR As Decimal = atrPayload.Max(Function(x)
                                                           If x.Key.Date = lastTradingDay.Date Then
                                                               Return x.Value
                                                           Else
                                                               Return Decimal.MinValue
                                                           End If
                                                       End Function)

            Dim slPoint As Decimal = Utilities.Numbers.ConvertFloorCeling(highestATR, 0.05, Utilities.Numbers.NumberManipulation.RoundOfType.Celing)
            Dim quantity As Integer = CalculateQuantityFromStoploss(price, price - slPoint, 500)
            Dim targetPoint As Decimal = CalculatorTargetPoint(price, quantity, 500)
            ret = Math.Round(targetPoint / slPoint, 2)
        End If
        Return ret
    End Function

    Private Function CalculateQuantityFromStoploss(ByVal buyPrice As Decimal, ByVal sellPrice As Decimal, ByVal netLossOfTrade As Decimal) As Integer
        Dim potentialBrokerage As Calculator.BrokerageAttributes = Nothing
        Dim calculator As New Calculator.BrokerageCalculator(_canceller)

        Dim quantity As Integer = 1
        Dim previousQuantity As Integer = 1
        For quantity = 1 To Integer.MaxValue
            calculator.Intraday_Equity(buyPrice, sellPrice, quantity, potentialBrokerage)

            If potentialBrokerage.NetProfitLoss < Math.Abs(netLossOfTrade) * -1 Then
                Exit For
            Else
                previousQuantity = quantity
            End If
        Next
        Return previousQuantity
    End Function

    Public Function CalculatorTargetPoint(ByVal entryPrice As Decimal, ByVal quantity As Integer, ByVal desiredProfitOfTrade As Decimal) As Decimal
        Dim potentialBrokerage As Calculator.BrokerageAttributes = Nothing
        Dim calculator As New Calculator.BrokerageCalculator(_canceller)
        Dim exitPrice As Decimal = entryPrice
        calculator.Intraday_Equity(entryPrice, exitPrice, quantity, potentialBrokerage)

        While Not potentialBrokerage.NetProfitLoss > desiredProfitOfTrade
            calculator.Intraday_Equity(entryPrice, exitPrice, quantity, potentialBrokerage)
            If potentialBrokerage.NetProfitLoss > desiredProfitOfTrade Then Exit While
            exitPrice += 0.05
        End While

        Return exitPrice - entryPrice
    End Function
End Class