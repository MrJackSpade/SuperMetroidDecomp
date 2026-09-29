# #312: Maridia pipe exit movement recording

Player report: our version fires Samus out of the Maridia pipe into the underwater
area unusually fast; the ROM walks nearly normally out of that door. This extends
the door-exit distance/timing issue, but a shared cause is not established.

## Preserved evidence

`player-input.smrec` is a copy of the existing desktop recording
`SuperMetroid-input-20260905-192757-242.smrec`. It contains its initial SRAM and
controller history, so subsequent saves cannot overwrite this evidence. It is a
private fixture, not a distributable recording.

`managed-trace.log` was generated with the existing Release DebugRunner binary:

```powershell
dotnet csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0/SuperMetroid.DebugRunner.dll --input-replay-trace csharp/test-fixtures/issue-312-maridia-door-exit/player-input.smrec "Super Metroid.smc" 36120 36330
```

The recording runs 64,516 frontend calls successfully. Relevant room transition:
`$8F:CF80` (pipe) -> `$8F:CEFB` (underwater room), heading left.

- Frame 36138 loads CEFB during state $0B.
- Frame 36219 returns to gameplay, unlocks input, and has X=$00D8, Y=$018B.
- Frames 36220–36229 have zero controller input. X falls from $00D5 to $00BE:
  26 pixels left over the first ten gameplay updates after handoff.
- Camera remains X=$0000, Y=$0100 throughout those updates.
- At frame 36230 pose changes from running $0A to falling $2A.

This records actual post-transition displacement, not a camera-relative illusion.
It does **not** establish the correct cartridge trajectory, identify a root cause,
or prove a fix. Compare the native cartridge movement state and per-frame X with
equivalent equipment, entry speed, and inputs before changing behavior. Cover
scrolling, final placement, and post-handoff movement separately; do not use a
successful room load as the assertion for this issue. No production fix applied.
