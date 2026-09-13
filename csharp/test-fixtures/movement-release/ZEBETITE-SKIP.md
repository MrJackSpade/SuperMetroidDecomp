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
