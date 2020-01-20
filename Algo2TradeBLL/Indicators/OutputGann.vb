Namespace Indicator
    Public Class OutputGann

        Private _BuyAt As Decimal
        Public Property BuyAt As Decimal
            Get
                Return Math.Round(_BuyAt, 4)
            End Get
            Set(value As Decimal)
                _BuyAt = value
            End Set
        End Property

        Private _SellAt As Decimal
        Public Property SellAt As Decimal
            Get
                Return Math.Round(_SellAt, 4)
            End Get
            Set(value As Decimal)
                _SellAt = value
            End Set
        End Property

        Public BuyTargets As List(Of Decimal)
        Public SellTargets As List(Of Decimal)
    End Class
End Namespace