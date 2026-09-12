# #401 Phantoon Super Missile enrage skip

## Result

The existing production implementation matches the pinned Japan/USA cartridge.
No gameplay correction was needed. Nine room-local, normal-input runs compare
15,300 gameplay frames against original 65816 execution, with zero mismatches.
This verifies mechanical timing, not full audiovisual or PAL parity.

The [technique reference](https://wiki.supermetroid.run/Phantoon#Enrage_Skip)
describes a one-frame Super Missile enrage skip. Pinned bank-A7 source shows why:
`EnemyShot_Phantoon` at `$A7:DD9B` checks nonlethal enrage only in `$D60D`,
`$D788`, and `$D678`. The intermediate `$D65C` phase bypasses that check.
Lethal damage takes the death branch before these checks. The fixture exercises
real collision before AI, not a direct call with an invented phase.

## Deterministic input and controls

Load Phantoon room `$8F:CD13`, RNG `$0061`, with its real grey-door PLM and FX.
Samus begins at world `(128,187)`, zero subpixels, standing right; requested
camera `(32,21)`. Equipment: Varia, no beams, 999 energy, 10 missiles and
10 Supers, no Power Bombs. No invincibility/infinite-ammo cheats. Boss starting
health is 2500, 700, or 500, configured only before gameplay; the latter two
are exact-lethal and overkill controls after a 100-damage missile primer.

Two setup frames select missiles and release input. Frame zero follows them.
Default input is Up; Right replaces it on frames 1460..1479. Hold Jump on
1548..1565, fire the missile on 1565, and select Supers on 1566. Fire the Super
on 1576, 1577, or 1578. No actor, projectile, or health edits follow setup.
Continue through frame 1699 with live enemy attacks and normal gameplay.

| Super impact | Entry phase | Nonlethal result | Lethal result |
| --- | --- | --- | --- |
| 1576 | EyeTracksSamus | FadeOutBeforeRage, 1800 HP | DyingFadeInOut |
| 1577 | BecomeSolidAndSwoop | No enrage, 1800 HP | DyingFadeInOut |
| 1578 | Swooping | FadeOutBeforeRage, 1800 HP | FinishFatalSwoop |

The skip's hurt animation initially keeps the intermediate phase active;
comparison continues beyond the hit rather than assuming immediate movement.
Assertions require exactly the primer and Super damage events, correct health
and reaction phase, and all 150 native numeric columns on every gameplay frame.
Columns include boss and Samus position/subpixels, pose, health, phase, hurt
clocks, five player projectile slots, eighteen enemy projectile slots, and
swoop velocity/target. Inactive projectile payloads normalize to zero.

## Reproduction

ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native upstream: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

Apply `movement-release/native-phantoon-enrage-entrypoint.patch` to the pinned
native checkout and rebuild Release x64. The entrypoint runs before SDL and
redirects its errors/warnings to stderr. The shared loader restores original ROM
bytes before the bounded CPU executes; no gameplay ROM patch is used.

```text
sm.exe --diagnostic-phantoon-enrage ROM output.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --phantoon-enrage-audit ROM output.csv
```

`movement-release/phantoon-enrage-401.zip` contains only numeric diagnostics,
including eighteen native setup records. Extracted CSV SHA256:
`B967BFADBB8515CAEA8DC3B8AB5E61965D0A258E4A537FCADFABA0335C089D12`.
Two independent native captures produced this identical hash. No ROM, save,
screenshot, artwork, or audio is included. Restore temporary hooks and rebuild
the ordinary native executable after capture.
