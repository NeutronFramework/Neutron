@echo off
setlocal EnableDelayedExpansion

set version=0.2.0
set PROJECT_URL=https://github.com/annasajkh/Neutron

if not exist build mkdir build
if exist build\publish rmdir /S /Q build\publish
if not exist build\artifacts mkdir build\artifacts

cd build

if exist neutroncli_choco rmdir /S /Q neutroncli_choco

dotnet publish ..\neutroncli.csproj --configuration Release --runtime win-x64 --self-contained true --output .\publish

where choco >nul 2>&1
if %errorlevel% neq 0 (
    echo Installing Chocolatey...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))"
    if !errorlevel! neq 0 (
        echo Failed to install Chocolatey. Exiting.
        exit /b 1
    )
)

mkdir neutroncli_choco\tools

copy ..\LICENSE.txt neutroncli_choco\tools\LICENSE.txt
copy ..\neutroncli.nuspec neutroncli.nuspec

for /f "skip=1 delims=" %%i in ('certutil -hashfile publish\neutroncli.exe SHA256') do (
    set "EXE_HASH=%%i"
    goto :breakloop
)
:breakloop

(
echo This package contains the neutroncli executable and associated files.
echo.
echo Verification:
echo 1. Download the binary from the official repository: %PROJECT_URL%/releases
echo 2. Ensure the file hashes match the following:
echo SHA256: %EXE_HASH%
) > neutroncli_choco\tools\VERIFICATION.txt

(
echo $ErrorActionPreference = 'Stop'
echo Write-Host "Removing neutroncli from the system..."
echo try {
echo     Remove-Item "$env:ProgramFiles\neutroncli" -Recurse -Force -ErrorAction Stop
echo     Write-Host "neutroncli uninstalled successfully."
echo } catch {
echo     Write-Host "Failed to uninstall neutroncli: $_"
echo     exit 1
echo }
) > neutroncli_choco\tools\chocolateyUninstall.ps1

(
echo $ErrorActionPreference = 'Stop'
echo Write-Host "Copying files to installation directory..."
echo try {
echo     $installDir = "$env:ProgramFiles\neutroncli"
echo     New-Item -ItemType Directory -Force -Path $installDir ^| Out-Null
echo     Copy-Item -Path "$env:ChocolateyPackageFolder\tools\*" -Destination $installDir -Recurse -Force
echo     Write-Host "neutroncli installed in $installDir"
echo } catch {
echo     Write-Host "Failed to install neutroncli: $_"
echo     exit 1
echo }
) > neutroncli_choco\tools\chocolateyInstall.ps1

xcopy /E /I publish neutroncli_choco\tools

echo Packing Chocolatey package...
choco pack neutroncli.nuspec

if exist neutroncli.%version%.nupkg (
    echo Package found, moving to artifacts folder...
    move neutroncli.%version%.nupkg artifacts\
    if exist artifacts\neutroncli.%version%.nupkg (
        echo File successfully moved to artifacts folder.
    ) else (
        echo File not found in artifacts folder.
        exit /b 1
    )
) else (
    echo Package neutroncli.%version%.nupkg not found.
    exit /b 1
)

if not defined CHOCOLATEY_API_KEY (
    echo API key environment variable not set. Please set CHOCOLATEY_API_KEY.
    exit /b 1
)

echo Pushing Chocolatey package to the Chocolatey repository...
choco push artifacts\neutroncli.%version%.nupkg --source https://push.chocolatey.org/ --api-key %CHOCOLATEY_API_KEY%

endlocal