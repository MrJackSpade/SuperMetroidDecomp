# C# port workspace

This directory contains the actively developed, cartridge-backed C# translation. It targets
`.NET 10` on Windows and treats warnings as errors. Open `SuperMetroid.slnx` in Visual Studio
or use the commands below from this directory. The desktop build also requires Visual Studio's
`Desktop development with C++` workload and the x64 `v145` toolset: it compiles the translated
SPC sequencer/DSP bridge automatically and copies the native DLL beside the managed executable.

This file is the authoritative high-level status summary. Detailed Samus movement coverage,
original routine addresses, and focused verification evidence live in
[`MOVEMENT_COVERAGE.md`](MOVEMENT_COVERAGE.md). A translated focused audit proves the named
subsystem; it does not imply that every surrounding room or top-level game state is connected.

## Playable status

`SuperMetroid.Game` is the normal entry point. The continuously playable C# path currently
covers:

1. reset, Nintendo/title sequence, file select, and options;
2. the opening narration and cinematic, including the optional host-configured skip;
3. Ceres approach, elevator arrival, station rooms, Ridley, escape, destruction, and Zebes
   landing;
4. the Crateria/Blue Brinstar route through Morph Ball and the first Missile;
5. Bomb Torizo using normal projectile/enemy collision, native health/phases/death, native
   Chozo-orb drops, condition-gated doors, and the Bombs item PLM; and
6. the return through Flyway to awakened Parlor.

The retained private-ROM controller regression reaches awakened Parlor without writing debug
state to defeat Bomb Torizo, create drops, open doors, acquire items, or perform pause/save
transitions. This is the current continuous-play boundary, not a claim that later rooms are
playable in sequence.

## Run the game

```powershell
dotnet run --project src/SuperMetroid.Game
```

No extracted-assets argument is required. Startup searches the current directory, executable
directory, and their parents for `Super Metroid.smc`, `Super Metroid.sfc`, `sm.smc`, or
`sm.sfc`. It also accepts one explicit ROM path or the `SUPERMETROID_ROM` environment variable:

```powershell
dotnet run --project src/SuperMetroid.Game -- "C:\path\to\Super Metroid.smc"
```

The host uses a 256x224 game framebuffer and scales it by an integer factor when the window
allows. Click the game image if keyboard focus has moved to the toolbar. Gamepad polling does
not depend on window focus.

### Controls

| Keyboard | SNES input | Game use |
| --- | --- | --- |
| Arrow keys | D-pad | Move and aim |
| `Space` or `X` | A | Jump/confirm |
| `Z` | B | Dash/cancel |
| `S` | X | Fire |
| `A` | Y | Item cancel |
| `Q` | L | Aim up |
| `W` | R | Aim down |
| `Enter` | Start | Pause/start |
| `Shift` | Select | Select HUD item |

Generic USB/DirectInput gamepads are detected automatically and can be connected or removed
while the game is running. The D-pad and left stick both provide directions. Face buttons map
by physical position: south is B/dash, east is A/jump, west is Y/item cancel, and north is
X/fire. The left/right shoulders provide L/R aim; Back/Select and Start provide the matching
SNES menu buttons. Keyboard and gamepad input remain active together and are merged into the
single SNES controller word captured by automatic recordings.

The toolbar's Pause/Play, Step, Press Start, and Restart controls wrap the same game dispatcher;
they do not install alternate gameplay state.

### Configuration and saves

The playable host reads `SuperMetroid.ini` beside the selected ROM and creates a documented
file if none exists. Its supported options are:

```ini
[Game]
SkipOpeningCinematic=false
Invincibility=false
InfiniteAmmo=false

[Audio]
Enabled=true
MasterVolumePercent=100
```

`SkipOpeningCinematic=true` preserves the title, file select, and options screens, then enters
the same new-game Ceres loader used after the cinematic. `Invincibility=true` allows normal
damage and hit reactions but prevents Samus from dropping below one energy. The
`InfiniteAmmo=true` option lets missiles, super missiles, and power bombs consume normally,
but raises an unlocked type from zero to one at the end of the frame. It does not grant
ammo upgrades whose maximum is still zero. The generated defaults are `false`; this private
workspace's checked-in `../SuperMetroid.ini` is intentionally set to `true` for development.
Audio is enabled by default; `MasterVolumePercent` is the final host gain from zero through
100 after SNES mixing. Unknown sections, unknown or duplicate keys, invalid Booleans, and
out-of-range volume values fail with a file and line number.

