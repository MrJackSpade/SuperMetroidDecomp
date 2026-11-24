namespace Engine.Core;

/// <summary>
/// How the sounds and music that gameplay asks for come to be heard: whether each is let in, how long it waits, and
/// on which update it starts. <see cref="SoundRequests"/> asks it; the audio director plays what it starts. The
/// engine's own, <see cref="ImmediateSoundFlow"/>, starts everything at once. A game whose sounds wait their turn
/// supplies its own through <see cref="GameDefinition.CreateSoundFlow"/>. Whatever it keeps is the game's state,
/// moved on once an update and never by a sound device.
/// </summary>
public interface ISoundFlow
{
    /// <summary>The music playing, by name, as gameplay sees it; empty for silence.</summary>
    string Music { get; }

    /// <summary>Whether a sound that was asked for is still waiting to be handed on to be heard.</summary>
    bool HasWaitingSounds { get; }

    /// <summary>Whether music that was asked for is still waiting to play.</summary>
    bool HasWaitingMusic { get; }

    /// <summary>Asks for a sound, as it is asked for in the update, and says what became of it.</summary>
    /// <param name="sound">The sound, by name.</param>
    /// <param name="channel">The channel the asker would have it sound on.</param>
    /// <param name="priority">How readily it gives way.</param>
    SoundAdmission Ask(string sound, string channel, SoundPriority priority);

    /// <summary>Asks for every sound to stop, with the priority of a sound asked for.</summary>
    void AskSilence(SoundPriority priority);

    /// <summary>Asks for the music to become <paramref name="music"/>, empty for silence, after a wait.</summary>
    /// <param name="music">The music, by name.</param>
    /// <param name="wait">How many updates it waits before it plays, once its turn has come; none for the shortest wait there is.</param>
    void AskMusic(string music, int wait);

    /// <summary>Opens an update, before the mode in control plays it.</summary>
    /// <param name="musicTurn">Whether the music waiting gets its turn.</param>
    void OpenUpdate(bool musicTurn);

    /// <summary>Closes an update, after the mode in control has played it.</summary>
    /// <param name="soundTurns">How many turns the sounds waiting get.</param>
    void CloseUpdate(int soundTurns);

    /// <summary>The sounds that started since they were last taken, in order. The audio director takes them once an update.</summary>
    IReadOnlyList<AcceptedSound> TakeStarted();

    /// <summary>Whether everything sounding is to fall silent at once, asked since this was last asked; asking takes it.</summary>
    bool TakeSilence();
}
