Imports System.ComponentModel
Imports System.Threading
Imports Microsoft.DirectX
Imports Microsoft.DirectX.Direct3D
Imports Microsoft.DirectX.DirectInput
Public Class TankGame
    'graphics and sound devices used for rendering and playing audio
    Private graphicsDevice As Direct3D.Device
    Private soundDevice As DirectSound.Device
    Private userInterface As Sprite

    'these manage the game loop - caps the framerate to 60fps, whilst taking into consideration the fps of the second device
    Private frameStopwatch As Stopwatch
    Private frameTime As Double
    Private frameRate As Double
    Private syncCounter As Integer = 1
    Private Const intDesiredFrameRate As Integer = 60
    Public Shared enemyFPS As Double = intDesiredFrameRate 'this keeps movements constant between devices
    Private Const maxFrameTime As Double = 1000 / intDesiredFrameRate

    'this keeps the game open
    Public Shared GameOpen As Boolean = False
    'this makes sure the game is initialised only once
    Private FirstRun As Boolean = True
    'check if objects are disposed of   
    Private isGameDisposed As Boolean = False
    Private isLost As Boolean = False
    Private previouslyLost As Boolean = True

    'primary buffer and the 3d sound listener are used to listen for sounds in 3d space
    Private primarySoundBuffer As DirectSound.Buffer
    Private soundListener As DirectSound.Listener3D

    'each sound requires a secondary buffer
    'each secondary buffer is made with a 3d buffer as sounds are played in 3d space
    Private shootAudio As DirectSound.SecondaryBuffer
    Private Shoot3DAudio As DirectSound.Buffer3D

    Private enemyShootAudio As DirectSound.SecondaryBuffer
    Private enemyShoot3DAudio As DirectSound.Buffer3D

    Private engineAudio As DirectSound.SecondaryBuffer
    Private engine3DAudio As DirectSound.Buffer3D

    Private enemyEngineAudio As DirectSound.SecondaryBuffer
    Private enemyEngine3DAudio As DirectSound.Buffer3D

    'input devices - used to detect user input
    Private keyboard As DirectInput.Device = Nothing
    Private mouse As DirectInput.Device = Nothing
    Private keyboardState As DirectInput.KeyboardState
    Private mouseState As DirectInput.MouseState

    'user spawn points - used to spawn the tanks and flags
    Private mySpawnPoint As Vector3
    Private enemySpawnPoint As Vector3

    'tanks - used to move, shoot, and campture the flags
    Private myTank As Tank
    Public Shared enemyTank As Tank

    'bullets - used to shoot the other tank
    Private bulletMesh As Mesh
    Private bulletTexture As Texture
    Private bullet As Projectile
    Private sentMyBullet As Boolean = True

    Private enemyBullet As Projectile
    Public Shared enemyBulletDetails(1) As Vector3
    Public Shared enemyBulletPending As Boolean

    'death status - used to respawn 
    Public Shared enemyDead As Boolean
    Public Shared allyDead As Boolean


    'flags - used for capture the flag
    Public Shared myFlag As Collidable
    Public Shared enemyFlag As Collidable
    Public Shared myFlagAttached As Boolean
    Public Shared enemyFlagAttached As Boolean

    'trees - used as obstacles 
    Private Trees() As Collidable

    'terrain
    Private myTerrain As Terrain
    Private myTerrainPoints As MyMatrix
    Private GameWalls(3) As Collidable

    'shop
    Private shopOpen As Boolean
    Private shopScreen As Texture
    Private shopSize As Size
    Private shopFont As Font

    'score
    Private Score() As Integer = {0, 0}
    Private Scored As Boolean = False
    Public Shared RecievedScore As Boolean = False
    Private ScoreFont As Font

    Private ReadOnly Property ScoreText As String
        Get
            Return Score(0).ToString() & " : " & Score(1).ToString()
        End Get
    End Property

    'upgrades
    Private fogMin As Integer = 300
    Private rotationSpeed As Single = 1.0

    Private testTerrain As Terrain
    Private testVB As Terrain.VertexBufferBitmap
    Private testIB() As Int32
    Private testVbuffer As VertexBuffer
    Private testIbuffer As IndexBuffer
    Private testTexture As Texture

    Private mainMenuForm As MainMenu
    '                   Initialise Form Settings                     '
    Sub New(ByRef mainForm As MainMenu, ByRef terrainPoints As MyMatrix)
        ' This call is required by the designer.
        InitializeComponent()

        'keep track of the main menu form so that it is properly closed
        mainMenuForm = mainForm

        myTerrainPoints = terrainPoints

        'move the form to the top left of the screen, make it full screen 
        Dim p As New Point(0, 0)
        Me.Location = p
        Me.Height = Screen.PrimaryScreen.Bounds.Height
        Me.Width = Screen.PrimaryScreen.Bounds.Width
        'start recieving UDP
        NetworkModule.RecieveUDP()

        'start the frame timer
        frameStopwatch = New Stopwatch()
        frameStopwatch.Start()
        'show the form
        Me.Activate()
        Me.Show()
    End Sub

    '                   Initialise                     '
    Private Sub InitAll()
        'make the form fullscreen
        DirectXModule.MakeFullscreen(Me)

        'take control of the graphics card
        graphicsDevice = DirectXModule.InitDevice(Me)

        'initialises the user interface
        userInterface = New Sprite(graphicsDevice)
        InitUI()

        'create the terrain
        myTerrain = New Terrain(8, 10, 120, graphicsDevice)
        myTerrain.points = myTerrainPoints
        myTerrain.GenerateMesh()

        InitTrees(myTerrain, MainMenu.treePos)

        'spawn tanks at their spawn points
        DefineSpawnPoints()
        InitTanks()
        'set the tank to control the camera
        DirectXModule.SetCurrentCamera(graphicsDevice, myTank.myCamera)

        'initialise the bullet meshes and textures
        InitBullets()
        'initialise the flag meshes and textures
        InitFlags()
        'create invisible walls that prevent the tanks driving off the map
        InitGameWalls()
        'initialise the lighting and fog
        InitLighting()
        enableFog()

        'initialise the audio files
        LoadAudio()
        'take control of the keyboard and mouse
        keyboard = DirectXModule.InitInputDevice(True, Me)
        mouse = DirectXModule.InitInputDevice(False, Me)
        UpdateInputs()

        'testTerrain = New Terrain(12, 2, 120, graphicsDevice)
        'testTerrain.GenerateMatrix(1.8, 0.9)
        'create a kernel which finds the average of the points surround a given point
        'Dim kernel As New MyMatrix(4, 4)
        'kernel.SetAllEntires(1 / 16)
        'complete a convolution over the matrix, in order to smoothen the terrain and then generate the terrain
        'testTerrain.points = MyMatrix.Convolution(testTerrain.points, kernel)

        'testVB = testTerrain.GenerateVertexBufferWithTexture()
        'testIB = testTerrain.GenerateIndexBuffer32Bit()
        'testTexture = New Texture(graphicsDevice, testVB.Texture, Usage.None, Pool.Managed)
        'testIbuffer = New IndexBuffer(New Int32().GetType(), testIB.Count, graphicsDevice, Usage.None, Pool.Managed)
        'testIbuffer.SetData(testIB, 0, LockFlags.None)
        '
        'testVbuffer = New VertexBuffer(New CustomVertex.PositionNormalTextured().GetType(), testTerrain.NumberOfVertices, graphicsDevice, Usage.None, CustomVertex.PositionNormalTextured.Format, Pool.Managed)
        'testVbuffer.SetData(testVB.VertexBuffer, 0, LockFlags.None)
    End Sub
    Private Sub InitUI()
        Dim scoreFontDesc As New FontDescription() With {
            .Height = -126,
            .Width = -60,
            .CharSet = CharacterSet.Default,
            .FaceName = "COMIC SANS",
            .Quality = FontQuality.Default}
        Dim shopFontDesc As FontDescription = scoreFontDesc
        shopFontDesc.Height = -84
        shopFontDesc.Width = -30

        ScoreFont = New Font(graphicsDevice, scoreFontDesc)
        shopFont = New Font(graphicsDevice, shopFontDesc)


        shopSize = Me.Size
        Dim bitmap As New Bitmap("models\shop.jpg")
        Dim resize As New Bitmap(bitmap, shopSize)
        shopScreen = Texture.FromBitmap(graphicsDevice, resize, Usage.None, Pool.Managed)

    End Sub
    Private Sub enableFog()
        'Enable fog calculations- a linear calculation is used.
        graphicsDevice.SetRenderState(RenderStates.FogEnable, True)
        graphicsDevice.SetRenderState(RenderStates.FogColor, Color.SkyBlue.ToArgb())

        graphicsDevice.SetRenderState(RenderStates.FogTableMode, FogMode.Linear)
        graphicsDevice.SetRenderState(RenderStates.FogStart, CSng(fogMin))
        graphicsDevice.SetRenderState(RenderStates.FogEnd, CSng(fogMin * 1.5))
    End Sub
    Private Sub DefineSpawnPoints()
        'spawn points are on the opposite corners of the map.
        'flags and tanks spawn here
        Dim myOffset As Single = 0.1 '10% of the way
        Dim enemyOffset As Single = 1 - myOffset '90% of the way
        'scale the values
        Dim myXZ As Integer = myTerrain.MaxSideLength * myOffset
        Dim enemyXZ As Integer = myTerrain.MaxSideLength * enemyOffset
        'height of the terrain at spawn points
        Dim heightAtOffest As Single = myTerrain.GetHeightAtPoint(myXZ, myXZ)
        Dim heightAtEnemyOffest As Single = myTerrain.GetHeightAtPoint(enemyXZ, enemyXZ)
        If MainMenu.firstPlayer Then
            mySpawnPoint = New Vector3(myXZ, heightAtOffest, myXZ)
            enemySpawnPoint = New Vector3(enemyXZ, heightAtEnemyOffest, enemyXZ)
        Else
            mySpawnPoint = New Vector3(enemyXZ, heightAtEnemyOffest, enemyXZ)
            enemySpawnPoint = New Vector3(myXZ, heightAtOffest, myXZ)
        End If
    End Sub
    Private Sub InitTanks()
        'create the tanks by loading the mesh, texture and material
        'and then creating a tank object
        Dim tank As Mesh = DirectXModule.InitMesh("models\tank.x", MeshFlags.Managed, graphicsDevice)
        Dim tankTexture As Texture = DirectXModule.InitTexture("models\tankTexture.jpg", graphicsDevice)
        Dim tankMaterial As Material = DirectXModule.defaultMaterial

        tankMaterial.Specular = Color.FromArgb(255, 105, 10)
        tankMaterial.SpecularSharpness = 15

        myTank = New Tank(graphicsDevice,           'graphics device
                          tank,                     'mesh
                          tankTexture,              'texture
                          tankMaterial,             'material
                          mySpawnPoint,                     'position
                          New Vector3(0, -Math.PI / 2, 0),  'mesh rotaion
                          New Vector3(34, 12, 10))           'bounding box dimensions

        enemyTank = New Tank(graphicsDevice,            'graphics device
                             tank,                      'mesh
                             tankTexture,               'texture
                             tankMaterial,              'material
                             enemySpawnPoint,                      'position
                             New Vector3(0, -Math.PI / 2, 0),      'mesh rotaion
                             New Vector3(34, 12, 10))    'bounding box dimensions

    End Sub
    Private Sub InitBullets()
        'initialise the bullet mesh and texture
        bulletMesh = DirectXModule.InitMesh("models\projectile.x", MeshFlags.Managed, graphicsDevice)
        bulletTexture = DirectXModule.InitTexture("models\bulletTexture.jpg", graphicsDevice)
        bullet = Nothing
        enemyBullet = Nothing
    End Sub
    Private Sub InitFlags()
        'initialise the flags - ally = red, enemy = red
        Dim flag As Mesh = DirectXModule.InitMesh("models\flag.x", MeshFlags.Managed, graphicsDevice)
        Dim myFlagTexture As Texture
        Dim enemyFlagTexture As Texture
        If MainMenu.firstPlayer Then
            myFlagTexture = DirectXModule.InitTexture("models\redFlag.jpg", graphicsDevice)
            enemyFlagTexture = DirectXModule.InitTexture("models\blueFlag.jpg", graphicsDevice)
        Else
            myFlagTexture = DirectXModule.InitTexture("models\blueFlag.jpg", graphicsDevice)
            enemyFlagTexture = DirectXModule.InitTexture("models\redFlag.jpg", graphicsDevice)
        End If

        myFlag = New Collidable(graphicsDevice,
                                flag,
                                myFlagTexture,
                                DirectXModule.defaultMaterial,
                                mySpawnPoint,
                                New Vector3(),
                                New Vector3(30, 30, 30))
        enemyFlag = New Collidable(graphicsDevice,
                                flag,
                                enemyFlagTexture,
                                DirectXModule.defaultMaterial,
                                enemySpawnPoint,
                                New Vector3(),
                                New Vector3(30, 30, 30))
    End Sub
    Private Sub InitLighting()
        'initialise lighting
        DirectXModule.InitLighting(graphicsDevice,
                                   Color.White,
                                   Color.White,
                                   Color.White)

        Dim height As New Vector3(0, 350, 0)
        Dim maxPos As New Vector3(myTerrain.MaxSideLength, 0, myTerrain.MaxSideLength)
        'blue light
        graphicsDevice.Lights(0).Position = maxPos + height
        'red light
        graphicsDevice.Lights(1).Position = New Vector3(0, 0, 0) + height
        maxPos.Multiply(0.5)
        'natural lights
        graphicsDevice.Lights(2).Position = maxPos + height
    End Sub
    Private Sub LoadAudio()
        'connects the application with the devices sound card
        soundDevice = DirectXModule.InitSoundDevice(Me)

        'one primary buffer is required for the application
        Dim buffDescription As New DirectSound.BufferDescription With {
            .Control3D = True,
            .PrimaryBuffer = True
        }
        primarySoundBuffer = New DirectSound.Buffer(buffDescription, soundDevice)

        'attatch a 3D sound listener to the primary sound buffer and ally tank, 
        'allowing the user to hear sounds from the tanks position.
        soundListener = New DirectSound.Listener3D(primarySoundBuffer)
        soundListener.Orientation = New DirectSound.Listener3DOrientation(myTank.myCamera.getFront(), myTank.myCamera.getTop())
        soundListener.Position = myTank.position

        'the following are secondary buffers
        buffDescription.PrimaryBuffer = False

        'a secondary buffer is required for each sound
        'these are 3D buffers, allowing 3D sound.

        'shooting sound for ally tank
        shootAudio = New DirectSound.SecondaryBuffer("sounds\shoot.wav", buffDescription, soundDevice)
        Shoot3DAudio = New DirectSound.Buffer3D(shootAudio) With {
            .Position = myTank.position,
            .MinDistance = 200,
            .MaxDistance = 1000}


        'shooting sound for enemy tank
        enemyShootAudio = New DirectSound.SecondaryBuffer("sounds\shoot.wav", buffDescription, soundDevice)
        enemyShoot3DAudio = New DirectSound.Buffer3D(enemyShootAudio) With {
            .Position = enemyTank.position,
            .MinDistance = 200,
            .MaxDistance = 1000}

        'engine audio for ally tank
        engineAudio = New DirectSound.SecondaryBuffer("sounds\engine.wav", buffDescription, soundDevice)
        engine3DAudio = New DirectSound.Buffer3D(engineAudio) With {
            .Position = myTank.position,
            .MinDistance = 100,
            .MaxDistance = 500}

        'engine audio for enemy tank
        enemyEngineAudio = New DirectSound.SecondaryBuffer("sounds\engine.wav", buffDescription, soundDevice)
        enemyEngine3DAudio = New DirectSound.Buffer3D(enemyEngineAudio) With {
            .Position = enemyTank.position,
            .MinDistance = 100,
            .MaxDistance = 500}


        engineAudio.Play(0, DirectSound.BufferPlayFlags.Looping)

        enemyEngineAudio.SetCurrentPosition(200)
        enemyEngineAudio.Play(0, DirectSound.BufferPlayFlags.Looping)


    End Sub
    Private Sub InitTrees(ByRef thisTerrain As Terrain, ByVal positions() As Vector3)
        'create trees that were previously generated
        Dim treeMesh As Mesh = DirectXModule.InitMesh("models\tree.x", MeshFlags.Managed, graphicsDevice)
        Dim treeTexture As Texture = DirectXModule.InitTexture("models\treeTexture.jpg", graphicsDevice)

        Dim treeDimensions As New Vector3(25, 80, 25)
        Dim treePositionOffset As New Vector3(10, 0, 0)

        Dim noRotation As New Vector3()
        ReDim Preserve Trees(positions.Count - 1)

        For index = 0 To positions.Count - 1
            Trees(index) = New Collidable(graphicsDevice,
                                          treeMesh,
                                          treeTexture,
                                          defaultMaterial,
                                          positions(index) + treePositionOffset,
                                          noRotation,
                                          treeDimensions)
        Next index

    End Sub
    Private Sub InitGameWalls()
        Dim halfSide As Single = myTerrain.MaxSideLength / 2
        Dim positions() As Vector3 = {New Vector3(halfSide, myTerrain.MaxHeight / 2, -75),
                                      New Vector3(-75, myTerrain.MaxHeight / 2, halfSide),
                                      New Vector3(halfSide, myTerrain.MaxHeight / 2, myTerrain.MaxSideLength + 75),
                                      New Vector3(myTerrain.MaxSideLength + 75, myTerrain.MaxHeight / 2, halfSide)}



        Dim rotations() As Vector3 = {New Vector3(),
                                      New Vector3(0, Math.PI / 2, 0),
                                      New Vector3(),
                                      New Vector3(0, Math.PI / 2, 0)}
        For index = 0 To 3
            GameWalls(index) = New Collidable(graphicsDevice, Nothing, Nothing, Nothing, positions(index), rotations(index), New Vector3(myTerrain.MaxSideLength, myTerrain.MaxHeight, 100))
        Next


    End Sub

    '                   Game Loop Events                  '
    Private Sub TankGame_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        'initialise and make sure the game does not crash because of DX9 not communicating with Windows.
        If FirstRun Then
            Try
                'Initiate all objects required for the game to start
                'this will happen until it is successful
                InitAll()
                If Not isLost Then
                    GameOpen = True
                    Console.WriteLine("init done")
                Else
                    Thread.Sleep(200)
                End If
            Catch fileMissing As System.IO.FileNotFoundException
                'defensive programming
                'if a file does not exist - close the game.
                FirstRun = False
                GameOpen = False

                MsgBox("There is a missing file.")
            Catch ex As Exception
                'Unexpected error, defensive programming
                If Not IsNothing(graphicsDevice) Then

                    graphicsDevice.Dispose()
                End If
                If Not IsNothing(userInterface) Then

                    userInterface.Dispose()
                End If

                Thread.Sleep(50)
                Console.WriteLine(ex.ToString())
            End Try

            While GameOpen
                'if graphics device is responding, then do all game events
                Try
                    graphicsDevice.TestCooperativeLevel()
                    If Not isLost Then

                        DoEvents()
                    End If
                    FirstRun = False
                Catch devLost As DeviceLostException
                    'if window has lost focus dispose of objects and wait
                    'DeviceLostException will be thrown until the user switches back to the window.

                    If Not isGameDisposed Then
                        userInterface.OnLostDevice()
                        ScoreFont.OnLostDevice()
                        shopFont.OnLostDevice()
                        previouslyLost = True
                        If keyboard IsNot Nothing Then
                            keyboard.Dispose()
                        End If
                        If mouse IsNot Nothing Then
                            mouse.Dispose()
                        End If
                        isGameDisposed = True
                    End If

                    Thread.Sleep(200)
                Catch devNotReset As DeviceNotResetException
                    'if form has regained its focus, the graphics will have to be reset
                    'resources in the default pool will have to be reinitialised
                    Try
                        'reset the 3d graphics 
                        Dim presentParams() As PresentParameters = New PresentParameters() {DirectXModule.ReturnPeresentParameters(Me)}
                        graphicsDevice.Reset(presentParams)
                        ScoreFont.OnResetDevice()
                        shopFont.OnResetDevice()

                        InitLighting()
                        enableFog()
                        'set the tank to control the camera
                        DirectXModule.SetCurrentCamera(graphicsDevice, myTank.myCamera)

                        'reset the user interface
                        userInterface.OnResetDevice()

                        keyboard = DirectXModule.InitInputDevice(True, Me)
                        mouse = DirectXModule.InitInputDevice(False, Me)

                        'resetting the device has finished
                        isGameDisposed = False

                    Catch devLostAgain As DeviceLostException
                        'this is thrown when the window loses focus
                        Console.WriteLine("DeviceLostException: device tried to reset." + devLostAgain.ToString())

                    Catch nullRef As NullReferenceException
                        'this can be thrown if the device reset is interupted
                        Console.WriteLine("NullReferenceException: device tried to reset." + nullRef.ToString())
                    Catch invalidCall As InvalidCallException
                        'this can be thrown if the device reset is interupted and DX9 cannot determine
                        ' if the present parameters are valid for the graphics card
                        Console.WriteLine("InvalidCallException: device tried to reset." + invalidCall.ToString())
                    Catch unknownExept As Exception
                        Console.WriteLine("Unknown error occured." + unknownExept.ToString())
                    End Try
                Catch exept As Exception
                    Thread.Sleep(200)
                    Console.WriteLine("Unhandled exception: tried to draw." + exept.ToString())
                End Try

                'allow Windows to complete events preventing the form from hanging
                Application.DoEvents()
            End While

            If Not (FirstRun Or GameOpen) Then
                'if game is not on its first run and the game is not running, shutdown.
                Shutdown()
            End If
        End If
    End Sub
    Public Sub DoEvents()
        'do game events if the framerate allows it
        frameTime = frameStopwatch.ElapsedMilliseconds
        If frameStopwatch.ElapsedMilliseconds >= maxFrameTime Then
            frameRate = 1000 / frameTime
            'update the the keyboard and mouse states
            UpdateInputs()

            If keyboardState.Item(Key.Escape) Then
                GameOpen = False
            End If
            If shopOpen Then
                'upgrade the tanks
                If keyboardState.Item(Key.K) Then
                    'movement and projectile speed
                    myTank.Speed += 15
                    shopOpen = False
                End If
                If keyboardState.Item(Key.J) Then
                    'fog range
                    fogMin += 70
                    graphicsDevice.SetRenderState(RenderStates.FogStart, CSng(fogMin))
                    graphicsDevice.SetRenderState(RenderStates.FogEnd, CSng(fogMin * 1.5))
                    shopOpen = False
                End If
                If keyboardState.Item(Key.L) Then
                    'rotation speed
                    rotationSpeed += 0.4
                    shopOpen = False
                End If
            End If

            UpdateGameLogic()

            SendUpdates()
            Render()
            'reset the frame timer
            frameStopwatch.Stop()
            frameStopwatch.Reset()
            frameStopwatch.Start()
        Else
            'freeze the thread until the if statement will be true
            Thread.Sleep(maxFrameTime - frameTime)
        End If
    End Sub
    Public Sub UpdateInputs()
        'updates mouse and kb
        mouse.Poll()
        keyboard.Poll()
        'then updates mouse and kb states
        mouseState = mouse.CurrentMouseState()
        keyboardState = keyboard.GetCurrentKeyboardState()

    End Sub
    Public Sub UpdateGameLogic()
        'This subroutine manages all the game logic.

        'Multipliying the changes in posision/rotation by the time taken for the previous frame to run
        ' will make the speeds constant over different frame rates, therefore different computers.
        ' This is important in my tank game as players will be executing the game on different hardware.
        ' If I did not do this, the player with the slower computer will be disadvantaged as they would
        ' move slower.
        Dim PreviousFrameTime As Double
        If previouslyLost Then
            PreviousFrameTime = 0
            previouslyLost = False
        Else
            Dim averageFramerate As Double = 2 / (enemyFPS + frameRate)
            PreviousFrameTime = averageFramerate / frameRate
            PreviousFrameTime *= 50
            Console.WriteLine("Frame time: " & frameTime.ToString() & "Frame Rate: " & frameRate.ToString())
        End If


        If Scored Or RecievedScore Then
            'if someone has score - reset everyone
            myTank.position = mySpawnPoint
            enemyTank.position = enemySpawnPoint

            myFlag.position = mySpawnPoint
            enemyFlag.position = enemySpawnPoint

            myFlagAttached = False
            enemyFlagAttached = False

            If Scored Then
                'if ally scored
                Score(0) += 1
            Else
                'else it must the enemy who scored
                Score(1) += 1
            End If

            shopOpen = True
            RecievedScore = False
            Scored = False
        End If

        ' -------- Spawn bullets
        If keyboardState.Item(Key.Space) Then
            If IsNothing(bullet) OrElse Not bullet.GetActive() Then
                bullet = myTank.Shoot(bulletMesh,
                                      bulletTexture,
                                      DirectXModule.defaultMaterial,
                                      300)
                If shootAudio.Status.Playing Then
                    shootAudio.Stop()
                    shootAudio.SetCurrentPosition(0)
                End If
                shootAudio.Play(0, DirectSound.BufferPlayFlags.Default)
                sentMyBullet = False
            End If
        End If
        If enemyBulletPending Then
            If IsNothing(enemyBullet) OrElse Not enemyBullet.GetActive() Then
                Dim rotationVector = New Vector3(0, enemyBulletDetails(0).Y, 0)
                enemyTank.rotation = rotationVector
                enemyTank.pitch = enemyBulletDetails(0).Z
                enemyTank.position = enemyBulletDetails(1)

                enemyBullet = enemyTank.Shoot(bulletMesh,
                                              bulletTexture,
                                              DirectXModule.defaultMaterial,
                                              300)

                enemyBullet.UpdatePosition(PreviousFrameTime)
                enemyBulletPending = False
                If enemyShootAudio.Status.Playing Then
                    enemyShootAudio.Stop()
                    enemyShootAudio.SetCurrentPosition(0)
                End If
                enemyShootAudio.Play(0, DirectSound.BufferPlayFlags.Default)

            End If
        End If
        ' ----------------------- Check Collisions------------------------
        ' ---- wall collisions
        Dim walls() As Collidable = Collidable.CollisionStatus(myTank, GameWalls)
        If walls.Count <> 0 Then
            enemyFlagAttached = False
            myTank.position = mySpawnPoint
        End If
        ' ---- tree collisions
        Dim collisingWithTrees() As Collidable = Collidable.CollisionStatus(myTank, Trees)
        Dim forwardCollision As Boolean = False
        Dim rearCollision As Boolean = False
        If collisingWithTrees.Count <> 0 Then
            For Each collidingTree As Collidable In collisingWithTrees
                forwardCollision = forwardCollision OrElse myTank.FrontCollisionStatus(collidingTree)
                rearCollision = rearCollision OrElse myTank.RearCollisionStatus(collidingTree)
            Next collidingTree
            'move away
            If forwardCollision Then
                myTank.Move(PreviousFrameTime * 0.125, False)
            ElseIf rearCollision Then
                myTank.Move(PreviousFrameTime * 0.125, True)
            End If
        End If

        Dim pitch As Double = mouseState.Y / 60
        Dim yaw As Double = 0

        If Not shopOpen Then
            'tanks can rotate upto "rotationSpeed" radians per second
            If keyboardState.Item(Key.D) Then
                'rotate right
                yaw += rotationSpeed
            End If
            If keyboardState.Item(Key.A) Then
                'rotate left
                yaw -= rotationSpeed
            End If

            'tanks move up to (125 + upgrades) units per second
            If keyboardState.Item(Key.W) And Not forwardCollision Then
                'move forward
                myTank.Move(PreviousFrameTime, True)
            End If
            If keyboardState.Item(Key.S) And Not rearCollision Then
                'move backward
                myTank.Move(PreviousFrameTime, False)
            End If
        End If

        yaw *= PreviousFrameTime
        pitch *= PreviousFrameTime
        myTank.rotation += New Vector3(0, yaw, pitch)

        'move my tank to the top of the terrain
        Dim terrainY As Single = myTerrain.GetHeightAtPoint(myTank.position.X - 15, myTank.position.Z - 5)
        If Not Single.IsNaN(terrainY) Then
            terrainY += 6
            If myTank.position.Y <> terrainY Then
                myTank.position -= New Vector3(0, (myTank.position.Y - terrainY) / 4, 0)
            End If
        End If

        ' ----------- Bullet Events / Death ------------
        'update ally bullet
        If Not IsNothing(bullet) AndAlso bullet.GetActive() Then
            bullet.UpdatePosition(PreviousFrameTime)

            'check if my bullet has hit the enemy tank
            If Collidable.CheckCollision(enemyTank, bullet) Then
                enemyDead = True
            End If
            'if bullet is out the the terrain range - delete it
            terrainY = myTerrain.GetHeightAtPoint(bullet.position.X, bullet.position.Z)
            If Not Single.IsNaN(terrainY) Then
                If bullet.position.Y < terrainY Then
                    Console.WriteLine("The bullet is below the map.")
                    bullet.SetActive(False)
                End If
            Else
                bullet.SetActive(False)
            End If
        End If
        'update enemy bullet
        If Not IsNothing(enemyBullet) AndAlso enemyBullet.GetActive() Then
            enemyBullet.UpdatePosition(PreviousFrameTime)

            If Collidable.CheckCollision(myTank, enemyBullet) Then
                allyDead = True
            End If
            'if bullet is out the the terrain range - delete it
            terrainY = myTerrain.GetHeightAtPoint(enemyBullet.position.X, enemyBullet.position.Z)
            If Not Single.IsNaN(terrainY) Then
                If enemyBullet.position.Y < terrainY Then
                    Console.WriteLine("The bullet is below the map.")
                    enemyBullet.SetActive(False)
                End If
            Else
                enemyBullet.SetActive(False)
            End If
        End If

        If enemyDead Then
            enemyTank.position = enemySpawnPoint
            myFlagAttached = False
            enemyDead = False
        End If
        If allyDead Then
            myTank.position = mySpawnPoint
            enemyFlagAttached = False
            allyDead = False
        End If

        '------------- Flag Events ------------
        'reset my flag
        If Not myFlagAttached AndAlso Collidable.CheckCollision(myTank, myFlag) Then
            'if my tank is touching my flag and the enemy tank is not
            'sent my flag to its spawn
            myFlag.position = mySpawnPoint

            'if the enemy flag is attached to my tank and my tank is still colliding the flag after it has been reset
            If enemyFlagAttached AndAlso Collidable.CheckCollision(myTank, myFlag) Then
                'the ally scores a point.
                Scored = True
            End If
        End If
        'grab enemy flag
        If enemyFlagAttached OrElse Collidable.CheckCollision(myTank, enemyFlag) Then
            enemyFlagAttached = True
        Else
            enemyFlagAttached = False
        End If
        'attach flags to tanks
        If myFlagAttached Then
            myFlag.position = enemyTank.position
        End If
        If enemyFlagAttached Then
            enemyFlag.position = myTank.position
        End If


        '------ Move 3D Sound positions' 
        soundListener.Orientation = New DirectSound.Listener3DOrientation(myTank.myCamera.getFront(), myTank.myCamera.getTop())
        soundListener.Position = myTank.position

        engine3DAudio.Position = myTank.position
        Shoot3DAudio.Position = myTank.position

        enemyShoot3DAudio.Position = enemyTank.position
        enemyEngine3DAudio.Position = enemyTank.position
    End Sub
    Public Sub SendUpdates()
        'send the position and rotation of my tank 
        '----- the other user recieves this as the enemy tank
        Dim count As Integer = 1
        If Not sentMyBullet Then
            count += 1
        End If
        Dim tankArray(count) As dataWrapper
        Dim myPosWrapper As New dataWrapper With {
            .description = DataWrapperDescription.Position,
            .data = myTank.position}
        Dim myRotWrapper As New dataWrapper With {
            .description = DataWrapperDescription.Rotation,
            .data = myTank.rotation}

        tankArray(0) = myPosWrapper
        tankArray(1) = myRotWrapper
        'send the initial conditions of the bullet
        If Not sentMyBullet Then
            Dim RotationAndPosition(1) As Vector3
            RotationAndPosition(0) = myTank.rotation + New Vector3(0, 0, myTank.pitch)
            RotationAndPosition(1) = myTank.position

            Dim myBulletWrapper As New dataWrapper With {
                .description = DataWrapperDescription.Shoot,
                .data = RotationAndPosition
            }
            sentMyBullet = True
            tankArray(2) = myBulletWrapper
        End If

        Dim tankWrapper As New dataWrapper With {
            .description = DataWrapperDescription.Tank,
            .data = tankArray}
        NetworkModule.SendUDP(tankWrapper)

        'if the enemy flag is attached to me tell them that it is, else send the flag position 
        '----- the other user recieves this as the position of their flag or if their flag is attached
        count = 0
        If Not myFlagAttached Then
            count += 1
        End If

        Dim flagWrapperArray(count) As dataWrapper
        If Not enemyFlagAttached Then
            Dim myFlagWrapper As New dataWrapper With {
            .description = DataWrapperDescription.MyFlagPos,
            .data = enemyFlag.position}
            flagWrapperArray(0) = myFlagWrapper
        Else
            Dim myAttachedWrapper As New dataWrapper With {
                .description = DataWrapperDescription.AttachMyFlag,
                .data = enemyFlagAttached}
            flagWrapperArray(0) = myAttachedWrapper
        End If

        'if my flag is not attached to the enemy tank, send its position to the other user 
        '----- the other user recieves this as the position of the enemy flag - the flag that they can pick up

        If Not myFlagAttached Then
            Dim enemyFlagWrapper As New dataWrapper With {
            .description = DataWrapperDescription.EnemyFlagPos,
            .data = myFlag.position}
            flagWrapperArray(1) = enemyFlagWrapper
        End If

        Dim flagWrapper As New dataWrapper With {
            .description = DataWrapperDescription.Flag,
            .data = flagWrapperArray}
        NetworkModule.SendUDP(flagWrapper)

        'every 10 frames, sync the frame rates between the two computers
        If syncCounter Mod 11 = 0 Then
            syncCounter = 1
            Dim syncFPSwrapper As New dataWrapper With {
                .description = DataWrapperDescription.SyncFPS,
                .data = frameRate
            }
            Dim IPwrapper As New dataWrapper With {
                .description = DataWrapperDescription.IP,
                .data = {syncFPSwrapper}
            }
            NetworkModule.SendUDP(IPwrapper)
        Else
            syncCounter += 1
        End If

        '----- Send information about scores, ally deaths and enemy deaths.
        count = -1
        If Scored Then
            count += 1
        End If
        If enemyDead Then
            count += 1
        End If
        If allyDead Then
            count += 1
        End If
        If count <> -1 Then
            Dim keyEventsArray(count) As dataWrapper
            count = 0
            If Scored Then
                Dim scoredWrapper As New dataWrapper With {
                    .description = DataWrapperDescription.Scored,
                    .data = True}
                keyEventsArray(count) = scoredWrapper
                count += 1

            End If
            If enemyDead Then
                Dim enemyDeadWrapper As New dataWrapper With {
                    .description = DataWrapperDescription.Dead,
                    .data = enemyDead}
                keyEventsArray(count) = enemyDeadWrapper
                count += 1
            End If
            If allyDead Then
                Dim allyDeadWrapper As New dataWrapper With {
                    .description = DataWrapperDescription.EnemyDead,
                    .data = enemyDead}
                keyEventsArray(count) = allyDeadWrapper
                count += 1
            End If
            Dim keyEventsWrapper As New dataWrapper With {
                .description = DataWrapperDescription.KeyEvents,
                .data = keyEventsArray}
            NetworkModule.Send(keyEventsWrapper)
        End If
    End Sub
    Public Sub Render()
        'wipes screen, (removes artifacts)
        graphicsDevice.Clear(ClearFlags.Target Or ClearFlags.ZBuffer, Color.SkyBlue, 1.0F, 0)
        'sets up the camera
        graphicsDevice.SetTransform(TransformType.View, myTank.myCamera.View)

        'allows for rendering
        graphicsDevice.BeginScene()
        graphicsDevice.Lights(0).Update()
        graphicsDevice.Lights(1).Update()
        graphicsDevice.Lights(2).Update()


        'graphicsDevice.VertexFormat = CustomVertex.PositionNormalTextured.Format
        'graphicsDevice.SetStreamSource(0, testVbuffer, 0, CustomVertex.PositionNormalTextured.StrideSize)
        'graphicsDevice.Material = defaultMaterial
        'graphicsDevice.Indices = testIbuffer
        'graphicsDevice.SetRenderState(RenderStates.Lighting, False)
        ''graphicsDevice.SetRenderState(RenderStates.CullMode, Cull.None)
        'graphicsDevice.SetTexture(0, testTexture)
        'graphicsDevice.SetTransform(TransformType.World, Matrix.Identity)
        '
        'graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, testTerrain.NumberOfVertices, 0, testTerrain.NumberOfTriangles)


        'tanks
        myTank.Render()
        enemyTank.Render()
        'flags
        myFlag.Render()
        enemyFlag.Render()
        'terrain
        myTerrain.Render()

        'render all trees
        For Each t As Collidable In Trees
            t.Render()
        Next

        'if bullets exist - render them
        If Not IsNothing(bullet) Then
            bullet.Render()
        End If
        If Not IsNothing(enemyBullet) Then
            enemyBullet.Render()
        End If




        'render the user interface
        userInterface.Begin(SpriteFlags.AlphaBlend)

        Dim textlength As Single = Len(ScoreText) * ScoreFont.Description.Width * 0.5
        Dim textHeight As Integer = ScoreFont.Description.Height * 0.5
        ScoreFont.DrawText(userInterface,
                           ScoreText,
                           New Point(Screen.PrimaryScreen.Bounds.Width * 0.5 + textlength, Screen.PrimaryScreen.Bounds.Height / 6 + textHeight),
                           Color.ForestGreen)

        If shopOpen Then
            userInterface.Draw(shopScreen,
                               New Vector3(shopSize.Width * 0.5, shopSize.Height * 0.5, 0),
                               New Vector3(Screen.PrimaryScreen.Bounds.Width * 0.5, Screen.PrimaryScreen.Bounds.Height * 0.5, 0),
                               Color.White.ToArgb())

            Dim text As String = "SHOP"
            textlength = Len(text) * shopFont.Description.Width / 2
            textHeight = shopFont.Description.Height / 2
            shopFont.DrawText(userInterface,
                           text,
                           New Point(Screen.PrimaryScreen.Bounds.Width * 0.5 + textlength - 40, Screen.PrimaryScreen.Bounds.Height * 0.5 + textHeight - 400),
                           Color.Brown)

            text = "Press J to upgrade visibility."
            shopFont.DrawText(userInterface,
                           text,
                           New Point(Screen.PrimaryScreen.Bounds.Width * 0.5 - 560, Screen.PrimaryScreen.Bounds.Height * 0.5 - 300),
                           Color.Brown)

            text = "Press K to upgrade movement speed."
            shopFont.DrawText(userInterface,
                           text,
                           New Point(Screen.PrimaryScreen.Bounds.Width * 0.5 - 560, Screen.PrimaryScreen.Bounds.Height * 0.5 - 2.5 * textHeight - 300),
                           Color.Brown)

            text = "Press L to upgrade rotation speed."
            shopFont.DrawText(userInterface,
                           text,
                           New Point(Screen.PrimaryScreen.Bounds.Width * 0.5 - 560, Screen.PrimaryScreen.Bounds.Height * 0.5 - 5 * textHeight - 300),
                           Color.Brown)
        End If

        'stop rendering UI
        userInterface.End()
        'stop rendering
        graphicsDevice.EndScene()
        'displays the screen
        graphicsDevice.Present()
    End Sub

    '                   End Game Events                   '
    Public Sub DisposeDXObjects()
        'dispose of everything

        'peripherals
        If keyboard IsNot Nothing Then
            keyboard.Dispose()
        End If
        If mouse IsNot Nothing Then
            mouse.Dispose()
        End If

        'game objects
        If Not IsNothing(myTerrain) Then
            myTerrain.Dispose()
        End If
        If myTank IsNot Nothing Then
            myTank.Dispose()
        End If
        If enemyTank IsNot Nothing Then
            enemyTank.Dispose()
        End If
        If myFlag IsNot Nothing Then
            myFlag.Dispose()
        End If
        If enemyFlag IsNot Nothing Then
            enemyFlag.Dispose()
        End If
        If bullet IsNot Nothing Then
            bullet.Dispose()
            bullet = Nothing
        End If
        If enemyBullet IsNot Nothing Then
            enemyBullet.Dispose()
            enemyBullet = Nothing
        End If

        'chop down all the trees
        For Each t As Collidable In Trees
            If t IsNot Nothing Then
                t.Dispose()
                t = Nothing
            End If
        Next t
        isGameDisposed = True
    End Sub
    Public Sub Shutdown()
        NetworkModule.StopThreads()

        mainMenuForm.Close()
        mouseState = Nothing
        keyboardState = Nothing

        DisposeDXObjects()
        'dispose ui and then 3d space
        userInterface.Dispose()
        graphicsDevice.Dispose()

        Me.Dispose()
        Close()
    End Sub
    Private Sub TankGame_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        StopThreads()
        FirstRun = False
        GameOpen = False
    End Sub
    Private Sub TankGame_Deactivate(sender As Object, e As EventArgs) Handles Me.Deactivate
        isLost = True
    End Sub
    Private Sub TankGame_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        Me.Show()
        isLost = False
    End Sub
End Class