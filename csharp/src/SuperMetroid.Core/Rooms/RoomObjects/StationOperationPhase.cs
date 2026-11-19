namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Where a map, energy or missile station's access arm is in its use (#1554): idle, in use with the access Samus used
/// (#2545), or finished with leaving the map screen still to release her.
/// </summary>
internal abstract record StationOperationPhase
{
    /// <summary>Only this type's own phases exist.</summary>
    private StationOperationPhase()
    {
    }

    /// <summary>No access sequence is in progress.</summary>
    internal static StationOperationPhase Idle { get; } = new IdlePhase();

    /// <summary>The access has finished; leaving the map screen still owns Samus's release.</summary>
    internal static StationOperationPhase AwaitingMapUnpause { get; } = new AwaitingMapUnpausePhase();

    /// <summary>No access sequence is in progress.</summary>
    private sealed record IdlePhase : StationOperationPhase;

    /// <summary>The access has finished; leaving the map screen still owns Samus's release.</summary>
    private sealed record AwaitingMapUnpausePhase : StationOperationPhase;

    /// <summary>
    /// The arm is in use at <paramref name="Use"/>, from the access's first draw to its end
    /// (<see cref="StationAccessTimingDefinitions.Use"/>).
    /// </summary>
    /// <param name="Use">The access Samus used, where the arm is drawn.</param>
    internal sealed record InUse(StationAccessUse Use) : StationOperationPhase;

    /// <summary>
    /// Samus's touch was accepted at <paramref name="Use"/> (#2545, #2562): the station's next update starts the arm there,
    /// as the game's station starts it an update after the touch; a later accepted touch before then replaces it.
    /// </summary>
    /// <param name="Use">The access Samus's touch was accepted at.</param>
    internal sealed record TouchAccepted(StationAccessUse Use) : StationOperationPhase;
}
