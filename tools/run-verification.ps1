# Builds a verification project and only then runs it, so a run never uses a binary older than the source (owner rule,
# 2026-11-11): a failed build runs nothing and exits with the build's code. The run is at Idle priority so it does not
# starve other work.
# The executable is the one MSBuild says the project builds: its output directory under its own target framework, as
# the project and the props it imports declare them (#1690). Only that path is considered, so an output left under
# another framework can never be run; a missing executable is a failure naming the expected path.
# Usage: pwsh -File tools/run-verification.ps1 <Verification|RenderVerification|DesktopVerification|IntegrationVerification> [arguments...]
param(
    [Parameter(Mandatory)][ValidateSet('Verification', 'RenderVerification', 'DesktopVerification', 'IntegrationVerification')][string] $Project,
    [Parameter(ValueFromRemainingArguments)] [string[]] $Arguments = @())
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot

# Evaluates the project as the build just did and returns the full path of the application host it builds.
function Resolve-ProjectExecutable([string] $ProjectFile) {
    $evaluation = & dotnet msbuild $ProjectFile -nologo `
        -getProperty:TargetFramework -getProperty:TargetDir -getProperty:AssemblyName `
        -getProperty:UseAppHost -getProperty:_NativeExecutableExtension
    if ($LASTEXITCODE -ne 0) {
        throw "Evaluating $ProjectFile failed (exit $LASTEXITCODE):`n$($evaluation -join "`n")"
    }
    $properties = ($evaluation -join "`n" | ConvertFrom-Json).Properties
    if ([string]::IsNullOrEmpty($properties.TargetFramework)) {
        throw "$ProjectFile declares no single TargetFramework, so it has no one executable to run."
    }
    if ($properties.UseAppHost -ne 'true') {
        throw "$ProjectFile ($($properties.TargetFramework)) builds no application host (UseAppHost '$($properties.UseAppHost)')."
    }
    return Join-Path $properties.TargetDir "$($properties.AssemblyName)$($properties._NativeExecutableExtension)"
}

try {
    $projectDirectory = Join-Path $repo "csharp/src/SuperMetroid.$Project"
    $projectFile = Join-Path $projectDirectory "SuperMetroid.$Project.csproj"
    & pwsh -File (Join-Path $PSScriptRoot 'build.ps1') $projectFile
    if ($LASTEXITCODE -ne 0) {
        [Console]::Error.WriteLine("The build of SuperMetroid.$Project failed; nothing was run.")
        exit $LASTEXITCODE
    }
    $expected = Resolve-ProjectExecutable $projectFile
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
