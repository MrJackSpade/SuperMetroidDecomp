using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The fake wall in the old Tourian escape shaft (#1554). From its second update on it watches for Samus strictly below
/// and right of its target; on the update she gets there it sets off the wall's explosion and opens the wall, and it
/// leaves the room on the next update.
/// </summary>
/// <param name="samus">Samus as the room reads her.</param>
/// <param name="explosion">The enemy effects that set off the wall's explosion.</param>
internal sealed class OldTourianWallObject(IRoomSamusAccess? samus, IOldTourianWallExplosion? explosion) : RoomObjectBehaviour
{
    /// <summary>Samus as the room reads her.</summary>
    private readonly IRoomSamusAccess? _samus = samus;

    /// <summary>The enemy effects that set off the wall's explosion.</summary>
    private readonly IOldTourianWallExplosion? _explosion = explosion;

    /// <summary>Where the wall is in its course.</summary>
    internal OldTourianWallPhase Phase { get; private set; } = OldTourianWallPhase.Starting;

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        switch (Phase)
        {
            case OldTourianWallPhase.Starting:
                Phase = OldTourianWallPhase.Waiting;
                return;
            case OldTourianWallPhase.Waiting:
                IRoomSamus current = _samus?.Current
                    ?? throw new InvalidOperationException("Old Tourian escape-shaft wall has no live Samus owner.");
                if (!OldTourianEscapeShaftWallObjectDefinitions.WakePoint.IsPassedBy(current))
                    return;
                (_explosion ?? throw new InvalidOperationException(
                    "Old Tourian escape-shaft wall has no enemy-projectile owner.")).SpawnOldTourianEscapeShaftWallExplosion();
                context.Draw(place, OldTourianEscapeShaftWallObjectDefinitions.OpenWallDraw);
                Phase = OldTourianWallPhase.Opened;
                return;
            case OldTourianWallPhase.Opened:
                context.RoomObjects.DeleteObject(place);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(OldTourianWallPhase)} {Phase}.");
        }
    }
}
