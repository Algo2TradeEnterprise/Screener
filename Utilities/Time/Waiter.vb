Imports System.Threading
Imports NLog
Imports Utilities.Network

Public Class Waiter
    Implements IDisposable
#Region "Logging and Status Progress"
    Public Shared logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Events"
    Public Event Heartbeat(ByVal msg As String)
    Public Event WaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
    'The below functions are needed to allow the derived classes to raise the above two events
    Protected Overridable Sub OnHeartbeat(ByVal msg As String)
        RaiseEvent Heartbeat(msg)
    End Sub
    Protected Overridable Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        RaiseEvent WaitingFor(elapsedSecs, totalSecs, msg)
    End Sub
#End Region

#Region "Constructors"
    Public Sub New(ByVal canceller As CancellationTokenSource)
        _canceller = canceller
    End Sub
#End Region

#Region "Private Attributes"
    Protected _canceller As CancellationTokenSource
#End Region

#Region "Public Methods"
    Public Sub SleepRequiredDuration(ByVal secondsToWait As Long, ByVal msg As String)
        logger.Debug("Sleeping the required duration")
        For sleepCtr As Integer = 1 To secondsToWait
            'Format the message with "..." if they dont exist
            If msg IsNot Nothing AndAlso Not msg.EndsWith("...") Then
                msg = String.Format("{0}...", msg)
            End If
            OnWaitingFor(sleepCtr, secondsToWait, msg)
            _canceller.Token.ThrowIfCancellationRequested()
            Thread.Sleep(1000)
            _canceller.Token.ThrowIfCancellationRequested()
        Next
        GC.Collect()
    End Sub
    Public Function WaitOnInternetFailure(ByVal waitDuration As TimeSpan) As Boolean
        logger.Debug("Waiting on internet failure")
        Dim ret As Boolean = False
        OnHeartbeat("Checking internet availability")
        While Not HttpBrowser.IsNetworkAvailableAsync(_canceller)
            GC.AddMemoryPressure(1024 * 1024)
            GC.Collect()
            ret = True
            _canceller.Token.ThrowIfCancellationRequested()
            SleepRequiredDuration(waitDuration.TotalSeconds, "Internet connection issue")
            _canceller.Token.ThrowIfCancellationRequested()
            OnHeartbeat("Checking internet availability")
        End While
        Return ret
    End Function
#End Region

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
