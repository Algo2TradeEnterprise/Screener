Imports System.Threading
Namespace Indicator
    Public Module DonchianChannel
        ReadOnly cts As New CancellationTokenSource
        Public Sub CalculateDonchianChannel(ByVal highPeriod As Integer, ByVal lowPeriod As Integer,
                                            ByVal inputPayload As Dictionary(Of Date, Payload),
                                            ByRef outputHighPayload As Dictionary(Of Date, Decimal),
                                            ByRef outputLowPayload As Dictionary(Of Date, Decimal),
                                            ByRef outputMiddlePayload As Dictionary(Of Date, Decimal))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim greaterPeriod As Integer = If(highPeriod > lowPeriod, highPeriod, lowPeriod)
                If inputPayload.Count < greaterPeriod Then
                    Throw New ApplicationException("Can't Calculate Donchian Channel")
                End If
                For Each runningPayload In inputPayload.Keys
                    Dim highDonchian As Decimal = Nothing
                    Dim lowDonchian As Decimal = Nothing
                    Dim middleDonchian As Decimal = Nothing
                    Dim previousNInputHighFieldPayload As List(Of KeyValuePair(Of DateTime, Payload)) = Common.GetSubPayload(inputPayload, runningPayload, highPeriod, False)
                    If previousNInputHighFieldPayload Is Nothing Then
                        highDonchian = 0
                    ElseIf previousNInputHighFieldPayload IsNot Nothing AndAlso previousNInputHighFieldPayload.Count < highPeriod Then
                        highDonchian = 0
                    Else
                        highDonchian = previousNInputHighFieldPayload.Max(Function(x) x.Value.High)
                    End If
                    Dim previousNInputLowFieldPayload As List(Of KeyValuePair(Of DateTime, Payload)) = Common.GetSubPayload(inputPayload, runningPayload, lowPeriod, False)
                    If previousNInputLowFieldPayload Is Nothing Then
                        lowDonchian = 0
                    ElseIf previousNInputLowFieldPayload IsNot Nothing AndAlso previousNInputLowFieldPayload.Count < lowPeriod Then
                        lowDonchian = 0
                    Else
                        lowDonchian = previousNInputLowFieldPayload.Min(Function(x) x.Value.Low)
                    End If
                    middleDonchian = (highDonchian + lowDonchian) / 2
                    If outputHighPayload Is Nothing Then outputHighPayload = New Dictionary(Of Date, Decimal)
                    outputHighPayload.Add(runningPayload, highDonchian)
                    If outputLowPayload Is Nothing Then outputLowPayload = New Dictionary(Of Date, Decimal)
                    outputLowPayload.Add(runningPayload, lowDonchian)
                    If outputMiddlePayload Is Nothing Then outputMiddlePayload = New Dictionary(Of Date, Decimal)
                    outputMiddlePayload.Add(runningPayload, middleDonchian)
                Next
            End If
        End Sub
    End Module
End Namespace
