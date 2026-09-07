# Renderer migration — issue 321

Status: all modeled packet layer kinds have GPU implementations, and the desktop
can select the captured software path or the dedicated Direct3D11 worker. Software
remains the migration default. Full qualification is **incomplete**. The acceptance authority is
[issue 321](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/321).

## Current integration and evidence (September 7, 2026)

Diagnostic failure artifacts are exercised by `ComparisonArtifactTests` in
`--solid-smoke`: two deliberately wrong pixels verify exact first coordinate,
inclusive bounds, count and color metadata; expected/actual/difference PNGs match
the intended images; the saved packet is byte-identical to the input and replays
exactly on GPU. Debug/Release hardware/WARP pass. Only test-created artifact
directories are removed after success; genuine comparison failures retain theirs.

The UI owns input/recording, game stepping, snapshot creation and managed audio
generation. The existing waveOut worker owns native audio submission. A separate
GPU thread owns device/context, composition, swapchain and presentation. Its bounded
latest-frame mailbox may replace visuals, never inputs or PCM. Software now consumes
the same captured packets at display refresh instead of rasterizing every catch-up
step. Software rendering still runs on the UI; it is the reference backend, not proof
that a slow software paint cannot affect the current UI-owned producer.

`[Video] Renderer=Direct3D11` selects hardware. `Auto` permits a logged hardware
startup failure to choose software; it does not suppress runtime/coverage failures.
Hardware/WARP selection is explicit in the diagnostic renderer. The player INI has
not been switched. Form closing awaits GPU shutdown before destroying its HWND.
Restart/load advances host generation independently of saved cartridge counters.
Captured states can republish a retained display without stepping. Legacy-only
debugger states require the software selection rather than a fabricated GPU packet.

Current GPU comparisons against software/legacy output include:

| Coverage | Evidence per hardware/WARP device |
| --- | --- |
| Tiles, OBJ, Mode 7 | 108 tile, 197 OBJ, 241 Mode-7 comparisons |
| Color effects, messages, child windows | 254 effect/message and 112 windowed-scene comparisons |
| Ordinary gameplay | 96 layer-mask/geometry/scanline cases |
| Title/frontend | 131 title samples and 251 frontend steps |
| Intro | 275 samples across 34 phases |
| Ceres/Zebes and game-over | 143 flight/destruction samples and 101 game-over frames |
| Saved-file maps/load | 502 map frames, six areas, wrapped scroll, zoom, load/cancel; 583 additional frontend map/load/appearance frames through control release, with appearance-counter parity |
| Ending | 444 samples across all three reward branches, repeat-render/cadence checks |
| Attract demo | 315 samples, bounded retail room, held frame, completion/cancellation |
| Runtime overlays | 182 frames and 90 retained replays: Ceres initial capture, ordinary room, combined Power Bomb/message/suit owners |
| Ridley mixed-mode room | 27 retail-memory scale/tilt/scroll captures plus reverse-order retained replays after clearing live VRAM/CGRAM; explicit floor/HUD presence assertions |
| Native Ceres Ridley escape | Low-energy and hit-threshold branches: 297/342 exact samples over 15/19 AI phases; each includes every one of 113 Mode-7 frames, 87 native matrices and explicit preserved-floor-layer checks. Both reach timer/ejection completion. Termination condition seeded at natural hover; all subsequent transforms are AI-driven. Debug/Release hardware/WARP; runtime-level fixture, not controller battle or new independent cartridge evidence. |
| Room publications | 104 frames plus retained checks across Landing Site, Parlor, Blue Brinstar elevator room, Green Brinstar shaft, Morph Ball, Ceres scientist, Brinstar water and Norfair Business Center; 13 runtime ticks each; liquid type/surface and three-valued heat distortion assertions |
| Retail doors | Ceres scientist/final-hall pair, left and right: 104 exact frames each, including 63 scrolling frames, through production DoorTransitionState with authored collision/door records |
| Retail eye cone | Morph Ball room production apex fixture: 96 exact frames covering activation, first visible beam, widening and full beam |
| Ceres elevator arrival | 192 exact frontend frames: fade, 73 pad positions, landing and 60 released-control frames; Debug/Release hardware/WARP. Does not cover general inter-area elevator travel or escape quakes. |
| Blue Brinstar upward elevator return | 209 exact frames through production vertical DoorTransitionState and destination actor: 55 scroll frames, 105 return frames, 25 camera positions, release and settled camera/BG1 up-scroller alignment; Debug/Release hardware/WARP. Source elevator status and authored door contact staged from the existing regression. Downward departure and the full frontend scheduler remain distinct coverage. |
| Blue Brinstar downward ride | 614 exact frames, including 54 departure ticks, 56 vertical-scroll ticks and 454 destination-return ticks, followed by eight settled ticks. Fresh Down input starts the real actor after a staged carrier position/contact; production collision finds the exit and DoorTransitionState performs the handoff. Debug/Release hardware/WARP; no full frontend scheduler or independent cartridge-correctness claim. |
| Ceres escape quakes | Dead-scientist room and final hallway: 240 exact runtime frames each, four native displacement states per room. Escape status seeded; native door actors generate the quakes. GPU/software parity, not new independent cartridge evidence. |
| Crocomire death | 180 exact samples across 42 production death phases, including both dissolves, nonuniform BG2 scanlines, skeleton return and corpse. Boss placed at bridge trigger; Samus repositioned only at the wall-return gate. No controller battle claim. |
| Samus death | 283 frontend frames from zero health through blackout, explosion, final fade and game-over handoff; independent legacy/captured owners, exact pixels and PCM |
| Automatic reserve recovery | 100 frontend frames from zero health through automatic refill and gameplay return; independent legacy/captured owners compare pixels, PCM, audio commands, health/reserves and lock/time-freeze state. Debug/Release hardware/WARP verified. |
| Save station | 230 exact Crateria save-room frames through confirmation, visibly colored electricity, completion and control release; room-local runtime test, not disk/SRAM persistence verification |
| Kraid encounter/exits | Real incoming door and rise: 105 samples/seven AI states; collision-triggered growth: 95/eight with camera/BG2 priority handoff; death: 97/13 with sinking, fade and four BG3 restore assertions; left/right exits: 104 frames each including 63 scroll frames, with defeated-room reload before the right exit |
| Host display | Eight scaled target sizes; hidden flip HWND resize/generation tests |

