using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Bosses.SporeSpawn.Artwork.SporeSpawnDeathColorDefinitions;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>Fades Spore Spawn's room level and background colours from the original room's colours to the death colours.</summary>
internal static class SporeSpawnDeathFade
{
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
