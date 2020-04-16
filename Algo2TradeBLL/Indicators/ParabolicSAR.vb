Imports System.Drawing

Namespace Indicator
    Public Module ParabolicSAR
        Public Sub CalculatePSAR(ByVal minimumAF As Decimal, ByVal maximumAF As Decimal, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPSARPayload As Dictionary(Of Date, Decimal), ByRef outputTrendPayload As Dictionary(Of Date, Color))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim uptrend As Boolean = False
                Dim EP As Decimal = 0
                Dim SAR As Decimal = 0
                Dim AF As Decimal = minimumAF
                Dim nextBarSAR As Decimal = 0
                Dim bar_index As Integer = 0
                For Each runningPayload In inputPayload
                    If bar_index > 0 Then
                        Dim firstTrendBar As Boolean = False
                        SAR = nextBarSAR
                        If bar_index = 1 Then
                            Dim prevSAR As Decimal = 0
                            Dim prevEP As Decimal = 0
                            Dim lowPrev As Decimal = runningPayload.Value.PreviousCandlePayload.Low
                            Dim highPrev As Decimal = runningPayload.Value.PreviousCandlePayload.High
                            Dim closeCur As Decimal = runningPayload.Value.Close
                            Dim closePrev As Decimal = runningPayload.Value.PreviousCandlePayload.Close
                            If closeCur > closePrev Then
                                uptrend = True
                                EP = runningPayload.Value.High
                                prevSAR = lowPrev
                                prevEP = runningPayload.Value.High
                            Else
                                uptrend = False
                                EP = runningPayload.Value.Low
                                prevSAR = highPrev
                                prevEP = runningPayload.Value.Low
                            End If
                            firstTrendBar = True
                            SAR = prevSAR + minimumAF * (prevEP - prevSAR)
                        End If

                        If uptrend Then
                            If SAR > runningPayload.Value.Low Then
                                firstTrendBar = True
                                uptrend = False
                                SAR = Math.Max(EP, runningPayload.Value.High)
                                EP = runningPayload.Value.Low
                                AF = minimumAF
                            End If
                        Else
                            If SAR < runningPayload.Value.High Then
                                firstTrendBar = True
                                uptrend = True
                                SAR = Math.Min(EP, runningPayload.Value.Low)
                                EP = runningPayload.Value.High
                                AF = minimumAF
                            End If
                        End If

                        If Not firstTrendBar Then
                            If uptrend Then
                                If runningPayload.Value.High > EP Then
                                    EP = runningPayload.Value.High
                                    AF = Math.Min(AF + minimumAF, maximumAF)
                                End If
                            Else
                                If runningPayload.Value.Low < EP Then
                                    EP = runningPayload.Value.Low
                                    AF = Math.Min(AF + minimumAF, maximumAF)
                                End If
                            End If
                        End If

                        If uptrend Then
                            SAR = Math.Min(SAR, runningPayload.Value.PreviousCandlePayload.Low)
                            If bar_index > 1 Then
                                SAR = Math.Min(SAR, runningPayload.Value.PreviousCandlePayload.PreviousCandlePayload.Low)
                            End If
                        Else
                            SAR = Math.Max(SAR, runningPayload.Value.PreviousCandlePayload.High)
                            If bar_index > 1 Then
                                SAR = Math.Max(SAR, runningPayload.Value.PreviousCandlePayload.PreviousCandlePayload.High)
                            End If
                        End If

                        nextBarSAR = SAR + AF * (EP - SAR)
                    End If

                    If outputPSARPayload Is Nothing Then outputPSARPayload = New Dictionary(Of Date, Decimal)
                    outputPSARPayload.Add(runningPayload.Key, SAR)
                    If outputTrendPayload Is Nothing Then outputTrendPayload = New Dictionary(Of Date, Color)
                    outputTrendPayload.Add(runningPayload.Key, If(uptrend, Color.Green, Color.Red))

                    bar_index += 1
                Next
            End If
        End Sub
    End Module
End Namespace
