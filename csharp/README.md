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
- normal-gameplay Samus render/movement slices: standing/running `$01/$02/$09/$0A`, grounded
  turns `$25/$26`, neutral jump `$4B-$4E`, spin jump `$19/$1A`, falling `$29/$2A`, and
  landing `$A4-$A7`, plus crouch/stand `$27/$28/$35/$36/$3B/$3C`; signed pose-offset math,
  top/bottom spritemap lookup, power-suit OBJ palette, bank-$92 animation/tile definitions,
  and bank-$80's four-destination dedicated NMI graphics DMA
- bank-$90 dry-room Samus animation countdown plus pose `$01`'s healthy and low-energy
  bytecode loops (`$F6` and `$FE,$04`), with distinct staged and NMI-visible OAM buffers
- bank-$91 prospective-pose matcher with exact required-new/required-held masks, ROM-order
  priority, and winning-record addresses; verified ground, turn, jump, fall, and landing
  results are applied while untranslated physics-dependent results stay blocked
- bank-$90 horizontal-speed core with the bank-$94 normal-air base-pointer handoff, exact
  12-byte cartridge entries, 16.16 acceleration/deceleration, divisor, and displacement clamp
- bank-$94 radius-aware horizontal/vertical room-block scans for air, non-square slopes, and
  ordinary solids, including ROM slope multipliers/heights and post-X Y alignment
- bank-$90 grounded standing/running in both directions plus `$25/$26` reversal: exact speed
  calculation, old-direction mode-one carry, `$F8` animation completion, collision, and fallback
- bank-$90 ordinary dry-air neutral/spin jump and falling: ROM initial velocity/gravity,
  old-speed 16.16 displacement, variable-height cutoff, ceiling/floor collision, radius-aligned
  landing, `$FD/$F8/$FE/$FF` animation bytecode, and spin bottom-half selection
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

To inspect the composed room and live translated state interactively, select `SuperMetroid.RoomViewer` as the startup project and choose its `Landing Site viewer` launch profile. No arguments are required: startup searches the current/executable directory and their parents for `standalone-assets/raw` and `Super Metroid.smc`; `SUPERMETROID_ROM` or an explicit ROM argument can override the latter. The equivalent command is `dotnet run --project src/SuperMetroid.RoomViewer`. The **Landing Site** tab renders the complete static room. **Frame runtime** starts in the grounded sandbox on Landing Site's real type-8/BTS-`$00` floor at block row `$4D`, where ROM-derived cave terrain is visibly present. Toggle **Hold Left**, **Hold Right**, **Hold Jump**, **Hold Up**, or **Hold Down**, then use **Step frame**, **Step 60**, or **Play**. Jump can be released for the native short arc or held for the full arc; standing selects `$4B/$4C -> $4D/$4E`, running selects spin poses `$19/$1A`, upward collision begins falling, and floor collision selects landing `$A4-$A7`. Down enters `$35/$36 -> $27/$28`; Up expands the collision body through `$3B/$3C -> $01/$02` and is rejected when terrain leaves insufficient standing room. Releasing a direction runs native momentum/deceleration and returns `$09/$0A` to `$01/$02`; pressing the opposite direction enters `$25/$26` and preserves old-direction momentum. The main-scrolling routine follows both horizontal and vertical movement and streams crossed room edges. The status line exposes X/Y 16.16 position and speed, vertical direction, pose/frame, camera, minimap, and pending ROM transition. **Live PPU layers** exposes the incomplete all-live background path; leaving it unchecked uses precomposed ROM terrain while HUD, Samus tiles, OAM, camera, animation, movement, and collision remain live. Useful breakpoints now include `SamusAerialMovement.StepNormalJump`, `SamusAerialMovement.StepSpinJump`, `SamusPostureMovement.StepCrouching`, `SamusState.TryApplyPostureTransition`, `SamusGroundedMovement.StepTurningOnGround`, `SamusBlockCollision.MoveHorizontal`, `SamusBlockCollision.MoveVertical`, `SamusState.HandleAnimationDelay`, `SamusState.Draw`, and `SuperMetroidRuntime.StepFrame`.

To step through the first translated game-support routines, select `SuperMetroid.Verification` as the startup project and place breakpoints in `Bank80SystemState`. From the command line:

```powershell
dotnet run --project src/SuperMetroid.Verification
```

`SuperMetroid.DebugRunner` is the evolving frame-level debugger host. Its Visual Studio launch profile finds the private ROM from the repository root. A particularly useful breakpoint is the deliberately non-inlined `FrameBreakpoint`; from the command line, run:

```powershell
dotnet run --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 240 --timer ceres
dotnet run --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 18 --grounded-run --right-frames 5
dotnet run --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 210 --reversal-script
dotnet run --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 190 --jump-script --output ../standalone-assets/runtime/JumpTraceFrame.png
dotnet run --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 125 --posture-script --output ../standalone-assets/runtime/PostureTraceFrame.png
```

The debug runner exercises real ROM-to-VRAM DMA, NMI/controller latching, timers, OAM/CGRAM, BG1/BG2/BG3, camera/minimap updates, collision, and animated Samus. `--reversal-script` reproducibly exercises both ROM turn poses and `$F8` completions. `--jump-script` performs `$01 -> $4B -> $4D -> $A4`, accelerates into `$09`, performs `$09 -> $19 -> $A6`, then returns through running deceleration. `--posture-script` crouches, stands, turns, then repeats while facing left, verifying movement types `$05/$0F` and the exact five-pixel radius alignment. Scripts log every 16.16 X/Y displacement, collision result, pose, animation command, and ROM definition address. Output produces the composed frame plus transparent OBJ, BG1, and BG2 diagnostics using the supplied base filename. Extracted raw assets are not a runtime dependency. This remains an incremental runtime shell, not yet a complete playable C# port; the exact admitted movement routes are tracked in `MOVEMENT_COVERAGE.md`.

The first PNG pass is deliberately diagnostic: tile images use grayscale pixel indices, while palette files become accurate color swatches. The static room path composes real foreground/background block layers for asset inspection. The interactive runtime intentionally reuses that ROM-derived composition until the live library-background path reproduces all terrain; **Live PPU layers** makes the incomplete all-live result directly comparable. Camera buttons and initial Samus placement remain explicit host stimuli because the landing-cutscene door has no normal-gameplay spawn. Square/non-square slope collision, grounded reversal, ordinary neutral/spin jumping, walk-off falling, ceiling collision, landing, crouching, and standing-radius collision are translated. Aimed poses, morph/spring ball, wall jump, aerial turns, grapple, damage movement, run-button/speed-booster state, native spawn selection, dynamic PLMs, enemies, effects, and actors remain. Unsupported routes throw rather than becoming guessed physics. Raw files and PNGs contain private ROM-derived material and must not be distributed; this private preservation repository intentionally retains them until a future shareable cleanup.
