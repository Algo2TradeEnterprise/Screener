Imports System.IO
Imports System.Threading
Imports Algo2TradeBLL
Imports MySql.Data.MySqlClient

Public Class PreMarketStocks
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
        ret.Columns.Add("Change %")

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
                    Dim conn As MySqlConnection = Nothing
                    If conn Is Nothing OrElse conn.State <> ConnectionState.Open Then
                        _canceller.Token.ThrowIfCancellationRequested()
                        conn = _cmn.OpenDBConnection()
                    End If
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim cm As MySqlCommand = New MySqlCommand("SELECT * FROM `v_pre_market` WHERE `APPLICABLE_DATE`=@sd", conn)
                    cm.Parameters.AddWithValue("@sd", tradingDate.ToString("yyyy-MM-dd"))
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim adapter As New MySqlDataAdapter(cm)
                    adapter.SelectCommand.CommandTimeout = 300
                    _canceller.Token.ThrowIfCancellationRequested()
                    Dim dt As DataTable = New DataTable
                    adapter.Fill(dt)
                    _canceller.Token.ThrowIfCancellationRequested()
                    If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                        Dim tempStockList As Dictionary(Of String, Decimal()) = Nothing
                        For i = 0 To dt.Rows.Count - 1
                            _canceller.Token.ThrowIfCancellationRequested()
                            Dim instrumentName As String = dt.Rows(i).Item(1).ToString.ToUpper
                            Dim changePer As String = Math.Round(((dt.Rows(i).Item(4) / dt.Rows(i).Item(7)) - 1) * 100, 2)
                            If atrStockList.ContainsKey(instrumentName) Then
                                If tempStockList Is Nothing Then tempStockList = New Dictionary(Of String, Decimal())
                                tempStockList.Add(instrumentName, {changePer, dt.Rows(i).Item(7)})
                            End If
                        Next
                        If tempStockList IsNot Nothing AndAlso tempStockList.Count > 0 Then
                            Dim stockCounter As Integer = 0
                            For Each runningStock In tempStockList.OrderByDescending(Function(x)
                                                                                         Return Math.Abs(x.Value(0))
                                                                                     End Function)
                                _canceller.Token.ThrowIfCancellationRequested()
                                Dim row As DataRow = ret.NewRow
                                row("Date") = tradingDate.ToString("dd-MM-yyyy")
                                row("Trading Symbol") = atrStockList(runningStock.Key).TradingSymbol
                                row("Lot Size") = atrStockList(runningStock.Key).LotSize
                                row("ATR %") = Math.Round(atrStockList(runningStock.Key).ATRPercentage, 4)
                                row("Blank Candle %") = atrStockList(runningStock.Key).BlankCandlePercentage
                                row("Day ATR") = Math.Round(atrStockList(runningStock.Key).DayATR, 4)
                                row("Previous Day Open") = atrStockList(runningStock.Key).PreviousDayOpen
                                row("Previous Day Low") = atrStockList(runningStock.Key).PreviousDayLow
                                row("Previous Day High") = atrStockList(runningStock.Key).PreviousDayHigh
                                row("Previous Day Close") = atrStockList(runningStock.Key).PreviousDayClose
                                row("Slab") = atrStockList(runningStock.Key).Slab
                                row("Change %") = runningStock.Value(0)

                                ret.Rows.Add(row)
                                stockCounter += 1
                                If stockCounter = My.Settings.NumberOfStockPerDay Then Exit For
                            Next
                        End If
                    End If
                End If

                tradingDate = tradingDate.AddDays(1)
            End While
        End Using
        Return ret
    End Function
End Class
