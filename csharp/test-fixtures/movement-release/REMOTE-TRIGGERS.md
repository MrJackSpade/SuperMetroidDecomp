# #463: remote collision triggers — partial evidence

Reference: https://wiki.supermetroid.run/Hitbox_Manipulation, Checking section,
revision 10438. **This is not yet a completed issue:** ceiling-contact paths, moving
item acquisition and native post-trigger item coroutine timing remain. Direct
horizontal, compact-pose expansion and moving spin-turn door paths are covered.

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

## Horizontal item owner and managed acquisition

`native-remote-item-probe.h` repeats the same 2,754-probe matrix, replacing the door
with type-$B/BTS-$45 and a visible exposed missile owner. Original `$94:967F`
executes the real item detector setup `$84:EEAB`, whose scan locates the seeded owner
and writes `$00FF` into its trigger timer. This is not a manually written expected
trigger. It captures that timer and retained owner header alongside collision and
position results. It does not run the subsequent native PLM coroutine.

- CSV SHA256: `86F1FB74D050FBA44FC68FCF63225FEC6ECF860F578331AEBBC2027D8B2A7FE5`.
- Archive: `remote-item-463-v1.zip`; same ROM/native/disassembly pins as above.
- Use `native-remote-item-entrypoint.patch`, then `--diagnostic-remote-item ROM CSV`.
- Managed comparison: `--remote-item-audit ROM CSV`.

The managed fixture loads an actual retail exposed missile definition through the
normal room-population loader. Only the one-record population list is overlaid.
It executes the first item draw before probing, matching the native visible seed.
Every original trigger result is then checked against **actual managed acquisition**:
the subsequent PLM step must publish one pickup event, add five current/max missiles,
and set permanent collected-item bit zero. Negative cases must do none of these.
The native trigger word is compared to this managed outcome, not claimed to be a
raw field or cycle-equivalent comparison of the native acquisition coroutine.

All 2,754 cases match. No production change was needed. The initial fixture omitted
the first draw step and correctly failed acquisition; fixing that fixture phase
alignment made the authored level words equivalent. Native acquisition timing,
full-frame movement/momentum and vertical trigger paths remain open work under #463.

## Reproduced defect: pose checks falsely start crumble blocks

The vertical pose-expansion investigation found 144 failures in 208 original-CPU
cases. Expanding from down-aim falling ($2D, radius ten) to normal falling ($29,
radius nineteen) above a contact-crumble block incorrectly changed its tile to
$80BC and allocated a live crumble PLM. Original `$91:F404` leaves the block $B000
and no active PLM. Position and pose themselves already matched.

`$94:96E3` sets collision direction to F before testing clearance. Crumble setup
`$84:CE37` requires direction three; it deletes its temporary PLM for the probe.
Managed code instead used positive probe displacement as proof of downward contact.
The fix passes the named non-directional probe value through the block-reaction
seam and gates crumble activation on the actual direction, preserving the existing
position/collision result and ordinary contact behavior.

Artifacts: `native-pose-crumble-probe.h`, `native-pose-crumble-entrypoint.patch`,
`pose-crumble-463-v1.zip`; CLI `--diagnostic-pose-crumble ROM CSV` / managed
`--pose-crumble-audit ROM CSV`. Same pinned sources as above. CSV SHA256:
`FB6559974148F212EEFB83864921D29FA0ECEE1BB3BDFD0BD588E89A7F50608E`.
Two native captures are identical. Matrix: thirteen gaps, both NMI scan parities,
all eight contact-crumble BTS variants. All 208 cases match after the fix. Paired
source-backed actual downward-contact controls still create one PLM and turn the
block solid; those controls are distinguished from the original-CPU CSV.

This fixes the reproduced crumble side effect, not all remaining directional
callbacks. Station, hand and sand observation behavior still needs its own exact
comparison, as do the positive vertical item/door triggers and full movement paths.

## Positive pose-expansion triggers above and below

The positive vertical subset now has 104 original `$91:F404` comparisons: doors
and exposed missile owners, above/below Samus, thirteen gaps and both NMI scan
parities. Source pose is down-aim falling ($2D), target ordinary falling ($29).
Native performs its own complete prospective-pose collision handling, rather than
seeding a trigger or calling a replacement collision routine.

Artifacts: `native-pose-trigger-probe.h`, `native-pose-trigger-entrypoint.patch`,
`pose-trigger-463-v1.zip`; native `--diagnostic-pose-trigger ROM CSV`, managed
`--pose-trigger-audit ROM CSV`. Same source pins; two captures are byte-identical.
CSV SHA256: `D226C25CF44C0A12037EB06B6C33A474A59957A8DB8AF076B6DAFFC4D59152C8`.

All cases match. Named assertions establish the exact success/failure boundary:
below gap eight triggers and gap nine fails; above gap nine triggers and gap ten
fails. Gaps are defined by the authored centers in the probe; do not interpret
these as a general maximum-distance statement independent of hitbox/subpixels.
Every case preserves complete X/Y fixed-point position and horizontal base speed.
Items execute the actual managed acquisition handler and are checked for five
current/max missiles and the persistent collection bit. As in the horizontal
item matrix, native capture observes owner-triggering rather than the subsequent
native message/pickup coroutine. Doors publish the actual pending transition.

