# C# movement coverage

This matrix is the working contract for the C# port. “Translated” means the listed native
route is implemented from the cartridge/disassembly and covered by deterministic or real-ROM
verification. It does not mean every pose sharing that movement-type dispatcher is silently
accepted. Unsupported poses and block reactions still throw at the exact boundary.

The authoritative dispatcher is `SamusMovementHandler_Normal` at `$90:A337`; its 28 entries
are listed below so later work cannot accidentally confuse “the current viewer runs” with
“Samus movement is complete.”

| Type | Native family | Current C# admission | Remaining native branches |
|---:|---|---|---|
| `$00` | Standing | Forward `$00/$9B` equipment selector, zero-status lock, active `$0E18` one-pixel elevator descent through no-solid-enemy `$94:9763`, power-suit chest-cover OAM; ordinary `$01-$08`, landing `$A4-$A7/$E0-$E7`, including held-Shot horizontal firing landings; X-ray `$D5/$D6` admission, angle art, time freeze, beam state, ROM-tangent window/color math, visor palette, and teardown | Live elevator actor/status producer; X-ray revealed-block BG2 tilemap producer |
| `$01` | Running | `$09/$0A/$0B/$0C/$0D-$12`, including horizontal gun extension with preserved native run phase; air/water/lava X tables and submerged Dash gate, ordinary Dash/B 2.0000 cap, equipped Speed Booster stages/7.0000 cap/palette/active and post-cancel echoes; ROM-timed wet/dust footsteps and area-selected landing impact | Live collision producers |
| `$02` | Normal jumping | `$4B-$4E/$13-$18/$51-$52/$55-$5A/$69-$6C`, including horizontal gun extension, compact straight-down collision changes, air/water/lava normal/Hi-Jump launch, X tables, gravity, persistent external X/Y displacement, variable height, ceiling/floor collision; liquid entry/exit splash, bubbles, sound, and damage | Collision-producer side effects |
| `$03` | Spin jumping | `$19/$1A`, Space Jump `$1B/$1C`, Screw Attack `$81/$82`, air/water/lava X/gravity/launch/repeat gates, variable height, split-body animation, charged-spin/Screw contact damage, speed-stage/Screw collision-bomb PLMs, underwater frame-selected Space-Jump sound, damage palette, block/solid-enemy wall contact and launch; liquid entry/exit splash, bubbles, sound, and damage | Live enemy actor producer |
| `$04` | Morph ball on ground | `$1D/$1E/$1F/$41`, air/water/lava X tables, persistent external X/Y displacement, slopes, reversal, deceleration, walk-off, normal/power-bomb deployment, fuse/HDMA window, expanding terrain reactions, normal-bomb bombable/shootable/special-block PLMs, solid/frozen-enemy clipping | Projectile door/bombable/special reactions; live enemy actor producer |
| `$05` | Crouching | `$27/$28/$71-$74/$85/$86`, grounded probe, aim fallback, momentum clear, direct `$01/$02` exits, `$4B/$4C` crouch-jump entry, ordinary/Spring morph entry; X-ray `$D9/$DA` admission, angle art, time freeze, beam state, ROM-tangent window/color math, visor palette, and teardown | X-ray revealed-block BG2 tilemap producer |
| `$06` | Falling | `$29-$2E/$67-$70`, including horizontal gun extension, compact straight-down collision changes, walk-off, air/water/lava X and gravity, persistent external X/Y displacement, held-Shot landing, aerial-turn entry, live `$F0` animation cadence, and liquid entry/exit effects | Collision-producer side effects |
| `$07` | Unused | — | Preserve only if an exhaustive compatibility route needs it |
| `$08` | Morph ball falling | `$31/$32`, air/water/lava X and gravity, persistent external X/Y displacement and bounce override, ceiling/floor/solid-enemy collision, two-stage hard bounce, gentle landing, normal/power-bomb deployment, fuse/HDMA window, expanding terrain reactions, normal-bomb bombable/shootable/special-block PLMs | Projectile door/bombable/special reactions; live enemy actor producer |
| `$09` | Unused | — | Preserve only if required |
| `$0A` | Knockback / crystal-flash ending | `$53/$54` plus pose-preserving Morph/Spring Ball knockback from types `$04/$08/$11-$13`, air/water/lava launch/X/gravity, ordinary-body timer, horizontal/vertical block collision, damage-boost input escape, same-pose ball cleanup, radius-aligned humanoid falling handoff; Crystal Flash ending is handled by the translated type-`$1B` special state; fatal-damage `$D7/$D8` movement-type start-frame selection, locked animation, VRAM/palette flash, whiteout, and suit explosion | Fatal-damage acquisition/music-wait producer and post-explosion game-state fade; live enemy producer |
| `$0B` | Unused | — | Preserve only if required |
| `$0C` | Unused | — | Preserve only if required |
| `$0D` | Unused | — | Preserve only if required |
| `$0E` | Turning on ground | `$25/$26/$43/$44/$8B-$8E/$9C/$9D/$BF-$C4`, old-direction mode-one momentum, native standing/crouch/moonwalk selectors, `$F8` completion; X-ray's dedicated opposite-direction angle mirror and exact frame-two/timer-one `$D5/$D6/$D9/$DA` return | No unadmitted reachable pose branch found; shared presentation/producer gaps remain tracked below |
| `$0F` | Crouch/stand/morph transition | `$35/$36/$3B/$3C/$37/$38/$3D/$3E/$F1-$FC`, bottom alignment, radius collision, `$F9/$FD` completion | No unadmitted reachable pose branch found; shared presentation/producer gaps remain tracked below |
| `$10` | Moonwalking | `$49/$4A/$75-$78`, option gate, air/water/lava X tables, persistent external X/Y displacement, reversed X input, aim changes, fallback, walk-off, and `$BF-$C4` jump bridge | No unadmitted reachable pose branch found; shared presentation/producer gaps remain tracked below |
| `$11` | Spring ball on ground | `$79-$7C`, air/water/lava X tables, persistent external X/Y displacement, slopes, reversal, jump entry, walk-off, normal/power-bomb deployment, fuse/HDMA window, expanding terrain reactions, normal-bomb bombable/shootable/special-block PLMs, solid/frozen-enemy clipping | Projectile door/bombable/special reactions; live enemy actor producer |
| `$12` | Spring ball in air | `$7F/$80`, air/water/lava launch/X/gravity, persistent external X/Y displacement, variable height, ceiling/floor/solid-enemy collision, normal/power-bomb deployment, fuse/HDMA window, expanding terrain reactions, normal-bomb bombable/shootable/special-block PLMs | Projectile door/bombable/special reactions; live enemy actor producer |
| `$13` | Spring ball falling | `$7D/$7E`, air/water/lava X/gravity, persistent external X/Y displacement and bounce override, held-jump relaunch, automatic bounce, solid/frozen-enemy clipping, normal/power-bomb deployment, fuse/HDMA window, expanding terrain reactions, normal-bomb bombable/shootable/special-block PLMs | Projectile door/bombable/special reactions; live enemy actor producer |
| `$14` | Wall jumping | `$83/$84`, air/water/lava normal/Hi-Jump launch tables, variable height, submerged `$FB` selection, spin handoff, terrain/solid-enemy launch and landing, ordinary/grapple launch sounds, charged frames 3-22 contact damage and frame-23+ Screw-style damage | Live enemy actor/shake consumer |
| `$15` | Ran into a wall | `$89/$8A/$CF-$D2`, terrain/solid-enemy prospective-run selector, one-pixel probe, persistent external X/Y displacement, aim/fallback/turn/jump/walk-off routes, grounded cleanup and liquid animation state | Live enemy actor producer |
| `$16` | Grappling | ROM-backed firing, four-step block collision, persistent and breakable type-`$E` acquisition/validation, bank-`$84` break/respawn/BTS/VRAM lifecycle, type-`$A` cancellation/Draygon-turret damage, all 30 standing/crouching/vertical connection records, `$B2/$B3` air/water pendulum, `$A8-$AB/$B4-$B7` locked poses, per-pixel rope collision, six-point terrain sweep/reflection with exact spike-air/spike-block damage tables, collision kick, all eight exact locked/wallgrab angles, `$B8/$B9` terrain/solid-enemy grace-window wall jump, dropped-pose tables, release `$51/$52` plus persistent air/water/lava `$90:946E` motion, ROM art/beam DMA and OAM | Enemy acquisition, live enemy actor/shake consumer |
| `$17` | Turning while jumping | Grounded-Y crouch turns `$97-$9A/$A2/$A3`; airborne `$2F/$30/$8F-$92/$9E/$9F`, persistent external X/Y displacement, momentum, collision, `$F8` | No unadmitted reachable pose branch found; shared presentation/producer gaps remain tracked below |
| `$18` | Turning while falling | `$87/$88/$93-$96/$A0/$A1`, persistent external X/Y displacement, momentum, gravity/collision, `$F8` | No unadmitted reachable pose branch found; shared presentation/producer gaps remain tracked below |
| `$19` | Damage boost | `$4F/$50`, fresh air/water/lava jump, type-indexed X physics, persistent external X/Y displacement, gravity, variable height, ceiling/floor collision, `$FF` sentinel landing | Live enemy producer |
| `$1A` | Grabbed by Draygon | `$BA-$BE/$EC-$F0`: exact owner pin, ten ROM pose/animation routes, input/fallback transitions, type-$1A vertical-result clear, 60-pattern escape hack, `$01/$02` release cleanup and owner signal | Live Draygon actor/flight producer |
| `$1B` | Shinespark / crystal flash / drained / Mother Brain damage | `$C7-$CE`: stored-shine windup, six launch poses, active terrain/solid-enemy motion, crash orbit/circle, released echoes, standing return; `$D3/$D4`: exact initiation checks, 20-pixel raise, NMI-timed 10/10/10 ammo drain, energy/reserve restore, bank-$88 bubble/afterglow, split bank-$91 body/bubble palette cycles, ROM finish animation, standing return; `$E8-$EB`: rainbow commands 5/`$18`/`$19`/`$17`, both Up-edge handlers, all five drained-controller calls, `$F7` fall/collision landing, asymmetric release, draw offsets/bottom halves, ten-palette Baby rainbow cadence/restoration, Hyper Beam grant, `$8D:E1F0` ten-frame projectile palette loop, and live `$9018` projectile/flare/Wave motion; bank `$A9`: repeat/active/final rainbow, painful-walk/corpse, revival, Baby murder/death, phase-three combat/death, and escape `$B8EB-$B3C5`, including body/head bytecode, live neck geometry, Baby graphics DMA/spawn, sine-driven entrance, moving-head latch, drain/corpse handshake, release, ceiling retreat, eight-record flight, generic-touch Samus latch, one-point healing, bank-$86 ring/bomb movement and damage, purple-breath/misc-explosion animation, high/low enemy-projectile OAM, release/stare/retreat/final charge/final blow, rainbow commands, six black palettes, 30 rendered death explosions, attack-tile DMA, room-light restoration, deletion, Hyper Beam, controller four, corpse rotting, escape timer, and exploded door | Earlier attack-selection, Hyper Beam enemy-hit/recoil integration, typewriter character engine/glyphs, Baby actor spritemap and remaining dust producers, live enemy actor producer |

## Active-pose and animation audit

- An exhaustive pass over bank `$91`'s pose definitions now leaves no active pose outside
  an explicitly admitted movement or special game-state family. Fatal-damage `$D7/$D8`,
  X-ray `$D5/$D6/$D9/$DA`, and X-ray's special use of `$25/$26/$43/$44` are translated
  and verified. The earlier blanket
  “later firing variants” labels were stale: ordinary active firing bodies are exactly
  `$0B/$0C`, `$13/$14`, `$67/$68`, `$E6/$E7`, plus translated Draygon `$BC/$EE`.
- The bank-`$90` animation command table is now complete across all sixteen low-nibble
  dispatch slots: six shared CLC/RTS entries `$F0-$F5`, active handlers
  `$F6-$F9/$FB/$FD-$FF`, and the explicitly unused but faithfully translated `$FA/$FC`
  Y-speed/equipment pose selectors. The C# interpreter preserves the live `$F0` cadence in
  aimed-falling `$6D-$70`: the command leaves frame/timer untouched, then zero underflows on
  the following tick and advances to the next literal delay. Synthetic bytecode independently
  proves both branches of each unused selector instead of claiming they are reachable poses.
- This audit is deliberately not a claim that all movement-related systems are complete.
  Fatal-damage acquisition/post-fade ownership, the elevator actor/status producer,
  X-ray hidden-block BG2 substitution, PLM reactions, and live enemy collision/displacement
  producers remain concrete cross-system gaps.

The bomb-jump movement handler is installed outside this normal dispatcher. `$90:E025`
performs its one-frame initialization and `$90:E032` owns the rising special arc; its
translated status is documented with the Morph Ball family below.

## Verified forward-facing standing slice

- `$91:E3F6` selects power-suit pose `$00` when neither suit bit is equipped and shared
  Varia/Gravity pose `$9B` when either `$0001` or `$0020` is present. Both records read their
  radius 24 and `$91:B56F` delay program from ROM, then clear the native base/extra X words,
  Y speed/subspeed/direction, Morph Ball bounce, and X acceleration mode.
