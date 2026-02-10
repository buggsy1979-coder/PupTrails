param(
    [string]$OutputPath = "${env:USERPROFILE}\Desktop\PupTrails",
    [switch]$SelfContained
)

$ErrorActionPreference = 'Stop'

function Write-Info($message) {
    Write-Host "[PupTrails Release] $message" -ForegroundColor Cyan
}

function Remove-PathIfExists($path) {
    if (Test-Path $path) {
        Write-Info "Removing $path"
        Remove-Item -LiteralPath $path -Recurse -Force -ErrorAction SilentlyContinue
    } else {
        Write-Info "Nothing to remove at $path"
    }
}

try {
    Write-Info "Stopping any running PupTrails processes"
    Get-Process -Name "PupTrails" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

    $roamingRoot = Join-Path $env:APPDATA 'PupTrails'
    $localRoot = Join-Path $env:LOCALAPPDATA 'PupTrail'

    Write-Info "Clearing persisted app data so the build launches clean"
    Remove-PathIfExists $roamingRoot
    Remove-PathIfExists $localRoot

    if (-not (Test-Path $OutputPath)) {
        Write-Info "Creating output directory at $OutputPath"
        New-Item -ItemType Directory -Path $OutputPath | Out-Null
    }
    else {
        Write-Info "Clearing existing output directory at $OutputPath"
        Get-ChildItem -LiteralPath $OutputPath | Remove-Item -Recurse -Force
    }

    $publishArgs = @('publish', 'PupTrailsV3.csproj', '-c', 'Release', '-r', 'win-x64', '-o', $OutputPath)
    if ($SelfContained.IsPresent) {
        $publishArgs += '--self-contained'
        $publishArgs += 'true'
        $publishArgs += '-p:PublishSingleFile=true'
    } else {
        $publishArgs += '--self-contained'
        $publishArgs += 'false'
    }

    Write-Info "Publishing release build"
    dotnet @publishArgs

    $portableFlag = Join-Path $OutputPath 'portable.mode'
    "# PupTrails portable mode marker" | Set-Content -Path $portableFlag -Encoding UTF8

    Write-Info "Release ready at $OutputPath"
    Write-Info "Launch PupTrails.exe from that folder to verify activation prompt"
}
catch {
    Write-Error "Release preparation failed: $_"
    exit 1
}
