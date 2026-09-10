# Stop on a Dime (#453)

Native v1 capture: `stop-on-dime-native-capture.zip`, containing
`stop-on-dime-453-v1.csv`, SHA-256
`32EF0272528F859608285FDC9E8987CC21A9396EF8230B780350F26B99DD78FD`.
An independent recapture produces the same hash.

```
SuperMetroid.DebugRunner --stop-on-dime-comparison-audit "Super Metroid.smc" stop-on-dime-453-v1.csv
```

## Reproduced defect and correction

The ordinary-release control exposed an incomplete running fallback. When alpha
retained the running pose and selected command one, C# only set acceleration mode
two. Cartridge `ProspectivePoseCmd_1` at $91:EC50 also adds extra dash speed into
base speed, cancels boost, and clears the extra-speed pair. The running fallback
now uses the existing shared command-one implementation after movement.

Before the correction, 640 of 15,360 frames differed. The first discrepancy,
right-facing dry Dash release at frame80, had matching X but C# retained
base=2.C000 / extra=2.0000 where native had base=4.C000 / extra=0. Subsequent
deceleration and delayed-aim stopping therefore consumed the wrong speed state.
After the fix every captured field agrees on every frame. Immediate Up/shoulder
stopping was already matching and has not been replaced by an input-driven clamp.

The native stop is not zero displacement on the first input frame: beta movement
precedes prospective-pose installation. With existing extra dash speed, the next
standing frame also consumes that speed before clearing it. The test preserves
both frames. In the right-facing dry Dash case, Up/R/L ends at X=$054E.D000,
versus ordinary release at $056C.4000. Suitless water ends at $0433.A000 with
aiming, versus $0475.0400 with ordinary release in this setup. These are fixture
positions, not universal stopping distances.

## Matrix and assertions

96 fresh cases, 160 frames each:

- Both facings; walking and Dash held during approach.
- Dry terrain, fully submerged suitless movement, and submerged Gravity Suit control.
- Eight patterns: ordinary release; Up; R; L; both shoulders; R while retaining
  direction (must not stop); R one frame after release; R one frame before release.
- Release direction and Dash at frame80, except the held-direction control.
- Match exact X/Y fractions, pose, movement type, animation frame/timer, base and
  extra velocity, acceleration mode, facing, vertical velocity/direction and charge.
- Assert input timeline, capture hash/dimensions/order, stable stopped position and
  zero speeds after the cancellation window, and the non-stopping held-direction control.

Flat constructed 144x80 room with floor row16; Samus starts X=1024, Y=235,
standing, health/max=99. Water surface Y=8, options=$80, FX type6 when submerged.
Morph Ball equipped, Gravity only in its control; no Speed Booster, beams, enemies,
cheats or player SRAM. Startup horizontal slope-enable word is three. Native
executes actual ROM alpha, movement, animation, projectile and pose-change stages;
C# executes the complete runtime StepFrame path on matching terrain.

## Reproduction and scope

Apply `native-stop-on-dime-entrypoint.patch` in pinned upstream-sm with
`git apply --unidiff-zero`, build Release x64, then run:

```
sm.exe --diagnostic-stop-on-dime "Super Metroid.smc" stop-on-dime-453-v1.csv
```

Reverse the patch after capture. The dispatch occurs before SDL, explicitly
suppresses GUI error dialogs and uses the bounded original-ROM CPU helper.
The archive contains numeric diagnostics, not ROM/SRAM.

ROM Japan/USA rev0 SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
upstream-sm: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`, bank91 $EC50-$EC80.
The issue's wiki Stop_on_a_Dime link currently could not be fetched; the claim
was tested against cartridge execution rather than inferred from that unavailable page.
Release DebugRunner and full core verification pass. No PAL, room-transition,
renderer, slope or Speed-Booster-equipment coverage is claimed by this matrix.
The issue remains open for player confirmation.
