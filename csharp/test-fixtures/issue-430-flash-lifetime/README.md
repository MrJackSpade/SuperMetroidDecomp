# Shinespark Suit lifetime (#430): X-Ray cancellation and recharge

Partial coverage; keep #430 open without awaiting-player-validation until the
other requested lifetime, cues, Flash and save/reload cases are covered.

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

## Recharge replaces retained Flash

`recharge.csv` uses the same verified generator with Speed Booster equipped,
starting X=1152 so both directions remain on the runway. After release/landing,
frames 300..349 have no input. Mode 2 holds forward+Dash for frames 350..489,
then Down+AimUp at 490 to store an earned charge. Mode 3 stops at frame 390,
before reaching stage four. Both release input afterward through frame 699.
Native HDMA and the port's production audio publication run: the echo queue's
accumulator return affects running cadence and must not be omitted by a fixture.

Before the fix, all four successful runs first diverged at crouching: native
replaced palette 7/timer 5 with palette 1/timer 179, while the port kept the
interrupted Flash owner. Successful charge storage now relinquishes Flash's
palette ownership. An insufficient stage does not relinquish it.

All 5,600 frames across eight cases match the same fields as the runtime audit.
Native and port explicitly assert the successful recharge expires to zero by
frame 699, while the insufficient-charge control still has the looping Flash
timer (1 on that frame). This also checks retained Flash survives forward Dash
until an actual charge is stored. No charge counter is injected.

LF-normalized trace SHA-256:
`65BB9B69CBCDC1A28E9F0243D5BAE9D43053D6F95AE676F27040F7C535A5D368`.

```powershell
cmd /c 'csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" recharge > csharp\test-temp\flash-recharge.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --flash-recharge-audit "Super Metroid.smc" csharp/test-fixtures/issue-430-flash-lifetime/recharge.csv
```

Remaining: visual cues, sand/Blue-Suit differences, further lifetime/use cases,
fresh Crystal Flash cancellation, and save/reload. The finite retention control
does not by itself prove indefinite lifetime. Keep the ticket in progress.

## Idle lifetime and invincibility body cue

`lifetime.csv` follows all four verified entry/facing cases for 1,000 frames.
There is no input after frame 300. During frames 300..999, Flash movement is
inactive while its palette timer repeats in 1..5. Native $91:DB93 decrements
that nonnegative word and reloads five at zero; the palette loop has no elapsed
duration limit. Termination requires another writer/handler, not waiting out
an ordinary stored-charge timeout. This source invariant plus the 700-tick
continuations establishes the idle loop; it does not assert immunity to every
other game event.

Every continuation frame also executes the actual native body drawing routine
and the port's `SamusState.Draw` with invincibility=100, zero knockback and a
visible camera position. A counterfactual second draw changes only the shine
timer to zero, then restores it before the next frame. Both outputs match:
retained Flash emits body OAM on odd and even frames, while the zero-timer
control emits none on odd frames. This is the body cue, not an arm-cannon or
beam-color assertion. The fixture introduces invincibility for the draw test,
not a claim to reproduce enemy contact or damage.

All 4,000 state frames and 2,800 draw pairs match without another production fix.
LF-normalized trace SHA-256:
`9208A09FB48468802B707F291507E3C04C9BBB2C0F154FB597F8B609DF1AEAD3`.

```powershell
cmd /c 'csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" lifetime > csharp\test-temp\flash-lifetime.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --flash-lifetime-audit "Super Metroid.smc" csharp/test-fixtures/issue-430-flash-lifetime/lifetime.csv
```

Still outstanding: the gray-beam cue, sand/Blue-Suit distinctions, fresh Flash
and save/reload cancellation. Keep #430 open and in progress.

## Fresh Crystal Flash cancellation

`repeat.csv` runs the verified generator, idles from frame 300, then invokes
the real Flash admission callback at the end of frame 350 with its exact
Down/L/R/Shoot chord. The ordinary frame input remains neutral: this isolates
the admission callback rather than pretending to compare an additional bomb
placement, refill, or unrelated Shoot/aim transition. The #431 refill fixture
covers that separate bomb-cleanup prerequisite.

The grab-during-Flash order still has 49 health and ten of each ammo, so the
new Flash is admitted. It runs to completion, consumes the ammo, and clears
the retained shine timer. The Flash-during-grab order has already restored
health and depleted missiles; its rejected attempt leaves the old timer
looping. No resources or movement phases are reset to force success.

All 3,200 frames across both orders/facings match pose/animation, movement,
resources and palette/timer state. Native and port explicitly assert admission
and final cancellation versus retained-state controls. No production change
was needed. LF-normalized trace SHA-256:
`1A8779EB4D6E56D2C7EF17E5ACE3DFA12161FB55BBD8EBBADBEA920BE233275D`.

```powershell
cmd /c 'csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" repeat > csharp\test-temp\flash-repeat.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --flash-repeat-audit "Super Metroid.smc" csharp/test-fixtures/issue-430-flash-lifetime/repeat.csv
```

Remaining: gray-beam cue, sand/Blue-Suit distinctions and save/reload cancellation.

## Sand versus Blue Suit

`sand.csv` extends the verified generator through frame 350, then installs one
body-overlap sample and runs the real bank-$94 inside-block dispatcher in Maridia.
Both entry orders/facings cover eight geometries: no special block, surface sand
at feet, surface sand at head, submerging sand, slow sandfall, fast sandfall,
surface sand reached through a horizontal extension, and a solid-block control.
No extra beta movement is claimed after this isolated body-sampling boundary.

All 11,232 state frames match, including the boost counter and extra vertical
displacement written by the reaction. Native explicitly asserts palette 7 and
shine timer 5 survive every sample. Actual displacement distinguishes executing
the sand reaction from accidentally skipping it. No production fix was needed.

The corresponding existing `--temporary-blue-sand-audit` matrix (6,416 frames)
still passes: surface sand clears the earned Blue Suit boost counter there,
while the retained Flash palette/timer survives here. The two states are not
interchangeable. The tests share only geometry preparation; their assertions
remain specific to the independently generated states.

LF-normalized trace SHA-256:
`8BEC4E7921B8B1B76DFFBC93CA970F8ECEFB7D3596DA69CE21D72C4C37B3C43C`.

```powershell
cmd /c 'csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" sand > csharp\test-temp\flash-sand.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --flash-sand-audit "Super Metroid.smc" csharp/test-fixtures/issue-430-flash-lifetime/sand.csv
```

Remaining: gray-beam cue and save/reload cancellation. Keep #430 in progress.
