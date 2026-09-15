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
existing Green Brinstar elevator frontend test. It admits Flash through the
runtime's power-bomb-cleanup admission seam, then supplies its chord with a
fresh Down edge. The real elevator AI, pending door, DoorTransitionState and
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

#434 remains in progress: complete power-bomb cleanup/elevator phase ordering
and actual suit pickup/fuse timing coverage.
