# Yapping Maw capture / issue 525

Affected player version: **0.1.1**. The reported "flesh flower" identity and room
are still unconfirmed. This work fixes a proven Yapping Maw translation defect;
it does not establish that every reported missed grab has been reproduced.

## Defect and native evidence

`Function_YappingMaw_Neutral` at $A8:A26A compares target distance with 64.
The BMI at $A26D skips the assignment for shorter distances. Longer distances
are capped at 64. The C# implementation had the condition reversed: short
distances became 64, while long ones were left unchanged. This changes the
mouth's entire curved trajectory and can make it overshoot a nearby target.

The new seven-distance test failed before the production change:
`distance=33, expected=32, actual=64`. The target's eight-bit cosine measurement
is one pixel shorter than the geometric vertical separation. After correction,
separations 33, 40, 48, 63, 64, 65 and 96 preserve/cap the measured length and
feed the correct half-length into the curve radius.

Sources: pinned `upstream-disassembly/src/bank_A8.asm` neutral/touch/cooldown
routines and `upstream-sm/src/sm_a8.c` `YappingMaw_Func_1` / `YappingMaw_Touch`.
ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
No emulator/controller comparison or player confirmation is claimed.

## Reproduction commands

Run the DebugRunner in Release with:

```
--yapping-maw-runtime-contact-audit "Super Metroid.smc"
--yapping-maw-audit "Super Metroid.smc"
```

The runtime test retains the retail ceiling Maw in room $965B, removes other
interactive enemies, and replaces terrain with a flat synthetic floor. It runs
normal `SuperMetroidRuntime.StepFrame`, including EnemyMain contact; it does not
invoke touch directly or move either actor into contact after initialization.
No suit/beam equipment, invulnerability timer, or host cheats are enabled.

- Standing target: capture on zero-based call 49.
- Jump held for the first 20 calls: capture on call 12.
- Both cases: release on call 140, no health loss, input lock during capture,
  input restored on release, and mouth-relative positioning checked each held
  call. The native shared-list early return retains the previous position on
  the single retraction-transition call.
- Starting already inside the resting root: 240 calls without a grab, with the
  native point-blank gate continually resetting to 48. This is deliberate native
  behavior at measured distances below 32, not proof of the player's diagnosis.

The two capture cases passed even before the clamp fix. They establish that
runtime capture is integrated, not that the reported missed capture was fixed.
The range assertion is the failing regression for this change.

The older multipart audit now starts 65 rather than 64 pixels below the root
to preserve its intended 64-pixel measured attack length. Its existing curve,
palette, projectile, grab, freeze-release, death and cleanup assertions remain.
The separate range test covers the formerly hidden rounding/clamp boundary.

## Remaining scope

Issue 525 stays open without the awaiting-player-validation label: identifying
the reported encounter and reproducing its missed grab are still outstanding.
No saved state from another report is assumed to identify this enemy.
