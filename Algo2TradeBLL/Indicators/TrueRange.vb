Namespace Indicator
    Public Module TrueRange
        Public Sub CalculateTrueRange(ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim firstPayload As Boolean = True
                Dim HighLow As Double = Nothing
                Dim HighClose As Double = Nothing
                Dim LowClose As Double = Nothing
                Dim PreviousClose As Double = Nothing
                Dim TR As Double = Nothing
                For Each runningInputPayload In inputPayload
                    HighLow = runningInputPayload.Value.High - runningInputPayload.Value.Low
                    If firstPayload = True Then
                        TR = HighLow
                        firstPayload = False
                    Else
                        HighClose = Math.Abs(runningInputPayload.Value.High - runningInputPayload.Value.PreviousCandlePayload.Close)
                        LowClose = Math.Abs(runningInputPayload.Value.Low - runningInputPayload.Value.PreviousCandlePayload.Close)
                        TR = Math.Max(HighLow, Math.Max(HighClose, LowClose))
                    End If
                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Decimal)
                    outputPayload.Add(runningInputPayload.Value.PayloadDate, Math.Round(TR, 4))
                Next
            End If
        End Sub
    End Module
End Namespace
