# #421: Pseudo Screw investigation (incomplete)

## Original enemy-touch capture

The bounded CPU probe calls the original generic touch body at $A0:A4A1.
It substitutes a disposable synthetic enemy definition at $A0:F000 and a
vulnerability table at $B4:F000, restoring both before exit. No ROM file is edited.
Enemy health starts at 5000; vulnerability bytes are 0, 1, 2 and $82. Contact
indices three and four distinguish real Screw Attack from Pseudo Screw.
Charge and animation words are explicit handler-boundary inputs, not proof that
the player can reach that state through controller input.

- ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- CSV SHA256: `B40AA1CCD742DC0DFD921A4A18816103F6561D1C1132D262737E9AC3AE304FBC`.
- Two independent captures are byte-identical; archive `pseudo-touch-421-v1.zip`.

The native handler consumes Pseudo Screw charge **before** vulnerability lookup.
Even an immune enemy clears the flare counter, charge-palette index and flare
animation words through Samus command four ($90:F19B/$90:F19E). Screw Attack
leaves those words untouched. Vulnerability two causes 200 Pseudo Screw damage,
versus 2000 Screw Attack damage; bit seven is masked. Successful damage clears
Samus's invincibility/knockback timers and sets the enemy's hurt/flash state.
The direct immune-handler case leaves those timers unchanged; the surrounding
enemy collision dispatcher has separate timer behavior that still needs coverage.

The managed generic touch body lacked command four entirely. The repair now
uses EnemyMain's live projectile dependency before vulnerability lookup, clearing
both the Samus flare mirror and projectile-system charge/animation/palette state.
The previous-charge sampling word remains unchanged, matching the native routine.
Missing live dependency fails explicitly rather than silently retaining charge.

`--pseudo-screw-contact-audit ROM` builds charge through 125 normal input frames,
then passes that live projectile owner into the real enemy contact phase. Four
Pseudo Screw cases failed before the repair; all eight pass afterward. Checks
cover damage, both charge copies, palette index, prior-charge preservation,
visible flare removal and all sixteen restored normal-suit palette colors.
Real Screw Attack controls retain the charge, flare and existing palette.
Full core verification passes. The existing charge-free contact-death fixture now
supplies its explicit projectile owner to exercise the same dependency contract.

## Reproduce native evidence

Apply `native-pseudo-touch-entrypoint.patch` to the pinned native tree, build
Release x64, and invoke only the bounded, dialog-free entrypoint:

```text
sm.exe --diagnostic-pseudo-touch "Super Metroid.smc" NEW.csv
```

Remove the hooks after capture. Build, duplicate capture hash, and patch reapply
checks passed. The native capture remains the handler-boundary oracle; it is not
an end-to-end proof of spin pose eligibility or enemy-projectile immunity.

## Remaining before player validation

- Cover charge-producing spin/walljump inputs, released-shot retention, negative
  walljump-check poses, yellow palette and suitless-liquid restrictions.
- Verify invulnerability semantics without granting universal protection.

The issue remains open without awaiting-player-validation. The broader contract
and source are https://wiki.supermetroid.run/Pseudo_Screw_Attack and issue #394.

## Generic enemy-projectile pass verified

`native-pseudo-projectile-probe.h` calls original $A0:9894 with one overlapping
projectile in slot 17. It substitutes only a disposable touch-list definition,
restored before exit. The 288 cases cover contact modes zero/three/four, initial
invincibility zero/nine, persistent/deleting contact, damage-disabled property,
X offsets -9/0/+9, and four radius combinations including either zero axis.

The C# `--pseudo-screw-projectile-audit ROM CSV` exercises the production collision
pass against the same data. All 288 cases match health, invincibility/knockback
timers, charge, projectile lifetime and active instruction state. Two vulnerable
controls take 40 damage. All 96 Pseudo Screw cases retain charge and health and
leave the projectile alive. This is independent of the enemy-touch consumption
above: protection comes from the generic pass-entry gate, not universal immunity.
Deleted projectile instruction state is normalized to zero on both sides because
this test does not claim parity of inaccessible stale fields after deletion.

- Accepted archive: `pseudo-projectile-421-v1.zip`.
- CSV SHA256: `F273FEBAD2950CCC5BDE3CFC091F13B170F4D45EF7E9CAD15CBCBA22FBAE1F6B`.
- Two native captures are byte-identical; same ROM/source pins as above.
- Regeneration: apply `native-pseudo-projectile-entrypoint.patch`, build, then
  run `sm.exe --diagnostic-pseudo-projectile ROM NEW.csv`.
- Temporary hooks removed and reapplication checked. No production change was
  needed for this pass. The earlier eight-case contact audit still passes.

Pose publication, custom attacks bypassing this pass, yellow flash and input
retention remain separate gates; this capture does not establish those properties.

## Input-driven dry walljump and palette capture

`native-pseudo-walljump-probe.h` exercises 34 input sequences (both facings,
17 Shoot-release timings), 140 frames each. Charge is earned through input;
no contact mode, charge counter or palette is injected. Every frame compares
position, speed, pose, animation, charge, contact mode and all sixteen suit colors.
The native palette handler runs after movement and animation, as on cartridge.

