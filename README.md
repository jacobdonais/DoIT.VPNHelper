# DoIT.VPNHelper
The DoIT.VPNHelper is designed to connect the user to the TAMU VPN for troubleshooting purposes. Once successfully connected, it will provide the user the IP address and host name to give to the service desk technician. 

## Compilation
**Please make sure `C:\Windows\Microsoft.NET\Framework64\v4.xxxx` is added to system path for csc.**
Use the makefile to install and package
```
make all
```

## Usage
Please see above on how to compile and package the file. Once package, email the user the zip file.
1. Have the user extract the zip.
2. Have the user run the .exe file.

Copyright (C) 2020 Jacob Donais
