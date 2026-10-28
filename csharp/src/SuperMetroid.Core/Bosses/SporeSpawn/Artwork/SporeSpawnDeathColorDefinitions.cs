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
        RgbColor.FromWord(0x5d22), RgbColor.FromWord(0x4463), RgbColor.FromWord(0x1840), RgbColor.FromWord(0x24c0), RgbColor.FromWord(0x1ca0), RgbColor.FromWord(0x1480), RgbColor.FromWord(0x1040),
        RgbColor.FromWord(0x16df), RgbColor.FromWord(0x15d7), RgbColor.FromWord(0x14ee), RgbColor.FromWord(0x1486), RgbColor.FromWord(0x16df), RgbColor.FromWord(0x0800),
    ];

    /// <summary>
    /// Required RoomStaticPalette.nativeBytes inputs at palette4 columns1..6,8..10,12..14.
    /// The chosen source colors remain pending independently of the calculated fade.
    /// </summary>
    internal static readonly RgbColor[] InitialRoomLevelColors =
    [
        RgbColor.FromWord(0x6318), RgbColor.FromWord(0x6318), RgbColor.FromWord(0x20e0), RgbColor.FromWord(0x1da7), RgbColor.FromWord(0x2d21), RgbColor.FromWord(0x28a0),
        RgbColor.FromWord(0x26a9), RgbColor.FromWord(0x25e9), RgbColor.FromWord(0x1542), RgbColor.FromWord(0x26a9), RgbColor.FromWord(0x0082), RgbColor.FromWord(0x2771),
    ];
}
