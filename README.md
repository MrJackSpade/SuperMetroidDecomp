# Super Metroid C# decompilation workspace

This private workspace translates the original Super Metroid program into heavily commented,
breakpoint-friendly C#. The cartridge remains the behavioral authority; the annotated
disassembly and native C reconstruction are cross-checks, not substitutes for observed ROM
behavior.

The repository intentionally contains a private ROM and ROM-derived assets at the owner's
request so one checkout remains runnable. It must remain private and is not suitable for
redistribution in its current form.

## Current status

The C# port is a working but incomplete game, not merely an asset viewer. Its continuously
playable path currently runs from power-on through the title, file select, options, opening
cinematic, Ceres, Zebes landing, Morph Ball, the first Missile, Bomb Torizo, Bomb acquisition,
and the return to awakened Parlor. The checked-in private-ROM regression completes that route
using controller input and native game state; it does not write debug values to win the fight,
open its doors, create drops, or acquire Bombs.

The following large subsystem passes are implemented:

- normal Samus movement, poses, animation, collision, liquids, grapple, Speed Booster,
  shinespark, Space Jump, Screw Attack, Crystal Flash, X-ray mechanics, knockback, and the
  translated death/body-special routes;
- beams, Charge Beam, Hyper Beam production and motion, Missiles, Super Missiles, normal
  Bombs, Power Bombs, trails, explosions, terrain reactions, and projectile animation;
- every retail enemy definition referenced by the named room populations, including its
  initialization/main dispatch, instruction execution, ordinary combat dispatch, touch/shot/
  bomb/Power-Bomb/grapple reactions, enemy projectiles, death, drops, and focused boss logic;
- cartridge room headers and state selection, level/graphics/background loading, scrolling,
  camera tracking, ordinary door transitions, the sequential bank-`$84` room-PLM loader,
  colored/grey doors, items, scrolls, elevator platforms, save/map/energy/missile stations,
  translated breakable blocks, and encounter PLMs currently used by translated bosses;
- HUD, minimap exploration, pause map/equipment screens, SRAM encoding/checksums, file-select
  save metadata, Ceres automatic save, gunship save/reload, and restoration of inventory and
  world-state bits; and
- hardware-first Direct3D11 rendering of the implemented frontend/gameplay paths,
  with an explicit software reference backend and portable owned render packets;
  [renderer qualification and remaining gates](csharp/RENDERER_ACCEPTANCE_AUDIT.md); and
- cartridge-derived audio: the translated SPC sequencer and SNES DSP/BRR mixer, retail
  bank-$80 music/SFX queues and acknowledgements, and buffered Windows PCM playback; and
- the outer frontend paths for options submenus, reserve-tank recovery, fatal damage,
  game-over/continue, the full four-direction door-opening scroll, successful Zebes escape,
  the bank-$8B ending, ROM-row credits, time-selected post-credit reward, item percentage,
  and final message.

Enemy translation is not the current blocker. The exhaustive enemy audits load all named
retail room-state populations and separately exercise lifecycle, direction, touch, weapon,
grapple, projectile, death, and drop behavior. A focused enemy or boss audit is not proof that
its entire retail room and surrounding game-state sequence are integrated, however.

## Known incomplete systems

These are implementation gaps, not merely missing tests:

- Bank `$84` room populations now parse once in ROM order and retain the native descending
  forty-slot allocation/reuse rules. The dispatcher translates all 941 retail records across
  all 70 headers, including downward gates, eye doors, Draygon cannons, elevators,
  save/map/energy/missile stations, the Speed Booster escape controller, the Wrecked Ship
  attic observer, the n00b tube, and the resident Metroid-room clear-state observers.
- Arbitrary room setup code, room-main code, FX records, and X-ray room data are not generally
  dispatched. Ceres and several encounter-specific paths have explicit translated owners.
- Audio playback is connected to every currently translated publisher: title/intro/room and
  boss music; menu and pause feedback; Samus movement, damage, weapon, visor, Crystal Flash,
  and shinespark effects; PLMs; ordinary enemies; and translated bosses. Older one-value enemy
  debugger fields are normalized through one central adapter so they do not require room-specific
  frontend fixes; they retain only the final same-family call if several overwrite the field in
  one frame. New/direct publishers use a lossless per-frame request list. The managed C# core
  translates this game's SPC driver and S-DSP behavior; it is not a general SPC700 CPU emulator.
- Audio calls owned by the translated outer frontend states now use the same cartridge queue as
  gameplay. Remaining absent calls belong to still-untranslated time-up/demo paths, not to a
  separate mixer or host-sample fallback.
