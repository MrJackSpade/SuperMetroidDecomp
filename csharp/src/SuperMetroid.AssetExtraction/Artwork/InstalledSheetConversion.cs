using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Turns a sheet the import composed against the cartridge's colours into what the installation holds (#1633): pixels that
/// are only their colours, shades that recolour by colour key, and the purpose-named maps the sheet needs. What a composed
/// palette index makes of a pixel (clear, opaque or blending) is decided here, once, and installed as the pixel's alpha, the
/// frame's or sheet's blending and the keys its shades name; nothing installed names a palette index.
/// </summary>
/// <remarks>
/// <para>
/// Pixels: an opaque pixel keeps its composed colour. A clear pixel that keeps a colour of its own, which a HUD or room
/// effect sheet's pixels of their group's first colour do, keeps that colour's RGB under alpha zero. Every other clear pixel
/// is clear black.
/// </para>
/// <para>
/// Colour keys: the composed indices a sheet's pixels and backdrop use are grouped by the colour each shows. Within one
/// colour, indices every shade writes alike (each shade leaving them all unwritten or writing them all with one colour) share
/// a recolour region; the clear pixels that keep no colour are one more group, which no shade writes. A colour's regions
/// are numbered in the order their first pixel lies in reading order, a region no pixel shows (the backdrop's) last. A
/// sheet whose every colour has one region has no recolour-region map.
/// </para>
/// <para>
/// Blending: a sheet composed in the sprite colours is drawn as sprites (gameplay's actors and every screen's sprite
/// entries, <see cref="ImageSpriteDraw"/>), and only a sprite draw blends by its pictures; every other sheet is drawn on a
/// layer, which decides its blending. Of a sprite-drawn sheet, a frame every opaque pixel of which blends is installed as a
/// blending frame, and a sheet with a frame that blends in some opaque pixels and not others gets a blend map.
/// </para>
/// </remarks>
internal static class InstalledSheetConversion
{
    /// <summary>Colours in one of the four-colour groups a HUD or room effect sheet is composed in.</summary>
    private const int TwoBppGroupColours = 4;

