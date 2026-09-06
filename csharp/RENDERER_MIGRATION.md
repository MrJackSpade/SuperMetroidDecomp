# Renderer migration — issue 321

Status: inventory and architecture gate in progress. No GPU backend is implemented
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

Paths below are relative to `src/SuperMetroid.Core`. Every row is **pending** for
packet extraction, reference parity, D3D parity and real-state qualification.
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

The ledger must gain fixture identifiers and test results as each row is migrated.
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
