# Ordinary bomb-chain parity (#412)

## Ceiling traversal candidate search

`BombTraversalSearch` uses ordinary `StepFrame` with controller-placed bombs in a
constructed Landing Site-width runway. Floor is row 16, ceiling row 0 or 12,
Samus (128,249), grounded right-facing Morph Ball, zero subpixels, Morph Ball and
Bombs only, no cheats; enemies are cleared as in the earlier synthetic fixtures.
Bombs are placed at frame 0, frame 52, then every N frames (24 through 30;
the expanded negative search also includes 48 through 58).
After frame 170 the controller steers toward the next bomb with fuse >=9,
plus a rightward offset, with a three-pixel neutral band. This is an exploratory
controller policy, not production behavior or native expected input.

The low-ceiling N=24, offset=4 candidate runs 600 frames, produces 22 launches,
ends at (192,218), and has 18 ceiling contacts and no floor contacts after frame
170. N=24/offset=5 and N=25/offset=4,5 also pass that search filter. High-ceiling
cases do not. Steering toward the newest bomb and zero-width steering dead bands
did not sustain the chain. No gameplay changes were made to obtain these results.

Run with a NEW output directory (existing files are never overwritten):

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --bomb-traversal-search "Super Metroid.smc" OUTPUT_DIRECTORY
```

The search emits exact 600-frame input CSVs for candidates. Two independent runs
produced byte-identical files. N=24/offset=4 input SHA256:
`C12496F859EFA52FFDB8DD576958EF449F38FFB3A4A702152746398C948D8026`.
The retained generator makes these inputs reproducible without a player state.

The N=24/offset=4 fixed sequence now has the native comparison below. Other
search candidates are still only managed observations.

## Fixed-input ceiling traversal: original-cartridge comparison

`native-bomb-traversal-probe.h` reads the preserved 600 inputs rather than running
the search policy. It executes the pinned unpatched cartridge's input, bomb
production, full projectile processing, bomb overlap, movement, animation and
pose-transition routines. The synthetic 144x80-block room has solid ceiling row
12 and floor row 16. This is the same bounded gameplay slice as the earlier bomb
chain probes; unrelated room graphics, enemies and camera tracking are omitted.
Each facing gets fresh CPU/WRAM. The left case swaps only Left/Right controller
bits and starts in the corresponding grounded-ball pose; it does not mirror
observed positions or invent a corrected trajectory.

All 1,200 frames match C#, including full 16.16 X/Y and speeds, pose, bomb
direction/count, input lock, start/main bomb movement ownership, and all five
slots' type/fuse/position/list/timer/spritemap fields. The actual native handler
pointers determine the three ownership flags. Both cases produce 22 launches,
18 ceiling contacts, and no floor contact after frame 170. Endpoints are exactly
X=$00C0.3000 (right) / $003F.D000 (left), Y=$00DA.1000. Dedicated assertions
require those traversal properties in addition to every native frame comparison.
No production change was needed.

`ceiling-traversal-bomb-412.zip` contains only the fixed input and numeric native
trace. Native capture was independently repeated; both trace hashes and the
extracted archive payload agree:
`0EF1E49E4EAC991804D0575F6E79F7FAAC2437060D956049B87CBEF3FA077309`.
The input hash remains the one above, including after extracting shared setup to
`BombTraversalFixture`. No player slots, ROM bytes or rendered assets are stored.

After extracting to a new directory:

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --bomb-traversal-comparison "Super Metroid.smc" EXTRACTED/bomb-traversal-ceiling-True-interval-24-offset-4.csv EXTRACTED/bomb-traversal-412-native.csv
```

Regeneration uses the same pinned sources below: temporarily include
`native-release-probe.h` then `native-bomb-traversal-probe.h` after `state_recorder`
in upstream `sm_rtl.c`; dispatch `DiagnosticBombTraversal(rom, inputs, newOutput)`
before SDL startup. Suppress explicit SDL dialogs on that headless branch. Force
Rebuild Release/x64 with PlatformToolset=v145; execute
`sm.exe --bomb-traversal-probe ROM INPUTS NEW_OUTPUT`. CPU calls are instruction-
bounded and output refuses overwrite. All temporary source hooks were removed
after both captures; normal native startup was not launched.

## Adjacent ceiling steering boundary

`BombTraversalBoundarySearch` varies only the first steering pulse's start and
duration, retaining all bomb timestamps and later controller input. The selected
adjacent case removes Right from frame 171: input $0100 becomes $0000, with every
other one of the 600 rows identical to the successful recording.

