namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The downward gate's wakes and its own resting return.</summary>
internal static class DownwardGatePreInstructions
{
    /// <summary><c>$84:BB52</c>: the gate wakes once triggered.</summary>
    internal static PlmPreInstruction WakeIfTriggered { get; } = Wake(DownwardGatePreInstruction.WakeIfTriggered);

    /// <summary><c>$84:BB6B</c>: the gate wakes once triggered or while Samus stands in its column.</summary>
    internal static PlmPreInstruction WakeIfTriggeredOrSamusBelow { get; } = Wake(DownwardGatePreInstruction.WakeIfTriggeredOrSamusBelow);

    /// <summary><c>$84:BB6A</c>, the return a woken gate installs: it does nothing.</summary>
    internal static PlmPreInstruction Inert { get; } = PlmPreInstruction.NoOp("downward gate rest", (ushort)DownwardGatePreInstruction.Inert);

    /// <summary>The wake object for <paramref name="routine"/>.</summary>
    private static PlmPreInstruction Wake(DownwardGatePreInstruction routine) =>
        new($"downward gate {routine}", (ushort)routine, (slot, context) => context.Plms.Gates.RunDownwardGatePreInstruction(slot, routine));
}
