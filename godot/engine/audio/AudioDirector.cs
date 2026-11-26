using Engine.Core;
using Godot;

namespace Engine.Audio;

/// <summary>
/// Plays what gameplay asked to hear. Once per update, after the game has run, it takes the sounds the session
/// accepted and plays each on its channel's player, and changes the music when the music asked for changes. It
/// decides nothing: which sound wins a channel was settled when the sound was asked for. Gameplay never waits on it.
/// </summary>
public partial class AudioDirector : Node
{
    private readonly GameSession _session;
    private readonly Dictionary<string, AudioStreamPlayer> _channels = new(StringComparer.Ordinal);
    private readonly Dictionary<string, AudioStream> _loaded = new(StringComparer.Ordinal);
    private AudioStreamPlayer? _music;
    private string _musicPlaying = "";

    /// <summary>A director playing what <paramref name="session"/> asks to hear.</summary>
    public AudioDirector(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        _session = session;
        Name = "AudioDirector";
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta) => Step();

    /// <summary>Plays what was accepted since the last update, and follows the music asked for.</summary>
    public void Step()
    {
        if (_session.Sounds.TakeStop())
        {
            foreach (AudioStreamPlayer player in _channels.Values)
                player.Stop();
        }

        foreach (AcceptedSound accepted in _session.Sounds.TakeAccepted())
        {
            AudioStreamPlayer player = ChannelPlayer(accepted.Channel);
            player.Stream = Load($"{_session.Definition.SoundFolder}/{accepted.Sound}.wav");
            player.Play();
        }

        string music = _session.Sounds.Music;
        if (music == _musicPlaying)
            return;
        _musicPlaying = music;
        _music ??= AddPlayer("Music");
        _music.Stop();
        if (music.Length == 0)
            return;
        _music.Stream = Load($"{_session.Definition.MusicFolder}/{music}.wav");
        _music.Play();
    }

    /// <summary>The player for a channel, made the first time the channel is used.</summary>
    private AudioStreamPlayer ChannelPlayer(string channel)
    {
        if (!_channels.TryGetValue(channel, out AudioStreamPlayer? player))
        {
            player = AddPlayer($"Channel{_channels.Count}");
            _channels[channel] = player;
        }

        return player;
    }

    /// <summary>Adds a player beneath the director.</summary>
    private AudioStreamPlayer AddPlayer(string name)
    {
        var player = new AudioStreamPlayer { Name = name };
        AddChild(player);
        return player;
    }

    /// <summary>The sound at a path in the assets tree, loaded once.</summary>
    private AudioStream Load(string path)
    {
        if (!_loaded.TryGetValue(path, out AudioStream? stream))
        {
            stream = _session.Content.LoadSound(path);
            _loaded[path] = stream;
        }

        return stream;
    }
}
