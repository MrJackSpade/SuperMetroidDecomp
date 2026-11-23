namespace Engine.Core;

/// <summary>
/// Something of a particular game's that lasts the whole session and runs beside play: a countdown, say. The
/// <see cref="GameDefinition"/> makes each one a session has; anything in the game asks the session for it by type.
/// </summary>
public interface ISessionService
{
    /// <summary>Called as the session's game starts afresh, as after a death or on loading a save.</summary>
    void BeginAgain();

    /// <summary>Plays one update of it. Called once for each update of play, after the room has been stepped.</summary>
    void Step(GameSession session);

    /// <summary>
    /// The mode it wants in control in place of play, asked after each update of play; null when it wants nothing, as
    /// nearly always.
    /// </summary>
    GameMode? TakesOver(PlayingMode playing);
}
