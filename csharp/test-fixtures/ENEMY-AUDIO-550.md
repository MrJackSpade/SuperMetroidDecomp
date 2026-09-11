# Candidate enemy audio investigation (#550)

Status: player report unresolved; two independent PCM loop handoff defects corrected.
The player reported an unidentified floor snake's Ice Beam-like sound on 0.1.1.
Yapping Maw remains a candidate only.

## Routing check

`--yapping-maw-audio-audit ROM` checks the actual ROM bytes for the immediate
sound ID and full JSL target at $A8:A13E: library two/$2F, QueueSound_Lib2_Max6
($80:90CB). It executes the attacking-list opcode through the production enemy
dispatcher with on-screen and off-screen camera positions. The visible case
queues exactly one Max6 request; the off-screen case queues none. Neither queues
a library-one weapon sound. Both cases pass.

## Standalone playback comparison

`--sound-native-audio-audit AUDIO_DIRECTORY NATIVE_DLL LIBRARY SOUND_HEX`
compares 180 frames of generated PCM and port acknowledgements. It loads the
common SPC engine, writes the selected sound on frame zero, and clears its input
port on frame one. A passing case must also produce nonzero PCM. The oracle is
the project's native audio translation, not an original SPC CPU or a listening
test through the desktop audio endpoint.

- Control: library one/$0B passes all 180 frames; peak 8048.
- Candidate: library two/$2F fails at frame 21, stereo sample index 682:
  managed 8, native 15. At that frame the compared final DSP registers match.

This small first divergence does not establish either its audible significance
or a link to the reported Ice Beam-like sound. The standalone command deliberately
fails on this known mismatch; it is not a passing regression test. Investigate
sample/envelope playback and the full difference interval before changing sound
selection or declaring the player report resolved. The room/enemy/action identity
still needs evidence from the reported encounter.

No ROM, samples, PCM, state files, or screenshots are published with this fixture.

## Live source correction (partial engine fix)

The native `upstream-sm/src/snes/dsp.c` decoder reads the live DIR/SRCN entry
at each END/LOOP, retaining its preceding interpolation and BRR predictor history.
The managed decoder instead retained the sample selected at key-on indefinitely.
The SFX driver restores a source number during release, so this distinction is
observable even without another key-on.

A constructed positive/negative looping-source test reproduced that stale source:
write SRCN during a note, preserve its current output, then require the next loop
to play the new source without KON. It failed before the change and passes after
resolving the current sample bank/source at the loop boundary. The full managed
verification suite also passes with this correction.

This is not complete PCM parity. The candidate comparison's first differing sample
moves from 682 (8 versus 15) to 738 (1 versus 0) in frame 21. The full native corpus
also has a pre-existing `map-scroll-confirm-overlap` failure: before the change,
frame 4/sample 1448 is -3459 versus -3457; afterward, the first difference is at
sample 1476 (15 versus 12). Both baseline and corrected runs were executed; the
remaining corpus failure is not a newly introduced regression.

Do not mark #550 resolved on these results. Further work must examine transition
predictor history and extracted loop representation, compare complete PCM rather
than merely moving the first failure, and establish the actual reported encounter.

## Complete comparison after the live-source correction

`--sound-native-audio-survey AUDIO_DIRECTORY NATIVE_DLL LIBRARY SOUND_HEX`
continues through the complete 180-frame standalone sequence instead of stopping
at its first PCM difference. It still checks every port acknowledgement and returns
failure if any PCM differs; it does not turn the known failure into a passing test.

For library two/$2F, only frame 21 differs: 166 interleaved stereo sample values,
maximum absolute delta 55, whole-run RMS delta 0.4703, managed peak 4015. The other
179 frames match exactly. All acknowledgements match. Control library one/$0B has
zero differing samples, maximum and RMS delta zero, and peak 8048.