    /// <summary>What the installation holds of a sheet composed as <paramref name="composed"/>.</summary>
    /// <param name="composed">The composed manifest: the colours the indices name, the shades by index, the backdrop and the frames.</param>
    /// <param name="size">The sheet's width and height.</param>
    /// <param name="pixels">The composed pixels, row by row from the top.</param>
    /// <param name="paletteIndices">Each pixel's composed palette index.</param>
    /// <exception cref="ArgumentException">The planes are not one entry per pixel of <paramref name="size"/>.</exception>
    /// <exception cref="InvalidDataException">
    /// The composed manifest names the wrong number of colours, a pixel, the backdrop or a shade names an index past them, a
    /// shade names an index twice, a pixel's colour or alpha disagrees with its index, or a frame leaves the sheet.
    /// </exception>
    internal static InstalledSheet Install(SpriteSheetDocument composed, Extent16<ImagePixels> size, ReadOnlySpan<RgbaPixel> pixels,
        ReadOnlySpan<byte> paletteIndices)
    {
        Ensure.NotNull(composed);
        int area = size.X * size.Y;
        Ensure.LengthEqual(pixels, area);
        Ensure.LengthEqual(paletteIndices, area);
        int tableColours = ImageColourGroup.ColourCountOf(composed.Palette);
        if (composed.Colours is null || composed.Colours.Length != tableColours)
            throw new InvalidDataException($"A {composed.Palette} sheet names {composed.Colours?.Length ?? 0} colours, not {tableColours}.");
        RgbColor[] table = [.. composed.Colours.Select(RgbColor.FromWord)];
        // The clear pixels that keep no colour of their own are told apart from every composed index as one more group, which
        // no shade writes, so where a shade recolours an opaque black they keep their black.
        int keepsNoColour = tableColours;
        var groupOf = new int[area];
        var shown = new RgbaPixel[area];
        var blending = new bool[area];
        var used = new bool[tableColours + 1];
        var firstPixel = new int[tableColours + 1];
        Array.Fill(firstPixel, int.MaxValue);
        for (int pixel = 0; pixel < area; pixel++)
        {
            int index = paletteIndices[pixel];
            if (index >= tableColours)
                throw new InvalidDataException($"A {composed.Palette} sheet's pixel {pixel} names palette index {index}, past its {tableColours} colours.");
            RgbaPixel composedPixel = pixels[pixel];
            ComposedPixelKind kind = KindOf(composed.Palette, index);
            int group = index;
            if (kind is ComposedPixelKind.Transparent or ComposedPixelKind.Clear)
            {
                if (composedPixel.A != 0)
                    throw new InvalidDataException($"A {composed.Palette} sheet's pixel {pixel} of palette index {index} is clear but composed with alpha {composedPixel.A}.");
                if (kind == ComposedPixelKind.Transparent)
                    group = keepsNoColour;
                else
                    shown[pixel] = table[index].ToRgbaPixel() with { A = 0 };
            }
            else
            {
                if (composedPixel.A == 0 || new RgbColor(composedPixel.R, composedPixel.G, composedPixel.B) != table[index])
                    throw new InvalidDataException($"A {composed.Palette} sheet's pixel {pixel} of palette index {index} is composed as {composedPixel}, not opaque {table[index]}.");
                shown[pixel] = composedPixel;
                blending[pixel] = kind == ComposedPixelKind.Blending;
            }
            groupOf[pixel] = group;
            if (!used[group])
            {
                used[group] = true;
                firstPixel[group] = pixel;
            }
        }
        if (composed.Backdrop is { } backdrop)
        {
            if (backdrop < 0 || backdrop >= tableColours)
                throw new InvalidDataException($"A {composed.Palette} sheet names backdrop palette index {backdrop}, past its {tableColours} colours.");
            used[backdrop] = true;
        }
        KeyValuePair<string, SpriteSheetShadeDocument>[] shades = [.. composed.Shades ?? []];
        int[][] written = [.. shades.Select(shade => WrittenBy(shade.Key, shade.Value, tableColours + 1))];
        // Each group in the order its first pixel shows it, the backdrop's last where no pixel does, joins the region of its
        // colour that every shade writes alike, or starts the colour's next region.
        int[] order = [.. Enumerable.Range(0, tableColours + 1).Where(group => used[group]).OrderBy(group => firstPixel[group]).ThenBy(group => group)];
        var regionsOf = new Dictionary<RgbColor, List<int>>();
        var regionOf = new int[tableColours + 1];
        var keys = new List<ColourKey>();
        var keyGroups = new List<int>();
        foreach (int group in order)
        {
            RgbColor colour = group == keepsNoColour ? RgbColor.Black : table[group];
            if (!regionsOf.TryGetValue(colour, out List<int>? regions))
                regionsOf.Add(colour, regions = []);
            int region = regions.FindIndex(other => WrittenAlike(written, other, group));
            if (region < 0)
            {
                region = regions.Count;
                regions.Add(group);
                keys.Add(new ColourKey(colour, region));
                keyGroups.Add(group);
            }
            regionOf[group] = region;
        }
        bool mapped = regionsOf.Values.Any(regions => regions.Count > 1);
        int[]? recolourRegions = mapped ? [.. groupOf.Select(group => regionOf[group])] : null;
        Dictionary<string, InstalledSheetShadeDocument>? installedShades = shades.Length == 0 ? null
            : shades.Select((shade, place) => (shade.Key, Document: Installed(shade.Value, written[place], keys, keyGroups)))
                .ToDictionary(shade => shade.Key, shade => shade.Document, StringComparer.Ordinal);
        bool drawnAsSprites = composed.Palette == SpriteImageColours.Sprite;
        bool mixed = false;
        var frames = new Dictionary<string, InstalledSheetFrameDocument>(StringComparer.Ordinal);
        foreach ((string name, SpriteSheetFrameDocument frame) in composed.Frames)
        {
            bool blends = false;
            if (drawnAsSprites)
            {
                (int opaque, int blendingPixels) = BlendingOf(frame, name, size, shown, blending);
                blends = opaque > 0 && blendingPixels == opaque;
                mixed |= blendingPixels > 0 && blendingPixels < opaque;
            }
            frames.Add(name, new InstalledSheetFrameDocument
            {
                Corner = frame.Corner, Size = frame.Size, Origin = frame.Origin, Depth = frame.Depth, Blends = blends,
            });
        }
        var manifest = new InstalledSheetDocument
        {
            Version = SpriteSheetFile.Version,
            Backdrop = composed.Backdrop is { } scene ? InstalledColourKeyDocument.Of(new ColourKey(table[scene], regionOf[scene])) : null,
            Shades = installedShades,
            Frames = frames,
            Blank = composed.Blank,
            RecolourRegions = mapped,
            BlendMap = mixed,
        };
        return new InstalledSheet(manifest, new SpriteImagePixels(shown, recolourRegions, mixed ? blending : null));
    }

