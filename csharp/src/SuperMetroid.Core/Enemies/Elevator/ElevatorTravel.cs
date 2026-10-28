using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Which newly pressed input starts an elevator's travel.</summary>
internal static class ElevatorTravel
{
    /// <summary>The newly pressed input that begins travel for the actor's doubled parameter one.</summary>
    /// <exception cref="InvalidDataException"><paramref name="tableByteOffset"/> selects neither native input mask.</exception>
    internal static ControllerButton RequiredDirectionInput(int tableByteOffset) =>
        ElevatorActorDefinitions.RequiredDirectionInputs.TryGetValue(tableByteOffset, out ControllerButton input)
            ? input
            : throw new InvalidDataException(
                $"Elevator direction offset ${tableByteOffset:X4} is outside the two native input masks.");
}