Original cartridge replay proves that this one-frame-shorter pulse fails only
rightward. It produces 11 launches, first returns to the floor on frame 282,
and has 207 floor-contact frames after frame 170. Final X/Y are $0007.4000 /
$00DC.3BFF. The mirrored leftward sequence still produces 22 launches without
floor contact, ending at $0041.5000 / $00DA.1000. C# matches all 1,200 records,
including the direction-dependent result. Explicit assertions preserve that
asymmetry rather than requiring a synthetic mirrored failure.

`ceiling-traversal-bomb-miss-412.zip` retains the changed input and native trace.
Two independent native captures and extracted payload hashes agree. Trace SHA256:
`8494900A5168D81508B9968AE9B526EDDCC9CAB4F86CE7800FF5B6CCDF53CCC1`.
Input SHA256:
`5DA568BB721036313DBF26657F145EB7BE86AA9877A04AA0C878323BAB982A69`.
Use the same native probe/regeneration procedure as the successful case.

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --bomb-traversal-miss-comparison "Super Metroid.smc" EXTRACTED/ceiling-pulse-170-1.csv EXTRACTED/bomb-traversal-412-native-miss.csv
```

Both archived comparisons pass (2,400 frames combined), with no gameplay change.
The nine-case local search is reproducible with `--bomb-traversal-boundary-search
ROM BASELINE_INPUT NEW_DIRECTORY`. A wider exploratory pulse at start 172,
duration 4 ran beyond this constructed room's left edge and hit the host's
outside-storage guard; it is not an accepted parity fixture. The retained local
search stays within the room and does not suppress that exception.

Remaining #412 acceptance: unconstrained horizontal traversal. The expanded
longer-interval feedback search has not established it. Do not mark the whole
issue ready based only on ceiling traversal; it remains open without the
validation label.

### Free-flight search limits

The retained free-flight search now covers 1,440 policies: bomb intervals
26/27/52/53/54, offsets 1..8, neutral bands 0..5 and steering starts
170/175/180/185/190/195. It stops a rejected branch on renewed floor support,
ceiling contact or departure from the safe search corridor. Candidates must
complete 600 frames, include at least six launches after steering starts, and
advance more than 32 pixels. These are search filters, not invented cartridge
rules or proof that every failed policy is impossible in native play.

No free-flight candidate met that filter. The four ceiling candidates and their
fixed input hashes remain unchanged. Additional exploratory attempts to delay
steering until bomb movement ended, brake throughout the rise, or predict a
future vertical interception did not produce a candidate either; those extra
heuristics are not retained as production physics or accepted parity tests.

Source inspection confirms the normal overlap dispatcher requires fuse 8 after
projectile processing, rather than an extended catch window. Successful short
sideways chains followed by a floor return are not counted as sustained flight.
The next investigation should use a demonstrated horizontal-bomb setup/input
reference instead of treating further variations of this feedback policy as
evidence that gameplay needs a change. #412 is still open, not deferred or ready
for validation.

The sections below record the earlier short-chain, repeated vertical-ascent,
three-bomb, ladder and steering matrices and the defects they exposed.

`native-bomb-chain-probe.h` executes unmodified cartridge instructions. The room
is 16 by 32 blocks, floor row 16, walls at columns 0/15, and ceiling at row 0 or
12. Samus begins in grounded Morph Ball at (128,249), zero subpixels/velocity,
99 health, Morph Ball + Bombs equipped, air, cheats off. Both facings are tested.

Every bomb is placed with Shoot input, not injected. The six schedules are a
single frame-zero bomb or three bombs at frames 0, N and 2N, with N in
40/44/48/52/56. Travel input is neutral, Left, or Right for frames 46–49.
The matrix contains 72 cases, 180 frames each, 12,960 total frames. It compares
position/subpixels, pose, bomb direction, vertical and horizontal velocities,
bomb count, and all five slots' type, fuse, position, animation-list pointer,
instruction timer and spritemap. These are explicitly bounded controller tests,
not a route through multiple rooms.

## Reproduced correction

Before the fix, the 24 diagonal/low-ceiling cases each differed for two frames
(48 mismatching frames). At fixture frame 74 the ceiling collision ends bomb
movement. Native retains horizontal speed $0000.3000 while the managed ordinary
no-input fallback cleared it. The native collision transition takes priority over
that fallback, just as for ordinary aerial ceiling collision. Including bomb
vertical collision in the shared post-movement collision transition corrects the
ordering; no bomb impulse, position, or speed table was adjusted.

All 12,960 frames match after the fix. The independent #413 full-fuse matrix also
still matches all 32,000 frames. The native capture was repeated independently
and the two CSV hashes agree:
`B618A4CA01E1B45C402E8110A600F251DB427E80A94BEA819482F2ECFE12695F`.

`bomb-chain-native-capture.zip` preserves that CSV, not ROM/save data. To capture,
include `native-release-probe.h` and this probe in `sm_rtl.c` after the forward
declaration of `StateRecorder`, then temporarily dispatch
`DiagnosticBombChains(romPath, newCsvPath)` before SDL initialization. Restore the
temporary hooks afterwards. Do not overwrite existing captures.

After extracting the archive to a new directory:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --bomb-chain-comparison-audit "Super Metroid.smc" path/to/bomb-chain-412.csv
```

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native C: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

