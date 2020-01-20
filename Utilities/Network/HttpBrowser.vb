Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports HtmlAgilityPack
Imports NLog
Imports Utilities.ErrorHandlers
Imports Newtonsoft.Json
Imports System.IO
Imports System.Net.Mime
Imports Utilities.Strings
Namespace Network
    Public Class HttpBrowser
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

#Region "Constructors"
        Public Sub New(ByVal cookies As CookieContainer,
                       ByVal proxyToBeUsed As HttpProxy,
                       ByVal automaticDecompression As DecompressionMethods,
                       ByVal connectTimeOut As TimeSpan,
                       ByVal canceller As CancellationTokenSource)
            Me.New(proxyToBeUsed, automaticDecompression, connectTimeOut, canceller)
            _AllCookies = cookies
        End Sub
        Public Sub New(ByVal proxyToBeUsed As HttpProxy,
                       ByVal automaticDecompression As DecompressionMethods,
                       ByVal connectTimeOut As TimeSpan,
                       ByVal canceller As CancellationTokenSource)
            If _AllCookies Is Nothing Then _AllCookies = New CookieContainer
            _canceller = canceller
            If proxyToBeUsed IsNot Nothing Then
                _httpHandler = New HttpClientHandler With {
                .AllowAutoRedirect = True,
                .AutomaticDecompression = automaticDecompression,
                .Proxy = proxyToBeUsed.Proxy,
                .CookieContainer = _AllCookies,
                .UseCookies = True,
                .UseProxy = True
                }
                _proxyToBeUsed = proxyToBeUsed
            Else
                _httpHandler = New HttpClientHandler With {
                            .AllowAutoRedirect = True,
                            .AutomaticDecompression = automaticDecompression,
                            .CookieContainer = _AllCookies,
                            .UseCookies = True,
                            .UseProxy = False
                            }
                _proxyToBeUsed = Nothing
            End If
            Me._ConnectTimeOut = connectTimeOut
            Me._httpInstance = New HttpClient(_httpHandler, False)
            Me._httpInstance.Timeout = Me.ConnectTimeOut
            HtmlNode.ElementsFlags.Remove("form")
        End Sub
#End Region

#Region "Private Attributes"
        Private _httpInstance As HttpClient
        Private _httpHandler As HttpClientHandler
        Private _proxyToBeUsed As HttpProxy

        'Private Shared _internetConnectionCheckerURL As String = "www.rediff.com"
        Private Shared _internetConnectionCheckerURL As String = "www.wikipedia.org"
        Protected _canceller As CancellationTokenSource
#End Region

#Region "Public Attributes"
        Private Shared _AllCookies As CookieContainer
        Public Shared ReadOnly Property AllCookies As CookieContainer
            Get
                Return _AllCookies
            End Get
        End Property

        Private _ConnectTimeOut As TimeSpan = TimeSpan.FromSeconds(90)
        Public Property ConnectTimeOut As TimeSpan
            Get
                Return _ConnectTimeOut
            End Get
            Set(value As TimeSpan)
                _ConnectTimeOut = value
                _httpInstance.Timeout = _ConnectTimeOut
            End Set
        End Property
        Public Property Accept As String = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
        Public Property AcceptLanguage = "en-US,en;q=0.8"
        Public Property AcceptEncoding = "gzip, deflate, sdch"
        Public Property UserAgent As String = "Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko"
        Public Property KeepAlive As Boolean = False
        Public Property FormPostContentType As String = "application/x-www-form-urlencoded"
        Public Property MaxReTries As Integer = 20
        Public Property WaitDurationOnConnectionFailure As TimeSpan = TimeSpan.FromSeconds(5)
        Public Property WaitDurationOnServiceUnavailbleFailure As TimeSpan = TimeSpan.FromSeconds(30)
        Public Property WaitDurationOnAnyFailure As TimeSpan = TimeSpan.FromSeconds(10)
#End Region

