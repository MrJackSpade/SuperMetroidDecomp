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
  aim `$03-$08/$0D-$12`, grounded
  turns `$25/$26`, neutral/aimed jump `$15-$18/$4B-$4E/$51-$5A/$69-$6C`, spin jump
  `$19/$1A`, ordinary/aimed falling `$29-$2E/$6D-$70`, and
  landing `$A4-$A7`, stationary aim `$03-$08`, plus crouch/stand
  `$27/$28/$35/$36/$3B/$3C`, direct crouch exits, and ordinary/aimed crouch-jump entry;
  ordinary Morph Ball `$1D/$1E/$1F/$31/$32/$37/$38/$3D/$3E/$41`, including item-gated
  entry, rolling/reversal, walk-off, dry-air bounce, landing, and collision-checked unmorph;
  signed pose-offset math,
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

To inspect the composed room and live translated state interactively, select `SuperMetroid.RoomViewer` as the startup project and choose its `Landing Site viewer` launch profile. No arguments are required: startup searches the current/executable directory and their parents for `standalone-assets/raw` and `Super Metroid.smc`; `SUPERMETROID_ROM` or an explicit ROM argument can override the latter. The equivalent command is `dotnet run --project src/SuperMetroid.RoomViewer`. The **Landing Site** tab renders the complete static room. **Frame runtime** starts in the grounded sandbox on Landing Site's real type-8/BTS-`$00` floor at block row `$4D`, where ROM-derived cave terrain is visibly present. Toggle **Hold Left**, **Hold Right**, **Hold Jump**, **Hold Up**, **Hold Down**, **Hold Aim Up**, or **Hold Aim Down**, then use **Step frame**, **Step 60**, or **Play**. Jump can be released for the native short arc or held for the full arc; standing selects `$4B/$4C -> $4D/$4E`, running selects spin poses `$19/$1A`, and stable crouch also selects `$4B/$4C` through its own radius/launch seam. Upward collision begins falling, and floor collision selects landing `$A4-$A7`. Down enters `$35/$36 -> $27/$28`; Up expands the collision body through `$3B/$3C -> $01/$02` and is rejected when terrain leaves insufficient standing room. Releasing Down while retaining the facing direction can use the table's direct `$27/$28 -> $01/$02` exit. From standing, Up selects `$03/$04`, Aim Up selects `$05/$06`, and Aim Down selects `$07/$08`. Holding the facing direction with Aim Up/Down selects aimed-running `$0F/$10` or `$11/$12`; releasing only the direction returns to stationary aim, while releasing every button spends native momentum before the pose-definition fallback to `$01/$02`. Pressing the opposite direction enters `$25/$26` and preserves old-direction momentum. The main-scrolling routine follows both horizontal and vertical movement and streams crossed room edges. The status line exposes X/Y 16.16 position and speed, vertical direction, pose/frame, camera, minimap, and pending ROM transition. **Live PPU layers** exposes the incomplete all-live background path; leaving it unchecked uses precomposed ROM terrain while HUD, Samus tiles, OAM, camera, animation, movement, and collision remain live. Useful breakpoints now include `SamusAerialMovement.StepNormalJump`, `SamusAerialMovement.StepSpinJump`, `SamusPostureMovement.StepCrouching`, `SamusState.TryApplyPostureTransition`, `SamusState.TryApplyCrouchJumpTransition`, `SamusState.TryApplyDirectCrouchToStandingTransition`, `SamusState.ApplyGroundedAimTransition`, `SamusGroundedMovement.StepTurningOnGround`, `SamusBlockCollision.MoveHorizontal`, `SamusBlockCollision.MoveVertical`, `SamusState.HandleAnimationDelay`, `SamusState.Draw`, and `SuperMetroidRuntime.StepFrame`.

The grounded viewer sandbox now explicitly grants only the Morph Ball item bit because save-file inventory loading has not been translated. To use the real input route, tap **Hold Down** to crouch, release it, then tap **Hold Down** again to morph. Left/Right rolls and reverses through `$1E/$1F`; Up performs collision-checked unmorph through `$3D/$3E -> $27/$28`. Check **Spring Ball equipped** before morphing to select `$79/$7A`; Jump then launches `$7F/$80` and can be released early for the native short arc. Useful breakpoints are `SamusMorphBallMovement.StepGrounded`, `SamusMorphBallMovement.StepFalling`, `SamusMorphBallMovement.StepSpringBallInAir`, `SamusMorphBallMovement.StepTransition`, and `SamusState.TryApplyMorphTransition`.

To step through the first translated game-support routines, select `SuperMetroid.Verification` as the startup project and place breakpoints in `Bank80SystemState`. From the command line:

```powershell
dotnet run --project src/SuperMetroid.Verification
```

`SuperMetroid.DebugRunner` is the evolving frame-level debugger host. Its Visual Studio launch profile finds the private ROM from the repository root. A particularly useful breakpoint is the deliberately non-inlined `FrameBreakpoint`; from the command line, run:

