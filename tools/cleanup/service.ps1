# The cleanup service (tools/cleanup/README.md): one at a time, it cleans the newest tip of the branch until no request
# is left. trigger.ps1 starts the installed copy, <cleanup directory>\bin\service.ps1, detached at Idle priority.
#
# Each run fetches, resets the clone hard to the remote tip (the clone never holds local work) and runs the tree's own
# tools/cleanup/run.ps1, which fixes, builds, commits and pushes. Requests that arrive during a run are coalesced: the
# next run takes whatever the tip is then. A push rejected because the branch moved discards the commit and runs again
# on the new tip; after MaxRejections consecutive rejections the service stops and notifies. A failed run writes STATUS,
# raises a notification and is not retried on the same tip.
param()
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'common.ps1')
$paths = Get-CleanupPaths (Split-Path $PSScriptRoot -Parent)

# Consecutive rejected pushes before the service stops. Each rejection is a developer push that landed during a run,
# so this many in a row means something keeps the push from landing, not ordinary activity; it bounds a runaway loop
# by count, not by time.
$MaxRejections = 10

function Invoke-Git([string[]]$Arguments) {
    # Runs git in the clone and returns its output, failing with the output on a nonzero exit.
    $output = & git -C $paths.Repository @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) { throw "git $($Arguments -join ' ') failed (exit $LASTEXITCODE): $($output -join "`n")" }
    return (($output | ForEach-Object { "$_" }) -join "`n").Trim()
}

function Invoke-Fetch($Config) {
    # Fetches the cleaned branch into the clone, retrying a failed fetch (Invoke-NetworkGit); fails after the last attempt.
    $fetched = Invoke-NetworkGit $paths.Repository @('fetch', '--quiet', '--no-tags', $Config.remote,
        "+refs/heads/$($Config.branch):refs/remotes/$($Config.remote)/$($Config.branch)") $null
    if ($fetched.Exit -ne 0) {
        throw "git fetch of $($Config.remote)/$($Config.branch) failed on all $($fetched.Attempt) attempts (exit $($fetched.Exit)): $($fetched.Output -join "`n")"
    }
    if ($fetched.Attempt -gt 1) { Write-LogLine $paths.ServiceLog "fetch succeeded on attempt $($fetched.Attempt)" }
}

function Read-ServiceState {
    # What the service remembers between runs: requests taken, and the last tips found clean and found failing.
    if (-not (Test-Path -LiteralPath $paths.State)) {
        return [ordered]@{ processedRequests = [long]0; lastCleanTip = ''; lastFailedTip = '' }
    }
    $saved = Get-Content -LiteralPath $paths.State -Raw | ConvertFrom-Json
    return [ordered]@{ processedRequests = [long]$saved.processedRequests; lastCleanTip = [string]$saved.lastCleanTip; lastFailedTip = [string]$saved.lastFailedTip }
}

function Save-ServiceState($State) {
    Write-AtomicText $paths.State ($State | ConvertTo-Json)
}

function Assert-InstalledScriptsCurrent([string]$Tip) {
    # The installed service and trigger must be the ones the tree describes; otherwise a change to them would not run.
    $stale = @($InstalledScripts | Where-Object {
        (Get-ScriptHash (Join-Path $paths.Bin $_)) -ne (Get-ScriptHash (Join-Path $paths.Repository "tools/cleanup/$_"))
    })
    if ($stale.Count -gt 0) {
        throw "The installed $($stale -join ', ') differ from tools/cleanup at $Tip; run pwsh -File tools/cleanup/install.ps1 to update the installation."
    }
}

function Invoke-CleanupRun($Config, $State) {
    # One run on the current remote tip. Returns already-clean, already-failed, nothing, pushed, rejected or failed.
    Invoke-Fetch $Config
    $tip = Invoke-Git @('rev-parse', "$($Config.remote)/$($Config.branch)")
    if ($tip -eq $State.lastCleanTip) {
        Write-LogLine $paths.ServiceLog "tip $tip is already clean"
        Write-CleanupStatus $paths 'idle' 'already-clean' 'The tip is already clean.' $tip '' ''
        return 'already-clean'
    }
    if ($tip -eq $State.lastFailedTip) {
        Write-LogLine $paths.ServiceLog "tip $tip already failed; STATUS keeps that failure"
        return 'already-failed'
    }
    $stamp = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss')
    $name = "$stamp-$($tip.Substring(0, 10))"
    $log = Join-Path $paths.Logs "$name.log"
    $artifacts = Join-Path $paths.Logs $name
    $null = New-Item -ItemType Directory -Force -Path $artifacts
    $result = Join-Path $artifacts 'result.json'
    Write-CleanupStatus $paths 'running' '' "Cleaning $tip." $tip $log ''
    Write-LogLine $paths.ServiceLog "run on $tip; log $log"
    $null = Invoke-Git @('reset', '--quiet', '--hard', $tip)
    $null = Invoke-Git @('clean', '--quiet', '-fd')
    Assert-InstalledScriptsCurrent $tip
    $run = Join-Path $paths.Repository 'tools/cleanup/run.ps1'
    if (-not (Test-Path -LiteralPath $run)) { throw "$tip has no tools/cleanup/run.ps1." }
    $pwsh = (Get-Process -Id $PID).Path
    & $pwsh -NoProfile -NonInteractive -File $run -Remote $Config.remote -Branch $Config.branch -Tip $tip -Artifacts $artifacts -Result $result *> $log
    $exit = $LASTEXITCODE
    $outcome = if (Test-Path -LiteralPath $result) { Get-Content -LiteralPath $result -Raw | ConvertFrom-Json } else { $null }
    if ($exit -ne 0 -or $null -eq $outcome) {
        $message = if ($null -ne $outcome -and $outcome.message) { $outcome.message } else { "run.ps1 exited $exit without a result." }
        Write-LogLine $log "service: run failed (exit $exit): $message"
        Write-LogLine $paths.ServiceLog "FAILED on $tip (exit $exit): $message"
        $State.lastFailedTip = $tip
        Save-ServiceState $State
        Write-CleanupStatus $paths 'failed' 'failed' $message $tip $log ''
        Send-CleanupNotification $paths 'SuperMetroidDecomp cleanup failed' "At $($tip.Substring(0, 10)): $message Log: $log"
        return 'failed'
    }
    Write-LogLine $log "service: run result $($outcome.result)"
    switch ($outcome.result) {
        'nothing' {
            $State.lastCleanTip = $tip
            $State.lastFailedTip = ''
            Save-ServiceState $State
            Write-CleanupStatus $paths 'idle' 'nothing' 'The tip needed no cleanup.' $tip $log ''
            Write-LogLine $paths.ServiceLog "nothing to clean on $tip"
            return 'nothing'
        }
        'pushed' {
            $State.lastCleanTip = $outcome.commit
            $State.lastFailedTip = ''
            Save-ServiceState $State
            Write-CleanupStatus $paths 'idle' 'pushed' $outcome.message $tip $log $outcome.commit
            Write-LogLine $paths.ServiceLog "pushed $($outcome.commit) on $tip`: $($outcome.message)"
            return 'pushed'
        }
        'rejected' {
            # Only a developer push that landed during the run may restart it; any other rejection is a failure.
            Invoke-Fetch $Config
            $moved = Invoke-Git @('rev-parse', "$($Config.remote)/$($Config.branch)")
            if ($moved -eq $tip) { throw "The push of $($outcome.commit) was rejected, but $($Config.branch) is still at ${tip}: $($outcome.message)" }
            Write-LogLine $paths.ServiceLog "push rejected: $($Config.branch) moved from $tip to $moved during the run; discarding $($outcome.commit)"
            return 'rejected'
        }
        default { throw "run.ps1 reported an unknown result '$($outcome.result)'." }
    }
}

