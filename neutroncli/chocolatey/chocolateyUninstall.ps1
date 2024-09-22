$ErrorActionPreference = 'Stop'
$installDir = "$env:ProgramFiles\neutroncli"

Write-Host "Removing neutroncli from the system..."

try {
    if (Test-Path $installDir) {
        Remove-Item $installDir -Recurse -Force -ErrorAction Stop
        Write-Host "neutroncli uninstalled successfully."
        
        Uninstall-ChocolateyPath $installDir 'user'
    } else {
        Write-Host "neutroncli installation directory not found. Nothing to uninstall."
    }
} catch {
    Write-Host "Failed to uninstall neutroncli: $_"
    exit 1
}
