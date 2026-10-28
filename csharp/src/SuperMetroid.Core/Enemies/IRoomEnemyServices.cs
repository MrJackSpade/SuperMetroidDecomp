using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The cartridge-state services enemy AI calls during a room: the union of every child operation's
/// service contract plus the RNG, events, boss bits and PLM presence the AI uses directly.
/// </summary>
public interface IRoomEnemyServices : IChozoStatueRoomServices, IMotherBrainRoomServices
{
    /// <summary>The cartridge RNG.</summary>
    IEnemyRandomNumbers Random { get; }

    /// <summary>Global event bits.</summary>
    IGlobalEventFlags Events { get; }

    /// <summary>The current area's boss bits.</summary>
    IAreaBossFlags Bosses { get; }

    /// <summary>The active PLM population.</summary>
    IRoomPlmPresence Plms { get; }

    /// <summary>Writes tiles over the room's BG1 from BG1 tilemap position <paramref name="first"/> onward.</summary>
    /// <param name="first">The first BG1 tilemap position written.</param>
    /// <param name="tiles">The tiles, in tilemap order.</param>
    void DrawBg1Overlay(int first, ReadOnlySpan<BackgroundTile> tiles);

    /// <summary>Writes tiles over the room's BG2 from tilemap position <paramref name="first"/> (#1348).</summary>
    void DrawBg2Overlay(int first, ReadOnlySpan<BackgroundTile> tiles);

    /// <summary>Writes tile numbers into the room's turning floor map from cell <paramref name="firstCell"/> (#1349).</summary>
    void DrawFloorMap(int firstCell, ReadOnlySpan<byte> tiles);
}
