Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL

Public Class HighATRStocks
    Inherits StockSelection

    Public Sub New(ByVal canceller As CancellationTokenSource,
                   ByVal cmn As Common,
                   ByVal stockType As Integer)
        MyBase.New(canceller, cmn, stockType)
    End Sub

    Public Overrides Async Function GetStockDataAsync(ByVal startDate As Date, ByVal endDate As Date) As Task(Of DataTable)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As New DataTable
        ret.Columns.Add("Date")
        ret.Columns.Add("Trading Symbol")
        ret.Columns.Add("Lot Size")
        ret.Columns.Add("ATR %")
        ret.Columns.Add("Blank Candle %")
        ret.Columns.Add("Day ATR")
        ret.Columns.Add("Previous Day Open")
        ret.Columns.Add("Previous Day Low")
        ret.Columns.Add("Previous Day High")
        ret.Columns.Add("Previous Day Close")
        ret.Columns.Add("Slab")

        Using atrStock As New ATRStockSelection(_canceller)
            AddHandler atrStock.Heartbeat, AddressOf OnHeartbeat

            Dim tradingDate As Date = startDate
            While tradingDate <= endDate
                _bannedStockFileName = Path.Combine(My.Application.Info.DirectoryPath, String.Format("Bannned Stocks {0}.csv", tradingDate.ToString("ddMMyyyy")))
                For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "Bannned Stocks *.csv")
                    If Not runningFile.Contains(tradingDate.ToString("ddMMyyyy")) Then File.Delete(runningFile)
                Next
                Dim bannedStockList As List(Of String) = Nothing
                Using bannedStock As New BannedStockDataFetcher(_bannedStockFileName, _canceller)
                    AddHandler bannedStock.Heartbeat, AddressOf OnHeartbeat
                    bannedStockList = Await bannedStock.GetBannedStocksData(tradingDate).ConfigureAwait(False)
                End Using

                Dim atrStockList As Dictionary(Of String, InstrumentDetails) = Await atrStock.GetATRStockData(_eodTable, tradingDate, bannedStockList, False).ConfigureAwait(False)
                If atrStockList IsNot Nothing AndAlso atrStockList.Count > 0 Then
                    Dim stockCounter As Integer = 0
                    For Each runningStock In atrStockList
                        _canceller.Token.ThrowIfCancellationRequested()
                        Dim row As DataRow = ret.NewRow
                        row("Date") = tradingDate.ToString("dd-MM-yyyy")
                        row("Trading Symbol") = runningStock.Value.TradingSymbol
                        row("Lot Size") = runningStock.Value.LotSize
                        row("ATR %") = Math.Round(runningStock.Value.ATRPercentage, 4)
                        row("Blank Candle %") = runningStock.Value.BlankCandlePercentage
                        row("Day ATR") = Math.Round(runningStock.Value.DayATR, 4)
                        row("Previous Day Open") = runningStock.Value.PreviousDayOpen
                        row("Previous Day Low") = runningStock.Value.PreviousDayLow
                        row("Previous Day High") = runningStock.Value.PreviousDayHigh
                        row("Previous Day Close") = runningStock.Value.PreviousDayClose
                        row("Slab") = runningStock.Value.Slab
                        ret.Rows.Add(row)
                        stockCounter += 1
                        If stockCounter = My.Settings.NumberOfStockPerDay Then Exit For
                    Next
                End If

                tradingDate = tradingDate.AddDays(1)
            End While
        End Using
        Return ret
    End Function
End Class
