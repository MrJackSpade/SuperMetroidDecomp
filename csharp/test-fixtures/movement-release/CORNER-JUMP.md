# #457: Corner / Flatley Jump

Reference: https://wiki.supermetroid.run/Corner_Jump (revision 142).
The original technique uses a turnaround before the falling state takes over,
then Jump during the turn. This audit confirms the actual cartridge window rather
than assuming that any jump near a ledge qualifies.

## Evidence and reproduction

- ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- CSV SHA256: `E2FD60B82E140AD17DE6C0962BC3C34995FB026EC1DD8EC2568A57804DE8DAB2`.
- Two independent native captures are byte-identical.

Apply `native-corner-jump-entrypoint.patch` to the pinned native tree, build and run:

```text
sm.exe --diagnostic-corner-jump "Super Metroid.smc" corner-jump-457-v1.csv
SuperMetroid.DebugRunner --corner-jump-audit "Super Metroid.smc" corner-jump-457-v1.csv
```

The accepted CSV is archived in `corner-jump-457-v1.zip`. The explicit diagnostic
entry point runs headlessly with bounded original CPU calls and restored retail
ROM bytes. Remove its temporary hooks after use.

## Matrix

2,700 cases x 112 frames = 302,400 frames. Both facings, initial extra-run speeds
zero/two/four, reverse/no-reverse controls, turn frames zero through 24, and Jump
delays zero through eight relative to the scheduled turn. Run remains held;
Jump is held for 40 frames; all input releases at frame 80. All cases start at
X=1024, Y=491 with running pose, base speed 1.25, no previous Jump and matching
pose history. Speed Booster/Morph are equipped; no liquid or cheat overrides.

The synthetic upper floor is row 32, ending at column 66 rightward or beginning
at column 62 leftward. A lower floor at row 48 catches failures. The fixture
executes real production input, movement, collision, pose and animation code,
without a cross-room controller route.

Every frame compares position/subpixels, pose/movement, animation frame/timer,
horizontal base/extra velocity, acceleration mode, facing, vertical speed/direction,
flare counter and active movement radii. Radii are sampled after native alpha and
before managed movement to compare equivalent lifecycle boundaries.

## Explicit success and failure witnesses

For initial extra speed two and turn frame six, both facings:

- Frame 11: Samus's entire horizontal hitbox is beyond support, yet movement remains
  ground-turn ($0E). The body is genuinely airborne, not merely overhanging an edge.
- Jump six frames after the turn succeeds at frame 12, entering spin with vertical
  speed `$0004:E000`. Center Y is `$0200:EFFF` rightward and `$0206:9FFF` leftward.
  Native collision scan asymmetry is preserved rather than forcibly mirroring Y.
- Jump seven frames after the turn fails: frame 12 finishes the turning animation,
  and frame 13 enters falling instead of launching. This is the adjacent one-frame
  failure control.
- Without the turnaround, the same delayed Jump inputs remain falling.

These properties are named assertions in addition to the complete CSV comparison.

## Why it works / result

Pinned bank $90:A67C performs ground-turn horizontal movement and no-speed-calculation
vertical movement, then explicitly clears the solid vertical collision result at
$90:A68F. Bank $91:8142 still performs normal transition-table lookup during this
state; $91:F8D3 folds run momentum into the turn. Consequently losing physical
support does not immediately replace the turn, and Jump can launch during that
animation window. Once the window expires, ordinary falling prevents the launch.

The existing managed implementation matches all 302,400 captured frames. No
production behavior change was needed; this change preserves reproducible evidence
and regression assertions. This is pinned-revision parity, not proof about every
regional revision or arbitrary modded movement table. Awaiting player validation.
