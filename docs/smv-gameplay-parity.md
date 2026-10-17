# SMV gameplay parity without hardware lag

The project targets functionally exact cartridge gameplay while eliminating SNES
hardware lag. The port's smoothness is a valued improvement. This policy applies to
every recording, present and future.

## Acceptance criterion for every movie

The task for a supplied movie is to play the entire recording through the port
without gameplay desynchronization, excluding hardware lag. That end-to-end replay
is the game-logic check.

Run from one initial state import with the recorded, update-aligned inputs and
read-only native comparisons through the movie's end. Diagnose and fix actual
replay divergences. Once the complete replay passes, the requested movie task is
complete; do not expand it into an exhaustive inventory of internal fields,
rendering/pixel/OAM/palette/audio parity, unrelated behaviors, or whole-game
coverage. Those are separate tasks only when requested. Existing additional
checks may remain, but missing additional coverage is not a completion blocker.
Do not claim that passing a movie proves every possible gameplay path.

## Units of comparison

An SMV video-frame count is not a gameplay-update count. A native update can span
multiple video refreshes, and a coroutine can deliberately wait for another NMI.
Neither repeated inputs nor unchanged positions prove that a frame is disposable.

Compare the complete ordered sequence of gameplay updates and the inputs actually
consumed by those updates. Preserve authored timers, animation progression,
intentional waits, button presses/releases, and state transitions. Do not require
matching hardware-dependent rendering/CPU stalls or elapsed wall-clock time. This
policy does not permit injecting later reference state to repair a diverged replay.

## Converter contract

A reusable SMV converter is the diagnostic tool. Its output is a separate port
replay, not a replacement for the original SMV or a promise that a shortened SMV
will replay identically in a SNES emulator.

The converter must:

1. Verify and retain the original SMV and ROM identities and emulator/capture
   revision. Never rewrite the source movie.
2. Obtain an instrumented native trace of completed updates and actual input
   consumption, including input reads during nested transition/pause coroutines.
   Video-boundary WRAM captures alone are insufficient when the CPU is partway
   through a routine.
3. Map each replay update to its original SMV frame or frame interval. Retain
   meaningful NMI waits. Exclude only evidenced hardware stalls; do not delete
   samples simply because their buttons or screen contents repeat.
4. Preserve held inputs and press/release semantics. If a single controller mask
   per port update cannot represent the native consumption sequence, report that
   mismatch and extend the replay format rather than silently dropping events.
5. Emit a reproducible conversion manifest with input hashes, source coverage,
   update count, timing classification, and the source-to-update map. Refuse
   unsupported or ambiguous trace boundaries instead of guessing.
6. Initialize the port once from the movie's starting state, then run production
   gameplay using only the converted input events. Expected native state remains
   read-only comparison data.
7. Compare gameplay progression through the recording's end. Record the first
   divergence with both source video-frame and port-update identifiers. A partial
   replay is insufficient; exhaustive internal-state coverage is not required.

The native trace and the comparison ranges are the oracle side of this contract.
They may name native addresses and routines, because their job is to read what
the cartridge did; nothing in the simulation learns those addresses from them.

## Conversion rules the manifest models

Each rule below was established from native evidence for a specific movie and
applies to every movie whose trace exhibits the same shape. A manifest version
records which rules a conversion applied; a later version never silently changes
an earlier rule's meaning.

- **Boot prelude.** Native `Boot` shows the logo with NMI enabled, then
  `CommonBootSection` clears bank $7E (including `$05B6` and held input) before
  `MainGameLoop`. Accepted NMIs before the first main-loop dispatch are folded
  (`initialRecord`), and the first retained update must see its press edge from
  an empty latch.
- **Held input in door music waits.** The music-wait upload consumes no input, but
  each of its NMIs latches the controller. The last such read is retained as
  `hardwareWaitLatch` and applied before the next update without a dispatch.
- **Upload tails.** When `SendAPUData` returns late in a frame, the rest of the
  same dispatch's prologue (HDMA objects, layer blending, RNG) can overrun into one
  more accepted NMI before the door function runs. `$0617` is already clear, but no
  main-loop dispatch began, so this `apu-upload-tail-continuation` is the same
  upload stall and is excluded like the music-wait NMIs.
- **Message-box start.** Bank $85's box polls the joypad itself on lag frames, so
  its frames consume input inside one dispatch. The capture records
  `MessageBox_Routine` entry (`$85:8080`) as `messageBoxStartFrame`. When the
  dispatch's gameplay overran its own frame before the box opened, those lag
  frames read no controller and the replay starts the box's polling after them.
