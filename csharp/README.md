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
  option-gated moonwalking `$49/$4A/$75-$78`, reversed input physics, six aimed routes,
  and the `$BF-$C4 -> $19/$1A` turning jump bridge;
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
- bank-$90/$93/$94 all twelve valid uncharged/charged beam combinations plus ordinary and Super Missiles: five ordinary slots, all ten muzzle directions,
  shared shot/bomb cooldown, signed 8.8 velocity and acceleration, terrain impact, cartridge
  animation/explosion lists, indexed damage/cooldown/sound data, no-wave collision, Wave Beam
  pass-through movement and three/four-frame trails, exact family-specific OAM flicker, Charge Beam's 60-frame
  hold/release producer and three-component ROM muzzle flare, exact tile/palette upload,
  missile exhaust, the Super Missile's invisible high-speed collision link and quake, and the
  complete type-`$4/$C` shootable-block table with ordinary, power-bomb, and Super-Missile
  permanent/respawning bank-$84 animation programs
- bank-$90/$93/$94/$88 Power Bomb production: HUD/ammo gate, one-at-a-time flag, sixty-frame
  slow/fast fuse, bank-$93 bomb art, 4:3 expanding terrain-border reactions, five exact
  fixed-color/shape phases, ROM curve-based host window composition, afterglow, cleanup, and
  centred Crystal Flash handoff
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
- generic bank-$A0 room-enemy loading: terminated bank-$A1 population records, bank-$B4
  graphics sets, complete 64-byte definitions, both tile-staging modes, palette/tile DMA,
  native spawn snapshots, boss/empty-room bookkeeping, fixed $40-byte slots, and a public
  header reader that does not pretend unsupported actor AI is translated; Landing Site and
  Parlor additionally exercise active/interactive selection, layer queues, bank-local instruction
  lists, common `$80ED` goto/`$812F` sleep opcodes, and `$81:8AB8` enemy OAM emission;
  the three-part gunship runs its real `$A2:A644/$A6D2` initialization, `$A759` main AI,
  ROM-timed four-phase bob, `$AD81/$ADDD/$AFDD` spritemaps, and the complete idle
  entry/open/lower/restore/prompt/open/raise/close/unlock animation chain through `$A2:ABA5`
- host-stimulated Landing Site camera controls over the live PPU-modeled layers
- a dependency-free verification executable with exhaustive/boundary-oriented checks

`SuperMetroid.AssetExtractor` is configured with debug arguments, so it can be selected as the startup project and stepped through immediately. From this directory, the equivalent command is:

```powershell
dotnet run --project src/SuperMetroid.AssetExtractor -- ../standalone-assets/raw ../standalone-assets/png
dotnet run --project src/SuperMetroid.AssetExtractor -- room ../standalone-assets/raw ../standalone-assets/rooms/LandingSite.png
```

To run the translated game from reset, select `SuperMetroid.Game` as the startup project and choose its `Super Metroid C#` launch profile. No arguments or extracted assets are required: startup searches the current/executable directory and their parents for `Super Metroid.smc`. `SUPERMETROID_ROM` or one explicit ROM argument can override that search. From this directory, the equivalent command is:

```powershell
dotnet run --project src/SuperMetroid.Game
```

The game executable runs the shared top-level dispatcher through the title, file select, options, complete opening cinematic, Ceres approach, elevator arrival, and interactive Ceres room. Click the game picture if keyboard focus has moved elsewhere. Controls are arrows = D-pad, `Space` or `X` = A/Jump, `Z` = B/Dash, `S` = X/Shoot, `A` = Y/Item Cancel, `Q` = L/Aim Up, `W` = R/Aim Down, Enter = Start, and Shift = Select. **Pause/Play**, **Step**, **Press Start**, and **Restart** are debugger controls around that same dispatcher; they do not install alternate game state.

The playable host reads `SuperMetroid.ini` from the directory containing the private ROM and
creates a documented default file there if it is missing. Set
`SkipOpeningCinematic=true` under `[Game]` to keep the title, file select, and options screens
but jump from accepted options to the normal Ceres new-game loader and elevator. The default
is `false`, so the narration, flashbacks, and Ceres approach still play. Unknown, duplicate,
or non-boolean options fail with the exact file and line number instead of being silently
ignored; the loaded path and effective value are also printed at startup.

The Ceres handoff now performs the cartridge's `$8B:C100 -> $81:8000` automatic save. The
desktop host writes the complete 8 KiB battery-backed image beside the ROM using the same
basename and an `.srm` extension, reloads it on restart, and displays valid slots through the
native redundant checksum, ENERGY, and TIME layout. The current saved-game loader admits the
translated Ceres area-six/load-station-zero checkpoint; selecting a later emulator save stops
on its unsupported area/station instead of silently loading the wrong room.

`SuperMetroid.RoomViewer` is the separate room/runtime diagnostics executable. It requires `standalone-assets/raw` in addition to the private ROM and can be launched with `dotnet run --project src/SuperMetroid.RoomViewer`.

