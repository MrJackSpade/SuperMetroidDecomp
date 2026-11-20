namespace SuperMetroid.Core.Audio;

/// <summary>A song change waiting for the music's next beat (#1478): play a track, or stop the music.</summary>
internal abstract record SongChange
{
    /// <summary>Only the nested cases derive.</summary>
    private SongChange()
    {
    }

    /// <summary>Plays a track of the loaded clip set.</summary>
    /// <param name="Track">The track.</param>
    internal sealed record Play(MusicTrack Track) : SongChange;

    /// <summary>Stops the music.</summary>
    internal sealed record Stop : SongChange;
}
