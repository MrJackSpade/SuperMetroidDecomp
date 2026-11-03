using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// The colours the Ridley encounters' shades show (#1512), laid in the cartridge's colour order for the checks that
/// compare them with the cartridge: a room picture's inks are the first hundred and twenty-eight colours and a sprite
/// sheet's ink is its colour less a hundred and twenty-eight.
/// </summary>
internal static class RidleyLaidColours
{
    /// <summary>The colour a room picture's ink zero stands at.</summary>
    internal const int Room = 0;

    /// <summary>The colour a sprite sheet's ink zero stands at.</summary>
    internal const int Sprites = 128;

    /// <summary>The number of colours the cartridge holds.</summary>
    private const int ColourCount = 256;

    /// <summary>Every colour as <paramref name="word"/>, the colours under the shades a check lays.</summary>
    internal static ushort[] Filled(ushort word) => [.. Enumerable.Repeat(word, ColourCount)];

    /// <summary>Lays <paramref name="shade"/>, as the import made it among <paramref name="made"/>, over <paramref name="colours"/> from <paramref name="offset"/> on.</summary>
    /// <exception cref="InvalidOperationException">The import did not make the shade once.</exception>
    internal static ushort[] Lay(ushort[] colours, IReadOnlyList<SheetShade> made, PictureShade shade, int offset)
    {
        SheetShade found = made.Single(candidate => candidate.Shade == shade);
        for (int index = 0; index < found.Inks.Length; index++)
            colours[offset + found.Inks[index]] = found.Colours[index].ToWord();
        return colours;
    }

    /// <summary>
    /// The colours <paramref name="state"/>'s encounter shows over <paramref name="under"/>: the shades it laid over its
    /// room, from <paramref name="room"/>, then those it laid over its sprites, from <paramref name="sprites"/>.
    /// </summary>
    internal static ushort[] Of(RidleyEnemyState state, IReadOnlyList<SheetShade> sprites, IReadOnlyList<SheetShade> room, ushort under) =>
        Over(state, sprites, room, Filled(under));

    /// <summary>
    /// <paramref name="colours"/> with the shades <paramref name="state"/>'s encounter laid over its room, from
    /// <paramref name="room"/>, then over its sprites, from <paramref name="sprites"/>, laid over them.
    /// </summary>
    internal static ushort[] Over(RidleyEnemyState state, IReadOnlyList<SheetShade> sprites, IReadOnlyList<SheetShade> room, ushort[] colours)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(colours);
        foreach (PictureShade shade in state.Colours.Room.Laid)
            Lay(colours, room, shade, Room);
        foreach (PictureShade shade in state.Colours.Sprites.Laid)
            Lay(colours, sprites, shade, Sprites);
        return colours;
    }

    /// <summary>
    /// The colours Ceres Ridley's encounter shows over black when <paramref name="state"/> holds its laid shades, from the
    /// import's shades of the Ceres colours <paramref name="maps"/> installs.
    /// </summary>
    internal static RgbColor[] Ceres(RidleyEnemyState state, AreaMapPresentation maps)
    {
        ArgumentNullException.ThrowIfNull(maps);
        return [.. Of(state, RidleySheetShades.CeresSprites(maps.CeresRidleyColors),
            RidleySheetShades.CeresRoom(maps.CeresRidleyColors, maps.CeresRidleyFloorColors, new RgbColor[Sprites]), 0)
            .Select(RgbColor.FromWord)];
    }
}
