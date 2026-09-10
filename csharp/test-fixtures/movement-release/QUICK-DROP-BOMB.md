# Quick Drop: bomb-family destruction — #456 completion evidence

Uses the same revision-zero ROM and pinned native/disassembly revisions documented
in [QUICK-DROP-CEILING.md](QUICK-DROP-CEILING.md).

## Scope and setup

160 independent 60-frame cases: two facings, normal Bomb / Power Bomb, 1x1
respawning / permanent bombable blocks (BTS 0 / 4), turn-input delays 0–4,
and explosion-contact frames 0–3. Total 9,600 frames.

Synthetic 144-by-80 room. Safety floor row 32; five bombable `$F000` blocks at
row 16, columns 62–66. Samus starts falling at `(1032,233)`, zero subpositions,
speed `3.0000`, direction down, pose `$29/$2A`, matching previous pose/movement,
Morph Ball only, 99 health, no beams, liquids, actors or cheats. Opposite direction
is held from the selected delay through frame 29. No jump input or RNG dependence.

On the selected explosion-contact frame, the native fixture runs original CPU
`$94:A052` for each block with projectile family `$0500` or `$0300`. The port runs
`CollectSingleBombedBlockReaction`, the production dispatcher shared by normal-bomb
crosses and Power Bomb boundaries. Both then execute actual PLM setup and live
instruction streams; terrain is never manually cleared. The sampled projectile
family is removed from native scratch slot 5 after dispatch so it is not treated
as a separate live projectile by the subsequent normal gameplay phases.

**Boundary limitation:** this seeds an explosion contacting terrain. It does not
test placing/fusing a bomb, bomb-jump interaction, Power Bomb expansion/range,
audio, or rendering. Those are separate mechanics; this fixture isolates #456's
velocity across bomb-destroyed floor contact. Likewise, it does not claim to test
the entire respawn tail, every block dimension, or PAL behavior.

Run ordinary movement/animation/pose stages in the same order as
[QUICK-DROP-TIMELINE.md](QUICK-DROP-TIMELINE.md), then the real `$84:85B4` PLM handler.
The C# side uses `StepFrame`. Compare exact per-frame input, X/Y subpixels, pose,
movement, animation frame/timer, horizontal speed/mode/facing, vertical speed/
direction, charge counter, and central block word. The archive hash, dimensions,
frame/case ordering, and named success/failure witnesses are guarded.

## Confirmed behavior

For every facing/family/respawn combination, contact frame 1 + turn delay 0:

- Frame 0 still has `$F000` terrain and has entered the falling turnaround.
- Frame 1 physically clips to Y `00ED.FFFF`, retains falling speed `3.3800`, then
  the actual PLM draw changes the floor to air `$0053`.
- Frame 6 finishes the turn at Y `00FF.2FFF`, speed `3.C400`, normal falling.

Delay 1 is one frame too late: the contact lands Samus and loses speed. Frame 2
correctly returns from landing to falling at Y `00EC.FFFF`, zero speed. This also
exercises the missing-floor landing correction in `bf8589ca` through bomb terrain,
not just crumble terrain. No additional production change was needed.

All 9,600 frames match the cartridge. Two independent native captures match SHA256
`295138040F122275E2BF5DEDD7C346DBAA1980D48C354005B441E2621A071ADA`.
Accepted CSV: `quick-drop-bomb-native-capture.zip`.

## Reproduce

Apply `native-quick-drop-bomb-entrypoint.patch` in `upstream-sm` with
`git apply --unidiff-zero`, build Release x64, then:

```text
sm.exe --diagnostic-quick-drop-bomb "Super Metroid.smc" capture.csv
SuperMetroid.DebugRunner --quick-drop-bomb-audit "Super Metroid.smc" capture.csv
```

The headless host suppresses explicit SDL error dialogs and bounds CPU execution.
Remove temporary host hooks after capturing. Player saves are never read or written.

## #456 requirement audit

- Temporary ceiling hit preserves ascent: 12 exact handler frames plus 24,640
  controller-driven frames, successful and one-frame-late failure cases, and
  persistent-ceiling collision after animation completion.
- Crumble contact preserves falling velocity during turns: 3,600 frames through
  real crumble setup/instructions, including velocity loss on late input and
  falling again when landing support disappears.
- Bomb/Power Bomb destruction: this 9,600-frame shared-boundary matrix verifies
  both projectile families and successful/late turn timing through real PLMs.
- Fixes are cartridge-based: `2c8098c3` preserves ceiling velocity only when the
  turn clears collision; `bf8589ca` admits landing art to the normal missing-floor
  transition. Collision remains active. Pre-fix failures are documented alongside
  each fixture. No native glitch was clamped away.

These fixtures satisfy the pinned NTSC mechanical scope. Mark #456 awaiting player
validation, not closed. They make no claim about unrelated remaining parity issues.
