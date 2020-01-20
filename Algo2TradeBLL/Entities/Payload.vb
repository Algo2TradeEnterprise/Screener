Imports System.Drawing
Public Class Payload
    Implements IDisposable

    Private _TradingSymbol As String
    Public Property TradingSymbol As String
        Get
            Return _TradingSymbol
        End Get
        Set(value As String)
            _TradingSymbol = value
        End Set
    End Property

    Private _Open As Decimal
    Public Property Open As Decimal
        Get
            Return _Open
        End Get
        Set(value As Decimal)
            _Open = value
        End Set
    End Property
    Private _High As Decimal
    Public Property High As Decimal
        Get
            Return _High
        End Get
        Set(value As Decimal)
            _High = value
        End Set
    End Property
    Private _Low As Decimal
    Public Property Low As Decimal
        Get
            Return _Low
        End Get
        Set(value As Decimal)
            _Low = value
        End Set
    End Property
    Private _Close As Decimal
    Public Property Close As Decimal
        Get
            Return _Close
        End Get
        Set(value As Decimal)
            _Close = value
        End Set
    End Property

    Private _H_L As Decimal
    Public Property H_L As Decimal
        Get
            Return Math.Round(_H_L, 4)
        End Get
        Set(value As Decimal)
            _H_L = value
        End Set
    End Property

    Private _C_AVG_HL As Decimal
    Public Property C_AVG_HL As Decimal
        Get
            Return Math.Round(_C_AVG_HL, 4)
        End Get
        Set(value As Decimal)
            _C_AVG_HL = value
        End Set
    End Property

    Private _SMI_EMA As Double
    Public Property SMI_EMA As Double
        Get
            Return Math.Round(_SMI_EMA, 4)
        End Get
        Set(value As Double)
            _SMI_EMA = value
        End Set
    End Property

    Private _Additional_Field As Double
    Public Property Additional_Field As Double
        Get
            Return Math.Round(_Additional_Field, 4)
        End Get
        Set(value As Double)
            _Additional_Field = value
        End Set
    End Property

    Private _Volume As Double
    Public Property Volume As Double
        Get
            If _Volume = Double.MinValue Then
                If PreviousCandlePayload IsNot Nothing Then
                    If PreviousCandlePayload.PayloadDate.Date <> Me.PayloadDate.Date Then
                        _Volume = CumulativeVolume
                    Else
                        _Volume = CumulativeVolume - PreviousCandlePayload.CumulativeVolume
                    End If
                ElseIf PreviousCandlePayload Is Nothing Then
                    _Volume = CumulativeVolume
                End If
            End If
            Return _Volume
        End Get
        Set(value As Double)
            _Volume = value
        End Set
    End Property
    Public Property PayloadDate As Date

    Private _CandleColor As Color
    Public ReadOnly Property CandleColor As Color
        Get
            If Close > Open Then
                _CandleColor = Color.Green
            ElseIf Close < Open Then
                _CandleColor = Color.Red
            Else
                _CandleColor = Color.White
            End If
            Return _CandleColor
        End Get
    End Property

    Private _VolumeColor As Color
    Public ReadOnly Property VolumeColor As Color
        Get
            If PreviousCandlePayload IsNot Nothing Then
                If Me.Close >= Me.PreviousCandlePayload.Close Then
                    _VolumeColor = Color.Green
                ElseIf Me.Close < Me.PreviousCandlePayload.Close Then
                    _VolumeColor = Color.Red
                Else
                    _VolumeColor = Color.White
                End If
            Else
                _VolumeColor = Color.White
            End If
            Return _VolumeColor
        End Get
    End Property
    Public Property PreviousCandlePayload As Payload
    Public Property PayloadSource As CandleDataSource

    Private _CandleStrengthNormal As StrongCandle
    Public ReadOnly Property CandleStrengthNormal As StrongCandle
        Get
            If Me.CandleBody / Me.CandleRange >= 0.7 Then
                If Me.CandleColor = Color.Green Then
                    _CandleStrengthNormal = StrongCandle.Bullish
                ElseIf Me.CandleColor = Color.Red Then
                    _CandleStrengthNormal = StrongCandle.Bearish
                Else
                    _CandleStrengthNormal = StrongCandle.None
                End If
            End If
            Return _CandleStrengthNormal
        End Get
    End Property

    Private _CandleStrengthHeikenAshi As StrongCandle
    Public ReadOnly Property CandleStrengthHeikenAshi As StrongCandle
        Get
            If Me.CandleColor = Color.Green Then
                If Math.Round(Me.Open, 2) = Math.Round(Me.Low, 2) Then
                    _CandleStrengthHeikenAshi = StrongCandle.Bullish
                End If
            ElseIf Me.CandleColor = Color.Red Then
                If Math.Round(Me.Open, 2) = Math.Round(Me.High, 2) Then
                    _CandleStrengthHeikenAshi = StrongCandle.Bearish
                End If
            End If
            Return _CandleStrengthHeikenAshi
        End Get
    End Property

    Private _CandleWicksPercentage As Wicks
    Public ReadOnly Property CandleWicksPercentage As Wicks
        Get
            If _CandleWicksPercentage Is Nothing Then
                Dim dummy = CandleWicks
            End If
            Return _CandleWicksPercentage
        End Get
    End Property

    Private _CandleWicks As Wicks
    Public ReadOnly Property CandleWicks As Wicks
        Get
            If _CandleWicks Is Nothing Then
                _CandleWicks = New Wicks
                _CandleWicksPercentage = New Wicks
                If Me.CandleColor = System.Drawing.Color.Green Then
                    With _CandleWicks
                        .Top = Me.High - Me.Close
                        .Bottom = Me.Open - Me.Low
                        With _CandleWicksPercentage
                            .Top = (_CandleWicks.Top / Me.Close) * 100
                            .Bottom = (_CandleWicks.Bottom / Me.Open) * 100
                        End With
                    End With
                ElseIf Me.CandleColor = System.Drawing.Color.Red Then
                    With _CandleWicks
                        .Top = Me.High - Me.Open
                        .Bottom = Me.Close - Me.Low
                        With _CandleWicksPercentage
                            .Top = (_CandleWicks.Top / Me.Open) * 100
                            .Bottom = (_CandleWicks.Bottom / Me.Close) * 100
                        End With
                    End With
                Else
                    With _CandleWicks
                        .Top = Me.High - Me.Open
                        .Bottom = Me.Close - Me.Low
                        With _CandleWicksPercentage
                            .Top = (_CandleWicks.Top / Me.Open) * 100
                            .Bottom = (_CandleWicks.Bottom / Me.Close) * 100
                        End With
                    End With
                End If
            End If
            Return _CandleWicks
        End Get
    End Property

    Private _VolumeIndex As Double
    Public ReadOnly Property VolumeIndex As Double
        Get
            _VolumeIndex = Me.Volume / (Me.High - Me.Low)
            Return _VolumeIndex
        End Get
    End Property

    Private _CandleRange As Double
    Public ReadOnly Property CandleRange As Double
        Get
            _CandleRange = Me.High - Me.Low
            Return _CandleRange
        End Get
    End Property

    Private _CandleRangePercentage As Double
    Public ReadOnly Property CandleRangePercentage As Double
        Get
            _CandleRangePercentage = Me.CandleRange * 100 / Me.Close
            Return _CandleRangePercentage
        End Get
    End Property

    Private _CandleBody As Double
    Public ReadOnly Property CandleBody As Double
        Get
            _CandleBody = Math.Abs(Me.Open - Me.Close)
            Return _CandleBody
        End Get
    End Property

    Private _DojiCandle As Double
    Public ReadOnly Property DojiCandle As Boolean
        Get
            If Me.CandleBody < Me.CandleRange / 4 Then
                _DojiCandle = True
            Else
                _DojiCandle = False
            End If
            Return _DojiCandle
        End Get
    End Property

    Private _DeadCandle As Double
    Public ReadOnly Property DeadCandle As Boolean
        Get
            If Me.CandleBody = 0 AndAlso Me.High = Me.Low Then
                _DeadCandle = True
            Else
                _DeadCandle = False
            End If
            Return _DeadCandle
        End Get
    End Property
    Private _IsMaribazu As Double
    Public ReadOnly Property IsMaribazu As Boolean
        Get
            If Me.CandleBody <> 0 AndAlso CandleWicks.Bottom = 0 AndAlso CandleWicks.Top = 0 Then
                _IsMaribazu = True
            Else
                _IsMaribazu = False
            End If
            Return _IsMaribazu
        End Get
    End Property
    Private _Ticks As List(Of Payload)
    Public ReadOnly Property Ticks As List(Of Payload)
        Get
            If _Ticks Is Nothing OrElse _Ticks.Count = 0 Then
                Dim multiplier As Short = 0
                Dim startPrice As Decimal = Me.Open
                Dim firstWick As Decimal = If(Me.CandleColor = Color.Red, Me.High, Me.Low)
                Dim secondWick As Decimal = If(Me.CandleColor = Color.Red, Me.Low, Me.High)
                Dim endPrice As Decimal = Me.Close
                Dim totalTicksToBeCreated As Integer = If(Me.CandleColor = Color.Red, ((Me.High - Me.Open) + (Me.High - Me.Low) + (Me.Close - Me.Low)) / 0.05,
                                                                                      ((Me.Open - Me.Low) + (Me.High - Me.Low) + (Me.High - Me.Close)) / 0.05)
                totalTicksToBeCreated += 3
                Dim ticksPerSecond As Double = totalTicksToBeCreated / 60

                If _Ticks Is Nothing Then _Ticks = New List(Of Payload)
                multiplier = If(Me.CandleColor = Color.Red, 1, -1)
                Dim runningTickCtr As Integer = 0
                Dim runningPayLoadDate As Date = Nothing
                For runningTick As Decimal = startPrice To firstWick Step 0.05 * multiplier
                    runningTickCtr += 1
                    'runningPayLoadDate = New Date(Me.PayloadDate.Year, Me.PayloadDate.Month, Me.PayloadDate.Day, Me.PayloadDate.Hour, Me.PayloadDate.Minute, Math.Round(runningTickCtr / ticksPerSecond, 0))
                    runningPayLoadDate = New Date(Me.PayloadDate.Year, Me.PayloadDate.Month, Me.PayloadDate.Day, Me.PayloadDate.Hour, Me.PayloadDate.Minute, If(Math.Round(runningTickCtr / ticksPerSecond, 0) >= 60, 59, Math.Round(runningTickCtr / ticksPerSecond, 0)))
                    _Ticks.Add(New Payload(CandleDataSource.Calculated) With {.TradingSymbol = Me.TradingSymbol, .Open = runningTick, .Low = runningTick, .High = runningTick, .Close = runningTick, .PayloadDate = runningPayLoadDate})
                Next
                multiplier = If(Me.CandleColor = Color.Red, -1, 1)
                For runningTick As Decimal = firstWick To secondWick Step 0.05 * multiplier
                    runningTickCtr += 1
                    Try
                        runningPayLoadDate = New Date(Me.PayloadDate.Year, Me.PayloadDate.Month, Me.PayloadDate.Day, Me.PayloadDate.Hour, Me.PayloadDate.Minute, If(Math.Round(runningTickCtr / ticksPerSecond, 0) >= 60, 59, Math.Round(runningTickCtr / ticksPerSecond, 0)))
                    Catch ex As Exception
                        Throw ex
                    End Try
                    _Ticks.Add(New Payload(CandleDataSource.Calculated) With {.TradingSymbol = Me.TradingSymbol, .Open = runningTick, .Low = runningTick, .High = runningTick, .Close = runningTick, .PayloadDate = runningPayLoadDate})
                Next
                multiplier = If(Me.CandleColor = Color.Red, 1, -1)
                For runningTick As Decimal = secondWick To endPrice Step 0.05 * multiplier
                    runningTickCtr += 1
                    Try
                        runningPayLoadDate = New Date(Me.PayloadDate.Year, Me.PayloadDate.Month, Me.PayloadDate.Day, Me.PayloadDate.Hour, Me.PayloadDate.Minute, If(Math.Round(runningTickCtr / ticksPerSecond, 0) >= 60, 59, Math.Round(runningTickCtr / ticksPerSecond, 0)))
                    Catch ex As Exception
                        Throw ex
                    End Try
                    _Ticks.Add(New Payload(CandleDataSource.Calculated) With {.TradingSymbol = Me.TradingSymbol, .Open = runningTick, .Low = runningTick, .High = runningTick, .Close = runningTick, .PayloadDate = runningPayLoadDate})
                Next
            End If
            Return _Ticks
        End Get
    End Property

    Public Sub New(ByVal payloadSource As CandleDataSource)
        Me.PayloadSource = payloadSource
        Me.Volume = Double.MinValue
    End Sub

    Public Property CumulativeVolume As Long
    Public Enum CandleDataSource
        Chart = 1
        Tick
        Calculated
    End Enum
    Public Enum PayloadFields
        Open = 1
        High
        Low
        Close
        Volume
        H_L
        C_AVG_HL
        SMI_EMA
        Additional_Field
    End Enum
    Public Enum StrongCandle
        Bullish = 1
        Bearish
        None
    End Enum

    Public Class Wicks
        Public Property Top As Double
        Public Property Bottom As Double
    End Class
#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Open = Nothing
                High = Nothing
                Low = Nothing
                Close = Nothing
                Volume = Nothing
                PayloadDate = Nothing
                _CandleColor = Nothing
                PreviousCandlePayload = Nothing
                PayloadSource = Nothing
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class