#Region "Private Methods"
        Private Async Function GenerateResponseOutputAsync(ByVal response As HttpResponseMessage, ByVal responseType As String) As Task(Of Object)
            logger.Debug("Generating response output")
            If response IsNot Nothing AndAlso response.Content IsNot Nothing Then
                If responseType IsNot Nothing AndAlso Not response.Content.Headers.ContentType.MediaType = responseType Then
                    Throw New ApplicationException("Error in expected response type, probably a server block message instead of json")
                End If
                If response.Content.Headers.ContentType.MediaType = "text/html" Then
                    logger.Debug("Inside text/html output conversion")
                    Dim retDoc As HtmlDocument = Nothing
                    Using ms As New System.IO.MemoryStream(Await response.Content.ReadAsByteArrayAsync.ConfigureAwait(False))
                        retDoc = New HtmlDocument
                        retDoc.Load(ms)
                        ms.Close()
                    End Using
                    Return retDoc
                ElseIf response.Content.Headers.ContentType.MediaType = "application/json" Then
                    logger.Debug("Inside application/json output conversion")
                    Dim jsonString As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
                    Dim retDictionary As Dictionary(Of String, Object) = JsonDeserialize(jsonString)
                    Return retDictionary
                ElseIf response.Content.Headers.ContentType.MediaType = "text/csv" Then
                    logger.Debug("Inside text/csv output conversion")
                    Throw New NotImplementedException
                ElseIf response.Content.Headers.ContentType.MediaType = "application/javascript" Then
                    logger.Debug("Inside application/javascript output conversion")
                    Dim jsString As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
                    Return jsString
                Else
                    Throw New NotImplementedException
                End If
            Else
                Return Nothing
            End If
        End Function

        Public Sub AddHeaders(ByVal request As HttpRequestMessage, ByVal referalURL As String, ByVal useRandomUserAgent As Boolean, ByVal headers As Dictionary(Of String, String))
            logger.Debug("Adding headers")

            'request.Headers.TryAddWithoutValidation("Host", "www.nseindia.com")
            'request.Headers.TryAddWithoutValidation("Connection", "Keep-Alive")
            'request.KeepAlive = True
            'request.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1")
            'request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3")
            'request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate")
            'request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "none")

            'request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate")
            'request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9")

            'request.ContentType = "application/x-www-form-urlencoded"
            'request.Referer = "https://www.google.com/"
            'request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Trident/7.0; rv:11.0) like Gecko")

            'headers = New Dictionary(Of String, String)
            'headers.Add("Host", "www.nseindia.com")
            'headers.Add("Upgrade-Insecure-Requests", "1")
            'headers.Add("Sec-Fetch-Mode", "navigate")
            'headers.Add("Sec-Fetch-Site", "none")

            If headers IsNot Nothing Then
                For Each item In headers
                    request.Headers.TryAddWithoutValidation(item.Key, item.Value)
                Next
            End If

            Dim dummy As IEnumerable(Of String) = Nothing

            request.Headers.TryGetValues("Accept", dummy)
            If dummy Is Nothing OrElse dummy.FirstOrDefault Is Nothing Then
                request.Headers.TryAddWithoutValidation("Accept", Accept)
            End If

            dummy = Nothing
            request.Headers.TryGetValues("Accept-Encoding", dummy)
            If dummy Is Nothing OrElse dummy.FirstOrDefault Is Nothing Then
                request.Headers.TryAddWithoutValidation("Accept-Encoding", AcceptEncoding)
            End If

            dummy = Nothing
            request.Headers.TryGetValues("Accept-Language", dummy)
            If dummy Is Nothing OrElse dummy.FirstOrDefault Is Nothing Then
                request.Headers.TryAddWithoutValidation("Accept-Language", AcceptLanguage)
            End If

            dummy = Nothing
            request.Headers.TryGetValues("Content-Type", dummy)
            If dummy Is Nothing OrElse dummy.FirstOrDefault Is Nothing Then
                request.Headers.TryAddWithoutValidation("Content-Type", FormPostContentType)
            End If

            If referalURL IsNot Nothing Then request.Headers.TryAddWithoutValidation("Referer", referalURL)

            dummy = Nothing
            request.Headers.TryGetValues("User-Agent", dummy)

            If dummy Is Nothing OrElse dummy.FirstOrDefault Is Nothing Then
                If useRandomUserAgent Then
                    request.Headers.TryAddWithoutValidation("User-Agent", HttpBrowserHelper.GetRandomUserAgent)
                Else
                    request.Headers.TryAddWithoutValidation("User-Agent", UserAgent)
                End If
            End If
            dummy = Nothing
            request.Headers.TryGetValues("Connection", dummy)
            If dummy Is Nothing OrElse dummy.FirstOrDefault Is Nothing Then
                Dim connectionString As String = Nothing
                If KeepAlive Then
                    connectionString = "Keep-Alive"
                Else
                    connectionString = "Close"
                End If
                request.Headers.TryAddWithoutValidation("Connection", connectionString)
            End If

            dummy = Nothing
            request.Headers.TryGetValues("Content-Type", dummy)
            If dummy Is Nothing OrElse dummy.FirstOrDefault Is Nothing Then
                request.Headers.TryAddWithoutValidation("Content-Type", FormPostContentType)
            End If

            dummy = Nothing
            request.Headers.TryGetValues("Referer", dummy)
            If dummy Is Nothing OrElse dummy.FirstOrDefault Is Nothing Then
                If referalURL IsNot Nothing Then request.Headers.TryAddWithoutValidation("Referer", referalURL)
            End If
        End Sub
