# C# port workspace

Open `SuperMetroid.slnx` in Visual Studio. The code currently contains the first independently translated subsystem:

- LoROM address conversion
- Super Metroid command-stream decompression (`$80:B119`)
- SNES 2bpp/4bpp planar tile decoding
- Mode 7 chunky tile decoding
- SNES BGR555 palette decoding
- dependency-free PNG output and JSON asset inventory
- a WinForms viewer with a static Landing Site tab and live frame-steppable runtime tab
- exact bank `$80` RNG, timed-held-input, event, boss-state, and multiplication routines
- typed bank `$80` VRAM write queue with exact VMAIN word/column stepping and DMA bank wrap
- the bank `$80` escape-timer state machine, including BCD arithmetic and fixed-point motion
- the `$80:9459/$80:9583` controller/NMI frame seam for incrementally wiring game logic
- a 24-bit cartridge/WRAM/SRAM bus with the native port's exact LoROM mirror expression
- packed 544-byte OAM and distinct bank `$81` generic/Samus spritemap emission
- modeled 256-color CGRAM and OBSEL/4-bpp OBJ rendering from real timer and Samus VRAM tiles
- first normal-gameplay Samus render/movement slice: standing pose `$01` and ordinary
  right-running pose `$09`, signed pose-offset math,
  top/bottom spritemap lookup, power-suit OBJ palette, bank-$92 animation/tile definitions,
  and bank-$80's four-destination dedicated NMI graphics DMA
- bank-$90 dry-room Samus animation countdown plus pose `$01`'s healthy and low-energy
  bytecode loops (`$F6` and `$FE,$04`), with distinct staged and NMI-visible OAM buffers
- read-only bank-$91 prospective-pose matcher with exact required-new/required-held masks,
  ROM-order priority, and winning-record addresses; the verified `$01`/`$09` route is applied
  while every other physics-dependent result stays blocked
- bank-$90 horizontal-speed core with the bank-$94 normal-air base-pointer handoff, exact
  12-byte cartridge entries, 16.16 acceleration/deceleration, divisor, and displacement clamp
- bank-$94 radius-aware horizontal/vertical room-block scans for air, non-square slopes, and
  ordinary solids, including ROM slope multipliers/heights and post-X Y alignment
- bank-$90 grounded standing/running in both directions plus `$25/$26` reversal: exact speed
  calculation, old-direction mode-one carry, `$F8` animation completion, collision, and fallback
- exact four-row BG3 HUD initialization, WRAM-to-VRAM updates, digits, icons, and palette bits
- exact Landing Site scroll-table loading, directional boundaries, stationary autoscroll,
  and bank-$90 16.16 Samus camera target/speed calculations
- bank-$80 BG1/BG2 parallax, signed block tracking, row/column staging, ring-buffer DMA
  geometry, and execution into modeled VRAM
- cartridge-backed Landing Site graphics loading, native 17-column initial BG1 fill, and a
  scroll-aware 4-bpp renderer used as the viewer's live foreground
- shared decompressed room allocation preserving logical BG1 words and their parallel BTS
  bytes for bank-$94 collision, while retaining native overread adjacency for BG streaming
- bank-$88 Landing Site fixed-point sky HDMA, circular four-row BG2 uploads, BGSC 32x64
  tilemap geometry, and a fully live BG2/BG1/BG3/OBJ compositor
- ROM-parsed Landing Site door entry and command-E library-background selection, including
  the cartridge's harmless camera-Y-zero wrapped pointer read
- host-stimulated Landing Site camera controls over the live PPU-modeled layers
- a dependency-free verification executable with exhaustive/boundary-oriented checks

`SuperMetroid.AssetExtractor` is configured with debug arguments, so it can be selected as the startup project and stepped through immediately. From this directory, the equivalent command is:

```powershell
dotnet run --project src/SuperMetroid.AssetExtractor -- ../standalone-assets/raw ../standalone-assets/png
dotnet run --project src/SuperMetroid.AssetExtractor -- room ../standalone-assets/raw ../standalone-assets/rooms/LandingSite.png
```

