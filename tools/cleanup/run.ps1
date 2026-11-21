# One cleanup run on the clone the service has just reset to the remote tip (tools/cleanup/README.md). The service
# runs the tree's own copy, so the fixes always match the code they clean. Everything it prints goes to the run's log.
#
# 1. Restores the solution and builds the repository's analyzer and code fixes in Debug (the code fixes load them from
#    the projects' Debug references) and the cleanup runner, CodeFixRunner.cs, with the audits it runs.
# 2. In one process, on one load of the solution (CodeFixRunner.cs --cleanup): takes turns at the code fixes of the
#    automatically fixed rules (read from csharp/.editorconfig), the reachability prune of dead code and the
#    architecture audit's prune of stale reviews, until one of each in a row changes nothing (a tree seen before means
#    the steps cycle), then regenerates the architecture and module inventories.
# 4. When anything changed: builds the solution once with tools/build.ps1, commits as "Cleanup: ..." and pushes. When a
#    developer push moved the branch during the run, it merges its commit onto the new tip, builds the merge, and when
#    that does not build cleans the new tip again (steps 1 to 3 on it) and commits that as the merge; it pushes the
#    merge, until a push lands. The run's work is never discarded.
# The owner's rules (2026-11-15, #2599): a fix is always applied, and when it cannot be applied without breaking the
# code it breaks the code; nothing is left unfixed. A fix that leaves the code not compiling, a failed prune or
# inventory step, steps that do not settle or undo each other, and a failed build are each recorded as a problem; the
# run goes on and commits and pushes what it has, naming every problem in the commit body and the result. A fixer that
# offers no fix or throws is a defect of that fixer and fails the run, as a clone, restore, tool build, workspace load
# or push failure, where there is nothing to commit or no way to deliver it, does.
#
# Writes the outcome to -Result as JSON: result nothing or pushed (exit 0), or failed (nonzero exit).
param(
    [Parameter(Mandatory)][string]$Remote,
    [Parameter(Mandatory)][string]$Branch,
    [Parameter(Mandatory)][string]$Tip,
    [Parameter(Mandatory)][string]$Artifacts,
    [Parameter(Mandatory)][string]$Result
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'common.ps1')
$repository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent

# The builds use a compiler server of the cleanup's own, named apart from the developer's, so it never serves the
# developer's builds and runs at the cleanup's priority; one warm server compiles the solution faster than a compiler
# process started per project. It outlives the run only by its idle timeout, and a run within that time reuses it.
$env:UseSharedCompilation = 'true'
$env:SharedCompilationId = 'supermetroid-cleanup'
$solution = 'csharp/SuperMetroid.Full.slnx'

# The words the commit message uses for each rule of the automatically fixed group. The group itself is read from
# csharp/.editorconfig (Get-FixedRules); a rule there with no words here, or words here for a rule not there, fails
# the run, so the two never drift apart silently.
$FixedRuleWords = [ordered]@{
    CA1823 = 'unused private fields removed'
    CS0162 = 'unreachable code removed'
    CS0168 = 'unused variables removed'
    CS0169 = 'never-used fields removed'
    CS0219 = 'unread constant locals removed'
    CS0414 = 'write-only private fields removed'
    IDE0004 = 'redundant casts removed'
    IDE0005 = 'unused usings removed'
    IDE0032 = 'auto-properties used'
    IDE0044 = 'fields made read-only'
    IDE0051 = 'unused private members removed'
    IDE0052 = 'unread private members removed'
    IDE0059 = 'unnecessary value assignments removed'
    IDE0076 = 'invalid suppression targets removed'
    IDE0077 = 'legacy suppression targets updated'
    IDE0079 = 'unnecessary suppressions removed'
    IDE0330 = 'locks use System.Threading.Lock'
    SME6360 = 'partial types merged'
    SME6391 = 'span adapters before SequenceEqual removed'
    SME6400 = 'extra top-level types moved to their own files'
    SME6401 = 'files named after their types'
}

# The reachability prune's categories, source prefix and destination for dead code tests still use.
$PruneCategories = 'unreachable-member,unreachable-type,unreferenced-field,unreferenced-enum-member,unreferenced-constant'
$PrunePathPrefix = 'csharp/src'
$PruneDestination = 'csharp/test-support/VerificationAccess.Core'

