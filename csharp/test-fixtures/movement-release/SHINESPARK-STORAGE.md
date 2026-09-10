# #465 Shinespark storage and launch investigation

Status: partial implementation; not yet awaiting player validation.

## Grounded held-forward input windows

`native-spark-window-probe.h` uses original alpha input, beta movement, animation,
pose transitions and palette countdown. Constructed 16x32-block room, floor row
16, Samus (128,235), zero initial subpixels/RNG, both facings, Speed Booster
equipped, health 99. Media: dry, suitless water (surface 8), water with Gravity.
Storage is seeded through the original crouch-storage routine at frame 20;
Jump is held from frame 24, and forward is held from frame 24+offset (0..35).
Capture stops on a directional spark pose or after frame 63. No gameplay cheats.

`--spark-window-audit ROM CSV` reproduces all 216 cases through the full managed
runtime, comparing every recorded pose, shine/windup timer, and 16.16 X/Y.
All cases match; no production fix needed. In this held-input setup, dry
forward offsets 0..30 produce horizontal spark and 31..35 time out vertically.
Suitless water offsets 0..3 do not reach a spark in the sample; 4..33 produce
horizontal spark and 34..35 time out vertically. A held direction survives
until an eligible frame: these are NOT one-frame tap-window measurements.

Two captures are identical. Accepted CSV in `spark-window-465-v1.zip`, SHA-256
`CA323D81D610223AA0C05579467F2359718231DCED8263DE394C4FEBE2D89A2D`.
Regenerate with `native-spark-window-entrypoint.patch` and bounded/dialog-free
`--diagnostic-spark-window ROM NEW.csv`. Temporary native hooks removed.
At that checkpoint, tap directions, aerial/Shot/angle restrictions, energy and actual liquid/sand
travel remain outstanding; this checkpoint does not mark the whole ticket ready.

## One-frame forward taps: reproduced initializer omission

The same original-CPU harness now accepts a tap variant: forward is pressed for
exactly one frame at jump-relative offsets 0..35, while Jump remains held. Both
facings and dry/water/water-with-Gravity environments produce 216 cases.
Before the fix, six suitless-water cases (offsets 1..3, both facings) failed:
on release, native normal-jump initialization replaced the neutral-jump pose
with shinespark windup and moved Y up one pixel; managed gameplay stayed in a
normal jump. There were 186 mismatching frame records across those six cases.

ApplyAerialAimTransition applied ordinary acceleration but omitted the stored
shine branch of SamusFunc_F468_NormalJump ($91:F543). Changed aerial poses now
call the existing shared windup initializer before ordinary acceleration.
All 216 tap cases and all 216 held-input cases match the native capture after
the fix, comparing pose, shine timer, windup timer and 16.16 X/Y every frame.
This verifies initiation through directional launch, not subsequent travel.

Two tap captures are identical. Accepted CSV in `spark-tap-window-465-v1.zip`,
SHA-256 `49165A9EFFD5CEB5C4625B2FCE77E171588A844B9C7B084D664F38A2CEA3FBD0`.
Regenerate using `native-spark-tap-window-entrypoint.patch` and bounded,
dialog-free `--diagnostic-spark-tap-window ROM NEW.csv`; compare with
`--spark-tap-window-audit ROM CSV`. Temporary native hooks were removed.
The held-input entrypoint remains supported by the shared probe header.
Issue #465 remains incomplete: aerial/Shot/angle restrictions, energy and
liquid/sand travel still need coverage.

## Aerial spin exits: reproduced aim-up initializer omission

`native-spark-aerial-probe.h` uses the same geometry and original-CPU frame
stages as the window capture. Store at frame 20; hold forward from frame 22,
Jump from frame 24; release forward at frame 30 and keep Jump plus one of
neutral/Shoot/Aim Up/Aim Down. Both facings and all three media give 24 cases.
Starting forward only on frame 24 was rejected as a fixture: dry/Gravity cases
launched before the tested input. The accepted v2 fixture starts moving first,
and the managed comparator rejects captures ending before frame 30.

Before the fix, Aim Up selected ordinary jump poses instead of windup in six
cases, producing 12 mismatched records through directional launch. The
spin-to-normal-jump helper omitted the stored-shine branch of the native
normal-jump initializer. It now reuses the windup initializer, preserving the
following Screw Attack palette-restore ordering. All 24 cases match per-frame
pose, shine/windup timers and 16.16 position. These inputs test spin exits;
they do not establish every held-button restriction or airborne launch window.

