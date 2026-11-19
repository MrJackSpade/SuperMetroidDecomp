namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The game state that a gameplay update moves the game to, decided where the update's out-of-health check runs (#2517).
/// Each is written by a step earlier in the update than the check, so each takes the update from Samus's death; where
/// more than one applies, the earliest listed wins.
/// </summary>
public enum GameplayUpdateTransition
{
    /// <summary>The update starts nothing: the game stays where it was.</summary>
    None,

    /// <summary>The escape countdown ran out during Samus's update: the time-up begins.</summary>
    TimeUp,

    /// <summary>The escaping gunship's top hull rose past the takeoff height during the enemies' update: the game leaves Zebes.</summary>
    LeavesZebes,

    /// <summary>The map station's completion message closed during the update: the game pauses on the map.</summary>
    MapStationPause,

    /// <summary>Samus's pause check admitted a new Start press: the game pauses.</summary>
    Pause,

    /// <summary>The Ceres elevator shaft's room main took Samus onto the elevator: the Ceres departure begins.</summary>
    CeresDeparture,

    /// <summary>Samus's movement touched a door, and no message box is open: the door transition begins.</summary>
    Door,
}
