using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The room's PLM owners Samus's movement, collision and projectiles reach (#1395): the room-level families, plus the
/// projectile triggers, quicksand and the Chozo statue hand. The room composition implements it.
/// </summary>
internal interface ISamusRoomPlms : IRoomPlms
{
    /// <summary>Spawns PLMs for breakable blocks struck by projectile impacts.</summary>
    ProjectileTriggerPlms ProjectileTriggers { get; }

    /// <summary>Runs quicksand room objects through the shared slot allocation.</summary>
    QuicksandPlms Quicksand { get; }

    /// <summary>The Chozo statue hand Samus's collision notifies.</summary>
    IChozoStatueHand ChozoStatueHand { get; }
}