Accepted capture: `spark-aerial-465-v2.zip`, CSV SHA-256
`50BF582B79BD6EFBD70D861FB7319698AD57E722AC22BC9D75A83FEE0598E647`.
Two native captures match. Regenerate with `native-spark-aerial-entrypoint.patch`
and `--diagnostic-spark-aerial ROM NEW.csv`; compare using
`--spark-aerial-audit ROM CSV`. Native hooks removed after capture.

## Held aerial buttons and compact-pose exits

`native-spark-restrictions-probe.h` extends the spin-exit setup. At frame 30
hold Shoot, Aim Down, Down, or Down+Shoot for six frames, then release to
neutral or Aim Up at frame 36, keeping Jump held. Both facings and the same
three media produce 48 cases. The comparator requires each native case to
reach the tested release frame, preventing premature launches from passing
as evidence of the intended transition.

Twelve cases failed before the fix: Down/Down+Shoot followed by Aim Up, both
facings in each medium. Native enters windup and adjusts Y up one pixel;
managed compact-pose expansion remained in ordinary jump. There were 24
divergent records through directional launch. After successful collision
resolution, the compact transition now invokes the existing stored-shine
initializer before ordinary acceleration. Rejected expansion still returns
before consuming stored shine. All 48 cases match pose, timers and 16.16 X/Y;
the previous 456 launch cases also remain matching.

Accepted CSV in `spark-restrictions-465-v1.zip`, SHA-256
`226191CABC7AD7862AC7CFDA2A01DEEDE039E2A264B2AD1B7087AF38A7080C1D`.
Two original-CPU captures match. Regenerate with
`native-spark-restrictions-entrypoint.patch` and bounded/dialog-free
`--diagnostic-spark-restrictions ROM NEW.csv`; compare using
`--spark-restrictions-audit ROM CSV`. Temporary native hooks removed.
This covers the stated held/release sequences, not every grounded restriction,
crouch exception, airborne input window, or subsequent energy/liquid travel.

## Dry travel, energy threshold, and diagonal wall collision

`native-spark-energy-probe.h` constructs the same floor plus ceiling row zero
and walls at columns zero/fifteen. Storage is seeded at frame 20, Jump held
from 24, and horizontal/diagonal direction from 28; vertical uses windup timeout.
Initial health is 1/28/29/30/31/99, both facings, all three directions: 36 cases.
Capture ends at native crash entry, with a 96-frame cap. Version 1's 64-frame
cap was rejected because high-energy vertical sparks had not hit the ceiling.
The comparator requires every case to end in crash entry and compares every
pose, shine/windup timer, 16.16 X/Y, health and crash-entry flag.

Two diagonal high-energy cases initially failed: native slides upward against
the wall until vertical collision, while managed crashed at first wall contact.
Native X and Y movement overwrite a shared collision flag; Y runs last. The
managed OR of both axis results incorrectly retained the earlier wall collision.
Production now uses the final executed axis's result, preserving this cartridge
behavior. All 36 cases match after the fix. Energy itself needed no change:
30 drains to 29 after moving, then the next frame moves before ending; values
already below 30 move once and end without draining. Collision at higher energy
still drains on the crash-entry frame. Cheats are off in this native comparison.

Accepted CSV in `spark-energy-465-v2.zip`, SHA-256
`E33BA87405556B25520B2DF9B8A3E428295A4D7AA51F980981F0FB25D02C2BDE`.
Two captures match. Regenerate using `native-spark-energy-entrypoint.patch`
and bounded/dialog-free `--diagnostic-spark-energy ROM NEW.csv`; compare with
`--spark-energy-audit ROM CSV`. Temporary native hooks removed.
Liquid/sand travel and remaining launch restriction/window coverage are still
outstanding. This fixture does not validate the separate invincibility override.

## Fully submerged water travel

`native-spark-water-probe.h` repeats the enclosed dry-travel room with water
surface Y=8 and options 80. Health starts at 99, Speed Booster equipped,
Gravity absent/present, three directions and both facings (12 cases). Storage
and launch input timing are unchanged. Capture ends at crash entry, capped
at 160 frames. Compare every pose, shine/windup timer, 16.16 X/Y, health and
crash flag through the full managed frame dispatcher. All 12 cases match;
no production change was needed. This covers submerged room travel, not
crossing a liquid boundary, lava/acid, or sand movement modifiers.

