# #402 X-Plasma parity

## Current remaining scope

Full normal-input original-CPU trajectories now match for steel Pirates and
Botwoon (charged Plasma and Hyper). Phantoon and Draygon have native contact,
freeze/release, and full-runtime boundary coverage, but still need the complete
normal-firing/X-ray controller encounter comparisons. Phantoon's primer must
remain part of that encounter. The chronological sections below retain earlier
limitations; later evidence supersedes only the explicitly covered cases.

## Reproduced shared dispatcher defect

The focused `--x-plasma-timers` Verification fixture loads an ordinary Ripper
through the production room-enemy loader, then processes it with time frozen.
Before the fix, the first frame retained invincibility 10 instead of decrementing
to 9. Movement, instruction position, flash and Ice-freeze timers are asserted
independently throughout twelve calls. The test does not inject a timer reset on
X-ray activation or pretend an enemy is a boss.

Pinned `upstream-disassembly/src/bank_A0.asm`, EnemyMain $A0:9021-$902E,
decrements a tangible enemy's invincibility before its time-freeze branch.
The pinned C translation in `upstream-sm/src/sm_a0.c`, EnemyMain, agrees.
The port instead decremented at the end and only when time was not frozen.
Production now decrements before actor work, respects intangible property $0400,
and retains the entry invincibility value for the collision gate. The call which
decrements one to zero still skips collision; the following call admits contact.
Tests cover both this edge and intangible actors retaining their timer.

## Verification and limits

Run the standard Verification suite or its `--x-plasma-timers` selector.
The counter assertion failed before production changes and passes afterward.
The full suite passes with the shared dispatcher correction. The additional
expiration/contact boundary assertions pass in the focused selector.

## Original-CPU counter comparison

`movement-release/native-xplasma-timer-probe.h` runs original cartridge
EnemyMain ($A0:8FD4) through the existing bounded 65C816 harness. Its headless
entrypoint patch suppresses dialogs and runs before SDL initialization. The
loader restores original ROM bytes after harness initialization. No translated
C EnemyMain call is used as the oracle.

Ten cases cover tangible/intangible actors with starting invincibility
0, 1, 2, 10 and 65535; twelve consecutive calls per case produce 120 records.
Flash, Ice freeze, X and instruction position are compared as well as the
invincibility countdown. All 120 records match the production dispatcher:

```text
sm.exe --diagnostic-xplasma-timers "Super Metroid.smc" <new-private-output.csv>
Verification --native-x-plasma-timers <new-private-output.csv>
```

The native actor is an invisible, active-offscreen Ripper with time frozen,
not a substituted boss. The private trace is not committed. After capture the
temporary patch was reversed, unrelated native edits retained, and the normal
native executable rebuilt. The reusable probe and comparator are tracked.

This establishes a required timing mechanism, not completion of #402.

## Botwoon retained-shot prerequisite

`DebugRunner --botwoon-x-plasma "Super Metroid.smc"` loads the real Botwoon
population and waits for its authored shootable head. A constructed charged
Plasma shot (100 damage, chosen to keep this test nonlethal) overlaps the head.
Before the second fix, ordinary shot collision changed type `$8018` into
explosion `$8718` and damage 100 into 8. No retained shot remained for X-Plasma.

Pinned `sm_a0.c` ordinary and multibox collision preludes mark direction bit
$10 only for non-Plasma shots or Plasma-blocking targets. The managed impact
adapter preserved this behavior for special beam-combo particles but unconditionally
killed ordinary beam-family projectiles. It now retains ordinary Plasma as well.

The fixture asserts health 3000 -> 2900 -> 2800 without rearming the shot,
fourteen frozen dispatcher calls with fixed flash/position and a live invincibility
countdown, rejection on the first release call (timer one), then a repeat hit
on the next call. Separate controls consume a non-Plasma charged beam and a
Plasma shot hitting a Plasma-blocking target. This is not an input-driven X-ray
or original-CPU boss comparison; those remain required below.

Standard Verification also checks all sixteen charged/uncharged Plasma-bearing
type combinations through two accepted impacts each, preserving type, damage,
direction, instruction pointer and animation timer. The focused selector is
`--plasma-penetration`.

