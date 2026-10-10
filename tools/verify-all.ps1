[CmdletBinding()]
param(
    # Converted replay trace directories (ignored inputs; see docs/smv-gameplay-parity.md).
    [string]$ReplayRoot = 'test-temp/issue-1275'
)
# Builds once, then runs every verifier project's full no-argument suite and every SMV
# replay concurrently from a copied bin, so a rebuild cannot race the run. Each command
# writes its own log; the script waits for all of them and fails if any failed.
# Run from the repository root.
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$run = 'csharp/test-temp/verify-all'
$replays = [ordered]@{
    'replay-ridley'  = @('--ridley-full-movie')
    'replay-mb'      = @('--mother-brain-glitch-movie', "$ReplayRoot/mb")
    'replay-low13'   = @('--lsmv-playthrough-movie', "$ReplayRoot/low13")
    'replay-full100' = @('--full-playthrough-movie', "$ReplayRoot/full100")
    'replay-drunk'   = @('--drunk-playthrough-movie', "$ReplayRoot/drunk")
}
$projects = [ordered]@{
    'SuperMetroid.Verification'            = 'net10.0'
    'SuperMetroid.IntegrationVerification' = 'net10.0'
    'SuperMetroid.RenderVerification'      = 'net10.0-windows'
    'SuperMetroid.DesktopVerification'     = 'net10.0-windows'
    'SuperMetroid.EnsureVerification'      = 'net10.0'
    'SuperMetroid.BuildPolicyVerification' = 'net10.0'
}

dotnet build csharp/SuperMetroid.slnx -c Release --nologo -v quiet -clp:ErrorsOnly
if (Test-Path -LiteralPath $run) { Remove-Item -LiteralPath $run -Recurse -Force }
New-Item -ItemType Directory -Path "$run/bin", "$run/logs" | Out-Null
foreach ($project in $projects.Keys) {
    Copy-Item -Recurse -LiteralPath "csharp/src/$project/bin/Release/$($projects[$project])" -Destination "$run/bin/$project"
}

$commands = [ordered]@{}
foreach ($project in $projects.Keys) { $commands[$project] = @("$run/bin/$project/$project.dll") }
foreach ($replay in $replays.Keys) { $commands[$replay] = @("$run/bin/SuperMetroid.Verification/SuperMetroid.Verification.dll") + $replays[$replay] }

$started = @()
foreach ($name in $commands.Keys) {
    # Start-Process joins arguments with spaces; quote each so paths with spaces survive.
    $arguments = $commands[$name] | ForEach-Object { '"' + $_ + '"' }
    $process = Start-Process dotnet -ArgumentList $arguments -NoNewWindow -PassThru `
        -RedirectStandardOutput "$run/logs/$name.out.log" -RedirectStandardError "$run/logs/$name.err.log"
    # Touching the handle keeps the exit code available after the process ends.
    $null = $process.Handle
    $started += [pscustomobject]@{ Name = $name; Process = $process; Start = Get-Date }
}

$failed = @()
foreach ($entry in $started) {
    $entry.Process.WaitForExit()
    $seconds = ($entry.Process.ExitTime - $entry.Start).TotalSeconds
    $code = $entry.Process.ExitCode
    $status = 'PASS'
    if ($code -ne 0) { $status = 'FAIL'; $failed += $entry.Name }
    '{0} {1,-38} exit {2,3}  {3,8:0.0} s  {4}/logs/{1}.*.log' -f $status, $entry.Name, $code, $seconds, $run
}
if ($failed.Count -gt 0) { throw "$($failed.Count) verification command(s) failed: $($failed -join ', ')" }
'All verification commands passed.'
