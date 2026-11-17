namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>The flight of a Fireflea that bobs up and down between two rows about where it started.</summary>
public sealed class BobbingFirefleaState : FirefleaFlightState
{
    /// <summary>Upper extent of the motion, the spawn Y minus the radius. Passing above it reverses the Fireflea's way after movement.</summary>
    public int MinimumY { get; internal set; }

    /// <summary>Lower extent of the motion, the spawn Y plus the radius. Reaching or passing it reverses the Fireflea's way without clamping.</summary>
    public int MaximumY { get; internal set; }

    /// <summary>The speed the Fireflea moves at toward its <see cref="Direction"/>, from its spawn settings.</summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>Which way the Fireflea moves at its speed (#2369); it reverses at each extent.</summary>
    internal VerticalDirection Direction { get; set; }

    /// <summary>The Fireflea's vertical velocity: its <see cref="Speed"/> toward its <see cref="Direction"/>.</summary>
    internal SignedSixteenSixteen Velocity => Direction == VerticalDirection.Down ? Speed : -Speed;
}
