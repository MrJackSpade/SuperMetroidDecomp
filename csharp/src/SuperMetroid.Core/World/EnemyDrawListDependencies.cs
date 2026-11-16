using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="EnemyDrawList"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record EnemyDrawListDependencies
{
    /// <summary>The drained Torizo, whose heap of sand is drawn after the other enemies.</summary>
    public required DeadTorizoBehavior DeadTorizoBehavior { get; init; }

    /// <summary>The enemy pass's frame counter, which times an enemy's hit flash.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The shared enemy animation.</summary>
    public required EnemyAnimationPlayer AnimationPlayer { get; init; }

    /// <summary>The encounters of the room's bosses, whose Mother Brain's brain and neck are drawn after the other enemies (#2194).</summary>
    public required RoomBossEncounters BossEncounters { get; init; }

    /// <summary>Mother Brain, whose last parts are drawn after the other enemies.</summary>
    public required MotherBrainBehavior MotherBrainBehavior { get; init; }

    /// <summary>How the room's enemies look, which shows each enemy's frame.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The room's enemy artwork, which every enemy frame is drawn from.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Told of each enemy the draw pass draws, which a Puyo's later ceiling strike reads.</summary>
    public required PuyoCeilingTurnFromLastDraw PuyoCeilingTurn { get; init; }
}
