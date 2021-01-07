Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class HighATRHighVolumeStocks
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
        ret.Columns.Add("Current Day Close")
        ret.Columns.Add("Slab")
        ret.Columns.Add("Volume Per Price")

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
                    Dim tempStockList As Dictionary(Of String, Long) = Nothing
                    For Each runningStock In atrStockList
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_eodTable, runningStock.Value.TradingSymbol, tradingDate.AddDays(-8), tradingDate)
                        If eodPayload IsNot Nothing AndAlso eodPayload.ContainsKey(tradingDate.Date) Then
                            Dim candle As Payload = eodPayload(tradingDate.Date)
                            If candle.Volume >= 500000 Then
                                If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Long)
                                tempStockList.Add(runningStock.Key, Math.Ceiling(candle.Volume / candle.CandleRange))
                            End If
                        End If
                    Next

                    If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                        Dim stockCounter As Integer = 0
                        For Each runningStock In tempStockList.OrderByDescending(Function(x)
                                                                                     Return x.Value
                                                                                 End Function)
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
                            row("Volume Per Price") = runningStock.Value

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

    Public Function CalculateQuantityFromStoploss(ByVal buyPrice As Decimal, ByVal sellPrice As Decimal, ByVal netLossPerTrade As Decimal) As Integer
        Dim ret As Integer = 1

        Dim calculator As New Calculator.BrokerageCalculator(_canceller)
        For quantity = 1 To Integer.MaxValue
            Dim potentialBrokerage As Calculator.BrokerageAttributes = New Calculator.BrokerageAttributes
            calculator.Intraday_Equity(buyPrice, sellPrice, quantity, potentialBrokerage)

            If potentialBrokerage.NetProfitLoss < netLossPerTrade Then
                Exit For
            Else
                ret = quantity
            End If
        Next
        Return ret
    End Function

    Public Function CalculateTarget(ByVal entryPrice As Decimal, ByVal quantity As Integer, ByVal desiredProfitOfTrade As Decimal) As Decimal
        Dim potentialBrokerage As Calculator.BrokerageAttributes = Nothing
        Dim calculator As New Calculator.BrokerageCalculator(_canceller)

        Dim ret As Decimal = entryPrice
        potentialBrokerage = New Calculator.BrokerageAttributes
        While Not potentialBrokerage.NetProfitLoss > desiredProfitOfTrade
            calculator.Intraday_Equity(entryPrice, ret, quantity, potentialBrokerage)
            If potentialBrokerage.NetProfitLoss > desiredProfitOfTrade Then Exit While
            ret += 0.05
        End While

        Return ret
    End Function
End Class