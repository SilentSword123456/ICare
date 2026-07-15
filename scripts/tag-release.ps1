$latestTag = git describe --tags --abbrev=0 2>$null
if (-not $latestTag) {
    $latestTag = "(no tags yet)"
}

Write-Host "Latest tag: $latestTag" -ForegroundColor Cyan
$Version = Read-Host "Enter new version tag (e.g. v1.4.7)"

$existingTags = git tag --list
if ($existingTags -contains $Version) {
    Write-Host "Tag '$Version' already exists. Aborting." -ForegroundColor Red
    Read-Host "Press Enter to close"
    exit 1
}

git tag $Version
git push origin $Version

Write-Host "Tagged and pushed $Version" -ForegroundColor Green
Read-Host "Press Enter to close"