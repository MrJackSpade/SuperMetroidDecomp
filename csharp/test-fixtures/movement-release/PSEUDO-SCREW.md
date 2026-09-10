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

The current managed generic touch body lacks command four entirely. A fix must
clear both the Samus flare mirror and the live projectile-system charge/animation/
palette state. Merely zeroing the mirror can be overwritten by the next producer.

## Reproduce native evidence

Apply `native-pseudo-touch-entrypoint.patch` to the pinned native tree, build
Release x64, and invoke only the bounded, dialog-free entrypoint:

```text
sm.exe --diagnostic-pseudo-touch "Super Metroid.smc" NEW.csv
```

Remove the hooks after capture. Build, duplicate capture hash, and patch reapply
checks passed. No production fix is claimed by this checkpoint.

## Remaining before player validation

- Reproduce the discrepancy through managed production contact and implement the
  complete live charge teardown at the native handoff boundary.
- Compare enemy and projectile contact separately, including immune cases.
- Cover charge-producing spin/walljump inputs, released-shot retention, negative
  walljump-check poses, yellow palette and suitless-liquid restrictions.
- Verify invulnerability semantics without granting universal protection.

The issue remains open without awaiting-player-validation. The broader contract
and source are https://wiki.supermetroid.run/Pseudo_Screw_Attack and issue #394.
