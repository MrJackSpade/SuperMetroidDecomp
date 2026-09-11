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