The **Landing Site** tab renders the complete static room. **Frame runtime** starts in the grounded sandbox on Landing Site's real type-8/BTS-`$00` floor at block row `$4D`, where ROM-derived cave terrain is visibly present. Toggle **Hold Left**, **Hold Right**, **Hold Jump**, **Hold Up**, **Hold Down**, **Hold Aim Up**, **Hold Aim Down**, or **Hold Shoot**, then use **Step frame**, **Step 60**, or **Play**. The **Beam** menu selects any of the cartridge's twelve valid power/ice/wave/spazer/plasma families and immediately performs its real tile/palette transfer. Hold Shoot allocates the real five-slot ordinary-projectile path and indexes that family's bank-$93 animation, damage, sound, cooldown, trail, collision, and flicker rules. Check **Charge Beam equipped** to change that same input to the native first ordinary shot, accumulating muzzle flare, and selected-family charged release at 60 held frames. Jump can be released for the native short arc or held for the full arc; standing selects `$4B/$4C -> $4D/$4E`, running selects spin poses `$19/$1A`, and stable crouch also selects `$4B/$4C` through its own radius/launch seam. Upward collision begins falling, and floor collision selects landing `$A4-$A7`. Down enters `$35/$36 -> $27/$28`; Up expands the collision body through `$3B/$3C -> $01/$02` and is rejected when terrain leaves insufficient standing room. Releasing Down while retaining the facing direction can use the table's direct `$27/$28 -> $01/$02` exit. From standing, Up selects `$03/$04`, Aim Up selects `$05/$06`, and Aim Down selects `$07/$08`. Holding a direction with Aim Up/Down selects aimed-running `$0F/$10` or `$11/$12`; releasing only the direction returns to stationary aim, while releasing every button spends native momentum before the pose-definition fallback to `$01/$02`. Pressing the opposite direction enters `$25/$26` and preserves old-direction momentum. The main-scrolling routine follows both horizontal and vertical movement and streams crossed room edges. The status line exposes X/Y 16.16 position and speed, vertical direction, pose/frame, camera, minimap, ordinary projectile count/last shot/impact, charge counter, and pending ROM transition. **Live PPU layers** exposes the incomplete all-live background path; leaving it unchecked uses precomposed ROM terrain while HUD, Samus tiles, OAM, camera, animation, movement, and collision remain live. Useful breakpoints now include `SamusAerialMovement.StepNormalJump`, `SamusAerialMovement.StepSpinJump`, `SamusPostureMovement.StepCrouching`, `SamusState.TryApplyPostureTransition`, `SamusState.TryApplyCrouchJumpTransition`, `SamusState.TryApplyDirectCrouchToStandingTransition`, `SamusState.ApplyGroundedAimTransition`, `SamusGroundedMovement.StepTurningOnGround`, `SamusBlockCollision.MoveHorizontal`, `SamusBlockCollision.MoveVertical`, `SamusProjectileSystem.StepFrame`, `SamusProjectileSystem.HandleChargeFlareAndDraw`, `SamusProjectileSystem.RunNoWaveBeamPreInstruction`, `SamusProjectileSystem.RunWaveBeamPreInstruction`, `SamusState.HandleAnimationDelay`, `SamusState.Draw`, and `SuperMetroidRuntime.StepFrame`.

The Landing Site gunship is now a live three-slot room actor rather than part of the static
background. Reach its 16-by-64 entrance region in a standing pose and tap Down to run the
native pad-opening and Samus-lowering sequence. Restoration stops at the explicit message-box
$1C seam; **Ship save: Yes/No** becomes enabled in the viewer and continues the same ROM-timed
opening, raising, closing, and input-unlock sequence. Yes records the still-unimplemented SRAM
request, while No is fully self-contained.

Check **Moonwalk enabled**, then hold Shoot and the direction opposite Samus's facing to enter
the real `$49/$4A` route. The aim toggles select `$75-$78`; Jump crosses the exact `$BF-$C4`
turn animation before starting `$19/$1A`. Releasing every button follows the ROM pose-definition
fallback rather than a host-authored shortcut. Useful breakpoints are
`SamusGroundedMovement.StepMoonwalking`, `SamusState.ApplyMoonwalkPoseChange`, and
`SamusState.ApplyMoonwalkTurnJump`.

Running into ordinary solid terrain now executes the block-backed part of `$91:EADE` and
movement type `$15`, including neutral `$89/$8A`, diagonal wall aim `$CF-$D2`, and the
retail one-pixel prospective-run probe. The dedicated DebugRunner route locates a suitable
floor/wall corner from Landing Site's decompressed level data; it does not inject a collision
or pose. Useful breakpoints are `SamusState.CheckProspectiveRunningPoseForWall`,
`SamusState.SelectRanIntoWallPose`, and `SamusGroundedMovement.StepRanIntoWall`. The room
enemy scheduler now publishes its native interactive-index snapshot into this collision
path; Landing Site's gunship intentionally contributes no bodies because its `$0400`
property excludes all three decorative/tangible components.

Dash is a literal `$90:973E` route, not a viewer speed multiplier. The grounded sandbox starts
with **Speed Booster equipped** checked; uncheck it to test ordinary Dash. Hold **Hold Run** with
Left/Right to add hexadecimal 0.1000 per type-one frame up to 2.0000 without Speed Booster
or 7.0000 with item bit `$2000`. Equipped staging reads countdowns from `$91:B61F` and five
alternate animation streams through `$91:B5DE`; stage four publishes echo/contact-damage
state. Native momentum carries the component through `$09/$0A -> $19/$1A`, and equipped
jumps receive the exact extra vertical bonus. The stage-four palette follows `$91:DAA9`'s
double-indirect ROM tables on the native one-then-four-frame cadence, and `$90:EEE7/$87BD`
capture/render the two visible trailing bodies. Any native momentum-cancel path changes their
shared index to `$FFFF`; the renderer then applies the cartridge's signed ±8 X and two-pixel Y
convergence until each stored body crosses Samus. At stage four, release Run and tap **Hold Down**
to store 180 shine frames. After crouch settles, tap **Hold Jump**; windup `$C7/$C8` accepts a
fresh direction for horizontal/vertical/diagonal `$C9-$CE`, accelerates in 16.16 through real
block collision, spawns and animates the dimension-correct bank-$84 bomb-block PLMs, and completes the native crash orbit,
circle, released echoes, palette restoration, and standing return. The status line exposes
the live shine phase/timer, crash subphase/radius, and departing-echo count. Useful breakpoints are
`SamusHorizontalSpeedState.HandleExtraRunSpeed`, `SamusGroundedMovement.StepRunningRight`,
`SamusShinesparkState.Step`, `SamusShinesparkState.StepReleasedCrashEchoProjectiles`, and
`SamusState.AnimateNoFx`.

The grounded viewer sandbox now explicitly grants only the Morph Ball item bit because save-file inventory loading has not been translated. To use the real input route, tap **Hold Down** to crouch, release it, then tap **Hold Down** again to morph. Left/Right rolls and reverses through `$1E/$1F`; Up performs collision-checked unmorph through `$3D/$3E -> $27/$28`. Check **Spring Ball equipped** before morphing to select `$79/$7A`; Jump then launches `$7F/$80` and can be released early for the native short arc. Useful breakpoints are `SamusMorphBallMovement.StepGrounded`, `SamusMorphBallMovement.StepFalling`, `SamusMorphBallMovement.StepSpringBallInAir`, `SamusMorphBallMovement.StepTransition`, and `SamusState.TryApplyMorphTransition`.

To step through the first translated game-support routines, select `SuperMetroid.Verification` as the startup project and place breakpoints in `Bank80SystemState`. From the command line:

```powershell
dotnet run --project src/SuperMetroid.Verification
```

`SuperMetroid.DebugRunner` is the evolving frame-level debugger host. Its Visual Studio launch profile finds the private ROM from the repository root. A particularly useful breakpoint is the deliberately non-inlined `FrameBreakpoint`; from the command line, run:

