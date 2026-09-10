# X-ray-assisted Bomb Spread carry (#471, partial)

Source: [Mockball with Bombspread Charge, method III](https://wiki.supermetroid.run/Mockball#Mockball_with_Bombspread_Charge).
This capture covers the running-jump approach. The soft-unmorph/slope-killer
preparation has a separate [capture](SOFT-UNMORPH-CHARGE.md). Full native
pause-separated Down timing now has a separate [capture](PAUSE-CHARGE.md). The
continuous-walljump branch has a separate [capture](CONTINUOUS-WALLJUMP.md).
Together the captures cover #471's mechanical scope, awaiting player confirmation.

## Reproduce

Run `SuperMetroid.DebugRunner --xray-charge-comparison-audit "Super Metroid.smc" xray-charge-471-v3.csv`.
The trace is archived in `xray-charge-native-capture.zip`. SHA-256:
`4893532C55D1162C79764520CEE8F4674F1FF9F1AACBBEC4856136215862B288`.
A second native capture is byte-identical.

The ROM is Japan/USA revision 0, SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native harness: `upstream-sm` at `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
`native-xray-charge-probe.h` uses the retail CPU after restoring unpatched ROM
bytes. It executes complete alpha, projectile interaction, beta movement,
animation/pose handling and the gameplay timer tail. No player save or gameplay
cheat is used. Temporary pre-SDL native entrypoint hooks were removed afterward.

## Fixture and inputs

A constructed 144-by-80-block room has a flat floor at row 16 and solid end walls.
Samus begins at (1024,235), facing either direction, with Morph Ball, Bombs,
X-ray and Charge Beam equipped. HUD selection and beam charge start at zero.

- Run forward starting at frame 30; jump at 70 and hold Jump.
- Release Run on jumping. Select at 75 reaches X-ray through the real HUD cycle.
- Shoot from 76 through 137 earns charge 60 after the initial spin-exit frames.
- At 138, release Shoot and hold Run instead. Airborne X-ray setup rejects
  activation, but its HUD dispatch retains charge without normal beam handling.
- First Down is two frames before the swept second Down (150 through 164).
  Release horizontal input across those presses, then resume forward movement.
- Repress Shoot during the morph animation, one frame after second Down.
  Release Run on the following frame, keep Down until 219, release it at 220.

Mode zero is the full-charge sequence. Mode one stops Shoot at 135 and starts
Run at 136, retaining only charge 58. Mode two omits the second Run hold entirely,
so Shoot release invokes the normal beam handler and consumes charge at 138.
Three modes, two facings and 15 Down timings give 90 cases / 22,500 frames.

The early 150–155 morphs bounce. Timings 156–162 soft-morph without bounce.
163–164 miss the airborne morph. Full-charge successful morphs retain charge 60
and release five bombs at 220; undercharge and missing-Run controls do not.
The test asserts that X-ray remains selected but never activates/freezes time.
No X-ray setup/HDMA emulation is omitted from an accepted active-scope sequence:
all accepted cases reject activation naturally. Later grounded timings require
a separate full-scope fixture and are not accepted here.

Every frame compares input, position/subpixels, horizontal/vertical velocity,
pose/movement/animation, charge, Bomb Spread counter, bomb count, first-bomb
type/list/position/velocity, bounce, HUD selection, frozen state and beam count.
This is not a bomb-rendering comparison; the companion equipment-toggle capture
compares all five bomb slots over release-strength boundaries.

## Reproduced defect and fix

The undercharge control exposed a missing side effect in the shared bomb-admission
helper. In the right-facing frame-150 case, the first stable ball HUD pass at 157
left charge 58 in C#, while the CPU cleared it. Native $90:C0E7 cancels nonzero
charge when it rejects bomb placement for lack of a new Shoot edge, full slots,
or cooldown. The translated helper previously returned no slot without notifying
the shared charge/palette/sound owner.

`TryPlaceBomb` now reports this specific cancellation through the existing
`BeamChargeConsumed` bridge. The runtime clears charge and restores the suit
palette; the bomb frame publishes one library-one CancelAll/Max9 request. Earlier
equipment and active-Power-Bomb guards still preserve charge because they do not
execute this native helper. Valid full-charge spread handling is unchanged.

Before the fix, the native comparison failed on the retained partial charge.
Afterward all 22,500 frames match. Twenty-four focused cases cover charge zero,
partial/full charge, Bombs equipped/disabled, normal/Power-Bomb selection and
held/new Shoot. They assert cancellation signaling, exactly one cancellation
sound when applicable, and allocation versus rejection. The full core
Verification suite passes; charged-walljump (7,020 frames) and equipment-toggle
(8,000 frames) native comparisons retain zero mismatches.

Capture history: v1 charged for too few frames and explored later grounded scope
activation without the full HDMA lifecycle, so it is not accepted. V2 corrected
charge duration and bounded the window to airborne/rejected-scope cases. V3 adds
undercharge/missing-Run controls and beam count; only v3 is accepted.
