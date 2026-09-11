# Super Missile enemy detachment (#563)

Affected reported version: 0.1.1. Status: shared crawler omission fixed and
representative native/projectile checks complete; awaiting player validation.
The sections below preserve the sequence of findings; the final section records
the current acceptance result rather than the earlier partial-work status.

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

## Species coverage and isolated contact assertion

The controller-impact fixture now covers Sciser, Zero, Viola, Zeela, Sova, Zoomer,
Stone Zoomer and HZoomer, each with normal and Super Missile controls, plus the two
frozen Zoomer controls: **18 cases / 1260 runtime frames**. Every unfrozen species
detaches on the Super impact frame and matches the first eight falling positions;
all normal-Missile and frozen controls remain attached. No enemy takes direct damage.
This uses constructed ceiling placement, not a claim that every species has a retail
ceiling placement or can be frozen. HZoomer uses its own attached function rather
than being forced through the ordinary crawler function.

The earlier Pre-Bowling contact failure is now corrected in the test: direct normal
touch must publish five damage, immunity 96, knockback timer five and direction one
for equal X; it must not immediately install active knockback. Native A0:A4A1 and the
production common contact owner both publish that pending request for the later
movement phase. The full Pre-Bowling audit now passes, including homing animation,
quake detachment and contact. No production contact behavior was changed.

The existing Aqueduct Yard audit also passes its hiding, release, beam launch,
gravity and qualifying-quake drop checks. A native comparison of Yard's separate
behavior gates (especially already-airborne states 3/4/5) remains outstanding; the
shared crawler comparison does not cover it. #563 is therefore still open, not
awaiting validation.

## Original-CPU Yard state comparison

The headless probe now also executes the original ROM's A3:CE64 Yard main.
Six behavior states, three earthquake controls (type20/timer30, timer29, type18),
two facing values, and eight frames produce **36 setups / 288 comparisons**.
All match the production enemy dispatcher: behavior and function selection,
instruction-list selection, position/subposition, and both 16.16 velocity words.
States 0/1/2 enter the earthquake fall; states 3/4/5 preserve their existing
airborne motion rather than restarting it. The two wrong-quake controls do not
detach. Nonzero initial velocities make an accidental reset observable.

This uses a constructed empty room with Samus far away, no instruction animation,
and controlled state fields. It isolates the quake gate and airborne owner; it is
not a controller replay of a retail Yard encounter or a native landing comparison.
No additional production change was needed. The native hook was removed and the
normal executable rebuilt after capture; no GUI was launched.

Regenerate with the same temporary patch/build/reverse workflow above:

```
upstream-sm/build/bin-x64-Release/sm.exe --diagnostic-yard-quake "Super Metroid.smc" NEW.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --yard-quake-native-audit "Super Metroid.smc" NEW.csv
```

Accepted CSV SHA256:
`FD56C7E6CF84C58FC6913DF14D7D7A6700817AAAE04111657F0FBEC143271581`.
Generated traces remain private in ignored test-temp. Remaining acceptance work
includes the Yard projectile-to-AI handoff and time-freeze suppression; do not
interpret this isolated comparison as completion of those integration checks.

## Final integration and acceptance

`--missile-yard-detach-audit ROM` executes controller firing, projectile movement,
remote wall impact, and Yard AI together. The normal Missile impacts at frame 19
without detachment; the Super Missile impacts and detaches Yard at frame 11.
Every position/subposition across both 25-frame runs is asserted. Yard immediately
executes its newly selected airborne owner (zero initial displacement, then one
eighth-pixel acceleration), unlike the shared crawler's next-call handoff. Health
and X remain unchanged, excluding a direct hit or kick as an alternate trigger.
This controlled fixture disables animation-driven crawling to isolate the event.

The Yard native-comparison test now also inserts five frozen-game-time frames
before every setup (180 frames total). Position, velocity, behavior, lists and the
pending quake remain unchanged. Resuming then matches all 288 original-CPU rows.
Native A0 enemy dispatch suppresses normal AI/instructions in frozen game time;
the earthquake consumer also stops. This is separate from the previously tested
Ice-frozen Zoomer AI controls.

The bank-A2..AD source scan identifies the shared crawler, HZoomer and Yard gates
as the enemy detach consumers of type20/timer30. Representative original-CPU
trajectories, all eight crawler wrappers' controller-to-impact routes, Yard's
distinct handoff/state controls, regular-Missile controls and freeze controls now
cover the request. This does not claim every room placement or every enemy should
fall. The only diagnosed gameplay mismatch was the shared attached-crawler gates,
fixed in 788cb9a2. Subsequent commits add verification, not alternate mechanics.
Keep #563 open for player confirmation.
