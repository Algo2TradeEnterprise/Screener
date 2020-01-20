Imports System.Text
Imports MySql.Data.MySqlClient
Imports System.Reflection
Imports NLog
Imports System.Threading
Imports Utilities.ErrorHandlers
Namespace DAL
    Public Class MySQLDBHelper
        Inherits DBHelper
        Implements IDisposable

        '************** Events and loggers are in the base class ***************
#Region "Constructor"
        Public Sub New(ByVal serverName As String,
                       ByVal dbName As String,
                       ByVal port As Integer,
                       ByVal userID As String,
                       ByVal password As String,
                       ByVal canceller As CancellationTokenSource)
            _serverName = serverName
            _dbName = dbName
            _port = port
            _userID = userID
            _password = password
            Me._canceller = canceller
            _connectionString = String.Format("Server={0};Database={1};Port={2};Uid={3};Pwd={4};default command timeout=180;Pooling=True;Min Pool Size=2;Max Pool Size=5;UseAffectedRows=false;Allow User Variables=True", _serverName, _dbName, _port, _userID, _password)
        End Sub
#End Region

#Region "Private Attributes"
#End Region

#Region "Public Attributes"
#End Region

#Region "Private Methods"
#End Region

#Region "Public Methods"
        Public Overrides Function IsConnected() As Boolean
            'Not implemented because this class creates a connection on every command execution
            'There is no global connection object in this class to check
            Throw New NotImplementedException
        End Function
        Public Overrides Sub Close()
            'Not implemented because this class creates a connection on every command execution
            'There is no global connection object in this class to close
            Throw New NotImplementedException
        End Sub

        Public Sub OpenConnection()
            logger.Debug("Opening connection")
            Dim allOKWithoutException As Boolean = False
            Dim lastException As Exception = Nothing
            Using waiter As New Waiter(_canceller)
                AddHandler waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler waiter.WaitingFor, AddressOf OnWaitingFor
                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    lastException = Nothing
                    allOKWithoutException = False
                    Using dbConn As New MySqlConnection(_connectionString)
                        Try
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", dbConn.ConnectionString))
                            OnDocumentRetryStatus(retryCtr, MaxReTries)
                            dbConn.Open()
                            _canceller.Token.ThrowIfCancellationRequested()
                            lastException = Nothing
                            allOKWithoutException = True
                            Exit For
                        Catch opx As OperationCanceledException
                            logger.Error(opx)
                            lastException = opx
                            If Not _canceller.Token.IsCancellationRequested Then
                                _canceller.Token.ThrowIfCancellationRequested()
                                If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                    'Provide required wait in case internet was already up
                                    logger.Debug("DB->Task cancelled without internet problem:{0}",
                                                 opx.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                Else
                                    logger.Debug("DB->Task cancelled with internet problem:{0}",
                                                 opx.Message)
                                    'Since internet was down, no need to consume retries
                                    retryCtr -= 1
                                End If
                            End If
                        Catch ex As Exception
                            logger.Error(ex)
                            lastException = ex
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                _canceller.Token.ThrowIfCancellationRequested()
                                If ExceptionExtensions.IsExceptionConnectionBusyRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type connection busy detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                ElseIf ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type internet related detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                Else
                                    logger.Debug("DB->Exception without internet problem of unknown type detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                End If
                            Else
                                logger.Debug("DB->Exception with internet problem:{0}",
                                             ex.Message)
                                'Since internet was down, no need to consume retries
                                retryCtr -= 1
                            End If
                        Finally
                            If dbConn IsNot Nothing Then dbConn.Close()
                            OnDocumentDownloadComplete()
                        End Try
                    End Using
                    _canceller.Token.ThrowIfCancellationRequested()
                Next
                _canceller.Token.ThrowIfCancellationRequested()
                RemoveHandler waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            If Not allOKWithoutException Then Throw lastException
        End Sub
        Public Overrides Async Function RunUpdateAsync(ByVal stmtUpdate As String) As Task(Of Integer)
            logger.Debug("Rnnning update statement asynchronously")
            Dim ret As Integer = Nothing
            Dim allOKWithoutException As Boolean = False
            Dim lastException As Exception = Nothing
            Using waiter As New Waiter(_canceller)
                AddHandler waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler waiter.WaitingFor, AddressOf OnWaitingFor
                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    ret = 0
                    lastException = Nothing
                    allOKWithoutException = False
                    Using dbConn As New MySqlConnection(_connectionString)
                        Try
                            'logger.Debug("Opening connection to DB using connection string: {0}", dbConn.ConnectionString)
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", dbConn.ConnectionString))
                            OnDocumentRetryStatus(retryCtr, MaxReTries)
                            dbConn.Open()
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat("Updating to DB")
                            Using cmd As New MySqlCommand(stmtUpdate, dbConn)
                                logger.Debug("Firing UPDATE/INSERT/DELETE statement:{0}", cmd.CommandText)
                                ret = Await cmd.ExecuteNonQueryAsync().ConfigureAwait(False)
                                _canceller.Token.ThrowIfCancellationRequested()
                                If ret = 0 Then
                                    logger.Warn("{0} {1} {0} did not insert/update/delete any records XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", vbNewLine, cmd.CommandText)
                                End If
                                lastException = Nothing
                                allOKWithoutException = True
                                Exit For
                            End Using
                            _canceller.Token.ThrowIfCancellationRequested()
                        Catch opx As OperationCanceledException
                            logger.Error(opx)
                            lastException = opx
                            If Not _canceller.Token.IsCancellationRequested Then
                                _canceller.Token.ThrowIfCancellationRequested()
                                If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                    'Provide required wait in case internet was already up
                                    logger.Debug("DB->Task cancelled without internet problem:{0}",
                                                 opx.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                Else
                                    logger.Debug("DB->Task cancelled with internet problem:{0}",
                                                 opx.Message)
                                    'Since internet was down, no need to consume retries
                                    retryCtr -= 1
                                End If
                            End If
                        Catch ex As Exception
                            logger.Error(ex)
                            lastException = ex
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                _canceller.Token.ThrowIfCancellationRequested()
                                If ExceptionExtensions.IsExceptionConnectionBusyRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type connection busy detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                ElseIf ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type internet related detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                Else
                                    logger.Debug("DB->Exception without internet problem of unknown type detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                End If
                            Else
                                logger.Debug("DB->Exception with internet problem:{0}",
                                             ex.Message)
                                'Since internet was down, no need to consume retries
                                retryCtr -= 1
                            End If
                        Finally
                            If dbConn IsNot Nothing Then dbConn.Close()
                            OnDocumentDownloadComplete()
                        End Try
                    End Using
                    _canceller.Token.ThrowIfCancellationRequested()
                Next
                _canceller.Token.ThrowIfCancellationRequested()
                RemoveHandler waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            If Not allOKWithoutException Then Throw lastException
            Return ret
        End Function
        Public Overrides Function RunUpdate(ByVal stmtUpdate As String) As Integer
            logger.Debug("Rnnning update statement synchronously")
            Dim ret As Integer = Nothing
            Dim allOKWithoutException As Boolean = False
            Dim lastException As Exception = Nothing
            Using waiter As New Waiter(_canceller)
                AddHandler waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler waiter.WaitingFor, AddressOf OnWaitingFor
                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    ret = 0
                    lastException = Nothing
                    allOKWithoutException = False
                    Using dbConn As New MySqlConnection(_connectionString)
                        Try
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", dbConn.ConnectionString))
                            OnDocumentRetryStatus(retryCtr, MaxReTries)
                            dbConn.Open()
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat("Updating to DB")
                            Using cmd As New MySqlCommand(stmtUpdate, dbConn)
                                logger.Debug("Firing UPDATE/INSERT/DELETE statement:{0}", cmd.CommandText)
                                ret = cmd.ExecuteNonQuery
                                _canceller.Token.ThrowIfCancellationRequested()
                                If ret = 0 Then
                                    logger.Warn("{0} {1} {0} did not insert/update/delete any records XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", vbNewLine, cmd.CommandText)
                                End If
                                lastException = Nothing
                                allOKWithoutException = True
                                Exit For
                            End Using
                            _canceller.Token.ThrowIfCancellationRequested()
                        Catch opx As OperationCanceledException
                            logger.Error(opx)
                            lastException = opx
                            If Not _canceller.Token.IsCancellationRequested Then
                                _canceller.Token.ThrowIfCancellationRequested()
                                If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                    'Provide required wait in case internet was already up
                                    logger.Debug("DB->Task cancelled without internet problem:{0}",
                                                 opx.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                Else
                                    logger.Debug("DB->Task cancelled with internet problem:{0}",
                                                 opx.Message)
                                    'Since internet was down, no need to consume retries
                                    retryCtr -= 1
                                End If
                            End If
                        Catch ex As Exception
                            logger.Error(ex)
                            lastException = ex
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                _canceller.Token.ThrowIfCancellationRequested()
                                If ExceptionExtensions.IsExceptionConnectionBusyRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type connection busy detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                ElseIf ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type internet related detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                Else
                                    logger.Debug("DB->Exception without internet problem of unknown type detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                End If
                            Else
                                logger.Debug("DB->Exception with internet problem:{0}",
                                             ex.Message)
                                'Since internet was down, no need to consume retries
                                retryCtr -= 1
                            End If
                        Finally
                            If dbConn IsNot Nothing Then dbConn.Close()
                            OnDocumentDownloadComplete()
                        End Try
                    End Using
                    _canceller.Token.ThrowIfCancellationRequested()
                Next
                _canceller.Token.ThrowIfCancellationRequested()
                RemoveHandler waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            If Not allOKWithoutException Then Throw lastException
            Return ret
        End Function
        Public Overrides Async Function RunSelectAsync(ByVal stmtSelect As String) As Task(Of DataTable)
            logger.Debug("Running select statement asynchronously")
            Dim ret As DataTable = Nothing
            Dim allOKWithoutException As Boolean = False
            Dim lastException As Exception = Nothing
            Using waiter As New Waiter(_canceller)
                AddHandler waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler waiter.WaitingFor, AddressOf OnWaitingFor
                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    ret = Nothing
                    lastException = Nothing
                    allOKWithoutException = False
                    Using dbConn As New MySqlConnection(_connectionString)
                        Try
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", dbConn.ConnectionString))
                            OnDocumentRetryStatus(retryCtr, MaxReTries)
                            dbConn.Open()
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat("Selecting from DB")
                            Using cmd As New MySqlCommand(stmtSelect, dbConn),
                                adptSelect As New MySqlDataAdapter(cmd),
                                tmpDs As New DataSet
                                logger.Debug("Firing SELECT statement:{0}", cmd.CommandText)
                                Await adptSelect.FillAsync(tmpDs).ConfigureAwait(False)
                                _canceller.Token.ThrowIfCancellationRequested()
                                If tmpDs.Tables.Count > 0 Then
                                    ret = tmpDs.Tables(0)
                                Else
                                    ret = Nothing
                                    logger.Warn("{0} {1} {0} did not select any records XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", vbNewLine, cmd.CommandText)
                                End If
                                lastException = Nothing
                                allOKWithoutException = True
                                Exit For
                            End Using
                            _canceller.Token.ThrowIfCancellationRequested()
                        Catch opx As OperationCanceledException
                            logger.Error(opx)
                            lastException = opx
                            If Not _canceller.Token.IsCancellationRequested Then
                                _canceller.Token.ThrowIfCancellationRequested()
                                If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                    'Provide required wait in case internet was already up
                                    logger.Debug("DB->Task cancelled without internet problem:{0}",
                                                 opx.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                Else
                                    logger.Debug("DB->Task cancelled with internet problem:{0}",
                                                 opx.Message)
                                    'Since internet was down, no need to consume retries
                                    retryCtr -= 1
                                End If
                            End If
                        Catch ex As Exception
                            logger.Error(ex)
                            lastException = ex
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                _canceller.Token.ThrowIfCancellationRequested()
                                If ExceptionExtensions.IsExceptionConnectionBusyRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type connection busy detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                ElseIf ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type internet related detected:{0}", ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                Else
                                    logger.Debug("DB->Exception without internet problem of unknown type detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                End If
                            Else
                                logger.Debug("DB->Exception with internet problem:{0}",
                                             ex.Message)
                                'Since internet was down, no need to consume retries
                                retryCtr -= 1
                            End If
                        Finally
                            If dbConn IsNot Nothing Then dbConn.Close()
                            OnDocumentDownloadComplete()
                        End Try
                    End Using
                    _canceller.Token.ThrowIfCancellationRequested()
                Next
                _canceller.Token.ThrowIfCancellationRequested()
                RemoveHandler waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            If Not allOKWithoutException Then Throw lastException
            Return ret
        End Function
        Public Overrides Function RunSelect(ByVal stmtSelect As String) As DataTable
            logger.Debug("Running select statement synchronously")
            Dim ret As DataTable = Nothing
            Dim allOKWithoutException As Boolean = False
            Dim lastException As Exception = Nothing
            Using waiter As New Waiter(_canceller)
                AddHandler waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler waiter.WaitingFor, AddressOf OnWaitingFor
                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    ret = Nothing
                    lastException = Nothing
                    allOKWithoutException = False
                    Using dbConn As New MySqlConnection(_connectionString)
                        Try
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", dbConn.ConnectionString))

                            OnDocumentRetryStatus(retryCtr, MaxReTries)
                            dbConn.Open()
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat("Selecting from DB")
                            Using cmd As New MySqlCommand(stmtSelect, dbConn),
                                adptSelect As New MySqlDataAdapter(cmd),
                                tmpDs As New DataSet
                                logger.Debug("Firing SELECT statement:{0}", cmd.CommandText)
                                adptSelect.Fill(tmpDs)
                                _canceller.Token.ThrowIfCancellationRequested()
                                If tmpDs.Tables.Count > 0 Then
                                    ret = tmpDs.Tables(0)
                                Else
                                    ret = Nothing
                                    logger.Warn("{0} {1} {0} did not select any records XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", vbNewLine, cmd.CommandText)
                                End If
                                lastException = Nothing
                                allOKWithoutException = True
                                Exit For
                            End Using
                            _canceller.Token.ThrowIfCancellationRequested()
                        Catch opx As OperationCanceledException
                            logger.Error(opx)
                            lastException = opx
                            If Not _canceller.Token.IsCancellationRequested Then
                                _canceller.Token.ThrowIfCancellationRequested()
                                If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                    'Provide required wait in case internet was already up
                                    logger.Debug("DB->Task cancelled without internet problem:{0}",
                                                 opx.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                Else
                                    logger.Debug("DB->Task cancelled with internet problem:{0}",
                                                 opx.Message)
                                    'Since internet was down, no need to consume retries
                                    retryCtr -= 1
                                End If
                            End If
                        Catch ex As Exception
                            logger.Error(ex)
                            lastException = ex
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                _canceller.Token.ThrowIfCancellationRequested()
                                If ExceptionExtensions.IsExceptionConnectionBusyRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type connection busy detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                ElseIf ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type internet related detected:{0}", ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                Else
                                    logger.Debug("DB->Exception without internet problem of unknown type detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                End If
                            Else
                                logger.Debug("DB->Exception with internet problem:{0}",
                                             ex.Message)
                                'Since internet was down, no need to consume retries
                                retryCtr -= 1
                            End If
                        Finally
                            If dbConn IsNot Nothing Then dbConn.Close()
                            OnDocumentDownloadComplete()
                        End Try
                    End Using
                    _canceller.Token.ThrowIfCancellationRequested()
                Next
                _canceller.Token.ThrowIfCancellationRequested()
                RemoveHandler waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            If Not allOKWithoutException Then Throw lastException
            Return ret
        End Function

        Public Overrides Async Function GetIdentityFromLastInsertAsync(ByVal tableName As String) As Task(Of ULong)
            Await Task.Delay(0, _canceller.Token).ConfigureAwait(False)
            Throw New NotImplementedException
        End Function
        Public Overrides Function GetIdentityFromLastInsert(ByVal tableName As String) As ULong
            Throw New NotImplementedException
        End Function

        Public Overrides Async Function GetIdentityFromLastInsertAsync() As Task(Of ULong)
            logger.Debug("Getting identity from last insert asynchronously")
            Dim ret As ULong = Nothing
            Dim allOKWithoutException As Boolean = False
            Dim lastException As Exception = Nothing
            Using waiter As New Waiter(_canceller)
                AddHandler waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler waiter.WaitingFor, AddressOf OnWaitingFor
                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    ret = 0
                    lastException = Nothing
                    allOKWithoutException = False
                    Using dbConn As New MySqlConnection(_connectionString)
                        Try
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", dbConn.ConnectionString))

                            OnDocumentRetryStatus(retryCtr, MaxReTries)
                            dbConn.Open()
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat("Selecting from DB")
                            Using cmd As New MySqlCommand("SELECT LAST_INSERT_ID()", dbConn)
                                logger.Debug("Firing LAST SELECT statement:{0}", cmd.CommandText)
                                Dim tmp As Object = Await cmd.ExecuteScalarAsync().ConfigureAwait(False)
                                _canceller.Token.ThrowIfCancellationRequested()
                                tmp = If(IsDBNull(tmp), "0", tmp.ToString)
                                ret = Long.Parse(tmp)
                                _canceller.Token.ThrowIfCancellationRequested()
                                logger.Debug("{0} {1} {0} returned ID as {2} VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV", vbNewLine, cmd.CommandText, ret)
                                lastException = Nothing
                                allOKWithoutException = True
                                Exit For
                            End Using
                            _canceller.Token.ThrowIfCancellationRequested()
                        Catch opx As OperationCanceledException
                            logger.Error(opx)
                            lastException = opx
                            If Not _canceller.Token.IsCancellationRequested Then
                                _canceller.Token.ThrowIfCancellationRequested()
                                If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                    'Provide required wait in case internet was already up
                                    logger.Debug("DB->Task cancelled without internet problem:{0}",
                                                 opx.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                Else
                                    logger.Debug("DB->Task cancelled with internet problem:{0}",
                                                 opx.Message)
                                    'Since internet was down, no need to consume retries
                                    retryCtr -= 1
                                End If
                            End If
                        Catch ex As Exception
                            logger.Error(ex)
                            lastException = ex
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                _canceller.Token.ThrowIfCancellationRequested()
                                If ExceptionExtensions.IsExceptionConnectionBusyRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type connection busy detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                ElseIf ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type internet related detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                Else
                                    logger.Debug("DB->Exception without internet problem of unknown type detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                End If
                            Else
                                logger.Debug("DB->Exception with internet problem:{0}",
                                             ex.Message)
                                'Since internet was down, no need to consume retries
                                retryCtr -= 1
                            End If
                        Finally
                            If dbConn IsNot Nothing Then dbConn.Close()
                            OnDocumentDownloadComplete()
                        End Try
                    End Using
                    _canceller.Token.ThrowIfCancellationRequested()
                Next
                _canceller.Token.ThrowIfCancellationRequested()
                RemoveHandler waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            If Not allOKWithoutException Then Throw lastException
            Return ret
        End Function
        Public Overrides Function GetIdentityFromLastInsert() As ULong
            logger.Debug("Getting identity from last insert synchronously")
            Dim ret As ULong = Nothing
            Dim allOKWithoutException As Boolean = False
            Dim lastException As Exception = Nothing
            Using waiter As New Waiter(_canceller)
                AddHandler waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler waiter.WaitingFor, AddressOf OnWaitingFor
                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    ret = 0
                    lastException = Nothing
                    allOKWithoutException = False
                    Using dbConn As New MySqlConnection(_connectionString)
                        Try
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", dbConn.ConnectionString))

                            OnDocumentRetryStatus(retryCtr, MaxReTries)
                            dbConn.Open()
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat("Selecting from DB")
                            Using cmd As New MySqlCommand("SELECT LAST_INSERT_ID()", dbConn)
                                logger.Debug("Firing LAST SELECT statement:{0}", cmd.CommandText)
                                Dim tmp As Object = cmd.ExecuteScalar
                                _canceller.Token.ThrowIfCancellationRequested()
                                tmp = If(IsDBNull(tmp), "0", tmp.ToString)
                                ret = Long.Parse(tmp)
                                _canceller.Token.ThrowIfCancellationRequested()
                                logger.Debug("{0} {1} {0} returned ID as {2} VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV", vbNewLine, cmd.CommandText, ret)
                                lastException = Nothing
                                allOKWithoutException = True
                                Exit For
                            End Using
                            _canceller.Token.ThrowIfCancellationRequested()
                        Catch opx As OperationCanceledException
                            logger.Error(opx)
                            lastException = opx
                            If Not _canceller.Token.IsCancellationRequested Then
                                _canceller.Token.ThrowIfCancellationRequested()
                                If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                    'Provide required wait in case internet was already up
                                    logger.Debug("DB->Task cancelled without internet problem:{0}",
                                                 opx.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                Else
                                    logger.Debug("DB->Task cancelled with internet problem:{0}",
                                                 opx.Message)
                                    'Since internet was down, no need to consume retries
                                    retryCtr -= 1
                                End If
                            End If
                        Catch ex As Exception
                            logger.Error(ex)
                            lastException = ex
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                _canceller.Token.ThrowIfCancellationRequested()
                                If ExceptionExtensions.IsExceptionConnectionBusyRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type connection busy detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                ElseIf ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type internet related detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                Else
                                    logger.Debug("DB->Exception without internet problem of unknown type detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                End If
                            Else
                                logger.Debug("DB->Exception with internet problem:{0}",
                                             ex.Message)
                                'Since internet was down, no need to consume retries
                                retryCtr -= 1
                            End If
                        Finally
                            If dbConn IsNot Nothing Then dbConn.Close()
                            OnDocumentDownloadComplete()
                        End Try
                    End Using
                    _canceller.Token.ThrowIfCancellationRequested()
                Next
                _canceller.Token.ThrowIfCancellationRequested()
                RemoveHandler waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            If Not allOKWithoutException Then Throw lastException
            Return ret
        End Function

        Public Overrides Async Function RunSelectSingleValueAsync(ByVal stmtSelect As String) As Task(Of Object)
            logger.Debug("Running select single value asynchronously")
            Dim ret As Object = Nothing
            Dim allOKWithoutException As Boolean = False
            Dim lastException As Exception = Nothing
            Using waiter As New Waiter(_canceller)
                AddHandler waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler waiter.WaitingFor, AddressOf OnWaitingFor
                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    ret = 0
                    lastException = Nothing
                    allOKWithoutException = False
                    Using dbConn As New MySqlConnection(_connectionString)
                        Try
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", dbConn.ConnectionString))

                            OnDocumentRetryStatus(retryCtr, MaxReTries)
                            dbConn.Open()
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat("Selecting from DB")
                            Using cmd As New MySqlCommand(stmtSelect, dbConn)
                                logger.Debug("Firing SELECT single value statement:{0}", cmd.CommandText)
                                Dim tmp As Object = Await cmd.ExecuteScalarAsync().ConfigureAwait(False)
                                '_canceller.Token.ThrowIfCancellationRequested()
                                'tmp = If(IsDBNull(tmp), "0", tmp.ToString)
                                'ret = Double.Parse(tmp)
                                ret = tmp
                                logger.Debug("{0} {1} {0} returned double as {2} VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV", vbNewLine, cmd.CommandText, ret)
                                lastException = Nothing
                                allOKWithoutException = True
                                Exit For
                            End Using
                            _canceller.Token.ThrowIfCancellationRequested()
                        Catch opx As OperationCanceledException
                            logger.Error(opx)
                            lastException = opx
                            If Not _canceller.Token.IsCancellationRequested Then
                                _canceller.Token.ThrowIfCancellationRequested()
                                If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                    'Provide required wait in case internet was already up
                                    logger.Debug("DB->Task cancelled without internet problem:{0}",
                                                 opx.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                Else
                                    logger.Debug("DB->Task cancelled with internet problem:{0}",
                                                 opx.Message)
                                    'Since internet was down, no need to consume retries
                                    retryCtr -= 1
                                End If
                            End If
                        Catch ex As Exception
                            logger.Error(ex)
                            lastException = ex
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                _canceller.Token.ThrowIfCancellationRequested()
                                If ExceptionExtensions.IsExceptionConnectionBusyRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type connection busy detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                ElseIf ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type internet related detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                Else
                                    logger.Debug("DB->Exception without internet problem of unknown type detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                End If
                            Else
                                logger.Debug("DB->Exception with internet problem:{0}",
                                             ex.Message)
                                'Since internet was down, no need to consume retries
                                retryCtr -= 1
                            End If
                        Finally
                            If dbConn IsNot Nothing Then dbConn.Close()
                            OnDocumentDownloadComplete()
                        End Try
                    End Using
                    _canceller.Token.ThrowIfCancellationRequested()
                Next
                _canceller.Token.ThrowIfCancellationRequested()
                RemoveHandler waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            If Not allOKWithoutException Then Throw lastException
            Return ret
        End Function
        Public Overrides Function RunSelectSingleValue(ByVal stmtSelect As String) As Object
            logger.Debug("Running select single value synchronously")
            Dim ret As Object = Nothing
            Dim allOKWithoutException As Boolean = False
            Dim lastException As Exception = Nothing
            Using waiter As New Waiter(_canceller)
                AddHandler waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler waiter.WaitingFor, AddressOf OnWaitingFor
                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    ret = 0
                    lastException = Nothing
                    allOKWithoutException = False
                    Using dbConn As New MySqlConnection(_connectionString)
                        Try
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", dbConn.ConnectionString))

                            OnDocumentRetryStatus(retryCtr, MaxReTries)
                            dbConn.Open()
                            _canceller.Token.ThrowIfCancellationRequested()
                            OnHeartbeat("Selecting from DB")
                            Using cmd As New MySqlCommand(stmtSelect, dbConn)
                                logger.Debug("Firing SELECT single value statement:{0}", cmd.CommandText)
                                Dim tmp As Object = cmd.ExecuteScalar
                                '_canceller.Token.ThrowIfCancellationRequested()
                                'tmp = If(IsDBNull(tmp), "0", tmp.ToString)
                                'ret = Double.Parse(tmp)
                                ret = tmp
                                logger.Debug("{0} {1} {0} returned double as {2} VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV", vbNewLine, cmd.CommandText, ret)
                                lastException = Nothing
                                allOKWithoutException = True
                                Exit For
                            End Using
                            _canceller.Token.ThrowIfCancellationRequested()
                        Catch opx As OperationCanceledException
                            logger.Error(opx)
                            lastException = opx
                            If Not _canceller.Token.IsCancellationRequested Then
                                _canceller.Token.ThrowIfCancellationRequested()
                                If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                    'Provide required wait in case internet was already up
                                    logger.Debug("DB->Task cancelled without internet problem:{0}",
                                                 opx.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                Else
                                    logger.Debug("DB->Task cancelled with internet problem:{0}",
                                                 opx.Message)
                                    'Since internet was down, no need to consume retries
                                    retryCtr -= 1
                                End If
                            End If
                        Catch ex As Exception
                            logger.Error(ex)
                            lastException = ex
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                _canceller.Token.ThrowIfCancellationRequested()
                                If ExceptionExtensions.IsExceptionConnectionBusyRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type connection busy detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                ElseIf ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                    logger.Debug("DB->Exception without internet problem but of type internet related detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                Else
                                    logger.Debug("DB->Exception without internet problem of unknown type detected:{0}",
                                                 ex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                End If
                            Else
                                logger.Debug("DB->Exception with internet problem:{0}",
                                             ex.Message)
                                'Since internet was down, no need to consume retries
                                retryCtr -= 1
                            End If
                        Finally
                            If dbConn IsNot Nothing Then dbConn.Close()
                            OnDocumentDownloadComplete()
                        End Try
                    End Using
                    _canceller.Token.ThrowIfCancellationRequested()
                Next
                _canceller.Token.ThrowIfCancellationRequested()
                RemoveHandler waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            If Not allOKWithoutException Then Throw lastException
            Return ret
        End Function

#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Shadows Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    MyBase.Dispose(disposing)
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
        Public Shadows Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace