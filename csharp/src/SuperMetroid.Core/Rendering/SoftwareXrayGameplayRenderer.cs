using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Source-aware reference for the native X-ray Mode-1/window/color-math configuration.</summary>
public static class SoftwareXrayGameplayRenderer
{
    public static Rgba32[] Render(PpuMemorySnapshot snapshot, XrayGameplayRenderLayer layer, byte objectSelection)
    {
        var memory = new SoftwarePpuSnapshotMemory(snapshot);
        var r = layer.Gameplay.Registers;
        int width = SnesPpuLayout.ScreenWidthPixels, height = SnesPpuLayout.ScreenHeightPixels;
        var output = new Rgba32[width * height];
        var objects = new Rgba32[output.Length];
        var priorities = new byte[output.Length];
        var palettes = new byte[output.Length];
        // Registers and backdrop are invariant for this immutable packet. Resolve
        // their flags once rather than repeating Enum.HasFlag in the pixel loop;
        // runtime optimization of that API differs between desktop JIT and Mono AOT.
        bool showBg1 = (r.MainScreenLayers & SnesMainScreenLayers.Bg1) != 0;
        bool showBg2 = (r.MainScreenLayers & SnesMainScreenLayers.Bg2) != 0;
        bool showObjects = (r.MainScreenLayers & SnesMainScreenLayers.Obj) != 0;
        bool halfEnabled = (layer.ColorMath & SnesColorMathControl.Half) != 0;
        bool subtract = (layer.ColorMath & SnesColorMathControl.Subtract) != 0;
        Rgba32 backdrop = memory.Cgram.GetRgba(0);
        SnesObjRenderer.RenderResolved(memory.Oam, memory.Vram, memory.Cgram, objectSelection,
            objects, priorities, width, height, palettes);
        for (int y = 0; y < SnesPpuLayout.GameplayHudHeightPixels; y++)
        for (int x = 0; x < width; x++)
            output[y * width + x] = Sample(SnesPpuLayout.GameplayHudTilemapWord, r.HudCharacterWord,
                32, 32, x, y, fourBit: false, opaqueZero: true).Color;
        for (int y = SnesPpuLayout.GameplayHudHeightPixels; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int i = y * width + x, lineIndex = y - SnesPpuLayout.GameplayHudHeightPixels;
            XrayWindowLine window = layer.Lines[y];
            bool inside = x >= window.Left && x <= window.Right;
            var winner = backdrop;
            int rank = -1;
            SnesColorMathControl source = SnesColorMathControl.Backdrop;
            // Window selection precedes priority resolution: BG1 is suppressed inside
            // the reveal beam, BG2 outside it. Excluded rooms leave both unmasked.
            if ((!layer.RevealBlocks || inside) && showBg2)
            {
                int sx = layer.Gameplay.HorizontalScrolls.IsEmpty ? r.Bg2X : layer.Gameplay.HorizontalScrolls[lineIndex];
                int sy = layer.Gameplay.VerticalScrolls.IsEmpty ? r.Bg2Y : layer.Gameplay.VerticalScrolls[lineIndex];
                var pixel = Sample(r.Bg2TilemapWord, r.Bg2CharacterWord, r.Bg2WidthTiles, r.Bg2HeightTiles, x + sx, y + sy, fourBit: true);
                Insert(pixel.Color, pixel.High ? 5 : 2, SnesColorMathControl.Bg2);
            }
            if ((!layer.RevealBlocks || !inside) && showBg1)
            {
                var pixel = Sample(SnesPpuLayout.GameplayBg1TilemapWord, r.Bg1CharacterWord, 64, 32, x + r.Bg1X, y + r.Bg1Y, fourBit: true);
                Insert(pixel.Color, pixel.High ? 6 : 3, SnesColorMathControl.Bg1);
            }
            if (showObjects && priorities[i] != SnesObjRenderer.TransparentPriority)
            {
                int objRank = priorities[i] switch { 0 => 0, 1 => 1, 2 => 4, 3 => 7, _ => throw new InvalidDataException("Invalid resolved OBJ priority.") };
                // OBJ palettes zero through three never participate in SNES color math,
                // even when the OBJ enable bit is set in CGADSUB.
                Insert(objects[i], objRank, palettes[i] >= 4 ? SnesColorMathControl.Obj : SnesColorMathControl.None);
            }
            if (!inside && (layer.ColorMath & source) != 0)
            {
                Rgba32 sub = default;
                if (layer.AddSubscreen && layer.Subscreen is { } bg3 && y >= bg3.FirstScanline)
                {
                    BackgroundLineScroll scroll = bg3.Scrolls[y];
                    sub = Sample(bg3.TilemapWord, bg3.CharacterWord, 32, bg3.MapHeightTiles,
                        x + scroll.X, y + scroll.Y, fourBit: false).Color;
                }
                bool useSub = layer.AddSubscreen && sub.A != 0;
                // A transparent subscreen falls back to COLDATA but disables halving.
                // Fixed-color-only mode does not have that exception.
                bool half = halfEnabled && (!layer.AddSubscreen || useSub);
                winner = new(Channel(winner.R, useSub ? sub.R >> 3 : layer.FixedRed),
                    Channel(winner.G, useSub ? sub.G >> 3 : layer.FixedGreen),
                    Channel(winner.B, useSub ? sub.B >> 3 : layer.FixedBlue), winner.A);
                byte Channel(byte main, int other)
                {
                    int value = subtract ? (main >> 3) - other : (main >> 3) + other;
                    if (half) value >>= 1;
                    value = Math.Clamp(value, 0, 31);
                    return (byte)((value << 3) | (value >> 2));
                }
            }
            output[i] = winner;

            void Insert(Rgba32 color, int candidateRank, SnesColorMathControl candidateSource)
            {
                if (color.A == 0 || candidateRank < rank) return;
                winner = color; rank = candidateRank; source = candidateSource;
            }
        }
        return output;

        (Rgba32 Color, bool High) Sample(int map, int characters, int mapWidth, int mapHeight, int x, int y, bool fourBit, bool opaqueZero = false)
        {
            x &= mapWidth * 8 - 1; y &= mapHeight * 8 - 1;
            int tx = x >> 3, ty = y >> 3;
            int page = ((ty >> 5) * (mapWidth >> 5) + (tx >> 5)) * 1024;
            SnesBgTilemapWord tile = memory.Vram.ReadWord((map + page + (ty & 31) * 32 + (tx & 31)) & 32767);
            int px = tile.FlipHorizontally ? 7 - (x & 7) : x & 7;
            int py = tile.FlipVertically ? 7 - (y & 7) : y & 7;
            int row = (((characters + tile.CharacterIndex * (fourBit ? 16 : 8)) & 32767) * 2 + py * 2) & 65535;
            int shift = 7 - px;
            int color = (Read(row) >> shift & 1) | (Read(row + 1) >> shift & 1) << 1;
            if (fourBit) color |= (Read(row + 16) >> shift & 1) << 2 | (Read(row + 17) >> shift & 1) << 3;
            return (color == 0 && !opaqueZero ? default : memory.Cgram.GetRgba(tile.PaletteIndex * (fourBit ? 16 : 4) + color), tile.HasPriority);
        }
        byte Read(int address) => memory.Vram.ReadByte(address & 65535);
    }
}
