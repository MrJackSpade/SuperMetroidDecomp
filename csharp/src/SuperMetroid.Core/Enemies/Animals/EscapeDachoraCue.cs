namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Dachora's animations: what it does, with the values it acts on.</summary>
internal abstract record EscapeDachoraCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EscapeDachoraCue()
    {
    }

    /// <summary>
    /// A low-tide lap ends (#2167): once the rising acid is above Y 206 the owner takes the Dachora on into the high-tide
    /// laps, counting on the laps it had left.
    /// </summary>
    /// <param name="Lap">The way the lap ran.</param>
    internal sealed record LowTideLapEnds(HorizontalDirection Lap) : EscapeDachoraCue;

    /// <summary>
    /// A lap ends, after a low-tide lap's tide test (#2167): once the critters have escaped the owner starts the Dachora's
    /// run for the escape.
    /// </summary>
    /// <param name="Lap">The way the lap ran.</param>
    internal sealed record LapEnds(HorizontalDirection Lap) : EscapeDachoraCue;

    /// <summary>Moves the Dachora by the cue's pixels (#2151).</summary>
    /// <param name="Offset">The cue's signed pixel offset.</param>
    internal sealed record Move(int Offset) : EscapeDachoraCue;
}
