using SuperMetroid.Core.Bosses.MotherBrain;
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
}