- `$90:A383` does not alias these records to ordinary standing. Elevator status zero performs
  no X scan, no grounding probe, and no momentum cleanup; its only write clears the solid-
  vertical-collision result. A nonzero status moves down exactly `1.0000` through `$94:9763`,
  intentionally skipping solid-enemy collision, and then clears the collision result even
  when terrain clips the move. `ElevatorStatus` is the explicit `$0E18` consumer seam; the
  eventual elevator actor remains responsible for publishing and changing that word.
- `$90:868D` draws the normal top spritemap, appends one small raw OBJ at world-relative
  `(-7,-17)` with exact attributes `$3821`, then draws the bottom spritemap. That patch covers
  the left side of the power-suit chest; `$9B` deliberately omits it because suited art is
  complete. Synthetic checks lock OAM order/fields and both ROM base indices.
- Real-ROM `--forward-facing-script` invokes the equipment-selected setup at the documented
  Landing Site debug-placement seam. Its composed and transparent diagnostics contain live
  ROM palette, tile DMA, `$00` top/chest/bottom OAM, sky, terrain, HUD, and minimap.
- Real-ROM `--elevator-script` starts the same power-suit pose eight pixels above the normal
  host placement, publishes status one, and validates five accepted one-pixel steps followed
  by true floor clipping at Y `$04B8.FFFF`. The input dispatcher remains locked in `$00`
  while status is active, matching `$91:804D-$8065`, rather than interpreting held controls.

## Verified X-ray slice

- `SamusXrayState.TryBegin` ports `$91:E16D`'s complete gate, including the peculiar
  cooldown-seven/bomb-count-five/X-divisor-two conjunction, excluded landing ranges, game
  state and power-bomb words, zero 16.16 Y velocity, previous/current movement classification,
  facing direction, and the exact `$D5/$D6/$D9/$DA` selection. Command five then installs
  frame two/timer `$3F`, angle `$40/$C0`, special palette type eight, frozen time, and sound nine.
- The dedicated pose handler mirrors the angle through `$0100-angle`, uses standing
  `$25/$26` or crouching `$43/$44`, and returns only at frame two/timer one. `$90:E94F`
  remains an RTS for type `$0E`; stable bodies force timer fifteen and select all five
  animation frames from the literal right/left angle thresholds.
- The explicit bank-$88 state retains eight setup calls, no-beam/widen/full/restore/finish,
  four-word carry-accurate widening, the 10.0000 clamp, Up-before-Down aiming and width-based
  angle limits, two queue-gated restoration halves, sound ten, pose/radius cleanup, and the
  retail crouched-turn release stand-up glitch. `$91:DCB4` changes only visor color four on
  its native five-frame cadence and restores the complete equipment-selected suit palette.
- The deterministic verifier covers all four stable poses, both turn classes, angle art,
  widening boundaries, palette words, teardown, rejection gates, and the glitch. The
  private-ROM `--xray-script` independently reaches full width on frame 36 and completes
  `$D5->$25->$D6` on frame 56 through live animation DMA/OAM over Landing Site terrain.
- Desktop composition now reconstructs both boundary rays from the cartridge's `$91:C9D4`
  8.8 absolute-tangent table, preserves the zero-width horizontal special case and inclusive
  fractional edge pixels, and applies `$88:817B`'s add-seven-then-half operation outside the
  moving polygon without touching the IRQ-owned HUD. The deterministic fixture locks the
  exact `$03FE` angle-`$36/$4A` boundary, while the checked-in private-ROM frame captures the
  final up-left beam after the live `$D5->$25->$D6` turn.
- Building the replacement BG2 tilemap in setup stages four through eight is still renderer
  work. Until that producer lands, the window moves and shades correctly but cannot substitute
  the special hidden-block tiles which differ from the ordinary BG1 terrain beneath it.

## Verified fatal-damage animation slice

- `SamusDeathSequenceState.Begin` ports `$9B:B3A7`: it reads the pre-death movement type,
  requests spin SFX `$32` only for type three, selects `$D7/$D8` from the live facing byte,
  and uses `$9B:B420`'s frame five for ordinary bodies or frame one for Morph/Spring Ball.
  The host retains world coordinates for shared diagnostics while separately capturing the
  exact layer-1-relative pair consumed by the death renderer.
- State `$16` alone advances `$91:B567`'s six two-tick frames, so ball families visibly
  unmorph before frame five loops. State `$17` freezes that body, queues four `$400` segments
  from `$9B:8400-$9000` to OBJ VRAM `$6200-$6800`, alternates the selected suit and suitless
  palettes on the native 3/1 cadence, and finishes on call 60.
- `$9B:B4B6` loads palette pair zero, queues the fifth `$9B:8000 -> $6000` segment, and draws
  explosion index zero in the same call. State `$18` then consumes literal timers
  21/6/3/4/5/5/6/6/80, palette indices 0/2/3/4/5/6/7/8/9, and right/left spritemaps
  `$81C-$824/$825-$82D`. `$9B:B710` writes all non-Samus/non-suitless palettes through the
  22 ROM shades and the terminal call deliberately draws no tenth frame.
- The deterministic verifier locks the complete 211-call post-music-wait route, both facing
  poses, ball and spin start cases, all transfer addresses, paired palettes, full-white end,
  and terminal no-draw. Private-ROM `--death-script` independently shows the first explosion
  over intact Landing Site terrain at frame 76 and a later suitless/explosion stage during
  native room whiteout at frame 110; frame 211 proves all nine spritemaps and completion.
- Fatal-damage detection, state-`$14` blackout, the state-`$15` music-queue wait, and the
  post-explosion room fade remain outer game-state seams. They do not change `$D7/$D8`
  movement or animation, and the debug entry names the exact cleared-music boundary.

## Verified liquid-physics slice

- `SamusLiquidPhysicsState` retains the actual room-FX words `$195E/$1962/$197E`, FX type,
  and remembered medium `$0AD2`. It does not reduce them to one host boolean: movement uses
  Samus's bottom boundary, full-submersion Spin/`$FB` checks use her top, and `$91:FB08`
  pose initialization samples `Y + radius - 1` exactly as the cartridge does.
- `$90:9BD1` selects complete ROM X-speed tables `$9F55/$A08D/$A1DD` for air, water, and
  lava/acid before adding each movement type's 12-byte offset. This now feeds every translated
  grounded, humanoid-aerial, Morph/Spring Ball, knockback, wall-jump, moonwalk, and damage-
  boost family rather than applying a guessed scalar slowdown.
- `$90:98BC/$9949/$99D6/$9A2C/$9C5B` select adjacent air/water/lava words for ordinary and
  Hi-Jump launches, wall jumps, knockback, bomb jumps, and gravity. Gravity Suit bit `$0020`
  forces air physics, while Speed Booster's independent two-word vertical bonus still runs
  after the selected launch pair.
- Submersion reaches `$90:9808` before the Dash-held branch. It cannot establish or grow
  running momentum; a pre-existing momentum flag freezes the accumulated extra component
  instead of erasing it. Water option bit two suppresses all water movement selection.
- Space Jump first rejects a fully submerged non-Gravity body using the top edge, then uses
  remembered `$0AD2` to choose inclusive minimum `$0080` in liquid or `$0280` in air, with
  shared exclusive maximum `$0500`. A fully submerged Screw body likewise skips contact-
  damage publication. Partial submersion therefore remains a distinct, verified state.
- `$90:8000-$82DB` publishes animation buffer three in water and two in lava/acid, maintains
  `$0AD2`, creates movement-type-selected entry/exit splashes and 128-frame RNG bubbles,
  emits the exact library-one/two/three sound requests, and accumulates ROM-authored lava/acid
  damage with carry from the fractional word. Lava alone executes the native Speed-Booster
  cancellation and Gravity-Suit early return; acid preserves momentum and remains harmful.
- `$90:8A4C-$8C1E` advances the four packed atmospheric slots in reverse order, including
  signed `$8002` delayed starts, ROM timer/count/pointer tables, lava/dust motion, clipping,
  direct small-OBJ attributes, and Samus-table diving/bubble spritemaps. The draw handler runs
  before Samus, retaining native OAM overlap. `$90:A3E5/$ED88-$EEE6` additionally ties wet or
  type-seven dust footsteps to running frames two/seven and the exact audio-suppression gates.
- `$91:F046-$F1D2` now runs at its actual collision-before-animation seam for ordinary,
  Morph Ball, Spring Ball, downward-knockback, and drained-Samus landings. The two special
  handlers publish their impact magnitude before clearing live velocity. The shared path
  distinguishes stationary, soft, and speed-five
  hard impacts; queues ordinary-spin/Screw termination and library-three impact IDs; reads
  Crateria's literal 16-byte room flag table; preserves its Brinstar-to-Tourian fallthrough;
  selects Maridia splashes or Norfair/Wrecked Ship/Tourian dust; suppresses particles below
  active liquid; and deletes only the packed words of slots two/three where retail does.
- Real-ROM `--landing-impact-script` changes only the debugger's room-owned area byte to
  Norfair because authentic Landing Site scrolling-sky metadata intentionally deletes these
  particles. A 29-frame route uses live Landing Site terrain/velocity, observes soft sound
  `$05`, retains both type-six slots through draw, and captures the ROM `$2A48` dust pair in
  `LandingImpactFrame.png` plus its three diagnostic layers.
- `$90:E9CE-$EA44` applies pending damage through the overlapping 8.8 Varia/Gravity shifts,
  subunit-energy borrow, fatal clamp, time-freeze clear, and accumulator reset. The outer
  fatal-damage game-state producer remains separate from this completed consumer.
- `$9B:C4BE-$C4EA` updates grapple's following-frame liquid bit with its narrower native
  general-FX-Y rule. Pump acceleration, angular gravity, kick, and angle advance consume it;
  release then persists through `$90:946E` using standalone air/water/lava records
  `$90:9F31/$9F3D/$9F49` until apex underflow or vertical collision restores normal movement.
- Synthetic verification locks surface equality/wrap semantics, option and Gravity Suit
  bypasses, every table offset, Dash retention, animation boundaries, and partial/full
  Space/Screw branches. Real-ROM `--water-space-jump-script` supplies only a documented
  Landing Site FX-surface stimulus, reads the retail water tables, and accepts two repeats at
  live falling magnitudes `$0134` and `$008C`; the 42-frame PNG captures authentic `$1B` art.
- `$91:D9B2-$D9D8` independently samples the full bottom boundary for Samus palette effects.
  Submerged Power/Varia returns carry-set before advancing Screw Attack or Speed Booster
  palette timer/index words; Gravity Suit bypasses the raw water/lava result. Synthetic tests
  cover water equality, disabled-water option bit two, lava fallback, both suppressed effects,
  and both Gravity bypasses. The combined private-ROM `--water-space-jump-script
  --screw-attack-script` route reaches frame 27, requires frozen zero timer/index words, and
  compares all sixteen live CGRAM colors with the ordinary bank-$9B Power Suit palette.

## Verified ordinary-air slice

- Standing jump uses the ROM transition `$01/$02 -> $4B/$4C`, calls the dry-air branch of
  `Make_Samus_Jump`, deliberately performs no vertical movement in the transition pose, and
  consumes animation command `$FD` to enter `$4D/$4E`.
- Running jump uses `$09/$0A -> $19/$1A`, selects the movement-type-three speed-table entry,
  applies the native carry/mode/forward-input rules, and uses the spin animation's conditional
  bottom-half drawing rule.
- Vertical motion copies the old 16.16 speed before applying gravity, preserves the separate
  up/down word, implements early jump release and signed speed underflow, and honors the
  native whole-word `Y speed == 5` terminal check.
- Upward collision zeroes speed and begins falling without fabricating a new pose. Downward
  collision selects `$A4/$A5` or `$A6/$A7`, clears native speed words, and aligns the larger
  landing radius while keeping Samus's feet on the collision boundary.
- A failed grounded probe selects `$29/$2A`; falling begins with the native stationary frame
  and then uses ROM gravity. Landing `$F8` returns to `$01/$02`, while held direction can take
  the shared landing table directly to `$09/$0A`.
- The RoomViewer exposes **Hold Jump** alongside Left/Right and displays Y position, 16.16 Y
  speed, and direction. DebugRunner `--jump-script` exercises a short neutral jump, run,
  full spin jump, ceiling/floor collision, both landing styles, camera tracking, and rendering
  against the private retail ROM.

## Verified ordinary-Dash slice

- `$90:973E` receives canonical Dash/B `$8000` from the same latched controller word used
  by pose matching. Movement type one establishes momentum word `$0B3C`, clears booster
  counter `$0B3E`, and adds exactly 0.1000 to `$0B42.$0B44` per frame without host time or
  floating-point conversion.
- The no-Speed-Booster path preserves the cartridge's signed two-word cap comparisons and
  clamps at 2.0000. Base speed still comes independently from type one's ROM record at
  `$90:9F61`; `$90:E4E6` adds the two components before collision.
- Releasing B or entering normal/spin jump/falling takes `$90:9808`. Set momentum retains
  the extra component, so `$09 -> $19` carries it into aerial calculation. Standing,
  landing, turns, walls, and block collision retain their exact cancellation/clear order.
