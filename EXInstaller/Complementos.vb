Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.Win32
Imports System.IO
Imports System.IO.Compression
Module Instalador

#Region "Installer"
    Sub Install() 'Instalamos el paquete descargado indicado por el instructivo
        shObj = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"))
        Main.SetCurrentStatus("Comparando los datos del equipo...")
        Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 3)
        TaskbarProgress.SetState(Main.Handle, TaskbarProgress.TaskbarStates.Indeterminate)
        DownloadedZipPackage = DIRTemp & "\" & AssemblyName & "_" & Instructive_Package_AssemblyVersion & ".zip"
        Try
            Main.SetCurrentStatus("Creando los directorios para la instalacion...")
            If Instructive_Package_IsComponent = "False" Then 'Si es componente, no se creara ni eliminara el directorio de instalacion, deberia existir previamente
                If My.Computer.FileSystem.DirectoryExists(InstallerPathBuilder) = True Then
                    My.Computer.FileSystem.DeleteDirectory(InstallerPathBuilder, FileIO.DeleteDirectoryOption.DeleteAllContents)
                End If
                If My.Computer.FileSystem.DirectoryExists(InstallerPathBuilder) = False Then
                    My.Computer.FileSystem.CreateDirectory(InstallerPathBuilder)
                End If
            End If
            'INSTALACION (COPIADO) DE FICHEROS EN ZIP A UBICACION FINAL DE INSTALACION ----------
            Try
                Main.SetCurrentStatus("Copiando los datos...")
                IO.Directory.CreateDirectory(InstallerPathBuilder) 'Creamos forzosamente el directorio de instalacion
                ZipFile.ExtractToDirectory(DownloadedZipPackage, InstallerPathBuilder)
            Catch ex As Exception
                AddToLog("[Install(CopyingData)@Complementos]Error: ", ex.Message, True)
                MsgBox("Algo fallo al instalar el programa", MsgBoxStyle.Critical, "Instalación")
                Complementos.Closing()
            End Try
            If Instructive_Package_IsComponent = "False" Then 'Si es componente, no se crearan accesos directos ni nada, es solo un componente.
                'CREACION DEL ACCESO DIRECTO EN LA CARPETA DE PROGRAMAS DE WINDOWS ----------
                Try
                    Main.SetCurrentStatus("Creando los datos post-instalacion...")
                    Dim StartUpWindowsFolder As String = "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\" & Instructive_Package_Company & "\" & Instructive_Package_PackageName
                    If My.Computer.FileSystem.DirectoryExists(StartUpWindowsFolder) = False Then
                        My.Computer.FileSystem.CreateDirectory(StartUpWindowsFolder)
                    End If
                    If My.Computer.FileSystem.FileExists(StartUpWindowsFolder & "\" & Instructive_Package_PackageName & ".lnk") = True Then
                        My.Computer.FileSystem.DeleteFile(StartUpWindowsFolder & "\" & Instructive_Package_PackageName & ".lnk")
                    End If
                    Dim WSHShell As Object = CreateObject("WScript.Shell")
                    Dim Shortcut As Object
                    Shortcut = WSHShell.CreateShortcut(StartUpWindowsFolder & "\" & Instructive_Package_PackageName & ".lnk")
                    Shortcut.IconLocation = ExePackage & ",0"
                    Shortcut.TargetPath = ExePackage
                    Shortcut.WindowStyle = 1
                    Shortcut.Description = "Run " & Instructive_Package_PackageName
                    Shortcut.Save()
                Catch ex As Exception
                    AddToLog("[Install(CreateShorcoutAndWindowsFolder)@Complementos]Error: ", ex.Message, True)
                End Try
                'CREACION DEL ACCESO DIRECTO EN EL ESCRITORIO ----------
                Try
                    Dim DesktopShortcut As String = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\" & Instructive_Package_PackageName & ".lnk"
                    If My.Computer.FileSystem.FileExists(DesktopShortcut) = True Then
                        My.Computer.FileSystem.DeleteFile(DesktopShortcut)
                    End If
                    Dim WSHShell As Object = CreateObject("WScript.Shell")
                    Dim Shortcut As Object = WSHShell.CreateShortcut(DesktopShortcut)
                    Shortcut.IconLocation = ExePackage & ",0"
                    Shortcut.TargetPath = ExePackage
                    'Shortcut.Arguments = "Debugger.DIRInstallFolder & "\" & Debugger.AssemblyName & ".exe""
                    Shortcut.WindowStyle = 1
                    Shortcut.Description = "Start " & Instructive_Package_PackageName
                    Shortcut.Save()
                Catch ex As Exception
                    AddToLog("[Install(CreateShorcoutDesktop)@Complementos]Error: ", ex.Message, True)
                End Try
                'INJECION DEL INSTRUCTIVO AL ASISTENTE POST-INSTALACION ----------
                Try
                    Dim stub As String
                    Const FS1 As String = "|EXI|"
                    Dim Temp As String = InstallerPathBuilder & "\uninstall.exe"
                    Dim bytesEXE As Byte() = System.IO.File.ReadAllBytes(Application.ExecutablePath)
                    File.WriteAllBytes(Temp, bytesEXE)
                    FileOpen(1, Temp, OpenMode.Binary, OpenAccess.Read, OpenShare.Default)
                    stub = Space(LOF(1))
                    FileGet(1, stub)
                    FileClose(1)
                    FileOpen(1, Temp, OpenMode.Binary, OpenAccess.ReadWrite, OpenShare.Default)
                    FilePut(1, stub & FS1 & Instructive_Package_AssemblyName & FS1 & Instructive_Package_AssemblyVersion & FS1 & InstructiveURL & FS1)
                    FileClose(1)
                Catch ex As Exception
                    AddToLog("[CreateAndStubTheUninstaller@Complementos]Error: ", ex.Message, True)
                End Try
            End If
            CreateRegistry()
        Catch ex As Exception
            AddToLog("[Install@Complementos]Error: ", ex.Message, True)
        End Try
    End Sub
    Sub CreateRegistry()
        Main.SetCurrentStatus("Registrando la instalacion...")
        If Instructive_Package_IsComponent = False Then 'Si es componente no se creara un desinstalador propio para el componente
            Try
                'CREACION DEL REGISTRO DE INSTALACION ----------
                RegistradorInstalacion.SetValue("InstallDate", DateTime.Now.ToString("dd/MM/yyyy"), RegistryValueKind.String)
                RegistradorInstalacion.SetValue("InstallLocation", InstallerPathBuilder, RegistryValueKind.ExpandString)
                RegistradorInstalacion.SetValue("Size", FormatBytes(PackageSize), RegistryValueKind.String)
                RegistradorInstalacion.SetValue("Comments", Instructive_Package_PackageName & " Official Software by " & Instructive_Package_Company, RegistryValueKind.String)
                RegistradorInstalacion.SetValue("DisplayIcon", ExePackage, RegistryValueKind.String)
                RegistradorInstalacion.SetValue("DisplayName", Instructive_Package_AssemblyName, RegistryValueKind.String)
                RegistradorInstalacion.SetValue("DisplayVersion", Instructive_Package_AssemblyVersion, RegistryValueKind.String)
                RegistradorInstalacion.SetValue("HelpLink", Instructive_HelpLinks_UseGuide, RegistryValueKind.String)
                RegistradorInstalacion.SetValue("Publisher", Instructive_Package_Company, RegistryValueKind.String)
                RegistradorInstalacion.SetValue("Contact", Instructive_HelpLinks_Contact, RegistryValueKind.String)
                RegistradorInstalacion.SetValue("Readme", Instructive_HelpLinks_AppAbout, RegistryValueKind.String)
                RegistradorInstalacion.SetValue("URLInfoAbout", Instructive_HelpLinks_AppAbout, RegistryValueKind.String)
                RegistradorInstalacion.SetValue("URLUpdateInfo", Instructive_HelpLinks_ChangeLogLink, RegistryValueKind.String)
                Try
                    Dim TotalSizeVal As String = Val(PackageSize)
                    RegistradorInstalacion.SetValue("EstimatedSize", TotalSizeVal.Remove(TotalSizeVal.Length - 3), RegistryValueKind.DWord)
                Catch
                End Try
                RegistradorInstalacion.SetValue("ModifyPath", InstallerPathBuilder & "\uninstall.exe /Assistant", RegistryValueKind.ExpandString)
                RegistradorInstalacion.SetValue("UninstallPath", InstallerPathBuilder & "\uninstall.exe /Uninstall", RegistryValueKind.ExpandString)
                RegistradorInstalacion.SetValue("UninstallString", """" & InstallerPathBuilder & "\uninstall.exe" & """" & " /Uninstall", RegistryValueKind.ExpandString)
                RegistradorInstalacion.SetValue("QuietUninstallString", """" & InstallerPathBuilder & "\uninstall.exe" & """" & " /S", RegistryValueKind.String)
            Catch ex As Exception
                AddToLog("[CreateInstallRegistry@Complementos]Error: ", ex.Message, True)
            End Try
        End If
        Try
            'VERIFICACION DE SI EL PROGRAMA NECESITA PERMISOS DE ADMINISTRADOR ----------
            If Instructive_Installer_NeedElevateAccess.StartsWith("True") Then
                Dim REGISTRADOR As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers", True)
                REGISTRADOR.SetValue(ExePackage, "RUNASADMIN", RegistryValueKind.String)
            End If
        Catch ex As Exception
            AddToLog("[CreateNeedElevateAccessRegistry@Complementos]Error: ", ex.Message, True)
        End Try
        Try
            'VERIFICACION DE SI EL PROGRAMA NECESITA INICIARSE CON WINDOWS ----------
            Dim REGISTRADOR As RegistryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", True)
            If Instructive_Installer_NeedStartUp.StartsWith("True") Then
                If Instructive_Installer_NeedStartUp.Contains(";") Then
                    If REGISTRADOR Is Nothing Then
                        Registry.LocalMachine.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run")
                    End If
                    Dim Args() As String = Instructive_Installer_NeedStartUp.Split(";")
                    If Args(1) = "NULL" Then
                        REGISTRADOR.SetValue(Instructive_Package_AssemblyName, """" & ExePackage & """", RegistryValueKind.ExpandString)
                    Else
                        REGISTRADOR.SetValue(Instructive_Package_AssemblyName, """" & ExePackage & """" & " " & Args(1), RegistryValueKind.ExpandString)
                    End If
                Else
                    REGISTRADOR.SetValue(Instructive_Package_AssemblyName, ExePackage, RegistryValueKind.ExpandString)
                End If
            End If
        Catch ex As Exception
            AddToLog("[CreateNeedStartupRegistry@Complementos]Error: ", ex.Message, True)
        End Try
        FinishInstall()
    End Sub
    Sub FinishInstall()
        TaskbarProgress.SetState(Main.Handle, TaskbarProgress.TaskbarStates.Normal)
        Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 5)
        Main.SetCurrentStatus("Instalacion finalizada correctamente.")
        Dim FinishedStatus As String = "Se ha instalado correctamente"
        If IsUninstall = True Then
            FinishedStatus = "Se ha desinstalado correctamente"
        ElseIf IsReinstall = True Then
            FinishedStatus = "Se ha reinstalado correctamente"
        End If
        MsgBox(FinishedStatus, MsgBoxStyle.Information, "EX Installer")
        'VERIFICACION DE SI EL PROGRAMA NECESITA UN REINICIO DEL EQUIPO ----------
        If Instructive_Installer_NeedRestart = "True" Then
            If MessageBox.Show("El programa requiere un reinicio del equipo." & vbCrLf & "¿Quiere reiniciar ahora?", "Reinicio pendiente", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                Process.Start("shutdown.exe", "/r")
            End If
        Else
            Try
                'VERIFICACION DE SI EL PROGRAMA NECESITA INICIARSE DESPUES DE LA INSTALACION ----------
                If Instructive_Installer_NeedToStart.StartsWith("True") Then
                    If Instructive_Installer_NeedToStart.Contains(";") Then
                        Dim Args() As String = Instructive_Installer_NeedToStart.Split(";")
                        If Args(1) = "NULL" Then
                            Process.Start(ExePackage)
                        Else
                            Process.Start(ExePackage, Args(1))
                        End If
                    End If
                End If
            Catch ex As Exception
                AddToLog("[CreateNeedStartupRegistry@Complementos]Error: ", ex.Message, True)
            End Try
        End If
            Closing()
    End Sub
#End Region

#Region "Uninstaller"
    'El Uninstaller hace todo lo contrario al Installer
    Sub Uninstall()
        'CONFIRMACION PARA LA DESINSTALACION ----------
        If MessageBox.Show("¿Want to uninstall the Software called " & Instructive_Package_PackageName & "?", "Confirm Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
            Closing()
            Exit Sub
        End If
        Try
            ExePackage = InstallerPathBuilder & "\" & Instructive_Package_PackageName & ".exe"
            'ELIMINACION DE LA UBICACION DE INSTALACION ----------
            Try
                Main.SetCurrentStatus("Eliminando los directorios de instalacion...")
                If My.Computer.FileSystem.DirectoryExists(InstallerPathBuilder) = True Then
                    My.Computer.FileSystem.DeleteDirectory(InstallerPathBuilder, FileIO.DeleteDirectoryOption.DeleteAllContents)
                End If
            Catch ex As Exception
                AddToLog("[Uninstall(DeleteInstallationFolder)@Complementos]Error: ", ex.Message, True)
            End Try
            Main.SetCurrentStatus("Eliminando los datos post-instalacion...")
            'ELIMINACION DEL ACCESO DIRECTO DE LA CARPETA PROGRAMAS DE WINDOWS ----------
            Try
                Dim StartUpWindowsFolder As String = "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\" & Instructive_Package_Company & "\" & Instructive_Package_PackageName
                If My.Computer.FileSystem.FileExists(StartUpWindowsFolder & "\" & Instructive_Package_PackageName & ".lnk") = True Then
                    My.Computer.FileSystem.DeleteFile(StartUpWindowsFolder & "\" & Instructive_Package_PackageName & ".lnk")
                End If
            Catch ex As Exception
                AddToLog("[Uninstall(DeleteShorcoutAndWindowsFolder)@Complementos]Error: ", ex.Message, True)
            End Try
            'ELIMINACION DEL ACCESO DIRECTO EN EL ESCRITORIO ----------
            Try
                Dim DesktopShortcut As String = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\" & Instructive_Package_PackageName & ".lnk"
                If My.Computer.FileSystem.FileExists(DesktopShortcut) = True Then
                    My.Computer.FileSystem.DeleteFile(DesktopShortcut)
                End If
            Catch ex As Exception
                AddToLog("[Uninstall(DeleteShorcoutDesktop)@Complementos]Error: ", ex.Message, True)
            End Try
        Catch ex As Exception
            AddToLog("[Uninstall@Complementos]Error: ", ex.Message, True)
        End Try
        DeleteRegistry()
    End Sub
    Sub DeleteRegistry()
        Try
            Dim regKey As RegistryKey
            If ArquitecturaSO = "32" Then
                regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", True)
            ElseIf ArquitecturaSO = "64" Then
                If Instructive_Package_BitsArch = "32" Then
                    regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", True)
                Else
                    regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall", True)
                End If
            Else
                regKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", True)
            End If
            regKey.DeleteSubKey(Instructive_Package_AssemblyName)
        Catch ex As Exception
            AddToLog("[DeleteInstallRegistry@Complementos]Error: ", ex.Message, True)
        End Try
        Try
            If Instructive_Installer_NeedElevateAccess.StartsWith("True") Then
                Dim REGISTRADOR As RegistryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers", True)
                REGISTRADOR.DeleteValue(ExePackage)
            End If
        Catch ex As Exception
            AddToLog("[DeleteElevateAccessRegistry@Complementos]Error: ", ex.Message, True)
        End Try
        Try
            If Instructive_Installer_NeedStartUp.StartsWith("True") Then
                Dim REGISTRADOR As RegistryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", True)
                If Instructive_Installer_NeedStartUp.Contains(";") Then
                    REGISTRADOR.DeleteValue(Instructive_Package_AssemblyName)
                Else
                    REGISTRADOR.DeleteValue(Instructive_Package_AssemblyName)
                End If
            End If
        Catch ex As Exception
            AddToLog("[DeleteNeedStartupRegistry@Complementos]Error: ", ex.Message, True)
        End Try
        FinishUninstall()
    End Sub
    Sub FinishUninstall()
        Main.SetCurrentStatus("Desinstalacion finalizada correctamente.")
        MsgBox("Se ha desinstalado correctamente", MsgBoxStyle.Information, "Desinstalacion Completada")
        Closing()
    End Sub
    'Problemas actuales
    '   1) No se elimina la ruta de instalacion porque "Algo la esta usando".
    '   2) El uninstaller.exe falla y no se xq y ademas algunas veces nos se elimina
    '       OSEA: por asi decirlo, no lanza el asistente
    '   3) el "asistente" al desinstalar tampoco elimina la ruta.
    '   Posibles causas
    '      1) Las variables estan mal ordenadas
    '       2) Dios no tiene piedad
    '       3) El modo asistente inicia dentro de la carpeta de instalacion y no hace el "puente"
    '   Posible soluciones
    '       morir
    '       Cuando se inicie uninstall.exe AUTOMATICAMENTE se copiara a otra ubicacion fuera de la carpeta de instalacion para servir como asistente
    '           segun yo, cuando se inicia uninstaller.exe no es hasta que el usuario hace algo -precionar uno de los dos botones- hasta que se copie
    '           a  la ubicacion temporal.
    '       rediseñar todo el inicio del programa, esto seria lo mejor, asi evito morir cada vez.

    '30/06/2021 05:55 PM Chile
    '
#End Region

End Module
Module Complementos

    Sub AddToLog(ByVal Header As String, ByVal content As String, Optional ByVal flag As Boolean = False)
        Try
            Dim Overwrite As Boolean = False
            If My.Computer.FileSystem.FileExists(DIRCommons & "\Install.log") = True Then
                Overwrite = True
            End If
            Dim LogContent As String = "(" & DateTime.Now.ToString("hh:mm:ss tt dd/MM/yyyy") & ")"
            If flag = True Then
                LogContent &= "[!!!] " & Header & content
            Else
                LogContent &= Header & content
            End If
            Console.WriteLine(LogContent)
            If CanSaveLog = True Then
                My.Computer.FileSystem.WriteAllText(DIRCommons & "\Install.log", LogContent & vbCrLf, Overwrite)
            End If
        Catch
        End Try
    End Sub

    Sub Closing()
        Try
            If My.Computer.FileSystem.DirectoryExists(DIRTemp) = True Then
                My.Computer.FileSystem.DeleteDirectory(DIRTemp, FileIO.DeleteDirectoryOption.DeleteAllContents)
            End If
            If My.Computer.FileSystem.FileExists(InstructiveFilePath) = True Then
                My.Computer.FileSystem.DeleteFile(InstructiveFilePath)
            End If
        Catch ex As Exception
            AddToLog("[Closing(DeletingTempFiles)@Complementos]Error: ", ex.Message, True)
        End Try
        AddToLog(vbCrLf & "Informe antes de cerrar", vbCrLf & vbCrLf & "#Registro de variables" &
                 vbCrLf & "DIRoot=" & DIRoot &
                 vbCrLf & "DIRCommons=" & DIRCommons &
                 vbCrLf & "DIRTemp=" & DIRTemp &
                 vbCrLf & "InstructiveFilePath=" & InstructiveFilePath &
                 vbCrLf & "StartParametros=" & StartParametros &
                 vbCrLf & "IsUninstall=" & IsUninstall &
                 vbCrLf & "IsReinstall=" & IsReinstall &
                 vbCrLf & "IsAssistant=" & IsAssistant &
                 vbCrLf & "InstallerPathBuilder=" & InstallerPathBuilder &
                 vbCrLf & "RegistradorInstalacion=" & RegistradorInstalacion.Name &
                 vbCrLf & "CanSaveLog=" & CanSaveLog &
                 vbCrLf & "x32bits=" & x32bits &
                 vbCrLf & "x64x32bits=" & x64x32bits &
                 vbCrLf & "ExePackage=" & ExePackage &
                 vbCrLf & "ArquitecturaSO=" & ArquitecturaSO &
                 vbCrLf & "PackageSize=" & PackageSize &
                 vbCrLf & "AssemblyName=" & AssemblyName &
                 vbCrLf & "AssemblyVersion=" & AssemblyVersion &
                 vbCrLf & "InstructiveURL=" & InstructiveURL &
                 vbCrLf & "CanOverwrite=" & CanOverwrite &
                 vbCrLf & "ExecutablePath=" & Application.ExecutablePath &
                 vbCrLf &
                 vbCrLf & "Instructive_Package_Status=" & Instructive_Package_Status &
                 vbCrLf & "Instructive_Package_AssemblyName=" & Instructive_Package_AssemblyName &
                 vbCrLf & "Instructive_Package_AssemblyVersion=" & Instructive_Package_AssemblyVersion &
                 vbCrLf & "Instructive_Package_Company=" & Instructive_Package_Company &
                 vbCrLf & "Instructive_Package_WebUrl=" & Instructive_Package_WebUrl &
                 vbCrLf & "Instructive_Package_PackageName=" & Instructive_Package_PackageName &
                 vbCrLf & "Instructive_Package_IsComponent=" & Instructive_Package_IsComponent &
                 vbCrLf & "Instructive_Package_InstallerVersion=" & Instructive_Package_InstallerVersion &
                 vbCrLf & "Instructive_Package_BitsArch=" & Instructive_Package_BitsArch &
                 vbCrLf & "Instructive_Installer_Status=" & Instructive_Installer_Status &
                 vbCrLf & "Instructive_Installer_EnableDowngrade=" & Instructive_Installer_EnableDowngrade &
                 vbCrLf & "Instructive_Installer_NeedRestart=" & Instructive_Installer_NeedRestart &
                 vbCrLf & "Instructive_Installer_NeedStartUp=" & Instructive_Installer_NeedStartUp &
                 vbCrLf & "Instructive_Installer_NeedElevateAccess=" & Instructive_Installer_NeedElevateAccess &
                 vbCrLf & "Instructive_Installer_InstallFolder=" & Instructive_Installer_InstallFolder &
                 vbCrLf & "Instructive_Installer_EULA=" & Instructive_Installer_EULA &
                 vbCrLf & "Instructive_Installer_Installer=" & Instructive_Installer_Installer &
                 vbCrLf & "Instructive_Installer_InstallPackage=" & Instructive_Installer_InstallPackage &
                 vbCrLf & "Instructive_HelpLinks_ChangeLogLink=" & Instructive_HelpLinks_ChangeLogLink &
                 vbCrLf & "Instructive_HelpLinks_UseGuide=" & Instructive_HelpLinks_UseGuide &
                 vbCrLf & "Instructive_HelpLinks_AppAbout=" & Instructive_HelpLinks_AppAbout &
                 vbCrLf & "Instructive_HelpLinks_Contact=" & Instructive_HelpLinks_Contact &
                 vbCrLf & vbCrLf & vbCrLf)
        End
    End Sub

    Dim DoubleBytes As Double
    Public Function FormatBytes(ByVal BytesCaller As ULong) As String
        Try
            Select Case BytesCaller
                Case Is >= 1099511627776
                    DoubleBytes = CDbl(BytesCaller / 1099511627776) 'TB
                    Return FormatNumber(DoubleBytes, 2) & " TB"
                Case 1073741824 To 1099511627775
                    DoubleBytes = CDbl(BytesCaller / 1073741824) 'GB
                    Return FormatNumber(DoubleBytes, 2) & " GB"
                Case 1048576 To 1073741823
                    DoubleBytes = CDbl(BytesCaller / 1048576) 'MB
                    Return FormatNumber(DoubleBytes, 2) & " MB"
                Case 1024 To 1048575
                    DoubleBytes = CDbl(BytesCaller / 1024) 'KB
                    Return FormatNumber(DoubleBytes, 2) & " KB"
                Case 0 To 1023
                    DoubleBytes = BytesCaller ' bytes
                    Return FormatNumber(DoubleBytes, 2) & " bytes"
                Case Else
                    Return Nothing
            End Select
        Catch
            Return Nothing
        End Try
    End Function
    <DllImport("kernel32")>
    Private Function GetPrivateProfileString(ByVal section As String, ByVal key As String, ByVal def As String, ByVal retVal As StringBuilder, ByVal size As Integer, ByVal filePath As String) As Integer
        'Use GetIniValue("KEY_HERE", "SubKEY_HERE", "filepath")
    End Function
    Public Function GetIniValue(ByVal section As String, ByVal key As String, ByVal filename As String, Optional ByVal defaultValue As String = Nothing) As String
        Dim sb As New StringBuilder(500)
        If GetPrivateProfileString(section, key, defaultValue, sb, sb.Capacity, filename) > 0 Then
            Return sb.ToString
        Else
            Return defaultValue
        End If
    End Function
End Module
Public Class TaskbarProgress
    Public Enum TaskbarStates
        Fail = 3
        NoProgress = 0
        Indeterminate = 1
        Normal = 2
        Paused = 8
    End Enum
    <ComImport(),
     Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Private Interface ITaskbarList3
        ' ITaskbarList
        <PreserveSig()>
        Sub HrInit()
        <PreserveSig()>
        Sub AddTab(ByVal hwnd As IntPtr)
        <PreserveSig()>
        Sub DeleteTab(ByVal hwnd As IntPtr)
        <PreserveSig()>
        Sub ActivateTab(ByVal hwnd As IntPtr)
        <PreserveSig()>
        Sub SetActiveAlt(ByVal hwnd As IntPtr)
        ' ITaskbarList2
        <PreserveSig()>
        Sub MarkFullscreenWindow(ByVal hwnd As IntPtr, ByVal fFullscreen As Boolean)
        ' ITaskbarList3
        <PreserveSig()>
        Sub SetProgressValue(ByVal hwnd As IntPtr, ByVal ullCompleted As UInt64, ByVal ullTotal As UInt64)
        <PreserveSig()>
        Sub SetProgressState(ByVal hwnd As IntPtr, ByVal state As TaskbarStates)
    End Interface
    <ComImport(),
     Guid("56fdf344-fd6d-11d0-958a-006097c9a090"),
     ClassInterface(ClassInterfaceType.None)>
    Private Class TaskbarInstances
    End Class
    Private Shared taskbarInstance As ITaskbarList3 = CType(New TaskbarInstances, ITaskbarList3)
    Private Shared taskbarSupported As Boolean = (Environment.OSVersion.Version >= New Version(6, 1))
    Public Shared Sub SetState(ByVal windowHandle As IntPtr, ByVal taskbarState As TaskbarStates)
        If taskbarSupported Then
            taskbarInstance.SetProgressState(windowHandle, taskbarState)
        End If
    End Sub
    Public Shared Sub SetValue(ByVal windowHandle As IntPtr, ByVal progressValue As Double, ByVal progressMax As Double)
        If taskbarSupported Then
            taskbarInstance.SetProgressValue(windowHandle, CType(progressValue, System.UInt64), CType(progressMax, System.UInt64))
        End If
    End Sub
End Class
Public Class WindowsApi
    Private Declare Function FlashWindowEx Lib "User32" (ByRef fwInfo As FLASHWINFO) As Boolean
    Public Enum FlashWindowFlags As UInt32
        FLASHW_STOP = 0
        FLASHW_CAPTION = 1
        FLASHW_TRAY = 2
        FLASHW_ALL = 3
        FLASHW_TIMER = 4
        FLASHW_TIMERNOFG = 12
    End Enum

    Public Structure FLASHWINFO
        Public cbSize As UInt32
        Public hwnd As IntPtr
        Public dwFlags As FlashWindowFlags
        Public uCount As UInt32
        Public dwTimeout As UInt32
    End Structure

    Public Shared Function FlashWindow(ByRef handle As IntPtr, ByVal FlashTitleBar As Boolean, ByVal FlashTray As Boolean, ByVal FlashCount As Integer) As Boolean
        If handle = Nothing Then Return False
        Try
            Dim fwi As New FLASHWINFO
            With fwi
                .hwnd = handle
                If FlashTitleBar Then .dwFlags = .dwFlags Or FlashWindowFlags.FLASHW_CAPTION
                If FlashTray Then .dwFlags = .dwFlags Or FlashWindowFlags.FLASHW_TRAY
                .uCount = CUInt(FlashCount)
                If FlashCount = 0 Then .dwFlags = .dwFlags Or FlashWindowFlags.FLASHW_TIMERNOFG
                .dwTimeout = 0
                .cbSize = CUInt(System.Runtime.InteropServices.Marshal.SizeOf(fwi))
            End With
            Return FlashWindowEx(fwi)
        Catch
            Return False
        End Try
    End Function
End Class