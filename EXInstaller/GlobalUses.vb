Imports Microsoft.Win32
Module GlobalUses

#Region "Debugger Vars"

#Region "Commons"
    Public DIRoot As String = "C:\Users\" & Environment.UserName & "\AppData\Local"
    Public DIRCommons As String = DIRoot & "\" & My.Application.Info.AssemblyName
    Public DIRTemp As String = DIRoot & "\Temp\" & My.Application.Info.AssemblyName
    Public InstructiveFilePath As String = DIRCommons & "\" & AssemblyVersion & "_Instructive.ini"
    Public StartParametros As String
    Public IsInjected As Boolean = True
    Public IsUninstall As Boolean = False
    Public IsReinstall As Boolean = False
    Public IsComponent As Boolean = False
    Public IsAssistant As Boolean = False
    Public IsUpdate As Boolean = False
    Public IsSilence As Boolean = False
    Public IsForced As Boolean = False
    Public InstallerPathBuilder As String
    Public RegistradorInstalacion As RegistryKey
    Public CanSaveLog As Boolean = True 'Para guardar el log del instalador
    Public x32bits As String
    Public x64x32bits As String
    Public ExePackage As String = InstallerPathBuilder & "\" & Instructive_Package_PackageName
    Public DownloadedZipPackage As String = DIRTemp & "\" & AssemblyName & "_" & Instructive_Package_AssemblyVersion & ".zip"
    Public shObj As Object

    Public ArquitecturaSO As String
    Public PackageSize As String
    'Si quieres puedes indicar el ensamblado su version y la url del instructivo
    Public AssemblyName As String '1
    Public AssemblyVersion As String '2
    Public InstructiveURL As String '3
    Public CanOverwrite As Boolean = True
#End Region

#Region "Instructive"
    Public Instructive_Package_Status As String
    Public Instructive_Package_AssemblyName As String
    Public Instructive_Package_AssemblyVersion As String
    Public Instructive_Package_Description As String
    Public Instructive_Package_Company As String
    Public Instructive_Package_WebUrl As String
    Public Instructive_Package_PackageName As String
    Public Instructive_Package_IsComponent As String
    Public Instructive_Package_InstallerVersion As String
    Public Instructive_Package_BitsArch As String

    Public Instructive_Installer_Status As String
    Public Instructive_Installer_EnableDowngrade As String
    Public Instructive_Installer_NeedRestart As String
    Public Instructive_Installer_NeedStartUp As String
    Public Instructive_Installer_NeedElevateAccess As String
    Public Instructive_Installer_NeedToStart As String
    Public Instructive_Installer_InstallFolder As String
    Public Instructive_Installer_EULA As String
    Public Instructive_Installer_Installer As String
    Public Instructive_Installer_AfterInstall As String
    Public Instructive_Installer_AfterUninstall As String
    Public Instructive_Installer_InstallPackage As String

    Public Instructive_HelpLinks_TelemetryPost As String
    Public Instructive_HelpLinks_ChangeLogLink As String
    Public Instructive_HelpLinks_UseGuide As String
    Public Instructive_HelpLinks_AppAbout As String
    Public Instructive_HelpLinks_Contact As String
#End Region

#End Region

End Module