param(
    [Parameter(Mandatory)][string] $ManagedTrace,
    [Parameter(Mandatory)][string] $NativeTrace,
    [ValidateSet(60,360)][int] $FrameCount = 60
)

# Compare the deliberately limited observables exported by the CPU consumer.
# A mismatch is an error, not an approved baseline or a reason to omit a field.
$ErrorActionPreference = 'Stop'
$managed = @(Get-Content -LiteralPath $ManagedTrace | ForEach-Object { $_ | ConvertFrom-Json })
$native = @(Import-Csv -LiteralPath $NativeTrace)
if ($managed.Count -ne $FrameCount -or $native.Count -ne $FrameCount) { throw "Expected $FrameCount consecutive frames starting at 60 in both traces." }
$mismatches = 0
for ($i = 0; $i -lt $FrameCount; $i++) {
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
    if ($FrameCount -eq 360) {
        $values.health = $m.Health
        $values.upperFlash = $m.Barriers[0].FlashTimer
        $values.lowerFlash = $m.Barriers[1].FlashTimer
        $values.upperAi = $m.Barriers[0].AiHandlerBits
        $values.lowerAi = $m.Barriers[1].AiHandlerBits
    }
    foreach ($field in $values.Keys) {
        if ($null -eq $values[$field] -or $null -eq $n.$field) { throw "Missing comparison field $field at frame $($m.Frame)." }
        if ([long]$values[$field] -ne [long]$n.$field) {
            Write-Output "Frame $($m.Frame), ${field}: managed=$($values[$field]), cartridge=$($n.$field)"
            $mismatches++
        }
    }
}
if ($mismatches) { throw "$mismatches field mismatches; native parity is NOT established." }
Write-Output "All $FrameCount frames match for the exported movement, camera, ammunition, projectile, and barrier-health fields."
