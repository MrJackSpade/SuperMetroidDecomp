# Golden Torizo combat audit (#617)

Affected player version: 0.3.4. Report: sustained Super Missile fire felt too
effective, with little damage taken. This does not establish a specific defect.

## Reproduced divergence: subsequent hit after caught Super

Pinned $AA:D667 first rejects flash, then sends animation-locked shots directly
to common damage. Otherwise $D67C tests behavioral bit $1000. If set, $D682
branches to $D69D and ORs $2000 before damage; it does not update the captured
projectile family. $2000 is later consumed by the health-gated stun instruction.

The port's normal C97C -> D667 path skipped that OR. The retail hitbox fixture
reproduced one Super hit at health 13500 with no flash/animation lock and $1000
set: health correctly became 12900 but flags remained $1000 instead of $3000.
This was documented in the issue before changing production code.

The correction adds the missing write only for the normal callback, without
changing damage, catch selection, flash rejection, or animation-lock routing.
Eight cases cover normal and stand-up/sit-down hitboxes, zero/nonzero animation
guard, and zero/nonzero flash. Each asserts hit admission, health-change behavior,
the counterattack bit, and preservation of the captured-family word.

Command: DebugRunner --golden-torizo-audit ROM. Before correction the new first
case fails; after correction all eight and the existing encounter audit pass.

## Coverage limits / remaining audit

### Super capture facing/leg matrix

Eight additional cases check graphical bits 0000/2000/8000/A000 with Samus
in front and behind. Independent $AA:D71D-D741 expectations select CDE1,
CE43, CEA5, CEFF respectively. Front hits retain health and set the catch
latch/list/timer; rear hits apply damage without capture. All cases passed
before cleanup. Two old catalog names incorrectly described the facing
direction; the names now spell out both facing and leading leg, with no
address or behavior change. This confirms selection, not full animation timing.

The existing audit exercises retail loading, animation/movement, five projectile
families, shot reactions, contact damage, death and boss completion. Some branches
are entered explicitly; successful execution does not establish native cadence.
The missing bit alone is not proof of the cause of the difficulty report.

Still required for #617: independent native comparisons of state selection,
attack cadence and movement under sustained firing, full shot/contact damage and
invulnerability timing, projectile trajectories, and any relevant conditional
AI inputs. Do not label the entire audit awaiting validation based on this fix.

## Original-CPU health decision comparison

`movement-release/golden-health-decisions-617.csv` captures 240 calls to the
original $AA:D474 and $AA:D49B routines. Six health values straddle 1928 and
10800, with the stun bit clear/set and ten RNG seeds. Each row records the
returned operand cursor, resulting RNG, saved link and decision counter.
Both taken and untaken paths occur. All fields match production dispatch via
`--golden-health-native-compare ROM CSV`; no gameplay correction was needed.

The native comparison establishes inclusive low-health admission (<=1928),
strict high-health admission (>10800 with stun), conditional random consumption,
and exact preserved/reset counter/link behavior. This covers these two decision
callbacks, not complete encounter cadence or sustained controller input.

To recapture, include native-release-probe.h then native-golden-decision-probe.h
from sm_rtl.c and dispatch DiagnosticGoldenDecisions(ROM, NEW_CSV) before SDL
initialization. Suppress SDL error dialogs for that headless entrypoint. The
loader restores original ROM bytes after SnesInit, and the bounded CPU helper
executes actual instructions, not native C translations. Input Y=$D000 addresses
a fixed ROM operand; C# uses opcode cursor $CFFE to read that same operand.
Initial link=$1234 and counter=9 make non-writes observable. Temporary hooks were
removed and the ordinary native executable rebuilt after capture.

## Original-CPU distance/facing comparison

`movement-release/golden-distance-decisions-617.csv` captures 1,024 calls to
$AA:D3EA (morphed behind) and $AA:D445 (medium-range random call). It covers
distances 3/4/31/32/39/40/95/96 on either side, both facings, standing/morph/
spring-ball poses and four RNG seeds. Production cursor, RNG, link and counter
match every row using `--golden-distance-native-compare ROM CSV`. Taken/untaken
counts are 96/416 and 32/480 respectively. The preceding 240 health cases still
pass after sharing the comparison harness. No gameplay changes were needed.

Recapture through DiagnosticGoldenDistanceDecisions using the same headless
setup above. Native enemy X is 256, input Y is $D000, link is $1234 and counter
is 9. The temporary hooks were removed and the ordinary executable rebuilt.
These are decision-boundary tests, not evidence that a particular room position
is safe or that full-fight attack cadence matches.

## Original-CPU jump decisions

