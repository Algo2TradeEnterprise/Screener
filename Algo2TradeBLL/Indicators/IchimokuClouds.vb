Namespace Indicator
    Public Module IchimokuClouds
        Public Sub CalculateIchimokuClouds(ByVal conversionLinePeriod As Integer,
                                          ByVal baseLinePeriod As Integer,
                                          ByVal leadingSpanBPeriod As Integer,
                                          ByVal lagggingSpanPeriod As Integer,
                                          ByVal inputPayload As Dictionary(Of Date, Payload),
                                          ByRef conversionLinePayload As Dictionary(Of Date, Decimal),
                                          ByRef baseLinePayload As Dictionary(Of Date, Decimal),
                                          ByRef leadingSpanAPayload As Dictionary(Of Date, Decimal),
                                          ByRef leadingSpanBPayload As Dictionary(Of Date, Decimal),
                                          ByRef laggingSpanPayload As Dictionary(Of Date, Decimal))

            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim candleCounter As Integer = 0
                For Each runningPayload In inputPayload.Keys
                    candleCounter += 1
                    Dim conversionLine As Decimal = 0
                    Dim baseLine As Decimal = 0
                    Dim leadingSpanA As Decimal = 0
                    Dim leadingSpanATime As Date = runningPayload
                    Dim leadingSpanB As Decimal = 0
                    Dim leadingSpanBTime As Date = runningPayload
                    Dim laggingSpan As Decimal = 0
                    Dim laggingSpanTime As Date = runningPayload

                    If candleCounter >= baseLinePeriod Then
                        'Conversion Line Calculation
                        Dim maxHigh As Decimal = Nothing
                        Dim minLow As Decimal = Nothing
                        Dim previousNPayload As List(Of KeyValuePair(Of Date, Payload)) = Common.GetSubPayload(inputPayload, runningPayload, conversionLinePeriod, True)
                        If previousNPayload IsNot Nothing AndAlso previousNPayload.Count = conversionLinePeriod Then
                            maxHigh = previousNPayload.Max(Function(x)
                                                               Return x.Value.High
                                                           End Function)
                            minLow = previousNPayload.Min(Function(x)
                                                              Return x.Value.Low
                                                          End Function)
                            conversionLine = (maxHigh + minLow) / 2
                        End If

                        'Base Line Calculation
                        previousNPayload = Common.GetSubPayload(inputPayload, runningPayload, baseLinePeriod, True)
                        If previousNPayload IsNot Nothing AndAlso previousNPayload.Count = baseLinePeriod Then
                            maxHigh = previousNPayload.Max(Function(x)
                                                               Return x.Value.High
                                                           End Function)
                            minLow = previousNPayload.Min(Function(x)
                                                              Return x.Value.Low
                                                          End Function)
                            baseLine = (maxHigh + minLow) / 2
                        End If

                        'Leading Span A calculation
                        Dim previousBaseLineData As List(Of KeyValuePair(Of Date, Decimal)) = Common.GetSubPayload(baseLinePayload, runningPayload, baseLinePeriod, False)
                        If previousBaseLineData IsNot Nothing AndAlso previousBaseLineData.Count = baseLinePeriod Then
                            leadingSpanATime = previousBaseLineData.FirstOrDefault.Key
                            leadingSpanA = (baseLinePayload(leadingSpanATime) + conversionLinePayload(leadingSpanATime)) / 2
                        End If

                        'Leading Span B Calculation
                        Dim previousLeadingSpanBPayload As List(Of KeyValuePair(Of Date, Payload)) = Common.GetSubPayload(inputPayload, runningPayload, baseLinePeriod - 1, False)
                        If previousLeadingSpanBPayload IsNot Nothing AndAlso previousLeadingSpanBPayload.Count >= baseLinePeriod - 1 Then
                            leadingSpanBTime = previousLeadingSpanBPayload.FirstOrDefault.Key
                            previousNPayload = Common.GetSubPayload(inputPayload, leadingSpanBTime, leadingSpanBPeriod, True)
                            If previousNPayload IsNot Nothing AndAlso previousNPayload.Count = leadingSpanBPeriod Then
                                maxHigh = previousNPayload.Max(Function(x)
                                                                   Return x.Value.High
                                                               End Function)
                                minLow = previousNPayload.Min(Function(x)
                                                                  Return x.Value.Low
                                                              End Function)
                                leadingSpanB = (maxHigh + minLow) / 2
                            End If
                        End If
                    End If

                    'Lagging Span Calculation
                    Dim previousNInputPayload As List(Of KeyValuePair(Of Date, Payload)) = Common.GetSubPayload(inputPayload, runningPayload, lagggingSpanPeriod, False)
                    If previousNInputPayload IsNot Nothing AndAlso previousNInputPayload.Count = lagggingSpanPeriod Then
                        laggingSpan = inputPayload(runningPayload).Close
                        laggingSpanTime = previousNInputPayload.FirstOrDefault.Key
                    End If

                    If conversionLinePayload Is Nothing Then conversionLinePayload = New Dictionary(Of Date, Decimal)
                    conversionLinePayload.Add(runningPayload, conversionLine)
                    If baseLinePayload Is Nothing Then baseLinePayload = New Dictionary(Of Date, Decimal)
                    baseLinePayload.Add(runningPayload, baseLine)
                    If leadingSpanAPayload Is Nothing Then leadingSpanAPayload = New Dictionary(Of Date, Decimal)
                    leadingSpanAPayload.Add(runningPayload, leadingSpanA)
                    If leadingSpanBPayload Is Nothing Then leadingSpanBPayload = New Dictionary(Of Date, Decimal)
                    leadingSpanBPayload.Add(runningPayload, leadingSpanB)
                    If laggingSpanPayload Is Nothing Then laggingSpanPayload = New Dictionary(Of Date, Decimal)
                    laggingSpanPayload(laggingSpanTime) = laggingSpan
                Next
            End If
        End Sub
    End Module
End Namespace