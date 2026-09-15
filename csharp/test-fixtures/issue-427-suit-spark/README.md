# Suit collection interrupting windup (#427)

This native-CPU fixture covers the complete Varia/Gravity transformation while
shinespark windup is suspended, its release, and subsequent X-Ray admission.
The bomb-jump alternative is covered below; Crystal Spark heights are covered
in `../issue-427-crystal-spark`.
The release matrix below additionally checks
X-Ray teardown and reuse of the retained boost.

## Regeneration

```powershell
cmd /c '"C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat" && csharp\native\TemporaryBlueSuitAudit\build.cmd'
cmd /c 'csharp\native\TemporaryBlueSuitAudit\audit.exe "Super Metroid.smc" suit-spark > csharp\test-temp\suit-spark.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --suit-spark-audit "Super Metroid.smc" csharp/test-fixtures/issue-427-suit-spark/native.csv
```

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Trace SHA-256 (LF-normalized UTF-8):
`3FB90B1CBCD167CFCFA1141679635392F2069F53A171322D36D0B96B2BCEE40F`.

Sixteen cases: both facings, Varia/Gravity, no input / Run+forward /
Run+backward / Run alone. Each has 330 frames. A controller-earned windup is
interrupted after frame 151 at the real post-message suit routine. Equipment is
granted as the preceding PLM would do. The capsule and message wait are outside
this fixture; no movement state or boost counter is injected.

The native side executes the actual ROM setup and HDMA pre-instruction plus
locked alpha while collection is active. The port calls its production suit
owner and normal runtime frame. Both retain the underlying movement pointer,
execute no beta during collection, then restore beta when HDMA finishes.

Every frame compares X/Y including fractions, pose, animation, base/extra speed,
boost/contact, palette timer/type, Y speed/direction, suit lifetime/substate,
equipment, windup timer and freeze state. While the suit owns the window, also
compare its full 256-word window checksum, beam position, widening speed, and
RGB color-math words. After X-Ray admission, the shared window no longer belongs
to the suit; window comparisons stop, but Samus/state comparisons continue.
The fixture does not execute X-Ray DMA or validate its window rendering.

## Reproduced corrections

- Frame 152: the port advanced special movement, animation, and palette during
  an empty-beta suit lock, eventually crashing cleanup on pose `$9B`. Runtime
  now suspends beta and palette while retaining the original movement owner.
- Frame 214: widening omitted the native exact-zero clamp, delaying reveal.
  Zero now takes the same full-width branch as a negative left endpoint.
- Frame 216: shrinking erased one extra lower scanline. Native's bottom wipe
  uses position minus one, unlike the top; both loops now retain that asymmetry.
- Frame 321: the X-Ray previous-movement snapshot was taken after beta, admitting
  X-Ray one frame too early. It now reflects the end-of-alpha snapshot `$90:EB30`.
- X-Ray admission used to replace movement and animation in alpha. It now
  freezes time there but commits its interrupted pose after movement/animation,
  preserving the old windup's last tick and the new pose's first animation tick.
- Frame 321: the one-pixel prospective-running collision move was restricted to
  normal grounded beta. Native performs it for a retained standing pose under
  special movement too; that shared pose check now does likewise.

Original-CPU assertions confirm that Run+direction installs X-Ray at frame 322
while preserving boost `$0400`. Port assertions also require the previous windup
owner to have relinquished movement. All 5,280 compared frames match.

Only numeric trace data is published; no ROM, SRAM, graphics or player state.

## X-Ray release and boost reuse

`release.csv` extends the Run+direction cases to 430 frames: eight cases across
both facings, both turn directions, and both suits (3,440 matching frames).

```powershell
cmd /c 'csharp\native\TemporaryBlueSuitAudit\audit.exe "Super Metroid.smc" suit-release > csharp\test-temp\suit-release.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --suit-release-audit "Super Metroid.smc" csharp/test-fixtures/issue-427-suit-spark/release.csv
```

