# Ceiling-bonk Continuous Wall Jump — partial #445

The [technique reference](https://wiki.supermetroid.run/Continuous_Wall_Jump)
describes ceiling contact as an alternative to releasing/repressing Forward.
This fixture proves that branch against original cartridge CPU execution, not
the translated C movement functions. It complements CONTINUOUS-WALLJUMP.md.

## Run and provenance

Run `SuperMetroid.DebugRunner --ceiling-walljump-comparison-audit "Super Metroid.smc" 445-ceiling-capture.csv`.
The accepted CSV is in `ceiling-walljump-native-capture.zip`. SHA-256:
`967C0F7E6B0BCEE3E48577E3BCC981701EDECB99F1BFA204B0F3EBF496067262`.
A second native CPU capture has the same hash. ROM and pinned harness/disassembly
revisions are recorded in XRAY-CHARGE.md (Japan/USA revision 0).

For recapture, include `native-release-probe.h` and
`native-ceiling-walljump-capture.h` after StateRecorder in sm_rtl.c. Dispatch
DiagnosticCeilingWalljumpCapture with absolute ROM/output paths before SDL starts.
The existing loader restores unpatched retail bytes. Captures use no player save
and no cheats. Temporary includes/entrypoints were removed after capture and the
ordinary native executable rebuilt. Existing unrelated upstream edits were kept.

## Setup and controls

The constructed room is 144x80 blocks, floor row 16, end walls columns 1/142.
The narrow platform begins at row 13, column 85 rightward or 58 leftward. Ceiling
cases add a solid row 7; controls omit it. Samus starts at Y=235 and
X=1024/right or 1279/left, plus a swept offset -6..6. Equipment is Morph Ball,
Bombs and Charge Beam; energy is 99, ordinary gravity, no liquids.

- Hold Shoot through frame 70, earning charge 71.
- Hold Forward continuously from frame 30, with Run through frame 69.
- Hold Jump from 70, release for exactly the frame before the second Jump.
- Sweep second Jump over 132..136; hold it afterward through frame 179.

Two facings, thirteen offsets, five Jump timings and ceiling/no-ceiling variants
give 260 cases / 46,800 frames. Every frame compares input, position/subpixels,
pose/movement/animation, base and extra speed, acceleration mode, vertical speed
and direction, charge, spread hold, bomb state and bounce state.

Before the second Jump, the ceiling limits minimum center Y to 140; no-ceiling
controls reach 124. Only ceiling cases at second Jump 134 succeed: rightward
offsets +4/+5 and leftward offsets -3/-2. All four jump from the far side of the
platform with extra speed 2.0 and charge 71, while the native launch resets base
speed. All adjacent position/timing and no-ceiling cases fail. C# matches every
captured frame and the exact success/failure windows. No production correction
was necessary.

## Search history and remaining scope

Simply adding a ceiling to the earlier row-11 platform did not produce a valid
CWJ: 7,056 ceiling/position/timing candidates and a subsequent 49,896-case
platform-column sweep found none. A smaller trace confirmed ceiling contact and
the last-different-movement prerequisite, but not a usable far-side trajectory.
The 12,420-case height search found the row-13/column-85/right-offset-4 seed.
`native-ceiling-walljump-search.h` preserves the broad search and the height
search (`SM_CWJ_NARROW` set), including failed-candidate pose/height output in the
latter. Search outputs are exploratory, not accepted parity traces. The large
per-CPU-call debug log was deleted; it contains no authoritative evidence.

This closes the synthetic ceiling-bonk coverage gap only. Retail Moat room
geometry, item-message timing and the regional setup differences remain outside
this capture. #445 remains open and is not awaiting player validation. Do not
claim PAL parity from this Japan/USA capture or substitute the constructed room
for the required retail Moat setup.
