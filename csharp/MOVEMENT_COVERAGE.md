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
| `$00` | Standing | `$01-$08`, landing `$A4-$A7/$E0-$E5` | Firing variants, forward pose, transitions from later systems |
| `$01` | Running | `$09/$0A/$0D-$12`, dry air, ordinary Dash/B 2.0000 cap, equipped Speed Booster stages and 7.0000 cap | Speed-echo drawing/palette copy, liquid/environment effects, gun-extended/fire variants |
| `$02` | Normal jumping | `$4B-$4E/$15-$18/$51-$52/$55-$5A/$69-$6C`, including compact straight-down collision changes, dry air, variable height, ceiling/floor collision | Equipment/liquids, external displacement |
| `$03` | Spin jumping | `$19/$1A`, dry air, variable height, split-body animation, block-wall contact/launch | Solid-enemy wall contact, space/screw spin poses, liquids |
| `$04` | Morph ball on ground | `$1D/$1E/$1F/$41`, dry ground, slopes, reversal, deceleration, walk-off, normal-bomb deployment | Bombable-block PLMs, liquids, enemy collision, external displacement |
| `$05` | Crouching | `$27/$28/$71-$74/$85/$86`, grounded probe, aim fallback, momentum clear, direct `$01/$02` exits, `$4B/$4C` crouch-jump entry | Fire variants and morph entry |
| `$06` | Falling | `$29-$2E/$6D-$70`, including compact straight-down collision changes, walk-off, dry-air gravity, landing, and aerial-turn entry | Equipment/liquids |
| `$07` | Unused | — | Preserve only if an exhaustive compatibility route needs it |
| `$08` | Morph ball falling | `$31/$32`, dry-air gravity, ceiling/floor collision, two-stage hard bounce, gentle landing, normal-bomb deployment | Bombable-block PLMs, liquids, enemy collision, external displacement |
| `$09` | Unused | — | Preserve only if required |
| `$0A` | Knockback / crystal-flash ending | `$53/$54`, dry-air ordinary-body timer, horizontal/vertical block collision, damage-boost input escape, falling handoff | Morph/ball knockback, liquids, enemy producer, crystal-flash ending |
| `$0B` | Unused | — | Preserve only if required |
| `$0C` | Unused | — | Preserve only if required |
| `$0D` | Unused | — | Preserve only if required |
| `$0E` | Turning on ground | `$25/$26/$43/$44/$8B-$8E/$9C/$9D/$BF-$C4`, old-direction mode-one momentum, native standing/crouch/moonwalk selectors, `$F8` completion | Fire variants and transitions originating in later families |
| `$0F` | Crouch/stand/morph transition | `$35/$36/$3B/$3C/$37/$38/$3D/$3E/$F1-$FC`, bottom alignment, radius collision, `$F9/$FD` completion | Fire variants and later equipment-dependent transitions |
| `$10` | Moonwalking | `$49/$4A/$75-$78`, option gate, reversed X input, aim changes, fallback, walk-off, and `$BF-$C4` jump bridge | Equipment/liquids and external displacement |
| `$11` | Spring ball on ground | `$79-$7C`, dry ground, slopes, reversal, jump entry, walk-off, normal-bomb deployment | Bombable-block PLMs, liquids, enemy collision, external displacement |
| `$12` | Spring ball in air | `$7F/$80`, dry-air powered jump, variable height, ceiling/floor collision, normal-bomb deployment | Bombable-block PLMs, liquids, enemy collision, external displacement |
| `$13` | Spring ball falling | `$7D/$7E`, dry-air gravity, held-jump relaunch, automatic bounce, normal-bomb deployment | Bombable-block PLMs, liquids, enemy collision, external displacement |
| `$14` | Wall jumping | `$83/$84`, dry/Hi-Jump launch tables, variable height, `$FB` animation family, spin handoff, landing | Liquids, solid-enemy trigger, sound/contact-damage side effects |
| `$15` | Ran into a wall | `$89/$8A/$CF-$D2`, block-backed prospective-run selector, one-pixel probe, aim/fallback/turn/jump/walk-off routes, grounded cleanup | Solid-enemy probe, liquids and external displacement |
| `$16` | Grappling | ROM-backed firing, four-step block collision, persistent type-`$E` acquisition/validation, all 30 standing/crouching/vertical connection records, `$B2/$B3` pendulum, `$A8-$AB/$B4-$B7` locked poses, per-pixel rope collision, six-point terrain sweep/reflection, collision kick, all eight exact locked/wallgrab angles, `$B8/$B9` grace-window wall jump, dropped-pose tables, release `$51/$52`, ROM art/beam DMA and OAM | Enemy acquisition, grapple spike-damage side effects, breakable PLMs, liquid/solid-enemy wall-jump branches |
| `$17` | Turning while jumping | Grounded-Y crouch turns `$97-$9A/$A2/$A3`; airborne `$2F/$30/$8F-$92/$9E/$9F`, momentum, collision, `$F8` | Later firing/external-displacement routes |
| `$18` | Turning while falling | `$87/$88/$93-$96/$A0/$A1`, momentum, gravity/collision, `$F8` | Later firing/external-displacement routes |
| `$19` | Damage boost | `$4F/$50`, fresh dry-air jump, type-indexed X physics, variable height, ceiling/floor collision, `$FF` sentinel landing | Liquids, external displacement, enemy producer |
| `$1A` | Grabbed by Draygon | — | Entire family |
| `$1B` | Shinespark / crystal flash / drained / Mother Brain damage | — | All subhandlers and scripted state |

