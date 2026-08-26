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
| `$01` | Running | `$09/$0A/$0D-$12`, dry air, no run button | Run button, speed booster, liquid/environment effects, gun-extended/fire variants |
| `$02` | Normal jumping | `$4B-$4E/$15-$18/$51-$52/$55-$5A/$69-$6C`, including compact straight-down collision changes, dry air, variable height, ceiling/floor collision | Equipment/liquids, external displacement |
| `$03` | Spin jumping | `$19/$1A`, dry air, variable height, split-body animation | Wall-jump trigger, space jump, screw attack, equipment/liquids |
| `$04` | Morph ball on ground | — | Entire family |
| `$05` | Crouching | `$27/$28/$71-$74/$85/$86`, grounded probe, aim fallback, momentum clear, direct `$01/$02` exits, `$4B/$4C` crouch-jump entry | Fire variants and morph entry |
| `$06` | Falling | `$29-$2E/$6D-$70`, including compact straight-down collision changes, walk-off, dry-air gravity and landing | Equipment/liquids, aerial turn transitions |
| `$07` | Unused | — | Preserve only if an exhaustive compatibility route needs it |
| `$08` | Morph ball falling | — | Entire family and bounce state |
| `$09` | Unused | — | Preserve only if required |
| `$0A` | Knockback / crystal-flash ending | — | Entire family |
| `$0B` | Unused | — | Preserve only if required |
| `$0C` | Unused | — | Preserve only if required |
| `$0D` | Unused | — | Preserve only if required |
| `$0E` | Turning on ground | `$25/$26/$43/$44/$8B-$8E/$9C/$9D`, old-direction mode-one momentum, native standing/crouch selector, `$F8` completion | Fire variants and transitions originating in later families |
| `$0F` | Crouch/stand/morph transition | `$35/$36/$3B/$3C/$F1-$FC`, bottom alignment, radius collision, `$FD` completion | Morph transitions and fire variants |
| `$10` | Moonwalking | — | Entire family |
| `$11` | Spring ball on ground | — | Entire family |
| `$12` | Spring ball in air | — | Entire family |
| `$13` | Spring ball falling | — | Entire family |
| `$14` | Wall jumping | — | Trigger check, launch physics, animation, landing |
| `$15` | Ran into a wall | — | Entire family |
| `$16` | Grappling | — | Swing, stuck, release, wall-jump seams |
| `$17` | Turning while jumping | Grounded-Y branch for crouched aim turns `$97-$9A/$A2/$A3` | Actual jumping turns `$2F/$30/$8F-$92/$9E/$9F`, airborne gravity/collision seams |
| `$18` | Turning while falling | — | Entire family |
| `$19` | Damage boost | — | Entire family |
| `$1A` | Grabbed by Draygon | — | Entire family |
| `$1B` | Shinespark / crystal flash / drained / Mother Brain damage | — | All subhandlers and scripted state |

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

## Evidence

- `SuperMetroid.Verification` checks synthetic ROM tables independently for `$FD/$F8`, jump
  constants, gravity ordering, jump cutoff, mirrored spin movement, walk-off falling, solid
  collision, radius alignment, and landing.
- The real-ROM `--jump-script` route currently completes through `$01 -> $4B -> $4D -> $A4
  -> $01 -> $09 -> $19 -> $A6 -> $09 -> $01`, with ROM-authored graphics/tile transfers and
  moving camera/background/minimap state.
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

## Next implementation order

1. Morph ball ground/fall/bounce, bombs, and spring ball.
2. Aerial turns and the real wall-jump trigger/launch.
3. Knockback and damage boost.
4. Grapple movement and release routes.
5. Speed booster/shinespark, crystal flash/drained, and remaining scripted movement.