Before the fix, 16 right-facing cases disagreed at frame 89: C# had normal suit
colors instead of the cartridge's yellow Pseudo Screw flash. Momentum cancellation
requests a normal suit copy during movement; C# deferred it until after charge
palette handling, erasing the newer colors. The runtime now flushes that pending
copy before palette dispatch, including drained/special handlers. This is an
ordering correction, not a frame-, direction-, or Pseudo Screw-specific override.

All 4,760 frames now match. Explicit release-80 witnesses retain charge 71 while
contact mode changes from four on frame 89 to zero on frame 95, and assert the
complete yellow palette on the previously failing frame. Full core verification
passes. Suitless-liquid eligibility remains unverified by this dry-room fixture.

- Archive: `pseudo-walljump-421-v1.zip`.
- CSV SHA256: `E7C6A3AD052DBE6466B00525798EE0B7E31F76DB5E2EDD2370A44C0276EE2BC0`.
- Two captures are byte-identical, using the same ROM/source pins above.
- Regenerate with `native-pseudo-walljump-entrypoint.patch`, rebuild, then run
  `sm.exe --diagnostic-pseudo-walljump ROM NEW.csv` (headless, dialogs suppressed).
- Managed audit: `--pseudo-screw-walljump-audit ROM CSV`.
- Temporary source hooks removed; patch reapplication checked.

## Liquid and negative-pose movement gates verified

`native-pseudo-liquid-probe.h` runs the original movement handlers at $90:A436,
$90:A734 and $90:A42E. The 5,760 cases cross both facings of normal spin, Screw
Attack, walljump and normal-jump poses; animation frames 0/2/3/22/23; charge
59/60/120; Gravity on/off; dry/water/lava/acid; surface one pixel above/equal/one
pixel below the pose's top; and water-disable bit on/off. Each case starts in an
empty 16x16 room at (128,128), zero subpixels/speeds, descending, Jump held,
animation timer two, no contact mode and no other equipment. There is no input
route or animation advance: this capture isolates one movement-handler boundary.
Seeded charge here is not a substitute for the earlier input-earned charge trace.

`--pseudo-screw-liquid-audit ROM CSV` executes the real managed aerial movement
methods in the same constructed room. All 5,760 contact-mode results agree.
Explicit witnesses cover the strict top-surface comparison and walljump's
animation thresholds. Ordinary spin loses charged contact only when fully
submerged without Gravity (unless water is disabled). Walljump ignores that
liquid exclusion: frames 0..2 have no damage, 3..22 use charged contact, and 23+
use Screw contact. Normal-jump controls do not gain contact damage from charge.
No production change was needed. This does not assert arbitrary injected poses
or animation frames are reachable with every equipment selection.

- Archive: `pseudo-liquid-421-v1.zip`.
- CSV SHA256: `2F59678185E1161C9B7384BF45BFE6A67D7F580242448ED4D4786E5C8DE91584`.
- Two native captures are byte-identical; same ROM/source pins as above.
- Regenerate using `native-pseudo-liquid-entrypoint.patch` and the headless
  `sm.exe --diagnostic-pseudo-liquid ROM NEW.csv` entrypoint.
- Temporary native hooks removed and reapplication checked.

The remaining explicit gap is the outer enemy collision dispatcher's
invulnerability reset (distinct from its already-tested generic touch body).
Issue #421 remains open without awaiting-player-validation until that is covered.

## Ordinary collision-entry timer reset

The original $A0:A07A capture in `native-pseudo-reset-probe.h` isolates reset from
touch damage with a non-overlapping enemy at (192,128), radii (8,8), and Samus at
(128,128), radii (5,12). The 24 cases cross contact modes 0/3/4, sprite map zero
or nonzero, normal/no-op touch pointers and initial invulnerability 0/9. The
synthetic enemy definition replaces only disposable ROM memory and is restored.

Four cases failed before correction: managed dispatch cleared invulnerability
before inspecting the enemy map. Native returns on a zero map first. The reset
now occurs per enemy after that gate, still before overlap testing. This also
prevents an empty interactive list from clearing the timer without any handler
call. All 24 native results now match; an additional empty-list assertion passes.
The native no-op ordinary callback still resets the timer when its map is present.
Full core verification passes. Extended-hitbox entry has a differently ordered
no-op gate and remains the next separate check; this fixture does not prove it.

- Archive: `pseudo-reset-421-v1.zip`.
- CSV SHA256: `B7FB5034CA07E962EA0F3D1184D32A58D3E0EB65972160DCDD4050BFCB59A035`.
- Two native captures are byte-identical; same ROM/source pins above.
- Regenerate with `native-pseudo-reset-entrypoint.patch` and the bounded,
  dialog-free `sm.exe --diagnostic-pseudo-reset ROM NEW.csv` entrypoint.
- Managed command: `--pseudo-screw-reset-audit ROM CSV`.
- Temporary hooks removed and patch reapplication checked.
