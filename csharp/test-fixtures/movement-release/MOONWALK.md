# Moonwalk gameplay parity (#467)

Settings handoff/persistence evidence is in MOONWALK-OPTIONS.md (commit
22c91dde). This capture covers gameplay; Moonfall remains separately tracked
under #468.

## Accepted native capture

`moonwalk-native-capture.zip` contains `moonwalk-467-v6.csv`, SHA-256:
`DCAB63C8B61CCC2C78504F4CC0D65ABBA6F889D35B41870CAC45B41E7197F438`.
Two independent native executions produced identical bytes. The comparator
rejects exploratory captures, which lacked complete bindings or matching NMI
parity. These omissions mattered for shoulder aiming and alternating spike
collision scans; they were fixture defects, not production collision fixes.

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`
(Japan/USA rev 0). Native runner upstream-sm:
`578f90b3cc49557bb70060ad033bb90b8cf8ac50`; disassembly:
`362be646929cf8e483f692b73a6561cfc2dc1d0d`.

Include native-release-probe.h followed by native-moonwalk-probe.h after
StateRecorder in sm_rtl.c. Before SDL in main, dispatch
`DiagnosticMoonwalk(argv[2], argv[3])` for `--moonwalk ROM CSV`.
Build the native project, run twice to distinct output paths, and remove the
temporary hooks afterward. The probe restores original ROM bytes, uses the
actual cartridge CPU, and does not launch a window or use player saves.

Run the production comparison after extracting the archive:

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --moonwalk-comparison-audit "Super Metroid.smc" PATH/moonwalk-467-v6.csv
```

## Fixture and coverage

168 cases x 120 frames: both visual facings, Moonwalk enabled/disabled, air,
water without Gravity, and water with Gravity, across fourteen scenarios:

| Scenario | Actual input/geometry |
| --- | --- |
| 0 | Hold Shoot, then backward direction from standing |
| 1 | Run forward while charging, then backward |
| 2 | Crouch before attempting backward entry |
| 3 | Run into a wall before attempting backward entry |
| 4 | Reverse forward on the frame immediately after Moonwalk entry |
| 5 | Jump backward with partial charge |
| 6 | Moonwalk off a ledge |
| 7 | Diagonal-up aimed Moonwalk |
| 8 | Jump backward with full charge |
| 9 | Move onto a real solid spike block while Moonwalking |
| 10/11 | Aim diagonally up/down while leaving the ledge |
| 12/13 | Simultaneous backward direction and Jump from standing, partial/full charge |

The constructed room is 144x80 blocks, floor row 16, zero BTS, initial Samus
1024,235 with zero subpixels and speeds. Morph Ball/Charge are equipped, plus
Gravity only in its dedicated medium. All enemies are removed, cheats are off,
and no Moonwalk pose or charge counter is injected. Full alpha, projectile
interaction, beta movement/animation/pose handling, and the gameplay timer tail
execute. Accepted-NMI counters match the C# fixture's initial one and advance
before each frame; collision scan parity is therefore meaningful.

Every frame compares position/subpixels, pose/movement/facing, animation frame
and timer, horizontal base/extra speed and mode, vertical speed/direction,
charge, health, invincibility and knockback. Additional assertions require
settings/entry restrictions, immediate reversal's half-pixel base speed,
actual C# shot allocation on partial/full-charge jump release, and movement
facing on ledge and hurt interruption. Spikes exercise the shared hurt path;
this is not an enemy-AI or beam-rendering parity claim.

## Reproduced production defects and repairs

1. C# advanced charge during ordinary turning (e.g. frame 41 charge 002A vs
   native 0029). DD74 admits the producer only with a pose muzzle handoff;
   otherwise it does not charge or release. Knockback/DamageBoost and the other
   JumpEtc entries likewise preserve charge when Grapple is inactive. Existing
   projectiles continue updating independently of producer admission.
2. The dedicated Moonwalk turn/jump initializer omitted the F8F3-F903 tagged
   old muzzle direction. The following alpha never forced the release, leaving
   charge live through the spin. It now publishes the same handoff as the
   ordinary grounded turn initializer. Native/C# release on frame 51 for partial
   charge and 81 for full charge; the disabled-setting control retains charge.
3. Walk-off selection used muzzle direction. PSP_Falling (91:E8F2) uses the
   physics-facing byte and ordinary falling pair. Moonwalk art faces opposite
   that byte, so the old implementation faced the wrong way and then diverged
   in movement. The common walk-off selector now uses physics facing. Angled
   walk-offs also first select ordinary falling, then process held aim next
   frame. The earlier synthetic test expecting immediate angled falling was
   corrected against both native angled-ledge captures, not simply relaxed.
4. StepFalling omitted its outer movement wrapper's fast-fall animation branch.
   At signed whole speed >= 5, ordinary/gun-extended falling poses advance to
   frame five with timer eight before AnimateSamus. The native branch is now
   implemented using named definition constants; aimed fall lists are untouched.

Simultaneous standing backward/Jump inputs are a separate control: unlike the
already-entered Moonwalk jump, the native sequence retains charge. Both paths
are compared rather than imposing the technique description on every input order.

The final native comparison passes all 20,160 frames. The full core Verification
suite and the menu regression pass. Broader regression results are recorded on
the GitHub issue after completion. Player confirmation is still required.
