Imports Microsoft.DirectX
Imports Microsoft.DirectX.Direct3D
Public Class GameObject
    '''
    '''A game object is a made of a mesh, texture, and a material.
    '''These are rendered at a specified rotation and position to a graphics device.
    '''

    'graphics device
    Protected device As Direct3D.Device
    'the mesh 
    Protected myMesh As Mesh
    'the texture applied to the mesh
    Protected myTexture As Texture
    'the material given to the texture and mesh
    Protected myMaterial As Material
    'rotation and postitions of object
    Protected _rotation As Vector3
    Protected _position As Vector3
    Protected worldMatrix As Matrix
    '''                         Properties                          '''
    Public Overridable Property position As Vector3
        Set(value As Vector3)
            'updates the position of the world matrix 
            _position = value
            GenerateWorldMatrix()
        End Set
        Get
            Return _position
        End Get
    End Property
    Public Overridable Property rotation As Vector3
        Set(value As Vector3)
            'updates the rotation of the world matrix 
            _rotation = value
            GenerateWorldMatrix()
        End Set
        Get
            Return _rotation
        End Get
    End Property

    Public Sub New(ByRef renderTo As Device,
                   ByRef objectMesh As Mesh,
                   ByRef objectTexture As Texture,
                   ByVal objectMaterial As Material,
                   ByVal objectPosition As Vector3,
                   ByVal objectRotation As Vector3)
        'Initialise a game object
        device = renderTo
        If Not IsNothing(objectMesh) Then
            objectMesh.ComputeNormals()
        End If
        myMesh = objectMesh
        myTexture = objectTexture
        myMaterial = objectMaterial
        _position = objectPosition
        _rotation = objectRotation
        GenerateWorldMatrix()
    End Sub

    '''                         Rendering                           '''
    Public Overridable Sub Render()
        'render the object with its material, texture and world matrix
        device.SetTransform(TransformType.World, worldMatrix)
        device.Material = myMaterial
        device.SetTexture(0, myTexture)
        myMesh.DrawSubset(0)
    End Sub
    Public Sub GenerateWorldMatrix()
        'create the world matrix for rendering
        'move it then rotate its orientation.
        worldMatrix = Matrix.Translation(_position)
        Dim tempMatrix As Matrix = Matrix.RotationYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z)
        worldMatrix = tempMatrix * worldMatrix
    End Sub
    '''                           Disposal                          '''
    Public Sub Dispose()
        'disposed all meshes and objects
        If Not myMesh.Disposed Then
            myMesh.Dispose()
        End If
        If myTexture IsNot Nothing AndAlso Not myTexture.Disposed Then
            myTexture.Dispose()
        End If
    End Sub
    Public Sub Reset(ByRef myNewMesh As Mesh, ByRef myNewTexture As Texture)
        'This is used to reset a dynamic game object that has been lost
        'due to the device being lost.
        'Most objects are managed, rather that dynamic, meaning that
        'they are not required to be disposed of when the graphics device
        'is lost.
        myMesh = myNewMesh
        If Not IsNothing(myMesh) Then
            myMesh.ComputeNormals()
        End If
        myTexture = myNewTexture
    End Sub
End Class