These are rendering-equivalence checks, not new claims of cartridge correctness.
Some gameplay effects have stronger constructed coverage than retail integrated
coverage. The complete boss/door/elevator/liquid/effect scene matrix remains a gate.
The room-publication fixtures use the direct room-load seam, not native incoming
door setup. They verify publication/composition while the room advances, not complete
elevator travel, door scrolling, landing animation or every room-specific effect.

The slow-consumer room/pause test compares legacy software, GPU-captured and
headless-captured owners in separate Alpha Power Bomb and Maridia tube slices,
with both normally consuming and deliberately blocked GPU workers.
The headless owner never requests pixels or publishes to a renderer. It compares
the complete runtime graph using
the exact-build debugger serializer at ten checkpoints: before blocking, every
40 ticks of the 160-tick pause slice, every 16 ticks of a subsequent 64-tick
movement/short-input slice, and after GPU consumption resumes. Position and pose
also agree every tick across all three owners. The movement slice requires actual
position/pose changes and rejects room-boundary crossings. Approximately
5.3 MB of private fields, arrays, reference topology and delegate state agree at
each checkpoint, without test-specific exclusions or normalization. The serializer's
existing external `SaveRamChanged` subscription exclusion remains unchanged; this
is not an assertion about host event subscribers. Hardware and WARP also match
974,400 PCM samples per room including startup (917,035/919,838 nonzero), with the GPU owner held
for at least 250 ms. This strengthens runtime-state coverage for that slice; it
does not establish the full frontend/save/backend/scene determinism matrix.
These are independent room-local fixtures, not traversal tests; the normal consumer
still uses a hidden/occluded HWND and is not a visible-presentation speed guarantee.

Device removal/reset HRESULTs rebuild resources on the GPU owner, retaining only
CPU packet data. Tests inject the actual SharpGen failure codes and cover retained
frames, retry of a dequeued frame, bounded failure and normal shutdown. Actual
driver reset and monitor/DPI/RDP changes remain unqualified.
No machine-wide forced GPU reset has been performed.

Closing the actual GameForm while a load/restart awaited the presentation gate
previously reproduced `InvalidOperationException: GPU worker is stopping`.
The asynchronous generation handoff now reports orderly shutdown cancellation
explicitly, and the host does not replace the game after that result. Genuine
worker faults are still rethrown. `CloseDuringLoadTests` holds the real gate,
starts each operation, closes the form, and releases the gate; both cases assert
successful worker shutdown and unchanged game ownership. Full DesktopVerification
passes in Debug and Release, including normal load/reset and startup failures.

The worker's zero-sized-surface branch is verified separately by
`SurfaceSuspensionTests` under `--swapchain-smoke` in Debug/Release on hardware
and WARP. Zero width, zero height and both zero each suspend submission; 32
publications remain bounded to one pending frame with 31 replacements and no
uploads or presentations over a 250-ms interval. Generation changes invalidate
that pending frame; restoring the surface consumes the latest new-generation
frame at the requested size. Shutdown also completes while suspended with a
pending frame. An internal render-owner acknowledgement makes these assertions
independent of host/worker scheduling races. This exercises the actual worker
and hidden swapchain, not an OS minimize, DPI change or RDP reconnect event.

GPU timestamp queries use a bounded nonblocking ring. The tooltip reports rolling
upload CPU submission percentiles and cumulative uploaded bytes/calls separately.
These count every UpdateSubresource, including reusable window children and display
constants. Upload time is already included in composition/display CPU intervals;
do not add it again. It excludes source packing and does not time asynchronous GPU
transfer completion. Exact parent/child byte and call totals are tested, including
empty windows; ordinary submission remains allocation-free.
The tooltip also reports rolling
p50/p95/p99 CPU composition, display/Present and GPU composition histories, excluding
60 warmup samples. GPU composition excludes scaling/Present; a second timestamp
interval includes display drawing but ends before the CPU Present call. Audio telemetry counts
native-empty-before-refill observations, excluding startup/reset; this is not an
endpoint-reported underrun duration. The real silent waveOut drain/refill test passes.

