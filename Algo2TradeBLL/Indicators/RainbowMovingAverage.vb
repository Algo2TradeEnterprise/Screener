Namespace Indicator
    Public Module RainbowMovingAverage
        Public Class RainbowMA
            Public Property SMA1 As Decimal
            Public Property SMA2 As Decimal
            Public Property SMA3 As Decimal
            Public Property SMA4 As Decimal
            Public Property SMA5 As Decimal
            Public Property SMA6 As Decimal
            Public Property SMA7 As Decimal
            Public Property SMA8 As Decimal
            Public Property SMA9 As Decimal
            Public Property SMA10 As Decimal
        End Class

        Public Sub CalculateRainbowMovingAverage(ByVal period As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, RainbowMA))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If inputPayload.Count < period + 1 Then
                    Throw New ApplicationException("Can't Calculate Rainbow Moving Average")
                End If
                Dim outputSMAPayload1 As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(period, Payload.PayloadFields.Close, inputPayload, outputSMAPayload1)
                Dim outputSMAPayload2 As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(period, Payload.PayloadFields.Additional_Field, Common.ConvertDecimalToPayload(Payload.PayloadFields.Additional_Field, outputSMAPayload1), outputSMAPayload2)
                Dim outputSMAPayload3 As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(period, Payload.PayloadFields.Additional_Field, Common.ConvertDecimalToPayload(Payload.PayloadFields.Additional_Field, outputSMAPayload2), outputSMAPayload3)
                Dim outputSMAPayload4 As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(period, Payload.PayloadFields.Additional_Field, Common.ConvertDecimalToPayload(Payload.PayloadFields.Additional_Field, outputSMAPayload3), outputSMAPayload4)
                Dim outputSMAPayload5 As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(period, Payload.PayloadFields.Additional_Field, Common.ConvertDecimalToPayload(Payload.PayloadFields.Additional_Field, outputSMAPayload4), outputSMAPayload5)
                Dim outputSMAPayload6 As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(period, Payload.PayloadFields.Additional_Field, Common.ConvertDecimalToPayload(Payload.PayloadFields.Additional_Field, outputSMAPayload5), outputSMAPayload6)
                Dim outputSMAPayload7 As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(period, Payload.PayloadFields.Additional_Field, Common.ConvertDecimalToPayload(Payload.PayloadFields.Additional_Field, outputSMAPayload6), outputSMAPayload7)
                Dim outputSMAPayload8 As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(period, Payload.PayloadFields.Additional_Field, Common.ConvertDecimalToPayload(Payload.PayloadFields.Additional_Field, outputSMAPayload7), outputSMAPayload8)
                Dim outputSMAPayload9 As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(period, Payload.PayloadFields.Additional_Field, Common.ConvertDecimalToPayload(Payload.PayloadFields.Additional_Field, outputSMAPayload8), outputSMAPayload9)
                Dim outputSMAPayload10 As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(period, Payload.PayloadFields.Additional_Field, Common.ConvertDecimalToPayload(Payload.PayloadFields.Additional_Field, outputSMAPayload9), outputSMAPayload10)

                For Each runningPayload In inputPayload.Keys
                    Dim rainbow As RainbowMA = New RainbowMA With {
                        .SMA1 = outputSMAPayload1(runningPayload),
                        .SMA2 = outputSMAPayload2(runningPayload),
                        .SMA3 = outputSMAPayload3(runningPayload),
                        .SMA4 = outputSMAPayload4(runningPayload),
                        .SMA5 = outputSMAPayload5(runningPayload),
                        .SMA6 = outputSMAPayload6(runningPayload),
                        .SMA7 = outputSMAPayload7(runningPayload),
                        .SMA8 = outputSMAPayload8(runningPayload),
                        .SMA9 = outputSMAPayload9(runningPayload),
                        .SMA10 = outputSMAPayload10(runningPayload)
                    }

                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, RainbowMA)
                    outputPayload.Add(runningPayload, rainbow)
                Next
            End If
        End Sub
    End Module
End Namespace