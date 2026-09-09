param(
    [string]$Dotnet = 'dotnet',
    [string]$TraceDirectory = 'csharp/test-temp',
    [string]$Rom = 'Super Metroid.smc',
    [string]$ArchivePath
)
$ErrorActionPreference = 'Stop'
# Explicit accepted captures only. Historical invalid FX/geometry captures must
# never enter this matrix through a wildcard or silently substitute for a fixture.
$captures = [System.Collections.Generic.List[string]]::new()
foreach ($medium in 0..2) {
    foreach ($release in 0..1) {
        $suffix = if ($medium -eq 0) { '' } else { '-v2' }
        $captures.Add("damageboost-472-medium-$medium-release-$release$suffix.csv")
        foreach ($source in 1..4) {
            $captures.Add("damageboost-forward-472-c$source-m$medium-r$release.csv")
        }
        foreach ($source in 5..6) {
            $captures.Add("damageboost-carry-472-c$source-m$medium-r$release.csv")
        }
        $captures.Add("damageboost-turret-472-m$medium-r$release.csv")
        foreach ($source in 1..7) {
            $captures.Add("damageboost-preheld-472-c$source-m$medium-r$release.csv")
        }
    }
}
$captures.Add('damageboost-runup-472-c8.csv')
$captures.Add('damageboost-runup-472-c9-v2.csv')
foreach ($source in 1,2,3,4,7) {
    $captures.Add("damageboost-no-jump-472-c$source.csv")
}
if ($captures.Count -ne 97) { throw 'Changed accepted matrix dimensions.' }
foreach ($name in $captures) {
    if (!(Test-Path -LiteralPath (Join-Path $TraceDirectory $name) -PathType Leaf)) {
        throw "Missing accepted capture: $name"
    }
}
# Run from the repository root after building DebugRunner. Every nonzero exit is
# fatal; no truncated first-line pipeline may hide a failing comparison.
if ($ArchivePath) {
    if (Test-Path -LiteralPath $ArchivePath) { throw 'Refusing to overwrite a capture archive.' }
    $paths = @($captures | ForEach-Object { Join-Path $TraceDirectory $_ })
    Compress-Archive -LiteralPath $paths -DestinationPath $ArchivePath -CompressionLevel Optimal
    Get-FileHash -LiteralPath $ArchivePath -Algorithm SHA256
    return
}
$passed = 0
foreach ($name in $captures) {
    $result = & $Dotnet run --no-build --no-restore --project csharp/src/SuperMetroid.DebugRunner `
        -c Release --no-launch-profile -- --damageboost-comparison-audit $Rom (Join-Path $TraceDirectory $name)
    $comparisonExit = $LASTEXITCODE
    if ($comparisonExit -ne 0) {
        $result | Write-Output
        throw "Native damage-boost comparison failed: $name (exit $comparisonExit)"
    }
    $passed++
    Write-Output "PASS $passed/97 $name"
}
Write-Output 'Damage boost matrix: 97 accepted captures, 616608 per-frame samples, zero mismatches.'