function Invoke-Requests($Config) {
    # Runs while requests are pending, restarting after a rejected push. Returns when none is left or after stopping.
    $state = Read-ServiceState
    $rejections = 0
    $restart = $false
    while ($true) {
        $length = Get-RequestLength $paths
        if ($length -le $state.processedRequests -and -not $restart) { return }
        $state.processedRequests = $length
        Save-ServiceState $state
        $restart = $false
        $outcome = Invoke-CleanupRun $Config $state
        if ($outcome -ne 'rejected') {
            $rejections = 0
            continue
        }
        $rejections++
        if ($rejections -ge $MaxRejections) {
            $message = "Stopped after $rejections consecutive rejected pushes; the next trigger starts again."
            Write-LogLine $paths.ServiceLog $message
            Write-CleanupStatus $paths 'stopped' 'rejections' $message '' '' ''
            Send-CleanupNotification $paths 'SuperMetroidDecomp cleanup stopped' $message
            return
        }
        $restart = $true
    }
}

try {
    Disable-ErrorDialogs
    $config = Read-CleanupConfig $paths
    # The service and every tool it runs: the worker's own temporary folder, no build servers or reused build nodes
    # shared with the developer's builds, no credential prompt, and hooks that know they run in the worker.
    $env:TEMP = $paths.Temp
    $env:TMP = $paths.Temp
    $env:MSBUILDDISABLENODEREUSE = '1'
    $env:UseSharedCompilation = 'false'
    $env:DOTNET_CLI_USE_MSBUILD_SERVER = '0'
    $env:DOTNET_NOLOGO = '1'
    $env:GIT_TERMINAL_PROMPT = '0'
    $env:GCM_INTERACTIVE = 'never'
    $env:SUPERMETROID_CLEANUP_WORKER = '1'
    while ($true) {
        $lock = Open-ServiceLock $paths
        if ($null -eq $lock) {
            Write-LogLine $paths.ServiceLog "process $PID found another service running; it takes the requests"
            exit 0
        }
        Write-LogLine $paths.ServiceLog "process $PID started"
        try { Invoke-Requests $config }
        finally { $lock.Dispose() }
        # A trigger that found the lock held recorded its request first; one recorded before this release is seen here.
        if ((Get-RequestLength $paths) -le (Read-ServiceState).processedRequests) { break }
    }
    Write-LogLine $paths.ServiceLog "process $PID finished"
    exit 0
}
catch {
    $failure = "$($_.Exception.ToString())`n$($_.ScriptStackTrace)"
    [Console]::Error.WriteLine($failure)
    Write-LogLine $paths.ServiceLog "FAILED: $failure"
    Write-CleanupStatus $paths 'failed' 'service-failed' $_.Exception.Message '' $paths.ServiceLog ''
    Send-CleanupNotification $paths 'SuperMetroidDecomp cleanup failed' "$($_.Exception.Message) See $($paths.ServiceLog)."
    exit 1
}
