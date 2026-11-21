namespace SuperMetroid.Core.Audio;

/// <summary>
/// One semantic audio event of an update (#1304, #1410): a music data set change, a music track selection, a
/// step of the sound acceptance clock, or a sound starting. Each is its own sealed record holding only what it carries.
/// </summary>
/// <remarks>
/// The session owns the <see cref="SoundAcceptanceClock{TLibrary, TSound}"/>, so these events already say when each library acts: a
/// sound starts only at a clock step that checks the requests and finds a changed request, never when the game asked.
/// Backends follow the events and never re-derive the clock.
/// </remarks>
public abstract partial record AudioEvent
{
    /// <summary>Closes the hierarchy to the events declared here.</summary>
    private AudioEvent()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this audio event, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(AudioEventReactions<TContext> reactions, TContext context);


    /// <summary>Switches to <see cref="DataSet"/>'s clip set before the update's clock steps run; the song stops.</summary>
    /// <param name="DataSet">The music data set switched to, or null for only the shared sounds.</param>
    public sealed partial record ChangeMusicDataSet(MusicDataSet? DataSet) : AudioEvent
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AudioEventReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ChangeMusicDataSet(this, context);

        /// <inheritdoc />
        public override string ToString() => $"data set:{DataSet?.ToString() ?? "shared"}";
    }

    /// <summary>
    /// Selects <see cref="Track"/> before the update's clock steps run; the song changes on the music's next beat.
    /// </summary>
    /// <param name="Track">The track selected.</param>
    public sealed partial record SelectMusicTrack(MusicTrack Track) : AudioEvent
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AudioEventReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SelectMusicTrack(this, context);

        /// <inheritdoc />
        public override string ToString() => $"music:{Track}";
    }

    /// <summary>Stops the music before the update's clock steps run, on the music's next beat.</summary>
    public sealed partial record StopMusic : AudioEvent
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AudioEventReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).StopMusic(this, context);

        /// <inheritdoc />
        public override string ToString() => "music:stop";
    }

    /// <summary>One step of the sound acceptance clock at its sample offset within the update.</summary>
    /// <param name="SampleOffset">Sound samples (<see cref="AudioSession.SampleRate"/> per second) from the update's start at which the step runs.</param>
    /// <param name="Steps">Elapsed step periods the step accounts for.</param>
    /// <param name="ChecksRequests">Whether the step also checks the three sound libraries' requests.</param>
    public sealed partial record ClockStep(int SampleOffset, int Steps, bool ChecksRequests) : AudioEvent
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AudioEventReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ClockStep(this, context);

        /// <inheritdoc />
        public override string ToString() => $"step@{SampleOffset}x{Steps}{(ChecksRequests ? "+check" : "")}";
    }

    /// <summary>Starts <see cref="SoundEffect"/> at the request check of the clock step at <see cref="SampleOffset"/>.</summary>
    /// <param name="SoundEffect">The sound effect started.</param>
    /// <param name="SampleOffset">Sound samples from the update's start of the checking step.</param>
    public sealed partial record StartSoundEffect(SoundEffectId SoundEffect, int SampleOffset) : AudioEvent
    {
        /// <inheritdoc/>
        internal override void React<TContext>(AudioEventReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).StartSoundEffect(this, context);

        /// <inheritdoc />
        public override string ToString() => $"start@{SampleOffset}:{SoundEffect}";
    }
}

/// <summary>What the owner of a <see cref="AudioEvent"/> does on each of its cases (#2553).</summary>
/// <param name="ChangeMusicDataSet">Reacts to a <see cref="AudioEvent.ChangeMusicDataSet"/>.</param>
/// <param name="SelectMusicTrack">Reacts to a <see cref="AudioEvent.SelectMusicTrack"/>.</param>
/// <param name="StopMusic">Reacts to a <see cref="AudioEvent.StopMusic"/>.</param>
/// <param name="ClockStep">Reacts to a <see cref="AudioEvent.ClockStep"/>.</param>
/// <param name="StartSoundEffect">Reacts to a <see cref="AudioEvent.StartSoundEffect"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record AudioEventReactions<TContext>(Action<AudioEvent.ChangeMusicDataSet, TContext> ChangeMusicDataSet, Action<AudioEvent.SelectMusicTrack, TContext> SelectMusicTrack, Action<AudioEvent.StopMusic, TContext> StopMusic, Action<AudioEvent.ClockStep, TContext> ClockStep, Action<AudioEvent.StartSoundEffect, TContext> StartSoundEffect);
