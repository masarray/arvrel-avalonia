[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location (Join-Path $root 'desktop')
try {
    dotnet restore ARVREL.Desktop.sln
    if ($LASTEXITCODE -ne 0) { throw 'Restore failed.' }

    dotnet build ARVREL.Desktop.sln -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }

    dotnet test ARVREL.Desktop.sln -c $Configuration --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Desktop tests failed.' }
}
finally {
    Pop-Location
}
