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
`SamusState.SelectRanIntoWallPose`, and `SamusGroundedMovement.StepRanIntoWall`. Solid-enemy
collision remains outside this slice until actors exist.

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
block collision, breaks bomb-block collision types, and completes the native crash orbit,
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
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 140 --moonwalk-script --output ../standalone-assets/runtime/MoonwalkTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 120 --ran-into-wall-script --output ../standalone-assets/runtime/RanIntoWallTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 110 --run-script --output ../standalone-assets/runtime/DashTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 125 --speed-booster-script --output ../standalone-assets/runtime/SpeedBoosterTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 136 --speed-booster-script --output ../standalone-assets/runtime/SpeedBoosterDepartureFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 310 --shinespark-script --output ../standalone-assets/runtime/ShinesparkTraceFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 43 --space-jump-script --output ../standalone-assets/runtime/SpaceJumpFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 42 --water-space-jump-script --output ../standalone-assets/runtime/WaterSpaceJumpFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 50 --screw-attack-script --output ../standalone-assets/runtime/ScrewAttackFrame.png
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
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 8 --grapple-fire-script --output ../standalone-assets/runtime/grapple-firing.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 14 --grapple-fire-script --output ../standalone-assets/runtime/GrappleFireTrace.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 2 --grapple-script --output ../standalone-assets/runtime/GrappleTerrainBounce.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 100 --grapple-script --output ../standalone-assets/runtime/GrappleReleaseFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 60 --grapple-script --output ../standalone-assets/runtime/GrappleSwingHeld.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 100 --grapple-script --output ../standalone-assets/runtime/GrappleSwing.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 100 --crystal-flash-script --output ../standalone-assets/runtime/CrystalFlashActiveFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 270 --crystal-flash-script --output ../standalone-assets/runtime/CrystalFlashFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 110 --drained-samus-script --output ../standalone-assets/runtime/DrainedSamusActiveFrame.png
dotnet run --no-launch-profile --project src/SuperMetroid.DebugRunner -- "../Super Metroid.smc" --frames 180 --drained-samus-script --output ../standalone-assets/runtime/DrainedSamusFrame.png
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
exact 7.0000 cap, and the 7.C400 boosted spin-jump launch.

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

The same route cancels boost at frame 135 against real Landing Site terrain. A 136-frame run
captures both returning bodies; a 190-frame run additionally asserts that `$FFFF` departure
state naturally clears after the last signed crossing on frame 141.

`--shinespark-script` extends that same natural runway: it charges stage four, enters
`$09 -> $35 -> $27`, stores shine through `$91:F7B0`, jumps into `$C7`, and supplies a fresh
Right edge to select `$C9`. Assertions require active block-backed motion, type-`$5/$D`
extension redispatch, a type-`$F`/BTS-7 bomb-block clear, collision crash, the exact 40-frame
orbit and 30-frame center circle, `$C9 -> $01`, and both radius-64 departing projectile echoes.

`--morph-ball-script` uses the required two distinct Down presses, reaches `$F9`'s grounded target, rolls and reverses while preserving the shared animation state, decelerates to `$41`, and expands through `$3E/$FD` to crouching. Its frame-count-aware assertions permit both a ball-pose PNG and the complete transition trace without accepting milestones that the requested frame count did not reach.

`--spring-ball-script` equips bits `$0004/$0002`, proves `$F9` selects `$79`, rolls through `$7B`, launches `$7F` with the cartridge's 4.E000 velocity, and follows real ceiling/floor collision through automatic bounce recovery. The 80-frame capture freezes its powered airborne pose; the 160-frame trace proves landing.

`--bomb-jump-script` morphs normally and presses only the default Shoot/X controller bit. The translated `$90:BF9D/$C0E7` producer allocates one of five real bomb slots, `$90:C128` counts 60 down through the fast-animation seam at 15, and `$93:81E9` interprets the private ROM's slow, fast, explosion, goto, and delete records. At timer eight, `$A0:97E2-$A0:984E` publishes straight direction two; the following frame's `$90:DF99` setup installs `$90:E025/$E032`. The same fixed-sprite upload and bank-$93 spritemaps render the bomb/explosion before ordinary Morph Ball descent and floor recovery. The 50-frame capture shows an active bomb, the 87-frame capture freezes the explosion during ascent, and the 160-frame trace proves allocation through deletion and landing.

`--knockback-script` supplies only the left/right result normally produced by an enemy collision, because actors are not translated yet. From that explicit seam, the cartridge supplies `$53`, the five-count hurt timer, 5.0000 velocity, transition record `$91:A8E4`, `$50` damage-boost art, type-`$19` jump physics, collision, and `$FF` sentinel landing. The 32-frame capture freezes the actual damage-boost pose; a longer run proves ordinary `$A5` landing.

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

