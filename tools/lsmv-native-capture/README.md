# Native lsnes movie capture

This Windows console adapter replays an lsnes `.lsmv` movie on the same bsnes v085
core lsnes records with, and writes the input-consumption trace that
`../convert-smv-updates.py` reads. It produces the same `input-events.csv` and
`update-boundaries.wram.gz` formats as `../smv-native-capture`. It does not alter
the movie or ROM. Keep the build directory, ROM, gzip WRAM traces and generated
replay private.

## Build

Use an ignored working directory.

1. Clone lsnes (`https://repo.or.cz/lsnes.git`) at commit `980f198a` and update its
   `bsnes` submodule. That submodule is bsnes v085 with lsnes's patches applied.
2. Build the core with the WinLibs GCC 13.3 MSVCRT toolchain (the UCRT build fails
   on nall's `_WIN32_WINNT` 0x0501):
   `make -C bsnes profile=compatibility options=debugger ui=ui-libsnes`
3. Unpack zlib 1.3.1 and compile its C sources to objects.
4. Compile and link the adapter:

```text
g++ -std=gnu++0x -O3 -fomit-frame-pointer -DDEBUGGER -DPROFILE_COMPATIBILITY ^
    -I lsnes/bsnes -I zlib -c lsmv_capture.cpp -o lsmv_capture.o
g++ -o lsmv_capture.exe lsmv_capture.o lsnes/bsnes/obj/*.o zlib-objects/*.o ^
    -mconsole -static -static-libgcc -static-libstdc++ ^
    -luuid -lkernel32 -luser32 -lgdi32 -lcomctl32 -lcomdlg32 -lshell32 -lole32
```

## Run

```text
python tools/lsmv-native-capture/lsmv-extract.py MOVIE --rom ROM --output WORK/frames.bin
lsmv_capture.exe ROM WORK/frames.bin OUTPUT_DIRECTORY
python tools/convert-smv-updates.py MOVIE OUTPUT_DIRECTORY --rom ROM --output OUTPUT_DIRECTORY/updates.json
```

`lsmv_capture.exe ROM FRAMES OUTPUT --trace START END` instead writes
`instructions.bin.gz`, the (line, PC) of every instruction in that line range.

`lsmv_movie.py` accepts only what the adapter reproduces exactly: an `lsnes-rr1`
NTSC movie on bsnes v085 from power-on, default core settings, one gamepad, no
resets or subframes, no savestate and no movie SRAM. It refuses anything else.

## Matching lsnes

- Settings: lsnes defaults (no random initial state, stock poll timings, no bus
  fixes, stock clock table), the cartridge loaded with generated markup, port one a
  gamepad and port two empty, one `system.run()` per movie line.
- Every poll during a line returns that line's buttons. The gamepad field order
  B Y Select Start Up Down Left Right A X L R is $4218 bit 15 downwards.
- With no movie SRAM, the cartridge keeps the RAM bsnes allocated, all $FF.
- bsnes returns from `run()` at vblank right after the automatic joypad latch, so
  the NMI that reads $4218 runs in the next line. Events are labelled with the line
  the controller latched (lsnes's `notifyLatched`), which is the line the game reads.
- The last line is latched in the movie's final frame. The adapter runs one more
  frame so the game reads it, as Snes9x reads an SMV's trailing word; the line count
  is therefore the source frame count plus one.
