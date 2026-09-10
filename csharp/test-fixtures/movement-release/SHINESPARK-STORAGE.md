# #465 Shinespark storage and launch investigation

Status: partial implementation; not yet awaiting player validation.

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
