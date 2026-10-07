# Issue #1202: duplicate HUD selection sound

## Report and reproduction

The player compared Select in the recompilation and cartridge and heard two sounds for one selection. Earlier v0.4.8 testing also reported stacking during rapid selection. The current comparison was reported during v0.4.13 testing.

`HudSelectSoundAudit` drives real controller input through the frontend, HUD update, audio publication, queue, and renderer acknowledgements. A flat floor and cleared enemies isolate the reported selection behavior. Before the fix, one press caused one HUD change but two library-1 $39 port writes, at relative frames 0 and 4.

## Cause and cartridge contract

The late HUD update sets a selection-sound request. Final frontend publication queued it without clearing it. The next frame's early health-check publication saw that stale flag before the HUD refreshed it. The publication guard belongs to one frontend frame, so it did not protect the following frame.

Pinned InsaneFirebat disassembly revision `362be646929cf8e483f692b73a6561cfc2dc1d0d`, NTSC J/U 1.0, bank 80: the HUD stores the new previous-selection index at $80:9C6F, applies movement/grapple/frozen-time gates, then loads $39 at $80:9C8F and calls QueueSound_Lib1_Max6 once at $80:9C92. The fix consumes the host request when published. Queue capacity, suppression, acknowledgement timing, and SPC playback remain unchanged.

The earlier request-boundary PCM comparison did not exercise the controller/HUD producer. Its matching transport and PCM results did not rule out this cross-frame duplicate.

## Confirmation

Release DebugRunner build succeeded. Run:

```powershell
dotnet csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0-windows/SuperMetroid.DebugRunner.dll --hud-select-sound-audit 'C:/Users/Service Account/AppData/Local/SuperMetroid'
```

After the fix, one press produces one selection change and one port write at frame 0. Six presses on frames 0,2,4,6,8,10 produce six changes and six writes at frames 0,4,8,12,16,20. Both cases observe 64 frames, including an idle tail with no extra writes. This confirms the reported producer defect; final audible player confirmation remains pending.
