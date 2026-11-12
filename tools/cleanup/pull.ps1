# Brings the asynchronous cleanup's commits (tools/cleanup/README.md) into the current branch before a push: fetches the
# cleaned branch and, when it holds commits the branch lacks, merges them with --no-ff (main's first-parent history,
# which the Discord tracker reads, stays the developer's), the developer's side winning every conflicting line. Cleanup
# output is regenerated, never hand-merged (owner, 2026-11-12): a line it changed that the developer also changed is
# simply cleaned again by the next run, after the push. Then prints the cleanup's status. It refuses to run over
# uncommitted changes, which a merge must not mix with.
# Usage: pwsh -File tools/cleanup/pull.ps1   (run before git push)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'common.ps1')

try {
    Disable-ErrorDialogs
    $branch = & git -C $PSScriptRoot config --get cleanup.branch
    if ($LASTEXITCODE -eq 1) { throw 'The cleanup is not installed in this checkout (cleanup.branch is unset); see tools/cleanup/README.md.' }
    if ($LASTEXITCODE -ne 0) { throw "git config --get cleanup.branch failed (exit $LASTEXITCODE)." }
    $dirty = & git status --porcelain --untracked-files=no
    if ($LASTEXITCODE -ne 0) { throw "git status failed (exit $LASTEXITCODE)." }
    if ($dirty) { throw "Uncommitted changes are present; commit them before bringing in the cleanup's commits:`n$($dirty -join "`n")" }
    & git fetch --quiet origin $branch
    if ($LASTEXITCODE -ne 0) { throw "git fetch origin $branch failed (exit $LASTEXITCODE)." }
    $missing = & git rev-list --count "HEAD..FETCH_HEAD"
    if ($LASTEXITCODE -ne 0) { throw "git rev-list HEAD..FETCH_HEAD failed (exit $LASTEXITCODE)." }
    if ([int]$missing -gt 0) {
        & git merge --no-ff -X ours FETCH_HEAD -m "Merge the asynchronous cleanup's commits from origin/$branch; conflicting lines keep the developer's side, which the next cleanup run regenerates"
        if ($LASTEXITCODE -ne 0) { throw "git merge of origin/$branch failed (exit $LASTEXITCODE); resolve what git reports above." }
    }
    & pwsh -NoProfile -NonInteractive -File (Join-Path $PSScriptRoot 'status.ps1')
    if ($LASTEXITCODE -ne 0) { throw "status.ps1 failed (exit $LASTEXITCODE)." }
    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 1
}
