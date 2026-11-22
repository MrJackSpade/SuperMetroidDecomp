namespace SuperMetroid.Core;

/// <summary>A cue an animation raises.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Direction">Where it points, which only checks read.</param>
public sealed record Cue(int Kind, int Direction);

/// <summary>The cues the animations raise.</summary>
public static class Cues
{
    /// <summary>The cue raised to the left.</summary>
    public static Cue Left => Raised(1, -2);

    /// <summary>A cue of <paramref name="kind"/> toward <paramref name="direction"/>.</summary>
    /// <param name="kind">What the cue does.</param>
    /// <param name="direction">Where it points.</param>
    private static Cue Raised(int kind, int direction) => new(kind, direction);
}
