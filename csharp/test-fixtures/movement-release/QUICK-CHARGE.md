# #447: delayed held-dash quick charging

No production defect found. Original cartridge execution and the real managed
frame path agree for all 8,082 frames in 82 independent trajectories.
Player confirmation remains; multi-tap and stutter techniques belong to #446.

## Provenance and reproduction

- ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- Accepted CSV SHA256: `808E12F14CD82102B5955A20BFC567D6D4C4ECCE3710BD60EC1A887ACB671A8A`.
- Two independent native captures have identical hashes.
- Source scope: https://wiki.supermetroid.run/Quick_charging and #394.

Extract `quick-charge-447-v2.zip` and run:

```text
SuperMetroid.DebugRunner --quick-charge-audit "Super Metroid.smc" quick-charge-447-v2.csv
```

To regenerate, apply `native-quick-charge-entrypoint.patch` in the pinned native
tree, build Release x64, then invoke the dialog-free bounded CPU entrypoint:

```text
sm.exe --diagnostic-quick-charge "Super Metroid.smc" NEW.csv
```

Remove the native hooks afterward. The probe invokes original ROM subroutines,
not their translated movement implementations. Each case resets CPU and RAM.
No save data, cross-room controller route, invincibility or ammo cheat is used.

## Setup and exact property

Both facings, dash delays 0 through 40 frames, from rest at X1024/Y491 on a flat
row-32 floor in a 144x80-block room. Only Morph Ball and Speed Booster are equipped.
Forward is held from frame zero; dash is pressed at the selected delay and held.
On the frame after the counter first reaches stage four, input changes to
Down+Dash. The sequence ends only after actual spark storage.

The CSV records inputs, fixed-point X/Y, pose and movement type, animation frame
and timer, base/extra speed, acceleration mode, boost counter and shine timer.
All fields are compared every frame, not only the endpoint. Input timelines,
matrix order and capture hash are checked. No charge counters are hand-seeded.

The following values include the storage frame, rather than only the frame that
first turns blue. Distance is absolute displacement from the initial position;
frame indices are zero-based. Left and right agree symmetrically.

| Dash starts | Spark stored | Distance (pixels) |
| --- | --- | --- |
| 0 | 90 | 487.25 |
| 25 | 86 | 342.375 |
| 26 | 115 | 556 |

These are cartridge-derived values for the specified initial animation phase,
not copied wiki distances. The one-frame-late neighboring input misses the
shorter charging window. Explicit assertions protect these three timing/distance
witnesses, Stored phase and end-of-frame shine timer 179 in addition to equality.

## Diagnostic corrections, not gameplay fixes

The first comparison omitted the frontend's audio publication callback; adding
the real callback matched the native queue-dependent stage-four reset. Pinned
`bank_90.asm` at $90:852C documents and implements the sound-call accumulator
clobber. It is already preserved in production, not normalized by this test.

The initial native extraction also omitted $91:D6F7 palette dispatch, leaving the
newly stored timer at 180 rather than its end-of-frame value 179. Accepted v2
executes that original dispatch and matches the managed frame. Neither finding
required changing production behavior. The v1 capture is not accepted evidence.

This covers the pinned NTSC ROM and a flat dry runway. It does not establish
multi-tap/stutter, heated-room audio occupancy or other-revision parity.
