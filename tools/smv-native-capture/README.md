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
