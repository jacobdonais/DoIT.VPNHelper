# DoIT.VPNHelper
The DoIT.VPNHelper is designed to connect the user to the TAMU VPN for troubleshooting purposes. Once successfully connected, it will provide the user the IP address and host name to give to the service desk technician. 

## Compilation
**Please make sure `C:\Windows\Microsoft.NET\Framework64\<version>` is added to system path for csc.**
### Compilation with PS Script file
Please download the file `InstallDoITVPNHelper.ps1` and run it with PowerShell.

### Compilation with makefile
Use the makefile to install and package
```
make all
```

### Command line
```
csc /reference:Resources/system.management.automation.dll /win32icon:Resources/favicon.ico -out:VPNConnect.exe main.cs -nologo
```

## Usage
Please see above on how to compile and package the file. Once package, email the user the .exe file.

Copyright (C) 2020 Jacob Donais
