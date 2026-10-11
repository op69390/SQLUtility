# Compile (et lance, en option) la solution SQLUtility.
#
# Usage :
#   .\build.ps1                      # compile en Debug
#   .\build.ps1 -Run                 # compile puis lance l'application
#   .\build.ps1 -Configuration Release
#   .\build.ps1 -Configuration Release -Run
param(
    [switch]$Run,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$racine = $PSScriptRoot
$solution = Join-Path $racine 'SQLUtility.sln'

# Localiser MSBuild via vswhere (fourni avec Visual Studio).
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) {
    throw "vswhere introuvable : Visual Studio (ou Build Tools) est requis."
}
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' |
    Select-Object -First 1
if (-not $msbuild) {
    throw "MSBuild introuvable. Installer le composant 'MSBuild' de Visual Studio."
}

Write-Host "MSBuild  : $msbuild"
Write-Host "Solution : $solution"
Write-Host "Config   : $Configuration"
Write-Host '--- Compilation (restore + build) ---'

# /restore est indispensable : restaure Newtonsoft.Json (PackageReference) avant le build.
& $msbuild $solution /restore /t:Build /p:Configuration=$Configuration /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw "Echec de la compilation (code $LASTEXITCODE)." }
Write-Host "Build OK ($Configuration)."

if ($Run) {
    $exe = Join-Path $racine ("SQLUtility.App\bin\{0}\SQLUtility.App.exe" -f $Configuration)
    if (-not (Test-Path $exe)) { throw "Executable introuvable : $exe" }
    Write-Host "--- Lancement de $exe ---"
    & $exe
}
