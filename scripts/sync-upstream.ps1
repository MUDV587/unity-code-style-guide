# Syncs this fork with the upstream repository:
#   1. fast-forwards local 'main' from 'upstream/main'
#   2. merges 'main' into 'Unity6000'
#   3. merges 'Unity6000' into 'Unity2022'
#   4. pushes all three branches (-Push)
#
# Rebase is deliberately not used — merge commits keep the layered history
# (main -> Unity6000 -> Unity2022) replayable and conflict-localized to the
# style-rename files. Run from anywhere inside the repo:
#
#   ./scripts/sync-upstream.ps1          # local merges only
#   ./scripts/sync-upstream.ps1 -Push    # also push main, Unity6000, Unity2022
#
# Conflicts, if any, are expected in the style-rename files (see UnityStyleGuide.md,
# the .editorconfig, and the docs). Resolve, then re-run with -Push.

[CmdletBinding()]
param(
    [switch]$Push
)

$ErrorActionPreference = "Stop"

function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GitArgs)
    git @GitArgs
    if ($LASTEXITCODE -ne 0) {
        throw "git $($GitArgs -join ' ') failed with exit code $LASTEXITCODE"
    }
}

# Preconditions
$remoteNames = git remote
if ($remoteNames -notcontains "upstream") {
    throw "No 'upstream' remote configured. Run: git remote add upstream https://github.com/krogh-jacobsen/unity-code-style-guide.git"
}
if (git status --porcelain) {
    throw "Working tree is not clean. Commit or stash your changes first."
}

$current = (git rev-parse --abbrev-ref HEAD).Trim()

try {
    Write-Host "==> Fetching upstream"
    Invoke-Git fetch upstream

    Write-Host "==> Fast-forwarding main from upstream/main"
    Invoke-Git checkout main
    Invoke-Git merge --ff-only upstream/main

    Write-Host "==> Merging main into Unity6000"
    Invoke-Git checkout Unity6000
    Invoke-Git merge --no-edit main

    Write-Host "==> Merging Unity6000 into Unity2022"
    Invoke-Git checkout Unity2022
    Invoke-Git merge --no-edit Unity6000

    if ($Push) {
        Write-Host "==> Pushing main, Unity6000, Unity2022"
        Invoke-Git push origin main Unity6000 Unity2022
    }
    else {
        Write-Host "==> Local merges complete. Re-run with -Push to push to origin."
    }

    Write-Host "==> Sync complete."
}
finally {
    if ($current -and (git rev-parse --verify $current 2>$null)) {
        git checkout $current | Out-Null
    }
}
