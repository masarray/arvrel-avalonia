[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location (Join-Path $root 'desktop')
try {
    dotnet run --project ..\src\Arvrel.Desktop\Arvrel.Desktop.csproj -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw 'Avalonia application run failed.' }
}
finally {
    Pop-Location
}
