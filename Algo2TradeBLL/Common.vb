Imports System.IO
Imports System.Net
Imports System.Threading
Imports MySql.Data.MySqlClient
Imports Utilities.Strings
Imports NLog

Public Class Common
    Implements IDisposable
    Dim conn As MySqlConnection

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
    Public Sub New(calceller As CancellationTokenSource)
        _cts = calceller
    End Sub

#Region "Enum"
    Public Enum DataBaseTable
        None = 1
        Intraday_Cash
        Intraday_Commodity
        Intraday_Currency
        Intraday_Futures
        EOD_Cash
        EOD_Commodity
        EOD_Currency
        EOD_Futures
        EOD_POSITIONAL
    End Enum
#End Region

#Region "Inner Class"
    Public Class CamarillaPivotPoints
        Public Property H1 As Decimal
        Public Property H2 As Decimal
        Public Property H3 As Decimal
        Public Property H4 As Decimal
        Public Property L1 As Decimal
        Public Property L2 As Decimal
        Public Property L3 As Decimal
        Public Property L4 As Decimal
    End Class

    Private Class ActiveInstrumentData
        Public Token As String
        Public TradingSymbol As String
        Public Expiry As Date
    End Class
#End Region

#Region "Public Shared Functions"
    Public Shared Function GetPayloadAtPositionOrPositionMinus1(ByVal beforeThisTime As DateTime, ByVal inputPayload As Dictionary(Of Date, Decimal)) As KeyValuePair(Of DateTime, Decimal)
        Dim ret As KeyValuePair(Of DateTime, Decimal) = Nothing
        If inputPayload IsNot Nothing Then
            Dim tempret = inputPayload.Where(Function(x)
                                                 Return x.Key < beforeThisTime
                                             End Function)
            If tempret IsNot Nothing Then
                ret = tempret.LastOrDefault
            End If
        End If
        Return ret
    End Function

    Public Shared Function GetSubPayload(ByVal inputPayload As Dictionary(Of Date, Decimal),
                                     ByVal beforeThisTime As DateTime,
                                      ByVal numberOfItemsToRetrive As Integer,
                                      ByVal includeTimePassedAsOneOftheItems As Boolean) As List(Of KeyValuePair(Of DateTime, Decimal))
        Dim ret As List(Of KeyValuePair(Of DateTime, Decimal)) = Nothing
        If inputPayload IsNot Nothing Then
            'Find the index of the time passed
            Dim firstIndexOfKey As Integer = -1
            Dim loopTerminatedOnCondition As Boolean = False
            For Each item In inputPayload
                firstIndexOfKey += 1
                If item.Key >= beforeThisTime Then
                    loopTerminatedOnCondition = True
                    Exit For
                End If
            Next
            If loopTerminatedOnCondition Then 'Specially useful for only 1 count of item is there
                If Not includeTimePassedAsOneOftheItems Then
                    firstIndexOfKey -= 1
                End If
            End If
            If firstIndexOfKey >= 0 Then
                Dim startIndex As Integer = Math.Max((firstIndexOfKey - numberOfItemsToRetrive) + 1, 0)
                Dim revisedNumberOfItemsToRetrieve As Integer = Math.Min(numberOfItemsToRetrive, (firstIndexOfKey - startIndex) + 1)
                Dim referencePayLoadAsList = inputPayload.ToList
                ret = referencePayLoadAsList.GetRange(startIndex, revisedNumberOfItemsToRetrieve)
            End If
        End If
        Return ret
    End Function

    Public Shared Function GetSubPayload(ByVal inputPayload As Dictionary(Of Date, Payload),
                                     ByVal beforeThisTime As DateTime,
                                      ByVal numberOfItemsToRetrive As Integer,
                                      ByVal includeTimePassedAsOneOftheItems As Boolean) As List(Of KeyValuePair(Of DateTime, Payload))
        Dim ret As List(Of KeyValuePair(Of DateTime, Payload)) = Nothing
        If inputPayload IsNot Nothing Then
            'Find the index of the time passed
            Dim firstIndexOfKey As Integer = -1
            Dim loopTerminatedOnCondition As Boolean = False
            For Each item In inputPayload

                firstIndexOfKey += 1
                If item.Key >= beforeThisTime Then
                    loopTerminatedOnCondition = True
                    Exit For
                End If
            Next
            If loopTerminatedOnCondition Then 'Specially useful for only 1 count of item is there
                If Not includeTimePassedAsOneOftheItems Then
                    firstIndexOfKey -= 1
                End If
            End If
            If firstIndexOfKey >= 0 Then
                Dim startIndex As Integer = Math.Max((firstIndexOfKey - numberOfItemsToRetrive) + 1, 0)
                Dim revisedNumberOfItemsToRetrieve As Integer = Math.Min(numberOfItemsToRetrive, (firstIndexOfKey - startIndex) + 1)
                Dim referencePayLoadAsList = inputPayload.ToList
                ret = referencePayLoadAsList.GetRange(startIndex, revisedNumberOfItemsToRetrieve)
            End If
        End If
        Return ret
    End Function

    Public Shared Function GetPayloadAt(ByVal inputPayload As Dictionary(Of Date, Payload),
                                        ByVal currentTime As Date,
                                        ByVal positionToRetrive As Integer) As KeyValuePair(Of Date, Payload)?
        Dim ret As KeyValuePair(Of Date, Payload)? = Nothing
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 AndAlso inputPayload.ContainsKey(currentTime) Then
            'Find the index of the time passed
            Dim firstIndexOfKey As Integer = Array.IndexOf(inputPayload.Keys.ToArray, currentTime)
            Dim indexToRetrive As Integer = firstIndexOfKey + positionToRetrive
            If positionToRetrive > 0 Then
                indexToRetrive = indexToRetrive - 1
            ElseIf positionToRetrive < 0 Then
                indexToRetrive = indexToRetrive + 1
            End If
            If indexToRetrive >= 0 AndAlso indexToRetrive < inputPayload.Count Then
                Dim retrivedDate As Date = inputPayload.Keys.ToArray(indexToRetrive)
                ret = New KeyValuePair(Of Date, Payload)(retrivedDate, inputPayload(retrivedDate))
            End If
        End If
        Return ret
    End Function

    Public Shared Function GetPayloadAt(ByVal inputPayload As Dictionary(Of Date, Decimal),
                                        ByVal currentTime As Date,
                                        ByVal positionToRetrive As Integer) As KeyValuePair(Of Date, Decimal)?
        Dim ret As KeyValuePair(Of Date, Decimal)? = Nothing
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 AndAlso inputPayload.ContainsKey(currentTime) Then
            'Find the index of the time passed
            Dim firstIndexOfKey As Integer = Array.IndexOf(inputPayload.Keys.ToArray, currentTime)
            Dim indexToRetrive As Integer = firstIndexOfKey + positionToRetrive
            If positionToRetrive > 0 Then
                indexToRetrive = indexToRetrive - 1
            ElseIf positionToRetrive < 0 Then
                indexToRetrive = indexToRetrive + 1
            End If
            If indexToRetrive >= 0 AndAlso indexToRetrive < inputPayload.Count Then
                Dim retrivedDate As Date = inputPayload.Keys.ToArray(indexToRetrive)
                ret = New KeyValuePair(Of Date, Decimal)(retrivedDate, inputPayload(retrivedDate))
            End If
        End If
        Return ret
    End Function

    Public Shared Function ConvertPayloadsToXMinutes(ByVal inputPayloads As Dictionary(Of Date, Payload), ByVal timeframe As Integer, ByVal exchangeStartTime As Date) As Dictionary(Of Date, Payload)
        Dim ret As Dictionary(Of Date, Payload) = Nothing
        If inputPayloads IsNot Nothing AndAlso inputPayloads.Count > 0 Then
            Dim previousCandlePayload As Payload = Nothing
            For Each runningPayload In inputPayloads.Values
                Dim blockDateInThisTimeframe As Date = Date.MinValue
                If exchangeStartTime.Minute Mod timeframe = 0 Then
                    blockDateInThisTimeframe = New Date(runningPayload.PayloadDate.Year,
                                                        runningPayload.PayloadDate.Month,
                                                        runningPayload.PayloadDate.Day,
                                                        runningPayload.PayloadDate.Hour,
                                                        Math.Floor(runningPayload.PayloadDate.Minute / timeframe) * timeframe, 0)
                Else
                    Dim exchangeTime As Date = New Date(runningPayload.PayloadDate.Year, runningPayload.PayloadDate.Month, runningPayload.PayloadDate.Day, exchangeStartTime.Hour, exchangeStartTime.Minute, 0)
                    Dim currentTime As Date = New Date(runningPayload.PayloadDate.Year, runningPayload.PayloadDate.Month, runningPayload.PayloadDate.Day, runningPayload.PayloadDate.Hour, runningPayload.PayloadDate.Minute, 0)
                    Dim timeDifference As Double = currentTime.Subtract(exchangeTime).TotalMinutes
                    Dim adjustedTimeDifference As Integer = Math.Floor(timeDifference / timeframe) * timeframe
                    Dim currentMinute As Date = exchangeTime.AddMinutes(adjustedTimeDifference)
                    blockDateInThisTimeframe = New Date(runningPayload.PayloadDate.Year,
                                                        runningPayload.PayloadDate.Month,
                                                        runningPayload.PayloadDate.Day,
                                                        currentMinute.Hour,
                                                        currentMinute.Minute, 0)
                End If
                If blockDateInThisTimeframe <> Date.MinValue Then
                    If ret Is Nothing Then ret = New Dictionary(Of Date, Payload)
                    If Not ret.ContainsKey(blockDateInThisTimeframe) Then
                        Dim xMinutePayload As Payload = New Payload(Payload.CandleDataSource.Calculated)
                        xMinutePayload.PayloadDate = blockDateInThisTimeframe
                        xMinutePayload.Open = runningPayload.Open
                        xMinutePayload.High = runningPayload.High
                        xMinutePayload.Low = runningPayload.Low
                        xMinutePayload.Close = runningPayload.Close
                        xMinutePayload.Volume = runningPayload.Volume
                        xMinutePayload.TradingSymbol = runningPayload.TradingSymbol
                        xMinutePayload.PreviousCandlePayload = previousCandlePayload

                        ret.Add(blockDateInThisTimeframe, xMinutePayload)
                        previousCandlePayload = xMinutePayload
                    Else
                        Dim xMinutePayload As Payload = ret(blockDateInThisTimeframe)
                        xMinutePayload.High = Math.Max(xMinutePayload.High, runningPayload.High)
                        xMinutePayload.Low = Math.Min(xMinutePayload.Low, runningPayload.Low)
                        xMinutePayload.Close = runningPayload.Close
                        xMinutePayload.Volume = xMinutePayload.Volume + runningPayload.Volume
                    End If

                    Dim currentXMinutePayload As Payload = ret(blockDateInThisTimeframe)
                    If currentXMinutePayload.PreviousCandlePayload Is Nothing Then
                        currentXMinutePayload.CumulativeVolume = currentXMinutePayload.Volume
                    ElseIf currentXMinutePayload.PreviousCandlePayload IsNot Nothing AndAlso currentXMinutePayload.PayloadDate.Date <> currentXMinutePayload.PreviousCandlePayload.PayloadDate.Date Then
                        currentXMinutePayload.CumulativeVolume = currentXMinutePayload.Volume
                    ElseIf currentXMinutePayload.PreviousCandlePayload IsNot Nothing AndAlso currentXMinutePayload.PayloadDate.Date = currentXMinutePayload.PreviousCandlePayload.PayloadDate.Date Then
                        currentXMinutePayload.CumulativeVolume = currentXMinutePayload.PreviousCandlePayload.CumulativeVolume + currentXMinutePayload.Volume
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Public Shared Function ConvertDecimalToPayload(ByVal targetfield As Payload.PayloadFields, ByVal inputpayload As Dictionary(Of Date, Decimal), ByRef outputpayload As Dictionary(Of Date, Payload))
        Dim output As Payload
        outputpayload = New Dictionary(Of Date, Payload)
        For Each runningitem In inputpayload
            output = New Payload(Payload.CandleDataSource.Chart)
            output.PayloadDate = runningitem.Key
            Select Case targetfield
                Case Payload.PayloadFields.Close
                    output.Close = runningitem.Value
                Case Payload.PayloadFields.C_AVG_HL
                    output.C_AVG_HL = runningitem.Value
                Case Payload.PayloadFields.High
                    output.High = runningitem.Value
                Case Payload.PayloadFields.H_L
                    output.H_L = runningitem.Value
                Case Payload.PayloadFields.Low
                    output.Low = runningitem.Value
                Case Payload.PayloadFields.Open
                    output.Open = runningitem.Value
                Case Payload.PayloadFields.Volume
                    output.Volume = runningitem.Value
                Case Payload.PayloadFields.SMI_EMA
                    output.SMI_EMA = runningitem.Value
                Case Payload.PayloadFields.Additional_Field
                    output.Additional_Field = runningitem.Value
            End Select
            outputpayload.Add(runningitem.Key, output)
        Next
        Return Nothing
    End Function

    Public Shared Function ConvertDataTableToPayload(ByVal dt As DataTable,
                                              ByVal openColumnIndex As Integer,
                                              ByVal lowColumnIndex As Integer,
                                              ByVal highColumnIndex As Integer,
                                              ByVal closeColumnIndex As Integer,
                                              ByVal volumeColumnIndex As Integer,
                                              ByVal dateColumnIndex As Integer,
                                              ByVal tradingSymbolColumnIndex As Integer) As Dictionary(Of Date, Payload)

        Dim inputpayload As Dictionary(Of Date, Payload) = Nothing

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim i As Integer = 0
            Dim cur_cum_vol As Long = Nothing
            inputpayload = New Dictionary(Of Date, Payload)
            Dim tempPreCandle As Payload = Nothing
            While Not i = dt.Rows.Count()

                Dim tempPayload As Payload
                tempPayload = New Payload(Payload.CandleDataSource.Chart)
                tempPayload.PreviousCandlePayload = tempPreCandle
                tempPayload.Open = dt.Rows(i).Item(openColumnIndex)
                tempPayload.Low = dt.Rows(i).Item(lowColumnIndex)
                tempPayload.High = dt.Rows(i).Item(highColumnIndex)
                tempPayload.Close = dt.Rows(i).Item(closeColumnIndex)
                tempPayload.PayloadDate = dt.Rows(i).Item(dateColumnIndex)
                tempPayload.TradingSymbol = dt.Rows(i).Item(tradingSymbolColumnIndex)
                If tempPayload.PreviousCandlePayload IsNot Nothing Then
                    If tempPayload.PayloadDate.Date = tempPayload.PreviousCandlePayload.PayloadDate.Date Then
                        tempPayload.CumulativeVolume = tempPayload.PreviousCandlePayload.CumulativeVolume + dt.Rows(i).Item(volumeColumnIndex)
                    Else
                        tempPayload.CumulativeVolume = dt.Rows(i).Item(volumeColumnIndex)
                    End If
                Else
                    tempPayload.CumulativeVolume = dt.Rows(i).Item(volumeColumnIndex)
                End If
                tempPreCandle = tempPayload
                inputpayload.Add(dt.Rows(i).Item(dateColumnIndex), tempPayload)
                i += 1
            End While
        End If
        Return inputpayload
    End Function

    Public Shared Function CalculateStandardDeviation(ByVal inputPayload As Dictionary(Of Date, Decimal)) As Double
        Dim ret As Double = Nothing
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim sum As Double = 0
            For Each runningPayload In inputPayload.Keys
                sum = sum + inputPayload(runningPayload)
            Next
            Dim mean As Double = sum / inputPayload.Count
            Dim sumVariance As Double = 0
            For Each runningPayload In inputPayload.Keys
                sumVariance = sumVariance + Math.Pow((inputPayload(runningPayload) - mean), 2)
            Next
            Dim sampleVariance As Double = sumVariance / (inputPayload.Count - 1)
            Dim standardDeviation As Double = Math.Sqrt(sampleVariance)
            ret = standardDeviation
        End If
        Return Math.Round(ret, 4)
    End Function

    Public Shared Function CalculateStandardDeviation(ParamArray numbers() As Double) As Double
        Dim ret As Double = Nothing
        If numbers.Count > 0 Then
            Dim sum As Double = 0
            For i = 0 To numbers.Count - 1
                sum = sum + numbers(i)
            Next
            Dim mean As Double = sum / numbers.Count
            Dim sumVariance As Double = 0
            For j = 0 To numbers.Count - 1
                sumVariance = sumVariance + Math.Pow((numbers(j) - mean), 2)
            Next
            Dim sampleVariance As Double = sumVariance / (numbers.Count - 1)
            Dim standardDeviation As Double = Math.Sqrt(sampleVariance)
            ret = standardDeviation
        End If
        Return Math.Round(ret, 4)
    End Function

    Public Shared Function CalculateStandardDeviationPA(ByVal inputPayload As Dictionary(Of Date, Decimal)) As Double
        Dim ret As Double = Nothing
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim sum As Double = 0
            For Each runningPayload In inputPayload.Keys
                sum = sum + inputPayload(runningPayload)
            Next
            Dim mean As Double = sum / inputPayload.Count
            Dim sumVariance As Double = 0
            For Each runningPayload In inputPayload.Keys
                sumVariance = sumVariance + Math.Pow((inputPayload(runningPayload) - mean), 2)
            Next
            Dim sampleVariance As Double = sumVariance / (inputPayload.Count)
            Dim standardDeviation As Double = Math.Sqrt(sampleVariance)
            ret = standardDeviation
        End If
        Return Math.Round(ret, 4)
    End Function

    Public Shared Function CalculateCamarillaPivotPoints(ByVal high As Decimal, ByVal low As Decimal, ByVal close As Decimal) As CamarillaPivotPoints
        Dim ret As CamarillaPivotPoints = Nothing
        ret = New CamarillaPivotPoints With
        {
            .H4 = close + ((high - low) * 0.55),
            .H3 = close + ((high - low) * 0.275),
            .H2 = close + ((high - low) * 0.183),
            .H1 = close + ((high - low) * 0.0916),
            .L4 = close - ((high - low) * 0.55),
            .L3 = close - ((high - low) * 0.275),
            .L2 = close - ((high - low) * 0.183),
            .L1 = close - ((high - low) * 0.0916)
        }
        Return ret
    End Function

