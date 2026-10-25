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

    /// <summary>Selects only the twelve reviewed room-level source columns needed for interpolated death colors.</summary>
    internal static RgbColor InitialLevelColor(int color) => color switch
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
    internal static bool TryLevelColor(RgbColor finalColor, int frame, int color, out RgbColor value)
    {
        if (color is 0 or 7 or 11 or 15)
        {
            value = RgbColor.Black;
            return false;
        }
        value = Interpolate(InitialLevelColor(color), finalColor, frame + 1);
        return true;
    }
    /// <summary>Selects the thirteen reviewed room-background source columns; constant final-color columns are excluded.</summary>
    internal static RgbColor InitialBackgroundColor(int color) => color switch
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
    internal static RgbColor BackgroundColor(RgbColor finalColor, int frame, int color)
    {
        if (color is 0 or 12 or 13) return finalColor;
        return Interpolate(InitialBackgroundColor(color), finalColor, frame + 1);
    }

    /// <summary>Moves each RGB5 channel toward its final color over the seven death-scene frames with nearest rounding.</summary>
    private static RgbColor Interpolate(RgbColor initial, RgbColor finalColor, int step)
    {
        int intervals = SporeSpawnColorDefinitions.DeathSceneFrameCount;
        return initial.Zip(finalColor, (_, first, last) => (first * (intervals - step) + last * step + intervals / 2) / intervals);
    }
}
