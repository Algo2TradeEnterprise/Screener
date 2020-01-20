Namespace Indicator
    Public Module Gann
        Public Sub calculateGann(ByVal open As Decimal, ByRef outputGann As OutputGann)

            Dim sqrt_open As Decimal = Math.Sqrt(open)
            Dim c4 As Decimal = Math.Floor(sqrt_open)
            Dim b4 As Decimal = c4 - 1
            Dim f4 As Decimal = Nothing
            If Int(sqrt_open) = sqrt_open Then
                f4 = sqrt_open + 1
            Else
                f4 = Math.Ceiling(sqrt_open)
            End If
            Dim g4 As Decimal = f4 + 1
            Const j1 As Decimal = 0.125
            Const j2 As Decimal = 0.25
            Const j3 As Decimal = 0.375
            Const j4 As Decimal = 0.5
            Const j5 As Decimal = 0.625
            Const j6 As Decimal = 0.75
            Const j7 As Decimal = 0.875
            Const j8 As Decimal = 1

            Dim gann(7, 7) As Decimal
            gann(3, 3) = Math.Pow(b4, 2)
            gann(2, 3) = Math.Pow((b4 + j3), 2)
            gann(1, 3) = Math.Pow((c4 + j3), 2)
            gann(0, 3) = Math.Pow((f4 + j3), 2)
            gann(4, 3) = Math.Pow((b4 + j7), 2)
            gann(5, 3) = Math.Pow((c4 + j7), 2)
            gann(6, 3) = Math.Pow((f4 + j7), 2)
            gann(3, 2) = Math.Pow((b4 + j1), 2)
            gann(3, 1) = Math.Pow((c4 + j1), 2)
            gann(3, 0) = Math.Pow((f4 + j1), 2)
            gann(3, 4) = Math.Pow((b4 + j5), 2)
            gann(3, 5) = Math.Pow((c4 + j5), 2)
            gann(3, 6) = Math.Pow((f4 + j5), 2)
            gann(2, 2) = Math.Pow((b4 + j2), 2)
            gann(1, 1) = Math.Pow((c4 + j2), 2)
            gann(0, 0) = Math.Pow((f4 + j2), 2)
            gann(2, 4) = Math.Pow((b4 + j4), 2)
            gann(1, 5) = Math.Pow((c4 + j4), 2)
            gann(0, 6) = Math.Pow((f4 + j4), 2)
            gann(4, 2) = Math.Pow((b4 + j8), 2)
            gann(5, 1) = Math.Pow((c4 + j8), 2)
            gann(6, 0) = Math.Pow((f4 + j8), 2)
            gann(4, 4) = Math.Pow((b4 + j6), 2)
            gann(5, 5) = Math.Pow((c4 + j6), 2)
            gann(6, 6) = Math.Pow((f4 + j6), 2)
            If open >= gann(0, 0) And open < gann(0, 3) Then
                gann(0, 1) = open
            Else
                gann(0, 1) = Nothing
            End If
            If open >= gann(0, 3) And open < gann(0, 6) Then
                gann(0, 4) = open
            Else
                gann(0, 4) = Nothing
            End If
            If open >= gann(1, 1) And open < gann(1, 3) Then
                gann(1, 2) = open
            Else
                gann(1, 2) = Nothing
            End If
            If open >= gann(1, 3) And open < gann(1, 5) Then
                gann(1, 4) = open
            Else
                gann(1, 4) = Nothing
            End If
            If open >= gann(0, 6) And open < gann(3, 6) Then
                gann(1, 6) = open
            Else
                gann(1, 6) = Nothing
            End If
            If open >= gann(6, 0) And open < gann(0, 0) Then
                gann(2, 0) = open
            Else
                gann(2, 0) = Nothing
            End If
            If open >= gann(3, 1) And open < gann(1, 1) Then
                gann(2, 1) = open
            Else
                gann(2, 1) = Nothing
            End If
            If open >= gann(1, 5) And open < gann(3, 5) Then
                gann(2, 5) = open
            Else
                gann(2, 5) = Nothing
            End If
            If open >= gann(4, 2) And open < gann(3, 1) Then
                gann(4, 1) = open
            Else
                gann(4, 1) = Nothing
            End If
            If open >= gann(3, 5) And open < gann(5, 5) Then
                gann(4, 5) = open
            Else
                gann(4, 5) = Nothing
            End If
            If open >= gann(3, 6) And open < gann(6, 6) Then
                gann(4, 6) = open
            Else
                gann(4, 6) = Nothing
            End If
            If open >= gann(5, 1) And open < gann(3, 0) Then
                gann(5, 0) = open
            Else
                gann(5, 0) = Nothing
            End If
            If open >= gann(5, 3) And open < gann(5, 1) Then
                gann(5, 2) = open
            Else
                gann(5, 2) = Nothing
            End If
            If open >= gann(5, 5) And open < gann(5, 3) Then
                gann(5, 4) = open
            Else
                gann(5, 4) = Nothing
            End If
            If open >= gann(6, 3) And open < gann(6, 0) Then
                gann(6, 2) = open
            Else
                gann(6, 2) = Nothing
            End If
            If open >= gann(6, 6) And open < gann(6, 3) Then
                gann(6, 5) = open
            Else
                gann(6, 5) = Nothing
            End If

            'If gann(4, 2) <> Nothing Then
            '    outputGann.BuyAt = gann(3, 1)
            'Else

            '    If gann(2, 1) <> Nothing Then
            '        outputGann.BuyAt = gann(1, 1)
            '    Else

            '    End If
            'End If
            Dim open_row, open_column As Integer
            For row As Integer = 0 To 6
                Dim flag As Integer = 0
                If row <> 3 Then
                    For column As Integer = 0 To 6
                        If column = 7 - row - 1 Then
                            'Do Nothing
                        Else
                            If column = row Then
                                'Do Nothing
                            Else
                                If column = 3 Then
                                    'Do Nothing
                                Else
                                    If open = gann(row, column) Then
                                        open_row = row
                                        open_column = column
                                        flag = 1
                                        Exit For
                                    End If
                                End If
                            End If
                        End If
                    Next
                    If flag = 1 Then
                        Exit For
                    End If
                End If
            Next
            Dim buyat As Decimal = Decimal.MaxValue
            Dim sellat As Decimal = Decimal.MinValue
            Dim i_end As Integer = open_row - 1 + 2
            Dim j_end As Integer = open_column - 1 + 2
            For i As Integer = open_row - 1 To i_end
                For j As Integer = open_column - 1 To j_end
                    If i = open_row And j = open_column Then
                        'Do Nothing
                    Else
                        If gann(i, j) > gann(open_row, open_column) And gann(i, j) < buyat Then
                            buyat = gann(i, j)
                        End If
                        If gann(i, j) <= gann(open_row, open_column) And gann(i, j) > sellat Then
                            sellat = gann(i, j)
                        End If
                    End If
                Next
            Next
            outputGann = New OutputGann
            outputGann.BuyAt = Math.Round(buyat, 4)
            outputGann.SellAt = Math.Round(sellat, 4)

            Dim sort_gann As New List(Of Decimal)

            For r As Integer = 0 To 6
                For c As Integer = 0 To 6
                    sort_gann.Add(gann(r, c))
                Next
            Next
            sort_gann.Sort()
            Dim position As Integer = Nothing
            For s As Integer = 0 To sort_gann.Count - 1
                If open = sort_gann(s) Then
                    position = s
                    Exit For
                End If
            Next
            If sort_gann(position + 1) = open Then
                position = position + 1
            End If

            outputGann.BuyTargets = New List(Of Decimal)
            outputGann.SellTargets = New List(Of Decimal)

            For resistance As Integer = position + 2 To position + 6
                outputGann.BuyTargets.Add(Math.Round((sort_gann(resistance) * 0.9995), 4))
            Next
            For support As Integer = position - 2 To position - 6 Step -1
                outputGann.SellTargets.Add(Math.Round((sort_gann(support) * 1.0005), 4))
            Next

        End Sub
    End Module
End Namespace
