Class NumberConverter
    '''
    '''A number converter object converts numbers between bases.
    '''

    '_value is stored as a denary number and returned to the base necessary as it is called
    Private _value As String
    'max base = 36
    Private ReadOnly digitsUsed As String = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    Public Sub New(Optional ByVal ValueOfBase As String = "0", Optional ByVal Base As Integer = 10)
        'convert base to denary:
        Value(Base) = ValueOfBase
    End Sub
    Public Property Value(ByVal Base As Integer) As String
        Get
            'returns the decimal _value in the provided base
            Return ConvertBase(_value, 10, Base)
        End Get
        Set(value As String)
            'set _value to a denary number by converting the passed in value
            _value = ConvertBase(value, Base, 10)
        End Set
    End Property
    Public Function ConvertBase(ByVal val As String, ByVal BaseIn As Integer, ByVal BaseOut As Integer) As String
        'converts from base-n to denary
        Dim decimalValue As Int64
        If BaseIn <> 10 Then
            Dim reverse() As Char = val.ToCharArray()
            Array.Reverse(reverse)
            val = New String(reverse)
            val = val.ToUpper()

            'multiply the value of each digit by the corresponding power of the base.
            'summing these values will get the denary representation.
            For power As Integer = 0 To val.Length() - 1
                Dim digit As Integer = digitsUsed.IndexOf(val(power))
                If digit >= BaseIn Then
                    Throw New IndexOutOfRangeException("Input is of a higher base than the base it is being converted from.")
                End If
                decimalValue += digit * Math.Pow(BaseIn, power)
            Next power
        Else
            decimalValue = val
        End If

        Dim baseOutValue As String = ""

        'convert from denary to base-n
        If BaseOut <> 10 Then
            'find largest power of base-n that fits into the decimal value
            Dim largestPower As Integer = 0
            Do Until Math.Pow(BaseOut, largestPower + 1) > decimalValue
                largestPower += 1
            Loop
            'repeatedly subtract the largest muliple of the current power to get the current digit
            Dim largestMultiple As Integer
            While largestPower >= 0 And decimalValue <> 0

                largestMultiple = decimalValue \ Math.Pow(BaseOut, largestPower)
                decimalValue -= largestMultiple * Math.Pow(BaseOut, largestPower)

                baseOutValue += digitsUsed(largestMultiple)
                largestPower -= 1
            End While
            'if the remainder is zero but the largest power is not, repeatedly add zeros
            For i = 0 To largestPower
                baseOutValue += "0"
            Next i
        Else
            baseOutValue = decimalValue.ToString()
        End If
        ConvertBase = baseOutValue
    End Function
End Class
