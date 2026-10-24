using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// The installed escape-timer typeface as the import pipeline reads it (#1409): selected timer contours separated from
/// shared pixel relationships and calculated default fill sites, compiled to the timer's four-bit planar characters.
/// </summary>
public sealed class EscapeTimerCharacters : EscapeTimerTileAtlas
{
    /// <summary>Independent fill sites of the native timer typeface beyond the calculated defaults.</summary>
    internal readonly HashSet<int> fillPixels;
    /// <summary>PNG edits that change whether a calculated basis pixel uses the fill ink.</summary>
    internal readonly Dictionary<int, bool> fillOverrides;
    /// <summary>Independent PNG colors that differ from a shared source pixel or calculated outline.</summary>
    internal readonly Dictionary<int, byte> pixelOverrides;

    /// <summary>Retains the typeface's separated pixels beside the characters they compile to.</summary>
    /// <param name="glyphs">The separated timer pixels.</param>
    private EscapeTimerCharacters(TimerGlyphs glyphs) : base(glyphs.Characters())
    {
        fillPixels = glyphs.fillPixels;
        fillOverrides = glyphs.fillOverrides;
        pixelOverrides = glyphs.pixelOverrides;
    }

    /// <summary>Loads the indexed PNG and compiles its pixels to the cartridge's two native DMA pages.</summary>
    /// <exception cref="InvalidDataException">The PNG is malformed, the wrong size, or compiles to the wrong length.</exception>
    public static EscapeTimerCharacters Read(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png,
            new(EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height));
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4);
        if (planar.Length != EscapeTimerTileAtlasFormat.TotalByteCount)
            throw new InvalidDataException(
                $"Escape timer artwork compiled to {planar.Length} bytes; expected {EscapeTimerTileAtlasFormat.TotalByteCount}.");
        return new(new TimerGlyphs(image.Pixels));
    }

    /// <summary>The timer typeface's pixels, held as calculated fill and outline rules and their independent edits.</summary>
    private sealed class TimerGlyphs
    {
        /// <summary>Independent fill sites of the native timer typeface beyond the calculated defaults.</summary>
        internal readonly HashSet<int> fillPixels;
        /// <summary>PNG edits that change whether a calculated basis pixel uses the fill ink.</summary>
        internal readonly Dictionary<int, bool> fillOverrides;
        /// <summary>Independent PNG colors that differ from a shared source pixel or calculated outline.</summary>
        internal readonly Dictionary<int, byte> pixelOverrides;

        /// <summary>Separates selected timer contours from shared pixel relationships and calculated default fill sites.</summary>
        internal TimerGlyphs(byte[] pixels)
        {
            fillPixels = new HashSet<int>();
            fillOverrides = new Dictionary<int, bool>();
            for (int index = 0; index < pixels.Length; index++)
            {
                if (EscapeTimerGlyphDefinitions.SourcePixel(index) != index) continue;
                bool filled = pixels[index] == EscapeTimerGlyphDefinitions.FillInk(index);
                if (EscapeTimerGlyphDefinitions.TryDefaultFill(index, out bool expected))
                {
                    if (filled != expected) fillOverrides.Add(index, filled);
                }
                else if (filled) fillPixels.Add(index);
            }
            pixelOverrides = Enumerable.Range(0, pixels.Length)
                .Where(index => EscapeTimerGlyphDefinitions.SourcePixel(index) != index && pixels[index] != pixels[EscapeTimerGlyphDefinitions.SourcePixel(index)])
                .ToDictionary(index => index, index => pixels[index]);
            foreach (int index in Enumerable.Range(0, pixels.Length))
                if (EscapeTimerGlyphDefinitions.SourcePixel(index) == index && !IsFill(index) &&
                    pixels[index] != EscapeTimerGlyphDefinitions.Outline(index, IsFill))
                    pixelOverrides.Add(index, pixels[index]);
        }

        /// <summary>Determines whether a pixel uses the glyph fill ink, honoring an independently edited shared pixel.</summary>
        private bool IsFill(int index)
        {
            int source = EscapeTimerGlyphDefinitions.SourcePixel(index);
            return source != index && pixelOverrides.TryGetValue(index, out byte value)
                ? value == EscapeTimerGlyphDefinitions.FillInk(index) : BasisFill(source);
        }

        /// <summary>Resolves a basis pixel from its PNG override, native calculated fill, or retained fill-site set.</summary>
        private bool BasisFill(int index) => fillOverrides.TryGetValue(index, out bool supplied) ? supplied :
            EscapeTimerGlyphDefinitions.TryDefaultFill(index, out bool calculated) ? calculated : fillPixels.Contains(index);

        /// <summary>Resolves an edited pixel, shared source pixel, or calculated fill and outline ink.</summary>
        private byte Pixel(int index)
        {
            if (pixelOverrides.TryGetValue(index, out byte value)) return value;
            int source = EscapeTimerGlyphDefinitions.SourcePixel(index);
            if (source != index) return Pixel(source);
            return BasisFill(index) ? EscapeTimerGlyphDefinitions.FillInk(index) : EscapeTimerGlyphDefinitions.Outline(index, IsFill);
        }

        /// <summary>Rebuilds all 25 timer characters in native four-bit planar DMA order.</summary>
        internal byte[] Characters() => SnesPlanarTileEncoder.Encode(
            Enumerable.Range(0, EscapeTimerTileAtlasFormat.Width * EscapeTimerTileAtlasFormat.Height).Select(Pixel).ToArray(),
            new(EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height), EscapeTimerTileAtlasFormat.BitsPerPixel);
    }
}
