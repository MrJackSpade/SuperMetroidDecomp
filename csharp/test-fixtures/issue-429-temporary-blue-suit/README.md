# Controller-earned temporary Blue Suit: initial retention

The initial matrix covers the run/crouch/angle-held expired-charge
method, partial counter retention and no-angle cancellation. It complements the
existing [speedball-unmorph evidence](../movement-release/TEMPORARY-BLUE.md) from
#470. The linked matrices below now cover the remaining #429 scope and are ready
for player validation. Earlier documents retain their historical partial-result
notes; the current coverage summary is [COMPLETION.md](COMPLETION.md).

Subsequent fixtures add [soft-unmorph carry](CARRY.md) and
[ordinary/Spring Ball bounce retention](BOUNCE.md). Those documents record the
additional reproduced fixes and the remaining issue scope.
The [Dash/equipment matrix](CANCELLATION.md) records native cancellation and
non-cancellation conditions after the charge has expired.
The [sand sampler matrix](SAND.md) distinguishes surface cancellation from
submerging sand and sandfalls, including an extension-tile path.
The [terrain collision matrix](TERRAIN.md) checks actual speed/bomb tile breaking
and movement admission using full and partial controller-earned counters.
The [five-cycle chain matrix](CHAIN.md) repeats complete soft-unmorph carries
without resetting Samus and includes adjacent failure timings.
The [equipment-menu matrix](MENU.md) drives real menu input and native teardown,
distinguishing disabled-on-unpause cancellation from an isolated equipment write.

## Reference and execution

- [Technique setup](https://wiki.supermetroid.run/Blue_Suit_Glitch#Temporary).
- [Bank $91 palette/charge handling](https://patrickjohnston.org/bank/91#fD6F7).
- Pinned local `upstream-sm/src/sm_90.c`: movement `$90:973E..9813` publishes
  boost contact damage; animation `$90:852C` advances the boost stage but does not
  publish damage. Its upstream C sound-call accumulator repair is **not** used.
- `csharp/native/TemporaryBlueSuitAudit` executes unpatched original 65816
  instructions through the shared console CPU adapter. It includes native input,
  interaction, movement, animation/pose transitions and `$91:D6F7` palette/charge
  ticking. No native GUI, translated C gameplay, hardware renderer or audio DSP
  participates. Native queue mutation is preserved; the C# comparison uses the
  actual `GameplayAudioFramePublication.QueueEcho` path, not the optional deferred
  callback fallback. No sound drain is simulated during this bounded sequence.

ROM SHA-256 (unheadered Japan/USA):
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.

`native.csv` SHA-256 after CRLF-to-LF normalization:
`017B7A761CA8CBDA8BA14CAF7886F5FFC4760B869C9FD6BFA8C37C91AE6A8E39`.
Only numeric observations are tracked; no ROM, SRAM, state or artwork is included.

## Deterministic setup

Synthetic 144x80-block room, solid floor on row 32 (Y=512), no enemies, other
terrain, liquid or cheats. Samus starts standing at Y=491, X=200 facing right or
X=2100 facing left, zero fractional positions/speeds, 99 energy, Morph Ball and
Speed Booster only. Animation starts frame zero/timer one. Buttons use native
defaults: B=run, A=jump, R/L=aim. The fixture records zero-based frames after the
complete movement/animation/charge pass.

32 cases: both facings, run lengths 60/100/140/180, then no aim/R/L/both. Hold
forward+B until the stop frame. On that frame replace them with Down plus the
selected aim input. Thereafter hold only the selected aim input through frame399.
Nothing assigns boost or charge state directly; both are earned by controls.

## Assertions and discrepancy

All 12,800 frames compare exact X/Y including subpixels, pose, animation frame and
timer, base/extra speed, boost counter, contact damage, charge timer/palette type,
vertical speed and direction. Independent semantic gates also assert:

- Full boost first becomes `$0401` on frame89, but contact remains zero; movement
  publishes contact one on frame90. This was the reproduced production defect:
  the port published damage in the animation callback one frame too early.
  Removing that extra publication leaves the native movement owner responsible.
- Full-boost crouch starts at 179 after that frame's native palette tick. Charge
  expires on `stop+179`; held aim preserves the boost counter past expiration.
- At frame399, no-angle controls have counter zero. Angle controls retain `$0201`
  after a 60-frame run, `$0402` after100, and `$0401` after140/180, all with expired
  charge. Thus the shorter-run control retains a partial counter, not full boost.

The first comparison lacked the frontend sound callback and mismatched cadence.
After correcting that fixture omission, exactly 24 frames differed, each the
first full-boost frame in a long-run case. The production ordering correction
reduces this matrix to zero differences. This is not a claim of pixel/palette
color parity or complete temporary-Blue-Suit technique coverage.

## Reproduce

From a Visual Studio x64 Native Tools prompt at the repository root:

```powershell
csharp\native\TemporaryBlueSuitAudit\build.cmd
& csharp/native/TemporaryBlueSuitAudit/audit.exe 'Super Metroid.smc' > csharp/test-temp/temporary-blue-native.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --temporary-blue-suit-audit 'Super Metroid.smc' csharp/test-fixtures/issue-429-temporary-blue-suit/native.csv
```

The comparison rejects changed trace identity, incomplete matrices and altered
input ordering. It runs the full production `SuperMetroidRuntime.StepFrame` on
constructed room geometry rather than asserting a helper's output in isolation.

Verification passed: two identical native captures, all 12,800 comparison frames
and semantic assertions, full Release Verification, and the Windows Release build
with zero warnings/errors. The exhaustive cadence test now also requires the
animation callback to leave contact damage unchanged at zero.
