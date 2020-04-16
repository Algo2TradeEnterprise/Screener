Imports System.Threading

Namespace Indicator
    Public Module MACD
        Dim cts As CancellationTokenSource
        Dim cmn As Common = New Common(cts)
        Public Sub CalculateMACD(ByVal fastMAPeriod As Integer, ByVal slowMAPeriod As Integer, ByVal signalPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef MACDPayload As Dictionary(Of Date, Decimal), ByRef signalPayload As Dictionary(Of Date, Decimal), ByRef histogramPayload As Dictionary(Of Date, Decimal))
            Dim fastEMA As Dictionary(Of Date, Decimal) = Nothing
            Dim slowEMA As Dictionary(Of Date, Decimal) = Nothing
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If inputPayload.Count < 100 Then
                    Throw New ApplicationException("Can't Calculate MACD")
                End If
                EMA.CalculateEMA(fastMAPeriod, Payload.PayloadFields.Close, inputPayload, fastEMA)
                EMA.CalculateEMA(slowMAPeriod, Payload.PayloadFields.Close, inputPayload, slowEMA)
                MACDPayload = New Dictionary(Of Date, Decimal)
                For Each runninginputpayload In inputPayload.Keys
                    MACDPayload.Add(runninginputpayload, (fastEMA(runninginputpayload) - slowEMA(runninginputpayload)))
                Next
                Dim tempMACDPayload As Dictionary(Of Date, Payload) = Nothing
                Common.ConvertDecimalToPayload(Payload.PayloadFields.Additional_Field, MACDPayload, tempMACDPayload)
                EMA.CalculateEMA(signalPeriod, Payload.PayloadFields.Additional_Field, tempMACDPayload, signalPayload)
                histogramPayload = New Dictionary(Of Date, Decimal)
                For Each runninginputpayload In inputPayload.Keys
                    histogramPayload.Add(runninginputpayload, (MACDPayload(runninginputpayload) - signalPayload(runninginputpayload)))
                Next
            End If
        End Sub
    End Module
End Namespace
