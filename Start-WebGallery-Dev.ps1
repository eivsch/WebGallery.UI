<#
.SYNOPSIS
Runs API, FileServer and UI from this checkout side-by-side with the live app.

.DESCRIPTION
Starts the three services on alternate ports against a private dev copy of the live data folder.
The dev data folder holds real copies of the small index files (*.dat etc.) and NTFS hard links
to the (huge) media files, so it costs almost no disk space and deleting/moving media through
the dev instances can never remove the live files. The live instances and live data are never touched.

.EXAMPLE
.\Start-WebGallery-Dev.ps1            # start (copies data on first run)
.\Start-WebGallery-Dev.ps1 -Refresh   # re-create the dev data copy from live data, then start
.\Start-WebGallery-Dev.ps1 -Stop      # stop only the dev processes started by this script
#>
[CmdletBinding()]
param(
    [string]$LiveRootPath = 'C:\WebGallery',
    [string]$LiveDataPath,
    [string]$DevDataPath,
    [int]$ApiPort = 5141,        # https port; http port is +1
    [int]$FileServerPort = 5131, # https port; http port is +1
    [int]$UiPort = 5151,         # https port; http port is the UI HTTP port below
    [int]$UiHttpPort = 5150,
    [switch]$Refresh,
    [switch]$Stop
)

$ErrorActionPreference = 'Stop'
$repoRoot = $PSScriptRoot

# Live (standard) ports - the dev instances must never use these.
$livePorts = 5031, 5032, 5041, 5042, 5050, 5051

function Get-LiveSetting([string]$File, [scriptblock]$Pick) {
    if (Test-Path -LiteralPath $File) { & $Pick (Get-Content -LiteralPath $File -Raw | ConvertFrom-Json) }
}

