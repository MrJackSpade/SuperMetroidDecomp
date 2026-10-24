# C# port workspace

This directory contains the actively developed C# translation. Cartridge access belongs to
asset import, not gameplay: Core contains no ROM allocation, cartridge reader, cartridge
decompressor, or importer reference. A complete extracted installation opens mutable WRAM/SRAM
and installed presentation resources without opening the retained private ROM. This
architectural boundary is not a claim of complete gameplay parity. It targets
`.NET 10` on Windows and treats warnings as errors. Open `SuperMetroid.slnx` in Visual Studio
or use the commands below from this directory. The playable build is fully managed and requires
the .NET 10 SDK; it no longer builds or deploys a native audio DLL or requires the C++ workload.

`SuperMetroid.slnx` includes all 15 non-Android projects, grouped as apps, libraries, tools,
and verification. `SuperMetroid.Full.slnx` includes all 16 projects and requires the Android
workload. See the [shared test support](test-support/README.md) for fixture imports.

Portable import validation is explicitly ROM-free:

```powershell
dotnet run --project src/SuperMetroid.IntegrationVerification -c Release -- --asset-import
```

Add a private ROM path and optional reference-audio directory after `--asset-import` for full
installation tests. Other integration commands retain their flags; the default integration
suite requires the private ROM and extracted audio in the repository working directory.

Static resource dependency auditing is separate from gameplay verification:

```powershell
dotnet run --project src/SuperMetroid.ResourceAudit -c Release -- --root .. --json ../out/resource-audit.json
```

It compares compiled resource references with production provider definitions, inventories
resource lookups from source, and fails on missing definitions **or unresolved coverage**.
No ROM, installed art, saves, frames or controller inputs are opened. The audit is not
currently clean; findings are development work, not proof that every listed identity is a
reachable player bug. See [scope, build integration and report format](src/SuperMetroid.ResourceAudit/README.md).

`SuperMetroid.ReachabilityAudit` reports every type, member, field and enum member that no
player host or tool reaches, regardless of accessibility. Verification projects are not users:
code only a test reaches is reported as unused. It also lists members touched only by JSON
serialization and production code reached only by tools. Members that only reflection reaches
carry `[AccessedByReflection]`. From the repository root:

```powershell
dotnet run --project csharp/src/SuperMetroid.ReachabilityAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --output out/reachability
```

This file is the authoritative high-level status summary. A translated focused audit proves the named
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

First launch offers a ROM picker and extracts the required assets automatically. Later launches
validate and use the extracted resources in `%LOCALAPPDATA%/SuperMetroid/game/` without
opening its private ROM copy. An incomplete installation enters the import/repair path.
The supported image is Japan/USA
NTSC v1.0, with or without a copier header. See [ROM setup](ROM-SETUP.md) for storage and CLI usage.
Startup also accepts one explicit ROM path or the `SUPERMETROID_ROM` environment variable:

```powershell
dotnet run --project src/SuperMetroid.Game -- "C:\path\to\Super Metroid.smc"
```

The host uses a 256x224 game framebuffer and scales it by an integer factor when the window
allows. Click the game image if keyboard focus has moved to the toolbar. Gamepad polling does
not depend on window focus.

### Renderer and diagnostics

The Windows host draws with Direct3D 11, its only renderer. A startup failure is reported as an error; there is no
fallback renderer. An older INI's `[Video] Renderer=Auto` or `Renderer=Direct3D11` line still loads;
`Renderer=Software` names the removed software renderer and is rejected. WARP is a verification-tool device, not the
game's fallback. The console reports the adapter.

Simulation/audio and GPU presentation rates are separate: low displayed FPS over
RDP does not by itself mean slow gameplay. The GPU worker uses a bounded latest-frame
mailbox; superseded pictures may be discarded, not simulation inputs or audio.
The native image stays 256x224 with TV-aspect scaling and letterboxing. Per-monitor
DPI awareness is enabled; an actual cross-monitor transition remains unverified.

See [build and packaging](D3D11_BUILD.md). Issue #321 is not yet fully qualified. RDP reconnect testing is explicitly deferred by the user.

### Development smoke audits

Development smoke audits now run through `SuperMetroid.DesktopVerification`, rather than
the player executable. Their argument names and ROM/recording parameters are unchanged:

