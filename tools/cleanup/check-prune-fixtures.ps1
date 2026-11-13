# Confirms the reachability prune on its fixtures (tools/cleanup/README.md, "Fixtures").
#
# Each tools/cleanup/prune-fixtures/<name> directory holds input/ (a small repository whose csharp/Fixture.slnx lists
# projects named like the repository's shipped, host and verification projects) and expected/ (every file that
# repository must hold after the prune). For each fixture, or only the names given, the check:
#   1. copies input/ into a new temporary directory and writes csharp/Directory.Build.props there, importing the
#      repository's csharp/Directory.Build.props and naming the repository's csharp/test-support as
#      RepositoryTestSupport, so the fixture's host can take the console error policy;
#   2. restores csharp/Fixture.slnx and runs the reachability prune on it exactly as tools/cleanup/run.ps1 does, with
#      csharp/test-support/VerificationAccess.Core as the destination;
#   3. compares every file of the copy (less bin/, obj/ and the file step 1 added) with expected/, byte for byte,
#      including which files exist;
#   4. builds the pruned copy's csharp/Fixture.slnx with tools/build.ps1.
# Before the fixtures it builds SuperMetroid.ReachabilityAudit once in Release, as the cleanup does. A failed fixture
# keeps its copy and names it; a passing one is deleted. Exits nonzero when any fixture fails.
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

# The file step 1 adds to a copy, which the comparison leaves out.
$GeneratedFiles = @('csharp/Directory.Build.props')

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
</Project>
"@, [Text.UTF8Encoding]::new($false))

    & dotnet restore $solution -v:q | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Restoring fixture $Fixture failed (exit $LASTEXITCODE)." }
    & dotnet run --project (Join-Path $repository 'csharp/src/SuperMetroid.ReachabilityAudit') -c Release --no-build -- `
        --solution $solution --prune-keeping-verification-used $PruneCategories --path-prefix csharp/src `
        --to (Join-Path $copy 'csharp/test-support/VerificationAccess.Core') --retired-fields "$copy.retired-fields.txt" | Out-Host
    if ($LASTEXITCODE -ne 0) {
        $failures.Add("the prune failed (exit $LASTEXITCODE)")
        return @{ Failures = $failures; Copy = $copy }
    }

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

    # The pruned copy must build whatever the comparison found: a collision or an unresolved cref shows here too.
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
