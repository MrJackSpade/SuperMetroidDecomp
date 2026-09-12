# Wrong-side gate investigation (#403)

Affected player version: 0.1.1. The reported room $02/$22 is Kronic Boost,
`RoomHeader_KronicBoost` at `$8F:AE74`, not an inferred substitute room.
Exact player weapon, trajectory and input sequence remain unspecified.

## Initial managed shot-origin sweep

`GateGlitchRoomAudit` loads the authored 32x48 room, whose downward gate is at
block `$0287`, pixel (112,320). Each trial clones unchanged loaded terrain/BTS
and reloads the resident PLM population, then advances its instructions twice.
The gate's initial spawn request is cleared from the observation queue.

Samus is stationary in normal-jump up-left aim pose `$6A`, with zero subpixels,
speed, equipped beams and items. X runs 128..152 inclusive; Y runs 304..384.
HUD selection is beam/missile/super (0/1/2), with ten actual missiles and supers,
no infinite-ammo setting. Shoot is pressed only on frame zero. Real projectile
production, collision and PLM stepping run for up to 24 frames per trial.
The positive observation is the resident gate's trigger timer becoming nonzero,
not Samus crossing the gate and not merely an impact animation.

| Weapon | Activating origins / 2,025 |
| --- | --- |
| Ordinary beam | 50 |
| Missile | 119 |
| Super Missile | 116 |

