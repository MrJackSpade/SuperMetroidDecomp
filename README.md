# Super Metroid decompilation notebook

This private workspace is for studying and translating the original Super Metroid program into heavily commented, breakpoint-friendly C#. The checked-out native C decompilation remains a working behavioral reference. At the owner's explicit request, the private repository includes the verified ROM and generated ROM-derived assets so one checkout is runnable without an extraction step. It must remain private and is not intended for redistribution.

## Verified input

`Super Metroid.smc` has been identified as the unheadered 3 MiB Japan/USA NTSC v1.0 ROM:

| Digest | Value |
| --- | --- |
| MD5 | `21f3e98df4780ee1c667b84e57d88675` |
| SHA-1 | `da957f0d63d14cb441d215462904c4fa8519c613` |
| SHA-256 | `12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72` |

It is a FastROM LoROM image. The reset vector is `$841C`; after reset the CPU begins at `$00:841C`, whose ROM mirror is conventionally written `$80:841C`.

## Runnable workspaces

- `standalone-native/` is the compiled native C port with its private ROM and SDL runtime. It has passed a headless game-loop smoke test.
- `standalone-assets/raw/` contains 1,130 named ROM chunks; `standalone-assets/png/` contains 724 generated PNG/manifest files; `standalone-assets/rooms/LandingSite.png` is the composed 2304x1280 room; and `standalone-assets/runtime/` contains composed and transparent-layer frames rendered from the live C# VRAM/OAM/CGRAM model.
- `csharp/SuperMetroid.slnx` contains the C# core, asset extractor, room viewer, translated-frame debug runner, and dependency-free verification executable. See `csharp/README.md` for launch instructions.

The C# room viewer and translated subsystem runner work now: the frame tab combines recognizable ROM-composed Landing Site terrain with live HUD, OAM, Samus animation, collision, camera, and minimap state. The deliberately exposed all-live PPU toggle renders cartridge-backed BG1, bank-$88's circular/HDMA-scrolled BG2, and exact BG3 HUD while the remaining library-background behavior is translated. Samus can stand/run in both directions, reverse through `$25/$26`, perform variable-height neutral and spin jumps, morph, wall jump, take ordinary dry-air knockback, execute the retail damage boost, swing and release from an already-connected grapple anchor, collide with ceilings, and land through the ROM selectors; animation commands, 16.16 physics, conditional body drawing, and grapple beam tiles/OAM are ROM-driven. Bank-$94 solid/square/non-square slope collision, bank-$80/$90 camera tracking, room scroll zones, BG streaming, 32x64 BGSC addressing, and the Landing Site door/library-background selection are translated and verified. The viewer supplies an explicitly host-selected spawn because the landing cinematic does not define a normal-gameplay Samus spawn. Grapple firing/acquisition/terrain sweep/wallgrab, live enemy damage producers, shinespark, special block dispatchers, PLMs, enemies, effects, and cutscene actors remain incremental work. See [`csharp/MOVEMENT_COVERAGE.md`](csharp/MOVEMENT_COVERAGE.md) for the exact dispatcher matrix. The native C build remains the complete playable desktop reference.

## Pinned upstream references

The large reference worktrees are deliberately not embedded as nested Git repositories. The local workspace used these exact revisions:

- Annotated disassembly: `https://github.com/InsaneFirebat/sm_disassembly.git` at `362be646929cf8e483f692b73a6561cfc2dc1d0d`
- Native C reconstruction: `https://github.com/snesrev/sm.git` at `578f90b3cc49557bb70060ad033bb90b8cf8ac50`

## ROM-analysis tools

The analyzer has no third-party dependencies and never modifies the ROM.

```powershell
python tools/rom_analyze.py "Super Metroid.smc" info
python tools/rom_analyze.py "Super Metroid.smc" disasm --address 00:841C --count 40
python -m unittest discover -s tests -v
```

The disassembler tracks the accumulator/index-width flags changed by `REP` and `SEP`. That matters on the 65C816: the same immediate instruction is two or three bytes long depending on processor state.

## Translation strategy

1. Establish ROM identity, address mapping, vectors, and reproducible extraction.
2. Recover control flow and distinguish code from embedded tables.
3. Name RAM, registers, routines, and data structures using observed behavior and existing annotated disassemblies as cross-checks.
4. Translate one subsystem at a time into C# while retaining the SNES memory model at the boundary.
5. Differentially test C# routines against the original 65C816 behavior, then replace hardware-facing layers deliberately.

The sensible unit of progress is a bank or subsystem, not a whole-ROM machine-generated dump. A syntactic translation is easy to produce but hard to understand; the goal here is aggressively documented code with original ROM addresses, memory layouts, arithmetic-width behavior, and verification evidence beside the implementation.
