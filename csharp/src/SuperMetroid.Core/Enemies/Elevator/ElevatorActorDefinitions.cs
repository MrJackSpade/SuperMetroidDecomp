using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Authored definitions shared by the elevator actor and its room audit.</summary>
public static class ElevatorActorDefinitions
{
    /// <summary>Samus's center is 26 pixels above the platform.</summary>
    public const int SamusYOffset = 26;

    /// <summary>
    /// Newly pressed inputs that begin downward
    /// and upward elevator travel, indexed by the actor's doubled parameter one.
    /// </summary>
    internal static readonly Dictionary<int, ControllerButton> RequiredDirectionInputs = new()
    {
        [0] = ControllerButton.Down,
        [2] = ControllerButton.Up,
    };
}