Required remaining work includes native comparison of the normal-firing trace below,
per-hit health/position traces for Phantoon (including the initial non-Plasma
hit), Botwoon, Draygon, steel pirates and Hyper Beam, failed timing and
nonpenetrating controls, and matching native encounter sequences. Leave the
issue open and not awaiting player validation until those cases are verified.

## Controller-driven scope integration

`DebugRunner --botwoon-x-plasma-controls "Super Metroid.smc"` uses the full
runtime, unmodified room geometry, normal ItemSelect and Run bindings, and no
gameplay cheats. Samus starts grounded at X=64 on the room's first supported
surface at/after block row eight. X-ray/Charge/Plasma are equipped; ammunition
is zero. After 360 ordinary frames an overlapping stationary 100-damage Plasma
fixture is installed once. Three 60-held/4-released Run cycles then produce
health changes at relative frames 0, 63, 127 and 191: 3000 -> 2600. Assertions
cover actual scope activations/releases, timer countdown, fixed position/flash
while frozen, damage exclusion during scanning, and retained projectile family.

This removes direct injection of time-freeze state from the earlier combat test.
It does not remove the synthetic projectile placement/lifetime: the ordinary
firing trajectory and an equivalent native encounter still need reproduction.
An initial normally fired-shot trial passed above the head and therefore did not
establish that case; it was not treated as a game defect or a successful test.

## Normally fired charged Plasma integration

`DebugRunner --botwoon-x-plasma-fired "Super Metroid.smc"` now covers a real
charged projectile with its normal trajectory, lifetime and controller-driven
X-ray activation. No actor, projectile or freeze state is injected after setup.
The fixture equips Charge/Plasma/X-ray, clears ammunition, and starts with 999
energy (no invincibility or other gameplay cheats). Samus starts at X=192 on
the room's supported surface, taps Left, and selects X-ray normally.

Two traces hold Shoot for 90 frames and release at relative frame 296 or 304.
After the first hit, each uses repeated 60-held/4-released Run cycles. Release
296 hits once at frame 318. Release 304 hits at 318, 382, 446, 510 and 574:
3000 -> 2550 -> 2100 -> 1650 -> 1200 -> 750. Both spawn exactly one charged
beam; Samus remains at 999 energy. The assertions check each 450-damage hit,
the post-hit invincibility counter, retained beam family, and unchanged enemy
and projectile position while the scope remains active. Botwoon's uncharged
beam immunity makes an uncharged-shot trial unsuitable for this test.

Both traces pass. These exact frame numbers are port regression observations,
not yet original-CPU parity evidence. Native encounter comparison and the
other targets required by #402 remain unfinished; the issue stays open without
the awaiting-player-validation label.

## Phantoon common-shot invincibility omission

`DebugRunner --phantoon-plasma-audit "Super Metroid.smc"` loads the real
Phantoon population and exercises its extended full-body hitbox while swooping.
A constructed charged Plasma shot deals 450 damage. Before correction, health
2500 -> 2050 was correct but invincibility remained zero. Pinned disassembly
`bank_A0.asm` at $A0:A854-$A0:A85F and `sm_a0.c`'s common shot handler both
set sixteen frames when the damaging projectile's Plasma bit is set.

Phantoon's separate common-damage adapter omitted that write. It now uses the
captured pre-impact type and the shared `EnemyShotTiming` definition. The same
fixture now verifies sixteen frames and rejects immediate repeat contact.
This reproduced adapter defect is fixed; full original-CPU Phantoon encounter
comparison, including its initial non-Plasma hit, remains required.

## Original-CPU Phantoon contact matrix

The `native-xplasma-phantoon-probe.h` probe executes original extended collision
at $A0:9B7F and Phantoon's real callback. Twelve constructed boss contact setups
cover eye-tracking/swooping, with/without a charged Power Beam primer, followed
by uncharged Power, uncharged Plasma, or charged Plasma. Two records per setup
compare health, invincibility, flash, function, reaction timer, properties,
accumulated damage and the tentacle reaction markers. All 24 records match.

The native result confirms the sixteen-frame Plasma timer. An uncharged primer
does no damage; the accepted fixture instead uses a charged 60-damage non-Plasma
hit. In eye-tracking it sets the reaction timer to sixteen and starts the round's
damage bookkeeping. Subsequent charged Plasma closes that window; swooping
instead sets the swoop timer to one. This is contact-level evidence, not yet
controller-driven X-ray activation or frozen/released AI evolution.

