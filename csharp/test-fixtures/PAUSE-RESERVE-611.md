# Reserve Tank outline alignment (#611)

Affected version: player reported v0.3.1.

Reproduced independently: the 400-capacity/199-energy equipment screen differs
from the pinned native PPU by 542 pixels in x=16..71, y=88..119. The previous
128-case test passed because both sides used the same C# background renderer.

The reserve OAM origin is correct: retail $82:B304/$B361/$B37C decrement the
authored Y before drawing. The error is background sampling. Physical visible
scanline one samples BG row VOFS+1, whereas OBJ coordinates are already in
output space. Pause background layers omitted that physical-line adjustment.
The correction applies it to BG1, BG2 and BG3, in both direct and captured
rendering; tank sprites and logical map-scroll registers remain unchanged.

The independent probe uses the pinned upstream PPU and registers from retail
$82:A09A/$A0F7/$9142, not managed layer descriptors. It consumes the captured
VRAM/CGRAM/OAM only. This verifies raster composition, not a full native
controller playback. No screenshots or memory captures are committed.

Reproduce from the repository root:

1. Run Verification `--pause-reserve-tanks` to generate `fill-199.smframe` under
   `csharp/test-temp/pause-reserve-tanks-527`.
2. Build `csharp/test-fixtures/ending-native-ppu/probe.vcxproj` in Release/x64.
3. Run the resulting `csharp/test-temp/ending-native-ppu/probe.exe` with
   `fill-199.smframe`, `fill-199.native.bgra`, and `pause` as its arguments
   (use full paths to the fixture files).
4. Run Verification `--pause-reserve-native`.

Before correction: the 542-pixel regional assertion fails.
After correction: zero regional differences and zero differences over the
entire 256x224 equipment frame. The existing 128 capacity/fill/flicker cases,
full core verification (including installed assets and pause transitions),
and Windows Release build also pass. Awaiting player validation.
