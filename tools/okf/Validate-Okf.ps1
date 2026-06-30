param(
    [string]$Root = "knowledge"
)

$ErrorActionPreference = "Stop"
$resolvedRoot = Resolve-Path -LiteralPath $Root
$errors = New-Object System.Collections.Generic.List[string]
$markdownFiles = Get-ChildItem -LiteralPath $resolvedRoot -Recurse -File -Filter "*.md"

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

foreach ($file in $markdownFiles) {
    $relative = (Get-RelativePathText -BaseDirectory $resolvedRoot -TargetPath $file.FullName).Replace("\", "/")
    $name = $file.Name.ToLowerInvariant()
    $lines = Get-Content -LiteralPath $file.FullName -Encoding UTF8

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

if ($errors.Count -gt 0) {
    Write-Host "OKF validation failed:" -ForegroundColor Red
    foreach ($err in $errors) {
        Write-Host " - $err" -ForegroundColor Red
    }
    exit 1
}

Write-Host "OKF validation passed for $($markdownFiles.Count) markdown files under $Root."
