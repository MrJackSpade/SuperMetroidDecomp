param()
$ErrorActionPreference = 'Stop'
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$artifacts = Join-Path $repository ('csharp/test-temp/desktop-publish/' + [Guid]::NewGuid().ToString('N'))
$gameOutput = Join-Path $artifacts 'game'
$verificationOutput = Join-Path $artifacts 'verification'

# Shared, new intermediate root proves the published dependency graph does not
# rely on ordinary bin/obj outputs. No player saves or INI are copied.
foreach ($entry in @(
    @{ Project = 'SuperMetroid.Game'; Output = $gameOutput },
    @{ Project = 'SuperMetroid.DesktopVerification'; Output = $verificationOutput }
)) {
    $project = Join-Path $repository ('csharp/src/' + $entry.Project + '/' + $entry.Project + '.csproj')
    dotnet publish $project -c Release --artifacts-path $artifacts --output $entry.Output -p:RestoreLockedMode=true
    if ($LASTEXITCODE -ne 0) { throw "Clean desktop publish failed; outputs retained at $artifacts" }
}

# Verification must exercise exactly the assemblies delivered with the game,
# not a second build that merely uses the same source names.
foreach ($assembly in @('SuperMetroid.Core.dll', 'SuperMetroid.Desktop.dll', 'SuperMetroid.Rendering.Direct3D11.dll')) {
    $gameHash = (Get-FileHash -LiteralPath (Join-Path $gameOutput $assembly) -Algorithm SHA256).Hash
    $testHash = (Get-FileHash -LiteralPath (Join-Path $verificationOutput $assembly) -Algorithm SHA256).Hash
    if ($gameHash -ne $testHash) { throw "Published dependency differs: $assembly" }
}
if (-not (Test-Path -LiteralPath (Join-Path $gameOutput 'D3D11_THIRD_PARTY_NOTICES.txt'))) {
    throw 'Game publish omitted renderer dependency notices.'
}
$audioSource = Join-Path $repository 'standalone-assets/audio'
if (-not (Test-Path -LiteralPath (Join-Path $audioSource 'audio-manifest.json'))) {
    throw 'Extract audio assets before qualifying desktop packaging.'
}
$audioFiles = @(Get-ChildItem -LiteralPath $audioSource -File -Recurse)
foreach ($source in $audioFiles) {
    $relative = $source.FullName.Substring($audioSource.Length).TrimStart('\', '/')
    $expected = (Get-FileHash -LiteralPath $source.FullName -Algorithm SHA256).Hash
    foreach ($output in @($gameOutput, $verificationOutput)) {
        $copy = Join-Path (Join-Path $output 'audio') $relative
        if (-not (Test-Path -LiteralPath $copy)) { throw "Missing packaged audio: $copy" }
        if ((Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash -ne $expected) {
            throw "Packaged audio differs: $copy"
        }
    }
}
Write-Output "Verified $($audioFiles.Count) audio asset hashes in both packages."

Push-Location $gameOutput
try {
    foreach ($audit in @('--unhandled-exception-console-audit', '--viewport-layout-audit', '--keyboard-input-audit', '--dpi-awareness-audit')) {
        dotnet ./SuperMetroid.Game.dll $audit
        if ($LASTEXITCODE -ne 0) { throw "Published game failed $audit" }
    }
} finally { Pop-Location }

# The hidden desktop checks use an isolated ROM and published audio content.
# Running here prevents repository-relative asset discovery from hiding omissions.
Copy-Item -LiteralPath (Join-Path $repository 'Super Metroid.smc') -Destination (Join-Path $verificationOutput 'Super Metroid.smc')
Push-Location $verificationOutput
try {
    dotnet ./SuperMetroid.DesktopVerification.dll
    if ($LASTEXITCODE -ne 0) { throw 'Published desktop lifecycle checks failed.' }
    dotnet ./SuperMetroid.DesktopVerification.dll --soak-desktop-hidden 5
    if ($LASTEXITCODE -ne 0) { throw 'Published desktop audio/renderer smoke failed.' }
} finally { Pop-Location }
Write-Output "Clean game publish and identical-dependency hidden desktop checks passed: $artifacts"
Write-Output 'Framework-dependent Windows package; runtime installation is still required. Private outputs retained for inspection.'
