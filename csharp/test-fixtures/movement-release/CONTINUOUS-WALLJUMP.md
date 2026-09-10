# Charged Continuous Wall Jump (#471; partial #445)

Sources: [Continuous Wall Jump](https://wiki.supermetroid.run/Continuous_Wall_Jump)
and [Mockball with Bombspread Charge](https://wiki.supermetroid.run/Mockball#Mockball_with_Bombspread_Charge).

This is a genuine far-side platform walljump retaining forward speed, not the
ordinary walljump already covered by CHARGED-WALLJUMP.md. No production change
was necessary for the captured sequence. It completes the continuous-walljump
branch of #471, but not #471's native pause timing. The soft-unmorph X-ray branch
now has a separate [capture](SOFT-UNMORPH-CHARGE.md).
It is only partial coverage of #445: ceiling-bonk, retail Moat setup and regional
differences still require investigation. Do not mark either entire issue ready
on this capture alone.

## Run and provenance

`SuperMetroid.DebugRunner --continuous-walljump-comparison-audit "Super Metroid.smc" continuous-walljump-471-v3.csv`

The accepted CSV is archived in `continuous-walljump-native-capture.zip`.
SHA-256: `831BCA9D826CC63A4E329BAD672E54E45DDB06D2B98F3751345887F741920878`.
A second CPU run is byte-identical. Japan/USA rev 0 ROM and pinned harness /
disassembly revisions are the same as [XRAY-CHARGE.md](XRAY-CHARGE.md).

Include `native-release-probe.h` then `native-continuous-walljump-probe.h` after
StateRecorder in upstream sm_rtl.c. Dispatch DiagnosticContinuousWalljump with
ROM and fresh CSV paths before SDL starts. The probe restores the retail ROM
bytes, executes CPU alpha/projectile interaction/beta/animation/pose/timer paths,
and never loads a player save or enables cheats. Remove temporary hooks after
capture; they have been removed from the working upstream source.

## Geometry and inputs

The room is 144 by 80 blocks, with a floor at row 16 and end walls at columns 1
and 142. A narrow platform reaches from row 11 through the floor, at column 87
for rightward travel or 56 for leftward travel. Samus starts standing at Y=235,
X=1024 facing right or X=1281 facing left. The two-pixel difference from a naive
mirror was found on the native CPU, not patched into movement code.

- Hold Shoot through frame 70 to earn charge 71.
- Run forward from frame 30; jump at 70, release Run then Shoot at 71.
- Sweep the second Jump from 143 through 147. Release Forward two frames before
  that Jump, repress it one frame later, and release Jump for the preceding frame.
- Hold Jump after the walljump attempt. Sweep Down + Shoot from 230 through 250
  to carry the charge through morphing. Keep Forward held.
- Hold Down until frame 299, then release it at 300 to initiate Bomb Spread.

An otherwise identical negative-control mode never releases Forward. There are
two facings, five Jump timings, 21 morph timings and two Forward modes:
420 cases / 134,400 gameplay frames.

At frame 145, and only with the Forward release, native Samus enters walljump
from the far side at X=1420/right or X=884/left. The pose retains extra run speed
2.0 and charge 71. The ordinary base-speed reset during launch remains intact;
the retained extra speed makes this a continuous walljump.

Morph timings 230–239 bounce and lose extra run speed. 240–245 soft-morph without
bouncing and retain extra speed 2.0 while rolling. 246–250 miss the airborne
morph. Successful morphs retain charge 71 until release and produce five bombs
at 300. The no-Forward-release controls never enter walljump.

Every frame compares input, position/subpixels, pose/movement/animation,
horizontal and vertical speed, charge, spread-hold counter, bomb count, first
bomb type/list/position/velocity and bounce state. Additional assertions enforce
the far-side location, exact walljump/morph windows, carried extra speed and
charge, bounce controls and five-bomb release. All 134,400 frames match C#.
This does not claim visual bomb or Moat-item-message parity.

## Search and capture history

`native-continuous-walljump-search.h` retains a small CPU-only position/timing
search around the platform. It sweeps starting offsets -8 through +8 and Jump
frames 140–150 in both directions. The successful offsets are -1/0 rightward and
+2/+3 leftward relative to a naive mirror, all at Jump 145.

An earlier wider geometry search found column 87 / top row 11. Its first version
released Shoot on the initial jump frame, consuming charge before spin became
active; the corrected search releases it the following frame. Full capture v1
used the naive left mirror and a coarse morph sweep, so it is not accepted.
V2 uses the successful left position and one-frame morph sweep. V3 adds the
held-Forward control; only v3 is accepted. All experimental outputs remain
non-authoritative; the committed accepted capture is hash-checked by the audit.
