# Draygon / Crystal Flash counter and ownership investigation (#431)

Status: reproduced, not fixed. The admission audit intentionally exits nonzero
until the production ownership defects are corrected. Do not treat this fixture
as completed technique parity or player validation.

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

The production runtime admission audit reproduces both orders in both facings:

- Flash while grabbed throws because the Draygon movement handler demands a
  grabbed pose, but native admits the Flash pose and executes Flash movement.
- Grab during Flash leaves `CrystalFlash.Phase=DrainingAmmo`, although native
  replaced its movement pointer with RTS. Current dispatch masks that stale owner
  while grabbed; it must not resume after release.

The independent `EscapeButtonCounter` and `AmmoDecrementTimer` also lack the
native shared-word coupling. A full frame comparison must be added as that
coupling is implemented, including counter cadence and inventory underflow.

```powershell
cmd /c '"C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat" && csharp\native\DraygonCrystalAudit\build.cmd'
cmd /c 'csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" > csharp\test-temp\draygon-crystal.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --draygon-crystal-admission-audit "Super Metroid.smc"
```

Remaining #431 requirements include full runtime/native trace comparison,
released spark usability, additional directional patterns and exact threshold
arithmetic, and the ten-capacity Power Bomb/refill setup. Keep the issue open and
without `awaiting-player-validation` until these are handled.