## Repeated vertical ascent and timing boundaries

The same probe's `DiagnosticRepeatedBombChains` entry runs 600 frames with Shoot
every N frames for each N in 48–56, both facings, both ceilings and all three
travel inputs. The expanded matrix is 108 cases / 64,800 frames. No production
change was needed: all compared words match the cartridge.

For neutral direction and the high ceiling, both facings sustain 11 launches at
N=52,53,54 with no return to the floor after frame 53. Every successive launch is
higher than the previous one. At N=51 the bombs miss their airborne handoffs and
Samus returns to the floor; N=55 also fails to sustain flight. These are explicit
assertions, in addition to all per-frame native comparisons. They establish a
bounded repeating ascent and adjacent misses, not an unbounded execution proof.

`repeated-bomb-chain-native-capture.zip` contains the independently repeated CSV:
SHA-256 `A2E70D33AD7AC5A7A07AC4B1E7E30D59BC327EC094A242002B460AD27A54DBC0`.
The same ROM/source pins and temporary-hook procedure apply.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --repeated-bomb-chain-comparison-audit "Super Metroid.smc" path/to/repeated-bomb-chain-412.csv
```

## Three-bomb airborne handoff

`DiagnosticTripleBombChains` uses bombs at frames 0, 50–55, and 68–84.
The third timestamp is encoded as `68 + travel` in this variant's CSV; the field
is a timing offset, NOT a direction, and no directional input is applied.
Both facings and both ceiling geometries give 408 cases / 73,440 frames.
All words match native without a production change.

For high-ceiling cases with the second bomb at 52, the third bomb produces a
third progressively higher airborne launch at all sampled timestamps except 80.
Timestamps 79 and 81 succeed on both sides of that native miss. Explicit
assertions retain this boundary rather than requiring every three-bomb setup
to succeed. Count actual start-handler execution: a new blast can restart an
already-active rise without a zero-to-armed direction-word edge.

`triple-bomb-chain-native-capture.zip` contains the independently repeated CSV:
SHA-256 `02901D49E901CE2238C3877FA912C1E93357206FA43A2FBB892A0CE9856FA8B2`.
Use the same native-hook procedure and pinned sources above.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --triple-bomb-chain-comparison-audit "Super Metroid.smc" path/to/triple-bomb-chain-412.csv
```

Remaining acceptance: sustained horizontal/ceiling traversal with adjacent misses. The existing brief
direction pulse tests diagonal displacement and ceiling contact, not sustained
horizontal traversal. Preserve exact input and slot-lifecycle comparisons when
expanding those fixtures.

## Steering after the first launch: ceiling collision priority

`DiagnosticHorizontalBombChains` probes bombs at frames 0, 52 and N (70 through
94, step two), with a facing-direction pulse starting at frame 74 and lasting
1 through 12 frames. Both facings and ceilings give 624 cases / 112,320 frames.
In this variant `travel` encodes pulse length minus one and `spacing` is N.
This is a steering/timing search, not yet proof of sustained horizontal traversal.

The matrix reproduced 2,730 divergent frames. In the shortest pulse's low-ceiling
cases, frame 74 resumes ordinary moving-ball movement; frame 75 releases input
and hits the ceiling. C# previously let the stationary-pose fallback clear the
new horizontal speed. Cartridge `$91:E8E5` instead selects the current pose and
collision command five before ordinary input fallback handling. The shared
runtime ceiling-priority branch covered aerial and bomb movers but omitted the
ordinary Morph Ball mover that had just taken over.

Adding that mover's ceiling result preserves the moving pose and `$0000.C000`
horizontal speed. Explicit frame-75 assertions check pose, speed and both
coordinates, mirrored for left/right. All 112,320 frames now match the native
trace, including complete bomb slot lifecycles. The earlier short, repeated,
three-bomb and live hurt-bomb matrices remain separate regression gates.

