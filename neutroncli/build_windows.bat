@echo off
setlocal

set version=0.3.7.0
set outputDir=build
set installerOutput=neutroncli_installer_0.3.7_x86_64.exe
set nsisScript=installer.nsi
set buildDir=%outputDir%\publish
set artifactsDir=%outputDir%\artifacts
set iconPath=%buildDir%\icon.ico

if not exist %outputDir% (
    mkdir %outputDir%
)

if exist %outputDir%\publish (
    rmdir /s /q %outputDir%\publish
)

if not exist %artifactsDir% (
    mkdir %artifactsDir%
)

dotnet publish --configuration Release --runtime win-x64 --self-contained true --output %buildDir%

if %ERRORLEVEL% neq 0 (
    echo Failed to build neutroncli.
    exit /b 1
)

if not exist "C:\Program Files (x86)\NSIS\makensis.exe" (
    choco install nsis -y
    if not exist "C:\Program Files (x86)\NSIS\makensis.exe" (
        echo NSIS installation failed.
        exit /b 1
    )
)

"C:\Program Files (x86)\NSIS\makensis.exe" %nsisScript%
if %ERRORLEVEL% neq 0 (
    echo Failed to create the installer with NSIS.
    exit /b 1
)

move %installerOutput% %artifactsDir%

endlocal