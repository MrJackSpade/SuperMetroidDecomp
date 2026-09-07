# Direct3D11 backend build and current scope

The isolated Windows assembly implements device ownership and integer compute for
solid packets, 4-bpp backgrounds, 2-bpp planes/viewports, raw OAM sprites,
fixed-color addition, signed Mode 7 projection and ordered brightness, ordinary and
mixed-mode gameplay, color effects, messages and windowed child scenes. Full retail
scene verification and live performance/recovery qualification are still pending.
Desktop selection and a dedicated GPU/presentation owner are implemented; see
`RENDERER_MIGRATION.md` for the current evidence matrix and remaining gates.
The console verification project never substitutes the CPU reference for GPU work.

The portable INI contract now accepts `[Video] Renderer=Software|Direct3D11|Auto`
(case-insensitive names; numbers, combinations, duplicates and unknown keys fail).
Software remains the temporary migration default. Direct3D11 selects the hardware
worker; Auto logs hardware startup failure before selecting software. Neither mode
silently substitutes CPU raster output for an unsupported GPU scene. Existing local
INI files are unchanged.

`D3D11FrameRenderer.Render` submits composition without readback or staging allocation.
`Readback` is an explicit diagnostic operation; `RenderForReadback` combines them for
comparison tests. Staging storage is allocated lazily, so child scenes and future
presentation-only consumers do not allocate CPU-readable textures. Solid smoke tests
cover 64 submissions before readback, repeated readback and the uninitialized guard
on both devices in Debug/Release. The desktop uses Render without normal readback.

The portable `RenderPresentationGate` provides atomic generation-check/presentation
ordering relative to load/reset. Its lock is separate from `LatestRenderFrameMailbox`:
ordinary publication never waits on presentation. Desktop load/reset asynchronously
awaits a presentation already inside the gate, then rejects that generation before
replacing game state. Playback and frame-affecting controls are suspended during this
handoff, but the UI message pump remains free. GPU completion and
waitable-swapchain waits must stay outside the gate. Component tests cover stale work,
concurrent reset, exceptions and reentrant reset rejection. Desktop load/reset is
wired to this boundary; hidden tests hold the gate while actual Load State and Restart
handlers return incomplete tasks, service UI continuations, and preserve old state
until release. Retained-pixel and stale-generation assertions pass afterward.
Visible Release soak evidence is recorded separately; real device/RDP transitions
still require qualification.

The GPU display pass samples the integer native frame directly into a BGRA render
target, using nearest-neighbor scaling and the desktop's centered 4:3 TV correction
with integral vertical scaling (small clients clip the minimum-sized image). Its
vertex/pixel shaders are build-generated with the same pinned compiler. No CPU frame
copy is involved. `--display-smoke` verifies eight target sizes from 1x1 to 1920x1080
against a coordinate/color oracle, then checks compute rendering after each display
pass for binding hazards. Hardware/WARP pass Debug/Release. This is an offscreen
display-target test, not an actual swapchain, DPI lifecycle or RDP recovery test.

`D3D11SwapchainPresenter` owns a two-buffer flip-discard BGRA swapchain with a
frame-latency waitable object and maximum latency one. Readiness polling is nonblocking;
rendering and waits stay outside the generation gate, which wraps the actual Present
call. Renderer device/identity must match. Occlusion is an explicit result, not a
successful displayed frame. Positive-dimension resize releases and recreates the view.
`--swapchain-smoke` uses a hidden HWND, checks creation/readiness, three resizes,
mismatched identities and stale generations, then disposes everything. Both devices
pass Debug/Release and return Occluded as expected. Visible presentation, minimization,
DPI/RDP transitions and production-clock pacing remain unverified. Device-loss
recreation now has injected-HRESULT tests; those are not actual driver-reset evidence.
Device-loss diagnostics now query the original device before disposal and report
the thrown HRESULT, device removal reason, adapter/backend, target size and retained
frame identity. `LastDeviceLoss` retains that immutable context across recreation.
Injected failures correctly show removal reason S_OK because the physical device
was not removed; the test asserts this distinction rather than fabricating a reason.

`D3D11RenderWorker` owns device/render/present/disposal on one background thread.
Publication sends owned snapshots through the latest-frame mailbox, resize requests
coalesce, zero client size suspends drawing, and faults surface through Ready,
Completion and ThrowIfFaulted. Hosts must asynchronously await startup/shutdown while
keeping the HWND and message pump alive. An acquired latency opportunity is retained
when no frame is queued; consuming it too early would stall first publication.
The hidden swapchain suite also tests idle startup followed by 1001 publications,
new-generation resize, bounded mailbox accounting and shutdown on both devices in
Debug/Release. The desktop game/audio loop now publishes to this worker. The hidden
tests do not establish visible presentation or the complete performance gates.