This localizes the remaining mismatch to a brief release transition, not the whole
attack sound. These measurements alone cannot establish perceptual audibility or
identify the player's enemy. The unfolded WAV loop encodes a particular predictor
history; switching to another source can supply a different history in the native
decoder. This is the next sample-representation seam to test, not yet a proven
complete explanation or an excuse to accept approximate parity.

## Constructed predictor-history reproduction

`--dsp-source-transition-audit NATIVE_DLL` needs no ROM or extracted assets.
It creates two private one-block looping BRR sources: filter-zero constant 1024,
then zero residuals with either filter zero (control) or filter one. From reset,
the second source decodes to the identical all-zero WAV for both filters. After
switching SRCN at tick 40 without KON, native filter one retains the preceding
predictor history and decays; the PCM source has already lost that information.

The probe runs the vendored native DSP decoder and managed DSP directly, with
identical register writes, pitch and direct gain. The filter-zero control matches
all 320 OUTX ticks. Filter one differs on **32 ticks**, first at tick **48**:
managed OUTX $06 versus native $07. Both cases require a nonzero initial signal.
The command deliberately exits **1** while either case differs; it is not included
as a passing standard regression and does not use a tolerance.

The diagnostic bridge adds explicitly named bounded RAM/register writes and a
single-cycle DSP call. Invalid addresses/null targets are rejected, and cycling
stops before overflowing the frame buffer. These exports affect only explicitly
allocated diagnostic instances; normal driver generation and playback are unchanged.
The runtime still uses managed audio, not this DLL. Native vendor files remain
untouched. Build the bridge before running this new command.

This establishes a real limitation of PCM-only loop transition representation,
independent of the reported enemy. Exact stock transitions need information about
the original loop boundaries and predictor operation/history, or an equivalent
precomputed representation that preserves all admitted incoming histories. Merely
switching WAVs or adjusting pitch cannot recover information absent from the WAV.
Any correction must keep ordinary HD PCM replacement supported and avoid imposing
stock BRR filtering on replacement audio. No production correction is made
here; tracing the actual $2F release against this mechanism remains necessary.

## Independent directory loop entry correction

The stock release mismatch was not the constructed predictor-history case above.
At frame 21 the driver restores voice seven SRCN to zero. Common-bank source zero
starts at $6E00 but its DIR loop address is $73C4, the start of source one. Source
zero's WAV is non-looping. The previous managed handoff restarted that WAV instead
of entering source one. The destination BRR header uses filter zero, discarding
incoming predictor history; the constructed filter-one limitation is independent.

The asset loader now derives cross-source loop aliases from existing manifest
start/loop addresses. The DSP resolves that entry at END/LOOP without changing
key-on lookup or the preceding interpolation window. Explicit WAV loops, including
replacement loops, keep their own entry. Unknown/internal loop destinations and
arbitrary predictor histories are not solved by this change: the existing fallback
remains for those entries. Do not claim complete BRR-address representation.

The constructed `--pcm-loop-entry` regression fails with the old decoder and passes
with the corrected one: a live source change must enter negative PCM at its separate
loop address, whereas a new key-on must play positive PCM at its start. Invalid
alias targets are rejected. Sources are long enough to observe the transition
before their non-looping end silences the voice.

After this correction, the complete 180-frame library-two/$2F survey has **zero**
differing samples (maximum/RMS delta zero, peak 4015); all port acknowledgements
match. Library-one/$0B still matches all 180 frames, peak 8048. The broader native
corpus matches 4680 complete frames before its next difference, Upper Crateria
music frame 320/sample 1358 (managed 590, native 589). Its former map-menu overlap
failure is no longer the first failing scenario. Full managed verification passes,
and the Windows desktop builds with zero warnings/errors. This remains a native
translation comparison, not original-SPC-CPU or endpoint listening evidence.

Issue #550 stays open without a validation label: the candidate sound now agrees,
but the player's exact enemy, action and Ice-Beam-like sound remain unconfirmed.
