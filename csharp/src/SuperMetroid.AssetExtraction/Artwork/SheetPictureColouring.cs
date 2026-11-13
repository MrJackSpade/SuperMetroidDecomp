using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A sprite sheet's colouring over the colour groups some of its pictures use (#1513): the palette bits its draws OR in,
/// the standard colours of those groups, and what each shade it is pre-coloured in shows of them. A sheet manifest stores
/// all 128 colours and every shade, so two sheets differ in groups a picture never shows; restricted to the groups a
/// picture uses, a colouring says exactly how that picture is drawn. What a picture shows in it (<see cref="WriteShown"/>,
/// through <see cref="SheetPicture.LookIn"/>) names its colours by the order the picture shows them, not by their indices,
/// so pictures drawn alike from different rows or palette bits look the same (#1544). Pictures held on one composed sheet
/// share its indices and palette bits, so those whose colourings agree on the groups they share can be held on one sheet.
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
    }

    /// <summary>
    /// The palette bits the sheet's draws OR into each ink's own row: where its colours came from, which the import's source
    /// record keeps, not what they show.
    /// </summary>
    internal int PaletteBits { get; }

    /// <summary>The colour groups the colouring covers.</summary>
    internal ImageColourGroups Groups { get; }

    /// <summary>The colouring sheet manifest <paramref name="document"/> gives the colour groups <paramref name="groups"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet is not a sprite sheet, or its colours or a shade are malformed.</exception>
    internal static SheetPictureColouring Of(SpriteSheetDocument document, ImageColourGroups groups)
    {
        Ensure.NotNull(document);
        if (document.Palette != SpriteImageColours.Sprite || document.BackgroundSets is not null)
            throw new InvalidDataException($"A {document.Palette} sheet's colouring is not a sprite sheet's.");
        if (document.Colours.Length != ImageColourGroup.HalfColours)
            throw new InvalidDataException($"A sprite sheet names {document.Colours.Length} standard colours, not {ImageColourGroup.HalfColours}.");
        var colours = new RgbColor[ImageColourGroup.HalfColours];
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
                if (ink is < 0 or >= ImageColourGroup.HalfColours)
                    throw new InvalidDataException($"Shade {key} recolours ink {ink}, past a sprite sheet's {ImageColourGroup.HalfColours}.");
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
    /// both name shows of a group both cover, or a shade only one names recolours a group the other covers. A shade only one
    /// names and that recolours nothing the other covers is kept as it is: a picture of the other was never drawn in it.
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
        // A shade only one names is kept as it is only while it recolours nothing the other covers: held on one sheet, the
        // other's pictures would show it over their own colours, and look like pictures that are drawn in it (#1713).
        if (RecoloursUnnamed(shades, other.shades, other.Groups) || RecoloursUnnamed(other.shades, shades, Groups))
            return null;
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

    /// <summary>
    /// Whether a shade of <paramref name="named"/> that <paramref name="others"/> does not name recolours an ink of a group
    /// <paramref name="groups"/> covers.
    /// </summary>
    private static bool RecoloursUnnamed(SortedDictionary<string, SheetPictureShade> named, SortedDictionary<string, SheetPictureShade> others,
        ImageColourGroups groups) =>
        named.Any(shade => !others.ContainsKey(shade.Key) && shade.Value.Colours.Keys.Any(ink => Covers(groups, ink)));

    /// <summary>Whether <paramref name="groups"/> covers <paramref name="ink"/>'s colour group.</summary>
    private static bool Covers(ImageColourGroups groups, int ink) => groups.Contains(ImageColourGroup.Numbered(ink / GroupColours));

    /// <summary>
    /// Writes what the colouring shows of the inks <paramref name="shown"/>, a picture's shown inks in the order it first
    /// shows them (#1544): each one's colour, then each shade that recolours one of them or blends, with its key, whether it
    /// blends, and the colour it shows each one it recolours in, named by its place in <paramref name="shown"/>. The inks'
    /// own indices and the palette bits are left out: they say where a colour came from, not what it is.
    /// </summary>
    /// <exception cref="InvalidOperationException">An ink lies in a group the colouring does not cover.</exception>
    internal void WriteShown(BinaryWriter writer, IReadOnlyList<int> shown)
    {
        Ensure.NotNull(writer);
        Ensure.NotNull(shown);
        foreach (int ink in shown)
        {
            if (!Covers(Groups, ink))
                throw new InvalidOperationException($"A picture shows ink {ink}, of a colour group its colouring does not cover.");
            WriteColour(writer, colours[ink]);
        }
        foreach ((string key, SheetPictureShade shade) in shades)
        {
            int[] recoloured = [.. Enumerable.Range(0, shown.Count).Where(order => shade.Colours.ContainsKey(shown[order]))];
            if (recoloured.Length == 0 && !shade.Blends)
                continue;
            writer.Write(key);
            writer.Write(shade.Blends);
            writer.Write(recoloured.Length);
            foreach (int order in recoloured)
            {
                writer.Write(order);
                WriteColour(writer, shade.Colours[shown[order]]);
            }
        }
    }

    /// <summary>
    /// This colouring of a picture whose shown inks are <paramref name="shown"/>, with every shade <paramref name="other"/>
    /// names as well, <paramref name="other"/> being the colouring of a picture that looks the same and whose shown inks are
    /// <paramref name="otherShown"/>, place for place (#1544). A shade only <paramref name="other"/> names is added over this
    /// picture's inks, recolouring each of its shown inks as it recolours the other picture's ink of the same place, so every
    /// shade either picture can be drawn in stays installed beside it.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The pictures' shown inks do not correspond place for place, or a shade both name shows them differently, which their
    /// one look rules out.
    /// </exception>
    internal SheetPictureColouring WithShadesOf(SheetPictureColouring other, IReadOnlyList<int> otherShown, IReadOnlyList<int> shown)
    {
        Ensure.NotNull(other);
        Ensure.NotNull(otherShown);
        Ensure.NotNull(shown);
        if (otherShown.Count != shown.Count)
            throw new InvalidOperationException($"A picture showing {shown.Count} colours looks the same as one showing {otherShown.Count}.");
        var merged = new SortedDictionary<string, SheetPictureShade>(shades, StringComparer.Ordinal);
        foreach ((string key, SheetPictureShade shade) in other.shades)
        {
            var translated = new SortedDictionary<int, RgbColor>();
            for (int order = 0; order < shown.Count; order++)
            {
                if (shade.Colours.TryGetValue(otherShown[order], out RgbColor colour))
                    translated.Add(shown[order], colour);
            }
            if (!merged.TryGetValue(key, out SheetPictureShade? kept))
            {
                merged.Add(key, new SheetPictureShade(shade.Blends, translated));
                continue;
            }
            bool alike = kept.Blends == shade.Blends && Enumerable.Range(0, shown.Count).All(order =>
                kept.Colours.TryGetValue(shown[order], out RgbColor keptColour) == translated.TryGetValue(shown[order], out RgbColor otherColour)
                && keptColour == otherColour);
            if (!alike)
                throw new InvalidOperationException($"Shade {key} shows two pictures of one look differently.");
        }
        return new SheetPictureColouring(PaletteBits, Groups, colours, merged);
    }

    /// <summary>Writes a colour's three channels.</summary>
    private static void WriteColour(BinaryWriter writer, RgbColor colour)
    {
        writer.Write(colour.Red);
        writer.Write(colour.Green);
        writer.Write(colour.Blue);
    }
}
