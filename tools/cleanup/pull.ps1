# Brings the asynchronous cleanup's commits (tools/cleanup/README.md) under the current branch before a push: fetches
# the cleaned branch and rebases local commits onto it, keeping local merge commits as merges (the Discord tracker and
# AGENTS.md rely on main's first-parent history) and setting uncommitted changes aside for the rebase. Then prints
# the cleanup's status. A conflict stops the rebase where git reports it, for the developer to resolve.
# Usage: pwsh -File tools/cleanup/pull.ps1   (run before git push)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'common.ps1')

try {
    Disable-ErrorDialogs
    $branch = & git -C $PSScriptRoot config --get cleanup.branch
    if ($LASTEXITCODE -eq 1) { throw 'The cleanup is not installed in this checkout (cleanup.branch is unset); see tools/cleanup/README.md.' }
    if ($LASTEXITCODE -ne 0) { throw "git config --get cleanup.branch failed (exit $LASTEXITCODE)." }
    & git pull --rebase=merges --autostash origin $branch
    if ($LASTEXITCODE -ne 0) { throw "git pull --rebase=merges --autostash origin $branch failed (exit $LASTEXITCODE); resolve what git reports above." }
    & pwsh -NoProfile -NonInteractive -File (Join-Path $PSScriptRoot 'status.ps1')
    if ($LASTEXITCODE -ne 0) { throw "status.ps1 failed (exit $LASTEXITCODE)." }
    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 1
}
