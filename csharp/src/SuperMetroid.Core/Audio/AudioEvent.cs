namespace SuperMetroid.Core.Audio;

/// <summary>
/// One semantic audio instruction of an update (#1304, #1410): a music data set change, a music track selection, a
/// step of the sound acceptance clock, or a sound starting. Each is its own sealed record holding only what it carries.
/// </summary>
/// <remarks>
/// The session owns the <see cref="SoundAcceptanceClock{TSound}"/>, so these events already say when each library acts: a
/// sound starts only at a clock step that checks the requests and finds a changed request, never when the game asked.
/// Backends follow the events and never re-derive the clock.
/// </remarks>
public abstract record AudioEvent
{
    /// <summary>Closes the hierarchy to the events declared here.</summary>
    private AudioEvent()
    {
    }

    /// <summary>Switches to <see cref="DataSet"/>'s clip set before the update's clock steps run; the song stops.</summary>
    /// <param name="DataSet">The music data set switched to, or null for only the shared sounds.</param>
    public sealed record ChangeMusicDataSet(MusicDataSet? DataSet) : AudioEvent
    {
        /// <inheritdoc />
        public override string ToString() => $"data set:{DataSet?.ToString() ?? "shared"}";
    }

    /// <summary>
    /// Selects <see cref="Track"/> before the update's clock steps run; the song changes on the music's next beat. A
    /// null track stops the music.
    /// </summary>
    /// <param name="Track">The track selected, or null to stop the music.</param>
    public sealed record SelectMusicTrack(MusicTrack? Track) : AudioEvent
    {
        /// <inheritdoc />
        public override string ToString() => Track is MusicTrack track ? $"music:{track.Number}" : "music:stop";
    }

    /// <summary>One step of the sound acceptance clock at its sample offset within the update.</summary>
    /// <param name="SampleOffset">Sound samples (32,040 per second) from the update's start at which the step runs.</param>
    /// <param name="Steps">Elapsed step periods the step accounts for.</param>
    /// <param name="ChecksRequests">Whether the step also checks the three sound libraries' requests.</param>
    public sealed record ClockStep(int SampleOffset, int Steps, bool ChecksRequests) : AudioEvent
    {
        /// <inheritdoc />
        public override string ToString() => $"step@{SampleOffset}x{Steps}{(ChecksRequests ? "+check" : "")}";
    }

    /// <summary>Starts <see cref="SoundEffect"/> at the request check of the clock step at <see cref="SampleOffset"/>.</summary>
    /// <param name="SoundEffect">The sound effect started.</param>
    /// <param name="SampleOffset">Sound samples from the update's start of the checking step.</param>
    public sealed record StartSoundEffect(SoundEffectId SoundEffect, int SampleOffset) : AudioEvent
    {
        /// <inheritdoc />
        public override string ToString() => $"start@{SampleOffset}:{SoundEffect}";
    }
}
