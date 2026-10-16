using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Owtch.Artwork;
namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>The Owtch species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class OwtchBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Owtch];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new OwtchEntity(this, slot, common);

    /// <summary>Supplies the shared room random sequence used for burial and emergence.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Binds Owtch AI to the room's shared random source.</summary>
    internal OwtchBehavior(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>Carries out one cue of Owtch's animations: each sets the walking direction main AI applies.</summary>
    private static OwtchProgram? ApplyOwtchCue(OwtchEnemyState state, OwtchCue cue)
    {
        switch (cue.Kind)
        {
            case OwtchCueKind.SetMovingLeft:
                SetOwtchMovingLeft(state);
                break;
            case OwtchCueKind.SetMovingRight:
                SetOwtchMovingRight(state);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(OwtchCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Owtch sinks sixteen whole pixels below its walking height.</summary>
    private const ushort OwtchMaximumBurialDepth = 16;

    /// <summary>Ports <c>InitAI_Owtch</c> at $A2:A3F9.</summary>
    internal static void InitializeOwtch(RoomEnemySlot slot)
    {
        int initialState = SnesLittleEndian.LowByte(slot.Parameter1);
        int undergroundTimerIndex = SnesLittleEndian.HighByte(slot.Parameter1);
        int speedIndex = SnesLittleEndian.LowByte(slot.Parameter2);
        int distanceIndex = SnesLittleEndian.HighByte(slot.Parameter2);
        if (initialState >= 5)
        {
            throw new InvalidDataException(
                $"Owtch parameter one ${slot.Parameter1:X4} selects state {initialState}, " +
                "outside its five-entry main-AI table.");
        }
        if (speedIndex >= EnemyLinearSpeedDefinitions.RecordCount)
        {
            throw new InvalidDataException(
                $"Owtch parameter two ${slot.Parameter2:X4} selects linear-speed " +
                $"index {speedIndex}, outside the NTSC retail table.");
        }
        ushort undergroundTimer =
            OwtchMovementDefinitions.UndergroundTimer((byte)undergroundTimerIndex);
        ushort travelDistance =
            OwtchMovementDefinitions.TravelDistance((byte)distanceIndex);

        // The initializer masks the raw state only for its two-entry facing table. Thus
        // states two and four begin with the left list, while state three begins right.
        OwtchProgram initialProgram = (initialState & 1) == 0 ? OwtchProgram.MovingLeft : OwtchProgram.MovingRight;

        int speedRecord = speedIndex * EnemyLinearSpeedDefinitions.RecordSize;
        var right = EnemyLinearSpeedDefinitions.Read(speedRecord);
        var left = EnemyLinearSpeedDefinitions.Read(speedRecord + 4);
        var state = new OwtchEnemyState(slot)
        {
            RightVelocity = unchecked((ushort)right.Whole),
            RightSubvelocity = right.Fraction,
            LeftVelocity = unchecked((ushort)left.Whole),
            LeftSubvelocity = left.Fraction,
            Behavior = (OwtchBehaviorState)initialState,
            SinkYOffset = 0,
            UndergroundTimer = NativeCountdown.FromRaw(undergroundTimer),
            MinimumXPosition = unchecked((ushort)(slot.Position.X.Whole - travelDistance)),
            MaximumXPosition = unchecked((ushort)(slot.Position.X.Whole + travelDistance)),
        };
        slot.SetSpeciesState(state);
        SetOwtchProgram(slot, state, initialProgram);

        // State two is the cartridge's special initially-buried form. It moves only the
        // whole coordinate and records the same sixteen-pixel depth used by normal sinking.
        if (state.Behavior == OwtchBehaviorState.Underground)
        {
            state.SinkYOffset = OwtchMaximumBurialDepth;
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + OwtchMaximumBurialDepth)));
        }
    }

    /// <summary>Ports <c>MainAI_Owtch</c> and its indirect state table at $A2:A47E.</summary>
    private void RunOwtchMain(RoomEnemySlot slot, OwtchEnemyState state)
    {
        switch (state.Behavior)
        {
            case OwtchBehaviorState.MovingLeft:
                MoveOwtch(slot, state, movingRight: false);
                MaybeMakeOwtchSink(slot, state);
                return;

            case OwtchBehaviorState.MovingRight:
                MoveOwtch(slot, state, movingRight: true);
                MaybeMakeOwtchSink(slot, state);
                return;

            case OwtchBehaviorState.Underground:
                state.UndergroundTimer = state.UndergroundTimer.Decrement().Remaining;
                if (state.UndergroundTimer.IsZero)
                    state.Behavior = OwtchBehaviorState.Rising;
                return;

            case OwtchBehaviorState.Sinking:
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 1)));
                state.SinkYOffset = unchecked((ushort)(state.SinkYOffset + 1));
                if (unchecked((short)(state.SinkYOffset - OwtchMaximumBurialDepth)) >= 0)
                {
                    state.Behavior = OwtchBehaviorState.Underground;
                    state.UndergroundTimer = NativeCountdown.FromRaw(OwtchMovementDefinitions.UndergroundTimer(
                        SnesLittleEndian.HighByte(slot.Parameter1)));
                }
                return;

            case OwtchBehaviorState.Rising:
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - 1)));
                state.SinkYOffset = NativeWordCounter.Decrement(state.SinkYOffset).Value;
                if (state.SinkYOffset == 0)
                {
                    // $A2:A549 samples RandomNumberSeed+1 without advancing the RNG. The
                    // gameplay runtime supplies that read seam; isolated callers without it
                    // retain the established advancing fallback used by other translated AI.
                    ushort random = _room.Random.Read();
                    state.Behavior = (random & OwtchMovementDefinitions.BehaviorRandomBit) == 0
                        ? OwtchBehaviorState.MovingLeft
                        : OwtchBehaviorState.MovingRight;
                }
                return;

            case OwtchBehaviorState.LeftBoundaryUnderflow:
                // The moving-left routine decrements zero instead of incrementing it. On
                // the next frame, direction $FFFF indexes two bytes before the function
                // table and accidentally calls the right-list initializer at $A2:A49D.
                SetOwtchProgram(slot, state, OwtchProgram.MovingRight);
                return;

            default:
                throw new InvalidDataException(
                    $"Owtch state/table index ${(ushort)state.Behavior:X4} is not translated.");
        }
    }

    /// <summary>Ports the carry-sensitive additions at $A2:A4B0/$A4D9.</summary>
    private static void MoveOwtch(
        RoomEnemySlot slot,
        OwtchEnemyState state,
        bool movingRight)
    {
        ushort subvelocity = movingRight
            ? state.RightSubvelocity
            : state.LeftSubvelocity;
        ushort velocity = movingRight
            ? state.RightVelocity
            : state.LeftVelocity;

        uint fractionalSum = (uint)slot.Position.X.Fraction + subvelocity;
        slot.Position = slot.Position.WithFractionX(unchecked((ushort)fractionalSum));
        if (fractionalSum > ushort.MaxValue)
            slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + 1)));
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + velocity)));

        if (!movingRight)
        {
            if (!EnemyMath.IsNegative16(unchecked((ushort)(
                    slot.Position.X.Whole - state.MinimumXPosition))))
            {
                return;
            }

            // This DEC is an original-game bug. Zero wraps to $FFFF; main AI then indexes
            // the preceding initializer pointer rather than a declared function entry.
            state.Behavior = OwtchBehaviorState.LeftBoundaryUnderflow;
            return;
        }

        if (!EnemyMath.IsNegative16(unchecked((ushort)(
                slot.Position.X.Whole - state.MaximumXPosition))))
        {
            state.Behavior = OwtchBehaviorState.MovingLeft;
        }
    }

    /// <summary>Ports Owtch's six-in-256 per-frame burial gate at $A2:A553.</summary>
    private void MaybeMakeOwtchSink(RoomEnemySlot slot, OwtchEnemyState state)
    {
        ushort random = _room.Random.Generate();
        byte decision = unchecked((byte)((byte)random + (byte)slot.FrameCounter));
        if (decision < 6)
            state.Behavior = OwtchBehaviorState.Sinking;
    }

    /// <summary>Animation command $A2:A56D: select the moving-left function index.</summary>
    private static void SetOwtchMovingLeft(OwtchEnemyState state) =>
        state.Behavior = OwtchBehaviorState.MovingLeft;

    /// <summary>Animation command $A2:A571: select the moving-right function index.</summary>
    private static void SetOwtchMovingRight(OwtchEnemyState state) =>
        state.Behavior = OwtchBehaviorState.MovingRight;

    /// <summary>
    /// Enemy-shot callback $A2:A579 calls common damage only when signed(state - 1) is
    /// negative. In shipped states that means moving left (plus the transient $FFFF bug).
    /// </summary>
    private static bool OwtchAcceptsOrdinaryShot(OwtchEnemyState state) =>
        unchecked((short)((ushort)state.Behavior - 1)) < 0;

    /// <summary>Restarts the selected facing animation at its first step.</summary>
    private static void SetOwtchProgram(RoomEnemySlot slot, OwtchEnemyState state, OwtchProgram program)
    {
        state.Program = AnimationProgramCursor<OwtchProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Rejects Owtch AI or collision callbacks before slot initialization.</summary>
    internal static OwtchEnemyState RequireOwtchState(RoomEnemySlot slot) =>
        slot.SpeciesState<OwtchEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Owtch state.");

    /// <summary>Allows common bomb damage only while Owtch occupies vulnerable movement state zero.</summary>
    internal static void RegisterOwtchBombReactions(EnemySharedReactions table)
    {
        // Owtch's shell accepts common damage only during its vulnerable state zero.
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Owtch &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.OwtchShot)
        {
            Rejects = c => !OwtchAcceptsOrdinaryShot(RequireOwtchState(c.Enemy)),
        });
    }

    /// <summary>Ports Owtch's initialization AI.</summary>
    internal static void RunOwtchInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeOwtch(slot);
    }

    /// <summary>Ports Owtch's main AI.</summary>
    internal void RunOwtchMainAi(RoomEnemySlot slot, IEnemyAiFrame _, byte _1)
    {
        RunOwtchMain(slot, RequireOwtchState(slot));
    }

    /// <summary>Advances Owtch's typed animation.</summary>
    internal static void AdvanceOwtchTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireOwtchState(slot);
        AnimationProgramCursor<OwtchProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, OwtchAnimations.Programs, ref cursor, OwtchFrameDefinitions.Spritemap,
                (cue, _) => ApplyOwtchCue(state, cue));
        state.Program = cursor;
    }

    /// <summary>Preserves collision marking while rejecting ordinary shots against Owtch's shell.</summary>
    internal static void RegisterOwtchShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Owtch &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.OwtchShot)
        {
            // Owtch's private $A2:A579 callback returns before common shot AI unless signed(state - 1) is negative.
            // The bank-$A0 collision prelude has already marked ordinary non-plasma shots as collided at this point,
            // but it has not converted them into an impact animation. Preserve that split so the shell is genuinely
            // immune while moving right, sinking, buried, or rising.
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (OwtchAcceptsOrdinaryShot(RequireOwtchState(c.Enemy)))
                    return false;
                if (c.MarksCollision)
                    c.Projectile.Direction = unchecked((ushort)(c.Projectile.Direction | SamusProjectileDirectionMasks.Spent));
                return true;
            },
        });
    }
}
