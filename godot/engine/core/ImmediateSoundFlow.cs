using Engine.World;

namespace Engine.Core;

/// <summary>
/// The engine's plain way with sound: every sound asked for is let in and starts on the update it was asked for, on
/// the channel its asker named, and nothing waits. Music asked for with a wait plays when the wait is over, each
/// piece after the one asked for before it.
/// </summary>
public sealed class ImmediateSoundFlow : ISoundFlow
{
    private readonly List<AcceptedSound> _started = [];
    private readonly Queue<(string Music, int Wait)> _music = new();
    private int _musicWait;
    private bool _silence;

    /// <inheritdoc/>
    public string Music { get; private set; } = "";

    /// <inheritdoc/>
    public bool HasWaitingSounds => false;

    /// <inheritdoc/>
    public bool HasWaitingMusic => _music.Count > 0;

    /// <inheritdoc/>
    public SoundAdmission Ask(string sound, string channel, SoundPriority priority)
    {
        _started.Add(new AcceptedSound(sound, channel));
        return new SoundAdmission(true, 0);
    }

    /// <summary>Everything falls silent at once; what was asked for earlier in the update is not heard.</summary>
    public void AskSilence(SoundPriority priority)
    {
        _started.Clear();
        _silence = true;
    }

    /// <inheritdoc/>
    public void AskMusic(string music, int wait)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(wait);
        if (wait == 0 && _music.Count == 0)
        {
            Music = music;
            return;
        }

        if (_music.Count == 0)
            _musicWait = wait;
        _music.Enqueue((music, wait));
    }

    /// <summary>The room's music is asked for at once, unless its version keeps whatever is playing.</summary>
    public void EnterRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (room.Version is not { KeepsMusic: true })
            AskMusic(room.Version?.Music ?? "", 0);
    }

    /// <inheritdoc/>
    public void OpenUpdate(bool musicTurn)
    {
        if (!musicTurn)
            return;
        if (_music.Count > 0 && _musicWait > 0)
            _musicWait--;
        while (_music.Count > 0 && _musicWait == 0)
        {
            Music = _music.Dequeue().Music;
            if (_music.Count > 0)
                _musicWait = _music.Peek().Wait;
        }
    }

    /// <inheritdoc/>
    public void CloseUpdate(int soundTurns)
    {
    }

    /// <inheritdoc/>
    public IReadOnlyList<AcceptedSound> TakeStarted()
    {
        var taken = _started.ToList();
        _started.Clear();
        return taken;
    }

    /// <inheritdoc/>
    public bool TakeSilence()
    {
        bool asked = _silence;
        _silence = false;
        return asked;
    }
}
