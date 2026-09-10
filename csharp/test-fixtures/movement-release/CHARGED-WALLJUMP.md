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

## Pause-separated Down integration baseline (partial #471)

Run `SuperMetroid.DebugRunner --pause-charge-carry-audit "Super Metroid.smc"`.
This additional fixture drives the real `SuperMetroidGame.StepCaptured` frontend,
including pause darkening, menu setup, delayed Start acceptance, menu fade-out,
and resumed gameplay. It uses an in-memory save and replaces the Landing Site
geometry with a flat floor; it does not read or overwrite player saves.

Shoot starts at frame zero, running at 30 and jumping at 70. The sweep presses
Start + Down at every frame from 120 through 150, releases Down during the frozen
menu, then presses it on the first resumed gameplay frame. Assertions cover:

- The normal pause states are reached within bounded waits.
- Pose, vertical position and beam charge stay frozen during the menu.
- Down is a new input edge on the first resumed gameplay frame.
- Frames 120–139 morph but bounce; 140–146 morph without bouncing and retain
  charge; 147–150 miss this airborne morph.
- Successful carries do not release bombs while Down is held. Releasing Down
  produces five bombs and consumes the charge.

These are 31 deterministic **C# integration baselines**, not a native-CPU
measurement of the pause timing window. No production behavior was changed for
this fixture. Cartridge pause comparison, X-ray-assisted
carry and continuous-walljump speed carry remain required before #471 is ready
for player validation. The native comparison above covers only its stated
charged-walljump sequence; it must not be cited as proof of this menu sequence.

An additional frame-142 case now disables Bombs through R, Right, Down and A in
the actual frontend equipment page. Samus soft-morphs and rolls with Down released
while retaining charge. A second actual pause re-enables Bombs; holding Down for
20 resumed frames postpones release, and releasing it produces five bombs.
The checks require unchanged collected items and Charge Beam equipment. All 32
frontend cases pass. A preliminary fixture accidentally created two A edges and
re-enabled Bombs before unpausing; correcting those test inputs required no
production change.

## Native equipment-toggle and release comparison

Run `SuperMetroid.DebugRunner --charge-equipment-comparison-audit "Super Metroid.smc" charge-equipment-471-v3.csv`.
The accepted trace is in `charge-equipment-native-capture.zip`. SHA-256:
`46037C3DB10645434246D811454A3C0C88EE01D3A838A0F9C2AE43CD5174F01F`.
A second CPU run produced the identical hash. Revision and native harness pins
are the same as the charged-walljump capture above.

`native-charge-equipment-probe.h` executes the cartridge CPU's initial equipment
selection ($82:AB47) and equipment-main ($82:AC4F), with Right, release, Down,
release, A, release inputs. C# uses the production `PauseMenuState` page/input
path. Neither side sets equipment bits to simulate a toggle. Both begin with
collected/equipped Morph Ball, Bombs and Charge Beam, no gameplay cheats, and earn
charge through the same running/jumping/walljump sequence used above.

Bombs are disabled before gameplay frame 150, Down starts the soft morph at 195,
and is released at 200 while Bombs remain disabled. Bombs are re-enabled before
260. The release sweep then holds Down for 0, 63, 64, 127, 128, 191, 192 or 193
frames. The last case verifies automatic release at the 192-frame timeout while
Down is still held, rather than simply verifying a coincident button release.
Both facings are tested, 500 gameplay frames each: 16 cases / 8,000 frames.

Assertions cover the earned walljump charge, soft-morph/no-bounce state, precise
equipment bits without changing collection, retention of charge 71, absence of
premature bombs and the exact release frame. Every frame compares movement,
subpixels, pose/animation, charge/hold counters and all five bomb slots' type,
instruction pointer, position, velocity and fuse. All 8,000 frames match.

This focused CPU probe does not execute the full frontend pause/fade/NMI sequence;
equipment input runs between gameplay frames. It proves menu dispatch and ensuing
movement/release behavior, not the separate pause-separated Down timing window.
The full frontend integration test above covers the host path without claiming
native timing parity. The running-jump X-ray variant is now covered by
[XRAY-CHARGE.md](XRAY-CHARGE.md); soft-unmorph X-ray preparation and
continuous-walljump variants remain outstanding.

Capture history: v1 reached the synthetic room's empty boundary, where C# clamps
position but the native isolated movement routine does not. Both fixtures gained
solid end walls to keep the experiment room-local; v2 added all five bomb slots,
and v3 added the adjacent timeout case. Only v3 is accepted. Temporary native
entrypoint hooks were removed after capture; no player saves were used.
