namespace Game.Compatibility;

/// <summary>
/// How long the console took to load a set of music, which is the timing of the hardware it ran on. While the
/// cartridge sends a set to its sound program the game does not play, and its count of frames runs on: what goes by
/// that count (which side her blocks are tried from, among others) finds it further on afterwards. The game loads a
/// set at once (<see cref="MusicLoadedAtOnce"/>). A host that knows how long a real console took, as one playing a
/// recording of the cartridge does, gives the flow its own (<see cref="CartridgeSounds"/>).
/// </summary>
public interface IMusicLoadTime
{
    /// <summary>How many updates' worth of time the load of <paramref name="set"/>, made on this update, takes beyond the update itself.</summary>
    int UpdatesToLoad(string set);
}

/// <summary>The game's own load of a set of music: at once.</summary>
public sealed class MusicLoadedAtOnce : IMusicLoadTime
{
    /// <summary>The one there is.</summary>
    public static MusicLoadedAtOnce Instance { get; } = new();

    private MusicLoadedAtOnce()
    {
    }

    /// <inheritdoc/>
    public int UpdatesToLoad(string set) => 0;
}
