# Prints the asynchronous cleanup's STATUS (tools/cleanup/README.md): idle, running, failed or stopped, the tip it
# concerns, what happened and the log to read. Exits 0 whatever the state; it only reports.
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
