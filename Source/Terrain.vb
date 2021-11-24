Imports Microsoft.DirectX
Imports Microsoft.DirectX.Direct3D
Public Class Terrain
    Inherits GameObject
    '''
    ''' A Terrain object is a game object that, by the use of a matrix, represents terrain.
    ''' This matrix should have values between 0.0 and 1.0. The terrain is scaled up by multiplying
    ''' the values by the max height. Therefore the height of the terrain can vary from 0 to the
    ''' max height of the terrain.
    '''
    Private _exponent As Integer 'this is the number of iterations of the diamond - square algorithm

    Public points As MyMatrix
    Private _numberOfTriangles As Integer

    Private _spacing As Integer
    Private _colourMap(19) As Integer
    Private _maxHeight As Integer

    '''                         Properties                          '''
    Public ReadOnly Property exponent As Integer
        Get
            Return _exponent
        End Get
    End Property
    Public ReadOnly Property MaxHeight As Integer
        Get
            Return _maxHeight
        End Get
    End Property
    Public ReadOnly Property MaxSideLength As Integer
        Get
            Return Spacing * (RowsOrColumns - 1)
        End Get
    End Property
    Public Property Spacing As Integer
        Get
            Return _spacing
        End Get
        Set(ByVal value As Integer)
            _spacing = value
        End Set
    End Property
    Public Property ColourMap As Integer()
        Get
            Return _colourMap
        End Get
        Set(ByVal value As Integer())
            _colourMap = value
        End Set
    End Property
    Public ReadOnly Property RowsOrColumns As Integer
        Get
            Return points.rows
        End Get
    End Property
    Public ReadOnly Property NumberOfVertices As Integer
        Get
            'the number of total vertecies/entries in the matrix 
            Return points.rows * points.columns
        End Get
    End Property
    Public ReadOnly Property NumberOfTriangles As Integer
        Get
            'the number of triangles used to represent the terrain is derived from the matrix
            '= 2 * 2^(n-1) * 2^(n-1) = 2^(2n-1) where n is the exponent given
            'this is equivalent to 2 * (RowsOrColumns-1)^2 
            Dim side As Integer = RowsOrColumns - 1
            Return 2 * side * side
        End Get
    End Property

    Public Sub New(ByVal exp As Integer, ByVal xzSpacing As Integer, ByVal maxY As Integer, ByRef graphicsDevice As Device)

        MyBase.New(graphicsDevice, Nothing, Nothing, defaultMaterial, New Vector3(0, 0, 0), New Vector3(0, 0, 0))

        _exponent = exp

        Dim rowsColumns As Integer = 1 + Math.Pow(2, exponent - 1) ' the number of row/columns that are formed
        _numberOfTriangles = Math.Pow(2, (2 * exponent) - 1)

        _spacing = xzSpacing
        _maxHeight = maxY
        points = New MyMatrix(rowsColumns, rowsColumns)

        'colours
        _colourMap(0) = Color.FromArgb(230, 252, 240).ToArgb()
        _colourMap(1) = Color.FromArgb(217, 248, 195).ToArgb()
        _colourMap(2) = Color.FromArgb(204, 240, 150).ToArgb()
        _colourMap(3) = Color.FromArgb(176, 235, 149).ToArgb()
        _colourMap(4) = Color.FromArgb(148, 230, 148).ToArgb()
        _colourMap(5) = Color.FromArgb(108, 200, 109).ToArgb()
        _colourMap(6) = Color.FromArgb(68, 170, 70).ToArgb()
        _colourMap(7) = Color.FromArgb(39, 155, 50).ToArgb()
        _colourMap(8) = Color.FromArgb(10, 140, 30).ToArgb()
        _colourMap(9) = Color.FromArgb(75, 105, 20).ToArgb()
        _colourMap(10) = Color.FromArgb(58, 98, 18).ToArgb()
        _colourMap(11) = Color.FromArgb(140, 70, 10).ToArgb()
        _colourMap(12) = Color.FromArgb(105, 56, 5).ToArgb()
        _colourMap(13) = Color.FromArgb(80, 51, 20).ToArgb()
        _colourMap(14) = Color.FromArgb(54, 45, 34).ToArgb()
        _colourMap(15) = Color.FromArgb(32, 28, 117).ToArgb()
        _colourMap(16) = Color.FromArgb(10, 10, 200).ToArgb()
        _colourMap(17) = Color.FromArgb(15, 15, 228).ToArgb()
        _colourMap(18) = Color.FromArgb(20, 20, 255).ToArgb()
        _colourMap(19) = Color.FromArgb(25, 25, 250).ToArgb()

    End Sub
    '''                   Terrain Height Generation                 '''
    Public Sub GenerateMatrix(ByVal jitter As Double, ByVal jitterDecay As Double)
        'generate a fractal terrain represented as a matrix
        'midpoint displacement algorithm so the matrix has to have sides of 1 + 2^n where n is an integer
        'columns represent x, rows represent z
        'the entry at a x,z represents its y value 

        Dim MaxIndex As Integer = RowsOrColumns - 1
        Randomize()
        Dim rand As New Random()
        'initialise
        'random values in the corners of the terrain
        'top left
        points.SetEntry(0, 0, rand.NextDouble())
        'top right
        points.SetEntry(0, MaxIndex, rand.NextDouble())
        'bottom left
        points.SetEntry(MaxIndex, 0, rand.NextDouble())
        'bottom right
        points.SetEntry(MaxIndex, MaxIndex, rand.NextDouble())
        'continue if the edges of the current square can be separeted
        DiamondStep(points, MaxIndex, jitter, jitterDecay)
    End Sub
    Public Sub DiamondStep(ByRef pointMatrix As MyMatrix, ByVal Stride As Integer, ByVal jitter As Double, ByVal jitterDecay As Double)
        'if matrix is further divisible, iterate through it and set the value of each "square" to make a "diamond"

        'the diamond step takes 4 corners and averages their values,
        'a small random value is added to the average
        'and then it is saved in the centre of the 4 corners
        If Stride Mod 2 = 0 Then
            Dim offset As Integer = Stride / 2

            Dim topRow, bottomRow As Integer
            Dim leftCol, rightCol As Integer

            Dim Value As Single

            For row = offset To RowsOrColumns - 1 Step Stride

                topRow = row - offset 'update row pointers
                bottomRow = row + offset

                'leftCol = -offset 'reset column pointers
                'rightCol = offset

                For col = offset To RowsOrColumns - 1 Step Stride
                    leftCol = col - offset 'update column pointers
                    rightCol = col + offset

                    Dim corners(3) As Single
                    'get values of corners
                    corners(0) = pointMatrix.GetEntry(topRow, leftCol) 'top left corner
                    corners(1) = pointMatrix.GetEntry(topRow, rightCol) 'top right corner
                    corners(2) = pointMatrix.GetEntry(bottomRow, leftCol) 'bottom left corner
                    corners(3) = pointMatrix.GetEntry(bottomRow, rightCol) 'bottom right corner

                    'set the centre of the square to the mean value of the 4 corners  
                    Value = corners.Sum() / corners.Count()
                    Value += RandomBetween(-jitter, jitter)
                    'keep values between 0 and 1
                    If Value > 1.0 Then
                        Value = 1.0 + RandomBetween(-jitter, 0)
                    ElseIf Value < 0.0 Then
                        Value = RandomBetween(0, jitter)
                    End If
                    pointMatrix.SetEntry(row, col, Value)

                Next col
            Next row
            SquareStep(pointMatrix, Stride, jitter, jitterDecay)
        End If
    End Sub
    Public Sub SquareStep(ByRef pointMatrix As MyMatrix, ByVal Stride As Integer, ByVal jitter As Double, ByVal jitterDecay As Double)

        'for every diamond, make it into smaller squares
        'this takes the splits sides of each diamond and randomly displaces the average of the nearest
        'points. this values is added in the centre of these points in the matrix.
        Dim everyOtherRow As Boolean = True
        Dim offset As Integer

        Dim smallStride As Integer = Stride / 2

        Dim indexLeft, indexRight, indexTop, indexBottom As Integer
        Dim Value As Single
        Dim side As Integer = RowsOrColumns - 1

        For row = 0 To side Step smallStride

            If everyOtherRow Then 'this ensures the column pointer starts in the correct position
                offset = Stride / 2
            Else
                offset = 0
            End If

            indexTop = row - smallStride
            indexBottom = row + smallStride

            indexTop = LimitToSide(indexTop, side)
            indexBottom = LimitToSide(indexBottom, side)

            For col = offset To side Step Stride


                indexLeft = col - smallStride
                indexRight = col + smallStride


                indexLeft = LimitToSide(indexLeft, side)
                indexRight = LimitToSide(indexRight, side)

                Dim sum As Single
                Dim count As Integer = 4


                'sum of all the vertecies around the current index
                sum = pointMatrix.GetEntry(row, indexLeft)
                sum += pointMatrix.GetEntry(row, indexRight)
                sum += pointMatrix.GetEntry(indexTop, col)
                sum += pointMatrix.GetEntry(indexBottom, col)

                'calculate the mean
                Value = sum / count
                'add a random value to the mean
                Value += RandomBetween(-jitter, jitter)
                'limit the value between 0 and 1
                If Value > 1.0 Then
                    Value = 1.0 + RandomBetween(-jitter / 10, 0)
                ElseIf Value < 0.0 Then
                    Value = 0 + RandomBetween(0.0, jitter / 10)
                End If
                pointMatrix.SetEntry(row, col, Value)


            Next col

            everyOtherRow = Not everyOtherRow
        Next row
        jitter *= Math.Pow(2, -jitterDecay)
        DiamondStep(pointMatrix, smallStride, jitter, jitterDecay)
    End Sub
    '''                        Mesh Generation                      '''
    Public Sub GenerateMesh()
        'Generate a textured mesh by using a vertex and index buffer
        'generate the vertex buffer
        Dim container As VertexBufferBitmap = GenerateVertexBufferWithTexture()
        Dim vertBuff() As CustomVertex.PositionNormalTextured = container.VertexBuffer
        Dim bitmapTexture As Bitmap = container.Texture
        'generate the index buffer
        Dim indexBuff() As Int16 = GenerateIndexBuffer16Bit()

        'make the texture and mesh managed so that they are not disposed of when the DX9 looses focus 
        myTexture = New Texture(device, bitmapTexture, Usage.None, Pool.Managed)
        myMesh = New Mesh(NumberOfTriangles, NumberOfVertices, MeshFlags.Managed, CustomVertex.PositionNormalTextured.Format, device)

        'set the vertex buffer
        myMesh.SetVertexBufferData(vertBuff, LockFlags.None)
        myMesh.UnlockVertexBuffer()
        'set the index buffer
        myMesh.SetIndexBufferData(indexBuff, LockFlags.None)
        myMesh.UnlockIndexBuffer()

        'compute the normals of the vertecies
        Dim adj(3 * NumberOfTriangles) As Integer
        myMesh.GenerateAdjacency(10, adj)
        myMesh.ComputeNormals(adj)
    End Sub
    Public Structure VertexBufferBitmap
        Dim VertexBuffer As CustomVertex.PositionNormalTextured()
        Dim Texture As Bitmap
    End Structure
    Public Function GenerateVertexBufferWithTexture() As VertexBufferBitmap
        'generate the vertex buffer and the texture to go with it
        'a vertex buffer is an array of all the points of a mesh
        ' this vertex buffer gives the position, the normal vector to the vertex,
        ' and the position of the vertex on the texture.

        Dim side As Integer = RowsOrColumns - 1
        Dim vertBuff(NumberOfVertices - 1) As CustomVertex.PositionNormalTextured
        Dim Position As New Vector3()
        Dim emptyNormalVector As New Vector3(0, 0, 0)
        Dim colour As Integer = ColourMap(19)
        Dim index As Integer = 0
        Dim textureBmp As New Bitmap(RowsOrColumns, RowsOrColumns)

        For row = 0 To side
            Position.Z = row * Spacing
            Position.X = 0
            For col = 0 To side
                'for every point in the matrix, scale the indexes by the spacing
                'to get the X and Z positions. to get the y positions, get the
                'entry of the grid and multiply it by th max height

                Position.X += Spacing
                Position.Y = points.GetEntry(row, col) * _maxHeight

                'this can loop can be optimised by preforming a binary chop search
                'set the colour of each pixel to th
                Dim percentile As Double = 1.0
                For i = 0 To 19
                    percentile -= 0.05
                    If Position.Y > _maxHeight * percentile Then
                        colour = ColourMap(i)
                        Exit For
                    End If
                Next i
                ''
                'Dim randomColor As New Vector3(2 + Rnd() * -4, 3 + Rnd() * -6, 0)
                'Dim newColor As Color = Color.FromArgb(randomColor.X + Color.FromArgb(colour).R, randomColor.Y + Color.FromArgb(colour).G, randomColor.Z + Color.FromArgb(colour).B)
                ''
                textureBmp.SetPixel(row, col, Color.FromArgb(colour))

                'U and V value are between 0 and 1.
                '(0,0) represents the top left corner of the texture,
                '(0,1) = top right corner
                '(1,0) = bottom left corner
                Dim U, V As Single
                U = row / side
                V = col / side

                vertBuff(index) = New CustomVertex.PositionNormalTextured(Position, emptyNormalVector, U, V)
                index += 1
            Next col
        Next row
        textureBmp.Save("map.bmp")
        Dim container As New VertexBufferBitmap With {
            .VertexBuffer = vertBuff,
            .Texture = textureBmp}

        Return container
    End Function
    Public Function GenerateIndexBuffer16Bit() As Int16()
        'generate the index buffer - ANTICLOCKWISE TRIANGLES
        'has to be 16 bit, as DX9 Meshes only support 16 bit indexes for meshes.
        Dim indexBuff(NumberOfTriangles * 3 - 1) As Int16
        Dim side As Integer = RowsOrColumns - 1
        Dim counter As Integer = 0
        For row = 0 To side - 1
            For col = 0 To side - 1
                'create two triangles, making a square for each iteration
                Dim indexTopLeft As Int16 = row * RowsOrColumns + col
                Dim indexTopRight As Int16 = indexTopLeft + 1
                Dim indexBottomLeft As Int16 = indexTopLeft + RowsOrColumns
                Dim indexBottomRight As Int16 = indexBottomLeft + 1
                'triangle one,
                indexBuff(counter) = indexTopLeft
                counter += 1
                indexBuff(counter) = indexBottomLeft
                counter += 1
                indexBuff(counter) = indexTopRight
                counter += 1

                'triangle Two,
                indexBuff(counter) = indexTopRight
                counter += 1
                indexBuff(counter) = indexBottomLeft
                counter += 1
                indexBuff(counter) = indexBottomRight
                counter += 1
            Next col
        Next row
        Return indexBuff
    End Function
    Public Function GenerateIndexBuffer32Bit() As Int32()
        'generate the index buffer - ANTICLOCKWISE TRIANGLES
        'DO NOT USE TO MAKE MESH OBJECT
        Dim indexBuff(NumberOfTriangles * 3 - 1) As Int32
        Dim side As Integer = RowsOrColumns - 1
        Dim counter As Integer = 0
        For row = 0 To side - 1
            For col = 0 To side - 1
                'create two triangles, making a square for each iteration
                Dim indexTopLeft As Int32 = row * RowsOrColumns + col
                Dim indexTopRight As Int32 = indexTopLeft + 1
                Dim indexBottomLeft As Int32 = indexTopLeft + RowsOrColumns
                Dim indexBottomRight As Int32 = indexBottomLeft + 1
                'triangle one,
                indexBuff(counter) = indexTopLeft
                counter += 1
                indexBuff(counter) = indexBottomLeft
                counter += 1
                indexBuff(counter) = indexTopRight
                counter += 1

                'triangle Two,
                indexBuff(counter) = indexTopRight
                counter += 1
                indexBuff(counter) = indexBottomLeft
                counter += 1
                indexBuff(counter) = indexBottomRight
                counter += 1
            Next col
        Next row
        Return indexBuff
    End Function

    Public Sub ChangeMeshVertex(ByVal row As Integer, ByVal col As Integer, ByVal pos As Vector3)
        Dim vertex As New CustomVertex.PositionNormalTextured(New Vector3(0, 0, 0), New Vector3(0, 0, 0), 0, 0)
        Dim myVerts As Array = myMesh.LockVertexBuffer(vertex.GetType(), LockFlags.None, {RowsOrColumns * RowsOrColumns})
        vertex = myVerts(row * RowsOrColumns + col)
        vertex.Position += pos
        myVerts(row * RowsOrColumns + col) = vertex
        points.SetEntry(row, col, vertex.Position.Y / _maxHeight)
        myMesh.UnlockVertexBuffer()
    End Sub
    Public Sub ComputeNormals()
        myMesh.ComputeNormals()
    End Sub
    '''                      Numberical Operations                  '''
    Public Function LimitToSide(ByVal index As Integer, ByVal sideLength As Integer) As Integer
        'limit the index to be between 0 and the sidelength
        If index < 0 Then
            index += sideLength
        ElseIf index > sideLength Then
            index -= sideLength
        End If
        Return index
    End Function
    Public Function RandomBetween(ByVal Num1 As Single, ByVal Num2 As Single) As Single
        'inclusive of both numbers
        Randomize()
        Dim range As Single
        If Num1 <= Num2 Then
            range = Num2 - Num1
            Return (Rnd() * range) + Num1
        Else
            range = Num1 - Num2
            Return (Rnd() * range) + Num2
        End If
    End Function
    Public Function GetHeightAtPoint(ByVal x As Single, ByVal z As Single) As Single
        'find the height of the terrain at a certain position
        If x < 0 OrElse z < 0 Then
            Return Single.NaN
        ElseIf x >= MaxSideLength OrElse z >= MaxSideLength Then
            Return Single.NaN
        Else
            Dim currentX As Integer = Math.Round(x / Spacing)
            Dim currentZ As Integer = Math.Round(z / Spacing)

            Dim terrainy As Single = points.GetEntry(currentZ, currentX) * MaxHeight
            Return terrainy
        End If

    End Function
End Class
