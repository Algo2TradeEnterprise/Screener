Imports System.Threading
Imports Utilities.DAL
Imports Algo2TradeBLL

Public Class frmStockSelection

#Region "Common Delegates"
    Delegate Sub SetObjectEnableDisable_Delegate(ByVal [obj] As Object, ByVal [value] As Boolean)
    Public Sub SetObjectEnableDisable_ThreadSafe(ByVal [obj] As Object, ByVal [value] As Boolean)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [obj].InvokeRequired Then
            Dim MyDelegate As New SetObjectEnableDisable_Delegate(AddressOf SetObjectEnableDisable_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[obj], [value]})
        Else
            [obj].Enabled = [value]
        End If
    End Sub

    Delegate Sub SetObjectVisible_Delegate(ByVal [obj] As Object, ByVal [value] As Boolean)
    Public Sub SetObjectVisible_ThreadSafe(ByVal [obj] As Object, ByVal [value] As Boolean)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [obj].InvokeRequired Then
            Dim MyDelegate As New SetObjectVisible_Delegate(AddressOf SetObjectVisible_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[obj], [value]})
        Else
            [obj].Visible = [value]
        End If
    End Sub

    Delegate Sub SetLabelText_Delegate(ByVal [label] As Label, ByVal [text] As String)
    Public Sub SetLabelText_ThreadSafe(ByVal [label] As Label, ByVal [text] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New SetLabelText_Delegate(AddressOf SetLabelText_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[label], [text]})
        Else
            [label].Text = [text]
        End If
    End Sub

    Delegate Function GetLabelText_Delegate(ByVal [label] As Label) As String
    Public Function GetLabelText_ThreadSafe(ByVal [label] As Label) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New GetLabelText_Delegate(AddressOf GetLabelText_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[label]})
        Else
            Return [label].Text
        End If
    End Function

    Delegate Sub SetLabelTag_Delegate(ByVal [label] As Label, ByVal [tag] As String)
    Public Sub SetLabelTag_ThreadSafe(ByVal [label] As Label, ByVal [tag] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New SetLabelTag_Delegate(AddressOf SetLabelTag_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[label], [tag]})
        Else
            [label].Tag = [tag]
        End If
    End Sub

    Delegate Function GetLabelTag_Delegate(ByVal [label] As Label) As String
    Public Function GetLabelTag_ThreadSafe(ByVal [label] As Label) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New GetLabelTag_Delegate(AddressOf GetLabelTag_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[label]})
        Else
            Return [label].Tag
        End If
    End Function
    Delegate Sub SetToolStripLabel_Delegate(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripStatusLabel, ByVal [text] As String)
    Public Sub SetToolStripLabel_ThreadSafe(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripStatusLabel, ByVal [text] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [toolStrip].InvokeRequired Then
            Dim MyDelegate As New SetToolStripLabel_Delegate(AddressOf SetToolStripLabel_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[toolStrip], [label], [text]})
        Else
            [label].Text = [text]
        End If
    End Sub

    Delegate Function GetToolStripLabel_Delegate(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripLabel) As String
    Public Function GetToolStripLabel_ThreadSafe(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripLabel) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [toolStrip].InvokeRequired Then
            Dim MyDelegate As New GetToolStripLabel_Delegate(AddressOf GetToolStripLabel_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[toolStrip], [label]})
        Else
            Return [label].Text
        End If
    End Function

    Delegate Function GetDateTimePickerValue_Delegate(ByVal [dateTimePicker] As DateTimePicker) As Date
    Public Function GetDateTimePickerValue_ThreadSafe(ByVal [dateTimePicker] As DateTimePicker) As Date
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [dateTimePicker].InvokeRequired Then
            Dim MyDelegate As New GetDateTimePickerValue_Delegate(AddressOf GetDateTimePickerValue_ThreadSafe)
            Return Me.Invoke(MyDelegate, New DateTimePicker() {[dateTimePicker]})
        Else
            Return [dateTimePicker].Value
        End If
    End Function

    Delegate Function GetNumericUpDownValue_Delegate(ByVal [numericUpDown] As NumericUpDown) As Integer
    Public Function GetNumericUpDownValue_ThreadSafe(ByVal [numericUpDown] As NumericUpDown) As Integer
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [numericUpDown].InvokeRequired Then
            Dim MyDelegate As New GetNumericUpDownValue_Delegate(AddressOf GetNumericUpDownValue_ThreadSafe)
            Return Me.Invoke(MyDelegate, New NumericUpDown() {[numericUpDown]})
        Else
            Return [numericUpDown].Value
        End If
    End Function

    Delegate Function GetComboBoxIndex_Delegate(ByVal [combobox] As ComboBox) As Integer
    Public Function GetComboBoxIndex_ThreadSafe(ByVal [combobox] As ComboBox) As Integer
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [combobox].InvokeRequired Then
            Dim MyDelegate As New GetComboBoxIndex_Delegate(AddressOf GetComboBoxIndex_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[combobox]})
        Else
            Return [combobox].SelectedIndex
        End If
    End Function

    Delegate Function GetComboBoxItem_Delegate(ByVal [ComboBox] As ComboBox) As String
    Public Function GetComboBoxItem_ThreadSafe(ByVal [ComboBox] As ComboBox) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [ComboBox].InvokeRequired Then
            Dim MyDelegate As New GetComboBoxItem_Delegate(AddressOf GetComboBoxItem_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[ComboBox]})
        Else
            Return [ComboBox].SelectedItem.ToString
        End If
    End Function

    Delegate Function GetTextBoxText_Delegate(ByVal [textBox] As TextBox) As String
    Public Function GetTextBoxText_ThreadSafe(ByVal [textBox] As TextBox) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [textBox].InvokeRequired Then
            Dim MyDelegate As New GetTextBoxText_Delegate(AddressOf GetTextBoxText_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[textBox]})
        Else
            Return [textBox].Text
        End If
    End Function

    Delegate Function GetCheckBoxChecked_Delegate(ByVal [checkBox] As CheckBox) As Boolean
    Public Function GetCheckBoxChecked_ThreadSafe(ByVal [checkBox] As CheckBox) As Boolean
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [checkBox].InvokeRequired Then
            Dim MyDelegate As New GetCheckBoxChecked_Delegate(AddressOf GetCheckBoxChecked_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[checkBox]})
        Else
            Return [checkBox].Checked
        End If
    End Function

    Delegate Function GetRadioButtonChecked_Delegate(ByVal [radioButton] As RadioButton) As Boolean
    Public Function GetRadioButtonChecked_ThreadSafe(ByVal [radioButton] As RadioButton) As Boolean
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [radioButton].InvokeRequired Then
            Dim MyDelegate As New GetRadioButtonChecked_Delegate(AddressOf GetRadioButtonChecked_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[radioButton]})
        Else
            Return [radioButton].Checked
        End If
    End Function

    Delegate Sub SetDatagridBindDatatable_Delegate(ByVal [datagrid] As DataGridView, ByVal [table] As DataTable)
    Public Sub SetDatagridBindDatatable_ThreadSafe(ByVal [datagrid] As DataGridView, ByVal [table] As DataTable)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [datagrid].InvokeRequired Then
            Dim MyDelegate As New SetDatagridBindDatatable_Delegate(AddressOf SetDatagridBindDatatable_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[datagrid], [table]})
        Else
            [datagrid].DataSource = [table]
            [datagrid].Refresh()
        End If
    End Sub
