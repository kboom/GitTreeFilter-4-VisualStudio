Write-Host "Updating submodules"

git submodule update --init --remote
git submodule foreach git pull origin main
git submodule foreach git checkout main

$TestResourcesDir = Join-Path $PSScriptRoot "../TestResources"

git --git-dir (Join-Path $TestResourcesDir "TestRepository") checkout feature
git --git-dir (Join-Path $TestResourcesDir "TestRepository2") checkout feature