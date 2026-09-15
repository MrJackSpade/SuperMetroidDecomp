# Elevator activation replaces Flash movement (#434)

`elevator-entry.csv` executes original-CPU elevator initialization/main
($A3:94E6/$A3:952A), including command seven ($90:F1C8), around actual Flash
admission ($90:D5A2). Its 32 cases vary elevator direction, initial facing,
contact flag, newly pressed versus held Down, and Flash before/after enemy AI.
The held chord is always the valid Flash chord Down/L/R/Shoot. An upward
elevator therefore rejects it; this is not a manufactured simultaneous Up.

Before the fix, the two valid downward/contact/new-Down cases with Flash
admitted first retained its raising handler in C#. Native installs normal
movement while leaving the independent Flash palette/timer intact. Elevator
departure now relinquishes that Flash movement/input owner. All 32 cases
match status, pose, position, active Flash movement and palette/timer. Other
ordering/admission cases are preserved unchanged.

The C# fixture loads a constructed population containing the real elevator
definition and uses RoomEnemySystem.StepFrame; no private AI method is invoked.
Samus starts at (136,235), the elevator at (136,256), 49/99 health, ten of each
ammo, no equipment/reserve energy. Contact is supplied at the already-detected
pseudo-door boundary. The fixture does not claim body-contact generation,
power-bomb cleanup timing or a full elevator ride.

Native source cross-check: pinned `sm_a3.c` and `sm_90.c` command seven.
Generate with `DraygonCrystalAudit/audit.exe "Super Metroid.smc" elevator-entry`.
Compare with DebugRunner `--flash-elevator-entry-audit "Super Metroid.smc"
<elevator-entry.csv>`. ROM identity is pinned in README.md. LF-normalized trace
SHA-256: `67113DB05F6FB27EC0107C15773E685BABCA645AABFB3DFDB240E7FF13B9F794`.

## Retail door handoff and post-ride control

DebugRunner `--flash-elevator-handoff-audit "Super Metroid.smc"` extends the
existing Green Brinstar elevator frontend test. It now places a real Power Bomb
(eleven ammo becomes ten), advances its fuse/explosion through 153 projectile
frames, then supplies the chord on the actual cleanup frame with a fresh Down
edge. The real elevator AI, pending door, DoorTransitionState and
destination arrival run without changing the earned timer or movement owners.
During 75 destination fade frames the platform is pinned and input remains
locked; after fade, the platform resumes and eventually restores control.

The timer/palette survive while Flash movement remains inactive. A direction
tap leaves the front-facing elevator pose; fresh Jump then Up+Jump produces a
traveling vertical spark, contact damage two and actual energy loss. Without
Flash, the same arrival and input sequence produces no spark. Both variants
pass. The original test command `--elevator-frontend-handoff-audit` now checks
that full normal-control case too. No new production fix was necessary.

This is a port integration extension grounded in the native admission matrix
and existing door-handshake evidence, not a new frame-for-frame native movie
of the entire ride. The initial attempt to jump directly from the front-facing
pose was a fixture input error, corrected with the normal direction tap.

## Actual cleanup and input-edge controls

`FlashElevatorBombSetup` replaces the original direct Flash admission call.
It advances production bomb placement/update with the real room PLM owner.
It only reads the final afterglow timer/step count to identify the upcoming
cleanup boundary; it never writes those fields or calls Flash directly.
The observer honors the signed timer (the native low-byte reload retains FF
in the high byte), not an assumed zero-only wait.

This remains a fixed-position subsystem setup: bomb placement uses a prepared
ball pose, then the test restores the elevator-facing pose without moving the
bomb origin. It does not claim a controller route for morphing/positioning.
The subsequent cleanup and elevator ride use full runtime frames.

Optional timing argument `-1` seeds the preceding controller sample with the
chord, so Down is held at cleanup: Flash starts but the elevator does not.
Argument `1` supplies no chord at cleanup and presses it next frame (with the
contact flag republished): the elevator departs without Flash or a stored spark.
Both negative controls pass; the exact-frame case passes through the ride and
usable spark. These integration cases complement the original-CPU admission
matrix rather than claiming a new full native movie.

#434 remains in progress for actual suit pickup/fuse timing coverage.
