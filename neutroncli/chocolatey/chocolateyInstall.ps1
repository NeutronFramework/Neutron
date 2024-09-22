$ErrorActionPreference = 'Stop'
$toolsDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$installDir = "$env:ProgramFiles\neutroncli"

Write-Host "Copying files to installation directory..."

try {
    New-Item -ItemType Directory -Force -Path $installDir | Out-Null
    Copy-Item -Path "$toolsDir\*" -Destination $installDir -Recurse -Force
    Write-Host "neutroncli installed in $installDir"
    
    Install-ChocolateyPath $installDir 'user'
    Write-Host "Added $installDir to USER PATH"
} catch {
    Write-Host "Failed to install neutroncli: $_"
    exit 1
}