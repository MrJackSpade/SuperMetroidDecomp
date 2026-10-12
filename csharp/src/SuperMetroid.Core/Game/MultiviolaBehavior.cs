using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Multiviola species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class MultiviolaBehavior : IEnemySpecies
{
    private readonly EnemyTerrain _terrain;

    internal MultiviolaBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    private const ushort MultiviolaCosineOffset = 0x0040;
    private const ushort MultiviolaSineOffset = 0x0080;
    private const ushort MultiviolaHorizontalReflection = 0x0040;
    private const ushort MultiviolaVerticalReflection = 0x00c0;

    internal readonly MultiviolaEnemyState?[] _multiviolaStates =
        new MultiviolaEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Multiviola</c> at $A2:B3E0.</summary>
    internal void InitializeMultiviola(RoomEnemySlot slot)
    {
        var state = new MultiviolaEnemyState(slot);
        _multiviolaStates[slot.SlotIndex] = state;

        // Retail redundantly performs the same magnitude calculation here and again at the
        // beginning of every main-AI call. Preserve the visible variable writes: debugging
        // immediately after room load should match $0FAC-$0FB2 before the first frame.
        CalculateMultiviolaVelocityMagnitudes(slot, state);
        slot.CurrentInstruction = MultiviolaInstructionProgramDefinitions.Flying;
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
        CalculateMultiviolaVelocityMagnitudes(slot, state);

        if ((unchecked((ushort)(slot.Parameter1 + MultiviolaCosineOffset)) & 0x0080) == 0)
        {
            NativeMultiviolaNegate(ref state, horizontal: true);
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
            slot.Parameter1 ^= MultiviolaHorizontalReflection;
            return;
        }

        if ((unchecked((ushort)(slot.Parameter1 + MultiviolaSineOffset)) & 0x0080) == 0)
            NativeMultiviolaNegate(ref state, horizontal: false);

        int verticalDisplacement = state.Velocity.Y.RawValue;
        if (_terrain.MoveEnemyVertically(level, slot, verticalDisplacement))
            slot.Parameter1 ^= MultiviolaVerticalReflection;
    }

    /// <summary>
    /// Recreates $A0:B643's unsigned |cos|/|sin| products. Only parameter two's low byte is
    /// copied into the native magnitude word; its high byte is intentionally ignored.
    /// </summary>
    private static void CalculateMultiviolaVelocityMagnitudes(
        RoomEnemySlot slot,
        MultiviolaEnemyState state)
    {
        ushort magnitude = (ushort)(SnesLittleEndian.LowByte(slot.Parameter2));
        int xProduct = EnemyMath.ReadUnsignedSineMagnitudeProduct(
            slot.Parameter1,
            magnitude,
            MultiviolaCosineOffset);
        int yProduct = EnemyMath.ReadUnsignedSineMagnitudeProduct(
            slot.Parameter1,
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
    private static void NativeMultiviolaNegate(
        ref MultiviolaEnemyState state,
        bool horizontal)
    {
        if (horizontal)
        {
            state.Velocity = state.Velocity.WithX(NativeMultiviolaNegate(state.Velocity.X));
            return;
        }

        state.Velocity = state.Velocity.WithY(NativeMultiviolaNegate(state.Velocity.Y));
    }

    private static SnesSignedSixteenSixteen NativeMultiviolaNegate(SnesSignedSixteenSixteen axis) =>
        SnesSignedSixteenSixteen.FromParts(
            unchecked((short)~axis.WholeWord), unchecked((ushort)(~axis.Fraction + 1)));

    private MultiviolaEnemyState RequireMultiviolaState(RoomEnemySlot slot) =>
        _multiviolaStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Multiviola state.");

    internal void RegisterMultiviolaInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Multiviola, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Multiviola, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeMultiviola(slot);
        });
    }

    internal void RegisterMultiviolaMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Multiviola, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Multiviola, (slot, frame, nmiFrameCounter8) =>
        {
            RunMultiviolaMain(slot, RequireMultiviolaState(slot), frame.Level);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_multiviolaStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
