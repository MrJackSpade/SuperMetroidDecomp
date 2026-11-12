# Records that the cleaned branch moved and starts the cleanup service if none is running (tools/cleanup/README.md).
# The git hook .githooks/reference-transaction runs the installed copy, <cleanup directory>\bin\trigger.ps1, after a
# push or fetch moves the branch's remote-tracking ref. It returns at once: the service is started detached, at Idle
# priority, outside the caller's process tree, and the trigger never waits for it.
# It writes nothing to standard output; a failure is written to standard error, the trigger log and STATUS, and raises a
# notification.
# Usage: trigger.ps1 [-Reason <text>] [-Commit <sha>]
param(
    [string]$Reason = 'manual',
    [string]$Commit = ''
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'common.ps1')
$paths = Get-CleanupPaths (Split-Path $PSScriptRoot -Parent)

function Start-DetachedService {
    # Starts the service through WMI, so it belongs to no caller's console or job and outlives the git command; the
    # headless console host gives it and every tool it runs a console that is never shown.
    $pwsh = (Get-Process -Id $PID).Path
    $service = Join-Path $paths.Bin 'service.ps1'
    $startup = New-CimInstance -ClassName Win32_ProcessStartup -ClientOnly -Property @{
        ShowWindow = [uint16]0
        PriorityClass = [uint32]64
    }
    $created = Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{
        CommandLine = "conhost.exe --headless `"$pwsh`" -NoProfile -NonInteractive -File `"$service`""
        CurrentDirectory = $paths.Root
        ProcessStartupInformation = $startup
    }
    if ($created.ReturnValue -ne 0) { throw "Starting the cleanup service failed: Win32_Process.Create returned $($created.ReturnValue)." }
    return $created.ProcessId
}

try {
    Disable-ErrorDialogs
    if (-not (Test-Path -LiteralPath $paths.Config)) { throw "$($paths.Config) is missing; run tools/cleanup/install.ps1." }
    # The request is recorded before the lock is tested: a service that is finishing re-reads the request log after
    # releasing its lock, so a request recorded while the lock was held is never lost.
    [IO.File]::AppendAllText($paths.Requests, "$([DateTime]::UtcNow.ToString('o'))`t$Reason`t$Commit`r`n", [Text.UTF8Encoding]::new($false))
    $lock = Open-ServiceLock $paths
    if ($null -eq $lock) {
        Write-LogLine $paths.TriggerLog "$Reason $Commit recorded; the running service will take it"
        exit 0
    }
    $lock.Dispose()
    $processId = Start-DetachedService
    Write-LogLine $paths.TriggerLog "$Reason $Commit started the service (process $processId)"
    exit 0
}
catch {
    $failure = "$($_.Exception.ToString())`n$($_.ScriptStackTrace)"
    [Console]::Error.WriteLine("cleanup trigger failed: $failure")
    Write-LogLine $paths.TriggerLog "FAILED $Reason $Commit`: $failure"
    Write-CleanupStatus $paths 'failed' 'trigger-failed' $_.Exception.Message $Commit $paths.TriggerLog ''
    Send-CleanupNotification $paths 'SuperMetroidDecomp cleanup did not start' "$($_.Exception.Message) See $($paths.TriggerLog)."
    exit 1
}
