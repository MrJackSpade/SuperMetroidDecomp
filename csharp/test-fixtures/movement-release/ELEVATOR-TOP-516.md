# Upward Blue Brinstar elevator top-edge reproduction (#516)

Affected version: **0.1.1**. Reproduced; one contributing scheduling defect is
fixed, but the remaining pixel failure is unresolved. No validation label yet.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --elevator-top-edge-audit 'Super Metroid.smc' csharp/test-temp/elevator-top-516
```

The fixture loads the retail Morph Ball room, equips Morph Ball and five
missiles to select the return state, positions Samus on the actual elevator,
and uses Up inputs to ride until the real door collision publishes the Blue
Brinstar elevator destination. It then runs the production door coroutine,
arrival movement, and thirty settled frames. It does not publish a fabricated
door at the elevator's resting position.

The 231-frame trace captures the transition and arrival. On frame 96 Samus's
world Y is 292, camera/display Y are both zero, and her boots are visibly drawn
near the top of the room, below the HUD. `frame-0096.png` was visually inspected.
This is the reported symptom, not an inference from an endpoint assertion.

The regression blackens Samus's dedicated OBJ palette in an immutable copy of
the same display packet, preserving OAM, tile memory, layer ordering and the
rest of CGRAM. It counts actual composite pixel changes in the top 80 rows
while Samus's center is at least 256 pixels below the viewport origin. It
currently **fails**, with 186 pixel-frame differences, first at frame 96.
The check does not alter gameplay or infer failure from camera alignment alone.

Native source review: `DrawSamusSpritemap` ($81:89AE) writes low-byte Y without
the generic actor renderer's vertical-wrap hiding rule. Therefore simply adding
that hiding rule to Samus's renderer is not justified. The upward transition's
camera offset, final nudge, and arrival timing must be compared against original
CPU execution before changing production code. A stable managed camera at zero
does not prove that zero is the correct cartridge camera position.

Local `csharp/test-temp/elevator-top-516-pixels` preserves the CSV and captured
frames, including the visible feet (96), a later arrival frame (120), and
settled state (224). Screenshots were not committed: the repository is public
and the player has instructed us not to publish screenshots. The Release
build passes; the diagnostic's intentional failure remains visible in the CLI.
## Partial fix: camera tracking during the fade

The pre-fix trace held camera Y=32 at the end of the opening IRQ and through
the music wait. At HandleTransition it abruptly became zero. The destination
OAM build calls the full runtime frame, inadvertently running normal camera
tracking. In contrast, native `$82:E737` runs enemy/draw owners without invoking
`MainScrollingRoutine` (`$90:94EC`). This was a scheduler mismatch, not a missing
Y-clipping rule in Samus's OAM writer.

Normal camera tracking now observes the existing `$0795` door-transition gate.
Despite its legacy elevator-oriented property name, the gate covers all ordinary
doors as well. Camera Y now stays 32 through the fade; the first resumed arrival
frames reduce it 30,28,...,0 rather than snapping it to zero before gameplay.
The audit explicitly asserts unchanged camera Y across HandleTransition and
each destination-fade step.

The same actual-room pixel reproduction drops from 186 to **3** differences,
first at frame 112 rather than 96. It still fails and the ticket remains open
without an awaiting-player-validation label. Do not regard this partial fix as
proof that clipping, camera tracking, or the player's full symptom is resolved.
The remaining three pixels require comparison with native arrival/draw timing.

The Release build, full Core verification, elevator frontend handoff audit, and
forty-frame spin-door native comparison (#517) pass with the scheduling change.
The #516 fade-camera assertion passes; its independent pixel assertion still
fails with the three differences above.

## Corrected audit cadence and residual witness

The earlier audit called `RunNmi` after `StepFrame`, but `StepFrame` already
accepts an NMI in its prologue. The desktop frontend does not make that second
call. This doubled the audit's NMI counter, defeated alternating elevator
visibility, and uploaded freshly built OAM earlier than the normal frontend.
The audit now follows the frontend's single-NMI cadence. Earlier pixel counts
above remain historical observations, not an exact native-timing comparison.

With the corrected cadence, the same 231-frame route still fails: **two**
pixel-frame differences, both at screen (130,32), on frames 113 and 115.
The first witness can be traced to its contributing OAM record by hiding each
palette-four record in a cloned display packet. The trace also includes pose,
animation frame, and NMI counter. This is diagnostic isolation only: neither
live OAM nor production rendering is changed. A native draw/arrival comparison
is still required before deciding whether that residual pixel is a defect.

All PNG output stays under the local test-temp directory. Publish only source,
these textual findings, and numerical diagnostics; never publish screenshots.

## Original CPU draw comparison

`native-elevator-draw-probe.h` executes retail `$90:8A00`, the drawing target
used by the elevator on visible NMIs. It sweeps world Y 256 through 294 and
camera Y 0 through 32 in two-pixel increments, with the observed pose/frame
zero and X=128. All **663** cases match the managed `Samus.Draw` output for
every emitted low-OAM byte and all 32 high-OAM bytes. The native program runs
the original restored ROM instructions, not the translated C draw function.

In particular, world Y=268/camera Y=0 emits record 9 at X=124/Y=20,
tile `$08`, attributes `$28`, exactly matching the residual pixel's owner.
This rules out a difference in sprite-record generation for these inputs.
It does **not** prove the remaining pixel is visible on the original console:
arrival camera timing, tile DMA and scene composition remain outside this probe.
Do not delete or weaken the failing scene assertion on the strength of this
narrow comparison, and do not introduce non-native clipping in the sprite writer.

Reproduce using pinned upstream `578f90b3cc49557bb70060ad033bb90b8cf8ac50`:

1. Confirm `upstream-sm` has no unstaged changes in `src/main.c` or `src/sm_rtl.c`.
2. Apply `native-elevator-draw-entrypoint.patch` from this directory with
   `git -C upstream-sm apply ../csharp/test-fixtures/movement-release/native-elevator-draw-entrypoint.patch`.
3. Build the upstream Release/x64 target with the installed v145 toolset.
4. Run `sm.exe --diagnostic-elevator-draw 'Super Metroid.smc' OUTPUT.csv`.
   The output must not already exist. This entrypoint is headless, suppresses
   error dialogs, and gives each CPU subroutine a finite instruction budget.
5. Reverse the exact patch with `git apply -R` and confirm the two files are clean.
6. Run the managed comparison:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --elevator-draw-native-comparison 'Super Metroid.smc' OUTPUT.csv
```

