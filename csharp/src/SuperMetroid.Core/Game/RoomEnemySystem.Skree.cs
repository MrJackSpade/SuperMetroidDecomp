using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A3 function pointer stored in a Skree's variable B.</summary>
public enum SkreeEnemyFunction : ushort
{
    /// <summary><c>$A3:C6D5</c>: waits until Samus is within 48 horizontal pixels.</summary>
    Idling = 0xc6d5,

    /// <summary><c>$A3:C6F7</c>: waits for the wind-up instruction to authorize the dive.</summary>
    PreparingAttack = 0xc6f7,

    /// <summary><c>$A3:C716</c>: descends six pixels and steers one pixel toward Samus until floor collision.</summary>
    Diving = 0xc716,

    /// <summary><c>$A3:C77F</c>: sinks for the retained 21-update lifetime, emits debris, then deletes the enemy.</summary>
    Burrowing = 0xc77f,
}

/// <summary>
/// Typed debugger view of the five private words overlaid on a Skree's common enemy slot.
/// The wrapper names the native A-E ownership without copying state away from the slot.
/// </summary>
public sealed class SkreeEnemyState
{
    /// <summary>Backing enemy slot whose variable words remain the authoritative Skree state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a typed view over the slot that owns this Skree's mutable variables.</summary>
    /// <param name="slot">Initialized enemy slot whose A-E words are exposed by this state view.</param>
    internal SkreeEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Gets or sets variable A, reloaded to 21 while diving and counted down while burrowing.</summary>
    public ushort BurrowTimer
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Gets or sets variable B, the literal bank-$A3 phase-function pointer.</summary>
    public SkreeEnemyFunction Function
    {
        get => (SkreeEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Gets or sets variable C, the animation phase requested by the current AI transition.</summary>
    public SkreeMetareeAnimationPhase RequestedInstructionIndex
    {
        get => (SkreeMetareeAnimationPhase)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    /// <summary>Gets or sets variable D, the animation phase whose instruction list is currently installed.</summary>
    public SkreeMetareeAnimationPhase InstalledInstructionIndex
    {
        get => (SkreeMetareeAnimationPhase)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Gets or sets variable E, the wind-up instruction's one-shot authorization to begin diving.</summary>
    public bool AttackReady
    {
        get => _slot.VariableE != 0;
        internal set => _slot.VariableE = value ? (ushort)1 : (ushort)0;
    }
}

/// <summary>Literal translation of enemy $DB7F at $A3:C6A4-$C7D4.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Native enemy-definition word used to identify Skree actors during room setup.</summary>
    internal const ushort SkreeDefinition = 0xdb7f;

    /// <summary>Per-slot typed views for Skrees initialized by this enemy system; other slots remain null.</summary>
    private readonly SkreeEnemyState?[] _skreeStates =
        new SkreeEnemyState?[MaximumEnemyCount];

    /// <summary>Seeds the Skree's idle animation and phase function while retaining its live variables in the enemy slot.</summary>
    /// <param name="slot">Room enemy slot being initialized as a Skree.</param>
    private void InitializeSkree(RoomEnemySlot slot)
    {
        var state = new SkreeEnemyState(slot)
        {
            RequestedInstructionIndex = SkreeMetareeAnimationPhase.Idling,
            InstalledInstructionIndex = SkreeMetareeAnimationPhase.Idling,
            AttackReady = false,
            Function = SkreeEnemyFunction.Idling,
        };
        _skreeStates[slot.SlotIndex] = state;
        slot.CurrentInstruction = OrdinaryEnemyInstructionLists.SkreeInitial;
    }

    /// <summary>Dispatches the current Skree phase, starting its wind-up, dive, or timed burrow as the relevant conditions are met.</summary>
    /// <param name="slot">Enemy slot supplying the current actor position and native variables.</param>
    /// <param name="samus">Active actor used for proximity and dive steering; required while Skree AI is running.</param>
    /// <param name="level">Room collision data required when the Skree is diving.</param>
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
                    state.RequestedInstructionIndex =
                        SkreeMetareeAnimationPhase.PreparingAttack;
                    InstallRequestedSkreeInstruction(slot, state);
                    state.Function = SkreeEnemyFunction.PreparingAttack;
                }
                return;

            case SkreeEnemyFunction.PreparingAttack:
                if (!state.AttackReady)
                    return;
                state.AttackReady = false;
                state.RequestedInstructionIndex = SkreeMetareeAnimationPhase.Diving;
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

    /// <summary>Advances the six-pixel descent, steering toward Samus until the downward collision probe starts the burrow phase.</summary>
    /// <param name="slot">Enemy slot whose position, collision flags, and instruction timing are updated.</param>
    /// <param name="state">Typed view used to switch the phase and retain the burrow lifetime.</param>
    /// <param name="samus">Target position that determines horizontal steering.</param>
    /// <param name="level">Room collision map queried by the downward probe.</param>
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

    /// <summary>Counts down the floor-impact lifetime, emits its midpoint particle burst, and deletes the actor at expiry.</summary>
    /// <param name="slot">Enemy slot whose vertical position and deletion state are advanced.</param>
    /// <param name="state">Typed view containing the remaining burrow updates.</param>
    private void RunSkreeBurrow(RoomEnemySlot slot, SkreeEnemyState state)
    {
        state.BurrowTimer = unchecked((ushort)(state.BurrowTimer - 1));
        if (state.BurrowTimer == 0)
        {
            slot.PaletteIndex = EnemyPaletteBits.Palette5;
            slot.VramTilesIndex = 0;
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        if (state.BurrowTimer == 8)
            SpawnSkreeParticleBurst(slot);
        slot.YPosition = unchecked((ushort)(slot.YPosition + 1));
    }

    /// <summary>Installs the requested animation list only when it differs from the currently installed phase.</summary>
    /// <param name="slot">Enemy slot receiving the new instruction list and reset timing fields.</param>
    /// <param name="state">Skree phase state naming the requested and installed animation phases.</param>
    private static void InstallRequestedSkreeInstruction(
        RoomEnemySlot slot,
        SkreeEnemyState state)
    {
        if (state.RequestedInstructionIndex == state.InstalledInstructionIndex)
            return;

        state.InstalledInstructionIndex = state.RequestedInstructionIndex;
        slot.CurrentInstruction = SkreeMetareeAnimationDefinitions.SkreeInstructionList(
            state.RequestedInstructionIndex);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Returns the typed state view associated with a slot or fails when that slot was not initialized as a Skree.</summary>
    /// <param name="slot">Enemy slot whose initialized Skree state is required.</param>
    /// <returns>The state view backed by the slot's native variable words.</returns>
    /// <exception cref="InvalidOperationException">The slot has no registered Skree state.</exception>
    private SkreeEnemyState RequireSkreeState(RoomEnemySlot slot) =>
        _skreeStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Skree state.");
}
