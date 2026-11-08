using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The Multiviola species owner: its per-room state, AI routines and the interactions it registers.</summary>
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

    /// <summary>Angle bits toggled after collision with a vertical surface.</summary>
    private const int MultiviolaHorizontalReflection = 64;
    /// <summary>Angle bits toggled after collision with a horizontal surface.</summary>
    private const int MultiviolaVerticalReflection = 192;

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
        state.Program = AnimationProgramCursor<MultiviolaProgram>.Start(MultiviolaProgram.Flying);
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
            NegateWithoutFractionCarry(ref state, axis: Axis.Horizontal);
        }

        int horizontalDisplacement = state.Velocity.X.RawValue;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalDisplacement))
        {
            // Reflect across a vertical surface. As in retail, vertical movement is skipped
            // on a horizontal collision, so a corner can consume two frames to reflect both
            // components instead of bouncing diagonally in a single host update.
            state.Heading = Reflected(state.Heading, MultiviolaHorizontalReflection);
            return;
        }

        if (!EnemyMath.SineIsNegative(state.Heading))
            NegateWithoutFractionCarry(ref state, axis: Axis.Vertical);

        int verticalDisplacement = state.Velocity.Y.RawValue;
        if (_terrain.MoveEnemyVertically(level, slot, verticalDisplacement))
            state.Heading = Reflected(state.Heading, MultiviolaVerticalReflection);
    }

    /// <summary>The heading with the reflection's angle bits toggled.</summary>
    private static ByteAngle Reflected(ByteAngle heading, int reflection) => new(heading.RawValue ^ reflection);

    /// <summary>
    /// Recreates the shared unsigned |cos|/|sin| products of the heading and speed.
    /// </summary>
    private static void CalculateMultiviolaVelocityMagnitudes(MultiviolaEnemyState state)
    {
        int magnitude = state.Speed;
        int xProduct = EnemyMath.ReadAbsoluteCosineProduct(state.Heading, magnitude);
        int yProduct = EnemyMath.ReadAbsoluteSineProduct(state.Heading, magnitude);
        state.Velocity = new FixedVector<RoomPixels>(
            SignedSixteenSixteen.FromRaw(xProduct), SignedSixteenSixteen.FromRaw(yProduct));
    }

    /// <summary>
    /// Reproduces the Multiviola's two-word negations. This is not
    /// a conventional 32-bit negation: when the fractional word is zero, its wrapped increment
    /// does not carry into the already-complemented whole word, leaving the velocity exactly
    /// one pixel/frame too negative. This is an original-game bug, kept because it is observable.
    /// </summary>
    private static void NegateWithoutFractionCarry(
        ref MultiviolaEnemyState state,
        Axis axis)
    {
        if (axis == Axis.Horizontal)
        {
            state.Velocity = state.Velocity.WithX(NegateWithoutFractionCarry(state.Velocity.X));
            return;
        }

        state.Velocity = state.Velocity.WithY(NegateWithoutFractionCarry(state.Velocity.Y));
    }

    /// <summary>Applies the separate-word complement and fractional increment, including its missing carry.</summary>
    private static SignedSixteenSixteen NegateWithoutFractionCarry(SignedSixteenSixteen axis) =>
        axis.ComplementedWholeNegatedFraction();

    /// <summary>Advances Multiviola's typed animation.</summary>
    internal static void AdvanceMultiviolaTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, MultiviolaAnimations.Programs, MultiviolaFrameDefinitions.ShownFrames.Of,
            static (MultiviolaEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports Multiviola's main AI.</summary>
    internal void RunMultiviolaMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMultiviolaMain(slot, slot.RequireSpeciesState<MultiviolaEnemyState>(), frame.Level);
    }
}
