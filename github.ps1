# Ensure script stops on errors
$ErrorActionPreference = "Stop"

# Fetch latest tags from remote
Write-Host "Fetching latest tags from remote..." -ForegroundColor Yellow
git fetch --tags

# Get the latest remote tag (sorted by version)
$lastTag = git ls-remote --tags origin | Select-String -Pattern 'refs/tags/(\d+\.\d+\.\d+(-preview|alpha|beta|rc)\.\d+)' | 
    ForEach-Object { $_.Matches.Groups[1].Value } | 
    Sort-Object { [System.Version]($_ -replace '-preview|alpha|beta|rc', '') } -Descending | 
    Select-Object -First 1

if ($lastTag -match '^(\d+)\.(\d+)\.(\d+)-(.+)\.(\d+)$') {
    Write-Host "Last valid tag found: $lastTag" -ForegroundColor Cyan
    $major = [int]$matches[1]
    $minor = [int]$matches[2]
    $patch = [int]$matches[3]
    $preRelease = $matches[4]  # e.g. "preview", "alpha", etc.
    $build = [int]$matches[5] + 1  # Increment last number in pre-release
    $newTag = "$major.$minor.$patch-$preRelease.$build"
} else {
    Write-Host "No valid tag found. Starting from 1.0.0-preview.0..." -ForegroundColor Green
    $newTag = "1.0.0-preview.0"
}

# Commit and add changes (if needed)
Write-Host "Committing changes with tag: $newTag" -ForegroundColor Cyan
git add .
git commit -m "Version $newTag"

# Remove all existing tags (local and remote)
Write-Host "Removing all local and remote tags..." -ForegroundColor Yellow
git tag | ForEach-Object { git tag -d $_ }
git ls-remote --tags origin | ForEach-Object {
    $tag = ($_ -split "refs/tags/")[-1]
    git push --delete origin $tag
}

# Create the new tag
Write-Host "Creating new tag: $newTag" -ForegroundColor Green
git tag $newTag

# Push everything (commits + tags) in one go
Write-Host "Pushing changes and tags to remote..." -ForegroundColor Yellow
# git push --force
# git push origin --tags --force

Write-Host "Tagging and pushing completed successfully!" -ForegroundColor Green