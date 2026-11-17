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
            Speed = place.SpawnSettings<MultiviolaSpawnSettings>().Speed,
        };
        place.SetSpeciesState(state);

        // The speeds along each axis are set as the room loads, before the first update, as each update
        // sets them again.
        CalculateMultiviolaVelocityMagnitudes(state);
        state.Playback = new(MultiviolaAnimations.StartingWith(MultiviolaSegment.Flying), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs the Multiviola's main AI.</summary>
    private void RunMultiviolaMain(
        RoomEnemyPlace place,
        MultiviolaEnemyState state,
        RoomLevelData level)
    {
        // Each update sets the speeds along each axis afresh from the heading the bounces turn; a bounce turns the
        // heading, never the velocity.
        CalculateMultiviolaVelocityMagnitudes(state);

        // The products are magnitudes; the signs of the heading's cosine and sine give each axis its direction.
        if (!EnemyMath.CosineIsNegative(state.Heading))
        {
            TurnLeftOrUp(state, Axis.Horizontal);
        }

        SignedSixteenSixteen horizontalDisplacement = state.Velocity.X;
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

        if (!EnemyMath.SineIsNegative(state.Heading))
            TurnLeftOrUp(state, Axis.Vertical);

        SignedSixteenSixteen verticalDisplacement = state.Velocity.Y;
        if (_terrain.MoveEnemyVertically(level, place, verticalDisplacement))
            state.Heading = MultiviolaQuarterBounce.OffFloorOrCeiling(state.Heading);
    }

    /// <summary>
    /// Sets the Multiviola's speed along each axis from its heading and speed, without direction: the heading's signs
    /// turn each speed left or up before it moves.
    /// </summary>
    private static void CalculateMultiviolaVelocityMagnitudes(MultiviolaEnemyState state)
    {
        int magnitude = state.Speed;
        state.Velocity = new FixedVector<RoomPixels>(
            EnemyMath.AbsoluteCosineSpeed(state.Heading, magnitude).AsDisplacement(),
            EnemyMath.AbsoluteSineSpeed(state.Heading, magnitude).AsDisplacement());
    }

    /// <summary>Turns the Multiviola's speed along <paramref name="axis"/> leftward or upward (<see cref="MultiviolaLeftOrUpSpeed"/>).</summary>
    private static void TurnLeftOrUp(MultiviolaEnemyState state, Axis axis)
    {
        if (axis == Axis.Horizontal)
        {
            state.Velocity = state.Velocity.WithX(MultiviolaLeftOrUpSpeed.Of(state.Velocity.X));
            return;
        }

        state.Velocity = state.Velocity.WithY(MultiviolaLeftOrUpSpeed.Of(state.Velocity.Y));
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
