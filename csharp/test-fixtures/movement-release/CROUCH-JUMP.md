# Crouch-jump height and angle timing (#462)

## Independent reference and scope

Original Japan/USA revision-zero ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native host: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
InsaneFirebat disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

[Hitbox Manipulation](https://wiki.supermetroid.run/Hitbox_Manipulation), revision
10438, describes the neutral crouch height bonus and the aimed-crouch exception.
The original CPU capture confirms those claims; expected values were not derived
from the managed implementation. Local `bank_91.asm` at $91:FC66-$FC98 confirms
that only previous neutral crouch poses receive the ten-pixel center subtraction.
Combined with the normal radius expansion/alignment, this produces the measured
eight-pixel advantage instead of an aimed crouch's two-pixel disadvantage.

## Deterministic setup

480 independent cases, 240 complete frames each (115,200 compared frames):

- Air, fully submerged water, and submerged water with Gravity Suit.
- Hi-Jump absent/present, both facings, standing/crouching.
- No aim, angle up, angle down, or both shoulder buttons.
- Aim starts two or one frames before Jump, simultaneously, or one/two frames later.

Synthetic 144x80-block room, floor row 48; everything else is air. Samus starts
(1024,747), zero subpositions/speeds, neutral standing pose with matching history,
animation frame 0/timer 1, health 99. Morph Ball plus only the equipment above;
no beams, enemies, PLMs, debug cheats or RNG-dependent actors. Water surface is
Y8 with liquid option $80. Same native bindings as the other movement fixtures.

Down is tapped on frame 2 for crouching cases. Jump is held frames 24 through 179;
aim is held from its selected offset through 179. Remaining input is neutral.
Every pose change is produced by the controller dispatcher. Captured stages are
alpha, interactive-enemy list, movement, animation, interruption, block/pose
dispatch, gamma, feet adjustment and enemy timers. The comparison invokes full
production `StepFrame` on the matching room, not direct jump initializers.

Each frame compares fixed X/Y, pose/type, animation/timer, horizontal momentum,
acceleration mode/facing, vertical speed/direction, charge counter, and movement
hitbox radii. Explicit assertions additionally check launch height, maximum
height and completed landing for every case.

## Results

All 115,200 frames match, with **no production change required**:

- Neutral crouch: center and apex eight pixels above standing.
- Aim already held before Jump: center and apex two pixels below standing.
- Aim first pressed on the Jump frame or afterward: retains eight-pixel bonus.
- Both shoulders obey the same height exception; all results hold for both
  facings, Hi-Jump and both underwater equipment cases.
- Every case returns to the floor at the original X, with standing hitbox and
  zero vertical direction. Gravity Suit restores the air apex underwater.

Standing 16.16 apex measurements (fixed launch Y `02EB.FFFF`): air normal
`027C.E7FF`, air Hi-Jump `0244.6BFF`, water normal `02BA.1FFF`, water Hi-Jump
`0286.BFFF`. These and the adjusted crouch apexes have named regression checks.

The initial diagnostic sampled native radius scratch latches after pose processing
and reported 960 differences, with all positions/velocities already identical.
Native alpha refreshes the active radius before movement; changed-pose collision
can restore the old radius afterward. C# keeps the new radius eagerly. Version 2
samples active movement radii at the same phase on both sides instead of treating
that storage-lifetime difference as a movement defect. No comparison field was
removed and no production workaround was added.

## Replay

Two independent v2 captures are byte-identical, SHA-256:
`46E1D8EE864C518FEEC25B745AB864D66D1E374B148CE690985AB429186C4774`.
Extract `crouch-jump-native-capture.zip`, then:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --crouch-jump-audit 'Super Metroid.smc' PATH/crouch-jump-462-v2.csv
```

For independent recapture, apply `native-crouch-jump-entrypoint.patch` inside
upstream-sm, build the native host, and invoke
`--diagnostic-crouch-jump ROM NEW.csv`. This explicit headless path uses bounded
original CPU execution and suppresses both SDL dialog paths. Temporary hooks
were removed after capture; the saved patch passes apply --check. No player save
or SRAM files are involved. These are pinned-revision room-local tests, not a
claim about untested ROM revisions or every vertical-movement technique (#423).

Issue #462 is ready for player validation and remains open.