```powershell
dotnet run --project src/SuperMetroid.DesktopVerification -- --keyboard-input-audit
dotnet run --project src/SuperMetroid.DesktopVerification -- --viewport-layout-audit
dotnet run --project src/SuperMetroid.DesktopVerification -- --frame-timing-audit
```

Other migrated switches are `--unhandled-exception-console-audit`, `--github-error-reporter-audit`,
`--input-batch-audit`, `--state-audit`, `--audio-input-replay-audit`,
`--full-audio-input-replay-audit`, `--recorded-pause-audio-audit`, `--waveout-audit`,
`--audio-audit`, `--managed-audio-audit`, and `--pause-audio-audit`.
`SuperMetroid.Game --replay` remains the player replay command. Its small
`--dpi-awareness-audit` probe also remains, to verify the actual Game startup policy.
It checks the STA apartment required by native Windows file dialogs as well as DPI awareness.
`DesktopVerification --rom-picker-audit <published-Game.dll>` exercises the real Choose ROM
button on the game's declared entry apartment, briefly opens and cancels its native dialog,
and does not install content or touch player saves.

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

The playable host reads `%LOCALAPPDATA%/SuperMetroid/SuperMetroid.ini` and creates a documented
file if none exists. Its supported options are:

```ini
[Game]
SkipOpeningCinematic=false
Invincibility=false
InfiniteAmmo=false

[Audio]
Enabled=true
MasterVolumePercent=100

[Diagnostics]
ReportErrorsToGitHub=false
GitHubErrorRepository=MrJackSpade/SuperMetroidDecomp
```

`SkipOpeningCinematic=true` preserves the title, file select, and options screens, then enters
the same new-game Ceres loader used after the cinematic. `Invincibility=true` allows normal
damage and hit reactions but prevents Samus from dropping below one energy. The
`InfiniteAmmo=true` option lets missiles, super missiles, and power bombs consume normally,
but raises an unlocked type from zero to one at the end of the frame. It does not grant
ammo upgrades whose maximum is still zero. The generated defaults are `false`; this private
workspace's checked-in `../SuperMetroid.ini` is intentionally set to `true` for development.
Audio is enabled by default; `MasterVolumePercent` is the final host gain from zero through
100 after SNES mixing. `ReportErrorsToGitHub=true` makes the desktop host print each distinct
recoverable error locally, queue a private GitHub issue through the authenticated `gh` CLI,
and attempt the next emulated frame. Reports include the exception, frame/input, active
room/state/door pointers, and recording path, but no ROM data. A stable `SMERR-...`
fingerprint suppresses repeats in the same process and is searched across both open and
closed issues before creation. Because CLR exceptions cannot resume at their exact throw
site, a repeatedly retried operation can still stall until its translation is fixed, though
it no longer terminates the host. Startup and fatal background-thread failures still stop
because their outer runtime boundary cannot resume. Unknown sections, unknown or duplicate
keys, invalid Booleans, malformed repository names, and out-of-range volume values fail with
a file and line number.

Battery-backed data is stored as `%LOCALAPPDATA%/SuperMetroid/SuperMetroid.save.json`. Each
slot names its checkpoint, resources, equipment, controller bindings, game time, progression
bit sets, boss state, and explored map coordinates. Schema v2 also names reserve missiles,
text language, the item-load counter, and global game completion; it has no raw SRAM blob.
Old v1 JSON is upgraded automatically with its original retained in `.bak`.
See [the save schema inventory](../docs/save-schema-v2.md) for field meanings and migration.
Writes use a same-directory temporary file and atomic replacement;
the previous JSON is retained as `.save.json.bak`.

When no JSON save exists, an existing 8 KiB `.srm` is imported once and immediately written
as JSON. The legacy `.srm` is left untouched as a migration backup and is no longer updated.
Malformed JSON, unknown properties, unsupported schema versions, invalid enum/bit values,
and wrong-sized legacy saves fail with the file and property path instead of being ignored.
The implementation retains cartridge checksums, file-select ENERGY/TIME metadata, inventory
and equipment, events, boss bits, Chozo/item bits, explored map data, Ceres automatic
checkpoint, natural gunship save/reload, and room save-station confirmation/persistence.
Existing saves resolve the native area/load-station record. Their native load-appearance
presentation runs its ROM palette effect and complete 360-frame locked front-facing sequence
before restoring ordinary movement.

### Automatic Windows session logs

