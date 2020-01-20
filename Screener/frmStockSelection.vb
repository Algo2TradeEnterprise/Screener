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
                    Throw New NotImplementedException
                    Dim instrumentNames As String = Nothing
                    Dim instrumentList As Dictionary(Of String, Decimal()) = Nothing
                    If procedureToRun = 0 Then
                        instrumentNames = GetTextBoxText_ThreadSafe(txtInstrumentList)
                        Dim instruments() As String = instrumentNames.Trim.Split(vbCrLf)
                        For Each runningInstrument In instruments
                            Dim instrument As String = runningInstrument.Trim
                            If instrumentList Is Nothing Then instrumentList = New Dictionary(Of String, Decimal())
                            instrumentList.Add(instrument.Trim.ToUpper, {0, 0})
                        Next
                        If instrumentList Is Nothing OrElse instrumentList.Count = 0 Then
                            Throw New ApplicationException("No instrument available in user given list")
                        End If
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
                    stock = New TopGainerTopLosser(_canceller, cmn, stockType, GetDateTimePickerValue_ThreadSafe(dtpkrTopGainerLosserChkTime), GetTextBoxText_ThreadSafe(txtTopGainerLosserNiftyChangePercentage))
                Case 7
                    stock = New HighLowGapStock(_canceller, cmn, stockType)
                Case 8
                    stock = New SpotFutureArbritrage(_canceller, cmn, stockType, 1)
                Case 9
                    stock = New HighTurnoverStock(_canceller, cmn, stockType)
                Case 10
                    stock = New TopGainerTopLosserEveryMinute(_canceller, cmn, stockType)
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
                lblDescription.Text = String.Format("Return the user given stocklist with proper lotsize and volume filter")
            Case 1
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks between price range which are greater than ATR% and satisfies the volume criteria")
            Case 2
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return ATR Stocks with Pre market change% and previous close")
            Case 3
                dtpkrVolumeSpikeChkTime.Value = New Date(Now.Year, Now.Month, Now.Day, 9, 18, 0)
                LoadSettings(pnlIntradayVolumeSpikeSettings)
                lblDescription.Text = String.Format("Return ATR stocks with volume change% till checking time compare to Previous 5 days average volume till checking time")
            Case 4
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return ATR Stocks where previous day Open=High or Open=Low")
            Case 5
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks which touch the previous day last candle on first minute")
            Case 6
                dtpkrTopGainerLosserChkTime.Value = New Date(Now.Year, Now.Month, Now.Day, 9, 19, 0)
                txtTopGainerLosserNiftyChangePercentage.Text = 0
                LoadSettings(pnlTopGainerLooserSettings)
                lblDescription.Text = String.Format("Return ATR stocks with change% till checking time compare to Previous day close")
            Case 7
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks which open above/below previous day high/low and continues the gap after 5 mins")
            Case 8
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Stocks which creats gap between cash and future")
            Case 9
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Cash Stocks with high turnover (5 day average of volume X close)")
            Case 10
                LoadSettings(Nothing)
                lblDescription.Text = String.Format("Return High ATR Cash Stocks with every minute top gainer losser")
            Case Else
                Throw New NotImplementedException()
        End Select
    End Sub

    Private Sub LoadSettings(ByVal panelName As Panel)
        Dim panelList As List(Of Panel) = New List(Of Panel)
        panelList.Add(pnlInstrumentList)
        panelList.Add(pnlTopGainerLooserSettings)
        panelList.Add(pnlIntradayVolumeSpikeSettings)

        For Each runningPanel In panelList
            If panelName IsNot Nothing AndAlso runningPanel.Name = panelName.Name Then
                SetObjectVisible_ThreadSafe(runningPanel, True)
            Else
                SetObjectVisible_ThreadSafe(runningPanel, False)
            End If
        Next
    End Sub
End Class