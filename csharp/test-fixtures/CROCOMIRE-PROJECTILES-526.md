# Crocomire projectile trajectory (#526)

Affected player version: **0.1.1**. The report was that Crocomire did not visibly
launch projectiles. The reproduced defect is wrong projectile direction, not an
absent attack-state implementation.

## Reproduction

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --crocomire-attack-entry-audit "Super Metroid.smc" csharp/test-temp/crocomire-526
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --crocomire-audit "Super Metroid.smc"
```

Keep generated captures local: they contain game artwork. No emulator GUI or
external controller replay is needed for these bounded checks.

The new runtime fixture loads the retail room, wakes Crocomire by proximity, and
injects one charged projectile inside the current mouth hitbox in each of his two
initial waiting-for-damage phases. The normal collision handler processes each
hit. It never writes fight function, instruction cursor, or attack RNG selection.
It then stops shooting and lets the production runtime reach a complete volley.
Continuous mouth hits can interrupt an attack; this is deliberately an
uninterrupted attack stimulus, not a claim that every player approach has the same
timing.

Observed deterministic sequence: wake frame 1, second waiting phase 240, step-back
290, projectile-attack selection 452, first emission 542, ninth emission 830.
Emissions are 36 calls apart, independently checked against the five timed ROM
records starting $A4:BB96 (8+7+7+7+7 on this revision).

Before the fix the first projectile had velocity (0,-1024) in signed 8.8 units:
straight up at four pixels per frame. It disappeared into the ceiling. The new
trajectory assertion failed on that result before production was edited.

## Cause and fix

Pinned `upstream-disassembly/src/bank_86.asm` shows the exact native operands:
$86:9095 reads X from the signed sine table at $A0:B443; $86:90A1 reads Y from
negative cosine at $A0:B3C3. Both are multiplied by four.

The C decompilation uses a combined 320-word table with a negative-cosine prefix.
Our translation used the sine label as its base but retained the combined-table
index offsets, rotating the trajectory by a quarter turn. The fix uses the two
actual native table operands directly. It does not change the angle routine,
random attack probability, attack cadence, damage, or rendering priority.

Afterward, the first zero-gradient projectile moves (-1024,0). The runtime test
checks every first-flight coordinate through the next emission, verifies actual
visible fire-colored pixels at its predicted position clear of both actors, and
requires nine live emissions at native cadence. The frame 566 image was visually
inspected: a fireball is visible traveling left across the room, not above the
mouth. The existing Crocomire audit also passes, including production projectile
contact/damage/deletion and the complete death graph.

This is cartridge instruction/table comparison, not a newly recorded full native
encounter. Leave #526 open awaiting player validation after verification.

## Separate Power Bomb report (#528)

Native $A4:B992 explicitly chooses the Power Bomb forward-charge program rather
than projectile knockback. The subsequent trajectory/timing comparison is recorded
in [CROCOMIRE-POWER-BOMB-528.md](CROCOMIRE-POWER-BOMB-528.md); this projectile fix does
not change that reaction.
