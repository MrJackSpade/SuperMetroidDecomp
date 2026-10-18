using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The coloured door caps' shot wakes.</summary>
internal static class ColoredDoorPreInstructions
{
    /// <summary><c>$84:BD26</c>: a yellow cap wakes on a power bomb.</summary>
    internal static PlmPreInstruction WakeOnPowerBomb { get; } = Wake(ColoredDoorPreInstruction.WakeOnPowerBomb);

    /// <summary><c>$84:BD50</c>: a red cap wakes on a missile or Super Missile.</summary>
    internal static PlmPreInstruction WakeOnMissile { get; } = Wake(ColoredDoorPreInstruction.WakeOnMissile);

    /// <summary><c>$84:BD88</c>: a green cap wakes on a Super Missile.</summary>
    internal static PlmPreInstruction WakeOnSuperMissile { get; } = Wake(ColoredDoorPreInstruction.WakeOnSuperMissile);

    /// <summary>The behavior of <paramref name="routine"/>.</summary>
    internal static IPlmPreInstruction Of(ColoredDoorPreInstruction routine) => routine switch
    {
        ColoredDoorPreInstruction.None => PlmPreInstructions.Inert,
        ColoredDoorPreInstruction.WakeOnPowerBomb => WakeOnPowerBomb,
        ColoredDoorPreInstruction.WakeOnMissile => WakeOnMissile,
        ColoredDoorPreInstruction.WakeOnSuperMissile => WakeOnSuperMissile,
        _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorPreInstruction)} {routine}."),
    };

    /// <summary>The wake object for <paramref name="routine"/>.</summary>
    private static PlmPreInstruction Wake(ColoredDoorPreInstruction routine) =>
        new($"coloured door {routine}", (slot, context) => context.Plms.DoorPlms.RunColoredDoorWake(slot, routine));
}
