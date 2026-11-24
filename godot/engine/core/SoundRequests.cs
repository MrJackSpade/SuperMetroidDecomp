using Engine.World;

namespace Engine.Core;

/// <summary>
/// Where gameplay asks to hear something. A sound is asked for by name, on a named channel, with a priority; music
/// by name. It plays nothing itself and decides nothing itself: the session's <see cref="ISoundFlow"/> says whether
/// each request is let in and when it starts, and the audio director plays what has started. Because the flow is
/// moved on once an update by the game, the few behaviours that wait on sound never depend on a sound device's timing.
/// </summary>
public sealed class SoundRequests
{
    /// <summary>Requests that go by the engine's plain flow, where everything starts at once.</summary>
    public SoundRequests()
        : this(new ImmediateSoundFlow())
    {
    }

    /// <summary>Requests that go by <paramref name="flow"/>.</summary>
    public SoundRequests(ISoundFlow flow)
    {
        ArgumentNullException.ThrowIfNull(flow);
        Flow = flow;
    }

    /// <summary>How what is asked for comes to be heard.</summary>
    public ISoundFlow Flow { get; }

    /// <summary>The music playing, by name, as gameplay sees it; empty for silence.</summary>
    public string Music => Flow.Music;

    /// <summary>Whether a sound that was asked for is still waiting to be handed on to be heard.</summary>
    public bool HasQueuedSounds => Flow.HasWaitingSounds;

    /// <summary>Whether music that was asked for is still waiting to play.</summary>
    public bool HasQueuedMusic => Flow.HasWaitingMusic;

    /// <summary>Asks for a sound, and says what became of the request.</summary>
    /// <param name="sound">The sound, by name.</param>
    /// <param name="channel">The channel it sounds on.</param>
    /// <param name="priority">How readily it gives way when sounds are waiting.</param>
    public SoundAdmission Request(string sound, string channel = "", SoundPriority priority = SoundPriority.Normal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sound);
        return Flow.Ask(sound, channel, priority);
    }

    /// <summary>
    /// Asks for every sound to stop, as when the game is paused, with the priority of a sound asked for. The music
    /// plays on.
    /// </summary>
    public void StopAll(SoundPriority priority = SoundPriority.Normal) => Flow.AskSilence(priority);

    /// <summary>Asks for the music to be <paramref name="music"/>, by name; empty for silence.</summary>
    /// <param name="music">The music.</param>
    /// <param name="wait">How many updates it waits before it plays, once its turn has come; none for the shortest wait there is.</param>
    public void RequestMusic(string music, int wait = 0)
    {
        ArgumentNullException.ThrowIfNull(music);
        Flow.AskMusic(music, wait);
    }

    /// <summary>Asks for the music of a room being set up to be played. The room asks, once its version is chosen.</summary>
    public void EnterRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        Flow.EnterRoom(room);
    }

    /// <summary>Whether everything sounding is to fall silent at once; asking takes the request. The audio director asks.</summary>
    public bool TakeStop() => Flow.TakeSilence();

    /// <summary>The sounds that started since they were last taken. The audio director takes them once per update.</summary>
    public IReadOnlyList<AcceptedSound> TakeAccepted() => Flow.TakeStarted();

    /// <summary>Opens an update, before the mode in control plays it.</summary>
    internal void OpenUpdate(bool musicTurn) => Flow.OpenUpdate(musicTurn);

    /// <summary>Closes an update, after the mode in control has played it.</summary>
    internal void CloseUpdate(int soundTurns) => Flow.CloseUpdate(soundTurns);
}
