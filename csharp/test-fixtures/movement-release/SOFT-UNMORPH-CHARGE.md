# Soft-unmorph X-ray charge carry (#471, partial)

Source: [Mockball with Bombspread Charge, method III](https://wiki.supermetroid.run/Mockball#Mockball_with_Bombspread_Charge).
This verifies the soft-unmorph preparation and ensuing charge carry. It does not
claim slope-traversal multiplier parity for #425 or full active-scope rendering.
The native pause-separated Down timing comparison is now in [PAUSE-CHARGE.md](PAUSE-CHARGE.md).

## Reproduce and provenance

`SuperMetroid.DebugRunner --soft-unmorph-charge-comparison-audit "Super Metroid.smc" soft-unmorph-charge-471-v2.csv`

The accepted capture is in `soft-unmorph-charge-native-capture.zip`. SHA-256:
`30B110B1866FCC94A05B3A6661BD387B4F710198FE3E601979EEB38A6FC3EC60`.
A repeat native CPU run produced the identical hash. Retail ROM, harness and
disassembly pins are those in [XRAY-CHARGE.md](XRAY-CHARGE.md).

Include `native-release-probe.h` and `native-soft-unmorph-charge-probe.h` after
StateRecorder in sm_rtl.c; dispatch DiagnosticSoftUnmorphCharge with ROM and fresh
CSV paths before SDL. The probe restores original ROM bytes and executes the
actual alpha/projectile interaction/beta/animation/pose/timer paths. It uses no
player save or cheats. Temporary native hooks were removed after capture.

## Setup, successful case and controls

A flat-floor 144-by-80-block room has floor row 16 and end walls at columns 1 and
142. Samus begins at X=1024 in the appropriate falling Morph Ball pose, with zero
vertical speed, downward direction and Y=201, 202 or 203. Up is pulsed at frame
19, 20 or 21. Both facings are covered. Morph Ball, Bombs, X-ray and Charge Beam
are equipped. Charge, HUD selection and horizontal speed begin at zero.

Only Y=202 / Up=20 lands crouched while retaining vertical speed 2.F400. Adjacent
heights/timings clear the vertical-speed word. The speed is produced by falling
and unmorphing, never injected to simulate the glitch. The retained search probe
(`native-soft-unmorph-search.h`) can rediscover additional height/timing pairs.

Select at 60 reaches X-ray. Shoot from 61 through 125 earns charge 65. The ordinary
landing controls end there, before Run would activate a scope; they prove failure
of the preparation, not a simulated active-scope lifecycle.

Successful preparations continue: replace Shoot with Run at 126. The residual
vertical speed rejects scope setup even though Samus is crouched on the floor,
so Run holds the charge. Start moving forward at 130, jump at 145, release
Forward around two Down presses, and repress Shoot during the morph animation.
The second Down is swept from 224 through 241. Run is released one frame after
Shoot resumes; Down remains held until release at 300.

Second-Down timings 224–230 bounce, 231–237 soft-morph, and 238–241 miss morphing.
Successful morphs retain charge 65 and produce five bombs at release. Accepted
cases never activate/freeze X-ray. There are 324 cases / 47,808 compared frames:
288 preparation controls of 126 frames and 36 follow-through cases of 320 frames.

Every frame compares inputs, position/subpixels, pose/movement/animation, velocity,
charge/spread counters, bomb count, first-bomb state, bounce, HUD item, frozen flag
and beam count. Explicit checks enforce the unique preparation, earned charge,
retention with Run, soft-morph window and release. No slope-speed or bomb-rendering
claim is inferred from these fields.

## Reproduced shared walk-off defect

The neighboring ordinary landing exposed a missing pose-expansion collision.
For right-facing Y=201 / Up=20, frame 26 ends crouched near the floor. The next
grounding probe selects falling art. C# installed that larger body directly,
leaving center/subpixel Y=240.6400; native F404 first calls changed-pose collision
and produces Y=237.FFFF. This caused 10,764 mismatching frames across the matrix.

`ApplyWalkedOffFloorTransition` now requires room collision data and uses the
existing `ResolveLargerPoseCollision` before installing falling. It preserves the
resolver's fractional clamp and compensating center shift. A rejected target does
not execute falling command five. Runtime and cinematic callers supply their
actual level and NMI parity; focused diagnostic callers supply their fixture.
No special-case height, room or technique logic was added to production.

After the fix, all 47,808 frames match, including the legitimate residual-speed
glitch. A focused two-facing regression asserts exact center/subpixel correction,
falling pose, cleared velocity and downward direction. Full core Verification
passes. Native regression captures for Moonwalk (20,160), Moonfall (37,440),
Mockball (67,200), Speedball (120,000) and continuous walljump (134,400) all retain
zero mismatches. DebugRunner builds without warnings/errors.

V1 explored later morph timings that actually activate X-ray after landing, outside
this probe's HDMA scope. V2 retains successful and adjacent missed-morph controls
without entering that lifecycle, and stops failed preparations before activation.
Only v2 is accepted. Full native pause timing is covered separately by PAUSE-CHARGE.md.
