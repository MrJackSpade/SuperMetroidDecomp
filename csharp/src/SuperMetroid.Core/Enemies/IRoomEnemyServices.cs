using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The game-state services enemy AI calls during a room: the union of every child operation's
/// service contract plus the RNG, events, boss bits and PLM presence the AI uses directly.
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

    /// <summary>The active PLM population.</summary>
    IRoomObjectPresence RoomObjects { get; }

    /// <summary>Writes <paramref name="picture"/> over the room's foreground at its tilemap cell (#1346, #1466).</summary>
    void WriteForegroundPicture(WrittenPicture picture);

    /// <summary>Writes <paramref name="picture"/> over the room's background at its tilemap cell (#1348, #1466).</summary>
    void WriteBackgroundPicture(WrittenPicture picture);

    /// <summary>Draws <paramref name="frame"/>'s picture over the part of the room's turning floor it covers (#1349, #1467).</summary>
    void DrawFloor(CeresFloorAnimationFrame frame);
}
