# #463: remote collision triggers — reproduction and verification

Reference: https://wiki.supermetroid.run/Hitbox_Manipulation, Checking section,
revision 10438. The pinned-NTSC implementation/reproduction gates are covered;
player confirmation remains. The sections below record incremental evidence;
their older remaining-work notes are historical. The completion review at the end
maps the issue requirements to the accepted fixtures. No cross-room routes or
claims about other ROM revisions are implied.

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

## Station direction rejection (#463)

`native-station-probe.h`, `native-station-probe-entrypoint.patch` and
`station-probe-463-v2.zip` compare ordinary `$94:9543` contact with observational
`$94:967F` checks. 120 cases cover map/energy/missile station access from both
sides, real versus observational contact, and ten gaps. Samus uses the matching
ran-into-wall pose and cannon-aligned Y=139; the resident parent has a sentinel
timer. Original station setup changes it to one only on accepted activation.
Managed comparison checks actual station trigger state, not just solidity.

Two captures match SHA256
`7214D1C1E3ABCB039ABD9EAA3D599908ADF30ED2F68632E0A6D393A67B8793AC`.
Commands: native `--diagnostic-station-probe ROM CSV`; managed
`--station-probe-audit ROM CSV`. Same source/ROM pins as above. V2 explicitly reads
the observational collision output latch rather than relying on residual CPU carry.

Before the fix, 48 observational cases incorrectly activated their station.
The native probe writes direction $F, and station setups reject it before parent
lookup. C# reused ordinary left/right movement and lost that distinction. The
horizontal dispatcher now carries the named non-directional probe override and
skips station activation in that case, preserving solid clipping and unrelated
item/door/scroll side effects. All 120 comparisons pass after the change. This
fixture deliberately isolates the access tile from other station graphics; it
does not claim a normal walljump can use a ran-into-wall pose. It verifies the
shared dispatcher contract independently of current callers' pose restrictions.
Vertical save/hand/sand checks remain a separate coverage gate.

## Vertical save-access direction (#463)

`native-save-probe.h`, `native-save-probe-entrypoint.patch`, and
`save-probe-463-v1.zip` compare `$94:9763` downward movement with `$94:96AB`
changed-pose observation. 80 cases cover real/probe direction, centered/off-center
X, ten gaps and both scan parities. The seven-pixel displacement stays below the
intermediate-probe threshold. The fixture isolates a resident save station's real
setup-created access tile at row ten, column eight; Samus has standing pose one
and radii five/twelve. It compares collision and actual owner activation, not the
different position-commit contracts of the two entrypoints.

Both native captures have SHA256
`7DF0EFBAE344C53029170497C3A575363CBFFED8704F910EF3CB6C7657698120`.
Native command `--diagnostic-save-probe ROM CSV`; managed
`--save-probe-audit ROM CSV`. Same ROM/source pins as above.

Before the fix, 16 centered direction-$F probes incorrectly activated the station.
The vertical dispatcher already received the probe identity but only used it for
crumble-block rejection. It now also rejects station setup before parent lookup,
matching `$84:B590`. All 80 comparisons pass afterward, retaining ordinary centered
landings and rejecting off-center ones. The managed fixture exercises the same
vertical dispatcher used by pose expansion; it does not claim to reproduce a full
player pose-transition trajectory. Hand/sand controls and ceiling-contact sequences
remain outstanding.

## Vertical quicksand direction (#463)

`native-sand-probe.h`, `native-sand-probe-entrypoint.patch`, and
`sand-probe-463-v1.zip` compare ordinary `$94:9763` movement with `$94:96AB`
observations at the Maridia surface-sand entry. The 32-case matrix covers both
signed displacements, all four vertical-direction states, ordinary/probe identity
and contact-damage indices zero/one. Two independent captures have SHA256
`17601C2854F9699AE262AE02E969DCFD2F087A432151266B82DC102985FDC400`.
Commands: native `--diagnostic-sand-probe ROM CSV`, managed
`--sand-probe-audit ROM CSV`. Same pinned ROM/source revisions as above.

Before the fix there were five displacement/collision mismatches: stationary
direction-$F probes incorrectly inherited downward sinking/clamping or collision,
and ordinary upward displacement lost the sand-contact carry when vertical state
was falling. The native B4C4 setup requires direction three for stationary states,
but its falling branch remains active for direction F. The managed reaction now
receives that direction identity, and ordinary vertical movement publishes sand
contact for either displacement sign, as `$94:9763` does.

All 32 cases now match absolute accepted displacement and collision. The shared
reaction's separate sand-contact output is also checked against the native flag,
including falling probes that report sand but return carry clear. This is a
dispatcher comparison using a live Samus owner; it does not cover the pose-copy
owner/contact-damage wiring or the horizontal sand-probe path. Those remain open
alongside the hand and ceiling-contact coverage gates.

## Full pose-change sand inputs and writes (#463)

