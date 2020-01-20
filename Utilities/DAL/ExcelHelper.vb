Imports Excel = Microsoft.Office.Interop.Excel
Imports System.IO
Imports NLog
Imports System.Drawing
Imports System.Threading

Namespace DAL
    Public Class ExcelHelper
        Implements IDisposable
#Region "Logging and Status Progress"
        Public Shared logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Events"
        Public Event WaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        Public Event Heartbeat(ByVal msg As String)
        'The below functions are needed to allow the derived classes to raise the above two events
        Protected Overridable Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
            RaiseEvent WaitingFor(elapsedSecs, totalSecs, msg)
        End Sub
        Protected Overridable Sub OnHeartbeat(ByVal msg As String)
            RaiseEvent Heartbeat(msg)
        End Sub
#End Region

#Region "Enums"
        Public Enum XLAlign
            Left
            Center
            Right
            General
        End Enum
        Public Enum XLColor
            Aqua = 42
            Black = 1
            Blue = 5
            BlueGray = 47
            BrightGreen = 4
            Brown = 53
            Cream = 19
            DarkBlue = 11
            DarkGreen = 51
            DarkPurple = 21
            DarkRed = 9
            DarkTeal = 49
            DarkYellow = 12
            Gold = 44
            Gray25 = 15
            Gray40 = 48
            Gray50 = 16
            Gray80 = 56
            Green = 10
            Indigo = 55
            Lavender = 39
            LightBlue = 41
            LightGreen = 35
            LightLavender = 24
            LightOrange = 45
            LightTurquoise = 20
            LightYellow = 36
            Lime = 43
            NavyBlue = 23
            OliveGreen = 52
            Orange = 46
            PaleBlue = 37
            Pink = 7
            Plum = 18
            PowderBlue = 17
            Red = 3
            Rose = 38
            Salmon = 22
            SeaGreen = 50
            SkyBlue = 33
            Tan = 40
            Teal = 14
            Turquoise = 8
            Violet = 13
            White = 2
            Yellow = 6
        End Enum
        Public Enum ExcelSaveType
            XLS_XLSX = 1
            CSV
            TAB
        End Enum
        Public Enum ExcelOpenStatus
            OpenAfreshForWrite = 1
            OpenExistingForReadWrite
        End Enum
        Public Enum XLBorder
            Thin
            Thick
            Medium
            Hairline
        End Enum
#End Region

#Region "Constructors"
        Public Sub New(ByVal fileName As String,
                       ByVal excelState As ExcelOpenStatus,
                       ByVal excelSaveType As ExcelSaveType,
                       ByVal canceller As CancellationTokenSource)
            _excelFileName = fileName
            _SaveType = excelSaveType
            _canceller = canceller
            logger.Debug("Preparing to close any open instances of excel")
            CloseOpenInstances()
            Select Case excelState
                Case ExcelOpenStatus.OpenAfreshForWrite
                    logger.Debug("File:{0}, Deleting any existing file since its a fresh write", _excelFileName)
                    If File.Exists(_excelFileName) Then
                        File.Delete(_excelFileName)
                    End If
                    Do Until Not File.Exists(_excelFileName)
                        _canceller.Token.ThrowIfCancellationRequested()
                        System.Windows.Forms.Application.DoEvents()
                    Loop
                    OnHeartbeat("Opening excel application")
                    _excelInstance = New Excel.Application
                    _wBookInstance = _excelInstance.Workbooks.Add
                Case ExcelOpenStatus.OpenExistingForReadWrite
                    OnHeartbeat("Opening excel application")
                    _excelInstance = New Excel.Application
                    _wBookInstance = _excelInstance.Workbooks.Open(_excelFileName)
            End Select
            _excelInstance.Visible = False
            _excelInstance.ScreenUpdating = False
            _excelInstance.DisplayAlerts = False
            _excelInstance.ErrorCheckingOptions.BackgroundChecking = False
            _excelInstance.ErrorCheckingOptions.NumberAsText = False
            _wSheetInstance = _wBookInstance.ActiveSheet
            OpeningColor = GetCellBackColor(1, 1)
        End Sub
