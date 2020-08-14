Imports System.Net.Http
Imports System.Threading
Imports Utilities.Network

Namespace Calculator
    Public Class BrokerageCalculator

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
        Public Sub New(calceller As CancellationTokenSource)
            _cts = calceller
        End Sub

#Region "BrowseHTTP"
        Private Async Function GetCommodityMultiplier() As Task
            Dim proxyToBeUsed As HttpProxy = Nothing
            Dim ret As List(Of String) = Nothing

            Using browser As New HttpBrowser(proxyToBeUsed, Net.DecompressionMethods.GZip, New TimeSpan(0, 1, 0), _cts)
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
                            If multiplierMap.EndsWith(",") Then
                                multiplierMap = multiplierMap.Substring(0, multiplierMap.Count - 1)
                            End If
                            multiplierMap = multiplierMap & "}"
                            GlobalVar.MultiplierMap = Utilities.Strings.JsonDeserialize(multiplierMap)
                        End If

                        Dim groupMap As String = Utilities.Strings.GetTextBetween("COMMODITY_GROUP_MAP=", "}", jString)
                        If groupMap IsNot Nothing Then
                            If groupMap.EndsWith(",") Then
                                groupMap = groupMap.Substring(0, groupMap.Count - 1)
                            End If
                            groupMap = groupMap & "}"
                            GlobalVar.GroupMap = Utilities.Strings.JsonDeserialize(groupMap)
                        End If
                    End If
                End If
            End Using
        End Function
#End Region

