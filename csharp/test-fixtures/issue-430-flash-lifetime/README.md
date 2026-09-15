# Shinespark Suit lifetime (#430): X-Ray cancellation

Partial coverage; keep #430 open without awaiting-player-validation until the
other requested lifetime, cues, recharge, Flash and save/reload cases are covered.

This extends the verified #431 generator, not a permanent debug award. Both
facings and both grab/Flash entry orders use alternating directional inputs,
escape and land. Frames 300 through 350 have no input. At the end of frame 350,
the native X-Ray admission and interrupted-pose dispatcher run. The port calls
the corresponding production admission on the state produced by StepFrame.
The caller represents an already-selected X-Ray item, as the native entry does;
HUD selection itself is not claimed by this isolated cancellation test.

Native HDMA runs before each frame, allowing Flash's actual bubble program to
finish before X-Ray admission. Without this phase, native correctly rejects
X-Ray because the fixture leaves the explosion status active. No status reset
is injected. The original shorter #431 movement traces remain unchanged.

Initial state and entry/input sequence otherwise match
`../issue-431-draygon-crystal/README.md`. No gameplay cheats, player snapshots or
RNG are used. Original unpatched Japan/USA ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
`xray.csv` LF-normalized SHA-256:
`84F6CA152C32A090EA394E31B77568839A2F1C9CFD6F1869471BDAB4FA523AD2`.

## Reproduced defect and correction

All four cases originally diverged at frame 350: native installed palette 8 and
cleared the live shine timer to zero, while the port installed X-Ray but retained
Flash's timer at 5. X-Ray relinquished only the ordinary shinespark owner, leaving
the interrupted Flash owner alive. The production activation now replaces both
Flash movement and palette ownership, following $91:EEA6.

All 1,404 compared frames now match position/fractions, vertical speed, pose and
animation, inventory/health, palette and the live shine timer. The native harness
also explicitly asserts frozen time, palette 8 and timer zero after activation.
This establishes cancellation at activation, not the rest of #430's scope.

```powershell
cmd /c '"C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat" && csharp\native\DraygonCrystalAudit\build.cmd'
cmd /c 'csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" xray > csharp\test-temp\flash-xray.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --flash-xray-cancellation-audit "Super Metroid.smc" csharp/test-fixtures/issue-430-flash-lifetime/xray.csv
```

The fixture executes original CPU/HDMA logic without claiming rendered-window,
PAL-revision, or save/reload coverage. Only numeric state is committed.