Ordinary GPU submission allocation fell from 116,760 to zero bytes/frame in the
focused test (capture/readback excluded). Reproducible unpaced CPU results are in
`test-fixtures/issue-321-performance`; they are not paced or whole-game results.

Additional commands, run from the repository root:

`--retail-all` runs the complete current 19-suite retail inventory in order on one
renderer per device, so GPU resources are reused across scene families. Individual
commands remain available. This aggregate is the inventory above, not a claim that
all remaining issue acceptance gates have been completed.

The 18-suite Debug aggregate at `bc2c5ae` passed on both hardware and WARP,
including both elevator directions and native Ridley escape branches. Reserve
recovery was added afterward and verified separately; it was not part of that run.

The complete 17-suite Release aggregate passed on hardware and WARP at
`119198f`. The same build's visible Debug five-minute gameplay/pause runs met
the measured CPU/GPU budgets with whole-run timing histories, no observed audio
queue drains, and no discarded wall-clock frames. See
`test-fixtures/issue-321-performance/desktop-visible-soak-debug.json` and its
README for scope, memory samples and RDP presentation limitations. These results
do not qualify the remaining scene/lifecycle gates or change the default backend.

```powershell
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-all
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-file-map
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-ending
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-attract
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --runtime-overlays
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-ridley
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-rooms
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-doors
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-kraid
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-eye
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-elevator
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-crocomire
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-ceres-quakes
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-death
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --retail-save
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --profile-simulation
dotnet run --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --audio-queue
dotnet run --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --soak-hidden 300
```

The last command runs five minutes per scene with a standalone paced producer,
managed audio, silent real waveOut and a hidden hardware HWND. It deliberately does
not count as visible-presentation or production-WinForms-clock qualification. Reports
state that scope and retain PCM counts, queue observations, timings and adapter.
The preserved report confirms 60-Hz production with zero observed queue drains.
The separate `--soak-desktop-hidden 300` command also passed five minutes per scene
using the actual production WinForms timer: 59.994 FPS, producer p95 0.2027/0.1383 ms,
zero observed audio queue drains. Both reports and precise scope limitations are in
`test-fixtures/issue-321-performance`. All presentations were occluded, not visible.
The full GPU interval measured about 31.7 ms in those hidden runs.
The subsequent interactive `--soak-desktop-visible 300` Release run passed both
five-minute scenes: 59.997/59.994 simulation FPS, zero observed audio queue drains,
GPU composition p95 3.1908/1.8637 ms and composition-plus-display p95
3.2594/1.9333 ms. Successful presentation was about 32 FPS under RDP, with zero
occluded Presents. The committed visible report documents sampling and driver
details; it does not establish RDP delivery rate.
The subsequent visible Debug run also passed five minutes per scene, retaining
all post-warmup observations: 59.995/59.992 simulation FPS, producer p95
0.9083/0.7065 ms, GPU composition p95 3.1560/3.1928 ms, no observed audio
queue drains and no discarded wall-clock frames. Its report includes memory,
maximum producer duration, p99 and upload telemetry. Both measured configurations
meet the specified budgets in these workloads, not a whole-game performance guarantee.
Auto startup failure and strict explicit-GPU failure policies now have hidden-host tests.

Portable Core/contract/software tests now build and pass on Ubuntu 24.04.4 LTS
with .NET 10.0.11 in an isolated Linux SDK container. See `PORTABLE_RENDER_TESTS.md`
for the pinned image, read-only staging command, coverage and scope limits.

Clean isolated publication has also passed through `tools/verify-desktop-publish.ps1`:
actual game entry-point audits, matching published verifier assemblies, all 138 audio
asset hashes, required notices and a short published production-timer run. This is a
framework-dependent Windows package; a fresh machine still requires the documented
.NET Desktop Runtime. See the script and performance notes for reproducibility.

Still required: finish the scene/publication and cross-backend state/PCM acceptance
audit (room-runtime fixtures do not independently prove every frontend handoff),
real recovery/DPI/RDP evidence, verified hardware-default selection,
final documentation and player-validation handoff. Do not close
#321 or mark its completion goal achieved from the milestones above.

## Historical inspection and incremental notes

The sections below record earlier stages in chronological context. Statements such
as "not integrated yet" describe that increment, not current status; use the current
section above and the issue's acceptance checklist for remaining work.

GPU tile increment: packed VRAM/CGRAM upload feeds shader-side 4-bpp and 2-bpp
decoding, flips, BGSC pages, priority filters and transparency; fixed-color addition
and ordered fades also execute on GPU. No composed CPU image is uploaded. Both RTX
3090 and WARP pass 108 cases plus the exact packed-byte regression in Debug/Release.
The first comparison exposed an optimized-byte-extraction miscompile; the failing
packet is preserved, and an equivalent explicit bit-selection path passes with
optimization retained. The comparison CLI now saves packet/pixel/difference artifacts
on mismatch. OBJ, Mode 7 and the remaining compound/effect operations are next.

