Imports System.Net
Imports NLog
Namespace Network
    Public Class HttpProxy
        Implements IDisposable
#Region "Logging and Status Progress"
        Public Shared logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Enums"
        Public Enum ProxyProvider
            Crawlera = 1
        End Enum
#End Region

#Region "Events"
#End Region

#Region "Constructors"
#End Region

#Region "Private Attributes"
#End Region

#Region "Public Attributes"
        Public Property Proxy As WebProxy
        Public Property RotationAfterNumberOfRequests As Integer 'Put the number of requests after which auto rotation will happen
        Public Property Provider As ProxyProvider
#End Region

#Region "Private Methods"
#End Region

#Region "Public Methods"
#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Proxy = Nothing
                    RotationAfterNumberOfRequests = Nothing
                    Provider = Nothing
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
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
