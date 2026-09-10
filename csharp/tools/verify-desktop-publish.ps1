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
foreach ($assembly in @('SuperMetroid.Core.dll', 'SuperMetroid.Desktop.dll', 'SuperMetroid.Rendering.Direct3D11.dll', 'SuperMetroid.Diagnostics.dll', 'SuperMetroid.AssetExtraction.dll')) {
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
foreach ($output in @($gameOutput, $verificationOutput)) {
    $bundled = @(Get-ChildItem -LiteralPath $output -File -Recurse | Where-Object {
        $_.Extension -in @('.smc', '.sfc', '.spcu', '.wav', '.smstate', '.smrec') -or $_.Name -eq 'audio-manifest.json'
    })
    if ($bundled.Count) { throw "Publish bundled private game/test content: $($bundled[0].FullName)" }
}
Write-Output 'Verified clean publishes contain no ROM, extracted audio, or private state/recording files.'

Push-Location $gameOutput
try {
    # This probe intentionally stays in Game: it checks the real executable's generated startup policy.
    dotnet ./SuperMetroid.Game.dll --dpi-awareness-audit
    if ($LASTEXITCODE -ne 0) { throw 'Published game failed its DPI startup probe.' }
} finally { Pop-Location }

# Private fixtures belong only in the test output, after checking both clean packages.
Copy-Item -LiteralPath $audioSource -Destination (Join-Path $verificationOutput 'audio') -Recurse
Copy-Item -LiteralPath (Join-Path $repository 'Super Metroid.smc') -Destination (Join-Path $verificationOutput 'Super Metroid.smc')
Push-Location $verificationOutput
try {
    dotnet ./SuperMetroid.DesktopVerification.dll --production-assembly-audit (Join-Path $gameOutput 'SuperMetroid.Game.dll') (Join-Path $gameOutput 'SuperMetroid.Desktop.dll')
    if ($LASTEXITCODE -ne 0) { throw 'Published player assemblies contain verification code.' }
    foreach ($audit in @('--unhandled-exception-console-audit', '--viewport-layout-audit', '--keyboard-input-audit', '--github-error-reporter-audit', '--input-batch-audit', '--frame-timing-audit')) {
        dotnet ./SuperMetroid.DesktopVerification.dll $audit
        if ($LASTEXITCODE -ne 0) { throw "Published desktop verification failed $audit" }
    }
    dotnet ./SuperMetroid.DesktopVerification.dll
    if ($LASTEXITCODE -ne 0) { throw 'Published desktop lifecycle checks failed.' }
    dotnet ./SuperMetroid.DesktopVerification.dll --soak-desktop-hidden 5
    if ($LASTEXITCODE -ne 0) { throw 'Published desktop audio/renderer smoke failed.' }
} finally { Pop-Location }
Write-Output "Clean game publish and identical-dependency hidden desktop checks passed: $artifacts"
Write-Output 'Framework-dependent Windows package; runtime installation is still required. Private outputs retained for inspection.'
