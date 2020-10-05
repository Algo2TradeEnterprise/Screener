Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class FractalConstriction
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
        ret.Columns.Add("Target to Stoploss Multiplier")
        ret.Columns.Add("Constriction End Time")

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
                    Dim previousTradingDay As Date = _cmn.GetPreviousTradingDay(_intradayTable, tradingDate)
                    Dim stockCounter As Integer = 0
                    For Each runningStock In atrStockList
                        _canceller.Token.ThrowIfCancellationRequested()
                        'Dim intradayPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_intradayTable, runningStock.Value.TradingSymbol, tradingDate.AddDays(-7), tradingDate)
                        Dim intradayPayload As Dictionary(Of Date, Payload) = Await _cmn.GetHistoricalDataAsync(_intradayTable, runningStock.Value.TradingSymbol, tradingDate.AddDays(-7), tradingDate).ConfigureAwait(False)
                        If intradayPayload IsNot Nothing AndAlso intradayPayload.Count > 0 Then
                            Dim hkPayload As Dictionary(Of Date, Payload) = Nothing
                            Indicator.HeikenAshi.ConvertToHeikenAshi(intradayPayload, hkPayload)

                            Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.ATR.CalculateATR(14, hkPayload, atrPayload)

                            Dim highestATR As Decimal = atrPayload.Max(Function(x)
                                                                           If x.Key.Date = previousTradingDay.Date Then
                                                                               Return x.Value
                                                                           Else
                                                                               Return Decimal.MinValue
                                                                           End If
                                                                       End Function)

                            Dim slPoint As Decimal = Utilities.Numbers.ConvertFloorCeling(highestATR, 0.05, Utilities.Numbers.NumberManipulation.RoundOfType.Celing)
                            Dim quantity As Integer = CalculateQuantityFromStoploss(runningStock.Value.PreviousDayClose, runningStock.Value.PreviousDayClose - slPoint, -500)
                            Dim target As Decimal = CalculateTarget(runningStock.Value.PreviousDayClose, quantity, 500)
                            Dim multiplier As Decimal = (target - runningStock.Value.PreviousDayClose) / slPoint

                            Dim fractalHighPayload As Dictionary(Of Date, Decimal) = Nothing
                            Dim fractalLowPayload As Dictionary(Of Date, Decimal) = Nothing
                            Indicator.FractalBands.CalculateFractal(intradayPayload, fractalHighPayload, fractalLowPayload)

                            Dim constrictionEndTime As Date = Date.MinValue
                            Dim constriction As Tuple(Of Boolean, Date) = IsFractalConstrictionDone(fractalHighPayload, fractalLowPayload, hkPayload, tradingDate)
                            If constriction IsNot Nothing AndAlso constriction.Item1 Then
                                constrictionEndTime = constriction.Item2
                            End If

                            Dim row As DataRow = ret.NewRow
                            row("Date") = tradingDate.ToString("dd-MM-yyyy")
                            row("Trading Symbol") = runningStock.Value.TradingSymbol
                            row("Lot Size") = runningStock.Value.LotSize
                            row("ATR %") = Math.Round(runningStock.Value.ATRPercentage, 4)
                            row("Blank Candle %") = runningStock.Value.BlankCandlePercentage
                            row("Day ATR") = Math.Round(runningStock.Value.DayATR, 4)
                            row("Previous Day Open") = runningStock.Value.PreviousDayOpen
                            row("Previous Day Low") = runningStock.Value.PreviousDayLow
                            row("Previous Day High") = runningStock.Value.PreviousDayHigh
                            row("Previous Day Close") = runningStock.Value.PreviousDayClose
                            row("Current Day Close") = runningStock.Value.CurrentDayClose
                            row("Slab") = runningStock.Value.Slab
                            row("Target to Stoploss Multiplier") = Math.Round(multiplier, 2)
                            row("Constriction End Time") = If(constrictionEndTime <> Date.MinValue, constrictionEndTime.ToString("HH:mm:ss"), Nothing)
                            ret.Rows.Add(row)
                            stockCounter += 1
                        End If
                        If stockCounter = My.Settings.NumberOfStockPerDay Then Exit For
                    Next
                End If

                tradingDate = tradingDate.AddDays(1)
            End While
        End Using
        Return ret
    End Function

    Private Function CalculateQuantityFromStoploss(ByVal buyPrice As Decimal, ByVal sellPrice As Decimal, ByVal NetLossOfTrade As Decimal) As Integer
        Dim ret As Integer = 1
        Dim calculator As New Calculator.BrokerageCalculator(_canceller)
        For quantity As Integer = 1 To Integer.MaxValue
            Dim potentialBrokerage As New Calculator.BrokerageAttributes
            calculator.Intraday_Equity(buyPrice, sellPrice, quantity, potentialBrokerage)

            If potentialBrokerage.NetProfitLoss < Math.Abs(NetLossOfTrade) * -1 Then
                Exit For
            Else
                ret = quantity
            End If
        Next
        Return ret
    End Function

    Private Function CalculateTarget(ByVal entryPrice As Decimal, ByVal quantity As Integer, ByVal desiredProfitOfTrade As Decimal) As Decimal
        Dim ret As Decimal = entryPrice
        Dim calculator As New Calculator.BrokerageCalculator(_canceller)
        While True
            Dim potentialBrokerage As New Calculator.BrokerageAttributes
            calculator.Intraday_Equity(entryPrice, ret, quantity, potentialBrokerage)
            If potentialBrokerage.NetProfitLoss > desiredProfitOfTrade Then Exit While
            ret += 0.05
        End While
        Return ret
    End Function

    Private Function IsFractalConstrictionDone(ByVal fractalHighPayload As Dictionary(Of Date, Decimal), ByVal fractalLowPayload As Dictionary(Of Date, Decimal),
                                               ByVal hkPayload As Dictionary(Of Date, Payload), ByVal tradingDate As Date) As Tuple(Of Boolean, Date)
        Dim ret As Tuple(Of Boolean, Date) = Nothing
        If hkPayload IsNot Nothing AndAlso hkPayload.Count > 0 Then
            For Each currentCandle In hkPayload
                If currentCandle.Key.Date = tradingDate.Date Then
                    Dim startTime As Date = tradingDate.Date
                    Dim lastValidTime As Date = Date.MinValue
                    While True
                        Dim upperFractalU As Tuple(Of Date, Date) = GetUpperFractalReverseU(fractalHighPayload, fractalLowPayload, hkPayload, startTime, currentCandle.Key)
                        Dim lowerFractalU As Tuple(Of Date, Date) = GetLowerFractalU(fractalHighPayload, fractalLowPayload, hkPayload, startTime, currentCandle.Key)
                        If upperFractalU IsNot Nothing AndAlso upperFractalU.Item1 <> Date.MinValue AndAlso upperFractalU.Item2 <> Date.MinValue AndAlso
                            lowerFractalU IsNot Nothing AndAlso lowerFractalU.Item1 <> Date.MinValue AndAlso lowerFractalU.Item2 <> Date.MinValue Then
                            Dim chkStartTime As Date = upperFractalU.Item1
                            If lowerFractalU.Item1 < upperFractalU.Item1 Then chkStartTime = lowerFractalU.Item1
                            Dim chkEndTime As Date = upperFractalU.Item2
                            If lowerFractalU.Item2 > upperFractalU.Item2 Then chkEndTime = lowerFractalU.Item2
                            For Each runningPayload In hkPayload
                                If runningPayload.Key > chkStartTime AndAlso runningPayload.Key <= chkEndTime Then
                                    Dim hkCandle As Payload = runningPayload.Value
                                    If hkCandle.Close >= fractalHighPayload(hkCandle.PayloadDate) OrElse
                                        hkCandle.Close <= fractalLowPayload(hkCandle.PayloadDate) Then
                                        startTime = hkCandle.PayloadDate
                                        Exit For
                                    End If
                                End If
                            Next
                            If startTime <= chkStartTime Then
                                'ret = New Tuple(Of Boolean, Date)(True, chkEndTime)
                                'Exit While
                                startTime = chkEndTime
                                lastValidTime = chkEndTime
                            End If
                        Else
                            Exit While
                        End If
                    End While
                    If lastValidTime <> Date.MinValue Then
                        ret = New Tuple(Of Boolean, Date)(True, lastValidTime)
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Private Function GetUpperFractalReverseU(ByVal fractalHighPayload As Dictionary(Of Date, Decimal), ByVal fractalLowPayload As Dictionary(Of Date, Decimal),
                                             ByVal hkPayload As Dictionary(Of Date, Payload), ByVal startTime As Date, ByVal endTime As Date) As Tuple(Of Date, Date)
        Dim ret As Tuple(Of Date, Date) = Nothing
        If fractalHighPayload IsNot Nothing AndAlso fractalHighPayload.Count > 0 AndAlso
            fractalLowPayload IsNot Nothing AndAlso fractalLowPayload.Count > 0 Then
            Dim firstTime As Date = Date.MinValue
            Dim secondTime As Date = Date.MinValue
            Dim firstFractal As Decimal = Decimal.MinValue
            Dim secondFractal As Decimal = Decimal.MinValue
            For Each runningPayload In fractalHighPayload
                If runningPayload.Key.Date = startTime.Date AndAlso runningPayload.Key > startTime AndAlso runningPayload.Key < endTime Then
                    Dim fractal As Decimal = runningPayload.Value
                    If firstFractal = Decimal.MinValue Then
                        firstFractal = fractal
                        firstTime = runningPayload.Key
                    Else
                        If secondFractal = Decimal.MinValue Then
                            If fractal > firstFractal Then
                                secondFractal = fractal
                                secondTime = runningPayload.Key
                            Else
                                firstFractal = fractal
                                firstTime = runningPayload.Key
                            End If
                        Else
                            If fractal < secondFractal Then
                                Dim closeFound As Boolean = False
                                For Each runningCandle In hkPayload
                                    If runningCandle.Key > firstTime AndAlso runningCandle.Key <= runningPayload.Key Then
                                        Dim hkCandle As Payload = runningCandle.Value
                                        If hkCandle.Close >= fractalHighPayload(hkCandle.PayloadDate) OrElse
                                            hkCandle.Close <= fractalLowPayload(hkCandle.PayloadDate) Then
                                            closeFound = True
                                            Exit For
                                        End If
                                    End If
                                Next
                                If Not closeFound Then
                                    ret = New Tuple(Of Date, Date)(firstTime, runningPayload.Key)
                                    Exit For
                                Else
                                    firstFractal = fractal
                                    firstTime = runningPayload.Key
                                    secondFractal = Decimal.MinValue
                                End If
                            ElseIf fractal > secondFractal Then
                                firstFractal = secondFractal
                                firstTime = secondTime
                                secondFractal = Decimal.MinValue
                            Else
                                secondFractal = fractal
                                secondTime = runningPayload.Key
                            End If
                        End If
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Private Function GetLowerFractalU(ByVal fractalHighPayload As Dictionary(Of Date, Decimal), ByVal fractalLowPayload As Dictionary(Of Date, Decimal),
                                      ByVal hkPayload As Dictionary(Of Date, Payload), ByVal startTime As Date, ByVal endTime As Date) As Tuple(Of Date, Date)
        Dim ret As Tuple(Of Date, Date) = Nothing
        If fractalHighPayload IsNot Nothing AndAlso fractalHighPayload.Count > 0 AndAlso
            fractalLowPayload IsNot Nothing AndAlso fractalLowPayload.Count > 0 Then
            Dim firstTime As Date = Date.MinValue
            Dim secondTime As Date = Date.MinValue
            Dim firstFractal As Decimal = Decimal.MinValue
            Dim secondFractal As Decimal = Decimal.MinValue
            For Each runningPayload In fractalLowPayload
                If runningPayload.Key.Date = startTime.Date AndAlso runningPayload.Key > startTime AndAlso runningPayload.Key < endTime Then
                    Dim fractal As Decimal = runningPayload.Value
                    If firstFractal = Decimal.MinValue Then
                        firstFractal = fractal
                        firstTime = runningPayload.Key
                    Else
                        If secondFractal = Decimal.MinValue Then
                            If fractal < firstFractal Then
                                secondFractal = fractal
                                secondTime = runningPayload.Key
                            Else
                                firstFractal = fractal
                                firstTime = runningPayload.Key
                            End If
                        Else
                            If fractal > secondFractal Then
                                Dim closeFound As Boolean = False
                                For Each runningCandle In hkPayload
                                    If runningCandle.Key > firstTime AndAlso runningCandle.Key <= runningPayload.Key Then
                                        Dim hkCandle As Payload = runningCandle.Value
                                        If hkCandle.Close >= fractalHighPayload(hkCandle.PayloadDate) OrElse
                                            hkCandle.Close <= fractalLowPayload(hkCandle.PayloadDate) Then
                                            closeFound = True
                                            Exit For
                                        End If
                                    End If
                                Next
                                If Not closeFound Then
                                    ret = New Tuple(Of Date, Date)(firstTime, runningPayload.Key)
                                    Exit For
                                Else
                                    firstFractal = fractal
                                    firstTime = runningPayload.Key
                                    secondFractal = Decimal.MinValue
                                End If
                            ElseIf fractal < secondFractal Then
                                firstFractal = secondFractal
                                firstTime = secondTime
                                secondFractal = Decimal.MinValue
                            Else
                                secondFractal = fractal
                                secondTime = runningPayload.Key
                            End If
                        End If
                    End If
                End If
            Next
        End If
        Return ret
    End Function
End Class