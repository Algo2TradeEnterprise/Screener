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
                Dim l As Tuple(Of Uri, Object) = Await browser.NonPOSTRequestAsync("https://zerodha.com/static/app.js",
                                                                                     HttpMethod.Get,
                                                                                     Nothing,
                                                                                     True,
                                                                                     Nothing,
                                                                                     False,
                                                                                     Nothing).ConfigureAwait(False)
                If l Is Nothing OrElse l.Item2 Is Nothing Then
                    Throw New ApplicationException(String.Format("No response in the additional site's historical race results landing page: {0}", "https://zerodha.com/static/app.js"))
                End If
                If l IsNot Nothing AndAlso l.Item2 IsNot Nothing Then
                    Dim jString As String = l.Item2
                    If jString IsNot Nothing Then
                        Dim map As String = Utilities.Strings.GetTextBetween("COMMODITY_MULTIPLIER_MAP=", "},", jString)
                        If map IsNot Nothing Then
                            map = map & "}"
                            GlobalVar.retDictionary = Utilities.Strings.JsonDeserialize(map)
                        End If
                    End If
                End If
            End Using
        End Function
#End Region

#Region "Public Fuctioncs"
        Public Function Intraday_Equity(ByVal Buy As Double, ByVal Sell As Double, ByVal Quantity As Integer, ByRef Output As BrokerageAttributes)

            Dim m = Buy
            Dim g = Sell
            Dim v = Quantity
            Output.Buy = Buy
            Output.Sell = Sell
            Output.Quantity = Quantity
            Output.Multiplier = 1
            Output.CTT = 0

            Dim o = If((m * v * 0.0001) > 20, 20, Math.Round((m * v * 0.0001), 2))
            Dim i = If((g * v * 0.0001) > 20, 20, Math.Round((g * v * 0.0001), 2))
            Dim n = Math.Round((o + i), 2)
            Dim a = Math.Round(((m + g) * v), 2)
            Dim r = Convert.ToInt32(g * v * 0.00025)
            Dim s = Math.Round((0.0000325 * a), 2)
            Dim l = 0
            Dim c = Math.Round((0.18 * (n + s)), (2))

            Output.Turnover = a
            Output.Brokerage = n
            Output.STT = r
            Output.Exchange = s
            Output.Clearing = l
            Output.GST = c
            Output.TotalTax = Output.Brokerage + Output.STT + Output.Exchange + Output.Clearing + Output.GST + Output.SEBI

            Return Nothing
        End Function
        Public Function Delivery_Equity(ByVal Buy As Double, ByVal Sell As Double, ByVal Quantity As Integer, ByRef Output As BrokerageAttributes)

            Dim m = Buy
            Dim g = Sell
            Dim v = Quantity
            Output.Buy = Buy
            Output.Sell = Sell
            Output.Quantity = Quantity
            Output.Multiplier = 1
            Output.CTT = 0

            'Dim t = 1.5
            'Dim e = 1.5

            Dim o = Math.Round(((m + g) * v), 2)
            Dim i = 0
            Dim n = Convert.ToInt32(0.001 * o)
            Dim a = Math.Round((0.0000325 * o), 2)
            Dim r = 0
            Dim s = Math.Round((0.18 * (i + a)), (2))

            Output.Turnover = o
            Output.Brokerage = i
            Output.STT = n
            Output.Exchange = a
            Output.Clearing = r
            Output.GST = s
            Output.TotalTax = Output.Brokerage + Output.STT + Output.Exchange + Output.Clearing + Output.GST + Output.SEBI

            Return Nothing
        End Function
        Public Function FO_Futures(ByVal Buy As Double, ByVal Sell As Double, ByVal Quantity As Integer, ByRef Output As BrokerageAttributes)

            Dim m = Buy
            Dim g = Sell
            Dim v = Quantity
            Output.Buy = Buy
            Output.Sell = Sell
            Output.Quantity = Quantity
            Output.Multiplier = 1
            Output.CTT = 0

            'Dim t = 1.5
            'Dim e = 1.5

            Dim o = Math.Round(((m + g) * v), (2))
            Dim i = If((m * v * 0.0001) > 20, 20, Math.Round((m * v * 0.0001), (2)))
            Dim n = If((g * v * 0.0001) > 20, 20, Math.Round((g * v * 0.0001), (2)))
            Dim a = Math.Round((i + n), (2))
            Dim r = Convert.ToInt32(g * v * 0.0001)
            Dim l = Math.Round((0.000019 * o), (2))
            Dim c = 0
            Dim p = Math.Round((0.18 * (a + l)), (2))

            Output.Turnover = o
            Output.Brokerage = a
            Output.STT = r
            Output.Exchange = l
            Output.Clearing = c
            Output.GST = p
            Output.TotalTax = Output.Brokerage + Output.STT + Output.Exchange + Output.Clearing + Output.GST + Output.SEBI

            Return Nothing
        End Function
        Public Function Commodity_MCX(ByVal item As String, ByVal Buy As Double, ByVal Sell As Double, ByVal Quantity As Integer, ByRef Output As BrokerageAttributes) As Task

            Dim m = Buy
            Dim g = Sell
            Dim v = Quantity
            Output.Buy = Buy
            Output.Sell = Sell
            Output.Quantity = Quantity

            If GlobalVar.retDictionary Is Nothing Then
                Dim task = GetCommodityMultiplier()
                task.Wait()
            End If

            Dim t = GlobalVar.retDictionary(item).ToString.Substring(0, GlobalVar.retDictionary(item).ToString.Length - 1)
            Dim e = GlobalVar.retDictionary(item).ToString.Substring(GlobalVar.retDictionary(item).ToString.Length - 1)
            Output.Multiplier = t
            Dim o = Math.Round(((m + g) * t * v), 2)
            Dim i = Nothing
            If m * t * v > 200000.0 Then
                i = 20
            Else
                i = If(m * t * v * 0.0001 > 20, 20, Math.Round((m * t * v * 0.0001), 2))
            End If
            Dim n = Nothing
            If g * t * v > 200000.0 Then
                n = 20
            Else
                n = If(g * t * v * 0.0001 > 20, 20, Math.Round((g * t * v * 0.0001), 2))
            End If
            Dim a = i + n
            Dim r = 0.00
            If e = "a" Then
                r = Math.Round((0.0001 * g * v * t), 2)
            End If
            Dim s = 0.00
            Dim l = 0.00
            Dim c = 0.00
            s = If(e = "a", Math.Round((0.000036 * o), 2), Math.Round((0.0000105 * o), 2))
            l = If(e = "a", Math.Round((0.000026 * o), 2), Math.Round((0.0000005 * o), 2))
            c = 0
            If item = "RBDPMOLEIN" And o >= 100000.0 Then
                Dim p = Convert.ToInt32(Math.Round((o / 100000.0), 2))
                s = p
            End If
            If item = "CASTORSEED" Then
                l = Math.Round((0.000005 * o), 2)
                c = 0
            ElseIf item = "RBDPMOLEIN" Then
                l = Math.Round((0.00001 * o), 2)
                c = 0
            ElseIf item = "PEPPER" Then
                l = Math.Round((0.0000005 * o), 2)
                c = 0
            End If
            Dim d = Math.Round((0.18 * (a + s)), 2)

            Output.Turnover = o
            Output.CTT = r
            Output.Brokerage = a
            Output.Exchange = l
            Output.Clearing = c
            Output.GST = d
            Output.TotalTax = a + r + s + d + Output.SEBI

            Return Nothing
        End Function
        Public Function Currency_Futures(ByVal Buy As Double, ByVal Sell As Double, ByVal Quantity As Integer, ByRef Output As BrokerageAttributes)

            Dim m = Buy
            Dim g = Sell
            Dim v = Quantity * 1000
            Output.Buy = Buy
            Output.Sell = Sell
            Output.Quantity = Quantity * 1000
            Output.Multiplier = 1
            Output.CTT = 0

            'Dim t = 1.5
            'Dim e = 1.5

            Dim t = Math.Round(((m + g) * v), (2))
            Dim e = If((m * v * 0.0001) > 20, 20, Math.Round((m * v * 0.0001), (2)))
            Dim o = If((g * v * 0.0001) > 20, 20, Math.Round((g * v * 0.0001), (2)))
            Dim i = Math.Round((e + o), (2))
            Dim n = Math.Round((0.000011 * t), (2))
            Dim a = Math.Round((0.000009 * t), (2))
            Dim r = 0
            Dim s = Math.Round((0.18 * (i + n)), (2))

            Output.Turnover = t
            Output.Brokerage = i
            Output.Exchange = a
            Output.Clearing = r
            Output.GST = s
            Output.TotalTax = Output.Brokerage + n + Output.GST + Output.SEBI

            Return Nothing
        End Function
#End Region
    End Class
End Namespace