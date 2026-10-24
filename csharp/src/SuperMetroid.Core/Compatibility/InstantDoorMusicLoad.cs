namespace SuperMetroid.Core.Compatibility;

/// <summary>The game's own timing: music loads instantly, so no frame is counted while it loads.</summary>
[CompatibilityService("music loads instantly, so no frame is counted while it loads")]
public sealed class InstantDoorMusicLoad : IDoorMusicLoadFrames
{
    /// <summary>The shared, stateless instance every session uses.</summary>
    public static InstantDoorMusicLoad Instance { get; } = new();

    /// <summary>Restricts the policy to its shared instance.</summary>
    private InstantDoorMusicLoad() { }

    /// <summary>Counts no frames for any music set.</summary>
    /// <param name="musicSet">The music set; every set loads instantly.</param>
    /// <returns>Zero.</returns>
    public int FramesCountedDuringMusicLoad(MusicDataSet musicSet) => 0;
}
