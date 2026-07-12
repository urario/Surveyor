[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$goldenPath = Join-Path $repoRoot 'tests\fixtures\reports\des-0012\golden\report-v1.happy.json'
$projectPath = Join-Path $repoRoot 'tests\Surveyor.Reports.Tests\Surveyor.Reports.Tests.csproj'
$tempPath = Join-Path ([System.IO.Path]::GetTempPath()) "surveyor-report-golden-$([Guid]::NewGuid().ToString('N')).json"

try {
    $env:SURVEYOR_REPORT_PROBE = '1'
    $env:SURVEYOR_REPORT_PROBE_OUTPUT = $tempPath
    $env:DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = '0'
    $env:LANG = 'tr-TR.UTF-8'

    dotnet test $projectPath --no-restore --filter 'FullyQualifiedName~ReportJsonBehaviorTests.UT0006ReportJsonIsByteStableAcrossFreshProcessAndChangedCulture' --logger 'console;verbosity=minimal' /p:CollectCoverage=false
    if ($LASTEXITCODE -ne 0) {
        throw "Golden report generation failed with exit code $LASTEXITCODE."
    }

    Copy-Item -LiteralPath $tempPath -Destination $goldenPath -Force
    Write-Host "Updated $goldenPath"
}
finally {
    Remove-Item Env:SURVEYOR_REPORT_PROBE -ErrorAction SilentlyContinue
    Remove-Item Env:SURVEYOR_REPORT_PROBE_OUTPUT -ErrorAction SilentlyContinue
    Remove-Item Env:DOTNET_SYSTEM_GLOBALIZATION_INVARIANT -ErrorAction SilentlyContinue
    Remove-Item Env:LANG -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $tempPath -ErrorAction SilentlyContinue
}
