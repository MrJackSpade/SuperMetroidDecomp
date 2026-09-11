# Candidate enemy audio investigation (#550)

Status: unresolved diagnostic, not a gameplay fix or player reproduction.
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