Battery-backed data is stored as an 8 KiB `.srm` beside the ROM. The implementation includes
the redundant cartridge checksums, file-select ENERGY/TIME metadata, inventory and equipment,
events, boss bits, Chozo/item bits, explored map data, Ceres automatic checkpoint, natural
gunship save/reload, and room save-station confirmation/persistence. Existing saves resolve the native area/load-station record. Their native
load-appearance presentation now runs its ROM palette effect and complete 360-frame locked
front-facing sequence before restoring ordinary movement.

### Automatic input recordings and replay

Every normal game reset creates an always-on recording under `input-recordings` beside the
private ROM. The `.smrec` file contains the reset-time 8 KiB SRAM image, the ROM's SHA-256
digest, the startup host option, and one raw controller word for every submitted frame. It
does not contain ROM bytes. Periodic atomic flushes run off the UI thread; closing the game,
restarting, or exiting after a managed failure waits for a final flush. Because Restart reads
the current `.srm` before opening a new recording, each session begins from the last durable
save instead of carrying an earlier in-memory run forward.

Replay a captured failure with the automatically discovered ROM:

```powershell
dotnet run --project src/SuperMetroid.Game -- --replay "C:\path\to\input-recordings\SuperMetroid-input-20260901-120000-000.smrec"
```

An explicit ROM path may follow the recording path. Replay rejects a SHA-256 mismatch, uses
the recording's SRAM/INI seed, ignores live keyboard/gamepad input, and never writes replay mutations
back to the user's `.srm`.

### Debugger save states

The playable window has a **State slot** selector and **Save State** / **Load State** buttons
for ten persistent debugger slots, numbered 0-9. Slot files are written to `debug-states`
beside the private ROM as `SuperMetroid-debug-slot-N.smstate`; that is the file to attach to
an issue when a failure depends on exact timing or late-game state. Loading replaces the
complete managed frontend/runtime/address-space graph at the captured frame, refreshes the
visible frame immediately, resets host audio buffers, and begins a new controller recording
from the restored boundary.

The state header records the slot, UTC timestamp, frame, game state, active room/state
pointers, exact core/desktop build IDs, and ROM SHA-256. Missing, malformed, different-ROM,
different-schema, and different-build states throw instead of falling back. These are private
debug artifacts: the compressed payload contains the full emulated address space, including
ROM and SRAM bytes, and is therefore not suitable for distribution.

The headless deterministic round-trip audit is:

```powershell
dotnet run --project src/SuperMetroid.Game -- --state-audit "..\Super Metroid.smc"
```

## Implementation coverage

### Samus and weapons

The translated normal-movement dispatcher covers all active retail movement types and poses
identified by the bank-$91 audit: standing/running/aiming, jumping/falling/turning, posture and
Morph/Spring Ball transitions, wall jump, grapple, moonwalk, knockback/damage boost, liquids,
Speed Booster, shinespark, Space Jump, Screw Attack, Crystal Flash, X-ray mechanics, Draygon
grab, drained/Mother-Brain routes, and the translated death presentation.

The projectile systems cover every valid ordinary/charged beam combination, Charge Beam flare,
Hyper Beam production/motion, Missiles, Super Missiles, normal Bombs, Power Bombs, trails,
explosions, terrain collision, breakable-block reactions, and the implemented HDMA/color-math
effects. See `MOVEMENT_COVERAGE.md` for exact admitted poses, native addresses, and focused
verification evidence. The current high-level gap list is maintained in this README.

### Enemies

The enemy pass is complete as a translated subsystem. `RoomEnemySystem` parses the terminated
bank-$A1 populations, bank-$B4 graphics sets, and complete bank-$A0 definitions, preserving
native slot order, spawn snapshots, graphics staging, boss bookkeeping, and the normal
scheduler.

Every retail definition referenced by a named room-state population has a translated dispatch
path. The audit suite covers initialization and main AI, instruction lists, long lifecycle and
directional behavior, touch/shot/normal-bomb/Power-Bomb/grapple reactions, Samus and enemy
projectiles, freezing where applicable, boss phases, death, drops, and pickup collection.
This supersedes older notes that described enemy AI or damage producers as untranslated.

Enemy completion does not make the whole game complete: a boss audit can directly construct
its native room state while unrelated room setup code, PLMs, frontend transitions, audio, or
PPU effects remain absent from continuous gameplay.

### Rooms, doors, and PLMs

Implemented room infrastructure includes:

- all six retail room-state selector functions and live event/boss/inventory inputs;
- cartridge room/door headers, compressed level data, tileset/palette/background loading,
  scroll grids, initial BG streaming, camera/minimap tracking, and four-way door placement;
