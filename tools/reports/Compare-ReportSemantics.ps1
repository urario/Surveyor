[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ExpectedPath,

    [Parameter(Mandatory = $true)]
    [string]$ActualPath
)

$ErrorActionPreference = 'Stop'

function Convert-ToCanonicalJson([string]$Path) {
    $json = Get-Content -Raw -LiteralPath $Path
    $parsed = $json | ConvertFrom-Json
    return $parsed | ConvertTo-Json -Depth 100
}

$expected = Convert-ToCanonicalJson $ExpectedPath
$actual = Convert-ToCanonicalJson $ActualPath

if ($expected -ne $actual) {
    Write-Error "Report semantics differ: expected='$ExpectedPath' actual='$ActualPath'"
}

Write-Host "Report semantics match."
