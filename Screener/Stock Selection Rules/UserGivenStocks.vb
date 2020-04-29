Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Algo2TradeBLL
Imports Utilities.Network

Public Class UserGivenStocks
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
                            Dim lotSize As Integer = _cmn.GetLotSize(_eodTable, currentTradingSymbol, tradingDate)
                            If currentTradingSymbol IsNot Nothing AndAlso lotSize <> Integer.MinValue Then
                                If _eodTable = Common.DataBaseTable.EOD_Currency Then
                                    lotSize = lotSize * 1000
                                ElseIf _eodTable = Common.DataBaseTable.EOD_Commodity Then
                                    If commodityMultiplierMap IsNot Nothing AndAlso commodityMultiplierMap.ContainsKey(runningStock) Then
                                        Dim multiplier As Long = commodityMultiplierMap(runningStock).ToString.Substring(0, commodityMultiplierMap(runningStock).ToString.Length - 1)
                                        lotSize = lotSize * multiplier
                                    End If
                                End If
                                Dim eodPayload As Dictionary(Of Date, Payload) = _cmn.GetRawPayloadForSpecificTradingSymbol(_eodTable, currentTradingSymbol, previousTradingDay.AddDays(-200), previousTradingDay)
                                If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
                                    If eodPayload.LastOrDefault.Value.PayloadDate.Date = previousTradingDay.Date Then
                                        Dim lastDayPayload As Payload = eodPayload.LastOrDefault.Value
                                        Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                                        Indicator.ATR.CalculateATR(14, eodPayload, atrPayload, True)
                                        Dim atr As Decimal = atrPayload(lastDayPayload.PayloadDate)
                                        Dim atrPer As Decimal = (atr / lastDayPayload.Close) * 100
                                        Dim slab As Decimal = CalculateSlab(lastDayPayload.Close, atrPer)

                                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                                        tempStockList.Add(currentTradingSymbol, {lotSize, atrPer, atr, lastDayPayload.Open, lastDayPayload.Low, lastDayPayload.High, lastDayPayload.Close, slab})
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
            If l Is Nothing OrElse l.Item2 Is Nothing Then
                Throw New ApplicationException(String.Format("No response in the additional site's to fetch commodity multiplier and group map: {0}",
                                                             "https://zerodha.com/static/js/brokerage.min.js"))
            End If
            If l IsNot Nothing AndAlso l.Item2 IsNot Nothing Then
                Dim jString As String = l.Item2
                If jString IsNot Nothing Then
                    Dim multiplierMap As String = Utilities.Strings.GetTextBetween("COMMODITY_MULTIPLIER_MAP=", "}", jString)
                    If multiplierMap IsNot Nothing Then
                        multiplierMap = multiplierMap & "}"
                        ret = Utilities.Strings.JsonDeserialize(multiplierMap)
                    End If

                    'Dim groupMap As String = Utilities.Strings.GetTextBetween("COMMODITY_GROUP_MAP=", "}", jString)
                    'If groupMap IsNot Nothing Then
                    '    groupMap = groupMap & "}"
                    '    GlobalVar.GroupMap = Utilities.Strings.JsonDeserialize(groupMap)
                    'End If
                End If
            End If
        End Using
        Return ret
    End Function
End Class