`native-pose-sand-probe.h`, `native-pose-sand-entrypoint.patch`, and
`pose-sand-463-v1.zip` run original `$91:F404` for falling down-aim to ordinary
falling (`$2D -> $29`). 104 cases cover thirteen vertical gaps, both scan parities,
surface/submerging sand and contact-damage modes zero/one. Samus starts at X=136,
Y=150-gap, Y fraction `$3456`, vertical speed `$0005:4000`, gravity `$0001:3000`,
base X speed `$0001:4000`, falling direction and no solid enemies. The isolated
sand tile is row ten/column eight, Maridia type-three BTS `$80` or `$83`.

Two original-CPU captures match SHA256
`9157D53D347C8A15441BCAD2F355090B39B6281C8B47093702E1C80A31C783F1`.
Native command `--diagnostic-pose-sand ROM CSV`; managed
`--pose-sand-audit ROM CSV`, through `TryApplyCompactAerialTransition` rather than
direct collision. Same ROM/source pins as above. Every case asserts X/Y/subpixels,
final pose, vertical speed/subspeed and gravity/subgravity.

Before the fix, 54 cases failed. Ownerless geometry copies lost contact damage,
so surface collision failed to adjust the expanding body's center. Submerging
sand cleared speed/gravity on the copy but those native side effects were thrown
away. The copy now snapshots only the needed contact-damage input, and the caller
copies back the four speed/gravity words after each vertical observation, without
committing speculative position. It remains ownerless to avoid implicitly enabling
other owner-dependent reactions. All 104 cases match after the fix. This closes
the vertical pose-copy sand gap; horizontal sand probes, hand checks and moving
ceiling-contact sequences remain outstanding.

## Horizontal sand observation (#463)

`native-wall-sand-probe.h`, `native-wall-sand-entrypoint.patch` and
`wall-sand-463-v1.zip` compare ordinary `$94:971E` movement with `$94:967F` wall
observation. 64 cases cover both directions, ordinary/probe identity, surface and
submerging sand, all vertical direction states and contact-damage modes zero/one.
The tile is isolated at row eight/column six or nine in Maridia; X is 117 or 139,
Y=136, X/Y fractions `$4000/$3456`, radii five/twelve, speed `$0005:4000`, gravity
`$0001:3000`, and signed displacement is seven pixels. Same ROM/source pins above.

Two captures match SHA256
`DF1234C0823060523974DD72BE8019BD0BC922A8D0746F290412E29172740359`.
Native command `--diagnostic-wall-sand ROM CSV`; managed
`--wall-sand-audit ROM CSV`. All cases compare collision, absolute accepted
displacement, X/Y/subpixels, speed/subspeed and gravity/subgravity. Unlike the
vertical direct-dispatch capture, the managed observational wrapper is exercised.

Before the fix, 18 cases failed. The wall copy omitted area selection and discarded
speed/gravity writes. The surface handler categorically rejected horizontal scans
despite native direction F passing its bit-one test; the horizontal caller also
ignored its carry. The wrapper now preserves the sand inputs/writes, and the
shared reaction uses native direction identity with its actual collision result.
Ordinary horizontal surface movement remains carry clear. Falling contact-damage
wall probes stop without changing position or subpixels; submerging sand clears
motion words even when observational. All 64 native comparisons pass. Chozo-hand
and moving ceiling-contact evidence remain outstanding for the overall issue.

## Chozo hand direction and actual side effects (#463)

`native-hand-probe.h`, `native-hand-probe-entrypoint.patch`, and
`hand-probe-463-v1.zip` cover 32 cases: both statue variants, ordinary downward
movement versus direction-F observation, all four ground morph/spring-ball poses,
and progression admission enabled/disabled. Native `$94:9763`/`$94:96AB` execute
the actual area-selected hand setup. Two captures match SHA256
`C6211AFA88A028CEE251A2EE580352253E04EE3A3D3B01AD25CF911E35D7F2E4`.
Commands: native `--diagnostic-hand-probe ROM CSV`; managed
`--hand-probe-audit ROM CSV`. Same ROM/source pins as above.

The native synthetic room has ample space for hardcoded PLM spawns. The managed
fixture loads each retail statue population and its real callbacks, then isolates
the equivalent access cell at (8,10). The test explicitly supplies the collision
probe identity to the production vertical dispatcher. It compares unconditional
solid collision, actual enemy parameter one, hand block type/visual bits, and the
Lower Norfair event. It is not a complete player pose-transition trajectory or
an animation comparison. The boss flag is cleared after population loading for
Wrecked Ship's negative control, so rejection is tested against a resident owner.

Before the fix, six observational cases incorrectly activated the statue, erased
hand collision and (in Lower Norfair) set the event. The hand call used displacement
sign without the direction-F override. It now requires the native downward
direction while retaining the setup's unconditional solidity. All 32 match after
the fix: three admitted poses work during real eligible contact, left-facing morph
ball is rejected, and all observational contacts reject. Moving ceiling-contact
sequences remain the outstanding #463 gate.

