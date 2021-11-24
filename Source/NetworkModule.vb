Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Threading
Module NetworkModule
    '''
    '''The network module handles network events.
    '''It support UDP and TCP connection on ports defined below.
    '''One listening thread is created for each protocol.
    '''One sending thread is created for each send request.
    '''The module also retrieves the IPv4 address of an active network adapter,
    '''both in dotted-decimal notation and hex notation.
    '''
    ''' Everything sent is to be sent in a dataWrapper structure to ease
    ''' the identification process.
    '''

    Public sendToIP As String = ""
    Public connected As Boolean = False

    Public ReadOnly myPortTCP As Integer = 7676
    Private tcpServer As Sockets.TcpListener

    Public ReadOnly myPortUDP As Integer = 7675
    Private udpClient As Sockets.UdpClient
    Private udpServer As Sockets.UdpClient

    <SerializableAttribute()>
    Structure dataWrapper
        Dim description As DataWrapperDescription
        Dim data As Object
    End Structure
    Public Enum DataWrapperDescription
        Tank
        Position
        Rotation
        Shoot
        IP
        Success
        Failure
        Join
        SyncFPS
        Terrain
        Flag
        AttachMyFlag
        AttachEnemyFlag
        EnemyFlagPos
        MyFlagPos
        KeyEvents
        Scored
        Dead
        EnemyDead
        TreePosition
    End Enum

    '''                  Get IP Addresses in Different Forms            '''
    Public Function GetOwnIP() As String
        'returns the users own ip address in dotted-decimal notation
        Dim IPAddress As String = ""
        Dim allNICs() As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces()

        'if a network adapter is active and does not point to the loop back address
        ' check if it has been assigned an IPv4 address.
        ' if so, take this address.
        For Each nic In allNICs
            If nic.OperationalStatus = OperationalStatus.Up AndAlso nic.NetworkInterfaceType <> NetworkInterfaceType.Loopback Then
                For Each address In nic.GetIPProperties().UnicastAddresses
                    'get IPv4 address
                    If address.Address.AddressFamily = Sockets.AddressFamily.InterNetwork Then

                        IPAddress = address.Address.ToString()
                    End If
                Next address
            End If
        Next nic

        If IPAddress = "" Then
            MessageBox.Show("Please connect to a network. You may need to temporarily disable other NICs.")
            IPAddress = "00.00.00.00"
        End If

        Return IPAddress
    End Function
    Public Function GetHexIP() As String
        'returns the users own IP address as hexadecimal
        Dim IP As String = GetOwnIP()
        Dim hexIP As String = IPtoHex(IP)
        Return hexIP
    End Function
    Public Function IPtoHex(ByVal IPAddress As String) As String
        'Convert from dotted-decimal to hex notation.
        Dim bytes() As String = IPAddress.Split(".")
        Dim finalString As String = ""

        For index = 0 To bytes.Count - 1
            Dim num As New NumberConverter(bytes(index), 10)

            bytes(index) = num.Value(16)
            While Len(bytes(index)) < 2
                bytes(index) = "0" + bytes(index)
            End While


            finalString += bytes(index)
        Next index

        Return finalString
    End Function
    Public Function HexToIP(ByVal hex As String) As String
        'Convert from hex to dotted-decimal notation.
        If Len(hex) = 8 Then
            Dim converter As New NumberConverter()
            Dim hexList(3) As String
            Dim ip As String = ""
            'select each byte
            For index = 0 To Len(hex) - 1 Step 2
                hexList(index / 2) = hex(index) + hex(index + 1)
            Next index
            'convert each byte
            For hexByte = 0 To hexList.Count() - 1
                ip += converter.ConvertBase(hexList(hexByte), 16, 10)
                If hexByte <> hexList.Count() - 1 Then
                    ip += "."
                End If
            Next hexByte
            Return ip
        Else
            Throw New IndexOutOfRangeException("Hex does not have a length of 8.")
        End If
    End Function

    '''                      Send and Recieve Using TCP                 '''
    Public Sub Send(ByVal data As dataWrapper)
        'data is sent on a seperate thread to prevent the main thread freezing up
        Dim myThread As Thread
        myThread = New Thread(AddressOf SendThread)
        myThread.Priority = ThreadPriority.Highest
        myThread.Start(data)
    End Sub
    Private Sub SendThread(ByVal data As dataWrapper)
        'send data over a network to a specified socket
        Try
            'data is sent by the writing the binary representation of the dataWrapper
            'to the network stream
            Dim tcpClient As New Sockets.TcpClient(sendToIP, myPortTCP)
            Dim netStream As Sockets.NetworkStream = tcpClient.GetStream()
            Dim binaryConverter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()

            binaryConverter.Serialize(netStream, data)

            netStream.Close()
            tcpClient.Close()
        Catch ConnectionClosed As System.IO.IOException
            Console.Write("TCP: Exception when sending. Connection closed. System.IO.IOException" + ConnectionClosed.ToString())
        Catch ArgNull As ArgumentNullException
            Console.Write("TCP: Exception when sending. ArgumentNullException" + ArgNull.ToString())
        Catch SocketExp As Net.Sockets.SocketException
            Console.Write("TCP: Exception when sending. SocketException" + SocketExp.ToString())
        End Try
    End Sub
    Public Sub Recieve()
        'data is recieved on a seperate thread to prevent the main thread freezing up
        Dim myThread As Threading.Thread
        myThread = New Threading.Thread(AddressOf RecieveThread)
        myThread.Priority = ThreadPriority.Highest
        myThread.Start()
    End Sub
    Private Sub RecieveThread()
        'recieve data over a network by listening on a specified port
        Try
            Dim endpoint As IPEndPoint
            endpoint = New IPEndPoint(IPAddress.Any, myPortTCP)

            tcpServer = New Sockets.TcpListener(endpoint)
            tcpServer.Start()

            Dim binaryConverter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            While True
                'accepting a TCP client freezes the thread until data is sent
                'to the port, or until the server is closed
                Dim tcpClient As Sockets.TcpClient = tcpServer.AcceptTcpClient()
                'convert data from its binary representation to a dataWrapper
                Dim netStream As Sockets.NetworkStream = tcpClient.GetStream()
                Dim myObject As Object = binaryConverter.Deserialize(netStream)
                Try
                    Dim recievedData As dataWrapper = CType(myObject, dataWrapper)
                    HandleRecievedData(recievedData)
                Catch castEx As System.InvalidCastException
                    'defensive programming
                    'only objects in a dataWrapper structure should be sent, so only dataWrapper structures should be recieved
                    Console.WriteLine("TCP: InvalidCastException was thrown when recieving packet." & castEx.ToString())
                End Try

                tcpClient.Close()
                netStream.Close()
            End While

        Catch ex As Exception
            Console.WriteLine("Exception when recieving." + ex.ToString())
        Finally
            tcpServer.Stop()
        End Try
    End Sub

    '''                     Send and Recieve Using UDP                  '''
    Public Sub SendUDP(ByVal data As dataWrapper)
        'data is sent on a seperate thread to prevent the main thread freezing up
        Dim myThread As Threading.Thread
        myThread = New Threading.Thread(AddressOf SendUDPThread)
        myThread.Priority = ThreadPriority.Highest
        myThread.Start(data)
    End Sub
    Private Sub SendUDPThread(ByVal data As dataWrapper)
        Try
            'serialise the data wrapper into bytes
            Dim binaryStream As New IO.MemoryStream()
            Dim binaryConverter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()

            binaryConverter.Serialize(binaryStream, data)
            Dim sendBytes() As Byte = binaryStream.ToArray()

            'send the bytes to the paired device
            udpClient = New Sockets.UdpClient(sendToIP, myPortUDP)
            udpClient.Send(sendBytes, sendBytes.Length)
        Catch ex As Exception
            Console.WriteLine("UDP: Exception when sending." & ex.ToString())
        End Try


    End Sub
    Public Sub RecieveUDP()
        'data is recieved on a seperate thread to prevent the main thread freezing up
        Dim myThread As Threading.Thread
        myThread = New Threading.Thread(AddressOf RecieveUDPThread)
        myThread.Priority = ThreadPriority.Highest
        myThread.Start()
    End Sub
    Private Sub RecieveUDPThread()
        'listen for UDP connections on a given port.
        Dim UDPendPoint As IPEndPoint = New IPEndPoint(IPAddress.Any, myPortUDP)
        udpServer = New Sockets.UdpClient(UDPendPoint)

        Dim binaryConverter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        'only recieve if the UDP client exists
        While Not IsNothing(udpServer)
            Try
                Dim data() As Byte
                'freeze the thread until a datagram is recieved
                data = udpServer.Receive(UDPendPoint)

                'convert data to a memory stream and deserialise it to get a dataWrapper
                Dim memStream As New IO.MemoryStream(data)
                Dim myObject As Object = binaryConverter.Deserialize(memStream)
                Dim recievedData As dataWrapper = CType(myObject, dataWrapper)
                'identify the data.
                HandleRecievedData(recievedData)
            Catch castEx As System.InvalidCastException
                'defensive programming
                'only objects in a dataWrapper structure should be sent, so only dataWrapper structures should be recieved
                Console.WriteLine("InvalidCastException was thrown when recieving packet." & castEx.ToString())
            Catch ex As Exception
                Console.WriteLine(ex.ToString())
            End Try
        End While
    End Sub

    '''                      Handle recieved data based on the description of the data wrapper                  '''
    Public Sub HandleRecievedData(ByRef recievedData As dataWrapper)
        Select Case recievedData.description

            'I have introduced branches here to optimise the number of comparisons being made.
            'This is easily comparable to the JSON format.

            '''                  Manage Tank Related Packets                '''
            Case DataWrapperDescription.Tank
                For Each wrapper As dataWrapper In recievedData.data
                    Select Case wrapper.description
                        Case DataWrapperDescription.Position
                            'if tank position has been recieved
                            If Not TankGame.enemyDead Then
                                TankGame.enemyTank.position = CType(wrapper.data, Microsoft.DirectX.Vector3)
                            End If
                        Case DataWrapperDescription.Rotation
                            'ff tank rotation has been recieved, update the enemy tank rotation
                            TankGame.enemyTank.rotation = CType(wrapper.data, Microsoft.DirectX.Vector3)
                        Case DataWrapperDescription.Shoot
                            'if the enemy shot a bullet, create one next frame
                            TankGame.enemyBulletDetails = CType(wrapper.data, Microsoft.DirectX.Vector3())
                            TankGame.enemyBulletPending = True
                        Case Else
                            'defensive programming
                            'errors can easily occur over a network
                            Console.WriteLine("Unknown data type has been recieved: " & wrapper.description & wrapper.data.ToString())
                    End Select
                Next wrapper
            '''                      Manage Connections                     '''
            Case DataWrapperDescription.IP
                For Each wrapper As dataWrapper In recievedData.data
                    Select Case wrapper.description
                        Case DataWrapperDescription.SyncFPS
                            TankGame.enemyFPS = CType(wrapper.data, Double)
                        Case DataWrapperDescription.Success
                            'the other user accepted the join request
                            MessageBox.Show("You successfully paired with " & IPtoHex(sendToIP) & "!")
                            connected = True
                        Case DataWrapperDescription.Failure
                            'the other user declined the join request
                            MessageBox.Show(IPtoHex(sendToIP) & " declinded your request to join.")
                            connected = False
                            sendToIP = ""
                        Case DataWrapperDescription.Join
                            'if a join requst has been sent, the data of the data wrapper will the an IP address.
                            Dim result As MsgBoxResult = MsgBox("Would you like to play a game with: " & IPtoHex(CType(wrapper.data, String)) & "?", MsgBoxStyle.YesNo, "Pairing")
                            Dim IPwrapper As New dataWrapper With {
                                .description = DataWrapperDescription.IP}
                            Dim response As New dataWrapper With {
                                .data = Nothing}
                            'reply to the join request
                            Select Case result
                                'accept by sending a success flag
                                Case MsgBoxResult.Yes
                                    sendToIP = CType(wrapper.data, String)

                                    response.description = DataWrapperDescription.Success
                                    IPwrapper.data = {response}
                                    Send(IPwrapper)
                                    connected = True
                                'reject by sending a fail flag
                                Case MsgBoxResult.No
                                    response.description = DataWrapperDescription.Failure
                                    IPwrapper.data = {response}
                                    Send(IPwrapper)
                                    connected = False
                                Case Else
                                    'defensive programming
                                    'program could be closed
                                    connected = False
                                    Console.WriteLine("Unknown response from message box.")
                            End Select
                        Case Else
                            'defensive programming
                            'errors can easily occur over a network
                            Console.WriteLine("Unknown data type has been recieved: " & wrapper.description & wrapper.data.ToString())
                    End Select
                Next wrapper
            '''                      Manage Flag Events                     '''
            Case DataWrapperDescription.Flag
                For Each wrapper As dataWrapper In recievedData.data
                    Select Case wrapper.description
                        Case DataWrapperDescription.AttachMyFlag
                            'attatch ally flag to the enemy tank
                            TankGame.myFlagAttached = CType(wrapper.data, Boolean)
                        Case DataWrapperDescription.AttachEnemyFlag
                            'attatch enemy flag to the ally tank
                            TankGame.enemyFlagAttached = CType(wrapper.data, Boolean)
                        Case DataWrapperDescription.MyFlagPos
                            'if ally flag position has been recieved, drop it
                            TankGame.myFlag.position = CType(wrapper.data, Microsoft.DirectX.Vector3)
                            TankGame.myFlagAttached = False
                        Case DataWrapperDescription.EnemyFlagPos
                            'if enemy flag position has been recieved, drop it
                            TankGame.enemyFlag.position = CType(wrapper.data, Microsoft.DirectX.Vector3)
                            TankGame.enemyFlagAttached = False
                        Case Else
                            'defensive programming
                            'errors can easily occur over a network
                            Console.WriteLine("Unknown data type has been recieved: " & wrapper.description & wrapper.data.ToString())
                    End Select
                Next wrapper
            '''                Manage Important Game Events                 '''
            Case DataWrapperDescription.KeyEvents
                For Each wrapper As dataWrapper In recievedData.data
                    Select Case wrapper.description
                        Case DataWrapperDescription.Dead
                            'respawn the ally tank
                            TankGame.allyDead = CType(wrapper.data, Boolean)
                        Case DataWrapperDescription.EnemyDead
                            'respawn the enemy tank
                            TankGame.enemyDead = CType(wrapper.data, Boolean)
                        Case DataWrapperDescription.Scored
                            'if score value has been recieved, the enemy has scored
                            TankGame.RecievedScore = CType(wrapper.data, Boolean)
                        Case Else
                            'defensive programming
                            'errors can easily occur over a network
                            Console.WriteLine("Unknown data type has been recieved: " & wrapper.description & wrapper.data.ToString())
                    End Select
                Next wrapper
            Case DataWrapperDescription.Terrain
                'if terrain data has been recieved, start the game
                MainMenu.firstPlayer = False
                MainMenu.terrainPoints = CType(recievedData.data, MyMatrix)
            Case DataWrapperDescription.TreePosition
                'this determines where the trees are placed.
                MainMenu.treePos = CType(recievedData.data, Microsoft.DirectX.Vector3())
            Case Else
                'defensive programming
                'errors can easily occur over a network
                Console.WriteLine("Unknown data type has been recieved: " & recievedData.description & recievedData.data.ToString())
        End Select

    End Sub

    '''                  Stop the threads runnning in the background - used when closing the program            '''
    Public Sub StopThreads()
        StopUDP()
        StopTCP()
    End Sub
    Public Sub StopUDP()
        If Not IsNothing(udpClient) Then
            udpClient.Close()
            udpClient = Nothing

        End If
        If Not IsNothing(udpServer) Then
            udpServer.Close()
            udpServer = Nothing
        End If
    End Sub
    Public Sub StopTCP()
        If Not IsNothing(tcpServer) Then
            tcpServer.Stop()
        End If
    End Sub
End Module
