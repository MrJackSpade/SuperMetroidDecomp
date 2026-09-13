# Zebetite skip investigation (#442)

Status: **unfinished**. No production fix or native skip-parity claim yet.

Sources: [14% techniques](https://wiki.supermetroid.run/14%25#Techniques) and
[Mother Brain Room](https://wiki.supermetroid.run/Mother_Brain_Room).
The issue covers Ice-only Rinka placement without Wave/Screw, and an up-left
shinespark followed by escape inputs. Passage, enemy collision and recovered
control must be asserted separately from destroying a Zebetite.

DebugRunner `--zebetite-skip-audit ROM` is a room-local exploratory trace. It loads
the intact first Zebetite in room `$8F:DD58`, with no destroyed-barrier events.
Samus starts at (900,100), zero subpixels, facing left, 399 energy, Morph/Gravity
and Ice only, with cheats off. No state is edited after the first frame begins.
Inputs walk left for 60 frames, then hold Up+Shoot through frame 119. All three
cases freeze the lower Rinka at (823,166) through the real projectile system.

The cases then step right for 0, 8 or 20 frames, followed by Left and 24-frame Jump
holds separated by 12-frame releases. The trace records exact controller words,
Samus coordinates/subpixels, pose, health/invulnerability, and live barrier/Rinka
positions, properties and freeze timers. It asserts the setup's Rinka freeze,
**not successful passage**. The observed wedging could still be invalid alignment;
do not change collision code based on this trace alone.

Next: reproduce the relevant alignment/trajectory using original CPU routines,
find a successful setup and adjacent failure, then compare passage and subsequent
control. The shinespark half remains untested. No player save is loaded or changed;
keep generated logs and any future ROM/state exports private.

## Original-CPU collision interval

`--zebetite-skip-export ROM PRIVATE_DIRECTORY` captures frames 120..159 for each
case, after the lower Rinka has frozen. It runs both the complete room and a
counterfactual omitting enemies other than native slots 128 (Zebetite) and 192
(frozen Rinka), and clearing projectiles. The exported movement/pose/animation/
radii/health/freeze CSVs must remain byte-identical. All three omission checks pass.
This establishes the omission only for these fields and forty frames, not later
damage, respawn, artwork, audio, or successful passage.

The native consumer uses MOV1 room/movement plus ZSK1 supplemental data containing
NMI/RNG, pose history, health, camera, and the two exact 64-byte enemy records.
Include `native-release-probe.h` followed by `native-zebetite-skip-probe.h` in
`sm_rtl.c` and temporarily dispatch before SDL:

```text
--zebetite-skip ROM MOV1 ACTORS OUTPUT_CSV STEP_BACK_FRAMES
```

Pass those five arguments to `DiagnosticZebetiteSkip`; use offsets 0, 8 and 20.
Remove the temporary entrypoint/includes and rebuild the normal executable after
the experiment. Generated seeds contain cartridge data and must remain private.

The original CPU reproduces the wedged trajectory: all 120 frames agree on
positions/subpixels, pose, animation frame/timer, X radius, health and Rinka freeze
timer. **Y radius differs on five pose-change frames:**

| Step-back frames | Frame | Managed Y radius | Native Y radius |
| --- | --- | --- | --- |
| 0 | 120 | 19 | 21 |
| 0 | 125 | 16 | 19 |
| 8 | 128 | 12 | 21 |
| 8 | 134 | 16 | 12 |
| 20 | 140 | 12 | 21 |

`--zebetite-skip-compare MANAGED_CSV NATIVE_CSV` checks every field and exits with
an error for these differences; they are not silently excluded from a parity pass.
Their effect on a successful skip remains unproven. Do not infer a collision fix
from wedging that the cartridge itself reproduces.

`--zebetite-skip-repeat-jumps ROM` explores repeated step-back/jump cycles with
seven offsets. This remains exploratory: no successful passage assertion or
native comparison for that longer sequence exists yet. The next required work is
the successful alignment/escape setup, the radius-publication discrepancy, and
the separate diagonal-shinespark method. #442 remains active.
