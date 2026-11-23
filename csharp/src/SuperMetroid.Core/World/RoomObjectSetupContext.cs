using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>What a placed room object's family setup reaches as the room's population places it (#2288).</summary>
internal sealed partial record RoomObjectSetupContext
{
    /// <summary>The room's level, progress and area.</summary>
    public required RoomObjectRoom Room { get; init; }

    /// <summary>The area's defeated bosses, which the Bomb Torizo's hand reads without being able to set
    /// them.</summary>
    public required IDefeatedBossesReader Bosses { get; init; }

    /// <summary>The place the placement took, holding its block.</summary>
    public required RoomObjectPlace Place { get; init; }

    /// <summary>The room's room-object families whose setups keep state of their own.</summary>
    public required RoomObjectPopulationLoaderDependencies Families { get; init; }

    /// <summary>
    /// The room's enemy projectiles' spawners of the projectiles room objects throw (#2514), which Mother Brain's glass and
    /// the Bomb Torizo statue's hand take as they set up.
    /// </summary>
    public required RoomObjectThrownProjectileSpawners Thrown { get; init; }
}
