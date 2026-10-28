namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>How fast grounded horizontal movement crosses each slope shape.</summary>
public static class SlopeSpeeds
{
    /// <summary>The horizontal distance multiplier of slope <paramref name="shape"/>, with eight fractional bits.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> is not one of the 32 slope shapes.</exception>
    public static ushort HorizontalMultiplier(int shape) =>
        (uint)shape < SlopeSpeedDefinitions.HorizontalMultipliers.Count
            ? SlopeSpeedDefinitions.HorizontalMultipliers[shape]
            : throw new ArgumentOutOfRangeException(nameof(shape));
}
