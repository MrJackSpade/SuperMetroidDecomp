namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>The flight of a Fireflea that bobs up and down between two rows about where it started.</summary>
public sealed class BobbingFirefleaState : FirefleaFlightState
{
    /// <summary>Upper extent of the motion, the spawn Y minus the radius with room-coordinate wraparound. Passing above it, by the wrapped difference, reverses speed after movement.</summary>
    public int MinimumY { get; internal set; }

    /// <summary>Lower extent of the motion, the spawn Y plus the radius with room-coordinate wraparound. Reaching or passing it, by the wrapped difference, reverses speed without clamping.</summary>
    public int MaximumY { get; internal set; }

    /// <summary>The speed the Fireflea moves at, from its spawn settings, toward increasing coordinates.</summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>Which way the Fireflea moves at its speed; it reverses at each extent.</summary>
    internal EnemySpeedSign SpeedSign { get; set; }

    /// <summary>The Fireflea's speed in the direction <see cref="SpeedSign"/> names: the speed itself or its negation.</summary>
    internal SignedSixteenSixteen Velocity => SpeedSign switch
    {
        EnemySpeedSign.Positive => Speed,
        EnemySpeedSign.Negative => -Speed,
        _ => throw new InvalidOperationException($"Undefined enemy speed sign {SpeedSign}."),
    };
}
