using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A sprite sheet's colouring over the colour groups some of its pictures use (#1513): the palette bits its draws OR in,
/// the standard colours of those groups, and what each shade it is pre-coloured in shows of them. A sheet manifest stores
/// all 128 colours and every shade, so two sheets differ in groups a picture never shows; restricted to the groups a
/// picture uses, a colouring says exactly how that picture is drawn, so sheets that colour it alike hold the same picture,
/// and pictures whose colourings agree on the groups they share can be held on one sheet.
/// <para>
/// Every shade a sheet names is kept, even one that recolours none of the covered groups' inks: a draw names a shade the
/// sheet must hold, and a picture moved to another sheet keeps every shade it could be drawn in.
/// </para>
/// </summary>
internal sealed class SheetPictureColouring
{
    /// <summary>Colours in one colour group.</summary>
    private const int GroupColours = ImageColourGroup.Colours;

    /// <summary>Each ink's standard colour; black for an ink of a group the colouring does not cover.</summary>
    private readonly RgbColor[] colours;

    /// <summary>Each shade the sheet is pre-coloured in, by its key, over the covered groups.</summary>
    private readonly SortedDictionary<string, SheetPictureShade> shades;

    /// <summary>Keeps a colouring and derives its key.</summary>
    private SheetPictureColouring(int paletteBits, ImageColourGroups groups, RgbColor[] colours, SortedDictionary<string, SheetPictureShade> shades)
    {
        PaletteBits = paletteBits;
        Groups = groups;
        this.colours = colours;
        this.shades = shades;
        Key = KeyOf();
    }

    /// <summary>The palette bits the sheet's draws OR into each ink's own row.</summary>
    internal int PaletteBits { get; }

    /// <summary>The colour groups the colouring covers.</summary>
    internal ImageColourGroups Groups { get; }

    /// <summary>
    /// A hash of everything the colouring shows: its palette bits, each covered group's colours, and each shade that
    /// recolours a covered ink or blends, with what it shows. Colourings with one key draw a picture over the groups alike.
    /// </summary>
    internal string Key { get; }

    /// <summary>The colouring sheet manifest <paramref name="document"/> gives the colour groups <paramref name="groups"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet is not a sprite sheet, or its colours or a shade are malformed.</exception>
    internal static SheetPictureColouring Of(SpriteSheetDocument document, ImageColourGroups groups)
    {
        Ensure.NotNull(document);
        if (document.Palette != SpriteImageColours.Sprite || document.BackgroundSets is not null)
            throw new InvalidDataException($"A {document.Palette} sheet's colouring is not a sprite sheet's.");
        if (document.Colours.Length != SpriteImage.SpriteColourCount)
            throw new InvalidDataException($"A sprite sheet names {document.Colours.Length} standard colours, not {SpriteImage.SpriteColourCount}.");
        var colours = new RgbColor[SpriteImage.SpriteColourCount];
        for (int ink = 0; ink < colours.Length; ink++)
        {
            if (Covers(groups, ink))
                colours[ink] = RgbColor.FromWord(document.Colours[ink]);
        }
        var shades = new SortedDictionary<string, SheetPictureShade>(StringComparer.Ordinal);
        foreach ((string key, SpriteSheetShadeDocument shade) in document.Shades ?? [])
        {
            if (shade.Inks.Length != shade.Colours.Length)
                throw new InvalidDataException($"Shade {key} recolours {shade.Inks.Length} inks with {shade.Colours.Length} colours.");
            var shown = new SortedDictionary<int, RgbColor>();
            for (int entry = 0; entry < shade.Inks.Length; entry++)
            {
                int ink = shade.Inks[entry];
                if (ink < 0 || ink >= SpriteImage.SpriteColourCount)
                    throw new InvalidDataException($"Shade {key} recolours ink {ink}, past a sprite sheet's {SpriteImage.SpriteColourCount}.");
                if (Covers(groups, ink))
                    shown.Add(ink, RgbColor.FromWord(shade.Colours[entry]));
            }
            shades.Add(key, new SheetPictureShade(shade.ColourMath, shown));
        }
        return new SheetPictureColouring(document.PaletteBits, groups, colours, shades);
    }

