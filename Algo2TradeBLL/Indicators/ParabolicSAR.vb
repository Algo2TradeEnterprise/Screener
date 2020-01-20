Imports System.Drawing

Namespace Indicator
    Public Module ParabolicSAR
        Public Sub CalculatePSAR(ByVal minimumAF As Decimal, ByVal maximumAF As Decimal, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPSARPayload As Dictionary(Of Date, Decimal), ByRef outputTrendPayload As Dictionary(Of Date, Color))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim previousTrend As Decimal = 0
                Dim previousCalculatedSAR As Decimal = 0
                Dim previousTentativeSAR As Decimal = 0
                Dim previousPSAR As Decimal = 0
                Dim previousEP As Decimal = 0
                Dim previousAF As Decimal = 0
                For Each runningPayload In inputPayload.Keys
                    Dim trend As Decimal = 0
                    Dim CalculatedSAR As Decimal = 0
                    Dim TentativeSAR As Decimal = 0
                    Dim PSAR As Decimal = 0
                    Dim EP As Decimal = 0
                    Dim AF As Decimal = 0
                    If inputPayload(runningPayload).PreviousCandlePayload IsNot Nothing AndAlso
                        inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload IsNot Nothing Then
                        CalculatedSAR = Math.Round(previousPSAR + previousAF * (previousEP - previousPSAR), 2)
                        If previousTrend < 0 Then
                            TentativeSAR = Math.Max(CalculatedSAR, Math.Max(inputPayload(runningPayload).PreviousCandlePayload.High, inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload.High))
                        Else
                            TentativeSAR = Math.Min(CalculatedSAR, Math.Min(inputPayload(runningPayload).PreviousCandlePayload.Low, inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload.Low))
                        End If
                        If previousTrend < 0 Then
                            If TentativeSAR < inputPayload(runningPayload).High Then
                                trend = 1
                            Else
                                trend = previousTrend - 1
                            End If
                        Else
                            If TentativeSAR > inputPayload(runningPayload).Low Then
                                trend = -1
                            Else
                                trend = previousTrend + 1
                            End If
                        End If
                        If trend = -1 Then
                            PSAR = Math.Max(previousEP, inputPayload(runningPayload).High)
                        ElseIf trend = 1 Then
                            PSAR = Math.Min(previousEP, inputPayload(runningPayload).Low)
                        Else
                            PSAR = TentativeSAR
                        End If
                        If trend < 0 Then
                            If trend = -1 Then
                                EP = inputPayload(runningPayload).Low
                            Else
                                EP = Math.Min(inputPayload(runningPayload).Low, previousEP)
                            End If
                        Else
                            If trend = 1 Then
                                EP = inputPayload(runningPayload).High
                            Else
                                EP = Math.Max(inputPayload(runningPayload).High, previousEP)
                            End If
                        End If
                        If Math.Abs(trend) = 1 Then
                            AF = minimumAF
                        Else
                            If EP = previousEP Then
                                AF = previousAF
                            Else
                                AF = Math.Min(maximumAF, previousAF + minimumAF)
                            End If
                        End If
                    ElseIf inputPayload(runningPayload).PreviousCandlePayload IsNot Nothing Then
                        trend = -1
                        If trend < 0 Then
                            PSAR = inputPayload(runningPayload).PreviousCandlePayload.High
                        Else
                            PSAR = inputPayload(runningPayload).PreviousCandlePayload.Low
                        End If
                        If trend < 0 Then
                            If trend = -1 Then
                                EP = inputPayload(runningPayload).Low
                            Else
                                EP = Math.Min(inputPayload(runningPayload).Low, previousEP)
                            End If
                        Else
                            If trend = 1 Then
                                EP = inputPayload(runningPayload).High
                            Else
                                EP = Math.Max(inputPayload(runningPayload).High, previousEP)
                            End If
                        End If
                        If Math.Abs(trend) = 1 Then
                            AF = minimumAF
                        Else
                            If EP = previousEP Then
                                AF = previousAF
                            Else
                                AF = Math.Min(maximumAF, previousAF + minimumAF)
                            End If
                        End If
                    End If

                    previousTrend = trend
                    previousCalculatedSAR = CalculatedSAR
                    previousTentativeSAR = TentativeSAR
                    previousPSAR = PSAR
                    previousEP = EP
                    previousAF = AF

                    If outputPSARPayload Is Nothing Then outputPSARPayload = New Dictionary(Of Date, Decimal)
                    outputPSARPayload.Add(runningPayload, PSAR)
                    If outputTrendPayload Is Nothing Then outputTrendPayload = New Dictionary(Of Date, Color)
                    outputTrendPayload.Add(runningPayload, If(trend < 0, Color.Red, If(trend > 0, Color.Green, Color.White)))
                Next
            End If
        End Sub
    End Module
End Namespace
