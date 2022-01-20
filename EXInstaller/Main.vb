Imports Microsoft.Win32
Public Class Main
    Dim WithEvents DownloadInstructive As New Net.WebClient
    Dim DownloadInstructiveURI As Uri

    Dim WithEvents DownloadInstallPackage As New Net.WebClient
    Dim DownloadInstallPackageURI As Uri

    Private Sub Main_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        SetCurrentStatus("Inicializando...")
        'TaskbarProgress.SetState(Me.Handle, TaskbarProgress.TaskbarStates.Indeterminate)
        StartParametros = Command() 'Lee argumentos
        Try
            If My.Computer.FileSystem.DirectoryExists(DIRCommons) Then
                My.Computer.FileSystem.DeleteDirectory(DIRCommons, FileIO.DeleteDirectoryOption.DeleteAllContents)
            End If
            My.Computer.FileSystem.CreateDirectory(DIRCommons)
            If My.Computer.FileSystem.DirectoryExists(DIRTemp) Then
                My.Computer.FileSystem.DeleteDirectory(DIRTemp, FileIO.DeleteDirectoryOption.DeleteAllContents)
            End If
            My.Computer.FileSystem.CreateDirectory(DIRTemp)
        Catch ex As Exception
            AddToLog("[Main_Load(Basics)@Main]Error: ", ex.Message, True)
        End Try
        ReadParameters()
    End Sub
    Private Sub Main_HelpRequested(sender As Object, hlpevent As HelpEventArgs) Handles Me.HelpRequested
        MsgBox(My.Application.Info.ProductName & " " & My.Application.Info.Version.ToString & " (" & Application.ProductVersion & ") created by " & My.Application.Info.CompanyName & vbCrLf & My.Application.Info.Description & vbCrLf & vbCrLf & vbCrLf & "https://github.com/CRZ-Labs" & vbCrLf & "CRZ Labs no se hace responsable por el mal uso que se le pueda dar a EX Installer.", MsgBoxStyle.Information)
    End Sub

    Sub ReadParameters()
        SetCurrentStatus("Leyendo parámetros...")
        Try
            Dim parametro As String = Command()
            Dim Argumentos() As String = Command().Split(" ")
            For Each arg As String In Argumentos
                If arg.Contains("/Uninstall") Then
                    IsUninstall = True
                    parametro = parametro.Replace("/Uninstall", Nothing)
                ElseIf arg.Contains("-F") Then
                    IsForced = True
                    parametro = parametro.Replace("-F", Nothing)
                ElseIf arg.Contains("-S") Then
                    IsSilence = True
                    parametro = parametro.Replace("-S", Nothing)
                ElseIf arg.Contains("/Assistant") Then 'Fue iniciado por el "post-instalador"
                    IsAssistant = True
                    parametro = parametro.Replace("/Assistant", Nothing)
                ElseIf arg.Contains("-Log") Then
                    CanSaveLog = True
                    parametro = parametro.Replace("-Log", Nothing)
                ElseIf arg.Contains("/Instructive~") Then
                    If CanOverwrite = True Then
                        parametro = parametro.Replace("/Instructive~", Nothing)
                        Dim Args As String() = parametro.Split(",")
                        AssemblyName = Args(0).Trim()
                        AssemblyVersion = Args(1).Trim()
                        InstructiveURL = Args(2).Trim()
                        IsInjected = False
                    End If
                End If
            Next

            If IsSilence = True Then
                Me.Hide()
                Me.FormBorderStyle = FormBorderStyle.FixedToolWindow
                Me.WindowState = FormWindowState.Minimized
                Me.Opacity = 0.0
                Me.ShowInTaskbar = False
                Me.ShowIcon = False
            End If

            If IsInjected = True Then
                LoadSTUB()
            End If

            If Application.ExecutablePath.Contains("uninstall.exe") Then
                If IsUninstall = True Then
                    Restart("/Uninstall")
                Else
                    Restart(Nothing)
                End If
            End If

            If IsSilence = True Then
                Me.Hide()
                Me.FormBorderStyle = FormBorderStyle.FixedToolWindow
                Me.ShowInTaskbar = False
            End If

            If IsForced = True Then
                Me.ControlBox = False
            End If

            If InstructiveURL = Nothing Then
                MsgBox("No se encontró una preconfiguración", MsgBoxStyle.Critical, "Instructivo")
                End
            End If

            CommonStart()
        Catch ex As Exception
            AddToLog("[ReadParameters@Main]Error: ", ex.Message, True)
            MsgBox("Error al leer los parámetros", MsgBoxStyle.Critical, "Instalador")
            End
        End Try
    End Sub
    Sub CommonStart()
        SetCurrentStatus("Consultando la información del equipo...")
        'APLICANDO VARIABLES basicas
        lblInfo.Text = AssemblyName & " " & AssemblyVersion
        InstructiveFilePath = DIRCommons & "\" & AssemblyVersion & "_Instructive.ini"
        Try
            Dim consultaSQLArquitectura As String = "SELECT * FROM Win32_Processor"
            Dim objArquitectura As New ManagementObjectSearcher(consultaSQLArquitectura)
            For Each info As ManagementObject In objArquitectura.Get()
                ArquitecturaSO = info.Properties("AddressWidth").Value.ToString()
            Next info
            If My.Computer.FileSystem.FileExists(InstructiveFilePath) Then
                My.Computer.FileSystem.DeleteFile(InstructiveFilePath)
            End If
        Catch ex As Exception
            AddToLog("[CommonStart@Main]Error: ", ex.Message, True)
        End Try
        GetInstructive()
    End Sub
    Sub GetInstructive()
        Try
            If My.Computer.Network.IsAvailable Then
                SetCurrentStatus("Comenzó la descarga del instructivo..." &
                             vbCrLf & "     " & InstructiveURL)
                Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 5)
                DownloadInstructiveURI = New Uri(InstructiveURL)
                DownloadInstructive.DownloadFileAsync(DownloadInstructiveURI, InstructiveFilePath)
            Else
                SetCurrentStatus("Sin conexion a internet...")
                OfflineMode()
            End If
        Catch ex As Exception
            AddToLog("[GetInstructive@Main]Error: ", ex.Message, True)
            Complementos.Closing()
        End Try
    End Sub

    Sub Restart(ByVal parameter As String)
        Try
            SetCurrentStatus("Iniciando desde otra ubicacion...")
            If My.Computer.FileSystem.FileExists(DIRTemp & "\" & AssemblyName & "_Assistant.exe") = True Then
                My.Computer.FileSystem.DeleteFile(DIRTemp & "\" & AssemblyName & "_Assistant.exe")
            End If
            My.Computer.FileSystem.CopyFile(Application.ExecutablePath, DIRTemp & "\" & AssemblyName & "_Assistant.exe")
            Process.Start(DIRTemp & "\" & AssemblyName & "_Assistant.exe", parameter & " /Instructive~" & AssemblyName & "," & AssemblyVersion & "," & InstructiveURL)
            End
            'Complementos.Closing()
        Catch ex As Exception
            AddToLog("[Restart@Main]Error: ", ex.Message, True)
        End Try
    End Sub

    Sub CheckUpdates()
        Try
            If GlobalUses.IsComponent = False Then
                Dim VersionReader As RegistryKey
                VersionReader = RegistradorInstalacion
                If VersionReader IsNot Nothing Then
                    Dim version1 = New Version(VersionReader.GetValue("DisplayVersion"))
                    Dim version2 = New Version(Instructive_Package_AssemblyVersion)
                    Dim result = version1.CompareTo(version2)
                    If (result > 0) Then
                        'Sobre actualizado
                    ElseIf (result < 0) Then
                        'Nueva version
                        Button1.Text = "Actualizar"
                        Button1.Enabled = True
                        IsUpdate = True
                    End If
                End If
            End If
        Catch ex As Exception
            AddToLog("[CheckUpdates@Main]Error: ", ex.Message, True)
        End Try
    End Sub

    Sub AssistantMode()
        SetCurrentStatus("Modo asistente")
        If IsSilence = False Then
            lblInfo.Text = AssemblyName & " " & RegistradorInstalacion.GetValue("DisplayVersion")
            Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 3)
            lblTitle.Text = "Asistente"
            If GlobalUses.IsComponent = False Then
                lblSubTitle.Text = "Este programa ya está instalado, seleccione una opción."
            Else
                lblSubTitle.Text = "Este componente ya está instalado, seleccione una opción."
            End If
            lblStatus.Text = "Esperando..."
            ProgressBarStatus.Style = ProgressBarStyle.Marquee
            Text = "Asistente " & Instructive_Package_AssemblyName
            If IsForced = False Then
                Button1.Enabled = True
                Button2.Enabled = True
                Button1.Visible = True
                Button2.Visible = True
                'check updates
                CheckUpdates()
            Else
                MsgBox("El modo forzado no permite la administración de la instalación.", MsgBoxStyle.Critical, "Asistente")
                End
            End If
        Else
            End
        End If
    End Sub
    Sub UninstallIt(ByVal Now As Boolean)
        SetCurrentStatus("Desinstalar")
        If IsForced = False Then
            If IsSilence = False Then
                IsUninstall = True
                lblTitle.Text = "Desinstalando..."
                If GlobalUses.IsComponent = False Then
                    lblSubTitle.Text = "Espere mientras la desinstalación del programa se completa."
                Else
                    lblSubTitle.Text = "Espere mientras la desinstalación del componente se completa."
                End If
                Text = "Desinstalar " & Instructive_Package_AssemblyName
                If Now Then
                    Dim MSG_Content As String = "¿Want to uninstall the Software called " & Instructive_Package_AssemblyName & "?"
                    If GlobalUses.IsComponent = True Then
                        MSG_Content = "¿Want to uninstall the Component called " & Instructive_Package_AssemblyName & "?"
                    End If
                    If MessageBox.Show(MSG_Content, "Confirm Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                        CheckIfRunning()
                        Uninstall()
                    Else
                        AssistantMode()
                    End If
                End If
            Else
                If Now Then
                    CheckIfRunning()
                    Uninstall()
                End If
            End If
        Else
            MsgBox("El modo forzado no permite la desinstalación.", MsgBoxStyle.Critical, "Asistente")
            End
        End If
    End Sub
    Sub Reinstall()
        SetCurrentStatus("Reinstalar")
        If IsSilence = False Then
            Dim MSG_Content As String = "¿Want to reinstall the Software called " & Instructive_Package_AssemblyName & "?"
            If GlobalUses.IsComponent Then
                MSG_Content = "¿Want to reinstall the Component called " & Instructive_Package_AssemblyName & "?"
            End If
            If MessageBox.Show(MSG_Content, "Confirm Reinstall", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 3)
                lblTitle.Text = "Reinstalando..."
                If GlobalUses.IsComponent = False Then
                    lblSubTitle.Text = "Espere mientras el programa se reinstala..."
                Else
                    lblSubTitle.Text = "Espere mientras el componente se reinstala..."
                End If
                lblStatus.Text = "Esperando..."
                ProgressBarStatus.Style = ProgressBarStyle.Marquee
                Text = "Reinstalar " & Instructive_Package_AssemblyName
                Button1.Enabled = False
                Button2.Enabled = False
                Button1.Visible = False
                Button2.Visible = False
                IsUpdate = True
                IsReinstall = True
                InstallIt()
            End If
        Else
            InstallIt()
        End If
    End Sub
    Sub IsComponent()
        SetCurrentStatus("Componente")
        GlobalUses.IsComponent = True
        If IsSilence = False Then
            Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 3)
            lblTitle.Text = "Aplicando..."
            lblSubTitle.Text = "Espere mientras el componente se aplica..."
            lblStatus.Text = "Esperando..."
            ProgressBarStatus.Style = ProgressBarStyle.Marquee
            Text = "Componente " & Instructive_Package_AssemblyName
        End If
    End Sub
    Sub InstallMode()
        If GlobalUses.IsComponent = False Then
            SetCurrentStatus("Modo instalación")
            Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 3)
            lblTitle.Text = "Instalando..."
            lblSubTitle.Text = "Espere mientras se completa la descarga e instalación del programa..."
            lblStatus.Text = "Esperando..."
            Text = "Instalador " & Instructive_Package_AssemblyName
        End If
    End Sub

    Sub SetCurrentStatus(ByVal Status As String)
        lblStatus.Text = Status
        AddToLog("Status: ", Status)
    End Sub

    Sub LoadSTUB()
        If CanOverwrite = True Then
            SetCurrentStatus("Leyendo datos precargados...")
            Try
                FileOpen(1, Application.ExecutablePath, OpenMode.Binary, OpenAccess.Read)
                Dim stubb As String = Space(LOF(1))
                Dim FileSplit = "|EXI|"
                FileGet(1, stubb)
                FileClose(1)
                Dim opt() As String = Split(stubb, FileSplit)
                AssemblyName = opt(1)
                AssemblyVersion = opt(2)
                InstructiveURL = opt(3)
                SetCurrentStatus("Datos precargados: " &
                                 vbCrLf & "     " & AssemblyName &
                                 vbCrLf & "     " & AssemblyVersion &
                                 vbCrLf & "     " & InstructiveURL)
            Catch ex As Exception
                AddToLog("[LoadSTUB@Debugger]Error: ", ex.Message, True)
                MsgBox("No se pudo leer el instructivo", MsgBoxStyle.Critical, "Información de Ensamblado")
                End
            End Try
        End If
    End Sub

    Sub LoadInstructive(Optional ByVal Offline As Boolean = False)
        Try
            SetCurrentStatus("Leyendo el instructivo... 1/3")
            Instructive_Package_Status = GetIniValue("Package", "Status", InstructiveFilePath)
            Instructive_Package_AssemblyName = GetIniValue("Package", "AssemblyName", InstructiveFilePath)
            Instructive_Package_AssemblyVersion = GetIniValue("Package", "AssemblyVersion", InstructiveFilePath)
            Instructive_Package_Description = GetIniValue("Package", "Description", InstructiveFilePath)
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
            Instructive_Installer_NeedToStart = GetIniValue("Installer", "NeedToStart", InstructiveFilePath)
            Instructive_Installer_InstallFolder = GetIniValue("Installer", "InstallFolder", InstructiveFilePath)
            Instructive_Installer_EULA = GetIniValue("Installer", "EULA", InstructiveFilePath)
            Instructive_Installer_Installer = GetIniValue("Installer", "Installer", InstructiveFilePath)
            Instructive_Installer_AfterInstall = GetIniValue("Installer", "AfterInstall", InstructiveFilePath)
            Instructive_Installer_AfterUninstall = GetIniValue("Installer", "AfterUninstall", InstructiveFilePath)
            Instructive_Installer_InstallPackage = GetIniValue("Installer", "InstallPackage", InstructiveFilePath)
            SetCurrentStatus("Leyendo el instructivo... 3/3")
            Instructive_HelpLinks_TelemetryPost = GetIniValue("HelpLinks", "TelemetryPost", InstructiveFilePath)
            Instructive_HelpLinks_ChangeLogLink = GetIniValue("HelpLinks", "ChangeLogLink", InstructiveFilePath)
            Instructive_HelpLinks_UseGuide = GetIniValue("HelpLinks", "UseGuide", InstructiveFilePath)
            Instructive_HelpLinks_AppAbout = GetIniValue("HelpLinks", "AppAbout", InstructiveFilePath)
            Instructive_HelpLinks_Contact = GetIniValue("HelpLinks", "Contact", InstructiveFilePath)

            If Offline = False Then
                'VERIFICAR QUE LOS DATOS ESTEN CORRECTOS
                If Instructive_Package_AssemblyName = Nothing Or Instructive_Package_AssemblyVersion = Nothing Or Instructive_Installer_EULA = Nothing Or Instructive_Installer_InstallPackage = Nothing Then
                    OfflineMode()
                    Exit Sub
                End If
            End If

            'APLICANDO VARIABLES avanzado
            shObj = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"))
            DownloadedZipPackage = DIRTemp & "\" & AssemblyName & "_" & Instructive_Package_AssemblyVersion & ".zip"
        Catch ex As Exception
            AddToLog("[LoadInstructive@Main]Error: ", ex.Message, True)
            MsgBox("No se logró entender la información dentro del instructivo", MsgBoxStyle.Critical, "Instructivo")
            Complementos.Closing()
        End Try
        If Offline = False Then
            SetCurrentStatus("Esperando...")
            'DEFINICIONES PARA LA INSTALACION ----------
            WhereDoIInstall()
            PreInstall()
        End If
    End Sub

    Sub PreInstall()
        Try
            SetCurrentStatus("Comprobando registros por instalación previa...")
            If Instructive_Package_IsComponent = "False" Then 'Si es componente no se comprueba su previa existencia
                If IsUninstall = False Then 'Si es modo instalacion (no es una desinstalacion)
                    If My.Computer.FileSystem.FileExists(ExePackage) = True Then 'Si existe entonces se activa el Modo Asistente
                        If RegistradorInstalacion IsNot Nothing Then
                            AssistantMode()
                            Exit Sub
                        Else
                            InstallIt()
                        End If
                    Else
                        InstallIt()
                    End If
                Else 'si es desinstalacion, entonces desinstala
                    UninstallIt(True)
                    Exit Sub
                End If
            Else
                IsComponent()
                If IsUninstall = False Then
                    If My.Computer.FileSystem.FileExists(ExePackage) = True Then 'Si existe entonces se activa el Modo Asistente
                        If RegistradorInstalacion IsNot Nothing Then
                            AssistantMode()
                            Exit Sub
                        Else
                            InstallIt()
                        End If
                    Else
                        InstallIt()
                    End If
                Else 'si es desinstalacion, entonces desinstala
                    UninstallIt(True)
                    Exit Sub
                End If
            End If
        Catch ex As Exception
            AddToLog("[PreAssistant@Main]Error: ", ex.Message, True)
        End Try
    End Sub

    Sub EULAAgree()
        Try
            If IsForced = False Then
                If Instructive_Installer_EULA <> "NULL" Then
                    If Instructive_Installer_EULA.Contains(";") Then
                        Dim Args() As String = Instructive_Installer_EULA.Split(";")
                        If Args(0) = "True" Then
                            If Args(1) <> "NULL" Then
                                Process.Start(Args(1))
                                If IsSilence = False Then
                                    If MessageBox.Show("Se ha abierto una pestaña de tu navegador con el EULA." & vbCrLf & vbCrLf & "¿Acepta el EULA ubicado en " & Args(1) & "?", "EULA", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                                        MsgBox("Debes aceptar el EULA para poder continuar con la instalación.", MsgBoxStyle.Critical, "EULA")
                                        Complementos.Closing()
                                        Exit Sub
                                    End If
                                End If
                            Else
                                If IsSilence = False Then
                                    Dim EULA_Content As String = My.Resources.EULA
                                    EULA_Content = EULA_Content.Replace("%assemblyname%", AssemblyName)
                                    EULA_Content = EULA_Content.Replace("%author%", Instructive_Package_Company)
                                    EULA_Content = EULA_Content.Replace("%weburl%", Instructive_Package_WebUrl)
                                    If MessageBox.Show(EULA_Content &
                                                       vbCrLf & vbCrLf & "Accept the EULA??", "EULA", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                                        MsgBox("Debes aceptar el EULA para poder continuar con la instalación.", MsgBoxStyle.Critical, "EULA")
                                        Complementos.Closing()
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            AddToLog("[EULAAgree@Main]Error: ", ex.Message, True)
            MsgBox("Error al aplicar el EULA" & vbCrLf & ex.Message, MsgBoxStyle.Critical, "EULA")
            Complementos.Closing()
        End Try
    End Sub
    Sub AskForInstall()
        Try
            If IsReinstall = False Then
                If IsForced = False Then
                    If IsSilence = False Then
                        If MessageBox.Show("Do you want to install " & AssemblyName & " in your computer?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                            Complementos.Closing()
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            AddToLog("[AskForInstall@Main]Error: ", ex.Message, True)
        End Try
    End Sub
    Sub InstallIt()
        InstallMode()
        SetCurrentStatus("Instalar")
        EULAAgree()
        Try
            If IsForced = False Then
                If IsReinstall = False Then
                    If Instructive_Installer_EnableDowngrade = "True" Then
                        Dim InstructiveVersionCheck As String = GetIniValue("Versions", AssemblyVersion, InstructiveFilePath)
                        If InstructiveVersionCheck <> Nothing Then
                            If (AssemblyVersion = Instructive_Package_AssemblyVersion) = False Then
                                If IsSilence = False Then
                                    If MessageBox.Show("La versión actual recomendada es " & Instructive_Package_AssemblyVersion & " pero el instalador está configurado con la versión " & AssemblyVersion &
                                                   vbCrLf & vbCrLf & "¿Desea descargar e instalar la versión recomendada?" &
                                                   vbCrLf & vbCrLf & "Si: Instalar versión recomendada (" & Instructive_Package_AssemblyVersion & ")." &
                                                   vbCrLf & "No: Instalar versión del instalador (" & AssemblyVersion & ").", "Control de Versión", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                                        Instructive_Installer_InstallPackage = GetIniValue("Versions", AssemblyVersion, InstructiveFilePath)
                                    Else
                                        lblInfo.Text = AssemblyName & " " & Instructive_Package_AssemblyVersion
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
            SetCurrentStatus("Descargando el paquete de instalación..." &
                             vbCrLf & "     " & Instructive_Installer_InstallPackage)
            ProgressBarStatus.Style = ProgressBarStyle.Blocks
            DownloadInstallPackageURI = New Uri(Instructive_Installer_InstallPackage)
            DownloadInstallPackage.DownloadFileAsync(DownloadInstallPackageURI, DownloadedZipPackage)
        Catch ex As Exception
            AddToLog("[InstallIt@Main]Error: ", ex.Message, True)
            MsgBox("No se pudo descargar el instructivo" & vbCrLf & ex.Message, MsgBoxStyle.Critical, "Instructivo")
            Complementos.Closing()
        End Try
    End Sub

    Private Sub DownloadInstallPackage_DownloadFileCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs) Handles DownloadInstallPackage.DownloadFileCompleted
        Install()
    End Sub
    Private Sub DownloadInstructive_DownloadFileCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs) Handles DownloadInstructive.DownloadFileCompleted
        LoadInstructive()
    End Sub
    Private Sub Desinstalar_Click(sender As Object, e As EventArgs) Handles Button2.Click
        UninstallIt(True)
    End Sub
    Private Sub Reinstalar_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Reinstall()
    End Sub
    Private Sub AppIcon_Click(sender As Object, e As EventArgs) Handles AppIcon.Click
        Try
            If MessageBox.Show("¿Iniciar " & Instructive_Package_PackageName & "?", "Iniciar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                Process.Start(ExePackage)
            End If
        Catch ex As Exception
            AddToLog("[AppIcon_Click@Main]Error: ", ex.Message, True)
        End Try
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
    Sub WhereDoIInstall()
        SetCurrentStatus("Aplicando las variables de instalación...")
        Try
            x32bits = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" & AssemblyName 'Usado para 64 en 64 tambien.
            x64x32bits = "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" & AssemblyName '32 en un pc 64
            'Es util saber que WOW6432Node se usa para aplicaciones de 32 bits en equipos de 64.
            'https://docs.microsoft.com/es-es/troubleshoot/windows-client/application-management/wow6432node-registry-key-present-32-bit-machine
            Dim UbicacionFinal As String
            Dim EsProgramFiles As Boolean = True
            Instructive_Installer_InstallFolder.Replace(Environment.UserName, "%username%")
            If Instructive_Installer_InstallFolder.Contains("%username%") Then
                EsProgramFiles = False
                Instructive_Installer_InstallFolder = Instructive_Installer_InstallFolder.Replace("%username%", Environment.UserName)
            ElseIf Instructive_Installer_InstallFolder.Contains("%programfiles%") Then
                EsProgramFiles = True
                Instructive_Installer_InstallFolder = Instructive_Installer_InstallFolder.Replace("%programfiles%", Nothing)
            End If
            If ArquitecturaSO = "32" Then 'Si el PC es x32 (x86)
                UbicacionFinal = "C:\Program Files" & Instructive_Installer_InstallFolder
                If RegistradorInstalacion Is Nothing Then
                    Registry.LocalMachine.CreateSubKey(x32bits)
                End If
                RegistradorInstalacion = Registry.LocalMachine.OpenSubKey(x32bits, True)
                If IsSilence = False Then
                    If Instructive_Package_BitsArch = "64" Then 'Si el PC es de x32 pero el programa es de x64
                        MsgBox("El programa por instalar requiere de un procesador de 64bits y no de 32bits", MsgBoxStyle.Critical, "No se puede instalar")
                        End 'END_PROGRAM
                    End If
                End If
            ElseIf ArquitecturaSO = "64" Then 'Si el PC es x64
                If Instructive_Package_BitsArch = "32" Then 'Si el PC es de x64 pero el programa es de x32
                    UbicacionFinal = "C:\Program Files (x86)" & Instructive_Installer_InstallFolder
                    If RegistradorInstalacion Is Nothing Then
                        Registry.LocalMachine.CreateSubKey(x64x32bits)
                    End If
                    RegistradorInstalacion = Registry.LocalMachine.OpenSubKey(x64x32bits, True)
                Else 'Si el PC es de x64 y el programa tambien es de x64
                    UbicacionFinal = "C:\Program Files" & Instructive_Installer_InstallFolder
                    If RegistradorInstalacion Is Nothing Then
                        Registry.LocalMachine.CreateSubKey(x32bits) 'no necesita el nodo. esta variables sirve para un 32 o 64 nativo.
                    End If
                    RegistradorInstalacion = Registry.LocalMachine.OpenSubKey(x32bits, True)
                End If
            Else 'Si nada aplica, se instala donde dios quiera
                UbicacionFinal = "C:\Program Files" & Instructive_Installer_InstallFolder
                If RegistradorInstalacion Is Nothing Then
                    Registry.LocalMachine.CreateSubKey(x32bits)
                End If
                RegistradorInstalacion = Registry.LocalMachine.OpenSubKey(x32bits, True)
            End If
            If EsProgramFiles = True Then
                InstallerPathBuilder = UbicacionFinal
            Else
                InstallerPathBuilder = Instructive_Installer_InstallFolder
            End If
            ExePackage = InstallerPathBuilder & "\" & Instructive_Package_PackageName 'Indicamos la ruta del ejecutable que se esta instalando
            Try
                Dim Icono As Icon = Icon.ExtractAssociatedIcon(ExePackage)
                AppIcon.Image = Icono.ToBitmap
                Me.Icon = Icono
                AppIcon.Visible = True
            Catch ex As Exception
                AppIcon.Visible = False
                AddToLog("[WhereDoIInstall(1)@Main]Error: ", ex.Message, False)
            End Try
        Catch ex As Exception
            AddToLog("[WhereDoIInstall@Main]Error: ", ex.Message, True)
        End Try
    End Sub

    Sub OfflineMode()
        OfflineRegistry()
        Try
            'Verificar si existe la instalacion
            If RegistradorInstalacion IsNot Nothing Then
                '   si existe, asistente. SOLO DESINSTALAR
                AssistantMode()
                Button1.Enabled = False
                Exit Sub
            Else
                '   si no existe, salir.
                MsgBox("Instructivo dañado.", MsgBoxStyle.Critical, "Instructivo")
                End
            End If
        Catch ex As Exception
            AddToLog("[OfflineMode@Main]Error: ", ex.Message, True)
        End Try
    End Sub
    Sub OfflineRegistry()
        Try
            'OBTENER EL REGISTRO CORRECTO DE INSTALACION PARA LEER DATOS
            Dim RegistroVerificador As RegistryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" & AssemblyName, False)
            If RegistroVerificador Is Nothing Then
                RegistradorInstalacion = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" & AssemblyName, True)
            Else
                RegistradorInstalacion = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" & AssemblyName, True)
            End If
        Catch ex As Exception
            AddToLog("[OfflineRegistry(0)@Main]Error: ", ex.Message, True)
        End Try
        Try
            Dim Contenido As String = "# EX Installer Instructive File" &
                vbCrLf & "[Package]" &
                vbCrLf & "Status=" & RegistradorInstalacion.GetValue("Instructive_Package_Status") &
                vbCrLf & "AssemblyName=" & RegistradorInstalacion.GetValue("Instructive_Package_AssemblyName") &
                vbCrLf & "AssemblyVersion=" & RegistradorInstalacion.GetValue("Instructive_Package_AssemblyVersion") &
                vbCrLf & "Description=" & RegistradorInstalacion.GetValue("Instructive_Package_Description") &
                vbCrLf & "Company=" & RegistradorInstalacion.GetValue("Instructive_Package_Company") &
                vbCrLf & "WebUrl=" & RegistradorInstalacion.GetValue("Instructive_Package_WebUrl") &
                vbCrLf & "PackageName=" & RegistradorInstalacion.GetValue("Instructive_Package_PackageName") &
                vbCrLf & "IsComponent=" & RegistradorInstalacion.GetValue("Instructive_Package_IsComponent") &
                vbCrLf & "InstallerVersion=" & RegistradorInstalacion.GetValue("Instructive_Package_InstallerVersion") &
                vbCrLf & "BitsArch=" & RegistradorInstalacion.GetValue("Instructive_Package_BitsArch") &
                vbCrLf & "[Installer]" &
                vbCrLf & "Status=" & RegistradorInstalacion.GetValue("Instructive_Installer_Status") &
                vbCrLf & "EnableDowngrade=" & RegistradorInstalacion.GetValue("Instructive_Installer_EnableDowngrade") &
                vbCrLf & "NeedRestart=" & RegistradorInstalacion.GetValue("Instructive_Installer_NeedRestart") &
                vbCrLf & "NeedStartUp=" & RegistradorInstalacion.GetValue("Instructive_Installer_NeedStartUp") &
                vbCrLf & "NeedElevateAccess=" & RegistradorInstalacion.GetValue("Instructive_Installer_NeedElevateAccess") &
                vbCrLf & "NeedToStart=" & RegistradorInstalacion.GetValue("Instructive_Installer_NeedToStart") &
                vbCrLf & "InstallFolder=" & RegistradorInstalacion.GetValue("Instructive_Installer_InstallFolder") &
                vbCrLf & "EULA=" & RegistradorInstalacion.GetValue("Instructive_Installer_EULA") &
                vbCrLf & "Installer=" & RegistradorInstalacion.GetValue("Instructive_Installer_Installer") &
                vbCrLf & "AfterInstall=" & RegistradorInstalacion.GetValue("Instructive_Installer_AfterInstall") &
                vbCrLf & "AfterUninstall=" & RegistradorInstalacion.GetValue("Instructive_Installer_AfterUninstall") &
                vbCrLf & "InstallPackage=" & RegistradorInstalacion.GetValue("Instructive_Installer_InstallPackage") &
                vbCrLf & "[HelpLinks]" &
                vbCrLf & "TelemetryPost=" & RegistradorInstalacion.GetValue("Instructive_HelpLinks_TelemetryPost") &
                vbCrLf & "ChangeLogLink=" & RegistradorInstalacion.GetValue("Instructive_HelpLinks_ChangeLogLink") &
                vbCrLf & "UseGuide=" & RegistradorInstalacion.GetValue("Instructive_HelpLinks_UseGuide") &
                vbCrLf & "AppAbout=" & RegistradorInstalacion.GetValue("Instructive_HelpLinks_AppAbout") &
                vbCrLf & "Contact=" & RegistradorInstalacion.GetValue("Instructive_HelpLinks_Contact")
            If My.Computer.FileSystem.FileExists(InstructiveFilePath) = True Then
                My.Computer.FileSystem.DeleteFile(InstructiveFilePath)
            End If
            My.Computer.FileSystem.WriteAllText(InstructiveFilePath, Contenido, False)
            LoadInstructive(True)
            WhereDoIInstall()
        Catch ex As Exception
            AddToLog("[OfflineRegistry(1)@Main]Error: ", ex.Message, True)
        End Try
    End Sub

    Private Sub Main_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        End
    End Sub
End Class