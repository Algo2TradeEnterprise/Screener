Namespace Indicator
    Public Module StandardDeviation

        Public Sub CalculateSD(ByVal period As Integer, ByVal field As Payload.PayloadFields, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim sd As Decimal = 0
                For Each runningInputPayload In inputPayload

                    'If it is less than IndicatorPeriod, we will need to take SMA of all previous prices, hence the call to GetSubPayload
                    Dim previousNInputFieldPayload As List(Of KeyValuePair(Of DateTime, Payload)) = Common.GetSubPayload(inputPayload, runningInputPayload.Key, period, True)
                    If previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count > 0 Then
                        Dim sdPayload As Dictionary(Of Date, Decimal) = Nothing
                        For Each runningPayload In previousNInputFieldPayload
                            Dim fieldValue As Decimal = Decimal.MinValue
                            Select Case field
                                Case Payload.PayloadFields.Close
                                    fieldValue = runningPayload.Value.Close
                                Case Payload.PayloadFields.High
                                    fieldValue = runningPayload.Value.High
                                Case Payload.PayloadFields.Low
                                    fieldValue = runningPayload.Value.Low
                                Case Payload.PayloadFields.Open
                                    fieldValue = runningPayload.Value.Open
                                Case Payload.PayloadFields.Volume
                                    fieldValue = runningPayload.Value.Volume
                                Case Payload.PayloadFields.Additional_Field
                                    fieldValue = runningPayload.Value.Additional_Field
                                Case Else
                                    Throw New NotImplementedException
                            End Select
                            If sdPayload Is Nothing Then sdPayload = New Dictionary(Of Date, Decimal)
                            sdPayload.Add(runningPayload.Key, fieldValue)
                        Next
                        sd = Common.CalculateStandardDeviationPA(sdPayload)
                    End If
                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Decimal)
                    outputPayload.Add(runningInputPayload.Key, sd)
                Next
            End If
        End Sub

    End Module
End Namespace