```powershell
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 240 --timer ceres
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 18 --grounded-run --right-frames 5
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 64 --grounded-run --output ../standalone-assets/runtime/EnemyLandingSiteAnimated.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 650 --gunship-script --output ../standalone-assets/runtime/GunshipExit.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 210 --reversal-script
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 190 --jump-script --output ../standalone-assets/runtime/JumpTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 29 --landing-impact-script --output standalone-assets/runtime/LandingImpactFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 125 --posture-script --output ../standalone-assets/runtime/PostureTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 165 --aim-script --output ../standalone-assets/runtime/AimTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 245 --aim-run-script --output ../standalone-assets/runtime/AimRunTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 390 --aim-air-script --output ../standalone-assets/runtime/AimAirTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 68 --charge-beam-script --output ../standalone-assets/runtime/ChargeBeamRelease.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 67 --charge-beam-script --output ../standalone-assets/runtime/ChargeBeamCharged.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 28 --gun-extended-script --beam-type 11 --output ../standalone-assets/runtime/IceWavePlasmaFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 72 --charge-beam-script --beam-type 11 --output ../standalone-assets/runtime/ChargedIceWavePlasmaFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 12 --hyper-beam-script --output ../standalone-assets/runtime/HyperBeamFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 8 --missile-script --output ../standalone-assets/runtime/MissileFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 8 --super-missile-script --output ../standalone-assets/runtime/SuperMissileFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 11 --visor-script --output ../standalone-assets/runtime/VisorFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 110 --power-bomb-script --output ../standalone-assets/runtime/PowerBombFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 120 --aerial-turn-script --output ../standalone-assets/runtime/AerialTurnFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 24 --compact-air-script --output ../standalone-assets/runtime/CompactAirPoseFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 240 --compact-air-script --output ../standalone-assets/runtime/CompactAirTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 220 --aim-crouch-script --output ../standalone-assets/runtime/AimCrouchTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 180 --aim-turn-script --output ../standalone-assets/runtime/AimTurnTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 240 --crouch-turn-script --output ../standalone-assets/runtime/CrouchTurnTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 260 --crouch-jump-script --output ../standalone-assets/runtime/CrouchJumpTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 140 --moonwalk-script --output ../standalone-assets/runtime/MoonwalkTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 120 --ran-into-wall-script --output ../standalone-assets/runtime/RanIntoWallTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 110 --run-script --output ../standalone-assets/runtime/DashTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 125 --speed-booster-script --output ../standalone-assets/runtime/SpeedBoosterTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 136 --speed-booster-script --output ../standalone-assets/runtime/SpeedBoosterDepartureFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 310 --shinespark-script --output ../standalone-assets/runtime/ShinesparkTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 43 --space-jump-script --output ../standalone-assets/runtime/SpaceJumpFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 42 --water-space-jump-script --output ../standalone-assets/runtime/WaterSpaceJumpFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 50 --screw-attack-script --output ../standalone-assets/runtime/ScrewAttackFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 50 --water-space-jump-script --screw-attack-script --output ../standalone-assets/runtime/WaterScrewAttackFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 70 --morph-ball-script --output ../standalone-assets/runtime/MorphBallPoseFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 220 --morph-ball-script --output ../standalone-assets/runtime/MorphBallTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 80 --spring-ball-script --output ../standalone-assets/runtime/SpringBallPoseFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 160 --spring-ball-script --output ../standalone-assets/runtime/SpringBallTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 50 --bomb-jump-script --output ../standalone-assets/runtime/BombPlacedFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 87 --bomb-jump-script --output ../standalone-assets/runtime/BombExplosionFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 160 --bomb-jump-script --output ../standalone-assets/runtime/BombLifecycleTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 32 --knockback-script --output ../standalone-assets/runtime/KnockbackDamageBoostFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 28 --morph-knockback-script --output ../standalone-assets/runtime/MorphKnockbackActiveFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 54 --morph-knockback-script --output ../standalone-assets/runtime/MorphKnockbackFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 2 --forward-facing-script --output ../standalone-assets/runtime/ForwardFacingFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 12 --elevator-script --output ../standalone-assets/runtime/ElevatorFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 160 --gun-extended-script --output standalone-assets/runtime/GunExtendedFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 8 --grapple-fire-script --output ../standalone-assets/runtime/grapple-firing.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 14 --grapple-fire-script --output ../standalone-assets/runtime/GrappleFireTrace.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 2 --grapple-script --output ../standalone-assets/runtime/GrappleTerrainBounce.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 100 --grapple-script --output ../standalone-assets/runtime/GrappleReleaseFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 60 --grapple-script --output ../standalone-assets/runtime/GrappleSwingHeld.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 100 --grapple-script --output ../standalone-assets/runtime/GrappleSwing.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 30 --crystal-flash-script --output ../standalone-assets/runtime/CrystalFlashBubbleFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 100 --crystal-flash-script --output ../standalone-assets/runtime/CrystalFlashActiveFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 270 --crystal-flash-script --output ../standalone-assets/runtime/CrystalFlashFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 80 --xray-script --output standalone-assets/runtime/XrayFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 76 --death-script --output standalone-assets/runtime/DeathExplosionFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 110 --death-script --output standalone-assets/runtime/DeathFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 110 --drained-samus-script --output ../standalone-assets/runtime/DrainedSamusActiveFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 180 --drained-samus-script --output ../standalone-assets/runtime/DrainedSamusFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 5690 --mother-brain-rainbow-script --output ../standalone-assets/runtime/DrainedRainbowFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 90 --draygon-grab-script --output ../standalone-assets/runtime/DraygonGrabMovingFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 180 --draygon-grab-script --output ../standalone-assets/runtime/DraygonGrabFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 40 --extra-displacement-script --output ../standalone-assets/runtime/ExtraDisplacementAirFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 72 --extra-displacement-script --output ../standalone-assets/runtime/ExtraDisplacementFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 5750 --mother-brain-rainbow-script --output ../standalone-assets/runtime/BabyDeathExplosionsFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 6320 --mother-brain-rainbow-script --output ../standalone-assets/runtime/MotherBrainProjectilesFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 6500 --mother-brain-rainbow-script --output ../standalone-assets/runtime/MotherBrainRevivalFrame.png
```

`--moonwalk-script` enables the otherwise host-disabled native option, holds Shoot plus backward, changes through the right-facing up/down aim variants, proves the no-button definition fallback, re-enters `$4A`, and jumps through `$BF -> $1A`. Its milestone assertions read actual post-frame poses from the private ROM-backed runtime; synthetic checks cover the mirrored routes.

