<#
.SYNOPSIS
    Sync a GitHub Issue's Project (v2) fields for "Surveyor Lifecycle Work".

.DESCRIPTION
    Reads the desired Project field values for an Issue and applies them to the
    GitHub Project item via `gh project item-edit`. By default the values are
    parsed from the Issue body's Project-fields block (the section every Surveyor
    Issue carries under a heading containing "Project", listing Status / Phase /
    Artifact / RQ / RD / Guardrail / Owner Role / Priority / Target). Individual
    values can be overridden with -Set, which is how status transitions are
    driven (e.g. "Status=Design Review").

    Single-select fields (Status, Phase, Guardrail, Owner Role, Priority, Target)
    resolve their option name to the option id automatically. Text fields
    (Artifact, RQ, RD) are written as-is. Fields not present in the project
    schema are skipped with a warning.

    Requires a `gh` token with the `project` scope. Run -DryRun first to see
    what would change without writing.

    This script is intentionally ASCII-only so Windows PowerShell 5.1 parses it
    correctly regardless of file encoding (BOM-less UTF-8 is read as ANSI there).

.PARAMETER IssueNumber
    The Issue number to sync (its Project item is added if not already present).

.PARAMETER Set
    Zero or more "Field=Value" overrides, applied on top of the Issue-body block.
    Example: -Set 'Status=Design Review','Owner Role=Human'

.PARAMETER NoIssueBody
    Do not parse the Issue body block; use only -Set overrides.

.PARAMETER Repo
    owner/name of the repository. Default: urario/Surveyor.

.PARAMETER Owner
    Project owner login. Default: urario.

.PARAMETER ProjectNumber
    Project number. Default: 1 (Surveyor Lifecycle Work).

.PARAMETER DryRun
    Print intended changes without calling item-edit.

.EXAMPLE
    ./tools/project/Sync-ProjectFields.ps1 -IssueNumber 20 -DryRun

.EXAMPLE
    ./tools/project/Sync-ProjectFields.ps1 -IssueNumber 31 -Set 'Status=Design Review','Owner Role=Human'
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [int]$IssueNumber,

    [string[]]$Set = @(),

    [switch]$NoIssueBody,

    [string]$Repo = 'urario/Surveyor',

    [string]$Owner = 'urario',

    [int]$ProjectNumber = 1,

    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

# Canonical Surveyor Project field names (single-select vs text is decided by the schema query).
$KnownFields = @('Status', 'Phase', 'Artifact', 'RQ', 'RD', 'Guardrail', 'Owner Role', 'Priority', 'Target')

# Colon characters that may separate "Name: Value" in a bullet: ASCII ':' and the
# full-width colon (U+FF1A) Japanese bullets sometimes use. Built from a code point so
# this source file stays pure ASCII.
$colonChars = ':' + [char]0xFF1A

function Invoke-Gh {
    param([Parameter(Mandatory = $true)][string[]]$GhArgs)
    $output = & gh @GhArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "gh $($GhArgs -join ' ') failed (exit $LASTEXITCODE): $output"
    }
    return $output
}

function Assert-ProjectScope {
    # gh auth status writes to stderr; capture as plain text (Out-String) so the
    # PS 5.1 ErrorRecord wrapping of native stderr does not break the scope match.
    $status = & { $ErrorActionPreference = 'Continue'; gh auth status 2>&1 } | Out-String
    if ($status -notmatch 'project') {
        throw "The active gh token is missing the 'project' scope. Run: gh auth refresh -h github.com -s project"
    }
}

function Get-ProjectFieldSchema {
    $json = Invoke-Gh @('project', 'field-list', "$ProjectNumber", '--owner', $Owner, '--format', 'json', '--limit', '100')
    $data = $json | ConvertFrom-Json
    $schema = @{}
    foreach ($f in $data.fields) {
        $options = @{}
        if (($f.PSObject.Properties.Name -contains 'options') -and $f.options) {
            foreach ($o in $f.options) { $options[$o.name] = $o.id }
        }
        $schema[$f.name] = [pscustomobject]@{
            Id       = $f.id
            IsSelect = ($options.Count -gt 0)
            Options  = $options
        }
    }
    return $schema
}

