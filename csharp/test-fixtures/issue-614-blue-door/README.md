# Blue door does not open when fired upon (#614)

`slot-0.smstate` is the production debugger save-state slot supplied with the
player report. It was copied unchanged from the live application's AppData
store on 2026-09-13; do not overwrite the live slot when investigating.

Reproduction:

1. Load `slot-0.smstate` as debugger save-state slot 0 from a disposable state
   directory.
2. Fire at the blue door.
3. Observe that the door remains closed instead of opening.

Affected version: Unknown (awaiting player version).

State SHA-256:

- Slot 0: `E1F043C5FDB042CC2E7DF2A601565796B97AC0A97F3B518EBC0FF8BB43B6E40C`

The state graph includes private game data and belongs only in this private
repository. Player confirmation will be required before closing the issue.

## Reproduced cause and verification

The unchanged state places Samus at (37,395), firing left at the four-cell cap
at column 1, rows 22–25. Before the fix, the Wave projectile travels through the
cap and out of the room; the cap remains solid after 150 frontend frames.

`$94:A3BA` executes XBA then BMI. XBA sets N/Z from its resulting low byte even
when M=0. The translation tested the original low byte instead of its high byte,
disabling horizontal Wave reactions for pixel offsets 128–255 in every screen.
The pinned disassembly and native CPU's `case 0xeb` confirm the flag semantics.

`--blue-door-state ROM slot-0.smstate` in DebugRunner restores a disposable copy,
runs the saved frontend/audio path, and asserts that all four cap cells become
air. It fails before the correction and passes after it. Verification's
`--gate-beam-collision` additionally checks 24 left/right pixel-half, screen,
outside-room, and negative-coordinate boundary cases. Full core verification and
the Windows Release build also pass. No live player save was modified.
