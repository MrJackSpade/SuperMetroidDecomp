namespace Engine.Core;

/// <summary>
/// A session service with something to do as each update of a room opens: before the player's input is read and
/// before anything in the room acts on this update. The room calls it there, whatever is stopped, in the order
/// the <see cref="GameDefinition"/> made the services.
/// </summary>
public interface IUpdateOpener : ISessionService
{
    /// <summary>Plays its part of the opening of one update of a room.</summary>
    void OpenUpdate(GameSession session);
}
