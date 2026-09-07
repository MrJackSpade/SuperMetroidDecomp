# Renderer migration — issue 321

Status: software packet extraction in progress. No GPU backend is implemented
or selected by this document. The acceptance authority is
[issue 321](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/321).

## Inspection baseline

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