Initial GPU increment: `SuperMetroid.Rendering.Direct3D11` owns an explicit hardware
or WARP device/context and builds an integer solid/fade shader from source.
`SuperMetroid.RenderVerification --solid-smoke` dispatches that shader and compares
readback to the reference: 64 RGBA/fade cases per device, Debug and Release, plus
wrong-thread rejection. Hardware enumeration was necessary because the default
driver-type request selected Basic Render Driver in this environment; explicit
enumeration now reports RTX 3090. See `D3D11_BUILD.md` for locked dependencies,
compiler hash, license review and commands. This is the beginning of the GPU
implementation, not satisfaction of any full-scene or performance acceptance gate.

Attract-demo capture: reproduced a legacy-raster fallback at tick 2469 entering the
first demo, then replaced all three demo-owned black allocations with PublishBlack.
The focused fixture retains retail first-room setup/input, shortens its duration to
sixteen gameplay ticks and ends the set after that room. Both natural completion
and hold cancellation return to title; 315 sampled frames match pixels and audio
command order, including the retained final image. Debug and Release suites pass.
This supersedes the three-black-frame gap noted in the prior increment below.

Saved-file map extraction increment: the live file-map display now publishes owned
area/room scenes in StepCaptured. Format ten adds 2-bpp subscreen addition with
optional foreground coverage and a rectangular child-scene insertion (no nested
insertions). Tests cover 502 exact area/room/reveal/return/load/cancel samples,
all six area maps, malformed window packets, retained ownership, and a saved-file
frontend sequence reaching gameplay with matching pixels/state/audio commands and
no legacy-raster fallback. The final call-site inventory still found three explicit
black-frame assignments in attract-demo dispatch; those need conversion and a demo
capture test before declaring the staged frontend fully extracted. No desktop
scheduling or GPU backend has been switched on by this increment.

Ending extraction increment: the state-$27 display call now publishes owned layers
in StepCaptured, including Mode-7 escape/explosion, scrolling credits, blank reward
transition and post-credit art/OBJ. Existing layer operations suffice, so there is
no format revision. Independent scene owners compare 444 sampled frames across all
three time-based rewards; retained packets are checked after subsequent simulation
updates. This is software-reference parity, not GPU or cartridge acceptance. The
saved-file map selection path still needs extraction.

Latest extraction increment: Mode-7 gameplay is now captured, superseding the
ordinary-integration section's historical fallback limitation. Explicit HUD and
optional Mode-1 floor bands carry physical scanline boundaries, transform and tile
registers. The software consumer has no Ceres/Ridley identity checks. Format version
9 adds this operation without changing older encodings. Ceres startup now requires
packet output. Forty-eight synthetic full-frame matrix/scroll/floor cases match the
independent legacy compositor, and a retail Ridley room with constructed getaway
registers checks producer parity and retained ownership after mode exit/memory
replacement. This is not a controller-driven battle or new cartridge-correctness
claim. Desktop still uses legacy Step; saved-file maps and ending capture, D3D11,
live scheduling, PCM determinism, device recovery and performance gates remain pending.

Inspected revision: `8e0b7b71f2c73808f0b9834f53146e32b1945a75` (2026-09-06).
SDK: 10.0.400. Host reports a Ryzen 9 5900X, RTX 3090 driver 32.0.15.9636,
and Microsoft Remote Display Adapter driver 10.0.26100.8972. Adapter enumeration
is not evidence of which adapter a future D3D device uses. WMI's 32-bit AdapterRAM
field is not a reliable measurement of this card's VRAM and is deliberately omitted.

Previously preserved Maridia pause fixture:
`test-fixtures/issue-54-maridia-pause`, including matching Core/Desktop binaries.
Its pre-optimization measurements and capture procedure are in that fixture's
README. These are historical results, not measurements of the new renderer.
Current Debug/Release workload distributions, GPU timings, and soaks remain pending.
Before rebuilding, preserve any additional live state together with its matching
assemblies; debugger-state MVID validation must not be bypassed.

## Decision: capture at existing display-production call sites

`SuperMetroidGame.Step` does not simply update state and render once at the end.
Some cases render and then mutate palettes, switch scenes, or install the next
state. For example, DeathSequenceStart renders before PrepareDeathPaletteFade;
MainGameplay produces a frame before publishing pause/door/escape transitions.
Capturing everything at the end of Step would shift those images by a frame.

Replace each current display-production call with capture at that same ordered
point. Retain explicit black frames and retained-frame cases. Audio collection and
acknowledgement order stay independent of that replacement. Outer brightness
operations become ordered packet operations, not mutations of a previously
published framebuffer. Preserve successive integer brightness operations rather
than collapsing them into one factor with different rounding.

The first contract will own complete memory copies and ordered, typed composition
data. It must not contain a delegate closing over a runtime, a mutable hardware
object, a ROM bus, or a GPU resource. Resolve ROM-dependent visual lookup data on
the simulation owner. Memory snapshots include raw VRAM, native CGRAM words and
both OAM tables. Register/scanline state and effect parameters are captured with
the matching memory owner. A memory snapshot alone is not a complete frame.

