Namespace Indicator
    Public Module ADX
        Public Sub CalculateADX(ByVal period As Integer, ByVal smoothingPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputADXPayload As Dictionary(Of Date, Decimal), ByRef outputDIPlusPayload As Dictionary(Of Date, Decimal), ByRef outputDIMinusPayload As Dictionary(Of Date, Decimal), ByRef trPayload As Dictionary(Of Date, Decimal), ByRef dm1PlusPayload As Dictionary(Of Date, Decimal), ByRef dm1MinusPayload As Dictionary(Of Date, Decimal), ByRef dxPayload As Dictionary(Of Date, Decimal), ByRef tr14Payload As Dictionary(Of Date, Decimal), ByRef dm14PlusPayload As Dictionary(Of Date, Decimal), ByRef dm14MinusPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim firstPayload As Boolean = True
                Dim HighLow As Decimal = Nothing
                Dim HighPClose As Decimal = Nothing
                Dim LowPClose As Decimal = Nothing
                Dim PreviousClose As Decimal = Nothing
                Dim TR As Decimal = 0
                Dim DM1_Plus As Decimal = 0
                Dim DM1_Minus As Decimal = 0
                Dim TRForPeriod As Decimal = 0
                Dim previousTRForPeriod As Decimal = 0
                Dim DM_PlusForPeriod As Decimal = 0
                Dim previousDM_PlusForPeriod As Decimal = 0
                Dim DM_MinusForPeriod As Decimal = 0
                Dim previousDM_MinusForPeriod As Decimal = 0
                Dim DIPlus As Decimal = 0
                Dim DIMinus As Decimal = 0
                Dim diffDI As Decimal = 0
                Dim sumDI As Decimal = 0
                Dim DX As Decimal = 0
                Dim ADX As Decimal = 0

                'Dim trPayload As Dictionary(Of Date, Decimal) = Nothing
                'Dim dm1PlusPayload As Dictionary(Of Date, Decimal) = Nothing
                'Dim dm1MinusPayload As Dictionary(Of Date, Decimal) = Nothing
                'Dim dxPayload As Dictionary(Of Date, Decimal) = Nothing

                'Dim tr14Payload As Dictionary(Of Date, Decimal) = Nothing
                'Dim dm14PlusPayload As Dictionary(Of Date, Decimal) = Nothing
                'Dim dm14MinusPayload As Dictionary(Of Date, Decimal) = Nothing

                Dim counter As Integer = 0

                For Each runningInputPayload In inputPayload
                    counter += 1
                    HighLow = runningInputPayload.Value.High - runningInputPayload.Value.Low
                    If firstPayload = True Then
                        TR = HighLow
                        firstPayload = False
                    Else
                        HighPClose = Math.Abs(runningInputPayload.Value.High - runningInputPayload.Value.PreviousCandlePayload.Close)
                        LowPClose = Math.Abs(runningInputPayload.Value.Low - runningInputPayload.Value.PreviousCandlePayload.Close)
                        TR = Math.Max(HighLow, Math.Max(HighPClose, LowPClose))
                        If (CSng(runningInputPayload.Value.High) - CSng(runningInputPayload.Value.PreviousCandlePayload.High)) > (CSng(runningInputPayload.Value.PreviousCandlePayload.Low) - CSng(runningInputPayload.Value.Low)) Then
                            DM1_Plus = Math.Max(runningInputPayload.Value.High - runningInputPayload.Value.PreviousCandlePayload.High, 0)
                        Else
                            DM1_Plus = 0
                        End If
                        If (CSng(runningInputPayload.Value.PreviousCandlePayload.Low) - CSng(runningInputPayload.Value.Low)) > (CSng(runningInputPayload.Value.High) - CSng(runningInputPayload.Value.PreviousCandlePayload.High)) Then
                            DM1_Minus = Math.Max(runningInputPayload.Value.PreviousCandlePayload.Low - runningInputPayload.Value.Low, 0)
                        Else
                            DM1_Minus = 0
                        End If
                    End If
                    If trPayload Is Nothing Then trPayload = New Dictionary(Of Date, Decimal)
                    trPayload.Add(runningInputPayload.Key, TR)
                    If dm1PlusPayload Is Nothing Then dm1PlusPayload = New Dictionary(Of Date, Decimal)
                    dm1PlusPayload.Add(runningInputPayload.Key, DM1_Plus)
                    If dm1MinusPayload Is Nothing Then dm1MinusPayload = New Dictionary(Of Date, Decimal)
                    dm1MinusPayload.Add(runningInputPayload.Key, DM1_Minus)

                    Dim previousNInputFieldPayload As List(Of KeyValuePair(Of Date, Decimal)) = Nothing

                    previousNInputFieldPayload = Common.GetSubPayload(trPayload, runningInputPayload.Key, smoothingPeriod, True)
                    If previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count > 0 Then
                        If counter = smoothingPeriod + 1 Then
                            TRForPeriod = previousNInputFieldPayload.Sum(Function(s)
                                                                             Return s.Value
                                                                         End Function)
                        ElseIf counter > smoothingPeriod + 1 Then
                            TRForPeriod = previousTRForPeriod - (previousTRForPeriod / smoothingPeriod) + TR
                        Else
                            TRForPeriod = 0
                        End If
                        previousTRForPeriod = TRForPeriod
                    End If
                    If tr14Payload Is Nothing Then tr14Payload = New Dictionary(Of Date, Decimal)
                    tr14Payload.Add(runningInputPayload.Key, TRForPeriod)

                    previousNInputFieldPayload = Nothing
                    previousNInputFieldPayload = Common.GetSubPayload(dm1PlusPayload, runningInputPayload.Key, smoothingPeriod, True)
                    If previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count > 0 Then
                        If counter = smoothingPeriod + 1 Then
                            DM_PlusForPeriod = previousNInputFieldPayload.Sum(Function(s)
                                                                                  Return s.Value
                                                                              End Function)
                        ElseIf counter > smoothingPeriod + 1 Then
                            DM_PlusForPeriod = previousDM_PlusForPeriod - (previousDM_PlusForPeriod / smoothingPeriod) + DM1_Plus
                        Else
                            DM_PlusForPeriod = 0
                        End If
                        previousDM_PlusForPeriod = DM_PlusForPeriod
                    End If
                    If dm14PlusPayload Is Nothing Then dm14PlusPayload = New Dictionary(Of Date, Decimal)
                    dm14PlusPayload.Add(runningInputPayload.Key, DM_PlusForPeriod)

                    previousNInputFieldPayload = Nothing
                    previousNInputFieldPayload = Common.GetSubPayload(dm1MinusPayload, runningInputPayload.Key, smoothingPeriod, True)
                    If previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count > 0 Then
                        If counter = smoothingPeriod + 1 Then
                            DM_MinusForPeriod = previousNInputFieldPayload.Sum(Function(s)
                                                                                   Return s.Value
                                                                               End Function)
                        ElseIf counter > smoothingPeriod + 1 Then
                            DM_MinusForPeriod = previousDM_MinusForPeriod - (previousDM_MinusForPeriod / smoothingPeriod) + DM1_Minus
                        Else
                            DM_MinusForPeriod = 0
                        End If
                        previousDM_MinusForPeriod = DM_MinusForPeriod
                    End If
                    If dm14MinusPayload Is Nothing Then dm14MinusPayload = New Dictionary(Of Date, Decimal)
                    dm14MinusPayload.Add(runningInputPayload.Key, DM_MinusForPeriod)

                    If counter >= smoothingPeriod + 1 Then
                        DIPlus = 100 * (DM_PlusForPeriod / TRForPeriod)
                        DIMinus = 100 * (DM_MinusForPeriod / TRForPeriod)
                        diffDI = Math.Abs(DIPlus - DIMinus)
                        sumDI = DIPlus + DIMinus
                        DX = 100 * (diffDI / sumDI)
                    Else
                        DIPlus = 0
                        DIMinus = 0
                        diffDI = 0
                        sumDI = 0
                        DX = 0
                    End If

                    If outputDIPlusPayload Is Nothing Then outputDIPlusPayload = New Dictionary(Of Date, Decimal)
                    outputDIPlusPayload.Add(runningInputPayload.Key, DIPlus)
                    If outputDIMinusPayload Is Nothing Then outputDIMinusPayload = New Dictionary(Of Date, Decimal)
                    outputDIMinusPayload.Add(runningInputPayload.Key, DIMinus)

                    If dxPayload Is Nothing Then dxPayload = New Dictionary(Of Date, Decimal)
                    dxPayload.Add(runningInputPayload.Key, DX)

                    previousNInputFieldPayload = Nothing
                    previousNInputFieldPayload = Common.GetSubPayload(dxPayload, runningInputPayload.Key, period, True)
                    If previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count > 0 Then
                        If counter = (period + smoothingPeriod) Then
                            ADX = previousNInputFieldPayload.Average(Function(a)
                                                                         Return a.Value
                                                                     End Function)
                        ElseIf counter > (period + smoothingPeriod) Then
                            ADX = ((outputADXPayload(runningInputPayload.Value.PreviousCandlePayload.PayloadDate) * (period - 1)) + DX) / period
                        Else
                            ADX = 0
                        End If
                    End If
                    If outputADXPayload Is Nothing Then outputADXPayload = New Dictionary(Of Date, Decimal)
                    outputADXPayload.Add(runningInputPayload.Key, ADX)
                Next
            End If
        End Sub
    End Module
End Namespace