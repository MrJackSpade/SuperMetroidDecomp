# Phantoon materialization reproduction (#555)

Affected player version: 0.1.1. Investigation checkpoint, not a completed fix.

Run DebugRunner `--phantoon-materialization-audit ROM LOCAL-OUTPUT-DIRECTORY`.
The missing introductory wave was reproduced before the integration below. The
diagnostic now passes; it remains a separate real-room audit. Do not mark #555
awaiting validation until the other reported visual properties are verified.

The diagnostic loads the real Phantoon room ($8F:CD13), places Samus on its floor,
then runs 1000 production runtime frames with no input. It does not set the boss's
AI phase, timer, palette or amplitude. Every qualifying introduction-wave frame
is rendered and saved locally; CSV records phases, amplitude, blending flags and
published per-row scroll data. Do not upload these images or ROM/state assets.

Observed before fix: 165 active-amplitude WavyFadeIn frames, zero frames with
per-row horizontal-scroll variation. At frame 575 the amplitude is 3072, blending
flags $4000, and the render packet contains zero horizontal-scroll rows. Frame
620 shows the body materialized but still spatially undistorted. Both images
were visually inspected. This reproduces missing introductory shimmer, not the
later black silhouette or fade-out symptoms.

Pinned source leads (before integration):

- `$A7:D508` starts materialization and calls `$88:E487`, spawning wave HDMA.
- `$88:E4BD` initializes its table/phase; `$88:E567` computes BG2 horizontal
  offsets per scanline from sine-table values and the mouth actor's amplitude.
- Managed Phantoon changes amplitude and `SemiTransparencyLayerFlags`, but its
  runtime handoff currently publishes only scalar BG2 scroll registers.
- `GameplayDisplayCapture` publishes water/lava/sky horizontal offsets, not
  Phantoon's wave. Its translucency flag currently has no rendering consumer.

The initial investigation plan was to translate wave initialization/update and blending setup with
correct frame ownership, compare its exact table output against original code,
then rerun this visual reproduction. Independently reproduce fade-out and the
black/translucent phase before claiming the whole issue resolved.

## Original-CPU scroll-table checkpoint

`native-phantoon-hdma-probe.h` executes original ROM routines $88:E4BD and
$88:E567 using the bounded CPU harness. It samples both modes, amplitudes 0,
$40, $100, $340, $C00 and $FFFF, and 40 successive phase updates per case.
The headless entrypoint patch suppresses native dialogs and must be reversed
after building/running the diagnostic. It was reversed after this capture.

`--phantoon-wave-comparison-audit ROM LOCAL-CSV` verifies all 46,080 scroll words
across 480 cycles against `PhantoonWaveTable.Build`. All match. This is the
original 65816 calculation, not an upstream C formula used as the oracle.
The builder truncates unsigned product magnitude before applying the sign, then
mirrors the first half-cycle and wraps the base scroll as a native word.

At this checkpoint the builder alone was not the gameplay fix. Lifecycle,
accepted-frame publication and row expansion were subsequently integrated below;
transparency remains unfinished.

Initialization clarification: $A7:D535 calls the HDMA spawn/reset *before*
$A7:D539-$D53C assigns the intro maximum amplitude. The existing maximum-amplitude
assignment is therefore not itself evidence of a bug. Native HDMA setup also
copies the pending wave mode into the eye record; the missing mode handoff must
be preserved when integrating this owner.

## Original-CPU lifecycle checkpoint

The native probe additionally writes `OUTPUT.lifecycle.csv`. It executes the
actual $A7:D508 transition (timer one), then the original $88:851C HDMA instruction
handler. It disables the mode at call five and, like the native outer handler,
does not call an already deleted channel. The corrected probe completes cleanly.

Observed sequence:

- Immediately after the AI transition: pending mode 2, active mode 0, amplitude
  3072, instruction timer 1, instruction list $E4A8, channel enabled.
- First HDMA call: active mode becomes 2; phase becomes $FFFE; list reaches
  $E4B9 (sleep); no scroll-data calculation has occurred yet.