#End Region

#Region "Public Functions"
    Public Function GetRawPayload(ByVal tableName As DataBaseTable, ByVal rawInstrumentName As String, ByVal startDate As Date, ByVal endDate As Date) As Dictionary(Of Date, Payload)
        Dim ret As Dictionary(Of Date, Payload) = Nothing
        Dim dt As DataTable = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing
        Dim connectionString As String = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim currentTradingSymbol As String = GetCurrentTradingSymbol(tableName, endDate, rawInstrumentName)
        If tableName = DataBaseTable.EOD_POSITIONAL Then currentTradingSymbol = rawInstrumentName
        _cts.Token.ThrowIfCancellationRequested()
        Select Case tableName
            Case DataBaseTable.Intraday_Cash
                connectionString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
            Case DataBaseTable.Intraday_Currency
                connectionString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
            Case DataBaseTable.Intraday_Commodity
                connectionString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
            Case DataBaseTable.Intraday_Futures
                connectionString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
            Case DataBaseTable.EOD_Cash
                connectionString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
            Case DataBaseTable.EOD_Currency
                connectionString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
            Case DataBaseTable.EOD_Commodity
                connectionString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
            Case DataBaseTable.EOD_Futures
                connectionString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
            Case DataBaseTable.EOD_POSITIONAL
                connectionString = String.Format("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_positional_data` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<='{0}' AND `SnapshotDate`>='{1}'", endDate.ToString("yyyy-MM-dd"), startDate.ToString("yyyy-MM-dd"))
        End Select
        cm = New MySqlCommand(connectionString, conn)

        _cts.Token.ThrowIfCancellationRequested()
        If currentTradingSymbol IsNot Nothing Then
            OnHeartbeat(String.Format("Fetching raw candle data from DataBase for {0} on {1}", currentTradingSymbol, endDate.ToShortDateString))
            cm.Parameters.AddWithValue("@trd", currentTradingSymbol)
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            dt = New DataTable()
            adapter.Fill(dt)
            _cts.Token.ThrowIfCancellationRequested()
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                ret = ConvertDataTableToPayload(dt, 0, 1, 2, 3, 4, 5, 6)
            End If
            _cts.Token.ThrowIfCancellationRequested()
        End If
        Return ret
    End Function

    Public Function GetRawPayloadForSpecificTradingSymbol(ByVal tableName As DataBaseTable, ByVal tradingSymbol As String, ByVal startDate As Date, ByVal endDate As Date) As Dictionary(Of Date, Payload)
        Dim ret As Dictionary(Of Date, Payload) = Nothing
        Dim dt As DataTable = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing

        _cts.Token.ThrowIfCancellationRequested()
        Select Case tableName
            Case DataBaseTable.Intraday_Cash
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Currency
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Commodity
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Futures
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDateTime`,`TradingSymbol` FROM `intraday_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Cash
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Currency
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Commodity
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Futures
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_POSITIONAL
                cm = New MySqlCommand("SELECT `Open`,`Low`,`High`,`Close`,`Volume`,`SnapshotDate`,`TradingSymbol` FROM `eod_positional_data` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<=@ed AND `SnapshotDate`>=@sd", conn)
        End Select

        _cts.Token.ThrowIfCancellationRequested()
        If tradingSymbol IsNot Nothing Then
            OnHeartbeat(String.Format("Fetching raw candle data from DataBase for {0} on {1}", tradingSymbol, endDate.ToShortDateString))
            cm.Parameters.AddWithValue("@trd", tradingSymbol)
            cm.Parameters.AddWithValue("@ed", endDate.ToString("yyyy-MM-dd"))
            cm.Parameters.AddWithValue("@sd", startDate.ToString("yyyy-MM-dd"))
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            dt = New DataTable()
            adapter.Fill(dt)
            _cts.Token.ThrowIfCancellationRequested()
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                ret = ConvertDataTableToPayload(dt, 0, 1, 2, 3, 4, 5, 6)
            End If
            _cts.Token.ThrowIfCancellationRequested()
        End If
        Return ret
    End Function

    Public Function GetPreviousTradingDay(ByVal tableName As DataBaseTable, ByVal tradingSymbol As String, ByVal currentDate As Date) As Date
        Dim ret As Date = Date.MinValue
        Dim dt As DataTable = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing

        _cts.Token.ThrowIfCancellationRequested()
        Select Case tableName
            Case DataBaseTable.Intraday_Cash
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Currency
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Commodity
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Futures
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Cash
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_cash` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Currency
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_currency` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Commodity
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_commodity` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Futures
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_futures` WHERE `TradingSymbol`=@trd AND `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
        End Select

        _cts.Token.ThrowIfCancellationRequested()
        OnHeartbeat(String.Format("Getting previous trading day from DataBase for {0} on {1}", tradingSymbol, currentDate.ToShortDateString))
        cm.Parameters.AddWithValue("@trd", tradingSymbol)
        cm.Parameters.AddWithValue("@ed", currentDate.ToString("yyyy-MM-dd"))
        cm.Parameters.AddWithValue("@sd", currentDate.Date.AddDays(-15).ToString("yyyy-MM-dd"))
        Dim adapter As New MySqlDataAdapter(cm)
        adapter.SelectCommand.CommandTimeout = 300
        dt = New DataTable()
        adapter.Fill(dt)
        _cts.Token.ThrowIfCancellationRequested()
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 AndAlso Not IsDBNull(dt.Rows(0).Item(0)) Then
            ret = dt.Rows(0).Item(0)
        End If
        _cts.Token.ThrowIfCancellationRequested()
        Return ret
    End Function

    Public Function GetPreviousTradingDay(ByVal tableName As DataBaseTable, ByVal currentDate As Date) As Date
        Dim ret As Date = Date.MinValue
        Dim dt As DataTable = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_cash` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Currency
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_currency` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Commodity
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_commodity` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.Intraday_Futures
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `intraday_prices_futures` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Cash
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_cash` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Currency
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_currency` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Commodity
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_commodity` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
            Case DataBaseTable.EOD_Futures
                cm = New MySqlCommand("SELECT MAX(`SnapshotDate`) FROM `eod_prices_futures` WHERE `SnapshotDate`<@ed AND `SnapshotDate`>=@sd", conn)
        End Select

        OnHeartbeat(String.Format("Getting previous trading day from DataBase for {0}", currentDate.ToShortDateString))

        cm.Parameters.AddWithValue("@ed", currentDate.Date.ToString("yyyy-MM-dd"))
        cm.Parameters.AddWithValue("@sd", currentDate.Date.AddDays(-15).ToString("yyyy-MM-dd"))
        Dim adapter As New MySqlDataAdapter(cm)
        adapter.SelectCommand.CommandTimeout = 300
        dt = New DataTable()
        adapter.Fill(dt)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            ret = dt.Rows(0).Item(0)
        End If

        Return ret
    End Function

    Public Function GetCurrentTradingSymbolWithInstrumentToken(ByVal tableName As DataBaseTable, ByVal tradingDate As Date, ByVal rawInstrumentName As String) As Tuple(Of String, String)
        Dim ret As Tuple(Of String, String) = Nothing
        Dim dt As DataTable = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing
        Dim activeInstruments As List(Of ActiveInstrumentData) = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash, DataBaseTable.EOD_Cash, DataBaseTable.EOD_POSITIONAL
                cm = New MySqlCommand("SELECT DISTINCT(`INSTRUMENT_TOKEN`),`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_cash` WHERE `TRADING_SYMBOL` = @trd AND `AS_ON_DATE`<=@sd AND `AS_ON_DATE`>=@ed", conn)
                cm.Parameters.AddWithValue("@trd", String.Format("{0}", rawInstrumentName))
                cm.Parameters.AddWithValue("@ed", tradingDate.AddDays(-15).ToString("yyyy-MM-dd"))
            Case DataBaseTable.Intraday_Currency, DataBaseTable.EOD_Currency
                cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_currency` WHERE `TRADING_SYMBOL` REGEXP @trd AND `SEGMENT`='CDS-FUT' AND `AS_ON_DATE`=@sd", conn)
                cm.Parameters.AddWithValue("@trd", String.Format("^{0}[0-9][0-9]*", rawInstrumentName))
            Case DataBaseTable.Intraday_Commodity, DataBaseTable.EOD_Commodity
                cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_commodity` WHERE `TRADING_SYMBOL` REGEXP @trd AND `SEGMENT`='MCX' AND `AS_ON_DATE`=@sd", conn)
                cm.Parameters.AddWithValue("@trd", String.Format("^{0}[0-9][0-9]*", rawInstrumentName))
            Case DataBaseTable.Intraday_Futures, DataBaseTable.EOD_Futures
                cm = New MySqlCommand("SELECT `INSTRUMENT_TOKEN`,`TRADING_SYMBOL`,`EXPIRY` FROM `active_instruments_futures` WHERE `TRADING_SYMBOL` REGEXP @trd AND `SEGMENT`='NFO-FUT' AND `AS_ON_DATE`=@sd", conn)
                cm.Parameters.AddWithValue("@trd", String.Format("^{0}[0-9][0-9]*", rawInstrumentName))
        End Select

        OnHeartbeat(String.Format("Getting current trading symbol and token from DataBase for {0} on {1}", rawInstrumentName, tradingDate.ToShortDateString))

        cm.Parameters.AddWithValue("@sd", tradingDate.Date.ToString("yyyy-MM-dd"))
        Dim adapter As New MySqlDataAdapter(cm)
        adapter.SelectCommand.CommandTimeout = 300
        dt = New DataTable()
        adapter.Fill(dt)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            For i = 0 To dt.Rows.Count - 1
                Dim instrumentData As New ActiveInstrumentData With
                        {.Token = dt.Rows(i).Item(0),
                         .TradingSymbol = dt.Rows(i).Item(1).ToString.ToUpper,
                         .Expiry = If(IsDBNull(dt.Rows(i).Item(2)), Date.MaxValue, dt.Rows(i).Item(2))}
                If activeInstruments Is Nothing Then activeInstruments = New List(Of ActiveInstrumentData)
                activeInstruments.Add(instrumentData)
            Next
        End If
        If activeInstruments IsNot Nothing AndAlso activeInstruments.Count > 0 Then
            Dim minExipry As Date = activeInstruments.Min(Function(x)
                                                              If x.Expiry.Date <= tradingDate.Date Then
                                                                  Return Date.MaxValue
                                                              Else
                                                                  Return x.Expiry
                                                              End If
                                                          End Function)
            Dim currentInstrument As ActiveInstrumentData = activeInstruments.Find(Function(x)
                                                                                       Return x.Expiry = minExipry
                                                                                   End Function)
            If currentInstrument IsNot Nothing Then
                ret = New Tuple(Of String, String)(currentInstrument.TradingSymbol, currentInstrument.Token)
            End If
        End If
        Return ret
    End Function

    Public Function GetCurrentTradingSymbol(ByVal tableName As DataBaseTable, ByVal tradingDate As Date, ByVal rawInstrumentName As String) As String
        Dim ret As String = Nothing
        Dim tradingSymbolWithToken As Tuple(Of String, String) = GetCurrentTradingSymbolWithInstrumentToken(tableName, tradingDate, rawInstrumentName)
        If tradingSymbolWithToken IsNot Nothing Then ret = tradingSymbolWithToken.Item1
        Return ret
    End Function

    Public Function GetLotSize(ByVal tableName As DataBaseTable, ByVal tradingSymbol As String, ByVal currentDate As Date) As Integer
        Dim ret As Integer = Integer.MinValue
        Dim dt As DataTable = Nothing
        Dim conn As MySqlConnection = OpenDBConnection()
        Dim cm As MySqlCommand = Nothing

        Select Case tableName
            Case DataBaseTable.Intraday_Cash, DataBaseTable.EOD_Cash
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_cash` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
            Case DataBaseTable.Intraday_Currency, DataBaseTable.EOD_Currency
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_currency` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
            Case DataBaseTable.Intraday_Commodity, DataBaseTable.EOD_Commodity
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_commodity` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
            Case DataBaseTable.Intraday_Futures, DataBaseTable.EOD_Futures
                cm = New MySqlCommand("SELECT `LOT_SIZE` FROM `active_instruments_futures` WHERE `TRADING_SYMBOL`=@trd AND `AS_ON_DATE`=@sd", conn)
        End Select

        OnHeartbeat(String.Format("Getting Lot Size from DataBase for {0} on {1}", tradingSymbol, currentDate.ToShortDateString))

        If tradingSymbol IsNot Nothing Then
            cm.Parameters.AddWithValue("@trd", tradingSymbol)
            cm.Parameters.AddWithValue("@sd", currentDate.ToString("yyyy-MM-dd"))
            Dim adapter As New MySqlDataAdapter(cm)
            adapter.SelectCommand.CommandTimeout = 300
            dt = New DataTable()
            adapter.Fill(dt)
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                ret = dt.Rows(0).Item(0)
            End If
        End If
        Return ret
    End Function

    Public Async Function GetHistoricalDataAsync(ByVal tableName As DataBaseTable, ByVal rawInstrumentName As String, ByVal startDate As Date, ByVal endDate As Date) As Task(Of Dictionary(Of Date, Payload))
        Dim ret As Dictionary(Of Date, Payload) = Nothing
        Dim instrumentToken As String = Nothing
        Dim tradingSymbol As String = Nothing
        Dim ZerodhaEODHistoricalURL As String = "https://kitecharts-aws.zerodha.com/api/chart/{0}/day?api_key=kitefront&access_token=K&from={1}&to={2}"
        Dim ZerodhaIntradayHistoricalURL As String = "https://kitecharts-aws.zerodha.com/api/chart/{0}/minute?api_key=kitefront&access_token=K&from={1}&to={2}"
        Dim ZerodhaHistoricalURL As String = Nothing
        Select Case tableName
            Case DataBaseTable.EOD_Cash, DataBaseTable.EOD_Commodity, DataBaseTable.EOD_Currency, DataBaseTable.EOD_Futures
                ZerodhaHistoricalURL = ZerodhaEODHistoricalURL
            Case DataBaseTable.Intraday_Cash, DataBaseTable.Intraday_Commodity, DataBaseTable.Intraday_Currency, DataBaseTable.Intraday_Futures
                ZerodhaHistoricalURL = ZerodhaIntradayHistoricalURL
        End Select
        Dim instrument As Tuple(Of String, String) = GetCurrentTradingSymbolWithInstrumentToken(tableName, endDate, rawInstrumentName)
        If instrument IsNot Nothing Then
            tradingSymbol = instrument.Item1
            instrumentToken = instrument.Item2
        End If
        If instrumentToken IsNot Nothing AndAlso instrumentToken <> "" Then
            Dim historicalDataURL As String = String.Format(ZerodhaHistoricalURL, instrumentToken, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"))
            OnHeartbeat(String.Format("Fetching historical Data: {0}", historicalDataURL))
            Dim historicalCandlesJSONDict As Dictionary(Of String, Object) = Nothing
            Using sr As New StreamReader(HttpWebRequest.Create(historicalDataURL).GetResponseAsync().Result.GetResponseStream)
                Dim jsonString = Await sr.ReadToEndAsync.ConfigureAwait(False)
                historicalCandlesJSONDict = StringManipulation.JsonDeserialize(jsonString)
            End Using
            If historicalCandlesJSONDict IsNot Nothing AndAlso historicalCandlesJSONDict.Count > 0 AndAlso
                historicalCandlesJSONDict.ContainsKey("data") Then
                Dim historicalCandlesDict As Dictionary(Of String, Object) = historicalCandlesJSONDict("data")
                If historicalCandlesDict.ContainsKey("candles") AndAlso historicalCandlesDict("candles").count > 0 Then
                    Dim historicalCandles As ArrayList = historicalCandlesDict("candles")
                    If ret Is Nothing Then ret = New Dictionary(Of Date, Payload)
                    OnHeartbeat(String.Format("Generating Payload for {0}", tradingSymbol))
                    Dim previousPayload As Payload = Nothing
                    For Each historicalCandle In historicalCandles
                        Dim runningSnapshotTime As Date = Utilities.Time.GetDateTimeTillMinutes(historicalCandle(0))

                        Dim runningPayload As Payload = New Payload(Payload.CandleDataSource.Chart)
                        With runningPayload
                            .PayloadDate = Utilities.Time.GetDateTimeTillMinutes(historicalCandle(0))
                            .TradingSymbol = tradingSymbol
                            .Open = historicalCandle(1)
                            .High = historicalCandle(2)
                            .Low = historicalCandle(3)
                            .Close = historicalCandle(4)
                            .Volume = historicalCandle(5)
                            .PreviousCandlePayload = previousPayload
                        End With
                        previousPayload = runningPayload
                        ret.Add(runningSnapshotTime, runningPayload)
                    Next
                End If
            End If
        End If
        Return ret
    End Function

#End Region

#Region "DB Connection"
    Public Function OpenDBConnection() As MySqlConnection
        If conn Is Nothing OrElse conn.State <> ConnectionState.Open Then
            OnHeartbeat("Connecting Database")
            Try
                conn = New MySqlConnection(My.Settings.dbConnectionLocal)
                conn.Open()
            Catch ex1 As MySqlException
                Try
                    conn = New MySqlConnection(My.Settings.dbConnectionLocalNetwork)
                    conn.Open()
                Catch ex3 As Exception
                    conn = New MySqlConnection(My.Settings.dbConnectionRemote)
                    conn.Open()
                End Try
            End Try
        End If
        Return conn
    End Function
#End Region

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
