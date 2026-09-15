# Bomb replaces the suspended Flash handler (#434)

Reproduced defect: after the suit-release bomb arc ended, the port resumed the
old Flash raising handler. Native had replaced it with bomb-jump movement and
then restored normal movement. The first mismatch was frame 191: the port
raised Samus two pixels while native fell with Y subspeed $1C00. There were
836 mismatching checkpoints in the initial 4,800-frame reproduction.

Special pose command three ($91:EE80) now relinquishes Flash's movement and
pose-input lock along with the previously handled suspended Shinespark. It
does not clear Flash's independently running palette/timer. The existing bomb
arc owns movement/input restoration; no special-case trajectory is added.

`suit-flow.csv` contains 12 cases of 440 frames, all from original ROM CPU
execution: both initial facings, Varia/Gravity, and no bomb / centered bomb /
13-pixel-offset bomb. Flash is admitted with the real chord and resources,
then suit setup takes over before Flash's first movement step. At the suit
HDMA release boundary, a constructed timer-eight bomb is published for one
collision sample; the actual enemy-interaction/pose dispatcher handles it.
The port runs the same setup and real runtime frames. No manual bomb direction,
Flash cancellation, retained timer or spark velocity is injected.

At frame 399 all cases have returned to normal movement. Only the centered
hit retains Flash's palette/timer; the no-bomb and offset controls finish the
Flash instead. At 400 press Jump, then hold Up+Jump. At 403 the centered cases
must have a traveling vertical spark, contact damage index two, and health 48
(down from 49). They have no Speed Booster equipment or preceding stored charge.

All 5,280 frames match pose, whole X/Y, Y subposition/speed/direction, active
shine timer/palette, shared counter and ammunition/health. The snapshot selects
the current Shinespark owner of shared scratch after launch rather than comparing
Flash's now-inactive private copy. The test does not compare full framebuffer,
X subposition, complete animation, or every native handler address.

Native source: `DraygonCrystalAudit/SuitFlow.h`, original movement entry catalog,
and pinned `sm_91.c` command three. Generate with `audit.exe "Super Metroid.smc"
suit-flow`; compare with DebugRunner `--flash-suit-flow-audit "Super Metroid.smc"
<suit-flow.csv>`. ROM identity is in README.md. LF-normalized trace SHA-256:
`BB58506A1B95B58BF991D87C8653AC77F3C8F2FA34E81E7DD74151FBF4F3E217`.

Remaining #434 scope: a real bomb fuse/collectible and adjacent pickup timing
matrix, plus downward-elevator activation. This fixture begins at the
post-message suit callback and timer-eight overlap, not a complete item route.
