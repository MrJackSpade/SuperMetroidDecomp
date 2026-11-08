using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// The colours the Ridley encounters show (#1512): the import's shades of their installed colours, laid in the
/// cartridge's colour order as an encounter's AI laid them, for the checks that compare them with the cartridge.
/// </summary>
internal static class RidleyLaidColours
{
    /// <summary>
    /// The colours <paramref name="state"/>'s encounter shows over <paramref name="under"/>: the shades it laid over its
    /// room, from <paramref name="room"/>, then those it laid over its sprites, from <paramref name="sprites"/>.
    /// </summary>
    internal static ushort[] Of(RidleyEnemyState state, IReadOnlyList<SheetShade> sprites, IReadOnlyList<SheetShade> room, ushort under) =>
        Over(state, sprites, room, BossShadeColours.Filled(under));

    /// <summary>
    /// <paramref name="colours"/> with the shades <paramref name="state"/>'s encounter laid over its room, from
    /// <paramref name="room"/>, then over its sprites, from <paramref name="sprites"/>, laid over them.
    /// </summary>
    internal static ushort[] Over(RidleyEnemyState state, IReadOnlyList<SheetShade> sprites, IReadOnlyList<SheetShade> room, ushort[] colours)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(colours);
        foreach (PictureShade shade in state.Colours.Room.Laid)
            BossShadeColours.Lay(colours, room, shade, BossShadeColours.Room);
        foreach (PictureShade shade in state.Colours.Sprites.Laid)
            BossShadeColours.Lay(colours, sprites, shade, BossShadeColours.Sprites);
        return colours;
    }

    /// <summary>
    /// The colours Ceres Ridley's encounter shows over black when <paramref name="state"/> holds its laid shades, from the
    /// import's shades of the Ceres colours <paramref name="maps"/> installs.
    /// </summary>
    internal static RgbColor[] Ceres(RidleyEnemyState state, AreaMapPresentation maps)
    {
        ArgumentNullException.ThrowIfNull(maps);
        return [.. Of(state, RidleySheetShades.CeresSprites(ImportAreaMapPresentation.CeresRidleyColorsOf(maps)),
            RidleySheetShades.CeresRoom(ImportAreaMapPresentation.CeresRidleyColorsOf(maps), ImportAreaMapPresentation.CeresRidleyFloorColorsOf(maps), new RgbColor[BossShadeColours.Sprites]), 0)
            .Select(RgbColor.FromWord)];
    }
}