## Moving ceiling/door scan ordering (#463)

`native-ceiling-door-probe.h`, `native-ceiling-door-entrypoint.patch`, and
`ceiling-door-463-v3.zip` compare 108 cases / 993 original-CPU movement frames.
Both facings, extra speeds zero/two/four, ceiling present/absent, and nine initial
Y offsets are covered. Same pins as above; two captures have SHA256
`54C2AA986B8EC1BA37964BFF23C5700CF98DDC5BA0F18D4619FF97A48A5396DF`.
Commands: native `--diagnostic-ceiling-door ROM CSV`; managed
`--ceiling-door-audit ROM CSV` through `SuperMetroidRuntime.StepFrame`.

Samus starts at X=1024/Y=476+offset, spinjumping up at speed four with base X speed
1.25 and running-momentum flag set. Jump+forward stay held. Row 28 has a door at
column 64 (right-facing seed) or 63 (left-facing seed); the optional adjacent solid
occupies the other column. The first door request ends the case; failures run 64
frames. The fixture does not cross into a new room. Initial v1 geometry produced
only ordinary falling contact and was rejected; v2 lacked the momentum flag and
collapsed the intended entry-speed variants, so only v3 is accepted.

Twelve cases trigger while the entire head remains below the ceiling/door row:
the scan visits the door before the neighboring solid clips Samus outside it.
All retain base `$0001:6000` plus their extra speed. Right-facing, speed-zero,
offset-four triggers on frame one at Y=476; adjacent offset-three hits the solid
first and never triggers. The test asserts those witnesses and every captured
position/subpixel, pose/animation, horizontal/vertical speed and door pointer.
All 993 frames match without production changes. Final acceptance review found
that down/back still needs a moving door-trigger witness; direct pose expansion
and the separate #461 movement audit are not substitutes for that side effect.

## Moving down/back door trigger (#463)

`native-downback-door-probe.h`, `native-downback-door-entrypoint.patch`, and
`downback-door-463-v2.zip` cover 1,836 cases / 28,818 original-CPU frames. Both
facings, three extra speeds, seventeen vertical gaps, nine input timings and
forward-only versus Down+back controls are exercised. Jump remains held. Samus
starts spin-falling at X=1024, Y=461-gap, base X speed 1.25, zero Y speed and
running momentum enabled; row 30 is a door boundary. The first actual door request
ends each case; failures end at frame 24. No destination room is traversed.

Two captures match SHA256
`8715112C906D7EEF7C91968B46FDAB2A1EBAB51D21DF51D2E9516ED5504229C3`.
Commands: native `--diagnostic-downback-door ROM CSV`; managed
`--downback-door-audit ROM CSV` through full `SuperMetroidRuntime.StepFrame`.
Same ROM/source pins as above. The initial side-door layout produced no remote
down/back witnesses and was rejected; only the lower-door v2 matrix is accepted.

150 cases trigger while the movement-phase hitbox still ends above the door row.
The Down+back input breaks spin into the short down-aim pose, then its turn expands
the body and executes the trigger check. Zero extra speed, gap zero and input
delay three trigger at frame four while base speed remains `$0001:4000` in reversal
mode one. Every frame compares position/subpixels, pose/animation, horizontal and
vertical motion, radii and the actual door pointer. Forward-only controls and the
adjacent gap/timing cases are included in the same pinned matrix. All 28,818 frames
match without production changes.

## Completion review

| Requirement | Accepted evidence |
| --- | --- |
| Walljump-check item and door effects | Remote item/door matrices plus full moving-item/moving-door frame captures |
| Real acquisition rather than notification alone | Item-acquisition handler boundary: resources, collected bit and message |
| Upper-solid early termination and trigger distance | Mirrored 2,754-case direct matrices, upper/lower blockers, adjacent reach failures |
| Ceiling checks and retained momentum | 993-frame ceiling/door capture, door-first success versus solid-first rejection |
| Down/back checks and retained momentum | 28,818-frame down/back capture with forward-only controls |
| Native special-block rejection and side effects | Crumble, station, save, hand, vertical/horizontal sand and full pose-sand captures |
| Real production paths and isolated room scope | Runtime movement sequences plus actual dispatcher/PLM owners; no multi-room route |
| Repeatability, revision and safe state | Pinned ROM/source hashes, repeated CPU captures, archived fixtures, fresh disposable state and no gameplay cheats |

Production mismatches found and fixed during this issue were directional crumble,
station/save/hand activation, sand direction/carry, and lost probe sand inputs or
speed/gravity writes. Source/fixture boundaries are documented per section; direct
dispatcher tests do not stand in for the separately captured movement sequences.
PAL/other revisions were not tested and are not claimed compatible by this audit.
The issue should remain open with `awaiting-player-validation`, not be closed on
these automated results alone.
