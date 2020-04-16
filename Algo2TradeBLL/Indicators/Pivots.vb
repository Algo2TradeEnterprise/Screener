Namespace Indicator
    Public Module Pivots
        Public Sub CalculatePivots(ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, PivotPoints))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim isFirstCandleOfTheDay As Boolean = False
                Dim pivotPointsData As PivotPoints = Nothing
                For Each runningInputPayload In inputPayload
                    If runningInputPayload.Value.PreviousCandlePayload IsNot Nothing AndAlso
                        runningInputPayload.Value.PayloadDate.Date <> runningInputPayload.Value.PreviousCandlePayload.PayloadDate.Date Then
                        isFirstCandleOfTheDay = True
                        pivotPointsData = New PivotPoints
                    End If
                    If isFirstCandleOfTheDay Then
                        Dim previousDay As Date = runningInputPayload.Value.PreviousCandlePayload.PayloadDate.Date
                        Dim previousDayPayloads As IEnumerable(Of KeyValuePair(Of Date, Payload)) = inputPayload.Where(Function(x)
                                                                                                                           Return x.Key.Date = previousDay.Date
                                                                                                                       End Function)
                        Dim prevHigh As Decimal = previousDayPayloads.Max(Function(x)
                                                                              Return x.Value.High
                                                                          End Function)
                        Dim prevLow As Decimal = previousDayPayloads.Min(Function(x)
                                                                             Return x.Value.Low
                                                                         End Function)
                        Dim prevClose As Decimal = previousDayPayloads.OrderBy(Function(x)
                                                                                   Return x.Key
                                                                               End Function).LastOrDefault.Value.Close
                        pivotPointsData.Pivot = (prevHigh + prevLow + prevClose) / 3
                        pivotPointsData.Support1 = (2 * pivotPointsData.Pivot) - prevHigh
                        pivotPointsData.Resistance1 = (2 * pivotPointsData.Pivot) - prevLow
                        pivotPointsData.Support2 = pivotPointsData.Pivot - (prevHigh - prevLow)
                        pivotPointsData.Resistance2 = pivotPointsData.Pivot + (prevHigh - prevLow)
                        pivotPointsData.Support3 = pivotPointsData.Support2 - (prevHigh - prevLow)
                        pivotPointsData.Resistance3 = pivotPointsData.Resistance2 + (prevHigh - prevLow)
                    End If
                    isFirstCandleOfTheDay = False
                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, PivotPoints)
                    outputPayload.Add(runningInputPayload.Key, pivotPointsData)
                Next
            End If
        End Sub
    End Module
End Namespace