# The bound on rounds of the code fixes, the dead-code prune and the stale-review prune. Each step either changes
# nothing or removes code, so real cascades settle in a few rounds; reaching this many means the steps keep undoing each
# other without repeating a tree exactly.
$MaxRounds = 6

function Write-Step([string]$Text) {
    Write-Host ''
    Write-Host "=== $([DateTime]::UtcNow.ToString('yyyy-MM-ddTHH:mm:ssZ')) $Text"
}

function Invoke-Checked([string]$Description, [scriptblock]$Command) {
    # Runs a native command with its output in the log; a nonzero exit fails the run, naming the step.
    $started = Get-Date
    & $Command | Out-Host
    $exit = $LASTEXITCODE
    Write-Host "--- $Description exit $exit after $([int]((Get-Date) - $started).TotalSeconds) s"
    if ($exit -ne 0) { throw "$Description failed (exit $exit)." }
}

function Invoke-Git([string[]]$Arguments) {
    $output = & git -C $repository @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) { throw "git $($Arguments -join ' ') failed (exit $LASTEXITCODE): $($output -join "`n")" }
    return (($output | ForEach-Object { "$_" }) -join "`n").Trim()
}

function Get-TreeId {
    # The tree the working copy holds now, including new and deleted files.
    $null = Invoke-Git @('add', '--all')
    return Invoke-Git @('write-tree')
}

function Get-ChangedPaths([string]$From, [string]$To) {
    $list = Invoke-Git @('diff', '--name-only', $From, $To)
    return @($list -split "`n" | Where-Object { $_ })
}

function Copy-BuildLog([string]$Name) {
    # tools/build.ps1 writes the build's full output to the temporary folder, which the service gives this run alone.
    $logs = @(Get-ChildItem -LiteralPath ([IO.Path]::GetTempPath()) -Filter 'supermetroid-build-*.log')
    if ($logs.Count -ne 1) { throw "Expected one build log in $([IO.Path]::GetTempPath()), found $($logs.Count)." }
    $target = Join-Path $Artifacts $Name
    Move-Item -LiteralPath $logs[0].FullName -Destination $target
    return $target
}

function Invoke-Build([string]$Description, [string]$LogName, [string[]]$Arguments) {
    # Builds through tools/build.ps1, keeping its full log with the run; returns the exit code and the log path.
    Get-ChildItem -LiteralPath ([IO.Path]::GetTempPath()) -Filter 'supermetroid-build-*.log' | Remove-Item
    $started = Get-Date
    & pwsh -NoProfile -NonInteractive -File (Join-Path $repository 'tools/build.ps1') @Arguments | Out-Host
    $exit = $LASTEXITCODE
    $log = Copy-BuildLog $LogName
    Write-Host "--- $Description exit $exit after $([int]((Get-Date) - $started).TotalSeconds) s; full output $log"
    return @{ Exit = $exit; Log = $log }
}

function Save-Result([hashtable]$Outcome) {
    Write-AtomicText $Result ($Outcome | ConvertTo-Json -Depth 6)
}

function Get-FixedRules {
    # The automatically fixed group: the `dotnet_diagnostic.<rule>.severity = none` lines directly under the comment in
    # csharp/.editorconfig that names the asynchronous cleanup.
    $path = Join-Path $repository 'csharp/.editorconfig'
    $lines = [IO.File]::ReadAllLines($path)
    $comments = @(for ($index = 0; $index -lt $lines.Count; $index++) { if ($lines[$index] -match '^#.*\basynchronous cleanup\b') { $index } })
    if ($comments.Count -ne 1) { throw "Expected one comment naming the asynchronous cleanup in $path, found $($comments.Count)." }
    $line = $comments[0]
    while ($line -lt $lines.Count -and $lines[$line].StartsWith('#')) { $line++ }
    $group = [Collections.Generic.List[string]]::new()
    while ($line -lt $lines.Count -and $lines[$line] -match '^dotnet_diagnostic\.(\w+)\.severity = none$') {
        $group.Add($Matches[1])
        $line++
    }
    if ($group.Count -eq 0) { throw "No 'dotnet_diagnostic.<rule>.severity = none' line follows the asynchronous cleanup's comment in $path." }
    foreach ($rule in $group) {
        if (-not $FixedRuleWords.Contains($rule)) {
            throw "$path puts $rule in the automatically fixed group, but run.ps1 has no commit-message words for it."
        }
    }
    foreach ($rule in @($FixedRuleWords.Keys)) {
        if (-not $group.Contains($rule)) { throw "run.ps1 names $rule, which $path does not put in the automatically fixed group." }
    }
    return @($group)
}

