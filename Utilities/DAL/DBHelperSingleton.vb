Imports System.Threading
Imports NLog

Namespace DAL
    Public MustInherit Class DBHelperSingleton
        Implements IDisposable
#Region "Logging and Status Progress"
        Public Shared logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Events"
        Public Event DocumentDownloadComplete()
        Public Event DocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
        Public Event WaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        Public Event Heartbeat(ByVal msg As String)
        'The below functions are needed to allow the derived classes to raise the above two events
        Protected Overridable Sub OnDocumentDownloadComplete()
            RaiseEvent DocumentDownloadComplete()
        End Sub
        Protected Overridable Sub OnDocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
            RaiseEvent DocumentRetryStatus(currentTry, totalTries)
        End Sub
        Protected Overridable Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
            RaiseEvent WaitingFor(elapsedSecs, totalSecs, msg)
        End Sub
        Protected Overridable Sub OnHeartbeat(ByVal msg As String)
            RaiseEvent Heartbeat(msg)
        End Sub
#End Region

#Region "Constructors"
#End Region

#Region "Private Attributes"
        Protected Shared _DBConnectSingleton As DBHelperSingleton
        Protected _serverName As String
        Protected _dbName As String
        Protected _port As Integer
        Protected _userID As String
        Protected _password As String
        Protected _connectionString As String
        Protected _canceller As CancellationTokenSource
#End Region

#Region "Public Attributes"
        Public Property WaitDurationOnConnectionFailure As TimeSpan = TimeSpan.FromSeconds(5)
        Public Property WaitDurationOnAnyFailure As TimeSpan = TimeSpan.FromSeconds(10)
        Public Property MaxReTries As Integer = 20
#End Region

#Region "Private Methods"
#End Region

#Region "Public Methods"
        Public MustOverride Function IsConnected() As Boolean
        Public MustOverride Sub Close()
        Public MustOverride Function RunUpdate(ByVal stmtUpdate As String) As Integer
        Public MustOverride Function RunSelect(ByVal stmtSelect As String) As DataTable
        Public MustOverride Function GetIdentityFromLastInsert() As ULong
        Public MustOverride Function GetIdentityFromLastInsert(ByVal tableName As String) As ULong
        Public MustOverride Function RunSelectSingleValue(ByVal stmtSelect As String) As Object
        Public Sub ResetCanceller(ByVal newCanceller As CancellationTokenSource)
            logger.Debug("Resetting canceller")
            Me._canceller = newCanceller
        End Sub
#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    _serverName = Nothing
                    _dbName = Nothing
                    _port = Nothing
                    _userID = Nothing
                    _password = Nothing
                    _connectionString = Nothing
                    WaitDurationOnConnectionFailure = Nothing
                    WaitDurationOnAnyFailure = Nothing
                    MaxReTries = Nothing
                    If _DBConnectSingleton IsNot Nothing Then
                        _DBConnectSingleton.Close()
                        _DBConnectSingleton.Dispose()
                    End If
                    _DBConnectSingleton = Nothing
                End If
                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        Protected Overrides Sub Finalize()
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(False)
            MyBase.Finalize()
        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace