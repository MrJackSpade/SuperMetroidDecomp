# Gunship recharge/save/release (#328)

Run from the repository root with the private retail ROM present:

```powershell
dotnet run --no-restore --no-launch-profile --project csharp/src/SuperMetroid.DebugRunner -c Release -- --gunship-recharge-audit 'Super Metroid.smc'
dotnet run --no-restore --no-launch-profile --project csharp/src/SuperMetroid.DebugRunner -c Release -- --gunship-save-audit 'Super Metroid.smc'
```

The refill reproduction failed on frame two: health reached 99, but the excess point
was discarded instead of filling reserves. The shared $91:DF12 restoration now covers
all three reserve modes, exact per-frame ammo/energy, and prompt readiness.

The runtime save reproduction failed because the ship requested confirmation but no
normal gameplay consumer displayed it. The fix translates the $85:80BF gunship branch,
including its 160-frame $85:8119 saving-sound wait and ordinary completion notice. The
runtime then resumes $A2:AB1F and uses the existing frontend SRAM persistence path.

The frontend fixture boots a constructed valid slot C through actual file/options/map
selection, then isolates Landing Site. It enters the ship with ordinary Down input,
recharges health and reserves, and exercises both YES and a visibly selected NO. It
checks the changed cursor tilemap, saving wait, exactly-once saving/opening/closing audio
port commands, completion notice, SRAM mutation only after the message returns, selected
slot C station-zero marker and restored energy, unchanged SRAM on NO, untouched slots A/B,
exact exit coordinates, unlocked control, and no repeated prompt after exit. Runtime-only
coverage additionally checks B cancellation and exactly-once persistence requests.

Tests use an in-memory SRAM image and acknowledge APU commands without opening audio or
GUI devices. No player save/state file is modified. Player confirmation remains required.
