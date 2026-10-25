using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Tourian's PLM pre-instructions: the Metroids-cleared observers and the old escape-shaft wall's wait.</summary>
internal static class TourianPreInstructions
{
    /// <summary>Entries in the Metroids-cleared argument table <c>$84:DB28-$DB40</c>.</summary>
    private const int MetroidsClearedEntryCount = 13;

    /// <summary>
    /// <c>$84:B927</c>: the old escape-shaft fake wall wakes once Samus is below and right of its target. A list installs
    /// it through <c>$84:86C1</c>.
    /// </summary>
    internal static PlmPreInstruction OldTourianEscapeShaftWallWaitForSamus { get; } = new(
        "old Tourian escape-shaft wall wait",
        static (slot, context) => context.Plms.Areas().Tourian.RunOldTourianEscapeShaftWallWait(slot));

    /// <summary>
    /// The Metroids-cleared table's routines, by entry: nine deliberate returns, then four quota observers that mark their
    /// room's event once the enemy-death word reaches the required-kill byte.
    /// </summary>
    private static readonly PlmPreInstruction[] MetroidsCleared =
    [
        .. Enumerable.Range(0, MetroidsClearedEntryCount).Select(entry =>
        {
            int roomArgument = (ushort)(entry * 2);
            return MetroidsClearedEvents.ResolveEvent(roomArgument) is { } eventNumber
                ? new PlmPreInstruction($"Metroids cleared: {eventNumber}", (slot, context) =>
                    context.Plms.Areas().Tourian.RunMetroidsClearedObserver(eventNumber, context.Frame.EnemyDeaths, context.Frame.EnemyDeathQuota))
                : PlmPreInstruction.NoOp($"Metroids cleared: entry {entry} return");
        }),
    ];

    /// <summary>The routine setup <c>$84:DB1E</c> selects for <paramref name="roomArgument"/>.</summary>
    /// <exception cref="InvalidDataException">The argument is not an even offset into the table.</exception>
    internal static PlmPreInstruction MetroidsClearedFor(int roomArgument)
    {
        MetroidsClearedEvents.ResolveEvent(roomArgument);
        return MetroidsCleared[roomArgument / 2];
    }
}
