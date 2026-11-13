# Confirms the reachability prune on its fixtures (tools/cleanup/README.md, "Fixtures").
#
# Each tools/cleanup/prune-fixtures/<name> directory holds input/ (a small repository whose csharp/Fixture.slnx lists
# projects named like the repository's shipped, host and verification projects), expected/ (every file that repository
# must hold after the prune) and, optionally, expected-output.txt (text the prune's output must hold, such as the MANUAL
# line of a move it keeps in place). For each fixture, or only the names given, the check:
#   1. copies input/ into a new temporary directory and writes csharp/Directory.Build.props there, importing the
#      repository's csharp/Directory.Build.props, naming the repository's csharp/test-support as RepositoryTestSupport,
#      so the fixture's host can take the console error policy, and adding the repository's analyzer
#      (SuperMetroid.EnsureAnalyzer) to every project; it copies the repository's csharp/.editorconfig beside it, so each
#      rule has the severity and scope the repository gives it (the SME rules in src/SuperMetroid.Core and
#      src/SuperMetroid.AssetExtraction only). The module map is not added: its rules (SME6402-SME6405) place the
#      repository's own modules, which a fixture does not have, and a prune changes no namespace or file path they read;
#   2. restores csharp/Fixture.slnx and runs the reachability prune on it exactly as tools/cleanup/run.ps1 does, with
#      csharp/test-support/VerificationAccess.Core as the destination;
#   3. requires the prune's output to hold expected-output.txt when there is one, compares every file of the copy (less
#      bin/, obj/ and the files step 1 added) with expected/, byte for byte, including which files exist;
#   4. builds the pruned copy's csharp/Fixture.slnx with tools/build.ps1, so an analyzer finding fails the fixture as it
#      fails the cleanup's build.
# Before the fixtures it builds SuperMetroid.ReachabilityAudit once in Release, as the cleanup does, which builds the
# analyzer in Release too. A failed fixture keeps its copy and names it; a passing one is deleted. Exits nonzero when any
# fixture fails.
#
# Usage: pwsh -File tools/cleanup/check-prune-fixtures.ps1 [-Name holder-collision]
param([string[]]$Name = @())
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'common.ps1')
$repository = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$fixtures = Join-Path $PSScriptRoot 'prune-fixtures'

# The categories tools/cleanup/run.ps1 gives the prune ($PruneCategories there).
$PruneCategories = 'unreachable-member,unreachable-type,unreferenced-field,unreferenced-enum-member,unreferenced-constant'

# The files step 1 adds to a copy, which the comparison leaves out.
$GeneratedFiles = @('csharp/Directory.Build.props', 'csharp/.editorconfig')

# The repository analyzer the Release build of the reachability audit builds (csharp/Directory.Build.targets references it).
$Analyzer = Join-Path $repository 'csharp/src/SuperMetroid.EnsureAnalyzer/bin/Release/netstandard2.0/SuperMetroid.EnsureAnalyzer.dll'

function Test-SameFiles([string]$Actual, [string]$Expected, [Collections.Generic.List[string]]$Failures) {
    # Adds a failure for every file missing from, added to or different in $Actual compared with $Expected.
    $actualFiles = Get-FixtureFiles $Actual
    $expectedFiles = Get-FixtureFiles $Expected
    foreach ($missing in $expectedFiles | Where-Object { $actualFiles -cnotcontains $_ }) { $Failures.Add("missing $missing") }
    foreach ($extra in $actualFiles | Where-Object { $expectedFiles -cnotcontains $_ }) { $Failures.Add("unexpected $extra") }
    foreach ($file in $expectedFiles | Where-Object { $actualFiles -ccontains $_ }) {
        $actualBytes = [IO.File]::ReadAllBytes((Join-Path $Actual $file))
        $expectedBytes = [IO.File]::ReadAllBytes((Join-Path $Expected $file))
        if ([Convert]::ToHexString($actualBytes) -cne [Convert]::ToHexString($expectedBytes)) {
            $Failures.Add("$file differs; actual:`n$([Text.Encoding]::UTF8.GetString($actualBytes))")
        }
    }
}

