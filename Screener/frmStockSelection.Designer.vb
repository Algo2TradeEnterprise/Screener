<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmStockSelection
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStockSelection))
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.chkbFOStock = New System.Windows.Forms.CheckBox()
        Me.txtATRPercentage = New System.Windows.Forms.TextBox()
        Me.txtNumberOfStock = New System.Windows.Forms.TextBox()
        Me.lblATR = New System.Windows.Forms.Label()
        Me.lblNumberOfStock = New System.Windows.Forms.Label()
        Me.txtMaxPrice = New System.Windows.Forms.TextBox()
        Me.lblMaxPrice = New System.Windows.Forms.Label()
        Me.txtMinPrice = New System.Windows.Forms.TextBox()
        Me.lblMinPrice = New System.Windows.Forms.Label()
        Me.cmbStockType = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtMaxBlankCandlePercentage = New System.Windows.Forms.TextBox()
        Me.lblMaxBlankCandlePercentage = New System.Windows.Forms.Label()
        Me.dtpckrToDate = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrFromDate = New System.Windows.Forms.DateTimePicker()
        Me.lblToDate = New System.Windows.Forms.Label()
        Me.lblFromDate = New System.Windows.Forms.Label()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.dgrvMain = New System.Windows.Forms.DataGridView()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.lblDescription = New System.Windows.Forms.Label()
        Me.cmbProcedure = New System.Windows.Forms.ComboBox()
        Me.lblProcedure = New System.Windows.Forms.Label()
        Me.saveFile = New System.Windows.Forms.SaveFileDialog()
        Me.pnlTopGainerLooserSettings = New System.Windows.Forms.Panel()
        Me.txtTopGainerLosserNiftyChangePercentage = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.dtpkrTopGainerLosserChkTime = New System.Windows.Forms.DateTimePicker()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.pnlIntradayVolumeSpikeSettings = New System.Windows.Forms.Panel()
        Me.dtpkrVolumeSpikeChkTime = New System.Windows.Forms.DateTimePicker()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.pnlInstrumentList = New System.Windows.Forms.Panel()
        Me.txtInstrumentList = New System.Windows.Forms.TextBox()
        Me.lblInstrumentList = New System.Windows.Forms.Label()
        Me.Panel1.SuspendLayout()
        CType(Me.dgrvMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlTopGainerLooserSettings.SuspendLayout()
        Me.pnlIntradayVolumeSpikeSettings.SuspendLayout()
        Me.pnlInstrumentList.SuspendLayout()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.chkbFOStock)
        Me.Panel1.Controls.Add(Me.txtATRPercentage)
        Me.Panel1.Controls.Add(Me.txtNumberOfStock)
        Me.Panel1.Controls.Add(Me.lblATR)
        Me.Panel1.Controls.Add(Me.lblNumberOfStock)
        Me.Panel1.Controls.Add(Me.txtMaxPrice)
        Me.Panel1.Controls.Add(Me.lblMaxPrice)
        Me.Panel1.Controls.Add(Me.txtMinPrice)
        Me.Panel1.Controls.Add(Me.lblMinPrice)
        Me.Panel1.Controls.Add(Me.cmbStockType)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Controls.Add(Me.txtMaxBlankCandlePercentage)
        Me.Panel1.Controls.Add(Me.lblMaxBlankCandlePercentage)
        Me.Panel1.Controls.Add(Me.dtpckrToDate)
        Me.Panel1.Controls.Add(Me.dtpckrFromDate)
        Me.Panel1.Controls.Add(Me.lblToDate)
        Me.Panel1.Controls.Add(Me.lblFromDate)
        Me.Panel1.Location = New System.Drawing.Point(2, 97)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(1295, 76)
        Me.Panel1.TabIndex = 0
        '
        'chkbFOStock
        '
        Me.chkbFOStock.AutoSize = True
        Me.chkbFOStock.Location = New System.Drawing.Point(896, 9)
        Me.chkbFOStock.Name = "chkbFOStock"
        Me.chkbFOStock.Size = New System.Drawing.Size(128, 21)
        Me.chkbFOStock.TabIndex = 95
        Me.chkbFOStock.Text = "Only FO Stocks"
        Me.chkbFOStock.UseVisualStyleBackColor = True
        '
        'txtATRPercentage
        '
        Me.txtATRPercentage.Location = New System.Drawing.Point(478, 44)
        Me.txtATRPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtATRPercentage.Name = "txtATRPercentage"
        Me.txtATRPercentage.Size = New System.Drawing.Size(113, 22)
        Me.txtATRPercentage.TabIndex = 91
        Me.txtATRPercentage.Tag = "ATR %"
        '
        'txtNumberOfStock
        '
        Me.txtNumberOfStock.Location = New System.Drawing.Point(779, 39)
        Me.txtNumberOfStock.Margin = New System.Windows.Forms.Padding(4)
        Me.txtNumberOfStock.Name = "txtNumberOfStock"
        Me.txtNumberOfStock.Size = New System.Drawing.Size(96, 22)
        Me.txtNumberOfStock.TabIndex = 94
        '
        'lblATR
        '
        Me.lblATR.AutoSize = True
        Me.lblATR.Location = New System.Drawing.Point(414, 48)
        Me.lblATR.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblATR.Name = "lblATR"
        Me.lblATR.Size = New System.Drawing.Size(56, 17)
        Me.lblATR.TabIndex = 92
        Me.lblATR.Text = "ATR %:"
        '
        'lblNumberOfStock
        '
        Me.lblNumberOfStock.AutoSize = True
        Me.lblNumberOfStock.Location = New System.Drawing.Point(601, 43)
        Me.lblNumberOfStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNumberOfStock.Name = "lblNumberOfStock"
        Me.lblNumberOfStock.Size = New System.Drawing.Size(175, 17)
        Me.lblNumberOfStock.TabIndex = 93
        Me.lblNumberOfStock.Text = "Number Of Stock Per Day:"
        '
        'txtMaxPrice
        '
        Me.txtMaxPrice.Location = New System.Drawing.Point(286, 43)
        Me.txtMaxPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxPrice.Name = "txtMaxPrice"
        Me.txtMaxPrice.Size = New System.Drawing.Size(113, 22)
        Me.txtMaxPrice.TabIndex = 89
        Me.txtMaxPrice.Tag = "Max Price"
        '
        'lblMaxPrice
        '
        Me.lblMaxPrice.AutoSize = True
        Me.lblMaxPrice.Location = New System.Drawing.Point(209, 46)
        Me.lblMaxPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxPrice.Name = "lblMaxPrice"
        Me.lblMaxPrice.Size = New System.Drawing.Size(73, 17)
        Me.lblMaxPrice.TabIndex = 90
        Me.lblMaxPrice.Text = "Max Price:"
        '
        'txtMinPrice
        '
        Me.txtMinPrice.Location = New System.Drawing.Point(84, 43)
        Me.txtMinPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinPrice.Name = "txtMinPrice"
        Me.txtMinPrice.Size = New System.Drawing.Size(113, 22)
        Me.txtMinPrice.TabIndex = 87
        Me.txtMinPrice.Tag = "Min Price"
        '
        'lblMinPrice
        '
        Me.lblMinPrice.AutoSize = True
        Me.lblMinPrice.Location = New System.Drawing.Point(6, 46)
        Me.lblMinPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinPrice.Name = "lblMinPrice"
        Me.lblMinPrice.Size = New System.Drawing.Size(70, 17)
        Me.lblMinPrice.TabIndex = 88
        Me.lblMinPrice.Text = "Min Price:"
        '
        'cmbStockType
        '
        Me.cmbStockType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbStockType.FormattingEnabled = True
        Me.cmbStockType.Items.AddRange(New Object() {"Cash", "Commodity", "Currency", "Futures"})
        Me.cmbStockType.Location = New System.Drawing.Point(478, 8)
        Me.cmbStockType.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbStockType.Name = "cmbStockType"
        Me.cmbStockType.Size = New System.Drawing.Size(120, 24)
        Me.cmbStockType.TabIndex = 86
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(391, 11)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(83, 17)
        Me.Label2.TabIndex = 85
        Me.Label2.Text = "Stock Type:"
        '
        'txtMaxBlankCandlePercentage
        '
        Me.txtMaxBlankCandlePercentage.Location = New System.Drawing.Point(761, 8)
        Me.txtMaxBlankCandlePercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxBlankCandlePercentage.Name = "txtMaxBlankCandlePercentage"
        Me.txtMaxBlankCandlePercentage.Size = New System.Drawing.Size(114, 22)
        Me.txtMaxBlankCandlePercentage.TabIndex = 84
        '
        'lblMaxBlankCandlePercentage
        '
        Me.lblMaxBlankCandlePercentage.AutoSize = True
        Me.lblMaxBlankCandlePercentage.Location = New System.Drawing.Point(620, 12)
        Me.lblMaxBlankCandlePercentage.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxBlankCandlePercentage.Name = "lblMaxBlankCandlePercentage"
        Me.lblMaxBlankCandlePercentage.Size = New System.Drawing.Size(140, 17)
        Me.lblMaxBlankCandlePercentage.TabIndex = 83
        Me.lblMaxBlankCandlePercentage.Text = "Max Blank Candle %:"
        '
        'dtpckrToDate
        '
        Me.dtpckrToDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrToDate.Location = New System.Drawing.Point(268, 7)
        Me.dtpckrToDate.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpckrToDate.Name = "dtpckrToDate"
        Me.dtpckrToDate.Size = New System.Drawing.Size(108, 22)
        Me.dtpckrToDate.TabIndex = 82
        '
        'dtpckrFromDate
        '
        Me.dtpckrFromDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrFromDate.Location = New System.Drawing.Point(84, 7)
        Me.dtpckrFromDate.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpckrFromDate.Name = "dtpckrFromDate"
        Me.dtpckrFromDate.Size = New System.Drawing.Size(108, 22)
        Me.dtpckrFromDate.TabIndex = 81
        '
        'lblToDate
        '
        Me.lblToDate.AutoSize = True
        Me.lblToDate.Location = New System.Drawing.Point(202, 11)
        Me.lblToDate.Name = "lblToDate"
        Me.lblToDate.Size = New System.Drawing.Size(63, 17)
        Me.lblToDate.TabIndex = 80
        Me.lblToDate.Text = "To Date:"
        '
        'lblFromDate
        '
        Me.lblFromDate.AutoSize = True
        Me.lblFromDate.Location = New System.Drawing.Point(6, 11)
        Me.lblFromDate.Name = "lblFromDate"
        Me.lblFromDate.Size = New System.Drawing.Size(78, 17)
        Me.lblFromDate.TabIndex = 79
        Me.lblFromDate.Text = "From Date:"
        '
        'btnStart
        '
        Me.btnStart.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnStart.Location = New System.Drawing.Point(1046, 6)
        Me.btnStart.Margin = New System.Windows.Forms.Padding(4)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(119, 38)
        Me.btnStart.TabIndex = 33
        Me.btnStart.Text = "Start"
        Me.btnStart.UseVisualStyleBackColor = True
        '
        'btnExport
        '
        Me.btnExport.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnExport.Location = New System.Drawing.Point(1073, 49)
        Me.btnExport.Margin = New System.Windows.Forms.Padding(4)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(205, 39)
        Me.btnExport.TabIndex = 34
        Me.btnExport.Text = "Export CSV"
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'btnStop
        '
        Me.btnStop.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnStop.Location = New System.Drawing.Point(1173, 6)
        Me.btnStop.Margin = New System.Windows.Forms.Padding(4)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(117, 38)
        Me.btnStop.TabIndex = 35
        Me.btnStop.Text = "Stop"
        Me.btnStop.UseVisualStyleBackColor = True
        '
        'dgrvMain
        '
        Me.dgrvMain.AllowUserToAddRows = False
        Me.dgrvMain.AllowUserToDeleteRows = False
        Me.dgrvMain.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgrvMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgrvMain.Location = New System.Drawing.Point(2, 182)
        Me.dgrvMain.Margin = New System.Windows.Forms.Padding(4)
        Me.dgrvMain.Name = "dgrvMain"
        Me.dgrvMain.ReadOnly = True
        Me.dgrvMain.RowHeadersVisible = False
        Me.dgrvMain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgrvMain.Size = New System.Drawing.Size(1295, 365)
        Me.dgrvMain.TabIndex = 49
        '
        'lblProgress
        '
        Me.lblProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblProgress.Location = New System.Drawing.Point(-1, 606)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(1298, 52)
        Me.lblProgress.TabIndex = 51
        Me.lblProgress.Text = "Progress Status"
        '
        'lblDescription
        '
        Me.lblDescription.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblDescription.Location = New System.Drawing.Point(-1, 551)
        Me.lblDescription.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDescription.Name = "lblDescription"
        Me.lblDescription.Size = New System.Drawing.Size(1298, 52)
        Me.lblDescription.TabIndex = 52
        Me.lblDescription.Text = "Description"
        '
        'cmbProcedure
        '
        Me.cmbProcedure.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbProcedure.FormattingEnabled = True
        Me.cmbProcedure.Items.AddRange(New Object() {"User Given", "ATR Based All Stock", "Pre Market Stock", "Intraday Volume Spike Stock", "OHL ATR Stock", "Touch Previous Day Last Candle", "Top Gainer Top Looser", "High Low Gap Stock", "Spot Future Arbritrage", "High Turnover Stock", "Top Gainer Top Looser Of Every Minute"})
        Me.cmbProcedure.Location = New System.Drawing.Point(87, 14)
        Me.cmbProcedure.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbProcedure.Name = "cmbProcedure"
        Me.cmbProcedure.Size = New System.Drawing.Size(389, 24)
        Me.cmbProcedure.TabIndex = 54
        '
        'lblProcedure
        '
        Me.lblProcedure.AutoSize = True
        Me.lblProcedure.Location = New System.Drawing.Point(8, 18)
        Me.lblProcedure.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProcedure.Name = "lblProcedure"
        Me.lblProcedure.Size = New System.Drawing.Size(78, 17)
        Me.lblProcedure.TabIndex = 53
        Me.lblProcedure.Text = "Procedure:"
        '
        'saveFile
        '
        '
        'pnlTopGainerLooserSettings
        '
        Me.pnlTopGainerLooserSettings.Controls.Add(Me.txtTopGainerLosserNiftyChangePercentage)
        Me.pnlTopGainerLooserSettings.Controls.Add(Me.Label5)
        Me.pnlTopGainerLooserSettings.Controls.Add(Me.dtpkrTopGainerLosserChkTime)
        Me.pnlTopGainerLooserSettings.Controls.Add(Me.Label4)
        Me.pnlTopGainerLooserSettings.Location = New System.Drawing.Point(493, 6)
        Me.pnlTopGainerLooserSettings.Name = "pnlTopGainerLooserSettings"
        Me.pnlTopGainerLooserSettings.Size = New System.Drawing.Size(257, 73)
        Me.pnlTopGainerLooserSettings.TabIndex = 61
        '
        'txtTopGainerLosserNiftyChangePercentage
        '
        Me.txtTopGainerLosserNiftyChangePercentage.Location = New System.Drawing.Point(118, 43)
        Me.txtTopGainerLosserNiftyChangePercentage.Name = "txtTopGainerLosserNiftyChangePercentage"
        Me.txtTopGainerLosserNiftyChangePercentage.Size = New System.Drawing.Size(100, 22)
        Me.txtTopGainerLosserNiftyChangePercentage.TabIndex = 7
        Me.txtTopGainerLosserNiftyChangePercentage.Text = "0"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(8, 43)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(105, 17)
        Me.Label5.TabIndex = 6
        Me.Label5.Text = "Nifty Change%:"
        '
        'dtpkrTopGainerLosserChkTime
        '
        Me.dtpkrTopGainerLosserChkTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpkrTopGainerLosserChkTime.Location = New System.Drawing.Point(118, 11)
        Me.dtpkrTopGainerLosserChkTime.Name = "dtpkrTopGainerLosserChkTime"
        Me.dtpkrTopGainerLosserChkTime.ShowUpDown = True
        Me.dtpkrTopGainerLosserChkTime.Size = New System.Drawing.Size(110, 22)
        Me.dtpkrTopGainerLosserChkTime.TabIndex = 5
        Me.dtpkrTopGainerLosserChkTime.Value = New Date(2019, 12, 8, 0, 0, 0, 0)
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(8, 12)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(108, 17)
        Me.Label4.TabIndex = 4
        Me.Label4.Text = "Check Till Time:"
        '
        'pnlIntradayVolumeSpikeSettings
        '
        Me.pnlIntradayVolumeSpikeSettings.Controls.Add(Me.dtpkrVolumeSpikeChkTime)
        Me.pnlIntradayVolumeSpikeSettings.Controls.Add(Me.Label3)
        Me.pnlIntradayVolumeSpikeSettings.Location = New System.Drawing.Point(493, 6)
        Me.pnlIntradayVolumeSpikeSettings.Name = "pnlIntradayVolumeSpikeSettings"
        Me.pnlIntradayVolumeSpikeSettings.Size = New System.Drawing.Size(257, 45)
        Me.pnlIntradayVolumeSpikeSettings.TabIndex = 60
        '
        'dtpkrVolumeSpikeChkTime
        '
        Me.dtpkrVolumeSpikeChkTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpkrVolumeSpikeChkTime.Location = New System.Drawing.Point(118, 11)
        Me.dtpkrVolumeSpikeChkTime.Name = "dtpkrVolumeSpikeChkTime"
        Me.dtpkrVolumeSpikeChkTime.ShowUpDown = True
        Me.dtpkrVolumeSpikeChkTime.Size = New System.Drawing.Size(110, 22)
        Me.dtpkrVolumeSpikeChkTime.TabIndex = 5
        Me.dtpkrVolumeSpikeChkTime.Value = New Date(2019, 12, 8, 0, 0, 0, 0)
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(8, 12)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(108, 17)
        Me.Label3.TabIndex = 4
        Me.Label3.Text = "Check Till Time:"
        '
        'pnlInstrumentList
        '
        Me.pnlInstrumentList.Controls.Add(Me.txtInstrumentList)
        Me.pnlInstrumentList.Controls.Add(Me.lblInstrumentList)
        Me.pnlInstrumentList.Location = New System.Drawing.Point(493, 6)
        Me.pnlInstrumentList.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlInstrumentList.Name = "pnlInstrumentList"
        Me.pnlInstrumentList.Size = New System.Drawing.Size(369, 84)
        Me.pnlInstrumentList.TabIndex = 59
        '
        'txtInstrumentList
        '
        Me.txtInstrumentList.Location = New System.Drawing.Point(116, 3)
        Me.txtInstrumentList.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentList.Multiline = True
        Me.txtInstrumentList.Name = "txtInstrumentList"
        Me.txtInstrumentList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtInstrumentList.Size = New System.Drawing.Size(244, 77)
        Me.txtInstrumentList.TabIndex = 41
        '
        'lblInstrumentList
        '
        Me.lblInstrumentList.AutoSize = True
        Me.lblInstrumentList.Location = New System.Drawing.Point(4, 1)
        Me.lblInstrumentList.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentList.Name = "lblInstrumentList"
        Me.lblInstrumentList.Size = New System.Drawing.Size(104, 17)
        Me.lblInstrumentList.TabIndex = 40
        Me.lblInstrumentList.Text = "Instrument List:"
        '
        'frmStockSelection
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1300, 661)
        Me.Controls.Add(Me.pnlTopGainerLooserSettings)
        Me.Controls.Add(Me.pnlIntradayVolumeSpikeSettings)
        Me.Controls.Add(Me.pnlInstrumentList)
        Me.Controls.Add(Me.cmbProcedure)
        Me.Controls.Add(Me.lblProcedure)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.lblDescription)
        Me.Controls.Add(Me.btnExport)
        Me.Controls.Add(Me.dgrvMain)
        Me.Controls.Add(Me.btnStart)
        Me.Controls.Add(Me.btnStop)
        Me.Controls.Add(Me.Panel1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmStockSelection"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Screener"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.dgrvMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlTopGainerLooserSettings.ResumeLayout(False)
        Me.pnlTopGainerLooserSettings.PerformLayout()
        Me.pnlIntradayVolumeSpikeSettings.ResumeLayout(False)
        Me.pnlIntradayVolumeSpikeSettings.PerformLayout()
        Me.pnlInstrumentList.ResumeLayout(False)
        Me.pnlInstrumentList.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Panel1 As Panel
    Friend WithEvents txtATRPercentage As TextBox
    Friend WithEvents txtNumberOfStock As TextBox
    Friend WithEvents lblATR As Label
    Friend WithEvents lblNumberOfStock As Label
    Friend WithEvents txtMaxPrice As TextBox
    Friend WithEvents lblMaxPrice As Label
    Friend WithEvents txtMinPrice As TextBox
    Friend WithEvents lblMinPrice As Label
    Friend WithEvents cmbStockType As ComboBox
    Friend WithEvents Label2 As Label
    Friend WithEvents txtMaxBlankCandlePercentage As TextBox
    Friend WithEvents lblMaxBlankCandlePercentage As Label
    Friend WithEvents dtpckrToDate As DateTimePicker
    Friend WithEvents dtpckrFromDate As DateTimePicker
    Friend WithEvents lblToDate As Label
    Friend WithEvents lblFromDate As Label
    Friend WithEvents chkbFOStock As CheckBox
    Friend WithEvents btnExport As Button
    Friend WithEvents btnStart As Button
    Friend WithEvents btnStop As Button
    Friend WithEvents dgrvMain As DataGridView
    Friend WithEvents lblProgress As Label
    Friend WithEvents lblDescription As Label
    Friend WithEvents cmbProcedure As ComboBox
    Friend WithEvents lblProcedure As Label
    Friend WithEvents saveFile As SaveFileDialog
    Friend WithEvents pnlTopGainerLooserSettings As Panel
    Friend WithEvents txtTopGainerLosserNiftyChangePercentage As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents dtpkrTopGainerLosserChkTime As DateTimePicker
    Friend WithEvents Label4 As Label
    Friend WithEvents pnlIntradayVolumeSpikeSettings As Panel
    Friend WithEvents dtpkrVolumeSpikeChkTime As DateTimePicker
    Friend WithEvents Label3 As Label
    Friend WithEvents pnlInstrumentList As Panel
    Friend WithEvents txtInstrumentList As TextBox
    Friend WithEvents lblInstrumentList As Label
End Class