`--grapple-fire-script` selects grapple at the explicit untranslated HUD seam, then presses only Shoot/X. Bank `$9B` supplies the current pose's direction, signed 16.16 velocities, hand/flare origins, twelve-pixel length growth, and 128-pixel cutoff. Bank `$94` performs four fractional endpoint probes per frame and dispatches real BG1/BTS collision; persistent type-`$E` BTS zero/three connects, while ordinary solid collision cancels and unsupported PLM-producing reactions throw. Landing Site contains no type-`$E` blocks, so its real-ROM trace honestly proves firing, rendering, and queued cancellation. The eight-frame capture freezes the visible extending beam; fourteen frames prove cancellation completion.

`--grapple-script` still supplies one already-accepted world-space anchor because Landing Site has no grapple block and enemies are not translated. Bank `$9B` supplies the pendulum pump, rope-length rules, quadrant gravity, 16-frame collision-kick gate, release products, art lookup, and one-frame `$51/$52` handoff. Bank `$94` walks rope changes one pixel at a time, sweeps six body points for every crossed whole angle byte, restores the last-safe `$xx80` angle, and negates arithmetic half velocity on terrain collision. It also supplies the sixteen staggered segment instruction phases and packed OAM attributes; bank `$9A` supplies the endpoint/rope tile data selected by the live angle. The two-frame capture freezes Landing Site's immediate reflected beam, while 100 frames proves terrain reflection, release, and jump-pose handoff.

`--crystal-flash-script` supplies the still-untranslated centred power-bomb cleanup call and a visible 10/10/10 inventory fixture, then calls `$90:D5A2` with exact Down+L+R+Shoot. The translated routine itself enforces zero Y speed, energy below 51, empty reserve, and every ammo threshold; it raises `$D3` twenty pixels, drains each ammo family only on NMI counters divisible by eight while applying `$91:DF12` energy overflow, and follows `$91:B545`'s `$FD,$01` finish into ordinary falling. The 100-frame capture freezes the cartridge's active Crystal Flash body; 270 frames prove all handlers and standing return. Bank-$88 window HDMA and bank-$91 palette cycling remain explicit presentation seams. The viewer's **Restart Crystal Flash** button runs the same deterministic route.

`--drained-samus-script` supplies only the call timing normally owned by the later Baby Metroid actor. It lifts the debugger body two blocks, calls the exact `$91:E4AD` controller entries, and leaves `$E8-$EB` animation bytecode, `$F7`, shared 16.16 gravity, room collision, signed draw offsets, tile DMA, and spritemaps ROM-authored. The 110-frame capture freezes authentic crouched drained art; 180 frames prove floor handoff, standing/crouching commands, `$FD,$01` release, and hyper-beam state.

`--draygon-grab-script` supplies one fixed Draygon owner coordinate at the exact `$A5:94A9` actor seam because Landing Site has no Draygon actor. It enters `$EC`, drives the private ROM's `$ED/$EE/$EF/$F0` transition records, proves the six-frame struggle loop and `$F0->$EC` fallback, then reaches `$90:E2A1`'s exact 60-pattern escape threshold and `$90:E2DE` release. The 90-frame capture freezes authentic moving grabbed art; 180 frames prove pose `$01`, native motion-word cleanup, and the owner-release signal. This translates Samus's complete grabbed family without pretending the missing boss flight path is Samus physics.

`--extra-displacement-script` is a narrow stand-in for the untranslated enemy/PLM producer of
WRAM `$0B56-$0B5C`; it does not replace any movement consumer. Frames 0-31 publish +1.0000 X,
frames 32-47 publish -0.8000 Y, frames 48-63 publish +0.8000 Y, and frame 64 clears all four
words. The 40-frame capture shows the resulting authentic `$29` falling body above real
Landing Site terrain. The 72-frame route additionally requires exact +32-pixel X travel,
normal floor collision/`$A4` landing, `$01` recovery, and zeroed producer words.

