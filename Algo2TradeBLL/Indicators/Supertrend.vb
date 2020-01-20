Imports System.Threading
Imports System.Drawing

Namespace Indicator
    Public Module Supertrend
        Public Sub CalculateSupertrend(ByVal periods As Integer, ByVal multiplier As Double, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Decimal), ByRef supertrendColor As Dictionary(Of Date, System.Drawing.Color), Optional neglectValidation As Boolean = False)
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                ATR.CalculateATR(periods, inputPayload, atrPayload, neglectValidation)             'This is using ATR(WILDER)
                If atrPayload IsNot Nothing AndAlso atrPayload.Count > 0 Then
                    Dim basicUpperband As Double = Nothing
                    Dim basicLowerband As Double = Nothing
                    Dim finalUpperband As Double = Nothing
                    Dim finalLowerband As Double = Nothing
                    Dim previousFinalUpperband As Double = 0
                    Dim previousFinalLowerband As Double = 0
                    Dim supertrend As Double = Nothing
                    Dim previousSupertrend As Double = 0
                    For Each runningInputPayload In inputPayload.Keys
                        If inputPayload(runningInputPayload).PreviousCandlePayload IsNot Nothing Then
                            basicUpperband = ((inputPayload(runningInputPayload).High + inputPayload(runningInputPayload).Low) / 2) + (multiplier * atrPayload(runningInputPayload))
                            basicLowerband = ((inputPayload(runningInputPayload).High + inputPayload(runningInputPayload).Low) / 2) - (multiplier * atrPayload(runningInputPayload))
                            finalUpperband = If(basicUpperband < previousFinalUpperband Or inputPayload(runningInputPayload).PreviousCandlePayload.Close > previousFinalUpperband, basicUpperband, previousFinalUpperband)
                            finalLowerband = If(basicLowerband > previousFinalLowerband Or inputPayload(runningInputPayload).PreviousCandlePayload.Close < previousFinalLowerband, basicLowerband, previousFinalLowerband)
                            If previousFinalUpperband = previousSupertrend AndAlso inputPayload(runningInputPayload).Close <= finalUpperband Then
                                supertrend = finalUpperband
                            ElseIf previousFinalUpperband = previousSupertrend AndAlso inputPayload(runningInputPayload).Close >= finalUpperband Then
                                supertrend = finalLowerband
                            ElseIf previousFinalLowerband = previousSupertrend AndAlso inputPayload(runningInputPayload).Close >= finalLowerband Then
                                supertrend = finalLowerband
                            ElseIf previousFinalLowerband = previousSupertrend AndAlso inputPayload(runningInputPayload).Close <= finalLowerband Then
                                supertrend = finalUpperband
                            Else
                                supertrend = 0
                            End If
                        Else
                            supertrend = 0
                        End If
                        If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Decimal)
                        outputPayload.Add(runningInputPayload, supertrend)
                        If supertrendColor Is Nothing Then supertrendColor = New Dictionary(Of Date, Drawing.Color)
                        supertrendColor.Add(runningInputPayload, If(inputPayload(runningInputPayload).Close < supertrend, Color.Red, Color.Green))
                        previousFinalLowerband = finalLowerband
                        previousFinalUpperband = finalUpperband
                        previousSupertrend = supertrend
                    Next
                End If
            End If
        End Sub
    End Module
End Namespace
