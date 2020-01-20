
Namespace Indicator
    Public Module VWAP
        Public Sub CalculateVWAP(ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim finalPriceToBeAdded As Decimal = 0

                Dim prevDate As Date = Date.MinValue
                Dim avgPrice As Decimal = Decimal.MinValue
                Dim avgPriceStarVolume As Decimal = Decimal.MinValue
                Dim cumAvgPriceStarVolume As Decimal = 0
                For Each runningInputPayload In inputPayload
                    If runningInputPayload.Key.Date <> prevDate Then
                        prevDate = runningInputPayload.Key.Date
                        avgPrice = Decimal.MinValue
                        avgPriceStarVolume = Decimal.MinValue
                        cumAvgPriceStarVolume = 0
                    End If
                    avgPrice = (runningInputPayload.Value.High + runningInputPayload.Value.Low + runningInputPayload.Value.Close) / 3
                    avgPriceStarVolume = avgPrice * runningInputPayload.Value.Volume
                    cumAvgPriceStarVolume += avgPriceStarVolume
                    finalPriceToBeAdded = cumAvgPriceStarVolume / runningInputPayload.Value.CumulativeVolume
                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Decimal)
                    outputPayload.Add(runningInputPayload.Key, finalPriceToBeAdded)
                Next
            End If
        End Sub

    End Module
End Namespace