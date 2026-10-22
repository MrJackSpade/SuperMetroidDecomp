using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Rendering;

/// <summary>Software projection of whole-frame sprite draws into an RGBA frame, each read from its bound artwork.</summary>
[PpuLayoutBoundary("backend: software OBJ rasterizer reading the OBJ palette rows of CGRAM")]
public static class SnesObjRenderer
{
    /// <summary>Sentinel used when no opaque OBJ owns a raster pixel.</summary>
    public const byte TransparentPriority = byte.MaxValue;

    /// <summary>
    /// Composites the complete OBJ plane over an existing backdrop/BG. This is only
    /// for compositions without BG insertion between OBJ priorities (such as titles).
    /// Transparent samples preserve the destination; optional palette ownership is
    /// reset and follows the winning draw part for subsequent color math.
    /// </summary>
    public static void CompositeUnfiltered(ImmutableArray<SpriteDraw> draws, IPaletteColors cgram,
        Span<Rgba32> pixels, Span<byte> palettes = default, Extent16<ScreenPixels>? size = null)
    {
        Ensure.NotNull(cgram);
        Extent16<ScreenPixels> frame = RequireFrame(size);
        if (pixels.Length != frame.X * frame.Y || (!palettes.IsEmpty && palettes.Length != pixels.Length))
            throw new ArgumentException("Unexpected OBJ composition buffer dimensions.");
        palettes.Fill(TransparentPriority);
        Paint(frame, draws, cgram, selectedPriority: null, new ObjRasterTargets(pixels, palettes: palettes));
    }