    /// <summary>
    /// The colouring that colours what this one and <paramref name="other"/> cover as each does, or none when they disagree
    /// on something both cover: their palette bits, a group's colours, whether a shade both name blends, or what a shade
    /// both name shows of a group both cover. A shade only one names is kept as it is: a picture of the other was never
    /// drawn in it.
    /// </summary>
    internal SheetPictureColouring? MergedWith(SheetPictureColouring other)
    {
        Ensure.NotNull(other);
        if (other.PaletteBits != PaletteBits)
            return null;
        for (int ink = 0; ink < colours.Length; ink++)
        {
            if (Covers(Groups, ink) && Covers(other.Groups, ink) && colours[ink] != other.colours[ink])
                return null;
        }
        foreach ((string key, SheetPictureShade shade) in shades)
        {
            if (other.shades.TryGetValue(key, out SheetPictureShade? otherShade)
                && (shade.Blends != otherShade.Blends || !SameOverBoth(shade, otherShade, other.Groups) || !SameOverBoth(otherShade, shade, Groups)))
                return null;
        }
        var merged = new RgbColor[colours.Length];
        for (int ink = 0; ink < merged.Length; ink++)
            merged[ink] = Covers(Groups, ink) ? colours[ink] : other.colours[ink];
        var mergedShades = new SortedDictionary<string, SheetPictureShade>(StringComparer.Ordinal);
        foreach ((string key, SheetPictureShade shade) in shades.Concat(other.shades))
        {
            if (!mergedShades.TryGetValue(key, out SheetPictureShade? kept))
                mergedShades.Add(key, kept = new SheetPictureShade(shade.Blends, new SortedDictionary<int, RgbColor>()));
            foreach ((int ink, RgbColor colour) in shade.Colours)
                kept.Colours.TryAdd(ink, colour);
        }
        return new SheetPictureColouring(PaletteBits, Groups.Union(other.Groups), merged, mergedShades);
    }

    /// <summary>The 128 standard colours a sheet of this colouring installs, as packed colour words.</summary>
    internal ushort[] ColourWords() => [.. colours.Select(colour => colour.ToWord())];

    /// <summary>The shades a sheet of this colouring installs, by key, or none when it names no shade.</summary>
    internal Dictionary<string, SpriteSheetShadeDocument>? ShadeDocuments() =>
        shades.Count == 0 ? null
            : shades.ToDictionary(shade => shade.Key, shade => new SpriteSheetShadeDocument
            {
                Inks = [.. shade.Value.Colours.Keys],
                Colours = [.. shade.Value.Colours.Values.Select(colour => colour.ToWord())],
                ColourMath = shade.Value.Blends,
            }, StringComparer.Ordinal);

    /// <summary>
    /// Whether every ink <paramref name="shade"/> recolours in a group <paramref name="groups"/> covers is recoloured alike
    /// by <paramref name="other"/>.
    /// </summary>
    private static bool SameOverBoth(SheetPictureShade shade, SheetPictureShade other, ImageColourGroups groups) =>
        shade.Colours.Where(entry => Covers(groups, entry.Key))
            .All(entry => other.Colours.TryGetValue(entry.Key, out RgbColor colour) && colour == entry.Value);

    /// <summary>Whether <paramref name="groups"/> covers <paramref name="ink"/>'s colour group.</summary>
    private static bool Covers(ImageColourGroups groups, int ink) => groups.Contains(ImageColourGroup.Numbered(ink / GroupColours));

    /// <summary>Hashes everything the colouring shows into <see cref="Key"/>.</summary>
    private string KeyOf()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(PaletteBits);
            for (int ink = 0; ink < colours.Length; ink++)
            {
                if (!Covers(Groups, ink))
                    continue;
                writer.Write(ink);
                WriteColour(writer, colours[ink]);
            }
            foreach ((string key, SheetPictureShade shade) in shades.Where(shade => shade.Value.Shows))
            {
                writer.Write(key);
                writer.Write(shade.Blends);
                writer.Write(shade.Colours.Count);
                foreach ((int ink, RgbColor colour) in shade.Colours)
                {
                    writer.Write(ink);
                    WriteColour(writer, colour);
                }
            }
        }
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    /// <summary>Writes a colour's three channels.</summary>
    private static void WriteColour(BinaryWriter writer, RgbColor colour)
    {
        writer.Write(colour.Red);
        writer.Write(colour.Green);
        writer.Write(colour.Blue);
    }
}
