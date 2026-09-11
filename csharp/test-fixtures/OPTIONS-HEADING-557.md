# Options heading borders (#557)

Affected version: 0.1.1. The player's controller-page ITEM CANCEL screenshot
showed a narrow border cutting through the heading. The same defect reproduces
through normal page-selection inputs from the primary options menu.

The port unconditionally emitted the OPTION MODE border at X=$7C/Y=$10.
Native page creation replaces that object with the controller or special border:

| Page | Instruction list | Sprite pointer / table ID | Setup X |
|---|---|---|---|
| Primary | $82:F47E | $82:D24B / $4B | $7C |
| Controller | $82:F48E | $82:D2F7 / $4C | $84 |
| Special | $82:F49E | table ID $4D | $80 |

Confirm controller ID using table entry $82:C601: the pinned disassembly's
symbol incorrectly contains `49`, although its comment correctly says `4C`.
The regression reads the actual list pointer and setup operands from the ROM,
independent of the production catalog IDs. Controller pre-instruction $82:F3A0
also subtracts/adds two pixels with vertical page scrolling; the port now anchors
the border to the same BG1 scroll rather than leaving it fixed on screen.

Run `--options-heading-audit ROM LOCAL_DIRECTORY` in DebugRunner. Before the
fix, controller entry differs from native border composition in 1125 heading
pixels. After the fix, five comparisons pass: primary, controller entry,
ITEM CANCEL, scrolled controller, and special. Each checks the entire top
32-pixel heading strip and equality between direct and snapshot rendering.
The reference reuses the page's BG memory but independently constructs border
OAM from the native lists; this is a border-composition test, not a claim of
whole-menu original-CPU framebuffer equivalence. Actual before/after images
were inspected, including the reported ITEM CANCEL selection. Captures stay local.

The existing options audit also passes controller-binding, Moonwalk, Icon Cancel,
abandoned-edits and SRAM-slot-isolation checks. Leave #557 open for player
confirmation after labeling awaiting-player-validation.
