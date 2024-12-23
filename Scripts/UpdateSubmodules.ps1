Write-Host "Updating submodules"

git submodule update --init --remote
git submodule foreach git pull origin main
git submodule foreach git checkout main

Set-Location "$PSScriptRoot/TestResources/TestRepository"
git checkout feature