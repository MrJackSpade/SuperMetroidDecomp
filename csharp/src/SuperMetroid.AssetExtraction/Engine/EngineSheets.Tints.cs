using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>What a shade of a sheet does to the sheet's colours as a whole, and sheets cut in several shades at once.</summary>
internal sealed partial class EngineSheets
{
    /// <summary>
    /// What each colour of a sheet shows as under several of its shades laid one over another: a later shade's word
    /// on a colour stands over an earlier one's, as the old game lays a fade over the dark it fades out of.
    /// </summary>
    private static Dictionary<(int Colour, int Region), RgbaPixel> ShadesOf(string sheet, InstalledSheetDocument document, IReadOnlyList<string> shades)
    {
        var shown = new Dictionary<(int Colour, int Region), RgbaPixel>();
        foreach (string shade in shades)
        {
            foreach (((int Colour, int Region) colour, RgbaPixel pixel) in ShadeOf(sheet, document, shade))
                shown[colour] = pixel;
        }

        return shown;
    }

    /// <summary>
    /// Cuts every frame of one sheet as several of its shades laid one over another show it. The pictures are of the
    /// size, and lie where, the sheet's own do.
    /// </summary>
    /// <exception cref="InvalidDataException">The installation has no such sheet, or the sheet lacks one of the shades.</exception>
    internal EngineSheetPictures CutShades(EngineAssetTree tree, string family, string folder, string sheet, params string[] shades)
    {
        InstalledSheetDocument document = _documents.TryGetValue((family, sheet), out InstalledSheetDocument? found)
            ? found
            : throw new InvalidDataException($"The installation has no sheet {sheet} in {family}.");
        return CutFrames(tree, family, folder, [.. document.Frames.Keys.Select(name => (name, sheet))], shades);
    }

    /// <summary>Cuts named pictures of what an enemy spawns as <see cref="CutSpawned"/> does, as one of their sheet's shades shows them.</summary>
    /// <exception cref="InvalidDataException">A picture is on no sheet, or its sheet has no such shade.</exception>
    internal EngineSheetPictures CutSpawnedShade(EngineAssetTree tree, string folder, string spawner, IEnumerable<string> frames, string shade)
    {
        var placed = new List<(string Name, string Sheet)>();
        foreach (string frame in frames.Distinct(StringComparer.Ordinal))
            placed.Add((frame, SheetHolding(spawner, frame)));
        return CutFrames(tree, EnemyFamily, folder, placed, [shade]);
    }

    /// <summary>
    /// How much of the red, green and blue of a sheet's own colours one of its shades shows, taken over every colour
    /// the shade speaks of: the tint that, laid over the sheet's pictures, comes nearest the shade. A shade that only
    /// darkens or lightens is shown exactly; one that changes each colour its own way is shown in the round.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet has no such shade, or has none of one of the three colours to compare.</exception>
    internal (double Red, double Green, double Blue) ShadeTint(string family, string sheet, string shade)
    {
        InstalledSheetDocument document = _documents.TryGetValue((family, sheet), out InstalledSheetDocument? found)
            ? found
            : throw new InvalidDataException($"The installation has no sheet {sheet} in {family}.");
        long red = 0, green = 0, blue = 0, shownRed = 0, shownGreen = 0, shownBlue = 0;
        foreach (((int colour, int _), RgbaPixel shown) in ShadeOf(sheet, document, shade))
        {
            red += (colour >> 16) & 0xFF;
            green += (colour >> 8) & 0xFF;
            blue += colour & 0xFF;
            shownRed += shown.R;
            shownGreen += shown.G;
            shownBlue += shown.B;
        }

        if (red == 0 || green == 0 || blue == 0)
            throw new InvalidDataException($"The sheet {sheet} has no red, green or blue for its shade {shade} to be measured against.");
        return ((double)shownRed / red, (double)shownGreen / green, (double)shownBlue / blue);
    }
}
