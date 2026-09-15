# Draygon / Crystal Flash counter and ownership investigation (#431)

Status: admission/ownership and shared-counter defects reproduced and corrected.
Both audits below now pass. This is partial #431 coverage, not completed
technique parity or player validation; remaining requirements are listed below.

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

Remaining #431 requirements include full runtime/native trace comparison,
released spark usability, additional directional patterns and exact threshold
arithmetic, and the ten-capacity Power Bomb/refill setup. Keep the issue open and
without `awaiting-player-validation` until these are handled.