function Get-ProjectId {
    $json = Invoke-Gh @('project', 'list', '--owner', $Owner, '--format', 'json', '--limit', '100')
    $data = $json | ConvertFrom-Json
    $match = $data.projects | Where-Object { $_.number -eq $ProjectNumber } | Select-Object -First 1
    if (-not $match) { throw "Project number $ProjectNumber not found for owner $Owner." }
    return $match.id
}

function Get-OrAddItemId {
    param([string]$IssueUrl)
    # item-add is idempotent: re-adding an existing issue returns the existing item.
    if (-not $DryRun) {
        Invoke-Gh @('project', 'item-add', "$ProjectNumber", '--owner', $Owner, '--url', $IssueUrl, '--format', 'json') | Out-Null
    }
    $json = Invoke-Gh @('project', 'item-list', "$ProjectNumber", '--owner', $Owner, '--format', 'json', '--limit', '1000')
    $data = $json | ConvertFrom-Json
    $item = $data.items | Where-Object { $_.content -and ($_.content.number -eq $IssueNumber) } | Select-Object -First 1
    if (-not $item) {
        if ($DryRun) { return $null }
        throw "Issue #$IssueNumber is not a project item even after add."
    }
    return $item.id
}

function Get-DesiredValues {
    param([string]$IssueBody)
    $desired = [ordered]@{}

    if ((-not $NoIssueBody) -and $IssueBody) {
        $inBlock = $false
        foreach ($line in ($IssueBody -split "`r?`n")) {
            if ($line -match '^\s*#{1,6}\s') {
                # A heading: enter the block when it mentions "Project", leave on the next heading.
                $inBlock = ($line -match 'Project')
                continue
            }
            $bullet = '^\s*[-*]\s*([^' + $colonChars + ']+?)\s*[' + $colonChars + ']\s*(.+?)\s*$'
            if ($inBlock -and ($line -match $bullet)) {
                $name = ($matches[1] -replace '`', '').Trim()
                $value = ($matches[2] -replace '`', '').Trim()
                if ($KnownFields -contains $name) { $desired[$name] = $value }
            }
        }
    }

    foreach ($override in $Set) {
        if ($override -match '^\s*([^=]+?)\s*=\s*(.*)$') {
            $desired[$matches[1].Trim()] = $matches[2].Trim()
        } else {
            throw "Invalid -Set override '$override'; expected 'Field=Value'."
        }
    }
    return $desired
}

# --- main ---
Assert-ProjectScope

$issueJson = Invoke-Gh @('issue', 'view', "$IssueNumber", '--repo', $Repo, '--json', 'number,url,body')
$issue = $issueJson | ConvertFrom-Json

$desired = Get-DesiredValues -IssueBody $issue.body
if ($desired.Count -eq 0) {
    Write-Host "No desired Project field values found for issue #$IssueNumber (no block and no -Set)." -ForegroundColor Yellow
    return
}

$schema = Get-ProjectFieldSchema
$projectId = Get-ProjectId
$itemId = Get-OrAddItemId -IssueUrl $issue.url

$itemLabel = if ($itemId) { $itemId } else { '<dry-run: not resolved>' }
Write-Host "Issue #$IssueNumber -> project $Owner/#$ProjectNumber (item $itemLabel)"

foreach ($name in $desired.Keys) {
    $value = $desired[$name]
    if (-not $schema.ContainsKey($name)) {
        Write-Host "  ! skip '$name' (not a project field)" -ForegroundColor Yellow
        continue
    }
    $field = $schema[$name]
    $editArgs = @('project', 'item-edit', '--id', $itemId, '--field-id', $field.Id, '--project-id', $projectId)

    if ($field.IsSelect) {
        if (-not $field.Options.ContainsKey($value)) {
            Write-Host "  ! skip '$name'='$value' (not a valid option: $($field.Options.Keys -join ', '))" -ForegroundColor Red
            continue
        }
        $editArgs += @('--single-select-option-id', $field.Options[$value])
    } else {
        $editArgs += @('--text', $value)
    }

    if ($DryRun) {
        Write-Host "  [dry-run] $name = '$value'" -ForegroundColor Cyan
    } else {
        Invoke-Gh $editArgs | Out-Null
        Write-Host "  set $name = '$value'" -ForegroundColor Green
    }
}

if ($DryRun) {
    Write-Host "Dry run complete (no changes written)." -ForegroundColor Cyan
} else {
    Write-Host "Project fields synced for issue #$IssueNumber." -ForegroundColor Green
}
