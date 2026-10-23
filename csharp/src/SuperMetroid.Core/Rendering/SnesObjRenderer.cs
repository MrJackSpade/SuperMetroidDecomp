using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Rendering;

/// <summary>Software projection of whole-frame sprite draws into an RGBA frame, each a pre-coloured image (#1392).</summary>
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
    public static void CompositeUnfiltered(ImmutableArray<SpriteDraw> draws,
        Span<Rgba32> pixels, Span<byte> palettes = default, Extent16<ScreenPixels>? size = null)
    {
        Extent16<ScreenPixels> frame = RequireFrame(size);
        if (pixels.Length != frame.X * frame.Y || (!palettes.IsEmpty && palettes.Length != pixels.Length))
            throw new ArgumentException("Unexpected OBJ composition buffer dimensions.");
        palettes.Fill(TransparentPriority);
        Paint(frame, draws, selectedPriority: null, new ObjRasterTargets(pixels, palettes: palettes));
    }

    /// <summary>
    /// Resolves the draws into caller-owned scratch buffers. The gameplay compositor rents these
    /// buffers because their contents live only until BG priority composition completes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void RenderResolved(
        ImmutableArray<SpriteDraw> draws,
        ObjRasterTargets targets,
        Extent16<ScreenPixels>? size = null)
    {
        Span<Rgba32> pixels = targets.Pixels;
        Span<byte> priorities = targets.Priorities;
        Span<byte> palettes = targets.Palettes;
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
        Paint(frame, draws, selectedPriority: null, targets);
    }

    /// <summary>
    /// Paints every draw from the back of the emission order to the front, so the earliest-emitted draw finally owns
    /// each pixel; within a draw the first part is frontmost.
    /// </summary>
    internal static void Paint(Extent16<ScreenPixels> frame, ImmutableArray<SpriteDraw> draws,
        int? selectedPriority, ObjRasterTargets targets)
    {
        if (draws.IsDefault)
            throw new ArgumentException("A frame needs a draw list.", nameof(draws));
        for (int draw = draws.Length - 1; draw >= 0; draw--)
            DrawEntry(frame, draws[draw], selectedPriority, targets);
    }

    /// <summary>Rasterizes one whole-frame draw by its kind.</summary>
    private static void DrawEntry(Extent16<ScreenPixels> frame, SpriteDraw draw,
        int? selectedPriority, ObjRasterTargets targets)
    {
        if (draw is ImageSpriteDraw image)
            DrawImage(frame, image, selectedPriority, targets);
        else
            throw new InvalidDataException($"The software renderer cannot draw a {draw.GetType().Name}.");
    }

    /// <summary>The row a blending pixel reports, so colour math treats it as the upper rows' sprites.</summary>
    private const byte BlendingRow = 4;

    /// <summary>Colours in one sprite palette row.</summary>
    private const int RowColours = 16;

    /// <summary>
    /// Rasterizes one image draw in its resolved colours: each pixel shows its image's own colour, or the colour its key
    /// shows in the frame, and blends as that key does.
    /// </summary>
    /// <exception cref="InvalidOperationException">The draw's colours were not resolved when its frame was published.</exception>
    private static void DrawImage(Extent16<ScreenPixels> frame, ImageSpriteDraw draw, int? selectedPriority,
        ObjRasterTargets targets)
    {
        Offset16<ScreenPixels> corner = draw.Corner;
        DrawColours colours = draw.Colours ?? throw new InvalidOperationException(
            "A sprite draw is drawn once its frame has resolved its colours.");
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
                int index = draw.PaletteIndexAt(new(x, y));
                if (index == 0)
                    continue;
                int key = colours.Key(index / RowColours, draw.PartRowAt(new(x, y)));
                Rgba32 pixel = colours.ShowsOwnColours(key)
                    ? draw.PixelAt(new(x, y))
                    : colours.Colour(key, index % RowColours).ToRgba32();
                // Colour math reads only whether a pixel's row is among the blending ones.
                byte blendRow = colours.Blends(key) ? BlendingRow : (byte)0;
                Plot(screenY * frame.X + screenX, pixel, blendRow, (byte)draw.PriorityAt(new(x, y)), selectedPriority, targets);
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
