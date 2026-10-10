using SuperMetroid.Core.Hardware;
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
    private static readonly Bgr555[] InitialRoomBackgroundColors =
    [
        Bgr555.FromWord(0x5d22), Bgr555.FromWord(0x4463), Bgr555.FromWord(0x1840), Bgr555.FromWord(0x24c0), Bgr555.FromWord(0x1ca0), Bgr555.FromWord(0x1480), Bgr555.FromWord(0x1040),
        Bgr555.FromWord(0x16df), Bgr555.FromWord(0x15d7), Bgr555.FromWord(0x14ee), Bgr555.FromWord(0x1486), Bgr555.FromWord(0x16df), Bgr555.FromWord(0x0800),
    ];

    /// <summary>
    /// Required RoomStaticPalette.nativeBytes inputs at palette4 columns1..6,8..10,12..14.
    /// The chosen source colors remain pending independently of the calculated fade.
    /// </summary>
    private static readonly Bgr555[] InitialRoomLevelColors =
    [
        Bgr555.FromWord(0x6318), Bgr555.FromWord(0x6318), Bgr555.FromWord(0x20e0), Bgr555.FromWord(0x1da7), Bgr555.FromWord(0x2d21), Bgr555.FromWord(0x28a0),
        Bgr555.FromWord(0x26a9), Bgr555.FromWord(0x25e9), Bgr555.FromWord(0x1542), Bgr555.FromWord(0x26a9), Bgr555.FromWord(0x0082), Bgr555.FromWord(0x2771),
    ];

    internal static Bgr555 InitialLevelColor(int color) => color switch
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
    internal static bool TryLevelColor(Bgr555 finalColor, int frame, int color, out Bgr555 value)
    {
        if (color is 0 or 7 or 11 or 15)
        {
            value = Bgr555.Black;
            return false;
        }
        value = Interpolate(InitialLevelColor(color), finalColor, frame + 1);
        return true;
    }
    internal static Bgr555 InitialBackgroundColor(int color) => color switch
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
    internal static Bgr555 BackgroundColor(Bgr555 finalColor, int frame, int color)
    {
        if (color is 0 or 12 or 13) return finalColor;
        return Interpolate(InitialBackgroundColor(color), finalColor, frame + 1);
    }

    private static Bgr555 Interpolate(Bgr555 initial, Bgr555 finalColor, int step)
    {
        int intervals = SporeSpawnColorRomData.DeathSceneFrameCount;
        return initial.Zip(finalColor, (_, first, last) => (first * (intervals - step) + last * step + intervals / 2) / intervals);
    }
}
