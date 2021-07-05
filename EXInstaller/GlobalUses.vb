Imports Microsoft.Win32
Module GlobalUses

#Region "Debugger Vars"

#Region "Commons"
    Public DIRoot As String = "C:\Users\" & Environment.UserName & "\AppData\Local"
    Public DIRCommons As String = DIRoot & "\" & My.Application.Info.AssemblyName
    Public DIRTemp As String = DIRoot & "\Temp\" & My.Application.Info.AssemblyName
    Public InstructiveFilePath As String = DIRCommons & "\Instructive.ini"
    Public StartParametros As String
    Public IsInjected As Boolean = True
    Public IsUninstall As Boolean = False
    Public IsReinstall As Boolean = False
    Public IsAssistant As Boolean = False
    Public InstallerPathBuilder As String
    Public RegistradorInstalacion As RegistryKey
    Public CanSaveLog As Boolean = False 'True solo para el desarrollo. El log se guarda en DIRTemp. Recomendado dejar en False.
    Public x32bits As String
    Public x64x32bits As String
    Public ExePackage As String = InstallerPathBuilder & "\" & Instructive_Package_PackageName & ".exe"
    Public DownloadedZipPackage As String = DIRTemp & "\" & AssemblyName & "_" & Instructive_Package_AssemblyVersion & ".zip"

    Public ArquitecturaSO As String
    Public PackageSize As String
    'Si quieres puedes indicar el ensamblado su version y la url del instructivo
    Public AssemblyName As String '1
    Public AssemblyVersion As String '2
    Public InstructiveURL As String '3
    Public CanOverwrite As Boolean = True 'Si indicas los 3 valores de arriba, pon este en False. Asi evitas que injecten o pasen parametros
#End Region

#Region "Instructive"
    Public Instructive_Package_Status As String
    Public Instructive_Package_AssemblyName As String
    Public Instructive_Package_AssemblyVersion As String
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
    Public Instructive_Installer_InstallFolder As String
    Public Instructive_Installer_EULA As String
    Public Instructive_Installer_Installer As String
    Public Instructive_Installer_InstallPackage As String

    Public Instructive_HelpLinks_ChangeLogLink As String
    Public Instructive_HelpLinks_UseGuide As String
    Public Instructive_HelpLinks_AppAbout As String
    Public Instructive_HelpLinks_Contact As String
#End Region

#End Region

End Module
