Imports Microsoft.DirectX
<SerializableAttribute()>
Public Class MyMatrix
    '''
    '''A MyMatrix object represents a matrix. A matrix is a rectangular array
    '''of numbers arranged in rows and columns. DirectX9 supports matrices, but
    '''the highest order matrix is 4x4, meaning it has 4 rows and 4 columns.
    '''A matrix is used to preform calculations on many values at once.
    '''If points are stored in a matrix, the convention of X values being stored
    '''in the first row and the Z values in the last row, is assumed.
    '''
    Private _rows, _columns As Integer
    Protected MatrixValues As List(Of List(Of Single))

    '''                         Properties                          '''
    Public ReadOnly Property rows As Integer
        Get
            Return _rows
        End Get
    End Property
    Public ReadOnly Property columns As Integer
        Get
            Return _columns
        End Get
    End Property

    Public Sub New(ByVal numRows As Integer, ByVal numColumns As Integer)
        'creates a new matrix with a set number of rows and columns
        _rows = numRows
        _columns = numColumns
        MatrixValues = New List(Of List(Of Single))(rows)
        For r = 0 To rows - 1
            MatrixValues.Add(New List(Of Single)(columns))
            For c = 0 To columns - 1
                MatrixValues(r).Add(0.0)
            Next c
        Next r
    End Sub
    Public Sub MakeFromPoints(ByVal mat As MyMatrix)
        'Copy a matrix ByVal
        MatrixValues = mat.MatrixValues
        _rows = mat.rows
        _columns = mat.columns
    End Sub
    '''                         Get/Set                             '''
    Public Function GetRow(ByVal rowNum As Integer) As List(Of Single)
        'returns specified row as a list
        Return MatrixValues(rowNum)
    End Function
    Public Function GetColumn(ByVal colNum As Integer) As List(Of Single)
        'returns specified column as a list
        Dim column As New List(Of Single)(rows)
        For Each r As List(Of Single) In MatrixValues
            column.Add(r.Item(colNum))
        Next r

        Return column
    End Function
    Public Sub SetAllEntires(ByVal Value As Single)
        'sets all entries of a matrix to be the same value
        For row = 0 To rows - 1
            For cols = 0 To columns - 1
                SetEntry(row, cols, Value)

            Next cols
        Next row
    End Sub
    Public Sub SetEntry(ByVal rowNum As Integer, ByVal colNum As Integer, ByVal value As Single)
        'sets the value of an entry in the matrix
        MatrixValues(rowNum).Item(colNum) = value
    End Sub
    Public Function GetEntry(ByVal rowNum As Integer, ByVal colNum As Integer) As Single
        'retruns the value of an entry
        Return MatrixValues(rowNum).Item(colNum)
    End Function
    Public Function GetMatrixValues() As List(Of List(Of Single))
        'retruns the 2D array
        Return MatrixValues
    End Function
    '''                         Geometry                            '''
    Public Shared Function MakeCube(ByVal Dimensions As Vector3, ByVal Centre As Vector3) As MyMatrix

        'creates a 3x8 matrix (represents 8 points consisting of x,y,z values) in the following form:
        'example of unit cube centre (0.5, 0.5, 0.5):
        'A = (0,0,1)
        'B = (0,1,1)
        'C = (0,1,0)
        'D = (0,0,0)
        'E = (1,0,1)
        'F = (1,1,1)
        'G = (1,1,0)
        'H = (1,0,0)

        Dim newMatrix As New MyMatrix(3, 8)
        'this centres the cube
        Dim midVals As Vector3 = Vector3.Multiply(Dimensions, 0.5)
        Dim min As Vector3 = Vector3.Subtract(Centre, midVals)
        Dim max As Vector3 = Vector3.Add(Centre, midVals)


        '   A   B   C   D   E   F   G   H 
        'x [min min min min max max max max] <--> [0 0 0 0 1 1 1 1] --> max after 4
        'y [min max max min min max max min] <--> [0 1 1 0 0 1 1 0] --> max 2 min 2 with delay of 1
        'z [max max min min max max min min] <--> [1 1 0 0 1 1 0 0] --> max 2 min 2 
        Dim switch As Boolean = True 'this is true to make the first Y value a min but Z value a max
        Dim state As Boolean = False
        'this for loop creates the matrix above
        For point = 0 To newMatrix.columns - 1
            'the first four points, A to D, have a minimum X value
            If point < 4 Then
                newMatrix.SetEntry(0, point, min.X)
            Else
                newMatrix.SetEntry(0, point, max.X)
            End If
            'when state is on, the Y value is at a maximum, the state switches if 
            'its value has not changes since the last iteration.
            If state Then
                newMatrix.SetEntry(1, point, max.Y)
            Else
                newMatrix.SetEntry(1, point, min.Y)
            End If
            'the state is switched mid iteration to provide the delay to the Y value
            If switch Then
                state = Not state
                switch = False
            Else
                switch = True
            End If
            'when state is on, the Z value is at a maximum, the state switches if
            If state Then
                newMatrix.SetEntry(2, point, max.Z)
            Else
                newMatrix.SetEntry(2, point, min.Z)
            End If
        Next point
        Return newMatrix
    End Function
    '''                         Basic Maths                         '''
    Public Shared Function AddVector(ByVal left As MyMatrix, ByVal right As Vector3) As MyMatrix
        'add the vector to each column of the matrix
        If left.rows = 3 Then
            Dim newMatrix As New MyMatrix(left.rows, left.columns)
            For col = 0 To newMatrix.columns - 1
                'x
                newMatrix.SetEntry(0, col, left.GetEntry(0, col) + right.X)
                'y
                newMatrix.SetEntry(1, col, left.GetEntry(1, col) + right.Y)
                'z
                newMatrix.SetEntry(2, col, left.GetEntry(2, col) + right.Z)
            Next col
            Return newMatrix
        Else
            Return Nothing
        End If
    End Function
    Public Shared Function SubtractVector(ByVal left As MyMatrix, ByVal right As Vector3) As MyMatrix
        'add the negative vector to each column of the matrix
        Return AddVector(left, -right)
    End Function
    Public Shared Function Multiply(ByVal A As MyMatrix, ByVal B As MyMatrix) As MyMatrix
        'multiplies to matrices together. returns AB not BA
        'returns nothing if not compatible
        If A.columns = B.rows Then
            Dim newMatrix As New MyMatrix(A.rows, B.columns)

            For row = 0 To newMatrix.rows - 1
                For col = 0 To newMatrix.columns - 1
                    Dim sum As Single = 0
                    For index = 0 To A.columns - 1
                        sum += A.GetEntry(row, index) * B.GetEntry(index, col)
                    Next index
                    newMatrix.SetEntry(row, col, sum)

                Next col
            Next row
            Return newMatrix
        Else
            Return Nothing
        End If
    End Function
    '''                         Rotations                           '''
    Public Shared Function RotateAboutX(ByVal rad As Single) As MyMatrix
        'returns a new rotation matrix that represents a rotation about the x axis.

        Dim cos, sin As Single
        cos = Math.Cos(rad)
        sin = Math.Sin(rad)

        Dim rotationMatrix As New MyMatrix(3, 3)
        With rotationMatrix
            .MatrixValues(0)(0) = 1.0

            .MatrixValues(1)(1) = cos
            .MatrixValues(1)(2) = -sin

            .MatrixValues(2)(1) = sin
            .MatrixValues(2)(2) = cos
        End With
        Return rotationMatrix
    End Function
    Public Shared Function RotateAboutY(ByVal rad As Single) As MyMatrix
        'returns a new rotation matrix that represents a rotation about the y axis.

        Dim cos, sin As Single
        cos = Math.Cos(rad)
        sin = Math.Sin(rad)

        Dim rotationMatrix As New MyMatrix(3, 3)
        With rotationMatrix
            .MatrixValues(0)(0) = cos
            .MatrixValues(0)(2) = sin

            .MatrixValues(1)(1) = 1.0

            .MatrixValues(2)(0) = -sin
            .MatrixValues(2)(2) = cos
        End With
        Return rotationMatrix

    End Function
    Public Shared Function RotateAboutZ(ByVal rad As Single) As MyMatrix
        'returns a new rotation matrix that represents a rotation about the z axis.

        Dim cos, sin As Single
        cos = Math.Cos(rad)
        sin = Math.Sin(rad)

        Dim rotationMatrix As New MyMatrix(3, 3)
        With rotationMatrix
            .MatrixValues(0)(0) = cos
            .MatrixValues(0)(1) = -sin

            .MatrixValues(1)(0) = sin
            .MatrixValues(1)(1) = cos

            .MatrixValues(2)(2) = 1.0
        End With
        Return rotationMatrix

    End Function
    Public Shared Function RotateGeneral(ByVal x As Single, ByVal y As Single, ByVal z As Single) As MyMatrix
        'returns a new rotation matrix that represents a rotation about the x,y and z axis.

        Dim cosX, sinX As Single
        Dim cosY, sinY As Single
        Dim cosZ, sinZ As Single

        cosX = Math.Cos(x)
        sinX = Math.Sin(x)
        cosY = Math.Cos(y)
        sinY = Math.Sin(y)
        cosZ = Math.Cos(z)
        sinZ = Math.Sin(z)

        Dim rotationMatrix As New MyMatrix(3, 3)
        With rotationMatrix
            .MatrixValues(0)(0) = cosZ * cosY
            .MatrixValues(0)(1) = cosZ * sinY * sinX - sinZ * cosX
            .MatrixValues(0)(2) = cosZ * sinY * cosX + sinZ * sinX

            .MatrixValues(1)(0) = sinZ * cosY
            .MatrixValues(1)(1) = sinZ * sinY * sinX + cosZ * cosX
            .MatrixValues(1)(2) = sinZ * sinY * cosX - cosZ * sinX

            .MatrixValues(2)(0) = -sinY
            .MatrixValues(2)(1) = cosY * sinX
            .MatrixValues(2)(2) = cosY * cosX
        End With
        Return rotationMatrix

    End Function
    '''                         Convolution                         '''
    Public Shared Function Convolution(ByVal OldMatrix As MyMatrix, ByVal Kernel As MyMatrix) As MyMatrix
        'A convolution maps a set of values to another set of values. The mapping is defined 
        'by the kernel. The kernel is passed over a matrix, where overlapping values are 
        'multiplied and summed. This sum is stored in a separate matrix as is the product of
        'a convolution. A kernel is sometimes called a filter.

        'New dimensions:
        Dim newRows As Integer = OldMatrix.rows - Kernel.rows + 1
        Dim newColumns As Integer = OldMatrix.columns - Kernel.columns + 1

        Dim convMatrix As New MyMatrix(newRows, newColumns)

        For row = 0 To convMatrix.rows - 1
            For column = 0 To convMatrix.columns - 1
                'for every entry in the new matrix
                Dim sum As Single = 0
                For kernelRow = 0 To Kernel.rows - 1
                    For kernelColumns = 0 To Kernel.columns - 1
                        'sum the products of the overlapping kernel and input values 
                        Dim oldValue As Single = OldMatrix.GetEntry(kernelRow + row, kernelColumns + column)
                        Dim kernelValue As Single = Kernel.GetEntry(kernelRow, kernelColumns)

                        sum += oldValue * kernelValue

                    Next kernelColumns
                Next kernelRow
                'set the entry to be equal to the sum.
                convMatrix.SetEntry(row, column, sum)

            Next column
        Next row

        Return convMatrix
    End Function
    '''                         Conversions                         '''
    Public Shared Function MyMatrixToVector3(ByVal matrix3by1 As MyMatrix) As Vector3
        'convert a 3x1 matrix to a vector
        If matrix3by1.rows = 3 AndAlso matrix3by1.columns = 1 Then
            Dim vec As New Vector3 With {
                    .X = matrix3by1.GetEntry(0, 0),
                    .Y = matrix3by1.GetEntry(1, 0),
                    .Z = matrix3by1.GetEntry(2, 0)
                }
            Return vec
        Else
            Throw New Exception("This matrix is not compatible with this operation.")
        End If

    End Function
    Public Shared Function MyMatrixFromVector3(ByVal vector As Vector3) As MyMatrix
        'convert a vector to a 3x1 matrix.
        Dim mat As New MyMatrix(3, 1)
        mat.SetEntry(0, 0, vector.X)
        mat.SetEntry(1, 0, vector.Y)
        mat.SetEntry(2, 0, vector.Z)
        Return mat
    End Function
End Class
