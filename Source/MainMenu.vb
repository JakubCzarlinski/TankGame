Imports System.ComponentModel
Imports Microsoft.DirectX
Imports Microsoft.DirectX.Direct3D
Public Class MainMenu
    '''
    '''The main menu creates a user interface that enables users
    '''to play join a game or create a game. They can create a game
    '''by either loading or generating terrain and then waiting for
    '''another player to join their game by using their game-code
    '''(local IPv4 Address in hexadecimal) to join.
    '''

    'these control the the current menu and keeps track of the previous menus
    Private previousMenu As New ArrayList()
    Private _menu As String = "main"

    'these are used to make the form draggable
    Private mDown As Boolean = False
    Private previousLocation As Point

    'this is opened once a terrain has been decided on and both players are connected to eachother.
    Public gameWindow As TankGame
    Public Shared terrainPoints As MyMatrix = Nothing
    Public Shared treePos() As Vector3 = Nothing
    Public Shared firstPlayer As Boolean = True

    '''                  Manage Current Menu                  '''
    Private Property menu As String
        'Changing the value of menu will deactive the old menu and activate the new menu.
        Get

            Return _menu
        End Get
        Set(value As String)
            'turn off previous menu
            Select Case _menu
                Case "main"
                    'remove main menu
                    CreateGameBTN.Visible = False
                    JoinGameBTN.Visible = False

                Case "create"
                    'remove create game menu
                    GenerateBTN.Visible = False
                    LoadBTN.Visible = False
                    GameCodeLbl.Visible = False

                Case "load"
                    'remove load game menu
                    LoadBTN.Visible = False

                Case "join"
                    'remove join game menu
                    IPCodeBack.Visible = False
                    IPCodeText.Visible = False

                Case "start"
                    'remove start game menu
                    GameCodeLbl.Visible = False
                    StartGameBtn.Visible = False
                Case Else
                    'defensive programming
                    Console.WriteLine("Exiting wrong Menu.")
            End Select


            'turn on the new menu
            Select Case value
                Case "main"
                    'enter the main menu
                    BigLabel.Text = "Tank Game"
                    CreateGameBTN.Visible = True
                    JoinGameBTN.Visible = True

                Case "create"
                    'enter the create geame menu
                    BigLabel.Text = "Create Game"
                    GameCodeLbl.Text = "Code: " & NetworkModule.GetHexIP()
                    GenerateBTN.Visible = True
                    LoadBTN.Visible = True
                    GameCodeLbl.Visible = True
                    CentreControlX(GameCodeLbl)

                Case "load"
                    'enter the load game menu
                    BigLabel.Text = "Load Game"
                    LoadBTN.Visible = True
                Case "join"
                    'enter the join game menu
                    BigLabel.Text = "Join Game"
                    IPCodeBack.Visible = True
                    IPCodeText.Visible = True
                    CentreControlX(IPCodeText)

                Case "start"
                    'enter the start game menu
                    BigLabel.Text = "Start Game"

                    GameCodeLbl.Visible = True
                    StartGameBtn.Visible = True
                    CentreControlX(StartGameBtn)

                Case Else
                    'defensive programming
                    Console.WriteLine("Entering wrong Menu")
            End Select
            CentreControlX(BigLabel)
            'add the previous menu to the list so that the user can go back.
            previousMenu.Add(_menu)
            _menu = value
        End Set
    End Property
    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        'centre the menu to the screen
        StartPosition = FormStartPosition.CenterScreen
        'centre buttons and lables to the centre of the form
        CentreControlX(BigLabel)
        CentreControlX(CreateGameBTN)
        CentreControlX(JoinGameBTN)

        'enable recieving using TCP
        NetworkModule.Recieve()

    End Sub

    '''                       Main Menu                       '''
    Private Sub CreateGameBTN_Click(sender As Object, e As EventArgs) Handles CreateGameBTN.Click
        'go to the create game menu
        menu = "create"
    End Sub
    Private Sub JoinGameBTN_Click(sender As Object, e As EventArgs) Handles JoinGameBTN.Click
        'go to the join game menu
        menu = "join"
    End Sub

    '''                   Create Game Menu                    '''
    Private Sub GenerateBTN_Click(sender As Object, e As EventArgs) Handles GenerateBTN.Click
        'generate the terrain for the game
        menu = "start"

        'generate a terrain matrix
        Dim myTerrain As New Terrain(8, 10, 120, Nothing)
        myTerrain.GenerateMatrix(1.7, 0.8)
        'create a kernel which finds the average of the points surround a given point
        Dim kernel As New MyMatrix(4, 4)
        kernel.SetAllEntires(1 / 16)
        'complete a convolution over the matrix, in order to smoothen the terrain and then generate the terrain
        myTerrain.points = MyMatrix.Convolution(myTerrain.points, kernel)
        terrainPoints = myTerrain.points

        'if a terrain folder does not exits, make it
        If Not IO.Directory.Exists("terrains/") Then
            IO.Directory.CreateDirectory("terrains/")
        End If

        'make the terrain into a serialised file
        Dim file As New IO.FileStream("terrains/terrainMadeAt" & Date.Now.ToString("HH_mm_ss__dd_MM_yyyy") & ".terrain",
                                      IO.FileMode.Create,
                                      IO.FileAccess.Write)
        Dim binaryFormatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        binaryFormatter.Serialize(file, terrainPoints)

        file.Close()
        firstPlayer = True
    End Sub
    Private Sub LoadBTN_Click(sender As Object, e As EventArgs) Handles LoadBTN.Click
        'load terrain from drive
        menu = "load"
        LoadFile.Filter = "Terrain File |*.terrain|All Files |*.*"
        LoadFile.ShowDialog()

        Dim terrainFile As String = LoadFile.FileName
        If terrainFile <> "" Then

            Try
                'open a terrain file into a stream and deserialise it
                Dim FileIOStream As IO.Stream = LoadFile.OpenFile()
                Dim binaryFormatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                Dim terrainPointsFile As Object = binaryFormatter.Deserialize(FileIOStream)
                FileIOStream.Close()

                'if it is a valid matrix object, acknowledge it
                If terrainPointsFile.GetType() Is New MyMatrix(0, 0).GetType() Then
                    terrainPoints = CType(terrainPointsFile, MyMatrix)
                    firstPlayer = True
                    menu = "start"
                End If
            Catch ex As Exception
                MessageBox.Show("Please load a valid terrain file.")
            End Try
        End If

    End Sub

    '''                    Join Game Menu                     '''
    Private Sub IPCodeText_MouseDown(sender As Object, e As MouseEventArgs) Handles IPCodeText.MouseDown
        'clear the text when the textbox is clicked on
        IPCodeText.Text = ""
    End Sub
    Private Sub IPCodeText_KeyUp(sender As Object, e As KeyEventArgs) Handles IPCodeText.KeyUp
        'if the user pressed enter send the ip address
        If e.KeyCode = Keys.Enter Then
            'the IP address cannot be the user's own IP address
            If IPCodeText.Text <> NetworkModule.GetHexIP() Then 'AndAlso IPCodeText.Text <> "7F000001" Then
                Try
                    'send own IP address 
                    NetworkModule.sendToIP = NetworkModule.HexToIP(IPCodeText.Text)

                    Dim joinRequest As New dataWrapper With {
                        .data = NetworkModule.GetOwnIP(),
                        .description = DataWrapperDescription.Join
                    }
                    Dim IPWrapper As New dataWrapper With {
                        .description = DataWrapperDescription.IP,
                        .data = {joinRequest}
                    }
                    NetworkModule.Send(IPWrapper)
                Catch ex As System.IndexOutOfRangeException
                    Console.WriteLine("Ensure the game code is 8 characters long and is valid.")
                End Try
            End If

        End If
    End Sub

    '''                   Start Game Menu                     '''
    Private Sub StartGameBtn_Click(sender As Object, e As EventArgs) Handles StartGameBtn.Click
        'start the game on both computers
        If NetworkModule.connected Then
            'send the terrain over TCP to the other player
            Dim terrainData As New dataWrapper With {
                .description = DataWrapperDescription.Terrain,
                .data = terrainPoints
            }
            NetworkModule.Send(terrainData)


            'generate and array of between 10 and 40 tree positions 
            'that will be sent to the connected user

            Dim randomGen As New Random()
            Dim randomNumber As Integer = randomGen.Next(10, 40)
            Dim X As Integer
            Dim Y As Single
            Dim Z As Integer
            Dim min As Integer = terrainPoints.rows * 0.1
            Dim max As Integer = terrainPoints.rows * 0.9
            Dim treePositions As New List(Of Vector3)


            For Index = 0 To randomNumber
                Do
                    'generate a position for the tree that is not in water
                    ' and is not in the mountains.
                    X = randomGen.Next(min, max)
                    Z = randomGen.Next(min, max)
                    Y = terrainPoints.GetEntry(Z, X)

                Loop Until BoundingBox.IsBetween(0.3, 0.9, Y)
                'scale the indexes to be inline with the terrain.
                X *= 10
                Y *= 120
                Z *= 10

                treePositions.Add(New Vector3(X, Y, Z))
            Next Index
            treePos = treePositions.ToArray()

            Dim treePosData As New dataWrapper With {
                .description = DataWrapperDescription.TreePosition,
                .data = treePos
            }
            NetworkModule.Send(treePosData)



            Me.Hide()

            CheckRecievedTimer.Enabled = False
            gameWindow = New TankGame(Me, terrainPoints)
        Else
            MessageBox.Show("Please wait for a player to connect.")
        End If

    End Sub
    Private Sub CheckRecievedTimer_Tick(sender As Object, e As EventArgs) Handles CheckRecievedTimer.Tick
        'if the user is connected, and did not create the game, and has already recieved the terrain values, start the game.
        If NetworkModule.connected AndAlso Not firstPlayer AndAlso Not IsNothing(terrainPoints) Then
            'stop the timer from running and open the game
            CheckRecievedTimer.Enabled = False
            Me.Hide()

            gameWindow = New TankGame(Me, terrainPoints)
        End If
    End Sub

    '''                  Generic Form Actions                  '''
    Private Sub ExitBTN_Click(sender As Object, e As EventArgs) Handles ExitBTN.Click
        'trigger the close event when the user closes the game.
        MyBase.Close()
    End Sub
    Private Sub MainMenu_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        'stop all network threads before closing.
        NetworkModule.StopThreads()
    End Sub
    Private Sub BackBTN_Click(sender As Object, e As EventArgs) Handles BackBTN.Click
        'when the back button has been pressed go to the previous menu
        If previousMenu.Count > 0 Then
            menu = previousMenu(previousMenu.Count - 1)
            'setting a value adds to previousMenu so the last two values of the list have to be removed
            previousMenu.RemoveAt(previousMenu.Count - 1)
            previousMenu.RemoveAt(previousMenu.Count - 1)
        Else
            Console.WriteLine("Previous Menu buffer is empty. The user is in the main menu.")
        End If
    End Sub
    Private Sub TopBarPB_MouseDown(sender As Object, e As MouseEventArgs) Handles TopBarPB.MouseDown
        'this tracks the first position of mouse down event
        ' used for dragging the screen
        mDown = True
        previousLocation = e.Location
    End Sub
    Private Sub TopBarPB_MouseMove(sender As Object, e As MouseEventArgs) Handles TopBarPB.MouseMove
        If mDown Then
            'if the window is being dragged by the header, move it but prevent it from going off screen
            Dim change As Point

            Dim x As Integer = e.Location.X - previousLocation.X
            Dim y As Integer = e.Location.Y - previousLocation.Y

            If x + MyBase.Location.X < 100 - MyBase.Width Then
                'if form goes too far left it will be limited
                x = -MyBase.Location.X + 100 - MyBase.Width
            ElseIf x + MyBase.Location.X > Screen.PrimaryScreen.Bounds.Width - 100 Then
                'if form goes too far right it will be limited
                x = -MyBase.Location.X + Screen.PrimaryScreen.Bounds.Width - 100
            End If

            If y + MyBase.Location.Y < 0 Then
                'if form goes too far up it will be limited
                y = -MyBase.Location.Y
            ElseIf y + MyBase.Location.Y > Screen.PrimaryScreen.Bounds.Height - 100 Then
                'if form goes too far down it will be limited
                y = -MyBase.Location.Y + Screen.PrimaryScreen.Bounds.Height - 100
            End If

            change = New Point(x, y)
            MyBase.Location += change
        End If

    End Sub
    Private Sub TopBarPB_MouseUp(sender As Object, e As MouseEventArgs) Handles TopBarPB.MouseUp
        'stops dragging
        mDown = False
    End Sub
    Private Sub CentreControlX(ByRef cont As Control)
        'centre control to form in the x direction
        Dim xPos As Integer = MyBase.Size.Width - cont.Size.Width
        xPos /= 2

        Dim newPoint As New Point(xPos, cont.Location.Y)
        cont.Location = newPoint
    End Sub
End Class