#End Region

#Region "Private Attributes"
        Private _excelFileName As String
        Private _excelInstance As Excel.Application
        Private _wBookInstance As Excel.Workbook
        Private _wSheetInstance As Excel.Worksheet
        Private _SaveType As ExcelSaveType
        Protected _canceller As CancellationTokenSource
#End Region

#Region "Public Attributes"
        Public OpeningColor As XLColor
#End Region

#Region "Private Methods"
        Private Sub ReleaseObject(ByVal obj As Object)
            Try
                logger.Debug("Releasing the excel object")
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj)
                obj = Nothing
            Catch ex As Exception
                logger.Debug("Supressed error")
                logger.Error(ex)
                obj = Nothing
            Finally
                GC.Collect()
            End Try
        End Sub
#End Region

#Region "Public Methods"
        Public Function GetExcelInMemory() As Object(,)
            logger.Debug("Getting excel file into memory")
            Dim rg As Excel.Range = _wSheetInstance.Range(Me.GetNamedRange(1, Me.GetLastRow, 1, Me.GetLastCol))
            Dim ret As Object(,) = DirectCast(rg.Value2, Object(,))
            rg = Nothing
            Return ret
        End Function
        Public Sub WriteArrayToExcel(ByVal arr(,) As Object, ByVal rangeStr As String)
            logger.Debug("Writing from memory(array) to file")
            _wSheetInstance.Range(rangeStr, Type.Missing).Value2 = arr
        End Sub
        Public Sub SetColumnFormat(ByVal columnNumber As Integer, ByVal numberFormat As String)
            logger.Debug("Setting column format")
            _wSheetInstance.Columns(GetColumnName(columnNumber)).NumberFormat = numberFormat
        End Sub
        Public Sub SetColumnsBlank(ByVal columnNumbersToBeSetBlank As List(Of Integer))
            logger.Debug("Setting columns blank")
            If _excelInstance IsNot Nothing AndAlso _wBookInstance IsNot Nothing AndAlso _wSheetInstance IsNot Nothing Then
                Dim lastRow As Long = GetLastRow()
                For rowCtr As Long = 2 To lastRow
                    OnHeartbeat(String.Format("Setting blank (row #{0}/{1})", rowCtr, lastRow))
                    logger.Debug("Setting blank (row #{0}/{1})", rowCtr, lastRow)
                    _canceller.Token.ThrowIfCancellationRequested()
                    If columnNumbersToBeSetBlank IsNot Nothing Then
                        For Each columnNumberToBeSetBlank As Integer In columnNumbersToBeSetBlank
                            _canceller.Token.ThrowIfCancellationRequested()
                            If columnNumberToBeSetBlank > 0 Then
                                SetData(rowCtr, columnNumberToBeSetBlank, "")
                            End If
                        Next
                    End If
                Next
            End If
        End Sub
        Public Function IsOpenInCloud() As Boolean
            logger.Debug("Checking if open in cloud")
            Dim ret As Boolean = False
            If _excelInstance IsNot Nothing AndAlso _wBookInstance IsNot Nothing AndAlso _wSheetInstance IsNot Nothing Then
                Dim columnName As String = GetColumnName(1)
                Dim cellBackGroundColor As XLColor = OpeningColor
                If cellBackGroundColor = XLColor.Red Or cellBackGroundColor = XLColor.Yellow Then
                    ret = True
                Else
                    ret = False
                End If
                cellBackGroundColor = Nothing
            Else
                ret = False
            End If
            Return ret
        End Function
        Public Sub MarkInUse()
            logger.Debug("Marking file in use")
            SetCellBackColor(1, 1, XLColor.Yellow)
        End Sub
        Public Sub MarkOriginalColor()
            logger.Debug("Marking original color")
            SetCellBackColor(1, 1, OpeningColor)
        End Sub
        Public Sub ClearWholeColumn(ByVal columnCtr As Integer)
            logger.Debug("Clearing whole column")
            Dim rg As Excel.Range = _wSheetInstance.Columns(String.Format("{0}:{1}", GetColumnName(columnCtr), GetColumnName(columnCtr))) ' delete the specific row
            rg.Clear()
            rg = Nothing
        End Sub
        Public Sub SetCellWrapWholeColumn(ByVal columnCtr As Integer, ByVal wrap As Boolean)
            logger.Debug("Setting cell wrap for whole column")
            Dim rg As Excel.Range = _wSheetInstance.Columns(String.Format("{0}:{1}", GetColumnName(columnCtr), GetColumnName(columnCtr))) ' delete the specific row
            rg.WrapText = wrap
            rg = Nothing
        End Sub
        Public Sub SortByColum(ByVal startRowCtr As Integer, ByVal columnCtr As Integer)
            logger.Debug("Sorting by column")
            Dim rg As Excel.Range = _wSheetInstance.Range(String.Format("A{0}", startRowCtr), Me.GetNamedRange(Me.GetLastRow, Me.GetLastCol, 1))
            rg.Select()
            OnHeartbeat(String.Format("Sorting the obtained range for the column (Column:{0})", columnCtr))
            rg.Sort(Key1:=rg.Range(Me.GetNamedRange(1, columnCtr, 1)),
                                    Order1:=Excel.XlSortOrder.xlAscending,
                                    Orientation:=Excel.XlSortOrientation.xlSortColumns)
            rg = Nothing
        End Sub
        Public Shared Function IsExcelOpen(ByVal fileName As String) As Boolean
            logger.Debug("Checking if file is open")
            Dim ret As Boolean = False
            'Function designed to test if a specific Excel
            'workbook is open or not.

            Dim i As Long
            Dim XLAppFx As Excel.Application
            Dim NotOpen As Boolean

            'Find/create an Excel instance
            On Error Resume Next
            logger.Debug("Opening excel application")
            XLAppFx = GetObject(, "Excel.Application")
            If Err.Number = 429 Then
                NotOpen = True
                XLAppFx = CreateObject("Excel.Application")
                Err.Clear()
            End If

            logger.Debug("Checking if file is open in excel (File:{0})", fileName)

            'Loop through all open workbooks in such instance
            For i = XLAppFx.Workbooks.Count To 1 Step -1
                If XLAppFx.Workbooks(i).Name = Path.GetFileName(fileName) Then Exit For
            Next i

            'Set all to False
            ret = False

            'Perform check to see if name was found
            If i <> 0 Then ret = True

            'Close if was closed
            logger.Debug("Quitting excel application")
            If NotOpen Then XLAppFx.Quit()

            'Release the instance
            XLAppFx = Nothing
            Return ret
        End Function
        Public Shared Function CloseOpenExcelWorkbook(ByVal fileName As String) As Boolean
            logger.Debug("Closing open excel workbook")
            Dim ret As Boolean = False
            'Function designed to test if a specific Excel
            'workbook is open or not.

            Dim i As Long
            Dim XLAppFx As Excel.Application
            Dim NotOpen As Boolean

            'Find/create an Excel instance
            logger.Debug("Opening excel application")
            On Error Resume Next
            XLAppFx = GetObject(, "Excel.Application")
            If Err.Number = 429 Then
                NotOpen = True
                XLAppFx = CreateObject("Excel.Application")
                Err.Clear()
            End If

            'Loop through all open workbooks in such instance
            logger.Debug("Closing if file is open in excel (File:{0})", fileName)
            For i = XLAppFx.Workbooks.Count To 1 Step -1
                If XLAppFx.Workbooks(i).Name = Path.GetFileName(fileName) Then
                    XLAppFx.Workbooks(i).Close(False)
                    Exit For
                End If
            Next i

            logger.Debug("Intentional delay before rechecking if open")
            'Recheck its not there
            'Give some time for closure
            Task.Delay(5000)

            'Assume this is done and hence setting flag = true

            logger.Debug("Waiting till file is closed (File:{0})", fileName)
            ret = True
            For i = XLAppFx.Workbooks.Count To 1 Step -1
                If XLAppFx.Workbooks(i).Name = Path.GetFileName(fileName) Then
                    ret = False
                    Exit For
                End If
            Next i

            'Close if was closed
            logger.Debug("Quitting excel application")
            If NotOpen Then XLAppFx.Quit()

            'Release the instance
            XLAppFx = Nothing
            Return ret
        End Function
        Public Shared Function IsFileOpen(ByVal fileName As String) As Boolean
            logger.Debug("Checking if file open")
            Dim filenum As Integer, errnum
            Dim ret As Boolean = False

            On Error Resume Next ' Turn error checking off.
            logger.Debug("Attempting to open the file in anticipation of error if already open (File{0})", fileName)
            filenum = FreeFile() ' Get a free file number.
            ' Attempt to open the file and lock it.
            Microsoft.VisualBasic.FileOpen(filenum, fileName, OpenMode.Input, OpenAccess.Read, OpenShare.LockRead)
            Microsoft.VisualBasic.FileClose(filenum) ' Close the file.
            errnum = Err() ' Save the error number that occurred.
            ret = False
            On Error GoTo 0 ' Turn error checking back on.
            ' Check to see which error occurred.
            Select Case errnum.lastdllerror
                ' No error occurred.
                ' File is NOT already open by another user.
                Case 0
                    ret = False
                Case 2
                    ret = False
                    ' Error number for "Permission Denied."
                    ' File is already opened by another user.
                Case 70
                    ret = True
                    ' Another error occurred.
                Case Else
                    ret = True
            End Select
            Return ret
        End Function
        Public Sub CreateNewSheet(ByVal sheetName As String)
            logger.Debug("Creating new sheet")
            _wSheetInstance = _wBookInstance.Worksheets.Add
            _wSheetInstance.Name = sheetName
        End Sub
        Public Function SetActiveSheet(ByVal sheetName As String) As Boolean
            logger.Debug("Setting active sheet")
            Dim ret As Boolean = False
            For i = 1 To _wBookInstance.Sheets.Count
                _canceller.Token.ThrowIfCancellationRequested()
                If _wBookInstance.Sheets.Item(i).Name.ToString.ToLower = sheetName.ToLower Then
                    _wSheetInstance = _wBookInstance.Sheets.Item(i)
                    ret = True
                    Exit For
                End If
            Next
            Return ret
        End Function
        Public Function GetNamedRange(ByVal rowCtr As Integer, ByVal startColumnCtr As Integer, ByVal totalColumns As Long) As String
            logger.Debug("Getting named range")
            Dim startNamedRange As String = String.Format("{0}{1}", GetColumnName(startColumnCtr), rowCtr)
            Dim endNamedRange As String = String.Format("{0}{1}", GetColumnName(startColumnCtr + totalColumns), rowCtr)
            Return String.Format("{0}:{1}", startNamedRange, endNamedRange)
        End Function
        Public Function GetNamedRange(ByVal startRowCtr As Integer, ByVal totalRows As Long, ByVal startColumnCtr As Integer, ByVal totalColumns As Long) As String
            logger.Debug("Getting named range")
            Dim startNamedRange As String = String.Format("{0}{1}", GetColumnName(startColumnCtr), startRowCtr)
            Dim endNamedRange As String = String.Format("{0}{1}", GetColumnName(startColumnCtr + totalColumns), startRowCtr + totalRows)
            Return String.Format("{0}:{1}", startNamedRange, endNamedRange)
        End Function
        Public Sub SetCellBackColor(ByVal row As Integer, ByVal col As Integer, ByVal color As XLColor)
            logger.Debug("Setting cell back color")
            Dim columnName As String = GetColumnName(col)
            _wSheetInstance.Range(String.Format("{0}{1}", columnName, row)).Interior.ColorIndex = color
            columnName = Nothing
        End Sub
        Public Function GetCellBackColor(ByVal row As Integer, ByVal col As Integer) As XLColor
            logger.Debug("Getting cell back color")
            Dim columnName As String = GetColumnName(col)
            Return _wSheetInstance.Range(String.Format("{0}{1}", columnName, 1)).Interior.ColorIndex
        End Function
        Public Sub SetCellFontColor(ByVal row As Integer, ByVal col As Integer, ByVal color As XLColor)
            logger.Debug("Setting cell font color")
            Dim columnName As String = GetColumnName(col)
            _wSheetInstance.Range(String.Format("{0}{1}", columnName, row)).Font.ColorIndex = color
        End Sub
        Public Sub SetCellBorder(ByVal startRow As Integer, ByVal startCol As Integer, ByVal endRow As Integer, ByVal endCol As Integer, Optional ByVal xlBorderStyle As XLBorder = XLBorder.Thin)
            logger.Debug("Setting cell border color")
            Dim rg As Excel.Range = _wSheetInstance.Range(String.Format("{0}{1}:{2}{3}", GetColumnName(startCol), startRow, GetColumnName(endCol), endRow))
            Select Case xlBorderStyle
                Case XLBorder.Thin
                    rg.BorderAround(, Excel.XlBorderWeight.xlThin, , )
                Case XLBorder.Hairline
                    rg.BorderAround(, Excel.XlBorderWeight.xlHairline, , )
                Case XLBorder.Medium
                    rg.BorderAround(, Excel.XlBorderWeight.xlMedium, , )
                Case XLBorder.Thick
                    rg.BorderAround(, Excel.XlBorderWeight.xlThick, , )
            End Select
            rg = Nothing
        End Sub
        Public Sub SetCellBorder(ByVal row As Integer, ByVal col As Integer, Optional ByVal xlBorderStyle As XLBorder = XLBorder.Thin)
            logger.Debug("Setting cell border")
            Dim rg As Excel.Range = _wSheetInstance.Range(String.Format("{0}{1}", GetColumnName(col), row))
            Select Case xlBorderStyle
                Case XLBorder.Thin
                    rg.BorderAround(, Excel.XlBorderWeight.xlThin, , )
                Case XLBorder.Hairline
                    rg.BorderAround(, Excel.XlBorderWeight.xlHairline, , )
                Case XLBorder.Medium
                    rg.BorderAround(, Excel.XlBorderWeight.xlMedium, , )
                Case XLBorder.Thick
                    rg.BorderAround(, Excel.XlBorderWeight.xlThick, , )
            End Select
            rg = Nothing
        End Sub
        Public Sub SetCellFontStyle(ByVal row As Integer, ByVal col As Integer, ByVal font As System.Drawing.Font)
            logger.Debug("Setting cell font style")
            Dim columnName As String = GetColumnName(col)
            _wSheetInstance.Range(String.Format("{0}{1}", columnName, row)).Font.Bold = font.Bold
            _wSheetInstance.Range(String.Format("{0}{1}", columnName, row)).Font.Italic = font.Italic
            _wSheetInstance.Range(String.Format("{0}{1}", columnName, row)).Font.Name = font.Name
            _wSheetInstance.Range(String.Format("{0}{1}", columnName, row)).Font.Size = font.Size
            _wSheetInstance.Range(String.Format("{0}{1}", columnName, row)).Font.Strikethrough = font.Strikeout
            _wSheetInstance.Range(String.Format("{0}{1}", columnName, row)).Font.Underline = font.Underline
            columnName = Nothing
        End Sub
        Public Sub CloseOpenInstances()
            logger.Debug("Closing any open instances")
            Try
                logger.Debug("Preparing to mark original color")
                MarkOriginalColor()
                _canceller.Token.ThrowIfCancellationRequested()
                logger.Debug("Preparing to save excel")
                SaveExcel()
                _canceller.Token.ThrowIfCancellationRequested()
            Catch ex As Exception
                logger.Debug("Supressed error")
                logger.Error(ex)
            End Try
            Try
                logger.Debug("Closing workbook")
                If Not _wBookInstance Is Nothing Then _wBookInstance.Close()
            Catch ex As Exception
                logger.Debug("Supressed error")
                logger.Error(ex)
            End Try
            Try
                logger.Debug("Closing excel instance")
                If Not _excelInstance Is Nothing Then _excelInstance.Quit()
            Catch ex As Exception
                logger.Debug("Supressed error")
                logger.Error(ex)
            End Try
            logger.Debug("Preparing to release object")
            _canceller.Token.ThrowIfCancellationRequested()
            ReleaseObject(_excelInstance)
            ReleaseObject(_wBookInstance)
            ReleaseObject(_wSheetInstance)
        End Sub
        Public Sub DeleteRow(ByVal rowCtr As Integer)
            logger.Debug("Deleting row")
            Dim rg As Excel.Range = _wSheetInstance.Rows(String.Format("{0}:{1}", rowCtr, rowCtr)) ' delete the specific row
            rg.Select()
            rg.Delete()
            rg = Nothing
        End Sub
        Public Sub DeleteColumn(ByVal columnCtr As Integer)
            logger.Debug("Deleting column")
            Dim rg As Excel.Range = _wSheetInstance.Columns(String.Format("{0}:{1}", GetColumnName(columnCtr), GetColumnName(columnCtr))) ' delete the specific row
            rg.Select()
            rg.Delete()
            rg = Nothing
        End Sub
        Public Function GetColumnName(ByVal colNum As Integer) As String
            logger.Debug("Getting column name")
            Dim d As Integer
            Dim m As Integer
            Dim name As String
            d = colNum
            name = ""
            Do While (d > 0)
                _canceller.Token.ThrowIfCancellationRequested()
                m = (d - 1) Mod 26
                name = Chr(65 + m) + name
                d = Int((d - m) / 26)
            Loop
            Return name
        End Function
        Public Function GetData(ByVal rowNum As Long, ByVal colNum As Long) As Object
            logger.Debug("Getting data")
            Return _wSheetInstance.Cells(rowNum, colNum).value
        End Function
        Public Sub SetCellWrap(ByVal rowNum As Integer, ByVal colNum As Integer, ByVal wrap As Boolean)
            logger.Debug("Setting cell wrap")
            _wSheetInstance.Cells(rowNum, colNum).WrapText = wrap
        End Sub
        Public Sub SetData(ByVal rowNum As Long, ByVal colNum As Long, ByVal data As String)
            logger.Debug("Setting data")
            SetData(rowNum, colNum, data, "@")
        End Sub
        Public Sub SetData(ByVal rowNum As Long, ByVal colNum As Long, ByVal data As String, ByVal isHyperLink As Boolean)
            logger.Debug("Setting data")
            If isHyperLink Then
                _wSheetInstance.Hyperlinks.Add(_wSheetInstance.Cells(rowNum, colNum), data)
            Else
                SetData(rowNum, colNum, data, "@")
            End If
        End Sub
        Public Sub SetData(ByVal rowNum As Long, ByVal colNum As Long, ByVal data As String, ByVal alignment As XLAlign)
            logger.Debug("Setting data")
            SetData(rowNum, colNum, data, "@", alignment)
        End Sub
        Public Sub SetData(ByVal rowNum As Long, ByVal colNum As Long, ByVal data As String, ByVal numberFormat As String)
            logger.Debug("Setting data")
            SetData(rowNum, colNum, data, XLAlign.General)
        End Sub
        Public Sub SetData(ByVal rowNum As Long, ByVal colNum As Long, ByVal data As String, ByVal numberFormat As String, ByVal alignment As XLAlign)
            logger.Debug("Setting data")
            _wSheetInstance.Cells(rowNum, colNum).NumberFormat = numberFormat
            _wSheetInstance.Cells(rowNum, colNum).value = data
            Select Case alignment
                Case XLAlign.Center
                    _wSheetInstance.Cells(rowNum, colNum).HorizontalAlignment = Excel.Constants.xlCenter
                Case XLAlign.Left
                    _wSheetInstance.Cells(rowNum, colNum).HorizontalAlignment = Excel.Constants.xlLeft
                Case XLAlign.Right
                    _wSheetInstance.Cells(rowNum, colNum).HorizontalAlignment = Excel.Constants.xlRight
                Case XLAlign.General
                    _wSheetInstance.Cells(rowNum, colNum).HorizontalAlignment = Excel.Constants.xlGeneral
            End Select
        End Sub
        Public Sub SetComment(ByVal rowNum As Long, ByVal colNum As Long, ByVal data As String, ByVal width As Integer, ByVal height As Integer)
            logger.Debug("Setting comment")
            _wSheetInstance.Cells(rowNum, colNum).AddComment()
            _wSheetInstance.Cells(rowNum, colNum).Comment.Visible = False
            _wSheetInstance.Cells(rowNum, colNum).Comment.Text(Text:=data)
            _wSheetInstance.Cells(rowNum, colNum).Comment.Shape.Height = height
            _wSheetInstance.Cells(rowNum, colNum).Comment.shape.Width = width
        End Sub
        Public Sub SaveExcel()
            logger.Debug("Saving excel")

            OnHeartbeat(String.Format("Saving excel (File:{0})", _excelFileName))
            Select Case _SaveType
                Case ExcelSaveType.XLS_XLSX
                    _wBookInstance.SaveAs(_excelFileName)
                Case ExcelSaveType.CSV
                    _wBookInstance.SaveAs(_excelFileName, FileFormat:=Excel.XlFileFormat.xlCSVWindows)
                Case ExcelSaveType.TAB
                    _wBookInstance.SaveAs(_excelFileName, FileFormat:=Excel.XlFileFormat.xlTextWindows)
            End Select
        End Sub
        Public Sub SetCellFormula(ByVal rowNum As Integer, ByVal colNum As Integer, ByVal formula As String)
            logger.Debug("Setting cell formula")
            SetCellFormula(rowNum, colNum, formula, "General")
        End Sub
        Public Sub SetCellFormula(ByVal rowNum As Integer, ByVal colNum As Integer, ByVal formula As String, ByVal alignment As XLAlign)
            logger.Debug("Setting cell formula")
            SetCellFormula(rowNum, colNum, formula, "General", alignment)
        End Sub
        Public Sub SetCellFormula(ByVal rowNum As Integer, ByVal colNum As Integer, ByVal formula As String, ByVal numberFormat As String, ByVal alignment As XLAlign)
            logger.Debug("Setting cell formula")
            _wSheetInstance.Cells(rowNum, colNum).NumberFormat = numberFormat
            _wSheetInstance.Cells(rowNum, colNum).FORMULA = formula
            Select Case alignment
                Case XLAlign.Center
                    _wSheetInstance.Cells(rowNum, colNum).HorizontalAlignment = Excel.Constants.xlCenter
                Case XLAlign.Left
                    _wSheetInstance.Cells(rowNum, colNum).HorizontalAlignment = Excel.Constants.xlLeft
                Case XLAlign.Right
                    _wSheetInstance.Cells(rowNum, colNum).HorizontalAlignment = Excel.Constants.xlRight
                Case XLAlign.General
                    _wSheetInstance.Cells(rowNum, colNum).HorizontalAlignment = Excel.Constants.xlGeneral
            End Select
        End Sub
        Public Sub SetCellFormula(ByVal rowNum As Integer, ByVal colNum As Integer, ByVal formula As String, ByVal numberFormat As String)
            logger.Debug("Setting cell formula")
            SetCellFormula(rowNum, colNum, formula, numberFormat, XLAlign.General)
        End Sub
        Public Sub SetCellWidth(ByVal rowNum As Integer, ByVal colNum As Integer, ByVal width As Integer)
            logger.Debug("Setting cell width")
            _wSheetInstance.Cells(rowNum, colNum).EntireColumn.ColumnWidth = width
        End Sub
        Public Sub SetCellHeight(ByVal rowNum As Integer, ByVal colNum As Integer, ByVal height As Integer)
            logger.Debug("Setting cell height")
            _wSheetInstance.Cells(rowNum, colNum).EntireRow.RowHeight = height
        End Sub
        Public Sub SetCellHeight(ByVal rowNum As Integer, ByVal height As Integer)
            logger.Debug("Setting cell height")
            _wSheetInstance.Rows(rowNum).RowHeight = height
        End Sub
        Public Sub CheckExcelSchema(ByVal excelSchema As String())
            logger.Debug("Checking excel schema")
            Try
                For colCtr As Integer = 1 To excelSchema.Count
                    _canceller.Token.ThrowIfCancellationRequested()
                    If _wSheetInstance.Cells(1, colCtr).value <> excelSchema(colCtr - 1) Then
                        Throw New ApplicationException(String.Format("Excel header in column {0} is not matching, existing = ""{1}"" while expected is ""{2}""", colCtr, _wSheetInstance.Cells(1, colCtr).value, excelSchema(colCtr - 1)))
                    End If
                Next
            Catch ex As Exception
                logger.Error(ex)
                Throw ex
            End Try
        End Sub
        Public Function GetLastRow()
            logger.Debug("Getting last row")
            Dim fullRows As Long = _wSheetInstance.Rows.Count
            Return _wSheetInstance.Cells(fullRows, 1).End(Excel.XlDirection.xlUp).Row
        End Function
        Public Function GetLastCol() As Long
            logger.Debug("Getting last col")
            Dim fullCols As Long = _wSheetInstance.Columns.Count
            Return _wSheetInstance.UsedRange(1, fullCols).End(Excel.XlDirection.xlToLeft).Column
        End Function
        Public Function FindAll(ByVal sText As String, ByVal sRange As String, Optional ByVal wholeMatch As Boolean = False) As List(Of Integer)
            logger.Debug("Finding all instances of a string")

            ' --------------------------------------------------------------------------------------------------------------
            ' FindAll - To find all instances of the given string and return the row numbers.
            '           If there are not any matches the function will return false
            ' --------------------------------------------------------------------------------------------------------------
            Dim ret As List(Of Integer) = Nothing
            Dim rFnd As Excel.Range                       ' Range Object
            Dim rFirstAddress                       ' Address of the First Find
            ' -----------------
            ' Clear the Array
            ' -----------------
            If wholeMatch Then
                rFnd = _wSheetInstance.Range(sRange).Find(What:=sText, LookIn:=Excel.XlFindLookIn.xlValues, LookAt:=Excel.XlLookAt.xlWhole)
            Else
                rFnd = _wSheetInstance.Range(sRange).Find(What:=sText, LookIn:=Excel.XlFindLookIn.xlValues, LookAt:=Excel.XlLookAt.xlPart)
            End If
            If Not rFnd Is Nothing Then
                rFirstAddress = rFnd.Address
                Do Until rFnd Is Nothing
                    _canceller.Token.ThrowIfCancellationRequested()
                    If ret Is Nothing Then ret = New List(Of Integer)
                    ret.Add(rFnd.Row)
                    rFnd = _wSheetInstance.Range(sRange).FindNext(rFnd)
                    If rFnd.Address = rFirstAddress Then Exit Do ' Do not allow wrapped search
                Loop
            Else
                ' ----------------------
                ' No Value is Found
                ' ----------------------
                ret = Nothing
            End If
            rFnd = Nothing
            rFirstAddress = Nothing
            Return ret
        End Function
#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Try
                        logger.Debug("Preparing to close open instances")
                        CloseOpenInstances()
                        _excelFileName = Nothing
                        _excelInstance = Nothing
                        _wBookInstance = Nothing
                        _wSheetInstance = Nothing
                        _SaveType = Nothing
                        OpeningColor = Nothing
                    Catch ex As Exception
                        logger.Debug("Supressed error")
                        logger.Error(ex)
                    End Try
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        Protected Overrides Sub Finalize()
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(False)
            MyBase.Finalize()
        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class

End Namespace
