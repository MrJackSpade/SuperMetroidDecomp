using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="EnemyDrawList"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record EnemyDrawListDependencies
{
    /// <summary>The encounters' sprites drawn after every queued enemy, in their order (#2571).</summary>
    public required IReadOnlyList<IEnemyLateDraw> LateDraws { get; init; }

    /// <summary>The enemy pass's frame counter, which times an enemy's hit flash.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The shared enemy animation.</summary>
    public required EnemyAnimationPlayer AnimationPlayer { get; init; }


    /// <summary>How the room's enemies look, which shows each enemy's frame.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The room's enemy artwork, which every enemy frame is drawn from.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Told of each enemy the draw pass draws, which a Puyo's later ceiling strike reads.</summary>
    public required PuyoCeilingTurnFromLastDraw PuyoCeilingTurn { get; init; }
}
