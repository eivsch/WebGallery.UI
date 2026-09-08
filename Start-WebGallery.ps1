[CmdletBinding()]
param(
    [string]$RootPath = 'C:\WebGallery'
)

$services = @(
    @{ Name = 'WebGallery FileServer'; Directory = 'FileServer'; Assembly = 'WebGallery.FileServer.dll' },
    @{ Name = 'MinimalGallery API'; Directory = 'MinimalGallery.API'; Assembly = 'MinimalGallery.API.dll' },
    @{ Name = 'WebGallery UI'; Directory = 'UI'; Assembly = 'WebGallery.UI.dll' }
)

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "The .NET runtime ('dotnet') was not found on PATH."
}

foreach ($service in $services) {
    $servicePath = Join-Path $RootPath $service.Directory
    $assemblyPath = Join-Path $servicePath $service.Assembly

    if (-not (Test-Path -LiteralPath $assemblyPath -PathType Leaf)) {
        throw "Could not find $($service.Assembly) at '$assemblyPath'."
    }
}

foreach ($service in $services) {
    $servicePath = Join-Path $RootPath $service.Directory
    $escapedServicePath = $servicePath.Replace("'", "''")
    $escapedAssembly = $service.Assembly.Replace("'", "''")
    $command = "& { Set-Location -LiteralPath '$escapedServicePath'; `$Host.UI.RawUI.WindowTitle = '$($service.Name)'; dotnet '$escapedAssembly' }"
    $encodedCommand = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))

    Start-Process -FilePath 'powershell.exe' -ArgumentList '-NoExit', '-EncodedCommand', $encodedCommand
}
