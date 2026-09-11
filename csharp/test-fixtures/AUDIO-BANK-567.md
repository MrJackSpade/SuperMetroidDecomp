# Audio bank transition regression (#567)

Affected release: 0.2.0.

Reproduction through `CartridgeAudioRenderer`: upload the common engine, upload
Empty Crateria, select track 5, generate 600 frames, upload the captivity bank,
select track 5, and continue generating audio. Before the fix this raises the
reported missing-source `$1A` exception in `DecodePcm`.

At failure voice 2 has SRCN 26, release envelope, gain zero, output zero and
previous END/LOOP flags 3. Its new directory entry is `FF FF FF FF`. The
extracted bank correctly has no playable waveform starting at `$FFFF`.

Pinned `vendor/sm/src/snes/dsp.c:dsp_decodeBrr` continues reading wrapping APU
RAM after END, even for a released voice. It does not perform a catalog lookup.
Release at zero gain cannot become audible through envelope/register changes;
only key-on restarts it. The managed implementation now follows raw block headers
for this retired, unmapped case, retaining address wrapping and ENDX. Sample
predictor work is unnecessary because all output is multiplied by zero. Key-on
clears the retired cursor and requires an ordinary mapped PCM sample. Audible
missing mappings still throw. No WAV replacement or asset regeneration is needed.

Verification:

- `--audio-bank-transition standalone-assets/audio`: all 24 music banks, 600
  pre-transition and 600 post-transition frames each; narration remains audible.
- Standard core suite includes the small deterministic retired-voice fixture:
  zero release, invalidated map, wrapping header processing/ENDX, silence,
  successful subsequent key-on, and loud rejection of an audible missing source.
- Standard core verification suite passed.
- Windows Release build passed with zero warnings/errors.
- `--github-error-reporter-audit`: playback-boundary failures continue, repeated
  reports deduplicate, reporting-disabled and reporting-failed paths do not throw,
  and a later successful audio attempt executes.

Audio rendering/submission now has a separate nonfatal host boundary. It logs the
failure locally, uses the existing GitHub reporter when enabled, and preserves
the already-completed gameplay/display frame. This is containment, not a claim
that arbitrary partially executed audio state has been repaired. It does not
fabricate replacement audio or change gameplay exception policy.

Player confirmation remains required. The reproduction above establishes the
same failure, not the player's exact preceding input sequence.
