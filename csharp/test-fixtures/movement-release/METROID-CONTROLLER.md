# Full-dispatcher Metroid bomb reproduction (#485)

Status: **reproduced mismatch, not fixed or awaiting player validation**. The
opt-in comparison intentionally exits 1 at the current implementation. This
supersedes the evidentiary limitation of manually positioning the attached enemy
in native-metroid-bomb-probe.h; it does not delete that earlier characterization.

## Exact setup

Untouched native ROM instructions run actual EnemyMain (A0:8FD4), including
collision, Metroid AI/instructions and sprite objects. The movement phase uses
the normal bomb producer and full Samus input, movement, animation, pose-priority
and history routines. C# runs SuperMetroidRuntime.StepFrame in the retail Metroid
room, with terrain replaced by the same flat floor. Neither path replaces the
enemy's coordinates with a hand-authored attachment trajectory.

Room geometry: 96 x 16 blocks; rows 10..15 solid, all prior cells air/BTS0.
Samus starts X128/Y153, zero subpixels/velocity, stationary ball 1D/41,
matching previous pose/direction, last-different history zero, animation frame
zero/timer one. Morph Ball and Bombs only (1004), health/max-health 999. No
invincibility/infinite ammo. The native environment is air; C# retained room acid
lies below the solid floor and is not entered. No other enemy remains active.

Metroid uses retail DD7F definition, health/radii from its header, bank A3,
properties 2000, initial position X128/Y145, zero subpixels and AI extension
state. Initialization uses native A3:EA4F / the production C# room loader.
Native active enemy list is explicitly one slot. Attachment happens through
ordinary contact on frame zero. No RNG-dependent movement is seeded; random
enemy sound selection is outside the compared output.

Controller schedule: Shoot at frame 1, then optionally at 1+gap and 1+2*gap,
where gap is 16, 24 or 48. Gap zero is the single-bomb control. Travel index
0..4 holds the facing direction starting at frame 46 for travel*4 frames.
Both facings, all schedules and travel choices run 180 frames: 40 cases / 7,200
samples. Inputs are checked against this formula during replay, not blindly
accepted from the CSV.

## Findings

On baseline 0f4d711b, 1,264 samples differ. All state/position/health fields agree
through frame 51 in the centered cases. At frame 52 C# has already published
0802 bomb-jump direction; native still has zero. Native GameState_8 (82:8B44)
calls SamusProjectileInteractionHandler before EnemyMain, before the later bomb
fuse update. C# publishes overlap inside the later BombProjectiles.StepFrame.
This is a phase discrepancy, not justification to change fuse constants or radii.

Travel index two is especially useful: with eight frames of movement, the first
native detachment is frame 63, escape timer 3 (the same frame's enemy AI already
ran). C# detaches at frame 62 with timer 4 after AI. Native EnemyMain invokes
bomb collision before touch and AI; runtime's bomb hit pass is after enemy AI
and bomb update. The matched fixture now makes both scheduling seams observable.

Native centered single bomb still misses. Centered gap24/gap48 cases detach at
frame109, while travel1/gap48 first detaches at159. Travel2 detaches at63 for all
four schedules. Do not replace these outcomes with a blanket must-detach rule.
The comparison includes X/Y subpixels for Samus and Metroid, Samus pose and
bomb-jump direction, Metroid state/escape timer, health and live bomb count.
It must remain failing until the production schedule is faithfully corrected;
future fixes must also verify bomb-jump fixtures using the actual bank-82 order.

## Capture and repeat

Include native-release-probe.h then native-metroid-controller-probe.h after
StateRecorder in sm_rtl.c, and temporarily dispatch DiagnosticMetroidController
before SDL. Capture output refuses overwrites. Hooks removed after measurement;
no emulator window, SRAM or player debugger slot is used.

metroid-controller-native-capture.zip preserves accepted v2. An independent
repeat is byte-identical, SHA256:
`12EDA7509F071C4BD1D622E40094ECBF0C684B601D7496427670103CD4F30733`.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --metroid-controller-comparison-audit "Super Metroid.smc" path/to/metroid-controller-485-v2.csv
```

DebugRunner builds with zero warnings/errors; reproduction exits 1 with 1,264
mismatches. This commit changes diagnostics only, not gameplay. No claim of a
passing regression suite or resolved player issue is made.

ROM Japan/USA NTSC rev0 SHA256:
12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72.
Native source 578f90b3cc49557bb70060ad033bb90b8cf8ac50; disassembly
362be646929cf8e483f692b73a6561cfc2dc1d0d. PAL parity is not established.