The bomb-jump movement handler is installed outside this normal dispatcher. `$90:E025`
performs its one-frame initialization and `$90:E032` owns the rising special arc; its
translated status is documented with the Morph Ball family below.

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
- Synthetic verification uses distinct countdowns/delay lists for all five stages and locks
  cap, palette state, stage-four events, and jump arithmetic. Real-ROM
  `--speed-booster-script` reaches stage four, echo/contact state, 7.0000, and a boosted
  `$09 -> $19` launch with initial vertical speed 7.C400.
- Suit-palette copying, rendered trailing echoes, stored shine/crouch release, shinespark,
  and the Landing Site type-`$F` door dispatcher remain explicit follow-up boundaries.

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
  fresh dry-air spin-jump launch.
- The viewer exposes **Moonwalk enabled** and uses the real Shoot-plus-backward controller
  chord. Synthetic verification exhausts all six stable and all six jump-turn routes; the
  private-ROM `--moonwalk-script` proves `$01->$4A->$76->$78->$07->$01->$4A->$BF->$1A`.

## Verified ran-into-wall slice

- `$91:EADE` first consumes the killed-X-speed result from current movement type one. If
  that branch is absent, only a prospective type-one pose triggers the forward block probe.
  The untranslated solid-enemy probe remains explicit instead of being approximated.
- The block probe moves exactly one pixel in the CURRENT pose direction. Collision retains
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
- The bank-$91 collision command installs `$83/$84`, clears old momentum, and reads dry-air
  ordinary or Hi-Jump launch values directly from `$90:9ED1-$90:9EE3`. Movement type `$14`
  reuses ordinary jumping physics. Animation command `$FB` selects ordinary, Space Jump, or
  Screw Attack frame ranges in cartridge priority order; the wall-specific bottom selector
  draws frames below three and at/above thirteen. Landing uses `$A6/$A7`.
- Synthetic verification exhausts all twenty selector-table entries, compact expansion,
  mode-one displacement, `$F8`, early wall rewind, the seven-pixel launch, trigger-frame Y
  suppression, `$FB`, and the 4.A000 dry wall arc. Solid-enemy wall jumps remain explicit.

## Verified knockback and damage-boost slice

- The untranslated enemy system is represented by one explicit producer seam: a caller
  supplies bank `$A0`'s left/right hit result. `$91:EDB0` then chooses knockback direction,
  installs `$53/$54`, reads dry-air 5.0000 from `$90:9EE9/$90:9EEF`, and starts the native
  five-count hurt timer. No enemy damage, velocity, pose, or duration is fabricated.
- The special `$90:DF38` handler takes precedence over the normal type dispatcher. It uses
  type `$0A`'s normal-air speed record and bank-$A0's X direction, applies either gravity or
  no-speed downward movement according to directions one/two/four/five, and performs exact
  bank-$94 block clipping. Vertical collision clears the same X/Y state as `$90:DF6E`.
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

## Verified grapple firing and connected slice

