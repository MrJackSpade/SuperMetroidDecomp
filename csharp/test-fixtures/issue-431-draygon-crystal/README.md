# Draygon / Crystal Flash counter and ownership investigation (#431)

Status: scoped cartridge comparisons pass; ready for player validation, not closed.
Admission/ownership, shared-counter coupling and retained-spark defects were
reproduced and corrected. The matrices below cover the remaining input boundaries
and ten-capacity refill prerequisite without a multi-room controller route.

## Native evidence

The fixture executes the original unpatched Japan/USA ROM through the shared
65816 CPU adapter. ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.

Numeric trace SHA-256 (LF-normalized UTF-8):
`329B05F1AEE54FFB802F1368EA2B7343C15F47D4A778C0DB84DEA56845A373DE`.

Sixteen cases: both facings, grab then Flash versus grab twelve frames into
Flash, and no input / one Right edge / alternating Left-Right / held Right.
Each captures 120 calls to active Flash movement, Draygon gamma, animation and
palette. It isolates the two owner-entry callbacks; there is no boss flight,
drop generation, Power Bomb placement, drawing, ordinary movement after release,
or late animation-driven pose dispatcher. Do not infer final Flash cleanup or
usable retained spark from this limited trace.

- Flash during grab writes ten to shared `$0DEC`; gamma increments the same
  word on a qualifying D-pad edge and Flash decrements it every eighth NMI.
- A single Right edge at frame 16 makes ten missiles underflow to `$FFFF` at
  frame 96. Holding Right has the same result: held input is not repeated input.
- Alternating Left/Right reaches the release threshold of 60. Release replaces
  the Flash movement pointer with normal movement.
- Grab during Flash replaces Flash movement with RTS and clears shared `$0DEC`;
  ammunition remains unchanged. Gamma and the Flash palette keep running.

These properties have native executable assertions, not wiki-only expectations.
Only numeric state is retained, with no ROM, SRAM, graphics or player snapshot.

## Port reproduction

The production runtime admission audit originally failed both orders in both facings:

- Flash while grabbed throws because the Draygon movement handler demands a
  grabbed pose, but native admits the Flash pose and executes Flash movement.
- Grab during Flash leaves `CrystalFlash.Phase=DrainingAmmo`, although native
  replaced its movement pointer with RTS. Current dispatch masks that stale owner
  while grabbed; it must not resume after release.

The independent `EscapeButtonCounter` and `AmmoDecrementTimer` also lacked the
native shared-word coupling. Writes now publish to both semantic views; the
counter audit compares both against every native frame, including each decrement,
reload, directional increment, wrap and release.

Corrections:

- Crystal Flash movement can replace the grab's RTS movement while grab gamma
  remains active. Gamma/actor placement no longer impose a held-pose invariant
  that the cartridge does not have. The ordinary held-pose movement dispatcher
  retains its own strict pose check.
- Grab and release relinquish the old Flash movement phase, without clearing
  the independently running special palette/shine timer.
- Flash entry/decrement/reload and grab entry/increments keep both views of
  shared `$0DEC` synchronized. Release tests the signed difference against 60
  only after a newly counted D-pad edge, matching the cartridge branch.

The four runtime admission cases pass. The counter-only comparison passes all
1,920 frames. It executes the real Flash beta and Draygon gamma methods; it does
not claim to compare the trace's animation/palette or full runtime trajectory.

```powershell
cmd /c '"C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat" && csharp\native\DraygonCrystalAudit\build.cmd'
cmd /c 'csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" > csharp\test-temp\draygon-crystal.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --draygon-crystal-admission-audit "Super Metroid.smc"
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --draygon-crystal-counter-audit "Super Metroid.smc" csharp/test-fixtures/issue-431-draygon-crystal/native.csv
```

## Full runtime continuation

`runtime.csv` extends all sixteen cases to 430 frames (6,880 total), running
native input, the installed movement handler, gamma, animation, pose transitions,
collision and palette phases. The port runs `SuperMetroidRuntime.StepFrame`.
LF-normalized SHA-256:
`E2F54300208FAA5BEF8F4D978FC2064900D2B04EE913432020D15785239487F7`.

This reproduced three further defects: completed Flash movement remained blocked
by an active grab gamma; release incorrectly restored pose input before the native
RTS input handler was replaced; retained Flash timers could not launch a spark
because admission required the abstract stored-charge phase. Movement ownership
and input-handler lifetime are now independent, and spark admission reads the
live shared shine timer. Launch replaces Flash movement and palette ownership.

