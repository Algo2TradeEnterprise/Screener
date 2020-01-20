Imports System.Runtime.Serialization
Namespace ErrorHandlers
    ''' <summary>
    ''' Custom class for handling business exceptions
    ''' </summary>
    <Serializable()>
    Public Class AuthenticationException
        Inherits ApplicationException
        Implements IDisposable

#Region "Enums"
        Public Enum TypeOfException
            FirstLevelFailure = 1
            SecondLevelFailure
            GenericConnectionFailure
        End Enum
#End Region

#Region "Public Properties"
        Public Property ExceptionType As TypeOfException
        Public Property ExceptionDetails As String
#End Region

#Region "Constructors"
        ''' <summary>
        ''' Default constructor
        ''' </summary>

        Public Sub New()
        End Sub

        ''' <summary>
        ''' Creates object with only the exception message
        ''' </summary>
        ''' <param name="message">Message associated with the exception</param>
        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal exceptionType As TypeOfException)
            MyBase.New(message)
            Me.ExceptionType = exceptionType
        End Sub

        ''' <summary>
        ''' Creates object with only the exception message
        ''' </summary>
        ''' <param name="message">Message associated with the exception</param>
        Public Sub New(ByVal message As String, ByVal exceptionDetails As String)
            MyBase.New(message)
            Me.ExceptionDetails = exceptionDetails
        End Sub

        Public Sub New(ByVal message As String, ByVal exceptionDetails As String, ByVal exceptionType As TypeOfException)
            MyBase.New(message)
            Me.ExceptionType = exceptionType
            Me.ExceptionDetails = exceptionDetails
        End Sub

        ''' <summary>
        ''' Creates object with the exception message and inner exception
        ''' </summary>
        ''' <param name="message">Message associated with the exception</param>
        ''' <param name="inner">Inner exception associated with the exception</param>
        Public Sub New(ByVal message As String, ByVal inner As Exception)
            MyBase.New(message, inner)
        End Sub

        Public Sub New(ByVal message As String, ByVal inner As Exception, ByVal exceptionType As TypeOfException)
            MyBase.New(message, inner)
            Me.ExceptionType = exceptionType
        End Sub

        ''' <summary>
        ''' Creates object with the exception message and inner exception
        ''' </summary>
        ''' <param name="message">Message associated with the exception</param>
        ''' <param name="inner">Inner exception associated with the exception</param>
        Public Sub New(ByVal message As String, ByVal inner As Exception, ByVal exceptionDetails As String)
            MyBase.New(message, inner)
            Me.ExceptionDetails = exceptionDetails
        End Sub

        Public Sub New(ByVal message As String, ByVal inner As Exception, ByVal exceptionDetails As String, ByVal exceptionType As TypeOfException)
            MyBase.New(message, inner)
            Me.ExceptionDetails = exceptionDetails
            Me.ExceptionType = exceptionType
        End Sub

        ''' <summary>
        ''' Creates object which is seralizable
        ''' </summary>
        ''' <param name="info">Serialization information</param>
        ''' <param name="context">Stream to be serialized</param>
        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
            MyBase.New(info, context)
        End Sub
#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    ExceptionDetails = Nothing
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