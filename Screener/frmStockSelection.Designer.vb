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
        Me.txtMaxAvgEODVolume = New System.Windows.Forms.TextBox()
        Me.lblMaxAvgEODVolume = New System.Windows.Forms.Label()
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
        Me.chkbTopGainerTopLosserOnlyBankniftyStocks = New System.Windows.Forms.CheckBox()
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
        Me.pnlNarrowRangeSettings = New System.Windows.Forms.Panel()
        Me.chkbNarrowRangeDownwardsChecking = New System.Windows.Forms.CheckBox()
        Me.txtNarrowRangeNmbrOfDays = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.pnlCPRNarrowRangeSettings = New System.Windows.Forms.Panel()
        Me.txtMinimumCPRRangePer = New System.Windows.Forms.TextBox()
        Me.lblMaxCPRPer = New System.Windows.Forms.Label()
        Me.pnlMultiTFSettings = New System.Windows.Forms.Panel()
        Me.cmbMultiTFIndicator = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.pnlLowRangeStocksOfXMinuteSettings = New System.Windows.Forms.Panel()
        Me.dtpckrLowRangeTime = New System.Windows.Forms.DateTimePicker()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.pnlMultiTFHKSignal = New System.Windows.Forms.Panel()
        Me.cmbMultiTFHKHTF = New System.Windows.Forms.ComboBox()
        Me.cmbMultiTFHKMTF = New System.Windows.Forms.ComboBox()
        Me.cmbMultiTFHKLTF = New System.Windows.Forms.ComboBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.pnlLastCandleNaughtyBoy = New System.Windows.Forms.Panel()
        Me.nmrcLastCandleNaughtyBoyTF = New System.Windows.Forms.NumericUpDown()
        Me.lblTFLastCandleNaughtyBoy = New System.Windows.Forms.Label()
        Me.Panel1.SuspendLayout()
        CType(Me.dgrvMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlTopGainerLooserSettings.SuspendLayout()
        Me.pnlIntradayVolumeSpikeSettings.SuspendLayout()
        Me.pnlInstrumentList.SuspendLayout()
        Me.pnlNarrowRangeSettings.SuspendLayout()
        Me.pnlCPRNarrowRangeSettings.SuspendLayout()
        Me.pnlMultiTFSettings.SuspendLayout()
        Me.pnlLowRangeStocksOfXMinuteSettings.SuspendLayout()
        Me.pnlMultiTFHKSignal.SuspendLayout()
        Me.pnlLastCandleNaughtyBoy.SuspendLayout()
        CType(Me.nmrcLastCandleNaughtyBoyTF, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.txtMaxAvgEODVolume)
        Me.Panel1.Controls.Add(Me.lblMaxAvgEODVolume)
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
        Me.Panel1.Location = New System.Drawing.Point(3, 97)
        Me.Panel1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(1295, 76)
        Me.Panel1.TabIndex = 0
        '
        'txtMaxAvgEODVolume
        '
        Me.txtMaxAvgEODVolume.Location = New System.Drawing.Point(1171, 9)
        Me.txtMaxAvgEODVolume.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtMaxAvgEODVolume.Name = "txtMaxAvgEODVolume"
        Me.txtMaxAvgEODVolume.Size = New System.Drawing.Size(113, 22)
        Me.txtMaxAvgEODVolume.TabIndex = 8
        '
        'lblMaxAvgEODVolume
        '
        Me.lblMaxAvgEODVolume.AutoSize = True
        Me.lblMaxAvgEODVolume.Location = New System.Drawing.Point(1012, 14)
        Me.lblMaxAvgEODVolume.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxAvgEODVolume.Name = "lblMaxAvgEODVolume"
        Me.lblMaxAvgEODVolume.Size = New System.Drawing.Size(150, 17)
        Me.lblMaxAvgEODVolume.TabIndex = 96
        Me.lblMaxAvgEODVolume.Text = "Max Avg EOD Volume:"
        '
        'chkbFOStock
        '
        Me.chkbFOStock.AutoSize = True
        Me.chkbFOStock.Location = New System.Drawing.Point(608, 11)
        Me.chkbFOStock.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.chkbFOStock.Name = "chkbFOStock"
        Me.chkbFOStock.Size = New System.Drawing.Size(128, 21)
        Me.chkbFOStock.TabIndex = 6
        Me.chkbFOStock.Text = "Only FO Stocks"
        Me.chkbFOStock.UseVisualStyleBackColor = True
        '
        'txtATRPercentage
        '
        Me.txtATRPercentage.Location = New System.Drawing.Point(473, 44)
        Me.txtATRPercentage.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtATRPercentage.Name = "txtATRPercentage"
        Me.txtATRPercentage.Size = New System.Drawing.Size(113, 22)
        Me.txtATRPercentage.TabIndex = 11
        Me.txtATRPercentage.Tag = "ATR %"
        '
        'txtNumberOfStock
        '
        Me.txtNumberOfStock.Location = New System.Drawing.Point(779, 44)
        Me.txtNumberOfStock.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtNumberOfStock.Name = "txtNumberOfStock"
        Me.txtNumberOfStock.Size = New System.Drawing.Size(96, 22)
        Me.txtNumberOfStock.TabIndex = 12
        '
        'lblATR
        '
        Me.lblATR.AutoSize = True
        Me.lblATR.Location = New System.Drawing.Point(413, 49)
        Me.lblATR.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblATR.Name = "lblATR"
        Me.lblATR.Size = New System.Drawing.Size(56, 17)
        Me.lblATR.TabIndex = 92
        Me.lblATR.Text = "ATR %:"
        '
        'lblNumberOfStock
        '
        Me.lblNumberOfStock.AutoSize = True
        Me.lblNumberOfStock.Location = New System.Drawing.Point(601, 49)
        Me.lblNumberOfStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNumberOfStock.Name = "lblNumberOfStock"
        Me.lblNumberOfStock.Size = New System.Drawing.Size(175, 17)
        Me.lblNumberOfStock.TabIndex = 93
        Me.lblNumberOfStock.Text = "Number Of Stock Per Day:"
        '
        'txtMaxPrice
        '
        Me.txtMaxPrice.Location = New System.Drawing.Point(287, 44)
        Me.txtMaxPrice.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtMaxPrice.Name = "txtMaxPrice"
        Me.txtMaxPrice.Size = New System.Drawing.Size(113, 22)
        Me.txtMaxPrice.TabIndex = 10
        Me.txtMaxPrice.Tag = "Max Price"
        '
        'lblMaxPrice
        '
        Me.lblMaxPrice.AutoSize = True
        Me.lblMaxPrice.Location = New System.Drawing.Point(203, 48)
        Me.lblMaxPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxPrice.Name = "lblMaxPrice"
        Me.lblMaxPrice.Size = New System.Drawing.Size(73, 17)
        Me.lblMaxPrice.TabIndex = 90
        Me.lblMaxPrice.Text = "Max Price:"
        '
        'txtMinPrice
        '
        Me.txtMinPrice.Location = New System.Drawing.Point(84, 44)
        Me.txtMinPrice.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtMinPrice.Name = "txtMinPrice"
        Me.txtMinPrice.Size = New System.Drawing.Size(108, 22)
        Me.txtMinPrice.TabIndex = 9
        Me.txtMinPrice.Tag = "Min Price"
        '
        'lblMinPrice
        '
        Me.lblMinPrice.AutoSize = True
        Me.lblMinPrice.Location = New System.Drawing.Point(5, 48)
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
        Me.cmbStockType.Location = New System.Drawing.Point(477, 9)
        Me.cmbStockType.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmbStockType.Name = "cmbStockType"
        Me.cmbStockType.Size = New System.Drawing.Size(120, 24)
        Me.cmbStockType.TabIndex = 5
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(391, 14)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(83, 17)
        Me.Label2.TabIndex = 85
        Me.Label2.Text = "Stock Type:"
        '
        'txtMaxBlankCandlePercentage
        '
        Me.txtMaxBlankCandlePercentage.Location = New System.Drawing.Point(885, 9)
        Me.txtMaxBlankCandlePercentage.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtMaxBlankCandlePercentage.Name = "txtMaxBlankCandlePercentage"
        Me.txtMaxBlankCandlePercentage.Size = New System.Drawing.Size(113, 22)
        Me.txtMaxBlankCandlePercentage.TabIndex = 7
        '
        'lblMaxBlankCandlePercentage
        '
        Me.lblMaxBlankCandlePercentage.AutoSize = True
        Me.lblMaxBlankCandlePercentage.Location = New System.Drawing.Point(739, 14)
        Me.lblMaxBlankCandlePercentage.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxBlankCandlePercentage.Name = "lblMaxBlankCandlePercentage"
        Me.lblMaxBlankCandlePercentage.Size = New System.Drawing.Size(140, 17)
        Me.lblMaxBlankCandlePercentage.TabIndex = 83
        Me.lblMaxBlankCandlePercentage.Text = "Max Blank Candle %:"
        '
        'dtpckrToDate
        '
        Me.dtpckrToDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrToDate.Location = New System.Drawing.Point(268, 9)
        Me.dtpckrToDate.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpckrToDate.Name = "dtpckrToDate"
        Me.dtpckrToDate.Size = New System.Drawing.Size(108, 22)
        Me.dtpckrToDate.TabIndex = 4
        '
        'dtpckrFromDate
        '
        Me.dtpckrFromDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrFromDate.Location = New System.Drawing.Point(84, 9)
        Me.dtpckrFromDate.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpckrFromDate.Name = "dtpckrFromDate"
        Me.dtpckrFromDate.Size = New System.Drawing.Size(108, 22)
        Me.dtpckrFromDate.TabIndex = 3
        '
        'lblToDate
        '
        Me.lblToDate.AutoSize = True
        Me.lblToDate.Location = New System.Drawing.Point(203, 14)
        Me.lblToDate.Name = "lblToDate"
        Me.lblToDate.Size = New System.Drawing.Size(63, 17)
        Me.lblToDate.TabIndex = 80
        Me.lblToDate.Text = "To Date:"
        '
        'lblFromDate
        '
        Me.lblFromDate.AutoSize = True
        Me.lblFromDate.Location = New System.Drawing.Point(5, 14)
        Me.lblFromDate.Name = "lblFromDate"
        Me.lblFromDate.Size = New System.Drawing.Size(78, 17)
        Me.lblFromDate.TabIndex = 79
        Me.lblFromDate.Text = "From Date:"
        '
        'btnStart
        '
        Me.btnStart.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnStart.Location = New System.Drawing.Point(1045, 6)
        Me.btnStart.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(119, 38)
        Me.btnStart.TabIndex = 0
        Me.btnStart.Text = "Start"
        Me.btnStart.UseVisualStyleBackColor = True
        '
        'btnExport
        '
        Me.btnExport.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnExport.Location = New System.Drawing.Point(1073, 49)
        Me.btnExport.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
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
        Me.btnStop.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(117, 38)
        Me.btnStop.TabIndex = 1
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
        Me.dgrvMain.Location = New System.Drawing.Point(3, 182)
        Me.dgrvMain.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.dgrvMain.Name = "dgrvMain"
        Me.dgrvMain.ReadOnly = True
        Me.dgrvMain.RowHeadersVisible = False
        Me.dgrvMain.RowHeadersWidth = 51
        Me.dgrvMain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        Me.dgrvMain.Size = New System.Drawing.Size(1295, 366)
        Me.dgrvMain.TabIndex = 49
        '
        'lblProgress
        '
        Me.lblProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblProgress.Location = New System.Drawing.Point(-1, 606)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(1299, 52)
        Me.lblProgress.TabIndex = 51
        Me.lblProgress.Text = "Progress Status"
        '
        'lblDescription
        '
        Me.lblDescription.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblDescription.Location = New System.Drawing.Point(-1, 551)
        Me.lblDescription.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDescription.Name = "lblDescription"
        Me.lblDescription.Size = New System.Drawing.Size(1299, 52)
        Me.lblDescription.TabIndex = 52
        Me.lblDescription.Text = "Description"
        '
        'cmbProcedure
        '
        Me.cmbProcedure.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbProcedure.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbProcedure.FormattingEnabled = True
        Me.cmbProcedure.Items.AddRange(New Object() {"User Given", "ATR Based All Stock", "Pre Market Stock", "Intraday Volume Spike Stock", "OHL ATR Stock", "Touch Previous Day Last Candle", "Top Gainer Top Looser", "High Low Gap Stock", "Spot Future Arbritrage", "High Turnover Stock", "Top Gainer Top Looser Of Every Minute", "High Slab Level Moved Stocks", "Open At High Low", "Multi Timeframe Signal", "Narrow Range Stocks", "Top Gainer Losser of Every Slab", "CPR Narrow Range Stocks", "Lowest Range Stock Of Every Minute", "Lowest Range Stock Of X Minute", "Lower Price Options With Volume OI", "Lower Price Options With OI Change%", "Strong Moved Stocks", "Low ATR Candle Quick Entry Stocks", "EOD Low Range Stocks", "Previous Day Strong HK Stocks", "Volume Sort PreviousDayClose Filter CEPE Options", "Volume Sort PreviousDayClose Filter Top2 Options", "Volume Sort CurrentDayOpen Filter CEPE Options", "Volume Sort CurrentDayOpen Filter Top2 Options", "PreviousDayCloseATR Sort Volume Filter CEPE Options", "PreviousDayCloseATR Sort Volume Filter Top2 Options", "CurrentDayOpen ATR Sort Volume Filter CEPE Options", "CurrentDayOpen ATR Sort Volume Filter Top2 Options", "Lowest Price At The Money Options", "Highest ATR At The Money Options", "Lower Deviation At The Money Options", "Lower Price Nearest Options", "Day Open At The Money Option", "Low Turnover Option", "Pre Market Options", "Fractal Constriction", "Max Slab Level Hits Stock", "ATR Based All Stock With Multiplier", "EOD Outside SMA Stocks", "EOD Outside EMA Stocks", "EOD Volume EMA Stocks", "Opening Price Options", "First Favourable Fracatal Top Gainer Looser", "EOD EMA Crossover Stocks", "EOD BTST NKS Stocks", "EOD BTST Bullish Engulfing Stocks", "EOD BTST Double TII Stocks", "EOD BTST 15 Min 23 Stocks", "EOD BTST 15 Min 57 Stocks", "EOD Rainbow Crossover", "Top Gainer Top Looser Options", "EOD Pivot Trend High ATR High Volume Stocks", "Nearest Options", "EOD HK Trend High ATR High Volume Stocks", "EOD HK MA Trend High ATR High Volume Stocks", "EOD Central Pivot Trend High ATR High Volume Stocks", "EOD HK Keltner Trend High ATR High Volume Stocks", "EOD Ichimoku Trend High ATR High Volume Stocks", "Hourly TII Trend High ATR High Volume Stocks", "Hourly HK MA Trend High ATR High Volume Stocks", "Multitimeframe HK Signal", "1 Year High Reached Stocks", "Below Fractal Low Stocks", "Naughty Boy Stocks", "Last Candle Naughty Boy Stocks", "Low Range ATR Stocks"})
        Me.cmbProcedure.Location = New System.Drawing.Point(87, 14)
        Me.cmbProcedure.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmbProcedure.Name = "cmbProcedure"
        Me.cmbProcedure.Size = New System.Drawing.Size(389, 26)
        Me.cmbProcedure.TabIndex = 2
        '
        'lblProcedure
        '
        Me.lblProcedure.AutoSize = True
        Me.lblProcedure.Location = New System.Drawing.Point(8, 17)
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
        Me.pnlTopGainerLooserSettings.Controls.Add(Me.chkbTopGainerTopLosserOnlyBankniftyStocks)
        Me.pnlTopGainerLooserSettings.Controls.Add(Me.txtTopGainerLosserNiftyChangePercentage)
        Me.pnlTopGainerLooserSettings.Controls.Add(Me.Label5)
        Me.pnlTopGainerLooserSettings.Controls.Add(Me.dtpkrTopGainerLosserChkTime)
        Me.pnlTopGainerLooserSettings.Controls.Add(Me.Label4)
        Me.pnlTopGainerLooserSettings.Location = New System.Drawing.Point(493, 6)
        Me.pnlTopGainerLooserSettings.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pnlTopGainerLooserSettings.Name = "pnlTopGainerLooserSettings"
        Me.pnlTopGainerLooserSettings.Size = New System.Drawing.Size(421, 70)
        Me.pnlTopGainerLooserSettings.TabIndex = 61
        '
        'chkbTopGainerTopLosserOnlyBankniftyStocks
        '
        Me.chkbTopGainerTopLosserOnlyBankniftyStocks.AutoSize = True
        Me.chkbTopGainerTopLosserOnlyBankniftyStocks.Location = New System.Drawing.Point(232, 46)
        Me.chkbTopGainerTopLosserOnlyBankniftyStocks.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.chkbTopGainerTopLosserOnlyBankniftyStocks.Name = "chkbTopGainerTopLosserOnlyBankniftyStocks"
        Me.chkbTopGainerTopLosserOnlyBankniftyStocks.Size = New System.Drawing.Size(185, 21)
        Me.chkbTopGainerTopLosserOnlyBankniftyStocks.TabIndex = 96
        Me.chkbTopGainerTopLosserOnlyBankniftyStocks.Text = "Only BANKNIFTY Stocks"
        Me.chkbTopGainerTopLosserOnlyBankniftyStocks.UseVisualStyleBackColor = True
        '
        'txtTopGainerLosserNiftyChangePercentage
        '
        Me.txtTopGainerLosserNiftyChangePercentage.Location = New System.Drawing.Point(117, 43)
        Me.txtTopGainerLosserNiftyChangePercentage.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
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
        Me.dtpkrTopGainerLosserChkTime.Location = New System.Drawing.Point(117, 11)
        Me.dtpkrTopGainerLosserChkTime.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpkrTopGainerLosserChkTime.Name = "dtpkrTopGainerLosserChkTime"
        Me.dtpkrTopGainerLosserChkTime.ShowUpDown = True
        Me.dtpkrTopGainerLosserChkTime.Size = New System.Drawing.Size(111, 22)
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
        Me.pnlIntradayVolumeSpikeSettings.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pnlIntradayVolumeSpikeSettings.Name = "pnlIntradayVolumeSpikeSettings"
        Me.pnlIntradayVolumeSpikeSettings.Size = New System.Drawing.Size(257, 46)
        Me.pnlIntradayVolumeSpikeSettings.TabIndex = 60
        '
        'dtpkrVolumeSpikeChkTime
        '
        Me.dtpkrVolumeSpikeChkTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpkrVolumeSpikeChkTime.Location = New System.Drawing.Point(117, 11)
        Me.dtpkrVolumeSpikeChkTime.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpkrVolumeSpikeChkTime.Name = "dtpkrVolumeSpikeChkTime"
        Me.dtpkrVolumeSpikeChkTime.ShowUpDown = True
        Me.dtpkrVolumeSpikeChkTime.Size = New System.Drawing.Size(111, 22)
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
        Me.pnlInstrumentList.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.pnlInstrumentList.Name = "pnlInstrumentList"
        Me.pnlInstrumentList.Size = New System.Drawing.Size(369, 84)
        Me.pnlInstrumentList.TabIndex = 59
        '
        'txtInstrumentList
        '
        Me.txtInstrumentList.Location = New System.Drawing.Point(116, 2)
        Me.txtInstrumentList.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
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
        'pnlNarrowRangeSettings
        '
        Me.pnlNarrowRangeSettings.Controls.Add(Me.chkbNarrowRangeDownwardsChecking)
        Me.pnlNarrowRangeSettings.Controls.Add(Me.txtNarrowRangeNmbrOfDays)
        Me.pnlNarrowRangeSettings.Controls.Add(Me.Label1)
        Me.pnlNarrowRangeSettings.Location = New System.Drawing.Point(493, 6)
        Me.pnlNarrowRangeSettings.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pnlNarrowRangeSettings.Name = "pnlNarrowRangeSettings"
        Me.pnlNarrowRangeSettings.Size = New System.Drawing.Size(323, 70)
        Me.pnlNarrowRangeSettings.TabIndex = 62
        '
        'chkbNarrowRangeDownwardsChecking
        '
        Me.chkbNarrowRangeDownwardsChecking.AutoSize = True
        Me.chkbNarrowRangeDownwardsChecking.Location = New System.Drawing.Point(11, 38)
        Me.chkbNarrowRangeDownwardsChecking.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.chkbNarrowRangeDownwardsChecking.Name = "chkbNarrowRangeDownwardsChecking"
        Me.chkbNarrowRangeDownwardsChecking.Size = New System.Drawing.Size(174, 21)
        Me.chkbNarrowRangeDownwardsChecking.TabIndex = 8
        Me.chkbNarrowRangeDownwardsChecking.Text = "Check Down Trend NR"
        Me.chkbNarrowRangeDownwardsChecking.UseVisualStyleBackColor = True
        '
        'txtNarrowRangeNmbrOfDays
        '
        Me.txtNarrowRangeNmbrOfDays.Location = New System.Drawing.Point(136, 11)
        Me.txtNarrowRangeNmbrOfDays.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtNarrowRangeNmbrOfDays.Name = "txtNarrowRangeNmbrOfDays"
        Me.txtNarrowRangeNmbrOfDays.Size = New System.Drawing.Size(100, 22)
        Me.txtNarrowRangeNmbrOfDays.TabIndex = 7
        Me.txtNarrowRangeNmbrOfDays.Text = "0"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(8, 11)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(117, 17)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "Number Of Days:"
        '
        'pnlCPRNarrowRangeSettings
        '
        Me.pnlCPRNarrowRangeSettings.Controls.Add(Me.txtMinimumCPRRangePer)
        Me.pnlCPRNarrowRangeSettings.Controls.Add(Me.lblMaxCPRPer)
        Me.pnlCPRNarrowRangeSettings.Location = New System.Drawing.Point(493, 6)
        Me.pnlCPRNarrowRangeSettings.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pnlCPRNarrowRangeSettings.Name = "pnlCPRNarrowRangeSettings"
        Me.pnlCPRNarrowRangeSettings.Size = New System.Drawing.Size(323, 70)
        Me.pnlCPRNarrowRangeSettings.TabIndex = 63
        '
        'txtMinimumCPRRangePer
        '
        Me.txtMinimumCPRRangePer.Location = New System.Drawing.Point(173, 14)
        Me.txtMinimumCPRRangePer.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtMinimumCPRRangePer.Name = "txtMinimumCPRRangePer"
        Me.txtMinimumCPRRangePer.Size = New System.Drawing.Size(100, 22)
        Me.txtMinimumCPRRangePer.TabIndex = 7
        Me.txtMinimumCPRRangePer.Text = "0"
        '
        'lblMaxCPRPer
        '
        Me.lblMaxCPRPer.AutoSize = True
        Me.lblMaxCPRPer.Location = New System.Drawing.Point(8, 14)
        Me.lblMaxCPRPer.Name = "lblMaxCPRPer"
        Me.lblMaxCPRPer.Size = New System.Drawing.Size(164, 17)
        Me.lblMaxCPRPer.TabIndex = 6
        Me.lblMaxCPRPer.Text = "Maximum CPR Range %:"
        '
        'pnlMultiTFSettings
        '
        Me.pnlMultiTFSettings.Controls.Add(Me.cmbMultiTFIndicator)
        Me.pnlMultiTFSettings.Controls.Add(Me.Label7)
        Me.pnlMultiTFSettings.Location = New System.Drawing.Point(493, 6)
        Me.pnlMultiTFSettings.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pnlMultiTFSettings.Name = "pnlMultiTFSettings"
        Me.pnlMultiTFSettings.Size = New System.Drawing.Size(323, 70)
        Me.pnlMultiTFSettings.TabIndex = 65
        '
        'cmbMultiTFIndicator
        '
        Me.cmbMultiTFIndicator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbMultiTFIndicator.FormattingEnabled = True
        Me.cmbMultiTFIndicator.Location = New System.Drawing.Point(84, 11)
        Me.cmbMultiTFIndicator.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.cmbMultiTFIndicator.Name = "cmbMultiTFIndicator"
        Me.cmbMultiTFIndicator.Size = New System.Drawing.Size(145, 24)
        Me.cmbMultiTFIndicator.TabIndex = 7
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(8, 14)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(66, 17)
        Me.Label7.TabIndex = 6
        Me.Label7.Text = "Indicator:"
        '
        'pnlLowRangeStocksOfXMinuteSettings
        '
        Me.pnlLowRangeStocksOfXMinuteSettings.Controls.Add(Me.dtpckrLowRangeTime)
        Me.pnlLowRangeStocksOfXMinuteSettings.Controls.Add(Me.Label9)
        Me.pnlLowRangeStocksOfXMinuteSettings.Location = New System.Drawing.Point(493, 6)
        Me.pnlLowRangeStocksOfXMinuteSettings.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pnlLowRangeStocksOfXMinuteSettings.Name = "pnlLowRangeStocksOfXMinuteSettings"
        Me.pnlLowRangeStocksOfXMinuteSettings.Size = New System.Drawing.Size(323, 70)
        Me.pnlLowRangeStocksOfXMinuteSettings.TabIndex = 66
        '
        'dtpckrLowRangeTime
        '
        Me.dtpckrLowRangeTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrLowRangeTime.Location = New System.Drawing.Point(100, 11)
        Me.dtpckrLowRangeTime.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.dtpckrLowRangeTime.Name = "dtpckrLowRangeTime"
        Me.dtpckrLowRangeTime.ShowUpDown = True
        Me.dtpckrLowRangeTime.Size = New System.Drawing.Size(111, 22)
        Me.dtpckrLowRangeTime.TabIndex = 5
        Me.dtpckrLowRangeTime.Value = New Date(2019, 12, 8, 0, 0, 0, 0)
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(8, 12)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(86, 17)
        Me.Label9.TabIndex = 4
        Me.Label9.Text = "Check Time:"
        '
        'pnlMultiTFHKSignal
        '
        Me.pnlMultiTFHKSignal.Controls.Add(Me.cmbMultiTFHKHTF)
        Me.pnlMultiTFHKSignal.Controls.Add(Me.cmbMultiTFHKMTF)
        Me.pnlMultiTFHKSignal.Controls.Add(Me.cmbMultiTFHKLTF)
        Me.pnlMultiTFHKSignal.Controls.Add(Me.Label10)
        Me.pnlMultiTFHKSignal.Controls.Add(Me.Label8)
        Me.pnlMultiTFHKSignal.Controls.Add(Me.Label6)
        Me.pnlMultiTFHKSignal.Location = New System.Drawing.Point(493, 6)
        Me.pnlMultiTFHKSignal.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pnlMultiTFHKSignal.Name = "pnlMultiTFHKSignal"
        Me.pnlMultiTFHKSignal.Size = New System.Drawing.Size(456, 84)
        Me.pnlMultiTFHKSignal.TabIndex = 67
        '
        'cmbMultiTFHKHTF
        '
        Me.cmbMultiTFHKHTF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbMultiTFHKHTF.FormattingEnabled = True
        Me.cmbMultiTFHKHTF.Items.AddRange(New Object() {"1 Min", "2 Min", "3 Min", "4 Min", "5 Min", "10 Min", "15 Min", "30 Min", "60 Min", "120 Min", "180 Min", "1 Day", "1 Week", "1 Month"})
        Me.cmbMultiTFHKHTF.Location = New System.Drawing.Point(348, 9)
        Me.cmbMultiTFHKHTF.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.cmbMultiTFHKHTF.Name = "cmbMultiTFHKHTF"
        Me.cmbMultiTFHKHTF.Size = New System.Drawing.Size(97, 24)
        Me.cmbMultiTFHKHTF.TabIndex = 9
        '
        'cmbMultiTFHKMTF
        '
        Me.cmbMultiTFHKMTF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbMultiTFHKMTF.FormattingEnabled = True
        Me.cmbMultiTFHKMTF.Items.AddRange(New Object() {"1 Min", "2 Min", "3 Min", "4 Min", "5 Min", "10 Min", "15 Min", "30 Min", "60 Min", "120 Min", "180 Min", "1 Day", "1 Week", "1 Month"})
        Me.cmbMultiTFHKMTF.Location = New System.Drawing.Point(197, 9)
        Me.cmbMultiTFHKMTF.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.cmbMultiTFHKMTF.Name = "cmbMultiTFHKMTF"
        Me.cmbMultiTFHKMTF.Size = New System.Drawing.Size(97, 24)
        Me.cmbMultiTFHKMTF.TabIndex = 8
        '
        'cmbMultiTFHKLTF
        '
        Me.cmbMultiTFHKLTF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbMultiTFHKLTF.FormattingEnabled = True
        Me.cmbMultiTFHKLTF.Items.AddRange(New Object() {"1 Min", "2 Mins", "3 Mins", "4 Mins", "5 Mins", "10 Mins", "15 Mins", "30 Mins", "60 Mins", "120 Mins", "180 Mins", "1 Day", "1 Week", "1 Month"})
        Me.cmbMultiTFHKLTF.Location = New System.Drawing.Point(47, 9)
        Me.cmbMultiTFHKLTF.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.cmbMultiTFHKLTF.Name = "cmbMultiTFHKLTF"
        Me.cmbMultiTFHKLTF.Size = New System.Drawing.Size(97, 24)
        Me.cmbMultiTFHKLTF.TabIndex = 7
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(308, 12)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(39, 17)
        Me.Label10.TabIndex = 6
        Me.Label10.Text = "HTF:"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(155, 12)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(40, 17)
        Me.Label8.TabIndex = 5
        Me.Label8.Text = "MTF:"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(8, 12)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(37, 17)
        Me.Label6.TabIndex = 4
        Me.Label6.Text = "LTF:"
        '
        'pnlLastCandleNaughtyBoy
        '
        Me.pnlLastCandleNaughtyBoy.Controls.Add(Me.nmrcLastCandleNaughtyBoyTF)
        Me.pnlLastCandleNaughtyBoy.Controls.Add(Me.lblTFLastCandleNaughtyBoy)
        Me.pnlLastCandleNaughtyBoy.Location = New System.Drawing.Point(493, 6)
        Me.pnlLastCandleNaughtyBoy.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pnlLastCandleNaughtyBoy.Name = "pnlLastCandleNaughtyBoy"
        Me.pnlLastCandleNaughtyBoy.Size = New System.Drawing.Size(456, 84)
        Me.pnlLastCandleNaughtyBoy.TabIndex = 68
        '
        'nmrcLastCandleNaughtyBoyTF
        '
        Me.nmrcLastCandleNaughtyBoyTF.Location = New System.Drawing.Point(44, 11)
        Me.nmrcLastCandleNaughtyBoyTF.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.nmrcLastCandleNaughtyBoyTF.Maximum = New Decimal(New Integer() {180, 0, 0, 0})
        Me.nmrcLastCandleNaughtyBoyTF.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nmrcLastCandleNaughtyBoyTF.Name = "nmrcLastCandleNaughtyBoyTF"
        Me.nmrcLastCandleNaughtyBoyTF.Size = New System.Drawing.Size(120, 22)
        Me.nmrcLastCandleNaughtyBoyTF.TabIndex = 5
        Me.nmrcLastCandleNaughtyBoyTF.Value = New Decimal(New Integer() {15, 0, 0, 0})
        '
        'lblTFLastCandleNaughtyBoy
        '
        Me.lblTFLastCandleNaughtyBoy.AutoSize = True
        Me.lblTFLastCandleNaughtyBoy.Location = New System.Drawing.Point(8, 12)
        Me.lblTFLastCandleNaughtyBoy.Name = "lblTFLastCandleNaughtyBoy"
        Me.lblTFLastCandleNaughtyBoy.Size = New System.Drawing.Size(29, 17)
        Me.lblTFLastCandleNaughtyBoy.TabIndex = 4
        Me.lblTFLastCandleNaughtyBoy.Text = "TF:"
        '
        'frmStockSelection
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1300, 661)
        Me.Controls.Add(Me.pnlLastCandleNaughtyBoy)
        Me.Controls.Add(Me.pnlMultiTFHKSignal)
        Me.Controls.Add(Me.pnlLowRangeStocksOfXMinuteSettings)
        Me.Controls.Add(Me.pnlMultiTFSettings)
        Me.Controls.Add(Me.pnlCPRNarrowRangeSettings)
        Me.Controls.Add(Me.pnlNarrowRangeSettings)
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
        Me.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
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
        Me.pnlNarrowRangeSettings.ResumeLayout(False)
        Me.pnlNarrowRangeSettings.PerformLayout()
        Me.pnlCPRNarrowRangeSettings.ResumeLayout(False)
        Me.pnlCPRNarrowRangeSettings.PerformLayout()
        Me.pnlMultiTFSettings.ResumeLayout(False)
        Me.pnlMultiTFSettings.PerformLayout()
        Me.pnlLowRangeStocksOfXMinuteSettings.ResumeLayout(False)
        Me.pnlLowRangeStocksOfXMinuteSettings.PerformLayout()
        Me.pnlMultiTFHKSignal.ResumeLayout(False)
        Me.pnlMultiTFHKSignal.PerformLayout()
        Me.pnlLastCandleNaughtyBoy.ResumeLayout(False)
        Me.pnlLastCandleNaughtyBoy.PerformLayout()
        CType(Me.nmrcLastCandleNaughtyBoyTF, System.ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents chkbTopGainerTopLosserOnlyBankniftyStocks As CheckBox
    Friend WithEvents pnlNarrowRangeSettings As Panel
    Friend WithEvents txtNarrowRangeNmbrOfDays As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents chkbNarrowRangeDownwardsChecking As CheckBox
    Friend WithEvents pnlCPRNarrowRangeSettings As Panel
    Friend WithEvents txtMinimumCPRRangePer As TextBox
    Friend WithEvents lblMaxCPRPer As Label
    Friend WithEvents pnlMultiTFSettings As Panel
    Friend WithEvents cmbMultiTFIndicator As ComboBox
    Friend WithEvents Label7 As Label
    Friend WithEvents pnlLowRangeStocksOfXMinuteSettings As Panel
    Friend WithEvents dtpckrLowRangeTime As DateTimePicker
    Friend WithEvents Label9 As Label
    Friend WithEvents pnlMultiTFHKSignal As Panel
    Friend WithEvents Label6 As Label
    Friend WithEvents cmbMultiTFHKLTF As ComboBox
    Friend WithEvents Label10 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents cmbMultiTFHKHTF As ComboBox
    Friend WithEvents cmbMultiTFHKMTF As ComboBox
    Friend WithEvents pnlLastCandleNaughtyBoy As Panel
    Friend WithEvents nmrcLastCandleNaughtyBoyTF As NumericUpDown
    Friend WithEvents lblTFLastCandleNaughtyBoy As Label
    Friend WithEvents txtMaxAvgEODVolume As TextBox
    Friend WithEvents lblMaxAvgEODVolume As Label
End Class
