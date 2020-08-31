[CmdletBinding()]Param (

)

$ModuleName = "DoIT.VPNHelper"
$GitHubUri = "https://github.com/jacobdonais/DoIT.VPNHelper/archive/master.zip"
$ResourceLocation = "./"
$ZipName = "master.zip"
$FileName = "$$($ModuleName)-master"

if (Test-Path -Path ($ResourceLocation + $ZipName)) {
    Write-Verbose "Removing exisiting zip file"
    try {
        Remove-Item -Path ($ResourceLocation + $ZipName) -Recurse -Force -Confirm:$false
        Write-Verbose ".Successfully removed the existing zip file"
    }
    catch {
        Write-Warning "Failed to remove the exisiting zip file"
        break
    }
}

Write-Verbose "Extracting module from GitHub"
try {
    Invoke-WebRequest -Uri $GitHubUri -UseBasicParsing -OutFile ($ResourceLocation + $ZipName)
    Write-Verbose ".Successfully extracted module from GitHub"
}
catch {
    Write-Warning "Failed to extract module from GitHub"
    break
}

if (Test-Path -Path "$($ResourceLocation)$($ModuleName)-master") {
    Write-Verbose "Removing exisiting extracted zip file"
    try {
        Remove-Item -Path "$($ResourceLocation)$($ModuleName)-master" -Recurse -Force -Confirm:$false
        Write-Verbose ".Successfully removed the existing extracted zip file"
    }
    catch {
        Write-Warning "Failed to remove the exisiting extracted zip file"
        break
    }
}

Write-Verbose "Extracting zip file"
try {
    Expand-Archive -Path ($ResourceLocation + $ZipName) -DestinationPath $ResourceLocation
    Write-Verbose ".Successfully extracted zip file"
}
catch {
    Write-Warning "Failed to extract zip file"
    break
}

Write-Verbose "Removing zip file"
try {
    Remove-Item -Path ($ResourceLocation + $ZipName) -Recurse -Force -Confirm:$false
    Write-Verbose ".Successfully removed zip file"
}
catch {
    Write-Warning "Failed to remove zip file"
    break
}

Write-Verbose "Compiling program"
try {
    $CurrentDir = Get-Location
    cd "$($ResourceLocation)$($ModuleName)-master"
    C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /reference:Resources/system.management.automation.dll /win32icon:Resources/TAMU.ico -out:VPNConnect.exe main.cs -nologo
    Move-Item -Path "$($ResourceLocation)VPNConnect.exe" -Destination $CurrentDir
    Set-Location $CurrentDir
    Write-Verbose ".Successfully compiled the program"
}
catch {
    Write-Warning "Failed to compile the program"
    break
}