```powershell
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 240 --timer ceres
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 18 --grounded-run --right-frames 5
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 210 --reversal-script
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 190 --jump-script --output ../standalone-assets/runtime/JumpTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 125 --posture-script --output ../standalone-assets/runtime/PostureTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 165 --aim-script --output ../standalone-assets/runtime/AimTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 245 --aim-run-script --output ../standalone-assets/runtime/AimRunTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 390 --aim-air-script --output ../standalone-assets/runtime/AimAirTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 120 --aerial-turn-script --output ../standalone-assets/runtime/AerialTurnFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 24 --compact-air-script --output ../standalone-assets/runtime/CompactAirPoseFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 240 --compact-air-script --output ../standalone-assets/runtime/CompactAirTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 220 --aim-crouch-script --output ../standalone-assets/runtime/AimCrouchTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 180 --aim-turn-script --output ../standalone-assets/runtime/AimTurnTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 240 --crouch-turn-script --output ../standalone-assets/runtime/CrouchTurnTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 260 --crouch-jump-script --output ../standalone-assets/runtime/CrouchJumpTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 70 --morph-ball-script --output ../standalone-assets/runtime/MorphBallPoseFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 220 --morph-ball-script --output ../standalone-assets/runtime/MorphBallTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 80 --spring-ball-script --output ../standalone-assets/runtime/SpringBallPoseFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 160 --spring-ball-script --output ../standalone-assets/runtime/SpringBallTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 50 --bomb-jump-script --output ../standalone-assets/runtime/BombPlacedFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 87 --bomb-jump-script --output ../standalone-assets/runtime/BombExplosionFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 160 --bomb-jump-script --output ../standalone-assets/runtime/BombLifecycleTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 32 --knockback-script --output ../standalone-assets/runtime/KnockbackDamageBoostFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 8 --grapple-fire-script --output ../standalone-assets/runtime/grapple-firing.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 14 --grapple-fire-script --output ../standalone-assets/runtime/GrappleFireTrace.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 2 --grapple-script --output ../standalone-assets/runtime/GrappleTerrainBounce.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 60 --grapple-script --output ../standalone-assets/runtime/GrappleSwingHeld.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 100 --grapple-script --output ../standalone-assets/runtime/GrappleSwing.png
```

`--morph-ball-script` uses the required two distinct Down presses, reaches `$F9`'s grounded target, rolls and reverses while preserving the shared animation state, decelerates to `$41`, and expands through `$3E/$FD` to crouching. Its frame-count-aware assertions permit both a ball-pose PNG and the complete transition trace without accepting milestones that the requested frame count did not reach.

`--spring-ball-script` equips bits `$0004/$0002`, proves `$F9` selects `$79`, rolls through `$7B`, launches `$7F` with the cartridge's 4.E000 velocity, and follows real ceiling/floor collision through automatic bounce recovery. The 80-frame capture freezes its powered airborne pose; the 160-frame trace proves landing.

`--bomb-jump-script` morphs normally and presses only the default Shoot/X controller bit. The translated `$90:BF9D/$C0E7` producer allocates one of five real bomb slots, `$90:C128` counts 60 down through the fast-animation seam at 15, and `$93:81E9` interprets the private ROM's slow, fast, explosion, goto, and delete records. At timer eight, `$A0:97E2-$A0:984E` publishes straight direction two; the following frame's `$90:DF99` setup installs `$90:E025/$E032`. The same fixed-sprite upload and bank-$93 spritemaps render the bomb/explosion before ordinary Morph Ball descent and floor recovery. The 50-frame capture shows an active bomb, the 87-frame capture freezes the explosion during ascent, and the 160-frame trace proves allocation through deletion and landing.

`--knockback-script` supplies only the left/right result normally produced by an enemy collision, because actors are not translated yet. From that explicit seam, the cartridge supplies `$53`, the five-count hurt timer, 5.0000 velocity, transition record `$91:A8E4`, `$50` damage-boost art, type-`$19` jump physics, collision, and `$FF` sentinel landing. The 32-frame capture freezes the actual damage-boost pose; a longer run proves ordinary `$A5` landing.

`--grapple-fire-script` selects grapple at the explicit untranslated HUD seam, then presses only Shoot/X. Bank `$9B` supplies the current pose's direction, signed 16.16 velocities, hand/flare origins, twelve-pixel length growth, and 128-pixel cutoff. Bank `$94` performs four fractional endpoint probes per frame and dispatches real BG1/BTS collision; persistent type-`$E` BTS zero/three connects, while ordinary solid collision cancels and unsupported PLM-producing reactions throw. Landing Site contains no type-`$E` blocks, so its real-ROM trace honestly proves firing, rendering, and queued cancellation. The eight-frame capture freezes the visible extending beam; fourteen frames prove cancellation completion.

