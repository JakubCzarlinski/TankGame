Imports Microsoft.DirectX
Imports Microsoft.DirectX.Direct3D
Public Class Collidable
    Inherits GameObject

    '''
    ''' A collidable object is a game object with a bounding box that checks for collisions. 
    ''' Collisions can be tested against a single collidable or and array of them.
    ''' When testing against an array, the function will return all of the collisions present.
    '''

    Protected _myBoundingBox As BoundingBox
    '''                         Properties                          '''
    Public ReadOnly Property myBoundingBox As BoundingBox
        Get
            Return _myBoundingBox
        End Get
    End Property
    Public Overrides Property position As Vector3
        'Changing the position also changes the position of the bounding box.
        Get
            Return MyBase.position
        End Get
        Set(value As Vector3)
            MyBase.position = value
            myBoundingBox.Centre = _position
        End Set
    End Property
    Public Overrides Property rotation As Vector3

        'Changing the rotation also changes the rotation of the bounding box.
        Get
            Return MyBase.rotation
        End Get
        Set(value As Vector3)
            MyBase.rotation = value
            myBoundingBox.Rotations = _rotation

        End Set
    End Property

    Public Sub New(ByRef renderTo As Device,
                   ByRef objectMesh As Mesh,
                   ByRef objectTexture As Texture,
                   ByVal objectMaterial As Material,
                   ByVal objectPosition As Vector3,
                   ByVal objectRotation As Vector3,
                   ByVal dimensions As Vector3)
        MyBase.New(renderTo, objectMesh, objectTexture, objectMaterial, objectPosition, objectRotation)

        'Initialising a collidable also initialises a bounding box.
        _myBoundingBox = New BoundingBox(dimensions, position, _rotation)
    End Sub

    '''                         Collisions                          '''
    Public Shared Function CheckCollision(ByRef objOne As Collidable, ByRef objTwo As Collidable) As Boolean
        'checks the state of collisions between two game objects
        Return BoundingBox.CheckIntersection(objOne.myBoundingBox, objTwo.myBoundingBox)
    End Function
    Public Shared Function CollisionStatus(ByRef gameObj As Collidable, ByRef collectionObjs() As Collidable) As Collidable()
        'creates and array of game objects that gameObj is colliding with from the list of game objects
        Dim collidesWith As New List(Of Collidable)()

        For Each obj As Collidable In collectionObjs

            If (Not IsNothing(obj)) AndAlso CheckCollision(gameObj, obj) Then
                collidesWith.Add(obj)
            End If
        Next obj
        'removes any null objects
        collidesWith.TrimExcess()

        Return collidesWith.ToArray()
    End Function
End Class