Two native captures match. Accepted CSV in `spark-water-465-v1.zip`, SHA-256
`59406F8B686F99FD126F88D170A7EBAE9BC58039D8297C73BCE5E580B01193D9`.
Regenerate using `native-spark-water-entrypoint.patch` and bounded/dialog-free
`--diagnostic-spark-water ROM NEW.csv`; compare with `--spark-water-audit ROM CSV`.
Temporary native hooks removed. The comparator rejects truncated cases that
have not reached crash entry. Gameplay cheats are disabled for this fixture.

## Lava and acid travel plus fractional damage

`native-spark-corrosive-probe.h` uses the enclosed travel room with lava/acid
surface at Y=8, health/max health 999, and Gravity absent/present. Three
directions, both facings and four liquid/equipment combinations yield 24 cases.
The normal animation FX handler and periodic-damage routine execute every
frame, alongside movement, transitions and palette processing. The managed
comparison uses the full gameplay dispatcher with matching liquid configuration.
Every pose, timer, 16.16 position, health, fractional health and crash flag matches.
No production change was necessary.

Lava cancels extra-run momentum before its Gravity check; acid does not. The
extremely slow horizontal lava sparks exhausted the initial 512-frame capture
without crashing, so that fixture was rejected. Version 2 allows 1200 frames
and requires every case to reach collision/energy termination. No cheat or
health refill is used. This verifies sustained immersion, not surface crossings.

Two captures match. Accepted CSV in `spark-corrosive-465-v2.zip`, SHA-256
`E779CABF32F0C8AB9E457583E22555638113DC1BBD80030A7906339C76B68638`.
Regenerate with `native-spark-corrosive-entrypoint.patch` and bounded/dialog-free
`--diagnostic-spark-corrosive ROM NEW.csv`; compare with
`--spark-corrosive-audit ROM CSV`. Temporary native hooks removed.
Sand, liquid-boundary crossings and remaining launch restrictions still need
coverage before #465 is ready for player validation.

## Launch attempts from inside sand

`native-spark-sand-probe.h` repeats the enclosed water fixture with Maridia
area identity and rows 13..15 filled with special-air BTS 82 or 83. Both
facings, three requested directions and Gravity absent/present give 24 cases.
Native one-shot sand PLM instruction lists execute each frame so repeated
contact cannot exhaust slots. The managed synthetic fixture replaces only
its header's area through reflection to select Maridia's real BTS tables;
it does not claim to reproduce the surrounding retail room.

All per-frame poses, shine/windup timers, 16.16 X/Y, health and crash flags
match. Twelve suitless attempts never launch and are observed through charge
expiry to frame 255. Twelve Gravity attempts launch and reach collision or
energy termination. The comparator explicitly checks these distinct capture
end conditions; a timed-out active spark cannot pass. No production fix needed.
This does not yet cover an already-active suitless spark entering sand.

Accepted CSV in `spark-sand-465-v1.zip`, SHA-256
`BA0837A6A362939F04A8C2A6B76B119883930D1A82E2B0C14FA636AA30FE9EE5`.
Two native captures match. Regenerate using `native-spark-sand-entrypoint.patch`
and bounded/dialog-free `--diagnostic-spark-sand ROM NEW.csv`; compare with
`--spark-sand-audit ROM CSV`. Temporary native hooks removed after capture.

## Active shinesparks entering sand

`native-spark-sand-entry-probe.h` moves the sand away from the initial body:
rows 8..12 span interior columns; rows 13..15 contain sand only in columns
11..14 for right-facing cases or 1..4 for left-facing cases. This permits
all 24 combinations to launch before entering surface/submerging sand,
including suitless sparks. The same water, health, equipment, input timing,
native PLM cleanup and managed synthetic area setup remain in use.

Every case reaches sand while in a directional spark pose and ends in crash;
the comparator asserts both conditions and compares per-frame pose, timers,
16.16 X/Y and health. All 24 cases match; no production fix needed. This
captures the strong horizontal slowdown in surface sand, the distinct
submerging reaction, and vertical/diagonal travel through the sand region.
The original 24 inside-sand launch attempts remain supported separately.

Accepted CSV in `spark-sand-entry-465-v1.zip`, SHA-256
`E83A95054A51FAC0AF7877467D65EAF8D748F70D64A2C2254D967F27A815A89E`.
Two native captures match. Regenerate with
`native-spark-sand-entry-entrypoint.patch` and bounded/dialog-free
`--diagnostic-spark-sand-entry ROM NEW.csv`; compare with
`--spark-sand-entry-audit ROM CSV`. Temporary native hooks removed.
Liquid surface crossings and remaining launch restrictions/override coverage
still remain before #465 can be marked ready for player validation.

