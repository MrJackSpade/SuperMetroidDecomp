using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="EnemyDrawList"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record EnemyDrawListDependencies
{
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

    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }
}
