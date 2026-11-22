namespace SuperMetroid.Core.Audio;

/// <summary>
/// One thing the game's music and sound queues ask of the audio layer during an update (#1410): play or stop a music
/// track, change the music data set, or set what a sound library is asked to play. The session applies an update's
/// requests in order to its <see cref="SoundAcceptanceClock{TLibrary, TSound}"/> and publishes them as <see cref="AudioEvent"/>s.
/// </summary>
internal abstract partial record AudioRequest
{
    /// <summary>Closes the hierarchy to the requests declared here.</summary>
    private AudioRequest()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this request, through the request it is (#2553).</summary>
    /// <typeparam name="TContext">What the request's reaction reads besides the request.</typeparam>
    /// <param name="reactions">What the session does on each request.</param>
    /// <param name="context">What the request's reaction reads besides the request, handed to it so the reactions are built once.</param>
    internal abstract void React<TContext>(AudioRequestReactions<TContext> reactions, TContext context);

    /// <summary>Plays <paramref name="Track"/> of the loaded music data set from the music's next beat.</summary>
    /// <param name="Track">The track to play.</param>
    internal sealed partial record PlayMusicTrack(MusicTrack Track) : AudioRequest
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AudioRequestReactions<TContext> reactions, TContext context) =>
            Ensure.NotNull(reactions).PlayMusicTrack(this, context);
    }

    /// <summary>Stops the music on its next beat.</summary>
    internal sealed partial record StopMusic : AudioRequest
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AudioRequestReactions<TContext> reactions, TContext context) =>
            Ensure.NotNull(reactions).StopMusic(this, context);
    }

    /// <summary>
    /// Switches the clips the music and sounds play from to <paramref name="DataSet"/>'s. The switch stops the music and
    /// drops every sound request not yet accepted (<see cref="SoundAcceptanceClock{TLibrary, TSound}.PauseForMusicLoad"/>).
    /// </summary>
    /// <param name="DataSet">The music data set switched to.</param>
    internal sealed partial record ChangeMusicDataSet(MusicDataSet DataSet) : AudioRequest
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AudioRequestReactions<TContext> reactions, TContext context) =>
            Ensure.NotNull(reactions).ChangeMusicDataSet(this, context);
    }

    /// <summary>
    /// Installs only the shared sounds, as at power-on: the music stops and the waiting song change is dropped. The songs
    /// asked for next are of the set loaded next.
    /// </summary>
    internal sealed partial record InstallSharedSounds : AudioRequest
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AudioRequestReactions<TContext> reactions, TContext context) =>
            Ensure.NotNull(reactions).InstallSharedSounds(this, context);
    }

    /// <summary>Asks <paramref name="Sound"/>'s library to play it; the library keeps the request until it is changed.</summary>
    /// <param name="Sound">The sound requested.</param>
    internal sealed partial record RequestSound(SoundEffectId Sound) : AudioRequest
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AudioRequestReactions<TContext> reactions, TContext context) =>
            Ensure.NotNull(reactions).RequestSound(this, context);
    }

    /// <summary>Withdraws whatever <paramref name="Library"/> is asked to play.</summary>
    /// <param name="Library">The library whose request is withdrawn.</param>
    internal sealed partial record ClearSoundRequest(SoundEffectLibrary Library) : AudioRequest
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AudioRequestReactions<TContext> reactions, TContext context) =>
            Ensure.NotNull(reactions).ClearSoundRequest(this, context);
    }

    /// <summary>The request that asks <paramref name="library"/> for <paramref name="sound"/>, or withdraws its request for none.</summary>
    /// <param name="library">The library asked.</param>
    /// <param name="sound">One of the library's sounds, or none.</param>
    /// <exception cref="ArgumentException"><paramref name="sound"/> belongs to another library.</exception>
    internal static AudioRequest ForSound(SoundEffectLibrary library, SoundEffectId? sound)
    {
        if (sound is not SoundEffectId requested)
            return new ClearSoundRequest(library);
        if (requested.Library != library)
            throw new ArgumentException($"{requested} does not belong to {library}.", nameof(sound));
        return new RequestSound(requested);
    }

    /// <summary>The request that plays <paramref name="track"/>, or stops the music for none.</summary>
    /// <param name="track">The track to play, or null to stop.</param>
    internal static AudioRequest ForTrack(MusicTrack? track) =>
        track is MusicTrack playing ? new PlayMusicTrack(playing) : new StopMusic();
}