`--run-script` holds canonical Right+Dash/B until `$0B42.$0B44` reaches 2.0000, takes the
cartridge's `$09 -> $19` transition, releases B while airborne, and requires both the numeric
extra component and momentum flag to survive in type three. It then lands through `$A6`,
where the existing standing-family movement performs native ordered cleanup.

`--speed-booster-script` explicitly grants only item bit `$2000` and moves the debugger spawn
left along the same cartridge-authored Landing Site floor to provide enough runway before the
type-`$F` block corridor. It verifies all five ROM-authored animation stages, stage-four
echo/contact state, double-indirect cyan palette cycling, alternating visible echo snapshots,
exact 7.0000 cap, the 7.C400 boosted spin-jump launch, and ROM-timed type-seven foot dust.
The shared `$90:8000-$82DB/$8A4C-$8C1E` translation also owns water entry/exit splashes,
128-frame RNG bubbles, four-way lava spray, exact sound-library requests, and fixed-point
lava/acid damage. `AtmosphericFrame.png` and its layer PNGs capture the `$2A48` dust puff
beneath the live Speed Booster body/echoes.

Landing collision also runs the complete `$91:F046-$F1D2` presentation before animation:
soft/hard impact and spin/Screw termination sounds, exact Crateria room flags and special
Y/FX tests, Brinstar's retail fallthrough, every area selector, submerged suppression, and
slot-two/three splash or dust placement. Ordinary, Morph/Spring, downward-knockback, and
drained-Samus collisions share it without losing pre-clear impact speed. `--landing-impact-script` host-selects only Norfair's
area handler because real Landing Site scrolling sky intentionally deletes landing particles;
its 29-frame private-ROM capture shows both `$2A48` dust objects on live collision terrain.

`--space-jump-script` grants only `$0200`, enters `$1B`, and emits a one-frame Jump pulse only
when the live unaligned 8.8 falling magnitude is in `$0280..$04FF`. `--screw-attack-script`
grants `$0208`, proves Screw art wins the native equipment priority, reverses near a scanned
ROM wall to expose frames 26/27, and requires contact-damage index three, Space Jump restart,
and the double-indirect bank-$91/bank-$9B Screw palette cycle. Their checked-in PNGs include
the composed frame and separate live BG1, BG2, and OBJ diagnostics.

`--water-space-jump-script` publishes one explicitly host-authored water FX surface through
the Landing Site debug spawn because that room's real FX is scrolling sky. Everything after
that producer seam uses the retail ROM: `$90:A08D` horizontal records, water launch/gravity,
animation delay, `$0AD2`, top/bottom boundary checks, transition tables, collision, and `$1B`
art. Its two accepted pulses occur at live `$0134` and `$008C` falling magnitudes, proving the
native underwater minimum is `$0080` rather than the dry `$0280` gate.
Combining it with `--screw-attack-script` reaches the same ROM-authored late Screw frames but
requires `$91:D9B2-$D9D8` to leave all sixteen ordinary suit colors and both palette-cycle
words untouched while Samus's bottom boundary is submerged. The normal dry Screw command
continues to require the six-entry flashing cycle, so the pair catches suppression in either
direction rather than merely accepting a static palette.

The same route cancels boost at frame 135 against real Landing Site terrain. A 136-frame run
captures both returning bodies; a 190-frame run additionally asserts that `$FFFF` departure
state naturally clears after the last signed crossing on frame 141.

`--shinespark-script` extends that same natural runway: it charges stage four, enters
`$09 -> $35 -> $27`, stores shine through `$91:F7B0`, jumps into `$C7`, and supplies a fresh
Right edge to select `$C9`. Assertions require active block-backed motion, type-`$5/$D`
extension redispatch, a type-`$F`/BTS-7 permanent 2x2 bomb-block PLM, collision crash, the exact 40-frame
orbit and 30-frame center circle, `$C9 -> $01`, and both radius-64 departing projectile echoes.

`--morph-ball-script` uses the required two distinct Down presses, reaches `$F9`'s grounded target, rolls and reverses while preserving the shared animation state, decelerates to `$41`, and expands through `$3E/$FD` to crouching. Its frame-count-aware assertions permit both a ball-pose PNG and the complete transition trace without accepting milestones that the requested frame count did not reach.

`--spring-ball-script` equips bits `$0004/$0002`, proves `$F9` selects `$79`, rolls through `$7B`, launches `$7F` with the cartridge's 4.E000 velocity, and follows real ceiling/floor collision through automatic bounce recovery. The 80-frame capture freezes its powered airborne pose; the 160-frame trace proves landing.

`--bomb-jump-script` morphs normally and presses only the default Shoot/X controller bit. The translated `$90:BF9D/$C0E7` producer allocates one of five real bomb slots, `$90:C128` counts 60 down through the fast-animation seam at 15, and `$93:81E9` interprets the private ROM's slow, fast, explosion, goto, and delete records. At timer eight, `$A0:97E2-$A0:984E` publishes straight direction two; the following frame's `$90:DF99` setup installs `$90:E025/$E032`. At timer zero, `$94:9CF4` visits center/up/right/left/down and follows type-`$5/$D` extensions. Type-`$7/$F` BTS 0..7 runs setup `$84:CEDA`; normal bombs skip the redundant sound `$0A`, retain type-$F terrain as type-$8 through movement beta, then start the dimension-specific permanent or 384-frame respawning list in the same frame's PLM pass. Type-`$4/$C` BTS 0..7 runs the equivalent shot-block lists with the native max-one sound rule, while BTS 8..B reveals the required power-bomb/super-missile word. Type-`$B` selects dimensioned crumble or speed-block reveals, including the area-dependent Brinstar table. The same fixed-sprite upload and bank-$93 spritemaps render the bomb/explosion before ordinary Morph Ball descent and floor recovery. The 50-frame capture shows an active bomb, the 87-frame capture freezes the explosion during ascent, and the 160-frame trace proves allocation through deletion and landing; synthetic verification additionally locks each reactive-terrain setup and ROM list.

