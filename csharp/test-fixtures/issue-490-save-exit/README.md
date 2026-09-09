# Tourian save-room camera report (#490)

The existing private recording is preserved locally at
`csharp/test-fixtures/issue-489-recordings/save-exit.smrec`. It starts from SRAM,
loads `$8F:DE23`, exits into `$8F:DDF3`, then enters `$8F:DD58`.
This directory does not contain or upload the private recording.

Capture the **frontend-published** images, not an independently rendered runtime:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-launch-profile -- --input-replay-capture csharp/test-fixtures/issue-489-recordings/save-exit.smrec "Super Metroid.smc" 1200 1500 csharp/test-temp/issue-490-save-exit
```

The inclusive range produces 301 PNGs plus 301 portable `.smframe` packets.
Capture runs `StepCaptured` from the beginning with gameplay publication enabled,
so retained frames, door scrolling, fades, and room handoffs are included. It does
not overwrite a debugger save slot. The ordinary audit keeps its render-disabled
behavior when no capture directory is requested.

Inspection on the current build:

- 1200: Samus approaches the save-room right door.
- 1216: outgoing-room fade.
- 1248/1280: moving doorway against the transition blackout.
- 1312/1344: aligned Rinka-shaft room entry.
- 1440: Samus descending through the shaft with the room visible.

These sampled frames do **not** establish the reported camera corruption. The
complete per-frame captures are available for finer inspection. Do not close the
ticket on this replay's lack of crashes or infer that a Mother Brain arena fix
also fixed the save-room exit. No production camera change has been made here.
