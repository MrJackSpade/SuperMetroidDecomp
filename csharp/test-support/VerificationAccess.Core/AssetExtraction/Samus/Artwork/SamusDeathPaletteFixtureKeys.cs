using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>
/// Keys the cartridge-order rows checks read straight from the cartridge by what they colour (#2565), as the import does:
/// the suited rows by suit in the suits' list order, and the explosion's selectors by the picture each record shows.
/// </summary>
internal static class SamusDeathPaletteFixtureKeys
{
    /// <summary>Each suit's rows, from rows listed in the suits' order.</summary>
    internal static Dictionary<SamusSuit, RgbColor[][]> Suits(RgbColor[][][] inSuitOrder) =>
        SamusSuitListLayout.Order.Select((suit, place) => KeyValuePair.Create(suit, inSuitOrder[place])).ToDictionary();

    /// <summary>Each explosion picture's death step, from the selectors of the explosion's records in animation order.</summary>
    internal static Dictionary<SamusDeathExplosionFrame, int> ExplosionSteps(ushort[] byRecord)
    {
        var steps = new Dictionary<SamusDeathExplosionFrame, int>();
        for (int record = 0; record < SamusDeathSheetDefinitions.Explosion.Count; record++)
            steps[SamusDeathSheetDefinitions.Explosion[record]] = byRecord[record];
        return steps;
    }
}
