# Spin-jump door exit investigation (#517)

Affected player version: **0.1.1**. Still open; no gameplay fix or cartridge
parity claim is established by this diagnostic.

The reported entrance is Flyway `$8F:9879` through `$83:8BC2` into Bomb Torizo
`$8F:9804`. The pinned disassembly's `Door_Flyway_1` confirms a rightward
horizontal entrance, destination screen (0,0), default spawn distance `$8000`.
Flyway is three screens wide. Using an arbitrary left-edge source coordinate
for this rightward door produces an invalid placement and is not a reproduction.

Run the bounded, headless constructed-boundary diagnostic:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --spin-door-exit-momentum-audit 'Super Metroid.smc' 9879 9804 02F8 0080
```

This extends the existing running-door diagnostic without changing gameplay.
It explicitly seeds a descending spin at the source boundary, uses the ROM's
spin horizontal-speed cap, and compares absent/present two-pixel dash momentum
(including its required ownership flag). It executes the real frontend door
coroutine and then twenty neutral-input gameplay frames. Output records each
phase, fixed positions, frame displacement, base/extra speed, vertical speed
and direction, pose, environment, and input lock.

Observed on the project ROM: horizontal base speed remains `$0001.6000`
through the transition; extra speed remains zero or `$0002.0000`. The first
unlocked frame moves 1.375 or 3.375 pixels respectively, with no extra horizontal
speed introduced at the boundary in these two cases. These numbers do **not**
show that the player report is resolved: the input-driven approach, held-input
exit, other momentum states, and original-CPU comparison remain outstanding.

The trace also exposes that the managed room loader clears vertical speed during
placement. Source inspection of the native placement/final-handoff routines does
not establish an equivalent clear, but the complete native transition must be
checked before attributing the reported trajectory to that difference. Do not
remove it based solely on this observation.

Verification: DebugRunner Release build succeeds; both bounded scenarios finish
their actual room transition and all twenty post-transition frames. No issue
closure or awaiting-player-validation label is appropriate yet.
