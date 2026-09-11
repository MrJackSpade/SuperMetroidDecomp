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
