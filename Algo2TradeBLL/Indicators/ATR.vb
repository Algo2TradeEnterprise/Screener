Namespace Indicator
    Public Module ATR
        Public Sub CalculateATR(ByVal ATRPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Decimal), Optional neglectValidation As Boolean = False)
            'Using WILDER Formula
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If Not neglectValidation AndAlso inputPayload.Count < 100 Then
                    Throw New ApplicationException("Can't Calculate ATR")
                End If
                Dim firstPayload As Boolean = True
                Dim HighLow As Double = Nothing
                Dim HighClose As Double = Nothing
                Dim LowClose As Double = Nothing
                Dim PreviousClose As Double = Nothing
                Dim TR As Double = Nothing
                Dim SumTR As Double = 0.00
                Dim AvgTR As Double = 0.00
                Dim counter As Integer = 0
                outputPayload = New Dictionary(Of Date, Decimal)
                For Each runningInputPayload In inputPayload
                    counter += 1
                    HighLow = runningInputPayload.Value.High - runningInputPayload.Value.Low
                    If firstPayload = True Then
                        TR = HighLow
                        firstPayload = False
                    Else
                        HighClose = Math.Abs(runningInputPayload.Value.High - runningInputPayload.Value.PreviousCandlePayload.Close)
                        LowClose = Math.Abs(runningInputPayload.Value.Low - runningInputPayload.Value.PreviousCandlePayload.Close)
                        TR = Math.Max(HighLow, Math.Max(HighClose, LowClose))
                    End If
                    SumTR = SumTR + TR
                    If counter = ATRPeriod Then
                        AvgTR = SumTR / ATRPeriod
                        outputPayload.Add(runningInputPayload.Value.PayloadDate, AvgTR)
                    ElseIf counter > ATRPeriod Then
                        AvgTR = (outputPayload(runningInputPayload.Value.PreviousCandlePayload.PayloadDate) * (ATRPeriod - 1) + TR) / ATRPeriod
                        outputPayload.Add(runningInputPayload.Value.PayloadDate, AvgTR)
                    Else
                        AvgTR = SumTR / counter
                        outputPayload.Add(runningInputPayload.Value.PayloadDate, AvgTR)
                    End If
                Next
            End If
        End Sub
    End Module
End Namespace