    /// <summary>
    /// How a pixel of a <paramref name="palette"/> sheet composed as <paramref name="index"/> shows once installed. On a sheet
    /// of four-bit or full-palette colours index zero is clear with no colour of its own; on a HUD or room effect sheet every
    /// group's first colour is clear, keeping that colour of its own (#1408). A sprite sheet's pixel of a sprite set that
    /// takes part in colour math blends.
    /// </summary>
    internal static ComposedPixelKind KindOf(SpriteImageColours palette, int index) =>
        palette == SpriteImageColours.TwoBppBackground
            ? index % TwoBppGroupColours == 0 ? ComposedPixelKind.Clear : ComposedPixelKind.Opaque
            : index == 0 ? ComposedPixelKind.Transparent
            : palette == SpriteImageColours.Sprite && index / ImageColourGroup.Colours >= SpriteColourMathDefinitions.FirstSetTakingColourMath
                ? ComposedPixelKind.Blending
                : ComposedPixelKind.Opaque;

    /// <summary>
    /// The colour word shade <paramref name="key"/> writes over each of <paramref name="groups"/> groups, or -1 where it writes
    /// none; the last group, the clear pixels that keep no colour, is never written.
    /// </summary>
    /// <exception cref="InvalidDataException">The shade's lists differ in length, or it names an index past the sheet's colours or twice.</exception>
    private static int[] WrittenBy(string key, SpriteSheetShadeDocument shade, int groups)
    {
        if (shade.Inks is null || shade.Colours is null || shade.Inks.Length != shade.Colours.Length)
            throw new InvalidDataException($"Shade {key} names {shade.Inks?.Length ?? 0} palette indices with {shade.Colours?.Length ?? 0} colours.");
        var written = new int[groups];
        Array.Fill(written, -1);
        for (int entry = 0; entry < shade.Inks.Length; entry++)
        {
            int index = shade.Inks[entry];
            if (index < 0 || index >= groups - 1)
                throw new InvalidDataException($"Shade {key} recolours palette index {index}, past its sheet's {groups - 1} colours.");
            if (written[index] >= 0)
                throw new InvalidDataException($"Shade {key} recolours palette index {index} twice.");
            written[index] = shade.Colours[entry];
        }
        return written;
    }

    /// <summary>Whether every shade writes groups <paramref name="left"/> and <paramref name="right"/> alike.</summary>
    private static bool WrittenAlike(int[][] written, int left, int right) =>
        written.All(shade => shade[left] == shade[right]);

    /// <summary>
    /// A composed shade as the installed manifest keeps it: one substitution for each colour key whose groups it writes,
    /// in the keys' order, including writes of a key's own colour, which restore it over an earlier shade.
    /// </summary>
    private static InstalledSheetShadeDocument Installed(SpriteSheetShadeDocument shade, int[] written, List<ColourKey> keys, List<int> keyGroups)
    {
        var substitutions = new List<InstalledColourSubstitutionDocument>();
        for (int key = 0; key < keys.Count; key++)
        {
            if (written[keyGroups[key]] is int word and >= 0)
                substitutions.Add(InstalledColourSubstitutionDocument.Of(new KeyedSubstitution(keys[key], RgbColor.FromWord(checked((ushort)word)))));
        }
        return new InstalledSheetShadeDocument { Substitutions = [.. substitutions], Blends = shade.ColourMath };
    }

    /// <summary>How many of <paramref name="frame"/>'s pixels are opaque and how many of those blend.</summary>
    /// <exception cref="InvalidDataException">The frame leaves the sheet.</exception>
    private static (int Opaque, int Blending) BlendingOf(SpriteSheetFrameDocument frame, string name, Extent16<ImagePixels> size,
        RgbaPixel[] shown, bool[] blending)
    {
        if (frame.Corner.X + frame.Size.X > size.X || frame.Corner.Y + frame.Size.Y > size.Y)
            throw new InvalidDataException($"Frame {name} at ({frame.Corner.X}, {frame.Corner.Y}) leaves its {size.X}x{size.Y} sheet.");
        int opaque = 0, blends = 0;
        for (int y = 0; y < frame.Size.Y; y++)
        {
            int row = (frame.Corner.Y + y) * size.X + frame.Corner.X;
            for (int x = 0; x < frame.Size.X; x++)
            {
                if (shown[row + x].A == 0)
                    continue;
                opaque++;
                if (blending[row + x])
                    blends++;
            }
        }
        return (opaque, blends);
    }
}
