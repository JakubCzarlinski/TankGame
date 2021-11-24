Imports Microsoft.DirectX
Imports Microsoft.DirectX.Direct3D
Imports Microsoft.DirectX.DirectInput
Module DirectXModule

    '''
    ''' This module allows for easy communication with DirectX9.
    ''' It handles initialising: lights,
    '''                           sound,
    '''                           meshes,
    '''                           textures,
    '''                           input devices,
    '''                           and graphics devices.
    '''

    Private _defaultMaterial As New Material With {
            .Ambient = Color.Black,
            .Diffuse = Color.White
            }
    Public ReadOnly Property defaultMaterial As Material
        Get
            Return _defaultMaterial
        End Get
    End Property
    Public Function InitDevice(ByRef fullScreenForm As Form) As Direct3D.Device
        'fullscreen
        MakeFullscreen(fullScreenForm)

        'prepares directx, tells it how to render
        Dim presentParams() As PresentParameters = New PresentParameters() {ReturnPeresentParameters(fullScreenForm)}
        'tells the graphics card where to reneder

        Dim graphics As New Direct3D.Device(Direct3D.Manager.Adapters.Default.Adapter, 'default graphics card will be used
                                            Direct3D.DeviceType.Hardware, 'used hardware to render.
                                            fullScreenForm.Handle, 'tells DX9 to render to the form
                                            CreateFlags.HardwareVertexProcessing, 'vertecies are processed by the hardware
                                            presentParams)
        InitDevice = graphics
    End Function
    Public Function ReturnPeresentParameters(ByRef fullScreenForm As Form) As PresentParameters
        Dim presentParams As PresentParameters = New PresentParameters With {
            .DeviceWindow = fullScreenForm,
            .BackBufferCount = 1, 'the number of backbuffers
            .BackBufferFormat = Direct3D.Manager.Adapters(0).CurrentDisplayMode.Format,'uses the default graphics format
            .BackBufferHeight = fullScreenForm.Height, 'set the backbuffer height
            .BackBufferWidth = fullScreenForm.Width, 'sets the backbuffer width
            .Windowed = False, 'makes it fullscreen
            .SwapEffect = SwapEffect.Discard, 'controls how backbuffers are swapped
            .PresentationInterval = PresentInterval.One, 'enables vertical sync
            .MultiSample = MultiSampleType.TwoSamples, 'makes lines look smoother
            .EnableAutoDepthStencil = True,
            .AutoDepthStencilFormat = DepthFormat.D24X8
        }
        Return presentParams
    End Function
    Public Sub MakeFullscreen(ByRef fullScreenForm As Form)
        fullScreenForm.FormBorderStyle = FormBorderStyle.None
        fullScreenForm.TopMost = True
        fullScreenForm.WindowState = FormWindowState.Normal

        With Direct3D.Manager.Adapters(0).CurrentDisplayMode
            fullScreenForm.Size = New Size(.Width, .Height)
        End With

    End Sub
    Public Sub SetCurrentCamera(ByRef graphicsDevice As Direct3D.Device, ByVal cam As Camera)
        'sets the current camera for the graphics device
        graphicsDevice.SetTransform(TransformType.Projection, cam.Projection)
    End Sub
    Public Sub InitLighting(ByRef graphicsDevice As Direct3D.Device,
                            ByVal ambientColour As Color,
                            ByVal diffuseColour As Color,
                            ByVal specularColour As Color)
        'only renders triangles facing the camera
        graphicsDevice.SetRenderState(RenderStates.CullMode, Cull.CounterClockwise)

        'enable ambient and diffuse lighting but disable specular lighting as it reduces preformance
        graphicsDevice.SetRenderState(RenderStates.Lighting, True)
        graphicsDevice.SetRenderState(RenderStates.SpecularEnable, False)

        'load the graphics devices with the defaul material
        graphicsDevice.Material = defaultMaterial

        'Create 3 lights
        'values are changed by reference
        Dim blueLight As Light = graphicsDevice.Lights(0)
        Dim redLight As Light = graphicsDevice.Lights(1)
        Dim whiteLight As Light = graphicsDevice.Lights(2)
        'create the blue light
        With blueLight

            .Type = LightType.Point
            .Range = 600

            'the attenuation tells us how fast light decays over a distance
            'the strength of light is defined by this formula:
            '       = 1/(a0 + a1 * D + a2 * D * D)
            '   where D is the distance from the light to that point,
            '   and a0 is attenuation0...
            .Attenuation0 = 1 'constant factor
            .Attenuation1 = 0 'linear factor
            .Attenuation2 = 0 'quadratic factor


            .Ambient = Color.Blue 'light coming from every direction
            .Diffuse = Color.Blue 'light directed away from the position of the light
            .Specular = specularColour 'highlights; although this is turned off

            .Position = New Vector3(0, 0, 0)
            .Direction = New Vector3(0, -1, 0)
            .Enabled = True
        End With
        'create the red light
        With redLight
            .Type = LightType.Point
            .Range = 600

            .Attenuation0 = 1
            .Attenuation1 = 0
            .Attenuation2 = 0

            .Ambient = Color.Red
            .Diffuse = Color.Red
            .Specular = specularColour

            .Position = New Vector3(0, 0, 0)
            .Direction = New Vector3(0, -1, 0)
            .Enabled = True
        End With
        'create the white light
        With whiteLight
            .Type = LightType.Point
            .Range = 600


            .Attenuation0 = 1
            .Attenuation1 = 0
            .Attenuation2 = 0

            .Ambient = ambientColour
            .Diffuse = diffuseColour
            .Specular = specularColour

            .Position = New Vector3(0, 0, 0)
            .Direction = New Vector3(0, -1, 0)
            .Enabled = True
        End With

    End Sub
    Function InitMesh(ByVal filename As String, ByVal flags As MeshFlags, ByRef graphicsDevice As Direct3D.Device) As Mesh
        'initialise mesh - is mesh is missing, provide a useful error.
        Try
            Return Mesh.FromFile(filename, flags, graphicsDevice)
        Catch ex As Direct3DXException
            Throw New System.IO.FileNotFoundException()
        End Try
    End Function
    Function InitTexture(ByVal filename As String, ByRef graphicsDevice As Direct3D.Device) As Texture
        'reads a texture from a file
        Dim stream As System.IO.FileStream = New IO.FileStream(filename, IO.FileMode.Open)
        InitTexture = Texture.FromStream(graphicsDevice, stream, Usage.None, Pool.Managed)
        stream.Close()
    End Function
    Function InitInputDevice(ByVal KeyBoardTrueMouseFalse As Boolean, ByRef fullScreenForm As Form) As DirectInput.Device
        'this initialises the keyboard and mouse to be usable
        'this function may throw an error if the window is not active
        ' it must be caught elsewhere because the error depends if
        ' the form is in focus or not

        Dim inputDevice As DirectInput.Device
        If KeyBoardTrueMouseFalse Then
            'keyboard
            inputDevice = New DirectInput.Device(DirectInput.SystemGuid.Keyboard)
            inputDevice.SetDataFormat(DeviceDataFormat.Keyboard)
        Else
            'mouse
            inputDevice = New DirectInput.Device(DirectInput.SystemGuid.Mouse)
            inputDevice.SetDataFormat(DeviceDataFormat.Mouse)
        End If
        'always track device actions
        inputDevice.SetCooperativeLevel(fullScreenForm, CooperativeLevelFlags.Foreground Or CooperativeLevelFlags.Exclusive)
        inputDevice.Acquire()
        Return inputDevice

    End Function
    Function InitSoundDevice(ByRef myForm As Control) As DirectSound.Device
        'create a sound device that the windows form and directX use to play sound
        InitSoundDevice = New DirectSound.Device()
        InitSoundDevice.SetCooperativeLevel(myForm, DirectSound.CooperativeLevel.Priority)
    End Function
End Module