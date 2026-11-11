namespace SuperMetroid.Core.Compatibility;

/// <summary>The game's own timing: music loads instantly, so no frame is counted while it loads.</summary>
/// <remarks>
/// Consulted once per door transition, when the transition starts the destination's music set. Evidence: the original
/// music upload stalls the main loop, consuming no input and running no gameplay update until it finishes; the replay
/// contract treats those frames as hardware lag, which the game eliminates, and a replay supplies the frames its
/// recording counted through its own <see cref="IDoorMusicLoadFrames"/>.
/// </remarks>
[CompatibilityService("music loads instantly, so no frame is counted while it loads")]
public sealed class InstantDoorMusicLoad : IDoorMusicLoadFrames
{
    /// <summary>The shared, stateless instance every session uses.</summary>
    public static InstantDoorMusicLoad Instance { get; } = new();

    /// <summary>Restricts the policy to its shared instance.</summary>
    private InstantDoorMusicLoad() { }

    /// <summary>Counts no frames: every music data set loads instantly.</summary>
    /// <returns>Zero.</returns>
    public int FramesCountedDuringMusicLoad() => 0;
}
