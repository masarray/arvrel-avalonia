[CmdletBinding()]
param(
    [string]$RepositoryUrl = 'https://github.com/masarray/arvrel-avalonia.git'
)

$ErrorActionPreference = 'Stop'

if (Test-Path '.git') {
    throw 'This folder already contains a .git directory. Use a clean extracted copy.'
}

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    throw 'Git is not installed or not available in PATH.'
}

git init
git add --all
git commit -m 'Initial Avalonia P5.9 migration snapshot'
git branch -M main
git remote add origin $RepositoryUrl
git push -u origin main

Write-Host "Pushed Avalonia migration snapshot to $RepositoryUrl"
