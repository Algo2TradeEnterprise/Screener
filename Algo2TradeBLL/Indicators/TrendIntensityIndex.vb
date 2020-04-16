Imports System.Threading
Namespace Indicator
    Public Module TrendIntensityIndex
        Public Sub CalculateTII(ByVal field As Payload.PayloadFields, ByVal period As Integer, ByVal signalPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef TIIPayload As Dictionary(Of Date, Decimal), ByRef signalLinePayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                If inputPayload.Count < period * 2 Then Throw New ApplicationException("Can't Calculate TII")
                Dim smaPayload As Dictionary(Of Date, Decimal) = Nothing
                Indicator.SMA.CalculateSMA(period, field, inputPayload, smaPayload)
                Dim posDevPayload As Dictionary(Of Date, Decimal) = Nothing
                Dim negDevPayload As Dictionary(Of Date, Decimal) = Nothing
                Dim halfPeriod As Integer = If(period Mod 2 = 0, period / 2, (period + 1) / 2)
                For Each runningPayload In inputPayload.Keys
                    Dim dev As Decimal = inputPayload(runningPayload).Close - smaPayload(runningPayload)
                    Dim posDev As Decimal = If(dev >= 0, dev, 0)
                    Dim negDev As Decimal = If(dev < 0, Math.Abs(dev), 0)
                    If posDevPayload Is Nothing Then posDevPayload = New Dictionary(Of Date, Decimal)
                    posDevPayload.Add(runningPayload, posDev)
                    If negDevPayload Is Nothing Then negDevPayload = New Dictionary(Of Date, Decimal)
                    negDevPayload.Add(runningPayload, negDev)
                    Dim previousNInputFieldPosDev As List(Of KeyValuePair(Of DateTime, Decimal)) = Common.GetSubPayload(posDevPayload, runningPayload, halfPeriod, True)
                    Dim previousNInputFieldNegDev As List(Of KeyValuePair(Of DateTime, Decimal)) = Common.GetSubPayload(negDevPayload, runningPayload, halfPeriod, True)
                    Dim sdPos As Decimal = 0
                    Dim sdNeg As Decimal = 0
                    If previousNInputFieldPosDev IsNot Nothing AndAlso previousNInputFieldNegDev IsNot Nothing AndAlso
                       previousNInputFieldPosDev.Count >= halfPeriod AndAlso previousNInputFieldNegDev.Count >= halfPeriod Then
                        sdPos = previousNInputFieldPosDev.Sum(Function(s) s.Value)
                        sdNeg = previousNInputFieldNegDev.Sum(Function(s) s.Value)
                    End If
                    Dim tii As Decimal = 0
                    If sdPos = 0 AndAlso sdNeg = 0 Then
                        tii = 0
                    Else
                        tii = 100 * sdPos / (sdPos + sdNeg)
                    End If
                    If TIIPayload Is Nothing Then TIIPayload = New Dictionary(Of Date, Decimal)
                    TIIPayload.Add(runningPayload, Math.Round(tii, 4))
                Next
                Dim emaInputPayload As Dictionary(Of Date, Payload) = Nothing
                Common.ConvertDecimalToPayload(Payload.PayloadFields.Additional_Field, TIIPayload, emaInputPayload)
                Indicator.EMA.CalculateEMA(signalPeriod, Payload.PayloadFields.Additional_Field, emaInputPayload, signalLinePayload)
            End If
        End Sub
    End Module
End Namespace
