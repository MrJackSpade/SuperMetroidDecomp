param(
    [Parameter(Mandatory)][string] $ManagedTrace,
    [Parameter(Mandatory)][string] $NativeTrace
)

# Compare the deliberately limited observables exported by the CPU consumer.
# A mismatch is an error, not an approved baseline or a reason to omit a field.
$ErrorActionPreference = 'Stop'
$managed = @(Get-Content -LiteralPath $ManagedTrace | ForEach-Object { $_ | ConvertFrom-Json })
$native = @(Import-Csv -LiteralPath $NativeTrace)
if ($managed.Count -ne 60 -or $native.Count -ne 60) { throw 'Expected frames 60 through 119 in both traces.' }
$mismatches = 0
for ($i = 0; $i -lt 60; $i++) {
    $m = $managed[$i]
    $n = $native[$i]
    if ($m.Frame -ne ($i + 60) -or [int]$n.frame -ne ($i + 60)) { throw "Unexpected frame order at row $i." }
    $values = [ordered]@{
        input = $m.Input; x = $m.XFixed; y = $m.YFixed
        pose = $m.Pose; anim = $m.AnimationFrame; timer = $m.AnimationFrameTimer
        cameraX = $m.CameraX; cameraY = $m.CameraY; missiles = $m.Missiles
        shotType = $m.Shots[0].Type; shotX = $m.Shots[0].XPosition; shotY = $m.Shots[0].YPosition
        upper = $m.Barriers[0].Health; lower = $m.Barriers[1].Health
    }
    foreach ($field in $values.Keys) {
        if ([long]$values[$field] -ne [long]$n.$field) {
            Write-Output "Frame $($m.Frame), ${field}: managed=$($values[$field]), cartridge=$($n.$field)"
            $mismatches++
        }
    }
}
if ($mismatches) { throw "$mismatches field mismatches; native parity is NOT established." }
Write-Output 'All 60 frames match for the exported movement, camera, ammunition, projectile, and barrier-health fields.'