To inspect the composed room and live translated state interactively, select `SuperMetroid.RoomViewer` as the startup project and choose its `Landing Site viewer` launch profile. No arguments are required: startup searches the current/executable directory and their parents for `standalone-assets/raw` and `Super Metroid.smc`; `SUPERMETROID_ROM` or an explicit ROM argument can override the latter. The equivalent command is `dotnet run --project src/SuperMetroid.RoomViewer`. The **Landing Site** tab renders the complete static room. **Frame runtime** now starts directly in the grounded sandbox, placing Samus on Landing Site's real type-8/BTS-`$00` solid floor at block row `$4D`, where the ROM-derived cave terrain is visibly present. Toggle **Hold Left** or **Hold Right**, then use **Step frame**, **Step 60**, or **Play**. Releasing a direction runs native momentum/deceleration and returns the matching running pose `$09/$0A` to standing `$01/$02`. Pressing the opposite direction enters ROM turn pose `$25/$26`: acceleration mode one carries Samus in her old direction while decelerating, animation command `$F8` finishes the facing change, and continued input starts the mirrored run. The translated main-scrolling routine follows Samus using the pose's ROM direction/movement bytes and Landing Site's `$70/$A0` header scrollers; crossed 16-pixel boundaries stream their new BG edge. The HUD minimap uses Landing Site's map origin `(23,0)`, Crateria's bank-$82/$B5 map records, visited-tile bits, and the native eight-frame center blink. Its live center coordinate is printed beside the camera in the status line. **Live PPU layers** switches back to the deliberately exposed incomplete BG1/BG2 path; leaving it unchecked uses the precomposed terrain only for the background while HUD, Samus tiles, OAM, camera, animation, movement, and collision remain live. The Ceres/Mother Brain restarts retain the earlier stationary cinematic render stimulus. Useful first breakpoints are `SamusGroundedMovement.StepTurningOnGround`, `SamusGroundedMovement.StepRunningLeft`, `SamusGroundedMovement.StepRunningRight`, `ScrollBoundaryCamera.TrackMovedSamusHorizontally`, `HudState.UpdateMinimap`, `SamusBlockCollision.MoveHorizontal`, `SamusBlockCollision.MoveVertical`, `SamusState.HandleAnimationDelay`, `SamusState.Draw`, `SamusHorizontalSpeedState.CalculateBaseSpeed`, and `SuperMetroidRuntime.StepFrame`.

To step through the first translated game-support routines, select `SuperMetroid.Verification` as the startup project and place breakpoints in `Bank80SystemState`. From the command line:

```powershell
dotnet run --project src/SuperMetroid.Verification
```

`SuperMetroid.DebugRunner` is the evolving frame-level debugger host. Its Visual Studio launch profile finds the private ROM from the repository root. A particularly useful breakpoint is the deliberately non-inlined `FrameBreakpoint`; from the command line, run:

```powershell
dotnet run --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 240 --timer ceres
dotnet run --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 18 --grounded-run --right-frames 5
dotnet run --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 210 --reversal-script
```

The debug runner currently exercises real ROM-to-VRAM DMA, NMI/controller latching, counters, the escape-timer state machine, packed OAM, CGRAM, the gameplay BG3 HUD, an OBSEL-aware software OBJ renderer, streamed BG1, bank-$88's scanline-scrolled BG2, and animated grounded Samus poses `$01/$02/$09/$0A/$25/$26`. It parses the landing-cutscene door header and matching library-background command directly from the ROM before frame one. Samus's pose tables, animation-delay bytecode, palette, split graphics definitions, four NMI DMA destinations, OAM pieces, prospective-pose input table, speed entry, slope multiplier/height, BG1 block, and BTS byte are likewise read from the cartridge. In `--grounded-run`, held Right applies the winning `$91:A16A` transition after the standing frame's movement/animation, then logs every 16.16 speed, accepted horizontal displacement, and grounding result. `--right-frames N` releases Right after N scripted samples so momentum deceleration and the `$09`→`$01` fallback are reproducible. `--reversal-script` automatically enables the grounded scenario and feeds Right 60 frames, Left 60, Right 60, then releases; this reproducibly exercises both ROM turn poses and both `$F8` completions. The runner writes the composed frame to `standalone-assets/runtime/EscapeTimerFrame.png`, its transparent sprite layer to `EscapeTimerFrame.objects.png`, live BG1 to `EscapeTimerFrame.background1.png`, and live BG2 to `EscapeTimerFrame.background2.png`; pass `--output path.png` to override the base output. Extracted raw assets are not a runtime dependency. This remains an incremental runtime shell, not yet a complete playable C# port.

The first PNG pass is deliberately diagnostic: tile images use grayscale pixel indices, while palette files become accurate color swatches. The static room path composes real foreground/background block layers for asset inspection. The interactive grounded view intentionally reuses that ROM-derived composition until the live library-background path reproduces all terrain; the **Live PPU layers** toggle makes the incomplete all-live result directly comparable. Camera buttons and the initial visible Samus X/screen framing remain explicit host stimuli. The grounded floor and movement results are native; the landing-cutscene door itself has no normal-gameplay Samus spawn, so the host does not pretend otherwise. Square and non-square slope collision and grounded left/right reversal are translated. Special block dispatchers, jumping/falling, run-button/speed-booster state, native spawn selection, dynamic PLMs, enemies, effects, and cutscene actors still remain. Unsupported collision paths throw instead of being treated as air or solid. The raw files and PNGs are private ROM-derived material and should not be committed or distributed.