Every Windows game launch automatically captures stdout and stderr under `logs` beside
`SuperMetroid.Game.exe`, regardless of the launch working directory or INI/GitHub settings.
Console output remains visible. Each session has a unique timestamped
`SuperMetroid-<UTC timestamp>-<PID>-<unique ID>.zip` containing only `session.log`: build version,
startup/configuration messages, full fatal and recoverable exceptions, their existing runtime
context, and reporting/shutdown output. Send the ZIP from the affected session when reporting
a failure; no shell redirection or GitHub login is required.

Normal exit finalizes the ZIP. A fatal boundary also publishes it **before** waiting for Enter,
including background-thread failures. A flushed sibling `.log` remains while the process runs;
if the process is forcibly killed, send that text instead. Failed archive writes preserve the
text and print their full exception without a modal dialog. The executable's directory must
allow creation of `logs`. Archives are local and not uploaded automatically; they do not
bundle ROMs, assets, saves, debugger states or input recordings. Paths and console diagnostics
can contain personal information, so review logs before public posting. Logs are not
automatically deleted between sessions.

Focused implementation confirmation (no gameplay):

```powershell
dotnet run --project src/SuperMetroid.DesktopVerification -- --session-log-audit "C:\path\to\SuperMetroid.Game.dll"
```

### Automatic input recordings and replay

Every normal game reset creates an always-on recording under
`%LOCALAPPDATA%/SuperMetroid/input-recordings`. The `.smrec` file contains the reset-time 8 KiB SRAM image, the ROM's SHA-256
digest, the startup host option, and one raw controller word for every submitted frame. It
does not contain ROM bytes. Periodic atomic flushes run off the UI thread; closing the game,
restarting, or exiting after a managed failure waits for a final flush. Because Restart reads
the current `.save.json` before opening a new recording, each session begins from the last durable
save instead of carrying an earlier in-memory run forward.

Replay a captured failure with the automatically discovered ROM:

```powershell
dotnet run --project src/SuperMetroid.Game -- --replay "C:\path\to\input-recordings\SuperMetroid-input-20260901-120000-000.smrec"
```

An explicit ROM path may follow the recording path. Replay rejects a SHA-256 mismatch, uses
the recording's SRAM/INI seed, ignores live keyboard/gamepad input, and never writes replay mutations
back to the user's `.save.json`.

### Debugger save states

The playable window has a **State slot** selector and **Save State** / **Load State** buttons
for ten persistent debugger slots, numbered 0-9. Slot files are written to `debug-states`
under `%LOCALAPPDATA%/SuperMetroid` as `SuperMetroid-debug-slot-N.smstate`; that is the file to attach to
an issue when a failure depends on exact timing or late-game state. Loading replaces the
complete managed frontend/runtime/address-space graph at the captured frame, refreshes the
visible frame immediately, restores the managed SPC sequencer/DSP state, resets only the host
audio buffers, and begins a new controller recording from the restored boundary.

The state header records the slot, UTC timestamp, frame, game state, active room/state
pointers, core/desktop build IDs, and ROM SHA-256. A different build produces a console/status
warning and attempts restoration; it does not require approval or reject the state by itself.
New schema-three states identify delegates by method name and signature instead of compiler
token order. Schema-two states remain readable, with an additional cross-build legacy-token
warning. Actual field/method incompatibilities, malformed data, unsupported schemas, and
different ROMs still fail clearly. Empty slots show an informational message and keep the
live game. These are private
debug artifacts: the compressed payload contains the full emulated address space, including
ROM and SRAM bytes, and is therefore not suitable for distribution.

The headless deterministic round-trip audit is:

