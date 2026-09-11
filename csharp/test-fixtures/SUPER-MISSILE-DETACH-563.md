# Super Missile enemy detachment (#563)

Affected reported version: 0.1.1. Status: shared crawler omission fixed;
broader native/projectile integration investigation still open, not awaiting validation.

## Cartridge trigger and omission

Pinned `sm_a3.c` contains exact `earthquake_timer == 30 && earthquake_type == 20`
checks in MaridiaSnail_Func_4 (Yard, A3:CE73), both HZoomer attached functions
(A3:E091/E168), and both shared crawler attached functions (A3:E6C8/E7F2).
The port already checks Yard and HZoomer, but omitted both shared crawler checks.
This is not a rule that every ceiling enemy falls, nor a reaction to simply firing.
The exact type/timer gate distinguishes this event from the type-18 enemy-hit quake.

The missing checks now use the shared existing fall helper and named projectile
quake definitions. As in the cartridge, the current attached movement call continues
after setting the next function. The falling displacement begins on the next AI call.
No extra velocity reset, instantaneous drop, or early return was added.

## Reproduction and regression

Run `dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --
--shared-crawler-audit "Super Metroid.smc"` on one command line.

After the existing unchanged retail-population animation checks, the fixture builds
a controlled ceiling/wall/floor in the loaded level and places the selected retail
crawler against it. Other actors are disabled. Tangential velocity is zero and the
instruction timer is held to isolate AI movement from animation-driven orientation
changes. This prevents an outside-corner fall from falsely passing the quake test.

Before the production fix, Sciser remains CrawlingHorizontally instead of Falling
on the type-20/timer-30 ceiling test. Afterward Sciser, Viola, Zeela and Sova pass:

- ceiling and vertical-wall detachment with the saved attached return function;
- eight subsequent exact 16.16 Y positions with half-pixel acceleration;
- timer-29 and type-18 controls remaining attached.

These are 24 admission cases and 64 falling-frame position comparisons through the
ordinary enemy dispatcher. Earthquake words are injected at the AI boundary: this
does **not** yet verify projectile firing/impact timing, the native CPU executing the
same room fixture, freezing/time-freeze suppression, all species, or Yard behavior.
Those acceptance items remain on #563. No issue closure or validation label is justified
by this scoped fix alone. No ROM, state, screenshots or generated assets are published.

Standard verification and Windows build pass (zero warnings/errors). The separate
Pre-Bowling HZoomer audit reaches/passes its quake check but fails its later contact
assertion: health=94, KnockbackActive=false. Removing the new shared crawler checks
produces the identical failure. HZoomer's helper behavior is unchanged; this older
audit expects immediate active knockback although normal contact now publishes a
pending timer for the runtime's later movement owner. This turn does not change that
assertion or claim the complete HZoomer audit passed.

## Controller-to-impact runtime integration

`--missile-crawler-detach-audit ROM` runs four 70-frame cases through the full
runtime with cheats off: regular/Super Missile crossed with frozen/unfrozen Zoomer.
A controller Shoot pulse launches toward a constructed right wall; the crawler is
behind Samus and attached to a ceiling above the projectile trajectory. Its health
must remain unchanged, ruling out a direct hit as the reason for detachment.

Regular Missile impact occurs at fixture frame 19 and never detaches the crawler.
Super Missile impact occurs at frame 11 and installs Falling on that same frame;
the following eight frames match the exact half-pixel-acceleration trajectory.
The crawler subsequently reaches the floor (Y120 -> Y248). Both frozen controls
retain Y120 without detaching. Ice stays equipped in all cases; the native frozen
handler would otherwise thaw immediately. Frozen AI bit and timer are constructed
starting conditions, not a claim that the fixture fired an Ice shot.

The same-frame transition is supported by native GameState_8_MainGameplay ordering:
HandleControllerInputForGamePhysics executes alpha and HandleProjectile before
EnemyMain. The falling handler first executes on the following AI frame. An initial
test expectation of next-frame *state selection* was incorrect and was corrected
after checking that source ordering, without changing production timing.

This establishes the managed controller/projectile/wall/quake/enemy route and one
frozen-state control. The original-CPU comparison and broader family/state acceptance
remain open. No additional gameplay fix was required in this integration step.

## Original-CPU shared crawler comparison

`movement-release/native-crawler-quake-probe.h` runs the unmodified ROM's A3:E6C2
shared main routine on the 65816 emulator, not the translated C routine. Six cases
(ceiling/wall crossed with valid, timer-29 and type-18 quakes) each run forty calls.
The constructed 16x16 room has square ceiling, wall and floor. Both implementations
use radius eight, matching positions/velocities, and no instruction animation; this
isolates the movement owner while the runtime test above covers its integration.

All **240 frames** match X/Y positions and subpositions, current/saved functions,
attached velocities, and falling velocity/subvelocity. The comparison continues
through floor collision: Y becomes 184.FFFF, falling velocity clears, and the
saved attached function resumes. No new production change was needed.

Pins rechecked: native source `578f90b3cc49557bb70060ad033bb90b8cf8ac50`,
disassembly `362be646929cf8e483f692b73a6561cfc2dc1d0d`, ROM SHA256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Accepted CSV SHA256:
`C73CA9BC272FE6C8383526213705E03146F3EF64456555BBDBAA1D3B5FA51C9D`.

To regenerate, apply `native-crawler-quake-entrypoint.patch` to `upstream-sm`, build
its Release x64 executable with the local toolset, and run the headless command:

```
upstream-sm/build/bin-x64-Release/sm.exe --diagnostic-crawler-quake "Super Metroid.smc" NEW.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --crawler-quake-native-audit "Super Metroid.smc" NEW.csv
```

The output must not already exist. Reverse the entrypoint patch and rebuild the
normal executable afterward; that cleanup was completed for this run. Output stays
ignored under test-temp. No native GUI was launched. No ROM, screenshots or state
fixture is added. Yard's distinct state gates and remaining species-specific scope
are not established by this shared-routine comparison; #563 remains open.
