Imports Microsoft.DirectX
Public Class Camera
    '''
    ''' A camera tells DirectX where to render objects and with what perspective.
    '''

    Public Shared AxisX As New Vector3(1, 0, 0)
    Public Shared AxisY As New Vector3(0, 1, 0)
    Public Shared AxisZ As New Vector3(0, 0, 1)

    Private _Position As Vector3 'stores the world position of the camera.
    Private Right, Top, Front As Vector3 'these are 3 perpendicular vectors representing the rotation of the camera

    Private Pitch, Yaw As Double 'pitch = rotation about the Right vector, yaw = rotaion about the Top vector.

    Public View, Projection, World As Matrix
    'these matricies hold the following data:
    'view: the direction that the camera is facing in respect to the Top vector.
    'projection: the perspective, field of view, and the aspect ratio of the camera.
    'world: the position, rotation and scale of the camera in world space.

    Public Property Position As Vector3
        Get
            Return _Position
        End Get
        Set(value As Vector3)
            'update postition using the local axes - these being Right, Top and Front
            Dim deltaPos As Vector3 = value - _Position
            _Position += ConvertWorldToCameraAxes(deltaPos)
        End Set
    End Property
    Sub New()
        'initialise the camera by making it face in the positive Z direction.
        Right = AxisX
        Top = AxisY
        Front = AxisZ

        _Position = New Vector3(0, 0, 0)

        With Direct3D.Manager.Adapters(0).CurrentDisplayMode
            Projection = Matrix.PerspectiveFovLH(Math.PI * 0.25, 'field of view
                                                 .Width / .Height, 'aspect ratio
                                                 0.5, 45000.0) ' min and max rendering distances
        End With

        World = Matrix.Identity
    End Sub
    Public Function ConvertWorldToCameraAxes(ByVal world As Vector3)
        'convert a world position to a local position, local to the camera position and rotation
        Dim vector As Vector3 = Vector3.Multiply(Right, world.X)
        vector += Vector3.Multiply(Top, world.Y)
        vector += Vector3.Multiply(Front, world.Z)
        Return vector
    End Function

    Public Sub SetWorldPosition(ByVal worldPos As Vector3)
        'sets the 
        _Position = worldPos
    End Sub
    Public Sub OrbitObject(ByVal objectYaw As Double, ByVal cameraPitch As Double, ByVal orbitingDistance As Double, ByVal centreOfRotation As Vector3)
        'updates the camera position, rotation and orientation

        Pitch += cameraPitch
        Yaw = objectYaw + Math.PI / 2
        If Pitch < Math.PI / 10 Then
            Pitch = Math.PI / 10
        End If
        If Pitch > Math.PI / 3 Then
            Pitch = Math.PI / 3
        End If

        'rotate front and top camera orientation vectors with pitch
        Dim rotate As Matrix = Matrix.RotationAxis(AxisX, Pitch)
        Top = Vector3.TransformCoordinate(AxisY, rotate)
        Front = Vector3.TransformCoordinate(AxisZ, rotate)

        'rotate camera orientation vectors with yaw
        rotate = Matrix.RotationY(Yaw)
        Right = Vector3.TransformCoordinate(AxisX, rotate)
        Top = Vector3.TransformCoordinate(Top, rotate)
        Front = Vector3.TransformCoordinate(Front, rotate)

        'normalise and make them all perpendicular to one another
        'the cross product is also know as the vector product
        'it finds the vector perpendicular to the two vector
        'this perpendicular vector is then normalised to get a unit vector.
        Front = Vector3.Normalize(Front)

        Top = Vector3.Cross(Front, Right)
        Top = Vector3.Normalize(Top)

        Right = Vector3.Cross(Top, Front)
        Right = Vector3.Normalize(Right)

        'offset the camera position by the orbiting distance
        Dim orbitingDirection As Vector3 = Me.ConvertWorldToCameraAxes(New Vector3(0, 0, orbitingDistance))
        Dim orbitingPosition = _Position - orbitingDirection

        'orbit the centreOfRotation at the orbiting at the fixed distance of orbitingDistance, whilst facing the centreOfRotation
        View = Matrix.LookAtLH(orbitingPosition, centreOfRotation, Top)
    End Sub
    Function getRight() As Vector3
        Return Right
    End Function
    Function getFront() As Vector3
        Return Front
    End Function
    Function getTop() As Vector3
        Return Top
    End Function
End Class
