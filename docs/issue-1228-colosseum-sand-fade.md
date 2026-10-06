# Issue #1228: Colosseum sand bypasses door fade

Affected build: 0.4.13-smoke1227; v0.4.14 was subsequently tagged from that same gameplay commit. The player identifies room $04/$28 and confirms the trigger is door entry.

## Reproduction and cause

Colosseum is room $8F:D72A. Its FX record $83:9F14 selects the Maridia sand-pit palette object $8D:F795, which animates CGRAM colors 36..43. The focused reproduction uses the actual Halfie Climb ($D913) door $83:A8E8 and the production DoorTransitionState through arrival and fade.

BuildDestinationOam borrows the full gameplay StepFrame path. That path ran the room palette-FX handler even though door transition ownership was still active. Consequently the sand colors jumped from all zero to `3ED9 2E57 2A35 25F3 25D2 1DB0 196E 112E` during HandleTransition, before the first fade step. The subsequent fade saw colors already equal to its target and could not fade them in.

## Cartridge contract and correction

Pinned InsaneFirebat disassembly revision 362be646929cf8e483f692b73a6561cfc2dc1d0d, NTSC J/U 1.0: normal gameplay calls PaletteFXObject_Handler at $82:8B54. The destination fade routine $82:E737 calls animated tiles, enemies, projectiles, drawing, and the gradual palette transition at $82:E752; it does not call palette FX. The sand program and its ten-frame records are at $8D:F4E9 onward.

The runtime now suppresses room palette-FX execution while the existing door-transition ownership flag is set, preserving the fade's control of current colors. It adds no room-specific exception or replacement colors. Normal palette execution resumes when the door coroutine clears that flag.

## Confirmation

The guarded DebugRunner fixture fails before the correction: HandleTransition, color 36, expected 0000, actual 3ED9. Afterward all eight sand colors match the room's gradual transition across all 16 destination setup/fade checks. The first gameplay frame preserves the completed colors, and frame 10 advances to the next native sand animation record. The reference transition uses the actual destination target and the shared existing fade algorithm; native routine inspection establishes that palette FX must not override it.

```powershell
dotnet csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0/SuperMetroid.DebugRunner.dll --colosseum-sand-fade-audit 'C:/Users/Service Account/AppData/Local/SuperMetroid'
```

Release build and scoped regression pass. Player visual confirmation remains pending.
