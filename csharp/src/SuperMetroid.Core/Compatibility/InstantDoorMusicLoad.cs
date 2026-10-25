namespace SuperMetroid.Core.Compatibility;

/// <summary>The game's own timing: music loads instantly, so no frame is counted while it loads.</summary>
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
