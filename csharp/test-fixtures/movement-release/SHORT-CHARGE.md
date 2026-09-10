# #446: multi-tap and stutter short charges

No production defect found in the scoped reproduction. All 16,358 original-CPU
frames match the managed production frame path across 126 trajectories. Player
confirmation remains. #447 separately covers delayed held dash.

## Provenance

- Technique source: https://wiki.supermetroid.run/Short_charge (revision 9532).
- ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- CSV SHA256: `2BA37E14FD5F3B7A762EF6A5F20520A98FF7C926A782FB3EC974A21C653A85DA`.
- Two independent native captures have identical hashes.

Pinned disassembly cross-checks: $90:852C gates animation-linked counter changes
on held dash and preserves the queue-dependent accumulator clobber; $91:E634
initializes the running boost timer; $91:F7B0 stores a spark during crouching
when the high-byte stage is at least four. The probe executes those original
instructions through the real frame stages, including $91:D6F7 palette timing.

## Inputs and fixture

Both facings, 2/3/4 taps, seven forward patterns, and tap timing shifts -1/0/+1.
The flat 144x80-block room, floor row 32, X1024/Y491 and initial standing animation
match #447. Samus starts at rest with Morph Ball and Speed Booster only; no boost
counter, momentum, cheat or save is injected.

Tap frames are 25, 50, 70, 85 (zero-based). The final tap is held; earlier taps
last one frame. Forward-release patterns are none, `11_13`, `9_14_`, `5_7_10_`,
`3_4_5_9_`, an early release on frame 1, and a long release on frames 11–40.
The underscores mean one released frame. After reaching stage four, Down+Dash
on the next frame stores the spark. Each case stops at actual storage or frame 180.

The initial six-frame release candidate still succeeded natively. Accepted v2
uses a genuinely long release instead of declaring that tolerated input wrong.

Every frame compares held input, X/Y subpixels, pose/movement, animation and
timer, base/extra speed, acceleration, boost counter and stored-spark timer.
Capture hash, row order, input generation and matrix count are enforced.
Additional property assertions cover stage changes on 25/50/70/85, incremental
extra speed for one-frame taps, Stored phase, and exact terminal distance/time.

## Cartridge-derived observations

All correctly timed normal and useful stutter cases store on frame 86. Their
stage progression is identical despite different dash acceleration and travel.
Early/long forward releases and one-frame-late taps miss this short window;
the continued held final tap can eventually recover and charge later.

Distances below are pixels from the initial position through the storage frame.
These are measured for this fixture's exact animation phase, not copied wiki
distance totals. The audit asserts every entry symmetrically for both facings.

| Forward pattern | 2 taps | 3 taps | 4 taps |
| --- | ---: | ---: | ---: |
| None | 268.125 | 236.0625 | 227.75 |
| Single | 241.6875 | 209.625 | 201.3125 |
| Double | 217.4375 | 185.375 | 177.0625 |
| Triple | 210.3125 | 178.25 | 169.9375 |
| Quadruple | 207.875 | 175.8125 | 167.5 |

## Re-run

Extract `short-charge-446-v2.zip`, then:

```text
SuperMetroid.DebugRunner --short-charge-audit "Super Metroid.smc" short-charge-446-v2.csv
```

To regenerate, apply `native-short-charge-entrypoint.patch` to the pinned native
tree and build Release x64. Invoke only the dialog-free bounded diagnostic:

```text
sm.exe --diagnostic-short-charge "Super Metroid.smc" NEW.csv
```

Remove the temporary hooks afterward. Scope: pinned NTSC flat dry room, published
single through quadruple stutters and neighboring controls. This does not claim
optimality, every possible stutter string, PAL parity or terrain-dependent routes.