`--knockback-script` supplies only the left/right result normally produced by an enemy collision, because actors are not translated yet. From that explicit seam, the cartridge supplies `$53`, the five-count hurt timer, 5.0000 velocity, transition record `$91:A8E4`, `$50` damage-boost art, type-`$19` jump physics, collision, and `$FF` sentinel landing. Command one also starts shared hurt counter `$0A48`: `$91:D8A5` gives its first six calls final palette priority, alternates three complete `$9B:A380` hurt palettes with three equipment-selected suit restores, queues impact sound `$35` on call two, performs interrupted spin/grapple/charge-audio recovery when the increment reaches forty, and clears when it reaches sixty. The private-ROM regression compares every visible color write with the cartridge. A 21-frame `KnockbackHurtFlash.png` capture freezes the first white hurt silhouette; the 32-frame `KnockbackFrame.png` freezes the actual damage-boost pose, and a longer run proves ordinary `$A5` landing.

`--morph-knockback-script` reaches `$1D` through the ordinary two-Down `$37/$F9` route,
then supplies that same one-bit enemy-side seam. It deliberately publishes leftward X while
holding forward on right-facing ball art: native `$91:EE27` must ignore both when selecting
up-right direction two, while `$90:8EDF` still moves left. Assertions require the current
pose and rolling animation to survive start, all five hurt frames, same-pose `$91:F31D`
cleanup, elevated `$1D -> $31`, and real floor recovery to `$1D` on frame 54. The 28-frame
capture freezes authentic ball art during knockback; 54 frames capture the landed result.

`--forward-facing-script` calls the translated `$91:E3F6` equipment selector from the
documented host-placement seam. With the debugger's empty inventory it selects power-suit
pose `$00`, reads radius/delay/tile/spritemap data from the private ROM, clears the movement
words owned by that native setup, and renders `$90:868D`'s raw `$3821` left-chest correction
between the top and bottom spritemaps. Pose `$9B` is independently verified to omit that
object. The two-frame capture preserves the SNES main-loop/NMI OAM delay and includes the
composed Landing Site layers plus separate BG1, BG2, and OBJ diagnostics.

`--elevator-script` exercises the active branch of the same forward-facing state. It starts
the enlarged radius-24 body eight pixels above the normal placement, publishes the actor-owned
`$0E18` status seam, and moves down exactly one pixel per frame through `$94:9763`'s terrain-
only collision route. The 12-frame validation proves five accepted steps, real floor clipping
at Y `$04B8.FFFF`, post-move collision-result clearing, and input lock in pose `$00`. The
generated composite and transparent layer diagnostics use the private ROM's live Samus,
terrain, background, HUD, and minimap assets.

`--gun-extended-script` holds Right+Shot through the ROM's `$09 -> $0B` records, preserving
the native running leg phase across the arm change, then enters `$4B/$4D -> $13` and lands
through `$E6` while Shot remains held. At frame 111 it publishes the documented `$91:E8F2`
walk-off producer seam because the fixed Landing Site debug spawn is not a natural ledge,
then lets the normal input table select `$67` and real room collision select `$E6` again.
The same held input now fires live uncharged beams through the five-slot producer,
bank-$93 instruction lists, terrain collision/explosion, and OAM path. Even a three-frame run
requires a real allocation and decoded spritemap; the 160-frame assertion additionally requires
all four Samus firing milestones. `--beam-type 0..11` selects the exact low-nibble family before
the beam tile/palette upload; the regression also requires the fired type to match. A 12-frame PNG freezes a travelling projectile, while the
running, jumping, falling, and landing PNGs retain authentic private-ROM body definitions.

`--charge-beam-script` grants only equipped-beam bit `$1000`, then holds Shoot for 65 live
frames and releases it. `$90:B80D` fires the initial ordinary shot, counts toward the exact
60-frame threshold, and `$90:BAFC` animates the central flare from count 15 plus both sparks
from count 30 through ROM delay lists and `$93:A1A1` spritemaps. Release selects `$90:B986`
and `$93:83D9`; the private ROM regression observes type `$9010`, damage `$003C`, cooldown
30, sound `$17`, the charged no-flicker OAM branch, and `$90:B657-$B80C`'s detached trail.
Supplying `--beam-type 0..11` makes the same route validate the selected charged data and sound
entries; type eleven is observed as `$901B`, damage `$0384`, and sound `$21`. The trail uses the retail 18-slot backward allocation scan, bank-$9B animation-frame offsets,
independent left/right bank-$90 instruction streams, inline position commands, and frozen-time
draw rule. A 50-frame capture freezes the active flare; a 68-frame capture freezes the release;
a 67-frame capture freezes the fully charged body/flare after all six `$91:D7D5` palette
entries have run, a 68-frame capture freezes the release, and a 72-frame capture freezes the
projectile with its orange trail. The live charge branch selects Power/Varia/Gravity through
nested bank-$91 pointers, advances byte offsets 0/2/4/6/8/10, wraps on the sixth call, and
switches to `$91:D7FF`'s independent pseudo-screw lists when contact-damage index four is live.
The one-frame `$0B5E`
pose-transition shot-direction bridge is live: normal-jump Shoot edges publish the new pose's
direction with flag `$8000`, Moonwalk turns publish the source direction with flag `$0100`, and
the following projectile pass forces the cartridge's charge-release decision before consuming
the low-byte direction. Charged shots also drive `$91:D799`'s four-call Samus-body sequence:
three calls replace only colors 1..15 with `$03FF`, preserving suit color zero, and the fourth
restores the full equipment-selected Power/Varia/Gravity palette.

`--hyper-beam-script` invokes the translated `$91:E5F0` endgame grant before initial beam
tiles and palette are uploaded, then supplies one Shoot sample. `$90:BCD1` allocates literal
type `$9018`, initializes art/radii through charged-table index eight, overrides damage to
`$03E8`, queues sound `$1F`, sets cooldown 21, and starts the special `$8014/$8000` glow and
three-component flare. `$90:B159` then shares Wave's signed 8.8 movement but deliberately
does not allocate a detached trail. Controller three also spawns palette-FX object
`$8D:E1F0`: its native timer-one initialization executes `$C655,$01C2`, then cycles ten
ROM-authored eight-color records for exactly two handler calls each before `$C61E` loops to
`$D904`. The private-ROM regression requires every projectile literal, decoded bank-$93 art,
all ten palette records in live CGRAM `$E1-$E8`, and unchanged neighboring colors. Independently,
`$91:D7B6` interprets the `$8014` body-glow timer through `$91:D829`: ten bank-$9B Samus palettes
alternate with ten hold/decrement calls, then call 21 restores the equipment-selected suit palette;
`HyperBeamFrame.png` captures the shot against live Landing Site terrain. The viewer exposes
the same path as **Hyper Beam enabled**;
disabling it is explicitly a debugger-only reverse seam because normal play never revokes it.

