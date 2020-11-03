Public Class ActiveInstrumentData
    Public Property Token As Integer
    Public Property TradingSymbol As String
    Public Property Expiry As Date
    Public Property LastDayOpen As Decimal
    Public Property LastDayLow As Decimal
    Public Property LastDayHigh As Decimal
    Public Property LastDayClose As Decimal
    Public Property CurrentDayOpen As Decimal
    Public Property CurrentDayLow As Decimal
    Public Property CurrentDayHigh As Decimal
    Public Property CurrentDayClose As Decimal
    Public ReadOnly Property RawInstrumentName As String
        Get
            If TradingSymbol.Contains("FUT") Then
                Return Me.TradingSymbol.Remove(Me.TradingSymbol.Count - 8)
            Else
                Return TradingSymbol
            End If
        End Get
    End Property
    Public Property CashInstrumentName As String
    Public Property CashInstrumentToken As String
End Class