`--slow-consumer-audio` now covers a bounded real-game slow-consumer test: separate
legacy/captured game owners, the actual desktop managed audio adapter, and a GPU
worker held after taking a frame for at least 250 ms. A 160-frame single-room/pause
slice continues while blocked; 872,000 PCM samples (814,672 nonzero, including startup)
match exactly, along with commands, frame/state and Samus position/pose. At least 159
visual packets are superseded and the worker resumes with the latest. Both devices
pass Debug/Release. This is not host wall-clock pacing, a five-minute soak, or complete
cross-backend state/PCM coverage. The test compiles the desktop adapter source directly
to avoid a duplicate audio implementation or a WinForms dependency.

The worker retains one consumed packet for redraw after resize and occlusion, so a
paused producer need not manufacture a game/audio tick to refill a recreated target.
Retained packets are checked against the mailbox generation and the final Present
gate. Hidden worker tests assert a redraw at the new target size with unchanged
publication/consumption sequence. Visible expose/recovery remains a separate gate.

## Reproducible inputs

`powershell -NoProfile -File csharp/tools/verify-render-publish.ps1` performs a
locked Release restore/publish using a fresh GUID-named artifacts tree, rebuilding
all four embedded shaders from source. It runs solid, ordinary gameplay and display
tests on hardware and WARP from the published directory, with no ROM or loose shader
files. The display fixture is copied alongside the diagnostic and resolved relative
to its application directory, not the repository working directory. Dependency
notices are required in the output. This gate passed September 7, 2026: 64 solid,
96 ordinary and eight display-size comparisons per device. Outputs are retained
under `test-temp/render-publish` for inspection; they are disposable diagnostics,
not alternate playable builds. This verifies renderer packaging, not the complete
desktop/audio-asset distribution or a fresh machine's prerequisite installation.

- .NET SDK 10.0.400, as used for current project verification.
- Vortice.Direct3D11 exactly 3.8.3, with checked-in `packages.lock.json` files.
  Use `dotnet restore csharp/src/SuperMetroid.RenderVerification --locked-mode`.
  The resolved Vortice packages are 3.8.3, Mathematics 2.1.0, and SharpGen Runtime/COM
  2.4.2-beta. The beta is an upstream transitive runtime dependency, explicitly
  recorded here rather than represented as entirely stable dependencies.
- Windows SDK directory 10.0.26100.0, x64 FXC file version 10.0.26100.8249,
  SHA256 `005EFF830845789C7EFB2831A0B41950EE6954E9BCD93BAF50DE67AD537728B2`.
  `ShaderBuild.targets` verifies the compiler bytes and compiles source before
  resource discovery with `/Ges /WX /O3 /T cs_5_0`. The bytecode is embedded in the
  assembly, not compiled at runtime or loaded from an untracked binary.
  Other serviced compiler versions fail the build and need an explicit reviewed
  tool-pin update. The SDK/compiler is not redistributed in this repository.

Run `dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --solid-smoke`
and repeat with Debug. Native error boxes are disabled and managed exceptions go
to stderr with exit code one. There is no interactive window or game launch.

## Dependency and ownership review

