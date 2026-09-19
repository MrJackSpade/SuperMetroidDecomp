# Editable audio instruments (#548)

## Ownership boundary

The lossless `.spcu` streams still contain the native SPC driver, music sequences,
sample-directory data, and a stock copy of each instrument table. The readable
`audio-manifest.json` bank records now supersede only the 42 six-byte instrument entries at
SPC `$6C00`: source/noise selection, ADSR1, ADSR2, gain, and the big-endian pitch-base word.
The renderer applies the complete validated manifest table immediately after every common or
music-bank upload and before that bank can produce another audio frame.

The manifest also supersedes the resident authored SFX channel programs. Stable effect IDs,
pointer-table addresses, voice allocation, and fixed program slots remain compiled mechanics;
270 decoded programs expose 1,321 named note/envelope/slide/repeat operations. Recompilation
must fit each original slot exactly and runs after every upload before another audio frame.

This is a content seam, not a new synthesizer path. The existing managed SPC sequencer and
S-DSP continue to consume those six bytes through their native instrument command, so edited
pitch, envelope, gain, sample selection, and noise rate retain the ordinary dynamic voice,
stealing, cancellation, echo, and modulation behavior.

## Validation

`Program.AudioInstruments` verifies all 1,050 stock instrument records (25 banks times 42)
byte-for-byte against the merged common-plus-bank upload image. It then copies the extracted
catalog, changes every Title Sequence pitch-base value only in the JSON manifest, reloads the
catalog, and proves:

- all edited words are installed in APU RAM after the ordinary renderer upload;
- both stock and edited Title Sequence playback remain audible;
- the two deterministic 600-frame PCM streams differ;
- an incomplete 41-record bank is rejected;
- a `usesNoise` value inconsistent with the packed selector's high bit is rejected.

The focused command is:

```powershell
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --audio-instruments
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --audio-sfx-programs
```

`Program.AudioSoundPrograms` additionally proves every decoded program recompiles byte-for-byte,
all 240 effects retain native routing, a JSON-only note edit reaches live APU RAM and changes
audible PCM, and malformed operations, capacities, or references fail explicitly. Installed
edits live in the persistent `overrides/audio` catalog and survive stock repair.

Music extraction covers 159 stable tracks, 263 phrase tables, 1,695 channel/subroutine programs,
and 103,046 named instructions across all 24 music banks. `Program.AudioMusicPrograms` proves the
complete compiled byte image and track-pointer routing match stock, then changes one Title music
note only in JSON and observes the live APU edit and changed audible PCM. Fixed-capacity and
missing-reference failures are covered by the focused command:

```powershell
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --audio-music-programs
```
