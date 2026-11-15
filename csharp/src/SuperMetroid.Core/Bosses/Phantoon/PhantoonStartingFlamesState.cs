namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The flames that circle Phantoon's room before he appears (#1933): which comes next and whether they orbit.</summary>
public sealed class PhantoonStartingFlamesState
{
    /// <summary>The next starting flame to spawn, zero through seven.</summary>
    public int NextFlame { get; internal set => field = Ensure.AtLeastZero(value); }

    /// <summary>Whether the starting flames are orbiting.</summary>
    public bool Orbiting { get; internal set; }
}
