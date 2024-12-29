Write-Host "Updating repository and submodules to ensure a clean state"

# Initialize and force update submodules
if (-not (Test-Path ".gitmodules")) {
  git submodule sync
  git submodule update --init --recursive --force
}

# Ensure submodules are reset to a clean state
git submodule foreach --recursive git rebase --abort
git submodule foreach --recursive git fetch origin --force
git submodule foreach --recursive git checkout main --force
git submodule foreach --recursive git reset --hard origin/main
git submodule foreach --recursive git clean -fdx

$TestResourcesDir = Join-Path $PSScriptRoot "../TestResources"

# Reset and update specific repositories
Push-Location (Join-Path $TestResourcesDir "TestRepository")
git rebase --abort
git fetch origin feature:feature --force
git checkout feature
git clean -fdx
Pop-Location

Push-Location (Join-Path $TestResourcesDir "TestRepository2")
git rebase --abort
git fetch origin feature:feature --force
git checkout feature
git clean -fdx
Pop-Location

Write-Host "Repositories and submodules are updated and clean."