function Get-FixtureFiles([string]$Root) {
    # Every file under $Root as a sorted list of forward-slash relative paths, less build output and generated files.
    $prefix = [IO.Path]::GetFullPath($Root).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    $files = foreach ($file in Get-ChildItem -LiteralPath $Root -Recurse -File -Force) {
        $relative = $file.FullName.Substring($prefix.Length).Replace('\', '/')
        $segments = $relative.Split('/')
        if (($segments | Where-Object { $_ -in @('bin', 'obj') }) -or $GeneratedFiles -contains $relative) { continue }
        $relative
    }
    return @($files | Sort-Object { $_ } -CaseSensitive)
}

function Test-Fixture([string]$Fixture) {
    # Runs one fixture; returns a list of failures, empty when it passes.
    $failures = [Collections.Generic.List[string]]::new()
    $inputDirectory = Join-Path $fixtures "$Fixture/input"
    $expected = Join-Path $fixtures "$Fixture/expected"
    foreach ($required in @($inputDirectory, $expected)) {
        if (-not (Test-Path -LiteralPath $required -PathType Container)) { throw "Fixture $Fixture has no $required." }
    }
    $copy = Join-Path ([IO.Path]::GetTempPath()) "prune-fixture-$Fixture-$([guid]::NewGuid().ToString('N'))"
    $null = New-Item -ItemType Directory -Path $copy
    Copy-Item -Path (Join-Path $inputDirectory '*') -Destination $copy -Recurse -Force
    $solution = Join-Path $copy 'csharp/Fixture.slnx'
    if (-not (Test-Path -LiteralPath $solution)) { throw "Fixture $Fixture has no input/csharp/Fixture.slnx." }
    $props = Join-Path $repository 'csharp/Directory.Build.props'
    $testSupport = Join-Path $repository 'csharp/test-support'
    [IO.File]::WriteAllText((Join-Path $copy 'csharp/Directory.Build.props'), @"
<Project>
  <Import Project="$props" />
  <PropertyGroup>
    <RepositoryTestSupport>$testSupport</RepositoryTestSupport>
  </PropertyGroup>
  <ItemGroup>
    <Analyzer Include="$Analyzer" />
  </ItemGroup>
</Project>
"@, [Text.UTF8Encoding]::new($false))
    Copy-Item -LiteralPath (Join-Path $repository 'csharp/.editorconfig') -Destination (Join-Path $copy 'csharp/.editorconfig')

    & dotnet restore $solution -v:q | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Restoring fixture $Fixture failed (exit $LASTEXITCODE)." }
    $output = @(& dotnet run --project (Join-Path $repository 'csharp/src/SuperMetroid.ReachabilityAudit') -c Release --no-build -- `
        --solution $solution --prune-keeping-verification-used $PruneCategories --path-prefix csharp/src `
        --to (Join-Path $copy 'csharp/test-support/VerificationAccess.Core') --retired-fields "$copy.retired-fields.txt" 2>&1 |
        ForEach-Object { "$_" })
    $pruneExit = $LASTEXITCODE
    $output | Out-Host
    if ($pruneExit -ne 0) {
        $failures.Add("the prune failed (exit $pruneExit)")
        return @{ Failures = $failures; Copy = $copy }
    }
    $expectedOutput = Join-Path $fixtures "$Fixture/expected-output.txt"
    if (Test-Path -LiteralPath $expectedOutput -PathType Leaf) {
        # A line the prune must print, such as the MANUAL line of a move it keeps in place.
        $text = [IO.File]::ReadAllText($expectedOutput).Trim()
        if (-not ($output -join "`n").Contains($text, [StringComparison]::Ordinal)) { $failures.Add("the prune's output does not say: $text") }
    }
    Test-SameFiles $copy $expected $failures

    # The pruned copy must build whatever the comparison found, analyzers included: a collision, an unresolved cref or a
    # rule a promotion breaks shows here too.
    & pwsh -NoProfile -NonInteractive -File (Join-Path $repository 'tools/build.ps1') $solution | Out-Host
    if ($LASTEXITCODE -ne 0) { $failures.Add("the pruned copy does not build (exit $LASTEXITCODE)") }
    return @{ Failures = $failures; Copy = $copy }
}

try {
    Disable-ErrorDialogs
    Set-Location $repository
    $names = @(Get-ChildItem -LiteralPath $fixtures -Directory | ForEach-Object Name | Sort-Object)
    # pwsh -File passes '-Name A,B' as one string, so the names are split on commas here.
    $Name = @($Name | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    if ($Name.Count -gt 0) {
        foreach ($fixture in $Name) { if ($names -notcontains $fixture) { throw "There is no prune fixture $fixture in $fixtures." } }
        $names = @($Name)
    }
    Write-Host "building the reachability audit (Release)"
    & pwsh -NoProfile -NonInteractive -File (Join-Path $repository 'tools/build.ps1') 'csharp/src/SuperMetroid.ReachabilityAudit' -c Release | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "SuperMetroid.ReachabilityAudit does not build (exit $LASTEXITCODE)." }
    if (-not (Test-Path -LiteralPath $Analyzer -PathType Leaf)) { throw "The Release build left no analyzer at $Analyzer." }

    $failed = [Collections.Generic.List[string]]::new()
    foreach ($fixture in $names) {
        Write-Host ''
        Write-Host "=== prune fixture $fixture"
        $outcome = Test-Fixture $fixture
        if ($outcome.Failures.Count -eq 0) {
            Remove-Item -LiteralPath $outcome.Copy -Recurse -Force
            Remove-Item -LiteralPath "$($outcome.Copy).retired-fields.txt" -Force
            Write-Host "--- $fixture passed"
            continue
        }
        $failed.Add($fixture)
        Write-Host "--- $fixture FAILED; the copy is kept at $($outcome.Copy)"
        $outcome.Failures | ForEach-Object { Write-Host "  $_" }
    }
    Write-Host ''
    if ($failed.Count -gt 0) {
        Write-Host "$($failed.Count) of $($names.Count) prune fixtures failed: $($failed -join ', ')"
        exit 1
    }
    Write-Host "all $($names.Count) prune fixtures passed"
    exit 0
}
catch {
    [Console]::Error.WriteLine("$($_.Exception.ToString())`n$($_.ScriptStackTrace)")
    exit 1
}
