# Builds a verification project and only then runs it, so a run never uses a binary older than the source (owner rule,
# 2026-11-11): a failed build runs nothing and exits with the build's code. The run is at Idle priority so it does not
# starve other work.
# The executable is the one MSBuild says the project builds: its output directory under its own target framework, as
# the project and the props it imports declare them (#1690). Only that path is considered, so an output left under
# another framework can never be run; a missing executable is a failure naming the expected path.
# With -ReadyToRun the project is instead published as the game's release is, self-contained for win-x64 in Release, with
# its code compiled ahead of time, into a fresh csharp/test-temp/published/<project> directory, and that executable runs: a
# failed publish runs nothing (#2619, measuring first-run costs as a player's build has them).
# Usage: pwsh -File tools/run-verification.ps1 [-ReadyToRun] <Verification|RenderVerification|DesktopVerification|IntegrationVerification> [arguments...]
param(
    [switch] $ReadyToRun,
    [Parameter(Mandatory)][ValidateSet('Verification', 'RenderVerification', 'DesktopVerification', 'IntegrationVerification')][string] $Project,
    [Parameter(ValueFromRemainingArguments)] [string[]] $Arguments = @())
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot

. (Join-Path $PSScriptRoot 'project-executable.ps1')

try {
    $projectDirectory = Join-Path $repo "csharp/src/SuperMetroid.$Project"
    $projectFile = Join-Path $projectDirectory "SuperMetroid.$Project.csproj"
    if ($ReadyToRun) {
        $published = Join-Path $repo "csharp/test-temp/published/SuperMetroid.$Project"
        if (Test-Path -LiteralPath $published) { Remove-Item -LiteralPath $published -Recurse -Force }
        $log = Join-Path ([IO.Path]::GetTempPath()) "supermetroid-publish-$PID.log"
        $started = Get-Date
        dotnet publish $projectFile -nologo -v:q -c Release -r win-x64 --self-contained true -p:PublishReadyToRun=true -o $published *> $log
        $code = $LASTEXITCODE
        Write-Output "publish exit $code after $([int]((Get-Date) - $started).TotalSeconds) s; full output $log"
        if ($code -ne 0) {
            Select-String -Path $log -Pattern ': error ' | ForEach-Object { $_.Line } | Sort-Object -Unique
            [Console]::Error.WriteLine("The publish of SuperMetroid.$Project failed; nothing was run.")
            exit $code
        }
        $expected = Join-Path $published "SuperMetroid.$Project.exe"
    }
    else {
        & pwsh -File (Join-Path $PSScriptRoot 'build.ps1') $projectFile
        if ($LASTEXITCODE -ne 0) {
            [Console]::Error.WriteLine("The build of SuperMetroid.$Project failed; nothing was run.")
            exit $LASTEXITCODE
        }
        $expected = Resolve-ProjectExecutable $projectFile
    }
    if (-not (Test-Path -LiteralPath $expected -PathType Leaf)) {
        throw "The build of SuperMetroid.$Project produced no executable at $expected."
    }
    $executable = Get-Item -LiteralPath $expected
    Write-Output "running $($executable.FullName) (built $($executable.LastWriteTime))"
    $start = @{ FilePath = $executable.FullName; WorkingDirectory = $repo; NoNewWindow = $true; PassThru = $true }
    if ($Arguments.Count -gt 0) { $start.ArgumentList = $Arguments }
    $process = Start-Process @start
    $process.PriorityClass = 'Idle'
    $process.WaitForExit()
    exit $process.ExitCode
}
catch {
    [Console]::Error.WriteLine(($_ | Out-String))
    exit 1
}