- `$90:852C-$8568` selects the shared running-delay list through live ROM pointer `$91:B5D1`.
  Its command interception resets frame zero with that ROM-authored delay while B is held;
  the viewer's **Hold Run** control therefore affects both motion and animation.
- Synthetic verification locks 32 exact additions, cap/retention/cancel behavior, shared
  animation selection, and command reset. Real-ROM `--run-script` observes
  `$01 -> $09 -> $19 -> $A6 -> $09 -> $01`, reaches 2.0000, and carries it through type three.

## Verified equipped-Speed-Booster slice

- `$90:973E` initializes `$0B3E` through live ROM table `$91:B61F`, retains the palette
  frame/timer words, adds hexadecimal 0.1000 per running+Dash frame, and uses the native
  signed-word comparisons to clamp at exactly 7.0000.
- `$90:852C` decrements only at running animation command boundaries. Its low-byte-zero
  test advances stages zero through four, reloads the countdown through `$91:B61F`, and
  selects each alternate delay list through `$91:B5DE`; stage four publishes the echo-sound
  flag and contact-damage index instead of approximating either from host elapsed time.
- Equipped jumps and wall jumps add the extra-run fractional word directly to Y subspeed
  and half the whole word to Y speed. The two 16-bit additions remain independent, including
  the cartridge's deliberately observable lack of fractional carry.
- `$91:D9B2` now follows the live suit table at `$91:DAA9`, then its bank-`$91` frame list,
  then each bank-`$9B` 32-byte palette. Stage four loads immediately from timer one and every
  four frames afterward. Its preceding raw bottom-boundary water/lava gate freezes the palette,
  timer, and list offset for submerged Power/Varia while Gravity Suit bypasses suppression;
  `$91:DE53` cancellation restores the normal suit through `$91:D727`.
- `$90:EEE7` captures alternating post-movement world positions on game-time multiples of
  four. `$90:87BD` draws slot one then slot zero with the current ROM spritemap indices, so
  the Room Viewer and composed DebugRunner PNG show the actual cyan trailing bodies.
- `$91:DE53` now preserves those two snapshots when any native movement cleanup cancels the
  boost, writes shared index `$FFFF`, and assigns signed X velocity -8 left/+8 right. The
  negative-index branch of `$90:87BD` advances slot one then zero during drawing, approaches
  live Samus Y by two pixels, and clears each X word on the exact signed crossing frame.
- Synthetic verification uses distinct countdowns/delay lists for all five stages and locks
  cap, double-indirect palette selection/timing/restoration, active cadence, both signed
  departure directions, repeated-cancel guards, crossing deletion, stage-four events, and
  jump arithmetic. Real-ROM `--speed-booster-script` reaches stage four, visibly renders
  both active echoes in the cycled palette, reaches 7.0000, launches `$09 -> $19` at 7.C400,
  then observes two cancellation bodies from frames 135 through 140 before index cleanup.

## Verified stored-shine and shinespark slice

- `$91:F7B0` stores shine only when the signed high-byte comparison sees Speed Booster stage
  four or later. It publishes timer 180 and palette handler one; timer 170 requests the native
  warning sound, while expiration restores the live suit palette through `$91:D727`.
- A crouched Jump follows the ordinary `$4B/$4C -> $4D/$4E` path before stored shine installs
  windup `$C7/$C8`. Its 30-frame handler accepts ROM input-table direction records or times
  out upward, selecting horizontal, vertical, or diagonal `$C9-$CE` without a host direction.
- `$90:D106/$D0AB/$D0D7` use retained 16.16 acceleration and the bank-`$94` block mover.
  Horizontal speed clamps at 15.0000; vertical speed clamps at 14.x. Active frames publish
  contact-damage two, hurt flash eight, 15 palette ticks, sampled echoes, and one-energy drain
  while health is at least 30.
- Collision type `$5/$D` redispatches signed horizontal/vertical extension BTS exactly.
  Shinespark-capable type `$F` BTS 0..7 allocates the matching bank-`$84` PLM and continues
  through it. The shared ROM interpreter queues sound `$06` with maximum three, follows
  `GotoY`, draws complete 1x1/2x1/1x2/2x2 level words, and preserves the BTS-0..3 384-frame
  blank hold plus linked collision restoration; BTS 4..7 remain permanently blank.
- Collision or health below 30 zeroes motion and starts `$90:D346`: four expansion frames,
  32 angle-separation frames, four contraction frames, then a 30-frame center hold. All orbit
  points use the positive half of the ROM sine table at `$A0:B443` (the complete table
  starts at `$A0:B3C3`); odd NMIs draw slot one before slot zero.
- `$90:D40D` samples the literal pose-indexed angle pair before restoring `$01/$02`. Capacity
  admits fixed projectile slots three/four at radius 64; `$90:D4D2` grows each by eight around
  the current Samus position and deletes it outside the camera's 256x256 viewport. Drawing
  retains native slot-four-before-slot-three OAM order and odd-NMI cadence.
- Synthetic checks lock storage, palettes, windup timeout, six-direction dispatch, motion,
  energy edge cases, extension/bomb-block spawn, multi-block ROM draws and respawn, 40+30 crash timing, sine positions, standing
  return, released-echo angles/radii, and viewport deletion. Real-ROM `--shinespark-script`
  proves `$09 -> $35 -> $27 -> $4B -> $C7 -> $C9 -> $01` through Landing Site terrain.

## Verified crouch/stand slice

- A new Down edge from ordinary standing, running, or landing enters `$35/$36`. The native
  command-seven pose change shrinks the vertical radius from 21 to 16 pixels and moves Samus's
  center down five pixels, preserving the bottom of the collision body exactly.
- `$35/$36` run the movement-type-`$0F` zero-base horizontal and grounded vertical probes;
  animation command `$FD` enters `$27/$28`. Crouching then runs movement type `$05`, including
  its native clearing of accumulated horizontal momentum.
- A new Up edge enters `$3B/$3C`. Radius expansion probes the five newly occupied pixels above
  and below, moves the center away from a single obstruction, and refuses the transition when
  a low tunnel fits crouching Samus but not standing Samus. `$FD` then returns to `$01/$02`.
