using Engine.Content;

namespace Engine.Ui;

/// <summary>
/// What one update does on which a message shows nothing, before its window opens or after it has closed, while it
/// covers play.
/// </summary>
/// <param name="MusicTurn">Whether the music waiting gets a turn.</param>
/// <param name="SoundTurns">How many turns the sounds waiting get.</param>
/// <param name="OpensRoom">Whether the room beneath opens an update of its own: its liquid, and its sequences' and projectiles' openings.</param>
public readonly record struct MessagePauseUpdate(bool MusicTurn, int SoundTurns, bool OpensRoom)
{
    /// <summary>Reads an update from its group in a style's data file.</summary>
    public static MessagePauseUpdate Read(DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var update = new MessagePauseUpdate(data.Flag("music"), data.Int("sound_turns"), data.Flag("opens_room"));
        return update.SoundTurns >= 0 ? update : throw data.Problem("gives the sounds fewer turns than none");
    }
}
