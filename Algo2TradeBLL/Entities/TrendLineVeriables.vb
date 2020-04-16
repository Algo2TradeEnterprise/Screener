Public Class TrendLineVeriables
    Public M As Decimal = Decimal.MinValue
    Public C As Decimal = Decimal.MinValue
    Public X As Decimal = Decimal.MinValue
    Public Point1 As Date
    Public Point2 As Date
    Public ReadOnly Property CurrentValue
        Get
            If Me.M <> Decimal.MinValue AndAlso Me.C <> Decimal.MinValue AndAlso Me.X <> Decimal.MinValue Then
                Return ((Me.M * Me.X) + Me.C)
            Else
                Return Decimal.MinValue
            End If
        End Get
    End Property
End Class
