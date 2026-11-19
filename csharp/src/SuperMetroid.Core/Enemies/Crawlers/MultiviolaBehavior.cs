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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new MultiviolaEntity(this, place, common);

    /// <summary>Room collision mover used after each attempted Multiviola velocity component.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds Multiviola movement to the active enemy terrain service.</summary>
    internal MultiviolaBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Initializes the Multiviola.</summary>
    internal static void InitializeMultiviola(RoomEnemyPlace place)
    {
        var state = new MultiviolaEnemyState
        {
            Heading = place.SpawnSettings<MultiviolaSpawnSettings>().Heading,
        };
        place.SetSpeciesState(state);
        state.Playback = new(MultiviolaAnimations.StartingWith(MultiviolaSegment.Flying), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs the Multiviola's main AI.</summary>
    private void RunMultiviolaMain(
        RoomEnemyPlace place,
        MultiviolaEnemyState state,
        RoomLevelData level)
    {
        // Each update takes the speeds along each axis afresh from the heading the bounces turn (#2531). The products are
        // magnitudes; the signs of the heading's cosine and sine give each axis its direction. They are not
        // EnemyMath.VelocityAlong's components: an axis the Multiviola heads left or up along takes MultiviolaLeftOrUpSpeed,
        // which is not the speed's exact reverse and moves it a pixel an update even where the axis has no speed.
        int speed = place.SpawnSettings<MultiviolaSpawnSettings>().Speed;
        SignedSixteenSixteen horizontalDisplacement = EnemyMath.AbsoluteCosineSpeed(state.Heading, speed).AsDisplacement();
        if (!EnemyMath.CosineIsNegative(state.Heading))
            horizontalDisplacement = MultiviolaLeftOrUpSpeed.Of(horizontalDisplacement);

        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                horizontalDisplacement))
        {
            // Bounce off the wall. As in the game, vertical movement is skipped
            // on a horizontal collision, so a corner can consume two frames to reflect both
            // components instead of bouncing diagonally in a single update.
            state.Heading = MultiviolaQuarterBounce.OffWall(state.Heading);
            return;
        }

        // The wall test left the heading as it was, so the vertical speed is the one this update's heading gives.
        SignedSixteenSixteen verticalDisplacement = EnemyMath.AbsoluteSineSpeed(state.Heading, speed).AsDisplacement();
        if (!EnemyMath.SineIsNegative(state.Heading))
            verticalDisplacement = MultiviolaLeftOrUpSpeed.Of(verticalDisplacement);

        if (_terrain.MoveEnemyVertically(level, place, verticalDisplacement))
            state.Heading = MultiviolaQuarterBounce.OffFloorOrCeiling(state.Heading);
    }

    /// <summary>Advances Multiviola's typed animation.</summary>
    internal static void AdvanceMultiviolaTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, MultiviolaAnimations.Segments,
            static (MultiviolaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Multiviola animation raised {cue}."));

    /// <summary>Runs Multiviola's main AI.</summary>
    internal void RunMultiviolaMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMultiviolaMain(place, place.RequireSpeciesState<MultiviolaEnemyState>(), frame.Level);
    }
}
