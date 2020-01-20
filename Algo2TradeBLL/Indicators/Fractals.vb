Imports System.Threading

Namespace Indicator
    Public Module Fractals
        Dim cts As CancellationTokenSource
        Dim cmn As Common = New Common(cts)
        Public Sub CalculateFractal(ByVal fractalPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputHighPayload As Dictionary(Of Date, Decimal), ByRef outputLowPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                For barProcessingCounter As Integer = 1 To inputPayload.Count

                    Dim runningInputPayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = inputPayload.Take(barProcessingCounter)

                    Dim fractalFractalHighPriceValue As Decimal = Decimal.MinValue
                    'If you have more than the indicator period as input payload, then take the max of everything
                    If (runningInputPayload.Count < fractalPeriod) Then
                        fractalFractalHighPriceValue = runningInputPayload.Max(Function(x) As Integer
                                                                                   Return x.Value.High
                                                                               End Function)
                    Else
                        'Now that you have equal to or more than the indicator period in the input payload, start the analysis
                        Dim midHighPoint As Integer
                        Dim midHighPointIndex As Integer
                        midHighPoint = Math.Floor(fractalPeriod / 2) 'Since its index, ok to take floor
                        'Calculate the index of potential pivot
                        midHighPointIndex = runningInputPayload.Count - 1 - midHighPoint
                        Dim midHighPointValue As Decimal = Decimal.MinValue
                        midHighPointValue = runningInputPayload.ElementAt(midHighPointIndex).Value.High

                        Dim okToCheckLeft As Boolean = True
                        Dim pivotFoundOk As Boolean = True
                        Dim leftCountFoundOk As Integer = 0
                        Dim iterationStartCtr As Integer = 0
                        For ind As Integer = runningInputPayload.Count - 1 To 0 Step -1
                            'Once a value that respects the pivot value is found to the left, we start the iteration counter.
                            'Half of the indicator period means that many values to the left should respect the pivot value.
                            'So when the iteration counter has started, that means the first value has respected. 
                            'We need to then see that by the time the iteration counter reaches the half of the indicator period count
                            'the loop should have found the pivot, if not then the iteration would be doing one more iteration
                            'than the half of the indicator period value and hence should exit
                            If iterationStartCtr > 0 Then iterationStartCtr += 1 'First value respecting pivot was found and hence iterationStartCtr is >0, so start its incremental loop
                            If iterationStartCtr > midHighPoint Then 'If the increment is more than half of the indicator period count, it means we didnt get the pivot by this time, so exit
                                pivotFoundOk = False
                                Exit For
                            End If

                            'Do the right part
                            If ind > midHighPointIndex Then
                                'Now item on the right can be equal to the pivot
                                If runningInputPayload.ElementAt(ind).Value.High >= midHighPointValue Then
                                    okToCheckLeft = False
                                    pivotFoundOk = False
                                    Exit For
                                End If
                            ElseIf ind < midHighPointIndex And okToCheckLeft Then
                                'For the immediate previous item from the pivot 3 cases exist:
                                '1)If it doesnt respect, we exit, no pivot found
                                '2)If its equal we move the pivot to that
                                '3)If it respects, we move start the iterationStartCtr
                                If ind = midHighPointIndex - 1 And runningInputPayload.ElementAt(ind).Value.High > midHighPointValue Then
                                    pivotFoundOk = False
                                    Exit For
                                ElseIf ind = midHighPointIndex - 1 And runningInputPayload.ElementAt(ind).Value.High = midHighPointValue Then
                                    midHighPointIndex = ind
                                ElseIf ind = midHighPointIndex - 1 And runningInputPayload.ElementAt(ind).Value.High < midHighPointValue Then
                                    leftCountFoundOk += 1
                                    iterationStartCtr += 1
                                ElseIf runningInputPayload.ElementAt(ind).Value.High <= midHighPointValue Then
                                    leftCountFoundOk += 1
                                End If
                            End If
                            If leftCountFoundOk = midHighPoint Then Exit For
                        Next
                        If pivotFoundOk Then
                            fractalFractalHighPriceValue = midHighPointValue
                        End If
                        'If we cant find the fractal, then
                        If fractalFractalHighPriceValue = Decimal.MinValue Then
                            'Get the previous fractal
                            If outputHighPayload IsNot Nothing Then
                                Dim outputFactalhighPayloadMinusOne = cmn.GetPayloadAtPositionOrPositionMinus1(runningInputPayload.ElementAt(barProcessingCounter - 1).Key, outputHighPayload)
                                If (outputFactalhighPayloadMinusOne.Key <> DateTime.MinValue) Then
                                    fractalFractalHighPriceValue = outputFactalhighPayloadMinusOne.Value
                                Else
                                    fractalFractalHighPriceValue = runningInputPayload.ElementAt(barProcessingCounter - 1).Value.High
                                End If
                            End If
                        End If
                    End If

                    If Not (fractalFractalHighPriceValue = Decimal.MinValue) Then
                        If outputHighPayload Is Nothing Then outputHighPayload = New Dictionary(Of Date, Decimal)
                        If Not outputHighPayload.ContainsKey(runningInputPayload.ElementAt(barProcessingCounter - 1).Key) Then
                            outputHighPayload.Add(runningInputPayload.ElementAt(barProcessingCounter - 1).Key, fractalFractalHighPriceValue)
                        Else
                            outputHighPayload(runningInputPayload.ElementAt(barProcessingCounter - 1).Key) = fractalFractalHighPriceValue
                        End If
                    End If


                    Dim fractalFractalLowPriceValue As Decimal = Decimal.MinValue
                    'If you have less than the indicator period as input payload, then take the min of everything
                    If (runningInputPayload.Count < fractalPeriod) Then
                        fractalFractalLowPriceValue = runningInputPayload.Min(Function(x) As Integer
                                                                                  Return x.Value.Low
                                                                              End Function)
                    Else
                        'Now that you have equal to or more than the indicator period in the input payload, start the analysis
                        Dim midLowPoint As Integer
                        Dim midLowPointIndex As Integer
                        midLowPoint = Math.Floor(fractalPeriod / 2) 'Since its index, ok to take floor
                        'Calculate the index of potential pivot
                        midLowPointIndex = runningInputPayload.Count - 1 - midLowPoint
                        Dim midLowPointValue As Decimal = Decimal.MinValue
                        midLowPointValue = runningInputPayload.ElementAt(midLowPointIndex).Value.Low

                        Dim okToCheckLeft As Boolean = True
                        Dim pivotFoundOk As Boolean = True
                        Dim leftCountFoundOk As Integer = 0
                        Dim iterationStartCtr As Integer = 0
                        For ind As Integer = runningInputPayload.Count - 1 To 0 Step -1
                            'Once a value that respects the pivot value is found to the left, we start the iteration counter.
                            'Half of the indicator period means that many values to the left should respect the pivot value.
                            'So when the iteration counter has started, that means the first value has respected. 
                            'We need to then see that by the time the iteration counter reaches the half of the indicator period count
                            'the loop should have found the pivot, if not then the iteration would be doing one more iteration
                            'than the half of the indicator period value and hence should exit
                            If iterationStartCtr > 0 Then iterationStartCtr += 1 'First value respecting pivot was found and hence iterationStartCtr is >0, so start its incremental loop
                            If iterationStartCtr > midLowPoint Then 'If the increment is more than half of the indicator period count, it means we didnt get the pivot by this time, so exit
                                pivotFoundOk = False
                                Exit For
                            End If

                            'Do the right part
                            If ind > midLowPointIndex Then
                                'Now item on the right can be equal to the pivot
                                If runningInputPayload.ElementAt(ind).Value.Low <= midLowPointValue Then
                                    okToCheckLeft = False
                                    pivotFoundOk = False
                                    Exit For
                                End If
                            ElseIf ind < midLowPointIndex And okToCheckLeft Then
                                'For the immediate previous item from the pivot 3 cases exist:
                                '1)If it doesnt respect, we exit, no pivot found
                                '2)If its equal we move the pivot to that
                                '3)If it respects, we move start the iterationStartCtr
                                If ind = midLowPointIndex - 1 And runningInputPayload.ElementAt(ind).Value.Low < midLowPointValue Then
                                    pivotFoundOk = False
                                    Exit For
                                ElseIf ind = midLowPointIndex - 1 And runningInputPayload.ElementAt(ind).Value.Low = midLowPointValue Then
                                    midLowPointIndex = ind
                                ElseIf ind = midLowPointIndex - 1 And runningInputPayload.ElementAt(ind).Value.Low > midLowPointValue Then
                                    leftCountFoundOk += 1
                                    iterationStartCtr += 1
                                ElseIf runningInputPayload.ElementAt(ind).Value.Low >= midLowPointValue Then
                                    leftCountFoundOk += 1
                                End If
                            End If
                            If leftCountFoundOk = midLowPoint Then Exit For
                        Next
                        If pivotFoundOk Then
                            fractalFractalLowPriceValue = midLowPointValue
                        End If
                        If fractalFractalLowPriceValue = Decimal.MinValue Then
                            'Get the previous fractal
                            If outputLowPayload IsNot Nothing Then
                                Dim outputFactalLowPayloadMinusOne = cmn.GetPayloadAtPositionOrPositionMinus1(runningInputPayload.ElementAt(barProcessingCounter - 1).Key, outputLowPayload)
                                If (outputFactalLowPayloadMinusOne.Key <> DateTime.MinValue) Then
                                    fractalFractalLowPriceValue = outputFactalLowPayloadMinusOne.Value
                                Else
                                    fractalFractalLowPriceValue = runningInputPayload.ElementAt(barProcessingCounter - 1).Value.Low
                                End If
                            End If
                        End If
                    End If
                    If Not (fractalFractalLowPriceValue = Decimal.MinValue) Then
                        If outputLowPayload Is Nothing Then outputLowPayload = New Dictionary(Of Date, Decimal)
                        If Not outputLowPayload.ContainsKey(runningInputPayload.ElementAt(barProcessingCounter - 1).Key) Then
                            outputLowPayload.Add(runningInputPayload.ElementAt(barProcessingCounter - 1).Key, fractalFractalLowPriceValue)
                        Else
                            outputLowPayload(runningInputPayload.ElementAt(barProcessingCounter - 1).Key) = fractalFractalLowPriceValue
                        End If
                    End If

                Next
            End If
        End Sub
        Public Function GetDifferentNthFractalBefore(ByVal beforeThisTime As Date, ByVal payload As Dictionary(Of Date, Decimal), ByVal howManyBefore As Integer) As KeyValuePair(Of Date, Decimal)
            Dim ret As KeyValuePair(Of Date, Decimal) = Nothing
            If payload IsNot Nothing AndAlso payload.Count > 0 Then
                Dim subPayload As IEnumerable(Of KeyValuePair(Of Date, Decimal)) = Common.GetSubPayload(payload, beforeThisTime, 200, False)
                Dim priceCheck As Decimal = payload(beforeThisTime)
                Dim ctrFound As Integer = 0
                If subPayload IsNot Nothing And subPayload.Count > 0 Then
                    For i = subPayload.Count - 1 To 0 Step -1
                        If subPayload(i).Value <> priceCheck Then
                            ret = subPayload(i)
                            ctrFound += 1
                            priceCheck = subPayload(i).Value
                        End If

                        If ctrFound = howManyBefore Then Exit For
                    Next
                End If
            End If
            Return ret
        End Function
        Public Function GetDifferentNFractalsBefore(ByVal beforeThisTime As Date, ByVal payload As Dictionary(Of Date, Decimal), ByVal howManyToRetrieve As Integer) As IEnumerable(Of KeyValuePair(Of Date, Decimal))
            Dim ret As IEnumerable(Of KeyValuePair(Of Date, Decimal)) = Nothing
            If payload IsNot Nothing AndAlso payload.Count > 0 Then
                Dim ret1 As Dictionary(Of Date, Decimal) = Nothing
                For i = 1 To howManyToRetrieve
                    Dim nthFractal As KeyValuePair(Of Date, Decimal) = GetDifferentNthFractalBefore(beforeThisTime, payload, i)
                    If ret1 Is Nothing Then ret1 = New Dictionary(Of Date, Decimal)
                    ret1.Add(nthFractal.Key, nthFractal.Value)
                Next
                ret = ret1
            End If
            Return ret
        End Function
        Public Function GetPreviousFractal(ByVal beforeThisTime As Date, ByVal payload As Dictionary(Of Date, Decimal)) As KeyValuePair(Of Date, Decimal)
            Dim ret As KeyValuePair(Of Date, Decimal) = Nothing
            If payload IsNot Nothing AndAlso payload.Count > 0 Then
                Dim subPayload As IEnumerable(Of KeyValuePair(Of Date, Decimal)) = Common.GetSubPayload(payload, beforeThisTime, 10, False)
                If subPayload IsNot Nothing AndAlso subPayload.Count > 0 Then
                    ret = subPayload(subPayload.Count - 1)
                End If
            End If
            Return ret
        End Function
    End Module
End Namespace
