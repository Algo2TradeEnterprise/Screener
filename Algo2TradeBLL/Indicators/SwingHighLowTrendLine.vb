Namespace Indicator
    Public Module SwingHighLowTrendLine
        Public Sub CalculateSwingHighLowTrendLine(ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputHighPayload As Dictionary(Of Date, TrendLineVeriables), ByRef outputLowPayload As Dictionary(Of Date, TrendLineVeriables))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim swingHighPayload As Dictionary(Of Date, Decimal) = Nothing
                Dim swingLowPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SwingHighLow.CalculateSwingHighLow(inputPayload, False, swingHighPayload, swingLowPayload)
                For Each runningPayload In inputPayload
                    Dim highLine As TrendLineVeriables = New TrendLineVeriables
                    Dim lowLine As TrendLineVeriables = New TrendLineVeriables

                    Dim lastHighUCandle As Payload = GetSwingFormingCandle(inputPayload, swingHighPayload, runningPayload.Key, 1)
                    If lastHighUCandle IsNot Nothing Then
                        Dim firstHighUCandle As Payload = lastHighUCandle
                        While firstHighUCandle.High <= lastHighUCandle.High
                            firstHighUCandle = GetSwingFormingCandle(inputPayload, swingHighPayload, firstHighUCandle.PayloadDate, 1)
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

                    Dim lastLowUCandle As Payload = GetSwingFormingCandle(inputPayload, swingLowPayload, runningPayload.Key, -1)
                    If lastLowUCandle IsNot Nothing Then
                        Dim firstLowUCandle As Payload = lastLowUCandle
                        While firstLowUCandle.Low >= lastLowUCandle.Low
                            firstLowUCandle = GetSwingFormingCandle(inputPayload, swingLowPayload, firstLowUCandle.PayloadDate, -1)
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

        Private Function GetSwingFormingCandle(ByVal inputPayload As Dictionary(Of Date, Payload), ByVal swingPayload As Dictionary(Of Date, Decimal), ByVal beforeThisTime As Date, ByVal direction As Integer) As Payload
            Dim ret As Payload = Nothing
            If swingPayload IsNot Nothing AndAlso swingPayload.Count > 0 Then
                Dim checkingPayload As IEnumerable(Of KeyValuePair(Of Date, Payload)) = inputPayload.Where(Function(x)
                                                                                                               Return x.Key < beforeThisTime
                                                                                                           End Function)
                If checkingPayload IsNot Nothing AndAlso checkingPayload.Count > 0 Then
                    For Each runningPayload In checkingPayload.OrderByDescending(Function(x)
                                                                                     Return x.Key
                                                                                 End Function)
                        If direction > 0 Then
                            If runningPayload.Value.High = swingPayload(beforeThisTime) Then
                                ret = runningPayload.Value
                                Exit For
                            End If
                        ElseIf direction < 0 Then
                            If runningPayload.Value.Low = swingPayload(beforeThisTime) Then
                                ret = runningPayload.Value
                                Exit For
                            End If
                        End If
                    Next
                End If
            End If
            Return ret
        End Function
    End Module
End Namespace