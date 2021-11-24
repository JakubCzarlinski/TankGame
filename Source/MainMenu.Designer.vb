<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainMenu
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.CreateGameBTN = New System.Windows.Forms.Button()
        Me.JoinGameBTN = New System.Windows.Forms.Button()
        Me.BigLabel = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.PictureBox3 = New System.Windows.Forms.PictureBox()
        Me.ExitBTN = New System.Windows.Forms.Button()
        Me.NameLabel = New System.Windows.Forms.Label()
        Me.BackBTN = New System.Windows.Forms.Button()
        Me.TopBarPB = New System.Windows.Forms.PictureBox()
        Me.LoadFile = New System.Windows.Forms.OpenFileDialog()
        Me.CheckRecievedTimer = New System.Windows.Forms.Timer(Me.components)
        Me.IPCodeBack = New System.Windows.Forms.PictureBox()
        Me.GenerateBTN = New System.Windows.Forms.Button()
        Me.GameCodeLbl = New System.Windows.Forms.Label()
        Me.LoadBTN = New System.Windows.Forms.Button()
        Me.StartGameBtn = New System.Windows.Forms.Button()
        Me.IPCodeText = New System.Windows.Forms.TextBox()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.TopBarPB, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.IPCodeBack, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'CreateGameBTN
        '
        Me.CreateGameBTN.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.CreateGameBTN.FlatAppearance.BorderSize = 0
        Me.CreateGameBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.CreateGameBTN.Font = New System.Drawing.Font("Arial Black", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CreateGameBTN.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.CreateGameBTN.Location = New System.Drawing.Point(11, 227)
        Me.CreateGameBTN.Name = "CreateGameBTN"
        Me.CreateGameBTN.Size = New System.Drawing.Size(222, 60)
        Me.CreateGameBTN.TabIndex = 0
        Me.CreateGameBTN.Text = "CREATE GAME"
        Me.CreateGameBTN.UseVisualStyleBackColor = True
        '
        'JoinGameBTN
        '
        Me.JoinGameBTN.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.JoinGameBTN.FlatAppearance.BorderSize = 0
        Me.JoinGameBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.JoinGameBTN.Font = New System.Drawing.Font("Arial Black", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.JoinGameBTN.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.JoinGameBTN.Location = New System.Drawing.Point(11, 313)
        Me.JoinGameBTN.Name = "JoinGameBTN"
        Me.JoinGameBTN.Size = New System.Drawing.Size(222, 60)
        Me.JoinGameBTN.TabIndex = 1
        Me.JoinGameBTN.Text = "JOIN GAME"
        Me.JoinGameBTN.UseVisualStyleBackColor = True
        '
        'BigLabel
        '
        Me.BigLabel.AutoSize = True
        Me.BigLabel.BackColor = System.Drawing.Color.MediumTurquoise
        Me.BigLabel.Font = New System.Drawing.Font("Microsoft Sans Serif", 72.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BigLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.BigLabel.Location = New System.Drawing.Point(76, 60)
        Me.BigLabel.Name = "BigLabel"
        Me.BigLabel.Size = New System.Drawing.Size(545, 108)
        Me.BigLabel.TabIndex = 2
        Me.BigLabel.Text = "Tank Game"
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.PictureBox1.Enabled = False
        Me.PictureBox1.ErrorImage = Nothing
        Me.PictureBox1.Location = New System.Drawing.Point(-8, 621)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(718, 84)
        Me.PictureBox1.TabIndex = 3
        Me.PictureBox1.TabStop = False
        '
        'PictureBox2
        '
        Me.PictureBox2.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.PictureBox2.Enabled = False
        Me.PictureBox2.ErrorImage = Nothing
        Me.PictureBox2.Location = New System.Drawing.Point(-8, 463)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(718, 84)
        Me.PictureBox2.TabIndex = 4
        Me.PictureBox2.TabStop = False
        '
        'PictureBox3
        '
        Me.PictureBox3.BackColor = System.Drawing.Color.Green
        Me.PictureBox3.Enabled = False
        Me.PictureBox3.ErrorImage = Nothing
        Me.PictureBox3.Location = New System.Drawing.Point(-8, 542)
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.Size = New System.Drawing.Size(718, 84)
        Me.PictureBox3.TabIndex = 5
        Me.PictureBox3.TabStop = False
        '
        'ExitBTN
        '
        Me.ExitBTN.BackColor = System.Drawing.Color.Red
        Me.ExitBTN.FlatAppearance.BorderSize = 0
        Me.ExitBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.ExitBTN.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ExitBTN.Location = New System.Drawing.Point(656, 12)
        Me.ExitBTN.Name = "ExitBTN"
        Me.ExitBTN.Size = New System.Drawing.Size(32, 32)
        Me.ExitBTN.TabIndex = 6
        Me.ExitBTN.Text = "X"
        Me.ExitBTN.UseVisualStyleBackColor = True
        '
        'NameLabel
        '
        Me.NameLabel.AutoSize = True
        Me.NameLabel.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.NameLabel.Enabled = False
        Me.NameLabel.Location = New System.Drawing.Point(13, 675)
        Me.NameLabel.Name = "NameLabel"
        Me.NameLabel.Size = New System.Drawing.Size(131, 13)
        Me.NameLabel.TabIndex = 7
        Me.NameLabel.Text = "Made By: Jakub Czarlinski"
        '
        'BackBTN
        '
        Me.BackBTN.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.BackBTN.FlatAppearance.BorderSize = 0
        Me.BackBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.BackBTN.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BackBTN.Location = New System.Drawing.Point(12, 12)
        Me.BackBTN.Name = "BackBTN"
        Me.BackBTN.Size = New System.Drawing.Size(32, 32)
        Me.BackBTN.TabIndex = 9
        Me.BackBTN.Text = "<"
        Me.BackBTN.UseVisualStyleBackColor = True
        '
        'TopBarPB
        '
        Me.TopBarPB.BackColor = System.Drawing.Color.DarkCyan
        Me.TopBarPB.ErrorImage = Nothing
        Me.TopBarPB.Location = New System.Drawing.Point(-8, -7)
        Me.TopBarPB.Name = "TopBarPB"
        Me.TopBarPB.Size = New System.Drawing.Size(718, 64)
        Me.TopBarPB.TabIndex = 10
        Me.TopBarPB.TabStop = False
        '
        'LoadFile
        '
        Me.LoadFile.Title = "Load Map"
        '
        'CheckRecievedTimer
        '
        Me.CheckRecievedTimer.Enabled = True
        '
        'IPCodeBack
        '
        Me.IPCodeBack.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.IPCodeBack.Enabled = False
        Me.IPCodeBack.ErrorImage = Nothing
        Me.IPCodeBack.Location = New System.Drawing.Point(239, 227)
        Me.IPCodeBack.Name = "IPCodeBack"
        Me.IPCodeBack.Size = New System.Drawing.Size(222, 60)
        Me.IPCodeBack.TabIndex = 12
        Me.IPCodeBack.TabStop = False
        Me.IPCodeBack.Visible = False
        '
        'GenerateBTN
        '
        Me.GenerateBTN.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.GenerateBTN.FlatAppearance.BorderSize = 0
        Me.GenerateBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.GenerateBTN.Font = New System.Drawing.Font("Arial Black", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GenerateBTN.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.GenerateBTN.Location = New System.Drawing.Point(239, 313)
        Me.GenerateBTN.Name = "GenerateBTN"
        Me.GenerateBTN.Size = New System.Drawing.Size(222, 60)
        Me.GenerateBTN.TabIndex = 8
        Me.GenerateBTN.Text = "GENERATE"
        Me.GenerateBTN.UseVisualStyleBackColor = False
        Me.GenerateBTN.Visible = False
        '
        'GameCodeLbl
        '
        Me.GameCodeLbl.AutoSize = True
        Me.GameCodeLbl.BackColor = System.Drawing.Color.MediumTurquoise
        Me.GameCodeLbl.Font = New System.Drawing.Font("Microsoft Sans Serif", 32.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GameCodeLbl.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.GameCodeLbl.Location = New System.Drawing.Point(219, 168)
        Me.GameCodeLbl.Name = "GameCodeLbl"
        Me.GameCodeLbl.Size = New System.Drawing.Size(262, 51)
        Me.GameCodeLbl.TabIndex = 2
        Me.GameCodeLbl.Text = "AABBCCDD"
        Me.GameCodeLbl.Visible = False
        '
        'LoadBTN
        '
        Me.LoadBTN.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.LoadBTN.FlatAppearance.BorderSize = 0
        Me.LoadBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.LoadBTN.Font = New System.Drawing.Font("Arial Black", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LoadBTN.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.LoadBTN.Location = New System.Drawing.Point(239, 227)
        Me.LoadBTN.Name = "LoadBTN"
        Me.LoadBTN.Size = New System.Drawing.Size(222, 60)
        Me.LoadBTN.TabIndex = 11
        Me.LoadBTN.Text = "LOAD"
        Me.LoadBTN.UseVisualStyleBackColor = True
        Me.LoadBTN.Visible = False
        '
        'StartGameBtn
        '
        Me.StartGameBtn.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.StartGameBtn.FlatAppearance.BorderSize = 0
        Me.StartGameBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.StartGameBtn.Font = New System.Drawing.Font("Arial Black", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.StartGameBtn.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.StartGameBtn.Location = New System.Drawing.Point(466, 227)
        Me.StartGameBtn.Name = "StartGameBtn"
        Me.StartGameBtn.Size = New System.Drawing.Size(222, 60)
        Me.StartGameBtn.TabIndex = 14
        Me.StartGameBtn.Text = "START GAME"
        Me.StartGameBtn.UseVisualStyleBackColor = True
        Me.StartGameBtn.Visible = False
        '
        'IPCodeText
        '
        Me.IPCodeText.BackColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(192, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.IPCodeText.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.IPCodeText.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.IPCodeText.Font = New System.Drawing.Font("Arial Black", 15.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.IPCodeText.ForeColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.IPCodeText.Location = New System.Drawing.Point(239, 242)
        Me.IPCodeText.MaxLength = 8
        Me.IPCodeText.Name = "IPCodeText"
        Me.IPCodeText.Size = New System.Drawing.Size(222, 30)
        Me.IPCodeText.TabIndex = 13
        Me.IPCodeText.Text = "ENTER CODE"
        Me.IPCodeText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        Me.IPCodeText.Visible = False
        '
        'MainMenu
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.MediumTurquoise
        Me.ClientSize = New System.Drawing.Size(700, 700)
        Me.Controls.Add(Me.StartGameBtn)
        Me.Controls.Add(Me.IPCodeText)
        Me.Controls.Add(Me.LoadBTN)
        Me.Controls.Add(Me.BackBTN)
        Me.Controls.Add(Me.GenerateBTN)
        Me.Controls.Add(Me.NameLabel)
        Me.Controls.Add(Me.ExitBTN)
        Me.Controls.Add(Me.PictureBox3)
        Me.Controls.Add(Me.PictureBox2)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.GameCodeLbl)
        Me.Controls.Add(Me.BigLabel)
        Me.Controls.Add(Me.JoinGameBTN)
        Me.Controls.Add(Me.CreateGameBTN)
        Me.Controls.Add(Me.TopBarPB)
        Me.Controls.Add(Me.IPCodeBack)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "MainMenu"
        Me.Text = "Tank Game"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.TopBarPB, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.IPCodeBack, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents CreateGameBTN As Button
    Friend WithEvents JoinGameBTN As Button
    Friend WithEvents BigLabel As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents ExitBTN As Button
    Friend WithEvents NameLabel As Label
    Friend WithEvents BackBTN As Button
    Friend WithEvents TopBarPB As PictureBox
    Friend WithEvents LoadFile As OpenFileDialog
    Friend WithEvents CheckRecievedTimer As Timer
    Friend WithEvents IPCodeBack As PictureBox
    Friend WithEvents GenerateBTN As Button
    Friend WithEvents GameCodeLbl As Label
    Friend WithEvents LoadBTN As Button
    Friend WithEvents StartGameBtn As Button
    Friend WithEvents IPCodeText As TextBox
End Class
