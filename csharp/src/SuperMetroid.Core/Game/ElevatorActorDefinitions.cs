using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>Cartridge definitions shared by the elevator actor and its room audit.</summary>
public static class ElevatorActorDefinitions
{
    /// <summary>$A3:9612, Elevator_Func_4: Samus's center is 26 pixels above the platform.</summary>
    public const ushort SamusYOffset = 26;

    /// <summary>
    /// $A3:94E2, ElevatorInputMaskTable: newly pressed inputs that begin downward
    /// and upward elevator travel, indexed by the actor's doubled parameter one.
    /// </summary>
    private static ReadOnlySpan<ushort> DirectionInputs =>
    [
        (ushort)SnesButton.Down,
        (ushort)SnesButton.Up,
    ];

    internal static ushort RequiredDirectionInput(ushort tableByteOffset)
    {
        if ((tableByteOffset & 1) != 0 || tableByteOffset > 2)
        {
            throw new InvalidDataException(
                $"Elevator direction offset ${tableByteOffset:X4} is outside the two native input masks.");
        }

        return DirectionInputs[tableByteOffset >> 1];
    }
}
