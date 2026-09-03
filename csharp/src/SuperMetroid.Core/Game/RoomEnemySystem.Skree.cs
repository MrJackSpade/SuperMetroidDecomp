using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A3 function pointer stored in a Skree's variable B.</summary>
public enum SkreeEnemyFunction : ushort
{
    Idling = 0xc6d5,
    PreparingAttack = 0xc6f7,
    Diving = 0xc716,
    Burrowing = 0xc77f,
}

/// <summary>
/// Typed debugger view of the five private words overlaid on a Skree's common enemy slot.
/// The wrapper names the native A-E ownership without copying state away from the slot.
/// </summary>
public sealed class SkreeEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal SkreeEnemyState(RoomEnemySlot slot) => _slot = slot;

    public ushort BurrowTimer
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    public SkreeEnemyFunction Function
    {
        get => (SkreeEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    public ushort RequestedInstructionIndex
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    public ushort InstalledInstructionIndex
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    public bool AttackReady
    {
        get => _slot.VariableE != 0;
        internal set => _slot.VariableE = value ? (ushort)1 : (ushort)0;
    }
}

/// <summary>Literal translation of enemy $DB7F at $A3:C6A4-$C7D4.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort SkreeDefinition = 0xdb7f;

    private const int SkreeInstructionPointerTable = 0xa3c69c;
    private readonly SkreeEnemyState?[] _skreeStates =
        new SkreeEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical slot currently owned by a Skree.</summary>
    public IReadOnlyList<SkreeEnemyState?> SkreeStates => _skreeStates;

    private void InitializeSkree(RoomEnemySlot slot)
    {
        var state = new SkreeEnemyState(slot)
        {
            RequestedInstructionIndex = 0,
            InstalledInstructionIndex = 0,
            AttackReady = false,
            Function = SkreeEnemyFunction.Idling,
        };
        _skreeStates[slot.SlotIndex] = state;
        slot.CurrentInstruction = OrdinaryEnemyInstructionLists.SkreeInitial;
    }

    private void RunSkreeMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Skree AI requires the active Samus actor.");
        SkreeEnemyState state = RequireSkreeState(slot);
        switch (state.Function)
        {
            case SkreeEnemyFunction.Idling:
                if (Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition))) < 0x30)
                {
                    state.RequestedInstructionIndex++;
                    InstallRequestedSkreeInstruction(slot, state);
                    state.Function = SkreeEnemyFunction.PreparingAttack;
                }
                return;

            case SkreeEnemyFunction.PreparingAttack:
                if (!state.AttackReady)
                    return;
                state.AttackReady = false;
                state.RequestedInstructionIndex++;
                InstallRequestedSkreeInstruction(slot, state);
                state.Function = SkreeEnemyFunction.Diving;
                // `$A3:C70F` queues library-two sound $5B on the exact frame the wind-up
                // instruction releases the dive. Publishing it as a frame event retains
                // native timing without coupling enemy AI to a particular audio backend.
                LastSkreeSoundEffect = 0x005b;
                QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x005b), maximumQueued: 6);
                return;

            case SkreeEnemyFunction.Diving:
                if (level is null)
                    throw new InvalidOperationException("Skree dive collision requires room level data.");
                RunSkreeDive(slot, state, samus, level);
                return;

            case SkreeEnemyFunction.Burrowing:
                RunSkreeBurrow(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Skree function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void RunSkreeDive(
        RoomEnemySlot slot,
        SkreeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // The USA/Japan build reloads 21 every airborne frame. It becomes the complete
        // burrow lifetime only on the frame whose six-pixel downward probe collides.
        state.BurrowTimer = 21;
        slot.Properties = unchecked((ushort)(slot.Properties | 3));
        if (EnemyHasSolidHighBitAhead(level, slot, 6 << 16, movingDown: true))
        {
            slot.InstructionTimer = 1;
            slot.Timer = 0;
            state.Function = SkreeEnemyFunction.Burrowing;
            // The floor collision has its own distinct impact/burrow request at `$A3:C771`.
            LastSkreeSoundEffect = 0x005c;
            QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x005c), maximumQueued: 6);
            return;
        }

        slot.YPosition = unchecked((ushort)(slot.YPosition + 6));
        slot.XPosition = unchecked((ushort)(slot.XPosition +
            (unchecked((short)(slot.XPosition - samus.XPosition)) >= 0 ? -1 : 1)));
    }

    private void RunSkreeBurrow(RoomEnemySlot slot, SkreeEnemyState state)
    {
        state.BurrowTimer = unchecked((ushort)(state.BurrowTimer - 1));
        if (state.BurrowTimer == 0)
        {
            slot.PaletteIndex = 0x0a00;
            slot.VramTilesIndex = 0;
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        if (state.BurrowTimer == 8)
            SpawnSkreeParticleBurst(slot);
        slot.YPosition = unchecked((ushort)(slot.YPosition + 1));
    }

    private void InstallRequestedSkreeInstruction(RoomEnemySlot slot, SkreeEnemyState state)
    {
        if (state.RequestedInstructionIndex == state.InstalledInstructionIndex)
            return;
        if (state.RequestedInstructionIndex >= 4)
        {
            throw new InvalidDataException(
                $"Skree instruction index {state.RequestedInstructionIndex} exceeds $A3:C69C.");
        }

        state.InstalledInstructionIndex = state.RequestedInstructionIndex;
        slot.CurrentInstruction = ReadWord(
            _bus!,
            SkreeInstructionPointerTable + state.RequestedInstructionIndex * 2);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private SkreeEnemyState RequireSkreeState(RoomEnemySlot slot) =>
        _skreeStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Skree state.");
}
