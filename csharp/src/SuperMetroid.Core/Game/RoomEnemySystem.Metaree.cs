using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A3 function pointer stored in a Metaree's variable B.</summary>
public enum MetareeEnemyFunction : ushort
{
    Idling = 0x8987,
    PreparingAttack = 0x89d4,
    LaunchedAttack = 0x89f3,
    Burrowing = 0x8a5c,
}

/// <summary>
/// Typed view of Metaree's six private words at <c>$0FA8-$0FB2</c>. The wrapper deliberately
/// aliases the live enemy slot: debugger edits and native-width wrapping remain observable,
/// while the runtime no longer spreads anonymous <c>VariableA-F</c> accesses through its AI.
/// </summary>
public sealed class MetareeEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal MetareeEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Reloaded to 21 during flight, then counted down while underground.</summary>
    public ushort BurrowTimer
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Direct bank-$A3 state-function pointer dispatched by main AI.</summary>
    public MetareeEnemyFunction Function
    {
        get => (MetareeEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Instruction-list index requested by AI.</summary>
    public ushort RequestedInstructionListIndex
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Instruction-list index currently installed in the common slot.</summary>
    public ushort InstalledInstructionListIndex
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Set by instruction <c>$A3:8956</c> after the launch crouch completes.</summary>
    public bool AttackReady
    {
        get => _slot.VariableE != 0;
        internal set => _slot.VariableE = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Unsigned whole-pixel downward speed calculated once from Samus's vertical position.
    /// The unsigned type is important: the original game's above-Samus bug relies on wrap.
    /// </summary>
    public ushort YVelocity
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}

/// <summary>Literal translation of Metaree enemy <c>$D67F</c> at <c>$A3:88F0-$8B64</c>.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort MetareeDefinition = 0xd67f;

    private const int MetareeInstructionListPointers = 0xa3894e;
    private const ushort MetareeHorizontalActivationDistance = 0x48;
    private const ushort MetareeDiveDivisorNtsc = 24;
    private const ushort MetareeMinimumYVelocity = 4;
    private const ushort MetareeHorizontalDiveSpeed = 2;
    private const ushort MetareeBurrowLifetimeNtsc = 21;
    private const ushort MetareeParticleFrameNtsc = 8;

    private readonly MetareeEnemyState?[] _metareeStates =
        new MetareeEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Metaree state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<MetareeEnemyState?> MetareeStates => _metareeStates;

    /// <summary>Ports <c>InitAI_Metaree</c> at <c>$A3:8960</c>.</summary>
    private void InitializeMetaree(RoomEnemySlot slot)
    {
        _metareeStates[slot.SlotIndex] = new MetareeEnemyState(slot)
        {
            BurrowTimer = 0,
            Function = MetareeEnemyFunction.Idling,
            RequestedInstructionListIndex = 0,
            InstalledInstructionListIndex = 0,
            AttackReady = false,
            YVelocity = 0,
        };

        // The initializer writes the idle pointer directly rather than going through the
        // change detector, because both requested and installed indexes intentionally start
        // at zero. InitializeEnemies clears the immediate map afterward; frame one restores
        // it from this real ROM list through the common instruction interpreter.
        slot.CurrentInstruction = OrdinaryEnemyInstructionLists.MetareeInitial;
    }

    /// <summary>Ports the indirect state dispatch in <c>MainAI_Metaree</c>.</summary>
    private void RunMetareeMain(
        RoomEnemySlot slot,
        MetareeEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Metaree AI requires the active Samus actor.");

        switch (state.Function)
        {
            case MetareeEnemyFunction.Idling:
                RunMetareeIdle(slot, state, samus);
                return;

            case MetareeEnemyFunction.PreparingAttack:
                RunMetareePreparation(slot, state);
                return;

            case MetareeEnemyFunction.LaunchedAttack:
                if (level is null)
                {
                    throw new InvalidOperationException(
                        "Metaree dive collision requires active room level data.");
                }
                RunMetareeDive(slot, state, samus, level);
                return;

            case MetareeEnemyFunction.Burrowing:
                RunMetareeBurrow(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Metaree function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void RunMetareeIdle(
        RoomEnemySlot slot,
        MetareeEnemyState state,
        SamusState samus)
    {
        // `$A3:898A-$899A` computes a wrapped signed difference, takes its two's-complement
        // absolute value, and performs an unsigned compare. It notably does *not* check that
        // Samus is below the actor; preserving that retail bug matters to the velocity math.
        int horizontalDistance = Math.Abs(
            unchecked((short)(slot.XPosition - samus.XPosition)));
        if (horizontalDistance >= MetareeHorizontalActivationDistance)
            return;

        // The SNES unsigned-divider input is the wrapped 16-bit Y difference. If Samus is
        // above Metaree, this deliberately creates the cartridge's enormous dive velocity
        // instead of sanitizing the old bug into modern signed physics.
        ushort verticalDifference = unchecked((ushort)(samus.YPosition - slot.YPosition));
        state.YVelocity = unchecked((ushort)(
            verticalDifference / MetareeDiveDivisorNtsc + MetareeMinimumYVelocity));
        state.RequestedInstructionListIndex++;
        InstallRequestedMetareeInstruction(slot, state);
        state.Function = MetareeEnemyFunction.PreparingAttack;
    }

    private void RunMetareePreparation(RoomEnemySlot slot, MetareeEnemyState state)
    {
        if (!state.AttackReady)
            return;

        state.AttackReady = false;
        state.RequestedInstructionListIndex++;
        InstallRequestedMetareeInstruction(slot, state);
        state.Function = MetareeEnemyFunction.LaunchedAttack;

        // QueueSound_Lib2_Max6 receives $5B at $A3:89EB. Audio mixing remains an outer
        // subsystem, so expose the exact event word for the frontend/debugger seam.
        LastMetareeSoundEffect = 0x005b;
    }

    private void RunMetareeDive(
        RoomEnemySlot slot,
        MetareeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // Native AI reloads this every airborne frame. It becomes a countdown only after
        // the solid-bit probe reports the ground, which is why interrupted dives never age.
        state.BurrowTimer = MetareeBurrowLifetimeNtsc;

        // Bits zero/one are still unnamed because their general engine semantics are not
        // proven. Metaree writes the literal mask `$0003`; retaining it raw avoids inventing
        // a flags-enum contract while preserving collision/processing behavior exactly.
        slot.Properties = unchecked((ushort)(slot.Properties | 0x0003));
        if (EnemyHasSolidHighBitAhead(
                level,
                slot,
                state.YVelocity << 16,
                movingDown: true))
        {
            slot.InstructionTimer = 1;
            slot.Timer = 0;
            state.Function = MetareeEnemyFunction.Burrowing;
            LastMetareeSoundEffect = 0x005c;
            return;
        }

        // Unlike common movement, Metaree applies whole-pixel words directly and leaves
        // both subpositions untouched. Horizontal steering re-evaluates Samus every frame.
        slot.YPosition = unchecked((ushort)(slot.YPosition + state.YVelocity));
        bool metareeIsLeftOfSamus =
            unchecked((short)(slot.XPosition - samus.XPosition)) < 0;
        slot.XPosition = unchecked((ushort)(slot.XPosition +
            (metareeIsLeftOfSamus
                ? MetareeHorizontalDiveSpeed
                : -MetareeHorizontalDiveSpeed)));
    }

    private void RunMetareeBurrow(RoomEnemySlot slot, MetareeEnemyState state)
    {
        state.BurrowTimer = unchecked((ushort)(state.BurrowTimer - 1));
        if (state.BurrowTimer == 0)
        {
            // `$A3:8A92` writes the packed live palette/tile indexes back into the spawn
            // record before hiding/deleting the actor. Preserve that odd state mutation so
            // debugger-visible spawn data agrees even though current respawn logic is later.
            slot.Spawn = slot.Spawn with
            {
                VramTilesIndex = unchecked((ushort)(slot.PaletteIndex | slot.VramTilesIndex)),
            };
            slot.PaletteIndex = EnemyPaletteBits.Palette5;
            slot.VramTilesIndex = 0;
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        if (state.BurrowTimer == MetareeParticleFrameNtsc)
            SpawnMetareeParticleBurst(slot);
        slot.YPosition = unchecked((ushort)(slot.YPosition + 1));
    }

    private void InstallRequestedMetareeInstruction(
        RoomEnemySlot slot,
        MetareeEnemyState state)
    {
        ushort requested = state.RequestedInstructionListIndex;
        if (requested == state.InstalledInstructionListIndex)
            return;
        if (requested >= 4)
        {
            throw new InvalidDataException(
                $"Metaree instruction-list index {requested} exceeds its four-entry table.");
        }

        state.InstalledInstructionListIndex = requested;
        slot.CurrentInstruction = ReadWord(
            _bus!,
            MetareeInstructionListPointers + requested * 2);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private MetareeEnemyState RequireMetareeState(RoomEnemySlot slot) =>
        _metareeStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Metaree state.");
}