- `$9B:C51E/$C703` read shot direction, signed extension velocities, fire angle, movement-
  specific hand/flare origins, and pose graphics Y correction directly from ROM. Length grows
  by twelve and cancels before collision at 128; release of Shoot queues the same cancel path.
- `$94:A85B` advances two signed 16.16 endpoint offsets in four collision substeps per frame.
  The grapple dispatcher handles air, slope/solid cancellation, horizontal/vertical BTS
  extensions, and persistent type-`$E` BTS zero/three's carry+overflow connection result.
  Breakable BTS one/two and other PLM-producing types stop explicitly until bank `$84` exists.
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
- `$94:A957` applies the direction-sensitive anchor low nibble (7 or 8) before both collision
  and drawing. Air, solid, slope, extension, and spike collision carry semantics follow the
  grapple-specific dispatcher; spike damage accumulation remains a separate gap.
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
  clears grapple/momentum, and reuses the ROM dry/Hi-Jump wall-launch table and existing `$FB`
  animation-command handoff. Liquid and solid-enemy branches remain explicit gaps.
- `$9B:BFA5` queues the two bank-`$9A` tile transfers selected through `$9B:C342/$C346` to
  VRAM `$6200/$6210`. `$94:AF87/$AFBA` retain all sixteen staggered segment instruction
  slots, emit small OAM tiles `$21-$24`, and draw endpoint tile `$20` with native flip bits.
- Releasing Shoot runs `$9B:CA65` immediately, including signed sine products and acceleration
  mode two, then preserves the one-frame function-pointer seam before `$9B:CB8B` installs
  `$51/$52`. Ordinary type-two movement, camera, collision, landing, and art own later frames.
- Synthetic verification fixes ROM table addresses, all thirty connection table routes,
  distinct rope Start/Flare geometry, shared speed cleanup/camera clamp, exact pendulum
  numbers, two VRAM queue records, four-step firing accumulation, persistent-block centering,
  integer connection angle, anchor-side bias, per-pixel rope obstruction, six-point angular collision/reflection,
  collision kick, anchor disconnection, all 30 wall-grace checks, exact `$B9 -> $83` reversal,
  locked fallback, compact dropped-table selection, camera-history clamp, six staggered rope
  OAM records, endpoint placement, release velocity, and queued handoff. `--grapple-fire-script`
  proves real-ROM firing and cancellation; `--grapple-script` proves live terrain reflection
  before its release handoff.

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
- Movement type `$08` copies old 16.16 vertical speed before gravity exactly like the native
  dry-air path. A hard floor impact enters bounce state one; the next impact enters state two
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
  `$79-$7C` use movement type `$11`'s literal speed record; Jump selects `$7F/$80` and
  initializes the ROM 4.E000 dry-air launch. Type `$12` retains the normal jump cutoff,
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
  literal dry-air `$90:9EF5/$90:9EFB` launch pair without moving on its start frame.
  `$90:E032` uses the standalone `$90:9F25` speed record for left/right displacement,
  performs the old-speed-before-gravity vertical pass, and ends on upward collision or
  signed-speed apex. Diagonal apex selects acceleration mode two; the preserved current
  ball pose then owns falling and ordinary two-stage bounce recovery. `$94:9CF4`'s
  center/up/right/left/down cross is complete for no-op air/slope/solid block families;
  shootable/special/bombable/extension reactions throw until their bank-$84 PLMs exist.

## Evidence

- `SuperMetroid.Verification` checks synthetic ROM tables independently for `$FD/$F8`, jump
  constants, gravity ordering, jump cutoff, mirrored spin movement, walk-off falling, solid
  collision, radius alignment, and landing.
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
  `$FF -> $A5`. A 32-frame capture freezes authentic damage-boost art and split-body OAM.
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

## Next implementation order

1. Grapple breakable PLMs, spike damage, and liquid/solid-enemy wall-jump branches.
2. Speed Booster palette/echo rendering, stored shine/shinespark, crystal flash/drained, and remaining scripted movement.
3. Space-jump/Screw-Attack spin families, liquids, and solid-enemy collision routes.
4. Enemy collision/damage producers so knockback and grapple begin from live actors instead of host seams.
5. Return with bank-$84 PLMs to make bombable terrain mutate instead of stopping explicitly.
