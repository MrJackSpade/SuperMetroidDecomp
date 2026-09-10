# Quick Drop ceiling handler regression — partial #456

The [technique description](https://wiki.supermetroid.run/Quick_Drop) includes
retaining ascent through temporary ceiling contact during a turnaround.
This fixture tests that handler boundary, not the entire input-driven technique.

## Authority and reproduction

- Japan/USA revision-zero ROM SHA256:
  `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host `upstream-sm`: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- Run original CPU routine `$90:A790`, not its translated C implementation.
  `$90:90E2` applies gravity and requests movement; `$90:E606` publishes a
  ceiling result without changing velocity. `$90:A7A8` clears that result.
- Four independent cases: both turn facings, persistent/removed ceiling.
  Three handler frames each. Synthetic 16-by-32 room, solid ceiling row 12,
  Samus center `(128,227)`, radii `(5,19)`, upward speed `2.0000`, gravity
  `0.2800`. Removed ceiling disappears before frame 1. No cheats or actors.
- No animation or input processing is run. Horizontal movement is recorded
  for diagnostics but deliberately not compared; the ceiling spans the room.
  Exact Y fixed-point position, velocity, direction and cleared collision
  request are compared, plus physical ceiling collision while present.

Apply `native-quick-drop-entrypoint.patch` in `upstream-sm` with
`git apply --unidiff-zero`, build the Release x64 native host, then run:

```text
sm.exe --diagnostic-quick-drop "Super Metroid.smc" capture.csv
```

The entrypoint suppresses explicit SDL error dialogs. CPU calls have an
instruction budget. Remove the temporary host hooks after capture.

Two independent captures matched SHA256:
`09C5404AC472984A672B7C3E11C61F521880C0C5C5072ADECE281E66B731A4ED`.
The accepted CSV is in `quick-drop-ceiling-native-capture.zip`. Compare using:

```text
SuperMetroid.DebugRunner --quick-drop-ceiling-audit "Super Metroid.smc" capture.csv
```

## Failure and correction

Before the fix, first contact correctly clamped Y to `00E3.0000` but reset speed
to zero and direction to down. Native retained `0001.D800` and direction up.
After correction all 12 handler frames match. With ceiling removal, native and
port reach Y `00DF.7800` on frame 2; with it retained, Y stays `00E3.0000`.

The turn now defers ceiling-stop velocity writes, just as it already suppresses
the collision-to-pose result. Gravity and solid collision remain active. Other
movement callers retain their existing ceiling response.

## Still required before #456 is ready

Final status: [QUICK-DROP-BOMB.md](QUICK-DROP-BOMB.md) contains the combined
completion audit and links all later coverage. The following limitations are
historical and apply to this handler fixture alone.

Follow-up: [QUICK-DROP-TIMELINE.md](QUICK-DROP-TIMELINE.md) now covers controller-driven
ceiling turn entry/completion and adjacent timing failures. The original limitation
below describes this handler fixture; falling/block-destruction coverage remains open.

Controller-driven turn entry/completion, adjacent timing failures, crumble and
bomb/power-bomb destruction paths, and falling-speed retention. This narrow
regression does not establish those behaviors or PAL parity. Keep the ticket
open without the awaiting-player-validation label until its remaining scope
has been handled.
