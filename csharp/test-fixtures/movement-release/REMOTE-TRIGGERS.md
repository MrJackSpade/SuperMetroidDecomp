# #463: remote collision triggers — partial evidence

Reference: https://wiki.supermetroid.run/Hitbox_Manipulation, Checking section,
revision 10438. **This is not yet a completed issue:** item acquisition, vertical
checks, pose-change checks and velocity-dependent full movement sequences remain.

## Horizontal door probe

- ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- CSV SHA256: `A2FF87BAC384E65CEFC7323F6C3F66E3ED07739E067CD01A176E00A415F20D58`.
- Two original-CPU captures are byte-identical; archive: `remote-door-463-v1.zip`.

Apply `native-remote-door-entrypoint.patch`, build the pinned native tree and run:

```text
sm.exe --diagnostic-remote-door "Super Metroid.smc" remote-door-463-v1.csv
SuperMetroid.DebugRunner --remote-door-audit "Super Metroid.smc" remote-door-463-v1.csv
```

The headless probe calls original `$94:967F` with DP `$12/$14` displacement,
not the translated wrapper. It records position/subpixels, collision flag,
returned absolute distance, native door pointer and game state. Remove the hooks
after capture; no ordinary GUI launch is required.

2,754 independent probes cover both directions, 17 gaps, three fractional X
positions, distances 1 through 9, and no blocker / upper blocker / lower blocker.
The 16x16 synthetic room has a door in row eight, at column nine or six. Samus
has radius 5x12 and center Y136, so horizontal scanning visits rows seven to nine.
The real Landing Site door list at `$8F:927B` resolves BTS zero to `$83:8916`.

All probes match `SamusBlockCollision.ProbeWallHorizontal`. This executes the real
production collision dispatcher and publishes `RoomLevelData.PendingDoorTransition`;
it is not replaced by a read-only test probe. The native state-nine observation is
compared to the managed pending request's presence, **not** claimed as a frontend
state-transition comparison. No destination room is loaded in this test.

Named witnesses confirm upper-solid early termination suppresses the middle-row
door, while a lower solid leaves the already-published door request intact. Gap
seven triggers with an eight-pixel probe; adjacent gap eight fails, in both
directions. Returned distance remains eight pixels on the unblocked trigger path.
Every captured integer position remains observational; collision subpixel writes
are compared rather than discarded. Full movement momentum remains a separate gate.

No production change was required for this subset. Do not mark #463 awaiting
validation based on these horizontal-only results.

## Remaining diagnostic detail

Pinned `$94:967F` and `$94:96AB` set collision direction to `$F`, intentionally
suppressing directional station/save/hand/crumble callbacks. The current shared
movement wrapper should be checked for these negative side effects as coverage
expands; passing the door-only matrix does not establish their correctness.
