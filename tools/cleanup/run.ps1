# One cleanup run on the clone the service has just reset to the remote tip (tools/cleanup/README.md). The service
# runs the tree's own copy, so the fixes always match the code they clean. Everything it prints goes to the run's log.
#
# 1. Restores the solution and builds the reachability audit in Release (the prune runs it without building).
# 2. Alternates the code fixes of the automatically fixed rules (CodeFixRunner.cs) with the reachability prune of dead
#    code until one of each in a row changes nothing; a tree seen before means the steps cycle, and the run fails.
# 3. Regenerates the architecture and module inventories as the pre-push gate does.
# 4. When anything changed: builds the solution once with tools/build.ps1, commits as "Cleanup: ..." and pushes with
#    a plain fast-forward push. A failed build pushes nothing; its full output is kept with the run.
#
# Writes the outcome to -Result as JSON: result nothing, pushed or rejected (exit 0), or failed (nonzero exit).
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
$solution = 'csharp/SuperMetroid.Full.slnx'

# The rules csharp/.editorconfig places in the automatically fixed group that a command-line code fix can apply, with
# the words the commit message uses for each. IDE0079 is in the group but no command-line analyzer reports it (the
# code-style package has none; README.md), so the cleanup cannot find its sites.
$FixedRules = [ordered]@{
    IDE0004 = 'redundant casts removed'
    IDE0005 = 'unused usings removed'
    IDE0044 = 'fields made read-only'
    IDE0076 = 'invalid suppression targets removed'
    IDE0077 = 'legacy suppression targets updated'
}

# The reachability prune's categories, source prefix and destination for dead code tests still use.
$PruneCategories = 'unreachable-member,unreachable-type,unreferenced-field,unreferenced-enum-member,unreferenced-constant'

# The bound on fix and prune steps. Each step either changes nothing or removes code, so real cascades settle in a few
# steps; reaching this many means the steps keep undoing each other without repeating a tree exactly.
$MaxSteps = 12

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

