Imports System.Threading

Namespace Indicator
    Public Module SMI
        Public Sub CalculateSMI(ByVal K_Periods As Integer, ByVal K_Smoothing As Integer, ByVal K_DoubleSmoothing As Integer, ByVal D_Periods As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputSMIsignalPayload As Dictionary(Of Date, Decimal), ByRef outputEMASMIsignalPayload As Dictionary(Of Date, Decimal))
            Dim SMIIntermediatePayload As New Dictionary(Of Date, Payload)
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If inputPayload.Count < 100 Then
                    Throw New ApplicationException("Can't Calculate SMI")
                End If

                For Each runninginputpayload In inputPayload

                    Dim previousNInputFieldPayload As List(Of KeyValuePair(Of DateTime, Payload)) = Common.GetSubPayload(inputPayload,
                                                                                                                       runninginputpayload.Key,
                                                                                                                        K_Periods,
                                                                                                                        True)
                    Dim low As Decimal = Nothing
                    Dim high As Decimal = Nothing
                    low = previousNInputFieldPayload.Min((Function(x) x.Value.Low))
                    high = previousNInputFieldPayload.Max((Function(x) x.Value.High))
                    Dim diff = high - low
                    Dim rdiff = runninginputpayload.Value.Close - ((high + low) / 2)
                    'Console.WriteLine(diff)
                    Dim tempPayload As Payload = New Payload(Payload.CandleDataSource.Chart)
                    tempPayload.PayloadDate = runninginputpayload.Value.PayloadDate
                    tempPayload.H_L = diff
                    tempPayload.C_AVG_HL = rdiff
                    SMIIntermediatePayload.Add(runninginputpayload.Value.PayloadDate, tempPayload)
                Next

                Dim diffEMAOutput As Dictionary(Of Date, Decimal) = Nothing
                Dim rdiffEMAOutput As Dictionary(Of Date, Decimal) = Nothing
                EMA.CalculateEMA(K_Smoothing, Payload.PayloadFields.H_L, SMIIntermediatePayload, diffEMAOutput)
                EMA.CalculateEMA(K_Smoothing, Payload.PayloadFields.C_AVG_HL, SMIIntermediatePayload, rdiffEMAOutput)

                Dim diffEMAOutputPayload As New Dictionary(Of Date, Payload)
                Common.ConvertDecimalToPayload(Payload.PayloadFields.H_L, diffEMAOutput, diffEMAOutputPayload)
                Dim rdiffEMAOutputPayload As New Dictionary(Of Date, Payload)
                Common.ConvertDecimalToPayload(Payload.PayloadFields.C_AVG_HL, rdiffEMAOutput, rdiffEMAOutputPayload)

                'For Each item In diffEMAOutput.Keys
                '    Console.WriteLine(diffEMAOutput(item))
                'Next

                Dim avgdiff As Dictionary(Of Date, Decimal) = Nothing
                Dim avgrdiff As Dictionary(Of Date, Decimal) = Nothing
                EMA.CalculateEMA(K_DoubleSmoothing, Payload.PayloadFields.H_L, diffEMAOutputPayload, avgdiff)
                EMA.CalculateEMA(K_DoubleSmoothing, Payload.PayloadFields.C_AVG_HL, rdiffEMAOutputPayload, avgrdiff)

                outputSMIsignalPayload = New Dictionary(Of Date, Decimal)
                For Each item In avgdiff.Keys

                    Dim cal As Decimal = If(avgdiff(item) <> 0, (avgrdiff(item) / (avgdiff(item) / 2) * 100), 0)
                    outputSMIsignalPayload.Add(item, Math.Round(cal, 4))
                Next

                Dim SMIPayload As New Dictionary(Of Date, Payload)
                Common.ConvertDecimalToPayload(Payload.PayloadFields.SMI_EMA, outputSMIsignalPayload, SMIPayload)

                EMA.CalculateEMA(D_Periods, Payload.PayloadFields.SMI_EMA, SMIPayload, outputEMASMIsignalPayload)

                'If outputSMIsignalPayload Is Nothing Then outputSMIsignalPayload = New Dictionary(Of Date, Decimal)
                'outputSMIsignalPayload.Add(runninginputpayload.Key, diff)
                'If outputEMASMIsignalPayload Is Nothing Then outputEMASMIsignalPayload = New Dictionary(Of Date, Decimal)
                'outputEMASMIsignalPayload.Add(runninginputpayload.Key, rdiff)
            End If
        End Sub
    End Module
End Namespace