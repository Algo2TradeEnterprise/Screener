Namespace Indicator
    Public Module FractalBandsTrendLine
        Public Sub CalculateFractalBandsTrendLine(ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputHighPayload As Dictionary(Of Date, TrendLineVeriables), ByRef outputLowPayload As Dictionary(Of Date, TrendLineVeriables))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim fractalHighPayload As Dictionary(Of Date, Decimal) = Nothing
                Dim fractalLowPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.FractalBands.CalculateFractal(inputPayload, fractalHighPayload, fractalLowPayload)
                For Each runningPayload In inputPayload
                    Dim highLine As TrendLineVeriables = New TrendLineVeriables
                    Dim lowLine As TrendLineVeriables = New TrendLineVeriables

                    Dim lastHighUCandle As Payload = GetFractalUFormingCandle(inputPayload, fractalHighPayload, runningPayload.Key, 1)
                    If lastHighUCandle IsNot Nothing Then
                        Dim firstHighUCandle As Payload = lastHighUCandle
                        While firstHighUCandle.High <= lastHighUCandle.High
                            firstHighUCandle = GetFractalUFormingCandle(inputPayload, fractalHighPayload, firstHighUCandle.PayloadDate, 1)
                            If firstHighUCandle Is Nothing Then Exit While
                        End While
                        If firstHighUCandle IsNot Nothing Then
                            Dim x1 As Decimal = 0
                            Dim y1 As Decimal = firstHighUCandle.High
                            Dim x2 As Decimal = inputPayload.Where(Function(x)
                                                                       Return x.Key > firstHighUCandle.PayloadDate AndAlso x.Key <= lastHighUCandle.PayloadDate
                                                                   End Function).Count
                            Dim y2 As Decimal = lastHighUCandle.High

                            Dim trendLine As TrendLineVeriables = Common.GetEquationOfTrendLine(x1, y1, x2, y2)
                            If trendLine IsNot Nothing Then
                                highLine.M = trendLine.M
                                highLine.C = trendLine.C
                                highLine.X = inputPayload.Where(Function(x)
                                                                    Return x.Key > firstHighUCandle.PayloadDate AndAlso x.Key <= runningPayload.Value.PayloadDate
                                                                End Function).Count
                            End If
                        Else
                            Dim previousHighLine As TrendLineVeriables = outputHighPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate)
                            If previousHighLine.M <> Decimal.MinValue Then
                                highLine.M = previousHighLine.M
                                highLine.C = previousHighLine.C
                                highLine.X = previousHighLine.X + 1
                            End If
                        End If
                    End If

                    Dim lastLowUCandle As Payload = GetFractalUFormingCandle(inputPayload, fractalLowPayload, runningPayload.Key, -1)
                    If lastLowUCandle IsNot Nothing Then
                        Dim firstLowUCandle As Payload = lastLowUCandle
                        While firstLowUCandle.Low >= lastLowUCandle.Low
                            firstLowUCandle = GetFractalUFormingCandle(inputPayload, fractalLowPayload, firstLowUCandle.PayloadDate, -1)
                            If firstLowUCandle Is Nothing Then Exit While
                        End While
                        If firstLowUCandle IsNot Nothing Then
                            Dim x1 As Decimal = 0
                            Dim y1 As Decimal = firstLowUCandle.Low
                            Dim x2 As Decimal = inputPayload.Where(Function(x)
                                                                       Return x.Key > firstLowUCandle.PayloadDate AndAlso x.Key <= lastLowUCandle.PayloadDate
                                                                   End Function).Count
                            Dim y2 As Decimal = lastLowUCandle.Low

                            Dim trendLine As TrendLineVeriables = Common.GetEquationOfTrendLine(x1, y1, x2, y2)
                            If trendLine IsNot Nothing Then
                                lowLine.M = trendLine.M
                                lowLine.C = trendLine.C
                                lowLine.X = inputPayload.Where(Function(x)
                                                                   Return x.Key > firstLowUCandle.PayloadDate AndAlso x.Key <= runningPayload.Value.PayloadDate
                                                               End Function).Count
                            End If
                        Else
                            Dim previousLowLine As TrendLineVeriables = outputLowPayload(runningPayload.Value.PreviousCandlePayload.PayloadDate)
                            If previousLowLine.M <> Decimal.MinValue Then
                                lowLine.M = previousLowLine.M
                                lowLine.C = previousLowLine.C
                                lowLine.X = previousLowLine.X + 1
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
        Private Function GetFractalUFormingCandle(ByVal inputPayload As Dictionary(Of Date, Payload), ByVal fractalPayload As Dictionary(Of Date, Decimal), ByVal beforeThisTime As Date, ByVal direction As Integer) As Payload
            Dim ret As Payload = Nothing
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
                                        ret = runningPayload.Value
                                        Exit For
                                    End If
                                ElseIf direction < 0 Then
                                    If runningPayload.Value.Low = fractalPayload(middleCandleTime) Then
                                        ret = runningPayload.Value
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