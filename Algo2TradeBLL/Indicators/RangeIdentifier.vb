Namespace Indicator
    Public Module RangeIdentifier
        Public Sub CalculateRangeIdentifier(ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputHighPayload As Dictionary(Of Date, Decimal), ByRef outputLowPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim tempHigh As Decimal = 0
                Dim tempLow As Decimal = 0
                For Each runningPayload In inputPayload.Keys
                    Dim highRange As Decimal = 0
                    Dim lowRange As Decimal = 0
                    If inputPayload(runningPayload).PreviousCandlePayload IsNot Nothing Then
                        If inputPayload(runningPayload).Close > tempHigh OrElse inputPayload(runningPayload).Close < tempLow Then
                            tempHigh = inputPayload(runningPayload).High
                            tempLow = inputPayload(runningPayload).Low
                        Else
                            If inputPayload(runningPayload).Close < tempHigh AndAlso inputPayload(runningPayload).Close > tempLow Then
                                highRange = tempHigh
                                lowRange = tempLow
                            End If
                        End If
                    End If
                    If outputHighPayload Is Nothing Then outputHighPayload = New Dictionary(Of Date, Decimal)
                    outputHighPayload.Add(runningPayload, highRange)
                    If outputLowPayload Is Nothing Then outputLowPayload = New Dictionary(Of Date, Decimal)
                    outputLowPayload.Add(runningPayload, lowRange)
                Next
            End If
        End Sub
    End Module
End Namespace