namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The flames that circle Phantoon's room before he appears (#1933): which comes next and whether they orbit.</summary>
public sealed class PhantoonStartingFlamesState
{
    /// <summary>The next starting flame to spawn among the intro's starts (<see cref="PhantoonFlameSpawns.StartingFlameStarts"/>).</summary>
    public ListStep<TrigonometryAngle> NextFlame { get; internal set; } = First;

    /// <summary>The intro's first starting flame, which every intro spawns first.</summary>
    internal static ListStep<TrigonometryAngle> First => ListStep<TrigonometryAngle>.FirstOf(PhantoonFlameSpawns.StartingFlameStarts);

    /// <summary>Whether the starting flames are orbiting.</summary>
    public bool Orbiting { get; internal set; }
}
