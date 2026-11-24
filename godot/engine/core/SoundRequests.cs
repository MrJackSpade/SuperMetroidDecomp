namespace Engine.Core;

/// <summary>
/// What gameplay has asked to hear and, as gameplay sees it, what is still sounding. Sounds are asked for on named
/// channels; a channel sounds one thing at a time, and a request of lower priority than what is sounding on its
/// channel is refused. It plays nothing itself: the audio director plays what it accepted. Because it is advanced
/// once per update by the game, the few behaviours that wait on a sound never depend on a sound device's timing.
/// </summary>
public sealed class SoundRequests
{
    /// <summary>What is sounding on each channel, as gameplay sees it.</summary>
    private readonly Dictionary<string, Sounding> _channels = new(StringComparer.Ordinal);

    /// <summary>The requests accepted since the audio director last took them, in the order they were accepted.</summary>
    private readonly List<AcceptedSound> _accepted = [];

    /// <summary>The music asked for, by name; empty for silence.</summary>
    public string Music { get; private set; } = "";

    /// <summary>
    /// Asks for a sound. Returns whether it was accepted: refused when its channel is sounding something of higher
    /// priority.
    /// </summary>
    /// <param name="sound">The sound, by name.</param>
    /// <param name="channel">The channel it sounds on.</param>
    /// <param name="priority">Its priority; a higher number wins.</param>
    /// <param name="updates">How many updates gameplay treats it as sounding for.</param>
    public bool Request(string sound, string channel = "", int priority = 0, int updates = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sound);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(updates);
        if (_channels.TryGetValue(channel, out Sounding current) && current.UpdatesLeft > 0 && current.Priority > priority)
            return false;
        _channels[channel] = new Sounding(sound, priority, updates);
        _accepted.Add(new AcceptedSound(sound, channel));
        return true;
    }

    /// <summary>
    /// Asks for every channel to fall silent at once, whatever it is sounding, as when the game is paused. What was
    /// asked for earlier in the update is not heard; what is asked for after is. The music plays on.
    /// </summary>
    public void StopAll()
    {
        _channels.Clear();
        _accepted.Clear();
        _stopAsked = true;
    }

    /// <summary>Whether every channel was asked to fall silent since this was last asked; asking takes the request.</summary>
    public bool TakeStop()
    {
        bool asked = _stopAsked;
        _stopAsked = false;
        return asked;
    }

    private bool _stopAsked;

    /// <summary>Asks for the music to be <paramref name="music"/>, by name; empty for silence.</summary>
    public void RequestMusic(string music) => Music = music;

    /// <summary>Whether <paramref name="sound"/> is still sounding, as gameplay sees it.</summary>
    public bool IsSounding(string sound) => _channels.Values.Any(sounding => sounding.Sound == sound && sounding.UpdatesLeft > 0);

    /// <summary>Whether anything is sounding on <paramref name="channel"/>, as gameplay sees it.</summary>
    public bool IsChannelBusy(string channel) => _channels.TryGetValue(channel, out Sounding sounding) && sounding.UpdatesLeft > 0;

    /// <summary>The requests accepted since they were last taken. The audio director takes them once per update.</summary>
    public IReadOnlyList<AcceptedSound> TakeAccepted()
    {
        var taken = _accepted.ToList();
        _accepted.Clear();
        return taken;
    }

    /// <summary>Moves every channel on by one update.</summary>
    internal void Step()
    {
        foreach (string channel in _channels.Keys.ToList())
        {
            Sounding sounding = _channels[channel];
            if (sounding.UpdatesLeft > 0)
                _channels[channel] = sounding with { UpdatesLeft = sounding.UpdatesLeft - 1 };
        }
    }

    /// <summary>What a channel is sounding.</summary>
    private readonly record struct Sounding(string Sound, int Priority, int UpdatesLeft);
}