Examples: missile (128,347) activates on frame zero; ordinary beam (140,360)
also activates on frame zero. These are measured managed outcomes, **not native
expectations**. The initial matrix deliberately includes possibly unreachable
origins and overlaps: it cannot establish a player-executable exploit or prove
that the activation window is too wide. No production fix or issue closure is
justified by these counts alone.

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --gate-glitch-room-audit "Super Metroid.smc"
```

The command prints every successful launch and per-weapon totals, preserving
pixel boundaries for a native sweep. It never moves Samus after seeding, changes
player save slots, or claims to reproduce the unspecified 0.1.1 input sequence.
Next: original-CPU comparison of these same launch windows, followed by real
rising/spinning/falling input cases, gate animation/state assertions, and blue/
green weapon and orientation controls. Existing gate-filter tests are not a
substitute for those technique checks. Source: the Gate_Glitch wiki and pinned
bank-$84/$94 routines; no wiki claim has yet been promoted to an expected result.

## Original-CPU comparison: ordinary beam discrepancy

`native-gate-origin-probe.h` repeats the same 6,075 setups. The authored level
is decompressed using the existing asset decoder; room PLM spawning (`$84:846A`),
two warm-up PLM passes, weapon production (`$90:B80D` / `$90:BE62`), projectile
processing (`$90:AECE`) and PLM handler are original ROM execution. Cooldown is
advanced explicitly. No movement, enemy AI or rendering is simulated.

Native results: zero ordinary-beam activations, 119 missile activations and 116
Super Missile activations. Every missile/super successful origin, hit frame and
projectile coordinate matches the managed sweep. The port's 50 ordinary-beam
activations are extra; this is a reproduced mismatch, not merely a wiki claim.
At (140,360), native firing already creates impact type `$8700`, list `$A007`,
at (113,329), then projectile processing advances the impact list to `$A00F`.
The managed beam instead activates the switch on frame zero at (110,326).
The next diagnosis is the initial ordinary-beam collision path; no production
fix has yet been made and #403 is not ready for player validation.

`gate-origin-403.csv` stores all 235 native successes. The matrix bounds above
define every omitted case as a negative. Two independent captures share SHA-256
`19658956DD961D6624F3B106F98F6BCEF8A344088B50708F534B1BE71378A6C5`.
An initial probe failed because an unparenthesized address expression expanded
incorrectly through `RomFixedPtr`; that run produced no usable results and is
excluded. The committed probe uses a separate address variable.

To reproduce, temporarily include `native-release-probe.h` and this probe after
`state_recorder` in upstream sm_rtl.c. Add a pre-SDL, console-only entry invoking
`DiagnosticGateOrigins(rom, output)`, rebuild Release x64, and run:

```
sm.exe --gate-origin-probe "Super Metroid.smc" NEW_OUTPUT.csv
```

Remove only those temporary hooks afterward. Execution is instruction-bounded,
and output creation is exclusive. The probe prints the isolated beam sample to
stderr for diagnosis. Numeric output contains no ROM or player-state bytes.

## Verified scan-termination fix

Instruction tracing of the original sample identifies gate setup `$84:B96C`
(header `$B974`) writing zero to span scratch `$26` and `$FFFF` to collision
scratch `$28`. The port incorrectly treated this as empty setup. Its aggregate
solid/air scan could therefore continue past the gate and reach the switch.

The projectile reaction now propagates successful gate allocation as scan
termination and forced ordinary-beam collision. Wave retains scan termination
but still discards collision carry. Exhausted pools do not execute setup and
therefore do not publish those scratch effects. Terrain remains unchanged.

The focused regression failed before the change (expected native impact X=113,
got 110), and now matches X=113, Y=329, type `$8700`, instruction `$A00F`, and
an untouched switch timer. Eight synthetic cases additionally exercise both
axes, Wave/non-Wave, and available/exhausted allocation pools.

The complete managed 6,075-origin sweep now exactly matches every native
positive record and negative: beam 0, missile 119, super missile 116, including
hit frames and projectile positions. The focused regression runs in the default
verification suite. This is a scoped fix, not completion of #403: moving
rising/spinning/falling setups and the remaining orientation/weapon controls
still need their own cartridge comparisons.

## Moving setup located (managed evidence, not yet native expectation)

`DebugRunner --gate-glitch-jump-audit ROM SHOOT_FRAME` now drives the whole
production runtime from Kronic Boost X=140, Y=379, no equipped items/beams,
ten missiles, missile selected. Sixty neutral settling frames establish the
authored floor and standing-left pose. Frame zero begins held Jump+Left+AimUp
(`$0290`); Shoot is pressed only on the requested frame. The trace includes
inputs, position/subpixels, pose, gate timer, and gate instruction pointer.

Sweeping firing frames 0 through 20 finds one activation: frame 8. Frames 7
and 9 are adjacent failures. At frame 8 Samus is X=133,Y=355, pose `$6A`;
the gate timer becomes 1 and its instruction changes from `$BC44` to `$BC51`.
The opening sequence progresses to `$BC5D` at frame 56 and Samus crosses the
gate afterward. This distinguishes switch activation from physical crossing.

An earlier trial began at Y=363 without enough settling; it was still falling
when Jump was pressed and is explicitly rejected as a jump reproduction.
The corrected setup has been rerun for frames 7,8,9. These results locate a
deterministic success and adjacent failures for the next original-CPU comparison;
they are not yet assertions of cartridge timing parity.

## Original-CPU moving comparison

`native-gate-jump-probe.h` now reproduces that setup with original 65816
input, movement, projectile, collision, pose and PLM handlers. All 140 records
for each firing frame 7, 8 and 9 match the managed trace exactly (420 records):
inputs, X/subpixel X, Y, pose, gate timer, and gate instruction. This covers the
entire jump, ceiling contact, landing, gate opening and subsequent crossing.
The successful timing is retained; adjacent firing frames do not open the gate.

The numeric native traces are `gate-jump-403-{7,8,9}.csv`; the default
verification suite drives `SuperMetroidRuntime.StepFrame` and compares every
record. `--gate-jump-traces` runs just those comparisons. For recapture, install
the same temporary headless hooks described above, substituting this header and
`DiagnosticGateJump(rom, output, shootFrame, aimFrame)`, then invoke
`sm.exe --gate-jump-probe ROM NEW_OUTPUT.csv SHOOT_FRAME AIM_FRAME` (use aim frame
zero for the original three traces). Remove hooks afterward.

This probe does not run the renderer or ordinary enemy AI. It uses authored
room terrain/population and the original gate setup/PLM opening sequence.
It does not establish spinning-shot, green-gate or reversed-orientation parity;
those parts of #403 remain open. No further production change was needed for
these three moving sequences.

## Spin negative control

An optional fourth DebugRunner argument selects delayed aim. With aim frame
greater than zero, frame -1 holds Left to establish running before Jump;
otherwise simultaneous Left+Jump from rest is a normal jump, not a spin test.
Aim frame 7 / Shoot frame 8 produces spin pose `$1A` in both implementations.
Aim and Shoot do not end that spin in this input sequence, and the gate remains
closed. All 140 native/managed records match; the default regression now checks
560 records including `gate-jump-403-spin-8-7.csv`.

This is a negative control, not proof that successful spinning gate glitches
are absent. No production change was made. A successful spinning setup and
green/orientation controls remain outstanding.

## Spin input ordering and release search

Pinned ROM pose `$1A` transition table `$91:A46E` tests new Shoot first, then
held shooting/aim combinations, then held Left+Jump before held Aim alone.
Consequently adding Aim while continuing Left+Jump does not itself cancel spin.
New Shoot selects the taller ordinary-jump pose; changed-pose collision can
reject that expansion under this room's ceiling. Do not treat the matching
spin negative control as a generic missing fire transition.

The optional fifth DebugRunner argument `true` releases Jump at the aim frame.
This permits the transition but also cancels upward jump speed. A managed search
of aim/release frames 2..7 and Shoot frames 4..12 (54 cases) found no activation.
For aim 5 / Shoot 8, pose changes from `$1A` to `$6A` at Y=361 and remains too
low for the demonstrated window. This is search evidence only, not a new native
timing claim. It does not exhaust spin setups with other origins or inputs.
