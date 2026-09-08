# Draygon body positioning — #381

The player's slot 0 was preserved locally as `slot-0-named.smstate` before replay.
SHA-256: `9A06B29B7EADC0CF6E96412C4591AADFF2DFD94129FC7E11BAB79D477BAE1A9B`.
The private ROM/SRAM-bearing binary is not included in this commit.

Run from the repository root:

```
dotnet run --no-launch-profile --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --draygon-position-audit
```

Before the fix, frame 0 produced body `(303,287)`, BG2 `(65024,384)`,
versus native `(64916,65275)`. The native graphics-drawn hook at `$A5:9342`
was never published to the runtime's background register mirrors.

The regression checks 600 real encounter frames against the hook's unsigned
camera/body/displacement arithmetic, then checks the accepted-NMI render capture
against the preceding frame's writes. It exercises 594 distinct body positions.
Selected rendered frames are written to `csharp/test-temp/issue-381-draygon`.
Cross-checked with pinned `upstream-sm/src/sm_a5.c` (`Draygon_Func_36`) and
`upstream-disassembly/src/bank_A5.asm` (`EnemyGraphicsDrawnHook_Draygon_SetBG2XYScroll`).
