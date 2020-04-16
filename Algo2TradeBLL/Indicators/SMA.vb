Imports System.Threading

Namespace Indicator
    Public Module SMA
        ReadOnly cts As New CancellationTokenSource
        Public Sub CalculateSMA(ByVal smaPeriod As Integer, ByVal smaField As Payload.PayloadFields, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim finalPriceToBeAdded As Decimal = 0
                For Each runningInputPayload In inputPayload

                    'If it is less than IndicatorPeriod, we will need to take SMA of all previous prices, hence the call to GetSubPayload
                    Dim previousNInputFieldPayload As List(Of KeyValuePair(Of DateTime, Payload)) = Common.GetSubPayload(inputPayload,
                                                                                                                       runningInputPayload.Key,
                                                                                                                        smaPeriod - 1,
                                                                                                                        False)
                    If previousNInputFieldPayload Is Nothing Then
                        Select Case smaField
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
                            Case Else
                                Throw New NotImplementedException
                        End Select
                    ElseIf previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count <= smaPeriod - 1 Then 'Because the first field is handled outside
                        Dim totalOfAllPrices As Decimal = 0

                        Select Case smaField
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
                            Case Else
                                Throw New NotImplementedException
                        End Select
                        finalPriceToBeAdded = totalOfAllPrices / (previousNInputFieldPayload.Count + 1)
                    Else
                        Dim totalOfAllPrices As Decimal = 0
                        Select Case smaField
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
                            Case Else
                                Throw New NotImplementedException
                        End Select
                        finalPriceToBeAdded = Math.Round((totalOfAllPrices / (previousNInputFieldPayload.Count + 1)), 2)
                    End If
                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Decimal)
                    outputPayload.Add(runningInputPayload.Key, finalPriceToBeAdded)
                Next
            End If
        End Sub

    End Module
End Namespace