#End Region

#Region "Public Methods"
        Public Shared Sub KillCookies()
            logger.Debug("Killing cookies")
            _AllCookies = Nothing
        End Sub

        Public Shared Function IsNetworkAvailableAsync(ByVal cts As CancellationTokenSource) As Boolean
            logger.Debug("Checking if network available")
            Dim ret As Boolean = False
            Using ping = New System.Net.NetworkInformation.Ping()
                Try
                    logger.Debug("Checking ping (URL:{0})", _internetConnectionCheckerURL)
                    Dim result = ping.Send(_internetConnectionCheckerURL)
                    If (result.Status <> System.Net.NetworkInformation.IPStatus.Success) Then
                        ret = False
                    Else
                        ret = True
                    End If
                Catch ex As Exception
                    logger.Debug("Supressed exception")
                    logger.Error(ex)
                    ret = False
                Finally
                    ping.Dispose()
                End Try
            End Using
            Return ret
        End Function
        Public Async Function GetFileAsync(ByVal downloadURL As String,
                                           ByVal filePath As String,
                                           ByVal useRandomUserAgent As Boolean,
                                           ByVal headers As Dictionary(Of String, String)) As Task(Of Boolean)
            logger.Debug("Getting file asynchronously")
            Dim ret As Boolean = Nothing
            Dim lastException As Exception = Nothing
            Dim request As HttpRequestMessage = Nothing
            Dim allOKWithoutException As Boolean = False
            Using waiter As New Waiter(_canceller)
                AddHandler waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler waiter.WaitingFor, AddressOf OnWaitingFor

                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    ret = Nothing
                    lastException = Nothing
                    request = Nothing
                    OnDocumentRetryStatus(retryCtr, MaxReTries)
                    request = New HttpRequestMessage() With
                                                    {
                                                        .RequestUri = New Uri(downloadURL),
                                                        .Method = HttpMethod.Get
                                                    }
                    OnHeartbeat(String.Format("Downloading file over GET (URL:{0})", downloadURL))
                    Try
                        AddHeaders(request, Nothing, useRandomUserAgent, headers)
                        _canceller.Token.ThrowIfCancellationRequested()

                        Dim responseMessage As HttpResponseMessage = Await _httpInstance.SendAsync(request,
                                                                                               _canceller.Token).ConfigureAwait(False)
                        logger.Debug("Processing response")
                        Using contentStream As Stream = Await (responseMessage).Content.ReadAsStreamAsync.ConfigureAwait(False)
                            _canceller.Token.ThrowIfCancellationRequested()
                            logger.Debug("Creating local file from the download (File:{0})", filePath)
                            Using localStream As FileStream = File.Create(filePath)
                                _canceller.Token.ThrowIfCancellationRequested()
                                contentStream.CopyTo(localStream)
                                localStream.Close()
                            End Using
                            contentStream.Close()
                        End Using
                        _canceller.Token.ThrowIfCancellationRequested()

                        If Not File.Exists(filePath) Then
                            Throw New ApplicationException(String.Format("File download did not create local file, URL attempted:{0}, Proxy:{1}", downloadURL, If(_httpHandler.Proxy IsNot Nothing, _httpHandler.Proxy.ToString, "NULL")))
                        Else
                            lastException = Nothing
                            allOKWithoutException = True
                            ret = True
                            Exit For
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                    Catch opx As OperationCanceledException
                        logger.Error(opx)
                        lastException = opx
                        If Not _canceller.Token.IsCancellationRequested Then
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                logger.Debug("HTTP->Task cancelled without internet problem:{0}",
                                         opx.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                _canceller.Token.ThrowIfCancellationRequested()
                            Else
                                logger.Debug("HTTP->Task cancelled due to internet problem:{0}, waited prescribed seconds, will now retry",
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
                            If ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                logger.Debug("HTTP->Exception without internet problem but of type internet related detected:{0}",
                                         ex.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                _canceller.Token.ThrowIfCancellationRequested()
                                'Since exception was internet related, no need to consume retries
                                retryCtr -= 1
                            Else
                                logger.Debug("HTTP->Exception without internet problem of unknown type detected:{0}",
                                         ex.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                _canceller.Token.ThrowIfCancellationRequested()
                            End If
                        Else
                            logger.Debug("HTTP->Exception with internet problem:{0}, waited prescribed seconds, will now retry",
                                     ex.Message)
                            'Since internet was down, no need to consume retries
                            retryCtr -= 1
                        End If
                    Finally
                        OnDocumentDownloadComplete()
                        'Console.WriteLine(downloadURL)
                        'For Each cookie As Cookie In _AllCookies.GetCookies(New Uri("http://kite.zerodha.com"))
                        '    Console.WriteLine("Name = {0} ; Value = {1} ; Domain = {2}", cookie.Name, cookie.Value,
                        '              cookie.Domain)

                        'Next
                    End Try
                    _canceller.Token.ThrowIfCancellationRequested()
                    GC.Collect()
                    If ret Then
                        Exit For
                    End If
                Next
                RemoveHandler waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            _canceller.Token.ThrowIfCancellationRequested()
            If Not allOKWithoutException Then Throw lastException
            Return ret
        End Function

        Public Async Function NonPOSTRequestAsync(ByVal URLToBrowse As String,
                                                  ByVal method As HttpMethod,
                                                  ByVal referalURL As String,
                                                  ByVal useRandomUserAgent As Boolean,
                                                  ByVal headers As Dictionary(Of String, String),
                                                  ByVal checkSuccessCode As Boolean,
                                                  ByVal responseType As String) As Task(Of Tuple(Of Uri, Object))
            'If URLToBrowse.ToUpper.Contains("kitecharts".ToUpper) Then Exit Function

            logger.Debug("Placing non POST request asynchronously")
            Dim retTuple As Tuple(Of Uri, Object) = Nothing

            Dim lastException As Exception = Nothing
            Dim response As HttpResponseMessage = Nothing
            Dim request As HttpRequestMessage = Nothing
            Dim allOKWithoutException As Boolean = False
            Using Waiter As New Waiter(_canceller)
                AddHandler Waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler Waiter.WaitingFor, AddressOf OnWaitingFor

                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    retTuple = Nothing
                    lastException = Nothing
                    request = Nothing
                    response = Nothing

                    If URLToBrowse <> _internetConnectionCheckerURL Then OnDocumentRetryStatus(retryCtr, MaxReTries)
                    request = New HttpRequestMessage() With
                                                    {
                                                        .RequestUri = New Uri(URLToBrowse),
                                                        .Method = method
                                                    }

                    OnHeartbeat(String.Format("Opening URL using non-POST request (URL:{0})", URLToBrowse))
                    Try
                        AddHeaders(request, referalURL, useRandomUserAgent, headers)

                        _canceller.Token.ThrowIfCancellationRequested()
                        response = Await _httpInstance.SendAsync(request, _canceller.Token).ConfigureAwait(False)

                        If checkSuccessCode Then response.EnsureSuccessStatusCode()
                        _canceller.Token.ThrowIfCancellationRequested()

                        logger.Debug("Processing response")
                        Dim tempRet = Await GenerateResponseOutputAsync(response, responseType).ConfigureAwait(False)

                        If tempRet IsNot Nothing Then
                            logger.Debug("Processing object to be returned (Response URL:{0})", response.RequestMessage.RequestUri)
                            lastException = Nothing
                            allOKWithoutException = True
                            retTuple = New Tuple(Of Uri, Object)(response.RequestMessage.RequestUri, tempRet)
                            Exit For
                        Else
                            Throw New ApplicationException(String.Format("HTML download did not succeed, URL attempted:{0}, Proxy:{1}", URLToBrowse, If(_httpHandler.Proxy IsNot Nothing, _httpHandler.Proxy.ToString, "NULL")))
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                    Catch opx As OperationCanceledException
                        'Exit Function
                        logger.Error(opx)
                        lastException = opx
                        If URLToBrowse = _internetConnectionCheckerURL Then
                            Exit For
                        End If
                        If Not _canceller.Token.IsCancellationRequested Then
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not Waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                logger.Debug("HTTP->Task was cancelled without internet problem:{0}",
                                             opx.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                _canceller.Token.ThrowIfCancellationRequested()
                            Else
                                logger.Debug("HTTP->Task was cancelled due to internet problem:{0}, waited prescribed seconds, will now retry",
                                             opx.Message)
                                'Since internet was down, no need to consume retries
                                retryCtr -= 1
                            End If
                        End If
                    Catch hex As HttpRequestException
                        logger.Error(hex)
                        lastException = hex
                        'Need to relogin, no point retrying
                        If (response IsNot Nothing AndAlso response.StatusCode = "400") Then
                            Throw New URLMisFormedException(hex.Message, hex, URLMisFormedException.TypeOfException.BadURL)
                        End If
                        If (response IsNot Nothing AndAlso response.StatusCode = "403") Then
                            Throw New ForbiddenException(hex.Message, hex, ForbiddenException.TypeOfException.PossibleCaptcha)
                        End If
                        If ExceptionExtensions.GetExceptionMessages(hex).Contains("trust relationship") Then
                            Throw New ForbiddenException(hex.Message, hex, ForbiddenException.TypeOfException.PossibleReloginRequired)
                        End If
                        If URLToBrowse = _internetConnectionCheckerURL Then
                            Exit For
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                        If Not Waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                            If hex.Message.Contains("429") Or hex.Message.Contains("503") Then
                                logger.Debug("HTTP->429/503 error without internet problem:{0}",
                                             hex.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Waiter.SleepRequiredDuration(WaitDurationOnServiceUnavailbleFailure.TotalSeconds, "Service unavailable(429/503)")
                                _canceller.Token.ThrowIfCancellationRequested()
                                'Since site service is blocked, no need to consume retries
                                retryCtr -= 1
                            ElseIf hex.Message.Contains("404") Then
                                logger.Debug("HTTP->404 error without internet problem:{0}",
                                             hex.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                'No point retrying, exit for
                                Exit For
                            Else
                                If ExceptionExtensions.IsExceptionConnectionRelated(hex) Then
                                    logger.Debug("HTTP->HttpRequestException without internet problem but of type internet related detected:{0}",
                                                 hex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    Waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection HttpRequestException")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                Else
                                    'Provide required wait in case internet was already up
                                    logger.Debug("HTTP->HttpRequestException without internet problem:{0}",
                                                 hex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    Waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown HttpRequestException:" & hex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                End If
                            End If
                        Else
                            logger.Debug("HTTP->HttpRequestException with internet problem:{0}, waited prescribed seconds, will now retry",
                                         hex.Message)
                            'Since internet was down, no need to consume retries
                            retryCtr -= 1
                        End If
                    Catch ex As Exception
                        logger.Error(ex)
                        lastException = ex
                        'Exit if it is a network failure check and stop retry to avoid stack overflow
                        'Need to relogin, no point retrying
                        If ExceptionExtensions.GetExceptionMessages(ex).Contains("disposed") Then
                            Throw New ForbiddenException(ex.Message, ex, ForbiddenException.TypeOfException.ExceptionInBetweenLoginProcess)
                        End If
                        If URLToBrowse = _internetConnectionCheckerURL Then
                            Exit For
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                        If Not Waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                            'Provide required wait in case internet was already up
                            _canceller.Token.ThrowIfCancellationRequested()
                            If ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                logger.Debug("HTTP->Exception without internet problem but of type internet related detected:{0}",
                                             ex.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                _canceller.Token.ThrowIfCancellationRequested()
                                'Since exception was internet related, no need to consume retries
                                retryCtr -= 1
                            Else
                                logger.Debug("HTTP->Exception without internet problem of unknown type detected:{0}",
                                             ex.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                _canceller.Token.ThrowIfCancellationRequested()
                            End If
                        Else
                            logger.Debug("HTTP->Exception with internet problem:{0}, waited prescribed seconds, will now retry",
                                         ex.Message)
                            'Since internet was down, no need to consume retries
                            retryCtr -= 1
                        End If
                    Finally
                        OnDocumentDownloadComplete()
                        If response IsNot Nothing Then response.Dispose()
                        'Console.WriteLine(URLToBrowse)
                        'If _AllCookies IsNot Nothing Then
                        '    For Each cookie As Cookie In _AllCookies.GetCookies(New Uri("http://kite.zerodha.com"))
                        '        Console.WriteLine("Name = {0} ; Value = {1} ; Domain = {2}", cookie.Name, cookie.Value,
                        '              cookie.Domain)

                        '    Next
                        'End If
                        GC.AddMemoryPressure(1024 * 1024)
                        GC.Collect()
                    End Try
                    _canceller.Token.ThrowIfCancellationRequested()
                    If retTuple IsNot Nothing Then
                        Exit For
                    End If
                Next
                RemoveHandler Waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler Waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            _canceller.Token.ThrowIfCancellationRequested()
            If Not allOKWithoutException Then Throw lastException
            Return retTuple
        End Function

        Private Async Function POSTRequestAsync(ByVal postURL As String,
                                               ByVal referalURL As String,
                                               ByVal usablePostContent As HttpContent,
                                               ByVal useRandomUserAgent As Boolean,
                                               ByVal headers As Dictionary(Of String, String),
                                               ByVal checkSuccessCode As Boolean) As Task(Of Tuple(Of Uri, Object))
            logger.Debug("Placing POST request asynchronously")
            Dim retTuple As Tuple(Of Uri, Object) = Nothing

            Dim lastException As Exception = Nothing
            Dim response As HttpResponseMessage = Nothing
            Dim request As HttpRequestMessage = Nothing
            Dim allOKWithoutException As Boolean = False
            Using Waiter As New Waiter(_canceller)
                AddHandler Waiter.Heartbeat, AddressOf OnHeartbeat
                AddHandler Waiter.WaitingFor, AddressOf OnWaitingFor

                Dim postString As String = Await usablePostContent.ReadAsStringAsync.ConfigureAwait(False)
                For retryCtr = 1 To MaxReTries
                    _canceller.Token.ThrowIfCancellationRequested()
                    retTuple = Nothing
                    lastException = Nothing
                    request = Nothing
                    response = Nothing

                    RaiseEvent DocumentRetryStatus(retryCtr, MaxReTries)
                    request = New HttpRequestMessage() With
                    {
                        .RequestUri = New Uri(postURL),
                        .Method = HttpMethod.Post,
                        .Content = New StringContent(postString, Text.Encoding.UTF8, "application/x-www-form-urlencoded")
                    }
                    OnHeartbeat(String.Format("Opening URL using POST request (URL:{0}, content:{1})", postURL, Await request.Content.ReadAsStringAsync().ConfigureAwait(False)))
                    Try
                        AddHeaders(request, referalURL, useRandomUserAgent, headers)

                        _canceller.Token.ThrowIfCancellationRequested()
                        response = Await _httpInstance.SendAsync(request, _canceller.Token).ConfigureAwait(False)
                        If checkSuccessCode Then response.EnsureSuccessStatusCode()
                        _canceller.Token.ThrowIfCancellationRequested()

                        logger.Debug("Processing response")
                        Dim tempRet = Await GenerateResponseOutputAsync(response, Nothing).ConfigureAwait(False)
                        If tempRet IsNot Nothing Then
                            logger.Debug("Processing object to be returned (Response URL:{0})", response.RequestMessage.RequestUri)
                            lastException = Nothing
                            allOKWithoutException = True
                            retTuple = New Tuple(Of Uri, Object)(response.RequestMessage.RequestUri, tempRet)
                            Exit For
                        Else
                            Throw New ApplicationException(String.Format("HTML post did not succeed, URL attempted:{0}, Proxy:{1}", postURL, If(_httpHandler.Proxy IsNot Nothing, _httpHandler.Proxy.ToString, "NULL")))
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                    Catch opx As OperationCanceledException
                        logger.Error(opx)
                        lastException = opx
                        If postURL = _internetConnectionCheckerURL Then
                            Exit For
                        End If
                        If Not _canceller.Token.IsCancellationRequested Then
                            _canceller.Token.ThrowIfCancellationRequested()
                            If Not Waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                                'Provide required wait in case internet was already up
                                logger.Debug("HTTP->Task was cancelled without internet problem:{0}",
                                             opx.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Non-explicit cancellation")
                                _canceller.Token.ThrowIfCancellationRequested()
                            Else
                                logger.Debug("HTTP->Task was cancelled due to internet problem:{0}, waited prescribed seconds, will now retry",
                                             opx.Message)
                                'Since internet was down, no need to consume retries
                                retryCtr -= 1
                            End If
                        End If
                    Catch hex As HttpRequestException
                        logger.Error(hex)
                        lastException = hex

                        'Need to relogin, no point retrying
                        If (response IsNot Nothing AndAlso response.StatusCode = "400") Then
                            Throw New URLMisFormedException(hex.Message, hex, URLMisFormedException.TypeOfException.BadURL)
                        End If
                        If (response IsNot Nothing AndAlso response.StatusCode = "403") Then
                            Throw New ForbiddenException(hex.Message, hex, ForbiddenException.TypeOfException.PossibleCaptcha)
                        End If
                        If ExceptionExtensions.GetExceptionMessages(hex).Contains("trust relationship") Then
                            Throw New ForbiddenException(hex.Message, hex, ForbiddenException.TypeOfException.PossibleReloginRequired)
                        End If
                        If postURL = _internetConnectionCheckerURL Then
                            Exit For
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                        If Not Waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                            If hex.Message.Contains("429") Or hex.Message.Contains("503") Then
                                logger.Debug("HTTP->429/503 error without internet problem:{0}",
                                             hex.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Waiter.SleepRequiredDuration(WaitDurationOnServiceUnavailbleFailure.TotalSeconds, "Service unavailable(429/503)")
                                _canceller.Token.ThrowIfCancellationRequested()
                                'Since site service is blocked, no need to consume retries
                                retryCtr -= 1
                            ElseIf hex.Message.Contains("404") Then
                                logger.Debug("HTTP->404 error without internet problem:{0}",
                                             hex.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                'No point retrying, exit for
                                Exit For
                            Else
                                If ExceptionExtensions.IsExceptionConnectionRelated(hex) Then
                                    logger.Debug("HTTP->HttpRequestException without internet problem but of type internet related detected:{0}",
                                                 hex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    Waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection HttpRequestException")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    'Since exception was internet related, no need to consume retries
                                    retryCtr -= 1
                                Else
                                    'Provide required wait in case internet was already up
                                    logger.Debug("HTTP->HttpRequestException without internet problem:{0}",
                                                 hex.Message)
                                    _canceller.Token.ThrowIfCancellationRequested()
                                    Waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown HttpRequestException")
                                    _canceller.Token.ThrowIfCancellationRequested()
                                End If
                            End If
                        Else
                            logger.Debug("HTTP->HttpRequestException with internet problem:{0}, waited prescribed seconds, will now retry",
                                         hex.Message)
                            'Since internet was down, no need to consume retries
                            retryCtr -= 1
                        End If
                    Catch ex As Exception
                        logger.Error(ex)
                        lastException = ex
                        'Exit if it is a network failure check and stop retry to avoid stack overflow
                        'Need to relogin, no point retrying
                        If ExceptionExtensions.GetExceptionMessages(ex).Contains("disposed") Then
                            Throw New ForbiddenException(ex.Message, ex, ForbiddenException.TypeOfException.ExceptionInBetweenLoginProcess)
                        End If

                        If postURL = _internetConnectionCheckerURL Then
                            Exit For
                        End If
                        _canceller.Token.ThrowIfCancellationRequested()
                        If Not Waiter.WaitOnInternetFailure(Me.WaitDurationOnConnectionFailure) Then
                            'Provide required wait in case internet was already up
                            _canceller.Token.ThrowIfCancellationRequested()
                            If ExceptionExtensions.IsExceptionConnectionRelated(ex) Then
                                logger.Debug("HTTP->Exception without internet problem but of type internet related detected:{0}",
                                             ex.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Waiter.SleepRequiredDuration(WaitDurationOnConnectionFailure.TotalSeconds, "Connection Exception")
                                _canceller.Token.ThrowIfCancellationRequested()
                                'Since exception was internet related, no need to consume retries
                                retryCtr -= 1
                            Else
                                logger.Debug("HTTP->Exception without internet problem of unknown type detected:{0}",
                                             ex.Message)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Waiter.SleepRequiredDuration(WaitDurationOnAnyFailure.TotalSeconds, "Unknown Exception")
                                _canceller.Token.ThrowIfCancellationRequested()
                            End If
                        Else
                            logger.Debug("HTTP->Exception with internet problem:{0}, waited prescribed seconds, will now retry",
                                         ex.Message)
                            'Since internet was down, no need to consume retries
                            retryCtr -= 1
                        End If
                    Finally
                        OnDocumentDownloadComplete()
                        If response IsNot Nothing Then response.Dispose()
                        'Console.WriteLine(postURL)
                        'For Each cookie As Cookie In _AllCookies.GetCookies(New Uri("http://kite.zerodha.com"))
                        '    Console.WriteLine("Name = {0} ; Value = {1} ; Domain = {2}", cookie.Name, cookie.Value,
                        '              cookie.Domain)

                        'Next
                    End Try
                    _canceller.Token.ThrowIfCancellationRequested()
                    If retTuple IsNot Nothing Then
                        Exit For
                    End If
                    GC.Collect()
                Next
                RemoveHandler Waiter.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler Waiter.WaitingFor, AddressOf OnWaitingFor
            End Using
            _canceller.Token.ThrowIfCancellationRequested()
            If Not allOKWithoutException Then Throw lastException
            Return retTuple
        End Function
        Public Async Function POSTRequestAsync(ByVal postURL As String,
                                               ByVal referalURL As String,
                                               ByVal postContent As StringContent,
                                               ByVal useRandomUserAgent As Boolean,
                                               ByVal headers As Dictionary(Of String, String),
                                               ByVal checkResponseCode As Boolean) As Task(Of Tuple(Of Uri, Object))
            logger.Debug("Placing POST request with string content asynchronously")
            Dim usableContent As HttpContent = postContent
            Return Await POSTRequestAsync(postURL, referalURL, usableContent, useRandomUserAgent, headers, checkResponseCode).ConfigureAwait(False)
        End Function

        Public Async Function POSTRequestAsync(ByVal postURL As String,
                                               ByVal referalURL As String,
                                               ByVal postContent As Dictionary(Of String, String),
                                               ByVal useRandomUserAgent As Boolean,
                                               ByVal headers As Dictionary(Of String, String),
                                               ByVal checkResponseCode As Boolean) As Task(Of Tuple(Of Uri, Object))
            logger.Debug("Placing POST request with dictionary content asynchronously")
            Dim usableContent As HttpContent = New FormUrlEncodedContent(postContent)
            Return Await POSTRequestAsync(postURL, referalURL, usableContent, useRandomUserAgent, headers, checkResponseCode).ConfigureAwait(False)
        End Function

#End Region

#Region " IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    If _httpInstance IsNot Nothing Then _httpInstance.Dispose()
                    If _httpHandler IsNot Nothing Then _httpHandler = Nothing
                    If _proxyToBeUsed IsNot Nothing Then _proxyToBeUsed.Dispose()

                    _ConnectTimeOut = Nothing
                    Accept = Nothing
                    AcceptLanguage = Nothing
                    AcceptEncoding = Nothing
                    UserAgent = Nothing
                    KeepAlive = Nothing
                    FormPostContentType = Nothing
                    MaxReTries = Nothing
                    WaitDurationOnConnectionFailure = Nothing
                    WaitDurationOnServiceUnavailbleFailure = Nothing
                    WaitDurationOnAnyFailure = Nothing
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