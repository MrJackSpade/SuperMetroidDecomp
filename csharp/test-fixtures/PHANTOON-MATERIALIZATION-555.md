# Phantoon materialization reproduction (#555)

Affected player version: 0.1.1. Investigation checkpoint, not a completed fix.

Run DebugRunner `--phantoon-materialization-audit ROM LOCAL-OUTPUT-DIRECTORY`.
This diagnostic currently **must fail** at its final wave assertion; it is not
registered as a passing general-suite test. Do not mark #555 awaiting validation.

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

Pinned source leads:

- `$A7:D508` starts materialization and calls `$88:E487`, spawning wave HDMA.
- `$88:E4BD` initializes its table/phase; `$88:E567` computes BG2 horizontal
  offsets per scanline from sine-table values and the mouth actor's amplitude.
- Managed Phantoon changes amplitude and `SemiTransparencyLayerFlags`, but its
  runtime handoff currently publishes only scalar BG2 scroll registers.
- `GameplayDisplayCapture` publishes water/lava/sky horizontal offsets, not
  Phantoon's wave. Its translucency flag currently has no rendering consumer.

Next: translate the native wave initialization/update and blending setup with
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

The builder alone is not the gameplay fix. Native HDMA lifecycle, accepted-frame
publication, screen-row expansion and transparency integration remain to be wired.
No claim that the materialization regression passes yet.

Initialization clarification: $A7:D535 calls the HDMA spawn/reset *before*
$A7:D539-$D53C assigns the intro maximum amplitude. The existing maximum-amplitude
assignment is therefore not itself evidence of a bug. Native HDMA setup also
copies the pending wave mode into the eye record; the missing mode handoff must
be preserved when integrating this owner.