Two native captures are identical, SHA-256
`6D1D3D531FBD8FBA747045DA88232BA0CFB6520B097567AE697EA66B8CFD907F`.
The comparator rejects other captures. Regenerate with the corresponding
`native-xplasma-phantoon-entrypoint.patch` and bounded, dialog-free invocation:

```text
sm.exe --diagnostic-xplasma-phantoon "Super Metroid.smc" NEW.csv
DebugRunner --native-phantoon-plasma-audit "Super Metroid.smc" NEW.csv
```

The trace remains private. Temporary native hooks were removed after capture.
No player state or recording is used. The broader #402 acceptance remains open.

### Post-hit frozen dispatcher comparison

The v4 probe adds twenty original EnemyMain calls with time frozen after each
contact pair. All 264 records match, including X/Y and instruction position as
well as the previously compared combat state. The boss body is the only active
native record; its real header, callbacks and spritemap remain in use. The
managed comparison uses the normal loaded boss and frozen enemy-frame entry.

Swooping charged-Plasma contacts count invincibility down from sixteen to zero
without moving the body or advancing its reaction/instruction state. Eye-tracking
charged contacts have already set the intangible property; their invincibility
stays at sixteen. This distinguishes the tangible countdown from incorrectly
resetting every boss timer on X-ray activation.

Two independent v4 captures agree, SHA-256
`85D7E253C3B9EBE1BA3819FDAC505AC8ACC7B8616360C22E518EF4E23CDCE631`.
The comparator now requires this expanded capture rather than v2. It does not
yet cover unfrozen boss evolution or full controller activation/release against
the native encounter, and therefore does not complete #402.

### Release ordering defect and full-runtime regression

The v5 probe adds the first unfrozen EnemyMain call after charged-hit cases.
The swooping boss accepts its retained shot before hurt AI and flash housekeeping:
health/invincibility/flash become 1600/16/15. A constructed equivalent boundary
through `SuperMetroidRuntime.StepFrame` instead produced 1600/16/16 before the fix.
The port published Phantoon beam hits after EnemyMain, resetting the clock after
its decrement and delaying hurt dispatch on newly accepted contacts.

Phantoon beam collisions now run in the existing per-enemy pre-AI collision phase,
before bombs and Samus touch, subject to the entry invincibility gate. The old
late runtime call is removed to avoid duplicate dispatch. Other boss/ordinary
beam ordering is not changed by this focused fix.

`DebugRunner --phantoon-plasma-release "Super Metroid.smc"` reproduces the
full-runtime boundary and now passes. The expanded native comparator retains
the original shot across frozen/released calls and all 268 records match.
Two v5 captures agree, SHA-256
`5DD3B0EBB99165DC54F986069329C488A7324FB6A10D00B27E992A9C0E560A88`;
the comparator requires v5. Full normal-firing/controller boss sequences and
the remaining targets still prevent closing #402.

The full runtime also checks entry invincibility one: the frame decrements to
zero but still rejects contact, preserving the cartridge's entry-value gate.
The complete Verification suite, normal-firing Botwoon regressions and Windows
Release build pass after the ordering change (build: zero warnings/errors).

## Draygon and shared ordinary-shot dispatch

The original-CPU Draygon release probe uses the real body header and extended
map, a retained 450-damage charged Plasma shot, and entry invincibility zero/one.
Original EnemyMain produces health/invincibility/flash 5550/16/11 or 6000/0/11,
respectively. Its native shot callback increases swoop acceleration by eight
only in the accepted case; both positions remain 128,128.

`DebugRunner --draygon-plasma-release "Super Metroid.smc"` reproduced
5550/16/12 through the full runtime before correction. The ordinary shot path
had the same late publication problem as Phantoon. It now accepts a current
native-slot parameter and runs before that actor's bombs/touch/AI, using the
existing entry-invincibility gate. The old whole-list late publication is removed.
This fixes shared dispatch, not a Draygon-specific timer adjustment.

Two native captures agree, SHA-256
`B10966384B016C372E0C81774D5D5288D6DAC81F73A98A34A1B2A20C62B687C0`.
Regenerate with `native-xplasma-draygon-entrypoint.patch` and
`sm.exe --diagnostic-xplasma-draygon "Super Metroid.smc" NEW.csv`.
Both full-runtime boundary cases now match, including acceleration and position.
Native hooks were removed and the ordinary executable rebuilt after capture.