try {
    Disable-ErrorDialogs
    Set-Location $repository
    Write-Step "cleanup of $Tip ($Remote/$Branch) in $repository"
    $head = Invoke-Git @('rev-parse', 'HEAD')
    if ($head -ne $Tip) { throw "The clone is at $head, not at $Tip." }
    if (Invoke-Git @('status', '--porcelain')) { throw 'The clone has local changes before the run.' }

    Write-Step 'restore and the reachability audit''s Release build'
    Invoke-Checked 'restore' { dotnet restore $solution -v:q }
    $audit = Invoke-Build 'reachability audit Release build' 'reachability-audit-build.log' @('csharp/src/SuperMetroid.ReachabilityAudit', '-c', 'Release')
    if ($audit.Exit -ne 0) { throw "The reachability audit does not build in Release; see $($audit.Log)." }

    $codeFixes = [ordered]@{}
    $kept = [ordered]@{}
    $prunedPaths = [Collections.Generic.SortedSet[string]]::new([StringComparer]::Ordinal)
    $prunedSummaries = [Collections.Generic.List[string]]::new()
    $retiredFields = [Collections.Generic.List[string]]::new()
    $fixedPaths = [Collections.Generic.SortedSet[string]]::new([StringComparer]::Ordinal)
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $tree = Get-TreeId
    $null = $seen.Add($tree)
    $unchangedInARow = 0
    for ($step = 1; $unchangedInARow -lt 2; $step++) {
        if ($step -gt $MaxSteps) { throw "The fix and prune steps did not settle within $MaxSteps steps." }
        $isFix = $step % 2 -eq 1
        if ($isFix) {
            Write-Step "step $step`: code fixes ($($FixedRules.Keys -join ', '))"
            $summaryPath = Join-Path $Artifacts "code-fixes-$step.json"
            Invoke-Checked 'code fixes' { dotnet run tools/cleanup/CodeFixRunner.cs -- $solution $summaryPath ($FixedRules.Keys -join ',') }
            $summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
            foreach ($property in $summary.Fixed.PSObject.Properties) { $codeFixes[$property.Name] = [int]$codeFixes[$property.Name] + [int]$property.Value }
            $kept = [ordered]@{}
            foreach ($property in $summary.Kept.PSObject.Properties) { $kept[$property.Name] = [int]$property.Value }
            if ($summary.ProjectsWithCompilerErrors.Count -gt 0) {
                Write-Output "projects with compiler errors, whose files were left alone: $($summary.ProjectsWithCompilerErrors -join ', ')"
            }
        }
        else {
            Write-Step "step $step`: reachability prune of dead code"
            $retired = Join-Path $Artifacts "retired-fields-$step.txt"
            [IO.File]::WriteAllText($retired, '')
            $output = & dotnet run --project csharp/src/SuperMetroid.ReachabilityAudit -c Release --no-build -- --solution $solution `
                --prune-keeping-verification-used $PruneCategories --path-prefix csharp/src `
                --to csharp/test-support/VerificationAccess.Core --retired-fields $retired 2>&1
            $exit = $LASTEXITCODE
            $output | ForEach-Object { "$_" }
            Write-Output "--- reachability prune exit $exit"
            if ($exit -ne 0) { throw "The reachability prune failed (exit $exit)." }
            $output | ForEach-Object { "$_" } | Where-Object { $_ -match '^(Removed|Moved|Relocated|Deleted)\b' } | ForEach-Object { $prunedSummaries.Add($_) }
            Get-Content -LiteralPath $retired | Where-Object { $_ } | ForEach-Object { $retiredFields.Add($_) }
        }
        $next = Get-TreeId
        if ($next -eq $tree) {
            $unchangedInARow++
            Write-Output 'no change'
            continue
        }
        $paths = Get-ChangedPaths $tree $next
        foreach ($path in $paths) { $null = $(if ($isFix) { $fixedPaths.Add($path) } else { $prunedPaths.Add($path) }) }
        Write-Output "changed $($paths.Count) files"
        if (-not $seen.Add($next)) { throw "Step $step returned the tree to an earlier state ($next): the fixes and the prune undo each other." }
        $tree = $next
        $unchangedInARow = 0
    }

    Write-Step 'inventories (ArchitectureAudit --write, as the pre-push gate runs it)'
    Invoke-Checked 'inventories' { dotnet run --project csharp/src/SuperMetroid.ArchitectureAudit -c Release -- --solution $solution --write }
    $final = Get-TreeId
    $inventoryPaths = @(Get-ChangedPaths $tree $final)

    $changed = @(Get-ChangedPaths $Tip $final)
    if ($changed.Count -eq 0) {
        Write-Step 'nothing to clean'
        Save-Result @{ result = 'nothing'; tip = $Tip; message = 'Nothing to clean.' }
        exit 0
    }
    $outside = @($changed | Where-Object { -not ($_.StartsWith('csharp/') -or $_.StartsWith('docs/')) })
    if ($outside.Count -gt 0) { throw "The cleanup changed files outside csharp/ and docs/: $($outside -join ', ')" }

    Write-Step 'build (tools/build.ps1, the whole solution)'
    $build = Invoke-Build 'build' 'build.log' @($solution)
    if ($build.Exit -ne 0) {
        $message = "The cleaned tree does not build; nothing was pushed. Build output: $($build.Log)"
        Save-Result @{ result = 'failed'; tip = $Tip; message = $message; buildLog = $build.Log }
        throw $message
    }
    $built = Get-TreeId
    if ($built -ne $final) {
        $byBuild = @(Get-ChangedPaths $final $built)
        Write-Output "the build script removed unused usings it reported in $($byBuild.Count) files"
        foreach ($path in $byBuild) { $null = $fixedPaths.Add($path) }
    }

    $parts = [Collections.Generic.List[string]]::new()
    foreach ($rule in $FixedRules.Keys) {
        if ($codeFixes[$rule]) { $parts.Add("$rule $($FixedRules[$rule]) $($codeFixes[$rule])") }
    }
    if ($prunedPaths.Count -gt 0) { $parts.Add("dead code pruned in $($prunedPaths.Count) files") }
    if ($inventoryPaths.Count -gt 0) { $parts.Add('inventories regenerated') }
    if ($parts.Count -eq 0) { $parts.Add("tools/build.ps1 removed unused usings in $(@(Get-ChangedPaths $final $built).Count) files") }
    $subject = "Cleanup: $($parts -join '; ')"
    $body = [Collections.Generic.List[string]]::new()
    $body.Add("The asynchronous cleanup of $Tip (tools/cleanup/README.md).")
    $body.Add('')
    if ($fixedPaths.Count -gt 0) {
        $body.Add("Code fixes in $($fixedPaths.Count) files:")
        $fixedPaths | ForEach-Object { $body.Add("  $_") }
    }
    if ($kept.Count -gt 0) {
        $body.Add('Left in files compiled by several projects that do not all report them: ' +
            (($kept.GetEnumerator() | ForEach-Object { "$($_.Key) $($_.Value)" }) -join ', '))
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
    if ($inventoryPaths.Count -gt 0) { $body.Add("Inventories regenerated: $($inventoryPaths -join ', ')") }
    $body.Add("Built with tools/build.ps1 $solution.")
    $messageFile = Join-Path $Artifacts 'commit-message.txt'
    [IO.File]::WriteAllText($messageFile, "$subject`n`n$($body -join "`n")`n", [Text.UTF8Encoding]::new($false))

    Write-Step 'commit'
    Invoke-Checked 'commit' { git -C $repository commit --quiet --file $messageFile }
    $commit = Invoke-Git @('rev-parse', 'HEAD')
    Write-Output "committed $commit`: $subject"

    Write-Step "push to $Remote $Branch"
    $push = & git -C $repository push --porcelain $Remote "HEAD:refs/heads/$Branch" 2>&1 | ForEach-Object { "$_" }
    $exit = $LASTEXITCODE
    $push | ForEach-Object { Write-Output $_ }
    Write-Output "--- push exit $exit"
    if ($exit -eq 0) {
        Save-Result @{ result = 'pushed'; tip = $Tip; commit = $commit; message = $subject }
        exit 0
    }
    # A stale tip is reported as rejected with "fetch first" or "non-fast-forward"; anything else is a failure.
    $rejection = @($push | Where-Object { $_ -match "^!\s+HEAD:refs/heads/$([regex]::Escape($Branch))\s+\[rejected\] \((fetch first|non-fast-forward)\)" })
    if ($rejection.Count -eq 1) {
        Save-Result @{ result = 'rejected'; tip = $Tip; commit = $commit; message = $rejection[0] }
        exit 0
    }
    throw "The push failed (exit $exit): $($push -join "`n")"
}
catch {
    $failure = "$($_.Exception.ToString())`n$($_.ScriptStackTrace)"
    [Console]::Error.WriteLine($failure)
    if (-not (Test-Path -LiteralPath $Result)) { Save-Result @{ result = 'failed'; tip = $Tip; message = $_.Exception.Message } }
    exit 1
}
