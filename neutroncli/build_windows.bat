@echo off
setlocal EnableDelayedExpansion

set version=0.2.0
set PROJECT_URL=https://github.com/NeutronFramework/Neutron

if not exist build mkdir build
if exist build\publish rmdir /S /Q build\publish
if not exist build\artifacts mkdir build\artifacts

cd build

if exist neutroncli_choco rmdir /S /Q neutroncli_choco

dotnet publish ..\neutroncli.csproj --configuration Release --runtime win-x64 --output .\publish /p:PublishTrimmed=false /p:PublishAot=false

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

copy ..\chocolatey\chocolateyInstall.ps1 neutroncli_choco\tools\chocolateyInstall.ps1
copy ..\chocolatey\chocolateyUninstall.ps1 neutroncli_choco\tools\chocolateyUninstall.ps1
copy ..\chocolatey\neutroncli.nuspec neutroncli.nuspec

for /f "skip=1 delims=" %%i in ('certutil -hashfile publish\neutroncli.exe SHA256') do (
    set "EXE_HASH=%%i"
    goto :breakloop
)
:breakloop

(
echo VERIFICATION
echo Verification is intended to assist the Chocolatey moderators and community
echo in verifying that this package's contents are trustworthy.
echo.
echo The executable in this package are built from source:
echo Repository: https://github.com/NeutronFramework/Neutron/tree/main/neutroncli
echo Tag: %version%
echo SHA: %EXE_HASH%
echo.
echo On the following environment:
echo * Windows 11 64-bit, x64-based processor. Version 10.0.22631 Build 22631
echo * Microsoft Visual Studio Community 2022 Version 17.11.4
echo * Powershell version 5.1.22621.4111 BuildVersion 10.0.22621.4111
echo * Required installation: dotnet 8, git
echo.
echo Building the executable:
echo * Open powershell
echo * Run git clone https://github.com/NeutronFramework/Neutron
echo * Run cd .\Neutron\neutroncli\
echo * Run dotnet publish neutroncli.csproj --configuration Release --runtime win-x64 /p:PublishTrimmed=false /p:PublishAot=false
echo.
echo executable can be found in bin\Release\net8.0\win-x64\publish
echo.
echo You can use powershell function 'Get-FileHash' to get the hash
echo * neutroncli.exe SHA should be: %EXE_HASH%

) > neutroncli_choco\tools\VERIFICATION.txt

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

endlocal