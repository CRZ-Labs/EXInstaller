Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.Win32
Imports System.IO
Module Instalador
    Dim shObj As Object = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"))
    Dim InstallerPathBuilder As String

#Region "Installer"
    Sub Install()
        Main.SetCurrentStatus("Comparando los datos del equipo...")
        Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 3)
        TaskbarProgress.SetState(Main.Handle, TaskbarProgress.TaskbarStates.Indeterminate)
        Try
            If ArquitecturaSO = "32" Then
                InstallerPathBuilder = "C:\Program Files" & Instructive_Installer_InstallFolder
            ElseIf ArquitecturaSO = "64" Then
                If Instructive_Package_BitsArch = "32" Then
                    InstallerPathBuilder = "C:\Program Files (x86)" & Instructive_Installer_InstallFolder
                Else
                    InstallerPathBuilder = "C:\Program Files" & Instructive_Installer_InstallFolder
                End If
            End If
            Main.SetCurrentStatus("Creando los directorios para la instalacion...")
            If My.Computer.FileSystem.DirectoryExists(InstallerPathBuilder) = True Then
                My.Computer.FileSystem.DeleteDirectory(InstallerPathBuilder, FileIO.DeleteDirectoryOption.DeleteAllContents)
            End If
            If My.Computer.FileSystem.DirectoryExists(InstallerPathBuilder) = False Then
                My.Computer.FileSystem.CreateDirectory(InstallerPathBuilder)
            End If
            IO.Directory.CreateDirectory(InstallerPathBuilder)
            Main.SetCurrentStatus("Copiando los datos...")
            Dim output As Object = shObj.NameSpace((InstallerPathBuilder))
            Dim input As Object = shObj.NameSpace((DIRTemp & "\" & AssemblyName & "_" & Instructive_Package_AssemblyVersion & ".zip"))
            output.CopyHere((input.Items), 4)
            Threading.Thread.Sleep(50)
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
                Shortcut.IconLocation = InstallerPathBuilder & "\" & Instructive_Package_PackageName & ".exe" & ",0"
                Shortcut.TargetPath = InstallerPathBuilder & "\" & Instructive_Package_PackageName & ".exe"
                Shortcut.WindowStyle = 1
                Shortcut.Description = "Run " & Instructive_Package_PackageName
                Shortcut.Save()
            Catch ex As Exception
                Console.WriteLine("[Install(CreateShorcoutAndWindowsFolder)@Complementos]Error: " & ex.Message)
            End Try
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
                Console.WriteLine("[CreateAndStubTheUninstaller@Complementos]Error: " & ex.Message)
            End Try
            CreateRegistry()
        Catch ex As Exception
            Console.WriteLine("[Install@Complementos]Error: " & ex.Message)
        End Try
    End Sub
    Sub CreateRegistry()
        Main.SetCurrentStatus("Registrando la instalacion...")
        Try
            Dim REGISTRADOR1 As RegistryKey
            Dim x32bits As String = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" & Instructive_Package_AssemblyName
            Dim x64x32bits As String = "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" & Instructive_Package_AssemblyName
            If ArquitecturaSO = "32" Then
                If REGISTRADOR1 Is Nothing Then
                    Registry.LocalMachine.CreateSubKey(x32bits)
                End If
                REGISTRADOR1 = Registry.LocalMachine.OpenSubKey(x32bits, True)
            ElseIf ArquitecturaSO = "64" Then
                If REGISTRADOR1 Is Nothing Then
                    Registry.LocalMachine.CreateSubKey(x64x32bits)
                End If
                REGISTRADOR1 = Registry.LocalMachine.OpenSubKey(x64x32bits, True)
            Else
                If REGISTRADOR1 Is Nothing Then
                    Registry.LocalMachine.CreateSubKey(x32bits)
                End If
                REGISTRADOR1 = Registry.LocalMachine.OpenSubKey(x32bits, True)
            End If
            'x86 (32bits) HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\
            'Para equipos con x64Bits
            '  32bits HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\
            '  64bits HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\
            REGISTRADOR1.SetValue("InstallDate", DateTime.Now.ToString("dd/MM/yyyy"), RegistryValueKind.String)
            REGISTRADOR1.SetValue("InstallLocation", InstallerPathBuilder, RegistryValueKind.ExpandString)
            REGISTRADOR1.SetValue("Size", FormatBytes(PackageSize), RegistryValueKind.String)
            REGISTRADOR1.SetValue("Comments", Instructive_Package_PackageName & " Official Software by " & Instructive_Package_Company, RegistryValueKind.String)
            REGISTRADOR1.SetValue("DisplayIcon", InstallerPathBuilder & "\" & Instructive_Package_PackageName & ".exe", RegistryValueKind.String)
            REGISTRADOR1.SetValue("DisplayName", Instructive_Package_AssemblyName, RegistryValueKind.String)
            REGISTRADOR1.SetValue("DisplayVersion", Instructive_Package_AssemblyVersion, RegistryValueKind.String)
            REGISTRADOR1.SetValue("HelpLink", Instructive_HelpLinks_UseGuide, RegistryValueKind.String)
            REGISTRADOR1.SetValue("Publisher", Instructive_Package_Company, RegistryValueKind.String)
            'REGISTRADOR1.SetValue("Contact", , RegistryValueKind.String)
            REGISTRADOR1.SetValue("Readme", Instructive_HelpLinks_AppAbout, RegistryValueKind.String)
            REGISTRADOR1.SetValue("URLInfoAbout", Instructive_HelpLinks_AppAbout, RegistryValueKind.String)
            REGISTRADOR1.SetValue("URLUpdateInfo", Instructive_HelpLinks_ChangeLogLink, RegistryValueKind.String)
            Try
                Dim TotalSizeVal As String = Val(PackageSize)
                REGISTRADOR1.SetValue("EstimatedSize", TotalSizeVal.Remove(TotalSizeVal.Length - 3), RegistryValueKind.DWord)
            Catch
            End Try
            'REGISTRADOR1.SetValue("ModifyPath", InstallerPathBuilder & "\uninstall.exe", RegistryValueKind.ExpandString)
            REGISTRADOR1.SetValue("UninstallPath", InstallerPathBuilder & "\uninstall.exe", RegistryValueKind.ExpandString)
            REGISTRADOR1.SetValue("UninstallString", """" & InstallerPathBuilder & "\uninstall.exe" & """", RegistryValueKind.ExpandString)
            REGISTRADOR1.SetValue("QuietUninstallString", """" & InstallerPathBuilder & "\uninstall.exe" & """" & " /S", RegistryValueKind.String)
        Catch ex As Exception
            Console.WriteLine("[CreateRegistry@Complementos]Error: " & ex.Message)
        End Try
        FinishInstall()
    End Sub
    Sub FinishInstall()
        TaskbarProgress.SetState(Main.Handle, TaskbarProgress.TaskbarStates.Normal)
        Dim StartBlinkForFocus = WindowsApi.FlashWindow(Process.GetCurrentProcess().MainWindowHandle, True, True, 5)
        Main.SetCurrentStatus("Instalacion finalizada correctamente.")
        MsgBox("Se ha instalado correctamente", MsgBoxStyle.Information, "Instalacion Completada")
        End
    End Sub
#End Region

#Region "Uninstaller"
    Sub Uninstall()
        If MessageBox.Show("¿Want to uninstall the Software called " & Instructive_Package_PackageName & "?", "Confirm Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
            End
            Exit Sub
        End If
        Try
            Main.SetCurrentStatus("Comparando los datos del equipo...")
            If ArquitecturaSO = "32" Then
                InstallerPathBuilder = "C:\Program Files" & Instructive_Installer_InstallFolder
            ElseIf ArquitecturaSO = "64" Then
                If Instructive_Package_BitsArch = "32" Then
                    InstallerPathBuilder = "C:\Program Files (x86)" & Instructive_Installer_InstallFolder
                Else
                    InstallerPathBuilder = "C:\Program Files" & Instructive_Installer_InstallFolder
                End If
            End If
            Main.SetCurrentStatus("Eliminando los directorios de instalacion...")
            If My.Computer.FileSystem.DirectoryExists(InstallerPathBuilder) = True Then
                My.Computer.FileSystem.DeleteDirectory(InstallerPathBuilder, FileIO.DeleteDirectoryOption.DeleteAllContents)
            End If
            Try
                Main.SetCurrentStatus("Eliminando los datos post-instalacion...")
                Dim StartUpWindowsFolder As String = "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\" & Instructive_Package_Company & "\" & Instructive_Package_PackageName
                If My.Computer.FileSystem.FileExists(StartUpWindowsFolder & "\" & Instructive_Package_PackageName & ".lnk") = True Then
                    My.Computer.FileSystem.DeleteFile(StartUpWindowsFolder & "\" & Instructive_Package_PackageName & ".lnk")
                End If
            Catch ex As Exception
                Console.WriteLine("[Uninstall(DeleteShorcoutAndWindowsFolder)@Complementos]Error: " & ex.Message)
            End Try
        Catch ex As Exception
            Console.WriteLine("[Uninstall@Complementos]Error: " & ex.Message)
        End Try
        DeleteRegistry()
    End Sub
    Sub DeleteRegistry()
        Try
            Dim x32bits As String = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" & Instructive_Package_AssemblyName
            Dim x64x32bits As String = "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" & Instructive_Package_AssemblyName
            If ArquitecturaSO = "32" Then
                Registry.LocalMachine.DeleteSubKey(x32bits, False)
            ElseIf ArquitecturaSO = "64" Then
                Registry.LocalMachine.DeleteSubKey(x64x32bits, False)
            Else
                Registry.LocalMachine.DeleteSubKey(x32bits, False)
            End If
        Catch ex As Exception
            Console.WriteLine("[DeleteRegistry@Complementos]Error: " & ex.Message)
        End Try
        FinishUninstall()
    End Sub
    Sub FinishUninstall()
        Main.SetCurrentStatus("Desinstalacion finalizada correctamente.")
        MsgBox("Se ha desinstalado correctamente", MsgBoxStyle.Information, "Desinstalacion Completada")
        End
    End Sub
#End Region
End Module
Module Complementos
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