No production fix was necessary for these positive paths. The shared item-list
fixture now accepts a row and its previous 2,754 horizontal cases still pass.

## Moving spin-turn door checks

918 room-local movement cases produce 6,402 frames through the first door trigger
or a 24-frame failure limit. Both facings, extra speeds zero/two/four, seventeen
initial gaps, and nine turn timings. Samus starts spinjumping with matching previous
pose history, base speed 1.25, downward direction, zero vertical speed, and held
Jump+forward. Opposite direction is then held from the selected turn frame. Inputs,
movement, collision, pose transitions and animation execute in native frame order;
the managed comparison uses `SuperMetroidRuntime.StepFrame`.

Artifacts: `native-moving-door-probe.h`, `native-moving-door-entrypoint.patch`,
`moving-door-463-v1.zip`. Native `--diagnostic-moving-door ROM CSV`; managed
`--moving-door-audit ROM CSV`. Same pins as above; independently repeated captures
have SHA256 `0C9637F22CC80465BAC7B22987A5D4141280FB98CDDAFA66ED7C0390E99B7A84`.

Every frame compares X/Y/subpixels, pose/movement, animation frame/timer, base/extra
horizontal speed, acceleration mode, facing, vertical speed/direction, flare count,
active movement radii and actual pending door pointer. The run ends at the first
door request; it does not traverse into another room or infer transition rendering.

390 cases actually trigger while the complete horizontal hitbox remains outside
the door column. Named witnesses with zero extra speed and immediate reversal:
right-going seed gap nine / left-going gap ten trigger on frame one, retain base
speed `$0000:E000` and reversal mode one. The adjacent seed gap fails for all 24
frames. The one-pixel left/right boundary difference follows the captured native
scans. These seed gaps are not identical to distance at the later probe frame.

All 6,402 frames match; no production change was needed. This supplies the
velocity-dependent door movement evidence missing from the direct-probe matrices.
Moving item collection and ceiling-contact sequences are still separate gates.

## Native acquisition handler boundary (#463)

`item-acquisition-463-v1.zip`, `native-item-acquisition-probe.h`, and
`native-item-acquisition-entrypoint.patch` extend the direct horizontal collision
evidence through the original PLM handler, rather than inferring an award from
the detector timer. Same ROM/source pins as above. Two independent captures have
SHA256 `AF61194A7FD39C287F711FA1518E48D03B11D51AA3EA85538634BF695C06B8A8`.
Native command: `--diagnostic-item-acquisition ROM CSV`; managed command:
`--item-acquisition-audit ROM CSV`.

408 cases cover both directions, seventeen gaps, three blocker arrangements and
all four remaining visible-item animation timers. The native seed is the exposed
missile loop: next draw `$84:E0CE`, preinstruction `$84:DF89`, link `$84:E0D6`.
The original wall probe `$94:967F` runs with signed eight-pixel displacement;
the original handler `$84:85B4` then runs until return or message entry `$85:8080`.
At message entry the real pickup opcode has already awarded ammo and the preceding
instruction has set the permanent bit. No message coroutine or artificial NMI is
executed. Each case resets CPU/RAM; the suspended coroutine is not resumed.

The C# fixture reaches corresponding timer phases with production PLM steps,
asserts collision has not yet awarded resources, then performs exactly one handler
step. All 408 match: 128 same-pass pickups with message 2, five current/max missiles
and item bit zero set; 280 negative controls with none of those effects. An upper
solid blocker suppresses pickup; a lower blocker does not undo the earlier trigger.
No production change was needed. This validates acquisition timing at the PLM
boundary, not message dismissal, full moving-item trajectories or ceiling-contact
sequences; those latter movement gates remain open.

## Moving-item acquisition frames (#463)

`moving-item-463-v1.zip`, `native-moving-item-probe.h` and
`native-moving-item-entrypoint.patch` use the moving-door trajectory matrix above,
replacing the door with an exposed missile owner. The same pinned original ROM
runs all movement stages and the actual PLM handler through message entry. Each
case ends on the pickup frame or after 24 unsuccessful frames; message dismissal
and later movement are outside this capture. All 918 cases / 6,402 frames match C#
`SuperMetroidRuntime.StepFrame` with gameplay cheats disabled.

Native `--diagnostic-moving-item ROM CSV`; managed `--moving-item-audit ROM CSV`.
Two independently repeated captures have SHA256
`D181C67647DD84414C4BA273E45FE89B43F27534D9422219C3A3DD308901FEBA`.
Every captured frame compares the movement/animation/radius fields listed in the
door section, plus the actual message index, current/max ammo and collected bit.
390 pickups occur while the complete horizontal hitbox remains outside the item
column. The same zero-extra-speed, immediate-turn witnesses retain base speed
`$0000:E000` and reversal mode one; the adjacent gap fails. The shared door fixture
edge constants intentionally describe the identical geometry here.

No production change was necessary. This closes the moving-item trajectory gap;
ceiling-contact sequences and remaining direction-sensitive negative controls
still prevent marking the overall issue ready for player validation.
