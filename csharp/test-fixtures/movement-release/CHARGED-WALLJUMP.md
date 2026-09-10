# Charged walljump / morph handoff (#471, partial)

This is a verified component of #471, not completion of its full technique audit.
Pause-separated Down presses, actual Bombs menu toggles, X-Ray-assisted carry,
and a continuous-walljump speed-carry setup remain to be investigated. Do not
label the entire issue awaiting validation on this fixture alone.

## Native evidence and reproduction

`charged-walljump-native-capture.zip` contains `charged-walljump-471-v3.csv`:
SHA-256 `6B2C108DA71D6DAFCC3AB6C36C8D4C9D94EC6368E25DC0EB0E90524D1B382F67`.
Two native CPU runs are byte-identical. v1 missed the wall and is not accepted;
v2 exercised the wall but omitted bounce state from its output. v3 records it.

Japan/USA rev 0 ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
upstream-sm pin: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly pin: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
Relevant routine: $90:DD8C HudSelectionHandler_CrouchEtcTrans, table $90:DDAA.

Include native-release-probe.h then native-charged-walljump-probe.h after
StateRecorder in sm_rtl.c. Add a pre-SDL dispatch for
`--charged-walljump ROM CSV` to `DiagnosticChargedWalljump(argv[2], argv[3])`.
Build, capture to distinct paths, then remove both temporary hooks. The probe
restores original ROM bytes and executes cartridge CPU instructions without GUI
or player save access.

Extract the archive, then run:

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --charged-walljump-comparison-audit "Super Metroid.smc" PATH/charged-walljump-471-v3.csv
```

## Setup and inputs

26 cases x 270 frames = 7,020 frame comparisons. Both facings; Down timing
150..210 in five-frame steps. The 144x80 constructed room has a solid floor
at row 16 and a wall at column 66 (right-facing start) or 61 (left-facing).
Samus starts at x=1000/1047, y=235, zero subpositions/speeds, ordinary pose,
99 health, Morph Ball + Bombs + Charge equipped, and cheats off. No enemies.
No charge, spin, walljump or morph state is injected.

- Hold Shoot through frame 79; begin running at 60, jump at 70.
- Release Dash at 73 and Shoot at 80, retaining actual spin charge.
- Turn away and release Jump at 92; press Jump again at 94.
- Hold Jump; press Down and Shoot at the case's morph frame, maintaining direction.
- Hold the charge while rolling; release Down at 240 to produce Bomb Spread.

The native state reaches walljump at frame 94 with charge 71. Down through
frame 195 retains that exact charge through morph/roll until release. The
frame-195 morph lands without bounce; earlier morphs include hard impacts.
Cases starting at 200+ miss that airborne handoff and form adjacent controls.
Different hold durations straddle the spread-strength threshold at 64 ticks.

Both harnesses execute complete alpha, projectile interaction, beta movement,
animation and pose selection, plus gameplay timer tail. NMI starts at one and
advances each frame. Comparisons include position/subpixels, pose/type, animation,
horizontal/vertical speeds, charge, spread hold counter, bomb count, bounce state,
and the first bomb's type/list/position/velocity. The final release requires five
bombs and consumed charge. This does not claim full five-projectile rendering or
audio parity; all five slots should be included in the remaining release audit.

## Reproduced fix

C# treated morph animation as ordinary beam-producing posture, incrementing charge
from 71 to 72 on its first frame and continuing throughout the animation. Native
DD8C preserves charge for morph/unmorph records when Grapple is inactive. The
cartridge table was already implemented for Grapple admission but not beams.

The table/identities now live in the shared SamusHudRomData catalog, and both
Grapple and beam admission use SamusHudInput.PostureTransitionAdmitsWeapons.
Normal crouch/stand transitions retain their native producer admission. Existing
projectiles still advance when production is suppressed.

Before the fix: 1,330 mismatching frames. After: 7,020 compared frames, zero
mismatches. Explicit assertions check actual charged walljump entry, exact charge
retention, the late no-bounce morph, and delayed spread release.

Regression verification: full core Verification passed, along with the existing
Moonwalk (20,160 frames), Moonfall (37,440 frames) and shot/bomb (20,800 frames)
native comparison matrices. All had zero mismatches.
