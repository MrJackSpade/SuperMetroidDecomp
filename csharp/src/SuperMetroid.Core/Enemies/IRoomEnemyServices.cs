using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The game-state services enemy AI calls during a room: the union of every child operation's
/// service contract plus the RNG, events, boss bits and room-object presence the AI uses directly.
/// </summary>
public interface IRoomEnemyServices : IChozoStatueRoomServices, IMotherBrainRoomServices
{
    /// <summary>The game RNG.</summary>
    IEnemyRandomNumbers Random { get; }

    /// <summary>Global event bits.</summary>
    IGlobalEventFlags Events { get; }

    /// <summary>How many Zebetites have been destroyed.</summary>
    IZebetiteProgress ZebetiteProgress { get; }

    /// <summary>The current area's boss bits.</summary>
    IAreaBossFlags Bosses { get; }

    /// <summary>The active room-object population.</summary>
    IRoomObjectPresence RoomObjects { get; }

    /// <summary>Shows <paramref name="frame"/> on the part of the room's turning floor it animates (#1349, #1467, #1626).</summary>
    void ShowFloorFrame(CeresFloorAnimationFrame frame);
}
