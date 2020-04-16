Namespace Indicator
    Public Module CentralPivotRange
        Public Sub CalculateCPR(ByVal eodPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, PivotRange))
            If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
                For Each runningInputPayload In eodPayload
                    Dim pivotRangeData As PivotRange = Nothing
                    If runningInputPayload.Value.PreviousCandlePayload IsNot Nothing Then
                        Dim prevHigh As Decimal = runningInputPayload.Value.PreviousCandlePayload.High
                        Dim prevLow As Decimal = runningInputPayload.Value.PreviousCandlePayload.Low
                        Dim prevClose As Decimal = runningInputPayload.Value.PreviousCandlePayload.Close

                        pivotRangeData = New PivotRange
                        pivotRangeData.Pivot = (prevHigh + prevLow + prevClose) / 3
                        pivotRangeData.BottomCentralPivot = (prevHigh + prevLow) / 2
                        pivotRangeData.TopCentralPivot = (pivotRangeData.Pivot - pivotRangeData.BottomCentralPivot) + pivotRangeData.Pivot
                    End If
                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, PivotRange)
                    outputPayload.Add(runningInputPayload.Key, pivotRangeData)
                Next
            End If
        End Sub
    End Module
End Namespace
