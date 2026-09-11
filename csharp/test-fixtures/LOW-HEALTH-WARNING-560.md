# Low-health warning (#560)

## Cause and implementation

The native `$90:EA7F` warning latch/producer was missing. The shared
`SamusHealthWarningState` now handles signed energy comparison with 31, queues
library-three `$02` on entry and `$01` on exit, and preserves the cartridge's
no-retry behavior when suppression or queue capacity rejects a request.
The latch persists with Samus; old debugger layouts explicitly warn and initialize
the previously unavailable latch inactive.

Ordinary gameplay and the gunship entry/exit handler publish through the frontend
audio pipeline. Generic locked, station, drained, elevator, pause and demo paths
do not fabricate an ordinary check. Automatic reserves retain their independent
external check after gameplay, even while frozen. See the reserve fixture document
for the reproduced missing automatic check and completion-order fixes.

## Independent verification

- Original restored ROM CPU: 198 health/latch/Power-Bomb/debug-suppression/queue
  combinations match the C# latch, accepted command and subsequent no-retry result.
  Includes queue occupancies 0/5/6 and signed-word boundary health values.
- Original handler execution: normal `$90:E725` and gunship `$90:E902` reach
  LowEnergyCheck. Locked `$90:E8DC`, station `$90:E8D6`, drained `$90:E8D9` and
  elevator `$90:E8EC` return without reaching it. These use the original Samus
  initializer and a constructed flat room. The initial probe without that
  initializer failed; it was not treated as evidence of a production defect.
- Real C# frontend: warning starts once, stops on reserve refill's 30-to-31 crossing,
  responds in ordinary gameplay, is omitted while generically locked/paused, and
  is admitted during gunship entry despite locked input. State migration and
  serialization are covered. Full verification and Windows build passed after
  integration.
- Managed audio output: 300 complete PCM and acknowledgement frames match the
  pinned native audio **translation**. Sustained warning peak is 4676; post-stop
  peak is zero. This is generated PCM, not an original SPC CPU or Windows/RDP
  endpoint listening claim. Player listening confirmation remains outstanding.

## Reproduction commands

Apply `movement-release/native-reserve-refill-entrypoint.patch` in `upstream-sm`,
build Release x64, run `sm.exe --diagnostic-reserve-refill ROM NEW_OUTPUT.csv`,
then reverse only that temporary patch. Verification accepts:

`--health-warning-native NEW_OUTPUT.csv.warning.csv`

Its companion `.warning-handlers.csv` is required. The probe exits before SDL
initialization, bounds CPU execution, and reports errors on the console.

DebugRunner accepts:

`--health-warning-native-audio-audit AUDIO_DIRECTORY NATIVE_AUDIO_DLL`

Native traces are ignored local diagnostics, not published fixtures. Current
comparison output is `csharp/test-temp/reserve-native-527/warning-handlers-v2-560.csv`.
No ROM, PCM, screenshots or player states are included in this change.
