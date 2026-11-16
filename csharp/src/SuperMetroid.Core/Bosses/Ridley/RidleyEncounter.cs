using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// What the room sees of Ridley's encounters (#2231): whether a live Ridley has begun one here, Ceres Ridley as he
/// attached himself to the room for its own Ceres reads, the door of his Ceres room, and the Ceres escape progress the
/// session keeps across rooms. His parts hold their own encounter from their initialization and never read it here.
/// </summary>
[RoomLifetime]
internal sealed class RidleyEncounter : ICeresEscapeStatus, ICeresRidleyScene
{
    /// <summary>Starts the room's view of Ridley's encounters; his body and Ceres's door attach as they initialize.</summary>
    /// <param name="session">The session that holds Ceres's escape progress across rooms.</param>
    internal RidleyEncounter(EnemySession session)
    {
        _session = Ensure.NotNull(session);
    }

    /// <summary>The session that holds Ceres's escape progress across rooms.</summary>
    private readonly EnemySession _session;

    /// <summary>
    /// Whether a live Ridley began his encounter in this room as he initialized: Ceres Ridley always, Lower Norfair Ridley
    /// unless he was found defeated. The room's loader reads it to hide the minimap.
    /// </summary>
    internal bool RidleyPresent { get; private set; }

    /// <summary>
    /// Ceres Ridley as he attached himself to the room as he initialized (#2231, #2194): the room's own Ceres reads, his
    /// shots and the Baby and door it draws, go through it; null in a room he has not initialized in. No part of his reads it.
    /// </summary>
    internal CeresRidleyAttachment? CeresRidley { get; private set; }

    /// <summary>
    /// The door of Ceres Ridley's room once it has initialized, whose drawing his scene takes over while the door hands it
    /// to him; null in any other room.
    /// </summary>
    internal RoomEnemyPlace? CeresRoomDoor { get; private set; }

    /// <summary>
    /// How far Ceres's escape has progressed, consumed by the Ceres doors and the elevator shaft. A fresh station load
    /// begins before Ridley escapes; Ceres Ridley publishes his escape and the self-destruct, and the elevator door's
    /// destruction publishes the last stage.
    /// </summary>
    public CeresEscapeStatus CeresStatus
    {
        get => _session.CeresStatus;
        set => _session.CeresStatus = value;
    }

    /// <summary>Records that Lower Norfair Ridley began his encounter in this room.</summary>
    internal void BeginLowerNorfair() => RidleyPresent = true;

    /// <summary>Attaches Ceres Ridley, his body <paramref name="body"/> and his encounter <paramref name="ridley"/>, as he begins it.</summary>
    /// <exception cref="InvalidDataException">Another enemy of the room has already initialized as Ceres Ridley.</exception>
    internal void BeginCeres(RoomEnemyPlace body, CeresRidleyState ridley)
    {
        Ensure.NotNull(body);
        if (CeresRidley is { } attached && !ReferenceEquals(attached.Body, body))
            throw new InvalidDataException($"Ceres Ridley initializes as enemy {body.Ordinal}, but enemy {attached.Body.Ordinal} already is.");
        CeresRidley = new CeresRidleyAttachment(body, Ensure.NotNull(ridley));
        RidleyPresent = true;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">Another door of the room has already initialized as the door of Ridley's room.</exception>
    void ICeresRidleyScene.AttachRoomDoor(RoomEnemyPlace door)
    {
        Ensure.NotNull(door);
        if (CeresRoomDoor is { } attached && !ReferenceEquals(attached, door))
            throw new InvalidDataException($"Ceres door {door.Ordinal} initializes as the door of Ridley's room, but door {attached.Ordinal} already is.");
        CeresRoomDoor = door;
    }
}
