# ADR-0002 spike measurement harness.
# Runs every PoC against one target window and aggregates per-axis evidence.
# The human owner runs this against real legacy targets (DES-0007 sec. 4.2)
# and archives the results directory as acceptance evidence.
#
# Usage:
#   powershell -File Measure-Spike.ps1 -TargetTitle "<window title substring>"
#   powershell -File Measure-Spike.ps1 -TargetHwnd 123456
#
# Determinism axis: each UIA PoC runs twice in fresh processes; the two
# CanonicalTreeSha256 values must match for an idle target.
# ASCII-only source for Windows PowerShell 5.1 compatibility.

[CmdletBinding()]
param(
    [string]$TargetTitle,
    [long]$TargetHwnd,
    [string]$Configuration = "Debug",
    # Default is resolved in the body: Windows PowerShell 5.1 does not
    # populate $PSScriptRoot while evaluating param() defaults.
    [string]$ResultsRoot
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $ResultsRoot) { $ResultsRoot = Join-Path $scriptRoot "results" }
$spikeRoot = Split-Path $scriptRoot -Parent
$tfm = "net10.0-windows10.0.19041.0"

if (-not $TargetTitle -and -not $TargetHwnd) {
    throw "Specify -TargetTitle or -TargetHwnd."
}

$targetArgs = @()
if ($TargetHwnd) { $targetArgs = @("--hwnd", "$TargetHwnd") } else { $targetArgs = @("--title", $TargetTitle) }

$stamp = (Get-Date).ToUniversalTime().ToString("yyyyMMdd-HHmmss")
$runDir = Join-Path $ResultsRoot $stamp
New-Item -ItemType Directory -Force -Path $runDir | Out-Null

$pocs = @(
    @{ Name = "UiaRawComPoc";         Runs = 2 },
    @{ Name = "UiaFlaUiPoc";          Runs = 2 },
    @{ Name = "CapturePrintWindowPoc"; Runs = 1 },
    @{ Name = "CaptureWgcPoc";        Runs = 1 }
)

Write-Host "== ADR-0002 spike run $stamp -> $runDir"
foreach ($poc in $pocs) {
    $exe = Join-Path $spikeRoot ("src\{0}\bin\{1}\{2}\{0}.exe" -f $poc.Name, $Configuration, $tfm)
    if (-not (Test-Path $exe)) {
        Write-Warning "$($poc.Name): not built ($exe). Run 'dotnet build' first."
        continue
    }
    for ($i = 1; $i -le $poc.Runs; $i++) {
        Write-Host ("-- {0} run {1}/{2}" -f $poc.Name, $i, $poc.Runs)
        & $exe @targetArgs --out $runDir
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "$($poc.Name) run $i exited with $LASTEXITCODE (errors recorded in its report JSON)."
        }
        Start-Sleep -Milliseconds 500
    }
}

# Aggregate: determinism check per UIA candidate + summary table.
$reports = Get-ChildItem $runDir -Filter "*.json" | ForEach-Object {
    $json = Get-Content $_.FullName -Raw | ConvertFrom-Json
    $json | Add-Member -NotePropertyName File -NotePropertyValue $_.Name -PassThru
}

$summaryPath = Join-Path $runDir "summary.md"
$lines = @()
$lines += "# ADR-0002 spike run $stamp"
$lines += ""
$lines += "Target: $($targetArgs -join ' ')"
$lines += ""
$lines += "| candidate | run | elapsed ms | elements | unavailable | tree sha256 | errors |"
$lines += "| -- | -- | -- | -- | -- | -- | -- |"
$runIndex = @{}
foreach ($r in ($reports | Sort-Object Candidate, File)) {
    if (-not $runIndex.ContainsKey($r.Candidate)) { $runIndex[$r.Candidate] = 0 }
    $runIndex[$r.Candidate]++
    $sha = if ($r.CanonicalTreeSha256) { $r.CanonicalTreeSha256.Substring(0, 12) } else { "-" }
    $err = if ($r.Errors.Count -gt 0) { $r.Errors.Count } else { 0 }
    $lines += "| $($r.Candidate) | $($runIndex[$r.Candidate]) | $($r.ElapsedMs) | $($r.ElementCount) | $($r.UnavailableNodeCount) | $sha | $err |"
}
$lines += ""
foreach ($candidate in @("uia-raw-com", "uia-flaui")) {
    $hashes = @($reports | Where-Object { $_.Candidate -eq $candidate } | ForEach-Object { $_.CanonicalTreeSha256 } | Where-Object { $_ })
    if ($hashes.Count -ge 2) {
        $stable = (($hashes | Select-Object -Unique).Count -eq 1)
        $verdict = if ($stable) { "PASS (identical across fresh processes)" } else { "FAIL (hashes differ - investigate target idleness vs API nondeterminism)" }
        $lines += "- Determinism axis / ${candidate}: $verdict"
    }
}
$lines += "- Read-only axis: review ApiCallsUsed in each report; run the before/after target-state check from README.md manually."
$lines += "- Copy the observations (threading, DPI, permissions, border/consent) into results-template.md."
Set-Content -Path $summaryPath -Value ($lines -join "`r`n") -Encoding utf8
Write-Host "== summary: $summaryPath"