    /// <summary>
    /// Resolves the draws into caller-owned scratch buffers. The gameplay compositor rents these
    /// buffers because their contents live only until BG priority composition completes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void RenderResolved(
        ImmutableArray<SpriteDraw> draws,
        IPaletteColors cgram,
        ObjRasterTargets targets,
        Extent16<ScreenPixels>? size = null)
    {
        Span<Rgba32> pixels = targets.Pixels;
        Span<byte> priorities = targets.Priorities;
        Span<byte> palettes = targets.Palettes;
        Ensure.NotNull(cgram);
        Extent16<ScreenPixels> frame = RequireFrame(size);
        int pixelCount = frame.X * frame.Y;
        if (pixels.Length != pixelCount)
            throw new ArgumentException("OBJ color scratch buffer has the wrong size.", nameof(pixels));
        if (priorities.Length != pixelCount)
            throw new ArgumentException("OBJ priority scratch buffer has the wrong size.", nameof(priorities));
        if (!palettes.IsEmpty && palettes.Length != pixelCount)
            throw new ArgumentException("OBJ palette scratch buffer has the wrong size.", nameof(palettes));

        pixels.Clear();
        priorities.Fill(TransparentPriority);
        palettes.Fill(TransparentPriority);

        // Walking backwards makes each earlier draw replace the previous winner. Unlike a priority-filtered plane, the
        // resolved buffer keeps that winner's priority beside its color so no later pass can resurrect a sprite that
        // already lost the OBJ-vs-OBJ comparison.
        Paint(frame, draws, cgram, selectedPriority: null, targets);
    }

    /// <summary>
    /// Paints every draw from the back of the emission order to the front, so the earliest-emitted draw finally owns
    /// each pixel; within a draw the first part is frontmost.
    /// </summary>
    internal static void Paint(Extent16<ScreenPixels> frame, ImmutableArray<SpriteDraw> draws, IPaletteColors cgram,
        int? selectedPriority, ObjRasterTargets targets)
    {
        if (draws.IsDefault)
            throw new ArgumentException("A frame needs a draw list.", nameof(draws));
        for (int draw = draws.Length - 1; draw >= 0; draw--)
            DrawEntry(frame, draws[draw], cgram, selectedPriority, targets);
    }

    /// <summary>Rasterizes one whole-frame draw by its kind.</summary>
    private static void DrawEntry(Extent16<ScreenPixels> frame, SpriteDraw draw, IPaletteColors cgram,
        int? selectedPriority, ObjRasterTargets targets)
    {
        if (draw is CharacterSpriteDraw character)
            DrawCharacters(frame, character, cgram, selectedPriority, targets);
        else if (draw is ImageSpriteDraw image)
            DrawImage(frame, image, cgram, selectedPriority, targets);
        else
            throw new InvalidDataException($"The software renderer cannot draw a {draw.GetType().Name}.");
    }

    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>The palette row a blending image reports, so colour math treats it as the upper rows' sprites.</summary>
    /// <summary>Colours in one sprite palette row.</summary>
    private const int RowColours = 16;

    /// <summary>
    /// Rasterizes one image draw from its pre-coloured pixels, a pixel with zero alpha being transparent. While a palette
    /// effect has changed a row the frame uses, its pixels show that row's live colours instead.
    /// </summary>
    private static void DrawImage(Extent16<ScreenPixels> frame, ImageSpriteDraw draw, IPaletteColors cgram, int? selectedPriority,
        ObjRasterTargets targets)
    {
        Offset16<ScreenPixels> corner = draw.Corner;
        bool standard = draw.PaletteOverride is null && draw.Image.ShowsStandardColours(cgram.Colors);
        for (ushort y = 0; y < draw.Image.Size.Y; y++)
        {
            int screenY = corner.Y + y;
            if ((uint)screenY >= frame.Y)
                continue;
            for (ushort x = 0; x < draw.Image.Size.X; x++)
            {
                int screenX = corner.X + x;
                if ((uint)screenX >= frame.X)
                    continue;
                // Each pixel's palette row decides its colour math, as the cartridge decides it for each OBJ.
                int index = draw.PaletteIndexAt(new(x, y));
                if (index == 0)
                    continue;
                Rgba32 pixel;
                byte pixelRow = (byte)(index / RowColours);
                if (standard)
                    pixel = draw.PixelAt(new(x, y));
                else
                {
                    if (draw.PaletteOverride is int overriding)
                    {
                        // The cartridge ORs the draw's palette bits into each part's own; the override takes the
                        // place of those the image was coloured with, and its row decides colour math.
                        pixelRow = (byte)(draw.PartRowAt(new(x, y)) | overriding);
                        index = pixelRow * RowColours + index % RowColours;
                    }
                    pixel = cgram.GetRgba(SpriteColours + index);
                }
                Plot(screenY * frame.X + screenX, pixel, pixelRow, draw.PriorityAt(new(x, y)), selectedPriority, targets);
            }
        }
    }

    /// <summary>Rasterizes one character draw from its bound artwork, last part first so the first part ends in front.</summary>
    private static void DrawCharacters(Extent16<ScreenPixels> frame, CharacterSpriteDraw draw, IPaletteColors cgram,
        int? selectedPriority, ObjRasterTargets targets)
    {
        for (int index = draw.Frame.Parts.Length - 1; index >= 0; index--)
        {
            SpriteScreenPart part = draw.Place(index);
            for (ushort row = 0; row < part.Size.Y; row++)
            {
                int screenY = part.Position.Y + row;
                if ((uint)screenY >= frame.Y)
                    continue;
                for (ushort column = 0; column < part.Size.X; column++)
                {
                    int screenX = part.Position.X + column;
                    if ((uint)screenX >= frame.X)
                        continue;
                    int character = part.CharacterAt(new(column, row), out Position16<CharacterPixels> pixel);
                    byte colorIndex = draw.Artwork.ColorIndex(character, pixel);
                    // OBJ palettes live in the upper half of CGRAM. Each OBJ palette number
                    // selects one 16-color row: 128 + palette*16 + the 4-bpp pixel value.
                    if (colorIndex != 0)
                        Plot(screenY * frame.X + screenX, cgram.GetRgba(128 + part.Palette * 16 + colorIndex), (byte)part.Palette,
                            (byte)part.Priority, selectedPriority, targets);
                }
            }
        }
    }

    /// <summary>Writes one opaque OBJ pixel under the caller's priority ownership mode.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Plot(int destination, Rgba32 color, byte palette, byte priority,
        int? selectedPriority, ObjRasterTargets targets)
    {
        Span<Rgba32> output = targets.Pixels;
        Span<byte> resolvedPriorities = targets.Priorities;
        Span<byte> resolvedPalettes = targets.Palettes;
        if (!resolvedPriorities.IsEmpty)
        {
            // Every call reaching this point represents an opaque source pixel.
            // The back-to-front walk guarantees that overwriting both fields here
            // leaves the earliest-emitted object as the final winner.
            output[destination] = color;
            resolvedPriorities[destination] = priority;
            // Color math eligibility depends on the winning OBJ palette, not
            // RGB: different palettes may contain exactly the same color.
            if (!resolvedPalettes.IsEmpty)
                resolvedPalettes[destination] = palette;
            return;
        }

        // This assignment is deliberately performed for opaque pixels belonging
        // to another priority plane. Because objects are visited back to front,
        // transparent here means "an earlier OBJ owns this pixel, but its BG-relative
        // priority is emitted by a different pass." It erases any later object
        // previously painted into this plane while leaving genuinely transparent
        // source pixels alone.
        if (selectedPriority.HasValue && priority != selectedPriority.Value)
        {
            output[destination] = default;
            return;
        }

        output[destination] = color;
        if (!resolvedPalettes.IsEmpty)
            resolvedPalettes[destination] = palette;
    }

    /// <summary>The requested raster, which must be non-empty; the visible screen when omitted.</summary>
    internal static Extent16<ScreenPixels> RequireFrame(Extent16<ScreenPixels>? size)
    {
        Extent16<ScreenPixels> frame = size ?? SnesPpuLayout.ScreenSize;
        if (frame.X == 0 || frame.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(size), frame, "An OBJ raster must have positive dimensions.");
        return frame;
    }
}
