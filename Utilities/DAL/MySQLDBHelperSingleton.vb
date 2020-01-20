Imports System.Text
Imports MySql.Data.MySqlClient
Imports System.Reflection
Imports NLog
Imports System.Threading
Imports Utilities.ErrorHandlers

Namespace DAL
    Public Class MySQLDBHelperSingleton
        Inherits DBHelperSingleton
        Implements IDisposable
        '************** Events and loggers are in the base class ***************
#Region "Constructor"
        Private Sub New(ByVal serverName As String,
                        ByVal dbName As String,
                        ByVal port As Integer,
                        ByVal userID As String,
                        ByVal password As String,
                        ByVal cts As CancellationTokenSource)
            _connectionString = String.Format("Server={0};Database={1};Port={2};Uid={3};Pwd={4};default command timeout=180;Pooling=True;Min Pool Size=2;Max Pool Size=5;UseAffectedRows=false;Allow User Variables=True", serverName, dbName, port, userID, password)
            _DBConnection = New MySqlConnection(_connectionString)
            OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", _DBConnection.ConnectionString))

            _DBConnection.Open()
            _serverName = serverName
            _dbName = dbName
            _port = port
            _userID = userID
            _password = password
            Me._canceller = cts
        End Sub
#End Region

#Region "Private Attributes"
        Private Shared _DBConnection As MySqlConnection
#End Region

#Region "Public Attributes"
        Public ReadOnly Property GetConnection() As MySqlConnection
            Get
                Return _DBConnection
            End Get
        End Property
#End Region

#Region "Private Methods"
#End Region

