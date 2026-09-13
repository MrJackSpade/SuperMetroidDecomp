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