function Initialize-Tools {
    # Restores the solution and builds what the run starts: the repository's analyzer and code fixes in Debug, which the
    # code fixes load from the projects' Debug references, and the cleanup runner with the audits it runs. Each comes
    # from the tree the working copy holds, so the fixes always match the code they clean.
    Write-Step 'restore, the code fixes'' Debug build and the cleanup runner''s build'
    Invoke-Checked 'restore' { dotnet restore $solution -v:q }
    $fixers = Invoke-Build 'repository analyzer and code fixes Debug build' "$script:PassFolder/code-fixes-build.log" @('csharp/src/SuperMetroid.EnsureAnalyzer.CodeFixes')
    if ($fixers.Exit -ne 0) { throw "The repository analyzer and code fixes do not build in Debug; see $($fixers.Log)." }
    $runner = Invoke-Build 'cleanup runner build (CodeFixRunner.cs and the audits it runs)' "$script:PassFolder/cleanup-runner-build.log" @('tools/cleanup/CodeFixRunner.cs')
    if ($runner.Exit -ne 0) { throw "tools/cleanup/CodeFixRunner.cs does not build; see $($runner.Log)." }
}

function Invoke-CleanupPass([string]$Of) {
    # Cleans the working copy, which holds commit $Of: the fix and prune loop and the inventories in one process on one
    # load of the solution (CodeFixRunner.cs --cleanup), which prints each step to the log and writes what the steps did
    # to the outcome file, then one build. Returns the changed paths (none when there was nothing to clean), the build's
    # exit code and log, and the commit subject and body that describe the pass. Its files go in $script:PassFolder.
    $folder = Join-Path $Artifacts $script:PassFolder
    $fixedRules = Get-FixedRules
    $outcomePath = Join-Path $folder 'cleanup-outcome.json'
    Invoke-Checked 'fix and prune loop and inventories' {
        dotnet run --no-build tools/cleanup/CodeFixRunner.cs -- --cleanup $solution $folder $outcomePath ($fixedRules -join ',') `
            $PruneCategories $PrunePathPrefix $PruneDestination $MaxRounds
    }
    $outcome = Get-Content -LiteralPath $outcomePath -Raw | ConvertFrom-Json
    $codeFixes = [ordered]@{}
    foreach ($property in $outcome.CodeFixes.PSObject.Properties) { $codeFixes[$property.Name] = [int]$property.Value }
    $fixedPaths = @($outcome.FixedPaths)
    $prunedPaths = @($outcome.PrunedPaths)
    $prunedSummaries = @($outcome.PrunedSummaries)
    $retiredFields = @($outcome.RetiredFields)
    $reviewPaths = @($outcome.ReviewPaths)
    $staleReviews = @($outcome.StaleReviews)
    $inventoryPaths = @($outcome.InventoryPaths)
    $problems = [Collections.Generic.List[string]]::new()
    foreach ($problem in $outcome.Problems) { $problems.Add($problem) }
    foreach ($step in $outcome.Steps) {
        Write-Host "step $(if ($null -ne $step.Number) { $step.Number } else { '-' }) $($step.Kind): $([int]$step.Seconds) s, $($step.ChangedFiles) files"
    }
    $final = Get-TreeId
    if ($final -ne $outcome.Tree) { throw "The working copy holds tree $final, not the $($outcome.Tree) the cleanup runner reported." }

    $changed = @(Get-ChangedPaths $Of $final)
    if ($changed.Count -eq 0) { return @{ Changed = $changed } }
    $outside = @($changed | Where-Object { -not ($_.StartsWith('csharp/') -or $_.StartsWith('docs/')) })
    if ($outside.Count -gt 0) { throw "The cleanup changed files outside csharp/ and docs/: $($outside -join ', ')" }

    if ($null -ne $outcome.BuildExit) {
        # The runner built the solution while it wrote the inventories, since the loop had changed the code; the build
        # left its full output in the temporary folder, as tools/build.ps1 always does.
        $build = @{ Exit = [int]$outcome.BuildExit; Log = Copy-BuildLog "$script:PassFolder/build.log" }
        Write-Host "--- build exit $($build.Exit); full output $($build.Log)"
    }
    else {
        Write-Step 'build (tools/build.ps1, the whole solution)'
        $build = Invoke-Build 'build' "$script:PassFolder/build.log" @($solution)
    }
    if ($build.Exit -ne 0) {
        $problems.Add("The cleaned tree does not build (it is committed and pushed anyway); build output: $($build.Log)")
        Write-Host "problem: $($problems[-1])"
    }
    $built = Get-TreeId
    if ($built -ne $final) {
        throw "The build changed the tree, which tools/build.ps1 never does: $(@(Get-ChangedPaths $final $built) -join ', ')"
    }

    $parts = [Collections.Generic.List[string]]::new()
    foreach ($rule in $fixedRules) {
        if ($codeFixes[$rule]) { $parts.Add("$rule $($FixedRuleWords[$rule]) $($codeFixes[$rule])") }
    }
    if ($prunedPaths.Count -gt 0) { $parts.Add("dead code pruned in $($prunedPaths.Count) files") }
    if ($staleReviews.Count -gt 0) { $parts.Add("stale architecture reviews removed $($staleReviews.Count)") }
    if ($inventoryPaths.Count -gt 0) { $parts.Add('inventories regenerated') }
    if ($parts.Count -eq 0) { $parts.Add("$($changed.Count) files changed that no step accounts for") }
    $subject = if ($build.Exit -ne 0) { "Cleanup (does not build): $($parts -join '; ')" } else { "Cleanup: $($parts -join '; ')" }
    $body = [Collections.Generic.List[string]]::new()
    $body.Add("The asynchronous cleanup of $Of (tools/cleanup/README.md).")
    $body.Add('')
    if ($fixedPaths.Count -gt 0) {
        $body.Add("Code fixes in $($fixedPaths.Count) files:")
        $fixedPaths | ForEach-Object { $body.Add("  $_") }
    }
    if ($prunedPaths.Count -gt 0) {
        $body.Add("Reachability prune ($PruneCategories) in $($prunedPaths.Count) files:")
        $prunedSummaries | ForEach-Object { $body.Add("  $_") }
        $prunedPaths | ForEach-Object { $body.Add("  $_") }
    }
    if ($retiredFields.Count -gt 0) {
        $body.Add('Retired instance fields:')
        $retiredFields | ForEach-Object { $body.Add("  $_") }
    }
    if ($staleReviews.Count -gt 0) {
        $body.Add("Stale architecture reviews removed ($($staleReviews.Count), in $($reviewPaths.Count) files):")
        $staleReviews | ForEach-Object { $body.Add("  $_") }
    }
    if ($inventoryPaths.Count -gt 0) { $body.Add("Inventories regenerated: $($inventoryPaths -join ', ')") }
    if ($problems.Count -gt 0) {
        $body.Add("Problems the run carried on past ($($problems.Count)):")
        $problems | ForEach-Object { $body.Add("  $(($_ -split "`n")[0])") }
    }
    $body.Add($(if ($build.Exit -ne 0) { "Does not build with tools/build.ps1 $solution; see the problems above." } else { "Built with tools/build.ps1 $solution." }))
    return @{ Changed = $changed; Build = $build; Subject = $subject; Body = $body; Problems = $problems }
}

