Public Class Form1
    Dim hahnPositionX As Integer = 100
    Dim hahnPositionY As Integer = 450
    Dim hahnSpeed As Integer = 20
    Dim obstacleSpeed As Integer = 20
    Dim score As Integer = 0
    Dim money As Integer = 500
    Dim lives As Integer = 3
    Dim passedObstacles As Integer = 0
    Dim currentCheckpoint As Integer = 0
    Dim currentSkin As String = "Yellow"
    Dim gameOver As Boolean = False
    Dim gameWon As Boolean = False
    Dim gameCompleted As Boolean = False
    Dim obstacles As New List(Of Rectangle)
    Dim rand As New Random()
    Dim checkpointColor As Color = Color.LightSkyBlue
    Dim lastObstacleTime As Integer = 0
    Dim obstacleSpawnInterval As Integer = 1000
    Dim currentScreen As String = "MainMenu"
    Dim gameTimer As Timer

    Dim backgroundColors As New List(Of Color) From {Color.CadetBlue, Color.DarkOliveGreen, Color.MediumOrchid, Color.Teal}
    Dim obstacleColors As New List(Of Color) From {Color.DarkRed, Color.ForestGreen, Color.RoyalBlue, Color.Orange}

    Dim skins As New Dictionary(Of String, Integer) From {
        {"Yellow", 0},
        {"Red", 50},
        {"Blue", 100},
        {"Green", 150},
        {"Pink", 200}
    }

    Public Sub New()
        InitializeComponent()
        Me.Size = New Drawing.Size(1109, 629)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.KeyPreview = True
        Me.DoubleBuffered = True
        InitializeMainMenu()
    End Sub

    Private Sub InitializeMainMenu()
        Me.Controls.Clear()
        AddButton("Start", New Point(400, 200), AddressOf StartButton_Click)
        AddButton("Shop", New Point(400, 300), AddressOf ShopButton_Click)
        currentScreen = "MainMenu"
        Me.Invalidate()
    End Sub

    Private Sub AddButton(text As String, location As Point, clickEvent As EventHandler)
        Dim button As New Button() With {
            .Text = text,
            .Size = New Size(200, 50),
            .Location = location
        }
        AddHandler button.Click, clickEvent
        Me.Controls.Add(button)
    End Sub

    Private Sub StartButton_Click(sender As Object, e As EventArgs)
        Me.Controls.Clear()
        InitializeGame()
    End Sub

    Private Sub ShopButton_Click(sender As Object, e As EventArgs)
        Me.Controls.Clear()
        ShowSkinShop()
    End Sub

    Private Sub InitializeGame()
        score = 0
        lives = 3
        passedObstacles = 0
        hahnPositionX = 100
        hahnPositionY = 450
        obstacles.Clear()
        gameOver = False
        gameWon = False
        gameCompleted = False
        currentCheckpoint = 0
        checkpointColor = backgroundColors(0)

        gameTimer = New Timer() With {.Interval = 20}
        AddHandler gameTimer.Tick, AddressOf Timer_Tick
        gameTimer.Start()
        AddObstacles()
        currentScreen = "Game"
        Me.Invalidate()
    End Sub

    Private Sub ShowSkinShop()
        Dim skinShopLabel As New Label() With {
            .Text = "Skin-Shop:",
            .Location = New Point(400, 100),
            .Font = New Font("Arial", 20)
        }
        Me.Controls.Add(skinShopLabel)

        Dim yOffset As Integer = 150
        For Each skin As KeyValuePair(Of String, Integer) In skins
            Dim skinButton As New Button() With {
                .Text = $"{skin.Key} - {skin.Value} Münzen",
                .Size = New Size(250, 40),
                .Location = New Point(400, yOffset)
            }
            AddHandler skinButton.Click, AddressOf SkinButton_Click
            skinButton.Tag = skin.Key
            Me.Controls.Add(skinButton)
            yOffset += 50
        Next

        AddButton("Back", New Point(400, yOffset), AddressOf BackButton_Click)
    End Sub

    Private Sub SkinButton_Click(sender As Object, e As EventArgs)
        Dim selectedSkin As String = CType(sender, Button).Tag.ToString()
        If skins(selectedSkin) <= money AndAlso currentSkin <> selectedSkin Then
            currentSkin = selectedSkin
            money -= skins(selectedSkin)
            MessageBox.Show($"Du hast den {selectedSkin} Skin gekauft!")
        ElseIf currentSkin = selectedSkin Then
            MessageBox.Show("Du hast diesen Skin bereits!")
        Else
            MessageBox.Show("Nicht genug Münzen!")
        End If
    End Sub

    Private Sub BackButton_Click(sender As Object, e As EventArgs)
        Me.Controls.Clear()
        InitializeMainMenu()
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        If gameOver OrElse gameWon Then
            Return
        End If

        MyBase.OnKeyDown(e)

        Select Case e.KeyCode
            Case Keys.W
                If hahnPositionY > 0 Then hahnPositionY -= hahnSpeed
            Case Keys.S
                If hahnPositionY < Me.ClientSize.Height - 30 Then hahnPositionY += hahnSpeed
            Case Keys.A
                If hahnPositionX > 0 Then hahnPositionX -= hahnSpeed
            Case Keys.D
                If hahnPositionX < Me.ClientSize.Width - 30 Then hahnPositionX += hahnSpeed
            Case Keys.R
                If gameOver OrElse gameWon Then
                    InitializeGame()
                End If
        End Select

        Me.Invalidate()
    End Sub

    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        If gameOver OrElse gameWon Then
            gameTimer.Stop()
            Me.Invalidate()
            Return
        End If

        lastObstacleTime += 20
        If lastObstacleTime >= obstacleSpawnInterval Then
            AddObstacles()
            lastObstacleTime = 0
        End If

        AdjustSpeedsBasedOnScore()

        UpdateGame()
        Me.Invalidate()
    End Sub

    Private Sub AdjustSpeedsBasedOnScore()
        If score >= 80 Then
            hahnSpeed = 9
            obstacleSpeed = 7
            checkpointColor = backgroundColors(3)
        ElseIf score >= 60 Then
            hahnSpeed = 8
            obstacleSpeed = 6
            checkpointColor = backgroundColors(2)
        ElseIf score >= 40 Then
            hahnSpeed = 7
            obstacleSpeed = 5
            checkpointColor = backgroundColors(1)
        ElseIf score >= 20 Then
            hahnSpeed = 6
            obstacleSpeed = 4
            checkpointColor = backgroundColors(0)
        End If
    End Sub

    Private Sub AddObstacles()
        Dim obstacle As New Rectangle(rand.Next(1100, 1200), rand.Next(0, Me.ClientSize.Height - 50), 30, 50)
        obstacles.Add(obstacle)
    End Sub

    Private Sub UpdateGame()
        For i As Integer = obstacles.Count - 1 To 0 Step -1
            obstacles(i) = New Rectangle(obstacles(i).X - obstacleSpeed, obstacles(i).Y, obstacles(i).Width, obstacles(i).Height)

            If obstacles(i).X < 0 Then
                obstacles.RemoveAt(i)
                passedObstacles += 1
                score += 1
            End If

            If New Rectangle(hahnPositionX, hahnPositionY, 30, 30).IntersectsWith(obstacles(i)) Then
                lives -= 1
                obstacles.RemoveAt(i)
                If lives <= 0 Then
                    gameOver = True
                    gameTimer.Stop()
                End If
            End If
        Next

        If passedObstacles >= 100 Then
            gameWon = True
            gameTimer.Stop()
        End If
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        e.Graphics.Clear(checkpointColor)

        Select Case currentSkin
            Case "Yellow"
                e.Graphics.FillRectangle(Brushes.Yellow, hahnPositionX, hahnPositionY, 30, 30)
            Case "Red"
                e.Graphics.FillRectangle(Brushes.Red, hahnPositionX, hahnPositionY, 30, 30)
            Case "Blue"
                e.Graphics.FillRectangle(Brushes.Blue, hahnPositionX, hahnPositionY, 30, 30)
            Case "Green"
                e.Graphics.FillRectangle(Brushes.Green, hahnPositionX, hahnPositionY, 30, 30)
            Case "Pink"
                e.Graphics.FillRectangle(Brushes.Pink, hahnPositionX, hahnPositionY, 30, 30)
        End Select

        For i As Integer = 0 To obstacles.Count - 1
            e.Graphics.FillRectangle(New SolidBrush(obstacleColors(Math.Min(score \ 20, obstacleColors.Count - 1))), obstacles(i))
        Next

        e.Graphics.DrawString($"Score: {score} Lives: {lives} Coins: {money}", Me.Font, Brushes.Black, 10, 10)

        If gameOver Then
            e.Graphics.DrawString("Game Over!", New Font("Arial", 30), Brushes.Red, 400, 250)
        ElseIf gameWon Then
            e.Graphics.DrawString("You Won!", New Font("Arial", 30), Brushes.Green, 400, 250)
        End If
    End Sub
End Class
