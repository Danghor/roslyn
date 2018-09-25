# Take care to return nothing if we don't yet have any vsman files
# as will occur at the start of the build.
# This will allow us to set it after the build when called again.

$vsmanpath = "Binaries/VSSetup/$Env:BuildConfiguration/Insertion"
if (Test-Path $vsmanpath) {
    $SetupManifests = [string]::Join(',', (Get-ChildItem "Binaries/VSSetup/$Env:BuildConfiguration/Insertion/*.vsman"))
    Set-Item -Path "env:SetupManifests" -Value $SetupManifests
}