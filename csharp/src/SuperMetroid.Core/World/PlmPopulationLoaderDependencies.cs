using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="PlmPopulationLoader"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record PlmPopulationLoaderDependencies
{
    /// <summary>Bomb torizo hand.</summary>
    public required BombTorizoHandPlms BombTorizoHand { get; init; }

    /// <summary>Collectible plms.</summary>
    public required CollectiblePlms CollectiblePlms { get; init; }

    /// <summary>Eye doors.</summary>
    public required EyeDoorPlms EyeDoors { get; init; }

    /// <summary>Gates.</summary>
    public required GatePlms Gates { get; init; }

    /// <summary>Mother brain glass.</summary>
    public required MotherBrainGlassPlms MotherBrainGlass { get; init; }

    /// <summary>Slot table.</summary>
    public required PlmSlotTable SlotTable { get; init; }

    /// <summary>Speed booster escape.</summary>
    public required SpeedBoosterEscapePlms SpeedBoosterEscape { get; init; }
}
