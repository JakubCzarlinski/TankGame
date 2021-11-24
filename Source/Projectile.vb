Imports Microsoft.DirectX
Imports Microsoft.DirectX.Direct3D
Public Class Projectile
    Inherits Collidable
    '''
    '''A projectile is a collidable object that follows projectile motion.
    '''It accelerates downwards at a constant rate. The projectile can be
    '''stopped from rendering and performing calculations by setting the
    '''active flag to false.
    '''
    Private Const gravity As Single = -6
    Private Velocity As Vector3
    Private active As Boolean
    Public Sub New(ByRef renderTo As Device,
                   ByRef objectMesh As Mesh,
                   ByRef objectTexture As Texture,
                   ByVal objectMaterial As Material,
                   ByVal objectPosition As Vector3,
                   ByVal objectRotation As Vector3,
                   ByVal dimensions As Vector3,
                   ByVal normalisedDirection As Vector3,
                   ByVal speed As Single)

        MyBase.New(renderTo, objectMesh, objectTexture, objectMaterial, objectPosition, objectRotation, dimensions)
        'determine the initial velocity of the projectile
        Velocity = speed * normalisedDirection
        active = True
    End Sub

    Public Sub UpdatePosition(ByVal frameTime As Double)
        If active Then
            'if the projectile is active, update its velocity and position
            Velocity.Y += gravity
            position += Velocity * frameTime
        End If
    End Sub
    Public Sub SetActive(ByVal Value As Boolean)
        active = Value
    End Sub
    Public Function GetActive() As Boolean
        Return active
    End Function
    Overloads Sub Render()
        'onlty render if active
        If active Then
            MyBase.Render()
        End If
    End Sub
End Class
