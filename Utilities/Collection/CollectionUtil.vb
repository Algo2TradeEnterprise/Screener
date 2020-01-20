Imports System.Runtime.CompilerServices

Namespace Collections
    Public Module CollectionUtil
        <Extension()>
        Public Function ConcatSingle(Of T)(ByVal e As IEnumerable(Of T), ByVal elem As T) As IEnumerable(Of T)
            Dim arr As T() = New T() {elem}
            If e IsNot Nothing Then
                Return e.Concat(arr)
            Else
                Return Enumerable.Repeat(elem, 1)
            End If
        End Function

    End Module
End Namespace
