# #400 Phantoon Doppler investigation

## Status

In progress. The initial 30-run matrix and grounded/aerial closure-window controls
now match native execution, but do not establish every technique in this ticket.
Remaining acceptance includes spaced barrages, moving/stuttered Dopplers,
Samus-position effects on the returning boss, and finisher controls. Do not mark awaiting validation
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

## Reproduced missing missile input carry

Original-CPU capture of the same 30 input sequences found the first mismatch at
start1640/cadence9/no-jump/frame1650: native projectile slot zero contains missile
`$8100`, while the port contains no projectile. The physical Shoot edge was on
1649. Native `$90:BE65..BE72` accepts either current new input or the saved
draw-time new input (`$0E00`), allowing a press one frame before cooldown expires
to fire on the following frame. The missile producer already supported that
word, but `SuperMetroidRuntime` omitted its argument. Only cinematic playback
passed it. The runtime now passes `Samus.PreviousDrawNewInput`, already owned by
the translated `$90:EAB3` draw epilogue. No new input buffer or boss exception
was introduced. Corrected the misleading comment that this word is ordinarily
zero during live gameplay.

`DebugRunner --missile-input-carry-audit ROM` is a full-runtime synthetic-room
regression, with ordinary missile selection and Shoot edges. After firing on
frame0, second presses at8/9/10/11 yield respectively no second shot, a shot at10,
a shot at10, and a shot at11. Holding Shoot for24 frames yields only the initial
shot. Ammo consumption is asserted alongside exact firing frames. Before the
fix, the frame9 case fails (only shot0); all five controls pass afterward.

The full normal-input Phantoon comparison then matches **55,500 frames** and
150 numeric columns per frame, covering Samus position/pose/health, boss
position/phase/hurt clocks, player/enemy projectiles, and swoop velocities/target.
This matrix does not yet compare the eye instruction timer or prove successful
spaced/aerial Dopplers. A jump input may be rejected while Samus is already
airborne or hurt; do not label such a case successful aerial coverage.

### Native reproduction

Use the same pinned ROM/upstream revisions documented in PHANTOON-ENRAGE-401.md.
The native fixture uses Varia only, no beams, 100 missiles, no Supers/Power Bombs,
health999 and RNG `$0061`. It runs original game-state-eight code with the real
room PLM/FX setup. No code, projectile or actor state is replaced after setup.

Apply `movement-release/native-phantoon-doppler-entrypoint.patch` to the pinned
native checkout, then rebuild Release x64. It dispatches before SDL and reports
errors/warnings to stderr. After use, remove hooks and rebuild the normal binary.

```text
sm.exe --diagnostic-phantoon-doppler ROM output.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --phantoon-doppler-search ROM output.csv
```

The optional CSV comparison pins the accepted fingerprint. Two independent
captures match SHA256
`609DBAFF42F94CEEBC53B670C8ABDD91C0D97503ADAC6B81876069015BFA0BC5`.
`movement-release/phantoon-doppler-400.zip` preserves the numeric-only CSV,
including60 native setup records. No ROM, save, screenshots, artwork or audio
is included. Full verification also passes with the production handoff fix.

## Grounded and airborne fourth-hit windows

The expanded search varies later barrage starts, 8..12-frame firing attempts,
walking/running/stationary input, and optional Jump. It is exploratory:
`--phantoon-doppler-expanded-search ROM`. Four selected controls now have an
original-CPU comparison through frame1849. They retain the initial setup above,
press Left only on the barrage-start frame, then release direction. Jump, where
specified, is held for20 frames. Shots continue at the given cadence.

| Start / cadence / Jump | Damage frames after primer1565 | Fade-out begins |
| --- | --- | --- |
| 1680 / 10 / no | 1693, 1701, 1711, 1720 | 1729 |
| 1680 / 11 / no | 1693, 1703, 1713 | 1722 |
| 1710 / 9 / yes | 1710, 1720, 1747, 1756 | 1765 |
| 1710 / 10 / yes | 1710, 1720, 1837 | 1846 |

The successful cases accept a fourth barrage hit after the third requested
closure, extending the damage window. The airborne success is actually airborne:
Samus Y106/122 at its last two hits, versus ground center187. Assertions require
every damage frame, final health, fade-out frame, and those airborne positions.
Each of7400 frames compares158 native columns: the prior150 plus body function
timer, body/eye instruction pointers and timers, accumulated round damage,
previous-new input, and shared projectile cooldown. This is not proof that
either recording demonstrates a walking/stuttered Doppler.

### Eye callback continuation correction

The added instruction columns failed first at frame1537: port eye instruction
`$CC9D`, native `$CC67`. `SetupEyeOpenPhantoonState` changes the body and eye
lists, but native common callback instruction `$A0:808A` resumes its local cursor
after the callback. The eye's following Sleep at `$A7:CC67` overwrites its list
change. The port prematurely returned from list interpretation instead.
Returning normal continuation restores the native ordering, while the separate
body actor retains its new list. The exact before/after trace now matches all
7400 records; this is a real animation-instruction mismatch, not an invented
Doppler rule or a change to damage thresholds.

Use `native-phantoon-window-entrypoint.patch` and `native-phantoon-window-probe.h`
with the same bounded original-CPU setup, then restore/rebuild the native host:

```text
sm.exe --diagnostic-phantoon-window ROM output.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --phantoon-doppler-window-audit ROM output.csv
```

Two independent captures share SHA256
`15A5E22F7BBCC381D98EE42B648489818A59F5733D002297C4AE40FF0F83DF1A`.
`movement-release/phantoon-window-400.zip` contains only the numeric CSV,
including8 setup records; no game assets or save data are included.
