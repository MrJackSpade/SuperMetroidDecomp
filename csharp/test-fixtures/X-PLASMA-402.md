# #402 X-Plasma parity

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
296 hits once at frame 317. Release 304 hits at 317, 381, 445, 509 and 573:
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
