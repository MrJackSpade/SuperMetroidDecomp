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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MultiviolaEntity(this, slot, common);

    /// <summary>Room collision mover used after each attempted Multiviola velocity component.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds Multiviola movement to the active enemy terrain service.</summary>
    internal MultiviolaBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Initializes the Multiviola.</summary>
    internal static void InitializeMultiviola(RoomEnemySlot slot)
    {
        var state = new MultiviolaEnemyState
        {
            Heading = slot.SpawnSettings<MultiviolaSpawnSettings>().Heading,
            Speed = slot.SpawnSettings<MultiviolaSpawnSettings>().Speed,
        };
        slot.SetSpeciesState(state);

        // Retail redundantly performs the same magnitude calculation here and again at the
        // beginning of every main-AI call, so the speeds are already set when the room
        // finishes loading, before the first frame.
        CalculateMultiviolaVelocityMagnitudes(state);
        state.Playback = new(MultiviolaAnimations.StartingWith(MultiviolaSegment.Flying), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs the Multiviola's main AI.</summary>
    private void RunMultiviolaMain(
        RoomEnemySlot slot,
        MultiviolaEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Multiviola movement requires the active room collision allocation.");
        }

        // The two magnitudes are always rebuilt from the current reflected angle. The game
        // therefore stores the attempted velocity for this frame, not a persistent
        // direction vector that can simply be negated after collision.
        CalculateMultiviolaVelocityMagnitudes(state);

        // The products are magnitudes; the signs of the heading's cosine and sine give each axis its direction.
        if (!EnemyMath.CosineIsNegative(state.Heading))
        {
            TurnLeftOrUp(state, Axis.Horizontal);
        }

        SignedSixteenSixteen horizontalDisplacement = state.Velocity.X;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalDisplacement))
        {
            // Bounce off the wall. As in retail, vertical movement is skipped
            // on a horizontal collision, so a corner can consume two frames to reflect both
            // components instead of bouncing diagonally in a single update.
            state.Heading = MultiviolaQuarterBounce.OffWall(state.Heading);
            return;
        }

        if (!EnemyMath.SineIsNegative(state.Heading))
            TurnLeftOrUp(state, Axis.Vertical);

        SignedSixteenSixteen verticalDisplacement = state.Velocity.Y;
        if (_terrain.MoveEnemyVertically(level, slot, verticalDisplacement))
            state.Heading = MultiviolaQuarterBounce.OffFloorOrCeiling(state.Heading);
    }

    /// <summary>
    /// Recreates the shared unsigned |cos|/|sin| products of the heading and speed.
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
    internal static void AdvanceMultiviolaTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, MultiviolaAnimations.Segments,
            static (MultiviolaEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Multiviola animation raised {cue}."));

    /// <summary>Runs Multiviola's main AI.</summary>
    internal void RunMultiviolaMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMultiviolaMain(slot, slot.RequireSpeciesState<MultiviolaEnemyState>(), frame.Level);
    }
}
