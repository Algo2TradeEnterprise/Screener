Imports System.Net.Http
Imports System.Threading
Imports NLog
Imports Utilities.Network

Namespace Notification
    Public Class Telegram
        Implements IDisposable

#Region "Logging and Status Progress"
        Public Shared logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Events"
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

        Private ReadOnly _apikey As String
        Private ReadOnly _chatId As String
        Protected _canceller As CancellationTokenSource
        Private Const _SMS_API_URL = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}"

        Public Sub New(ByVal apiKey As String, ByVal chatId As String, ByVal canceller As CancellationTokenSource)
            _apikey = apiKey
            _chatId = chatId
            _canceller = canceller
        End Sub

        Public Async Function SendMessageGetAsync(ByVal message As String) As Task
            Dim proxyToBeUsed As HttpProxy = Nothing
            Dim ret As List(Of String) = Nothing

            Using browser As New HttpBrowser(proxyToBeUsed, Net.DecompressionMethods.GZip, New TimeSpan(0, 1, 0), _canceller)
                AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
                AddHandler browser.Heartbeat, AddressOf OnHeartbeat
                AddHandler browser.WaitingFor, AddressOf OnWaitingFor
                AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
                'Get to the landing page first
                Dim url As String = String.Format(_SMS_API_URL, _apikey, _chatId, message)
                Dim l As Tuple(Of Uri, Object) = Await browser.NonPOSTRequestAsync(url,
                                                                                 HttpMethod.Get,
                                                                                 Nothing,
                                                                                 True,
                                                                                 Nothing,
                                                                                 False,
                                                                                 Nothing).ConfigureAwait(False)
                If l Is Nothing OrElse l.Item2 Is Nothing Then
                    Throw New ApplicationException(String.Format("No response while sending telegram message for: {0}", url))
                End If
                'RaiseEvent Heartbeat("Parsing additional site's...")
                If l IsNot Nothing AndAlso l.Item2 IsNot Nothing Then
                    Dim jString As Dictionary(Of String, Object) = l.Item2
                    'If jString IsNot Nothing AndAlso jString.Count > 0 Then
                    'End If
                End If
                RemoveHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
                RemoveHandler browser.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler browser.WaitingFor, AddressOf OnWaitingFor
                RemoveHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
            End Using
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
End Namespace