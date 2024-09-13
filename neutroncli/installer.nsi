!include "MUI2.nsh" 

Name "neutroncli" 
OutFile "neutroncli_installer_0.3.7_x86_64.exe" 
InstallDir "$PROGRAMFILES64\neutroncli" 
Icon "build\publish\icon.ico" 

!define MUI_ICON "build\publish\icon.ico" 
!define MUI_UNICON "build\publish\icon.ico" 

VIProductVersion "0.3.7.0" 
VIAddVersionKey "ProductName" "neutroncli" 
VIAddVersionKey "FileDescription" "Build apps with C# and web technologies using webview" 
VIAddVersionKey "LegalCopyright" "AnnasVirtual" 
VIAddVersionKey "FileVersion" "0.3.7.0" 
VIAddVersionKey "ProductVersion" "0.3.7.0" 
VIAddVersionKey "CompanyName" "AnnasVirtual" 

!insertmacro MUI_PAGE_WELCOME 
!insertmacro MUI_PAGE_DIRECTORY 
!insertmacro MUI_PAGE_INSTFILES 
!insertmacro MUI_PAGE_FINISH 
!insertmacro MUI_UNPAGE_WELCOME 
!insertmacro MUI_UNPAGE_CONFIRM 
!insertmacro MUI_UNPAGE_INSTFILES 
!insertmacro MUI_UNPAGE_FINISH 
!insertmacro MUI_LANGUAGE "English" 

Section 
SetOutPath "$INSTDIR" 
File /r "build\publish\*.*" 

CreateDirectory "$SMPROGRAMS\neutroncli" 
CreateShortcut "$SMPROGRAMS\neutroncli\neutroncli.lnk" "$INSTDIR\neutroncli.exe" "" "$INSTDIR\icon.ico" 
WriteUninstaller "$INSTDIR\Uninstall.exe" 

WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\neutroncli" "DisplayName" "neutroncli" 
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\neutroncli" "UninstallString" "$INSTDIR\Uninstall.exe" 
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\neutroncli" "DisplayIcon" "$INSTDIR\icon.ico" 
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\neutroncli" "Publisher" "AnnasVirtual"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\neutroncli" "DisplayVersion" "0.3.7.0"

ReadRegStr $0 HKCU "Environment" "Path" 
WriteRegExpandStr HKCU "Environment" "Path" "$0;$INSTDIR" 
SectionEnd 

Section "Uninstall" 
Delete "$SMPROGRAMS\neutroncli\neutroncli.lnk" 
RMDir "$SMPROGRAMS\neutroncli" 
RMDir /r "$INSTDIR" 

DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\neutroncli" 
DeleteRegKey HKCU "Software\neutroncli" 
SectionEnd 