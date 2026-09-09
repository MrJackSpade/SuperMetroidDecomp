param(
    [string]$Dotnet = 'dotnet',
    [string]$TraceDirectory = 'csharp/test-temp',
    [string]$Rom = 'Super Metroid.smc'
)
$ErrorActionPreference = 'Stop'
# Run from the repository root after building DebugRunner. Only the corrected
# pre-alpha native captures are accepted; historical post-alpha files are not.
$cases = @(
    @('bomb-phase-485-0', 'bomb-chain', 'AEC15B8407BDB0110049621795D7370AC3B7C7C87E6DCCA1D86022513AF4AF16'),
    @('bomb-phase-485-1', 'repeated-bomb-chain', 'B451B76778B35495D5C036B29EF61C4EB8606A9437BBF99E1B71C6E1F9C088DF'),
    @('bomb-phase-485-2', 'triple-bomb-chain', 'C4B328F071977D612FBB4EE73A153AE4F7B6BFC6DC94C34470E9DB9649ACCA15'),
    @('bomb-phase-485-3', 'horizontal-bomb-chain', 'CE95071FE1DFE8215A711D3B69D50AA945439448EF448F9D79F613184B7C50B4'),
    @('bomb-phase-485-4', 'ladder-bomb-chain', '85E08286FE190E92883973200B0789CAE9EB64EB1C4974872166E3F194B2BD1B'),
    @('bomb-phase-485-5', 'ceiling-steering-bomb-chain', 'BE7658EEE1EA4A4B98CA8B0C68AA649A7B694714355E39E388EF0937BCEF7825'),
    @('hurt-phase-485', 'hurt-bomb', 'AF2ACBCBB039BECCE90A803377027679CE22B70CC4D152892355489589C97ADD'),
    @('live-hurt-phase-485', 'live-hurt-bomb', 'DC6BD7206C301F6BE433506A0B8A2346FF002F96918B54C8D501D340025CCDD4')
)
foreach ($case in $cases) {
    $capture = Join-Path $TraceDirectory ($case[0] + '.csv')
    if ((Get-FileHash -LiteralPath $capture -Algorithm SHA256).Hash -ne $case[2]) {
        throw "Changed accepted native capture: $capture"
    }
    & $Dotnet csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0/SuperMetroid.DebugRunner.dll `
        "--$($case[1])-comparison-audit" $Rom $capture
    if ($LASTEXITCODE -ne 0) { throw "Bomb phase comparison failed: $capture" }
}
Write-Output 'Bomb phase matrix: 8 accepted captures, 468160 frames, zero mismatches.'
