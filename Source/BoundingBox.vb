Imports Microsoft.DirectX
Public Class BoundingBox
    '''
    ''' A bounding box is used to test collisions between collidables.
    ''' 
    ''' A bounding box is made of a 3x8 matrix, representing 8 3D points.
    ''' Each column represents a point; there are 8 columns.
    ''' Each row represents a direction; there are 3 directions (x,y,z)
    ''' 
    ''' Collisions are tested by finding the minimum and maximum values 
    ''' in the x, y, and z directions and testing for overlaps between
    ''' the ranges of two bounding boxes.
    ''' This results in the bounding boxes being axis aligned.
    '''

    Inherits MyMatrix
    Private _dimensions As Vector3 'the size of the object
    Private _rotations As Vector3 'the rotation of the bounding box
    'the bounding box is axis alligned, however, the dimensions can be rotated
    'and then the min and max values recalculated to simulate a rotation.
    Private _centre As Vector3
    Private _minValues As Vector3
    Private _maxValues As Vector3
    '''                         Properties                          '''
    Public Property Centre As Vector3
        Get
            'returns the centre point
            Return _centre
        End Get
        Set(ByVal value As Vector3)
            'sets the centre point by adding the difference between 
            'the new and the old centres and adding it to each point
            'calculate difference
            Dim difference As Vector3 = Vector3.Subtract(value, _centre)
            'add the difference to the min and max values
            _minValues = Vector3.Add(_minValues, difference)
            _maxValues = Vector3.Add(_maxValues, difference)

            'add the difference to all the points
            Dim x, y, z As Single
            For coordinates = 0 To columns - 1
                x = difference.X + GetEntry(0, coordinates)
                y = difference.Y + GetEntry(1, coordinates)
                z = difference.Z + GetEntry(2, coordinates)

                SetEntry(0, coordinates, x)
                SetEntry(1, coordinates, y)
                SetEntry(2, coordinates, z)
            Next coordinates
            _centre = value

        End Set
    End Property
    Public Property Dimensions As Vector3
        Get
            'return the size of the bounding box
            Return _dimensions
        End Get
        Set(value As Vector3)
            'resizes the bounding box
            _dimensions = value
            CalculateBounds()
        End Set
    End Property
    Public Property Rotations As Vector3
        Get
            'returns the rotation of the bounding box
            Return _rotations
        End Get
        Set(ByVal value As Vector3)
            'rotates the bounding box
            _rotations = value
            CalculateBounds()
        End Set
    End Property
    Public ReadOnly Property MinBounds As Vector3
        Get
            Return _minValues
        End Get
    End Property
    Public ReadOnly Property MaxBounds As Vector3
        Get
            Return _maxValues
        End Get
    End Property

    Public Sub New(ByVal dimensions As Vector3, ByVal centrePoint As Vector3, ByVal rotation As Vector3)
        MyBase.New(3, 8)
        'make a cube given its dimensions, centre point and rotation.
        'then calculate the min and max.
        MatrixValues = MakeCube(dimensions, centrePoint).GetMatrixValues()
        _dimensions = dimensions
        _centre = centrePoint
        _rotations = rotation

        CalculateBounds()

    End Sub
    Public Sub CalculateBounds()
        'Calculate the minimum and maximum bounds of the bounding box in each axis.
        Dim rotatedPoints As MyMatrix
        Dim rotationMatrix As MyMatrix
        'move the points to be centred around the origin rather that the bounding box's centre
        rotatedPoints = SubtractVector(Me, _centre)
        'create the 3x3 rotation matrix - represents a 3D rotation about the x, y, z axes.
        rotationMatrix = RotateGeneral(_rotations.X, _rotations.Y, _rotations.Z)

        'rotate the points by multipling the rotation matrix BY the points
        'matrix multiplication is not commutitive
        rotatedPoints = Multiply(rotationMatrix, rotatedPoints)
        'translate the rotated points to be centred about the bounding box's centre
        rotatedPoints = AddVector(rotatedPoints, _centre)
        'copy the entries
        MatrixValues = rotatedPoints.GetMatrixValues()

        'select the find the min and maximum by sorting
        'here i decided to sort so that i could potentailly average 
        'the two minimum value and the the maximum values to find a better value
        'for min and max bounds.

        Dim XVals, YVals, ZVals As List(Of Single)

        XVals = New List(Of Single)(Me.GetRow(0))
        YVals = New List(Of Single)(Me.GetRow(1))
        ZVals = New List(Of Single)(Me.GetRow(2))

        XVals = Sort.MergeSort(XVals)
        YVals = Sort.MergeSort(YVals)
        ZVals = Sort.MergeSort(ZVals)

        _minValues = New Vector3(XVals(0), YVals(0), ZVals(0))
        _maxValues = New Vector3(XVals.Last(), YVals.Last(), ZVals.Last())
    End Sub

    '''                         Collisions                          '''
    Public Shared Function CheckIntersection(ByRef boxOne As BoundingBox, ByRef boxTwo As BoundingBox) As Boolean
        Dim xIntersect As Boolean = CheckIntersectionInOneDirection(boxOne.MinBounds.X,
                                                                    boxOne.MaxBounds.X,
                                                                    boxTwo.MinBounds.X,
                                                                    boxTwo.MaxBounds.X)
        If xIntersect Then
            'collides in the x direction, check the y direction
            Dim yIntersect As Boolean = CheckIntersectionInOneDirection(boxOne.MinBounds.Y,
                                                                        boxOne.MaxBounds.Y,
                                                                        boxTwo.MinBounds.Y,
                                                                        boxTwo.MaxBounds.Y)
            If yIntersect Then
                'collides in the y direction, check the z direction

                Dim zIntersect As Boolean = CheckIntersectionInOneDirection(boxOne.MinBounds.Z,
                                                                            boxOne.MaxBounds.Z,
                                                                            boxTwo.MinBounds.Z,
                                                                            boxTwo.MaxBounds.Z)
                If zIntersect Then
                    'collides in the z direction,
                    ' collision persent
                    Return True
                End If
            End If
        End If
        'collision does not exist in atleast one direction
        Return False
    End Function
    Public Shared Function CheckIntersectionInOneDirection(ByVal minOne As Single,
                                                           ByVal maxOne As Single,
                                                           ByVal minTwo As Single,
                                                           ByVal maxTwo As Single) As Boolean
        Dim intersect As Boolean
        'checks if the minimum of object1 is inbetween object2,
        intersect = IsBetween(minTwo, maxTwo, minOne)
        'or if maximimum of object1 is inbetween object2
        intersect = intersect OrElse IsBetween(minTwo, maxTwo, maxOne)
        'or if objects2 is smaller than object 1 and is inside of object1.
        intersect = intersect OrElse IsBetween(minOne, maxOne, maxTwo)

        Return intersect

    End Function
    Public Shared Function IsBetween(ByVal min As Single, ByVal max As Single, ByVal num As Single) As Boolean
        'check if a number is inbetween another two numbers
        If min <= num AndAlso num <= max Then
            Return True
        Else
            Return False
        End If
    End Function
End Class