Each packet carries host sequence, reset/load generation and simulation identity.
Sequence is host-owned and does not rewind with a debugger state. Complete packets
allow intermediate visual frames to be discarded without losing dirty updates.
Start without pooling; introduce reuse only after lifetime tests and measurements.

## Ownership and scheduling decision

- UI owns controls, window lifetime and input-event collection. It sends commands
  and timestamped input changes, never a mutable game reference, to the simulation
  owner. Existing short-tap semantics must survive the handoff.
- Simulation owner exclusively advances game, input recording, cartridge audio,
  APU and acknowledgements. Snapshot publication occurs at the ordered display
  boundary. Save/load, pause/step and shutdown are frame-boundary commands.
- Render owner consumes the newest complete packet, owns immediate D3D context,
  swap chain and GPU resources, and may wait for presentation. It cannot block the
  simulation through Present, resize, fences or screenshot readback.
- A bounded latest-frame mailbox counts replacements. Generation invalidation
  prevents an in-flight old scene from being presented after load/reset. UI labels
  receive immutable diagnostics rather than reading live simulation fields.
- Host suspension/debugger stops need an explicit rebase policy. Normal slow
  presentation is not a request to pause gameplay or discard PCM/input ticks.

Software will consume the same packets before D3D implementation begins. Legacy
synchronous callers retain an explicit capture-plus-software-render adapter.
Portable contract/reference projects must not reference Desktop or Windows APIs.
D3D binding selection, minimum feature level and packaging remain decisions to
verify against supported current packages; no wrapper is selected by this note.

## Scene coverage ledger

Paths below are relative to `src/SuperMetroid.Core`. D3D parity and real-state
qualification remain pending for every row. Software extraction evidence follows
the inventory; rows not covered by that evidence remain unextracted.
Listing a producer does not claim its nested methods are already observational.

| Scene family | Current producer(s) | Required state / extraction hazards |
| --- | --- | --- |
| Black, retained and outer fade frames | `Frontend/SuperMetroidGame.cs` | Preserve call-site order, retained images and integer brightness passes; no end-of-Step recapture |
| Nintendo/year cards, title and attract return | `Frontend/TitleSequenceState.cs` | Mode 7 enable, signed matrix/offset, private VRAM/CGRAM; Render constructs/finalizes OAM |
| File select, copy, clear and confirmations | `Frontend/FileSelectMenuState.cs` | Menu-owned memory, sprite selector/head, text, fades; audit OAM construction |
| Options and controller submenus | `Frontend/GameOptionsMenuState.cs` | Private memory, cursor/text/priority, fade and submenu selection |
| Existing-save area selection and zoom | `Frontend/FileSelectMapMenuState.cs`, `FileSelectAreaMapGraphics.cs`, `FileSelectMapEntry.cs` | Area map color math, markers, zoom/clipping; separate from gameplay map |
| Existing-save station map | `Frontend/FileSelectRoomMapGraphics.cs` | Scrolls, frame, markers and clipping |
| Narration and illustrated pages | `Frontend/IntroCinematicState.cs` | Text/caret, page crossfades, object OAM, private palette/VRAM |
| Mother Brain, discovery, scientist flashbacks | Same, specialized Render methods | Gameplay-like layers with cinematic-owned sprites, Samus graphics and color math |
| Ceres approach / colony title | `Frontend/IntroCeresFlightState.cs` | Mode 7, object graphics ownership, title sprites and flight brightness |
| Ceres destruction / Zebes descent | `Frontend/CeresDestructionCinematicState.Rendering.cs` | Mixed cinematic backgrounds and actors; Render rebuilds OAM; signed addressing/wrap |
| Ordinary gameplay and reserve recovery | `Rendering/SuperMetroidRuntimeFrameRenderer.cs` | Displayed scroll/shake/OAM, live VRAM/CGRAM, HUD split, BG/OBJ priority |
| Scrolling sky and boss-owned BG2 | Same | Sky line scrolls, Kraid tilemap geometry/scroll, Crocomire scroll/death scanlines; capture live inputs |
| Ceres elevator Mode 7 | Same | NMI-published Samus transform paired with scene scrolls and OAM |
| Ceres Ridley getaway | Same | Live enemy matrix currently read during Render; floor returns to Mode 1 below mixed-mode region |
| Liquid/heat/layer-three FX | Same, `Rendering/SnesGameplayFrameRenderer.cs` | Published FX snapshot, scanline distortion, surface masks and color math |
| Ceres haze | Same | Boss-bit-selected gradient; resolve selection before publication |
| Eye cone and power bomb | Same | Resolve bus-dependent tables and mutable explosion state before publication; fixed-color/window operations |
| Message boxes and suit pickup | `Rendering/GameplayMessageBoxRenderer.cs`, `SamusSuitPickupRenderer.cs` | Ordered final overlays, window state, active message and suit state |
| Doors, elevators and gameplay load/save | Frontend dispatcher + runtime renderer | Old/new room publication, screen masks, palette/OBJ handoff, fade and load/save animation |
| Pause map/equipment and transitions | `Frontend/PauseMenuState.cs` | Own memory plus copied gameplay HUD; Render rebuilds OAM; page brightness followed by outer fade |
| Death, whiteout and game over | Dispatcher + runtime renderer, `Frontend/GameOverMenuState.cs` | Palette mutation order, fatal OAM, whiteout, menu-owned memory |
| Attract gameplay | `Frontend/SuperMetroidGame.AttractDemo.cs` | Gameplay render path with demo input/state cadence unchanged |
| Ending, credits, post-credits | `Frontend/EndingCreditsState.Rendering.cs` | Mode 7 and post-credit backgrounds, variable OBSEL; RenderSprites rebuilds OAM |

