Namespace Indicator
    Public Module SwingHighLow
        Public Sub CalculateSwingHighLow(ByVal inputPayload As Dictionary(Of Date, Payload), ByVal strict As Boolean, ByRef outputHighPayload As Dictionary(Of Date, Decimal), ByRef outputLowPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                For Each runningPayload In inputPayload.Keys
                    Dim swingHigh As Decimal = 0
                    Dim swingLow As Decimal = 0
                    If strict Then
                        If inputPayload(runningPayload).PreviousCandlePayload Is Nothing Then
                            swingHigh = inputPayload(runningPayload).High
                            swingLow = inputPayload(runningPayload).Low
                        ElseIf inputPayload(runningPayload).PreviousCandlePayload IsNot Nothing AndAlso inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload Is Nothing Then
                            If inputPayload(runningPayload).PreviousCandlePayload.High > inputPayload(runningPayload).High AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.Volume <> 0 Then
                                swingHigh = inputPayload(runningPayload).PreviousCandlePayload.High
                            Else
                                swingHigh = outputHighPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate)
                            End If
                            If inputPayload(runningPayload).PreviousCandlePayload.Low < inputPayload(runningPayload).Low AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.Volume <> 0 Then
                                swingLow = inputPayload(runningPayload).PreviousCandlePayload.Low
                            Else
                                swingLow = outputLowPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate)
                            End If
                        Else
                            If inputPayload(runningPayload).PreviousCandlePayload.High > inputPayload(runningPayload).High AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.High > inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload.High AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.Volume <> 0 Then
                                swingHigh = inputPayload(runningPayload).PreviousCandlePayload.High
                            Else
                                swingHigh = outputHighPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate)
                            End If
                            If inputPayload(runningPayload).PreviousCandlePayload.Low < inputPayload(runningPayload).Low AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.Low < inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload.Low AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.Volume <> 0 Then
                                swingLow = inputPayload(runningPayload).PreviousCandlePayload.Low
                            Else
                                swingLow = outputLowPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate)
                            End If
                        End If
                    Else
                        If inputPayload(runningPayload).PreviousCandlePayload Is Nothing Then
                            swingHigh = inputPayload(runningPayload).High
                            swingLow = inputPayload(runningPayload).Low
                        ElseIf inputPayload(runningPayload).PreviousCandlePayload IsNot Nothing AndAlso inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload Is Nothing Then
                            If inputPayload(runningPayload).PreviousCandlePayload.High >= inputPayload(runningPayload).High AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.Volume <> 0 Then
                                swingHigh = inputPayload(runningPayload).PreviousCandlePayload.High
                            Else
                                swingHigh = outputHighPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate)
                            End If
                            If inputPayload(runningPayload).PreviousCandlePayload.Low <= inputPayload(runningPayload).Low AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.Volume <> 0 Then
                                swingLow = inputPayload(runningPayload).PreviousCandlePayload.Low
                            Else
                                swingLow = outputLowPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate)
                            End If
                        Else
                            If inputPayload(runningPayload).PreviousCandlePayload.High >= inputPayload(runningPayload).High AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.High >= inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload.High AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.Volume <> 0 Then
                                swingHigh = inputPayload(runningPayload).PreviousCandlePayload.High
                            Else
                                swingHigh = outputHighPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate)
                            End If
                            If inputPayload(runningPayload).PreviousCandlePayload.Low <= inputPayload(runningPayload).Low AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.Low <= inputPayload(runningPayload).PreviousCandlePayload.PreviousCandlePayload.Low AndAlso
                                inputPayload(runningPayload).PreviousCandlePayload.Volume <> 0 Then
                                swingLow = inputPayload(runningPayload).PreviousCandlePayload.Low
                            Else
                                swingLow = outputLowPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate)
                            End If
                        End If
                    End If

                    If outputHighPayload Is Nothing Then outputHighPayload = New Dictionary(Of Date, Decimal)
                    outputHighPayload.Add(runningPayload, swingHigh)
                    If outputLowPayload Is Nothing Then outputLowPayload = New Dictionary(Of Date, Decimal)
                    outputLowPayload.Add(runningPayload, swingLow)
                Next
            End If
        End Sub
    End Module
End Namespace
