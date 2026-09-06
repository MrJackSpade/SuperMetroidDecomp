param(
    [Parameter(Mandatory=$true)][string]$NativeTrace,
    [Parameter(Mandatory=$true)][string]$ManagedTrace
)
$ErrorActionPreference = 'Stop'
# The door audit prints four variants. Its final twenty gameplay lines are the
# broken-tube, carried-speed case. Require a complete native trace, not a prefix
# whose matching samples might hide the first divergence.
$nativeFrames = @(Get-Content -LiteralPath $NativeTrace | Where-Object { $_ -like 'ROOM_RELEASE *' })
$managedFrames = @(Get-Content -LiteralPath $ManagedTrace | Where-Object { $_ -like '*phase=Gameplay*' } | Select-Object -Last 20)
if ($nativeFrames.Count -ne 20 -or $managedFrames.Count -ne 20) {
    throw 'Both traces must provide twenty room-release samples.'
}
for ($frame = 0; $frame -lt 20; $frame++) {
    foreach ($field in @('x', 'y', 'pose', 'base', 'mode')) {
        $pattern = '\b' + $field + '=([0-9A-F]+)'
        $expectedMatch = [regex]::Match($nativeFrames[$frame], $pattern)
        $actualMatch = [regex]::Match($managedFrames[$frame], $pattern)
        if (!$expectedMatch.Success -or !$actualMatch.Success) { throw "Missing $field on frame $frame." }
        $expected = $expectedMatch.Groups[1].Value
        $actual = $actualMatch.Groups[1].Value
        if ($expected -ne $actual) { throw "Frame $frame field ${field}: managed $actual, native $expected." }
    }
}
Write-Output 'PASS: twenty room-release frames match native X, Y, pose, base speed, and acceleration mode.'