`--missile-script` grants ten rounds and selects HUD item one at the untranslated save/pause
seam, then sends one fresh Shoot edge. The private-ROM regression requires type `$8100`, damage
`$0064`, sound `$03`, bank-$93 missile art, a one-round decrement, and `$90:B5A1` exhaust by
frame seven. HUD stability now also drives `$90:C5C4-$C790`'s independent arm-cannon cover:
three opening frames use the pose/animation direction records, preserve the record's
before/after-body OAM order, and upload the selected 32-byte bank-$9A tile to VRAM `$61F0`.
The regression independently re-reads those cartridge pointers and requires frames 1/2/3,
the exact `$281F`-family attribute word, one visible small OBJ, and the matching queued DMA.
`MissileArmCannon.png` plus its transparent OBJ/BG diagnostics capture the fully open cover.
The viewer exposes the same seam as **Missiles selected**; its finite 99-round debugger reserve
still runs the translated producer, cooldown, acceleration, trail, point collision, missile
explosion, deletion, and cover animation on every stepped frame.

`--super-missile-script` grants ten rounds and selects HUD item two at that same explicit
inventory seam. The translated `$90:BE62/$AFE5` route produces type `$8200`, retail damage
`$012C`, sound `$04`, the twenty-frame cooldown, and two-frame exhaust cadence. Ignition also
allocates `$90:BF46`'s invisible linked slot; once the owner exceeds ten pixels per frame the
link samples the otherwise skipped interval and follows the owner's collision/explosion
lifecycle. Type-$1 point reactions use the cartridge's `$94:8B2B/$8E54` non-square and square
slope definitions, while an impact selects `$93:8693`, publishes quake `$14` for thirty
frames, and clears the link. The viewer exposes this as **Supers selected**.

`--power-bomb-script` grants Morph Ball plus ten rounds, selects HUD item three, and enters
the ball through the ordinary two-Down route before sending one fresh Shoot edge. The
translated `$90:BF9D/$C128/$C157` path consumes one round, enforces the one-at-a-time flag,
interprets the real type-`$0300` bank-$93 slow/fast fuse lists, and spawns the two cooperating
bank-$88 HDMA objects after sixty frames. Their five phases use the cartridge's fixed-color
tables, exact 8.8 radii/speed/acceleration, four yellow and seventeen white 192-byte shape
records, 32-step afterglow, and final Crystal Flash attempt/cleanup. The damaging radius scans
the inclusive 4:3 rectangle border in native top/left/bottom/right order and dispatches live
bombable, shootable, and special terrain PLMs. Desktop composition replays the literal
`$88:A266-$A2A5` integer curve bands and center-outward pre-scaled profiles over live BG/OBJ;
`PowerBombFrame.png` captures the expanding yellow phase. The viewer's **Power Bombs selected**
toggle runs this same producer and lifecycle interactively.

`--grapple-fire-script` selects grapple at the explicit untranslated HUD seam, then presses only Shoot/X. Bank `$9B` supplies the current pose's direction, signed 16.16 velocities, separately refreshed hand/flare origins, twelve-pixel length growth, and 128-pixel cutoff. Bank `$94` performs four fractional endpoint probes per frame and dispatches real BG1/BTS collision; persistent type-`$E` BTS zero/three connects, while ordinary solid collision cancels and unsupported PLM-producing reactions throw. The installed `$90:EB86` display handler runs its own bank-$93 flare bytecode before atmosphere and Samus, suppresses ordinary charge flare and speed/shinespark echoes, uploads endpoint/angle tiles after Samus, then draws the staggered rope and endpoint. Its pending-cancel frame takes the native body/cannon/echo fallback without leaking either flare family or rope. Landing Site contains no type-`$E` blocks, so its real-ROM trace honestly proves firing, presentation, and queued cancellation. `GrappleFlareFrame.png` and its transparent layers freeze the visible extending beam; fourteen frames prove cancellation completion.

`--grapple-script` still supplies one already-accepted world-space anchor because Landing Site has no grapple block and enemies are not translated. Bank `$9B` supplies the pendulum pump, rope-length rules, quadrant gravity, 16-frame collision-kick gate, release products, art lookup, independent flare counter/program, and one-frame `$51/$52` handoff. Bank `$94` walks rope changes one pixel at a time, sweeps six body points for every crossed whole angle byte, restores the last-safe `$xx80` angle, and negates arithmetic half velocity on terrain collision. It also supplies the sixteen staggered segment instruction phases and packed OAM attributes; bank `$9A` supplies the endpoint/rope tile data selected by the live angle. The release-queued frame retains `$90:EB86` but follows its signed pointer-range fallback, so ordinary echoes return while grapple flare and rope disappear before the next call restores the default handler. `GrappleSwingDrawFrame.png` and its three diagnostic layers freeze the active reflected beam, while 100 frames proves terrain reflection, release presentation, and jump-pose handoff.

`--crystal-flash-script` invokes the translated centred power-bomb cleanup entry directly with a visible 10/10/10 inventory fixture, skipping only the already-covered fuse/explosion wait, then calls `$90:D5A2` with exact Down+L+R+Shoot. The translated routine itself enforces zero Y speed, energy below 51, empty reserve, and every ammo threshold; it raises `$D3` twenty pixels, drains each ammo family only on NMI counters divisible by eight while applying `$91:DF12` energy overflow, and follows `$91:B545`'s `$FD,$01` finish into ordinary falling. On the tenth rise call it clears the shared bomb flag and spawns `$88:A2BD/$A32A`: the bubble expands from `$04.00` through `$20B0` in eighteen calls, holds the last integer curve window while fixed color fades every four calls, and cleans up independently of the longer Samus animation. Bank `$91:DB93` simultaneously cycles ten body and six bubble colors from separate bank-$9B pointer/timer tables, then restores the selected beam palette. The 30-frame capture shows the live bubble and recolored body; 100 frames freezes the continuing Crystal Flash pose; 270 frames prove all handlers and standing return. The viewer's **Restart Crystal Flash** button runs the same deterministic route.