## Upward liquid surface crossings

`native-spark-surface-probe.h` moves water/lava/acid surfaces to Y=128 so
vertical and diagonal sparks leave the liquid before hitting the ceiling.
Both facings and Gravity absent/present produce 24 cases, health/max 999,
cheats off. Compare per-frame pose, timers, 16.16 position, whole/fractional
health, crash entry, liquid physics state and split gravity acceleration.
All 24 cases match; no production change was needed. The comparator requires
each case to start submerged, become air while still sparking, and end in crash
outside the liquid, avoiding a false pass from a fixture that never crosses.

Accepted CSV in `spark-surface-465-v1.zip`, SHA-256
`94E65FA92F45E63FD67C329234E27ADCC435C210B5CB41CEC95A367F89C66963`.
Two native captures match. Regenerate with `native-spark-surface-entrypoint.patch`
and bounded/dialog-free `--diagnostic-spark-surface ROM NEW.csv`; compare with
`--spark-surface-audit ROM CSV`. Temporary native hooks removed.
This verifies upward exit through a fixed liquid surface, not changing-height
liquid entry. Remaining launch restrictions and explicit host override checks
still need completion before #465 is ready for player validation.

## Storage admission and palette-owned expiry

Original-CPU fixture `native-shine-storage-probe.h` executes Samus_CrouchTrans
($91:F7B0), then the live stored-shine palette handler ($91:DAC7). It covers
Speed Booster counter words 0000/03FF/0400/04FF/0500/8300/8400/FF00 and all three
suit palette indices. WRAM/subpixels/RNG start zero. No controller movement or
cheats are used; these are explicit handler-boundary seeds, including unreachable
ordinary-play counter values to verify the native signed comparison.

For each case, capture initialization plus 182 update opportunities. Compare
the timer, palette kind/frame, warning sound request and all 16 live palette
words. On expiry the outer palette dispatcher restores normal suit colors;
that restoration is deliberately not claimed by this handler-only capture.
All **4,392 records match** production TryStoreFromSpeedBooster/UpdatePalette.
No production fix was necessary. The warning is requested when the pre-update
timer is 170, and normal stored charge reaches zero on update 180.

Two native captures are byte-identical. Accepted CSV in
`shine-storage-465-v1.zip`, SHA-256
`EF2CE1E9112C7F0DAB9A1615766B83B3C9DEF85CCB6BFCF8B441E81A9501E638`.
Uses the same pinned NTSC ROM/native/disassembly revisions documented in
SHINESPARK-COMBO.md. Regenerate with `native-shine-storage-entrypoint.patch`
and bounded/dialog-free `--diagnostic-shine-storage ROM NEW.csv`; compare using
`--shine-storage-audit ROM CSV`. Temporary native hooks removed after capture.

This establishes the underlying stored lifetime, not the real-input launch
window, charged-shot delays to palette processing, or liquid/energy behavior.

## X-Ray cancellation

Pinned source `upstream-sm/src/sm_91.c`, Samus_HandleTransitionsB_5 ($91:EEA6),
clears samus_shine_timer and replaces the special-palette handler with X-Ray.
The managed X-Ray initializer installed its own palette state but left the
separately modeled shinespark timer/palette owner active.

`SuperMetroid.DebugRunner --xray-stored-shine-audit ROM` reproduces this through
the real gameplay frame dispatcher and Run input. Constructed flat-floor room,
standing left/right, selected HUD X-Ray, with/without equipped scope, stored
shine remaining 2/170/180 frames. Stored charge is seeded through the production
storage routine; palette updates age it before the one Run-input frame. No
controller route or native-CPU capture is claimed by this focused regression.

Before the fix, all six accepted X-Ray cases incorrectly retained the stored
phase and decremented its timer rather than clearing it. Six missing-equipment
rejection controls passed. X-Ray activation now relinquishes the shinespark
palette owner and clears the stored timer/phase at the successful initialization
boundary, after every rejection check. All 12 cases pass, including zero stored
palette type/frame after successful activation. Rejected activation continues
normal stored-charge countdown.

This is a reproduced, source-cross-checked cancellation fix, not completion of
the broader parity ticket. Remaining scope includes pinned original-CPU storage
expiry/input-window comparisons, grounded/aerial launch restrictions, suitless
timing, liquid/sand trajectories and energy behavior. #466 separately covers
crash/echo lifetime parity. Do not mark #465 ready based solely on this test.
