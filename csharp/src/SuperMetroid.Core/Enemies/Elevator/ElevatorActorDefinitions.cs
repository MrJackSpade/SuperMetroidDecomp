using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Cartridge definitions shared by the elevator actor and its room audit.</summary>
public static class ElevatorActorDefinitions
{
    /// <summary>$A3:9612, Elevator_Func_4: Samus's center is 26 pixels above the platform.</summary>
    public const int SamusYOffset = 26;

    /// <summary>
    /// $A3:94E2, ElevatorInputMaskTable: newly pressed inputs that begin downward
    /// and upward elevator travel, indexed by the actor's doubled parameter one.
    /// </summary>
    internal static readonly Dictionary<int, ControllerButton> RequiredDirectionInputs = new()
    {
        [0] = ControllerButton.Down,
        [2] = ControllerButton.Up,
    };
}