`--xray-script` grants only scope item bit `$8000` at the explicit already-selected HUD seam,
then lets `$91:E16D` admit the grounded body and choose `$D5`. Holding Dash executes the eight
bank-$88 setup calls and reaches the carry-accurate 10.0000 width clamp on frame 36; Up crosses
the ROM animation thresholds, and Left mirrors the center angle and completes `$D5->$25->$D6`
on frame 56. The compositor builds the two beam edges from the ROM's `$91:C9D4` 8.8 tangent
words, retains `$91:C901`'s zero-width horizontal line, and applies the bank-$88 outside-window
half color math after live Landing Site BG1/BG2, Samus DMA, and OAM composition. The 80-frame
PNG therefore captures the actual final up-left scanner window. Setup stages four through
eight do not yet build the replacement hidden-block BG2 tilemap, so special reveal tiles are
the remaining visual half of X-ray rather than being faked inside the otherwise live polygon.

`--visor-script` publishes only room/HDMA-owned layer-blending configuration `$28` because
Landing Site normally uses configuration two. Inactive charge handling then falls through to
the literal `$91:D83F` routine: packed WRAM bytes `$0A72/$0A73` count down from `$0601`, write
only Samus CGRAM color 196 every five calls, and rotate offsets 6/8/10 from `$9B:A3C0`.
The 11-frame private-ROM route independently checks all three selected words against the
cartridge while ordinary running animation, Samus tile DMA, OAM, camera, and terrain continue.
`VisorFrame.png` captures the third authentic room-cycle color; X-ray handler eight freezes
this packed state and retains ownership of the same color.

`--death-script` enters only after the outer fatal-damage music wait has cleared, then lets
bank `$9B` select `$D7/$D8` and the movement-type-specific start frame. Sixteen preflash calls
advance the ROM's ball-to-human art, 60 flash calls transfer all five `$400` death-tile
segments and alternate the exact suit/suitless palettes, and the following 135 calls render
all nine explosion spritemaps while whitening every non-Samus room palette through the ROM's
22 shades. Frame 76 freezes the first special spritemap over intact terrain; frame 110 shows
suitless Samus and flying suit pieces during the authentic whiteout. Fatal-damage acquisition,
music polling, and the post-explosion fade remain explicit outer game-state seams.

`--drained-samus-script` supplies only the call timing normally owned by the later Baby Metroid actor. It lifts the debugger body two blocks, calls the exact `$91:E4AD` controller entries, and leaves `$E8-$EB` animation bytecode, `$F7`, shared 16.16 gravity, room collision, signed draw offsets, tile DMA, and spritemaps ROM-authored. The 110-frame capture freezes authentic crouched drained art; 180 frames prove floor handoff, standing/crouching commands, `$FD,$01` release, and hyper-beam state. The full Mother Brain/Baby route additionally drives command `$16` and `$91:D954`'s ten complete Hyper Beam body palettes using the Baby's gradually increasing one-through-ten cadence. `DrainedRainbowFrame.png` captures that real-ROM palette animation, command `$17` restores the equipment-selected suit palette in the same frame, and the frame-6168 grant starts the independent `$8D:E1F0` projectile-palette loop described above.

`--draygon-grab-script` supplies one fixed Draygon owner coordinate at the exact `$A5:94A9` actor seam because Landing Site has no Draygon actor. It enters `$EC`, drives the private ROM's `$ED/$EE/$EF/$F0` transition records, proves the six-frame struggle loop and `$F0->$EC` fallback, then reaches `$90:E2A1`'s exact 60-pattern escape threshold and `$90:E2DE` release. The 90-frame capture freezes authentic moving grabbed art; 180 frames prove pose `$01`, native motion-word cleanup, and the owner-release signal. This translates Samus's complete grabbed family without pretending the missing boss flight path is Samus physics.

`--extra-displacement-script` is a narrow stand-in for the untranslated enemy/PLM producer of
WRAM `$0B56-$0B5C`; it does not replace any movement consumer. Frames 0-31 publish +1.0000 X,
frames 32-47 publish -0.8000 Y, frames 48-63 publish +0.8000 Y, and frame 64 clears all four
words. The 40-frame capture shows the resulting authentic `$29` falling body above real
Landing Site terrain. The 72-frame route additionally requires exact +32-pixel X travel,
normal floor collision/`$A4` landing, `$01` recovery, and zeroed producer words.

`--mother-brain-rainbow-script` runs the translated `$A9:B8EB-$C3EE` repeat/active/final rainbow, painful-walk/corpse, revival, Baby-murder/death, phase-three recovery, and phase-three combat scheduler plus Baby `$C710-$CD26`. It advances body AI, body bytecode, brain-slot neck/head bytecode, the later Baby slot, and bank-$86 enemy projectiles in retail scheduler order. The run reads the cartridge's real body programs and sine table, preserves all four 257-call waits, drains resources for exactly 300 calls, and copies all `$800` Baby graphics bytes on frames 1401..1404. With the enemy header's real `$24/$24` radii, the Baby pins to the moving head at 1679, the corpse publishes on 3336, ceiling retreat installs `$CA24` on 3447, the ROM route advances on 3524/3591/3688/3689/3768/3788/3805, touch latches on 3819, and 699 one-point calls finish healing on 4536. Mother Brain revives through grey-table timing, wakes at 4466, walks up, then its real `$9DB1-$9DF5` head program emits four-ring volleys. Bank `$86:C2F3-$C795` supplies highest-free-slot allocation, eight-frame ring head tracking, ROM sine flight, ring/bomb spritemap bytecode, Baby-first ring collision and `$50` damage, room/Samus ring collisions, and the complete phase-three bomb lifecycle. Ordinary volleys reduce 3200 health to zero at 5355; the Baby releases Samus, stares Mother Brain down, retreats, and begins final charge on 5584. The four final rings spawn at 5600/5603/5606/5609, and the first lands on 5618 and deletes its siblings after health reaches zero. Baby AI freezes Samus at 5703 and spawns 30 cadence-accurate `$86:E509` parameter-three explosions. Each follows `$E42C`'s ROM pointer table, six bank-$8D frames for 31 visible calls, call-32 deletion, strict layer-1 origin clipping, and high-priority OAM; the 5750 capture shows six simultaneous stages. Six black palettes follow, the Baby hides at 5853, restores the four bank-$B7 attack-tile rows on 5982..5985, restores seven room-light palettes on 6161..6167, and deletes itself/grants Hyper Beam on 6168. Recovery falls through `$C209` on 6202 and immediately selects four rings; they hit Samus on 6259/6262/6265/6268. The 65-call cooldown ends on 6267, a bomb is selected on 6272, and `$9F00` allocates both its bomb and `$86:CB2F` purple breath on 6304. The stationary breath interprets all eight bank-$8D frames for exactly 76 visible calls and deletes on call 77; the bomb's exact 8.8 friction/gravity path first bounces on 6371. Bombs traverse all nine `$C550` acceleration stages, loop the 34-call ROM animation, preserve their afterburn parameter in X subposition, maintain Mother Brain's active-bomb counter, and distinguish natural afterburn/dust/sound expiry from destruction by a real timer-zero Samus bomb. Rings, bombs, breath, misc dust, door fragments, and the alternate subtitle share the native 18-slot high/low-priority OAM passes; bank-$8D tile addition, palette OR, clipping, nine-bit X/size packing, vertical carry rules, and placement around Samus's draw layer are verified independently. The inch-forward scheduler starts visible body motion on 6404. Synthetic verification additionally locks `$B562-$B5C4` recoil and `$AEE1-$B345` death/escape: retreat/stumble, simultaneous RNG explosions, black/grey fades, decapitated-head 8.8 fall, six corpse DMAs, the shared 48-entry 4bpp corpse-rotting engine, 117 frames of six-record WRAM-to-VRAM updates, exact call-118 completion/dust/music hooks, the following 20-frame delay/brain-coordinate clear, seven NTSC escape-timer DMAs, same-call fallthrough into two exploded-door pages, palette/music/quake/typewriter setup, external typewriter carry, the 33-call exploding-door timer, Samus/timer/boss/event requests, eight fragments, hardcoded door PLM, and the final quake refresh. Bank `$86:C961-$CB11` keeps those fragments in the same pool, applies exact 8.8 friction/gravity for 33 calls, interprets all eight ROM spritemaps, emits terminal dust, and supports the pinned alternate-language subtitle. The typewriter character engine and its individual glyph projectiles remain explicit producer seams.

