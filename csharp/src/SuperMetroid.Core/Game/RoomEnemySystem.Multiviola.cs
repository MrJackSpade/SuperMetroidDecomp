using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed view of Multiviola's four velocity words. Retail recomputes these words from the
/// population angle and speed every active frame; exposing the native layout makes the two
/// deliberately imperfect reflection negations visible in a debugger.
/// </summary>
public sealed class MultiviolaEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal MultiviolaEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Native <c>$0FAC,x</c>, whole half of horizontal 16.16 velocity.</summary>
    public ushort XVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native <c>$0FAE,x</c>, fractional half of horizontal velocity.</summary>
    public ushort XSubvelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native <c>$0FB0,x</c>, whole half of vertical 16.16 velocity.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native <c>$0FB2,x</c>, fractional half of vertical velocity.</summary>
    public ushort YSubvelocity
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}

/// <summary>
/// Literal translation of retail Multiviola enemy <c>$A0:D1BF</c>, commonly called the
/// Norfair erratic fireball. Its population words encode a standard-mathematics angle and
/// an unsigned speed; the actor flies continuously and reflects that angle on terrain.
/// Animation, touch damage, beam damage, freezing, death, and Grapple cancellation remain
/// on the cartridge's shared enemy paths named by the header rather than bespoke host code.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort MultiviolaDefinition = 0xd1bf;

    private const ushort MultiviolaInstructionList = 0xb2dc;
    private const ushort MultiviolaCosineOffset = 0x0040;
    private const ushort MultiviolaSineOffset = 0x0080;
    private const ushort MultiviolaHorizontalReflection = 0x0040;
    private const ushort MultiviolaVerticalReflection = 0x00c0;

    private readonly MultiviolaEnemyState?[] _multiviolaStates =
        new MultiviolaEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical Multiviola-capable enemy slot.</summary>
    public IReadOnlyList<MultiviolaEnemyState?> MultiviolaStates => _multiviolaStates;

    /// <summary>Ports <c>InitAI_Multiviola</c> at $A2:B3E0.</summary>
    private void InitializeMultiviola(RoomEnemySlot slot)
    {
        var state = new MultiviolaEnemyState(slot);
        _multiviolaStates[slot.SlotIndex] = state;

        // Retail redundantly performs the same magnitude calculation here and again at the
        // beginning of every main-AI call. Preserve the visible variable writes: debugging
        // immediately after room load should match $0FAC-$0FB2 before the first frame.
        CalculateMultiviolaVelocityMagnitudes(slot, state);
        slot.CurrentInstruction = MultiviolaInstructionList;
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

        int horizontalDisplacement = JoinMultiviolaVelocity(
            state.XVelocity,
            state.XSubvelocity);
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
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

        int verticalDisplacement = JoinMultiviolaVelocity(
            state.YVelocity,
            state.YSubvelocity);
        if (MoveEnemyVertically(level, slot, verticalDisplacement))
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
        ushort magnitude = (ushort)(slot.Parameter2 & 0x00ff);
        int xProduct = ReadUnsignedSineMagnitudeProduct(
            slot.Parameter1,
            magnitude,
            MultiviolaCosineOffset);
        int yProduct = ReadUnsignedSineMagnitudeProduct(
            slot.Parameter1,
            magnitude,
            MultiviolaSineOffset);
        state.XVelocity = unchecked((ushort)(xProduct >> 16));
        state.XSubvelocity = unchecked((ushort)xProduct);
        state.YVelocity = unchecked((ushort)(yProduct >> 16));
        state.YSubvelocity = unchecked((ushort)yProduct);
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
            state.XVelocity = unchecked((ushort)~state.XVelocity);
            state.XSubvelocity = unchecked((ushort)(~state.XSubvelocity + 1));
            return;
        }

        state.YVelocity = unchecked((ushort)~state.YVelocity);
        state.YSubvelocity = unchecked((ushort)(~state.YSubvelocity + 1));
    }

    private static int JoinMultiviolaVelocity(ushort whole, ushort fraction) =>
        unchecked((whole << 16) | fraction);

    private MultiviolaEnemyState RequireMultiviolaState(RoomEnemySlot slot) =>
        _multiviolaStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Multiviola state.");
}
