# Running down-aim gap skips (#460)

## Reference and method

Original Japan/USA revision-zero ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native host `578f90b3cc49557bb70060ad033bb90b8cf8ac50`;
InsaneFirebat disassembly `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

[Hitbox Manipulation](https://wiki.supermetroid.run/Hitbox_Manipulation), revision
10438, describes carrying running speed over gaps using down-aim. Original CPU
execution verifies the technique and limits below. The reference runs the normal
falling input table ($91:80B6), falling movement ($90:9168/$A58D), and ordinary
changed-pose collision ($91:FDAE). No Jump input or extra grace period is added.

## Exact fixture

2,400 independent 112-frame cases, 268,800 frames total:

- Both facings; initial extra dash speed 0, 2 or 4 pixels/frame.
- Gap width 1 through 4 tiles (16 through 64 pixels).
- Down begins on frames 0 through 24.
- Down held for 0 (run-off control), 1, 4 or 12 frames.

Synthetic room 144x80 blocks. Upper floor row32/topY512, lower catch floor
row48/topY768. Facing right, gap starts at column66 and extends right by width;
facing left, it ends at column61 and extends left by width. All other upper-floor
tiles are solid. No enemies, PLMs, liquids or RNG-dependent actors.

Samus begins at (1024,491), running pose $09/$0A with matching previous metadata,
zero subpositions and Y velocity, base speed `0001.4000`, selected extra speed,
animation frame0/timer1. Morph Ball and Speed Booster only, no beams, health99,
no debug cheats. Run+forward is already held. Run continues throughout the Down
window, but Down replaces forward. Run+forward resumes when the window expires;
all input releases at frame80. Thus the speed seeds evolve through real running
and falling code rather than being forced to a constant trajectory.

Original CPU order: alpha, interactive-enemy list, movement, animation,
interruption, block/pose dispatch, gamma, feet adjustment and enemy timers.
Managed comparison uses full production runtime frames with matching terrain.
Compared each frame: fixed X/Y, pose/type, animation/timer, horizontal momentum
and mode, facing, Y speed/direction, charge counter and movement-phase radii.
The phase alignment for radius scratch storage is documented in CROUCH-JUMP.md.

## Actual passage and failure assertions

All 268,800 frames match; no production change was required.

Named witness, both facings: extra speed4, three-tile gap, Down held four frames
starting on frame8. Frame8 enters down-aim with radius10 while still falling.
On frame12, forward input restores radius19 and catches the far edge, correcting
the center to `01ED.FFFF`. Frame13 lands at `01EB.FFFF`, radius21, zero Y direction,
with the center beyond the entire gap on the far platform.

The same setup fails when Down starts one frame earlier, when initial extra speed
is reduced to2, or when the gap widens to four tiles. All ordinary run-off controls
in this matrix fail to land on the far upper platform. These are measured limits
of these particular seeds, not a claim that all unassisted gaps are impossible.

Assertions check the actual compressed falling state, far-edge expansion center,
first full-body far-platform support frame and the paired failed cases. Every
frame also guards against penetration of the lower catch floor. Merely entering
down-aim or passing the gap horizontally at the wrong height does not count.

## Replay and verification

Two independent captures have identical SHA-256:
`055D0A80B997F1196BC7F6AF7D9831BCC3F0778CB21ABDCA5BFC40DABCAFB746`.
Accepted CSV is archived in `gap-skip-native-capture.zip`.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --gap-skip-audit 'Super Metroid.smc' PATH/gap-skip-460-v1.csv
```

To recapture, apply `native-gap-skip-entrypoint.patch` inside upstream-sm, rebuild,
then run `--diagnostic-gap-skip ROM NEW.csv`. The explicit headless path suppresses
both SDL dialog paths and bounds original CPU execution. Temporary hooks were
removed after capture; the patch passes apply --check. No save slots/SRAM were
used. Build succeeds without warnings/errors. This is pinned-revision, room-local
evidence, not a whole-game route or every liquid/speed variant. #460 remains open
awaiting player validation.
