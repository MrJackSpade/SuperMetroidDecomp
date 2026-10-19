using SuperMetroid.Core.Assets;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Rendering;

/// <summary>Software projection of SNES 4-bpp OBJ tiles into an RGBA frame.</summary>
/// <remarks>
/// Draws the OAM sprites that actors not yet converted still emit, read through VRAM, together with
/// whole-frame sprite draws read from their own artwork, in one emission order.
/// </remarks>
[PpuLayoutBoundary("backend: software PPU OBJ rasterizer (OAM evaluation and OBJ character fetch)")]
public static class SnesObjRenderer
{
    /// <summary>The ninth OBJ character-name bit, selecting the second name table.</summary>
    private const int NameSelectBit = 1 << 8;
    /// <summary>The values of OAM's nine-bit X: the upper half lies left of the screen.</summary>
    private const int XRange = 512;
    /// <summary>The values of OAM's eight-bit Y.</summary>
    private const int YRange = 256;

    /// <summary>Sentinel used when no opaque OBJ owns a raster pixel.</summary>
    public const byte TransparentPriority = byte.MaxValue;

    /// <summary>
    /// Decodes OBSEL bits 7-5 into small and large dimensions. Modes six and seven retain
    /// the rectangular interlace layouts; gameplay normally uses mode zero (8x8 / 16x16).
    /// </summary>
    private static readonly (int SmallWidth, int SmallHeight, int LargeWidth, int LargeHeight)[] SizeModes =
    [
        (8, 8, 16, 16),
        (8, 8, 32, 32),
        (8, 8, 64, 64),
        (16, 16, 32, 32),
        (16, 16, 64, 64),
        (32, 32, 64, 64),
        (16, 32, 32, 64),
        (16, 32, 32, 32),
    ];

    /// <summary>
    /// Composites the complete OBJ plane over an existing backdrop/BG. This is only
    /// for compositions without BG insertion between OBJ priorities (such as titles).
    /// Transparent samples preserve the destination; optional palette ownership is
    /// reset and follows the winning OAM record for subsequent color math.
    /// </summary>
    public static void CompositeUnfiltered(ObjSpriteImage sprites, SnesVram vram, SnesCgram cgram,
        byte obsel, Span<Rgba32> pixels, Span<byte> palettes = default,
        Extent16<ScreenPixels>? size = null)
    {
        Ensure.NotNull(sprites.Oam);
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Extent16<ScreenPixels> frame = RequireFrame(size);
        if (pixels.Length != frame.X * frame.Y || (!palettes.IsEmpty && palettes.Length != pixels.Length))
            throw new ArgumentException("Unexpected OBJ composition buffer dimensions.");
        palettes.Fill(TransparentPriority);
        Paint(frame, sprites, vram, cgram, obsel, selectedPriority: null, new ObjRasterTargets(pixels, palettes: palettes));
    }

