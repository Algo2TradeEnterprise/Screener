Imports System.Net.WebSockets
Imports System.Text
Imports System.Threading
Imports Utilities.ErrorHandlers

Namespace Network

    ''' <summary>
    ''' A wrapper for .Net's ClientWebSocket with callbacks
    ''' </summary>
    Public Class WebSocket
        Implements IDisposable
        'Instance of built in ClientWebSocket
        Private _ws As ClientWebSocket
        Private _url As String
        Private _bufferLength As Integer 'Length of buffer to keep binary chunk
        Protected _canceller As CancellationTokenSource

#Region "Events"
        'Events that can be subscribed
        Public Event Heartbeat(ByVal msg As String)
        Public Event ConnectionDone()
        Public Event DisconnectionDone()
        Public Event ErrorOcurred(ByVal message As String)
        Public Event DataReceived(ByVal data As Byte(), ByVal count As Integer, ByVal messageType As WebSocketMessageType)

        'The below functions are needed to allow the derived classes to raise the above two events
        'Event triggered when ticker is connected
        Protected Overridable Sub OnHeartbeat(ByVal msg As String)
            RaiseEvent Heartbeat(msg)
        End Sub

        Protected Overridable Sub OnConnectionDone()
            RaiseEvent ConnectionDone()
        End Sub
        'Event triggered when ticker is disconnected
        Protected Overridable Sub OnDisconnectionDone()
            RaiseEvent DisconnectionDone()
        End Sub
        'Event triggered when ticker receives a tick
        Protected Overridable Sub OnDataReceived(ByVal data As Byte(), ByVal count As Integer, ByVal messageType As WebSocketMessageType)
            RaiseEvent DataReceived(data, count, messageType)
        End Sub
        'Event triggered when ticker encounters an error
        Protected Overridable Sub OnErrorOcurred(ByVal message As String)
            RaiseEvent ErrorOcurred(message)
        End Sub
#End Region

        ''' <summary>
        ''' Initialize WebSocket class
        ''' </summary>
        ''' <param name="canceller">Cancellation token source for coming out on cancellation.</param>
        ''' <param name="bufferLength">Size of buffer to keep byte stream chunk.</param>
        Public Sub New(ByVal canceller As CancellationTokenSource, Optional ByVal bufferLength As Integer = 2000000)
            _canceller = canceller
            _bufferLength = bufferLength
        End Sub

        ''' <summary>
        ''' Check if WebSocket is connected or not
        ''' </summary>
        ''' <returns>True if connection is live</returns>
        Public Function IsConnected() As Boolean
            logger.Debug("Checking if connected")
            If _ws Is Nothing Then
                Return False
            End If
            Return If(_ws.State = WebSocketState.Open, True, False)
        End Function


        ''' <summary>
        ''' Connect to WebSocket
        ''' </summary>
        Public Sub Connect(ByVal url As String, ByVal headers As Dictionary(Of String, String))
            logger.Debug("Connecting")
            Try
                _url = url
                'Initialize ClientWebSocket instance And connect with Url
                _ws = New ClientWebSocket()
                If headers IsNot Nothing Then
                    For Each key In headers.Keys
                        _ws.Options.SetRequestHeader(key, headers(key))
                    Next
                End If
                OnHeartbeat(String.Format("Connecting to websocket (URL:{0})", _url))
                _ws.ConnectAsync(New Uri(_url), _canceller.Token).Wait()
                _canceller.Token.ThrowIfCancellationRequested()
            Catch ae As AggregateException
                logger.Error(ae)
                _canceller.Token.ThrowIfCancellationRequested()
                For Each ie As String In GetExceptionMessages(ae, ae.Message)
                    _canceller.Token.ThrowIfCancellationRequested()
                    OnErrorOcurred(String.Format("Error while connecting. Message: {0}", ie))
                    If ie.ToUpper.Contains("Forbidden".ToUpper) And ie.ToUpper.Contains("403") Then
                        logger.Debug("Forbidden or 403 error, disconnecting")
                        OnDisconnectionDone()
                    End If
                Next
                _canceller.Token.ThrowIfCancellationRequested()
                Return
            Catch ex As Exception
                logger.Error(ex)
                _canceller.Token.ThrowIfCancellationRequested()
                OnErrorOcurred(String.Format("Error while connecting. Message: {0}", ExceptionExtensions.GetExceptionMessages(ex)))
                Return
            End Try
            OnConnectionDone()

            _canceller.Token.ThrowIfCancellationRequested()
            Dim buffer As Byte() = New Byte(_bufferLength) {}
            Dim callback As Action(Of Task(Of WebSocketReceiveResult)) = Nothing
            Try
                'Callback for receiving data
                callback = Sub(t)
                               Try
                                   OnDataReceived(buffer, t.Result.Count, t.Result.MessageType)
                                   'Again try to receive data
                                   _ws.ReceiveAsync(New ArraySegment(Of Byte)(buffer), _canceller.Token).ContinueWith(callback)
                                   _canceller.Token.ThrowIfCancellationRequested()
                               Catch ex As Exception
                                   logger.Error(ex)
                                   _canceller.Token.ThrowIfCancellationRequested()
                                   If (IsConnected()) Then
                                       OnErrorOcurred(String.Format("Error while recieving data. Message: {0}", ex.Message))
                                   Else
                                       OnErrorOcurred("Lost ticker connection")
                                   End If
                               End Try
                           End Sub
                'To start the receive loop in the beginning
                _ws.ReceiveAsync(New ArraySegment(Of Byte)(buffer), _canceller.Token).ContinueWith(callback)
                _canceller.Token.ThrowIfCancellationRequested()
            Catch ex As Exception
                logger.Error(ex)
                _canceller.Token.ThrowIfCancellationRequested()
                If (IsConnected()) Then
                    OnErrorOcurred(String.Format("Error while recieving data. Message: {0}", ex.Message))
                Else
                    OnErrorOcurred("Lost ticker connection")
                End If
            End Try
        End Sub
        ''' <summary>
        ''' Send message to socket connection
        ''' </summary>
        ''' <param name="Message">Message to send</param>
        Public Sub Send(Message As String)
            logger.Debug("Sending message")
            If _ws.State = WebSocketState.Open Then
                Try
                    _ws.SendAsync(New ArraySegment(Of Byte)(Encoding.UTF8.GetBytes(Message)), WebSocketMessageType.Text, True, _canceller.Token).Wait()
                    _canceller.Token.ThrowIfCancellationRequested()
                Catch ex As Exception
                    logger.Error(ex)
                    _canceller.Token.ThrowIfCancellationRequested()
                    OnErrorOcurred(String.Format("Error while sending data. Message: {0}", ex.Message))
                End Try
            End If
        End Sub
        ''' <summary>
        ''' Close the WebSocket connection
        ''' </summary>
        ''' <param name="Abort">If true WebSocket will not send 'Close' signal to server. Used when connection is disconnected due to netork issues.</param>
        Public Sub Close(Optional Abort As Boolean = False)
            logger.Debug("Closing")
            If _ws.State = WebSocketState.Open Then
                Try
                    If Abort Then
                        _ws.Abort()
                    Else
                        _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _canceller.Token).Wait()
                        OnDisconnectionDone()
                    End If
                    _canceller.Token.ThrowIfCancellationRequested()
                Catch ex As Exception
                    logger.Error(ex)
                    _canceller.Token.ThrowIfCancellationRequested()
                    OnErrorOcurred(String.Format("Error while closing connection. Message: {0}", ex.Message))
                End Try
            End If
        End Sub
#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    _ws = Nothing
                    _url = Nothing
                    _bufferLength = Nothing
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
End Namespace