- Following calls: phases 14, 30, 46, 62 and first scroll samples 2, 4, 6, 8
  with zero base scroll. The instruction timer underflows during sleep, as native.
- Disabling active mode deletes the channel during that same call; it leaves
  phase and scroll data unchanged. Later outer-handler calls skip that channel.

`RunOneFrameOfGameInner` calls HDMA before enemy AI. Runtime capture must latch
the prior completed effect data alongside the other NMI presentation state;
feeding live post-AI amplitude into render capture would change that ownership.
The first setup-only call must not be collapsed into an immediate wave update.
These original-CPU observations are now covered by the integrated lifecycle test.

## Integrated introductory wave checkpoint

The runtime now advances the separate bank-$88 wave owner before enemy AI and
latches its completed scroll data at NMI. Both gameplay rendering paths consume
that same latched per-row BG2 table. The native setup-only first call is retained;
the mode handoff also prevents the body from updating its scalar anchor while
wave HDMA owns BG2. Intro and death call sites schedule their native wave modes.

The real-room audit now finds 165 qualifying amplitude frames and 162 frames
with varying horizontal scroll rows. It compares each varying frame against an
otherwise identical undistorted render: gameplay pixels must change while HUD
pixels remain identical. This verifies the introductory wave, not translucency.
The ordinary-base capture intentionally does not establish final color blending.

Core verification covers original-CPU lifecycle values, immutable display
latching, exact debugger-state round-trip, deletion, and legacy field migration
selection. Legacy states have no recorded wave history: loading them emits an
explicit warning and leaves that missing owner inactive until its next native
spawn. No exact reconstruction of a legacy mid-wave phase is claimed.

Core verification and ordinary render parity pass (96 gameplay comparisons and
128 window frames each on hardware and WARP). Final-death wave wiring still needs
full battle verification. Later reappearances, fade-out, and the reported black
silhouette/background contribution remain unfinished; #555 stays open without
the awaiting-player-validation label.

## Full-frame black silhouette reproduction

Run `--phantoon-transparency-audit ROM LOCAL-OUTPUT-DIRECTORY`. This separate
diagnostic initially **failed intentionally**; the integrated checkpoint below now passes.
It uses the same unforced real encounter but runs 2400 frames and captures full
gameplay composition rather than the ordinary base alone. It checks 1363 frames
with the native semi-transparency flag set. A single introductory frame (620)
did not reproduce the black silhouette; the extended sequence does.

At frame 1936, phase $D7F7, 1974 otherwise colored scenery pixels are replaced
with black. The saved full-frame image visibly shows the opaque black body;
the corresponding image with BG2 removed shows the scenery underneath. Both
were inspected locally. CSV includes every tested frame and the first erased
pixel coordinate. Images remain local and must not be published.

Cartridge $88:E449 selects blending configuration $1A while the flag is set.
The pinned disassembly's $88:80D9 sets main screen $15, subscreen $02, and
CGADSUB $35: body BG2 belongs to the additive subscreen. Black cannot erase
the main scene in that operation. Current full capture instead leaves BG2 on
the opaque ordinary main screen and never consumes the transparency flag.

This establishes the black-silhouette reproduction, not a completed blending
fix. Its no-erased-color assertion alone would also pass if the body were
incorrectly hidden altogether. The implementation must additionally verify
nonzero body-color contribution, source-sensitive OBJ behavior, frame ownership,
and the cartridge's hidden/opaque/translucent transitions; do not substitute
this single negative assertion for those positive fidelity checks.

## Shared compositor prerequisite

Source-aware gameplay composition now supports selecting its BG2 plane as the
additive subscreen operand. This extends the existing window/color-math operation
rather than applying a whole-frame tint. Both software and Direct3D sample the
same BG2 geometry and per-scanline scrolls. BG3 and BG2 operands are mutually
exclusive and invalid descriptor combinations throw.

