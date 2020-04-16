Imports System.Threading

Namespace Indicator
    Public Module BollingerBands
        Dim cts As CancellationTokenSource
        Dim cmn As Common = New Common(cts)
        Public Sub CalculateBollingerBands(ByVal period As Integer, ByVal field As Payload.PayloadFields, ByVal standardDeviationMultiplier As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputHighPayload As Dictionary(Of Date, Decimal), ByRef outputLowPayload As Dictionary(Of Date, Decimal), ByRef outputSMAPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If inputPayload.Count < period + 1 Then
                    Throw New ApplicationException("Can't Calculate Bollinger Bands")
                End If
                Indicator.SMA.CalculateSMA(period, field, inputPayload, outputSMAPayload)
                For Each runningPayload In inputPayload.Keys
                    Dim highBand As Decimal = Nothing
                    Dim lowBand As Decimal = Nothing
                    Dim sd As Decimal = Nothing
                    Dim previousNInputFieldPayload As List(Of KeyValuePair(Of Date, Payload)) = Common.GetSubPayload(inputPayload,
                                                                                                                       runningPayload,
                                                                                                                        period,
                                                                                                                        True)
                    Dim previousNInputData As Dictionary(Of Date, Decimal) = Nothing
                    previousNInputData = previousNInputFieldPayload.ToDictionary(Of Date, Decimal)(Function(x1)
                                                                                                       Return x1.Key
                                                                                                   End Function, Function(y)
                                                                                                                     Select Case field
                                                                                                                         Case Payload.PayloadFields.Close
                                                                                                                             Return y.Value.Close
                                                                                                                         Case Payload.PayloadFields.High
                                                                                                                             Return y.Value.High
                                                                                                                         Case Payload.PayloadFields.Low
                                                                                                                             Return y.Value.Low
                                                                                                                         Case Payload.PayloadFields.Open
                                                                                                                             Return y.Value.Open
                                                                                                                         Case Else
                                                                                                                             Throw New NotImplementedException
                                                                                                                     End Select
                                                                                                                 End Function)

                    If previousNInputData.Count > 2 Then
                        sd = Common.CalculateStandardDeviationPA(previousNInputData)
                    Else
                        sd = 0
                    End If
                    highBand = outputSMAPayload(runningPayload) + standardDeviationMultiplier * sd
                    lowBand = outputSMAPayload(runningPayload) - standardDeviationMultiplier * sd

                    If outputHighPayload Is Nothing Then outputHighPayload = New Dictionary(Of Date, Decimal)
                    outputHighPayload.Add(runningPayload, highBand)
                    If outputLowPayload Is Nothing Then outputLowPayload = New Dictionary(Of Date, Decimal)
                    outputLowPayload.Add(runningPayload, lowBand)
                Next
            End If
        End Sub
    End Module
End Namespace
