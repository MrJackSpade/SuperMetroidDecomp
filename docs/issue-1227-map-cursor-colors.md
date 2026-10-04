# Issue #1227: pause-map cursor colors

Affected version: 0.4.13-smoke1202, the local test build handed off before this report.

The player reported a black cursor with an orange outline instead of white with a black outline. The pause-map position indicator supplied OBJ palette 3 ($0600), incorrectly borrowing the animated highlight selector at $82:C100. Native MapScreen_DrawSamusPositionIndicator loads $0E00 at $82:B9C8: OBJ palette 7. Source: pinned InsaneFirebat disassembly 362be646929cf8e483f692b73a6561cfc2dc1d0d, NTSC J/U 1.0, bank_82.asm. The three native spritemaps at $82:CF7C/$CF92/$CFA8 also identify palette 7.

Corrected the dedicated marker palette constant and its provenance. Updated the existing verification assertion to compare the actual position-indicator immediate operand at $82:B9C9 instead of the unrelated selector palette table. Artwork and animation timing are unchanged.

The guarded DebugRunner reproduction enters the real pause menu through Start, isolates its production DrawMapPositionIndicator output, and renders its installed artwork and live CGRAM through SnesObjRenderer. Before the fix, opaque pixels were black and RGB(140,107,66); the white/black assertion failed. Afterward all 24 ticks of the full animation have exactly white and black opaque pixels, and all four marker parts select native palette 7.

```powershell
dotnet csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0/SuperMetroid.DebugRunner.dll --pause-map-cursor-audit 'C:/Users/Service Account/AppData/Local/SuperMetroid'
```

Release build and focused reproduction pass. Final visual confirmation remains with the player.
