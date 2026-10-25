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

    /// <summary>Quarter-turn sine-table offset used for the horizontal cosine magnitude.</summary>
    private const ushort MultiviolaCosineOffset = 64;
    /// <summary>Half-turn sine-table offset used for the vertical magnitude.</summary>
    private const ushort MultiviolaSineOffset = 128;
    /// <summary>Angle bits toggled after collision with a vertical surface.</summary>
    private const ushort MultiviolaHorizontalReflection = 64;
    /// <summary>Angle bits toggled after collision with a horizontal surface.</summary>
    private const ushort MultiviolaVerticalReflection = 192;

    /// <summary>Ports <c>InitAI_Multiviola</c> at $A2:B3E0.</summary>
    internal static void InitializeMultiviola(RoomEnemySlot slot)
    {
        var state = new MultiviolaEnemyState
        {
            Heading = slot.Spawn.Population.Parameter1,
            Speed = SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter2),
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // Retail redundantly performs the same magnitude calculation here and again at the
        // beginning of every main-AI call. Preserve the visible variable writes: debugging
        // immediately after room load should match $0FAC-$0FB2 before the first frame.
        CalculateMultiviolaVelocityMagnitudes(state);
        state.Program = AnimationProgramCursor<MultiviolaProgram>.Start(MultiviolaProgram.Flying);
    }

    /// <summary>Ports <c>MainAI_Multiviola</c> at $A2:B40F.</summary>
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

        // The two magnitudes are always rebuilt from the current reflected angle. Native
        // code therefore stores the attempted velocity for this frame, not a persistent
        // direction vector that can simply be negated after collision.
        CalculateMultiviolaVelocityMagnitudes(state);

        if ((unchecked((ushort)(state.Heading + MultiviolaCosineOffset)) & EnemyByteAngleDefinitions.HalfTurn) == 0)
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
            state.Heading ^= MultiviolaHorizontalReflection;
            return;
        }

        if ((unchecked((ushort)(state.Heading + MultiviolaSineOffset)) & EnemyByteAngleDefinitions.HalfTurn) == 0)
            NegateWithoutFractionCarry(ref state, axis: Axis.Vertical);

        int verticalDisplacement = state.Velocity.Y.RawValue;
        if (_terrain.MoveEnemyVertically(level, slot, verticalDisplacement))
            state.Heading ^= MultiviolaVerticalReflection;
    }

    /// <summary>
    /// Recreates $A0:B643's unsigned |cos|/|sin| products of the heading and speed.
    /// </summary>
    private static void CalculateMultiviolaVelocityMagnitudes(MultiviolaEnemyState state)
    {
        int magnitude = checked((ushort)state.Speed);
        int xProduct = EnemyMath.ReadUnsignedSineMagnitudeProduct(
            checked(new SnesByteAngle(unchecked((byte)((ushort)state.Heading)))),
            magnitude,
            MultiviolaCosineOffset);
        int yProduct = EnemyMath.ReadUnsignedSineMagnitudeProduct(
            checked(new SnesByteAngle(unchecked((byte)((ushort)state.Heading)))),
            magnitude,
            MultiviolaSineOffset);
        state.Velocity = new FixedVector<RoomPixels>(
            SnesSignedSixteenSixteen.FromRaw(xProduct), SnesSignedSixteenSixteen.FromRaw(yProduct));
    }

    /// <summary>
    /// Reproduces the two-word EOR/EOR+INC sequences at $A2:B443 and $A2:B47D. This is not
    /// a conventional 32-bit negation: when the fractional word is zero, its wrapped INC
    /// does not carry into the already-complemented whole word, leaving the velocity exactly
    /// one pixel/frame too negative. The retail disassembly explicitly documents this bug.
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

    /// <summary>Applies the native separate-word complement and fractional increment, including its missing carry.</summary>
    private static SnesSignedSixteenSixteen NegateWithoutFractionCarry(SnesSignedSixteenSixteen axis) =>
        SnesSignedSixteenSixteen.FromParts(
            unchecked((short)~axis.WholeWord), unchecked((ushort)(~axis.Fraction + 1)));

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
