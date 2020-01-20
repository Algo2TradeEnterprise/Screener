Imports System.Data.SqlClient
Imports System.Runtime.CompilerServices
Imports MySql.Data.MySqlClient
Imports NLog

Namespace ErrorHandlers
    Public Module ExceptionExtensions

#Region "Logging and Status Progress"
        Public logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Private Attributes"
#End Region

#Region "Public Attributes"
#End Region

#Region "Private Methods"
#End Region

#Region "Public Methods"
        Public Function IsExceptionConnectionBusyRelated(ByVal ex As Exception) As Boolean
            Dim tx As Exception = ex.GetBaseException
            While tx.InnerException IsNot Nothing
                tx = tx.InnerException
            End While

            If tx.[GetType]().IsAssignableFrom(GetType(SqlException)) AndAlso tx.Message.ToUpper.Contains("timeout".ToUpper) _
            Then
                Return True
            Else
                Return False
            End If
        End Function
        Public Function IsExceptionConnectionRelated(ByVal ex As Exception) As Boolean
            Dim tx As Exception = ex.GetBaseException
            While tx.InnerException IsNot Nothing
                tx = tx.InnerException
            End While

            If tx.[GetType]().IsAssignableFrom(GetType(System.TimeoutException)) Or
               tx.[GetType]().IsAssignableFrom(GetType(System.Net.WebException)) Or
               tx.[GetType]().IsAssignableFrom(GetType(System.Net.Sockets.SocketException)) Or
               tx.[GetType]().IsAssignableFrom(GetType(System.IO.IOException)) Or
               tx.[GetType]().IsAssignableFrom(GetType(System.IO.EndOfStreamException)) _
            Then
                Return True
            Else
                Return False
            End If
        End Function
        Public Function GetExceptionMessages(e As Exception, Optional msgs As String = "") As String
            If e Is Nothing Then
                Return String.Empty
            End If
            If msgs = "" Then
                msgs = e.Message
            End If
            If e.InnerException IsNot Nothing Then
                msgs += Convert.ToString(vbCr & vbLf) & GetExceptionMessages(e.InnerException)
            End If
            Return msgs
        End Function
#End Region

    End Module
End Namespace