Namespace Indicator
    Public Module Pivots
        Public Enum PivotType
            Standard = 1
            Fobonacci
        End Enum
        Public Sub CalculatePivots(ByVal pivotType As PivotType,
                                   ByVal prevHigh As Decimal,
                                   ByVal prevLow As Decimal,
                                   ByVal prevClose As Decimal,
                                   ByRef outputS3 As Decimal,
                                   ByRef outputS2 As Decimal,
                                   ByRef outputS1 As Decimal,
                                   ByRef outputPivot As Decimal,
                                   ByRef outputR1 As Decimal,
                                   ByRef outputR2 As Decimal,
                                   ByRef outputR3 As Decimal
                                   )
            Select Case pivotType
                Case PivotType.Standard
                    outputPivot = (prevHigh + prevLow + prevClose) / 3
                    outputS1 = (2 * outputPivot) - prevHigh
                    outputR1 = (2 * outputPivot) - prevLow
                    outputS2 = outputPivot - (prevHigh - prevLow)
                    outputR2 = outputPivot + (prevHigh - prevLow)
                    outputS3 = outputS2 - (prevHigh - prevLow)
                    outputR3 = outputR2 + (prevHigh - prevLow)
                Case Else
                    Throw New NotImplementedException("Other than standard, no other pivot type is implemented")
            End Select
        End Sub
    End Module
End Namespace