#Region "Public Fuctioncs"
        Public Sub Intraday_Equity(ByVal Buy As Double, ByVal Sell As Double, ByVal Quantity As Integer, ByRef Output As BrokerageAttributes)
            Dim bp As Decimal = Buy
            Dim sp As Decimal = Sell
            Dim qty As Integer = Quantity

            Dim turnover As Decimal = Math.Round((bp + sp) * qty, 2)
            Dim brokerage_buy As Decimal = If((bp * qty * 0.0003) > 20, 20, Math.Round(bp * qty * 0.0003, 2))
            Dim brokerage_sell As Decimal = If((sp * qty * 0.0003) > 20, 20, Math.Round(sp * qty * 0.0003, 2))
            Dim brokerage As Decimal = Math.Round(brokerage_buy + brokerage_sell, 2)
            Dim stt_total As Decimal = Math.Round(sp * qty * 0.00025, 2)
            Dim etc As Decimal = Math.Round(0.0000325 * turnover, 2)
            Dim cc As Decimal = 0
            Dim stax As Decimal = Math.Round(0.18 * (brokerage + etc), 2)
            Dim sebi_charges As Decimal = Math.Round(turnover * 0.0000005, 2)
            Dim stamp_charges As Decimal = Math.Round(bp * qty * 0.00003, 2)
            Dim total_tax As Decimal = Math.Round(brokerage + stt_total + etc + cc + stax + sebi_charges + stamp_charges, 2)
            Dim breakeven As Decimal = Math.Round(total_tax / qty, 2)
            Dim net_profit As Decimal = Math.Round(((sp - bp) * qty) - total_tax, 2)

            Output = New BrokerageAttributes With {
                        .Buy = bp,
                        .Sell = sp,
                        .Quantity = qty,
                        .Turnover = turnover,
                        .Brokerage = brokerage,
                        .STT = stt_total,
                        .ExchangeFees = etc,
                        .Clearing = cc,
                        .GST = stax,
                        .SEBI = sebi_charges,
                        .StampDuty = stamp_charges,
                        .TotalTax = total_tax,
                        .BreakevenPoints = breakeven,
                        .NetProfitLoss = net_profit
                    }
        End Sub
        Public Sub Delivery_Equity(ByVal Buy As Double, ByVal Sell As Double, ByVal Quantity As Integer, ByRef Output As BrokerageAttributes)
            Dim bp As Decimal = Buy
            Dim sp As Decimal = Sell
            Dim qty As Integer = Quantity

            Dim turnover As Decimal = Math.Round((bp + sp) * qty, 2)
            Dim brokerage As Decimal = 0
            Dim stt_total As Decimal = Math.Round(turnover * 0.001, 2)
            Dim etc As Decimal = Math.Round(0.0000325 * turnover, 2)
            Dim cc As Decimal = 0
            Dim stax As Decimal = Math.Round(0.18 * (brokerage + etc), 2)
            Dim sebi_charges As Decimal = Math.Round(turnover * 0.0000005, 2)
            Dim stamp_charges As Decimal = Math.Round(bp * qty * 0.00015, 2)
            Dim total_tax As Decimal = Math.Round(brokerage + stt_total + etc + cc + stax + sebi_charges + stamp_charges, 2)
            Dim breakeven As Decimal = Math.Round(total_tax / qty, 2)
            Dim net_profit As Decimal = Math.Round(((sp - bp) * qty) - total_tax, 2)

            Output = New BrokerageAttributes With {
                        .Buy = bp,
                        .Sell = sp,
                        .Quantity = qty,
                        .Turnover = turnover,
                        .Brokerage = brokerage,
                        .STT = stt_total,
                        .ExchangeFees = etc,
                        .Clearing = 0,
                        .GST = stax,
                        .SEBI = sebi_charges,
                        .StampDuty = stamp_charges,
                        .TotalTax = total_tax,
                        .BreakevenPoints = breakeven,
                        .NetProfitLoss = net_profit
                    }
        End Sub
        Public Sub FO_Futures(ByVal Buy As Double, ByVal Sell As Double, ByVal Quantity As Integer, ByRef Output As BrokerageAttributes)
            Dim bp As Decimal = Buy
            Dim sp As Decimal = Sell
            Dim qty As Integer = Quantity

            Dim turnover As Decimal = Math.Round((bp + sp) * qty, 2)
            Dim brokerage_buy As Decimal = If((bp * qty * 0.0003) > 20, 20, Math.Round(bp * qty * 0.0003, 2))
            Dim brokerage_sell As Decimal = If((sp * qty * 0.0003) > 20, 20, Math.Round(sp * qty * 0.0003, 2))
            Dim brokerage As Decimal = Math.Round(brokerage_buy + brokerage_sell, 2)
            Dim stt_total As Decimal = Math.Round(sp * qty * 0.0001, 2)
            Dim etc As Decimal = Math.Round(0.000019 * turnover, 2)
            Dim stax As Decimal = Math.Round(0.18 * (brokerage + etc), 2)
            Dim sebi_charges As Decimal = Math.Round(turnover * 0.0000005, 2)
            Dim stamp_charges As Decimal = Math.Round(bp * qty * 0.00002, 2)
            Dim total_tax As Decimal = Math.Round(brokerage + stt_total + etc + stax + sebi_charges + stamp_charges, 2)
            Dim breakeven As Decimal = Math.Round(total_tax / qty, 2)
            Dim net_profit As Decimal = Math.Round(((sp - bp) * qty) - total_tax, 2)

            Output = New BrokerageAttributes With {
                        .Buy = bp,
                        .Sell = sp,
                        .Quantity = qty,
                        .Turnover = turnover,
                        .Brokerage = brokerage,
                        .STT = stt_total,
                        .ExchangeFees = etc,
                        .GST = stax,
                        .SEBI = sebi_charges,
                        .StampDuty = stamp_charges,
                        .TotalTax = total_tax,
                        .BreakevenPoints = breakeven,
                        .NetProfitLoss = net_profit
                    }
        End Sub
        Public Sub FO_Options(ByVal Buy As Double, ByVal Sell As Double, ByVal Quantity As Integer, ByRef Output As BrokerageAttributes)
            Dim bp As Decimal = Buy
            Dim sp As Decimal = Sell
            Dim qty As Integer = Quantity

            Dim turnover As Decimal = Math.Round((bp + sp) * qty, 2)
            Dim brokerage As Decimal = 40
            Dim stt_total As Decimal = Math.Round(sp * qty * 0.0005, 2)
            Dim etc As Decimal = Math.Round(0.0005 * turnover, 2)
            Dim stax As Decimal = Math.Round(0.18 * (brokerage + etc), 2)
            Dim sebi_charges As Decimal = Math.Round(turnover * 0.0000005, 2)
            Dim stamp_charges As Decimal = Math.Round(bp * qty * 0.00003, 2)
            Dim total_tax As Decimal = Math.Round(brokerage + stt_total + etc + stax + sebi_charges + stamp_charges, 2)
            Dim breakeven As Decimal = Math.Round(total_tax / qty, 2)
            Dim net_profit As Decimal = Math.Round(((sp - bp) * qty) - total_tax, 2)

            Output = New BrokerageAttributes With {
                        .Buy = bp,
                        .Sell = sp,
                        .Quantity = qty,
                        .Turnover = turnover,
                        .Brokerage = brokerage,
                        .STT = stt_total,
                        .ExchangeFees = etc,
                        .GST = stax,
                        .SEBI = sebi_charges,
                        .StampDuty = stamp_charges,
                        .TotalTax = total_tax,
                        .BreakevenPoints = breakeven,
                        .NetProfitLoss = net_profit
                    }
        End Sub
        Public Sub Commodity_MCX(ByVal item As String, ByVal Buy As Double, ByVal Sell As Double, ByVal Quantity As Integer, ByRef Output As BrokerageAttributes)
            If GlobalVar.MultiplierMap Is Nothing OrElse GlobalVar.GroupMap Is Nothing Then
                Dim task = GetCommodityMultiplier()
                task.Wait()
            End If

            Dim stockName As String = item
            Dim bp As Decimal = Buy
            Dim sp As Decimal = Sell
            Dim qty As Integer = Quantity

            Dim commodity_value As Long = GlobalVar.MultiplierMap(item).ToString.Substring(0, GlobalVar.MultiplierMap(item).ToString.Length - 1)
            Dim commodity_cat As String = GlobalVar.MultiplierMap(item).ToString.Substring(GlobalVar.MultiplierMap(item).ToString.Length - 1)
            Dim commodity_group As String = GlobalVar.GroupMap(item).ToString.Substring(GlobalVar.GroupMap(item).ToString.Length - 1)
            Dim turnover As Decimal = Math.Round((bp + sp) * commodity_value * qty, 2)
            Dim brokerage_buy As Decimal = 0
            If (bp * commodity_value * qty) > 200000 Then
                brokerage_buy = 20
            Else
                brokerage_buy = If((bp * commodity_value * qty * 0.0003) > 20, 20, Math.Round(bp * commodity_value * qty * 0.0003, 2))
            End If
            Dim brokerage_sell As Decimal = 0
            If (sp * commodity_value * qty) > 200000 Then
                brokerage_sell = 20
            Else
                brokerage_sell = If((sp * commodity_value * qty * 0.0003) > 20, 20, Math.Round(sp * commodity_value * qty * 0.0003, 2))
            End If
            Dim brokerage As Decimal = brokerage_buy + brokerage_sell
            Dim ctt As Decimal = 0
            If commodity_cat = "a" Then
                ctt = Math.Round(0.0001 * sp * qty * commodity_value, 2)
            End If
            Dim etc As Decimal = 0
            Dim cc As Decimal = 0
            etc = If(commodity_cat = "a", Math.Round(0.000026 * turnover, 2), Math.Round(0.0000005 * turnover, 2))
            If stockName = "RBDPMOLEIN" Then
                If turnover >= 100000 Then
                    Dim rbd_multiplier As Integer = CInt(turnover / 100000)
                    etc = Math.Round(rbd_multiplier, 2)
                End If
            End If
            If stockName = "CASTORSEED" Then
                etc = Math.Round(0.000005 * turnover, 2)
            ElseIf stockName = "RBDPMOLEIN" Then
                etc = Math.Round(0.00001 * turnover, 2)
            ElseIf stockName = "PEPPER" Then
                etc = Math.Round(0.0000005 * turnover, 2)
            ElseIf stockName = "KAPAS" Then
                etc = Math.Round(0.000005 * turnover, 2)
            End If
            Dim stax As Decimal = Math.Round(0.18 * (brokerage + etc), 2)
            Dim sebi_charges As Decimal = Math.Round(turnover * 0.0000005, 2)
            If commodity_group = "a" Then
                sebi_charges = Math.Round(turnover * 0.0000001, 2)
            End If
            Dim stamp_charges As Decimal = Math.Round(bp * qty * commodity_value * 0.00002, 2)
            Dim total_tax As Decimal = Math.Round(brokerage + ctt + etc + stax + sebi_charges + stamp_charges, 2)
            Dim breakeven As Decimal = Math.Round(total_tax / (qty * commodity_value), 2)
            Dim net_profit As Decimal = Math.Round(((sp - bp) * qty * commodity_value) - total_tax, 2)

            Output = New BrokerageAttributes With {
                        .Buy = bp,
                        .Sell = sp,
                        .Quantity = qty,
                        .Turnover = turnover,
                        .CTT = ctt,
                        .Brokerage = brokerage,
                        .ExchangeFees = etc,
                        .Clearing = cc,
                        .GST = stax,
                        .SEBI = sebi_charges,
                        .StampDuty = stamp_charges,
                        .TotalTax = total_tax,
                        .BreakevenPoints = breakeven,
                        .NetProfitLoss = net_profit
                    }
        End Sub
        Public Sub Currency_Futures(ByVal Buy As Double, ByVal Sell As Double, ByVal Quantity As Integer, ByRef Output As BrokerageAttributes)
            Dim bp As Decimal = Buy
            Dim sp As Decimal = Sell
            Dim qty As Integer = Quantity

            Dim turnover As Decimal = Math.Round((bp + sp) * qty * 1000, 2)
            Dim brokerage_buy As Decimal = If((bp * qty * 1000 * 0.0003) > 20, 20, Math.Round(bp * qty * 1000 * 0.0003, 2))
            Dim brokerage_sell As Decimal = If((sp * qty * 1000 * 0.0003) > 20, 20, Math.Round(sp * qty * 1000 * 0.0003, 2))
            Dim brokerage As Decimal = Math.Round(brokerage_buy + brokerage_sell, 2)
            Dim etc As Decimal = Math.Round(0.000009 * turnover, 2)
            Dim cc As Decimal = 0
            Dim total_trans_charge As Decimal = etc + cc
            Dim stax As Decimal = Math.Round(0.18 * (brokerage + total_trans_charge), 2)
            Dim sebi_charges As Decimal = Math.Round(turnover * 0.0000005, 2)
            Dim stamp_charges As Decimal = Math.Round(bp * qty * 1000 * 0.000001, 2)
            Dim total_tax As Decimal = Math.Round(brokerage + total_trans_charge + stax + sebi_charges + stamp_charges, 2)
            Dim breakeven As Decimal = Math.Round(total_tax / (qty * 1000), 4)
            Dim pips As Decimal = Math.Ceiling(breakeven / 0.0025)
            Dim net_profit As Decimal = Math.Round(((sp - bp) * qty * 1000) - total_tax, 2)

            Output = New BrokerageAttributes With {
                        .Buy = bp,
                        .Sell = sp,
                        .Quantity = qty,
                        .Turnover = turnover,
                        .Brokerage = brokerage,
                        .ExchangeFees = etc,
                        .Clearing = cc,
                        .GST = stax,
                        .SEBI = sebi_charges,
                        .StampDuty = stamp_charges,
                        .TotalTax = total_tax,
                        .BreakevenPoints = breakeven,
                        .NetProfitLoss = net_profit
                    }
        End Sub
#End Region
    End Class
End Namespace