Botwoon's normally fired port regression now hits one frame later because shot
collision samples the pre-AI head position rather than its newly moved position.
The documented frames above are updated; one-hit/five-hit outcomes and damage
remain unchanged. These are still port observations, not a claim that the full
Botwoon trajectory has been compared to original CPU execution.

After this shared change, the complete Verification suite, both Phantoon release
and native comparison checks, both Botwoon input traces, and Windows Release
build pass. Build output contains zero warnings and zero errors.

## Hyper Beam variant

`DebugRunner --botwoon-hyper-audit "Super Metroid.smc"` uses normal firing
and ItemSelect/Run controls after initializing Hyper plus X-ray in the retail
room. It never installs a projectile or modifies the boss after setup. Each
case fires exactly one real `$9018`, 1000-damage Hyper projectile:

- Fire frame 300 with scope: hits 318/382/446, health 3000 -> 0.
- Fire frame 296 with scope: hits 318/382/446, health 3000 -> 0 (after the
  shared activation-frame correction documented below).
- Fire frame 300 without scope: hit 318 only, health 3000 -> 2000.

Assertions cover firing metadata, retained beam family, per-hit damage, normal
scope selection, and fixed boss/projectile positions and flash while frozen.
These exact trajectories are port regressions; a matching original-CPU
controller encounter is still outstanding.

The Draygon native probe and full-runtime release fixture additionally cover
Hyper's type/damage at the release boundary. Native results are 5000/16/11 for
health/invincibility/flash when admitted and 6000/0/11 at entry timer one; the
port agrees, including acceleration and position. Two v2 captures agree:
SHA-256 `3920203CBC1B550415DBA4E5871FCCFC98274188BEE49B151C2F7CBB0F561007`.
All three Botwoon cases and all four Draygon cases pass. This expands evidence
without another production change and does not finish #402's remaining scope.

## Steel Pirate overlap boundary

`DebugRunner --ninja-x-plasma-audit "Super Metroid.smc"` loads the Metal
Pirates room and isolates one gold Ninja. The initial vulnerable component map
and stationary overlapping Plasma shot are constructed. This is an enemy-pass
prerequisite, not controller admission or a normal jump/firing trajectory.

Both uncharged (150 damage) and charged (450 damage) Plasma survive contact
with the vulnerable component. Frozen passes preserve position, map, health,
and flash while counting invincibility down. Releasing after 15 frozen passes
rejects the next hit even though the entry timer changes from one to zero;
releasing after 16 admits the retained shot through the production pre-AI
dispatcher. Final health is respectively 1650/1500 for uncharged and 1350/900
for charged. Projectile type and damage are retained throughout.

The pinned B2:87C8 callback checks the actual beam vulnerability before normal
shot handling; B2:883E reflects armored contacts. The new four-case boundary
audit and existing complete Ninja family audit pass without production changes.
An original-CPU steel-Pirate comparison and normally fired/controller-driven
vulnerability-window test remain outstanding. Do not treat this as completion
of #402 or proof of the practical window's duration.

### Original-CPU steel comparison

The overlap fixture now also executes the pinned cartridge's extended collision
routine and EnemyMain through `native-xplasma-steel-probe.h`. Both sides start
the actor at (128,128), with the projectile at (118,118). Optional CSV input to
`--ninja-x-plasma-audit` compares every contact, frozen pass, and release record:
all 70 records match for health, invincibility, flash, map, actor position,
projectile type, and damage. The native flash is 12 during freeze and 11 after
release, whether the entry invincibility gate accepts or rejects the shot.

Two independent private captures agree, SHA-256
`536D49163068522E708A29DA9C15D8C487A6C0C8D008859BFD020A8CF9DEAB1A`.
Regenerate using `native-xplasma-steel-entrypoint.patch`, then
`sm.exe --diagnostic-xplasma-steel "Super Metroid.smc" NEW.csv`. Supply that
CSV after the ROM argument to the managed audit. Native hooks were reversed
and the ordinary native executable rebuilt afterward. This supersedes the
missing original-CPU overlap comparison above; the practical firing window
and controller-driven encounter remain unverified. No production change.

