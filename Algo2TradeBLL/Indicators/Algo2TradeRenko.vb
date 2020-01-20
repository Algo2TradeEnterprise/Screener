Namespace Indicator
    Public Module Algo2TradeRenko
        Public Sub ConvertToRenko(ByVal range As Decimal, ByVal inputPayload As Dictionary(Of Date, Payload), ByRef outputPayload As Dictionary(Of String, Payload))
            If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
                Dim open As Decimal = 0
                Dim close As Decimal = 0
                For Each runningInputPayload In inputPayload
                    If runningInputPayload.Value.PreviousCandlePayload Is Nothing Then
                        open = runningInputPayload.Value.Open
                        close = open
                    End If
                    For Each tick In runningInputPayload.Value.Ticks
                        close = tick.Open
                        If Math.Abs(open - close) >= range Then
                            Dim count As Integer = Math.Floor(Math.Abs(open - close) / range)
                            For i As Integer = 1 To count
                                Dim modifiedClose As Decimal = 0
                                If open > close Then
                                    modifiedClose = open - range
                                Else
                                    modifiedClose = open + range
                                End If
                                Dim tempPreRenkoPayload As Payload = outputPayload?.LastOrDefault.Value
                                If tempPreRenkoPayload IsNot Nothing AndAlso
                                    (modifiedClose = tempPreRenkoPayload.Open OrElse modifiedClose = tempPreRenkoPayload.Close) Then
                                    open = modifiedClose
                                    Continue For
                                End If
                                Dim tempRenkoPayload As Payload = New Payload(Payload.CandleDataSource.Chart)
                                With tempRenkoPayload
                                    .PreviousCandlePayload = tempPreRenkoPayload
                                    .Open = open
                                    .Close = modifiedClose
                                    .Low = If(.Open < .Close, .Open, .Close)
                                    .High = If(.Open > .Close, .Open, .Close)
                                    .PayloadDate = runningInputPayload.Value.PayloadDate
                                    .TradingSymbol = runningInputPayload.Value.TradingSymbol
                                End With

                                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of String, Payload)
                                outputPayload.Add(String.Format("{0}_{1}", tick.PayloadDate, i), tempRenkoPayload)
                                open = modifiedClose
                            Next
                        End If
                    Next
                Next
            End If
        End Sub
    End Module
End Namespace
