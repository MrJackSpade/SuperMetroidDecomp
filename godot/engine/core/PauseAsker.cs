namespace Engine.Core;

/// <summary>
/// Who asked for a pause. A game's pause may go differently for one the player asked for and one something of the
/// room did, and is told which as it is made (<see cref="GameDefinition.CreatePauseMode"/>).
/// </summary>
public enum PauseAsker
{
    /// <summary>The player, by their input.</summary>
    Player,

    /// <summary>Something of the room, as a station that shows the pause's own screen does.</summary>
    Room,
}
