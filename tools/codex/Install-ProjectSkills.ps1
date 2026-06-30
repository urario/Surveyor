param(
    [string]$ProjectSkillRoot = ".codex/skills",
    [string]$CodexHome = $(if ($env:CODEX_HOME) { $env:CODEX_HOME } else { Join-Path $HOME ".codex" })
)

$ErrorActionPreference = "Stop"

$source = Resolve-Path -LiteralPath $ProjectSkillRoot
$target = Join-Path $CodexHome "skills"
if (-not (Test-Path -LiteralPath $target)) {
    New-Item -ItemType Directory -Path $target | Out-Null
}

Get-ChildItem -LiteralPath $source -Directory | ForEach-Object {
    $destination = Join-Path $target $_.Name
    Copy-Item -LiteralPath $_.FullName -Destination $destination -Recurse -Force
    Write-Host "Installed Codex skill: $($_.Name) -> $destination"
}

Write-Host "Restart Codex to pick up newly installed skills."