### Current software extraction evidence

All tests below are in `src/SuperMetroid.Verification`; no row claims GPU parity.

| Extracted family | Reference evidence |
| --- | --- |
| Title/cards | `TitleSnapshotTests`: 101 constructed and 131 retail samples |
| File select/options | `FileMenuSnapshotTests`: 284 samples; does not cover saved-file area/station map |
| Pause map/equipment | `PauseSnapshotTests`: 128 samples with retained HUD |
| Entire intro, including flashbacks and approach | `CinematicSnapshotTests`: 275 samples across 34 phases, independent scene owners; separate 108-sample approach test |
| Ceres destruction/Zebes descent | `CinematicSnapshotTests`: 143 samples across 15 phases |
| Game-over menu | `CinematicSnapshotTests`: 101 samples, both choices |
| Envelope/retained frame/fades | `RenderFrameHandoffTests`, `RenderPacketCodecTests`, `FrontendRenderCaptureTests`; component and menu-handoff evidence only |

Gameplay base extraction is partial (see below); full gameplay, saved-file maps,
endings, and runtime-dependent overlays remain pending.
The general tile/OBJ/Mode7/compositor/brightness kernels additionally require
synthetic edge coverage; scene screenshots alone cannot cover memory boundaries.

## Confirmed render-side work that cannot move blindly to a worker

`PauseMenuState.Render` calls BeginFrame, map/equipment sprite builders and
FinalizeFrame on persistent OAM. `TitleSequenceState.Render` does the same for
title sprites. Ceres destruction and ending renderers also rebuild actor OAM.
These builds belong to capture preparation on the simulation owner; rasterization
must consume the resulting immutable OAM. Audit nested draw methods for timer,
sound or RNG mutations before calling capture observational.

Runtime composition currently reads live Ceres Ridley matrices, Kraid/Crocomire
state, scrolling sky, boss bits, power-bomb state, message boxes and suit pickup,
as well as already-published PPU snapshots. This is safe only under the current
synchronous ownership. Do not make it concurrent by retaining runtime references.

## Next evidence gates

1. Preserve required live-state builds and record current workload baselines.
2. Inventory nested render-side mutations and add repeat-capture observation tests.
3. Extract owned memory and typed composition operations; prove software parity at
   the existing publication call sites before changing scheduling.
4. Add packet serialization, comparison diagnostics and bounded mailbox tests.
5. Follow the remaining D3D, full-scene, portability and performance gates in #321.

No acceptance checkbox is satisfied solely by this plan. Remaining work stays in
the original issue rather than being deferred into narrower completion goals.

## Boundary extraction evidence

The title producer now exposes `CaptureRenderSnapshot`, which prepares OAM on the
simulation owner and returns owned Mode 7/OBJ composition data without rasterizing.
`SoftwareMode7ObjSnapshotRenderer` consumes it without a scene/ROM reference.
The legacy `Render` path remains independent for parity checks during migration;
the frontend is not yet switched and this is not a scheduling/performance change.

`TitleSnapshotTests` compares 101 constructed and 131 retail frames across twelve
natural title phases, including motion samples and confirmation fades. It checks
repeat rendering and retains packets across subsequent scene steps. Both paths
produce byte-identical pixels. This is software-boundary evidence, not GPU parity
or complete gameplay/audio determinism evidence.

The first retail comparison failed at YearText frame 68: OBJ pixels were absent.
Investigation showed existing OBJ kernels use `LastFinalizedSpriteCount` rather
than inspecting all physical records. The memory contract now retains that modeled
count alongside raw OAM. Restoring only physical bytes is insufficient to preserve
this implementation's sprite semantics. The same comparison passes after capture
and restore preserve the count. Do not replace it with inferred off-screen markers.

Pause now has a capture producer and an observational layered reference consumer.
The packet owns its ordered sealed BG/OBJ records, memory image, OBSEL and page
brightness. `PauseSnapshotTests` covers 128 retail-data frames in Maridia across
both pages and every fade level, with constructed retained HUD data. Pixels match
the independent legacy Render path, including when packets are rendered repeatedly
or retained across later menu steps. This is a focused fixture, not replay of the
player's exact debugger-state save and not yet a performance qualification.

