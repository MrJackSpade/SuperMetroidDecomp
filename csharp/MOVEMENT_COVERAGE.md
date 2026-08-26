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
| `$00` | Standing | `$01/$02`, landing `$A4-$A7` | Aim/fire variants, forward pose, transitions from later systems |
| `$01` | Running | `$09/$0A`, dry air, no run button | Run button, speed booster, liquid/environment effects, aim/fire variants |
| `$02` | Normal jumping | `$4B-$4E`, dry air, variable height, ceiling/floor collision | Aimed jump poses, equipment/liquids, external displacement |
| `$03` | Spin jumping | `$19/$1A`, dry air, variable height, split-body animation | Wall-jump trigger, space jump, screw attack, equipment/liquids |
| `$04` | Morph ball on ground | — | Entire family |
| `$05` | Crouching | — | Entire family and stand/crouch alignment |
| `$06` | Falling | `$29/$2A`, walk-off, dry-air gravity and landing | Aimed falling poses, equipment/liquids, aerial turn transitions |
| `$07` | Unused | — | Preserve only if an exhaustive compatibility route needs it |
| `$08` | Morph ball falling | — | Entire family and bounce state |
| `$09` | Unused | — | Preserve only if required |
| `$0A` | Knockback / crystal-flash ending | — | Entire family |
| `$0B` | Unused | — | Preserve only if required |
| `$0C` | Unused | — | Preserve only if required |
| `$0D` | Unused | — | Preserve only if required |
| `$0E` | Turning on ground | `$25/$26`, old-direction mode-one momentum, `$F8` completion | Aim/fire variants and transitions originating in later families |
| `$0F` | Crouch/stand/morph transition | — | Entire family, radius collision, animation commands |
| `$10` | Moonwalking | — | Entire family |
| `$11` | Spring ball on ground | — | Entire family |
| `$12` | Spring ball in air | — | Entire family |
| `$13` | Spring ball falling | — | Entire family |
| `$14` | Wall jumping | — | Trigger check, launch physics, animation, landing |
| `$15` | Ran into a wall | — | Entire family |
| `$16` | Grappling | — | Swing, stuck, release, wall-jump seams |
| `$17` | Turning while jumping | — | Entire family |
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

## Evidence

- `SuperMetroid.Verification` checks synthetic ROM tables independently for `$FD/$F8`, jump
  constants, gravity ordering, jump cutoff, mirrored spin movement, walk-off falling, solid
  collision, radius alignment, and landing.
- The real-ROM `--jump-script` route currently completes through `$01 -> $4B -> $4D -> $A4
  -> $01 -> $09 -> $19 -> $A6 -> $09 -> $01`, with ROM-authored graphics/tile transfers and
  moving camera/background/minimap state.

## Next implementation order

1. Crouch/stand transitions and aimed ordinary poses.
2. Morph ball ground/fall/bounce, bombs, and spring ball.
3. Aerial turns and the real wall-jump trigger/launch.
4. Knockback and damage boost.
5. Grapple movement and release routes.
6. Speed booster/shinespark, crystal flash/drained, and remaining scripted movement.
