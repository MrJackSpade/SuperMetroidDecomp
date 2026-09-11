using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Buffers;

namespace SuperMetroid.Core.Rendering;

/// <summary>Source-aware reference for the native X-ray Mode-1/window/color-math configuration.</summary>
public static class SoftwareXrayGameplayRenderer
{
    public static Rgba32[] Render(PpuMemorySnapshot snapshot, XrayGameplayRenderLayer layer, byte objectSelection)
        => Render(new SoftwarePpuSnapshotMemory(snapshot), layer, objectSelection, null);

    internal static Rgba32[] Render(SoftwarePpuSnapshotMemory memory, XrayGameplayRenderLayer layer,
        byte objectSelection, Rgba32[]? outputBuffer)
    {
        if (layer.Gameplay.Registers.MainScreenWindowMask != SnesMainScreenLayers.None)
            throw new NotSupportedException("Combining captured hardware windows with X-ray composition is not translated.");
        int count = SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels;
        if (outputBuffer is not null && outputBuffer.Length != count)
            throw new ArgumentException("Unexpected X-ray output dimensions.", nameof(outputBuffer));
        var output = outputBuffer ?? new Rgba32[count];
        var objects = ArrayPool<Rgba32>.Shared.Rent(count);
        try
        {
            var priorities = ArrayPool<byte>.Shared.Rent(count);
            try
            {
                var palettes = ArrayPool<byte>.Shared.Rent(count);
                try { return RenderWithScratch(memory, layer, objectSelection, output, objects, priorities, palettes); }
                finally { ArrayPool<byte>.Shared.Return(palettes); }
            }
            finally { ArrayPool<byte>.Shared.Return(priorities); }
        }
        finally { ArrayPool<Rgba32>.Shared.Return(objects); }
    }

    private static Rgba32[] RenderWithScratch(SoftwarePpuSnapshotMemory memory, XrayGameplayRenderLayer layer,
        byte objectSelection, Rgba32[] output, Rgba32[] objects, byte[] priorities, byte[] palettes)
    {
        var r = layer.Gameplay.Registers;
        int width = SnesPpuLayout.ScreenWidthPixels, height = SnesPpuLayout.ScreenHeightPixels;
        // Registers and backdrop are invariant for this immutable packet. Resolve
        // their flags once rather than repeating Enum.HasFlag in the pixel loop;
        // runtime optimization of that API differs between desktop JIT and Mono AOT.
        bool showBg1 = (r.MainScreenLayers & SnesMainScreenLayers.Bg1) != 0;
        bool showBg2 = (r.MainScreenLayers & SnesMainScreenLayers.Bg2) != 0;
        bool showObjects = (r.MainScreenLayers & SnesMainScreenLayers.Obj) != 0;
        bool halfEnabled = (layer.ColorMath & SnesColorMathControl.Half) != 0;
        bool subtract = (layer.ColorMath & SnesColorMathControl.Subtract) != 0;
        Rgba32 backdrop = memory.Cgram.GetRgba(0);
        var colors = new Rgba32[256];
        for (int color = 0; color < colors.Length; color++) colors[color] = memory.Cgram.GetRgba(color);
        var hud = new SnesBackgroundPixelSampler(memory.Vram, colors, SnesPpuLayout.GameplayHudTilemapWord,
            r.HudCharacterWord, 32, 32, false);
        var bg1Sampler = new SnesBackgroundPixelSampler(memory.Vram, colors, SnesPpuLayout.GameplayBg1TilemapWord,
            r.Bg1CharacterWord, 64, 32, true);
        var bg2Sampler = new SnesBackgroundPixelSampler(memory.Vram, colors, r.Bg2TilemapWord,
            r.Bg2CharacterWord, r.Bg2WidthTiles, r.Bg2HeightTiles, true);
        var subSampler = layer.Subscreen is { } subLayer ? new SnesBackgroundPixelSampler(memory.Vram, colors,
            subLayer.TilemapWord, subLayer.CharacterWord, 32, subLayer.MapHeightTiles, false) : null;
        SnesObjRenderer.RenderResolved(memory.Oam, memory.Vram, memory.Cgram, objectSelection,
            objects.AsSpan(0, output.Length), priorities.AsSpan(0, output.Length), width, height,
            palettes.AsSpan(0, output.Length));
        for (int y = 0; y < SnesPpuLayout.GameplayHudHeightPixels; y++)
        for (int x = 0; x < width; x++)
            output[y * width + x] = hud.Sample(x, y, opaqueZero: true).Color;
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
                var pixel = bg2Sampler.Sample(x + sx, y + sy + SnesPpuLayout.FirstVisibleBackgroundScanline);
                Insert(pixel.Color, pixel.High ? 5 : 2, SnesColorMathControl.Bg2);
            }
            if ((!layer.RevealBlocks || !inside) && showBg1)
            {
                var pixel = bg1Sampler.Sample(x + r.Bg1X, y + r.Bg1Y + SnesPpuLayout.FirstVisibleBackgroundScanline);
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
                    sub = subSampler!.Sample(x + scroll.X, y + scroll.Y).Color;
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

    }
}