`--grapple-script` still supplies one already-accepted world-space anchor because Landing Site has no grapple block and enemies are not translated. Bank `$9B` supplies the pendulum pump, rope-length rules, quadrant gravity, 16-frame collision-kick gate, release products, art lookup, and one-frame `$51/$52` handoff. Bank `$94` walks rope changes one pixel at a time, sweeps six body points for every crossed whole angle byte, restores the last-safe `$xx80` angle, and negates arithmetic half velocity on terrain collision. It also supplies the sixteen staggered segment instruction phases and packed OAM attributes; bank `$9A` supplies the endpoint/rope tile data selected by the live angle. The two-frame capture freezes Landing Site's immediate reflected beam, while 100 frames proves terrain reflection, release, and jump-pose handoff.

The debug runner exercises real ROM-to-VRAM DMA, NMI/controller latching, timers, OAM/CGRAM, BG1/BG2/BG3, camera/minimap updates, collision, and animated Samus. `--reversal-script` reproducibly exercises both ROM turn poses and `$F8` completions. `--jump-script` performs `$01 -> $4B -> $4D -> $A4`, accelerates into `$09`, performs `$09 -> $19 -> $A6`, then returns through running deceleration. `--posture-script` crouches, stands, turns, then repeats while facing left, verifying movement types `$05/$0F` and the exact five-pixel radius alignment. `--aim-script` selects all six stationary straight/diagonal aim poses in both directions and verifies their no-input fallbacks. `--aim-run-script` drives `$0F/$11/$10/$12` to the native speed cap, crosses running/stationary aim seams, and verifies aimed momentum fallbacks. `--aim-air-script` drives both facings through ROM `$FD` launches, live diagonal aim changes, velocity-preserving fallbacks, shot-direction-selected `$E0-$E5` landings, and `$F8` completion. `--compact-air-script` enters/exits radius-ten `$17/$18`, lands through shot-direction-selected `$A4/$A5`, and checks actual cartridge-backed post-frame poses at deterministic milestones. `--aim-crouch-script` exercises `$F1-$FC` radius changes, their exact `$FD` targets, live crouched shoulder changes, and definition fallbacks in both directions. `--aim-turn-script` verifies all six aimed type-`$0E` turns, native selector replacements, old-direction momentum, and exact `$F8` destinations. `--crouch-turn-script` verifies `$43/$44` and all six aimed crouched turns, including the native grounded branch hidden under movement type `$17`. `--crouch-jump-script` verifies direct `$01/$02` exits plus ordinary and aimed `$4B/$4C` launch geometry through real ceiling/floor collision and landing. Scripts log every 16.16 X/Y displacement, collision result, pose, animation command, and ROM definition address. Output produces the composed frame plus transparent OBJ, BG1, and BG2 diagnostics using the supplied base filename. Extracted raw assets are not a runtime dependency. This remains an incremental runtime shell, not yet a complete playable C# port; the exact admitted movement routes are tracked in `MOVEMENT_COVERAGE.md`.

The first PNG pass is deliberately diagnostic: tile images use grayscale pixel indices, while palette files become accurate color swatches. The static room path composes real foreground/background block layers for asset inspection. The interactive runtime intentionally reuses that ROM-derived composition until the live library-background path reproduces all terrain; **Live PPU layers** makes the incomplete all-live result directly comparable. Camera buttons and initial Samus placement remain explicit host stimuli because the landing-cutscene door has no normal-gameplay spawn. The grounded viewer grants Morph Ball and Bomb item bits; click **Hold Shoot** on for one fresh X edge, then off before another placement. Square/non-square slope collision, grounded ordinary/aimed standing and crouched reversal, ordinary neutral/spin/crouch jumping, walk-off falling, ceiling collision, landing, animated/direct crouch-to-stand collision, stationary/running/crouching aiming, equal-radius aimed jumping/falling/landing, compact straight-down radius/landing collision, aerial turns, block-triggered dry wall jump, ordinary dry-air knockback/damage boost, grapple firing/persistent-block acquisition/all thirty standing-crouching-airborne connection records/terrain reflection/collision kick/exact locked and wallgrab angles/grace-window wall jump/dropped-pose selection/release/beam rendering, ordinary ground/falling/bouncing Morph Ball, Spring Ball ground/powered-jump/falling movement, normal-bomb placement/countdown/rendering/explosion/delete, and the morphed bomb-jump start/rise/handoff are translated. Bomb-triggered dynamic PLMs, live enemy damage producers, solid-enemy/liquid wall-jump branches, grapple spike-damage side effects, breakable grapple PLMs, run-button/speed-booster state, native spawn selection, enemies, effects, and actors remain. Unsupported routes throw rather than becoming guessed physics. Raw files and PNGs contain private ROM-derived material and must not be distributed; this private preservation repository intentionally retains them until a future shareable cleanup.
