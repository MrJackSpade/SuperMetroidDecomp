using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

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
        new RgbColor(2, 9, 23), new RgbColor(3, 3, 17), new RgbColor(0, 2, 6), new RgbColor(0, 6, 9), new RgbColor(0, 5, 7), new RgbColor(0, 4, 5), new RgbColor(0, 2, 4),
        new RgbColor(31, 22, 5), new RgbColor(23, 14, 5), new RgbColor(14, 7, 5), new RgbColor(6, 4, 5), new RgbColor(31, 22, 5), new RgbColor(0, 0, 2),
    ];

    /// <summary>
    /// Required RoomStaticPalette.nativeBytes inputs at palette4 columns1..6,8..10,12..14.
    /// The chosen source colors remain pending independently of the calculated fade.
    /// </summary>
    internal static readonly RgbColor[] InitialRoomLevelColors =
    [
        new RgbColor(24, 24, 24), new RgbColor(24, 24, 24), new RgbColor(0, 7, 8), new RgbColor(7, 13, 7), new RgbColor(1, 9, 11), new RgbColor(0, 5, 10),
        new RgbColor(9, 21, 9), new RgbColor(9, 15, 9), new RgbColor(2, 10, 5), new RgbColor(9, 21, 9), new RgbColor(2, 4, 0), new RgbColor(17, 27, 9),
    ];
}
