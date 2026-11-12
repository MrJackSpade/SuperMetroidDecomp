# Confirms the code-fix runner on its fixtures (tools/cleanup/README.md, "Fixtures").
#
# Each tools/cleanup/fixtures/<rule> directory holds input/ (a small project, Fixture.csproj, with the violating code)
# and expected/ (every file the project must hold after the fix). For each fixture, or only the rules named, the
# check:
#   1. copies input/ into a new temporary directory, with the repository's csharp/.editorconfig beside it and a
#      Directory.Build.props that imports csharp/Directory.Build.props and names the repository's analyzer and
#      code-fix assemblies (CleanupAnalyzer, CleanupCodeFixes) for the fixture's own <Analyzer> items;
#   2. restores it and runs tools/cleanup/CodeFixRunner.cs on its Fixture.csproj for that one rule;
#   3. compares every file of the copy (less bin/, obj/ and the two files step 1 added) with expected/, byte for byte,
#      including which files exist;
#   4. builds the fixed copy with tools/build.ps1.
# Before the fixtures it builds SuperMetroid.EnsureAnalyzer.CodeFixes (and so the analyzer) once in Debug. A failed
# fixture keeps its copy and names it; a passing one is deleted. Exits nonzero when any fixture fails.
#
# Usage: pwsh -File tools/cleanup/check-fixtures.ps1 [-Rule IDE0032,SME6391]
param([string[]]$Rule = @())
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'common.ps1')
$repository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$fixtures = Join-Path $PSScriptRoot 'fixtures'

# The files step 1 adds to a copy, which the comparison leaves out.
$GeneratedFiles = @('.editorconfig', 'Directory.Build.props')

