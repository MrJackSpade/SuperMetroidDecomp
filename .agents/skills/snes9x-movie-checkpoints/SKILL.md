---
name: snes9x-movie-checkpoints
description: Inspect Snes9x 1.43 SMV movies and export deterministic native WRAM checkpoints for cartridge-vs-port diagnostics. Use for supplied SMV recordings, recorded glitch techniques, or native input/state comparison; do not reach for Windows UI automation merely to play a movie.
---

# Snes9x movie checkpoints

Use the bundled scripts to turn an SMV v1 movie with an embedded snapshot into
controller inputs and native pre-frame WRAM checkpoints. Prefer this workflow to
GUI automation when diagnosing a recorded Super Metroid technique.

## Workflow

1. Run `scripts/Inspect-Snes9xMovie.ps1 -MoviePath <movie>` to validate the movie
   and export its controller samples when needed. Note `RecordedRomName`.
   Snes9x 1.43 compares this embedded `NAM` value during snapshot restore and can
   reject a byte-identical ROM whose basename differs. If the hash/revision is
   correct, make an ignored temporary copy using the recorded basename; do not go
   hunting for another revision based on that error alone.
2. Choose sparse checkpoint frames first. Run
   `scripts/Export-Snes9xMovieCheckpoints.ps1` with the original ROM, the supplied
   1.43 capture build, and those frame numbers. Read room/state/position words from
   the resulting `frame-N.wram` files to narrow the interesting interval.
3. Export every frame only across the narrowed interval. A checkpoint for frame N
   is native state immediately before controller sample N.
4. Build the smallest deterministic port fixture from one native checkpoint and
   the movie's recorded inputs. Compare exact fields relevant to the report.

The known capture build accepts:

```text
snes9x.exe --rip ROM SHORTENED_MOVIE CAPTURE_DIRECTORY FRAME
```

The export script writes a temporary movie whose header frame count is N+1 for
each checkpoint. Each playback starts from the original embedded native snapshot,
so checkpoints are independent rather than accumulated rollback state.

## Constraints

- Treat the ROM, SMV, gzip snapshots, and extracted WRAM as private diagnostic
  fixtures. Keep them in ignored/temp storage and never commit or publish them.
- Do not infer native expectations from port output. Checked-in evidence may contain
  controller inputs, hashes, addresses, and numeric expectations derived from the
  native checkpoints.
- The scripts intentionally accept only SMV v1 with one recorded controller. Extend
  them explicitly if a future artifact requires another format.
- Preserve the supplied movie unchanged. The exporter creates shortened copies in
  its own output directory.
- Preserve the canonical ROM unchanged. A snapshot-name compatibility copy belongs
  only in ignored/temp storage and must retain the canonical ROM's hash.
- For this project, use the pinned Japan/USA ROM and cross-check addresses against
  the pinned disassembly before naming fields.
