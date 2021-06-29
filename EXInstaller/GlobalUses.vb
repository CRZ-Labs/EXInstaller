Module GlobalUses

#Region "Debugger Vars"

#Region "Commons"
    Public DIRoot As String = "C:\Users\" & Environment.UserName & "\AppData\Local"
    Public DIRCommons As String = DIRoot & "\" & My.Application.Info.AssemblyName
    Public DIRTemp As String = DIRoot & "\Temp\" & My.Application.Info.AssemblyName
    Public InstructiveFilePath As String = DIRCommons & "\Instructive.ini"
    Public StartParametros As String
    Public IsUninstall As Boolean = False

    Public ArquitecturaSO As String
    Public PackageSize As String
    Public AssemblyName As String
    Public AssemblyVersion As String
    Public InstructiveURL As String
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
#End Region

#End Region

End Module
