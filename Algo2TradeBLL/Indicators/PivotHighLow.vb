Imports System.Drawing

Namespace Indicator
    Public Module PivotHighLow
        Public Class Pivot
            Public Property PivotHigh As Decimal
            Public Property PivotHighTime As Date
            Public Property PivotLow As Decimal
            Public Property PivotLowTime As Date
        End Class

        Public Sub CalculatePivotHighLow(ByVal period As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Pivot))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If inputPayload.Count <= period * 2 + 1 Then
                    Throw New ApplicationException("Can not calculate pivot high low")
                End If
                For Each runningPayload In inputPayload.Keys
                    Dim pivotData As Pivot = Nothing

                    Dim previousNInputPayload As List(Of KeyValuePair(Of Date, Payload)) = Common.GetSubPayload(inputPayload, runningPayload, period, True)
                    If previousNInputPayload IsNot Nothing AndAlso previousNInputPayload.Count = period Then
                        Dim highestHigh As Decimal = previousNInputPayload.Max(Function(x)
                                                                                   Return x.Value.High
                                                                               End Function)
                        Dim lowestLow As Decimal = previousNInputPayload.Min(Function(x)
                                                                                 Return x.Value.Low
                                                                             End Function)

                        Dim lastCandleTime As Date = previousNInputPayload.Min(Function(x)
                                                                                   Return x.Key
                                                                               End Function)

                        Dim pivotCandle As Payload = inputPayload(lastCandleTime).PreviousCandlePayload
                        If pivotCandle IsNot Nothing Then
                            Dim prePreviousNInputPayload As List(Of KeyValuePair(Of Date, Payload)) = Common.GetSubPayload(inputPayload, pivotCandle.PayloadDate, period, False)
                            If prePreviousNInputPayload IsNot Nothing AndAlso prePreviousNInputPayload.Count = period Then
                                Dim preHighestHigh As Decimal = prePreviousNInputPayload.Max(Function(x)
                                                                                                 Return x.Value.High
                                                                                             End Function)
                                Dim preLowestLow As Decimal = prePreviousNInputPayload.Min(Function(x)
                                                                                               Return x.Value.Low
                                                                                           End Function)

                                If pivotCandle.High > highestHigh AndAlso pivotCandle.High > preHighestHigh Then
                                    If pivotData Is Nothing Then pivotData = New Pivot
                                    pivotData.PivotHigh = pivotCandle.High
                                    pivotData.PivotHighTime = pivotCandle.PayloadDate
                                Else
                                    If outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate) IsNot Nothing Then
                                        If pivotData Is Nothing Then pivotData = New Pivot
                                        pivotData.PivotHigh = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotHigh
                                        pivotData.PivotHighTime = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotHighTime
                                    End If
                                End If
                                If pivotCandle.Low < lowestLow AndAlso pivotCandle.Low < preLowestLow Then
                                    If pivotData Is Nothing Then pivotData = New Pivot
                                    pivotData.PivotLow = pivotCandle.Low
                                    pivotData.PivotLowTime = pivotCandle.PayloadDate
                                Else
                                    If outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate) IsNot Nothing Then
                                        If pivotData Is Nothing Then pivotData = New Pivot
                                        pivotData.PivotLow = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotLow
                                        pivotData.PivotLowTime = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotLowTime
                                    End If
                                End If
                            Else
                                If outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate) IsNot Nothing Then
                                    pivotData = New Pivot With {
                                        .PivotHigh = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotHigh,
                                        .PivotHighTime = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotHighTime,
                                        .PivotLow = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotLow,
                                        .PivotLowTime = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotLowTime
                                    }
                                End If
                            End If
                        Else
                            If outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate) IsNot Nothing Then
                                pivotData = New Pivot With {
                                    .PivotHigh = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotHigh,
                                    .PivotHighTime = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotHighTime,
                                    .PivotLow = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotLow,
                                    .PivotLowTime = outputPayload(inputPayload(runningPayload).PreviousCandlePayload.PayloadDate).PivotLowTime
                                }
                            End If
                        End If
                    End If

                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Pivot)
                    outputPayload.Add(runningPayload, pivotData)
                Next
            End If
        End Sub

        Public Sub CalculatePivotHighLowTrend(ByVal period As Integer, ByVal trendPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputHighPayload As Dictionary(Of Date, Decimal), ByRef outputLowPayload As Dictionary(Of Date, Decimal), ByRef outputTrendPayload As Dictionary(Of Date, Color))
            Dim pivotPayload As Dictionary(Of Date, Pivot) = Nothing
            CalculatePivotHighLow(period, inputPayload, pivotPayload)

            Dim trend As Color = Color.White
            For Each runningPayload In inputPayload.Keys
                Dim highTrend As Decimal = 0
                Dim lowTrend As Decimal = 0


                Dim lastPivotHighTime As Date = Date.MinValue
                Dim lastPivotLowTime As Date = Date.MinValue
                Dim highCount As Integer = 0
                Dim lowCount As Integer = 0
                Dim highSum As Decimal = 0
                Dim lowSum As Decimal = 0
                Dim previousPayloads As IEnumerable(Of KeyValuePair(Of Date, Payload)) = inputPayload.Where(Function(x)
                                                                                                                Return x.Key <= runningPayload
                                                                                                            End Function)
                If previousPayloads IsNot Nothing AndAlso previousPayloads.Count > 0 Then
                    For Each innerPayload In previousPayloads.OrderByDescending(Function(x)
                                                                                    Return x.Key
                                                                                End Function)
                        If pivotPayload.ContainsKey(innerPayload.Key) AndAlso pivotPayload(innerPayload.Key) IsNot Nothing Then
                            If highCount < trendPeriod AndAlso pivotPayload(innerPayload.Key).PivotHighTime <> Date.MinValue AndAlso pivotPayload(innerPayload.Key).PivotHighTime <> lastPivotHighTime Then
                                lastPivotHighTime = pivotPayload(innerPayload.Key).PivotHighTime
                                highCount += 1
                                highSum += (inputPayload(runningPayload).Close - pivotPayload(innerPayload.Key).PivotHigh) / pivotPayload(innerPayload.Key).PivotHigh
                            End If
                            If lowCount < trendPeriod AndAlso pivotPayload(innerPayload.Key).PivotLowTime <> Date.MinValue AndAlso pivotPayload(innerPayload.Key).PivotLowTime <> lastPivotLowTime Then
                                lastPivotLowTime = pivotPayload(innerPayload.Key).PivotLowTime
                                lowCount += 1
                                lowSum += (inputPayload(runningPayload).Close - pivotPayload(innerPayload.Key).PivotLow) / pivotPayload(innerPayload.Key).PivotLow
                            End If
                        End If
                        If highCount >= trendPeriod AndAlso lowCount >= trendPeriod Then
                            Exit For
                        End If
                    Next
                End If
                highTrend = highSum / trendPeriod
                lowTrend = lowSum / trendPeriod

                If outputHighPayload Is Nothing Then outputHighPayload = New Dictionary(Of Date, Decimal)
                outputHighPayload.Add(runningPayload, highTrend)
                If outputLowPayload Is Nothing Then outputLowPayload = New Dictionary(Of Date, Decimal)
                outputLowPayload.Add(runningPayload, lowTrend)
                If highTrend > 0 AndAlso lowTrend > 0 Then
                    trend = Color.Green
                ElseIf highTrend < 0 AndAlso lowTrend < 0 Then
                    trend = Color.Red
                End If
                If outputTrendPayload Is Nothing Then outputTrendPayload = New Dictionary(Of Date, Color)
                outputTrendPayload.Add(runningPayload, trend)
            Next
        End Sub
    End Module
End Namespace