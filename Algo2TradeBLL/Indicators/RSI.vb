Namespace Indicator
    Public Module RSI
        Public Sub CalculateRSI(ByVal RSIPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputRSI As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If inputPayload.Count < 100 Then
                    Throw New ApplicationException("Can't Calculate RSI")
                End If

                Dim counter As Integer = 0
                Dim sumGain As Decimal = 0
                Dim sumLoss As Decimal = 0
                Dim avgGain As Decimal = 0
                Dim avgLoss As Decimal = 0
                Dim preGain As Decimal = 0
                Dim preLoss As Decimal = 0
                Dim rs As Decimal = 0
                Dim closeChange As Decimal = 0
                outputRSI = New Dictionary(Of Date, Decimal)
                For Each runningInputPayload In inputPayload

                    counter += 1

                    If counter > 1 Then
                        closeChange = runningInputPayload.Value.Close - runningInputPayload.Value.PreviousCandlePayload.Close
                        If closeChange > 0 Then
                            sumGain += closeChange
                        ElseIf closeChange < 0 Then
                            sumLoss += Math.Abs(closeChange)
                        End If
                    End If

                    If counter = RSIPeriod Then
                        avgGain = sumGain / RSIPeriod
                        avgLoss = sumLoss / RSIPeriod
                    ElseIf counter > RSIPeriod Then
                        If closeChange > 0 Then
                            avgGain = (preGain * (RSIPeriod - 1) + closeChange) / RSIPeriod
                            avgLoss = (preLoss * (RSIPeriod - 1) + 0) / RSIPeriod
                        ElseIf closeChange < 0 Then
                            avgGain = (preGain * (RSIPeriod - 1) + 0) / RSIPeriod
                            avgLoss = (preLoss * (RSIPeriod - 1) + Math.Abs(closeChange)) / RSIPeriod
                        Else
                            avgGain = (preGain * (RSIPeriod - 1) + 0) / RSIPeriod
                            avgLoss = (preLoss * (RSIPeriod - 1) + 0) / RSIPeriod
                        End If
                    Else
                        avgGain = sumGain / counter
                        avgLoss = sumLoss / counter
                    End If
                    rs = If(Math.Round(avgLoss, 8) = 0, 100, (100 - (100 / (1 + (avgGain / avgLoss)))))
                    outputRSI.Add(runningInputPayload.Value.PayloadDate, Math.Round(rs, 4))
                    preGain = avgGain
                    preLoss = avgLoss
                Next
            End If
        End Sub
    End Module
End Namespace