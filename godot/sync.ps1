# Brings the committed work of the other checkouts that play movies into this one. godot/movie.ps1 runs it before it
# builds, so that every movie is played over all the work there is and over nobody's unfinished edits.
# Usage: pwsh -File godot/sync.ps1
#
# Each checkout that plays movies is a worktree with a branch of its own: main, and branches named movie/<name>.
# This merges main and every movie/* branch, but for the branch checked out here, into that branch. Nothing is pushed
# and nothing is fetched: the worktrees share one repository, so a commit made in one is there for the others.
#
# Edits not yet committed here are set aside for the merge and put back after it (git's --autostash). A merge that
# brings a change to the extractor or to the old code it reads leaves this checkout's assets older than its
# extractor; godot/assets.ps1 -IfStale, which godot/movie.ps1 runs next, sees that for itself and makes them again.
#
# It ends with 0 when there was nothing to merge or all of it merged, with 1 when a merge needs somebody's hand (the
# two sides changed the same lines, or the edits put back met what was merged): the files are listed, and are left as
# git leaves them for that to be done. It ends with 2 on an error.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# The paths git holds as not merged yet.
function Get-UnmergedPaths([string] $Root) {
    $paths = @(& git -C $Root diff --name-only --diff-filter=U)
    if ($LASTEXITCODE -ne 0) { throw 'git could not list the paths not merged yet.' }
    return $paths
}

try {
    $root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

    $here = (& git -C $root symbolic-ref --short -q HEAD)
    if ($LASTEXITCODE -ne 0 -or -not $here) { throw "The checkout at $root is on no branch, so there is nothing to merge into." }

    $gitFolder = (& git -C $root rev-parse --absolute-git-dir)
    $unmerged = @(Get-UnmergedPaths $root)
    if ($unmerged.Count -gt 0 -or (Test-Path (Join-Path $gitFolder 'MERGE_HEAD'))) {
        Write-Output "SYNC STOPPED: a merge begun earlier in $root is not finished. Resolve the files below, commit the merge if one is open, then play the movie again."
        $unmerged | ForEach-Object { Write-Output "  not merged: $_" }
        exit 1
    }

    $branches = @(& git -C $root for-each-ref '--format=%(refname:short)' refs/heads/main refs/heads/movie) | Where-Object { $_ -ne $here }
    foreach ($branch in $branches) {
        & git -C $root merge-base --is-ancestor $branch HEAD
        if ($LASTEXITCODE -eq 0) { continue }
        if ($LASTEXITCODE -ne 1) { throw "git could not tell whether $branch is already in $here." }

        $arriving = @(& git -C $root log '--format=  %h %s' "HEAD..$branch")
        $said = (& git -C $root merge --no-edit --autostash $branch 2>&1 | Out-String)
        $merged = $LASTEXITCODE -eq 0
        $unmerged = @(Get-UnmergedPaths $root)

        if (-not $merged -or $unmerged.Count -gt 0) {
            Write-Output "SYNC STOPPED: $branch did not merge into $here by itself. Nothing was played."
            Write-Output $said.TrimEnd()
            $unmerged | ForEach-Object { Write-Output "  not merged: $_" }
            if ($merged) {
                Write-Output "The merge itself is made. The edits not yet committed here met it: they are in the files above, marked, and whole in git's stash. Resolve the files, take them out of the index (git restore --staged), then: git -C `"$root`" stash drop"
            }
            else {
                Write-Output "Resolve the files above and commit the merge, or leave it with: git -C `"$root`" merge --abort"
            }
            exit 1
        }

        Write-Output "Merged $branch into ${here}:"
        $arriving | ForEach-Object { Write-Output $_ }
    }

    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 2
}