`--mother-brain-rainbow-script` runs the translated `$A9:B8EB-$C3EE` repeat/active/final rainbow, painful-walk/corpse, revival, Baby-murder/death, phase-three recovery, and phase-three combat scheduler plus Baby `$C710-$CD26`. It advances body AI, body bytecode, brain-slot neck/head bytecode, the later Baby slot, and bank-$86 enemy projectiles in retail scheduler order. The run reads the cartridge's real body programs and sine table, preserves all four 257-call waits, drains resources for exactly 300 calls, and copies all `$800` Baby graphics bytes on frames 1401..1404. With the enemy header's real `$24/$24` radii, the Baby pins to the moving head at 1679, the corpse publishes on 3336, ceiling retreat installs `$CA24` on 3447, the ROM route advances on 3524/3591/3688/3689/3768/3788/3805, touch latches on 3819, and 699 one-point calls finish healing on 4536. Mother Brain revives through grey-table timing, wakes at 4466, walks up, then its real `$9DB1-$9DF5` head program emits four-ring volleys. Bank `$86:C2F3-$C795` supplies highest-free-slot allocation, eight-frame ring head tracking, ROM sine flight, ring/bomb spritemap bytecode, Baby-first ring collision and `$50` damage, room/Samus ring collisions, and the complete phase-three bomb lifecycle. Ordinary volleys reduce 3200 health to zero at 5355; the Baby releases Samus, stares Mother Brain down, retreats, and begins final charge on 5584. The four final rings spawn at 5600/5603/5606/5609, and the first lands on 5618 and deletes its siblings after health reaches zero. Baby AI freezes Samus at 5703 and spawns 30 cadence-accurate `$86:E509` parameter-three explosions. Each follows `$E42C`'s ROM pointer table, six bank-$8D frames for 31 visible calls, call-32 deletion, strict layer-1 origin clipping, and high-priority OAM; the 5750 capture shows six simultaneous stages. Six black palettes follow, the Baby hides at 5853, restores the four bank-$B7 attack-tile rows on 5982..5985, restores seven room-light palettes on 6161..6167, and deletes itself/grants Hyper Beam on 6168. Recovery falls through `$C209` on 6202 and immediately selects four rings; they hit Samus on 6259/6262/6265/6268. The 65-call cooldown ends on 6267, a bomb is selected on 6272, and `$9F00` allocates both its bomb and `$86:CB2F` purple breath on 6304. The stationary breath interprets all eight bank-$8D frames for exactly 76 visible calls and deletes on call 77; the bomb's exact 8.8 friction/gravity path first bounces on 6371. Bombs traverse all nine `$C550` acceleration stages, loop the 34-call ROM animation, preserve their afterburn parameter in X subposition, maintain Mother Brain's active-bomb counter, and distinguish natural afterburn/dust/sound expiry from destruction by a real timer-zero Samus bomb. Rings, bombs, breath, misc dust, door fragments, and the alternate subtitle share the native 18-slot high/low-priority OAM passes; bank-$8D tile addition, palette OR, clipping, nine-bit X/size packing, vertical carry rules, and placement around Samus's draw layer are verified independently. The inch-forward scheduler starts visible body motion on 6404. Synthetic verification additionally locks `$B562-$B5C4` recoil and `$AEE1-$B345` death/escape: retreat/stumble, simultaneous RNG explosions, black/grey fades, decapitated-head 8.8 fall, six corpse DMAs, the shared 48-entry 4bpp corpse-rotting engine, 117 frames of six-record WRAM-to-VRAM updates, exact call-118 completion/dust/music hooks, the following 20-frame delay/brain-coordinate clear, seven NTSC escape-timer DMAs, same-call fallthrough into two exploded-door pages, palette/music/quake/typewriter setup, external typewriter carry, the 33-call exploding-door timer, Samus/timer/boss/event requests, eight fragments, hardcoded door PLM, and the final quake refresh. Bank `$86:C961-$CB11` keeps those fragments in the same pool, applies exact 8.8 friction/gravity for 33 calls, interprets all eight ROM spritemaps, emits terminal dust, and supports the pinned alternate-language subtitle. The typewriter character engine and its individual glyph projectiles remain explicit producer seams.

