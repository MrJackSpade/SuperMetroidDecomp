using System.Buffers;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Rendering;

/// <summary>Source-aware reference for native Mode-1/window/color-math configurations.</summary>
[PpuLayoutBoundary("backend: software PPU gameplay colour-math compositor")]
public static class SoftwareGameplayColorMathRenderer
{
    /// <summary>Renders gameplay color math using pooled OBJ and priority scratch arrays.</summary>
    internal static Rgba32[] Render(SoftwarePpuSnapshotMemory memory, GameplayColorMathRenderLayer layer,
        Rgba32[]? outputBuffer)
    {
        if (layer.Gameplay.Registers.MainScreenWindowMask != SnesMainScreenLayers.None)
            throw new NotSupportedException("Combining captured hardware windows with gameplay color math is not translated.");
        int count = SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels;
        if (outputBuffer is not null && outputBuffer.Length != count)
            throw new ArgumentException("Unexpected gameplay color-math output dimensions.", nameof(outputBuffer));
        var output = outputBuffer ?? new Rgba32[count];
        var objects = ArrayPool<Rgba32>.Shared.Rent(count);
        try
        {
            var priorities = ArrayPool<byte>.Shared.Rent(count);
            try
            {
                var palettes = ArrayPool<byte>.Shared.Rent(count);
                try { return RenderWithScratch(memory, layer, output, objects, priorities, palettes); }
                finally { ArrayPool<byte>.Shared.Return(palettes); }
            }
            finally { ArrayPool<byte>.Shared.Return(priorities); }
        }
        finally { ArrayPool<Rgba32>.Shared.Return(objects); }
    }

    /// <summary>Resolves Mode-1 main layers, reveal windows, subscreen operands, and fixed-color arithmetic per pixel.</summary>
    private static Rgba32[] RenderWithScratch(SoftwarePpuSnapshotMemory memory, GameplayColorMathRenderLayer layer,
        Rgba32[] output, Rgba32[] objects, byte[] priorities, byte[] palettes)
    {
        var r = layer.Gameplay.Registers;
        int width = SnesPpuLayout.ScreenWidthPixels, height = SnesPpuLayout.ScreenHeightPixels;
        bool halfEnabled = (layer.ColorMath & SnesColorMathControl.Half) != 0;
        bool subtract = (layer.ColorMath & SnesColorMathControl.Subtract) != 0;
        Rgba32 backdrop = memory.Cgram.GetRgba(0);
        var colors = new Rgba32[256];
        for (int color = 0; color < colors.Length; color++) colors[color] = memory.Cgram.GetRgba(color);
        RoomBackgrounds rooms = layer.Gameplay.Rooms;
        var hud = new BgPictureSampler(colors, memory.Cgram, rooms.ShownBg3.Shown(0, SnesPpuLayout.TilemapPageWidthInTiles));
        // BG1 and BG2 draw from the pictures the room publishes (#1403).
        var bg1Sampler = new BgPictureSampler(colors, memory.Cgram, rooms.Bg1Source);
        var bg2Sampler = new BgPictureSampler(colors, memory.Cgram, rooms.Bg2Shown(r.Bg2MapSize));
        var subSampler = layer.Subscreen is { } subLayer ? new BgPictureSampler(colors, memory.Cgram,
            subLayer.Plane.Shown(subLayer.FirstRow, subLayer.MapHeightTiles)) : null;
        SnesObjRenderer.RenderResolved(memory.Sprites.Draws, memory.Cgram, new ObjRasterTargets(objects.AsSpan(0, output.Length), priorities.AsSpan(0, output.Length), palettes.AsSpan(0, output.Length)), new Extent16<ScreenPixels>(checked((ushort)width), checked((ushort)height)));
        for (int y = 0; y < SnesPpuLayout.GameplayHudHeightPixels; y++)
        for (int x = 0; x < width; x++)
            output[y * width + x] = hud.Sample(new Position16<BgPlanePixels>((ushort)x, (ushort)y), colorZero: ColorZero.Opaque).Color;
        for (int y = SnesPpuLayout.GameplayHudHeightPixels; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int i = y * width + x, lineIndex = y - SnesPpuLayout.GameplayHudHeightPixels;
            var mainScreen = layer.Gameplay.MainScreenLayersByLine.IsEmpty ? r.MainScreenLayers
                : (SnesMainScreenLayers)layer.Gameplay.MainScreenLayersByLine[lineIndex];
            bool showBg1 = (mainScreen & SnesMainScreenLayers.Bg1) != 0;
            bool showBg2 = (mainScreen & SnesMainScreenLayers.Bg2) != 0 &&
                y >= r.Bg2FirstScanline && y < r.Bg2EndScanline;
            bool showObjects = (mainScreen & SnesMainScreenLayers.Obj) != 0;
            XrayWindowLine window = layer.Lines[y];
            bool inside = x >= window.Left && x <= window.Right;
            var winner = backdrop;
            int rank = -1;
            SnesColorMathControl source = SnesColorMathControl.Backdrop;
            // Window selection precedes priority resolution: BG1 is suppressed inside
            // the reveal beam, BG2 outside it. Excluded rooms leave both unmasked.
            if ((!layer.RevealBlocks || inside) && showBg2)
            {
                var pixel = bg2Sampler.Sample(Bg2Source(r, layer.Gameplay, lineIndex, new Position16<ScreenPixels>((ushort)x, (ushort)y)));
                Insert(pixel.Color, pixel.High ? 5 : 2, SnesColorMathControl.Bg2);
            }
            if ((!layer.RevealBlocks || !inside) && showBg1)
            {
                var pixel = bg1Sampler.Sample(r.Bg1Scroll.AsBgPlaneScroll() +
                    new Offset16<BgPlanePixels>((short)x, (short)(y + SnesPpuLayout.FirstVisibleBackgroundScanline)));
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
                if (layer.SubscreenUsesBg2)
                {
                    sub = bg2Sampler.Sample(Bg2Source(r, layer.Gameplay, lineIndex, new Position16<ScreenPixels>((ushort)x, (ushort)y))).Color;
                }
                if (layer.AddSubscreen && layer.Subscreen is { } bg3 && subSampler is not null && y >= bg3.FirstScanline)
                {
                    Position16<BgPlanePixels> scroll = bg3.Scrolls[y];
                    var pixel = subSampler.Sample(scroll + new Offset16<BgPlanePixels>((short)x, (short)y));
                    // Gameplay uses Mode 1 with BG3 priority: high BG3 beats both
                    // BG2 priorities; low BG3 is behind both. Transparent pixels
                    // never replace an opaque operand on the same subscreen.
                    if (pixel.Color.A != 0 && (pixel.High || sub.A == 0))
                        sub = pixel.Color;
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

    /// <summary>
    /// The BG2 plane pixel sampled at a screen pixel: the line's HDMA scroll replacements (or the
    /// latched registers), then mosaic grouping.
    /// </summary>
    private static Position16<BgPlanePixels> Bg2Source(OrdinaryGameplayRegisters r, OrdinaryGameplayRenderLayer gameplay,
        int lineIndex, Position16<ScreenPixels> screen)
    {
        Position16<Bg2Pixels> scroll = gameplay.Bg2ScrollAt(lineIndex);
        return new(unchecked((ushort)r.Bg2Mosaic.SourceX(screen.X, scroll.X)),
            unchecked((ushort)r.Bg2Mosaic.SourceY(screen.Y, scroll.Y)));
    }
}
