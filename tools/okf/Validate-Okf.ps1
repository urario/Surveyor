param(
    [string]$Root = "knowledge",
    [string]$RequirementSource = "docs/gui-testability-analyzer-requirements.md",
    [string]$RqIndex = "knowledge/requirements/rq-index.generated.md",
    [switch]$SkipRqIndexFreshness
)

$ErrorActionPreference = "Stop"
$resolvedRoot = Resolve-Path -LiteralPath $Root
$repoRoot = Split-Path -Parent $resolvedRoot
$errors = New-Object System.Collections.Generic.List[string]
$markdownFiles = @(Get-ChildItem -LiteralPath $resolvedRoot -Recurse -File -Filter "*.md")

function Get-RelativePathText {
    param(
        [string]$BaseDirectory,
        [string]$TargetPath
    )

    $baseFullPath = [System.IO.Path]::GetFullPath($BaseDirectory)
    if (-not $baseFullPath.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
        $baseFullPath += [System.IO.Path]::DirectorySeparatorChar
    }

    $targetFullPath = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = New-Object System.Uri($baseFullPath)
    $targetUri = New-Object System.Uri($targetFullPath)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString())
}

function Get-ContentWithoutGeneratedTimestamp {
    param(
        [string]$Path
    )

    $lines = Get-Content -LiteralPath $Path -Encoding UTF8
    return (($lines | Where-Object { $_ -notmatch "^\s*timestamp\s*:" }) -join "`n")
}

$linkPattern = [regex]'\[[^\]]+\]\(([^)]+)\)'
$reachable = New-Object 'System.Collections.Generic.HashSet[string]'
$edges = @{}

foreach ($file in $markdownFiles) {
    $relative = (Get-RelativePathText -BaseDirectory $resolvedRoot -TargetPath $file.FullName).Replace("\", "/")
    $name = $file.Name.ToLowerInvariant()
    $lines = Get-Content -LiteralPath $file.FullName -Encoding UTF8
    $content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
    $fileDirectory = Split-Path -Parent $file.FullName
    $localKnowledgeLinks = New-Object System.Collections.Generic.List[string]

    foreach ($match in $linkPattern.Matches($content)) {
        $target = $match.Groups[1].Value.Trim()
        if ([string]::IsNullOrWhiteSpace($target)) {
            continue
        }

        $targetWithoutAnchor = ($target -split '#', 2)[0]
        $targetWithoutQuery = ($targetWithoutAnchor -split '\?', 2)[0]
        if ([string]::IsNullOrWhiteSpace($targetWithoutQuery)) {
            continue
        }

        if ($targetWithoutQuery -match '^[a-zA-Z][a-zA-Z0-9+.-]*:') {
            continue
        }

        $targetPath = [System.IO.Path]::GetFullPath((Join-Path $fileDirectory $targetWithoutQuery))
        if (-not (Test-Path -LiteralPath $targetPath -PathType Leaf)) {
            $errors.Add("${relative}: broken markdown link '$target'")
            continue
        }

        if ($targetPath.StartsWith([System.IO.Path]::GetFullPath($resolvedRoot))) {
            $linkedRelative = (Get-RelativePathText -BaseDirectory $resolvedRoot -TargetPath $targetPath).Replace("\", "/")
            $localKnowledgeLinks.Add($linkedRelative) | Out-Null
        }
    }

    $edges[$relative] = @($localKnowledgeLinks)

    if ($name -eq "log.md") {
        continue
    }

    if ($name -eq "index.md") {
        $isRootIndex = $file.FullName -eq (Join-Path $resolvedRoot "index.md")
        if (-not $isRootIndex -and $lines.Count -gt 0 -and $lines[0] -eq "---") {
            $errors.Add("${relative}: nested index.md must not use frontmatter")
        }
        continue
    }

    if ($lines.Count -lt 3 -or $lines[0] -ne "---") {
        $errors.Add("${relative}: missing YAML frontmatter")
        continue
    }

    $closingIndex = -1
    for ($i = 1; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -eq "---") {
            $closingIndex = $i
            break
        }
    }

    if ($closingIndex -lt 0) {
        $errors.Add("${relative}: missing closing YAML frontmatter delimiter")
        continue
    }

    $frontmatter = $lines[1..($closingIndex - 1)]
    $typeLine = $frontmatter | Where-Object { $_ -match "^\s*type\s*:\s*(.+?)\s*$" } | Select-Object -First 1
    if (-not $typeLine) {
        $errors.Add("${relative}: missing required type field")
        continue
    }

    $typeValue = ($typeLine -replace "^\s*type\s*:\s*", "").Trim().Trim('"').Trim("'")
    if ([string]::IsNullOrWhiteSpace($typeValue)) {
        $errors.Add("${relative}: type field is empty")
    }
}

$queue = New-Object System.Collections.Generic.Queue[string]
$reachable.Add("index.md") | Out-Null
$queue.Enqueue("index.md")

while ($queue.Count -gt 0) {
    $current = $queue.Dequeue()
    if (-not $edges.ContainsKey($current)) {
        continue
    }

    foreach ($linked in $edges[$current]) {
        if ($reachable.Add($linked)) {
            $queue.Enqueue($linked)
        }
    }
}

foreach ($file in $markdownFiles) {
    $relative = (Get-RelativePathText -BaseDirectory $resolvedRoot -TargetPath $file.FullName).Replace("\", "/")
    $name = $file.Name.ToLowerInvariant()
    if ($name -eq "log.md") {
        continue
    }

    if (-not $reachable.Contains($relative)) {
        $errors.Add("${relative}: not reachable from knowledge/index.md")
    }
}

if (-not $SkipRqIndexFreshness) {
    $rqIndexPath = Resolve-Path -LiteralPath $RqIndex
    $requirementSourcePath = Resolve-Path -LiteralPath $RequirementSource
    $exporterPath = Join-Path $repoRoot "tools/requirements/Export-RqIndex.ps1"
    $tempPath = Join-Path ([System.IO.Path]::GetTempPath()) ("surveyor-rq-index-" + [System.Guid]::NewGuid().ToString("N") + ".md")

    try {
        & $exporterPath -Source $requirementSourcePath -Output $tempPath -Quiet
        $currentIndex = Get-ContentWithoutGeneratedTimestamp -Path $rqIndexPath
        $expectedIndex = Get-ContentWithoutGeneratedTimestamp -Path $tempPath

        if ($currentIndex -ne $expectedIndex) {
            $relativeRqIndex = (Get-RelativePathText -BaseDirectory $repoRoot -TargetPath $rqIndexPath).Replace("\", "/")
            $errors.Add("${relativeRqIndex}: generated RQ index is stale; run tools/requirements/Export-RqIndex.ps1")
        }
    }
    finally {
        if (Test-Path -LiteralPath $tempPath) {
            Remove-Item -LiteralPath $tempPath
        }
    }
}

if ($errors.Count -gt 0) {
    Write-Host "OKF validation failed:" -ForegroundColor Red
    foreach ($err in $errors) {
        Write-Host " - $err" -ForegroundColor Red
    }
    exit 1
}

Write-Host "OKF validation passed for $($markdownFiles.Count) markdown files under $Root."
