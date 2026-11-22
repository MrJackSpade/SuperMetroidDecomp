namespace SuperMetroid.Core;

/// <summary>A cue an animation raises.</summary>
/// <param name="Kind">What the cue does.</param>
public sealed record Cue(int Kind);

/// <summary>The cues the animations raise.</summary>
public static class Cues
{
    /// <summary>The cue raised to the left.</summary>
    public static Cue Left => Raised(1);

    /// <summary>A cue of <paramref name="kind"/> toward <c>direction</c>.</summary>
    /// <param name="kind">What the cue does.</param>
    private static Cue Raised(int kind) => new(kind);
}