- blue door traversal plus persistent yellow, green, red, and condition-gated grey door PLMs;
- permanent exposed/Chozo/shot-block collectible PLMs and suit/item message presentation;
- scroll-trigger PLMs;
- the six room elevator-platform PLMs and save, map-download, energy-recharge, and
  missile-recharge station families, including collision blocks, animations, messages,
  resource/world-state mutation, and selected-slot SRAM persistence;
- collision-bomb, projectile-bombable, shootable, special-bomb, and breakable-grapple block
  PLMs, including permanent and respawning variants; and
- translated encounter mutations for Bomb Torizo, Spore Spawn, Botwoon, Crocomire, Shitroid,
  and Mother Brain.

Room loading now parses every six-byte population record exactly once in ROM order. It allocates
the native forty-slot pool from highest ID downward before dispatching reusable setup handlers;
synchronous-delete setups free their physical slot for the next record just as `$84:846A` does.
Unsupported headers throw with population pointer, record index, header/setup/list pointers,
coordinates, and room argument. The private-ROM exhaustive audit currently measures 882 of 941
retail records (54 of 70 headers) as translated. The remaining 59 records are gates, eye doors,
escape/progression actors, Draygon cannons, Noob Tube, and related special families—not silent
skips or station/elevator gaps.

Room headers expose FX, X-ray, room-main, PLM, background, and setup pointers, but only the
translated consumers are executed. Room loading now selects the door-matched sixteen-byte FX
record and runs its bank-$8D palette-object bitset through a shared ROM interpreter; unsupported
setup/pre-instruction/audio side effects fail with their native pointers. FX types, liquid/tide
state, palette blending, bank-$87 animated tiles, and X-ray room-data-driven BG2 substitution are
not general yet. Arbitrary room setup code and room-main code are also not generally dispatched;
the Ceres elevator shaft and encounter-specific owners remain explicit specialized translations.

### Frontend and persistent state

The connected top-level states cover reset/title, file select, options, opening cinematic,
new-game setup, Ceres handoff/destruction, load-station entry, gameplay fade-in, main gameplay,
all four native door-opening scrolls, the pause/equipment/map transition cycle, reserve-tank
automatic recovery, fatal damage, game-over/continue, successful Zebes escape, the ending,
credits, time-selected reward, item percentage, and final message.

The remaining top-level dispatcher gaps are time-up and the attract-mode demo family. Continuous
play from the current early-game route to the final escape still depends on general room setup,
room-main, remaining special-PLM, and progression work; that is distinct from the now-integrated ending
state family itself.

### Rendering and audio

Rendering is a software composition of modeled VRAM, CGRAM, OAM, BG tilemaps, Mode 7, windows,
and the translated color-math/HDMA effects. It is sufficient for the implemented frontend,
playable route, and diagnostic captures, but it is CPU-heavy and is not a complete
cycle-accurate SNES PPU. Arbitrary room FX/HDMA, mosaic, window, priority, and special-background
combinations remain to be implemented as they are encountered. A GPU-backed compositor is a
separate performance task.

Audio uses verbatim, pinned copies of the cartridge-derived sequencer and SNES DSP/BRR mixer
under `native/SuperMetroid.AudioNative/vendor/sm`, reached through the narrow x64 bridge in
that project. C# owns the retail bank-$80 music ring, three bank-$82 SFX
handshakes, acknowledgement timing, and ROM upload-stream resolution; Windows `waveOut` owns
only buffered delivery of the resulting 48 kHz stereo PCM. That separation keeps queue state
and each translated producer inspectable in the managed debugger while avoiding host-authored
replacement samples.

Every currently translated audio publisher is connected: title, intro, Ceres, room and boss
music; file-select/options/pause feedback; Samus movement, damage, liquid, X-ray, Crystal Flash,
shinespark, death, and weapon effects; PLMs; pickups/deaths; elevators; ordinary enemies; and all
translated bosses. New enemy code publishes lossless per-frame requests directly. Older verified
enemy translations are normalized by one central adapter that preserves the retained call's native
library and `QueueSfxN_MaxM` admission limit without introducing room-specific playback code. Those
legacy nullable debugger fields can retain only the final same-family call when several overwrite
one another in a frame; migrating their individual call sites remains a fidelity cleanup, not a
prerequisite for hearing those systems.