function Set-PassFolder([string]$Name) {
    # Gives the next pass's outcome, step summaries and build logs a folder of their own in the run's artifacts.
    $script:PassFolder = $Name
    $null = New-Item -ItemType Directory -Force -Path (Join-Path $Artifacts $Name)
}

function Complete-Merge([string]$Commit, [string]$Moved) {
    # Merges $Commit onto $Moved, which a developer push made the branch's tip during the run, leaving the merge
    # uncommitted. The new tip is the merge's first parent, so the branch's first-parent history stays the developer's,
    # and the developer's side wins every conflict, as tools/cleanup/pull.ps1 merges the other way; the next run
    # regenerates whatever that leaves. Returns the files a conflict kept whole from the developer's side.
    $null = Invoke-Git @('checkout', '--quiet', '--detach', $Moved)
    $merge = & git -C $repository merge --no-ff --no-commit -X ours $Commit 2>&1
    $mergeExit = $LASTEXITCODE
    $merge | ForEach-Object { Write-Host "$_" }
    # Exit 1 means conflicts -X ours could not settle (a file one side deleted and the other changed); others fail.
    if ($mergeExit -ne 0 -and $mergeExit -ne 1) { throw "git merge of $Commit onto $Moved failed (exit $mergeExit): $($merge -join "`n")" }
    $unmerged = @((Invoke-Git @('diff', '--name-only', '--diff-filter=U')) -split "`n" | Where-Object { $_ })
    foreach ($path in $unmerged) {
        # The developer's side wins: their version of the file, or its absence when they deleted it.
        & git -C $repository cat-file -e "HEAD:$path" 2>$null
        if ($LASTEXITCODE -eq 0) {
            $null = Invoke-Git @('checkout', '--ours', '--', $path)
            $null = Invoke-Git @('add', '--', $path)
        }
        else {
            $null = Invoke-Git @('rm', '--quiet', '--', $path)
        }
        Write-Host "conflict in $path kept the developer's side"
    }
    return $unmerged
}

function Write-CommitMessage([string]$Name, [string]$Subject, [Collections.Generic.List[string]]$Body) {
    # Writes a commit message to the run's artifacts and returns its path.
    $path = Join-Path $Artifacts $Name
    [IO.File]::WriteAllText($path, "$Subject`n`n$($Body -join "`n")`n", [Text.UTF8Encoding]::new($false))
    return $path
}

try {
    Disable-ErrorDialogs
    Set-Location $repository
    Write-Step "cleanup of $Tip ($Remote/$Branch) in $repository"
    $head = Invoke-Git @('rev-parse', 'HEAD')
    if ($head -ne $Tip) { throw "The clone is at $head, not at $Tip." }
    if (Invoke-Git @('status', '--porcelain')) { throw 'The clone has local changes before the run.' }

    Set-PassFolder '.'
    Initialize-Tools
    $pass = Invoke-CleanupPass $Tip
    if ($pass.Changed.Count -eq 0) {
        Write-Step 'nothing to clean'
        Save-Result @{ result = 'nothing'; tip = $Tip; message = 'Nothing to clean.' }
        exit 0
    }
    $subject = $pass.Subject
    $problems = $pass.Problems
    $messageFile = Write-CommitMessage 'commit-message.txt' $subject $pass.Body

    Write-Step 'commit'
    Invoke-Checked 'commit' { git -C $repository commit --quiet --file $messageFile }
    $commit = Invoke-Git @('rev-parse', 'HEAD')
    Write-Host "committed $commit`: $subject"

    # A push rejected because a developer push landed during the run never discards the run's work (owner, 2026-11-15):
    # the cleanup's commit is merged onto the branch's new tip (Complete-Merge) and that merge is pushed, until a push
    # lands. The merged tree is built first. The cleanup of an older tip can break the newer one: a member it moved into
    # test-support or a field it removed as unread that the developer's newer code uses, or a caller the developer added
    # of a member it moved (the merge of e4171830fc onto 4813017a1d, 2026-11-15). When the merged tree does not build,
    # the cleanup runs again on the new tip itself, as a fresh run of that tip would, and its result is committed as the
    # merge (an "ours" merge of the earlier commit, which stays in the history as the second parent), so what is pushed is
    # the cleanup of the code it lands on. Should that still not build, it is committed and pushed anyway, naming the
    # failure, as the first pass would be. A rejection while the branch has not moved, or any other push failure, fails
    # the run.
    $rejectedPattern = "^!\s+HEAD:refs/heads/$([regex]::Escape($Branch))\s+\[rejected\] \((fetch first|non-fast-forward)\)"
    $base = $Tip
    $delivered = $commit
    $merges = 0
    while ($true) {
        Write-Step "push to $Remote $Branch"
        $pushed = Invoke-NetworkGit $repository @('push', '--porcelain', $Remote, "HEAD:refs/heads/$Branch") {
            param($Lines) @($Lines | Where-Object { $_ -match $rejectedPattern }).Count -eq 1
        }.GetNewClosure()
        $push = $pushed.Output
        $exit = $pushed.Exit
        $push | ForEach-Object { Write-Host $_ }
        Write-Host "--- push exit $exit after $($pushed.Attempt) attempt(s)"
        if ($exit -eq 0) {
            Save-Result @{ result = 'pushed'; tip = $Tip; commit = $delivered; message = $subject; problems = @($problems) }
            exit 0
        }
        # A stale tip is reported as rejected with "fetch first" or "non-fast-forward"; anything else is a failure.
        $rejection = @($push | Where-Object { $_ -match $rejectedPattern })
        if ($rejection.Count -ne 1) { throw "The push failed (exit $exit): $($push -join "`n")" }
        $fetched = Invoke-NetworkGit $repository @('fetch', '--quiet', '--no-tags', $Remote,
            "+refs/heads/${Branch}:refs/remotes/$Remote/$Branch") $null
        if ($fetched.Exit -ne 0) {
            throw "git fetch of $Remote/$Branch failed on all $($fetched.Attempt) attempts (exit $($fetched.Exit)): $($fetched.Output -join "`n")"
        }
        $moved = Invoke-Git @('rev-parse', "$Remote/$Branch")
        if ($moved -eq $base) { throw "The push of $delivered was rejected, but $Branch is still at ${base}: $($rejection[0])" }
        $merges++

        Write-Step "merge $delivered onto $moved, where a developer push moved $Branch during the run"
        $unmerged = @(Complete-Merge $delivered $moved)
        Set-PassFolder "merge-$merges"
        Invoke-Checked 'restore of the merged tree' { dotnet restore $solution -v:q }
        $mergedBuild = Invoke-Build 'build of the merged tree' "merge-$merges/merged-build.log" @($solution)
        $mergedSubject = "$subject (merged onto $($moved.Substring(0, 10)))"
        if ($mergedBuild.Exit -eq 0) {
            $mergeMessage = Write-CommitMessage "merge-$merges/merge-message.txt" $mergedSubject ([Collections.Generic.List[string]]@(
                "The asynchronous cleanup of $Tip (commit $delivered), merged onto $moved, where a developer push moved $Branch " +
                "during the run. Conflicting lines keep the developer's side, which the next cleanup run regenerates" +
                $(if ($unmerged.Count -gt 0) { " (whole files: $($unmerged -join ', '))" } else { '' }) +
                ". The merged tree builds with tools/build.ps1 $solution."))
        }
        else {
            # The merge does not build: the cleanup runs again on the developer's new tip, and its result is the merge.
            Write-Host "problem: the merge of $delivered onto $moved does not build; see $($mergedBuild.Log). The cleanup runs again on $moved."
            $null = Invoke-Git @('merge', '--abort')
            $null = Invoke-Git @('checkout', '--quiet', '--detach', $moved)
            $ours = & git -C $repository merge --no-ff --no-commit -s ours $delivered 2>&1
            if ($LASTEXITCODE -ne 0) { throw "git merge -s ours of $delivered onto $moved failed (exit $LASTEXITCODE): $($ours -join "`n")" }
            Initialize-Tools
            $rerun = Invoke-CleanupPass $moved
            if ($rerun.Changed.Count -eq 0) {
                # The new tip needs nothing the cleanup does: what the earlier commit held is already there or gone.
                $null = Invoke-Git @('merge', '--abort')
                Write-Step "nothing to clean on $moved"
                Save-Result @{ result = 'nothing'; tip = $Tip; message = "Nothing to clean on $moved, which a developer push made the tip during the run of $Tip." }
                exit 0
            }
            $subject = $rerun.Subject
            $problems = $rerun.Problems
            $mergedSubject = "$subject (merged onto $($moved.Substring(0, 10)), cleaned again)"
            $body = [Collections.Generic.List[string]]::new()
            $body.Add("The asynchronous cleanup of $Tip (commit $delivered) did not build merged onto $moved, where a developer push " +
                "moved $Branch during the run (build output: $($mergedBuild.Log)). The cleanup ran again on $moved; this merge " +
                "holds its result, and $delivered stays in the history as the merge's second parent.")
            $body.Add('')
            $rerun.Body | ForEach-Object { $body.Add($_) }
            $mergeMessage = Write-CommitMessage "merge-$merges/merge-message.txt" $mergedSubject $body
        }
        Invoke-Checked 'merge commit' { git -C $repository commit --quiet --file $mergeMessage }
        $delivered = Invoke-Git @('rev-parse', 'HEAD')
        $base = $moved
        Write-Host "merged as $delivered"
    }
}
catch {
    $failure = "$($_.Exception.ToString())`n$($_.ScriptStackTrace)"
    [Console]::Error.WriteLine($failure)
    if (-not (Test-Path -LiteralPath $Result)) { Save-Result @{ result = 'failed'; tip = $Tip; message = $_.Exception.Message } }
    exit 1
}
