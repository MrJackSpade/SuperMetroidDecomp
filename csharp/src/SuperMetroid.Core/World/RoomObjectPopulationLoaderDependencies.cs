using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="RoomObjectPopulationLoader"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record RoomObjectPopulationLoaderDependencies
{
    /// <summary>Bomb torizo hand.</summary>
    public required BombTorizoHandObjects BombTorizoHand { get; init; }

    /// <summary>Collectible roomObjects.</summary>
    public required CollectibleObjects CollectibleObjects { get; init; }

    /// <summary>Eye doors.</summary>
    public required EyeDoorObjects EyeDoors { get; init; }

    /// <summary>Gates.</summary>
    public required GateObjects Gates { get; init; }

    /// <summary>Mother brain glass.</summary>
    public required MotherBrainGlassObjects MotherBrainGlass { get; init; }

    /// <summary>Slot table.</summary>
    public required RoomObjectSlotTable SlotTable { get; init; }

    /// <summary>Speed booster escape.</summary>
    public required SpeedBoosterEscapeObjects SpeedBoosterEscape { get; init; }

    /// <summary>Maridia's glass tube, which population sets up.</summary>
    public required NoobTubeObjects NoobTube { get; init; }
}
