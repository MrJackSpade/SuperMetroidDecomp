using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>Sheets cut in several shades at once.</summary>
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
}
