# Crystal Flash suit interruption: shared counter (#434)

First reproduced discrepancy: native suit entry writes zero to $0DEC and its
HDMA stages continue to update that word. The suspended Flash reads the same
word as its ammunition-decrement counter. The port kept a private counter at
ten instead. Every one of 644 checkpoints failed before the fix.

`suit-entry.csv` executes original-CPU Flash admission ($90:D5A2), followed by
Varia or Gravity setup ($91:D4E4/$91:D5BA), then 160 suit HDMA callbacks
($88:E026/$88:E05C). Both initial facings are covered. Samus begins at (256,400),
49/99 health, ten of each ammo, no reserve energy or equipment, and supplies
the exact Down/L/R/Shoot chord. Inventory is awarded at the post-message suit
setup boundary. No movement phases are stepped during this isolated callback
comparison; the fixture does not claim an earned pickup or bomb interaction.

The C# test calls real Flash admission and suit Begin/Step. It compares the
Flash counter, suit substate, beam position, shine timer and palette owner.
Suit writes now publish the shared counter to Flash and Draygon alongside
the already-existing Shinespark scratch publication. No Flash handler is
cancelled and no retained spark is manufactured by this change.

All 644 checkpoints now match. Pinned source cross-checks are in
`upstream-sm/src/sm_91.c`, `sm_88.c` and `variables.h` ($0DEC is `substate`).
The expected trace executes the pinned original Japan/USA cartridge, not the
C translation. ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Normalized LF UTF-8 trace SHA-256:
`C3CF44F4E8D518AD8360CE194E4FF760A0D72F2C7752D82A33953613D40BA894`.

Generate: `DraygonCrystalAudit/audit.exe "Super Metroid.smc" suit-entry`.
Compare: DebugRunner `--flash-suit-scratch-audit "Super Metroid.smc" <suit-entry.csv>`.

The next reproduced movement-owner defect and its 5,280-frame hit/control
comparison are documented in [SUIT-BOMB.md](SUIT-BOMB.md).

The elevator command-seven ownership fix and its 32 ordering/admission cases
are documented in [ELEVATOR-ENTRY.md](ELEVATOR-ENTRY.md).

Actual bomb placement/fuse timing now matches 8,800 native frames, including
adjacent failures; see [SUIT-FUSE.md](SUIT-FUSE.md).

#434's elevator extension uses real bomb placement/cleanup, early/late
input-edge controls, and the retail handoff through restored control and spark
use. Its suit branch now also covers the actual Varia-room PLM, message,
transformation, adjacent fuse outcomes, control restoration and spark launch.
The exact native-versus-integration boundaries are documented above and in
[SUIT-FUSE.md](SUIT-FUSE.md).
