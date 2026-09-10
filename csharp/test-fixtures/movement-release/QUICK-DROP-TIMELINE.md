# Quick Drop: input-driven ceiling continuation — partial #456

Extends [the handler regression](QUICK-DROP-CEILING.md) through actual controller
turn entry, animation completion, and adjacent failing input timings. ROM and
pinned native/disassembly revisions are identical to that document.

## Fixture and authority

308 independent cases, 80 frames each: two starting facings, opposite direction
delays 0 through 10, and 14 ceiling-removal cases. Jump is held on frames 8–49;
opposite direction starts at frame `8 + delay` and ends after frame 49. The ceiling
is removed before frame `8 + remove` for removal cases 0–12, or never for case 13.

Synthetic 144-by-80 room, floor row 16, ceiling row 12, center `(1024,235)`,
zero initial subpositions/velocity. Morph Ball only, no beams, no liquids,
99 health, no cheats, no enemies or PLMs. Terrain removal is an explicit fixture
event, **not** a simulation of bomb/crumble PLM execution.

Original CPU order: alpha `$90:E695`, interactive enemies `$A0:9785`, live movement
handler, animation `$90:8000`, projectiles `$90:DDE9`, collision/input `$91:E8B6`,
pose `$91:EB88`, draw `$90:EAB3`, palette `$90:E9CE`, projectile processing `$A0:9169`.
Normal startup horizontal-slope mode is 3; NMI counter is frame + 2. Port uses
`SuperMetroidRuntime.StepFrame`, after clearing unrelated loaded actors and terrain.

Every frame compares exact X/Y subpixels, pose, movement type, animation frame/timer,
base/extra horizontal speed, acceleration mode, facing, vertical speed/direction,
and charge counter. Input, row ordering, dimensions, and capture hash are guarded.

## Named success/failure witnesses

Both facings behave symmetrically:

- Delay 2/removal 4: turn enters on frame 10, touches ceiling on 11, resumes ascent
  on 12; on frame 16 its animation finishes in normal jump at Y `00CC.D000`,
  upward speed `0004.1C00`.
- Delay 3/removal 4: one frame later is too late. Frame 11 hits the ceiling before
  the turn can suppress its result: Y `00E3.0000`, speed zero, direction down.
- Delay 2/never removed: the ceiling still blocks movement throughout the turn.
  Animation finishes on 16; ordinary jumping stops ascent on 17. This rejects
  implementations that simply disable ceiling collision.

The accepted capture was also run with the production ceiling fix temporarily
disabled: 4,140 of 24,640 frames mismatched, beginning at frame 11. Restoring the
fix produced zero mismatches. No additional production change was needed here.

## Reproduction

Apply `native-quick-drop-timeline-entrypoint.patch` in `upstream-sm` using
`git apply --unidiff-zero`, build Release x64, then:

```text
sm.exe --diagnostic-quick-drop-timeline "Super Metroid.smc" capture.csv
SuperMetroid.DebugRunner --quick-drop-timeline-audit "Super Metroid.smc" capture.csv
```

The headless entrypoint suppresses explicit SDL dialogs and bounds CPU execution.
Remove its temporary hooks after capture. Two independent captures match SHA256
`DB3FBBF371C2D0152072C3D7F2F9BA23A2B939A7D5A6AD608D4F12DA6ADDC700`.
Accepted CSV: `quick-drop-timeline-native-capture.zip`.

## Remaining scope

Follow-ups: [QUICK-DROP-CRUMBLE.md](QUICK-DROP-CRUMBLE.md) and
[QUICK-DROP-BOMB.md](QUICK-DROP-BOMB.md) now cover the destruction paths below.
The latter contains the combined completion audit; this section records the
scope of the ceiling timeline fixture alone.

Ceiling input timing is covered. Falling velocity retention across actual crumble,
bomb, and Power Bomb block destruction is not covered by these manually removed
ceiling fixtures. #456 remains open without awaiting-player-validation until that
remaining work is complete. This capture does not establish PAL parity.
