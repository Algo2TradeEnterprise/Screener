Imports Algo2TradeBLL
Imports System.Threading
Public MustInherit Class StockSelection
    Implements IDisposable

#Region "Events/Event handlers"
    Public Event DocumentDownloadComplete()
    Public Event DocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
    Public Event Heartbeat(ByVal msg As String)
    Public Event WaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
    'The below functions are needed to allow the derived classes to raise the above two events
    Protected Overridable Sub OnDocumentDownloadComplete()
        RaiseEvent DocumentDownloadComplete()
    End Sub
    Protected Overridable Sub OnDocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
        RaiseEvent DocumentRetryStatus(currentTry, totalTries)
    End Sub
    Protected Overridable Sub OnHeartbeat(ByVal msg As String)
        RaiseEvent Heartbeat(msg)
    End Sub
    Protected Overridable Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        RaiseEvent WaitingFor(elapsedSecs, totalSecs, msg)
    End Sub
#End Region

    Protected ReadOnly _canceller As CancellationTokenSource
    Protected ReadOnly _cmn As Common
    Protected ReadOnly _intradayTable As Common.DataBaseTable
    Protected ReadOnly _eodTable As Common.DataBaseTable
    Protected _bannedStockFileName As String

    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer)
        _canceller = canceller
        _cmn = cmn

        Select Case stockType
            Case 0
                _intradayTable = Common.DataBaseTable.Intraday_Cash
                _eodTable = Common.DataBaseTable.EOD_Cash
            Case 1
                _intradayTable = Common.DataBaseTable.Intraday_Commodity
                _eodTable = Common.DataBaseTable.EOD_Commodity
            Case 2
                _intradayTable = Common.DataBaseTable.Intraday_Currency
                _eodTable = Common.DataBaseTable.EOD_Currency
            Case 3
                _intradayTable = Common.DataBaseTable.Intraday_Futures
                _eodTable = Common.DataBaseTable.EOD_Futures
        End Select
    End Sub

    Public MustOverride Async Function GetStockDataAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)

    Protected Function CalculateBuffer(ByVal price As Decimal, ByVal floorOrCeiling As Utilities.Numbers.RoundOfType) As Decimal
        Dim bufferPrice As Decimal = Nothing
        'Assuming 1% target, we can afford to have buffer as 2.5% of that 1% target
        bufferPrice = Utilities.Numbers.NumberManipulation.ConvertFloorCeling(price * 0.01 * 0.025, 0.05, floorOrCeiling)
        Return bufferPrice
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
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
