Imports Utilities.Network
Imports Utilities.DAL
Imports System.Threading
Imports System.IO
Public Class BannedStockDataFetcher
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

    Private ReadOnly _cts As CancellationTokenSource
    Private ReadOnly _bannedStockFileName As String

    Public Sub New(ByVal bannedStockFileName As String, ByVal canceller As CancellationTokenSource)
        _bannedStockFileName = bannedStockFileName
        _cts = canceller
    End Sub

    Private Function GetBannedStockURL(ByVal tradingDate As Date) As String
        Dim ret As String = Nothing
        Dim bannedStockURL As String = "https://www.nseindia.com/archives/fo/sec_ban/fo_secban_{0}.csv"
        If tradingDate <> Date.MinValue Then
            ret = String.Format(bannedStockURL, tradingDate.ToString("ddMMyyyy"))
        End If
        Return ret
    End Function

    Private Async Function GetBannedStockFileAsync(ByVal tradingDate As Date) As Task(Of Boolean)
        Dim ret As Boolean = False
        If File.Exists(_bannedStockFileName) Then
            If File.ReadAllText(_bannedStockFileName).ToUpper.Contains("Not Found".ToUpper) Then
                ret = False
            Else
                ret = True
            End If
        Else
            Using browser As New HttpBrowser(Nothing, Net.DecompressionMethods.GZip, TimeSpan.FromSeconds(30), _cts)
                AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
                AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
                AddHandler browser.WaitingFor, AddressOf OnWaitingFor
                AddHandler browser.Heartbeat, AddressOf OnHeartbeat

                browser.KeepAlive = True
                Dim headersToBeSent As New Dictionary(Of String, String)
                headersToBeSent.Add("Host", "www.nseindia.com")
                headersToBeSent.Add("Upgrade-Insecure-Requests", "1")
                headersToBeSent.Add("Sec-Fetch-Mode", "navigate")
                headersToBeSent.Add("Sec-Fetch-Site", "none")

                Dim targetURL As String = GetBannedStockURL(tradingDate)
                If targetURL IsNot Nothing Then
                    Dim innerRet As Boolean = Await browser.GetFileAsync(targetURL, _bannedStockFileName, False, headersToBeSent).ConfigureAwait(False)
                    If innerRet AndAlso File.ReadAllText(_bannedStockFileName).ToUpper.Contains("Not Found".ToUpper) Then
                        ret = False
                    ElseIf innerRet Then
                        ret = True
                    End If
                End If
            End Using
        End If
        Return ret
    End Function
    Public Async Function GetBannedStocksData(ByVal tradingDate As Date) As Task(Of List(Of String))
        Dim ret As List(Of String) = Nothing
        Dim bannedStockAvailable As Boolean = Await GetBannedStockFileAsync(tradingDate).ConfigureAwait(False)
        If bannedStockAvailable AndAlso File.Exists(_bannedStockFileName) Then
            If Not File.ReadAllText(_bannedStockFileName).ToUpper.Contains("Not Found".ToUpper) Then
                Dim dt As DataTable = Nothing
                Using csv As New CSVHelper(_bannedStockFileName, ",", _cts)
                    AddHandler csv.Heartbeat, AddressOf OnHeartbeat
                    dt = csv.GetDataTableFromCSV(2)
                End Using
                If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                    For i = 0 To dt.Rows.Count - 1
                        If ret Is Nothing Then ret = New List(Of String)
                        ret.Add(dt.Rows(i).Item(1).ToString.ToUpper)
                    Next
                End If
            End If
        End If
        Return ret
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
