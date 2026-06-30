param(
    [string]$HooksPath = ".githooks"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath ".git")) {
    throw "This script must be run from the repository root."
}

if (-not (Test-Path -LiteralPath $HooksPath)) {
    throw "Hooks path not found: $HooksPath"
}

git config core.hooksPath $HooksPath
Write-Host "Configured Git hooks path: $HooksPath"
Write-Host "Direct commits and pushes on main will be blocked by local hooks."

