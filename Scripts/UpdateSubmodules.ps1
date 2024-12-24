Write-Host "Updating submodules"

git submodule update --init --remote
git submodule foreach git pull origin main
git submodule foreach git checkout main

$TestResourcesDir = Join-Path $PSScriptRoot "../TestResources"

Push-Location (Join-Path $TestResourcesDir "TestRepository")
git checkout feature
Pop-Location

Push-Location (Join-Path $TestResourcesDir "TestRepository2")
git checkout feature1
Pop-Location