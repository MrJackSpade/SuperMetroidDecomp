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

#434 remains in progress: complete power-bomb/elevator phase ordering and
post-ride control/spark usability, and actual suit pickup/fuse timing coverage.
