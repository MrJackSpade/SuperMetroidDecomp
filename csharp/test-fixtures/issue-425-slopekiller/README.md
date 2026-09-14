# Slopekiller controller-sequence parity (#425)

Result: **36 cases, 5,400 frames, zero native/port mismatches**. This is a
controller-earned soft unmorph followed by slope traversal, not a test that sets
nonzero vertical speed on an already-running Samus. No gameplay fix was needed.
The issue remains open for player validation.

## Reproduction

The constructed room is 144 by 80 blocks. It has a flat floor at Y=256,
followed by 45-degree descending slopes (BTS $52 rightward, $12 leftward).
Samus starts as a falling Morph Ball, with zero fractional position and zero
velocity, Morph Ball only, 99 energy, and no beams, cheats, enemies or liquids.
The native harness begins with cleared WRAM. RNG is not relevant to this fixture.

The complete matrix uses:

- Both facing/travel directions.
- Starting Y=201, 202 or 203.
- A one-frame Up press at frame 19, 20 or 21 (zero-based).
- Starting X=1024 or one pixel further in the travel direction.
- No further inputs until frame 60, then held forward through frame 149.

Y=202 and Up at frame 20 retain Y speed `$0002.F400` after reaching the
crouched floor position. The neighboring heights and input timings clear it.
Both outcomes are produced by original cartridge instructions, not seeded flags.

The rightward X=1024 successful case remains running throughout frames 92–110,
moving exactly `$0002.C000` pixels each frame while retaining `$0002.F400` Y
speed. At frame 92, the Y=203 control instead moves `$0002.1000`, the normal
three-quarter slope multiplier. Moving the successful starting X one pixel
right loses the retained state earlier: it is falling by frame 110. Leftward
cases are independently compared, not assumed symmetric or asserted to be
successful just because the rightward case is. The trace includes subsequent
loss of retained speed and ordinary falling/landing, through frame 149.

The checker compares every frame's whole/fractional X and Y, pose, movement
type, animation frame/timer, base/extra speed, acceleration mode, both Y-speed
words, Y direction and incoming Y speed. It also has explicit success and
adjacent-failure assertions independent of the trace's expected strings.

## Run the regression

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --slopekiller-comparison-audit 'Super Metroid.smc' `
  csharp/test-fixtures/issue-425-slopekiller/native.csv
```

## Rebuild native evidence

From an x64 Visual Studio developer command prompt at the repository root:

```bat
csharp\native\SlopekillerAudit\build.cmd
csharp\native\SlopekillerAudit\audit.exe "Super Metroid.smc" > new-native.csv
```

This dedicated console runner executes the original, unpatched ROM with the
existing 65816 CPU implementation. It does not start SDL or create dialogs. It
uses instruction budgets and rejects unmapped accesses. The hardware arithmetic
adapter exposes completed multiply/divide results; this is not a scanline or
intermediate-hardware-latency test. Each frame executes native input, interaction,
movement, animation, transition, pose/collision and bookkeeping entry points.
The managed side uses `SuperMetroidRuntime.StepFrame` with the same room/setup.

The native entry identities are in `csharp/native/SlopekillerAudit/fixture.h`.
The collision multiplier/early-return path is `$94:84D6`; the pinned disassembly
and `upstream-sm/src/sm_94.c` agree that either nonzero Y-speed word bypasses it.
The base-sub-speed modifier arithmetic nearby stores no result; it is not an
omitted extra speed adjustment.

Identities:

- Japan/USA ROM SHA-256:
  `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`
- Trace SHA-256 after CRLF-to-LF normalization, UTF-8:
  `EF831726BEC46443B192F7267BE86A4AFC45D0D8CF4BBF785CF2275CF5AA1C1C`

Only diagnostic source and numeric expectations are published. No ROM, SRAM,
embedded snapshot, extracted artwork or player save is included or modified.
PAL and whole-playthrough timing are not covered. This is a room-local technique
comparison, not a claim about every possible slope, setup or host controller.

## References

- [Horizontal Speed / Slopes](https://wiki.supermetroid.run/Horizontal_Speed#Slopes)
- [TAS author discussion of perfect soft unmorph prerequisites](https://tasvideos.org/Forum/Topics/18579?CurrentPage=4&Highlight=443814)
- Pinned `upstream-disassembly/src/bank_94.asm`, routine `$84D6`.

The written descriptions were diagnostic leads; expectations above come from
executing the pinned cartridge, not from those descriptions.
