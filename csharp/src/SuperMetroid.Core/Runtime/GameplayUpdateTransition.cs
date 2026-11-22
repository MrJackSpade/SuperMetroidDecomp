using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The game state that a gameplay update moves the game to, decided where the update's out-of-health check runs (#2517).
/// Each is written by a step earlier in the update than the check, so each takes the update from Samus's death; where
/// more than one applies, the earliest listed wins. A door's transition carries the door it was decided with (#2554).
/// </summary>
public abstract partial record GameplayUpdateTransition
{
    /// <summary>Only the cases below are transitions.</summary>
    private GameplayUpdateTransition()
    {
    }

    /// <summary>The update starts nothing: the game stays where it was.</summary>
    public sealed partial record Stays : GameplayUpdateTransition;

    /// <summary>
    /// The escape countdown ran out during Samus's update, or, once the Zebes time bomb is set, her draw returns the game to
    /// the time-up the run-out countdown started (#2518): the time-up begins, or goes on where it stood.
    /// </summary>
    public sealed partial record TimeUp : GameplayUpdateTransition;

    /// <summary>The escaping gunship's top hull rose past the takeoff height during the enemies' update: the game leaves Zebes.</summary>
    public sealed partial record LeavesZebes : GameplayUpdateTransition;

    /// <summary>The map station's completion message closed during the update: the game pauses on the map.</summary>
    public sealed partial record MapStationPause : GameplayUpdateTransition;

    /// <summary>Samus's pause check admitted a new Start press: the game pauses.</summary>
    public sealed partial record Pause : GameplayUpdateTransition;

    /// <summary>The Ceres elevator shaft's room main took Samus onto the elevator: the Ceres departure begins.</summary>
    public sealed partial record CeresDeparture : GameplayUpdateTransition;

    /// <summary>Samus's movement touched a door, and no message box is open: the door transition begins through it.</summary>
    /// <param name="Entered">The door Samus entered, which the transition goes through.</param>
    public sealed partial record Door(DoorDefinition Entered) : GameplayUpdateTransition;
}
