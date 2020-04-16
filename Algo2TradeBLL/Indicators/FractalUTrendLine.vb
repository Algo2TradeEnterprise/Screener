Namespace Indicator
    Public Module FractalUTrendLine
        Public Sub CalculateFractalUTrendLine(ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputHighPayload As Dictionary(Of Date, TrendLineVeriables), ByRef outputLowPayload As Dictionary(Of Date, TrendLineVeriables), ByRef fractalHighPayload As Dictionary(Of Date, Decimal), ByRef fractalLowPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Indicator.FractalBands.CalculateFractal(inputPayload, fractalHighPayload, fractalLowPayload)
                For Each runningPayload In inputPayload
                    Dim highLine As TrendLineVeriables = New TrendLineVeriables
                    Dim lowLine As TrendLineVeriables = New TrendLineVeriables

                    Dim lastHighUCandle As Tuple(Of Payload, Date) = GetFractalUFormingCandleAndMiddleCandle(inputPayload, fractalHighPayload, runningPayload.Key, 1)
                    If lastHighUCandle IsNot Nothing AndAlso lastHighUCandle.Item1 IsNot Nothing AndAlso lastHighUCandle.Item1.PayloadDate.Date = runningPayload.Key.Date Then
                        Dim firstHighUCandle As Tuple(Of Payload, Date) = GetFractalUFormingCandleAndMiddleCandle(inputPayload, fractalHighPayload, lastHighUCandle.Item1.PayloadDate, 1)
                        If firstHighUCandle IsNot Nothing AndAlso firstHighUCandle.Item1 IsNot Nothing AndAlso firstHighUCandle.Item1.High <= lastHighUCandle.Item1.High Then
                            firstHighUCandle = Nothing
                        End If
                        If firstHighUCandle IsNot Nothing AndAlso firstHighUCandle.Item1 IsNot Nothing Then
                            Dim x1 As Decimal = 0
                            Dim y1 As Decimal = firstHighUCandle.Item1.High
                            Dim x2 As Decimal = inputPayload.Where(Function(x)
                                                                       Return x.Key > firstHighUCandle.Item1.PayloadDate AndAlso x.Key <= lastHighUCandle.Item2
                                                                   End Function).Count
                            Dim y2 As Decimal = fractalHighPayload(lastHighUCandle.Item2)

                            Dim trendLine As TrendLineVeriables = Common.GetEquationOfTrendLine(x1, y1, x2, y2)
                            If trendLine IsNot Nothing Then
                                highLine.M = trendLine.M
                                highLine.C = trendLine.C
                                highLine.X = inputPayload.Where(Function(x)
                                                                    Return x.Key > firstHighUCandle.Item1.PayloadDate AndAlso x.Key <= runningPayload.Value.PayloadDate
                                                                End Function).Count
                                highLine.Point1 = firstHighUCandle.Item1.PayloadDate
                                highLine.Point2 = lastHighUCandle.Item2
                            End If
                        Else
                            Dim previousHighLine As TrendLineVeriables = outputHighPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate)
                            If previousHighLine.M <> Decimal.MinValue Then
                                highLine.M = previousHighLine.M
                                highLine.C = previousHighLine.C
                                highLine.X = previousHighLine.X + 1
                                highLine.Point1 = previousHighLine.Point1
                                highLine.Point2 = previousHighLine.Point2
                            End If
                        End If
                    End If

                    Dim lastLowUCandle As Tuple(Of Payload, Date) = GetFractalUFormingCandleAndMiddleCandle(inputPayload, fractalLowPayload, runningPayload.Key, -1)
                    If lastLowUCandle IsNot Nothing AndAlso lastLowUCandle.Item1 IsNot Nothing AndAlso lastLowUCandle.Item1.PayloadDate.Date = runningPayload.Key.Date Then
                        Dim firstLowUCandle As Tuple(Of Payload, Date) = GetFractalUFormingCandleAndMiddleCandle(inputPayload, fractalLowPayload, lastLowUCandle.Item1.PayloadDate, -1)
                        If firstLowUCandle IsNot Nothing AndAlso firstLowUCandle.Item1 IsNot Nothing AndAlso firstLowUCandle.Item1.Low >= lastLowUCandle.Item1.Low Then
                            firstLowUCandle = Nothing
                        End If
                        If firstLowUCandle IsNot Nothing Then
                            Dim x1 As Decimal = 0
                            Dim y1 As Decimal = firstLowUCandle.Item1.Low
                            Dim x2 As Decimal = inputPayload.Where(Function(x)
                                                                       Return x.Key > firstLowUCandle.Item1.PayloadDate AndAlso x.Key <= lastLowUCandle.Item2
                                                                   End Function).Count
                            Dim y2 As Decimal = fractalLowPayload(lastLowUCandle.Item2)

                            Dim trendLine As TrendLineVeriables = Common.GetEquationOfTrendLine(x1, y1, x2, y2)
                            If trendLine IsNot Nothing Then
                                lowLine.M = trendLine.M
                                lowLine.C = trendLine.C
                                lowLine.X = inputPayload.Where(Function(x)
                                                                   Return x.Key > firstLowUCandle.Item1.PayloadDate AndAlso x.Key <= runningPayload.Value.PayloadDate
                                                               End Function).Count
                                lowLine.Point1 = firstLowUCandle.Item1.PayloadDate
                                lowLine.Point2 = lastLowUCandle.Item2
                            End If
                        Else
                            Dim previousLowLine As TrendLineVeriables = outputLowPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate)
                            If previousLowLine.M <> Decimal.MinValue Then
                                lowLine.M = previousLowLine.M
                                lowLine.C = previousLowLine.C
                                lowLine.X = previousLowLine.X + 1
                                lowLine.Point1 = previousLowLine.Point1
                                lowLine.Point2 = previousLowLine.Point2
                            End If
                        End If
                    End If

                    If outputHighPayload Is Nothing Then outputHighPayload = New Dictionary(Of Date, TrendLineVeriables)
                    outputHighPayload.Add(runningPayload.Key, highLine)
                    If outputLowPayload Is Nothing Then outputLowPayload = New Dictionary(Of Date, TrendLineVeriables)
                    outputLowPayload.Add(runningPayload.Key, lowLine)
                Next
            End If
        End Sub
        Private Function GetFractalUFormingCandleAndMiddleCandle(ByVal inputPayload As Dictionary(Of Date, Payload), ByVal fractalPayload As Dictionary(Of Date, Decimal), ByVal beforeThisTime As Date, ByVal direction As Integer) As Tuple(Of Payload, Date)
            Dim ret As Tuple(Of Payload, Date) = Nothing
            If fractalPayload IsNot Nothing AndAlso fractalPayload.Count > 0 Then
                Dim checkingPayload As IEnumerable(Of KeyValuePair(Of Date, Decimal)) = fractalPayload.Where(Function(x)
                                                                                                                 Return x.Key <= beforeThisTime
                                                                                                             End Function)
                If checkingPayload IsNot Nothing AndAlso checkingPayload.Count > 0 Then
                    Dim firstCandleTime As Date = Date.MinValue
                    Dim middleCandleTime As Date = Date.MinValue
                    Dim lastCandleTime As Date = Date.MinValue
                    For Each runningPayload In checkingPayload.OrderByDescending(Function(x)
                                                                                     Return x.Key
                                                                                 End Function)
                        If direction > 0 Then
                            If firstCandleTime = Date.MinValue Then
                                firstCandleTime = runningPayload.Key
                            Else
                                If middleCandleTime = Date.MinValue Then
                                    If fractalPayload(firstCandleTime) >= runningPayload.Value Then
                                        firstCandleTime = runningPayload.Key
                                    Else
                                        middleCandleTime = runningPayload.Key
                                    End If
                                Else
                                    If fractalPayload(middleCandleTime) < runningPayload.Value Then
                                        middleCandleTime = runningPayload.Key
                                    ElseIf fractalPayload(middleCandleTime) > runningPayload.Value Then
                                        lastCandleTime = runningPayload.Key
                                        Exit For
                                    End If
                                End If
                            End If
                        ElseIf direction < 0 Then
                            If firstCandleTime = Date.MinValue Then
                                firstCandleTime = runningPayload.Key
                            Else
                                If middleCandleTime = Date.MinValue Then
                                    If fractalPayload(firstCandleTime) <= runningPayload.Value Then
                                        firstCandleTime = runningPayload.Key
                                    Else
                                        middleCandleTime = runningPayload.Key
                                    End If
                                Else
                                    If fractalPayload(middleCandleTime) > runningPayload.Value Then
                                        middleCandleTime = runningPayload.Key
                                    ElseIf fractalPayload(middleCandleTime) < runningPayload.Value Then
                                        lastCandleTime = runningPayload.Key
                                        Exit For
                                    End If
                                End If
                            End If
                        End If
                    Next
                    If lastCandleTime <> Date.MinValue Then
                        For Each runningPayload In inputPayload.OrderByDescending(Function(x)
                                                                                      Return x.Key
                                                                                  End Function)
                            If runningPayload.Key < lastCandleTime Then
                                If direction > 0 Then
                                    If runningPayload.Value.High = fractalPayload(middleCandleTime) Then
                                        ret = New Tuple(Of Payload, Date)(runningPayload.Value, middleCandleTime)
                                        Exit For
                                    End If
                                ElseIf direction < 0 Then
                                    If runningPayload.Value.Low = fractalPayload(middleCandleTime) Then
                                        ret = New Tuple(Of Payload, Date)(runningPayload.Value, middleCandleTime)
                                        Exit For
                                    End If
                                End If
                            End If
                        Next
                    End If
                End If
            End If
            Return ret
        End Function
    End Module
End Namespace