- Rendering is CPU-based and slow. It covers the paths already used by the playable slice and
  focused audits, but it is not a complete cycle-accurate SNES PPU and does not yet reproduce
  every room's HDMA, window, mosaic, priority, or special background behavior.
- The top-level game dispatcher still lacks the time-up and attract-mode demo families. Reserve
  recovery, fatal damage/game-over/continue, final escape, ending, credits, and post-credits are
  integrated; reaching the final escape through every intervening room still depends on the
  general room/PLM/setup gaps listed above.
- Existing-save loading deliberately skips the untranslated native load-appearance presentation
  and enters its stable standing endpoint.
- Bosses and late-game systems with focused translations still need their surrounding rooms,
  PLMs, room code, frontend states, and progression connected into continuous gameplay.

Detailed Samus dispatcher routing and focused verification evidence are maintained in
[`csharp/MOVEMENT_COVERAGE.md`](csharp/MOVEMENT_COVERAGE.md). Runnable-project details and the
testing policy are in [`csharp/README.md`](csharp/README.md).

## Verified input

`Super Metroid.smc` is the unheadered 3 MiB Japan/USA NTSC v1.0 ROM:

| Digest | Value |
| --- | --- |
| MD5 | `21f3e98df4780ee1c667b84e57d88675` |
| SHA-1 | `da957f0d63d14cb441d215462904c4fa8519c613` |
| SHA-256 | `12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72` |

It is a FastROM LoROM image. The reset vector is `$841C`; execution begins at `$00:841C`,
whose ROM mirror is conventionally written `$80:841C`.

## Workspaces

- `csharp/` contains the actively developed C# game, core library, desktop controls, asset
  extractor, room viewer, verification runner, and private-ROM debug runner.
- `standalone-assets/raw/` contains named ROM chunks; `standalone-assets/png/` and
  `standalone-assets/runtime/` contain inspectable PNG assets and rendered diagnostic layers.
- `standalone-assets/audio/` contains exact SPC sequence/instrument upload streams, a
  SHA-verified metadata catalog, and 112 deduplicated, stable-ID PCM WAV samples used directly
  by managed audio playback. The 935 bank/source aliases make those WAVs replaceable without
  rewriting game code or sequence data.
- `standalone-native/` contains the compiled native C reference and SDL runtime.
- `upstream-disassembly/` and `upstream-sm/` are pinned source references used during
  translation and differential inspection.

Install the .NET 10 SDK, then open `csharp/SuperMetroid.slnx` in Visual Studio or run the game
from `csharp/`. No C++ workload or native runtime DLL is required:

```powershell
dotnet run --project src/SuperMetroid.Game
```

The host locates the private ROM automatically, accepts one explicit ROM path, or honors the
`SUPERMETROID_ROM` environment variable. Normal play continuously writes replayable controller
sessions under `input-recordings/` beside the ROM; each recording includes the reset-time SRAM
seed and ROM digest. Ten exact-frame debugger slots are available from the playable toolbar;
their attachable files live under `debug-states/` beside the ROM. `csharp/README.md` documents
both the `--replay` command and debugger-state compatibility rules.

## Verification

```powershell
dotnet build csharp/SuperMetroid.slnx
dotnet run --project csharp/src/SuperMetroid.Verification
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- --retail-enemy-coverage-audit "Super Metroid.smc"
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- --retail-enemy-execution-audit "Super Metroid.smc"
```

The Python ROM analyzer remains dependency-free and never modifies the ROM:

```powershell
python tools/rom_analyze.py "Super Metroid.smc" info
python tools/rom_analyze.py "Super Metroid.smc" disasm --address 00:841C --count 40
python -m unittest discover -s tests -v
```

## Pinned references

- Annotated disassembly: `https://github.com/InsaneFirebat/sm_disassembly.git` at
  `362be646929cf8e483f692b73a6561cfc2dc1d0d`
- Native C reconstruction: `https://github.com/snesrev/sm.git` at
  `578f90b3cc49557bb70060ad033bb90b8cf8ac50`

## Translation policy

1. Identify the ROM and preserve 65C816 arithmetic widths and address mapping.
2. Recover control flow and distinguish executable code from embedded data.
3. Name state using the ROM, annotated disassembly, and native reconstruction as evidence.
4. Translate a bounded subsystem while retaining original addresses, tables, and side effects.
5. Verify routines with synthetic boundary checks and private-ROM execution audits.
6. Connect proven subsystems into gameplay without replacing missing behavior with bespoke
   room fixes.

The sensible unit of implementation is a subsystem or native dispatcher family. Controller
tests are reserved for practical room-local slices and a small number of retained integration
routes; they are not the primary coverage mechanism for every room in the game.
