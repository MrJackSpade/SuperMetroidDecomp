namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The downward gate's wakes and its own resting return.</summary>
internal static class DownwardGateFrameBehaviors
{
    /// <summary>The gate wakes once triggered.</summary>
    internal static PlmFrameBehavior WakeIfTriggered { get; } = Wake(DownwardGateFrameBehavior.WakeIfTriggered);

    /// <summary>The gate wakes once triggered or while Samus stands in its column.</summary>
    internal static PlmFrameBehavior WakeIfTriggeredOrSamusBelow { get; } = Wake(DownwardGateFrameBehavior.WakeIfTriggeredOrSamusBelow);

    /// <summary>The return a woken gate installs: it does nothing.</summary>
    internal static PlmFrameBehavior Inert { get; } = PlmFrameBehavior.NoOp("downward gate rest");

    /// <summary>The wake object for <paramref name="routine"/>.</summary>
    private static PlmFrameBehavior Wake(DownwardGateFrameBehavior routine) =>
        new($"downward gate {routine}", (slot, context) => context.Plms.Gates.RunDownwardGateFrameBehavior(slot, routine));
}