if (-not $LiveDataPath) {
    $LiveDataPath = Get-LiveSetting (Join-Path $LiveRootPath 'MinimalGallery.API\appsettings.json') { param($j) $j.IndexFilesStoragePath }
    if (-not $LiveDataPath) { $LiveDataPath = 'C:\WebGallery\Data' }
}
$liveData = [IO.Path]::GetFullPath($LiveDataPath).TrimEnd('\')
if (-not $DevDataPath) { $DevDataPath = Join-Path (Split-Path $liveData -Parent) ((Split-Path $liveData -Leaf) + '-Dev') }
$devData = [IO.Path]::GetFullPath($DevDataPath).TrimEnd('\')

$stateFile = Join-Path (Split-Path $devData -Parent) 'WebGallery-Dev-state.json'
$logDir = Join-Path (Split-Path $devData -Parent) 'WebGallery-Dev-logs'

function Stop-DevProcesses {
    if (-not (Test-Path -LiteralPath $stateFile)) { Write-Host 'No dev instances are tracked.'; return }
    $state = Get-Content -LiteralPath $stateFile -Raw | ConvertFrom-Json
    $all = Get-CimInstance Win32_Process
    function Get-Tree($id) {
        $all | Where-Object ParentProcessId -eq $id | ForEach-Object { Get-Tree $_.ProcessId; $_.ProcessId }
    }
    foreach ($svc in $state.Processes) {
        $p = Get-Process -Id $svc.Pid -ErrorAction SilentlyContinue
        # Guard against PID reuse: only stop if it is the same process we started.
        if ($p -and $p.StartTime.ToUniversalTime().ToString('o') -eq $svc.StartTimeUtc) {
            foreach ($child in @(Get-Tree $svc.Pid)) { Stop-Process -Id $child -Force -ErrorAction SilentlyContinue }
            Stop-Process -Id $svc.Pid -Force -ErrorAction SilentlyContinue
            Write-Host "Stopped dev $($svc.Name) (PID $($svc.Pid))"
        } else {
            Write-Host "Dev $($svc.Name) (PID $($svc.Pid)) is not running."
        }
    }
    Remove-Item -LiteralPath $stateFile -Force
}

if ($Stop) { Stop-DevProcesses; return }

# --- Safety checks ---------------------------------------------------------
$ports = @($ApiPort, ($ApiPort + 1), $FileServerPort, ($FileServerPort + 1), $UiPort, $UiHttpPort)
if (($ports | Select-Object -Unique).Count -ne $ports.Count) { throw "Dev ports overlap each other: $($ports -join ', ')" }
$clash = $ports | Where-Object { $livePorts -contains $_ }
if ($clash) { throw "Refusing to run: dev would use live port(s) $($clash -join ', ')." }

$a = $devData + '\'; $b = $liveData + '\'
if ($a.StartsWith($b, 'OrdinalIgnoreCase') -or $b.StartsWith($a, 'OrdinalIgnoreCase')) {
    throw "Refusing to run: dev data path '$devData' is the same as, inside, or contains the live data path '$liveData'."
}
if ($a.StartsWith($repoRoot.TrimEnd('\') + '\', 'OrdinalIgnoreCase')) {
    throw "Dev data path must be outside the repository."
}
if (-not (Test-Path -LiteralPath $liveData -PathType Container)) { throw "Live data folder '$liveData' not found. Use -LiveDataPath." }
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw "The .NET SDK ('dotnet') was not found on PATH." }

if (Test-Path -LiteralPath $stateFile) {
    $state = Get-Content -LiteralPath $stateFile -Raw | ConvertFrom-Json
    $running = $state.Processes | Where-Object { Get-Process -Id $_.Pid -ErrorAction SilentlyContinue }
    if ($running) { throw "Dev instances appear to be running already. Run: .\Start-WebGallery-Dev.ps1 -Stop" }
    Remove-Item -LiteralPath $stateFile -Force
}

foreach ($port in $ports) {
    if (Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue) {
        throw "Port $port is already in use. Choose other ports or stop what is using it."
    }
}

# --- Dev data ----------------------------------------------------------------
$marker = Join-Path $devData '.webgallery-dev-data'
$indexExt = '.dat', '.json', '.txt', '.csv', '.xml', '.db', '.sqlite', '.jpg'

if ((Test-Path -LiteralPath $devData) -and $Refresh) {
    if (-not (Test-Path -LiteralPath $marker)) { throw "'$devData' exists but was not created by this script; refusing to delete it." }
    Write-Host "Removing old dev data copy '$devData' (hard links only; live files are untouched)..."
    Remove-Item -LiteralPath $devData -Recurse -Force
}

if (-not (Test-Path -LiteralPath $devData)) {
    if ((Split-Path $liveData -Qualifier) -ne (Split-Path $devData -Qualifier)) {
        throw "Dev data must be on the same drive as live data ('$liveData') so media can be hard-linked instead of copied. Use -DevDataPath."
    }
    Write-Host "Creating dev data from '$liveData' -> '$devData' ..."
    $files = Get-ChildItem -LiteralPath $liveData -Recurse -File -Force
    $totalGb = [math]::Round(($files | Measure-Object Length -Sum).Sum / 1GB, 1)
    $copy = @($files | Where-Object { $indexExt -contains $_.Extension.ToLower() })
    $copyMb = [math]::Round(($copy | Measure-Object Length -Sum).Sum / 1MB, 1)
    Write-Host "Live data: $($files.Count) files, $totalGb GB. Copying $($copy.Count) index files ($copyMb MB); hard-linking the rest (no extra disk space)."
    if ($copyMb -gt 2048) { Write-Warning "Index files to copy are large ($copyMb MB)." }

    New-Item -ItemType Directory -Path $devData | Out-Null
    # New-Item -Target treats [ ] in names as wildcards, so use the Win32 API directly.
    if (-not ('WgNative' -as [type])) {
        Add-Type -Namespace '' -Name WgNative -MemberDefinition '[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)] public static extern bool CreateHardLink(string newFile, string existingFile, System.IntPtr sa);' 2>$null
    }
    try {
        $i = 0
        foreach ($f in $files) {
            $rel = $f.FullName.Substring($liveData.Length + 1)
            $dest = Join-Path $devData $rel
            $destDir = Split-Path $dest -Parent
            if (-not (Test-Path -LiteralPath $destDir)) { New-Item -ItemType Directory -Path $destDir | Out-Null }
            if ($indexExt -contains $f.Extension.ToLower()) {
                Copy-Item -LiteralPath $f.FullName -Destination $dest
            } else {
                if (-not [WgNative]::CreateHardLink("\\?\$dest", "\\?\$($f.FullName)", [IntPtr]::Zero)) {
                    throw "Could not hard-link '$($f.FullName)': $([ComponentModel.Win32Exception][Runtime.InteropServices.Marshal]::GetLastWin32Error())"
                }
            }
            if ((++$i % 2000) -eq 0) { Write-Host "  $i / $($files.Count)" }
        }
        Set-Content -LiteralPath $marker -Value "Dev copy of '$liveData' created $(Get-Date -Format o)"
    } catch {
        Write-Warning "Copy failed; removing incomplete dev data (hard links only)."
        Remove-Item -LiteralPath $devData -Recurse -Force -ErrorAction SilentlyContinue
        throw
    }
} else {
    if (-not (Test-Path -LiteralPath $marker)) { throw "'$devData' exists but was not created by this script; refusing to use it." }
    Write-Host "Using existing dev data '$devData' (use -Refresh to re-create)."
}

# --- Build ---------------------------------------------------------------------
$ffmpeg = Join-Path $LiveRootPath 'ffmpeg\ffmpeg.exe'
$services = @(
    @{ Name = 'FileServer'; Project = 'FileServer\WebGallery.FileServer.csproj'; Https = $FileServerPort; Http = $FileServerPort + 1
       Env = @{ ConnectionStrings__FileSystemRoot = $devData; FfmpegPath = $(if (Test-Path $ffmpeg) { $ffmpeg } else { 'ffmpeg' }) } },
    @{ Name = 'API'; Project = 'API\MinimalGallery.API.csproj'; Https = $ApiPort; Http = $ApiPort + 1
       Env = @{ IndexFilesStoragePath = $devData } },
    @{ Name = 'UI'; Project = 'UI\WebGallery.UI\WebGallery.UI.csproj'; Https = $UiPort; Http = $UiHttpPort
       Env = @{ ConnectionStrings__ApiEndpoint = "https://localhost:$ApiPort"; ConnectionStrings__FileServerUrl = "https://localhost:$FileServerPort" } }
)

foreach ($s in $services) {
    Write-Host "Building $($s.Name)..."
    $out = & dotnet build (Join-Path $repoRoot $s.Project) --nologo -v quiet 2>&1
    if ($LASTEXITCODE -ne 0) { $out | Out-Host; throw "Build of $($s.Name) failed." }
}

# --- Start ---------------------------------------------------------------------
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$tracked = @()
try {
    foreach ($s in $services) {
        $projectDir = Split-Path (Join-Path $repoRoot $s.Project) -Parent
        $env:WEBGALLERY_HTTPS_PORT = "$($s.Https)"
        $env:WEBGALLERY_HTTP_PORT = "$($s.Http)"
        $env:ASPNETCORE_ENVIRONMENT = 'Development'
        foreach ($k in $s.Env.Keys) { Set-Item -Path "env:$k" -Value $s.Env[$k] }

        $proc = Start-Process -FilePath 'dotnet' -ArgumentList 'run', '--no-build', '--no-launch-profile' -WorkingDirectory $projectDir `
            -WindowStyle Hidden -PassThru `
            -RedirectStandardOutput (Join-Path $logDir "$($s.Name).out.log") -RedirectStandardError (Join-Path $logDir "$($s.Name).err.log")

        foreach ($k in $s.Env.Keys) { Remove-Item -Path "env:$k" }
        $tracked += @{ Name = $s.Name; Pid = $proc.Id; StartTimeUtc = $proc.StartTime.ToUniversalTime().ToString('o') }
        @{ Processes = $tracked } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $stateFile
    }
} finally {
    Remove-Item env:WEBGALLERY_HTTPS_PORT, env:WEBGALLERY_HTTP_PORT, env:ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
}

foreach ($s in $services) {
    $deadline = (Get-Date).AddSeconds(90)
    while (-not (Get-NetTCPConnection -State Listen -LocalPort $s.Http -ErrorAction SilentlyContinue)) {
        if ((Get-Date) -gt $deadline) {
            Stop-DevProcesses
            throw "$($s.Name) did not start listening on $($s.Http). See logs in '$logDir'."
        }
        Start-Sleep -Milliseconds 500
    }
}

Write-Host ''
Write-Host 'Dev instances running (live app untouched):' -ForegroundColor Green
Write-Host "  UI          http://localhost:$UiHttpPort   (https://localhost:$UiPort)"
Write-Host "  API         https://localhost:$ApiPort   (http://localhost:$($ApiPort + 1))"
Write-Host "  FileServer  https://localhost:$FileServerPort   (http://localhost:$($FileServerPort + 1))"
Write-Host "  Data        $devData"
Write-Host "  Logs        $logDir"
Write-Host 'Stop with:    .\Start-WebGallery-Dev.ps1 -Stop'