#End Region

#Region "Event Handlers"
    Private Sub OnHeartbeat(message As String)
        SetLabelText_ThreadSafe(lblProgress, message)
    End Sub
    Private Sub OnHeartbeatMain(message As String)
        SetLabelText_ThreadSafe(lblProgress, message)
    End Sub
    Private Sub OnDocumentDownloadComplete()
        'OnHeartbeat("Document download compelete")
    End Sub
    Private Sub OnDocumentRetryStatus(currentTry As Integer, totalTries As Integer)
        OnHeartbeat(String.Format("Try #{0}/{1}: Connecting...", currentTry, totalTries))
    End Sub
    Public Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        OnHeartbeat(String.Format("{0}, waiting {1}/{2} secs", msg, elapsedSecs, totalSecs))
    End Sub
#End Region

    Private _canceller As CancellationTokenSource

    Private Sub frmStockSelection_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetObjectEnableDisable_ThreadSafe(btnStop, False)
        SetObjectEnableDisable_ThreadSafe(btnExport, False)
        If My.Settings.StartDate <> Date.MinValue Then dtpckrFromDate.Value = My.Settings.StartDate
        If My.Settings.EndDate <> Date.MinValue Then dtpckrToDate.Value = My.Settings.EndDate
        cmbProcedure.SelectedIndex = My.Settings.ProcedureNumber
        txtMaxBlankCandlePercentage.Text = My.Settings.MaxBlankCandlePercentage
        txtInstrumentList.Text = My.Settings.InstrumentList
        txtNumberOfStock.Text = My.Settings.NumberOfStockPerDay
        cmbStockType.SelectedIndex = My.Settings.StockType
        txtMinPrice.Text = My.Settings.MinClose
        txtMaxPrice.Text = My.Settings.MaxClose
        txtATRPercentage.Text = My.Settings.ATRPercentage
        chkbFOStock.Checked = My.Settings.OnlyFOStocks
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        _canceller.Cancel()
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        If dgrvMain IsNot Nothing AndAlso dgrvMain.Rows.Count > 0 Then
            saveFile.AddExtension = True
            saveFile.FileName = String.Format("{0} {1} to {2}.csv",
                                              GetComboBoxItem_ThreadSafe(cmbProcedure),
                                              GetDateTimePickerValue_ThreadSafe(dtpckrFromDate).ToString("dd_MM_yy"),
                                              GetDateTimePickerValue_ThreadSafe(dtpckrToDate).ToString("dd_MM_yy"))
            saveFile.Filter = "CSV (*.csv)|*.csv"
            saveFile.ShowDialog()
        Else
            MessageBox.Show("Empty DataGrid. Nothing to export.", "Future Stock CSV File", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub saveFile_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles saveFile.FileOk
        Using export As New CSVHelper(saveFile.FileName, ",", _canceller)
            export.GetCSVFromDataGrid(dgrvMain)
        End Using
        If MessageBox.Show("Do you want to open file?", "Future Stock List CSV File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Process.Start(saveFile.FileName)
        End If
    End Sub

    Private Sub cmbStockType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbStockType.SelectedIndexChanged
        Dim index As Integer = GetComboBoxIndex_ThreadSafe(cmbStockType)
        Select Case index
            Case 0
                chkbFOStock.Checked = My.Settings.OnlyFOStocks
                chkbFOStock.Enabled = True
            Case Else
                chkbFOStock.Checked = True
                chkbFOStock.Enabled = False
        End Select
    End Sub

    Private Async Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        _canceller = New CancellationTokenSource
        SetDatagridBindDatatable_ThreadSafe(dgrvMain, Nothing)
        dgrvMain.Refresh()
        SetObjectEnableDisable_ThreadSafe(btnStart, False)
        SetObjectEnableDisable_ThreadSafe(btnExport, False)
        SetObjectEnableDisable_ThreadSafe(btnStop, True)
        My.Settings.StartDate = dtpckrFromDate.Value
        My.Settings.EndDate = dtpckrToDate.Value
        My.Settings.ProcedureNumber = cmbProcedure.SelectedIndex
        My.Settings.MaxBlankCandlePercentage = txtMaxBlankCandlePercentage.Text
        My.Settings.InstrumentList = txtInstrumentList.Text
        My.Settings.StockType = cmbStockType.SelectedIndex
        My.Settings.NumberOfStockPerDay = txtNumberOfStock.Text
        My.Settings.MinClose = txtMinPrice.Text
        My.Settings.MaxClose = txtMaxPrice.Text
        My.Settings.ATRPercentage = txtATRPercentage.Text
        My.Settings.OnlyFOStocks = chkbFOStock.Checked
        My.Settings.Save()

        Await Task.Run(AddressOf StartProcessingAsync).ConfigureAwait(False)
    End Sub

    Private Async Function StartProcessingAsync() As Task
        Try
            Dim startDate As Date = GetDateTimePickerValue_ThreadSafe(dtpckrFromDate)
            Dim endDate As Date = GetDateTimePickerValue_ThreadSafe(dtpckrToDate)
            Dim procedureToRun As Integer = GetComboBoxIndex_ThreadSafe(cmbProcedure)
            Dim stockType As Integer = GetComboBoxIndex_ThreadSafe(cmbStockType)
            Dim cmn = New Common(_canceller)
            AddHandler cmn.Heartbeat, AddressOf OnHeartbeat
            AddHandler cmn.WaitingFor, AddressOf OnWaitingFor
            AddHandler cmn.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
            AddHandler cmn.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete

            Dim stock As StockSelection = Nothing
            Select Case procedureToRun
                Case 0
                    Dim instrumentNames As String = Nothing
                    Dim instrumentList As List(Of String) = Nothing
                    If procedureToRun = 0 Then
                        instrumentNames = GetTextBoxText_ThreadSafe(txtInstrumentList)
                        Dim instruments() As String = instrumentNames.Trim.Split(vbCrLf)
                        For Each runningInstrument In instruments
                            Dim instrument As String = runningInstrument.Trim
                            If instrumentList Is Nothing Then instrumentList = New List(Of String)
                            instrumentList.Add(instrument.Trim.ToUpper)
                        Next
                        If instrumentList Is Nothing OrElse instrumentList.Count = 0 Then
                            Throw New ApplicationException("No instrument available in user given list")
                        End If
                        stock = New UserGivenStocks(_canceller, cmn, stockType, instrumentList)
                    End If
                Case 1
                    stock = New HighATRStocks(_canceller, cmn, stockType)
                Case 2
                    stock = New PreMarketStocks(_canceller, cmn, stockType)
                Case 3
                    stock = New IntradayVolumeSpike(_canceller, cmn, stockType, GetDateTimePickerValue_ThreadSafe(dtpkrVolumeSpikeChkTime))
                Case 4
                    stock = New OHLStocks(_canceller, cmn, stockType)
                Case 5
                    stock = New TouchPreviousDayLastCandle(_canceller, cmn, stockType)
                Case 6
                    stock = New TopGainerTopLosser(_canceller, cmn, stockType, GetDateTimePickerValue_ThreadSafe(dtpkrTopGainerLosserChkTime), GetTextBoxText_ThreadSafe(txtTopGainerLosserNiftyChangePercentage), GetCheckBoxChecked_ThreadSafe(chkbTopGainerTopLosserOnlyBankniftyStocks))
                Case 7
                    stock = New HighLowGapStock(_canceller, cmn, stockType)
                Case 8
                    stock = New SpotFutureArbritrage(_canceller, cmn, stockType, 1)
                Case 9
                    stock = New HighTurnoverStock(_canceller, cmn, stockType)
                Case 10
                    stock = New TopGainerTopLosserEveryMinute(_canceller, cmn, stockType)
                Case 11
                    stock = New HighSlabLevelMovedStocks(_canceller, cmn, stockType)
                Case 12
                    stock = New OpenAtHighLow(_canceller, cmn, stockType)
                Case 13
                    stock = New MultiTimeframeSignal(_canceller, cmn, stockType, GetComboBoxIndex_ThreadSafe(cmbMultiTFIndicator))
                Case 14
                    stock = New NarrowRangeStocks(_canceller, cmn, stockType, GetTextBoxText_ThreadSafe(txtNarrowRangeNmbrOfDays), GetCheckBoxChecked_ThreadSafe(chkbNarrowRangeDownwardsChecking))
                Case 15
                    stock = New TopGainerTopLosserOfEverySlab(_canceller, cmn, stockType)
                Case 16
                    stock = New CPRNarrowRangeStocks(_canceller, cmn, stockType, GetTextBoxText_ThreadSafe(txtMinimumCPRRangePer))
                Case 17
                    stock = New LowestRangeStocksOfEveryMinute(_canceller, cmn, stockType)
                Case 18
                    stock = New LowestRangeStockOfXMinute(_canceller, cmn, stockType, GetDateTimePickerValue_ThreadSafe(dtpckrLowRangeTime))
                Case 19
                    stock = New LowerPriceOptions(_canceller, cmn, stockType)
                Case 20
                    stock = New LowerPriceOptionsWithOIChange(_canceller, cmn, stockType)
                Case 21
                    stock = New StrongMovedStocks(_canceller, cmn, stockType)
                Case 22
                    stock = New LowATRCandleQuickEntryStocks(_canceller, cmn, stockType)
                Case 23
                    stock = New EODLowRangeStock(_canceller, cmn, stockType)
                Case 24
                    stock = New PreviousDayStrongHKStocks(_canceller, cmn, stockType)
                Case 25
                    stock = New VolumeSortPreviousDayCloseFilterCEPEOptions(_canceller, cmn, stockType)
                Case 26
                    stock = New VolumeSortPreviousDayCloseFilterTop2Options(_canceller, cmn, stockType)
                Case 27
                    stock = New VolumeSortCurrentDayOpenFilterCEPEOptions(_canceller, cmn, stockType)
                Case 28
                    stock = New VolumeSortCurrentDayOpenFilterTop2Options(_canceller, cmn, stockType)
                Case 29
                    stock = New PreviousDayCloseATRSortVolumeFilterCEPEOptions(_canceller, cmn, stockType)
                Case 30
                    stock = New PreviousDayCloseATRSortVolumeFilterTop2Options(_canceller, cmn, stockType)
                Case 31
                    stock = New CurrentDayOpenATRSortVolumeFilterCEPEOptions(_canceller, cmn, stockType)
                Case 32
                    stock = New CurrentDayOpenATRSortVolumeFilterTop2Options(_canceller, cmn, stockType)
                Case 33
                    stock = New LowestPriceAtTheMoneyOptions(_canceller, cmn, stockType)
                Case 34
                    stock = New HighestATRAtTheMoneyOptions(_canceller, cmn, stockType)
                Case 35
                    stock = New LowerDeviationAtTheMoneyOptions(_canceller, cmn, stockType)
                Case 36
                    stock = New LowerPriceNearestOptions(_canceller, cmn, stockType)
                Case 37
                    stock = New DayOpenAtTheMoneyOptions(_canceller, cmn, stockType)
                Case 38
                    stock = New LowTurnoverOption(_canceller, cmn, stockType)
                Case 39
                    stock = New PreMarketOptions(_canceller, cmn, stockType)
                Case 40
                    stock = New FractalConstriction(_canceller, cmn, stockType)
                Case 41
                    stock = New MaxSlabLevelHitsStocks(_canceller, cmn, stockType)
                Case 42
                    stock = New HighATRStocksWithMultiplier(_canceller, cmn, stockType)
                Case 43
                    stock = New EODOutsideSMAStocks(_canceller, cmn, stockType)
                Case 44
                    stock = New EODOutsideEMAStocks(_canceller, cmn, stockType)
                Case 45
                    stock = New EODVolumeEMAStocks(_canceller, cmn, stockType)
                Case 46
                    stock = New OpeningPriceOptions(_canceller, cmn, stockType)
                Case 47
                    stock = New FirstFavourableFractalTopGainerLooser(_canceller, cmn, stockType)
                Case 48
                    stock = New EODEMACrossoverStocks(_canceller, cmn, stockType)
                Case 49
                    stock = New EODBTST_NKSStocks(_canceller, cmn, stockType)
                Case 50
                    stock = New EODBTST_BullishEngulfingStocks(_canceller, cmn, stockType)
                Case 51
                    stock = New EODBTST_DoubleTIIStocks(_canceller, cmn, stockType)
                Case 52
                    stock = New EODBTST_15Min23Stocks(_canceller, cmn, stockType)
                Case 53
                    stock = New EODBTST_15Min57Stocks(_canceller, cmn, stockType)
                Case 54
                    stock = New EODRainbowCrossover(_canceller, cmn, stockType)
                Case 55
                    stock = New TopGainerTopLosserOptions(_canceller, cmn, stockType)
                Case 56
                    stock = New HighATRHighVolumeStocks(_canceller, cmn, stockType)
            End Select
            AddHandler stock.Heartbeat, AddressOf OnHeartbeat

            Dim dt As DataTable = Await stock.GetStockDataAsync(startDate.Date, endDate.Date).ConfigureAwait(False)
            SetDatagridBindDatatable_ThreadSafe(dgrvMain, dt)
        Catch oex As OperationCanceledException
            MsgBox(oex.Message)
        Catch ex As Exception
            MsgBox(ex.ToString)
        Finally
            SetObjectEnableDisable_ThreadSafe(btnExport, True)
            SetObjectEnableDisable_ThreadSafe(btnStart, True)
            SetObjectEnableDisable_ThreadSafe(btnStop, False)
            OnHeartbeat(String.Format("Process Complete. Number of records: {0}", dgrvMain.Rows.Count))
        End Try
    End Function

    Private Sub cmbProcedure_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbProcedure.SelectedIndexChanged
        Dim index As Integer = GetComboBoxIndex_ThreadSafe(cmbProcedure)
        Select Case index
            Case 0
                LoadSettings(pnlInstrumentList)
                lblDescription.Text = String.Format("Return the user given stocklist with proper lotsize. If you want option data please enter 'InstrumentName'-OPT e.g. 'NIFTY-OPT'. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 1
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks between price range which are greater than ATR% and satisfies the volume criteria. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 2
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return ATR Stocks with Pre market change% and previous close. Give the date you want to trade. Current Date is also accepted if pre market backup is done.(Expecting that previous day data is there in the database)")
            Case 3
                dtpkrVolumeSpikeChkTime.Value = New Date(Now.Year, Now.Month, Now.Day, 9, 18, 0)
                LoadSettings(pnlIntradayVolumeSpikeSettings)
                lblDescription.Text = String.Format("Return ATR stocks with volume change% till checking time compare to Previous 5 days average volume till checking time. Give the date you want to trade if it is not the current date.")
            Case 4
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return ATR Stocks where previous day Open=High or Open=Low. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 5
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks which touch the previous day last candle on first minute. Give the date you want to trade if it is not the current date.")
            Case 6
                dtpkrTopGainerLosserChkTime.Value = New Date(Now.Year, Now.Month, Now.Day, 9, 19, 0)
                txtTopGainerLosserNiftyChangePercentage.Text = 0
                chkbTopGainerTopLosserOnlyBankniftyStocks.Checked = False
                LoadSettings(pnlTopGainerLooserSettings)
                lblDescription.Text = String.Format("Return ATR stocks with change% till checking time compare to Previous day close. Give the date you want to trade if it is not the current date.")
            Case 7
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks which open above/below previous day high/low and continues the gap after 5 mins. Give the date you want to trade if it is not the current date.")
            Case 8
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks which creats minimum 1% gap between cash and future. Give the date you want to trade if it is not the current date.")
            Case 9
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Cash Stocks with high turnover (5 day average of volume X close). If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 10
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Cash Stocks with every minute top gainer losser. Give the date you want to trade if it is not the current date.")
            Case 11
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Cash Stocks with high slab lavel moved. Give the date you want to trade if it is not the current date.")
            Case 12
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Cash Stocks with open between one slab of previous day high or low. Give the date you want to trade if it is not the current date.")
            Case 13
                For i = 1 To 7
                    Dim indicatorType As MultiTimeframeSignal.TypeOfIndicator = i
                    If Val(indicatorType.ToString) <> i Then cmbMultiTFIndicator.Items.Add(indicatorType.ToString)
                Next
                cmbMultiTFIndicator.SelectedIndex = 1
                LoadSettings(pnlMultiTFSettings)
                lblDescription.Text = String.Format("Return High ATR Cash Stocks with last Week, Day, Hour, 15 Minutes candle value for respective indicator. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 14
                txtNarrowRangeNmbrOfDays.Text = 7
                LoadSettings(pnlNarrowRangeSettings)
                lblDescription.Text = String.Format("Return High ATR Cash Stocks where current day candle range is less than last X(Number Of Days) days candle range. Give the date you want to trade if it is not the current date.")
            Case 15
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Cash Stocks with every minute top gainer losser(top 5 with no duplicate records). Give the date you want to trade if it is not the current date.")
            Case 16
                txtMinimumCPRRangePer.Text = 100
                LoadSettings(pnlCPRNarrowRangeSettings)
                lblDescription.Text = String.Format("Return High ATR stocks where current day CPR is narrow compare to previous 5 day average. Give the date you want to trade if it is not the current date.")
            Case 17
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return lowest candle range stock in every minute from 9:15 to 10:30. Give the date you want to trade if it is not the current date.")
            Case 18
                dtpckrLowRangeTime.Value = New Date(Now.Year, Now.Month, Now.Day, 9, 15, 0)
                LoadSettings(pnlLowRangeStocksOfXMinuteSettings)
                lblDescription.Text = String.Format("Return lowest candle range stocks of x minute in ascending order. Give the date you want to trade if it is not the current date.")
            Case 19
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return BANKNIFTY option stocks with Volume sort and OI where Close<30. Give the date you want to trade if it is not the current date.")
            Case 20
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return NIFTY option stocks with Volume and OI Change % where Close<10. Give the date you want to trade if it is not the current date.")
            Case 21
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks which previous day open to close movement greater than 5%. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 22
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks where 1 min candle range is less than 1*ATR but greater than 0.5*ATR. Give the date you want to trade if it is not the current date.")
            Case 23
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks with lowest candle range with respect to ATR. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 24
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks with previous day strong HK Candle (Bullish:Open=Low, Bearish:Open=High). If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 25, 26, 27, 28
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return BANKNIFTY option stocks. Give the date you want to trade if it is not the current date.")
            Case 29, 30, 31, 32
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return BANKNIFTY option stocks. Give the date you want to trade if it is not the current date.")
            Case 33
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return BANKNIFTY option stocks. Give the date you want to trade if it is not the current date.")
            Case 34
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return BANKNIFTY option stocks. Give the date you want to trade if it is not the current date.")
            Case 35
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return BANKNIFTY option stocks. Give the date you want to trade if it is not the current date.")
            Case 36
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return BANKNIFTY option stocks. Give the date you want to trade if it is not the current date.")
            Case 37
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return NIFTY option stocks. Give the date you want to trade if it is not the current date.")
            Case 38
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return BANKNIFTY option stocks. Give the date you want to trade if it is not the current date.")
            Case 39
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return atm options for top n-Pre market stocks in quantity desc order. Give the date you want to trade. Current Date is also accepted if pre market backup is done.(Expecting that previous day data is there in the database)")
            Case 40
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks between price range which are greater than ATR% and satisfies the volume criteria. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 41
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks with slab levels which are crossed maximum times on previous day. Give the date you want to trade if it is not the current date.")
            Case 42
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks between price range which are greater than ATR% and satisfies the volume criteria. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 43
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks where EOD Candle is not touching 20 SMA. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 44
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks where EOD Candle is not touching 20 EMA with 15-min first hammer candle time. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 45
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks where EOD Candle Volume is greater than 2 times of EMA of EOD Volume. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case 46
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("")
            Case 47
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return ATR stocks if the stock was in top 10 gainer/looser when there first favourable fractal breakout. Give the date you want to trade if it is not the current date.")
            Case 48
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("EOD Candle Close crosses above EMA(5) on High or EOD Candle Close crosses below EMA(5) on Low")
            Case 49
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("")
            Case 50
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("")
            Case 51
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("")
            Case 52
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("")
            Case 53
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("")
            Case 54
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("")
            Case 55
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return Top Gainer Losser stocks as 09:29 Close and their respective option with 1% devation. Give the date you want to trade if it is not the current date.")
            Case 56
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks between price range which are greater than ATR% and satisfies the volume criteria and also eod volume >= 1000000. If you want to trade today give today's date.(Expecting that previous day data is there in the database)")
            Case Else
                Throw New NotImplementedException()
        End Select

        Select Case index
            Case 13
                cmbStockType.SelectedIndex = 0
                SetObjectEnableDisable_ThreadSafe(cmbStockType, False)
            Case 19, 20, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38
                cmbStockType.SelectedIndex = 3
                SetObjectEnableDisable_ThreadSafe(cmbStockType, False)
            Case 49, 50, 51, 52, 53, 54
                cmbStockType.SelectedIndex = 0
                SetObjectEnableDisable_ThreadSafe(cmbStockType, False)
            Case Else
                SetObjectEnableDisable_ThreadSafe(cmbStockType, True)
        End Select

        Select Case index
            Case 0, 19, 20, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38
                lblMaxBlankCandlePercentage.Visible = False
                txtMaxBlankCandlePercentage.Visible = False
                chkbFOStock.Visible = False
                lblMinPrice.Visible = False
                txtMinPrice.Visible = False
                lblMaxPrice.Visible = False
                txtMaxPrice.Visible = False
                lblATR.Visible = False
                txtATRPercentage.Visible = False
                lblNumberOfStock.Visible = False
                txtNumberOfStock.Visible = False
            Case Else
                lblMaxBlankCandlePercentage.Visible = True
                txtMaxBlankCandlePercentage.Visible = True
                chkbFOStock.Visible = True
                lblMinPrice.Visible = True
                txtMinPrice.Visible = True
                lblMaxPrice.Visible = True
                txtMaxPrice.Visible = True
                lblATR.Visible = True
                txtATRPercentage.Visible = True
                lblNumberOfStock.Visible = True
                txtNumberOfStock.Visible = True
        End Select
    End Sub

    Private Sub LoadSettings(ByVal panelName As Panel)
        Dim panelList As List(Of Panel) = New List(Of Panel)
        panelList.Add(pnlInstrumentList)
        panelList.Add(pnlTopGainerLooserSettings)
        panelList.Add(pnlIntradayVolumeSpikeSettings)
        panelList.Add(pnlNarrowRangeSettings)
        panelList.Add(pnlCPRNarrowRangeSettings)
        panelList.Add(pnlMultiTFSettings)
        panelList.Add(pnlLowRangeStocksOfXMinuteSettings)

        For Each runningPanel In panelList
            If panelName IsNot Nothing AndAlso runningPanel.Name = panelName.Name Then
                SetObjectVisible_ThreadSafe(runningPanel, True)
            Else
                SetObjectVisible_ThreadSafe(runningPanel, False)
            End If
        Next
    End Sub
End Class