### Normally fired steel Pirate trace

`--ninja-fired-plasma-audit "Super Metroid.smc"` now exercises the untouched
Metal Pirates population with actual Shoot and ItemSelect/Run inputs. Initial
grounded Samus requests X=328 / screen Y=166, searching from floor row 8, with 999 energy, no ammo,
X-ray and Varia (the room is heated), and uncharged Plasma. Setup inputs are
Left, neutral, ItemSelect, leaving her at (325,187). No enemy, projectile, or
freeze state is modified after initialization; gameplay cheats are not enabled.

One-frame Shoot at frame 16, followed after the first hit by repeating 60 Run /
4 neutral frames, deals 150 damage at frames 29 and 93. Shoot at 17 instead
hits once at frame 30. Shoot at 16 without Run hits once at 29. Every case fires
exactly one uncharged projectile. The scope cases complete two releases and
enter a third scan; assertions compare health and invincibility at every scope
boundary, plus frozen actor/map/flash/projectile position and subposition.
The first failed repeat reaches the armor reflection timer (10), not another
damage event. Final Pirate HP is 1500/1650/1650. Samus remains at 999 with scope;
the unpaused control takes 10 damage at frame 33 and ends at 989.

All three cases pass without a production change. This fills the previously
missing practical port firing-window coverage. Exact controller trajectories
have not yet been compared to original CPU execution; the native steel oracle
above proves the isolated contact/freeze/release semantics, not this whole
encounter. #402 is still incomplete under the parent parity contract.

### Native controller comparison and activation-frame correction

`native-steel-fired-probe.h` loads the retail room, level/scroll/CRE data,
and both enemies, then runs original game-state 8 with the same controller
sequence. It runs HDMA before gameplay and advances frame RNG, as the main
loop does. Enemy projectiles are enabled. Samus starts at (328,187), camera
(232,21), pose 1, and zero subpositions; equipment/ammo match the managed setup.
The setup's three input frames are retained in the CSV; the following 529
frames compare directly through optional CSV input to `--ninja-fired-plasma-audit`.
Compared fields are input, freeze, Pirate HP/invincibility/flash/map/XY, Samus
XY/pose/HUD selection, and shot type/XY. Subpixel preservation is separately
asserted during frozen port frames, not compared as a native CSV column.

This stronger comparison reproduced a real mismatch: native shot X remained
250 on activation frame 30 while the port moved it to 245. The runtime tested
freeze before scope admission, then both projectile owners still advanced
their slots after admission. Native `Samus_HandleHudSpecificBehaviorAndProjs`
tests the newly changed flag before `HandleProjectile`. Both slot loops now
honor that live flag, preserving the cooldown pass; the post-alpha bomb overlap
call is also skipped while frozen. A focused constructed-bomb test verifies
that activation preserves fuse and instruction timer with default/remapped Run.
The normal firing test now asserts frozen position on activation itself, not
only after two consecutive frozen frames.

All 529 native frames now match, including successful and adjacent failed repeat
hits and the no-scope control. Two final captures agree, SHA-256
`DDC67E5FDD751B54409684225656ED32D2125AD64D3B9AAFDEEEC31BB43A9DD3`.
Regenerate with `native-steel-fired-entrypoint.patch` and
`sm.exe --diagnostic-steel-fired "Super Metroid.smc" NEW.csv`. Native hooks
were removed and the ordinary executable rebuilt. Intermediate diagnostic
captures had incomplete camera/dispatcher setup and are not accepted oracles.

The shared fix leaves Botwoon's charged-Plasma hit frames unchanged. The Hyper
frame-296 port trace now reaches a third hit at 446 instead of moving beyond
the target after two hits; its assertion is updated accordingly. Both scoped
Hyper cases now kill with three hits, while the no-scope case still hits once.
These Botwoon trajectories still need their own full native comparison. This
section supersedes the earlier missing steel full-input comparison, not the
remaining #402 encounter work.

Verification after the shared correction: full Verification suite passes;
focused X-ray controls pass in Release with both binding layouts; all 529
steel native records and both charged-Plasma / all three Hyper Botwoon cases
pass. Windows Release builds with zero warnings and errors. The focused bomb
assertions were added after the full suite started and run separately in Release.

