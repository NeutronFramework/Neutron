@echo off
setlocal EnableDelayedExpansion

set version=0.2.6
set PROJECT_URL=https://github.com/NeutronFramework/Neutron

where choco >nul 2>&1
if %errorlevel% neq 0 (
    echo Installing Chocolatey...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))"
    if !errorlevel! neq 0 (
        echo Failed to install Chocolatey. Exiting.
        exit /b 1
    )
)

if not exist build mkdir build
if exist build\publish rmdir /S /Q build\publish
if not exist build\artifacts mkdir build\artifacts

cd build

if exist neutroncli_choco rmdir /S /Q neutroncli_choco

mkdir neutroncli_choco\tools

copy ..\LICENSE.txt neutroncli_choco\tools\LICENSE.txt

copy ..\chocolatey\chocolateyInstall.ps1 neutroncli_choco\tools\chocolateyInstall.ps1
copy ..\chocolatey\chocolateyUninstall.ps1 neutroncli_choco\tools\chocolateyUninstall.ps1
copy ..\chocolatey\neutroncli.nuspec neutroncli.nuspec

dotnet publish ..\neutroncli.csproj --configuration Release --runtime win-x64 --output .\publish

if %errorlevel% neq 0 (
    echo Dotnet publish failed.
    exit /b %errorlevel%
)

cd publish
tar -a -cf ..\neutroncli_%version%_win_x86_64.zip *.*

if %errorlevel% neq 0 (
    echo Compression failed.
    exit /b %errorlevel%
)

cd ..
move neutroncli_%version%_win_x86_64.zip artifacts\

if %errorlevel% neq 0 (
    echo Move failed.
    exit /b %errorlevel%
)

for /f %%i in ('powershell -command "(Get-FileHash publish\neutroncli.exe -Algorithm SHA256).Hash"') do (
    set "EXE_HASH=%%i"
)

(
echo VERIFICATION
echo.
echo Verification is intended to assist the Chocolatey moderators and community
echo in verifying that this package's contents are trustworthy.
echo.
echo Go to https://github.com/NeutronFramework/Neutron/releases/tag/%version% and download neutroncli_%version%_win_86_64.zip
echo extract it and you will find neutroncli.exe
echo.
echo You can then use 'Get-FileHash neutroncli.exe -Algorithm SHA256' to get the hash
echo * neutroncli.exe SHA hash should be: %EXE_HASH%
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