Imports System.Text.RegularExpressions
Imports NLog
Imports System.Net.WebUtility
Imports System.Text
Imports System.ComponentModel
Imports System.Globalization
Imports System.Security.Cryptography
Imports System.Web.Script.Serialization
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary

Namespace Strings
    Public Module StringManipulation
#Region "Logging and Status Progress"
        Public logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Enums"
        Public Enum StringMatchoptions
            MatchFullWord
            MatchPartialWord
        End Enum
        Public Enum HyperLinkType
            Internal = 1
            External
            Both
        End Enum
#End Region

#Region "Public Methods"
        Public Function JsonSerialize(x As Object) As String
            'logger.Debug("Serializing to JSON")
            Dim jss = New JavaScriptSerializer()
            Return jss.Serialize(x)
        End Function
        Public Function JsonDeserialize(Json As String) As Dictionary(Of String, Object)
            'logger.Debug("Deserializing from JSON")
            Dim jss = New JavaScriptSerializer()
            jss.MaxJsonLength = Integer.MaxValue
            Dim dict As Dictionary(Of String, Object) = jss.Deserialize(Of Dictionary(Of String, Object))(Json)
            Return dict
        End Function
        Public Function SHA256(Data As String) As String
            logger.Debug("Converting data to SHA256")
            Dim sha256__1 As New SHA256Managed()
            Dim hexhash As New StringBuilder()
            Dim hash As Byte() = sha256__1.ComputeHash(Encoding.UTF8.GetBytes(Data), 0, Encoding.UTF8.GetByteCount(Data))
            For Each b As Byte In hash
                hexhash.Append(b.ToString("x2"))
            Next
            Return hexhash.ToString()
        End Function
        Public Function GetTextBetween(ByVal firstText As String, ByVal secondText As String, ByVal searchFromText As String) As String
            logger.Debug("Getting text in between")
            Dim regex As New Regex(firstText & "(.*?)" & secondText, RegexOptions.Singleline) 'SingleLine options makes it return multilined text
            Dim match As Match = regex.Match(searchFromText)
            Return match.Groups(1).Value
        End Function
        Public Function StripTags(ByVal html As String) As String
            logger.Debug("Stripping HTML tags")
            Return Regex.Replace(html, "<.*?>", "")
        End Function
        Public Function RemoveBeginningAndEndingBlanks(ByVal inputStr As String) As String
            logger.Debug("Removing beginning and ending blanks")
            Return Regex.Replace(inputStr, "^\s+$[\r\n]*", "", RegexOptions.Multiline)
        End Function
        Public Function ConvertHTMLToReadableText(ByVal inputHTML As String) As String
            logger.Debug("Converting HTMl to readable text")
            Dim result As String = inputHTML

            'First replace the anchor tags with the just the URL
            Dim doc As New HtmlAgilityPack.HtmlDocument
            doc.LoadHtml(result)
            Dim links = doc.DocumentNode.Descendants("a")
            If links IsNot Nothing AndAlso links.Count > 0 Then

                For linkCtr As Integer = links.Count - 1 To 0 Step -1
                    Dim anchorLink = links(linkCtr)
                    'Dim url As String = String.Empty
                    'If anchorLink.Attributes("href") IsNot Nothing AndAlso anchorLink.Attributes("href").Value IsNot Nothing Then
                    '    url = anchorLink.Attributes("href").Value
                    'End If
                    Dim anchorText As String = String.Empty
                    If anchorLink.InnerHtml IsNot Nothing Then
                        anchorText = anchorLink.InnerHtml
                    End If
                    Dim newNode As HtmlAgilityPack.HtmlNode = doc.CreateElement("span")
                    'newNode.InnerHtml = url
                    newNode.InnerHtml = anchorText

                    anchorLink.ParentNode.InsertBefore(newNode, anchorLink)
                    anchorLink.Remove()
                Next
            End If

            result = doc.DocumentNode.InnerHtml


            ' Remove HTML Development formatting
            result = result.Replace(vbCr, "#NEW LINE#")
            ' Replace line breaks with space because browsers inserts space
            result = result.Replace(vbLf, "#NEW LINE#")
            ' Replace line breaks with space because browsers inserts space
            result = result.Replace(vbTab, String.Empty)
            ' Remove step-formatting
            result = System.Text.RegularExpressions.Regex.Replace(result, "( )+", " ")
            ' Remove repeating speces becuase browsers ignore them
            ' Remove the header (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*head([^>])*>", "<head>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "(<( )*(/)( )*head( )*>)", "</head>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "(<head>).*(</head>)", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

            ' remove all scripts (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*script([^>])*>", "<script>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "(<( )*(/)( )*script( )*>)", "</script>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            'result = System.Text.RegularExpressions.Regex.Replace(result, @"(<script>)([^(<script>\.</script>)])*(</script>)",string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            result = System.Text.RegularExpressions.Regex.Replace(result, "(<script>).*(</script>)", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

            ' remove all styles (prepare first by clearing attributes)
            result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*style([^>])*>", "<style>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "(<( )*(/)( )*style( )*>)", "</style>", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "(<style>).*(</style>)", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)


            ' insert tabs in spaces of <td> tags
            result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*td([^>])*>", vbTab, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

            ' insert line breaks in places of <BR> and <LI> tags
            result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*br( )*>", vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*li( )*>", vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

            ' insert line paragraphs (double line breaks) in place if <P>, <DIV> and <TR> tags
            result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*div([^>])*>", vbCr & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*tr([^>])*>", vbCr & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "<( )*p([^>])*>", vbCr & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

            ' Remove remaining tags like <a>, links, images, comments etc - anything thats enclosed inside < >
            'result = System.Text.RegularExpressions.Regex.Replace(result, "<[^>]*>", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            ' Remove except for <a> all other remaining tags like <a>, links, images, comments etc - anything thats enclosed inside < >
            result = System.Text.RegularExpressions.Regex.Replace(result, "<(?!\/?a(?=>|\s.*>))\/?.*?>", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

            ' replace special characters:
            result = System.Text.RegularExpressions.Regex.Replace(result, "&nbsp;", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "&quot;", """", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "&bull;", " * ", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "&lsaquo;", "<", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "&rsaquo;", ">", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "&trade;", "(tm)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "&frasl;", "/", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "&lt;", "<", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "&gt;", ">", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "&copy;", "(c)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "&reg;", "(r)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            ' Remove all others. More can be added, see http://hotwired.lycos.com/webmonkey/reference/special_characters/
            result = System.Text.RegularExpressions.Regex.Replace(result, "&(.{2,6});", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase)

            ' for testng
            'System.Text.RegularExpressions.Regex.Replace(result, this.txtRegex.Text,string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            ' make line breaking consistent
            result = result.Replace(vbLf, vbCr)

            ' Remove extra line breaks and tabs: replace over 2 breaks with 2 and over 4 tabs with 4. 
            ' Prepare first to remove any whitespaces inbetween the escaped characters and remove redundant tabs inbetween linebreaks
            result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbCr & ")( )+(" & vbCr & ")", vbCr & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbTab & ")( )+(" & vbTab & ")", vbTab & vbTab, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbTab & ")( )+(" & vbCr & ")", vbTab & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbCr & ")( )+(" & vbTab & ")", vbCr & vbTab, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbCr & ")(" & vbTab & ")+(" & vbCr & ")", vbCr & vbCr, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            ' Remove redundant tabs
            result = System.Text.RegularExpressions.Regex.Replace(result, "(" & vbCr & ")(" & vbTab & ")+", vbCr & vbTab, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            ' Remove multible tabs followind a linebreak with just one tab
            Dim breaks As String = vbCr & vbCr & vbCr
            ' Initial replacement target string for linebreaks
            Dim tabs As String = vbTab & vbTab & vbTab & vbTab & vbTab
            ' Initial replacement target string for tabs
            For index As Integer = 0 To result.Length - 1
                result = result.Replace(breaks, vbCr & vbCr)
                result = result.Replace(tabs, vbTab & vbTab & vbTab & vbTab)
                breaks = breaks + vbCr
                tabs = tabs + vbTab
            Next

            ' Thats it.
            result = Replace(result, "#NEW LINE#", vbCr)
            result = Replace(result, vbCr, vbCrLf)
            Return RemoveBeginningAndEndingBlanks(result)
        End Function
        Public Function EncodeString(ByVal inputString As String)
            logger.Debug("Encoding string")
            Return HtmlEncode(inputString)
        End Function
        Public Function DecodeString(ByVal inputString As String)
            logger.Debug("Decoding string")
            Return HtmlDecode(inputString)
        End Function
        Public Function ContainsText(ByVal inputString As String, ByVal searchString As String, ByVal options As StringMatchoptions) As Boolean
            logger.Debug("Checking contains text")
            Dim ret = False
            Select Case options
                Case StringMatchoptions.MatchFullWord
                    If Regex.Match(inputString, String.Format("(?<![-/])\b{0}\b(?!-)", searchString), RegexOptions.IgnoreCase).Success Then
                        ret = True
                    Else
                        ret = False
                    End If
                Case StringMatchoptions.MatchPartialWord
                    If Regex.Match(inputString, searchString, RegexOptions.IgnoreCase).Success Then
                        ret = True
                    Else
                        ret = False
                    End If
            End Select
            Return ret
        End Function
        Public Function GetWordCount(ByVal inputString As String) As Long
            logger.Debug("Getting word count")
            Dim ret As Integer = 0
            If inputString IsNot Nothing AndAlso inputString.Trim.Length > 0 Then
                inputString = inputString.Trim
                ret = Regex.Matches(inputString, "\w+").Count
            End If
            Return ret
        End Function
        Public Function GetWordCount(ByVal inputString As String, ByVal searchString As String, ByVal options As StringMatchoptions) As Long
            logger.Debug("Getting word count")
            Dim ret As Integer = 0
            Select Case options
                Case StringMatchoptions.MatchFullWord
                    ret = Regex.Matches(inputString, String.Format("(?<![-/])\b{0}\b(?!-)", searchString), RegexOptions.IgnoreCase).Count
                Case StringMatchoptions.MatchPartialWord
                    ret = Regex.Matches(inputString, searchString, RegexOptions.IgnoreCase).Count
            End Select
            Return ret
        End Function
        Public Function GetWords(ByVal inputString As String) As String()
            logger.Debug("Getting words")
            Dim ret() As String = Nothing
            If inputString IsNot Nothing AndAlso inputString.Trim.Length > 0 Then
                inputString = inputString.Trim
                Dim matches As MatchCollection = Regex.Matches(inputString, "\w+")
                ret = matches.Cast(Of Match)().[Select](Function(m)
                                                            Return m.Value
                                                        End Function).ToArray()
            End If
            Return ret
        End Function
        Public Function GetWordByWordNumber(ByVal wordNumber As Integer, ByVal numBerOfWordsToExtract As Integer, ByVal inputString As String) As String
            logger.Debug("Getting word by word number")
            Dim ret As String = String.Empty
            If inputString IsNot Nothing AndAlso inputString.Trim.Length > 0 Then
                inputString = inputString.Trim
                Dim matches As MatchCollection = Regex.Matches(inputString, "\w+")
                Dim runningWordCounter As Integer = 1
                Dim startIndex As Integer = 0
                For Each matchedWord As Match In matches
                    If runningWordCounter = wordNumber Then
                        'Get the start index of this word
                        Dim indexDetails As Capture = matchedWord.Captures(0)
                        startIndex = indexDetails.Index + 1 'Since comparer return zero index for a string matching in the first word
                        Exit For
                    End If
                    runningWordCounter += 1
                Next
                If startIndex > 0 Then
                    Dim modifiedSourceString As String = Mid(inputString, startIndex).Trim
                    'Now from this string we have to match numBerOfWordsToExtract
                    matches = Nothing
                    matches = Regex.Matches(modifiedSourceString, "\w+")
                    runningWordCounter = 1
                    Dim endIndex As Integer = 0
                    For Each matchedWord As Match In matches
                        If runningWordCounter = numBerOfWordsToExtract Then
                            'Get the start index of this word
                            Dim indexDetails As Capture = matchedWord.Captures(0)
                            If indexDetails IsNot Nothing Then
                                endIndex = indexDetails.Index + indexDetails.Length 'I have not added a 1 like before because here we are talking of endindex where I hadded the length
                                Exit For
                            Else
                                'Now match somehow so better to exit and return null
                                Exit For
                            End If
                        End If
                        runningWordCounter += 1
                    Next
                    If endIndex > 0 Then
                        ret = Left(modifiedSourceString, endIndex)
                    End If
                End If
                matches = Nothing
            End If
            Return ret
        End Function
        Public Function GetCleanedHTML(ByVal inputStr As String) As String
            logger.Debug("Getting cleaned HTML")
            Dim ret As String = inputStr
            'Remove the blank lines
            ret = Regex.Replace(ret, "^\s*$[\r\n]*", String.Empty, RegexOptions.Multiline)
            'value = Regex.Replace(x, "^\r?\n?$", String.Empty, RegexOptions.Multiline)
            'value = Regex.Replace(x, "^\s*$\n", String.Empty, RegexOptions.Multiline)
            'value = Regex.Replace(x, "^\s*$\r", String.Empty, RegexOptions.Multiline)
            'value = Regex.Replace(x, "(\r)?(^\s*$)+", String.Empty, RegexOptions.Multiline)
            'value = Regex.Replace(x, "(\n)?(^\s*$)+", String.Empty, RegexOptions.Multiline)
            'Remove more than one white spaces
            ret = Regex.Replace(ret, "[ ]{2,}", " ", RegexOptions.None)


            ret = ret.Trim
            'resultString = Regex.Replace(subjectString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
            'string fix = Regex.Replace(original, @"^\s*$\n", string.Empty, RegexOptions.Multiline);
            Return ret
        End Function
        Public Function GetStringSimilarityPercentage(ByVal firstString As String, ByVal secondString As String) As Double
            logger.Debug("Getting string similarity percentage")
            If firstString Is Nothing Then firstString = ""
            If secondString Is Nothing Then secondString = ""
            Dim numMatch As Integer = 0
            Dim numNotMatch As Integer = 0
            Dim numCharLargestString As Integer = 0
            Dim strFirstLength As Integer
            Dim strSecondLength As Integer
            Dim counter As Integer
            Dim percentage As Double
            Dim LoopControl As Integer
            strFirstLength = firstString.Length()
            strSecondLength = secondString.Length()
            If strFirstLength > strSecondLength Then
                LoopControl = strSecondLength - 1
                numCharLargestString = strFirstLength
            Else
                LoopControl = strFirstLength - 1
                numCharLargestString = strSecondLength
            End If
            For counter = 0 To LoopControl
                If firstString(counter).CompareTo(secondString(counter)) = 0 Then
                    numMatch += 1
                Else
                    numNotMatch += 1
                End If
            Next
            percentage = numMatch * 100 / numCharLargestString
            Return percentage
        End Function
        Public Function GetHyperLinks(ByVal inputText As String, ByVal linkType As HyperLinkType, Optional ByVal baseDomain As String = Nothing) As List(Of String)
            logger.Debug("Getting hyperlinks")
            If baseDomain Is Nothing Then linkType = HyperLinkType.Both
            Dim ret As New List(Of String)
            'Get the links
            Dim regger As New Regex("((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[\w]*))?)", RegexOptions.Multiline)
            For Each mt As Match In regger.Matches(inputText)
                Select Case linkType
                    Case HyperLinkType.External
                        If Not mt.Value.ToLower.Contains(baseDomain.ToLower) Then
                            ret.Add(mt.Value)
                        End If
                    Case HyperLinkType.Internal
                        If mt.Value.ToLower.Contains(baseDomain.ToLower) Then
                            ret.Add(mt.Value)
                        End If
                    Case HyperLinkType.Both
                        ret.Add(mt.Value)
                End Select
            Next
            Return ret
        End Function
        Public Function GetEnumValue(ByVal enumType As Type, ByVal enumText As String) As Integer
            logger.Debug("Getting enum value")
            Return [Enum].Parse(enumType, enumText)
        End Function
        Public Function GetParsedDateValueFromString(ByVal parsedString As String, ByVal format As String) As Date
            logger.Debug("Getting parsed date value from string")
            Dim ret As Date = Date.MinValue
            If parsedString IsNot Nothing AndAlso IsDate(parsedString) Then
                ret = Date.ParseExact(parsedString, format, CultureInfo.InvariantCulture)
            End If
            Return ret
        End Function
        Public Function GetParsedDoubleValueFromString(ByVal parsedString As String) As Double
            logger.Debug("Getting parsed double value from string")
            Dim ret As Double = Double.MinValue
            parsedString = Replace(parsedString, ",", "")
            If parsedString IsNot Nothing AndAlso IsNumeric(parsedString) Then
                ret = CDbl(parsedString)
            End If
            Return ret
        End Function
        Public Function GetParsedLongValueFromString(ByVal parsedString As String) As Long
            logger.Debug("Getting parsed long value from string")
            Dim ret As Long = Long.MinValue
            parsedString = Replace(parsedString, ",", "")
            If parsedString IsNot Nothing AndAlso IsNumeric(parsedString) Then
                ret = CLng(parsedString)
            End If
            Return ret
        End Function
        Public Function StringToDate(ByVal DateString As String) As DateTime?
            logger.Debug("Convert string to date")
            Try
                If DateString.Length = 10 Then
                    Return DateTime.ParseExact(DateString, "yyyy-MM-dd", Nothing)
                Else
                    Return DateTime.ParseExact(DateString, "yyyy-MM-dd HH:mm:ss", Nothing)
                End If
            Catch e As Exception
                logger.Debug("Supressed exception")
                logger.Error(e)
                Return Nothing
            End Try
        End Function
        Public Function GetEnumDescription(ByVal EnumConstant As [Enum]) As String
            logger.Debug("Getting enum description")
            Dim attr() As DescriptionAttribute = DirectCast(EnumConstant.GetType().GetField(EnumConstant.ToString()).GetCustomAttributes(GetType(DescriptionAttribute), False), DescriptionAttribute())
            Return If(attr.Length > 0, attr(0).Description, EnumConstant.ToString)
        End Function

        Public Sub SerializeFromCollection(Of T)(ByVal outputFilePath As String, ByVal collectionToBeSerialized As T)
            'logger.Debug("Serialize from collection")
            'serialize
            Using stream As Stream = File.Open(outputFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)
                Dim bformatter = New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                bformatter.Serialize(stream, collectionToBeSerialized)
                stream.Close()
            End Using
        End Sub
        Public Function DeserializeToCollection(Of T)(ByVal inputFilePath As String) As T
            'logger.Debug("Deserialize to collection")
            Using stream As Stream = File.Open(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Dim binaryFormatter = New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                Return DirectCast(binaryFormatter.Deserialize(stream), T)
            End Using
        End Function
        Public Function Encrypt(ByVal stringToEncrypt As String, ByVal key As String) As String
            'logger.Debug("Encrytping a string")
            Dim DES As New TripleDESCryptoServiceProvider
            Dim MD5 As New MD5CryptoServiceProvider
            DES.Key = MD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key))
            DES.Mode = CipherMode.ECB
            Dim Buffer As Byte() = ASCIIEncoding.ASCII.GetBytes(stringToEncrypt)
            Return Convert.ToBase64String(DES.CreateEncryptor().TransformFinalBlock(Buffer, 0, Buffer.Length))
        End Function
        Public Function Decrypt(ByVal encryptedString As String, ByVal key As String) As String
            'logger.Debug("Decrytping a string")
            Dim ret As String = Nothing
            Try
                Dim DES As New TripleDESCryptoServiceProvider
                Dim MD5 As New MD5CryptoServiceProvider
                DES.Key = MD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key))
                DES.Mode = CipherMode.ECB
                Dim Buffer As Byte() = Convert.FromBase64String(encryptedString)
                ret = ASCIIEncoding.ASCII.GetString(DES.CreateDecryptor().TransformFinalBlock(Buffer, 0, Buffer.Length))
            Catch ex As Exception
                logger.Error(ex)
                'MsgBox("Invalid-Decryption Failed")
            End Try
            Return ret
        End Function
        Public Function DeepClone(Of T)(ByRef orig As T) As T

            ' Don't serialize a null object, simply return the default for that object
            If (Object.ReferenceEquals(orig, Nothing)) Then Return Nothing

            Dim formatter As New BinaryFormatter()
            Dim stream As New MemoryStream()

            formatter.Serialize(stream, orig)
            stream.Seek(0, SeekOrigin.Begin)

            Return CType(formatter.Deserialize(stream), T)
        End Function
    End Module
#End Region
End Namespace