The debug runner exercises real ROM-to-VRAM DMA, NMI/controller latching, timers, OAM/CGRAM, BG1/BG2/BG3, camera/minimap updates, collision, and animated Samus. `--reversal-script` reproducibly exercises both ROM turn poses and `$F8` completions. `--jump-script` performs `$01 -> $4B -> $4D -> $A4`, accelerates into `$09`, performs `$09 -> $19 -> $A6`, then returns through running deceleration. `--posture-script` crouches, stands, turns, then repeats while facing left, verifying movement types `$05/$0F` and the exact five-pixel radius alignment. `--aim-script` selects all six stationary straight/diagonal aim poses in both directions and verifies their no-input fallbacks. `--aim-run-script` drives `$0F/$11/$10/$12` to the native speed cap, crosses running/stationary aim seams, and verifies aimed momentum fallbacks. `--aim-air-script` drives both facings through ROM `$FD` launches, live diagonal aim changes, velocity-preserving fallbacks, shot-direction-selected `$E0-$E5` landings, and `$F8` completion. `--gun-extended-script` proves `$0B/$13/$67/$E6`, native running-phase retention, and held-Shot landing selection. `--compact-air-script` enters/exits radius-ten `$17/$18`, lands through shot-direction-selected `$A4/$A5`, and checks actual cartridge-backed post-frame poses at deterministic milestones. `--aim-crouch-script` exercises `$F1-$FC` radius changes, their exact `$FD` targets, live crouched shoulder changes, and definition fallbacks in both directions. `--aim-turn-script` verifies all six aimed type-`$0E` turns, native selector replacements, old-direction momentum, and exact `$F8` destinations. `--crouch-turn-script` verifies `$43/$44` and all six aimed crouched turns, including the native grounded branch hidden under movement type `$17`. `--crouch-jump-script` verifies direct `$01/$02` exits plus ordinary and aimed `$4B/$4C` launch geometry through real ceiling/floor collision and landing. `--moonwalk-script` verifies the option-gated right-facing aim family and `$BF -> $1A` turning jump. `--power-bomb-script` verifies the complete producer, fuse, terrain scan, HDMA phases, color window, and cleanup; `--crystal-flash-script` directly exercises the subsequent Crystal Flash handlers and cartridge delay program. `--drained-samus-script` verifies all five drained-controller entries and `$F7` after one documented actor-timing seam. `--draygon-grab-script` verifies the complete `$1A` Samus family after one documented fixed-owner seam. The Mother Brain script additionally logs the live cross-actor phase changes, head spawns, shared projectile slots, bomb bounces/expiry, hit coordinates, health transitions, next-frame cry consumption, escape timer request, door dust, fragment allocation/expiration, and PLM seam. It no longer host-starts the Mother Brain timer during the fight. Output produces the composed frame plus transparent OBJ, BG1, and BG2 diagnostics using the supplied base filename. Extracted raw assets are not a runtime dependency. This remains an incremental runtime shell, not yet a complete playable C# port; the exact admitted movement routes are tracked in `MOVEMENT_COVERAGE.md`.

The first PNG pass is deliberately diagnostic: tile images use grayscale pixel indices, while
palette files become accurate color swatches. The static room path composes real foreground
and background block layers for asset inspection. The interactive runtime can show either
that composition or its incomplete all-live PPU layers for direct comparison. Camera buttons,
initial placement, optional water, and actor-owned triggers remain clearly labelled host
stimuli because Landing Site's cinematic door does not define a gameplay spawn.

Projectile collision now publishes type-`$4/$C` shot-block PLMs directly from ordinary beam,
Wave, missile, and linked Super-Missile scans. All `$94:9EA6` BTS entries preserve their native
weapon gates, `$x052/$x057/$x09F` setup words, bank-$84 sound opcodes, permanent or 384-frame
respawn timing, and no-op allocation behavior; Wave executes the side effect while retaining
its unconditional pass-through result.

The translated runtime now covers the admitted grounded/aerial/posture/aim/turn/landing,
Dash/Speed Booster/shinespark, Space Jump/Screw Attack, Morph/Spring/Bomb jump, knockback,
grapple, all twelve ordinary/charged beam combinations plus Hyper Beam production/motion,
Crystal Flash, X-ray mechanics/window color math, drained/Draygon,
liquid/atmospheric/landing-impact, and documented
Mother Brain/Baby routes described above. Unsupported paths throw instead of becoming guessed
physics. Remaining cross-system work includes earlier Mother Brain attack selection,
Hyper Beam enemy-hit/recoil integration,
X-ray hidden-block BG2 substitution,
projectile-triggered door/bombable/special-block PLMs,
missing actor spritemaps, live enemy damage producers,
native enemy spawn selection, and the unported enemies/effects/
actors. Raw files and PNGs contain private ROM-derived material and must not be distributed;
this private preservation repository intentionally retains them until a future shareable cleanup.
