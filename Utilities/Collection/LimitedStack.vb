Namespace Collections
    Public Class LimitedStack(Of T)
        Private _limit As Integer
        Private _stack As List(Of T)

        Public Sub New(ByVal Optional limit As Integer = 32)
            _limit = limit
            _stack = New List(Of T)(_limit)
        End Sub

        Public Sub Push(ByVal item As T)
            Try
                If _stack.Count = _limit Then _stack.RemoveAt(0)
                _stack.Add(item)
            Catch ex As Exception
                Throw ex
            End Try
        End Sub

        Public Function Peek() As T
            If _stack.Count > 0 Then
                Return _stack(_stack.Count - 1)
            Else
                Return Nothing
            End If
        End Function

        Public Sub Pop()
            If _stack.Count > 0 Then
                _stack.RemoveAt(_stack.Count - 1)
            Else
                Throw New ApplicationException("Cannot peek as no items are present")
            End If
        End Sub

        Public ReadOnly Property Count As Integer
            Get
                Return _stack.Count
            End Get
        End Property
    End Class
End Namespace