`RenderVerification --xray-window` passes on hardware and WARP: 177 source-aware
cases and 176 window cases per device. New independent scalar checks verify
nonzero BG2 color addition, black identity, HUD preservation and all eight OBJ
palette groups. Patterned scenes exercise BG2 scrolls/arithmetic; version-25
packets round-trip, and constructed version-24 packets retain their old behavior.
Core verification also passes. This prerequisite alone does not change the live
Phantoon encounter: timed native blend-state publication still needs integration.

## Integrated translucency checkpoint

The independent $A7:CE96/$88:E449 owner now runs before enemy AI. Its two setup
calls precede the first pre-instruction. Each pass starts from the room blend
default; the native flag selects translucent mode, low control zero selects
hidden, other low control values preserve that pass's default, and $FF selects
hidden and deletes the owner. Flag priority over $FF is preserved. NMI separately
latches the completed configuration, so rendering never consults post-AI flags.
Both immediate software rendering and captured GPU frames use the same source-aware
composition. Debugger state preserves this owner; older layouts emit a loss-aware
warning and restart absent blend setup rather than pretending its history exists.

Visual inspection caught an independent diagnostic setup defect: the Ceres
bootstrap's queued $4000..$4FFF upload overwrote the newly loaded boss map on the
first NMI. Opaque BG1 had masked this outside-body garbage. The fixture now drains
that bootstrap transfer before loading Phantoon, and asserts the unused BG2 map
word remains native blank tile $0338. This is a fixture correction, not an in-game
room-loader change. With the corrected fixture, the opaque-render control still
reproduces exactly 1974 blackened scenery pixels, preserving the original finding.

The 2400-frame integrated run checks 1363 flagged frames: zero blackened scenery
pixels; 1353 latched additive frames, 1094 with positive body-color contribution.
Every additive frame preserves the HUD and never darkens a background channel.
Frames 620 and 1936 were visually inspected and match exactly on hardware and WARP
after packet serialization. Captures remain local. Intro wave remains 165/162;
core verification and the 1400-frame positioning regression pass. Lifecycle tests
cover setup, control-byte semantics, latch isolation, deletion and debugger restore.

The reproduced silhouette is fixed. #555 remains open pending verification of
the rest of its fade-out/reappearance and final-death visual scope; these checks
do not yet establish complete parity throughout every battle branch.

## Original-CPU palette-fade correction

Ordinary fade-out/reappearance paths call palette routines $A7:D464/$D486;
they do not all spawn a wavy HDMA channel. The pinned source has wave spawns at
the introduction and final death. Do not add a bespoke wave to every fade.

The native probe now also emits `OUTPUT.fade.csv`, executing those original
65816 wrappers for fade-in/out, denominators 1/12, health 1/312/313/2496/2500,
and 40 consecutive NMI counts. It captures all 16 colors and numerator/completion
latches. `--phantoon-fade-comparison-audit ROM LOCAL-CSV` invokes the production
callbacks and checks the pinned capture: 800 calls and 12800 colors.

Before correction, the comparison failed at fade-out denominator 1, health 1,
frame 2, color 0: managed color 7 versus native 0. The old formula used
`denominator - numerator + 2` and truncated signed integer division. Cartridge
$A7:DCF1 instead divides the absolute delta in 8.8 fixed point by the eight-bit
`denominator - numerator + 1`, applies the sign, then selects the result's high
byte. This also rounds decreasing colors differently. Production now uses the
native arithmetic, including the register's divide-by-zero result.

All 12800 colors and 800 timing states now agree. The final rebuilt native probe
reproduces SHA256 A5C86B65561B3CC7695125294D5F54DB41A636F85E711949344FEF9C7883E2A0;
its temporary upstream patch was reversed. Core verification includes fixed
original-CPU color expectations without needing the local trace, and passes.
The real-room wave remains 165/162 frames; transparency remains 1363 flagged /
1353 additive / zero erased pixels. Positive-contribution frames change from
1094 to 1084 because the corrected native rounding removes the incorrect faint
fade tail. The old opaque control still reproduces 1974 erased pixels.

Final-death wave/mosaic composition and remaining full-sequence visual checks
are still pending, so #555 is not yet awaiting player validation.
