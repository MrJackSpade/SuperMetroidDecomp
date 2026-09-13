param([Parameter(Mandatory)][string]$ManagedTrace, [Parameter(Mandatory)][string]$NativeTrace)
$ErrorActionPreference = 'Stop'
$managed = @(Get-Content -LiteralPath $ManagedTrace | ForEach-Object { $_ | ConvertFrom-Json })
$native = @(Import-Csv -LiteralPath $NativeTrace)
if ($managed.Count -ne 500 -or $native.Count -ne 500) { throw 'Expected 500 frames.' }
$failures=0
$fieldFailures=[ordered]@{}
for($i=0;$i -lt 500;$i++) {
    $m=$managed[$i]; $n=$native[$i]; $e=$m.Enemy; $c=$m.Crawler
    if($m.Frame -ne $i -or [int]$n.frame -ne $i) { throw 'Unexpected frame order.' }
    $values=[ordered]@{
        input=$m.Input; x=$m.XFixed; y=$m.YFixed; pose=$m.Pose; anim=$m.AnimationFrame; timer=$m.AnimationFrameTimer
        health=$m.Health; supers=$m.SuperMissiles; cameraX=$m.CameraX; cameraY=$m.CameraY
        ex=$e.XPosition; exsub=$e.XSubposition; ey=$e.YPosition; eysub=$e.YSubposition; ehp=$e.Health
        frozen=$e.FrozenTimer; flash=$e.FlashTimer; ai=$e.AiHandlerBits; list=$e.CurrentInstruction; listTimer=$e.InstructionTimer; map=$e.SpritemapPointer
        vx=$c.XVelocity; vy=$c.YVelocity; function=$c.Function; fallsub=$c.FallingYSubvelocity; fall=$c.FallingYVelocity; return=$c.NonFallingFunction; turn=$c.ConsecutiveTurnCounter
    }
    for($j=0;$j -lt 4;$j++) { $values["support$j"]=$m.Support[$j] }
    for($j=0;$j -lt 5;$j++) { $values["shotType$j"]=$m.Shots[$j].Type; $values["shotX$j"]=$m.Shots[$j].XPosition; $values["shotY$j"]=$m.Shots[$j].YPosition }
    foreach($field in $values.Keys) {
        if($null -eq $values[$field] -or $null -eq $n.$field) { throw "Missing $field at frame $i." }
        if([long]$values[$field] -ne [long]$n.$field) {
            if($failures -lt 40) { Write-Output "Frame $i ${field}: managed=$($values[$field]) native=$($n.$field)" }
            $failures++
            if(-not $fieldFailures.Contains($field)) { $fieldFailures[$field]=0 }
            $fieldFailures[$field]++
        }
    }
}
if($failures) {
    foreach($field in $fieldFailures.Keys) { Write-Output "${field}: $($fieldFailures[$field]) mismatching frames" }
    throw "$failures mismatches; parity not established."
}
Write-Output 'All 500 frames match movement, projectiles, crawler fall/freeze state and solid-enemy support.'
