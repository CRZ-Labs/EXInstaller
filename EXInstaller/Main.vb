Imports Microsoft.Win32
Public Class Main
    Dim WithEvents DownloadInstructive As New Net.WebClient
    Dim DownloadInstructiveURI As Uri

    Dim WithEvents DownloadInstallPackage As New Net.WebClient
    Dim DownloadInstallPackageURI As Uri

    Private Sub Debugger_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        SetCurrentStatus("Inicializando...")
        TaskbarProgress.SetState(Me.Handle, TaskbarProgress.TaskbarStates.Indeterminate)
        StartParametros = Command()
        If My.Computer.FileSystem.DirectoryExists(DIRCommons) = False Then
            My.Computer.FileSystem.CreateDirectory(DIRCommons)
        End If
        If My.Computer.FileSystem.DirectoryExists(DIRTemp) = False Then
            My.Computer.FileSystem.CreateDirectory(DIRTemp)
        End If
        ReadParameters()
    End Sub

    Sub ReadParameters()
        SetCurrentStatus("Leyendo parametros...")
        Try
            Dim Argumento As String = StartParametros
            If Argumento.Contains("-") Then
                Argumento = Argumento.Remove(0, Argumento.LastIndexOf("-") + 1)
            End If
            If StartParametros.Contains("/Uninstall") Then
                IsUninstall = True
                StartParametros = StartParametros.Replace("/Uninstall ", Nothing)
            End If
            If StartParametros.Contains("/Reinstall") Then
                IsReinstall = True
                StartParametros = StartParametros.Replace("/Reinstall ", Nothing)
            End If
            If StartParametros.Contains("/Assistant") Then
                IsAssistant = True
                IsUninstall = False
                IsReinstall = False
                StartParametros = StartParametros.Replace("/Assistant ", Nothing)
            End If
            If StartParametros = Nothing Then
            ElseIf StartParametros.Contains("/Instructive-") Then
                Dim Args As String() = Argumento.Split(",")
                AssemblyName = Args(0).Trim()
                AssemblyVersion = Args(1).Trim()
                InstructiveURL = Args(2).Trim()
            End If
            If InstructiveURL = Nothing Then
                LoadSTUB()
            End If
            If IsAssistant = False Then
                If IsUninstall = True Then
                    UninstallMode(False)
                Else
                    If Application.ExecutablePath.Contains("uninstall.exe") Then
                        UninstallMode(True)
                    End If
                End If
            End If
            CommonStart()
        Catch ex As Exception
            AddToLog("[ReadParameters@Debugger]Error: ", ex.Message, True)
            MsgBox("Error al leer los parametros", MsgBoxStyle.Critical, "Instalador")
            End
        End Try
    End Sub

    Sub UninstallMode(ByVal RestartIt As Boolean)
        If RestartIt = True Then
            Restart("/Uninstall")
        End If
        IsUninstall = True
        lblTitle.Text = "Desinstalando..."
        lblSubTitle.Text = "Espere mientras la desinstalación del programa se completa."
        Text = "Desinstalador"
    End Sub

    Sub Restart(ByVal parameter As String)
        If My.Computer.FileSystem.FileExists(DIRTemp & "\" & AssemblyName & "_Assistant.exe") = True Then
            My.Computer.FileSystem.DeleteFile(DIRTemp & "\" & AssemblyName & "_Assistant.exe")
        End If
        My.Computer.FileSystem.CopyFile(Application.ExecutablePath, DIRTemp & "\" & AssemblyName & "_Assistant.exe")
        Process.Start(DIRTemp & "\" & AssemblyName & "_Assistant.exe", parameter & " /Instructive-" & AssemblyName & "," & AssemblyVersion & "," & InstructiveURL)
        End
    End Sub

    Sub AssistantMode()
        Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 3)
        lblTitle.Text = "Asistente"
        lblSubTitle.Text = "Este programa ya esta instalado, seleccione una opcion."
        lblStatus.Text = "Esperando..."
        ProgressBarStatus.Style = ProgressBarStyle.Marquee
        Text = "Asistente"
        Button1.Enabled = True
        Button2.Enabled = True
        Button1.Visible = True
        Button2.Visible = True
    End Sub

    Sub Reinstall()
        Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 3)
        lblTitle.Text = "Reinstalando..."
        lblSubTitle.Text = "Espere mientras el programa se reinstala..."
        lblStatus.Text = "Esperando..."
        ProgressBarStatus.Style = ProgressBarStyle.Marquee
        Text = "Reinstalador"
        Button1.Enabled = False
        Button2.Enabled = False
        Button1.Visible = False
        Button2.Visible = False
        InstallIt()
    End Sub

    Sub CommonStart()
        SetCurrentStatus("Consultando la informacion del equipo...")
        Try
            Dim consultaSQLArquitectura As String = "SELECT * FROM Win32_Processor"
            Dim objArquitectura As New ManagementObjectSearcher(consultaSQLArquitectura)
            For Each info As ManagementObject In objArquitectura.Get()
                ArquitecturaSO = info.Properties("AddressWidth").Value.ToString()
            Next info
        Catch ex As Exception
            AddToLog("[InformationQuery@Main]Error: ", ex.Message, True)
        End Try
        If My.Computer.FileSystem.FileExists(InstructiveFilePath) = True Then
            My.Computer.FileSystem.DeleteFile(InstructiveFilePath)
        End If
        GetInstructive()
    End Sub

    Sub SetCurrentStatus(ByVal Status As String)
        lblStatus.Text = Status
        AddToLog("Status: ", Status)
    End Sub

    Sub GetInstructive()
        Try
            SetCurrentStatus("Comenzo la descarga del instructivo...")
            Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 5)
            DownloadInstructiveURI = New Uri(InstructiveURL)
            DownloadInstructive.DownloadFileAsync(DownloadInstructiveURI, InstructiveFilePath)
        Catch ex As Exception
            AddToLog("[GetInstructive@Main]Error: ", ex.Message, True)
        End Try
    End Sub

    Sub LoadSTUB()
        SetCurrentStatus("Leyendo datos pre-cargados...")
        Try
            FileOpen(1, Application.ExecutablePath, OpenMode.Binary, OpenAccess.Read)
            Dim stubb As String = Space(LOF(1))
            Dim FileSplit = "|EXI|"
            FileGet(1, stubb)
            FileClose(1)
            Dim opt() As String = Split(stubb, FileSplit)
            If opt(1) IsNot "None" Then
                AssemblyName = opt(1)
                AssemblyVersion = opt(2)
                InstructiveURL = opt(3)
            End If
        Catch ex As Exception
            AddToLog("[LoadSTUB@Debugger]Error: ", ex.Message, True)
            MsgBox("No hay ninguna pre-configuracion", MsgBoxStyle.Critical, "Instalador")
            End
        End Try
    End Sub

    Sub LoadInstructive()
        Try
            SetCurrentStatus("Leyendo el instructivo... 1/3")
            Instructive_Package_Status = GetIniValue("Package", "Status", InstructiveFilePath)
            Instructive_Package_AssemblyName = GetIniValue("Package", "AssemblyName", InstructiveFilePath)
            Instructive_Package_AssemblyVersion = GetIniValue("Package", "AssemblyVersion", InstructiveFilePath)
            Instructive_Package_Company = GetIniValue("Package", "Company", InstructiveFilePath)
            Instructive_Package_WebUrl = GetIniValue("Package", "WebUrl", InstructiveFilePath)
            Instructive_Package_PackageName = GetIniValue("Package", "PackageName", InstructiveFilePath)
            Instructive_Package_IsComponent = GetIniValue("Package", "IsComponent", InstructiveFilePath)
            Instructive_Package_InstallerVersion = GetIniValue("Package", "InstallerVersion", InstructiveFilePath)
            Instructive_Package_BitsArch = GetIniValue("Package", "BitsArch", InstructiveFilePath)
            SetCurrentStatus("Leyendo el instructivo... 2/3")
            Instructive_Installer_Status = GetIniValue("Installer", "Status", InstructiveFilePath)
            Instructive_Installer_EnableDowngrade = GetIniValue("Installer", "EnableDowngrade", InstructiveFilePath)
            Instructive_Installer_NeedRestart = GetIniValue("Installer", "NeedRestart", InstructiveFilePath)
            Instructive_Installer_NeedStartUp = GetIniValue("Installer", "NeedStartUp", InstructiveFilePath)
            Instructive_Installer_NeedElevateAccess = GetIniValue("Installer", "NeedElevateAccess", InstructiveFilePath)
            Instructive_Installer_InstallFolder = GetIniValue("Installer", "InstallFolder", InstructiveFilePath)
            Instructive_Installer_EULA = GetIniValue("Installer", "EULA", InstructiveFilePath)
            Instructive_Installer_Installer = GetIniValue("Installer", "Installer", InstructiveFilePath)
            Instructive_Installer_InstallPackage = GetIniValue("Installer", "InstallPackage", InstructiveFilePath)
            SetCurrentStatus("Leyendo el instructivo... 3/3")
            Instructive_HelpLinks_ChangeLogLink = GetIniValue("HelpLinks", "ChangeLogLink", InstructiveFilePath)
            Instructive_HelpLinks_UseGuide = GetIniValue("HelpLinks", "UseGuide", InstructiveFilePath)
            Instructive_HelpLinks_AppAbout = GetIniValue("HelpLinks", "AppAbout", InstructiveFilePath)
            Instructive_HelpLinks_Contact = GetIniValue("HelpLinks", "Contact", InstructiveFilePath)
        Catch ex As Exception
            AddToLog("[LoadInstructive(0)@Debugger]Error: ", ex.Message, True)
        End Try
        SetCurrentStatus("Esperando...")
        'DEFINICIONES PARA LA INSTALACION ----------
        Try
            x32bits = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" & Instructive_Package_AssemblyName
            x64x32bits = "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" & Instructive_Package_AssemblyName
            If ArquitecturaSO = "32" Then
                InstallerPathBuilder = "C:\Program Files" & Instructive_Installer_InstallFolder
                If RegistradorInstalacion Is Nothing Then
                    Registry.LocalMachine.CreateSubKey(x32bits)
                End If
                RegistradorInstalacion = Registry.LocalMachine.OpenSubKey(x32bits, True)
                If Instructive_Package_BitsArch = "64" Then
                    MsgBox("El programa a instalar requiere de un procesador de 64bits y no de 32bits", MsgBoxStyle.Critical, "No se puede instalar")
                    End
                End If
            ElseIf ArquitecturaSO = "64" Then
                If Instructive_Package_BitsArch = "32" Then
                    InstallerPathBuilder = "C:\Program Files (x86)" & Instructive_Installer_InstallFolder
                    If RegistradorInstalacion Is Nothing Then
                        Registry.LocalMachine.CreateSubKey(x32bits)
                    End If
                    RegistradorInstalacion = Registry.LocalMachine.OpenSubKey(x32bits, True)
                Else
                    InstallerPathBuilder = "C:\Program Files" & Instructive_Installer_InstallFolder
                    If RegistradorInstalacion Is Nothing Then
                        Registry.LocalMachine.CreateSubKey(x64x32bits)
                    End If
                    RegistradorInstalacion = Registry.LocalMachine.OpenSubKey(x64x32bits, True)
                End If
            Else
                InstallerPathBuilder = "C:\Program Files" & Instructive_Installer_InstallFolder
                If RegistradorInstalacion Is Nothing Then
                    Registry.LocalMachine.CreateSubKey(x32bits)
                End If
                RegistradorInstalacion = Registry.LocalMachine.OpenSubKey(x32bits, True)
            End If
            If IsUninstall = False Then
                If My.Computer.FileSystem.DirectoryExists(InstallerPathBuilder) = True Then
                    If RegistradorInstalacion IsNot Nothing Then
                        If IsReinstall = True Then
                            Reinstall()
                        Else
                            AssistantMode()
                        End If
                        Exit Sub
                    End If
                End If
                InstallIt()
            Else
                Uninstall()
            End If
        Catch ex As Exception
            AddToLog("[LoadInstructive(1)@Debugger]Error: ", ex.Message, True)
        End Try
    End Sub

    Sub InstallIt()
        Try
            SetCurrentStatus("Descargando el paquete de instalacion...")
            ProgressBarStatus.Style = ProgressBarStyle.Blocks
            DownloadInstallPackageURI = New Uri(Instructive_Installer_InstallPackage)
            DownloadInstallPackage.DownloadFileAsync(DownloadInstallPackageURI, DIRTemp & "\" & AssemblyName & "_" & Instructive_Package_AssemblyVersion & ".zip")
        Catch ex As Exception
            AddToLog("[InstallIt@Main]Error: ", ex.Message, True)
            MsgBox("No se pudo descargar el instructivo", MsgBoxStyle.Critical, "Instructivo")
            End
        End Try
    End Sub

    Private Sub DownloadInstallPackage_DownloadFileCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs) Handles DownloadInstallPackage.DownloadFileCompleted
        Install()
    End Sub

    Private Sub DownloadInstallPackage_DownloadProgressChanged(ByVal sender As Object, ByVal e As System.Net.DownloadProgressChangedEventArgs) Handles DownloadInstallPackage.DownloadProgressChanged
        Try
            Dim bytesIn As Double = Double.Parse(e.BytesReceived.ToString())
            Dim totalBytes As Double = Double.Parse(e.TotalBytesToReceive.ToString())
            lblStatusStatus.Text = CStr(e.ProgressPercentage & ("% ")) & FormatBytes(bytesIn) & "/" & FormatBytes(totalBytes)
        Catch
        End Try
        Try
            ProgressBarStatus.Value = e.ProgressPercentage
        Catch
        End Try
        Try
            PackageSize = e.TotalBytesToReceive
        Catch
        End Try
        Try
            TaskbarProgress.SetValue(Me.Handle, e.ProgressPercentage.ToString, 100)
        Catch
        End Try
    End Sub

    Private Sub DownloadInstructive_DownloadFileCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs) Handles DownloadInstructive.DownloadFileCompleted
        LoadInstructive()
    End Sub

    Private Sub Desinstalar_Click(sender As Object, e As EventArgs) Handles Button2.Click
        UninstallMode(True)
    End Sub
    Private Sub Reinstalar_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Restart("/Reinstall")
    End Sub
End Class