Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class UserGivenStocks
    Inherits StockSelection

    Private ReadOnly _stockList As List(Of String) = Nothing

    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer,
                   ByVal instrumentList As List(Of String))
        MyBase.New(canceller, cmn, stockType)
        _stockList = instrumentList
    End Sub

    Public Overrides Async Function GetStockDataAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Lot Size")

        Dim tradingDate As Date = startDate
        While tradingDate <= endDate
            If _stockList IsNot Nothing AndAlso _stockList.Count > 0 Then
                _canceller.Token.ThrowIfCancellationRequested()
                Dim tempStockList As Dictionary(Of String, String()) = Nothing
                For Each runningStock In _stockList
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim currentTradingSymbol As String = _cmn.GetCurrentTradingSymbol(_eodTable, tradingDate, runningStock)
                    Dim lotSize As Integer = _cmn.GetLotSize(_eodTable, currentTradingSymbol, tradingDate)
                    If currentTradingSymbol IsNot Nothing AndAlso lotSize <> Integer.MinValue Then
                        If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, String())
                        tempStockList.Add(currentTradingSymbol, {lotSize})
                    End If
                Next
                If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                    Dim stockCounter As Integer = 0
                    For Each runningStock In tempStockList
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim row As DataRow = ret.NewRow
                        row("Date") = tradingDate.ToString("dd-MM-yyyy")
                        row("Trading Symbol") = runningStock.Key
                        row("Lot Size") = runningStock.Value(0)

                        ret.Rows.Add(row)
                    Next
                End If
            End If

            tradingDate = tradingDate.AddDays(1)
        End While

        Return ret
    End Function
End Class
