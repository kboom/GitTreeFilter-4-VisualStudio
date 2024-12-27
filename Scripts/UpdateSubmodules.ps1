Write-Host "Updating submodules"

git submodule update --init --remote
git submodule foreach git fetch origin
git submodule foreach git checkout main
git submodule foreach git reset --hard origin/main

$TestResourcesDir = Join-Path $PSScriptRoot "../TestResources"

Push-Location (Join-Path $TestResourcesDir "TestRepository")
git branch -D feature
git fetch origin feature
git checkout feature
Pop-Location

Push-Location (Join-Path $TestResourcesDir "TestRepository2")
git branch -D feature1
git fetch origin feature1
git checkout feature1
Pop-Location

Push-Location (Join-Path $TestResourcesDir "TestRepository2")
git branch -D feature2
git fetch origin feature2
git checkout feature2
Pop-Location