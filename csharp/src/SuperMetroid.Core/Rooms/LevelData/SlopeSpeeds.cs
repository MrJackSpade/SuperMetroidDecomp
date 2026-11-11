namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>How fast grounded horizontal movement crosses each slope shape.</summary>
public static class SlopeSpeeds
{
    /// <summary>The share of a grounded horizontal move that crosses slope <paramref name="shape"/>, in 256ths of the move.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> is not one of the 32 slope shapes.</exception>
    public static int HorizontalMultiplier(int shape) =>
        (uint)shape < SlopeSpeedDefinitions.HorizontalMultipliers.Count
            ? SlopeSpeedDefinitions.HorizontalMultipliers[shape]
            : throw new ArgumentOutOfRangeException(nameof(shape));
}
