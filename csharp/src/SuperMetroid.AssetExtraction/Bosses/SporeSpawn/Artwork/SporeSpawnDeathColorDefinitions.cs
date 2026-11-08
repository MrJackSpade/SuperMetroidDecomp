using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;

/// <summary>Spore Spawn death fades and their explicitly unresolved original room-color inputs.</summary>
internal static class SporeSpawnDeathColorDefinitions
{

    /// <summary>
    /// Required RoomStaticPalette.nativeBytes inputs: palette7 columns1..11,14,15.
    /// These thirteen chosen words are not exempted by deriving the intervening fade.
    /// Columns0,12,13 instead remain constant at the independently supplied final color.
    /// </summary>
    internal static readonly RgbColor[] InitialRoomBackgroundColors =
    [
        new RgbColor(16, 74, 189), new RgbColor(24, 24, 140), new RgbColor(0, 16, 49), new RgbColor(0, 49, 74), new RgbColor(0, 41, 57), new RgbColor(0, 33, 41), new RgbColor(0, 16, 33),
        new RgbColor(255, 181, 41), new RgbColor(189, 115, 41), new RgbColor(115, 57, 41), new RgbColor(49, 33, 41), new RgbColor(255, 181, 41), new RgbColor(0, 0, 16),
    ];

    /// <summary>
    /// Required RoomStaticPalette.nativeBytes inputs at palette4 columns1..6,8..10,12..14.
    /// The chosen source colors remain pending independently of the calculated fade.
    /// </summary>
    internal static readonly RgbColor[] InitialRoomLevelColors =
    [
        new RgbColor(198, 198, 198), new RgbColor(198, 198, 198), new RgbColor(0, 57, 66), new RgbColor(57, 107, 57), new RgbColor(8, 74, 90), new RgbColor(0, 41, 82),
        new RgbColor(74, 173, 74), new RgbColor(74, 123, 74), new RgbColor(16, 82, 41), new RgbColor(74, 173, 74), new RgbColor(16, 33, 0), new RgbColor(140, 222, 74),
    ];
}
