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
