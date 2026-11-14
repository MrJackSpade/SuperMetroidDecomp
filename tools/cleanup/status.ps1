# Prints the asynchronous cleanup's STATUS (tools/cleanup/README.md): idle, running, failed or stopped, the tip it
# concerns, what happened and the log to read; when it failed, also how many runs in a row have failed, since when, and
# the last clean tip. Exits 0 whatever the state; it only reports.
# Usage: pwsh -File tools/cleanup/status.ps1
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'common.ps1')

function Show-CleanupStatus {
    $directory = & git -C $PSScriptRoot config --get cleanup.directory
    if ($LASTEXITCODE -eq 1) {
        Write-Output 'cleanup: not installed in this checkout (cleanup.directory is unset; see tools/cleanup/README.md)'
        return
    }
    if ($LASTEXITCODE -ne 0) { throw "git config --get cleanup.directory failed (exit $LASTEXITCODE)." }
    $paths = Get-CleanupPaths $directory
    if (-not (Test-Path -LiteralPath $paths.Status)) {
        Write-Output "cleanup: installed at $($paths.Root); no run yet"
        return
    }
    $status = Get-Content -LiteralPath $paths.Status -Raw | ConvertFrom-Json
    $tip = if ($status.tip) { " at $($status.tip.Substring(0, 10))" } else { '' }
    Write-Output "cleanup: $($status.state)$tip ($($status.result)) $($status.updated)"
    if ($status.message) { Write-Output "  $($status.message)" }
    if ($status.commit) { Write-Output "  pushed $($status.commit)" }
    if ($status.log) { Write-Output "  log: $($status.log)" }
    if ($status.state -eq 'failed') { Get-FailureStreak $paths }
}

function Get-FailureStreak($Paths) {
    # How long the cleanup has been failing: the runs that failed in a row at the end of service.log, since the last run
    # that pushed or found nothing to clean, and the last tip state.json records as clean. A failure that persists over
    # several tips is a defect in the cleanup's own tools on main, which every new tip runs into again.
    $lines = @([IO.File]::ReadAllLines($Paths.ServiceLog))
    $failures = [Collections.Generic.List[string]]::new()
    for ($index = $lines.Count - 1; $index -ge 0; $index--) {
        $line = $lines[$index]
        if ($line -match '^\S+ (pushed \S+ on |nothing to clean on )') { break }
        if ($line -match '^\S+ FAILED on ') { $failures.Add($line) }
    }
    if ($failures.Count -eq 0) { return }
    $first = $failures[$failures.Count - 1].Split(' ', 2)[0]
    $state = Get-Content -LiteralPath $Paths.State -Raw | ConvertFrom-Json
    $clean = if ($state.lastCleanTip) { $state.lastCleanTip.Substring(0, 10) } else { 'none yet' }
    Write-Output "  $($failures.Count) run(s) in a row have failed, the first at $first; the last clean tip is $clean ($($Paths.ServiceLog))"
}

try {
    Disable-ErrorDialogs
    Show-CleanupStatus
    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 1
}
