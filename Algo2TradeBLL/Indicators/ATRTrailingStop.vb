Imports System.Drawing

Namespace Indicator
    Public Module ATRTrailingStop
        Public Sub CalculateATRTrailingStop(ByVal ATRPeriod As Integer, ByVal Multiplier As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Decimal), ByRef outputColorPayload As Dictionary(Of Date, Color), Optional neglectValidation As Boolean = False)
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If Not neglectValidation AndAlso inputPayload.Count < 100 Then
                    Throw New ApplicationException("Can't Calculate ATR Trailing Stop")
                End If
                Dim outputATR As Dictionary(Of Date, Decimal) = Nothing
                ATR.CalculateATR(ATRPeriod, inputPayload, outputATR, neglectValidation)
                Dim previousTrailingStop As Decimal = Nothing
                Dim counter As Integer = 0
                For Each runningInputPayload In inputPayload.Keys
                    Dim currentATRTrailingStop As Decimal = Nothing
                    If inputPayload(runningInputPayload).PreviousCandlePayload Is Nothing OrElse Not inputPayload.ContainsKey(inputPayload(runningInputPayload).PreviousCandlePayload.PayloadDate) Then
                        currentATRTrailingStop = 0
                    ElseIf inputPayload(runningInputPayload).PreviousCandlePayload.PreviousCandlePayload Is Nothing OrElse Not inputPayload.ContainsKey(inputPayload(runningInputPayload).PreviousCandlePayload.PreviousCandlePayload.PayloadDate) Then
                        If inputPayload(runningInputPayload).PreviousCandlePayload.Close >= previousTrailingStop Then
                            currentATRTrailingStop = inputPayload(runningInputPayload).PreviousCandlePayload.Close - Multiplier * outputATR(inputPayload(runningInputPayload).PreviousCandlePayload.PayloadDate)
                        Else
                            currentATRTrailingStop = inputPayload(runningInputPayload).PreviousCandlePayload.Close + Multiplier * outputATR(inputPayload(runningInputPayload).PreviousCandlePayload.PayloadDate)
                        End If
                    Else
                        If inputPayload(runningInputPayload).PreviousCandlePayload.Close > previousTrailingStop AndAlso inputPayload(runningInputPayload).PreviousCandlePayload.PreviousCandlePayload.Close > previousTrailingStop Then
                            currentATRTrailingStop = Math.Max(previousTrailingStop, inputPayload(runningInputPayload).PreviousCandlePayload.Close - Multiplier * outputATR(inputPayload(runningInputPayload).PreviousCandlePayload.PayloadDate))
                        ElseIf inputPayload(runningInputPayload).PreviousCandlePayload.Close < previousTrailingStop AndAlso inputPayload(runningInputPayload).PreviousCandlePayload.PreviousCandlePayload.Close < previousTrailingStop Then
                            currentATRTrailingStop = Math.Min(previousTrailingStop, inputPayload(runningInputPayload).PreviousCandlePayload.Close + Multiplier * outputATR(inputPayload(runningInputPayload).PreviousCandlePayload.PayloadDate))
                        Else
                            If inputPayload(runningInputPayload).PreviousCandlePayload.Close > previousTrailingStop Then
                                currentATRTrailingStop = inputPayload(runningInputPayload).PreviousCandlePayload.Close - Multiplier * outputATR(inputPayload(runningInputPayload).PreviousCandlePayload.PayloadDate)
                            ElseIf inputPayload(runningInputPayload).PreviousCandlePayload.Close < previousTrailingStop Then
                                currentATRTrailingStop = inputPayload(runningInputPayload).PreviousCandlePayload.Close + Multiplier * outputATR(inputPayload(runningInputPayload).PreviousCandlePayload.PayloadDate)
                            Else
                                currentATRTrailingStop = previousTrailingStop
                            End If
                        End If
                    End If
                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Decimal)
                    If outputColorPayload Is Nothing Then outputColorPayload = New Dictionary(Of Date, Color)
                    outputPayload.Add(runningInputPayload, currentATRTrailingStop)
                    Dim signalColor As Color = Color.Black
                    If inputPayload(runningInputPayload).PreviousCandlePayload Is Nothing OrElse Not inputPayload.ContainsKey(inputPayload(runningInputPayload).PreviousCandlePayload.PayloadDate) Then
                        signalColor = Color.Green
                    Else
                        signalColor = If(inputPayload(runningInputPayload).PreviousCandlePayload.Close < previousTrailingStop, Color.Red, Color.Green)
                    End If
                    outputColorPayload.Add(runningInputPayload, signalColor)
                    previousTrailingStop = currentATRTrailingStop
                Next
            End If
        End Sub
    End Module
End Namespace