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
internal sealed class OldTourianWallObject(IRoomSamusAccess samus, IOldTourianWallExplosion explosion) : RoomObjectBehaviour
{
    /// <summary>Samus as the room reads her.</summary>
    private readonly IRoomSamusAccess _samus = samus;

    /// <summary>The enemy effects that set off the wall's explosion.</summary>
    private readonly IOldTourianWallExplosion _explosion = explosion;

    /// <summary>Where the wall is in its course.</summary>
    internal OldTourianWallPhase Phase { get; private set; } = OldTourianWallPhase.Starting;

    /// <summary>What <c>Update</c> does for each <see cref="OldTourianWallPhase"/> member, each member's own arm.</summary>
    private IdentityTable<OldTourianWallPhase, Action<RoomObjectPlace, RoomObjectStepContext>> UpdateForOldTourianWallPhase => field ??= new(new Dictionary<OldTourianWallPhase, Action<RoomObjectPlace, RoomObjectStepContext>>
    {
        [OldTourianWallPhase.Starting] = (place, context) => Phase = OldTourianWallPhase.Waiting,
        [OldTourianWallPhase.Waiting] = (place, context) =>
        {
            IRoomSamus current = _samus.Current;
            if (!OldTourianEscapeShaftWallObjectDefinitions.WakePoint.IsPassedBy(current))
                return;
            _explosion.SpawnOldTourianEscapeShaftWallExplosion();
            context.Draw(place, OldTourianEscapeShaftWallObjectDefinitions.OpenWallDraw);
            Phase = OldTourianWallPhase.Opened;
        },
        [OldTourianWallPhase.Opened] = (place, context) => context.RoomObjects.DeleteObject(place),
    });

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        UpdateForOldTourianWallPhase[Phase](place, context);
    }
}
