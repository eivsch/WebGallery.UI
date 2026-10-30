[CmdletBinding()]
param(
    [string]$RootPath = 'C:\WebGallery',
    [switch]$SkipBackup
)

$ErrorActionPreference = 'Stop'
$repo = $PSScriptRoot

$services = @(
    @{ Directory = 'MinimalGallery.API'; Project = 'API\MinimalGallery.API.csproj' },
    @{ Directory = 'UI'; Project = 'UI\WebGallery.UI\WebGallery.UI.csproj' },
    @{ Directory = 'FileServer'; Project = 'FileServer\WebGallery.FileServer.csproj' }
)

# Live-specific config/state that must never be overwritten or deleted.
$preserve = @('appsettings*.json', 'appsettings *.json', '*.pfx', '*.log', '*.db', '*.sqlite', 'Certificates', 'logs', 'Logs', 'Data', 'data')

$publishRoot = Join-Path ([IO.Path]::GetTempPath()) "webgallery-publish-$(Get-Date -Format yyyyMMddHHmmss)"

foreach ($s in $services) {
    Write-Host "Publishing $($s.Directory)..."
    dotnet publish (Join-Path $repo $s.Project) -c Release -o (Join-Path $publishRoot $s.Directory) --nologo -v quiet
    if ($LASTEXITCODE -ne 0) { throw "Publish failed for $($s.Directory)" }
}

& (Join-Path $repo 'Stop-WebGallery.ps1') -RootPath $RootPath

if (-not $SkipBackup) {
    $backup = Join-Path $RootPath "_backup\$(Get-Date -Format yyyyMMdd-HHmmss)"
    Write-Host "Backing up to $backup"
    foreach ($s in $services) {
        Copy-Item -LiteralPath (Join-Path $RootPath $s.Directory) -Destination (Join-Path $backup $s.Directory) -Recurse -Force
    }
}

foreach ($s in $services) {
    $src = Join-Path $publishRoot $s.Directory
    $dst = Join-Path $RootPath $s.Directory
    Write-Host "Updating $dst"

    # Remove stale files from the old version, except preserved ones.
    Get-ChildItem -LiteralPath $dst -Force | Where-Object {
        $name = $_.Name
        -not ($preserve | Where-Object { $name -like $_ })
    } | Remove-Item -Recurse -Force

    # Copy new output, skipping preserved top-level items.
    Get-ChildItem -LiteralPath $src -Force | Where-Object {
        $name = $_.Name
        -not ($preserve | Where-Object { $name -like $_ })
    } | Copy-Item -Destination $dst -Recurse -Force
}

Remove-Item -LiteralPath $publishRoot -Recurse -Force -ErrorAction SilentlyContinue

& (Join-Path $repo 'Start-WebGallery.ps1') -RootPath $RootPath
Write-Host 'Deploy complete.'