`horizontal-bomb-chain-native-capture.zip` preserves the independently repeated
CSV, SHA-256:
`F0E2549C283376D28A9DAD3355F0DC0A1EC602698E93DE6CA433CD283AA53649`.
Use the same pinned sources and temporary headless dispatch procedure above.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --horizontal-bomb-chain-comparison-audit "Super Metroid.smc" path/to/horizontal-bomb-chain-412.csv
```

The issue remains open without the validation label until sustained horizontal
and ceiling traversal and adjacent timing misses are demonstrated.

## Repeating three-bomb ladder and collision interruption

`DiagnosticLadderBombChains` starts with bombs at frames 0 and 52, then places
another every N frames (24 through 28) from that second timestamp. Neutral input
or a four-frame Left/Right pulse at 122 through 125, both facings and both ceiling
heights, produce 60 cases of 360 frames (21,600 compared frames).

High-ceiling neutral cases at N=26 and 27 sustain eight progressively higher
launches with no return to the floor. The eighth launch is over 110 pixels above
the first. Adjacent N=25 and 28 lose the chain and return to the floor. Explicit
assertions verify these properties on both facings. This proves repeated ladder
ascent, not only the earlier isolated three-bomb handoff.

The matrix initially reproduced 612 divergent frames from two defects:

* At frame 127 in low-ceiling, N=24 steered cases, another bomb arms on the same
  frame as a ceiling contact. The ball mover eagerly zeroed vertical speed,
  although native bomb interruption takes priority over `$91:EFDF`. Movement now
  only publishes its ceiling result; the winning runtime transition applies
  velocity cleanup. The exact frame asserts the new bomb start and retained
  `$0000.5800` upward speed. A separate carried-ball check explicitly resolves the
  winning ceiling command and verifies the following frame's motion.
* At frame 208 in low-ceiling, N=28 steered cases, the final landing installed
  grounded art but left base momentum behind. The native collision dispatcher
  clears base speed and acceleration mode when the bounce handler returns carry
  clear. Final Morph Ball landing now does so, while both rebounds retain their
  momentum. Dedicated assertions cover rebound retention and final clearance.

All 21,600 frames now match. Earlier short, repeated, triple and steering matrices
(263,520 frames), live hurt-bomb (32,000 frames), and full core verification pass.
`BombChainAuditScenario` makes the managed schedule selection mutually exclusive
rather than accumulating independent boolean switches.

`ladder-bomb-chain-native-capture.zip` preserves the independently repeated CSV:
SHA-256 `8DC6035CF886D4CE7B56950D7ABF53CAB2778972681CB593D24ADB77D83367EB`.
The same ROM/source pins and temporary headless-hook procedure apply.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --ladder-bomb-chain-comparison-audit "Super Metroid.smc" path/to/ladder-bomb-chain-412.csv
```

Controller-only hover searches also tried one-direction and away/forward pulses
every 52–56 frames. They did not establish sustained horizontal traversal: short
pulses stopped producing lateral displacement after the initial launch, while
larger offsets lost the subsequent bomb overlap. These negative candidates are
not counted as acceptance evidence and are not the archived ladder schedule.

## Repeated ceiling steering: wall momentum regression

`DiagnosticCeilingTravelBombChains` runs only the low ceiling, both facings,
with bombs at frame 0 and every 24 frames from frame 52. Steering starts at
`170 + travel`, where travel is 0 through 23. Each 24-frame cycle holds away
for one frame, then toward the initial facing for `spacing` frames (1 through 6).
These 288 cases run 480 frames each, for 138,240 compared frames. The managed
scenario is named `CeilingSteering`: its purpose is boundary coverage, not a
claim that these inputs successfully traverse the ceiling.

The sweep reproduced 625 divergent frames at the shaft walls. The diagonal
bomb mover called block collision without the momentum teardown performed by
native `$90:E5CE` inside the direction-aware X mover. Position clipped correctly,
but base speed accumulated against the wall. The bomb mover now performs the
shared horizontal-momentum clear on X collision, while preserving the later Y
scan's authority over whether the upward bomb arc continues. Frame 465 of the
right-facing travel=0/spacing=6 case explicitly asserts the wall coordinate,
zero base speed, upward direction and still-active bomb movement. The separate
wall-contact verification also asserts momentum clearance without loss of ascent.

All 138,240 frames match after correction. The earlier five bomb-chain matrices
(285,120 frames), live hurt-bomb (32,000 frames), and full core verification pass.
The independently repeated CSV is preserved in
`ceiling-steering-bomb-chain-native-capture.zip`, SHA-256:
`E57A37D9AE84DD09DE7AEC3FC86F88385A7536E4E3A3D851728754E4167886A5`.
Use the same ROM/source pins and headless dispatch procedure as above.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --ceiling-steering-bomb-chain-comparison-audit "Super Metroid.smc" path/to/ceiling-travel-bomb-chain-412-v2.csv
```

Neither this away/toward sweep nor the earlier toward-only sweep established
sustained sideways traversal. Candidates that stayed airborne had negligible
lateral displacement; moving candidates missed subsequent bombs and returned to
the floor. Successful horizontal/ceiling traversal remains required for #412.