### Botwoon activation-frame regression coverage

The charged-Plasma controller audit now asserts projectile whole/subpixel
coordinates on the first X-ray activation frame, not just consecutive frozen
frames. Enemy freeze checks remain consecutive-frame checks because enemy AI
can run before scope admission. Both release-296 and release-304 traces pass
in Release: one hit versus five hits at the documented frames, with player
health unchanged. This strengthens coverage of the already-fixed shared
activation ordering; it is not a new gameplay fix or a substitute for the
remaining full native Botwoon trajectory comparison.

### Full native Botwoon charged-Plasma controller comparison

`native-botwoon-fired-probe.h` runs the original game-state-eight dispatcher,
HDMA, and frame RNG using the same two charged-shot input sequences. It loads
the retail Botwoon room, level/scroll/CRE, enemy population, and FX header.
Initial Samus is (192,187), pose 1, camera (96,21), X-ray and Charge/Plasma,
999 energy, zero ammo, and no gameplay cheats. Four setup input frames are
neutral, Left, neutral, ItemSelect. No hit or freeze state is injected.

All **1,200 gameplay records** match: input, freeze, boss HP/invincibility/flash,
spritemap and XY, Samus XY/pose/HUD selection, and projectile slot-zero type/XY.
Release 296 hits once at 318; release 304 hits at 318/382/446/510/574. Each hit
deals 450. These are full controller trajectories, superseding the earlier
missing native charged-Plasma comparison. Hyper trajectories remain outstanding.

Regenerate using `movement-release/native-botwoon-fired-entrypoint.patch` and
`sm.exe --diagnostic-botwoon-fired "Super Metroid.smc" NEW.csv`, then compare:
`SuperMetroid.DebugRunner --botwoon-x-plasma-fired "Super Metroid.smc" NEW.csv`.
Two final native captures agree, SHA-256
`A3C781AFF7813230C75C63AFAE4BC68F6238562738288875FDD481503ED028A8`.
The numeric CSV, including the eight setup rows, is preserved in
`movement-release/botwoon-fired-402.zip`; no ROM or visual assets are included.

Harness limitations: original $91:CD42 decrements the left-edge block index
without checking zero, unlike the translated upstream helper. At camera (0,0),
the original $91:CDD6 BTS read reaches $80:6401/2. The upstream diagnostic mapper
aborts on this unmapped read. The entrypoint patch allows the emulator's existing
open-bus return for those two addresses only, only in this headless diagnostic,
and reports it to stderr. All other invalid reads still fail. This does not
patch cartridge instructions or port gameplay. X-ray tilemap visuals and exact
hardware open-bus timing are not asserted by this combat comparison.

Earlier captures used a dry native room or the requested screen Y as world Y;
they are rejected as evidence. The final probe loads the actual water FX and
the grounded world position. Temporary upstream hooks were removed afterward.

### Full native Botwoon Hyper controller comparison

The same native probe now exposes `--diagnostic-botwoon-hyper`. It uses the
charged fixture's complete retail room/FX setup, equips Charge/Wave/Plasma,
and sets the actual Hyper flag during initialization. The three normal-input
cases are Shoot at 296 or 300 with repeated scope cycles, and Shoot at 300
without scope. Scoped traces stop after the lethal third hit; the control runs
through frame 519. No actor, projectile, or freeze state is edited after setup.

All **1,414 gameplay records** match through optional CSV input to
`--botwoon-hyper-audit`: input, freeze, boss HP/invincibility/flash/map/XY,
Samus XY/pose/HUD, and projectile slot-zero type/XY. Both scoped cases deal
1,000 damage at frames 318/382/446; the no-scope case hits only at 318. The
managed fixture also checks unchanged projectile whole/subpixel coordinates
on X-ray admission itself, normal spawn metadata, and retained beam family.

Two native captures agree, SHA-256
`E2C523FDBCE44AE94C067EE7585535771EB77A3FD6A0F312B428B0DB07B58E64`.
Numeric evidence is in `movement-release/botwoon-hyper-402.zip`, including
twelve setup rows in addition to the compared gameplay rows. The shared
probe still regenerates the charged-Plasma CSV with its previous exact hash.
The documented headless open-bus exception and visual/hardware-timing limits
apply unchanged. No production correction was needed for these sequences.
