Namespace Calculator
    Public Class BrokerageAttributes
        Private _Buy As Decimal
        Public Property Buy As Decimal
            Get
                Return Math.Round(_Buy, 4)
            End Get
            Set(value As Decimal)
                _Buy = value
            End Set
        End Property

        Private _Sell As Decimal
        Public Property Sell As Decimal
            Get
                Return Math.Round(_Sell, 4)
            End Get
            Set(value As Decimal)
                _Sell = value
            End Set
        End Property

        Private _Quantity As Decimal
        Public Property Quantity As Decimal
            Get
                Return Math.Round(_Quantity, 2)
            End Get
            Set(value As Decimal)
                _Quantity = value
            End Set
        End Property

        Private _Multiplier As Decimal
        Public Property Multiplier As Decimal
            Get
                Return _Multiplier
            End Get
            Set(value As Decimal)
                _Multiplier = value
            End Set
        End Property

        Private _Turnover As Decimal
        Public Property Turnover As Decimal
            Get
                Return Math.Round(_Turnover, 2)
            End Get
            Set(value As Decimal)
                _Turnover = value
            End Set
        End Property

        Private _Brokerage As Decimal
        Public Property Brokerage As Decimal
            Get
                Return Math.Round(_Brokerage, 2)
            End Get
            Set(value As Decimal)
                _Brokerage = value
            End Set
        End Property

        Private _STT As Integer
        Public Property STT As Integer
            Get
                Return Math.Round(_STT, 2)
            End Get
            Set(value As Integer)
                _STT = value
            End Set
        End Property

        Private _CTT As Decimal
        Public Property CTT As Decimal
            Get
                Return Math.Round(_CTT, 2)
            End Get
            Set(value As Decimal)
                _CTT = value
            End Set
        End Property

        Private _Exchange As Decimal
        Public Property Exchange As Decimal
            Get
                Return Math.Round(_Exchange, 2)
            End Get
            Set(value As Decimal)
                _Exchange = value
            End Set
        End Property

        Private _Clearing As Decimal
        Public Property Clearing As Decimal
            Get
                Return Math.Round(_Clearing, 2)
            End Get
            Set(value As Decimal)
                _Clearing = value
            End Set
        End Property

        Private _GST As Decimal
        Public Property GST As Decimal
            Get
                Return Math.Round(_GST, 2)
            End Get
            Set(value As Decimal)
                _GST = value
            End Set
        End Property

        Private _SEBI As Decimal
        Public Property SEBI As Decimal
            Get
                _SEBI = 0.000001 * _Turnover
                Return Math.Round(_SEBI, 2)
            End Get
            Set(value As Decimal)
                _SEBI = value
            End Set
        End Property

        Private _TotalTax As Decimal
        Public Property TotalTax As Decimal
            Get
                Return Math.Round(_TotalTax, 2)
            End Get
            Set(value As Decimal)
                _TotalTax = value
            End Set
        End Property

        Private _BreakevenPoints As Decimal
        Public Property BreakevenPoints As Decimal
            Get
                _BreakevenPoints = _TotalTax / (_Quantity * _Multiplier)
                Return Math.Round(_BreakevenPoints, 2)
            End Get
            Set(value As Decimal)
                _BreakevenPoints = value
            End Set
        End Property

        Private _NetProfitLoss As Decimal
        Public Property NetProfitLoss As Decimal
            Get
                _NetProfitLoss = ((_Sell - _Buy) * _Quantity * _Multiplier) - _TotalTax
                Return Math.Round(_NetProfitLoss, 2)
            End Get
            Set(value As Decimal)
                _NetProfitLoss = value
            End Set
        End Property
    End Class
End Namespace