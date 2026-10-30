[CmdletBinding()]
param(
    [string]$RootPath = 'C:\WebGallery',
    [int]$TimeoutSeconds = 30
)

# Services are identified by the powershell window that Start-WebGallery.ps1 launched for them:
# its -EncodedCommand contains the service folder path and the assembly name.
$services = @(
    @{ Directory = 'FileServer'; Assembly = 'WebGallery.FileServer.dll' },
    @{ Directory = 'MinimalGallery.API'; Assembly = 'MinimalGallery.API.dll' },
    @{ Directory = 'UI'; Assembly = 'WebGallery.UI.dll' }
)

function Get-DecodedCommand([string]$commandLine) {
    if ($commandLine -match '-EncodedCommand\s+(\S+)') {
        try { return [Text.Encoding]::Unicode.GetString([Convert]::FromBase64String($Matches[1])) } catch { }
    }
    return ''
}

$root = [IO.Path]::GetFullPath($RootPath).TrimEnd('\')
$all = @(Get-CimInstance Win32_Process)
$targets = @()

foreach ($service in $services) {
    $servicePath = Join-Path $root $service.Directory

    foreach ($proc in $all | Where-Object { $_.Name -eq 'dotnet.exe' -and $_.CommandLine -like "*$($service.Assembly)*" }) {
        $parent = $all | Where-Object { $_.ProcessId -eq $proc.ParentProcessId -and $_.Name -eq 'powershell.exe' }
        $viaParent = $parent -and (Get-DecodedCommand $parent.CommandLine) -like "*'$servicePath'*"
        $viaPath = $proc.CommandLine -like "*$servicePath\$($service.Assembly)*"

        if ($viaParent -or $viaPath) {
            $targets += [pscustomobject]@{ Service = $service.Directory; Id = $proc.ProcessId; Kind = 'dotnet' }
            if ($viaParent) {
                $targets += [pscustomobject]@{ Service = $service.Directory; Id = $parent.ProcessId; Kind = 'powershell window' }
            }
        }
    }
}

if (-not $targets) {
    Write-Host "No WebGallery processes found for '$root'."
    return
}

# Stop dotnet first so the host windows do not linger, then close the windows.
foreach ($t in $targets | Sort-Object Kind) {
    Write-Host "Stopping $($t.Service) $($t.Kind) (PID $($t.Id))"
    Stop-Process -Id $t.Id -Force -ErrorAction SilentlyContinue
}

$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
do {
    $stillRunning = @($targets | Where-Object { Get-Process -Id $_.Id -ErrorAction SilentlyContinue })
    if (-not $stillRunning) { break }
    $stillRunning | ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
    Start-Sleep -Milliseconds 500
} while ((Get-Date) -lt $deadline)

$stillRunning = @($targets | Where-Object { Get-Process -Id $_.Id -ErrorAction SilentlyContinue })
if ($stillRunning) {
    throw "Processes still running: $($stillRunning.Id -join ', ')"
}

Write-Host "Stopped $($targets.Count) process(es)."
