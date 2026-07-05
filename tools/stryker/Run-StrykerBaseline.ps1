[CmdletBinding()]
param(
    [ValidateSet('All', 'Domain', 'Policy')]
    [string]$Target = 'All',

    [switch]$SkipVersionCheck
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$manifestPath = Join-Path $repoRoot '.config\dotnet-tools.json'
if (-not (Test-Path -LiteralPath $manifestPath)) {
    throw "Missing tool manifest: $manifestPath"
}

$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
$stryker = $manifest.tools.'dotnet-stryker'
if (-not $stryker) {
    throw "dotnet-stryker is not registered in $manifestPath. Run dotnet tool restore after updating the manifest."
}

$packageRoot = Join-Path $env:USERPROFILE ".nuget\packages\dotnet-stryker\$($stryker.version)"
$strykerDll = Join-Path $packageRoot 'tools\net8.0\any\Stryker.CLI.dll'
if (-not (Test-Path -LiteralPath $strykerDll)) {
    throw "Missing Stryker CLI at $strykerDll. Run dotnet tool restore first."
}

$runs = @(
    [pscustomobject]@{
        Name = 'Domain'
        WorkDir = Join-Path $repoRoot 'tests\Surveyor.Domain.Tests'
        Config = '..\..\eng\stryker\domain.stryker-config.json'
        Output = '..\..\artifacts\stryker\domain'
    },
    [pscustomobject]@{
        Name = 'Policy'
        WorkDir = Join-Path $repoRoot 'tests\Surveyor.Policy.Tests'
        Config = '..\..\eng\stryker\policy.stryker-config.json'
        Output = '..\..\artifacts\stryker\policy'
    }
)

foreach ($run in $runs) {
    if ($Target -ne 'All' -and $Target -ne $run.Name) {
        continue
    }

    Push-Location $run.WorkDir
    try {
        $args = @(
            $strykerDll,
            '--config-file', $run.Config,
            '--output', $run.Output
        )
        if ($SkipVersionCheck) {
            $args += '--skip-version-check'
        }

        Write-Host "Running Stryker baseline for $($run.Name)..."
        & dotnet @args
        if ($LASTEXITCODE -ne 0) {
            throw "Stryker baseline failed for $($run.Name) with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}
