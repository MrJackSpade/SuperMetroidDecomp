using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.LavaJumpers.Artwork;
namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>The Norfair lava-jumping enemy species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class NorfairLavaJumpingEnemyBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Squeept];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new NorfairLavaJumpingEnemyEntity(this, slot, common);

    /// <summary>Supplies one random word per parent and follower AI frame.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Provides the preceding physical enemy record for follower WRAM aliases.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Binds lava-jumper behavior to room RNG and physical enemy slots.</summary>
    internal NorfairLavaJumpingEnemyBehavior(EnemyRoomBinding room, EnemySlotTable slotTable)
    {
        _room = room;
        _slotTable = slotTable;
    }

    /// <summary>8.8 velocity increment applied after each moving frame.</summary>
    private const ushort NorfairLavaJumpingEnemyGravity = 56;
    /// <summary>Unsigned rising-velocity threshold that switches to the visible jump list.</summary>
    private const ushort NorfairLavaJumpingEnemyAnimationSwitchVelocity = unchecked((ushort)-1024);

    /// <summary>Most recent library-two jump sound request emitted during this frame.</summary>
    public SoundEffectId? LastNorfairLavaJumpingEnemySoundEffect { get; private set; }

    /// <summary>Ports <c>NorfairLavajumpingEnemy_Init</c> at $A2:BE99.</summary>
    internal void InitializeNorfairLavaJumpingEnemy(RoomEnemySlot slot)
    {
        var state = new NorfairLavaJumpingEnemyState
        {
            AnimationFinished = false,
            InstalledProgram = null,
            IsFollower = (slot.Spawn.Population.Parameter1 & EnemyParameterMasks.FollowerBit) != 0,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        if (state.IsFollower)
        {
            // The retail population always places this cosmetic second half immediately
            // after its moving parent. Its main AI relies on WRAM aliasing across those two
            // physical 64-byte records, so reject reordered/custom populations explicitly.
            ValidateNorfairLavaJumpingEnemyParent(slot);
            state.Program = AnimationProgramCursor<NorfairLavaJumperProgram>.Start(NorfairLavaJumperProgram.Follower);
            state.Function = NorfairLavaJumpingEnemyFunction.FollowParent;
            return;
        }

        // The jumping parent returns to exactly these population coordinates after every
        // arc. Subpositions are deliberately not saved or cleared by the retail routine.
        state.SpawnPosition = slot.Position.Whole;
        state.Program = AnimationProgramCursor<NorfairLavaJumperProgram>.Start(NorfairLavaJumperProgram.Hidden);
        state.Function = NorfairLavaJumpingEnemyFunction.BeginJump;
    }

    /// <summary>
    /// Ports <c>NorfairLavajumpingEnemy_Main</c> and functions one through five at
    /// $A2:BED2-$C011. Main advances the shared enemy RNG once for every parent and follower
    /// frame; even the follower discards the word, which still affects later room actors.
    /// </summary>
    internal void RunNorfairLavaJumpingEnemyMain(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state)
    {
        int random = _room.Random.Generate();

        switch (state.Function)
        {
            case NorfairLavaJumpingEnemyFunction.FollowParent:
                FollowNorfairLavaJumpingEnemyParent(slot);
                return;

            case NorfairLavaJumpingEnemyFunction.BeginJump:
                state.YVelocity = new SnesSignedEightEight(NorfairLavaJumpDefinitions.InitialVerticalVelocity(random));
                state.Function = NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch;
                slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                LastNorfairLavaJumpingEnemySoundEffect = EnemySoundEffectDefinitions.NorfairLavaJumpingEnemyJumpSound;
                return;

            case NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch:
                MoveNorfairLavaJumpingEnemyVertically(slot, state);
                state.ApplyGravity(NorfairLavaJumpingEnemyGravity);

                // The 65816 CMP is unsigned. Starting jump velocities are near $F800; the
                // test becomes true while still negative at $FC00, before the arc apex.
                if (state.YVelocity.RawValue >= NorfairLavaJumpingEnemyAnimationSwitchVelocity)
                {
                    InstallNorfairLavaJumpingEnemyProgram(slot, state, NorfairLavaJumperProgram.Jump);
                    state.Function = NorfairLavaJumpingEnemyFunction.MoveUntilAnimationSignal;
                }
                return;

            case NorfairLavaJumpingEnemyFunction.MoveUntilAnimationSignal:
                MoveNorfairLavaJumpingEnemyVertically(slot, state);
                state.ApplyGravity(NorfairLavaJumpingEnemyGravity);
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
                if ((slot.Position.Y.Whole & RoomPixelMasks.BlockRowWithinScreenBits) == 240)
                {
                    slot.Position = slot.Position.WithWhole(state.SpawnPosition);
                    InstallNorfairLavaJumpingEnemyProgram(slot, state, NorfairLavaJumperProgram.Hidden);
                    state.Function = NorfairLavaJumpingEnemyFunction.BeginJump;
                    slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                }
                else
                {
                    state.ApplyGravity(NorfairLavaJumpingEnemyGravity);
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Norfair lava-jumping enemy function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Carries out the jump animation's finishing cue, the private handshake at $A2:BE8E.</summary>
    private static NorfairLavaJumperProgram? ApplyNorfairLavaJumperCue(NorfairLavaJumpingEnemyState state, NorfairLavaJumperCue cue)
    {
        if (cue.Kind != NorfairLavaJumperCueKind.FinishAnimation)
            throw new InvalidOperationException($"Undefined {nameof(NorfairLavaJumperCueKind)} {cue.Kind}.");
        state.AnimationFinished = true;
        return null;
    }

    /// <summary>Mirrors the preceding parent's frozen timer and upward Y position into the cosmetic follower.</summary>
    private void FollowNorfairLavaJumpingEnemyParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairLavaJumpingEnemySlot(follower);

        // These four reads look nonsensical in the disassembly because the routine indexes
        // enemy_drawing_queue with offsets +100/+109/+2/+93. For a follower one slot after
        // its parent, those WRAM addresses alias the preceding enemy record exactly:
        // health, frozen timer, the parent's Y velocity, and Y position respectively.
        if (parent.Health == 0)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Deleted);
            return;
        }

        // Pair identity is checked at population initialization, not on every AI frame.
        // The native flame routine reads the preceding physical record even after its
        // header has been cleared: script deletion does not zero health or movement words.
        // Requiring a live definition here rejects valid retained-WRAM reads (#322).

        follower.FrozenTimer = parent.FrozenTimer;
        NorfairLavaJumpingEnemyState parentState = parent.SpeciesState<NorfairLavaJumpingEnemyState>() ??
            throw new InvalidDataException($"The Norfair lava-jumping follower in slot {follower.SlotIndex} has no parent state.");
        if (!parent.FrozenTimer.IsZero || parentState.YVelocity.SignedRawValue >= 0)
        {
            // The second half appears only while the parent is travelling upward. At the
            // apex and during descent, the parent animation alone owns the visible shape.
            follower.Properties = follower.Properties.With(EnemyProperties.Invisible);
            return;
        }

        follower.Properties = follower.Properties.Without(EnemyProperties.Invisible);
        follower.Position = follower.Position.WithWholeY(parent.Position.Y.Whole);
    }

    /// <summary>Returns the immediately preceding physical enemy record used by native follower aliasing.</summary>
    private RoomEnemySlot GetPrecedingNorfairLavaJumpingEnemySlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
        {
            throw new InvalidDataException(
                "A Norfair lava-jumping follower cannot occupy enemy slot zero.");
        }

        return _slotTable.Slots[follower.SlotIndex - 1];
    }

    /// <summary>Rejects a follower population record without a same-species parent directly before it.</summary>
    private void ValidateNorfairLavaJumpingEnemyParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairLavaJumpingEnemySlot(follower);
        if (parent.EnemyDefinitionPointer != EnemyDefinitionId.Squeept ||
            (parent.Spawn.Population.Parameter1 & EnemyParameterMasks.FollowerBit) != 0)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping follower in slot {follower.SlotIndex} must immediately follow " +
                "a parent record of the same definition. " +
                $"Preceding slot: definition=${(int)parent.EnemyDefinitionPointer:X4}, " +
                $"health=${parent.Health:X4}, parameter=${parent.Spawn.Population.Parameter1:X4}, properties={parent.Properties}.");
        }
    }

    /// <summary>Advances position by the parent's signed 8.8 vertical velocity.</summary>
    private static void MoveNorfairLavaJumpingEnemyVertically(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state)
    {
        slot.Position = slot.Position.AdvanceY(state.YVelocity.ToSixteenSixteenDelta());
    }

    /// <summary>Ports function six at $A2:C012 without restarting an unchanged list.</summary>
    private static void InstallNorfairLavaJumpingEnemyProgram(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state,
        NorfairLavaJumperProgram program) =>
        EnemyProgramAnimation.InstallOnce(slot, state.InstalledProgram, program, installed =>
        {
            state.InstalledProgram = installed;
            state.Program = AnimationProgramCursor<NorfairLavaJumperProgram>.Start(installed);
        });

    /// <summary>Ports NorfairLavaJumpingEnemy's initialization AI.</summary>
    internal void RunNorfairLavaJumpingEnemyInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeNorfairLavaJumpingEnemy(slot);
    }

    /// <summary>Ports NorfairLavaJumpingEnemy's main AI.</summary>
    internal void RunNorfairLavaJumpingEnemyMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunNorfairLavaJumpingEnemyMain(
            slot,
            slot.RequireSpeciesState<NorfairLavaJumpingEnemyState>());
    }

    /// <summary>Advances NorfairLavaJumpingEnemy's typed animation.</summary>
    internal static void AdvanceNorfairLavaJumpingEnemyTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, NorfairLavaJumperAnimations.Programs, NorfairLavaJumperFrameDefinitions.ShownFrames.Of,
            static (NorfairLavaJumpingEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyNorfairLavaJumperCue(state, cue));

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastNorfairLavaJumpingEnemySoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueEnemySoundIfAny(LastNorfairLavaJumpingEnemySoundEffect, maximumQueued: 6);
    }
}