    /// <summary>
    /// Resolves OAM into caller-owned scratch buffers. The gameplay compositor rents these
    /// buffers because their contents live only until BG priority composition completes;
    /// the allocating overload remains available to diagnostics which retain the raster.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void RenderResolved(
        ObjSpriteImage sprites,
        SnesVram vram,
        SnesCgram cgram,
        byte obsel,
        ObjRasterTargets targets,
        Extent16<ScreenPixels>? size = null)
    {
        Span<Rgba32> pixels = targets.Pixels;
        Span<byte> priorities = targets.Priorities;
        Span<byte> palettes = targets.Palettes;
        Ensure.NotNull(sprites.Oam);
        Ensure.NotNull(vram);
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

        // As in Render, walking backwards makes each successively lower OAM number replace
        // the previous winner. Unlike a priority-filtered plane, the resolved buffer keeps
        // that winner's priority beside its color so no later pass can resurrect a sprite
        // that already lost the OBJ-vs-OBJ comparison.
        Paint(frame, sprites, vram, cgram, obsel, selectedPriority: null, targets);

    }

    /// <summary>
    /// Paints every OAM sprite and whole-frame draw from the back of the emission order to the front, so the
    /// earliest-emitted object finally owns each pixel. A draw sits in front of OAM sprite
    /// <see cref="OrderedSpriteDraw.OamOrdinal"/> and later ones; within a draw the first part is frontmost.
    /// </summary>
    internal static void Paint(Extent16<ScreenPixels> frame, ObjSpriteImage sprites,
        SnesVram vram, SnesCgram cgram, byte obsel, int? selectedPriority, ObjRasterTargets targets)
    {
        OamBuffer oam = sprites.Oam;
        ReadOnlySpan<OrderedSpriteDraw> draws = sprites.Draws.AsSpan();
        int draw = draws.Length - 1;
        for (int spriteIndex = oam.LastFinalizedSpriteCount - 1; spriteIndex >= 0; spriteIndex--)
        {
            for (; draw >= 0 && draws[draw].OamOrdinal > spriteIndex; draw--)
                DrawEntry(frame, draws[draw].Draw, cgram, selectedPriority, targets);
            DrawSprite(frame, oam.GetEntry(spriteIndex), vram, cgram, obsel, selectedPriority, targets);
        }
        for (; draw >= 0; draw--)
            DrawEntry(frame, draws[draw].Draw, cgram, selectedPriority, targets);
    }

    /// <summary>Rasterizes one whole-frame draw from its bound artwork, last part first so the first part ends in front.</summary>
    private static void DrawEntry(Extent16<ScreenPixels> frame, SpriteDraw draw, SnesCgram cgram,
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
                    if (colorIndex != 0)
                        Plot(screenY * frame.X + screenX, colorIndex, (byte)part.Palette, (byte)part.Priority, cgram, selectedPriority, targets);
                }
            }
        }
    }

    /// <summary>
    /// Rasterizes one OAM entry with nine-bit X wrapping, eight-bit Y wrapping, 4-bpp
    /// character decoding, and the caller's chosen priority ownership mode.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void DrawSprite(
        Extent16<ScreenPixels> frame,
        OamEntry entry,
        SnesVram vram,
        SnesCgram cgram,
        byte obsel,
        int? selectedPriority,
        ObjRasterTargets targets)
    {
        int frameWidth = frame.X;
        int frameHeight = frame.Y;
        var mode = SizeModes[(obsel >> 5) & 7];
        int objectWidth = entry.IsLarge ? mode.LargeWidth : mode.SmallWidth;
        int objectHeight = entry.IsLarge ? mode.LargeHeight : mode.SmallHeight;

        // OAM X is a signed/modular 9-bit coordinate: $1F0 means -16. Y is similarly an
        // 8-bit coordinate, but the visible NTSC field ends at 223, so $E0-$FF wraps above
        // the screen. Converting before clipping naturally draws partially visible OBJs.
        Position16<OamPixels> position = entry.Position;
        int objectX = position.X >= XRange / 2 ? position.X - XRange : position.X;
        int objectY = position.Y >= SnesPpuLayout.ScreenHeightPixels ? position.Y - YRange : position.Y;

        for (int destinationY = 0; destinationY < objectHeight; destinationY++)
        {
            int screenY = objectY + destinationY;
            if ((uint)screenY >= frameHeight)
                continue;

            int sourceY = entry.FlipY ? objectHeight - 1 - destinationY : destinationY;
            for (int destinationX = 0; destinationX < objectWidth; destinationX++)
            {
                int screenX = objectX + destinationX;
                if ((uint)screenX >= frameWidth)
                    continue;

                int sourceX = entry.FlipX ? objectWidth - 1 - destinationX : destinationX;
                int tileColumn = sourceX >> 3;
                int tileRow = sourceY >> 3;

                // OBJ character names occupy a conceptual 16-tile-wide grid. Horizontal
                // subtiles increment the low nibble and vertical subtiles add $10. The
                // ninth name-select bit stays fixed if the low byte wraps at $FF.
                int tileNumber = (entry.TileNumber & NameSelectBit)
                    | unchecked((byte)(entry.TileNumber + tileRow * 16 + tileColumn));
                byte colorIndex = Decode4BppPixel(
                    vram,
                    ResolveTileByteAddress(tileNumber, obsel),
                    new Position16<CharacterPixels>((ushort)(sourceX & 7), (ushort)(sourceY & 7)));

                if (colorIndex == 0)
                    continue;

                Plot(screenY * frameWidth + screenX, colorIndex, (byte)entry.Palette, (byte)entry.Priority, cgram, selectedPriority, targets);
            }
        }
    }

    /// <summary>Writes one opaque OBJ pixel under the caller's priority ownership mode.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Plot(int destination, byte colorIndex, byte palette, byte priority, SnesCgram cgram,
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
            output[destination] = cgram.GetRgba(128 + palette * 16 + colorIndex);
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

        // OBJ palettes live in the upper half of CGRAM. Each OBJ palette number
        // selects one 16-color row: 128 + palette*16 + the 4-bpp pixel value.
        output[destination] = cgram.GetRgba(128 + palette * 16 + colorIndex);
        if (!resolvedPalettes.IsEmpty)
            resolvedPalettes[destination] = palette;
    }

    /// <summary>Resolves a nine-bit OBJ character name through OBSEL into byte VRAM.</summary>
    public static int ResolveTileByteAddress(int tileNumber, byte obsel)
    {
        if ((uint)tileNumber >= 2 * NameSelectBit)
            throw new ArgumentOutOfRangeException(nameof(tileNumber));

        // OBSEL bits 0-2 choose the base in $2000-word ($4000-byte) units. A set ninth
        // tile bit selects the second name table, whose programmable displacement is
        // ((bits 3-4) + 1) * $1000 words. Each 4-bpp 8x8 tile occupies 16 VRAM words.
        int baseWord = (obsel & 7) << 13;
        int nameSelectWordOffset = (((obsel >> 3) & 3) + 1) << 12;
        int tileWord = baseWord + unchecked((byte)tileNumber) * 16;
        if ((tileNumber & NameSelectBit) != 0)
            tileWord += nameSelectWordOffset;

        // Physical VRAM has only 15 word-address bits, so register arithmetic wraps.
        return (tileWord & (SnesPpuLayout.VramWordCount - 1)) * 2;
    }

    /// <summary>The requested raster, which must be non-empty; the visible screen when omitted.</summary>
    internal static Extent16<ScreenPixels> RequireFrame(Extent16<ScreenPixels>? size)
    {
        Extent16<ScreenPixels> frame = size ?? SnesPpuLayout.ScreenSize;
        if (frame.X == 0 || frame.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(size), frame, "An OBJ raster must have positive dimensions.");
        return frame;
    }

    /// <summary>Combines the four planar VRAM bits at one 8x8 character pixel into its palette index.</summary>
    private static byte Decode4BppPixel(SnesVram vram, int tileByteAddress, Position16<CharacterPixels> pixel)
    {
        // Planes 0/1 are interleaved in bytes 0-15; planes 2/3 repeat that layout in
        // bytes 16-31. The left pixel is bit 7, exactly as in SnesGraphics' bulk decoder.
        int mask = 1 << (7 - pixel.X);
        int plane01 = tileByteAddress + pixel.Y * 2;
        int plane23 = tileByteAddress + 16 + pixel.Y * 2;
        return (byte)(
            ((vram.ReadByte(plane01) & mask) != 0 ? 1 : 0)
            | ((vram.ReadByte(plane01 + 1) & mask) != 0 ? 2 : 0)
            | ((vram.ReadByte(plane23) & mask) != 0 ? 4 : 0)
            | ((vram.ReadByte(plane23 + 1) & mask) != 0 ? 8 : 0));
    }
}