- The renderer applies `$90:8D3C`'s transition-frame Y offsets instead of drawing the changing
  collision center directly. RoomViewer exposes mutually exclusive **Hold Up**/**Hold Down**
  controls, and DebugRunner `--posture-script` verifies both facing directions against the ROM.
- Down plus the canonical shoulder chords selects `$F1-$F6`; their command `$FD` targets
  `$85/$86/$71-$74`. All six stable aimed crouches share radius 16, movement type five,
  grounding and momentum cleanup with `$27/$28`, while definition byte two returns them to
  ordinary crouch when every controller bit is released.
- Up plus the same shoulder chords selects `$F7-$FC`. Their radius-21 expansion uses the
  existing five-pixel ceiling/floor collision probes and `$FD` targets `$03-$08`. The
  synthetic matrix covers all twelve transition records and the real-ROM
  `--aim-crouch-script` crosses both facings, live aim changes, and both fallback paths.
- Releasing Down while retaining the facing direction matches the literal direct records
  `$91:A6B4/$91:A704`, installing `$01/$02` without passing through `$3B/$3C`. The same
  larger-radius collision resolver aligns the 21-pixel body to the floor; a following held
  direction uses the already translated standing table to enter `$09/$0A` on the next frame.
- A new Jump edge from any stable crouch selects `$4B/$4C`. Pose-change collision expands
  radius 16 -> 19 before `Make_Samus_Jump`; `$91:FC7D` then subtracts ten additional Y pixels
  only when the literal source was ordinary `$27/$28`. Aimed crouches use the same jump art
  and ROM 4.E000 velocity without that extra subtraction. If both ceiling and floor block
  expansion, `$91:FFA7` selects ordinary `$27/$28` and the jump is never started.

## Verified moonwalk slice

- The host option is an explicit model of the native enable word read at `$91:F88C`. When it
  is off, a standing backward-plus-Shoot record is replaced by ordinary `$25/$26`; when it is
  on, the cartridge-selected `$49/$4A/$75-$78` pose is retained.
- Movement type `$10` executes the real X-speed calculation with Samus facing opposite the
  held direction, then runs the ordinary horizontal block scan and no-speed grounded probe.
  The six stable poses therefore share normal acceleration and collision while preserving
  their literal straight, diagonal-up, and diagonal-down shot-direction metadata.
- The no-button route does not invent a transition-table record. It publishes command two and
  consumes pose-definition byte two, so each aimed moonwalk returns through its exact visual
  family before the existing standing fallback resumes.
- Jump from a stable moonwalk maps shot directions `1/2/3/6/7/8` to `$C1/$BF/$C3/$C4/$C0/$C2`.
  Those six turn poses fold extra run speed into base 16.16 speed, use the native type-`$0E`
  old-direction deceleration, and finish their three-visible-frame `$F8` lists in `$1A/$19` with a
  fresh environment-selected spin-jump launch.
- The viewer exposes **Moonwalk enabled** and uses the real Shoot-plus-backward controller
  chord. Synthetic verification exhausts all six stable and all six jump-turn routes; the
  private-ROM `--moonwalk-script` proves `$01->$4A->$76->$78->$07->$01->$4A->$BF->$1A`.

## Verified solid-enemy movement collision slice

- `SamusSolidEnemyCollision` is a literal port of `$A0:A8F0-$AB80`. It consumes interactive
  enemies in native list order, admits a slot only when frozen or property bit `$8000` is
  set, and returns the first qualifying collision rather than sorting by geometric distance.
- `$A0:A919-$A9B5`'s target construction remains 16-bit whole/fraction arithmetic. Fractional
  borrow/carry adjusts the whole coordinate and every nonzero resulting fraction rounds one
  additional pixel outward. Broad X/Y overlap is strict, using the same subtract-radius then
  CMP/BCC sequence; exact tangency is therefore rejected.
- The directional pass compares current leading boundaries after the future broad phase.
  A positive gap becomes the returned whole movement distance with fractional distance zero;
  zero takes the touching path; a negative/BPL result means Samus is already embedded and
  skips that enemy. Touching deliberately preserves the retail `STZ $0AFC` bug, clearing
  Samus's Y subposition even for horizontal contact.
- Every shared horizontal/vertical movement path now probes the current ordered enemy snapshot
  before bank-$94 terrain. Enemy success clips movement to its boundary and bypasses blocks;
  a miss retains the previous room-block behavior. `BlockMoveResult` distinguishes the enemy
  slot from a terrain block so callers do not infer actor identity from scenery.
- Ordinary and grapple wall-jump probes use the same source and publish native
  `EnemyIndexToShake` only when a fresh Jump edge accepts an enemy-backed launch. Ordinary
  terrain and enemy-backed launches both publish solid-vertical collision result five at
  their distinct `$90:9E5E/$90:9E7F` exits; contact or a held-only Jump chord does neither.
  Prospective wall and larger-pose copies share the immutable snapshot but commit no motion;
  only the touching routine's real Y-subposition side effect is copied back.
- Synthetic verification covers empty/non-solid/frozen/property-solid lists, all four target
  directions, fractional underflow/overflow, strict tangency, perpendicular separation,
  embedded rejection, first-list priority, exact gap clipping, the horizontal-touch bug,
  enemy-before-terrain integration, observational wall probes, and enemy wall-jump identity.
  A live room enemy loader/AI loop is still required to populate and consume these snapshots.

## Verified ran-into-wall slice

- `$91:EADE` first consumes the killed-X-speed result from current movement type one. If
  that branch is absent, only a prospective type-one pose triggers the forward collision probe.
  The shared native-order pass tests solid/frozen enemies before room blocks.
- The probe moves exactly one pixel in the CURRENT pose direction. Collision retains
  the last-safe X and selects wall art from the PROSPECTIVE pose's shot direction; clear
  terrain retains the pixel movement, preserving the retail arm-pump bug.
- The literal `$91:EB74` selector maps all ten shot directions to `$03/$CF/$89/$D1/$89/`
  `$8A/$D2/$8A/$D0/$04`. The six type-`$15` records share the cartridge's `$10,$FF`
  animation list, no-base X movement, grounding probe, and unconditional horizontal-speed
  cleanup from `$90:A75F`.
- Input remains table-driven: aim changes pass through prospective running `$0F/$11`,
  no-input definition fallback returns `$CF/$D1` to `$89`, and a fresh Jump edge follows
  `$89 -> $4B -> $4D`. Ground turns, same-facing running exits, moonwalk entry, posture,
  and walk-off all reuse their already translated retail routes.
- DebugRunner `--ran-into-wall-script` scans Landing Site's decompressed BG1/BTS planes for
  an ordinary type-`$8` floor/wall corner with type-`$0` body clearance. Its real-ROM route
  observes `$01 -> $89 -> $CF -> $D1 -> $89 -> $4B -> $4D` and requires at least one
  blocked one-pixel bank-`$94` probe, so a host-selected pose cannot satisfy the regression.

## Verified grounded-aim slice

- The shared right table `$91:A0EC` admits `$01/$03/$05/$07`; its left mirror `$91:A172`
  admits `$02/$04/$06/$08`. Straight Up and the canonical `$0010/$0020` shoulder bits select
  the exact ROM poses, in table priority order, without host-authored aim state.
- All six aim poses execute movement type zero's real horizontal/collision/grounding/cleanup
  path. Their pose-definition direction, shot-direction, graphics offset, radius, animation
  delay list, bank-$92 tile definitions, and spritemaps continue to come from the cartridge.
- When all controller bits are released, the transition matcher intentionally does no lookup;
  `$91:82D9` instead reads pose-definition byte two, yielding `$01` for right aim and `$02`
  for left aim. The runtime models that distinct fallback seam explicitly.
- Aimed walk-off reads pose-definition shot direction and selects `$2B/$2C/$6D-$70`; a later
  no-input fallback still comes from definition byte two and selects unaimed `$29/$2A`.
- Holding a direction with canonical aim-up/down selects `$0F/$10` or `$11/$12` directly
  from `$91:A1F8/$91:A242`. Those poses execute the same exact movement-type-one acceleration,
  collision, slope, animation, camera, and momentum paths as `$09/$0A`; `$0D/$0E`'s unused
  straight-up records are also dispatcher-correct but have no ordinary retail input route.
- Releasing only the direction changes aimed running to the matching stationary aim pose
  after that frame's movement; releasing every button preserves the aimed run pose while
  native mode-two momentum decelerates, then pose-definition byte two returns to `$01/$02`.

## Verified aimed grounded-turn slice

- The input transition tables continue to publish generic `$25/$26`. The initializer at
  `$91:F8D3` reads the previous pose's shot-direction byte and indexes the literal ten-entry
  `$91:F9C2` table, selecting straight-up `$8B/$8C`, diagonal-down `$8D/$8E`, diagonal-up
  `$9C/$9D`, or ordinary `$25/$26` without host-authored aim state.
- All eight admitted movement-type-`$0E` poses fold the extra run component into base 16.16
  speed, clear the consumed words, select acceleration mode one, decelerate through the ROM
  `$90:9FFD` record, and carry momentum opposite the new facing direction.
- The six aimed animation lists each contain three two-tick NTSC frames followed by command
  `$F8`. Their exact destinations are `$8B->$04`, `$8C->$03`, `$8D->$08`, `$8E->$07`,
  `$9C->$06`, and `$9D->$05`.
- Crouched aimed turns use movement type `$17`, not this handler, and are tracked separately
  below. Downward-collision table `$90:E65A` assigns both turn types result `$04` (“no pose
  change”), so a ledge turn finishes `$F8`; its destination pose detects the fall afterward.

## Verified crouched-turn slice

- Crouching input tables publish `$43/$44` directly on a new opposite-direction edge. The
  same `$91:F8D3` initializer detects previous movement type five and indexes `$91:F9CC`,
  selecting unaimed `$43/$44`, straight-up `$97/$98`, diagonal-down `$99/$9A`, or
  diagonal-up `$A2/$A3` from the previous pose's literal shot direction.
- `$43/$44` are native movement type `$0E`. The six aimed turns are native type `$17`, but
  crouched entry leaves Y direction at zero, so `$90:A790` executes `Samus_X_Movement` and
  the no-speed grounding probe rather than its airborne speed/gravity branch. The port
  admits exactly that grounded-Y behavior and rejects a type-`$17` crouch turn that somehow
  becomes airborne.
- Every crouched turn preserves radius 16, folds and decelerates old-direction momentum,
  uses three two-tick frames, and completes through exact `$F8` destinations: `$43->$28`,
  `$44->$27`, `$97->$86`, `$98->$85`, `$99->$74`, `$9A->$73`, `$A2->$72`, `$A3->$71`.
- Type `$17` uses the native usual spritemap-position selector and unconditional bottom-half
  draw. The active `$97` diagnostic confirms that renderer against private-ROM terrain/HUD.

## Verified aimed-air slice

- Standing and running aim enter the ROM's `$55-$5A` transition records. Animation command
  `$FD` selects `$15/$16/$69-$6C`; the transition frame performs horizontal movement but no
  vertical pass, exactly like the ordinary jump transition family.
- Shoulder changes while airborne select equal-radius active jump or fall poses without
  resetting live 16.16 vertical velocity. Releasing aim uses definition byte two, preserving
  the native distinction between jump fallbacks `$51/$52` and falling fallbacks `$29/$2A`.
- Grounded probe failure maps shot directions `0/1/2/3/6/7/8/9` to the exact falling family,
  and normal-jump gravity, early release, ceiling collision, camera, and terrain collision
  continue through the already verified movement-type-two/six paths.
- Landing reproduces `$91:E95D/$91:E9F3`: shot direction chooses `$E0-$E5`, radius expansion
  keeps the feet aligned, and animation command `$F8` returns to `$03-$08`. Spin landings
  remain `$A6/$A7`; straight unaimed landings remain `$A4/$A5`.
- Holding Down in the normal-jump or falling families selects compact straight-down
  `$17/$18/$2D/$2E`. Their pose records shrink the collision radius from 19 to 10 without
  moving the center or disturbing live 16.16 velocity; definition byte two is `$FF`, so a
  no-input frame retains the compact pose instead of inventing an ordinary fallback.
- Leaving a compact pose runs `$91:FDAE`'s larger-pose collision path over the nine newly
  occupied pixels above and below. One obstruction calculates a center displacement and
  triggers the native opposite-side safety probe; failure there retains the compact source.
  Two simultaneous initial hits invoke `$91:FFA7`, select ordinary crouch `$27/$28`, and
  apply its otherwise easy-to-miss radius-10-to-16 six-pixel center correction.
- Compact landing uses shot directions four/five from `$91:E9F3`, selecting `$A4/$A5`.
  The 10 -> 21 expansion probes real room blocks, keeps the floor boundary aligned, and then
  collision command five clears both velocity axes. Both jump and falling compact records
  continue through their existing movement-type-two/six gravity, camera, and draw paths.

## Verified horizontal-firing movement slice

- The ordinary running, normal-jump, and falling input tables select the cartridge-authored
  horizontal gun-extended bodies `$0B/$0C`, `$13/$14`, and `$67/$68` while Shot is held.
  These are not substitute sprites: pose direction, radii, delay lists, top/bottom tile
  definitions, DMA, and OAM continue to come from the private ROM.
- `$91:F50C` uses animation delay `$8000` as a sentinel when a running arm change must retain
  the current leg phase. The translated same-movement transition therefore preserves both
  animation frame and timer instead of restarting the ten-frame run cycle at frame zero.
- Aerial shoulder changes preserve the live signed 16.16 velocity and collision body. On
  floor contact, `$91:E99B` checks horizontal shot directions two/seven and the currently
  held Shot bit: held Shot selects `$E6/$E7`; released Shot selects ordinary `$A4/$A5`.
- `$E6/$E7` share movement type zero's zero-base-speed horizontal pass, grounding probe, and
  momentum cleanup, then use the ROM delay program's `$F8` command to return to `$01/$02`.
- Synthetic verification covers both facings, ROM input selection, six extended bodies,
  running-phase preservation, aerial velocity preservation, held/released landing choice,
  and `$F8`. The 160-frame private-ROM route independently observes `$0B -> $13 -> $E6`,
  publishes the documented walk-off producer seam, and then observes `$67 -> $E6` through
  real Landing Site block collision. The checked-in pose captures preserve running, jumping,
  falling, and landing art against the live terrain/HUD layers.

## Verified Space Jump and Screw Attack slice

- `$91:F624` selects `$81/$82` when Screw bit `$0008` is equipped, otherwise `$1B/$1C`
  when Space Jump bit `$0200` is equipped. Initial grounded spins start at frame zero;
  spin/wall direction changes accept both generic and specialized ROM targets and start at one.
- `$90:A436` accepts a fresh Jump edge only while falling and while the unaligned 8.8 Y
  magnitude is at least `$0280` and below `$0500`. Both item bits retain Space Jump physics
  while Screw art wins priority and republishes contact-damage index three every spin frame.
- Ordinary and Space Jump wall contact rewind to frame ten; Screw Attack rewinds to 26 and
  reaches eligibility/palette cycling at 27. Screw and Space bottom halves use their native
  always-drawn rule. Landing and wall-jump exit restore the normal suit palette.
- The shared `$90:85E2` draw gate now hides body OAM on odd invincibility frames while still
  selecting bank-$92 graphics DMA. Knockback or a nonzero stored/active shine timer forces
  the body visible; the independent arm cannon retains `$90:C663`'s stricter blink rule.
- Lower-body selection now includes `$90:86EE`'s `$D7/$D8` frame-0-through-2 suppression
  and the complete `$90:870C` type-`$0F` pose/frame matrix: ordinary crouch/stand, top-only
  morph/unmorph, `$DB/$DC` frame zero, `$DD-$F0` frame two, and always-split `$F1+` art.
- Standing-position dispatch preserves front-view frame-two-plus Y-1 placement and landing
  poses `$A4-$A7`'s packed `$90:8D28` offsets, including the native unaligned 16-bit read;
  visible OAM gets its intended low-byte nudge while debugger state retains the wrapped word.
- Type-`$0F` positioning reads all twelve signed two-byte records from `$90:8D80` directly,
  including the unused zero-offset `$39/$3A/$3F/$40` records, rather than duplicating only
  the eight ordinary transition values in host code.
- The renderer admits every native movement-type slot `$00-$1B`. Unused `$07/$09` remain
  top-only, `$0B/$0C` remain split, and `$0D` retains `$65/$66`'s frame-zero-only lower half;
  diagnostics can therefore step valid ROM pose records without inventing or rejecting art.
- Synthetic checks cover both velocity boundaries, held-versus-new input, equipment priority,
  specialized direction targets, contact damage, wall frames, six palette pointers/wrap, and
  landing restoration. The private-ROM routes freeze `$1B` and frame-27 `$82` against live
  Landing Site terrain and require every reachable pose/physics/palette milestone.

## Verified Crystal Flash movement/animation slice

- `SamusCrystalFlashState` translates the three installed handlers at `$90:D678/$D6CE/$D75B`
  as named debugger phases. Initiation retains `$90:D5A2`'s exact controller equality test
  (Down + L + R + Shot with no extra buttons), zero whole/fractional Y speed, energy below
  51, empty reserve, and current 10/10/10 ammunition requirements.
- The raise handler changes only whole Y by two for ten calls, then forces ROM animation
  frame six/timer three and publishes the bank-$88 HDMA spawn seam. Main decrements missiles,
  supers, and power bombs only on accepted NMI counters divisible by eight and calls the
  translated `$91:DF12` energy/reserve overflow behavior after every shot.
- The thirtieth decrement forces frame twelve/timer three. The unchanged `$91:B545/$B556`
  delay programs advance to `$FD,$01/$02`; the installed finish handler sees movement type
  zero on the following beta frame and only then restores normal movement and requests normal
  palette restoration. The resulting 20-pixel unsupported drop is consequently handled by
  the existing ordinary walk-off/falling route rather than by a fabricated Crystal Flash fall.
- The real-ROM `--crystal-flash-script` directly invokes the translated power-bomb cleanup
  call and save inventory. It rendered actual `$D3` art, consumed the three cartridge-timed
  ammo groups at NMI 248, returned through `$01` at frame 256, and completed the handler at
  frame 257. `Restart Crystal Flash` exposes the same route in the interactive viewer.
- `$90:D6AE-$D6C5` clears the Power Bomb flag on the tenth rise call and spawns the shared
  bank-$88 window owner at Samus's raised position. `$88:A552` reaches radius `$20B0` after
  eighteen exact 8.8 updates; `$88:A35D` retains that integer curve window while fading each
  nonzero fixed-color component every four calls, then `$88:A317` clears the shared state.
- Palette handler seven at `$91:DB93` copies ten body colors and six bubble colors into the
  two halves of sprite palette six. Their independent ten-record/six-record ROM tables and
  ten-/five-call cadences are live, including `$90:ACC2` beam-palette restoration at finish.

## Verified drained-Samus movement/animation slice

- `SamusDrainedState` translates every function behind `$91:E4AD`: let fall, standing
  drained, release, enable hyper beam, and crouching/falling drained. Direction is read from
  the source pose before `$E8-$EB` selection; function zero preserves the old bottom boundary
  while changing to radius 21 and clears the exact base-X/Y words.
- `$E8/$E9` begin at literal animation byte index two. Command `$F7` installs `$90:94CB`
  and performs its second frame increment before loading the following ROM delay. That
  handler reuses the same `$90:90E2` old-speed-before-gravity implementation as ordinary
  jumping, but performs no horizontal movement. Any translated block collision restores
  normal movement, writes frame seven/timer eight, and clears both Y-speed halves.
- Controller functions one/four intentionally write pose, frame, and timer without ordinary
  pose initialization. The C# cache-rebind seam preserves that cartridge behavior instead of
  recomputing animation-buffer/radius state. Release preserves the surprising literal byte
  indices 13 or four, then follows each asymmetric `$91:B257-$B298` program to `$FD,$01/$02`.
- `$90:8D98`'s 32-byte signed crouched-offset table and standing index-five `-3` override are
  used directly from ROM. `$90:8790` suppresses the drained lower body only at indices zero
  and one. Thus the authentic private-ROM art is rendered without generic type-`$1B` guesses.
- `--drained-samus-script` supplies only the missing bank-$A9 actor call timing. Against live
  Landing Site terrain it reaches `$E8`, installs `$F7` at frame 18, lands at frame 44,
  invokes standing `$EA`, returns to `$E8`, releases through `$FD,$01`, and installs hyper
  beam `$8000`. The 110-frame capture freezes the real crouched drained art.
- Samus commands five and `$18` now share the exact `$54` setup while retaining distinct
  `$90:E09B/$E0C5` timer handlers. The able route requires left drained pose `$E9`, frame
  eight or later, and a fresh Up edge before selecting frame 13 and replacing itself with
  RTS. The unable route accepts only frames 8..11, jumps to frame 18, and remains installed.
  Commands `$19/$17` directly freeze at frame 28 or resume at frame 13 with timer one.
- Command `$16`'s negative super-special flag now takes its real priority at `$91:D6F7`.
  `$91:D96F-$D997` loads all sixteen colors through the ten ROM pointers at `$91:D99E`,
  decrements after the copy, wraps the palette index at ten, and uses the Baby-owned
  one-through-ten special frame as its gradually increasing reload delay. Controller zero
  and command `$17` perform their same-frame equipment-selected full suit restorations.
  Synthetic palettes lock load-before-decrement ordering and the two-call hold; the real
  5,690-frame Mother Brain route captures the resulting private-ROM yellow/purple drained
  body in `DrainedRainbowFrame.png`.
- `MotherBrainRainbowBeamSamusMovement` translates `$A9:BBB5-$BC75` without converting its
  unusual arithmetic to floats. It adds signed 8.8 velocity to only the high subposition
  byte, carries that addition into whole pixels, preserves the low fractional byte, and
  mirrors the result into the native previous-position words. `$86:C272` Y components read
  the signed ROM sine table and preserve product bits 8..23.
- The beam moves right by `$10.00` to hardcoded X `$EB`, vertically follows the live angle,
  oscillates around wall Y `$7C` by `$00.40`, and after firing eases X from `-$01.00` by
  `$00.02` while adding `$00.18` Y acceleration. Ceiling/floor clamps `$30/$C0`, carry
  precedence, and subposition clears are verified separately. The previous-position writes
  deliberately suppress ordinary camera tracking during this actor-owned motion.
- `MotherBrainRainbowBeamAttackSequence` now translates the contiguous attack/final chain
  `$A9:B8EB-$C1A6`. Its first two `$0100` waits, native fallthroughs, power-bomb timer freeze,
  neutral/charging/firing head-list pointers, neck indices/deltas, and sound `$71` request
  lead into the active beam. Start chooses drained command five or `$18` at the literal 700-energy
  boundary, locks input, and initializes the HDMA-width, explosion, sound, and actor-function
  words. The wall, one-frame delay, drain initializer, and drain loop retain native
  fallthrough instead of flattening the sequence into host animation states.
- Verification requires all 300 `$012B` drain calls, the seven calls produced by sound
  counter six, exact explosion-table order, width growth through `$0C00`, and the resource
  cadence: energy every call, missiles/supers every fourth enemy execution, and power bombs
  every call. `$A9:C4E8/C515/C53A` always reload literal zero before the shared ammo writes,
  while selected-HUD-item clearing remains conditional; `$A9:C57D` retains its genuinely
  carry-dependent one/two damage.
- Beam shutdown narrows below `$0200`, disables its modeled HDMA channel, unlocks Samus,
  seeds cooldown eight, and falls through drained controller zero into the custom accelerated
  fall. Floor `$C0`, lower-head timer `$80`, the 129-call DEC/BPL decision, and the health
  `$0190` repeat/finish boundary are exact.
- The finish-off loop retains suit division before its `$50*4+20` threshold, the `$FA0`
  no-attack boundary, `$FF0` bomb boundary, two-onion-ring fallback, and every literal head
  list. It waits on body pose rather than skipping an in-progress walk, falls through into
  the `$10` admire timer, performs the 257-call final charge, and consumes one of four
  `$200`-byte Baby tile-transfer records per AI call. The fourth call observes the zero
  terminator, retracts Mother Brain's head, requests population `$A0:ECBF` at `$180,$40`,
  loads another `$100` wait, and reaches the final firing/self-return hold after 257 calls.
- `BabyMetroidCutsceneState` translates `$A9:C710-$CD26`: exact population-property OR,
  `$F8`/249-call stationary delay, `$FE80/$FA00` angle curves, speed clamp, cartridge sine
  components, byte-`+1` subposition movement, `$24x$24` collision radii, controller-one
  request, gradual `$1/16` latch acceleration, really-fast body stumble, predictive `$0200`
  target acceleration, exact head pin, draining instruction list, sound request, and the
  cross-enemy `$BE38` function write, four-entry shaking table, corpse-state poll, 65-call
  drain-stop delay, release acceleration during the 32-count dust timer, three-cloud request,
  ceiling collision, controller-four call, `$CA24`'s eight overlapping movement records,
  `$F45F/$F466` wrong-way acceleration extras, pre-move route collision, gradual `$CA66`
  pursuit, generic `$CF03` touch acceleration, exact Samus pin, 699 one-point heals from
  200 to 899, reserve fill, the `$CABD` wait for zero Baby health, `$CB13-$CC05` release,
  stare-down, off-screen retreat, final-charge setup, final blow, theme delay, hyper-beam
  preparation, command-`$19` freeze, signed downward acceleration, six-entry black fade,
  five-call explosion cadence and ten-pair offsets, even/odd blinking, four bank-$B7 attack
  tile DMAs after the 129-call unload wait, fractional rainbow slowdown, 177-call post-DMA
  delay, seven reverse room-light palettes, command-`$17`, Hyper Beam, enemy deletion, and
  phase-three cross-actor handoff. Mother Brain
  translates `$BE38-$BFCF`: the `$30`/49-call regain, eight alternating painful walks with
  literal 16/32/48/64 pauses, beam shutdown, rear-room walk, one-way neck raise, fast crouch,
  65-call pre-grey wait, eight palette copies plus terminating ninth probe, 36,000-health
  rewrite, and same-frame corpse publication. `$C059-$C1A6` then performs the `$300` revival
  delay, 224-step grey transition, wake/stretch/walk-up chain, random murder cooldowns,
  head-bytecode volleys, body-pose responses, and final retreat/hold. Bank `$86:C2F3-$C432`
  supplies its 18-slot onion rings, highest-free allocation, delayed head pin, sine flight,
  animation radii, Baby-first/Samus/room collision order, `$50` Baby damage, flashing/cry
  request, and suit-divided Samus damage. `$C1CF-$C3EE` now publishes form four, the backward
  recovery request, exact `$20` wait/fallthrough, all three reachable walking functions,
  normal/recoil neck functions, RNG-gated bomb/four-ring selection, and the `$40` attack
  cooldown. `$B562-$B5C4` applies the exact missile/generic/Hyper Beam walk-counter reactions;
  Hyper Beam underflow installs `$9BE7`, disables attacks, runs the `$0B` recoil and `$10`
  recovery timers, and restores `$9DBB`. `$AEE1-$B1D4` now translates the phase-three death
  retreat, 129-call smoky idle, mid-program stumble carry, effect shutdown, mixed/smoky
  explosion batches with one global RNG call per projectile, sixteen-step black fade plus
  terminator, BG2 clear request, decapitation, 8.8 accelerating brain fall/clamp, six `$1C0`
  corpse tile DMAs, eight-step grey fade plus terminator, and 257-call corpse display delay.
  `$B1D5-$B222` now runs the real shared 48-entry corpse-rotting table: `$E08B` extracts the
  right-hand bank-$B7 corpse frame into `$7E:9000`, `$DB12` performs the staggered 4bpp
  bitplane-row copy/move/clear operations, `$E1F4` queues all six WRAM-to-VRAM records on
  each of 117 active calls, and all 48 `$B223` dust hooks finish on call 118. Carry-clear
  completion applies both brain property words, queues `$0000/$FF24`, consumes the same-call
  first decrement of the `$14` delay, then clears brain X/Y after twenty further calls.
  `$B258-$B2D0` then emits seven NTSC escape-timer tile records; the seventh falls through
  and emits exploded-door page zero on the same call. Page one completes the second list,
  requests the exact fourteen-color door palette copy and four `$8D:FFC9-$FFD5` red-flash
  objects, queues track seven, holds quake 5 at `$FFFF`, disables the Mother Brain unpause
  hook, and initializes the Zebes escape typewriter with its `$20` text timer.
  `$B2D1-$B345` now preserves the alternate subtitle's simultaneous typewriter calls,
  accepts the external `$2610` completion carry, runs the 33-call exploding-door countdown,
  cycles the four absolute dust positions at five-call intervals with one global RNG advance,
  publishes Samus command `$0F`, TimerStatus `$0002`, boss bit `$02`, and event `$0E`, then
  emits parameters 0..7 plus hardcoded PLM `$B677` and maintains the final global quake.
  Bank `$86:C482-$CB11` shares all 18 physical slots between blue rings, phase-three bombs,
  the eight door fragments, and the alternate subtitle. `$9F00-$9F32` head bytecode now
  executes after the Baby slot is gone, queues its cry, publishes the one-afterburn bomb and
  large-purple-breath request, then returns through phase-three neutral `$9CB9`. Bombs retain
  their parameter in the low X-subposition byte, apply pre-bounce `$02` friction, common 8.8
  movement, screen-edge reflection, all nine `$C550` acceleration/bounce stages, and the
  34-call nine-spritemap loop. Natural expiry requests the preserved afterburn, dust three,
  and sound `$13`; collision with a real timer-zero normal Samus bomb instead requests dust
  nine and Mother Brain-head drops. Both paths maintain body bomb counter `$802A` exactly.
  Fragment initial Y/velocity tables, `$10` X
  friction, `$20` gravity, common 8.8 movement, 18-call ROM spritemap loop, 33-call lifetime,
  four-pixel terminal Y adjustment, and parameter-nine dust requests are verified exactly.
  `$86:CB2F` now allocates in that same pool, stays fixed at brain `(+6,+16)`, interprets
  eight bank-$8D spritemaps for exactly 76 visible calls, and deletes on call 77. Shared
  `$8390/$83B2` passes draw definitions around Samus according to property bit `$1000` and
  preserve native bank-$8D tile addition, palette OR, clipping, vertical carry, X-high, and
  OBJ-size rules. `$86:E509` additionally follows `$E42C` for the Baby's parameter-three small explosions:
  six ROM frames remain visible for 31 calls, delete on call 32, and use `$E6E0`'s strict
  256x256 layer-origin test. The earlier attack-selection trigger, live Hyper Beam shot/damage
  producer, typewriter character engine/glyphs, Baby actor spritemap/other dust producers, and
  HDMA effects beyond the translated direct-CGRAM palette objects remain seams.
- `MotherBrainBodyAnimationState` translates the ordinary enemy-instruction stage used by
  Mother Brain's painful fast/medium/slow/really-slow walks in both directions plus
  `$99C6/$99E2/$99F2/$9A26` stand/lean/crouch lists.
  It reads duration/spritemap pairs and exact movement/pose/sleep opcodes from bank `$A9`,
  mirrors body movement into BG2 scroll (including the posture opcodes' independent X
  compensation), and retains footstep earthquakes and the form-three sound gate. The backward attack walk
  crosses the helper's hard X `$30` boundary at `$2E`, so AI advances before the still-running
  animation settles at `$28`; verification locks that easily missed scheduling detail.
- `--mother-brain-rainbow-script` executes the route against the private ROM in native
  AI-then-enemy-instruction order. It reaches retract at 257, crosses X `$30` at 298, begins
  active firing at 570, drains exactly through 872, releases Samus at 879/880, reaches floor
  `$C0` at 917, selects finish-off at 1047, finishes the body walk at 1128, starts final
  charge at 1144, copies real Baby graphics into VRAM on frames 1401..1404, and reaches the
  final-beam hold at 1661 and starts the Baby curve at 1652. The brain slot now runs the real
  `$9072-$92AA` neck angles and five-segment geometry: the Baby begins face/latch convergence
  at 1662/1672, pins to the moving brain at 1679, and executes Mother's `$BE38` response on
  1680. Painful walking ends the beam at 2338, reaches low power at 2722, requests the corpse
  crouch at 3119, and publishes corpse state one on 3336. The Baby stops draining through
  3401, requests dust on 3434, and installs `$CA24` after ceiling collision on 3447, with
  energy/ammo `200 / 5 / 5 / 100`. Its route pointers advance on frames
  3524/3591/3688/3689/3768/3788/3805, touch AI latches on 3819, and the 699th heal enters
  `$CABD` on 4536 with 899 energy and 99 reserves. Mother Brain wakes on 4466, starts murder
  cooldown on 4709, and the ring system takes Baby health to zero on 5355. Release, stare,
  retreat, and final-charge phases reach 5356/5357/5423/5488/5583/5584; the four final rings
  spawn on 5600/5603/5606/5609, strike on 5618, and enter death on 5703. Six black palettes
  land on 5799..5844. All 30 five-call-cadence explosions allocate in the shared pool; the
  5750 capture contains six simultaneous ROM animation stages. Invisibility begins on 5853,
  bank-$B7 attack rows replace the Baby rows
  on 5982..5985, seven room-light palettes land on 6161..6167, and deletion/Hyper Beam/handoff
  occurs on 6168. Mother Brain executes recovery on 6169; `$C1F0` falls through `$C209` on
  6202 and deterministic RNG `$8EC6` immediately selects four rings. Those rings strike
  Samus on 6259/6262/6265/6268 after the deleted Baby slot is correctly ignored. Cooldown
  expires on 6267, RNG `$B475` selects a bomb on 6272, and `$9EBD/$9B6D` publish its real
  bank-$86 bomb and purple breath into slots 17/16 on 6304. The 6320 diagnostic shows both
  real bank-$8D animations in OAM; a second bomb/breath pair uses slots 15/14 on 6370. Bomb
  floor bounces begin accelerating on 6371/6437 while a later `$A5D1` four-ring call requests
  the forward body list on 6404. The real body animation advances X from `$28` through `$59`
  by frame 6500. The regression compares the final four `$200` attack transfers against the
  supplied cartridge and reads every flight/neck/projectile component from its real sine table.

## Verified grabbed-by-Draygon movement/animation slice

- `SamusDraygonGrabbedState` keeps the boss-owned and Samus-owned halves separate. Entry
  reproduces `$90:E23B`'s installed RTS movement handler, chooses `$BA/$EC` from Draygon's
  direction, and `$A5:94A9` pins Samus at owner X minus/plus eight and owner Y plus `$28`
  without discarding either subposition.
- All ten `$BA-$BE/$EC-$F0` definitions remain cartridge data. The left and right transition
  lists at `$91:AE18/$AE56` preserve native priority and never cross facings; neutral, aim-up,
  firing, aim-down, and six-frame moving bodies use their real delay lists, split tile DMA,
  spritemaps, draw offsets, and 21-pixel radius. No-input fallback maps moving `$BE/$F0` back
  to `$BA/$EC` through pose-definition byte two.
- The ordinary movement-type `$1A` dispatcher performs only `$90:A7D2`'s vertical-solid-result
  clear. While the boss grab is active, the installed RTS handler prevents ordinary physics
  and the owner-coordinate seam remains authoritative instead of inventing a C# flight path.
- `$90:E2A1` masks newly pressed input to `$0F00`, ignores zero and a repeated previous D-pad
  pattern, preserves the previous pattern across blank frames, suppresses pose transitions
  while grapple is locked, and releases at exactly 60 accepted changes. `$90:E2DE` selects
  `$01/$02`, clears the native base-X/Y/prospective/bounce/acceleration words, deliberately
  preserves extra run speed, and publishes owner flag bit one.
- Independent synthetic fixtures cover both facings, literal records/delays, priority,
  fallback, owner offsets/subpositions, the sole type-`$1A` write, repetition rules, the exact
  release threshold, cleanup/preservation, and one-shot owner signal. The private-ROM
  `--draygon-grab-script` reaches `$EC->$ED->$EE->$EF->$F0->$EC->$F0->$01`; a 90-frame capture
  freezes the authentic moving body against live Landing Site terrain, and 180 frames prove
  release. The fixed owner coordinate is explicitly a substitute for the absent Draygon actor,
  not a claim that its enemy flight AI has been translated.

## Verified aerial-turn and wall-jump slice

- Opposite-direction input from every admitted normal-jump/falling aim family publishes
  generic `$2F/$30/$87/$88`. `$91:F952/$91:F98A` then index the previous pose's literal
  ten-way shot direction through `$91:F9D6/$91:F9E0`, including compact directions four/five,
  to select all sixteen `$2F/$30/$87-$96/$9E-$A1` turn records.
- The initializer performs the native wrapping 16.16 fold of extra-run speed into base speed,
  clears the consumed component, and selects mode one. Movement types `$17/$18` call the
  deceleration-allowed X routine, so the newly facing pose initially travels in the OLD world
  direction. Their simple-Y path retains old-speed-before-gravity ordering and collision
  command five clears vertical motion without prematurely replacing the turn pose.
- Each airborne turn spends three two-tick NTSC frames and consumes its ROM `$F8` operand.
  Exact endpoints preserve unaimed, straight-up, diagonal-up, and compact/down aim metadata.
  The real-ROM `--aerial-turn-script` proves `$4B->$4D->$2F->$52->$A5` and asserts each
  post-frame milestone rather than merely checking that the runner did not throw.
- `$90:9D35` probes eight pixels through `$94:967F` without mutating live position. During
  spin frames below `$0B`, a block contact writes timer one/frame `$0A`; at eligible frames a
  fresh Jump edge launches only when the clipped whole-pixel distance is strictly below eight.
  Carry set skips gravity/displacement on that trigger frame.
- The bank-$91 collision command installs `$83/$84`, clears old momentum, and reads air,
  water, or lava ordinary/Hi-Jump values directly from `$90:9ED1-$90:9EE3`. Movement type `$14`
  reuses ordinary jumping physics. Animation command `$FB` selects ordinary, Space Jump, or
  Screw Attack frame ranges in cartridge priority order; the wall-specific bottom selector
  draws frames below three and at/above thirteen. Landing uses `$A6/$A7`.
- Synthetic verification exhausts all twenty selector-table entries, compact expansion,
  mode-one displacement, `$F8`, early wall rewind, the seven-pixel launch, trigger-frame Y
  suppression, `$FB`, and the 4.A000 dry wall arc. Solid-enemy wall jumps remain explicit.

## Verified knockback and damage-boost slice

- The untranslated enemy system is represented by one explicit producer seam: a caller
  supplies bank `$A0`'s left/right hit result. `$91:EDB0` then chooses knockback direction,
  installs `$53/$54`, reads the air/water/lava entry from `$90:9EE9/$90:9EEF`, and starts the native
  five-count hurt timer. No enemy damage, velocity, pose, or duration is fabricated.
- The same `$91:ED63` command writes one to shared hurt-flash counter `$0A48`; this state now
  lives on Samus rather than incorrectly inside the shinespark child. Final-priority palette
  handler `$91:D8A5-$D910` alternates complete `$9B:A380` hurt colors on counters 1/3/5 with
  the ROM-selected Power/Varia/Gravity suit on 2/4/6, leaves CGRAM untouched on 7..59, and
  clears when counter 59 increments to 60. Cinematics use `$9B:A3A0` for even restores and
  suppress the counter-two library-one `$35` impact sound.
- Incremented counter forty reproduces `$91:D911-$D953`: active spin/Space/Screw movement
  queues `$31/$3E/$33`, the five pre-cancel grapple handlers queue `$06`, and a non-spinning
  held charge arms the same-frame `$41` post-draw sound latch. Active shinespark frames now
  publish eight to this shared word and crash/windup clear the same word, matching their
  original cross-system ownership.
- `$90:DF15/$91:EE27` now admit Morph Ball and Spring Ball types `$04/$08/$11-$13`.
  Unlike the humanoid branch, they republish the current pose, preserve its rolling animation
  frame/timer, ignore controller input and damage-source side when selecting vertical direction,
  and choose direction one/two solely from left/right pose metadata. Horizontal travel still
  independently follows bank `$A0`'s X-side word, matching `$90:8EDF`.
- The special `$90:DF38` handler takes precedence over the normal type dispatcher. It uses
  type `$0A`'s normal-air speed record and bank-$A0's X direction, applies either gravity or
  no-speed downward movement according to directions one/two/four/five, and performs exact
  bank-$94 block clipping. Vertical collision clears the same X/Y state as `$90:DF6E`.
- Timer-zero `$91:F31D` clears Morph Ball bounce state, both Y-speed words, and publishes
  direction two for every family. `$53/$54` additionally become `$29/$2A` and move their
  radius-19 center down two pixels to keep the old radius-21 feet fixed. A ball keeps its
  exact pose and animation, then ordinary grounding naturally selects `$31/$32` while elevated.
- Input remains live during ordinary knockback. Right-facing `$53` plus canonical `$0280`
  (Left+Jump) follows literal `$91:A8E4 -> $50`; left-facing `$54` mirrors through `$0180`
  (Right+Jump) to `$4F`. Crossing movement families calls Make_Samus_Jump, clears the hurt
  timer/direction, and restores the normal handler through `$91:F8AE`.
- Movement type `$19` at `$90:A7CA` directly reuses ordinary jumping movement: type-indexed
  16.16 X acceleration, variable-height Jump release, old-speed-before-gravity Y movement,
  ceiling/floor collision, and camera tracking. Its bottom half is hidden only on animation
  frames two through eight, matching `$90:877C`.
- `$4F/$50` store shot direction `$FF`. Landing therefore takes `$91:E95D`'s sentinel branch
  to ordinary `$A4/$A5` by live X direction before the aimed-landing lookup; radius expansion,
  velocity cleanup, and `$F8` completion use the existing verified landing path.
- DebugRunner `--knockback-script` injects only the missing enemy-side bit, observes `$53`,
  matches the cartridge's `$91:A8E4` record, executes `$50` type-`$19` movement, and renders
  the private-ROM pose against live terrain. Synthetic verification independently locks the
  timer, exact 16.16 values, damage-boost handoff, `$FF` landing, and timeout to `$29/$2A`.
- DebugRunner `--morph-knockback-script` reaches `$1D` through real `$37/$F9`, publishes the
  same one-bit enemy seam with deliberately conflicting facing/input, and requires preserved
  pose/animation, five `$DF38` moves, same-pose completion, `$1D -> $31`, and real floor
  collision back to `$1D`. Its active and landed PNG sets retain ROM ball tiles, palette,
  terrain, sky, HUD, camera, minimap, and OAM.

## Verified grapple firing and connected slice

- `$9B:C51E/$C703` read shot direction, signed extension velocities, fire angle, movement-
  specific hand/flare origins, and pose graphics Y correction directly from ROM. Length grows
  by twelve and cancels before collision at 128; release of Shoot queues the same cancel path.
- `$94:A85B` advances two signed 16.16 endpoint offsets in four collision substeps per frame.
  The grapple dispatcher handles air, slope/solid cancellation, horizontal/vertical BTS
  extensions, and persistent type-`$E` BTS zero/three's carry+overflow connection result.
  Breakable BTS one/two install the native 40-slot bank-`$84` room owner, save the complete
  level word, clear BTS, and return the same carry+overflow connection result.
- `$84:D0DC/$D0E0` now interpret the cartridge's `$CD6A/$CDA9` lists. Their exact 240/120-
  frame delays, sound `$0A`, `$E0B7/$0053/$0054/$0055/$00FF` terrain words, BTS-one
  restoration, one-frame delayed deletion, BG1 ring redraw, and respawn/non-respawn split
  continue independently after Samus releases the rope.
- On connection, `$9B:B97C` reads all ten records from the default `$C3C6`, moving-vertical
  `$C3EE`, or crouching `$C416` table. Their literal function/handler pairs select swinging
  `$B2/$B3` or stationary `$A8-$AB/$B4-$B7`; crouching horizontal directions four/five
  deliberately use standing `$AB`. `$BA61/$BA9B` center the endpoint, shorten a 64+-pixel
  rope by 24, and calculate its angle through the integer octants at `$A0:C0B1`.
- Native rope Start and flare/draw coordinates remain separate. Swing command nine copies
  Start to Flare before angle-authored body positioning; locked command ten positions Samus
  from Start minus raw no-run Origin, then independently rebuilds Flare. Both commands run
  the shared X/run/Y-speed cleanup and twelve-pixel camera-history clamp. One explicit debug
  producer remains for the connected real-ROM route because Landing Site has no type-`$E`
  blocks; it supplies no pendulum position, velocity, art, or release result.
- `$9B:C79D` owns Shoot retention, new Up/Down rope-length deltas, the lower-half Left/Right
  pump gate, exact `$8000` motionless kick, quadrant gravity, signed angular-velocity clamp,
  jump impulse, and angle integration. `$94:AC31` walks every intermediate rope pixel and
  tests the appropriate leading body frontier. `$94:ACFE/$ABE6` walks each crossed angle byte,
  probes six radial points at eight-pixel intervals, restores the last-safe `$xx80` angle on
  collision, negates arithmetic half base/extra velocity, and opens the 16-frame kick window.
- Ordinary wall launch now preserves `$91:F2D3`'s library-three sound five, while grapple's
  `$9B:C9CE` publishes generic library-one sound seven and clears the projectile flare
  counter. Movement type `$14` uses its exact frame thresholds: charged frames 3..22 publish
  contact-damage index four and every frame from 23 onward publishes index three.
- `$94:A957` applies the direction-sensitive anchor low nibble (7 or 8) before both collision
  and drawing. Air, solid, slope, extension, and spike collision carry semantics follow the
  grapple-specific dispatcher. `$94:AA9E/$AB17` now apply the exact spike-air and solid-spike
  BTS damage tables, including the per-sweep invincibility guard and `$A0:9169` timer aging;
  firing into type-`$A` also preserves Draygon-turret BTS-three's one-damage connection.
- Persistent block anchors are revalidated at `$9B:B8F1` on every connected frame. Air
  replacement queues moving release or the motionless dropped handler. `$9B:C8C5` selects
  `$01/$02` directly for `$B2/$B3`, otherwise reads the standing `$C9BA` or compact `$C9C4`
  ten-direction table and runs the shared larger-pose collision resolver before clearing
  X/Y launch velocity. Bank `$A0`'s signed sine table plus `$9B:C1C2/$C2C2/$C302` publish
  beam origin, body center, and animation.
- A close radial collision at rope length eight and point six/five sets bank `$94`'s special
  flag. `$9B:BAD5` scans the eight ten-byte records at `$C43E` in reverse, requires the exact
  `$xx80` angle, installs the record's `$B4-$B9` pose and signed anchor-relative offsets,
  selects locked `$C77E` or wallgrab `$C814`, and clamps camera history to twelve pixels.
- Locked poses remain frozen while Shoot and the anchor survive. Release queues `$C856`, then
  movement type `$16` command six follows the pose-definition fallback to `$01/$02/$07/$08`
  or `$27/$28` while clearing horizontal/run momentum. Wallgrab release installs decimal 30;
  `$C832` permits checks 29 through zero, probes exactly 16 pixels toward the pose direction,
  and requires a fresh Jump edge. `$C9CE` deliberately maps `$B8 -> $84` and `$B9 -> $83`,
  clears grapple/momentum, and reuses the ROM environment-selected normal/Hi-Jump wall-launch
  table and existing `$FB` animation-command handoff. Enemy-backed probes additionally publish
  the exact slot to `EnemyIndexToShake`; live enemy AI consumption remains a separate seam.
- `$9B:BFA5` queues the two bank-`$9A` tile transfers selected through `$9B:C342/$C346` to
  VRAM `$6200/$6210`. `$94:AF87/$AFBA` retain all sixteen staggered segment instruction
  slots, emit small OAM tiles `$21-$24`, and draw endpoint tile `$20` with native flip bits.
- Releasing Shoot runs `$9B:CA65` immediately, including signed sine products and acceleration
  mode two, then preserves the one-frame function-pointer seam before `$9B:CB8B` installs
  `$51/$52`. Independent movement handler `$90:946E` starts on the release frame, selects the
  standalone environment record through `$90:9C21`, performs block X/Y collision and gravity,
  and remains active after beam cleanup until apex underflow or vertical collision.
- Synthetic verification fixes ROM table addresses, all thirty connection table routes,
  distinct rope Start/Flare geometry, shared speed cleanup/camera clamp, exact pendulum
  numbers, two VRAM queue records, four-step firing accumulation, persistent-block centering,
  integer connection angle, anchor-side bias, per-pixel rope obstruction, six-point angular collision/reflection,
  collision kick, anchor disconnection, all 30 wall-grace checks, exact `$B9 -> $83` reversal,
  locked fallback, compact dropped-table selection, camera-history clamp, six staggered rope
  OAM records, endpoint placement, release velocity, and queued handoff. `--grapple-fire-script`
  proves real-ROM firing and cancellation; `--grapple-script` proves live terrain reflection
  before its release handoff, then the 100-frame route proves `$90:946E` keeps moving the
  `$51/$52` body after the beam function has become inactive.

## Verified combined/Charge/Hyper Beam/missile projectile slice

- HUD item zero/three dispatches `$90:B80D` into the five ordinary slots while sharing
  cooldown `$0CCC` with bombs. All twelve valid low-nibble beam combinations use all ten pose-authored directions,
  cartridge muzzle origins, signed 8.8 velocity/acceleration, bank-$94 terrain collision,
  indexed bank-$93 data/instruction/explosion lists, individual cooldown/sound entries, and
  exact beam tile/palette upload. Spazer's three streaks are one ROM spritemap in one slot.
- `$90:B887/$B986` dispatch even combinations to no-wave terrain impact. Odd combinations
  use `$94:A352/$A3E4`'s pass-through Wave movement; uncharged types one/three reload trails
  every three frames, while later and charged Wave families reload four. `$93:8268` uses
  its separate NMI-bit-one/opposite-phase flicker branch for Spazer/plasma art.
- Equipped Charge bit `$1000` fires one ordinary shot on the first held frame, increments
  `$0CD0` to the 120 clamp, treats 60 as the armed threshold, and chooses ordinary or charged
  release accordingly. The same live counter is mirrored into Samus beta movement so charged
  spin/wall-jump contact damage consumes the word produced during projectile alpha.
- `$90:BAFC-$BC98` initializes flare timers 3/5/4, draws the central component from count 15,
  adds both sparks from count 30, and interprets `$FF` loop/`$FE` rewind commands through
  `$90:C481`. Direction/running origins and all `$93:A1A1` spritemaps remain ROM-authored.
- Charged beams use `$93:83D9`, type bit `$0010`, the charged cooldown/sound tables,
  and `$93:8268`'s no-flicker branch. The real ROM route observes final type `$9010`, damage
  `$003C`, sound `$17`, and a visible orange cannon flare in the composed/OAM capture.
- `$90:B657-$B80C` owns eighteen detached trail slots, scans from native index `$22`, and
  selects left/right instruction lists from the projectile's exact type bits. Charged plain
  power selects `$B4CB` on the left and the intentional empty `$B4C9` on the right. Spawn
  position uses `$93:81D1`'s current animation field and bank-$9B's signed four-byte offset;
  drawing interprets timed tile records plus `$B525/$B587/$B5B3` position commands, preserves
  live records while time is frozen, and writes the native raw small-OBJ form after beams.
- Synthetic verification covers all ten ordinary directions, all twelve combination indices,
  both Wave trail cadences/pass-through, charge threshold/release, flare/trail animation,
  charged data routing and family flicker OAM, fixed-point travel, type-eight
  impact, explosion retention, and deletion. `--charge-beam-script` supplies the private-ROM
  hold/release regression; `--beam-type 0..11` selects and asserts any family. Real-ROM type
  eleven produces uncharged Ice+Wave+Plasma multi-streak art and charged type `$901B`, damage
  `$0384`, sound `$21`. `$91:F5CF/$F8F3` now publish the normal-jump (`$8000`) and Moonwalk-turn
  (`$0100`) forms of the one-frame `$0B5E` transition-direction bridge; `$90:B82D/$BA5F/$EB20`
  force release, consume its low-byte direction, and clear it in native phase order. `$91:D799`
  also runs the charged shot's three white-body calls plus equipment-selected suit restoration.
  `$91:D743-$D793` cycles the six Power/Varia/Gravity charge palettes through nested
  `$D7D5` pointers, selects `$D7FF`'s six pseudo-screw entries on contact-damage index four,
  and resets byte-offset `$0B62` whenever charge or grapple eligibility ends. The private-ROM
  67-frame route validates and visibly captures every ordinary charge entry; synthetic checks
  cover all 36 family/suit/frame combinations and the grapple reset.
  The non-shootable projectile block families remain seams;
  plain power's left/right trail instruction pointers are deliberately empty in retail data.
- Hyper Beam follows the forced `$91:E5F0` equipment `$1009`/flag `$8000` grant and bypasses
  ordinary charge input. `$90:BCD1` publishes type `$9018`, charged data-table index eight,
  damage `$03E8`, sound `$1F`, cooldown 21, glow `$8014`, and flare sentinel `$8000`.
  `$90:B159` reuses Wave fixed-point motion without its detached trail. The same grant spawns
  `$8D:E1F0`; timer-one initialization executes `$C655,$01C2`, then ten eight-color records
  each last two handler calls and write only CGRAM `$E1-$E8` before `$C61E,$D904` loops.
  The independent `$8014` Samus-body timer follows `$91:D7B6/$D829`: ten bank-$9B palettes
  alternate with ten hold calls before the 21st call restores the selected suit palette.
  Synthetic and private-ROM checks require every projectile literal, native flare lifetime,
  decoded bank-$93 art, all ten palette records, exact cadence, and no trail; the interactive
  viewer exposes the same path through **Hyper Beam enabled**.
- HUD item one now dispatches `$90:BE62`: fresh Shoot allocates from the shared five slots,
  installs invincibility 20/cooldown 10, consumes one round, reads damage/direction art through
  `$93:83F3 -> $8641`, and auto-deselects on the last round. `$90:AF68/$B2F6` preserves the
  one-frame `$0100` ignition and then combines generic `$10` acceleration with the signed
  cardinal/diagonal `$90:C303` table. Movement uses bank-$94's center-point solid test; impact
  applies the native radius-leading-edge correction and selects family `$0800`/`$93:867F`.
  Trail family `$20` selects retail left list `$B5A1` and its `$2A48-$2A4B` four-frame exhaust;
  the right stream is intentionally empty. Point collision now handles square/non-square slopes
  and the same complete shootable-block dispatcher as beams; extension, door, bombable, and
  special-block reactions remain explicit boundaries instead of guessed air or solid.
- Type-`$4/$C` collision now translates all sixteen `$94:9EA6` entries. BTS 0..3 execute
  `$84:CE6B` and the dimensioned 384-frame respawn lists; 4..7 execute permanent `$B3C1`
  lists. BTS 8/9 gate power-bomb family `$0300` through `$D084/$D088`, while A/B gate Super
  Missile family `$0200` through `$D08C/$D090`; rejected beam/missile families clear the
  provisional PLM without terrain mutation. The accepted lists preserve `$CB71/$CB94/$CC0B/
  $CC20`, both max-one opcode entry points, max-six Super sound, synthesized `$x052/$x057/
  $x09F` restore words, Wave side effects with unconditional pass-through, and C..F no-op
  allocation/deletion. Synthetic tests fire ordinary and Wave beams plus a live linked Super
  Missile through these reactions and exhaust every permanent/respawning weapon-gated timer.

## Verified ordinary Morph Ball slice

- Stable crouch requires a second newly-pressed Down edge, not merely the held input that
  entered crouching. `$91:F7CE` rejects the entry unless equipped-items bit `$0004` is set;
  the grounded viewer sandbox exposes that one inventory bit as a documented host stimulus.
- `$37/$38` shrink radius 16 -> 7 and command seven moves the center down nine pixels so the
  collision body's bottom is unchanged. Command `$F9` reads live Y speed/subspeed and chooses
  grounded `$1D/$41` or airborne `$31/$32`; the stable ball poses share the same animation
  list, so direction and movement changes preserve the rolling frame and countdown.
- Movement type `$04` uses the literal Morph Ball speed-table record, including mode-one
  opposite-direction deceleration, normal acceleration, slope alignment, no-speed grounded
  probing, and `$1D/$41 <-> $1E/$1F` fallbacks. Walking off selects `$31/$32` without
  resetting existing horizontal state.
- Movement type `$08` copies old 16.16 vertical speed before environment-selected gravity
  exactly like the native path. A hard floor impact enters bounce state one; the next impact enters state two
  with whole Y speed reduced by one; a gentle or state-two impact restores `$1D/$41` and
  clears vertical speed. Ceiling collision reverses the active upward pass.
- Up from a stable ball selects `$3D/$3E`; command seven attempts radius 7 -> 16 while moving
  the center up nine pixels. The shared larger-pose collision resolver aligns away from one
  obstruction and retains the ball in a tunnel blocked on both sides. Successful `$FD`
  completion enters `$27/$28` with the same bottom boundary.
- Morph transition art uses the exact `$90:8D80` render offsets: entry frames `-4,-2` and
  exit frames `+5,+4`. Stable ground/air ball poses use the ordinary bank-$91 spritemap path
  while deliberately omitting the humanoid bottom-half draw.
- Equipped Spring Ball makes the same `$F9` command select `$79/$7A` or `$7D/$7E`.
  `$79-$7C` use movement type `$11`'s environment-selected speed record; Jump selects `$7F/$80`
  and initializes the ROM air/water/lava launch. Type `$12` retains the normal jump cutoff,
  while type `$13` retains morphed falling. Landing with Jump held relaunches immediately;
  automatic rebounds use the native `$0601/$0602` state before returning to `$79/$7A`.
- `$90:BF9D/$90:C0E7` require equipped bit `$1000`, a fresh Shoot/X edge, fewer than five
  bombs, and a clear low cooldown byte. Placement uses physical slots `$0A-$12`, stores
  timer 60 in the `projectile_variables` / `bomb_timers` alias, resolves damage/list data
  through `$93:83F1`, and immediately runs the new slot to timer 59 and its first art frame.
- `$90:C128` switches the live instruction pointer from slow to phase-equivalent fast art
  at timer 15 and selects the `$93:A06B` explosion list at zero. `$93:81E9` executes timed
  frames plus `$8239` goto and `$822F` delete; deletion clears the slot and decrements the
  native aggregate. `$93:834D/$81:8A4B` draw the cartridge spritemaps from the standard
  `$9A:D200 -> VRAM $6000` sprite upload, before Samus in OAM order.
- At timer eight, strict X/Y radius overlap in `$A0:97E2-$A0:984E` publishes direction
  one/two/three. The direction remains a low word until the following frame, then becomes
  `$0801/$0802/$0803` through the
  morphed bomb-jump setup and bank-$91 special command three. `$90:E025` consumes the
  environment-indexed `$90:9EF5/$90:9EFB` launch pair without moving on its start frame.
  `$90:E032` uses the standalone `$90:9F25` speed record for left/right displacement,
  performs the old-speed-before-gravity vertical pass, and ends on upward collision or
  signed-speed apex. Diagonal apex selects acceleration mode two; the preserved current
  ball pose then owns falling and ordinary two-stage bounce recovery. `$94:9CF4`'s
  center/up/right/left/down cross is complete for no-op air/slope/solid families,
  type-`$7/$F` bombable terrain, type-`$4/$C` shootable terrain, and type-`$B` special
  terrain. Type-`$5/$D` extensions redispatch to their signed parent;
  `$84:CEDA` preserves a type-$F parent as temporary type-$8 terrain through movement beta,
  skips redundant reaction sound `$0A` for normal bombs, and starts the real 1x1/2x1/1x2/2x2
  permanent or 384-frame respawn list in the same frame's PLM pass. Shootable BTS 0..7 now
  runs the dimension-correct permanent/respawning shot lists and max-one sound opcode; BTS
  8..B reveals the exact power-bomb/super-missile word selected by normal-bomb setup.
  Special BTS 0..7 reveals dimensioned crumble blocks, E/F reveals speed blocks, and the
  negative-BTS path uses the literal area table, including Brinstar entries 2..5.

## Verified external-displacement consumer slice

- `SamusKinematicsState` now retains native producer-owned words `$0B56-$0B5C` as two
  signed 16.16 X/Y values. Movement reads but never clears them: enemies, PLMs, and scripted
  room effects remain responsible for publishing and retiring their own displacement.
- `$90:E464/$90:E4AD` combine external X with the signed native horizontal result before
  applying the ±15-whole-pixel collision clamp. Every translated ordinary grounded, aerial,
  Morph/Spring Ball, posture, bomb-jump, knockback/damage-boost, and shinespark consumer now
  reaches that arithmetic instead of silently dropping the producer words.
- Gravity movement preserves the cartridge's ordering: it snapshots old internal velocity,
  advances stored velocity, gives that snapshot the direction word's sign, and only then adds
  external Y. The external result may reverse actual collision travel without changing the
  stored direction word.
- No-speed `$90:923F` behavior is intentionally asymmetric. Negative external Y moves upward
  exactly; positive external Y receives the native +1.0000 downward collision bias. With no
  producer it retains the slope-adjustment/total-X grounding probe. Transition-only
  `$90:9288` uses the same signed external rule and returns no Y move when the producer is zero.
- Active Morph/Spring Ball bounce gives a nonzero external Y producer priority over rebound
  physics when knockback is zero: it clears both bounce-speed words, forces direction two,
  and scans using external-only displacement. Vertical shinespark adds external Y after
  negating its speed and applies the 15-pixel temporary clamp only to results still moving up;
  a producer-reversed downward spark deliberately bypasses that upward-only clamp.
- Synthetic verification locks positive/negative fractions, persistence, no-speed bias,
  transition-only Y, gravity reversal, Morph Ball bounce override, and shinespark reversal.
  The private-ROM `--extra-displacement-script` supplies only the missing producer seam. Its
  40-frame capture freezes authentic falling art over cartridge terrain; the 72-frame route
  requires exact +32-pixel X travel, an external lift, normal gravity/floor recovery, camera
  and minimap tracking, and explicit producer-word cleanup.

## Evidence

- The real-ROM `--forward-facing-script` renders pose `$00` with definitions `$92:CD45` and
  `$92:D1F2`; its object-only capture visibly retains the power-suit chest correction while
  the synthetic fixture proves suited `$9B` emits no extra raw object.
- `SuperMetroid.Verification` checks synthetic ROM tables independently for `$FD/$F8`, jump
  constants, gravity ordering, jump cutoff, mirrored spin movement, walk-off falling, solid
  collision, radius alignment, and landing.
- The real-ROM `--extra-displacement-script` holds +1.0000 X for 32 frames, applies signed
  -0.8000 then +0.8000 Y intervals, and clears all four words at frame 64. The runtime moves
  exactly 32 pixels right, enters falling pose `$29` during the lift, lands through `$A4`,
  returns to `$01`, and renders aligned terrain, sky, HUD, minimap, camera, and OAM captures.
- The real-ROM `--jump-script` route currently completes through `$01 -> $4B -> $4D -> $A4
  -> $01 -> $09 -> $19 -> $A6 -> $09 -> $01`, with ROM-authored graphics/tile transfers and
  moving camera/background/minimap state.
- The real-ROM `--run-script` reaches `$09`'s ordinary-Dash cap at frame 35, retains 2.0000
  across `$09 -> $19`, collides with Landing Site's real ceiling/floor, and clears the extra
  component in landing's ordered pass. Assertions require both `$0B3C` and type-three carry.
- The real-ROM `--posture-script` route completes through `$01 -> $35 -> $27 -> $3B -> $01
  -> $25 -> $02 -> $0A -> $02 -> $36 -> $28 -> $3C -> $02`, with the expected five-pixel
  center shifts and unchanged feet, terrain, camera, HUD, and ROM-authored graphics.
- The real-ROM `--aim-script` route completes all six stationary poses and fallbacks:
  `$01 -> $03/$05/$07 -> $01`, turns and decelerates to `$02`, then runs
  `$02 -> $04/$06/$08 -> $02`; its visible `$08` diagnostic retains terrain/HUD alignment.
- The real-ROM `--aim-run-script` route completes `$01 -> $0F -> $05`, `$01 -> $11 -> $01`,
  turns and decelerates left, then completes `$02 -> $10 -> $06` and `$02 -> $12 -> $02`.
  It reaches the native speed cap in both directions and keeps camera, slopes, HUD, and tiles aligned.
- The real-ROM `--aim-air-script` route completes right and left aimed launches, live diagonal
  aim changes, definition fallbacks, `$E2/$E4/$E5` landings, and their `$F8` returns while
  retaining the ordinary collision, camera, minimap, terrain, and ROM-authored animation paths.
- The real-ROM `--compact-air-script` route observes `$69 -> $17 -> $69 -> $17 -> $A4`,
  turns, then mirrors `$6A -> $18 -> $6A -> $18 -> $A5`. Its milestone assertions use the
  actual post-frame poses, and its PNGs retain ROM terrain, sky, HUD, tiles, and OAM.
- The real-ROM `--aim-crouch-script` route completes `$01 -> $F3 -> $71 -> $73 -> $85 -> $27
  -> $F9 -> $05`, turns left, then mirrors through `$F4/$72/$74/$86/$28/$FC/$08`. Radius
  changes keep the feet fixed and the active-pose diagnostic retains terrain/HUD alignment.
- The real-ROM `--aim-turn-script` route observes generic `$25/$26` prospective poses become
  `$9C/$9D`, `$8B/$8C`, and `$8D/$8E` from the source pose metadata. Every pair completes
  through its exact `$F8` standing-aim destination while preserving old-direction momentum.
- The real-ROM `--crouch-turn-script` route observes `$43/$44` prospective poses become all
  eight ordinary/aimed crouched turns. It completes every `$F8` target while retaining the
  16-pixel collision radius, grounded probe, old-direction momentum, and live rendering.
- The real-ROM `--crouch-jump-script` observes both direct `$27 -> $01` exits, ordinary
  `$27 -> $4B -> $4D -> $A4`, and aimed `$71 -> $4B -> $4D/$69 -> $51 -> $A4`. Its trace
  records the distinct `$04B3` ordinary and `$04BD` aimed launch centers, authentic 4.E000
  velocity, ceiling/floor collision, landings, camera, minimap, and ROM-authored graphics.
- The real-ROM `--morph-ball-script` observes `$01 -> $35 -> $27 -> $37 -> $1D -> $1E`,
  reverses through `$1F`, decelerates to `$41`, and completes `$3E -> $28`. Its required
  milestones, animation definitions, terrain collision, slope alignment, camera, HUD, OAM,
  and generated PNGs all come from the private retail image. Synthetic table fixtures also
  cover `$F9`'s airborne target, walk-off, both hard-bounce stages, gentle landing, successful
  unmorph expansion, and the boxed-tunnel retention branch.
- The real-ROM `--spring-ball-script` observes `$37 -> $79 -> $7B -> $7F`, a ROM-authored
  powered arc, ceiling collision, automatic bounce, and `$79` recovery. Its 80-frame pose
  capture freezes the airborne ball against cartridge-derived terrain and live camera state.
- The real-ROM `--bomb-jump-script` observes `$37 -> $1D`, presses Shoot/X once, allocates
  slot zero, reaches bank-$A0's straight timer-eight overlap at accepted NMI 77, installs
  `$90:E025` on 78, begins displacement on 79, starts explosion art on 85, deletes the bomb
  on 95, ends the special handler on 105, and recovers `$31 -> $1D` on floor collision at
  133. Captures show real OBJ tile `$14C` for the bomb and `$18B` for the explosion against
  cartridge-derived terrain; no projectile direction or velocity is host-injected.
- The real-ROM `--knockback-script` injects bank `$A0` X direction one at the documented
  enemy-collision seam, executes `$53`, matches cartridge record `$91:A8E4` with held
  `$0280`, enters `$50`, runs type `$19` through ceiling/floor collision, and lands through
  `$FF -> $A5`. It also requires three hurt and three suit palette writes, compares all
  sixteen live CGRAM colors on each call with the private ROM, and observes impact SFX `$35`.
  A 21-frame capture freezes the white `$9B:A380` silhouette; a 32-frame capture freezes
  authentic damage-boost art and split-body OAM.
- The real-ROM `--ran-into-wall-script` chooses only a Landing Site test coordinate, then
  observes `$89/$CF/$D1`, definition fallback, and `$4B/$4D` from retail input tables and a
  blocked type-`$8` collision probe. The selected ROM corner is column `$10`, floor row
  `$20`; Samus remains at last-safe center X `$00FB` during every rejected +1.0000 move.
- The real-ROM `--grapple-script` publishes only an accepted anchor, then collides on frame
  one against Landing Site terrain at radial point 6/6. It records safe angle `$4080`,
  reflected velocity `$FF2E`, and kick timer 16 before releasing through `$B2 -> $52`.
  The two-frame object capture visibly retains the cartridge-selected beam at that reflection.
- The real-ROM `--grapple-fire-script` selects grapple at the documented HUD seam, reads pose
  `$01`'s direction and every velocity/origin/tile pointer from the cartridge, renders the
  growing horizontal beam, and completes its queued no-target cancellation. A room-wide
  collision scan confirms Landing Site contains no type-`$E` grapple blocks.
- The real-ROM `--charge-beam-script` reaches counter 65 through controller samples, draws
  the ROM-authored main flare and sparks, then releases a type-`$9010`, damage-`$003C` shot
  with sound `$17`. The 50-frame capture visibly retains the cannon flare; the 68-frame
  regression requires charged-family allocation and nonzero bank-$93 art; a 71+ frame run
  also requires the native charged-trail allocation and the 72-frame PNG captures it.
- The real-ROM `--missile-script` fires one type-`$8100`, damage-`$0064` missile with sound
  `$03`, decrements ten rounds to nine, decodes its live bank-$93 spritemap, and allocates the
  bank-$90 exhaust by frame seven. The same run now observes arm-cannon cover frames 0/1/2/3,
  independently checks the pose-selected direction, OAM word, bank-$9A tile pointer, and
  `$20`-byte VRAM `$61F0` transfer against the cartridge, and requires a visible cover OBJ.
  `MissileArmCannon.png` and its three diagnostic layers capture the fully open cover, live
  projectile, and detached trail over cartridge-derived Landing Site terrain.
- The real-ROM `--visor-script` supplies only room-owned layer-blending configuration `$28`,
  then observes packed `$0A72/$0A73` select offsets 6/8/10 at five-call cadence. Every live
  CGRAM color-196 write is independently compared with `$9B:A3C0`; normal rooms reset to
  `$0601`, and X-ray special palette type eight freezes the packed state without overwriting it.
- The real-ROM `--draygon-grab-script` publishes a fixed owner coordinate at `$A5:94A9`, then
  observes every right-facing grabbed pose, the moving-body animation loop, `$F0->$EC`
  fallback, 60-pattern escape, owner signal, and `$01` release. Its assertions sample actual
  post-frame state, while its PNG uses private-ROM tiles, palettes, spritemaps, BG1, and BG2.

## Next implementation order

The Baby release, corpse-row, escape-door burst, terminal-fragment, and bomb-collision dust
producers now allocate concrete `$86:E509` slots in native scheduler order. The unresolved
projectile effects are Mother Brain's distinct death-explosion definition, the natural-bomb
Ridley-afterburn-first chain, and the typewriter glyph family.

1. Wire translated Hyper Beam enemy hits into `$B562` recoil/health and the earlier attack-
   selection trigger that enters `$B8EB`, then translate the Mother Brain death explosion, natural-bomb
   afterburn chain, and typewriter glyph producers.
2. Live room-enemy loading/updates so translated solid collision and shake words have real actors.
3. Enemy touch/damage producers so knockback and grapple acquisition begin from live actors.
4. Translate the remaining projectile door/bombable/special block reaction branches;
   ordinary beams, Wave, missiles, and linked Super Missiles now
   execute the complete type-`$4/$C` shootable table and its real bank-$84 lists.
