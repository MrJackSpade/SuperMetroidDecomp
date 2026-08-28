namespace SuperMetroid.Core.Game;

/// <summary>
/// Exact bank-$A2 function words stored in variable F by the Norfair lava-jumping enemy.
/// The retail source is commonly labelled <c>NorfairLavajumpingEnemy</c>; the ROM name
/// record spells the internal name <c>SQUEEWPT</c>.
/// </summary>
public enum NorfairLavaJumpingEnemyFunction : ushort
{
    FollowParent = 0xbedc,
    BeginJump = 0xbf1a,
    RiseBeforeAnimationSwitch = 0xbf3e,
    MoveUntilAnimationSignal = 0xbf7c,
    FallBackIntoLava = 0xbfbc,
}

/// <summary>
/// Typed projection of the common C-F enemy words and the family's two extra-RAM words used
/// by $A2:BE99-$C02B. Common values remain in the physical slot; only the two words which
/// genuinely live in family-extra WRAM receive host fields.
/// </summary>
public sealed class NorfairLavaJumpingEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal NorfairLavaJumpingEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Native family-extra variable $00. Instruction $BE8E sets this handshake after the
    /// jump animation reaches the frame where main AI may begin the final falling phase.
    /// It is deliberately host-backed: this word lives outside the six common A-F words.
    /// </summary>
    public bool AnimationFinished { get; internal set; }

    /// <summary>
    /// Native family-extra variable $01. Function six remembers the last installed list so
    /// it does not restart an animation which is already running. Like variable $00, this
    /// is separate from the ordinary A-F words and therefore has explicit host storage.
    /// </summary>
    public ushort InstalledInstructionList { get; internal set; }

    /// <summary>Signed 8.8 vertical velocity, stored in native enemy variable C.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Parent variant's immutable population X coordinate.</summary>
    public ushort SpawnX
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Parent variant's immutable population Y coordinate at the lava surface.</summary>
    public ushort SpawnY
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public NorfairLavaJumpingEnemyFunction Function
    {
        get => (NorfairLavaJumpingEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>
    /// Population parameter-one bit 15 selects the visual follower half. It is not a
    /// freely combinable runtime flag, so expose it as a predicate rather than an enum.
    /// </summary>
    public bool IsFollower => (_slot.Parameter1 & 0x8000) != 0;
}

/// <summary>Literal translation of enemy AI $A2:BE8E-$C02B.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort NorfairLavaJumpingEnemyDefinition = 0xd2bf;

    private const ushort NorfairLavaJumpingEnemyHiddenInstructionList = 0xbe3c;
    private const ushort NorfairLavaJumpingEnemyJumpInstructionList = 0xbe42;
    private const ushort NorfairLavaJumpingEnemyFollowerInstructionList = 0xbe62;
    private const ushort NorfairLavaJumpingEnemyAnimationSignalInstruction = 0xbe8e;
    private const ushort NorfairLavaJumpingEnemyGravity = 56;
    private const ushort NorfairLavaJumpingEnemyAnimationSwitchVelocity = 0xfc00;
    private const ushort NorfairLavaJumpingEnemyJumpSound = 0x000d;
    private const int NorfairLavaJumpingEnemyVelocityTableAddress = 0xa2be86;

    private readonly NorfairLavaJumpingEnemyState?[] _norfairLavaJumpingEnemyStates =
        new NorfairLavaJumpingEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical slot currently owned by this family.</summary>
    public IReadOnlyList<NorfairLavaJumpingEnemyState?> NorfairLavaJumpingEnemyStates =>
        _norfairLavaJumpingEnemyStates;

    /// <summary>Most recent library-two jump sound request emitted during this frame.</summary>
    public ushort? LastNorfairLavaJumpingEnemySoundEffect { get; private set; }

    private void ResetNorfairLavaJumpingEnemyRoomState()
    {
        Array.Clear(_norfairLavaJumpingEnemyStates);
        LastNorfairLavaJumpingEnemySoundEffect = null;
    }

    /// <summary>Ports <c>NorfairLavajumpingEnemy_Init</c> at $A2:BE99.</summary>
    private void InitializeNorfairLavaJumpingEnemy(RoomEnemySlot slot)
    {
        var state = new NorfairLavaJumpingEnemyState(slot)
        {
            AnimationFinished = false,
            InstalledInstructionList = 0,
        };
        _norfairLavaJumpingEnemyStates[slot.SlotIndex] = state;

        if (state.IsFollower)
        {
            // The retail population always places this cosmetic second half immediately
            // after its moving parent. Its main AI relies on WRAM aliasing across those two
            // physical 64-byte records, so reject reordered/custom populations explicitly.
            ValidateNorfairLavaJumpingEnemyParent(slot);
            slot.CurrentInstruction = NorfairLavaJumpingEnemyFollowerInstructionList;
            state.Function = NorfairLavaJumpingEnemyFunction.FollowParent;
            return;
        }

        // The jumping parent returns to exactly these population coordinates after every
        // arc. Subpositions are deliberately not saved or cleared by the retail routine.
        state.SpawnX = slot.XPosition;
        state.SpawnY = slot.YPosition;
        slot.CurrentInstruction = NorfairLavaJumpingEnemyHiddenInstructionList;
        state.Function = NorfairLavaJumpingEnemyFunction.BeginJump;
    }

    /// <summary>
    /// Ports <c>NorfairLavajumpingEnemy_Main</c> and functions one through five at
    /// $A2:BED2-$C011. Main advances the shared enemy RNG once for every parent and follower
    /// frame; even the follower discards the word, which still affects later room actors.
    /// </summary>
    private void RunNorfairLavaJumpingEnemyMain(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state)
    {
        ushort random = _nextRandom!();

        switch (state.Function)
        {
            case NorfairLavaJumpingEnemyFunction.FollowParent:
                FollowNorfairLavaJumpingEnemyParent(slot);
                return;

            case NorfairLavaJumpingEnemyFunction.BeginJump:
                // HIBYTE(random) & 6 is already the byte offset into the four-word table.
                // Do not divide it again when constructing the SNES address.
                int velocityAddress = NorfairLavaJumpingEnemyVelocityTableAddress +
                    ((random >> 8) & 0x0006);
                state.YVelocity = ReadWord(_bus!, velocityAddress);
                state.Function = NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch;
                slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                LastNorfairLavaJumpingEnemySoundEffect = NorfairLavaJumpingEnemyJumpSound;
                return;

            case NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch:
                MoveNorfairLavaJumpingEnemyVertically(slot, state);
                state.YVelocity = unchecked((ushort)(state.YVelocity + NorfairLavaJumpingEnemyGravity));

                // The 65816 CMP is unsigned. Starting jump velocities are near $F800; the
                // test becomes true while still negative at $FC00, before the arc apex.
                if (state.YVelocity >= NorfairLavaJumpingEnemyAnimationSwitchVelocity)
                {
                    InstallNorfairLavaJumpingEnemyInstructionList(
                        slot,
                        state,
                        NorfairLavaJumpingEnemyJumpInstructionList);
                    state.Function = NorfairLavaJumpingEnemyFunction.MoveUntilAnimationSignal;
                }
                return;

            case NorfairLavaJumpingEnemyFunction.MoveUntilAnimationSignal:
                MoveNorfairLavaJumpingEnemyVertically(slot, state);
                state.YVelocity = unchecked((ushort)(state.YVelocity + NorfairLavaJumpingEnemyGravity));
                if (state.AnimationFinished)
                {
                    state.AnimationFinished = false;
                    state.Function = NorfairLavaJumpingEnemyFunction.FallBackIntoLava;
                }
                return;

            case NorfairLavaJumpingEnemyFunction.FallBackIntoLava:
                MoveNorfairLavaJumpingEnemyVertically(slot, state);

                // The low screen-row nibble is the retail lava-entry sentinel. Once the
                // actor reaches row $xF0 it snaps to its exact population origin, restores
                // the hidden list, and sleeps off-screen until the next jump begins.
                if ((slot.YPosition & 0x00f0) == 0x00f0)
                {
                    slot.XPosition = state.SpawnX;
                    slot.YPosition = state.SpawnY;
                    InstallNorfairLavaJumpingEnemyInstructionList(
                        slot,
                        state,
                        NorfairLavaJumpingEnemyHiddenInstructionList);
                    state.Function = NorfairLavaJumpingEnemyFunction.BeginJump;
                    slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                }
                else
                {
                    state.YVelocity = unchecked((ushort)(state.YVelocity + NorfairLavaJumpingEnemyGravity));
                }
                return;

            default:
                throw new NotSupportedException(
                    $"Norfair lava-jumping enemy function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the private no-operand animation handshake at $A2:BE8E.</summary>
    private bool TryProcessNorfairLavaJumpingEnemyInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != NorfairLavaJumpingEnemyDefinition ||
            opcode != NorfairLavaJumpingEnemyAnimationSignalInstruction)
        {
            return false;
        }

        RequireNorfairLavaJumpingEnemyState(slot).AnimationFinished = true;
        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    private void FollowNorfairLavaJumpingEnemyParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairLavaJumpingEnemySlot(follower);

        // These four reads look nonsensical in the disassembly because the routine indexes
        // enemy_drawing_queue with offsets +100/+109/+2/+93. For a follower one slot after
        // its parent, those WRAM addresses alias the preceding EnemyData record exactly:
        // health, frozen timer, variable C (Y velocity), and Y position respectively.
        if (parent.Health == 0)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Deleted);
            return;
        }

        // A live source must still be the paired parent. A dead parent's definition can be
        // cleared by DetermineWhichEnemiesToProcess before the follower gets its last AI
        // frame, which is why the health-zero exit deliberately precedes this validation.
        ValidateNorfairLavaJumpingEnemyParent(follower);

        follower.FrozenTimer = parent.FrozenTimer;
        if (parent.FrozenTimer != 0 || (parent.VariableC & 0x8000) == 0)
        {
            // The second half appears only while the parent is travelling upward. At the
            // apex and during descent, the parent animation alone owns the visible shape.
            follower.Properties = follower.Properties.With(EnemyProperties.Invisible);
            return;
        }

        follower.Properties = follower.Properties.Without(EnemyProperties.Invisible);
        follower.YPosition = parent.YPosition;
    }

    private RoomEnemySlot GetPrecedingNorfairLavaJumpingEnemySlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
        {
            throw new InvalidDataException(
                "A Norfair lava-jumping follower cannot occupy enemy slot zero.");
        }

        return _slots[follower.SlotIndex - 1];
    }

    private void ValidateNorfairLavaJumpingEnemyParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairLavaJumpingEnemySlot(follower);
        if (parent.EnemyDefinitionPointer != NorfairLavaJumpingEnemyDefinition ||
            (parent.Parameter1 & 0x8000) != 0)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping follower in slot {follower.SlotIndex} must immediately follow " +
                "a parent record of the same definition.");
        }
    }

    private static void MoveNorfairLavaJumpingEnemyVertically(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state)
    {
        (slot.YPosition, slot.YSubposition) = AddEightBitVelocity(
            slot.YPosition,
            slot.YSubposition,
            state.YVelocity);
    }

    /// <summary>Ports function six at $A2:C012 without restarting an unchanged list.</summary>
    private static void InstallNorfairLavaJumpingEnemyInstructionList(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state,
        ushort instructionList)
    {
        if (state.InstalledInstructionList == instructionList)
            return;

        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private NorfairLavaJumpingEnemyState RequireNorfairLavaJumpingEnemyState(RoomEnemySlot slot) =>
        _norfairLavaJumpingEnemyStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Norfair lava-jumping state.");
}
