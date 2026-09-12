# #400 Phantoon Doppler investigation

## Status

In progress. The normal-input barrage search is diagnostic, not a completed
native parity matrix. Remaining acceptance includes spaced barrages, moving and
aerial Dopplers, missile cooldown/closure boundary sweeps, Samus-position effects
on the returning boss, and finisher controls. Do not mark awaiting validation
based only on this first crash correction.

## Reproduced explosion-family crash

`DebugRunner --phantoon-doppler-search ROM` runs 30 seeded room-local input
sequences. Samus starts at `(128,187)` in Phantoon's room, with 999 energy,
Varia, no beams, 100 missiles, and no cheats. Setup and initial movement follow
the #401 encounter, with missile attempts at 1565 and 1575. Barrage starts
1640/1650/1660, attempts shots every 8/9/10/11/12 frames while holding Left,
and optionally holds Jump for its first 20 frames. An attempted jump is not
proof of an airborne barrage; actual trajectories still need classification.
Each run ends after frame1849. No runtime actor state is injected after setup.

Before correction, start1660/cadence8/no-jump throws:
`Projectile family $800 has no translated vulnerability field.`
A missile explosion overlapping Phantoon reaches his vulnerability lookup.
`RoomEnemySystem.PhantoonCollision` rejected only family `$700`, not `$800`.
Pinned original `bank_A0.asm` `$A0:9BE6..9BF6` rejects families `$300`, `$500`,
and **every** masked family at or above `$700` before hitbox processing.
The correction restores that exact range check, rather than adding an explosion
vulnerability or swallowing the exception.

The focused `--phantoon-plasma-audit` regression overlaps Phantoon with every
family `$700..$F00`, asserting no contact, damage, or projectile direction mark.
It reproduces the same `$800` exception before the production fix and passes
afterward. The 30-run search then completes all 55,500 gameplay frames.
Existing original-CPU comparisons also remain green: #401's 15,300 frames and
#402's 16,770 Phantoon frames. This correction is verified independently of the
still-incomplete Doppler parity investigation. No exception handler or native
emulation rule was loosened.
