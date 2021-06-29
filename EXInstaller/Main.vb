Imports System.Management
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
            If StartParametros = Nothing Then
                LoadSTUB()
            ElseIf StartParametros.Contains("/Instructive-") Then
                Dim Args As String() = Argumento.Split(",")
                AssemblyName = Args(0).Trim()
                AssemblyVersion = Args(1).Trim()
                InstructiveURL = Args(2).Trim()
            End If
            If IsUninstall = True Then
                UninstallMode()
            Else
                If Application.ExecutablePath.Contains("uninstall.exe") Then
                    UninstallMode()
                End If
            End If
            CommonStart()
        Catch ex As Exception
            Console.WriteLine("[ReadParameters@Debugger]Error: " & ex.Message)
            MsgBox("Error al leer los parametros", MsgBoxStyle.Critical, "Instalador")
            End
        End Try
    End Sub

    Sub UninstallMode()
        If Application.ExecutablePath.Contains("uninstall.exe") Then
            If My.Computer.FileSystem.FileExists(DIRTemp & "\" & AssemblyName & "_Uninstaller.exe") = True Then
                My.Computer.FileSystem.DeleteFile(DIRTemp & "\" & AssemblyName & "_Uninstaller.exe")
            End If
            My.Computer.FileSystem.CopyFile(Application.ExecutablePath, DIRTemp & "\" & AssemblyName & "_Uninstaller.exe")
            Process.Start(DIRTemp & "\" & AssemblyName & "_Uninstaller.exe", " /Uninstall /Instructive-" & AssemblyName & "," & AssemblyVersion & "," & InstructiveURL)
            End
        End If
        IsUninstall = True
        lblTitle.Text = "Desinstalando..."
        lblSubTitle.Text = "Espere mientras la desinstalación del programa se completa."
        Text = "Desinstalador"
    End Sub

    Sub CommonStart()
        SetCurrentStatus("Consultando la informacion del equipo...")
        Try
            Dim consultaSQLArquitectura As String = "SELECT * FROM Win32_Processor"
            Dim objArquitectura As New ManagementObjectSearcher(consultaSQLArquitectura)
            For Each info As ManagementObject In objArquitectura.Get()
                ArquitecturaSO = info.Properties("AddressWidth").Value.ToString()
            Next info
        Catch
        End Try
        If My.Computer.FileSystem.FileExists(InstructiveFilePath) = True Then
            My.Computer.FileSystem.DeleteFile(InstructiveFilePath)
        End If
        GetInstructive()
    End Sub

    Sub SetCurrentStatus(ByVal Status As String)
        lblStatus.Text = Status
    End Sub

    Sub GetInstructive()
        SetCurrentStatus("Comenzo la descarga del instructivo...")
        Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 5)
        DownloadInstructiveURI = New Uri(InstructiveURL)
        DownloadInstructive.DownloadFileAsync(DownloadInstructiveURI, InstructiveFilePath)
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
            Console.WriteLine("[LoadSTUB@Debugger]Error: " & ex.Message)
            MsgBox("No hay ninguna pre-configuracion", MsgBoxStyle.Critical, "Instalador")
            End
        End Try
    End Sub

    Sub LoadInstructive()
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
        SetCurrentStatus("Esperando...")
        If IsUninstall = False Then
            SetCurrentStatus("Descargando el paquete de instalacion...")
            ProgressBarStatus.Style = ProgressBarStyle.Blocks
            DownloadInstallPackageURI = New Uri(Instructive_Installer_InstallPackage)
            DownloadInstallPackage.DownloadFileAsync(DownloadInstallPackageURI, DIRTemp & "\" & AssemblyName & "_" & Instructive_Package_AssemblyVersion & ".zip")
        Else
            Uninstall()
        End If
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
End Class