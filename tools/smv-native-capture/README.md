# Native SMV update capture

This Windows console adapter instruments Snes9x 1.60 to produce the input-consumption
trace required by `../convert-smv-updates.py`. It does not alter the movie or ROM.
Keep the build directory, ROM, movies, gzip WRAM traces and generated replay private.

Use an ignored working directory, with copies of `capture.cpp` and `CMakeLists.txt`.
Clone Snes9x into `snes9x-160` at commit
`913b75d07c6e8d54e966e2c4a79d7c55428007df`, and zlib into `zlib` at tag `v1.3.1`.
Apply `instrument.py` to that Snes9x checkout, then configure/build with CMake and
Visual Studio C++ in Release mode. The adapter uses libretro's ordinary execution
loop and the original `S9xMovieOpen`; it does not shorten the movie.

Run:

```text
ridley_capture.exe ROM MOVIE OUTPUT_DIRECTORY 0,10890
python tools/convert-smv-updates.py MOVIE OUTPUT_DIRECTORY --rom ROM --output OUTPUT_DIRECTORY/updates.json
```

Replace `10890` with the supplied movie length. The terminal checkpoint is always
captured even if it is omitted from the optional comma-separated snapshot list.
The source ROM must be the pinned J/U revision; the converter checks its hash.

Observed instruction boundaries, cross-checked against the pinned disassembly:

- `$80:9459`: accepted-NMI controller-read entry. Capture the completed preceding
  update before replacing its held/new input words.
- `$80:9496`: controller-read return. Record the actual held/new input consumed.
- `$82:8948`: main-loop entry before HDMA and RNG.
- `$82:897A`: completed outer update before its NMI wait.
- `$80:8338`: explicit NMI wait entry, useful for coroutine classification.
- `$85:8080`: `MessageBox_Routine` entry, the first frame of the box's own polling.
- `$85:80BA`: `MessageBox_Routine`'s common return. The dispatch may run on past it
  (a save station writes SRAM), so frames after it are the dispatch's, not the box's.

`input-events.csv` preserves their order and source movie frame. Each compressed
checkpoint record contains little-endian source-frame and boundary-PC words
(two 32-bit integers), followed by 128 KiB WRAM. The final record has PC zero and
contains the original movie's terminal state. The converter verifies these records
against the event stream, preserves all input edges and refuses ambiguous traces.

The movie counter becomes unavailable when Snes9x finishes playback. The adapter
records the terminal iteration with the original declared movie length, while
allowing that already-running emulator iteration to finish. No extra `retro_run`
is executed afterward.

The port verifier currently compares selected gameplay fields and stops at the
first mismatch. A successfully converted movie is not proof that the port matches
it. Capture artifacts remain private, and the converter's manifest records hashes
rather than embedding WRAM or ROM data.

Manifest v3 records the APU-upload flag (`$0617`), door-scroll counter (`$0925`),
completed-scroll flag (`$0931`), game state and door dispatcher at each boundary.
It folds only proven, neutral-input, post-scroll APU waits into their enclosing
outer dispatch. Every removed input remains in `excludedUploadInputs`; retained
updates map to the original private checkpoint records. Non-neutral inputs,
concurrent moving owners or an unfinished terminal upload refuse normalization.
The original input/WRAM stream is still validated in full before conversion.

The verifier normalizes reference NMI bookkeeping by the recorded excluded count;
it does not change port state. Any gameplay effect of that timing difference still
needs to match the compared properties. See `docs/smv-gameplay-parity.md` for the
current scope and first unresolved mismatch. Run `python tests/smv-timing.Tests.py`
for classification, completed-boundary mapping, and unsafe-collapse rejection.

Manifest v4 also supports power-on movies: it folds the native boot prelude
before the first main-loop dispatch (`initialRecord`) and retains the last
controller read of each door music wait as `hardwareWaitLatch`, so held buttons
no longer require neutral input. The capture adapter itself is unchanged; Snes9x's
own `S9xMovieOpen` performs the reset and SRAM restore.

Manifest v7 models `CommonBootSection` ($80:8482), which clears bank $7E before
`MainGameLoop` at power-on and again when quitting from game over or soft resetting.
A main-loop dispatch entered with game state and NMI counter both zero consumes the
cleared controller latch, so its update's input is zero and the next read's edge starts
from zero. When the clear follows another dispatch under the same controller read (the
game-over menu jumps into the boot without waiting for NMI), the post-boot dispatch
becomes its own update with no checkpoint of its own: its predecessor's
`expectedRecord` is null. A power-on movie's first dispatch must show the clear.
Snes9x opens a movie's start state with `gzdopen`, so an uncompressed reset-start SRAM
or snapshot is read as-is.
