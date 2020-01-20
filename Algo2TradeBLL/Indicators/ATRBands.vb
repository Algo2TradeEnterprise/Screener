Namespace Indicator
    Public Module ATRBands
        Public Sub CalculateATRBands(ByVal ATRShift As Integer, ByVal ATRPeriod As Integer, ByVal shiftField As Payload.PayloadFields, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputHighPayload As Dictionary(Of Date, Decimal), ByRef outputLowPayload As Dictionary(Of Date, Decimal), Optional neglectValidation As Boolean = False)
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If Not neglectValidation AndAlso inputPayload.Count < 100 Then
                    Throw New ApplicationException("Can't Calculate ATR")
                End If
                Dim ATROutputPayload As Dictionary(Of Date, Decimal) = Nothing
                ATR.CalculateATR(ATRPeriod, inputPayload, ATROutputPayload, neglectValidation)

                For Each runningPayload In inputPayload.Keys
                    Dim highBand As Decimal = Nothing
                    Dim lowBand As Decimal = Nothing
                    Select Case shiftField
                        Case Payload.PayloadFields.Close
                            highBand = inputPayload(runningPayload).Close + ATRShift * ATROutputPayload(runningPayload)
                            lowBand = inputPayload(runningPayload).Close - ATRShift * ATROutputPayload(runningPayload)
                        Case Payload.PayloadFields.High
                            highBand = inputPayload(runningPayload).High + ATRShift * ATROutputPayload(runningPayload)
                            lowBand = inputPayload(runningPayload).High - ATRShift * ATROutputPayload(runningPayload)
                        Case Payload.PayloadFields.Low
                            highBand = inputPayload(runningPayload).Low + ATRShift * ATROutputPayload(runningPayload)
                            lowBand = inputPayload(runningPayload).Low - ATRShift * ATROutputPayload(runningPayload)
                        Case Payload.PayloadFields.Open
                            highBand = inputPayload(runningPayload).Open + ATRShift * ATROutputPayload(runningPayload)
                            lowBand = inputPayload(runningPayload).Open - ATRShift * ATROutputPayload(runningPayload)
                        Case Else
                            Throw New NotImplementedException
                    End Select
                    If outputHighPayload Is Nothing Then outputHighPayload = New Dictionary(Of Date, Decimal)
                    outputHighPayload.Add(runningPayload, highBand)
                    If outputLowPayload Is Nothing Then outputLowPayload = New Dictionary(Of Date, Decimal)
                    outputLowPayload.Add(runningPayload, lowBand)
                Next
            End If
        End Sub
    End Module
End Namespace