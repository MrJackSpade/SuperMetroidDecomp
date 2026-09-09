# Ending finale native PPU comparison (#506)

This headless probe compiles the pinned upstream `src/snes/ppu.c` separately.
It does not launch SDL, change the ROM, modify upstream sources, or load a player
save. It consumes the memory prefix of a generated `.smframe` and supplies the
retail `$8B:F2FA` Mode-1 finale registers independently of C# layer descriptors.
It is a raster oracle, **not** a complete native cinematic playback oracle.

From the repository root in PowerShell:

```powershell
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --ending-planet-boundary
& 'C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe' csharp/test-fixtures/ending-native-ppu/probe.vcxproj /p:Configuration=Release /p:Platform=x64 /v:minimal
& csharp/test-temp/ending-native-ppu/probe.exe csharp/test-temp/ending-506/later-0512-ZebesExplosionAnimation.smframe csharp/test-temp/ending-506/later-0512-ZebesExplosionAnimation.bgra
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --ending-native-ppu
```

Before the production coordinate correction: **11,485 pixels differ**. The
comparison exports a native-PPU image alongside the existing frame captures.

The following controlled experiment sets **non-retail** BG1/BG2 VOFS=-1 to
compensate for the native renderer's physical scanline-one sampling:

```powershell
& csharp/test-temp/ending-native-ppu/probe.exe csharp/test-temp/ending-506/later-0512-ZebesExplosionAnimation.smframe csharp/test-temp/ending-506/later-0512-ZebesExplosionAnimation.offset-check.bgra offset-check
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --ending-native-offset-check
```

That comparison has **zero differing pixels** before the production fix. It
isolates a BG coordinate error; changing the oracle's scroll is not a fix. The
native-register comparison must become green using unchanged retail registers.
The full reported square-edge issue must still be assessed after that correction.

The initial crossfade and the complete explosion were also checked against an
independent reconstruction of the upper VRAM DMA ranges, including the exact
$1000 font upload at byte $A000: zero byte differences and zero changed pixels.
This rules out the suspected font-overwrite explanation for these captured frames.

Generated images, memory packets, raw BGRA outputs, and compiler artifacts remain
in `csharp/test-temp`; only diagnostic source is committed.