- **Message-box return.** The capture also records `MessageBox_Routine`'s common
  return (`$85:80BA`) as `messageBoxEndFrame` on the update in which the box
  closed. The port's box must close on that frame. A dispatch can run on past it:
  a save station's SRAM write overruns into a lag frame, so the frames between the
  return and the next controller read are that dispatch's hardware lag, not box
  frames.
- **Boot clear inside a dispatch.** `CommonBootSection`'s bank-$7E clear is
  modeled wherever it runs: a main-loop dispatch entered with game state and NMI
  counter both zero consumes the cleared latch. When the game-over menu jumps into
  that boot inside another dispatch's controller read, the post-boot dispatch is
  its own update without a checkpoint of its own (`expectedRecord` null on its
  predecessor).
- **Console reset.** Snes9x records the console's reset button as the controller
  word `$FFFF`. The reset vector reboots the cartridge, whose boot accepts NMIs that
  read the controller before `CommonBootSection` clears bank `$7E`, as at power-on.
  Those boot-prelude reads are excluded (`consoleResets` keeps their audit trail),
  and the first boot-cleared dispatch after the reset is marked `consoleReset`. Only
  SRAM survives the reset. The interrupted update's checkpoint is the boot's read,
  which precedes `CommonBootSection`, so like a power-on movie's first record it is
  read but not compared.
- **Snapshot starts.** A movie that starts from an embedded snapshot imports that
  snapshot once (including any pause menu, boss, projectile and global clock state
  it holds); the replay then runs on converted input alone.

## Replay trace storage

The native capture adapter writes a private full capture: `updates.json` from the converter
and `update-boundaries.wram.gz`, whose records each hold the 128 KiB WRAM. Those streams are
far too large to commit, so the replay verifiers read a committed compact trace instead:

| Movie | Committed trace | Command |
| --- | --- | --- |
| Mother Brain glitch | `csharp/test-fixtures/replay-traces/mother-brain-glitch` | `--mother-brain-glitch-movie DIR` |
| 13% Speed Booster | `csharp/test-fixtures/replay-traces/low-13-percent` | `--lsmv-playthrough-movie DIR` |
| 100% | `csharp/test-fixtures/replay-traces/full-100-percent` | `--full-playthrough-movie DIR` |
| Samus drunk | `csharp/test-fixtures/replay-traces/samus-drunk` | `--drunk-playthrough-movie DIR` |
| lowc (contributed) | `csharp/test-fixtures/replay-traces/lowc` | `--lowc-playthrough-movie DIR` |
| Ridley showcase | `csharp/test-fixtures/replay-traces/ridley` | `--ridley-full-movie` |

`tools/verify-all.ps1` passes these paths. Produce or refresh a trace from its full capture
(the destination must be absent or empty):

```text
dotnet csharp/src/SuperMetroid.Verification/bin/Release/net10.0/SuperMetroid.Verification.dll --compact-replay-trace SOURCE_DIR DEST_DIR
```

The compactor selects the layout from the movie identity in `updates.json`, writes the trace,
then reopens it through the verifiers' reader and compares every record's header and layout
bytes with the full capture. A compact trace holds:

- `updates.json.gz`: the converter's `updates.json`, byte for byte, gzip-compressed.
- `initial.wram.gz`: the record at `initialRecord` (source-frame and boundary-PC words, then
  the complete WRAM), so the one initial-state import still sees every byte.
- `expected.bin.gz`: every record in capture order, including hardware-wait records the
  replay skips and the terminal record. Each record is the two little-endian 32-bit header
  words followed by the layout's payload: its WRAM ranges in ascending address order, XORed
  with the previous record's payload (the first record with zeros). The layout is fixed;
  unchanged bytes are zero, so the stream compresses to what changed.
- `compact-manifest.json`: format `super-metroid-compact-replay-trace-v1`, the layout name,
  SHA-256 and payload size, the record count and initial record, the movie SHA-256, the
  source capture's `checkpointsSha256`, and each compact file's SHA-256.

`ReplayTraceMemoryCatalog` (in `SuperMetroid.Verification`) is the single source of truth
for the compared ranges: the `movie-desync` layout serves the playthrough movies, including
their sound acknowledgements and `--trace-from` diagnostic fields; the `ridley-showcase`
layout serves the Ridley movie. Opening a compact trace checks the format, the layout hash
against the catalog, every file hash, and that the update manifest names the same movie,
source capture and initial record. A checkpoint read outside the layout fails with the
address instead of returning a value, so widening a comparison means extending the catalog
and recompacting from the full capture. Diagnostics that need complete WRAM after the
initial record, such as `--snapshot-import-audit`, read the private full capture directly.

## Which movies pass

Whether a given movie currently passes, how many updates it retains, and what its last
divergence was are facts about a point in time. They belong in the ticket for that movie
and in the verifier's output, not here. Investigation notes from past divergences are in
the history of this file and in the tickets that fixed them.
