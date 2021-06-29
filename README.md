# EXInstaller
Instalador simple y bonito

## About

EXInstaller is a deploy software to install any third party program.

### The project

EXInstaller was programmed in Visual Studio 2010 with the WinForms or VB.NET language.

It is currently being developed without the help of Worcome Studios.

Currently you can help us with some little things that we still can't add, it would be great.

### Using it

  1) You must first create the service file.
```
#CRZ Installer Instructive File
[Package]
Status=True //still in development
AssemblyName=(Ex.: OurSpeakHome) //assembly name,
AssemblyVersion=(Ex.: 1.0.0.0)//assembly version
Company=(Ex.: CRZ Labs)//company name
WebUrl=//company web page
PackageName=(Ex.: OSH) //package name: The name of the executable. (Ex .: OSH.exe. but without the extension, OSH)
IsComponent=False //still in development
InstallerVersion=0.1.0.0 //still in development
BitsArch=(Ex.: 32)//program bits: Under what architecture does the program to install work
[Installer]
Status=True //still in development
EnableDowngrade=True //still in development
NeedRestart=False //still in development
NeedStartUp=False;/background //still in development
NeedElevateAccess=False //still in development
InstallFolder=(Ex.: \CRZLabs\OurSpeakHome) //install folder: It will be installed in program files. but, you can indicate the folder and subfolders where you want it to be installed. The program will automatically install the software depending on the value in BitsArch and the architecture of the program.
EULA=False;http://YourCompanyWebsiteURL/EULA_Template.txt //still in development
Installer=None //still in development
InstallPackage=(Ex.: http://YourCompanyWebsiteURL/OSH/OSHPackage.zip)//ZipPackage: The zip with the files that were installed. The files must NOT be in subfolders, they must be in the root of the zip. All the files in the .zip will be copied to the installation folder
[HelpLinks]
ChangeLogLink=(Ex.: http://YourCompanyWebsiteURL/OSH/OurSpeakHome.html#WhatsNew) //The change log of the program
UseGuide=(Ex.: http://YourCompanyWebsiteURL/OSH/OurSpeakHome.html#HowTo) //A user guide or manual
AppAbout=(Ex.: http://YourCompanyWebsiteURL/OSH/OurSpeakHome.html) //Information about the program
Contact=(Ex.: http://YourCompanyWebsiteURL/Contact.html) //Contact form
```
  2) Then you must start the EXInstallerIDE
    * In the Assembly box you must enter the "AssemblyName" set in the service file.
    * In the Version box you must enter the "AssemblyVersion" set in the service file.
    * In the URL box you must enter the direct download link of the service file.
  3) When you click on "Inject", the program will ask you to select EXInstaller.exe to be able to inject into it. Another executable will be created with the name of the assembly, that is ready for deployment.
  4) Ready!. The installer is ready to distribute.

## Contributing

We appreciate all contributions to improve OurSpeakHome.
