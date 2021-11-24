Imports Microsoft.DirectX
Imports Microsoft.DirectX.Direct3D
Public Class Tank
    Inherits Collidable
    '''
    '''A Tank is a collidable object with 4 additional bounding boxes. It also 
    '''has a camera object, which allows directX to render the surroundings from
    '''the tank's perspective. A tank can also shoot a bullet.
    '''
    Public myCamera As Camera

    Private frontBoundingBox As BoundingBox
    Private rearBoundingBox As BoundingBox
    Private leftBoundingBox As BoundingBox
    Private rightBoundingBox As BoundingBox

    Public pitch As Single = 0
    Private _speed As Integer
    '''                          Properties                         '''
    Public Property Speed As Integer
        Get
            Return _speed
        End Get
        Set(value As Integer)
            _speed = value
        End Set
    End Property
    Public Overrides Property position As Vector3
        Get
            Return MyBase.position
        End Get
        Set(value As Vector3)
            'move the tank by moving the collidable, camera and
            'the side bounding boxes.
            Dim difference As Vector3 = value - MyBase.position
            myCamera.SetWorldPosition(value)
            MyBase.position = value


            frontBoundingBox.Centre += difference
            rearBoundingBox.Centre += difference
            leftBoundingBox.Centre += difference
            rightBoundingBox.Centre += difference
        End Set
    End Property
    Public Overrides Property rotation As Vector3
        Get
            Return MyBase.rotation
        End Get
        Set(value As Vector3)
            Dim difference As Single = value.Y - MyBase.rotation.Y
            MyBase.rotation = New Vector3(0, value.Y, 0)

            RotateBoundingBoxes(difference)
            'restrict the pitch to be between Pi/10 and Pi/3 radians.
            pitch -= value.Z
            If pitch < Math.PI / 10 Then
                pitch = Math.PI / 10
            End If
            If pitch > Math.PI / 3 Then
                pitch = Math.PI / 3
            End If
            'rotate the camera about a point 30 units above the tank.
            Dim centreOfRotation As Vector3 = position + New Vector3(0, 30, 0)
            myCamera.OrbitObject(MyBase.rotation.Y, value.Z, 100, centreOfRotation)
        End Set
    End Property

    Public Sub New(ByRef renderTo As Device,
                   ByRef objectMesh As Mesh,
                   ByRef objectTexture As Texture,
                   ByVal objectMaterial As Material,
                   ByVal objectPosition As Vector3,
                   ByVal objectRotation As Vector3,
                   ByVal dimensions As Vector3)
        MyBase.New(renderTo,
                   objectMesh,
                   objectTexture,
                   objectMaterial,
                   objectPosition,
                   objectRotation,
                   dimensions)

        myCamera = New Camera()
        pitch = Math.PI / 3
        _speed = 125
        GenerateSideBoundingBoxes()
    End Sub

    '''                           Movement                          '''
    Public Sub Move(ByVal frameTime As Double, ByVal forward As Boolean)
        'this moves the tank in the direction that it is pointing to.
        'the assumption is made the frametime averages to a constant
        'this allows the tank to move the same distance over a longer time 
        'period despite possible irregularities in the frame time.
        Dim velocity As New MyMatrix(3, 1)
        velocity.SetEntry(0, 0, _speed * frameTime)
        'rotate the velocity in the direction that the rank is facing
        Dim tankRotation As MyMatrix = MyMatrix.RotateAboutY(rotation.Y)
        velocity = MyMatrix.Multiply(tankRotation, velocity)
        Dim velocityVector As Vector3 = MyMatrix.MyMatrixToVector3(velocity)

        If Not forward Then
            velocityVector *= -1
        End If
        position += velocityVector
    End Sub
    '''                            Shoot                            '''
    Public Function Shoot(ByRef BulletMesh As Mesh,
                          ByRef BulletTexture As Texture,
                          ByVal BulletMaterial As Material,
                          ByVal RelativeSpeed As Single) As Projectile
        'Shooting return a projectile with a velocity dependant on the direction of the tank.

        'determine the normailised direction of the bullet by rotating the vector (1,0,0)
        'by the rotations in the Y and Z axes. Y = Yaw, Z = Pitch
        Dim rotationMatrixY As MyMatrix = MyMatrix.RotateAboutY(rotation.Y)
        Dim rotationMatrixZ As MyMatrix = MyMatrix.RotateAboutZ(pitch - Math.PI / 6)
        Dim rotationMatrix As MyMatrix = MyMatrix.Multiply(rotationMatrixY,
                                                           rotationMatrixZ)

        Dim normalVector As New MyMatrix(3, 1)
        normalVector.SetEntry(0, 0, 1) 'this is the vector (1,0,0)

        Dim rotatedVector As MyMatrix = MyMatrix.Multiply(rotationMatrix, normalVector)
        Dim normailisedDirection As Vector3 = MyMatrix.MyMatrixToVector3(rotatedVector)

        'Define where the bullet begins - this should be the end of the muzzle.
        'the muzzle is at the postition vector of (14,7,0) relative to the centre of the tank
        'thererfore rotate (14,7,0) about the centre of the tank.
        Dim tankRiflePos As New MyMatrix(3, 1)
        tankRiflePos.SetEntry(0, 0, 14)
        tankRiflePos.SetEntry(1, 0, 7)

        rotatedVector = MyMatrix.Multiply(rotationMatrixY, tankRiflePos)
        Dim startPos As Vector3 = MyMatrix.MyMatrixToVector3(rotatedVector)

        'create the bullet and return it
        Dim bullet As New Projectile(device,
                            BulletMesh,
                            BulletTexture,
                            DirectXModule.defaultMaterial,
                            position + startPos,
                            New Vector3(0, 0, 0),
                            New Vector3(10, 10, 10),
                            normailisedDirection,
                            RelativeSpeed + Speed)
        Return bullet
    End Function

    '''                          Collisions                         '''
    Public Sub GenerateSideBoundingBoxes()
        'creates 4 bounding boxes which determine where the collision is coming from
        With myBoundingBox.Dimensions
            'determine the long and short side of the 4 bounding boxes 
            Dim xLong As Single = .X / 2
            Dim xShort As Single = 3 * .X * 0.25

            Dim y As Single = .Y

            Dim zLong As Single = .Z * 0.35
            Dim zShort As Single = .Z

            'put the dimensions of the bounding boxes in a vector
            Dim dimensionLeftRight As New Vector3(xShort, y, zLong)
            Dim dimensionFrontRear As New Vector3(xLong, y, zShort)


            'determine where to centre boxes
            Dim xOffset As Single = (.X - zShort / 2) / 2
            Dim zOffset As Single = (.Z - xShort / 2) / 2

            Dim noRotation As New Vector3(0, 0, 0)
            With myBoundingBox.Centre
                'determines the centre of the bounding boxes
                'these are offset from the main bounding box's centre
                Dim leftPosition As Vector3 = New Vector3(myBoundingBox.MinBounds.X, .Y, .Z)
                Dim rightPosition As Vector3 = New Vector3(myBoundingBox.MaxBounds.X, .Y, .Z)
                Dim frontPosition As Vector3 = New Vector3(.X, .Y, myBoundingBox.MaxBounds.Z)
                Dim rearPosition As Vector3 = New Vector3(.X, .Y, myBoundingBox.MinBounds.Z)

                leftBoundingBox = New BoundingBox(dimensionLeftRight, leftPosition, noRotation)
                rightBoundingBox = New BoundingBox(dimensionLeftRight, rightPosition, noRotation)
                frontBoundingBox = New BoundingBox(dimensionFrontRear, frontPosition, noRotation)
                rearBoundingBox = New BoundingBox(dimensionFrontRear, rearPosition, noRotation)
            End With
        End With

    End Sub
    Public Sub RotateBoundingBoxes(ByVal deltaAngle As Single)
        'rotate the bounding boxes about the centre of the tank
        'AND rotate the centres of the bounding boxes around the main tank bounding box.
        If deltaAngle <> 0 Then
            Dim rotationMatrix As MyMatrix = MyMatrix.RotateAboutY(deltaAngle)
            Dim sideBoundingBoxes() As BoundingBox = {frontBoundingBox, rearBoundingBox, leftBoundingBox, rightBoundingBox}
            For Each box As BoundingBox In sideBoundingBoxes
                'move all points of the bounding box to be centred about the origin
                'rotate the point and move them back
                '-- this rotates the point about THEIR centre --
                Dim boxPoints As MyMatrix = MyMatrix.SubtractVector(box, position)
                boxPoints = MyMatrix.Multiply(rotationMatrix, boxPoints)
                boxPoints = MyMatrix.AddVector(boxPoints, position)

                'now rotate the centre of the points above about the centre of the TANK
                Dim offset As MyMatrix = MyMatrix.MyMatrixFromVector3(box.Centre - myBoundingBox.Centre)
                offset = MyMatrix.Multiply(rotationMatrix, offset)
                offset = MyMatrix.AddVector(offset, myBoundingBox.Centre)

                'update the points and move them to the new centre
                box.MakeFromPoints(boxPoints)
                box.Centre = MyMatrix.MyMatrixToVector3(offset)
            Next box
        End If
    End Sub
    Public Function LeftCollisionStatus(ByRef colliableObj As Collidable) As Boolean
        Return BoundingBox.CheckIntersection(leftBoundingBox, colliableObj.myBoundingBox)
    End Function
    Public Function RightCollisionStatus(ByRef colliableObj As Collidable) As Boolean
        Return BoundingBox.CheckIntersection(rightBoundingBox, colliableObj.myBoundingBox)
    End Function
    Public Function FrontCollisionStatus(ByRef colliableObj As Collidable) As Boolean
        Return BoundingBox.CheckIntersection(frontBoundingBox, colliableObj.myBoundingBox)
    End Function
    Public Function RearCollisionStatus(ByRef colliableObj As Collidable) As Boolean
        Return BoundingBox.CheckIntersection(rearBoundingBox, colliableObj.myBoundingBox)
    End Function
End Class
