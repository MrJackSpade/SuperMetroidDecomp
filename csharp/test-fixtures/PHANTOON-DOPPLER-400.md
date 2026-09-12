# #400 Phantoon Doppler investigation

## Status

Ready for player validation. Original-CPU comparisons cover the initial timing
matrix, grounded/aerial closure windows, moving barrages, successful spaced and
stuttered barrages, Samus-height influence on return, and finisher boundaries.
Three earlier production mismatches were corrected and reproduced; the final
spaced/stuttered controls required no additional gameplay change. Historical
in-progress sections below document the evidence available at those stages.

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

## Super Missile finisher controls

Four normal-input sequences compare 7,400 gameplay frames, with all 150 native
columns checked each frame. Initial boss health is 1000 or 1001; four missiles
hit at frames 1565, 1693, 1701, and 1711. Item Select at 1719 switches to Supers.
A Super fired at 1720 hits for 600: remaining health 600 produces the fatal-swoop
phase, while 601 leaves one health and enters enrage. Firing at 1721 instead
misses the closed damage window in both controls. Assertions require the exact
hit frames, health, and lethal/nonlethal phase priority. No production correction
was necessary for these cases; this does not complete the remaining techniques.

Use `movement-release/native-phantoon-finisher-entrypoint.patch` and its probe
header with the same bounded original-CPU setup, then restore/rebuild the host:

```text
sm.exe --diagnostic-phantoon-finisher ROM output.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --phantoon-finisher-audit ROM output.csv
```

Two independent captures share SHA256
`5281FB368D7040098169DDE7B25E89B4D5359381597F8109E84CD4F222CD32D4`.
`movement-release/phantoon-finisher-400.zip` preserves numeric CSV only, including
eight setup records. The shared audit's original #401 matrix also still matches
all 15,300 native gameplay frames.

## Spaced-shot movement search controls (not a completed technique proof)

The pattern search runs 56 sequences: first shot at 1640 through 1700 in ten-frame
steps, late barrage cadence 10 or 11, and four movement policies. Relative shot
times are 0, 10, 40, 50, then a repeating barrage from 80. Movement policies are
continuous Left, Left only from 80, alternating ten frames Left/ten neutral, or
one frame Left followed by neutral. These are physical input words; initial setup
matches the earlier barrage matrix and no actor state is subsequently injected.

All 103,600 gameplay frames match the original CPU across 158 columns, including
position/subpixels, hits, eye instructions/timers, and accumulated damage. Two
independent captures have SHA256
`70F2F64F7A6D4169D2F4382CB6C617D9A6440DEE3A37983CE9F4D34105BDF928`.
Numeric-only evidence is `movement-release/phantoon-pattern-400.zip`.

For example, start1640/cadence10 continuous walking hits at1565,1640,1680,
whereas stop-start walking also hits1690 and closes at1699. At start1700,
continuous walking and standing still both hit1565,1702,1710, but end with boss
Y136 and Y165 respectively. Full native trajectories, not merely these endpoints,
are compared. None of these controls accepts a fourth hit in the swooping round,
so they do not yet prove the full spaced/moving Doppler extension. Keep the ticket
in progress; successful technique and adjacent failure controls remain required.

```text
sm.exe --diagnostic-phantoon-pattern ROM output.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --phantoon-doppler-pattern-search ROM output.csv
```

The matching entrypoint patch and probe are under `movement-release`. Omit the
CSV argument for an exploratory port-only search; that output alone is not parity
evidence. Restore and rebuild the ordinary native host after capturing.

## Successful moving barrage and neighboring failures

The same initial encounter and start1680 barrage now begins continuous Left at
1705 or1710, with shot cadence10 or11. Before that, one Left frame at1680 sets
facing and the remaining inputs are neutral except Shoot. This preserves a real
moving success rather than placing Samus at a favorable coordinate mid-barrage.

| Left begins / cadence | Hits after primer1565 | First eye fade |
| --- | --- | --- |
| 1705 / 10 | 1693,1701,1710 | 1719 |
| 1705 / 11 | 1693,1703,1713 | 1722 |
| 1710 / 10 | 1693,1701,1711,1720 | 1729 |
| 1710 / 11 | 1693,1703,1713 | 1722 |

The successful fourth barrage hit interrupts closure while Samus is moving left:
X116 at1711, X105 at1720, Y187 at both. Tests require exact accepted hits, final
health, eye-fade frame, and the successful ground trajectory. All7,400 gameplay
frames compare158 native columns. No new production correction was necessary.
Two independent original-CPU captures have SHA256
`E6CA849236B495FB2B7E1971E4998F118022F7BACE5CB6DCF8C34AB4283519EA`.

```text
sm.exe --diagnostic-phantoon-moving ROM output.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --phantoon-doppler-moving-audit ROM output.csv
```

