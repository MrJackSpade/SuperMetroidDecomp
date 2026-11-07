namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The downward gate's wakes and its own resting return.</summary>
internal static class DownwardGateFrameBehaviors
{
    /// <summary>The gate wakes once triggered.</summary>
    internal static RoomObjectFrameBehavior WakeIfTriggered { get; } = Wake(DownwardGateFrameBehavior.WakeIfTriggered);

    /// <summary>The gate wakes once triggered or while Samus stands in its column.</summary>
    internal static RoomObjectFrameBehavior WakeIfTriggeredOrSamusBelow { get; } = Wake(DownwardGateFrameBehavior.WakeIfTriggeredOrSamusBelow);

    /// <summary>The return a woken gate installs: it does nothing.</summary>
    internal static RoomObjectFrameBehavior Inert { get; } = RoomObjectFrameBehavior.NoOp("downward gate rest");

    /// <summary>The wake object for <paramref name="routine"/>.</summary>
    private static RoomObjectFrameBehavior Wake(DownwardGateFrameBehavior routine) =>
        new($"downward gate {routine}", (slot, context) => context.RoomObjects.Gates.RunDownwardGateFrameBehavior(slot, routine));
}
