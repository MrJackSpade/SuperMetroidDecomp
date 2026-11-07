using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The coloured door caps' shot wakes.</summary>
internal static class ColoredDoorFrameBehaviors
{
    /// <summary>A yellow cap wakes on a power bomb.</summary>
    internal static RoomObjectFrameBehavior WakeOnPowerBomb { get; } = Wake(ColoredDoorFrameBehavior.WakeOnPowerBomb);

    /// <summary>A red cap wakes on a missile or Super Missile.</summary>
    internal static RoomObjectFrameBehavior WakeOnMissile { get; } = Wake(ColoredDoorFrameBehavior.WakeOnMissile);

    /// <summary>A green cap wakes on a Super Missile.</summary>
    internal static RoomObjectFrameBehavior WakeOnSuperMissile { get; } = Wake(ColoredDoorFrameBehavior.WakeOnSuperMissile);

    /// <summary>The behavior of <paramref name="routine"/>.</summary>
    internal static IRoomObjectFrameBehavior Of(ColoredDoorFrameBehavior routine) => routine switch
    {
        ColoredDoorFrameBehavior.None => RoomObjectFrameBehaviors.Inert,
        ColoredDoorFrameBehavior.WakeOnPowerBomb => WakeOnPowerBomb,
        ColoredDoorFrameBehavior.WakeOnMissile => WakeOnMissile,
        ColoredDoorFrameBehavior.WakeOnSuperMissile => WakeOnSuperMissile,
        _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorFrameBehavior)} {routine}."),
    };

    /// <summary>The wake object for <paramref name="routine"/>.</summary>
    private static RoomObjectFrameBehavior Wake(ColoredDoorFrameBehavior routine) =>
        new($"coloured door {routine}", (slot, context) => context.RoomObjects.DoorObjects.RunColoredDoorWake(slot, routine));
}