The layered consumer resolves OAM once before inserting priority planes. Filtering
and drawing each priority's sprite list independently would change OBJ-vs-OBJ ties.
Software memory rehydration is shared with the Mode 7 consumer and remains local
to each render call. These temporary software scratch copies are not GPU requirements.
Frontend publication and scheduling are still pending; neither capture API is yet
the default frontend output.

File select and options now expose the same capture boundary. Shared menu capture
retains their established whole-BG2, whole-BG1, whole-OBJ ordering rather than
substituting the gameplay priority ladder. Nullable BG priority means the existing
unfiltered plane operation; an explicit whole-OBJ insertion retains OAM precedence.
`FileMenuSnapshotTests` checks 284 retail-data frames through COPY/CLEAR selection,
confirmation/completion, controller-page scrolling and special settings, including
fade frames and packets retained across later state changes. Save operations affect
only a fresh in-memory address space. Frontend output is still the legacy path.

`RenderFrameSnapshot` now envelopes the supported composition shapes (and explicit
solid frames) with positive host sequence/generation and the independent cartridge
frame counter. Outer brightness passes are owned and applied in order, preserving
per-pass integer rounding. It does not yet carry every gameplay effect or a durable
serialization version; those gates remain pending.

`LatestRenderFrameMailbox` holds at most one pending visual packet. Its short lock
contains pointer/counter operations only. `RenderFrameHandoffTests` publishes 1,000
packets while a consumer holds an old frame and remains blocked for at least 250ms;
the next acquisition returns the latest packet and accounting balances. It also
checks cartridge-counter wrap, non-rewinding host sequence, generation invalidation
and ordered fades. This is component evidence only, not a live audio/simulation
stress result. The presenter must still coordinate final submission with reset;
an `IsCurrent` check alone is not an atomic check-and-Present guarantee.

Portable fixture format version one is documented in `RENDER_PACKET_FORMAT.md`.
The codec round-trips all 644 sampled title/pause/file/options frames with exact
pixel and byte equality, as well as solid/ordered-fade packets. Structural-error
tests cover truncation, signature/version, size and trailing data. GPU comparison
CLI and a preserved packet corpus remain pending. Debugger-state files are not
modified or converted by this display-only codec.

## Staged frontend entry point

`SuperMetroidGame.StepCaptured(input, hostSequence, hostGeneration)` now invokes
the extracted title/file/options/pause producers at their original display call
sites. Converted frames return an immutable snapshot and no raster buffer. Other
scenes explicitly return `UsedLegacyRaster=true`; this must remain visible until
their extraction is complete. The normal desktop still calls the unchanged
compatibility `Step` entry point and has not switched rendering backends.

Outer pause fades append ordered packet operations. Replacing a display with a
legacy or black frame clears the prior captured content; retained display states
reidentify their existing packet rather than recapturing mutated scene state.
Host identity is supplied outside the game graph on every captured call, including
after restoration. Game-side stored packet identity is not a host sequence source.

`FrontendRenderCaptureTests` compares 250 captured frontend frames through title,
file select and options against an independent normal dispatcher, asserting exact
pixels, game-state/phase cadence and audio-command order. It verifies explicit
intro fallback, retained title packet lifetime and switching back to normal Step.
This does not establish gameplay/PCM determinism or live scheduling independence;
pause frontend integration and remaining scenes still need their own gates.

Game-over and Ceres destruction/Zebes descent now publish captured displays through
the staged frontend API. `CinematicSnapshotTests` matches 101 game-over frames
(both choices and fades) and 143 cinematic samples across 15 phases, retaining
packets across steps and through codec round trips. Mode 7 is a general layer
insertion between OBJ priorities, not a room-specific shader rule. Format version
two adds that operation while retaining version-one decoding and rejecting new
operations falsely marked as old-version data. Existing omitted mosaic behavior
is preserved as a known reference limitation, not silently corrected in the port.

Ceres approach now supplies an ordered layered snapshot through the intro's staged
capture dispatcher. Earlier intro pages still explicitly fall back. Its five active
phases match across 108 retail samples, including nonzero rear-view fixed-color
addition, caption priority and fades, repeated rendering after scene advancement,
and codec round trips. Format version three adds the full-screen fixed-color
operation; versions one/two remain readable and cannot falsely contain the new kind.
This preserves the existing five-bit reduction/expansion rules without introducing
general main/subscreen/window semantics prematurely.

The complete intro now publishes packets, including narration, Mother Brain,
SR388 discovery, delivery/examination and Ceres flight. Format four adds an explicit
wrapping 2-bpp viewport with priority and color-zero opacity; older formats remain
readable. OAM preparation and draw-owned projectile-trail updates still execute
once at the producer's original display boundary, never on the consumer.
Independent legacy/capture scene owners complete all 34 intro phases and match
275 sampled displays through serialization and after subsequent state advances.
The test also checks actor positions, hit and projectile counts, and phase cadence.
Another 24 constructed cases cover vertical wrap, priority and opacity. These are
software-reference parity checks, not GPU or PCM determinism evidence. Earlier
notes about the intro's legacy fallback describe the preceding migration stage;
gameplay, other remaining scenes, the GPU backend and live scheduling are pending.