Reserve-tank auto-refill, game-over/continue, ending/credits, both options submenus, and the full
door-transition fade/scroll/wait owner now publish through the shared retail queue. Time-up and
demo-only calls remain absent with those untranslated states. The native implementation translates
Super Metroid's SPC program; it is intentionally not a general-purpose SPC700 instruction emulator.

For a non-device smoke test that loads the private ROM, runs the real initial upload/music
sequence, and proves the mixer produced nonzero PCM:

```powershell
dotnet run --project src/SuperMetroid.Game -- --audio-audit "..\Super Metroid.smc"
```

To exercise the Windows `waveOut` queue beyond its six-buffer capacity without a ROM or an
audible test tone, run:

```powershell
dotnet run --project src/SuperMetroid.Game -- --waveout-audit
```

To replay an always-on controller journal through the native SPC without opening the window
or an audio endpoint (useful when an SFX handshake appears to block a door transition):

```powershell
dotnet run --project src/SuperMetroid.Game -- --audio-input-replay-audit `
  "..\input-recordings\SuperMetroid-input-YYYYMMDD-HHMMSS-fff.smrec" `
  "..\Super Metroid.smc"
```

## Other runnable projects

### Verification

`SuperMetroid.Verification` is the fast, dependency-free synthetic regression executable:

```powershell
dotnet run --project src/SuperMetroid.Verification
```

It verifies typed address/memory behavior, decompression and rendering primitives, movement,
collision, projectiles, PLMs, enemies, frontend state, pause/save behavior, and other translated
dispatcher boundaries. It is not a complete-game playthrough.

### Debug runner

`SuperMetroid.DebugRunner` hosts private-ROM subsystem and room-state audits. Representative
commands are shown below. Run the named retail-population audits from the repository root;
they deliberately read the pinned `upstream-sm/assets/names.txt` population boundaries from
that working directory.

```powershell
cd ..
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- --retail-enemy-coverage-audit "Super Metroid.smc"
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- --retail-enemy-execution-audit "Super Metroid.smc"
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- --retail-enemy-lifecycle-audit "Super Metroid.smc"
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- --retail-enemy-touch-audit "Super Metroid.smc"
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- --retail-enemy-attack-audit "Super Metroid.smc"
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- --retail-plm-population-audit "Super Metroid.smc"
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- --retail-scroll-ownership-audit "Super Metroid.smc"
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- --early-controller-route-audit "Super Metroid.smc"
```

The scroll-ownership audit loads every named room state through the production PLM loader,
checks every type-$3/BTS-$46 collision trigger against its resident `$B703` owner, exercises
the first native touch/wake/sleep cycle, and reports untranslated PLMs that prevent a state
from reaching that check.

Focused actor and boss flags are defined near the top of
`src/SuperMetroid.DebugRunner/Program.cs`. The ordinary frame-script parser also supports
movement and weapon scripts such as `--jump-script`, `--grapple-script`,
`--power-bomb-script`, and `--mother-brain-rainbow-script`.

### Room viewer

`SuperMetroid.RoomViewer` is a separate diagnostic UI, not the game entry point:

```powershell
dotnet run --project src/SuperMetroid.RoomViewer
```

It uses the private ROM plus `../standalone-assets/raw` for static-room and frame-stepping
inspection. Its optional diagnostic composition modes should not be confused with the normal
`SuperMetroid.Game` runtime.

### Asset extractor

```powershell
dotnet run --project src/SuperMetroid.AssetExtractor -- ../standalone-assets/raw ../standalone-assets/png
dotnet run --project src/SuperMetroid.AssetExtractor -- room ../standalone-assets/raw ../standalone-assets/rooms/LandingSite.png
```

The extractor produces named raw chunks, PNGs, manifests, and composed-room diagnostics. The
game itself reads cartridge code/data directly; PNGs are inspectable artifacts rather than a
replacement source of gameplay truth.

## Testing policy

- Synthetic tests cover arithmetic, dispatch, state-machine, and malformed-data boundaries.
- Private-ROM audits cover real tables, populations, instruction streams, and focused room or
  subsystem behavior.
- Controller tests should normally begin from a deterministic cartridge-derived state and stay
  within one room. Door handoff and destination loading are tested as data/state integration
  seams rather than by scripting long journeys.
- The existing Ceres and early-game multi-room controller routes are retained as historical
  integration regressions, but they should not be extended unless explicitly requested.
- Missing behavior must remain named and visible. Tests and runtime code must not invent
  bespoke room fixes or write debug state merely to make a route pass.

## Build

```powershell
dotnet build SuperMetroid.slnx
```

Generated screenshots and route logs belong under `test-temp/` and are intentionally not part
of stable source checkpoints.
