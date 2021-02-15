Namespace Indicator
    Public Module KeltnerChannel
        Public Sub CalculateSMAKeltnerChannel(ByVal period As Integer, ByVal shift As Decimal, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputHighPayload As Dictionary(Of Date, Decimal), ByRef outputLowPayload As Dictionary(Of Date, Decimal), ByRef outputSMAPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.ATR.CalculateATR(period, inputPayload, atrPayload)

                Indicator.SMA.CalculateSMA(period, Payload.PayloadFields.Close, inputPayload, outputSMAPayload)

                For Each runningPayload In inputPayload.Keys
                    Dim sma As Decimal = outputSMAPayload(runningPayload)
                    Dim atr As Decimal = atrPayload(runningPayload)
                    Dim highKeltner As Decimal = sma + shift * atr
                    Dim lowKeltner As Decimal = sma - shift * atr

                    If outputHighPayload Is Nothing Then outputHighPayload = New Dictionary(Of Date, Decimal)
                    outputHighPayload.Add(runningPayload, Math.Round(highKeltner, 4))
                    If outputLowPayload Is Nothing Then outputLowPayload = New Dictionary(Of Date, Decimal)
                    outputLowPayload.Add(runningPayload, Math.Round(lowKeltner, 4))
                Next
            End If
        End Sub

        Public Sub CalculateEMAKeltnerChannel(ByVal period As Integer, ByVal shift As Decimal, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputHighPayload As Dictionary(Of Date, Decimal), ByRef outputLowPayload As Dictionary(Of Date, Decimal), ByRef outputEMAPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.ATR.CalculateATR(period, inputPayload, atrPayload)

                Indicator.EMA.CalculateEMA(period, Payload.PayloadFields.Close, inputPayload, outputEMAPayload)

                For Each runningPayload In inputPayload.Keys
                    Dim ema As Decimal = outputEMAPayload(runningPayload)
                    Dim atr As Decimal = atrPayload(runningPayload)
                    Dim highKeltner As Decimal = ema + shift * atr
                    Dim lowKeltner As Decimal = ema - shift * atr

                    If outputHighPayload Is Nothing Then outputHighPayload = New Dictionary(Of Date, Decimal)
                    outputHighPayload.Add(runningPayload, Math.Round(highKeltner, 4))
                    If outputLowPayload Is Nothing Then outputLowPayload = New Dictionary(Of Date, Decimal)
                    outputLowPayload.Add(runningPayload, Math.Round(lowKeltner, 4))
                Next
            End If
        End Sub
    End Module
End Namespace