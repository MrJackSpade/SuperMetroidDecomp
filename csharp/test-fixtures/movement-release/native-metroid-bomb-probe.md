# Metroid bomb investigation (#485)

This is an isolated trajectory measurement using the retail 65816 instructions,
not a full bomb/Metroid simulation or proof that a centered bomb must detach one.
The full C# runtime fixture currently misses, whereas its stationary isolated
collision fixture detaches. Do not fix this by delaying bomb jumps without a
native full-sequence comparison: native A0:9785 publishes at fuse value eight.

## Reproduction

Temporarily include `native-release-probe.h`, then
`native-metroid-bomb-probe.h` from `upstream-sm/src/sm_rtl.c` after its includes.
Temporarily dispatch `DiagnosticMetroidBombJump(argv[2])` from `main.c` when
`argc == 3` and `argv[1]` is `--metroid-bomb-probe`, before any SDL startup.
Declare that function extern at the dispatch. Restore both temporary hooks after
the experiment. The checked-in native application has no diagnostic hook.

Build `upstream-sm/sm.sln` Release/x64 with MSBuild, overriding
`PlatformToolset=v145` for the installed VS 18 toolset. Run:

```powershell
& upstream-sm/build/bin-x64-Release/sm.exe --metroid-bomb-probe 'Super Metroid.smc'
```

The loader restores the retail ROM bytes after the native comparison harness's
patches, and disables Windows critical-error/fault dialogs before loading.
This probe does not initialize SDL or open a game window.

## Observed native output (2026-09-09)

Start Y is `00B9.0000`, upward speed `0002.C000`. Successive calls to
`$90:E032` produce these Y/speed words:

| Update | Y | Upward speed |
| --- | --- | --- |
| 0 | 00B6.4000 | 0002.A400 |
| 1 | 00B3.9C00 | 0002.8800 |
| 2 | 00B1.1400 | 0002.6C00 |
| 3 | 00AE.A800 | 0002.5000 |
| 4 | 00AC.5800 | 0002.3400 |
| 5 | 00AA.2400 | 0002.1800 |
| 6 | 00A8.0C00 | 0001.FC00 |
| 7 | 00A6.1000 | 0001.E000 |
| 8 | 00A4.3000 | 0001.C400 |
| 9 | 00A2.6C00 | 0001.A800 |
| 10 | 00A0.C400 | 0001.8C00 |
| 11 | 009F.3800 | 0001.7000 |

This confirms substantial movement during the pre-explosion interval is not by
itself evidence of a mistranslated jump. It does not settle frame scheduling,
Metroid positioning, bomb explosion radius evolution, or detach/reattach timing.
Those remain required comparisons. No production fix or validation claim is
made by this diagnostic.
