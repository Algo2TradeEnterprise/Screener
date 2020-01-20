Namespace Indicator
    Public Module HeikenAshi
        Public Sub ConvertToHeikenAshi(ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of Date, Payload))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If inputPayload.Count < 30 Then
                    Throw New ApplicationException("Can't Calculate Heikenshi Properly")
                End If

                Dim tempHAPayload As Payload = Nothing
                Dim tempPreHAPayload As Payload = Nothing

                For Each runningInputPayload In inputPayload

                    tempHAPayload = New Payload(Payload.CandleDataSource.Chart)
                    tempHAPayload.PreviousCandlePayload = tempPreHAPayload
                    If tempPreHAPayload Is Nothing Then
                        tempHAPayload.Open = (runningInputPayload.Value.Open + runningInputPayload.Value.Close) / 2
                    Else
                        tempHAPayload.Open = (tempPreHAPayload.Open + tempPreHAPayload.Close) / 2
                    End If
                    tempHAPayload.Close = (runningInputPayload.Value.Open + runningInputPayload.Value.Close + runningInputPayload.Value.High + runningInputPayload.Value.Low) / 4
                    tempHAPayload.High = Math.Max(runningInputPayload.Value.High, Math.Max(tempHAPayload.Open, tempHAPayload.Close))
                    tempHAPayload.Low = Math.Min(runningInputPayload.Value.Low, Math.Min(tempHAPayload.Open, tempHAPayload.Close))
                    tempHAPayload.Volume = runningInputPayload.Value.Volume
                    tempHAPayload.CumulativeVolume = runningInputPayload.Value.CumulativeVolume
                    tempHAPayload.PayloadDate = runningInputPayload.Value.PayloadDate
                    tempHAPayload.TradingSymbol = runningInputPayload.Value.TradingSymbol
                    tempPreHAPayload = tempHAPayload
                    If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Payload)
                    outputPayload.Add(runningInputPayload.Key, tempHAPayload)
                Next
            End If
        End Sub
    End Module
End Namespace
