Imports System.Threading

Namespace Indicator
    Public Module CCI
        Dim cts As CancellationTokenSource
        Dim cmn As Common = New Common(cts)
        Public Sub CalculateCCI(ByVal CCIPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputCCI As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If inputPayload.Count < 100 Then
                    Throw New ApplicationException("Can't Calculate CCI")
                End If
                Dim CCIPeriodSMA As Decimal = 0
                For Each runninginputpayload In inputPayload

                    Dim previousNInputFieldPayload As List(Of KeyValuePair(Of DateTime, Payload)) = Common.GetSubPayload(inputPayload,
                                                                                                                           runninginputpayload.Key,
                                                                                                                            CCIPeriod,
                                                                                                                            True)
                    Dim typicalPrice As Decimal = 0
                    If previousNInputFieldPayload Is Nothing Then
                        CCIPeriodSMA = (runninginputpayload.Value.High + runninginputpayload.Value.Low + runninginputpayload.Value.Close) / 3
                    ElseIf previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count <= CCIPeriod - 1 Then 'Because the first field is handled outside
                        CCIPeriodSMA = previousNInputFieldPayload.Average(Function(s) ((s.Value.High + s.Value.Low + s.Value.Close) / 3))
                    Else
                        CCIPeriodSMA = previousNInputFieldPayload.Average(Function(s) ((s.Value.High + s.Value.Low + s.Value.Close) / 3))
                        Dim CCIPeriodMeanDeviation As Double = 0
                        For Each previousNInputFieldPayloadItem In previousNInputFieldPayload
                            typicalPrice = (previousNInputFieldPayloadItem.Value.High + previousNInputFieldPayloadItem.Value.Low + previousNInputFieldPayloadItem.Value.Close) / 3
                            CCIPeriodMeanDeviation += Math.Abs(CCIPeriodSMA - typicalPrice)
                        Next
                        CCIPeriodMeanDeviation = CCIPeriodMeanDeviation / CCIPeriod
                        Dim CCIValue As Decimal = Nothing
                        If Math.Round(CCIPeriodMeanDeviation, 4) = 0 Then
                            CCIValue = 1            'As CCI Period Mean Deviation is 0, So according to Zerodha Chart CCI Value is 1
                        Else
                            CCIValue = (typicalPrice - CCIPeriodSMA) / (0.015 * CCIPeriodMeanDeviation)
                        End If
                        If outputCCI Is Nothing Then outputCCI = New Dictionary(Of Date, Decimal)
                        outputCCI.Add(runninginputpayload.Key, Math.Round(CCIValue, 4))
                    End If
                Next
            End If
        End Sub
    End Module
End Namespace
