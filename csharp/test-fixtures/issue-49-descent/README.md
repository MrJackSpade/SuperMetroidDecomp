# Issue 49: Zebes wraps during descent

The before/after captures are slide frame 32, rendered from the same retail cinematic.
Before: planet tiles reappear along the bottom. After: only the remaining planet edge
at the top is drawn. No cinematic movement, timing, or Mode-7 matrix was changed.

`Program.ZebesDescent.cs` reproduces the actual visible failure: before the fix,
at zero-based slide frame 30 the gold planet region's bottom jumps from row 4 to
row 223. The old connected-component presence test missed this because some planet
pixels remained visible continuously. The new regression checks every slide frame.

Native reference: `$8B:9746` selects `$81:8853` for off-screen origins and `$81:879F`
for on-screen origins. `IntroDiscoverySprite.Draw` previously always chose the latter.
The off-screen loader reverses the on-screen loader's vertical-wrap parking condition.
An exhaustive 65,536-case origin/offset test checks that rule independently.

Run the complete verification suite:

```powershell
dotnet run --no-launch-profile --project csharp/src/SuperMetroid.Verification -c Release
```

Captures were produced with `--ceres-destruction-audit`. That diagnostic currently
fails a separate post-landing save-reload assertion (it observes MainGameplayFadeIn
while expecting MainGameplay). Its captured descent is valid, but that command is
not recorded as a passing full end-to-end test. Affected Android host playback still
needs confirmation; the issue must remain open.