`movement-release/golden-jump-decisions-617.csv` contains 768 calls to $AA:D4BA
and $AA:D4FD. Distances 31/32/111/112 on both sides/facings, Space Jump counts
359/360/361, decision counts 7/8, no direction/right held and two RNG seeds
exercise jump and no-jump branches. `--golden-jump-native-compare ROM CSV`
matches every returned cursor, RNG, decision counter, horizontal/vertical
velocity, acceleration and instruction timer. Nonzero initial sentinels prove
that rejected jumps do not overwrite motion or timer state.

Recapture with DiagnosticGoldenJumpDecisions through the same headless setup.
The native probe runs actual decision and jump-initialization instructions, not
a handwritten reference equation. No gameplay changes were necessary. This
verifies jump selection/initialization; airborne trajectories, landings and the
frequency of decision calls during a complete encounter remain separate work.

## Projectile table-origin regression

Original-CPU `golden-projectile-inits-617.csv` covers five projectile initializers
in both facings with RNG seeds 0..255 (2,560 rows). Spawn X/Y, velocity X/Y,
instruction list and resulting RNG are compared by
`--golden-projectile-native-compare ROM CSV`. Orb, sonic boom, egg and held-Super
initialization matched before the correction. Eye beams did not: left-facing,
seed zero produced native velocity (-1408,+1480), but the port used
(-1480,-1408). Both had position (236,354), list $B410 and RNG 8463.

`golden-super-aim-617.csv` separately executes $86:B269/$B272 for a projectile
at (512,512), with Samus offsets -300/-64/0/64/300 on each axis. The 50-row
`--golden-super-native-compare ROM CSV` comparison also failed before correction:
rightward aim at (212,212) expected (+724,+724), but produced (-724,+724).
The out-of-byte-range inputs intentionally retain the cartridge divider behavior.

Both defects came from treating $A0:B443 as the start of the combined negative-
cosine/sine table, which actually starts at $A0:B3C3. Reuse the existing compiled
`EnemyTrigonometryTables.SignedNegativeCosineWord` table to preserve sine for X
and negative cosine for Y, without changing angle calculation or direction masks.
Pinned disassembly $86:B361/B36B and $86:B27E/B287 independently show those bases.

Recapture through `DiagnosticGoldenProjectileInitializers` and
`DiagnosticGoldenSuperAim` in the headless probe, using the same original-ROM
restoration and bounded CPU setup above. Temporary hooks were removed and the
ordinary native executable rebuilt. These traces verify projectile initialization
and aiming; they do not establish full-fight cadence or confirm the reported safe spot.

After correction all 2,560 initialization rows and 50 aim rows match. The existing
Golden Torizo encounter audit, full core verification and Windows Release build
also pass. No other projectile initializer required a gameplay change.

## Failing integrated room comparison: shared lava RNG

`movement-release/golden-encounter-617.csv` records 3,000 original CPU game-state-
eight frames in the Golden Torizo room. Initial Samus is (384,395), camera
(256,229), Varia equipped, health 9999, 100 Missiles and 99 Supers with Supers
selected. RNG is $1234. Input turns left on frame 60, then fires every 20 frames
from 500. No boss state or projectile outcomes are forced during the sequence.
High initial health keeps the comparison running without suppressing damage.

`--golden-encounter-trace ROM [NATIVE_CSV]` runs the matching production runtime.
Without the CSV it emits a complete diagnostic trace; with the CSV it fails at
the first differing field. This is deliberately a failing reproduction, not a
passing regression or evidence that the entire encounter agrees.

Current result: frame 0 agrees in every recorded field. Frame 1 RNG is native
27613 versus port 52602. The cartridge lava/acid BG3 HDMA pre-instruction swaps
the bytes of the shared RNG word before main-loop random generation. The port's
room FX owner omits that mutation. All other recorded fields agree through frame
683; frame 684 differs in list, timer, function and horizontal velocity. Correct
the shared owner/order and startup timing, not Golden Torizo's RNG in isolation.

The native fixture loads room data and enemy initialization through $82:DE6F,
$82:DEF2, $82:EA73, $A0:8A1E and $A0:8A9E, then calls the real HDMA handler,
main-loop RNG and $82:8B44 each frame. It does not initialize the entire room PLM
population, so later door/item interactions are not proven by this fixture.
The first RNG mismatch is before any such interaction. Recapture through
`DiagnosticGoldenEncounter` in `native-golden-encounter-probe.h` using the same
headless entrypoint pattern above. Temporary hooks were removed and the normal
native binary rebuilt. The new comparator builds and reproduces the stated
frame-1 failure; no production correction is included in this diagnostic change.
