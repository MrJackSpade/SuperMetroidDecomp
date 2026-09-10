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