The debug runner exercises real ROM-to-VRAM DMA, NMI/controller latching, timers, OAM/CGRAM, BG1/BG2/BG3, camera/minimap updates, collision, and animated Samus. `--reversal-script` reproducibly exercises both ROM turn poses and `$F8` completions. `--jump-script` performs `$01 -> $4B -> $4D -> $A4`, accelerates into `$09`, performs `$09 -> $19 -> $A6`, then returns through running deceleration. `--posture-script` crouches, stands, turns, then repeats while facing left, verifying movement types `$05/$0F` and the exact five-pixel radius alignment. `--aim-script` selects all six stationary straight/diagonal aim poses in both directions and verifies their no-input fallbacks. `--aim-run-script` drives `$0F/$11/$10/$12` to the native speed cap, crosses running/stationary aim seams, and verifies aimed momentum fallbacks. `--aim-air-script` drives both facings through ROM `$FD` launches, live diagonal aim changes, velocity-preserving fallbacks, shot-direction-selected `$E0-$E5` landings, and `$F8` completion. `--compact-air-script` enters/exits radius-ten `$17/$18`, lands through shot-direction-selected `$A4/$A5`, and checks actual cartridge-backed post-frame poses at deterministic milestones. `--aim-crouch-script` exercises `$F1-$FC` radius changes, their exact `$FD` targets, live crouched shoulder changes, and definition fallbacks in both directions. `--aim-turn-script` verifies all six aimed type-`$0E` turns, native selector replacements, old-direction momentum, and exact `$F8` destinations. `--crouch-turn-script` verifies `$43/$44` and all six aimed crouched turns, including the native grounded branch hidden under movement type `$17`. `--crouch-jump-script` verifies direct `$01/$02` exits plus ordinary and aimed `$4B/$4C` launch geometry through real ceiling/floor collision and landing. `--moonwalk-script` verifies option-gated entry, the right-facing stable aim family, no-input fallback, and the `$BF -> $1A` turning jump. `--crystal-flash-script` verifies the three installed handlers and cartridge delay program after one documented power-bomb-producer seam. `--drained-samus-script` verifies all five drained-controller entries and `$F7` after one documented actor-timing seam. `--draygon-grab-script` verifies the complete `$1A` Samus family after one documented fixed-owner seam. The Mother Brain script additionally logs the live cross-actor phase changes, head spawns, shared projectile slots, bomb bounces/expiry, hit coordinates, health transitions, next-frame cry consumption, escape timer request, door dust, fragment allocation/expiration, and PLM seam. It no longer host-starts the Mother Brain timer during the fight. Output produces the composed frame plus transparent OBJ, BG1, and BG2 diagnostics using the supplied base filename. Extracted raw assets are not a runtime dependency. This remains an incremental runtime shell, not yet a complete playable C# port; the exact admitted movement routes are tracked in `MOVEMENT_COVERAGE.md`.

The first PNG pass is deliberately diagnostic: tile images use grayscale pixel indices, while palette files become accurate color swatches. The static room path composes real foreground/background block layers for asset inspection. The interactive runtime intentionally reuses that ROM-derived composition until the live library-background path reproduces all terrain; **Live PPU layers** makes the incomplete all-live result directly comparable. Camera buttons and initial Samus placement remain explicit host stimuli because the landing-cutscene door has no normal-gameplay spawn. The grounded viewer grants Morph Ball, Bomb, and default-on Speed Booster and Space Jump item bits; **Space Jump equipped** and **Screw Attack equipped** independently expose the `$1B/$1C` and `$81/$82` families. **Water at current Y** captures a fixed physics-only surface at Samus's center (without fabricating a water overlay), while **Gravity Suit equipped** toggles the retail `$0020` bypass. **Restart Crystal Flash** grants a documented private 10/10/10 fixture and starts the exact `$D3` handler route. Click **Hold Shoot** on for one fresh X edge, then off before another placement. Square/non-square slope collision, grounded ordinary/aimed standing and crouched reversal, ordinary Dash plus equipped Speed Booster staging/cap/jump bonus/palette/active and returning cancellation echoes, stored shine and all six shinespark poses with crash/released echoes, Crystal Flash initiation/movement/ammo-energy cadence/ROM finish animation, all five drained-controller commands plus `$E8-$EB` movement/animation/draw/release/hyper-beam state, option-gated straight/aimed moonwalking and its turning jump, ordinary neutral/spin/crouch jumping, Space Jump's native repeat window, Screw Attack art/contact damage/palette, walk-off falling, ceiling collision, landing, animated/direct crouch-to-stand collision, stationary/running/crouching aiming, equal-radius aimed jumping/falling/landing, compact straight-down radius/landing collision, aerial turns, block-triggered air/water/lava wall jump, ordinary air/water/lava knockback/damage boost, grapple firing/persistent-block acquisition/all thirty standing-crouching-airborne connection records/air-water pendulum/terrain reflection/collision kick/exact locked and wallgrab angles/grace-window wall jump/dropped-pose selection/persistent environment-selected release/beam rendering, ordinary ground/falling/bouncing Morph Ball, Spring Ball ground/powered-jump/falling movement, normal-bomb placement/countdown/rendering/explosion/delete, the morphed bomb-jump start/rise/handoff, Mother Brain's rainbow/drain/corpse/revival/murder producer, its blue-ring/bomb/purple-breath/misc-explosion projectile animation and rendering, the Baby's complete entrance-through-deletion cutscene, and Mother Brain's phase-three recovery/combat/death movement through escape initialization are translated. Crystal Flash/drain presentation, earlier Mother Brain attack selection, live Hyper Beam shot/damage routing, the Baby actor's own spritemap, other corpse/escape dust producers, typewriter glyph projectiles, bomb-block PLM tile animation, live enemy damage producers, solid-enemy wall-jump branches, grapple spike-damage side effects, breakable grapple PLMs, liquid particles/damage/audio, native spawn selection, enemies, effects, and actors remain. Unsupported routes throw rather than becoming guessed physics. Raw files and PNGs contain private ROM-derived material and must not be distributed; this private preservation repository intentionally retains them until a future shareable cleanup.
