using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Tourian's PLM pre-instructions: the Metroids-cleared observers and the old escape-shaft wall's wait.</summary>
internal static class TourianFrameBehaviors
{
    /// <summary>Entries in the Metroids-cleared argument table.</summary>
    private const int MetroidsClearedEntryCount = 13;

    /// <summary>
    /// The old escape-shaft fake wall wakes once Samus is below and right of its target. A list installs
    /// it through its wake cue.
    /// </summary>
    internal static RoomObjectFrameBehavior OldTourianEscapeShaftWallWaitForSamus { get; } = new(
        "old Tourian escape-shaft wall wait",
        static (slot, context) => context.RoomObjects.Areas().Tourian.RunOldTourianEscapeShaftWallWait(slot));

    /// <summary>
    /// The Metroids-cleared table's routines, by entry: nine deliberate returns, then four quota observers that mark their
    /// room's event once the enemy-death word reaches the required-kill byte.
    /// </summary>
    private static readonly RoomObjectFrameBehavior[] MetroidsCleared =
    [
        .. Enumerable.Range(0, MetroidsClearedEntryCount).Select(entry =>
        {
            return MetroidsClearedEvents.ResolveEvent(entry) is { } eventNumber
                ? new RoomObjectFrameBehavior($"Metroids cleared: {eventNumber}", (slot, context) =>
                    context.RoomObjects.Areas().Tourian.RunMetroidsClearedObserver(eventNumber, context.Frame.EnemyDeaths, context.Frame.EnemyDeathQuota))
                : RoomObjectFrameBehavior.NoOp($"Metroids cleared: entry {entry} return");
        }),
    ];

    /// <summary>The frame behaviour the Metroids-cleared setup selects for table <paramref name="entry"/>.</summary>
    /// <exception cref="InvalidDataException">The entry is outside the table.</exception>
    internal static RoomObjectFrameBehavior MetroidsClearedFor(int entry)
    {
        MetroidsClearedEvents.ResolveEvent(entry);
        return MetroidsCleared[entry];
    }
}
