namespace Engine.Core;

/// <summary>
/// Something of a particular game's that lasts the whole session and runs beside play: a countdown, say. The
/// <see cref="GameDefinition"/> makes each one a session has; anything in the game asks the session for it by type.
/// </summary>
public interface ISessionService
{
    /// <summary>Called as the session's game starts afresh, as after a death or on loading a save.</summary>
    void BeginAgain();

    /// <summary>
    /// Plays one update of it. Called once for each update of play, by the room as play's own part of its update:
    /// once the view has followed the player, before the room's sequences play.
    /// </summary>
    void Step(GameSession session);

    /// <summary>
    /// Whether it wants a mode of its own in control in place of play. Asked on each update of play before the
    /// player's energy is settled, since a player whose energy is gone does not begin to die on an update that takes
    /// play elsewhere. Asking changes nothing.
    /// </summary>
    bool WantsToTakeOver { get; }

    /// <summary>
    /// The mode it wants in control in place of play, asked at the end of an update of play on which it said it
    /// wants to take over and play let it; null when it wants nothing after all.
    /// </summary>
    GameMode? TakesOver(PlayingMode playing);
}
