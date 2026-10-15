using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Core.Enemies;

/// <summary>The collaborators a <see cref="EnemyDrawList"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record EnemyDrawListDependencies
{
    /// <summary>Ceres ridley behavior.</summary>
    public required CeresRidleyBehavior CeresRidleyBehavior { get; init; }

    /// <summary>Dead torizo behavior.</summary>
    public required DeadTorizoBehavior DeadTorizoBehavior { get; init; }

    /// <summary>Frame context.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Instructions.</summary>
    public required EnemyInstructionInterpreter Instructions { get; init; }

    /// <summary>Mother brain behavior.</summary>
    public required MotherBrainBehavior MotherBrainBehavior { get; init; }

    /// <summary>Presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Ridley encounter.</summary>
    public required RidleyEncounter RidleyEncounter { get; init; }

    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Slot table.</summary>
    public required EnemySlotTable SlotTable { get; init; }
}
