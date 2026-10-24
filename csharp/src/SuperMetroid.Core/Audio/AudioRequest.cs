namespace SuperMetroid.Core.Audio;

/// <summary>
/// One thing the game's music and sound queues ask of the audio layer during an update (#1410): play or stop a music
/// track, change the music data set, or set what a sound library is asked to play. The session applies an update's
/// requests in order to its <see cref="SoundAcceptanceClock"/> and publishes them as <see cref="AudioEvent"/>s.
/// </summary>
internal abstract record AudioRequest
{
    /// <summary>Closes the hierarchy to the requests declared here.</summary>
    private AudioRequest()
    {
    }

    /// <summary>Plays <paramref name="Track"/> of the loaded music data set from the next music tick.</summary>
    /// <param name="Track">The track to play.</param>
    internal sealed record PlayMusicTrack(MusicTrack Track) : AudioRequest;

    /// <summary>Stops the music at the next music tick.</summary>
    internal sealed record StopMusic : AudioRequest;

    /// <summary>
    /// Switches the clips the music and sounds play from to <paramref name="DataSet"/>'s. The switch stops the music and
    /// drops every sound request not yet accepted (<see cref="SoundAcceptanceClock.PauseForMusicLoad"/>).
    /// </summary>
    /// <param name="DataSet">The music data set switched to, or null for only the shared sounds, as at power-on.</param>
    internal sealed record ChangeMusicDataSet(MusicDataSet? DataSet) : AudioRequest;

    /// <summary>Asks <paramref name="Sound"/>'s library to play it; the library keeps the request until it is changed.</summary>
    /// <param name="Sound">The sound requested.</param>
    internal sealed record RequestSound(SoundEffectId Sound) : AudioRequest;

    /// <summary>Withdraws whatever <paramref name="Library"/> is asked to play.</summary>
    /// <param name="Library">The library whose request is withdrawn.</param>
    internal sealed record ClearSoundRequest(SoundEffectLibrary Library) : AudioRequest;

    /// <summary>The request that asks <paramref name="library"/> for sound <paramref name="sound"/>, or withdraws its request for zero.</summary>
    /// <param name="library">The library asked.</param>
    /// <param name="sound">The library's sound number, or zero for none.</param>
    internal static AudioRequest ForSound(SoundEffectLibrary library, int sound) =>
        sound == 0 ? new ClearSoundRequest(library) : new RequestSound(new SoundEffectId(library, sound));

    /// <summary>The request that plays <paramref name="track"/>, or stops the music for none.</summary>
    /// <param name="track">The track to play, or null to stop.</param>
    internal static AudioRequest ForTrack(MusicTrack? track) =>
        track is MusicTrack playing ? new PlayMusicTrack(playing) : new StopMusic();
}
