# #465 Shinespark storage and launch investigation

Status: partial implementation; not yet awaiting player validation.

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