Local trace: `csharp/test-temp/elevator-native-draw-516-v1.csv`, SHA-256
`8E74CD1FB6F3D015793140863C21E7924F5266D9AD2D221127BEB5B53DAF9103`.
The trace and screenshots are not published. Source, comparison tooling and
these numerical findings are committed. Release build and all 663 comparisons
pass; no production change or player-validation claim is made here.

## Original CPU arrival-camera comparison

The same headless entrypoint also accepts `--diagnostic-elevator-arrival ROM
OUTPUT.csv`. It seeds the destination's one-screen room, blue scroll cell,
native scroller offsets and elevator population values, then executes retail
`Elevator_Init`, followed by 100 calls to the elevator state dispatcher and
`MainScrollingRoutine`. Initial camera Y=32 is the observed end-of-door value;
this probe does not independently establish the preceding door coroutine.

Pass its output as the optional fourth argument to `--elevator-top-edge-audit`:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --elevator-top-edge-audit 'Super Metroid.smc' csharp/test-temp/elevator-arrival `
  csharp/test-temp/elevator-native-arrival-516-v1.csv
```

All **100 arrival frames** match the actual managed room route for Samus Y,
camera Y, fractional camera Y, and elevator status. Native camera Y follows
32 -> 30 -> 28 -> ... -> 0, including the two frames with the residual pixel.
This eliminates an arrival actor/camera arithmetic mismatch for this initial
state; it does not establish scene composition, tile DMA, or IRQ/display timing.
The scene assertion still fails with two pixel-frame differences at (130,32),
and the overall command intentionally exits nonzero after reporting the passing
camera comparison. Do not describe that overall run as a passing visual test.

Local native arrival trace SHA-256:
`0EA0825420B282DF23F4EAEF22353D108224057BE0628A6B557295463B9AFFB5`.
The temporary native entrypoint patch was reversed after the bounded probe;
no screenshots, ROM data or generated trace were committed.

## Reproduced compositor defect and shared correction

The first failing packet is now written to `local-ppu-witness.bin` inside the
local output directory. It is never committed or uploaded. The bounded native
entrypoint accepts `--diagnostic-elevator-ppu ROM INPUT.bin OUTPUT.csv` to load
the exact same VRAM/CGRAM/OAM and ordinary-layer register values into the
independent upstream SNES PPU. It emits only numerical palette-difference
coordinates, not an image. This is an independent renderer comparison, **not**
a full original-cartridge playthrough or proof of the captured tile-memory state.

For Blue Brinstar frame 113, the independent PPU finds **zero** visible
palette-four changes. A negative control moving only BG1/BG2 sampling back one
line produces precisely the managed failure: one pixel at (130,32). The managed
ordinary compositor sampled backgrounds using zero-based output Y; the original
PPU uses physical scanline Y+1 while its OBJ evaluation uses the preceding line.
This left one pixel of the wrapped boot exposed above the terrain.

The software and Direct3D ordinary BG1/BG2 samplers now apply the physical-line
offset. X-ray's alternate BG1/BG2 compositor applies the same correction so
switching the reveal effect does not shift the underlying terrain. OAM packing,
Samus coordinates, camera movement and scene-specific clipping are unchanged.
The constructed BGSC test now puts its single colored pixel on character row
one, explicitly verifying this physical-line relationship instead of preserving
the former zero-based assumption.

Verification:

- Blue Brinstar actual upward ride: **231 frames, zero wrapped pixel differences**;
  all 100 original-CPU arrival comparisons remain exact.
- Full Core verification passes.
- Direct3D hardware and WARP: 96 ordinary comparisons and 128 hardware-window
  cases each pass, including vertical scroll arrays and alternate BGSC layouts.
- Direct3D hardware and WARP: 161 source-aware X-ray and 176 X-ray window cases
  each pass.
- Temporary native source changes were reversed; captures stay local.

## Broader upward-elevator report: still open

The player notes this may affect every upward elevator (affected version 0.1.1).
`--green-elevator-top-edge-audit ROM OUTPUT_DIRECTORY` now reproduces the actual
Green Brinstar Main Shaft -> Crateria elevator, using the same door/camera checks.
It **still fails** with 36 pixel-frame differences, beginning at frame 103.
The independent PPU also exposes 18 pixels for its first captured failing frame,
including (116,32), with either BG-sampling control. Thus the verified compositor
fix does not establish resolution of that second room's artifact. Its prior
room loading/streaming and full native sequence still need comparison. Keep
#516 open without claiming all upward elevators are ready for validation.