Release trace SHA-256 (LF-normalized UTF-8):
`44FA7575F8D4CFD542AAC07C7ABF57C5A2EB870ABAEC111E7F085E6BC2C06DB3`.

The window setup is isolated: before frame 330, the port advances only its X-Ray
window owner to the first restoration boundary, without modifying Samus's
movement or boost. Native frames 330/331/332 execute `$88:8934`, `$88:89BA`, and
`$88:8A08` before alpha; the runtime executes the corresponding teardown stages.
This does not test elapsed window-setup timing, VRAM transfer contents or rendering.

Both implementations assert normal movement and retained boost at frame 332;
Down stores a fresh 179-frame charge at frame 390, without another run-up.
Jump/Up then launches a new spark: frame 403 has vertical speed `$0007:1C00`,
contact damage index 2 and health 98. No boost or velocity is injected.
All earlier movement/animation comparisons continue. After teardown, suit
substate is excluded because native `$0DEC` is scratch reused by the subsequent
spark (`$90:CFFA` writes 7); it is no longer the inactive suit owner's state.

This extension verifies the preceding gameplay corrections; it required no new
production change. The original suit and Crystal Spark matrices also pass.

Regression checks passed: 169,332 existing temporary-boost, Draygon and Crystal
Spark frame comparisons; the complete bank-$80 verification executable; and
Windows Desktop Release build with zero warnings/errors.

## Bomb interruption without X-Ray

`bomb.csv` has 16 cases, 430 frames each (6,880 matching frames): both initial
facings, both suits, centered/left/right bomb hits and the first missing pixel.
X-Ray is not equipped or selected. The same controller-earned windup is suspended
through suit collection. On the first unlocked frame (314), construct only a
timer-eight bomb overlap, as in the existing hurt/bomb fixture. The production
bank-$A0 interaction pass publishes its direction; normal beta and bank-$91
command three consume it. Clear the collision stimulus on the next frame.
This isolates the bomb's collision boundary, not its placement or fuse animation.

The front-facing suit pose has X radius 5, the bomb radius 8: offsets 0 and
plus/minus 12 hit; offset 13 misses. A ceiling at block row 16 catches the missed
control's uninterrupted spark within valid room geometry. It does not affect
the successful bomb arcs. No boost, velocity or movement-handler state is injected.

Explicit assertions require bomb-start ownership at frame 314, normal movement
at 343, retained boost at 389, a new stored charge at 390, and a damaging,
energy-consuming second spark at 403. Every frame compares the same movement,
pose, animation, palette, boost and windup fields. Shared suit scratch is also
checked directly against native while the suit owns it (151–314). After release,
the inactive suit's separate window/substate is not compared to re-used scratch.

### Reproduced corrections

- Successful hit, frame 343: the port resumed old windup after the bomb arc.
  Command three now relinquishes the previous spark movement/input ownership,
  while retaining the independently running palette and boost words. The bomb
  handler subsequently returns to normal movement just as `$91:EE80` specifies.
- Missed hit, frame 344: the port accelerated the resumed spark by 7.109375,
  versus native 0.109375. Suit HDMA overwrites shared `$0DEC/$0DEE`; the separate
  port owners had preserved stale spark acceleration. Suit entry and every HDMA
  step now publish those writes into the spark's acceleration pair, including
  the zero pair at teardown. The resulting uninterrupted trajectory now matches.

```powershell
cmd /c 'csharp\native\TemporaryBlueSuitAudit\audit.exe "Super Metroid.smc" suit-bomb > csharp\test-temp\suit-bomb.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --suit-bomb-audit "Super Metroid.smc" csharp/test-fixtures/issue-427-suit-spark/bomb.csv
```

Bomb trace SHA-256 (LF-normalized UTF-8):
`44A748121B150A2C522001EA5B3E715AED1FD93EEC040C2387DA11306D5170FC`.
The completed bank-$80 verification executable and Windows Release build pass.
Suit/X-Ray/Crystal, temporary boost and Draygon matrices remain regression gates.
