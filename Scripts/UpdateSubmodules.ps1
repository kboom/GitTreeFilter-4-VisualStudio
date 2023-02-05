Write-Host "Updating submodules"
git submodule update --init
git fetch
git submodule foreach git pull origin main
git submodule foreach git checkout main