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

    /// <summary>The shared enemy animation.</summary>
    public required EnemyAnimationPlayer AnimationPlayer { get; init; }

    /// <summary>Mother brain behavior.</summary>
    public required MotherBrainBehavior MotherBrainBehavior { get; init; }

    /// <summary>Presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Told of each enemy the draw pass draws, which a Puyo's later ceiling strike reads.</summary>
    public required PuyoCeilingTurnFromLastDraw PuyoCeilingTurn { get; init; }
}
