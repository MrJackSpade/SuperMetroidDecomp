namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Chooses a Cacatac spike's animation program and launch speeds by its direction.</summary>
internal static class CacatacSpikes
{
    /// <summary>The named animation program of a spike launched in <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is not one of the ten native selectors.</exception>
    internal static CacatacProjectileProgram Program(CacatacSpikeDirection direction) =>
        CacatacProjectileDefinitions.Programs.TryGetValue(direction, out CacatacProjectileProgram program)
            ? program
            : throw new InvalidDataException($"Cacatac spike direction {direction} is outside the ten native selectors.");

    /// <summary>
    /// The launch speeds of a spike in <paramref name="direction"/>. The positive word is the signed 8.8 magnitude and
    /// the negative word is its 16-bit two's complement.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is not one of the ten native selectors.</exception>
    internal static CacatacSpikeSpeedPair SpeedPair(CacatacSpikeDirection direction) =>
        CacatacProjectileDefinitions.Speeds.TryGetValue(direction, out CacatacSpikeSpeedPair speeds)
            ? speeds
            : throw new InvalidDataException($"Cacatac spike direction {direction} is outside the ten native selectors.");
}
