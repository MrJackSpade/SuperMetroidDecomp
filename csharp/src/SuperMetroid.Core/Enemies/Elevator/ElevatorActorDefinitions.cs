using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Authored definitions shared by the elevator actor and its room audit.</summary>
public static class ElevatorActorDefinitions
{
    /// <summary>Samus's center is 26 pixels above the platform.</summary>
    public const int SamusYOffset = 26;

    /// <summary>The newly pressed input that begins each direction's travel.</summary>
    internal static readonly Dictionary<ElevatorTravelDirection, ControllerButton> RequiredDirectionInputs = new()
    {
        [ElevatorTravelDirection.Down] = ControllerButton.Down,
        [ElevatorTravelDirection.Up] = ControllerButton.Up,
    };
}
