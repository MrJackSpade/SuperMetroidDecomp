namespace Game.Compatibility;

/// <summary>
/// How long the console took over the room a door leads to, which is the timing of the hardware it ran on. The
/// cartridge loads the next room while the door's scroll goes on by itself; its main loop turns again once the load
/// is done, and the room's enemies are set up some updates of the scroll after that, when their own loading is
/// done. The game loads a room at once (<see cref="RoomLoadedAtOnce"/>). A host that knows how long a real console
/// took, as one playing a recording of the cartridge does, gives the door's passage its own, through the game's
/// definition (<see cref="SuperMetroid.RoomLoadPace"/>).
/// </summary>
public interface IRoomLoadPace
{
    /// <summary>How many of the scroll's updates pass, after the one the room is loaded on, before the main loop turns again.</summary>
    int UpdatesBeforeTurn { get; }

    /// <summary>How many of the scroll's updates pass, after the one the main loop turns on, before the room's enemies are set up.</summary>
    int UpdatesBeforeEnemies { get; }
}

/// <summary>The game's own pace: a room is loaded at once, its main loop's turn and its enemies with it.</summary>
public sealed class RoomLoadedAtOnce : IRoomLoadPace
{
    /// <summary>The one there is.</summary>
    public static RoomLoadedAtOnce Instance { get; } = new();

    private RoomLoadedAtOnce()
    {
    }

    /// <inheritdoc/>
    public int UpdatesBeforeTurn => 0;

    /// <inheritdoc/>
    public int UpdatesBeforeEnemies => 0;
}
