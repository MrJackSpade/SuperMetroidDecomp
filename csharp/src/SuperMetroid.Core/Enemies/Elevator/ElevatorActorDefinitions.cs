using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Cartridge definitions shared by the elevator actor and its room audit.</summary>
public static class ElevatorActorDefinitions
{
    /// <summary>$A3:9612, Elevator_Func_4: Samus's center is 26 pixels above the platform.</summary>
    public const ushort SamusYOffset = 26;

    /// <summary>
    /// $A3:94E2, ElevatorInputMaskTable: newly pressed inputs that begin downward
    /// and upward elevator travel, indexed by the actor's doubled parameter one.
    /// </summary>
    internal static SnesButton RequiredDirectionInput(int tableByteOffset) => tableByteOffset switch
    {
        0 => SnesButton.Down,
        2 => SnesButton.Up,
        _ => throw new InvalidDataException(
            $"Elevator direction offset ${tableByteOffset:X4} is outside the two native input masks."),
    };
}
