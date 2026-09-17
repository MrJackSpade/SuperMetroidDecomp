---
name: snes9x-movie-checkpoints
description: Inspect Snes9x 1.43 SMV movies and export deterministic native WRAM checkpoints for cartridge-vs-port diagnostics. Use for supplied SMV recordings, recorded glitch techniques, or native input/state comparison; do not reach for Windows UI automation merely to play a movie.
---

# Snes9x movie checkpoints

Use the bundled scripts to inspect SMV v1, v4, or v5 movies with embedded
snapshots, export their controller inputs, and produce deterministic native WRAM
checkpoints. Prefer this workflow to GUI automation when diagnosing a recorded
Super Metroid technique.

## Workflow

1. Run `scripts/Inspect-Snes9xMovie.ps1 -MoviePath <movie>` to validate the movie
   and export its controller samples when needed. Note `RecordedRomName`.
   Snes9x 1.43 compares this embedded `NAM` value during snapshot restore and can
   reject a byte-identical ROM whose basename differs. If the hash/revision is
   correct, make an ignored temporary copy using the recorded basename; do not go
   hunting for another revision based on that error alone.
2. Choose sparse intermediate checkpoint frames first. Run
   `scripts/Export-Snes9xMovieCheckpoints.ps1` with the original ROM, the supplied
   matching capture build, and those frame numbers. Read room/state/position words
   from the resulting `frame-N.wram` files to narrow the interesting interval.
   Use the 1.43 capture build for v1 and the 1.51 capture build for v4/v5; a movie
   must be replayed by the emulator generation that recorded its snapshot format.
3. Export every frame only across the narrowed interval. An intermediate checkpoint
   for frame N is native state immediately before controller sample N. These
   checkpoints intentionally use independent shortened copies and can locate the
   first divergence, but they cannot establish whether the complete technique works.
4. For the success/failure verdict, always request the movie's declared frame count.
   That terminal checkpoint must play the original, unmodified SMV through its end
   and capture native state after the final recorded frame. Never substitute an
   earlier shortened checkpoint for this terminal result.
5. Build the smallest deterministic port fixture from one native checkpoint and
   the movie's recorded inputs. Compare exact fields relevant to the report.

The known capture build accepts:

```text
snes9x.exe --rip ROM SHORTENED_MOVIE CAPTURE_DIRECTORY FRAME
```

For an intermediate checkpoint, the export script writes a temporary movie whose
header frame count is N+1. Each playback starts from the original embedded native
snapshot, so checkpoints are independent rather than accumulated rollback state.
For the terminal checkpoint (N equals the declared frame count), the script passes
the original SMV to the capture build without rewriting or copying it; the capture
build must save after full playback rather than at a pre-frame boundary.

## Constraints

- Treat the ROM, SMV, gzip snapshots, and extracted WRAM as private diagnostic
  fixtures. Keep them in ignored/temp storage and never commit or publish them.
- Do not infer native expectations from port output. Checked-in evidence may contain
  controller inputs, hashes, addresses, and numeric expectations derived from the
  native checkpoints.
- Inspection and checkpoint export accept SMV v1, v4, and v5 with one recorded
  controller. Select a compatible capture executable: Snes9x 1.43 for v1, or the
  Snes9x 1.51 capture build for v4/v5.
- Preserve the supplied movie unchanged. The exporter creates shortened copies in
  its own output directory only for intermediate divergence checkpoints.
- Preserve the canonical ROM unchanged. A snapshot-name compatibility copy belongs
  only in ignored/temp storage and must retain the canonical ROM's hash.
- For this project, use the pinned Japan/USA ROM and cross-check addresses against
  the pinned disassembly before naming fields.