function Get-FixtureFiles([string]$Root) {
    # Every file under $Root as a sorted list of forward-slash relative paths, less build output and generated files.
    $prefix = [IO.Path]::GetFullPath($Root).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    $files = foreach ($file in Get-ChildItem -LiteralPath $Root -Recurse -File -Force) {
        $relative = $file.FullName.Substring($prefix.Length).Replace('\', '/')
        $first = $relative.Split('/')[0]
        if ($first -in @('bin', 'obj') -or $GeneratedFiles -contains $relative) { continue }
        $relative
    }
    return @($files | Sort-Object { $_ } -CaseSensitive)
}

function Test-Fixture([string]$Name) {
    # Runs one fixture; returns a list of failures, empty when it passes.
    $failures = [Collections.Generic.List[string]]::new()
    $inputDirectory = Join-Path $fixtures "$Name/input"
    $expected = Join-Path $fixtures "$Name/expected"
    foreach ($required in @($inputDirectory, $expected)) {
        if (-not (Test-Path -LiteralPath $required -PathType Container)) { throw "Fixture $Name has no $required." }
    }
    $copy = Join-Path ([IO.Path]::GetTempPath()) "cleanup-fixture-$Name-$([guid]::NewGuid().ToString('N'))"
    $null = New-Item -ItemType Directory -Path $copy
    Copy-Item -Path (Join-Path $inputDirectory '*') -Destination $copy -Recurse -Force
    Copy-Item -LiteralPath (Join-Path $repository 'csharp/.editorconfig') -Destination (Join-Path $copy '.editorconfig')
    $props = Join-Path $repository 'csharp/Directory.Build.props'
    $analyzer = Join-Path $repository 'csharp/src/SuperMetroid.EnsureAnalyzer/bin/Debug/netstandard2.0/SuperMetroid.EnsureAnalyzer.dll'
    $codeFixes = Join-Path $repository 'csharp/src/SuperMetroid.EnsureAnalyzer.CodeFixes/bin/Debug/netstandard2.0/SuperMetroid.EnsureAnalyzer.CodeFixes.dll'
    [IO.File]::WriteAllText((Join-Path $copy 'Directory.Build.props'), @"
<Project>
  <Import Project="$props" />
  <PropertyGroup>
    <CleanupAnalyzer>$analyzer</CleanupAnalyzer>
    <CleanupCodeFixes>$codeFixes</CleanupCodeFixes>
  </PropertyGroup>
</Project>
"@, [Text.UTF8Encoding]::new($false))
    $project = Join-Path $copy 'Fixture.csproj'
    if (-not (Test-Path -LiteralPath $project)) { throw "Fixture $Name has no input/Fixture.csproj." }

    & dotnet restore $project -v:q | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Restoring fixture $Name failed (exit $LASTEXITCODE)." }
    $summary = "$copy.summary.json"
    & dotnet run (Join-Path $PSScriptRoot 'CodeFixRunner.cs') -- $project $summary $Name | Out-Host
    if ($LASTEXITCODE -ne 0) {
        $failures.Add("the runner failed (exit $LASTEXITCODE)")
        return @{ Failures = $failures; Copy = $copy }
    }
    Write-Host (Get-Content -LiteralPath $summary -Raw)

    $actualFiles = Get-FixtureFiles $copy
    $expectedFiles = Get-FixtureFiles $expected
    foreach ($missing in $expectedFiles | Where-Object { $actualFiles -cnotcontains $_ }) { $failures.Add("missing $missing") }
    foreach ($extra in $actualFiles | Where-Object { $expectedFiles -cnotcontains $_ }) { $failures.Add("unexpected $extra") }
    foreach ($file in $expectedFiles | Where-Object { $actualFiles -ccontains $_ }) {
        $actualBytes = [IO.File]::ReadAllBytes((Join-Path $copy $file))
        $expectedBytes = [IO.File]::ReadAllBytes((Join-Path $expected $file))
        if ([Convert]::ToHexString($actualBytes) -cne [Convert]::ToHexString($expectedBytes)) {
            $failures.Add("$file differs; actual:`n$([Text.Encoding]::UTF8.GetString($actualBytes))")
        }
    }
    if ($failures.Count -gt 0) { return @{ Failures = $failures; Copy = $copy } }

    & pwsh -NoProfile -NonInteractive -File (Join-Path $repository 'tools/build.ps1') $project | Out-Host
    if ($LASTEXITCODE -ne 0) { $failures.Add("the fixed copy does not build (exit $LASTEXITCODE)") }
    return @{ Failures = $failures; Copy = $copy }
}

try {
    Disable-ErrorDialogs
    Set-Location $repository
    $names = @(Get-ChildItem -LiteralPath $fixtures -Directory | ForEach-Object Name | Sort-Object)
    # pwsh -File passes '-Rule A,B' as one string, so the names are split on commas here.
    $Rule = @($Rule | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    if ($Rule.Count -gt 0) {
        foreach ($name in $Rule) { if ($names -notcontains $name) { throw "There is no fixture for $name in $fixtures." } }
        $names = @($Rule)
    }
    Write-Host "building the repository analyzer and code fixes (Debug)"
    & pwsh -NoProfile -NonInteractive -File (Join-Path $repository 'tools/build.ps1') 'csharp/src/SuperMetroid.EnsureAnalyzer.CodeFixes' | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "SuperMetroid.EnsureAnalyzer.CodeFixes does not build (exit $LASTEXITCODE)." }

    $failed = [Collections.Generic.List[string]]::new()
    foreach ($name in $names) {
        Write-Host ''
        Write-Host "=== fixture $name"
        $outcome = Test-Fixture $name
        if ($outcome.Failures.Count -eq 0) {
            Remove-Item -LiteralPath $outcome.Copy -Recurse -Force
            Remove-Item -LiteralPath "$($outcome.Copy).summary.json"
            Write-Host "--- $name passed"
            continue
        }
        $failed.Add($name)
        Write-Host "--- $name FAILED; the copy is kept at $($outcome.Copy)"
        $outcome.Failures | ForEach-Object { Write-Host "  $_" }
    }
    Write-Host ''
    if ($failed.Count -gt 0) {
        Write-Host "$($failed.Count) of $($names.Count) fixtures failed: $($failed -join ', ')"
        exit 1
    }
    Write-Host "all $($names.Count) fixtures passed"
    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 1
}
