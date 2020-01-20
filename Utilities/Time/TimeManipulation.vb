Imports NLog
Namespace Time
    Public Module TimeManipulation
#Region "Logging and Status Progress"
        Public logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Private Attributes"
        Private INDIAN_ZONE As TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
#End Region

#Region "Public Methods"
        Public Function ConcatenateDateTime(ByVal dateToConcatenate As Date, ByVal timeToConcatenate As TimeSpan) As Date
            'logger.Debug("Concatenating date time")
            Return New Date(dateToConcatenate.Date.Year, dateToConcatenate.Date.Month, dateToConcatenate.Date.Day,
                            timeToConcatenate.Hours, timeToConcatenate.Minutes, timeToConcatenate.Seconds)
        End Function
        Public Function GetDateTimeTillMinutes(ByVal datetime1 As Date) As Date
            'logger.Debug("Converting datetime till minutes")
            Return New Date(datetime1.Date.Year, datetime1.Date.Month, datetime1.Date.Day,
                            datetime1.Hour, datetime1.Minute, 0)
        End Function
        Public Function IsDateTimeLessTillMinutes(ByVal datetime1 As Date, ByVal datetime2 As Date) As Boolean
            'logger.Debug("Checking if date time is less till minutes")
            Dim convertedDt1 As Date = New Date(datetime1.Date.Year, datetime1.Date.Month, datetime1.Date.Day,
                            datetime1.Hour, datetime1.Minute, 0)
            Dim convertedDt2 As Date = New Date(datetime2.Date.Year, datetime2.Date.Month, datetime2.Date.Day,
                            datetime2.Hour, datetime2.Minute, 0)
            Return convertedDt1 < convertedDt2
        End Function

        Public Function IsDateTimeLessEqualTillMinutes(ByVal datetime1 As Date, ByVal datetime2 As Date) As Boolean
            'logger.Debug("Checking if date time is less equal till minutes")
            Dim convertedDt1 As Date = New Date(datetime1.Date.Year, datetime1.Date.Month, datetime1.Date.Day,
                            datetime1.Hour, datetime1.Minute, 0)
            Dim convertedDt2 As Date = New Date(datetime2.Date.Year, datetime2.Date.Month, datetime2.Date.Day,
                            datetime2.Hour, datetime2.Minute, 0)
            Return convertedDt1 <= convertedDt2
        End Function
        Public Function IsDateTimeEqualTillMinutes(ByVal datetime1 As Date, ByVal datetime2 As Date) As Boolean
            'logger.Debug("Checking if date time is equal till minutes")
            Return datetime1.Date = datetime2.Date And
                    datetime1.Hour = datetime2.Hour And
                    datetime1.Minute = datetime2.Minute
        End Function
        Public Function ISTNow() As Date
            'logger.Debug("Getting current IST time as datetime")
            Return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE)
        End Function
        Public Function UnixToDateTime(ByVal unixTimeStamp As UInt64) As Date
            'logger.Debug("Converting Unix time to normal datetime")
            Dim dateTime As Date = New DateTime(1970, 1, 1, 5, 30, 0, 0, DateTimeKind.Unspecified)
            dateTime = dateTime.AddSeconds(unixTimeStamp)
            Return dateTime
        End Function
        Public Function IsTimeEqualTillSeconds(ByVal timespan1 As TimeSpan, ByVal timespan2 As TimeSpan) As Boolean
            'logger.Debug("Checking if time is equal till seconds")
            Return Math.Floor(timespan1.TotalSeconds) = Math.Floor(timespan2.TotalSeconds)
        End Function
        Public Function IsTimeEqualTillSeconds(ByVal datetime1 As Date, ByVal datetime2 As Date) As Boolean
            'logger.Debug("Checking if time is equal till seconds")
            Return Math.Floor(datetime1.TimeOfDay.TotalSeconds) = Math.Floor(datetime2.TimeOfDay.TotalSeconds)
        End Function
        Public Function IsTimeEqualTillSeconds(ByVal datetime1 As Date, ByVal timespan2 As TimeSpan) As Boolean
            'logger.Debug("Checking if time is equal till seconds")
            Return Math.Floor(datetime1.TimeOfDay.TotalSeconds) = Math.Floor(timespan2.TotalSeconds)
        End Function
        Public Function IsTimeEqualTillSeconds(ByVal timespan1 As TimeSpan, ByVal datetime2 As Date) As Boolean
            'logger.Debug("Checking if time is equal till seconds")
            Return Math.Floor(timespan1.TotalSeconds) = Math.Floor(datetime2.TimeOfDay.TotalSeconds)
        End Function
#End Region
    End Module
End Namespace