The numeric archive is `movement-release/phantoon-moving-400.zip`, with its
matching probe and entrypoint patch. The expanded search also includes brief
one-to-four-frame movements per ten-frame cycle and delayed movement starts;
those exploratory outputs are not substitutes for the pinned successful controls.

## Isolated Samus-height influence on return

Two sequences differ only in releasing the opening Jump at1566 or1574. Both fire
one missile at1565 and no later shots; there are no barrage movement inputs.
Both deal exactly100 damage. All3,700 gameplay frames match158 native columns.

Pinned disassembly `$A7:D374-D39D` steers the vertical swoop toward Samus Y minus
48 (the fatal swoop uses a different target). At1578, the shorter jump produces
boss Y77/vertical velocity1088, while the longer jump gives Y76/velocity960.
Both later cross the right edge and re-enter at frame1817, X254. The short-jump
boss returns at Y158; the long-jump boss returns at Y177. Samus is X119/Y187 in
both cases at re-entry. Tests assert this exact19-pixel difference, the first
steering response, the shared single hit, and the actual edge-crossing event.
No production change was needed.

```text
sm.exe --diagnostic-phantoon-return ROM output.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --phantoon-return-position-audit ROM output.csv
```

Two captures share SHA256
`2608E48604B294FBFB81AC42C7C297852CD1D219DE02AAC9E06121BF2025EDC2`.
Numeric-only archive: `movement-release/phantoon-return-400.zip`, alongside its
matching original-CPU probe and entrypoint patch.

### Earlier incomplete spaced-opening setup

The technique reference's two initial opening hits are not reproduced by the
existing primer pair: with the short jump, the second missile hits the shell;
with longer jump holds it hits only after the swoop begins. Earlier shot attempts
are obstructed by the opening flames. The successful input-only setup below
supersedes this incomplete setup; collision and timers were not changed to force
the technique to succeed.

## Successful spaced and stuttered barrages

The same room, equipment, 999 energy, and 100 missiles are used, with no cheats.
Unlike the earlier probes, the two setup frames are neutral: missiles are not
selected until after clearing the opening flames. Each 1,950-frame sequence uses:

- Up by default; Right on frames 1460..1479.
- Uncharged beam taps at multiples of 12 in 1400..1509; Select at 1512.
- Jump held 1530..1569; missile taps at 1540 and 1550.
- One Left frame at 1640 to face the returning boss; shots at 1640 and 1650.
- A repeating missile barrage from 1700, with the movement/cadence controls below.

The first two missiles hit at 1546 and 1550 while the eye is tracking Samus.
The spaced pair hits at 1645 and 1658 during the swoop. The remaining barrage
starts after that pair. No positions, phases, health, or projectile slots are
changed after setup. A stutter cycle holds Left for two frames and releases it
for eight, starting at 1700; Shoot edges are independent of this movement cycle.

| Movement from 1700 | Shot cadence | Hits after the opening and spaced pairs | Eye fade starts |
| --- | --- | --- | --- |
| Stand still | 10 | 1709 | 1718 |
| Continuous Left | 10 | 1709, 1713, 1720 | 1729 |
| Two-frame Left stutter | 9 | 1709, 1718 | 1727 |
| Two-frame Left stutter | 10 | 1709, 1718, 1727, 1735, 1744, 1752, 1760 | 1769 |
| Two-frame Left stutter | 11 | 1709, 1718, 1726, 1735, 1744 | 1753 |

The ten-frame stutter therefore lands eleven missiles in this opening: two
initial, two spaced, seven barrage hits. This proves a successful extension,
not merely an attempted input pattern. The nine-frame cadence and standing
controls show earlier closure. Exact health, accepted hit frames, opening phase,
and closure timing are explicit assertions. Every frame additionally compares
158 native fields, including projectile state, Samus/boss position and subpixels,
eye instructions, hurt timers, round damage, and cooldown/input carry.

All 9,750 gameplay frames match the pinned NTSC original CPU. Two independent
captures have SHA256
`00DC7756E95692D4C969A15D83BF60B2490E80328F05B48CD7F379B4D771EA7C`.
`movement-release/phantoon-spaced-400.zip` contains numeric CSV only, including
ten setup rows. The matching probe and headless entrypoint patch are alongside it.
The fixture uses the same pinned ROM/source revisions as the preceding controls;
it is not a claim about PAL or optimal world-record damage.

```text
sm.exe --diagnostic-phantoon-spaced ROM new-output.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --phantoon-spaced-audit ROM new-output.csv
```

`--phantoon-opening-search ROM` preserves the exploratory flame-clear/jump/shot
matrix; `--phantoon-spaced-search ROM` searches the neighboring input patterns.
Their unpinned output is not used as a cartridge oracle. Restore the native
entrypoint hooks and rebuild the ordinary host after collecting traces.
