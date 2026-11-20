using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The Multiviola species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MultiviolaBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Multiviola];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new MultiviolaEntity(this, occupant, place, common));

    /// <summary>Room collision mover used after each attempted Multiviola velocity component.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds Multiviola movement to the active enemy terrain service.</summary>
    internal MultiviolaBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Initializes the Multiviola.</summary>
    internal static void InitializeMultiviola(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        var state = new MultiviolaEnemyState
        {
            Heading = occupant.SpawnSettings<MultiviolaSpawnSettings>().Heading,
        };
        place.SetSpeciesState(state);
        state.Playback = new(MultiviolaAnimations.StartingWith(MultiviolaSegment.Flying), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs the Multiviola's main AI.</summary>
    private void RunMultiviolaMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        MultiviolaEnemyState state,
        RoomLevelData level)
    {
        // Each update takes the velocity afresh from the heading the bounces turn (#2531). It is not
        // EnemyMath.VelocityAlong: an axis the Multiviola heads left or up along moves at MultiviolaLeftOrUpSpeed, which is
        // not the speed's exact reverse and moves it a pixel an update even where the axis has no speed.
        FixedVector<RoomPixels> velocity = MultiviolaLeftOrUpSpeed.VelocityAlong(state.Heading, occupant.SpawnSettings<MultiviolaSpawnSettings>().Speed);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                velocity.X))
        {
            // Bounce off the wall. As in the game, vertical movement is skipped
            // on a horizontal collision, so a corner can consume two frames to reflect both
            // components instead of bouncing diagonally in a single update.
            state.Heading = MultiviolaQuarterBounce.OffWall(state.Heading);
            return;
        }

        // The wall test left the heading as it was, so the vertical speed is the one this update's heading gives.
        if (_terrain.MoveEnemyVertically(level, place, velocity.Y))
            state.Heading = MultiviolaQuarterBounce.OffFloorOrCeiling(state.Heading);
    }

    /// <summary>Advances Multiviola's typed animation.</summary>
    internal static void AdvanceMultiviolaTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, MultiviolaAnimations.Segments,
            static (MultiviolaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Multiviola animation raised {cue}."));

    /// <summary>Runs Multiviola's main AI.</summary>
    internal void RunMultiviolaMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMultiviolaMain(occupant, place, place.RequireSpeciesState<MultiviolaEnemyState>(), frame.Level);
    }
}
