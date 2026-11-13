namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Chooses a Cacatac spike's animation program and launch speeds by its direction.</summary>
internal static class CacatacSpikes
{
    /// <summary>The named animation program of a spike launched in <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is not one of the ten selectors.</exception>
    internal static CacatacProjectileSegment Animation(CacatacSpikeDirection direction) =>
        CacatacProjectileDefinitions.Animations.TryGetValue(direction, out CacatacProjectileSegment segment)
            ? segment
            : throw new InvalidDataException($"Cacatac spike direction {direction} is outside the ten spike directions.");

    /// <summary>
    /// The launch speeds of a spike in <paramref name="direction"/>. The positive speed is the signed 8.8 magnitude and
    /// the negative speed is its negation.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is not one of the ten selectors.</exception>
    internal static CacatacSpikeSpeedPair SpeedPair(CacatacSpikeDirection direction) =>
        CacatacProjectileDefinitions.Speeds.TryGetValue(direction, out CacatacSpikeSpeedPair speeds)
            ? speeds
            : throw new InvalidDataException($"Cacatac spike direction {direction} is outside the ten spike directions.");

    /// <summary>
    /// The velocity a spike launched in <paramref name="direction"/> flies at: its launch speed toward the side or sides
    /// its direction names, and none on an axis it does not move along.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is not one of the ten selectors.</exception>
    internal static EightEightVector<RoomPixels> Velocity(CacatacSpikeDirection direction)
    {
        CacatacSpikeSpeedPair speeds = SpeedPair(direction);
        SignedEightEight still = new(0);
        return direction switch
        {
            CacatacSpikeDirection.LeftFacingUp or CacatacSpikeDirection.LeftFacingDown => new(speeds.Negative, still),
            CacatacSpikeDirection.RightFacingUp or CacatacSpikeDirection.RightFacingDown => new(speeds.Positive, still),
            CacatacSpikeDirection.Up => new(still, speeds.Negative),
            CacatacSpikeDirection.Down => new(still, speeds.Positive),
            CacatacSpikeDirection.UpLeft => new(speeds.Negative, speeds.Negative),
            CacatacSpikeDirection.UpRight => new(speeds.Positive, speeds.Negative),
            CacatacSpikeDirection.DownLeft => new(speeds.Negative, speeds.Positive),
            CacatacSpikeDirection.DownRight => new(speeds.Positive, speeds.Positive),
            _ => throw new InvalidDataException($"Undefined {nameof(CacatacSpikeDirection)} {direction}."),
        };
    }
}
