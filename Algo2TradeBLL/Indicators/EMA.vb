Imports System.Threading

Namespace Indicator
    Public Module EMA
        Dim cts As CancellationTokenSource
        Public Sub CalculateEMA(ByVal emaPeriod As Integer, ByVal emaField As Payload.PayloadFields, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim finalPriceToBeAdded As Decimal = 0
                For Each runningInputPayload In inputPayload
                    'If it is less than IndicatorPeriod, we will need to take SMA of all previous prices, hence the call to GetSubPayload
                    Dim previousNInputFieldPayload As List(Of KeyValuePair(Of DateTime, Payload)) = Common.GetSubPayload(inputPayload,
                                                                                                                           runningInputPayload.Key,
                                                                                                                            emaPeriod,
                                                                                                                            False)
                    If previousNInputFieldPayload Is Nothing Then
                        Select Case emaField
                            Case Payload.PayloadFields.Close
                                finalPriceToBeAdded += runningInputPayload.Value.Close
                            Case Payload.PayloadFields.High
                                finalPriceToBeAdded += runningInputPayload.Value.High
                            Case Payload.PayloadFields.Low
                                finalPriceToBeAdded += runningInputPayload.Value.Low
                            Case Payload.PayloadFields.Open
                                finalPriceToBeAdded += runningInputPayload.Value.Open
                            Case Payload.PayloadFields.Volume
                                finalPriceToBeAdded += runningInputPayload.Value.Volume
                            Case Payload.PayloadFields.H_L
                                finalPriceToBeAdded += runningInputPayload.Value.H_L
                            Case Payload.PayloadFields.C_AVG_HL
                                finalPriceToBeAdded += runningInputPayload.Value.C_AVG_HL
                            Case Payload.PayloadFields.SMI_EMA
                                finalPriceToBeAdded += runningInputPayload.Value.SMI_EMA
                            Case Payload.PayloadFields.Additional_Field
                                finalPriceToBeAdded += runningInputPayload.Value.Additional_Field
                            Case Else
                                Throw New NotImplementedException
                        End Select
                    ElseIf previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count <= emaPeriod - 1 Then 'Because the first field is handled outside
                        Dim totalOfAllPrices As Decimal = 0
                        Select Case emaField
                            Case Payload.PayloadFields.Close
                                totalOfAllPrices = runningInputPayload.Value.Close
                                totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.Close)
                            Case Payload.PayloadFields.High
                                totalOfAllPrices = runningInputPayload.Value.High
                                totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.High)
                            Case Payload.PayloadFields.Low
                                totalOfAllPrices = runningInputPayload.Value.Low
                                totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.Low)
                            Case Payload.PayloadFields.Open
                                totalOfAllPrices = runningInputPayload.Value.Open
                                totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.Open)
                            Case Payload.PayloadFields.Volume
                                totalOfAllPrices = runningInputPayload.Value.Volume
                                totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.Volume)
                            Case Payload.PayloadFields.H_L
                                totalOfAllPrices = runningInputPayload.Value.H_L
                                totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.H_L)
                            Case Payload.PayloadFields.C_AVG_HL
                                totalOfAllPrices = runningInputPayload.Value.C_AVG_HL
                                totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.C_AVG_HL)
                            Case Payload.PayloadFields.SMI_EMA
                                totalOfAllPrices = runningInputPayload.Value.SMI_EMA
                                totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.SMI_EMA)
                            Case Payload.PayloadFields.Additional_Field
                                totalOfAllPrices = runningInputPayload.Value.Additional_Field
                                totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.Additional_Field)
                            Case Else
                                Throw New NotImplementedException
                        End Select
                        finalPriceToBeAdded = totalOfAllPrices / (previousNInputFieldPayload.Count + 1)
                    Else
                        Dim previousInputFieldData = Common.GetPayloadAtPositionOrPositionMinus1(runningInputPayload.Key, outputPayload)
                        If previousInputFieldData.Key <> DateTime.MinValue Then
                            Dim previousInputFieldValue As Decimal = previousInputFieldData.Value
                            Select Case emaField
                                Case Payload.PayloadFields.Close
                                    finalPriceToBeAdded = (runningInputPayload.Value.Close * (2 / (1 + emaPeriod))) + (previousInputFieldValue * (1 - (2 / (emaPeriod + 1))))
                                Case Payload.PayloadFields.High
                                    finalPriceToBeAdded = (runningInputPayload.Value.High * (2 / (1 + emaPeriod))) + (previousInputFieldValue * (1 - (2 / (emaPeriod + 1))))
                                Case Payload.PayloadFields.Low
                                    finalPriceToBeAdded = (runningInputPayload.Value.Low * (2 / (1 + emaPeriod))) + (previousInputFieldValue * (1 - (2 / (emaPeriod + 1))))
                                Case Payload.PayloadFields.Open
                                    finalPriceToBeAdded = (runningInputPayload.Value.Open * (2 / (1 + emaPeriod))) + (previousInputFieldValue * (1 - (2 / (emaPeriod + 1))))
                                Case Payload.PayloadFields.Volume
                                    finalPriceToBeAdded = (runningInputPayload.Value.Volume * (2 / (1 + emaPeriod))) + (previousInputFieldValue * (1 - (2 / (emaPeriod + 1))))
                                Case Payload.PayloadFields.H_L
                                    finalPriceToBeAdded = (runningInputPayload.Value.H_L * (2 / (1 + emaPeriod))) + (previousInputFieldValue * (1 - (2 / (emaPeriod + 1))))
                                Case Payload.PayloadFields.C_AVG_HL
                                    finalPriceToBeAdded = (runningInputPayload.Value.C_AVG_HL * (2 / (1 + emaPeriod))) + (previousInputFieldValue * (1 - (2 / (emaPeriod + 1))))
                                Case Payload.PayloadFields.SMI_EMA
                                    finalPriceToBeAdded = (runningInputPayload.Value.SMI_EMA * (2 / (1 + emaPeriod))) + (previousInputFieldValue * (1 - (2 / (emaPeriod + 1))))
                                Case Payload.PayloadFields.Additional_Field
                                    finalPriceToBeAdded = (runningInputPayload.Value.Additional_Field * (2 / (1 + emaPeriod))) + (previousInputFieldValue * (1 - (2 / (emaPeriod + 1))))
                                Case Else
                                    Throw New NotImplementedException
                            End Select
                        End If
                    End If
                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Decimal)
                    outputPayload.Add(runningInputPayload.Key, finalPriceToBeAdded)
                Next
            End If
        End Sub
    End Module
End Namespace
