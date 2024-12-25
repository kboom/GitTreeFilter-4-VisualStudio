Write-Host "Updating submodules"

git submodule update --init --remote
git submodule foreach git fetch origin
git submodule foreach git checkout main
git submodule foreach git reset --hard origin/main

$TestResourcesDir = Join-Path $PSScriptRoot "../TestResources"

Push-Location (Join-Path $TestResourcesDir "TestRepository")
git checkout feature
git reset --hard origin/feature
Pop-Location

Push-Location (Join-Path $TestResourcesDir "TestRepository2")
git checkout feature1
git reset --hard origin/feature1
Pop-Location
