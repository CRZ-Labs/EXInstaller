Imports System.Runtime.InteropServices
Imports System.Text
Public Class InstructiveCreator
    Dim filePath As String

    Private Sub InstructiveCreator_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cb_Package_InstallerVersion.Items.Add("1.2.0.0")
        cb_Package_InstallerVersion.Items.Add("1.1.0.0")
        'TabPage2.Enabled = False
        'TabPage3.Enabled = False
        'TabPage4.Enabled = False
        'TabPage5.Enabled = False
    End Sub

    Private Sub rb_General_Save_CheckedChanged(sender As Object, e As EventArgs) Handles rb_General_Save.CheckedChanged
        If rb_General_Save.Checked Then
            lbl_General_Save_Open.Enabled = True
            lbl_General_Save_InstructiveLocation.Enabled = True
            tb_General_Save.Enabled = True
        Else
            lbl_General_Save_Open.Enabled = False
            lbl_General_Save_InstructiveLocation.Enabled = False
            tb_General_Save.Enabled = False
        End If
    End Sub
    Private Sub rb_General_Open_CheckedChanged(sender As Object, e As EventArgs) Handles rb_General_Open.CheckedChanged
        If rb_General_Open.Checked Then
            lbl_General_Open_InstructiveLocation.Enabled = True
            lbl_General_Open_Open.Enabled = True
            tb_General_Open.Enabled = True
        Else
            lbl_General_Open_InstructiveLocation.Enabled = False
            lbl_General_Open_Open.Enabled = False
            tb_General_Open.Enabled = False
        End If
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        If TabPage1.Enabled = True Then
            If filePath <> Nothing Then
                TabPage2.Enabled = True
                TabPage1.Enabled = False
                'btnBack.Visible = True
                TabControl1.SelectTab(1)
            Else
                MsgBox("Falta informacion", MsgBoxStyle.Critical, "Crear Instructivo")
            End If
        ElseIf TabPage2.Enabled = True Then
            TabPage3.Enabled = True
            TabPage2.Enabled = False
            TabControl1.SelectTab(2)
        ElseIf TabPage3.Enabled = True Then
            TabPage4.Enabled = True
            TabPage3.Enabled = False
            TabControl1.SelectTab(3)
        ElseIf TabPage4.Enabled = True Then
            TabPage5.Enabled = True
            TabPage4.Enabled = False
            TabControl1.SelectTab(4)
            'final
            TextBox1.Text = tb_Package_AssemblyName.Text
            TextBox2.Text = tb_Package_AssemblyVersion.Text
            Main.TextBox1.Text = tb_Package_AssemblyName.Text
            Main.TextBox2.Text = tb_Package_AssemblyVersion.Text
            btnNext.Enabled = False
            btnCreate.Enabled = True
        End If
    End Sub
    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click

    End Sub

    Private Sub btnCreate_Click(sender As Object, e As EventArgs) Handles btnCreate.Click
        Try
            Dim Contenido As String = "# EX Installer Instructive File" &
                vbCrLf & "[Package]" &
                vbCrLf & "Status=" & cb_Package_Status.Text &
                vbCrLf & "AssemblyName=" & tb_Package_AssemblyName.Text &
                vbCrLf & "AssemblyVersion=" & tb_Package_AssemblyVersion.Text &
                vbCrLf & "Description=" & tb_Package_Description.Text &
                vbCrLf & "Company=" & tb_Package_Company.Text &
                vbCrLf & "WebUrl=" & tb_Package_WebUrl.Text &
                vbCrLf & "PackageName=" & tb_Package_PackageName.Text &
                vbCrLf & "IsComponent=" & cb_Package_IsComponent.Text &
                vbCrLf & "InstallerVersion=" & cb_Package_InstallerVersion.Text &
                vbCrLf & "BitsArch=" & cb_Package_ProcessorArchitecture.Text &
                vbCrLf & "[Installer]" &
                vbCrLf & "Status=" & cb_Installer_Status.Text &
                vbCrLf & "EnableDowngrade=" & cb_Installer_EnableDowngrade.Text &
                vbCrLf & "NeedRestart=" & cb_Installer_NeedRestart.Text &
                vbCrLf & "NeedStartUp=" & tb_Installer_NeedStartUp.Text &
                vbCrLf & "NeedElevateAccess=" & cb_Installer_NeedElevateAccess.Text &
                vbCrLf & "NeedToStart=" & tb_Installer_NeedToStart.Text &
                vbCrLf & "InstallFolder=" & tb_Installer_InstallFolder.Text &
                vbCrLf & "EULA=" & tb_Installer_EULA.Text &
                vbCrLf & "Installer=" & tb_Installer_Installer.Text &
                vbCrLf & "InstallPackage=" & tb_Installer_InstallPackage.Text &
                vbCrLf & "[HelpLinks]" &
                vbCrLf & "ChangeLogLink=" & tb_HelpLinks_ChangeLog.Text &
                vbCrLf & "UseGuide=" & tb_HelpLinks_UseGuide.Text &
                vbCrLf & "AppAbout=" & tb_HelpLinks_AppAbout.Text &
                vbCrLf & "Contact=" & tb_HelpLinks_Contact.Text

            If My.Computer.FileSystem.FileExists(filePath) = True Then
                My.Computer.FileSystem.DeleteFile(filePath)
            End If
            My.Computer.FileSystem.WriteAllText(filePath, Contenido, False)
            MsgBox("Instructivo creado!" & vbCrLf & "Almacenado en: " & filePath, MsgBoxStyle.Information, "Instructivo")
        Catch ex As Exception
        End Try
    End Sub

    Private Sub lbl_General_Save_Open_Click(sender As Object, e As EventArgs) Handles lbl_General_Save_Open.Click
        Dim SaveFile As New SaveFileDialog
        SaveFile.Filter = "Todos los archivos(*.*)|*.*"
        SaveFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        SaveFile.Title = "Guardar instructivo..."
        If SaveFile.ShowDialog() = DialogResult.OK Then
            filePath = SaveFile.FileName
            tb_General_Save.Text = SaveFile.FileName
        End If
    End Sub
    Private Sub lbl_General_Open_Open_Click(sender As Object, e As EventArgs) Handles lbl_General_Open_Open.Click
        Dim OpenFile As New OpenFileDialog
        OpenFile.Filter = "Todos los archivos(*.*)|*.*"
        OpenFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        OpenFile.Title = "Abrir instructivo..."
        If OpenFile.ShowDialog() = DialogResult.OK Then
            tb_General_Open.Text = OpenFile.FileName
            ReadInstructiveFile(OpenFile.FileName)
        End If
    End Sub

    Sub ReadInstructiveFile(ByVal file As String)
        filePath = file
        Try
            'Package
            cb_Package_Status.Text = GetIniValue("Package", "Status", filePath)
            tb_Package_AssemblyName.Text = GetIniValue("Package", "AssemblyName", filePath)
            tb_Package_AssemblyVersion.Text = GetIniValue("Package", "AssemblyVersion", filePath)
            tb_Package_Company.Text = GetIniValue("Package", "Company", filePath)
            tb_Package_WebUrl.Text = GetIniValue("Package", "WebUrl", filePath)
            tb_Package_PackageName.Text = GetIniValue("Package", "PackageName", filePath)
            cb_Package_IsComponent.Text = GetIniValue("Package", "IsComponent", filePath)
            cb_Package_InstallerVersion.Text = GetIniValue("Package", "InstallerVersion", filePath)
            cb_Package_ProcessorArchitecture.Text = GetIniValue("Package", "BitsArch", filePath)

            'Installer
            cb_Installer_Status.Text = GetIniValue("Installer", "Status", filePath)
            cb_Installer_EnableDowngrade.Text = GetIniValue("Installer", "EnableDowngrade", filePath)
            cb_Installer_NeedRestart.Text = GetIniValue("Installer", "NeedRestart", filePath)
            tb_Installer_NeedStartUp.Text = GetIniValue("Installer", "NeedStartUp", filePath)
            cb_Installer_NeedElevateAccess.Text = GetIniValue("Installer", "NeedElevateAccess", filePath)
            tb_Installer_NeedToStart.Text = GetIniValue("Installer", "NeedToStart", filePath)
            tb_Installer_InstallFolder.Text = GetIniValue("Installer", "InstallFolder", filePath)
            tb_Installer_EULA.Text = GetIniValue("Installer", "EULA", filePath)
            tb_Installer_Installer.Text = GetIniValue("Installer", "Installer", filePath)
            tb_Installer_InstallPackage.Text = GetIniValue("Installer", "InstallPackage", filePath)

            'HelpLinks
            tb_HelpLinks_ChangeLog.Text = GetIniValue("HelpLinks", "ChangeLogLink", filePath)
            tb_HelpLinks_UseGuide.Text = GetIniValue("HelpLinks", "UseGuide", filePath)
            tb_HelpLinks_AppAbout.Text = GetIniValue("HelpLinks", "AppAbout", filePath)
            tb_HelpLinks_Contact.Text = GetIniValue("HelpLinks", "Contact", filePath)
        Catch ex As Exception
        End Try
    End Sub
End Class
Module Lector
    <DllImport("kernel32")>
    Private Function GetPrivateProfileString(ByVal section As String, ByVal key As String, ByVal def As String, ByVal retVal As StringBuilder, ByVal size As Integer, ByVal filePath As String) As Integer
        'Use GetIniValue("KEY_HERE", "SubKEY_HERE", "filepath")
    End Function

    Public Function GetIniValue(section As String, key As String, filename As String, Optional defaultValue As String = Nothing) As String
        Dim sb As New StringBuilder(500)
        If GetPrivateProfileString(section, key, defaultValue, sb, sb.Capacity, filename) > 0 Then
            Return sb.ToString
        Else
            Return defaultValue
        End If
    End Function
End Module