#Region "Public Methods"
        Public Shared Function GetInstance(ByVal serverName As String, ByVal dbName As String, ByVal port As Integer, ByVal userID As String, ByVal password As String, ByVal cts As CancellationTokenSource) As MySQLDBHelperSingleton
            logger.Debug("Getting instance")
            If _DBConnectSingleton Is Nothing OrElse _DBConnection Is Nothing OrElse _DBConnection.State = ConnectionState.Closed Then
                'If _DBConnection IsNot Nothing Then
                '    _DBConnection.Close()
                '    _DBConnection.Dispose()
                'End If
                _DBConnectSingleton = New MySQLDBHelperSingleton(serverName, dbName, port, userID, password, cts)
            Else
                logger.Debug("Preparing to reset canceller")
                _DBConnectSingleton.ResetCanceller(cts)
            End If
            Return _DBConnectSingleton
        End Function
        Public Overrides Function IsConnected() As Boolean
            logger.Debug("Checking if connected")
            If _DBConnection IsNot Nothing Then
                If _DBConnection.State <> ConnectionState.Open Then
                    Return False
                Else
                    Return True
                End If
            Else
                Return True
            End If
        End Function
        Public Overrides Sub Close()
            logger.Debug("Closing connection")
            If _DBConnection IsNot Nothing Then
                If _DBConnection.State <> ConnectionState.Closed Then
                    'Though this hits the internet, we will not go by the conventional process of 
                    'going through the framework as defined in examples below like 'RunUpdate'
                    'since the close will be closing the connections and any other checks on the the connection state
                    'from within the framework may result in recursive open and recursive close
                    _DBConnection.Close()
                End If
            End If
        End Sub
        Public Function RunProcedure(ByRef cmd As MySqlCommand) As Integer
            logger.Debug("Running procedure")
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
                    Try
                        OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", _DBConnection.ConnectionString))

                        OnDocumentRetryStatus(retryCtr, MaxReTries)
                        While GetConnection Is Nothing OrElse GetConnection.State = ConnectionState.Closed
                            _canceller.Token.ThrowIfCancellationRequested()
                            logger.Debug("Retrying connection before running SQL statement")
                            _DBConnectSingleton = New MySQLDBHelperSingleton(_serverName, _dbName, _port, _userID, _password, _canceller)
                            _DBConnectSingleton.WaitDurationOnConnectionFailure = Me.WaitDurationOnConnectionFailure
                            _DBConnectSingleton.WaitDurationOnAnyFailure = Me.WaitDurationOnAnyFailure
                            _DBConnectSingleton.MaxReTries = Me.MaxReTries
                            logger.Debug("Connection re-opened")
                        End While
                        _canceller.Token.ThrowIfCancellationRequested()
                        cmd.Connection = _DBConnection

                        OnHeartbeat("Running procedure in DB")
                        ret = cmd.ExecuteNonQuery
                        If ret = 0 Then
                            logger.Warn("{0} {1} {0} did not retrieve any records for this stored procedure execution", vbNewLine, cmd.CommandText)
                        End If
                        lastException = Nothing
                        allOKWithoutException = True
                        Exit For
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
                        OnDocumentDownloadComplete()
                    End Try
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
                    Try
                        OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", _DBConnection.ConnectionString))

                        OnDocumentRetryStatus(retryCtr, MaxReTries)
                        While GetConnection Is Nothing OrElse GetConnection.State = ConnectionState.Closed
                            _canceller.Token.ThrowIfCancellationRequested()
                            logger.Debug("Retrying connection before running SQL statement")
                            _DBConnectSingleton = New MySQLDBHelperSingleton(_serverName, _dbName, _port, _userID, _password, _canceller)
                            _DBConnectSingleton.WaitDurationOnConnectionFailure = Me.WaitDurationOnConnectionFailure
                            _DBConnectSingleton.WaitDurationOnAnyFailure = Me.WaitDurationOnAnyFailure
                            _DBConnectSingleton.MaxReTries = Me.MaxReTries
                            logger.Debug("Connection re-opened")
                        End While
                        _canceller.Token.ThrowIfCancellationRequested()

                        OnHeartbeat("Updating in DB")
                        Using cmd As New MySqlCommand(stmtUpdate, _DBConnection)
                            logger.Debug("Firing UPDATE/INSERT/DELETE statement:{0}", cmd.CommandText)
                            ret = cmd.ExecuteNonQuery
                            If ret = 0 Then
                                logger.Warn("{0} {1} {0} did not insert/update/delete any records", vbNewLine, cmd.CommandText)
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
                        OnDocumentDownloadComplete()
                    End Try
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
                    Try
                        OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", _DBConnection.ConnectionString))

                        OnDocumentRetryStatus(retryCtr, MaxReTries)
                        While GetConnection Is Nothing OrElse GetConnection.State = ConnectionState.Closed
                            _canceller.Token.ThrowIfCancellationRequested()
                            logger.Debug("Retrying connection before running SQL statement")
                            _DBConnectSingleton = New MySQLDBHelperSingleton(_serverName, _dbName, _port, _userID, _password, _canceller)
                            _DBConnectSingleton.WaitDurationOnConnectionFailure = Me.WaitDurationOnConnectionFailure
                            _DBConnectSingleton.WaitDurationOnAnyFailure = Me.WaitDurationOnAnyFailure
                            _DBConnectSingleton.MaxReTries = Me.MaxReTries
                            logger.Debug("Connection re-opened")
                        End While

                        OnHeartbeat("Selecting from DB")
                        Using cmd As New MySqlCommand(stmtSelect, _DBConnection),
                                adptSelect As New MySqlDataAdapter(cmd),
                                tmpDs As New DataSet
                            logger.Debug("Firing SELECT statement:{0}", cmd.CommandText)
                            adptSelect.Fill(tmpDs)
                            If tmpDs.Tables.Count > 0 Then
                                ret = tmpDs.Tables(0)
                            Else
                                ret = Nothing
                                logger.Warn("{0} {1} {0} did not select any records", vbNewLine, cmd.CommandText)
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
                        OnDocumentDownloadComplete()
                    End Try
                    _canceller.Token.ThrowIfCancellationRequested()
                Next
                _canceller.Token.ThrowIfCancellationRequested()
                RemoveHandler waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            If Not allOKWithoutException Then Throw lastException
            Return ret
        End Function
        Public Overrides Function GetIdentityFromLastInsert(ByVal tableName As String) As ULong
            Throw New NotImplementedException
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
                    Try
                        OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", _DBConnection.ConnectionString))

                        OnDocumentRetryStatus(retryCtr, MaxReTries)
                        While GetConnection Is Nothing OrElse GetConnection.State = ConnectionState.Closed
                            _canceller.Token.ThrowIfCancellationRequested()
                            logger.Debug("Retrying connection before running SQL statement")
                            _DBConnectSingleton = New MySQLDBHelperSingleton(_serverName, _dbName, _port, _userID, _password, _canceller)
                            _DBConnectSingleton.WaitDurationOnConnectionFailure = Me.WaitDurationOnConnectionFailure
                            _DBConnectSingleton.WaitDurationOnAnyFailure = Me.WaitDurationOnAnyFailure
                            _DBConnectSingleton.MaxReTries = Me.MaxReTries
                            logger.Debug("Connection re-opened")
                        End While
                        OnHeartbeat("Selecting from DB")

                        Using cmd As New MySqlCommand("SELECT LAST_INSERT_ID()", _DBConnection)
                            ret = Long.Parse(cmd.ExecuteScalar())
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
                        OnDocumentDownloadComplete()
                    End Try
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
                    Try
                        OnHeartbeat(String.Format("Opening connection to DB (Connection string: {0})", _DBConnection.ConnectionString))

                        OnDocumentRetryStatus(retryCtr, MaxReTries)
                        While GetConnection Is Nothing OrElse GetConnection.State = ConnectionState.Closed
                            _canceller.Token.ThrowIfCancellationRequested()
                            logger.Debug("Retrying connection before running SQL statement")
                            _DBConnectSingleton = New MySQLDBHelperSingleton(_serverName, _dbName, _port, _userID, _password, _canceller)
                            _DBConnectSingleton.WaitDurationOnConnectionFailure = Me.WaitDurationOnConnectionFailure
                            _DBConnectSingleton.WaitDurationOnAnyFailure = Me.WaitDurationOnAnyFailure
                            _DBConnectSingleton.MaxReTries = Me.MaxReTries
                            logger.Debug("Connection re-opened")
                        End While

                        OnHeartbeat("Selecting from DB")
                        Using cmd As New MySqlCommand(stmtSelect, _DBConnection)
                            logger.Debug("Firing SELECT single value statement:{0}", cmd.CommandText)
                            Dim tmp As Object = cmd.ExecuteScalar()
                            'tmp = IIf(IsDBNull(tmp), "0", tmp.ToString)
                            'ret = Double.Parse(tmp)
                            ret = tmp
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
                        OnDocumentDownloadComplete()
                    End Try
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

                    _DBConnection.Close()
                    _DBConnection.Dispose()
                    _DBConnection = Nothing
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