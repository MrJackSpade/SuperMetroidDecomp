# Pause-separated Bomb Spread charge carry (#471)

`pause-charge-native-capture.zip` contains `pause-charge-471-v2.csv`, SHA-256:
`5B1C12F4F9D0B657A7936989D7041D5BC5E282EBE1F0A7125C6E64D7A2C5A4B2`.
An independent second native run produced the same hash.

Run `SuperMetroid.DebugRunner --pause-charge-comparison-audit "Super Metroid.smc" pause-charge-471-v2.csv`.
There are 124 cases / 35,960 frames, all matching after the fix. The comparator
validates the capture hash, complete matrix, input schedule and sequential frames.

## Native scope and fixture

ROM: Japan/USA rev 0, SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native harness: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
No PAL or other-revision equivalence is claimed.

Unlike the earlier isolated fade recurrence, this probe executes the actual
cartridge game-state entries: $82:8B44, $82:8CCF, $82:8CEF, $82:90C8, $82:90E8,
$82:9324, $82:9367 and $82:93A1. Thus it includes gameplay, pause admission,
the complete menu setup/fades/input filter, graphics restoration and the Samus
equipment reconciliation callback. None of those ROM instructions are patched.
The CPU subroutine harness supplies one controller sample and frame counter per
iteration and drains pending draw queues between iterations; it does not model
scanline/NMI cycle timing or compare raster/audio output.

Both sides use Landing Site metadata with an empty flat room, floor at block row
16, no enemies or projectiles initially, standing at X=1000/Y=235 with zero
subpixels, animation frame 0/timer 1, health/max health 99, Morph Ball, Bombs and
Charge Beam. Speed Booster is equipped in half the cases. Each setup is tested
facing left and right. No cheats, player SRAM, state slots or relevant RNG are
used. The native fixture installs the normal empty enemy-draw hook and empty
pause hooks; retail metadata supplies valid decompression/map pointers to the
real graphics teardown. C# enters gameplay through a disposable in-memory save
and uses the production `SuperMetroidGame.StepCaptured` path.

Shoot begins at frame 0, Run/forward at 30 and Jump at 70. Start+Down removes
Run/forward at the swept frame. Down stays held during the 30 gameplay-darkening
frames, then releases throughout the menu. After one neutral interactive-menu
frame, Start is held until the native delayed-input filter accepts it. The first
resumed gameplay frame presses Down again; subsequent frames add forward. Down
releases after 51 resumed frames, and the trace runs 14 more frames.

- Without Speed Booster: Start frames 105–135. 105–124 bounce, 125–131 soft-morph
  without bouncing, and 132–135 miss the airborne soft-morph window.
- With Speed Booster: Start frames 115–145. 115–134 bounce, 135–141 soft-morph,
  and 142–145 miss the airborne window. The cartridge's different jump height
  is preserved rather than forcing both variants into one timing window.
- Successful soft morphs retain charge, do not produce bombs while Down is held,
  and release five bombs on its release. Early bouncing cases are not claimed
  to lose charge; the test distinguishes the bounce from the soft-morph technique.

Every frame compares dispatcher state before/after, brightness, X/Y and
subpixels, pose/movement/animation timers, base and extra horizontal speed,
acceleration mode, vertical speed/direction, charge and spread timers, bomb
count, bounce state, momentum/boost counter, and each of five bomb slots' type,
instruction pointer, position, velocity and fuse. Down-edge separation and the
successful/adjacent failing windows have explicit assertions.

## Reproduced defect and fix

The first single-case comparison matched until teardown/resumption, then had
65 mismatching frames: C# retained extra sideways movement after unpausing.
The native $82:A2E3 -> command $0C -> $91:E633 path clears the momentum flag,
boost counters and echo slots when Speed Booster is not equipped. It deliberately
leaves the numeric extra-speed pair until the following movement update clears
it. C# had omitted this equipment-dependent reconciliation.

`ReconcilePauseSpeedBoosterState` now runs at that teardown boundary. It also
preserves an existing equipped boost and arms a zero boost countdown when
momentum exists and Speed Booster has been enabled. It does not reuse ordinary
momentum cancellation, which would incorrectly start departing echoes. Eight
focused core cases verify those branches, deferred speed clearing and echo reset.
This implements the speed/echo part of the native equipment callback; it is not
a claim that every possible equipment-toggle pose or sound has been audited.

The full native matrix is green; the earlier fade comparison remains green.
Other #471 variants have separate evidence in CHARGED-WALLJUMP.md,
CONTINUOUS-WALLJUMP.md, XRAY-CHARGE.md and SOFT-UNMORPH-CHARGE.md. Together these
cover the reported charge-carry techniques; player confirmation is still required.

## Reproduction and capture history

Apply `native-pause-charge-entrypoint.patch` inside the pinned `upstream-sm`
checkout with `git apply --unidiff-zero` (the entrypoint hunks intentionally
use a single anchor line). Build its Release x64 target and run:

```
sm.exe --diagnostic-pause-charge "Super Metroid.smc" pause-charge-471-v2.csv
```

The patch dispatches before SDL, routes explicit SDL errors to the console, and
the probe imposes a CPU instruction budget. Reverse with
`git apply --reverse --unidiff-zero` when finished.
The helper restores the unmodified ROM after the stock harness's initialization.
The accepted archive contains numeric diagnostics only, not ROM/SRAM bytes.

Exploratory setup one/two lacked the default empty enemy-draw hook and did not
produce gameplay frames. Exploration three completed one case and reproduced
the momentum mismatch. Matrix v1 used the same sweep for both inventories;
v2 shifts the Speed Booster sweep to include its later successful and failing
neighbors. Only v2 is accepted. Temporary entrypoint changes were removed after
capture, and no native test process is left running.
