using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Spore Spawn death fades and their explicitly unresolved original room-color inputs.</summary>
internal static class SporeSpawnDeathColorDefinitions
{

    /// <summary>
    /// Required RoomStaticPalette.nativeBytes inputs: palette7 columns1..11,14,15.
    /// These thirteen chosen words are not exempted by deriving the intervening fade.
    /// Columns0,12,13 instead remain constant at the independently supplied final color.
    /// </summary>
    private static readonly ushort[] InitialRoomBackgroundColors =
    [
        0x5d22, 0x4463, 0x1840, 0x24c0, 0x1ca0, 0x1480, 0x1040,
        0x16df, 0x15d7, 0x14ee, 0x1486, 0x16df, 0x0800,
    ];

    /// <summary>
    /// Required RoomStaticPalette.nativeBytes inputs at palette4 columns1..6,8..10,12..14.
    /// The chosen source colors remain pending independently of the calculated fade.
    /// </summary>
    private static readonly ushort[] InitialRoomLevelColors =
    [
        0x6318, 0x6318, 0x20e0, 0x1da7, 0x2d21, 0x28a0,
        0x26a9, 0x25e9, 0x1542, 0x26a9, 0x0082, 0x2771,
    ];

    internal static ushort InitialLevelColor(int color) => color switch
    {
        >= 1 and <= 6 => InitialRoomLevelColors[color - 1],
        >= 8 and <= 10 => InitialRoomLevelColors[color - 2],
        >= 12 and <= 14 => InitialRoomLevelColors[color - 3],
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };

    /// <summary>
    /// $A5:E4F9..E5D8, Palette_SporeSpawn_DeathSequence_Level_0..6: twelve color
    /// columns follow the original-room seven-step fade. All seven words in each
    /// of columns0,7,11,15 remain required unresolved trajectories, without exemption.
    /// </summary>
    internal static bool TryLevelColor(ushort finalColor, int frame, int color, out ushort value)
    {
        if (color is 0 or 7 or 11 or 15)
        {
            value = 0;
            return false;
        }
        value = Interpolate(InitialLevelColor(color), finalColor, frame + 1);
        return true;
    }
    internal static ushort InitialBackgroundColor(int color) => color switch
    {
        >= 1 and <= 11 => InitialRoomBackgroundColors[color - 1],
        14 or 15 => InitialRoomBackgroundColors[color - 3],
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };

    /// <summary>
    /// $A5:E5D9..E6B8, Palette_SporeSpawn_DeathSequence_Background_0..6:
    /// seven equally spaced, rounded RGB5 steps from the original room color to the
    /// final death color. The first stored row is step1, not the initial endpoint.
    /// </summary>
    internal static ushort BackgroundColor(ushort finalColor, int frame, int color)
    {
        if (color is 0 or 12 or 13) return finalColor;
        return Interpolate(InitialBackgroundColor(color), finalColor, frame + 1);
    }

    private static ushort Interpolate(ushort initial, ushort finalColor, int step)
    {
        int intervals = SporeSpawnColorRomData.DeathSceneFrameCount;
        int result = 0;
        for (int shift = 0; shift <= 10; shift += 5)
        {
            int first = (initial >> shift) & 31;
            int last = (finalColor >> shift) & 31;
            int channel = (first * (intervals - step) + last * step + intervals / 2) / intervals;
            result |= channel << shift;
        }
        return (ushort)result;
    }
}