`GameplayDisplayCapture.CaptureOrdinaryBase` now resolves ordinary room register
selection, shakes, sky/liquid BG2 scroll tables, boss-owned BG2 geometry, HUD
character base and door TM selection into owned data. It explicitly rejects Mode 7
and does not claim to include final FX/windows/messages/suit overlays. It is not
yet wired to the frontend; the player's normal renderer is unchanged.

The sealed `OrdinaryGameplayRenderLayer` owns its two optional 192-word tables.
Its software consumer reuses the existing fused Mode-1/HUD compositor. Format five
serializes registers and tables; earlier formats cannot contain the new operation.
`GameplaySnapshotTests` covers 96 mask/HDMA/geometry combinations, mutation of caller
scroll arrays, malformed table length/operation order, a retail Alpha Power Bomb
room with inactive overlays, and retained output after replacing live VRAM/CGRAM.
These tests verify extraction and ownership against the existing reference, not
independent cartridge correctness or complete gameplay visual coverage.

`ScanlineColorAddRenderLayer` now carries 224 owned inclusive windows and expanded
RGB byte operands. Suit-pickup, Ceres haze and Power Bomb producers resolve their
state/ROM-dependent geometry before publication; the consumer needs no live actor
or bus. Format six serializes the fixed-size table. This intentionally preserves
the existing byte-domain addition separately from the intro's five-bit operation.
`ColorWindowSnapshotTests` passes 238 comparisons spanning both suits, both haze
colors and three Power Bomb centers through their phases, including retained
packets after state advances, clipping, alpha and saturation, mutation of input
tables and codec rejection cases. These producers are not yet integrated into a
complete gameplay capture; the normal desktop renderer remains unchanged.

`MessageBoxRenderLayer` owns bank-$85 tile words and reveal radius. The renderer now
has an overload accepting this data instead of the live message coroutine. Format
seven adds bounded row/radius/tile serialization. `MessageSnapshotTests` covers all
26 retail message definitions using constructed glyph memory (not screenshot
goldens), and an independent solid-glyph oracle across 3–6 rows and every radius
0–24, plus the save-selection tile change. Retained packets survive closing/reusing the live
message owner; caller-array mutation, invalid shapes and codec failures are tested.
Temporary palette colors and clipping remain reference behavior, not new effects.
This completes the message producer's extraction, not full gameplay integration.

The Morph Ball security-eye producer now resolves its published origin/angle and
ROM tangent values into the same owned scanline color-window operation. It solves
the reference cross-product inequalities with signed floor/ceiling division once
per line, retaining the one-line apex offset, fractional tolerance, horizontal-ray
special cases and HUD exclusion. No per-pixel capture raster or new packet version
is needed. `EyeWindowSnapshotTests` matches 933 angle/width/clipped-origin cases
against the original pixel loop, including all 256 angle indices and signed camera
wrap. A separate 6,859-case integer inequality oracle checks both endpoint rounding
directions. This is extraction parity, not a correction of eye placement, and is
not yet connected to the complete frontend gameplay packet.

Room layer-three FX now captures a generic 2-bpp color-math plane with owned physical
scanline scroll pairs. The producer resolves water wave phase, signed liquid absence,
surface-relative scroll, atmosphere geometry and add/subtract selection. Consumers
receive no room/FX object. Format eight carries the plane geometry/equation/registers.
`RoomFxSnapshotTests` compares 252 constructed cases across the five FX types and
seven valid blend configurations, six surface positions, three wave phases and
visible/absent liquid state. The HUD stays untouched; ownership, invalid inputs and
format rejection are checked. Runtime/frontend integration and real-room qualification
remain pending; this is parity with the existing software effect, not a lava fix.

`GameplayDisplayCapture.TryCaptureFrame` now assembles the ordinary base and all
effects used by `SuperMetroidRuntimeFrameRenderer`, preserving door-IRQ suppression
and the ordering FX/haze/eye/Power Bomb/message/suit. Its null result explicitly
identifies Mode-7 gameplay, which still uses the legacy raster. `StepCaptured` calls
this producer at every existing gameplay display point, including death, doors,
pause fades and attract gameplay. Outer gameplay brightness is recorded as ordered
passes rather than forcing a raster. Normal `Step` remains the compatibility path.

`GameplayCaptureIntegrationTests` verifies a composed overlapping Power Bomb,
message and suit fixture, then independent frontend owners through startup and a
160-frame directly loaded ordinary-room pause/unpause slice. Pixels, game/phase,
Samus position/pose and emitted audio-command order match. Mode-7 fallback is checked
explicitly. This is not yet PCM/slow-consumer independence or the full real-room
matrix; those scheduling and qualification gates still remain. The desktop has
not switched to `StepCaptured` or gained a GPU backend.

The ordinary frontend slice explicitly requires return to MainGameplay after
unpause. Its Start hold uses the existing delayed-held menu filter. Tightening this
gate exposed UnpausingB's legacy black bitmap; shared forced-blank publication now
uses a solid packet in capture mode. The same producer covers other explicit
black-frame call sites while retaining the normal software compatibility behavior.
