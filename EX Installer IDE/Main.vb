Imports System.IO
Public Class Main
    Dim ExecutableFilePath As String

    Private Sub Main_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Sub StubTheThing()
        Try
            Dim stub As String
            Const FS1 As String = "|EXI|"
            Dim Temp As String = Application.StartupPath & "\" & TextBox1.Text & " Installer.exe"
            Dim bytesEXE As Byte() = System.IO.File.ReadAllBytes(ExecutableFilePath)
            File.WriteAllBytes(Temp, bytesEXE)
            FileOpen(1, Temp, OpenMode.Binary, OpenAccess.Read, OpenShare.Default)
            stub = Space(LOF(1))
            FileGet(1, stub)
            FileClose(1)
            FileOpen(1, Temp, OpenMode.Binary, OpenAccess.ReadWrite, OpenShare.Default)
            FilePut(1, stub & FS1 & TextBox1.Text & FS1 & textBox2.Text & FS1 & textBox3.Text & FS1)
            FileClose(1)
            MsgBox("Injectado correctamente!" & vbCrLf & "Guardado en : " & Temp)
        Catch ex As Exception
            MsgBox("Error al injectar el instructivo." & vbCrLf & ex.Message, MsgBoxStyle.Critical, "Instructive Injection")
            Console.WriteLine("[CreateAndStub@Main]Error: " & ex.Message)
        End Try
    End Sub

    Private Sub Injectar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim OpenFile As New OpenFileDialog
        OpenFile.Title = "Abrir EXInstaller..."
        OpenFile.Filter = "All file types (*.*)|*.*|Executable (*.exe)|*.exe"
        OpenFile.InitialDirectory = Application.StartupPath
        OpenFile.Multiselect = False
        If OpenFile.ShowDialog() = Windows.Forms.DialogResult.OK Then
            ExecutableFilePath = OpenFile.FileName
            StubTheThing()
        End If
    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click
        InstructiveCreator.Show()
        InstructiveCreator.Focus()
    End Sub
End Class