[Vortice 3.8.3](https://www.nuget.org/packages/Vortice.Direct3D11/3.8.3) provides
managed D3D11/DXGI COM bindings, not an application-specific native renderer DLL.
Its package source revision is `9e609cb9439c9872aa1b339f177e40ec96f77239`.
Vortice/Mathematics and SharpGen package manifests declare MIT licenses; attribution
and license text are included in `D3D11_THIRD_PARTY_NOTICES.txt` and copied to output.
Native D3D11/DXGI runtime and display-driver behavior remain Windows dependencies.

The device, immediate context, shaders and views have explicit disposal. Constructor
failure releases previously created resources. Calls on a different managed thread
are rejected before submitting context work, consistent with Microsoft's
[D3D11 threading rules](https://learn.microsoft.com/en-us/windows/win32/direct3d11/overviews-direct3d-11-render-multi-thread-intro).
This is ownership enforcement, not the still-pending simulation/render scheduling split.

Hardware mode explicitly enumerates a non-software DXGI adapter and creates the
device with that adapter. A default driver-type request was experimentally found
to select Microsoft Basic Render Driver in this environment; that route is not used.
WARP must be explicitly requested and is reported separately. No automatic fallback.

Current smoke coverage: exact RGBA for 64 solid/fade/alpha combinations on each of
RTX 3090 and WARP, plus wrong-thread rejection, in Debug and Release. These results
are functional checks, not performance gates or a claim that the game renders on GPU.

`--tile-smoke` additionally checks 108 geometry/priority/scroll/transparency/color
cases plus a focused packed-byte regression on both devices. `--compare <frame.smframe>
--device hardware|warp` compares a portable fixture on the explicitly selected device.
Failures write the packet, expected/actual/difference PNGs, first pixel, mismatch count
and bounding rectangle into a unique `csharp/test-temp/render-comparison` directory.
Unsupported layer types fail before GPU submission; there is no raster fallback.

`--obj-smoke` checks 197 exact comparisons per device in Debug and Release:
all eight size modes, varying name-table selection, zero through 128 modeled sprites,
mixed BG/OBJ priority insertion and deliberately overlapping sprites with transparent
holes. The GPU resolves the first opaque OAM pixel before filtering its BG priority,
so filtering cannot resurrect a later hidden sprite. Raw OAM bytes are uploaded;
no CPU sprite raster is supplied. These remain correctness checks, not GPU timings.

`--mode7-smoke` checks 241 exact comparisons per device in Debug and Release,
including signed matrix/offset extremes, transparent overflow, character-zero fill,
disabled background, OBJ composition and ordered-layer composition. Projection and
interleaved tile/character sampling execute on GPU from raw memory/registers. These
comparisons establish software-reference parity, not independent cartridge fidelity.
This includes 48 mixed-mode gameplay HUD/floor cases. Physical scanline clipping
keeps projection coordinates intact; the floor composites from backdrop with its
own BG/OBJ ordering, and HUD color zero is opaque. Retail scene-matrix coverage
and overlays/effects on these bands remain to be verified as the backend expands.

`--window-smoke` checks 254 exact color-effect/message comparisons on each device in
Debug and Release. Coverage includes empty and single-pixel windows at both edges,
inclusive endpoints, byte saturation, repeated overlays, fades and both Ceres haze
captures. Window endpoint/color data is uploaded, not a CPU-rendered overlay. Other
effect layer kinds and retail effect-scene coverage remain pending. The suite also
includes 64 scrolling 2-bpp BG add/subtract cases: both map heights, per-line register
changes, VRAM/scroll wrapping, bottom-line clipping and disabled planes. The GPU
samples raw tile/character memory and clamps expanded-byte arithmetic per component.
Another 24 comparisons cover five-bit subscreen addition with optional 4-bpp main
coverage, all map geometries and priority filters. This path samples the coverage
on GPU and uses five-bit addition rather than the room-effect byte-domain equation.
The additional 100 message cases cover every supported row count (3–6) and reveal
radius (0–24), random tile flips/palettes and the two temporary message colors.
Glyphs are decoded on GPU from captured VRAM; gameplay CGRAM remains unchanged.

`--ordinary-smoke` checks 96 exact ordinary gameplay comparisons on both devices in
Debug and Release. All eight BG1/BG2/OBJ enable combinations, three BG2 geometries,
and independent X/Y HDMA tables are covered. The GPU composes the Mode-1 priority
ladder below an opaque HUD, using physical scanline scroll registers. This establishes
reference parity for constructed inputs; retail gameplay capture coverage, desktop
integration and timing gates remain outstanding.

`--scene-window-smoke` checks 112 exact comparisons per device in Debug and Release:
empty/full/one-pixel/interior rectangles, all child brightness levels, distinct parent
and child memory, repeated insertions and parent overlays after child rendering. One
reusable child renderer bounds scratch resources; GPU region copies replace pixels
including backdrop. No child raster is read back or uploaded from the CPU.

`--retail-frontend` requires `Super Metroid.smc` in the working directory (absence
fails, not skips). On each device in Debug and Release it compares 131 sampled
natural-title frames, including zoom, and all 251 frontend frames through title,
file select, options and intro handoff directly against the legacy live renderer.
Paired simulation instances also compare frontend state, frame identity and ordered
audio commands. SRAM is private in memory. PCM output, other scenes and host pacing
are not established by this test.

`--retail-intro` compares 275 samples across all 34 intro phases, through completed
Ceres flight, using independent legacy and captured display owners. Each retained
packet is also rendered after the simulation advances. Samus coordinates, projectile
count, Mother Brain hits and phase agree between owners. The verification assembly
has internal capture access, like the existing portable verification harness; the GPU
production assembly does not. This is not a PCM or desktop scheduling test.

`--retail-transitions` checks 143 Ceres destruction/Zebes approach samples across
15 phases and 101 game-over frames covering both choices. Each packet is compared
again after scene advancement. Mode-1 and Mode-7 coverage and terminal outcomes are
required. These direct live-renderer comparisons pass hardware/WARP in Debug/Release;
they do not cover gameplay door transitions or prove independent cartridge fidelity.

The first tile comparison caught a pinned-FXC optimization problem in dynamic byte
extraction. `/Od` passed; disassembly and shader probes showed `/O3` selecting the
wrong byte. Two explicit byte-selection steps preserve optimized compilation and
pass the archived fixture in `test-fixtures/issue-321-tile-byte-selection`.