```powershell
dotnet run --project src/SuperMetroid.DesktopVerification -- --state-audit "..\Super Metroid.smc"
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
effects. The current high-level gap list is maintained in this README.

### Enemies

The enemy pass is complete as a translated subsystem. `RoomEnemySystem` consumes compiled
ordered populations, graphics sets and complete enemy definitions, preserving native slot
order, spawn snapshots, graphics staging, boss bookkeeping and the normal scheduler.
Installed PNG/JSON catalogs supply presentation; Core cannot read cartridge bytes.

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

Room loading now consumes immutable decoded PLM placements exactly once in native order. Core
cannot parse population/header/scroll/item-graphics bytes from a cartridge bus. It allocates
the native forty-slot pool from highest ID downward before dispatching reusable setup handlers;
synchronous-delete setups free their physical slot for the next record just as `$84:846A` does.
Unsupported headers throw with population pointer, record index, header/setup/list pointers,
coordinates, and room argument. The private-ROM exhaustive audit currently measures all 941
retail records across all 70 headers as translated, including the three-header Draygon-cannon
family and its shared bank-$A5 firing-control words.

Room headers expose FX, X-ray, room-main, PLM, background, and setup pointers, but only the
translated consumers are executed. Room loading now selects the door-matched sixteen-byte FX
record from compiled definitions and runs its bank-$8D palette-object bitset through a shared
compiled-program interpreter; unsupported
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

Each frame publishes a display packet (its layers, placed images, colours and effect descriptions), and the
Direct3D 11 backend draws it from the installed pre-coloured images; `docs/display-backend.md` lists each pass's
composition rule.

Audio is a C# translation of Super Metroid's SPC sequencer and eight-voice S-DSP mixer. It
implements pitch, pan/volume, ADSR and gain envelopes, Gaussian interpolation, noise, pitch
modulation, voice borrowing, and the shared FIR echo buffer. C# also owns the retail bank-$80
music ring, three bank-$82 SFX handshakes, and acknowledgement timing. The game session advances
the SPC port model every update whether or not audio is enabled or a device exists; Windows
`waveOut` owns only buffered delivery of the resulting 48 kHz stereo PCM.

Runtime upload commands resolve against the SHA-verified catalog in
the installed `game/audio/` directory. Developer tools may still use
`../standalone-assets/audio`. Exact `.spcu` streams remain
the source of sequence and SFX command data; the manifest's per-bank `instruments` arrays now
own the editable source/noise selector, ADSR bytes, gain byte, and pitch-base word installed
after each upload. BRR decoding occurs only during asset extraction. Runtime voices read
16-bit mono PCM WAVs and apply pitch, pan, envelopes,
looping, cancellation/voice stealing, Gaussian interpolation, noise, pitch modulation, and FIR
echo dynamically in C#. The manifest maps 935 bank/source aliases onto 112 deduplicated WAVs,
each with a stable ID, sample rate, loop frame, and SHA-256 digest. Unknown commands, malformed
streams/WAVs, incomplete or inconsistent instrument banks, missing sources, invalid loops,
address misses, and digest mismatches throw.

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
demo-only calls remain absent with those untranslated states. The managed implementation translates
Super Metroid's sound driver; it is intentionally not a general-purpose SPC700 CPU emulator.

The permanent ROM-free audio regression covers every music bank, all three SFX libraries,
simultaneous SFX and cancellation, bank changes, stop, pause/resume, track restoration, all 935
aliases, and the 112-WAV deduplication invariant. The acknowledgement hash remains identical to
the pinned cartridge-driver oracle. The PCM hash is the stock extracted-WAV baseline: unlike
live BRR, a PCM loop repeats decoded frames rather than reapplying BRR predictor history at each
loop boundary, which is the intentional seam that makes arbitrary replacement audio possible.

```powershell
dotnet run --project src/SuperMetroid.DesktopVerification -- --managed-audio-audit
```

For a non-device smoke test that runs the real initial upload/music sequence from the installed
audio and proves the mixer produced nonzero PCM:

```powershell
dotnet run --project src/SuperMetroid.DesktopVerification -- --audio-audit
```

To exercise the Windows `waveOut` queue beyond its six-buffer capacity without a ROM or an
audible test tone, run:

```powershell
dotnet run --project src/SuperMetroid.DesktopVerification -- --waveout-audit
```

To replay an always-on controller journal through the managed SPC without opening the window
or an audio endpoint (useful when an SFX handshake appears to block a door transition):

```powershell
dotnet run --project src/SuperMetroid.DesktopVerification -- --audio-input-replay-audit `
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

`tools/verify-all.ps1` (run from the repository root) builds once and runs every verifier
project's suite plus the SMV replays. The replays read committed compact traces under
`csharp/test-fixtures/replay-traces/` (`mother-brain-glitch`, `low-13-percent`,
`full-100-percent`, `samus-drunk`, `lowc`, `ridley`), produced from private full native captures with
`--compact-replay-trace SOURCE_DIR DEST_DIR`; see `docs/smv-gameplay-parity.md`.

### Developer tools

`SuperMetroid.DebugRunner` hosts file-taking developer tools: extracted-installation
preparation and validation, Android bundle replay, assembly-metadata and captured-frame
comparisons, state-fixture exporters, and the generated-definition writer
`--generate-room-fx-records`. The enemy visual selector catalog is frozen generated data (#1367);
`SuperMetroid.Verification --verify-enemy-visual-selectors` checks every entry against the cartridge. The dispatchers live in
`src/SuperMetroid.DebugRunner/Tools/`.

### Asset commands

Asset operations share the DebugRunner developer executable under the `assets` subcommand.

```powershell
dotnet run --project src/SuperMetroid.DebugRunner -- assets ../standalone-assets/raw ../standalone-assets/png
dotnet run --project src/SuperMetroid.DebugRunner -- assets audio ../standalone-assets/raw ../standalone-assets/audio
dotnet run --project src/SuperMetroid.DebugRunner -- assets audio-override-init "$env:LOCALAPPDATA\SuperMetroid"
dotnet run --project src/SuperMetroid.DebugRunner -- assets audio-replace "$env:LOCALAPPDATA\SuperMetroid" music-0f-track-05 brinstar.wav 2563200
dotnet run --project src/SuperMetroid.DebugRunner -- assets audio-replace "$env:LOCALAPPDATA\SuperMetroid" sfx-1-0b power-beam.wav none
dotnet run --project src/SuperMetroid.DebugRunner -- assets room ../standalone-assets/raw ../standalone-assets/rooms/LandingSite.png
```

The extractor produces named raw chunks, PNGs, manifests, composed-room diagnostics, exact SPC
upload streams, audio metadata, and canonical decoded PCM WAVs. Installation then renders every
song and every sound effect, in every bank context, into finished 16-bit stereo clips at the
native 32,040 Hz (`game/audio/clips`, listed in `audio-clips.json`); the game plays those clips.

Audio is modded by replacing finished clips. Initialize `overrides/audio` once with
`audio-override-init`, then install each replacement with `audio-replace`, naming the clip by its
stable identity: `music-{data index}-track-{nn}` for a song (the data index in hexadecimal, as
the extracted catalog names tracks) or `sfx-{library}-{command}` for a sound effect (the command
in hexadecimal; it replaces the sound in every bank context). A replacement must be an
uncompressed 16-bit stereo WAV at 32,040 Hz. The optional last argument is the frame a song loops
back to, or `none` to play it once; a sound that never ends on the cartridge needs a loop, and a
one-shot sound cannot have one. Each edit is validated against the stock clips before it replaces
the override, so a rejected edit leaves the previous override in place.

Startup validates the immutable `game/audio` clips first and the override second; corrupt stock
or user content fails loudly instead of silently choosing the other copy. The override sits
outside the replaceable `game/` directory, survives stock repair and application updates, and is
selected by both Windows and Android after restart. Initialization refuses to overwrite an
existing override directory. Instrument, sample and sequence edits are not overrides: they are
stock-content diagnostics in `game/audio` and are replaced by repair.

Room tileset artwork is modded the same way: place an edited copy of a stock file in
`overrides/room-characters`, `overrides/room-blocks` or `overrides/room-palettes` under the same name
as the stock file. Files are named for the artwork, not its cartridge location: a character sheet
is `room-characters-{set}.png` (for example `room-characters-upper-crateria.png`), a block table
`room-blocks-{set}.json` and a palette `room-palette-{set}.json` (for example
`room-palette-heated-norfair.json`); the common room elements stay `room-characters-cre.png` and
`room-blocks-cre.json`. Overrides saved under the older names, which carried the cartridge address
(`room-characters-BAC629.png`), are renamed when the installation is rebuilt; an override present
under both names stops the rebuild, leaving the previous installation in place, until one of them is removed.

Sprites are moving to finished images. A sprite family that has moved is drawn from a pre-coloured
sheet in `game/sprite-sheets`: an RGBA PNG (`{family}.png`, transparent where alpha is zero) and its
frame list (`{family}.json`: each frame's rectangle on the sheet, its origin, whether it takes part
in colour math, and the colours it was coloured with), and a palette map (`{family}.palette.png`: each
pixel's sprite palette row and colour, in red) that lets in-game palette effects such as flashes recolour
the sheet. Place an edited copy of either in `overrides/sprite-sheets` under the same name; an
edited frame list must keep every frame the family draws. The escape countdown (`escape-timer`) is drawn
this way, as are Samus's projectiles, bombs, charge flare and trails: one sheet per beam combination
(`projectiles-power`, `projectiles-plasma-ice-wave` and so on), whose characters and beam colours differ,
and `intro-projectiles` for the intro's flashbacks, the grapple beam's endpoint and rope (`grapple`), Samus's body (`samus-body-top` and `samus-body-bottom`, in
the Power Suit's colours; the other suits recolour it through its palette map), the arm cannon's covers and the
chest cover drawn over it (`samus-overlays`, coloured likewise), and her footstep splashes, lava spray, dust, diving
splash and bubbles (`samus-atmosphere`), and her suit explosion as she dies (`samus-death`; the death palette recolours
it). Ordinary enemies are drawn from `enemy-sheets`: one sheet for each enemy as a room's graphics set loads it, in the
colours its palette row holds there, so an enemy shown in another palette has a sheet of its own; `enemy-sheets.json`
names each graphics set's sheet for each of its enemies. The `Spawned-` sheets beside them hold what enemies spawn
(their projectiles and the room's dust, explosions and other sprite objects, and the Ceres elevator's platforms), one
for each graphics set and tile base the objects draw at; `enemy-sheets.json` names those too. A room with no enemies
loads no set and draws under the one the previous room left, or, as a session's first room, under `SessionStart`: the
standard characters alone, with only the objects any room spawns. A boss that uploads characters over its room's
mid-fight (Draygon, the gunship, Crocomire, the Torizos, Ceres Ridley's getaway) has, for each frame drawn over them, layers of its parts each
pictured for every upload the characters it reads can hold, and the game draws the layers for what is uploaded now;
`enemy-sheets.json` names each such frame's layers. Frames an encounter draws itself, as Ridley's tail and wings and
the Baby he carries, are in its set's character-zero `Spawned-` sheet. Graphics sets loading Mother Brain or a corpse,
whose characters change in ways not yet pictured, still draw from characters. The game-over screen's Baby, egg and cursor are drawn from
`game-over`, file select's borders, cursor, helmets and copy arrows from `file-select`, the options menu's headings and
cursor from `game-options`, and the file-select and pause maps' markers, labels, arrows and position indicator from
`map-sprites`, the pause menu's equipment selectors and reserve-tank strip from `pause-sprites`, and the title's
reveals and copyright from `title`, the intro's cinematic actors and caret from `intro-sprites`, the Ceres approach's stars and rear view from
`ceres-flight-sprites`, the Ceres destruction scene's actors from `ceres-destruction-sprites` and, once Zebes is
revealed, `zebes-reveal-sprites`, and the ending's actors from a sheet for each set of characters its scenes show them
over: `ending-cloud-sprites`, `ending-explosion-sprites`, `ending-suited-reward-sprites` or
`ending-suitless-reward-sprites`, then `ending-shooting-sprites`, which gives way to `ending-shooting-logo-quarter-sprites`,
`ending-shooting-logo-half-sprites`, `ending-shooting-logo-three-quarters-sprites` and `ending-logo-sprites` as the
logo's characters replace the shooting scene's. The escape countdown's layout, `escape-timer.json` in `overrides/maps`, now holds only the
anchors and digit spacing; a layout written for the earlier version, which also held the frames and
palette, is rejected until it is rewritten.

Room blocks are drawn from pre-coloured block sheets in `game/room-block-sheets`, one for each tileset
(`{blocks}.{characters}.{palette}`, for example `Norfair.Norfair.HeatedNorfair`), in the same three files as a sprite
sheet. Each 16x16 picture is one block as its definition gives it, before the level mirrors it, coloured with the
tileset's background palette; its name lists the block's four tile words and, for each, the artwork its character
shows. A block whose characters a room rebinds after loading (animated spikes, sand and treadmills, the Tourian statues,
collectibles' graphics, library background transfers and Kraid's backdrop) has a picture for each artwork that can
show there, and the room draws the one its bindings show now. X-ray's reveal draws its blocks from the same pictures.
A room's library background is a full image in the same directory, one for each background, door variant and tileset
(`{background}[.{door}].{tileset}`): both BG2 pages it loads, the first above the second, with their tilemap words listed
in `room-backgrounds.json`. A 16x16 cell whose characters the room rebinds, as the Tourian statues' are, draws that
cell's block picture while they show something else. The scrolling sky is an image of all its rows the same way, one for each
sky (`SkyLand`, `SkyOcean`) and tileset. Words written over a room's BG2 after it loads (the bodies of Kraid, Phantoon,
Draygon, Crocomire and Mother Brain, and the Zebes escape warning) are pictured on one sheet per tileset,
`Words.<tileset>`, with an image of each part each writer writes; `tile-words.json` records where each tile word is
pictured over each artwork that can show at its character. BG3 has a sheet per tileset too, `Bg3Words.<tileset>`: the
HUD fully stocked, every other word the HUD writes, the minimap's map tiles and each room effect's page, in BG3's
four-colour rows with every pixel keeping its palette index, colour zero included. Each tileset's gameplay message boxes
are `Messages.<tileset>`: one image of each message in each form it takes (each button a panel can name, each save
choice), in its BG3 colours with the two colours a message box installs; `message-boxes.json` records the words each
image pictures. Each Ceres tileset's Mode 7 tiles are one picture, `Mode7.<tileset>`, sixteen tiles to a row;
`mode7-tiles.json` records what each tile was pictured over.
Edited copies go in `overrides/room-block-sheets`. The
Ceres Ridley arena, whose planes read the object characters, still draws its blocks from characters.

Sound effects are decoded into the manifest's `soundPrograms` catalog. Each effect in
`soundLibraries` has a stable ID such as `sfx-1-01` and names the channel programs it routes.
Routing pointers and voice-allocation configurations are compiled engine mechanics and cannot
be overridden. A program may edit operands or substitute a same-size operation, but its encoded
byte count must retain `byteCapacity`; this prevents an authored edit from relocating into an
adjacent resident-driver table. Instruction arguments are:

- `playNote`: instrument, volume, pan, note (`246` retains the previous note), duration.
- `setAdsr`: ADSR1, ADSR2, and the two native reserved bytes.
- `pitchSlideLegato` / `pitchSlide`: subnote delta and target note.
- `beginRepeat`: repeat count; `endRepeat` closes it; `repeatForever` loops indefinitely.
- `enableNoise` and `end`: no operands.

The loader recompiles and validates every program after each audio-bank upload. Unknown
operations, reserved instrument opcodes, missing program references, routing changes, and
encoded-size changes fail with the responsible stable ID instead of falling back to opaque data.

Music banks likewise expose three address-stable layers: `musicTracks` contains top-level phrase,
repeat, and fast-forward flow; `musicPhrases` routes eight channels; and `musicPrograms` contains
notes plus named effects such as `setInstrument`, `setTempo`, `enableVibrato`, `callPattern`, and
`configureEcho`. A channel instruction keeps its optional length/articulation bytes in `timing`,
its readable operation name, exact `opcode`, and effect operands in `arguments`. Track, phrase,
and program IDs include the bank data index and native address so references remain unambiguous.
As with SFX, edits must retain each program's `byteCapacity`; overlapping native entry points are
accepted only while they compile to the same bytes. Conflicting aliases, missing phrases/called
programs, changed routing identities, or malformed effect operand counts fail explicitly.

Each validated audio selection has a canonical `ContentIdentity`: SHA-256 over the normalized
manifest, which already contains every upload/WAV digest and all authored definitions. Windows
and Android combine that value with the selected map/projectile identities, supported source
revision, and compiled Core build into a framed installation identity printed at session start.
Formatting-only JSON changes retain identity; any audible or visual content edit changes the
responsible component and aggregate digest. New controller recordings and debugger states store
that installed-content identity alongside the supported source-revision digest. Older artifact
schemas remain loadable with an explicit compatibility warning. Installed hosts obtain the known
revision digest from the verified catalog and no longer reopen the private ROM merely to hash it.

Gameplay consumes compiled mechanics, installed presentation catalogs and active console
memory. Cartridge decoding is isolated in AssetExtraction; the ROM-free boundary is not
whole-game parity proof.

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
of stable source checkpoints. `test-temp/` holds only per-run scratch that runs create and
delete. Persistent local caches that runs reuse (the verifiers' extracted installation, the
suite-duration hints and the built ending PPU probe) live in ignored `csharp/.cache/`; deleting
it only costs the next run their rebuild.