All 6,880 frames match position/subposition, vertical velocity, pose/animation,
health/ammunition and palette/timer state. Both facings and entry orders with
alternating input launch a damaging, energy-consuming vertical spark at frame
363, without equipping Speed Booster or injecting a stored charge. Both native
and port audits assert that result explicitly. The other input patterns remain
controls. This still uses prepared entry callbacks, not boss flight or drops.

```powershell
cmd /c 'csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" runtime > csharp\test-temp\draygon-crystal-runtime.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --draygon-crystal-runtime-audit "Super Metroid.smc" csharp/test-fixtures/issue-431-draygon-crystal/runtime.csv
```

## Escape arithmetic and input boundaries

`edges.csv` executes the native gamma handler in 12,288 prepared cases: both
facings, grapple locked/unlocked, all sixteen previous and newly pressed D-pad
patterns, and shared-counter seeds 0, 1, 58, 59, 60, 61, $7FFF, $8000, $803A,
$803B, $FFFE and $FFFF. Every input also contains Shoot to check D-pad masking.
The port calls its production escape handler with matching prepared values.
LF-normalized SHA-256:
`192094A1D48791F91D58CFB096FF05E99CF0F1DB585D8340D92E4AD7FC43B414`.

All 12,288 cases match counter, last counted pattern, active/released state,
pose and prospective-input suppression; the two shared counter views remain
equal. These seeds deliberately exercise native signed subtraction and wrap,
not a claim that every seed is naturally reachable during the technique.
No further production discrepancy was found in this matrix.

```powershell
cmd /c 'csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" edges > csharp\test-temp\draygon-crystal-edges.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --draygon-crystal-edges-audit "Super Metroid.smc" csharp/test-fixtures/issue-431-draygon-crystal/edges.csv
```

## Ten-capacity Power Bomb/refill prerequisite

`refill.csv` contains 80 intermediate inventory/admission comparisons across
sixteen cases (both facings, eight setups). Native executes real placement
`$90:BF9D`, pickup collision `$86:EFE0`, and cleanup `$88:8B4E`; the port executes
the corresponding production routines and advances its placed bomb through fuse,
explosion and cleanup. No ammunition is injected after initialization.
LF-normalized SHA-256:
`4A6BCFCBB8FE018A61139A83A2E93BD78ACA02D3207ADA66B510FB8C54D3834C`.

Initial position is (256.0000, 400.0000), stationary Morph Ball, Morph Ball
equipment only, no beams, 49/99 health, no reserve energy, ten missiles/supers,
and selected Power Bomb. Cheats are not enabled. An existing PB pickup is
prepared at the same position (or 100 pixels away for the missed-drop control),
with radius 5 and lifetime 400; no RNG or death/drop generation is asserted.
Shoot places the bomb, then Draygon's actual grab entry runs and the exact
Down/L/R/Shoot chord is held through cleanup. The grab does not alter coordinates.

- Ten capacity, no drop: placement leaves nine; Flash rejected.
- Drop before placement: refill caps at ten, then placement leaves nine; rejected.
- Drop after placement: nine becomes ten; Flash starts while grabbed.
- Distant drop: collision misses, leaving nine; rejected.
- Nine capacity: placement leaves eight, refill caps at nine; rejected.
- Eleven capacity without a drop: placement leaves ten; successful control.
- Ten with refill but extra Jump held: rejected.
- Ten with refill but one-pixel displacement from bomb origin: rejected.

All five recorded stages match ammunition and pose/admission in every case.
This is an inventory/admission comparison, not a native explosion-duration or
rendering comparison: native invokes cleanup at the corresponding boundary,
whereas C# advances the actual bomb to it. Boss flight and random drop generation
are deliberately outside this isolated prerequisite fixture. The earlier runtime
matrix covers post-admission movement, release and usable retained spark.

```powershell
cmd /c 'csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" refill > csharp\test-temp\draygon-crystal-refill.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --draygon-crystal-refill-audit "Super Metroid.smc" csharp/test-fixtures/issue-431-draygon-crystal/refill.csv
```

These results apply to the pinned Japan/USA cartridge, not a PAL parity claim.
Keep #431 open with `awaiting-player-validation` until player confirmation.
