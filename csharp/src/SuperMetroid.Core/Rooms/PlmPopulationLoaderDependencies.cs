using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>The collaborators a <see cref="PlmPopulationLoader"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record PlmPopulationLoaderDependencies
{
    /// <summary>Bindings.</summary>
    public required PlmRoomBindings Bindings { get; init; }

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
