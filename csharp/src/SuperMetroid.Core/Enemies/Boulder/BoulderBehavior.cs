using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Boulder.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>The Boulder species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BoulderBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Boulder];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BoulderEntity(this, slot, common);

    /// <summary>Moves the actor through room collision during fall and roll phases.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Creates the dust explosion when a rolling or brittle boulder breaks.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Connects terrain collision and the shared projectile engine for Boulder AI.</summary>
    internal BoulderBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles)
    {
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>Room-graphics dust animation selected by terminal impacts.</summary>
    private const MiscDustAnimation BoulderDustAnimationIndex = MiscDustAnimation.RockParticles;

    /// <summary>Most recent library-two sound emitted by a landing or terminal impact.</summary>
    public ushort? LastBoulderSoundEffect { get; private set; }

    /// <summary>Interprets both spawn parameters as direction, trigger windows, vertical compensation, and initial height; a zero offset starts rolling without bounces.</summary>
    internal static void InitializeBoulder(RoomEnemySlot slot)
    {
        var state = new BoulderEnemyState(slot);
        slot.SetSpeciesState(state);

        state.BounceSpeedAccumulator = 0;
        state.TravelSpeedAccumulator = 0;
        state.BounceCounter = NativeCountdown.Start(2);
        state.Function = BoulderAiFunction.WaitingForSamus;
        state.InitialPosition = slot.Position.Whole;
        state.PreviousYPosition = slot.Position.Y.Whole;
        state.PreviousYSubposition = slot.Position.Y.Fraction;

        byte initialYOffset = SnesLittleEndian.HighByte(slot.Parameter2);
        if (initialYOffset == 0)
        {
            // A zero high byte is not “no offset.” Native substitutes one pixel and sets
            // var07 so the trigger jumps directly to rolling instead of the bounce chain.
            initialYOffset = 1;
            state.StartsRollingWithoutBounce = true;
        }
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(state.InitialPosition.Y - initialYOffset)));

        state.HorizontalTriggerLimit = unchecked((short)-(byte)slot.Parameter2);

        // The population record's initialization parameter was copied to current_instruction
        // before init AI ran. Boulder consumes only its low byte as the strict Samus-Y
        // trigger threshold, then starts the real rolling animation in its place.
        state.VerticalTriggerLimit = unchecked((byte)slot.CurrentInstruction);
        state.DirectionSelector = SnesLittleEndian.HighByte(slot.Parameter1);
        if (state.DirectionSelector == 0)
        {
            state.HorizontalTriggerLimit = unchecked((short)-state.HorizontalTriggerLimit);
            state.Program = AnimationProgramCursor<BoulderProgram>.Start(BoulderProgram.Right);
        }
        else
        {
            state.Program = AnimationProgramCursor<BoulderProgram>.Start(BoulderProgram.Left);
        }

        state.VerticalCompensation = (byte)slot.Parameter1 == 0 ? (ushort)2 : (ushort)5;
    }

    /// <summary>Dispatches wait, fall, rebound, roll, and inert phases with required Samus and terrain state.</summary>
    private void RunBoulderMain(RoomEnemySlot slot, BoulderEnemyState state, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Boulder AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Boulder AI requires active room collision data.");

        switch (state.Function)
        {
            case BoulderAiFunction.WaitingForSamus:
                RunBoulderWaiting(slot, state, samus);
                break;
            case BoulderAiFunction.InitialFall:
                RunBoulderInitialFall(slot, state);
                break;
            case BoulderAiFunction.Rebound:
                RunBoulderRebound(slot, state);
                break;
            case BoulderAiFunction.Falling:
                RunBoulderFalling(slot, state, level);
                break;
            case BoulderAiFunction.Rolling:
                RunBoulderRolling(slot, state, level);
                break;
            case BoulderAiFunction.Inert:
                break;
            default:
                throw new InvalidDataException(
                    $"Boulder function $A6:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Starts the bounce chain or direct roll only when Samus enters the direction-specific X window below the boulder.</summary>
    private static void RunBoulderWaiting(RoomEnemySlot slot, BoulderEnemyState state, SamusState samus)
    {
        short deltaY = unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole));
        if (deltaY < 0 || unchecked((short)(deltaY - state.VerticalTriggerLimit)) >= 0)
            return;

        short deltaX = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole));
        bool enteredHorizontalWindow = state.DirectionSelector != 0
            ? deltaX < 0 && unchecked((short)(deltaX - state.HorizontalTriggerLimit)) >= 0
            : deltaX >= 0 && unchecked((short)(deltaX - state.HorizontalTriggerLimit)) < 0;
        if (!enteredHorizontalWindow)
            return;

        state.Function = state.StartsRollingWithoutBounce
            ? BoulderAiFunction.Rolling
            : BoulderAiFunction.InitialFall;
    }

    /// <summary>Accelerates down toward the recorded spawn height, then clamps and starts the first rebound.</summary>
    private static void RunBoulderInitialFall(RoomEnemySlot slot, BoulderEnemyState state)
    {
        AddBoulderVerticalVelocity(slot, state.BounceSpeedAccumulator, negative: false);
        if (unchecked((short)(slot.Position.Y.Whole - state.InitialPosition.Y)) < 0)
        {
            state.BounceSpeedAccumulator = AddAndCapUnsigned(
                state.BounceSpeedAccumulator,
                256,
                20480);
            return;
        }

        slot.Position = slot.Position.WithWholeY(state.InitialPosition.Y);
        state.Function = BoulderAiFunction.Rebound;
        state.BounceSpeedAccumulator = 8192;
    }

    /// <summary>Applies upward quadratic motion while reducing bounce speed, adding sideways travel, then returns to falling.</summary>
    private static void RunBoulderRebound(RoomEnemySlot slot, BoulderEnemyState state)
    {
        AddBoulderVerticalVelocity(slot, state.BounceSpeedAccumulator, negative: true);
        state.BounceSpeedAccumulator = unchecked((ushort)(state.BounceSpeedAccumulator - 256));
        if (unchecked((short)state.BounceSpeedAccumulator) < 0)
        {
            state.BounceSpeedAccumulator = 0;
            state.Function = BoulderAiFunction.Falling;
            return;
        }

        AddBoulderHorizontalVelocity(slot, state);
        state.TravelSpeedAccumulator = AddAndCapUnsigned(
            state.TravelSpeedAccumulator,
            32,
            20480);
    }

    /// <summary>Accelerates through terrain contact; ordinary impacts consume bounce indexes while selector two breaks immediately.</summary>
    internal void RunBoulderFalling(RoomEnemySlot slot, BoulderEnemyState state, RoomLevelData level)
    {
        int verticalDisplacement = EnemyMath.ReadQuadraticEnemySpeed(
            unchecked((byte)(SnesEightEight.WholeOf(state.BounceSpeedAccumulator))),
            negative: false);
        if (_terrain.MoveEnemyVertically(level, slot, verticalDisplacement))
        {
            LastBoulderSoundEffect = EnemySoundEffectDefinitions.BoulderImpactSound;
            if (state.DirectionSelector == 2)
            {
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                _projectiles.SpawnRoomGraphicsDustExplosion(
                    slot.Position.Whole,
                    BoulderDustAnimationIndex);
                LastBoulderSoundEffect = EnemySoundEffectDefinitions.BoulderBreakSound;
                return;
            }

            state.Function = BoulderAiFunction.Rebound;

            // The third impact still installs the leading zero sample before the counter
            // underflows. Preserve that write even though this impact enters rolling.
            state.BounceSpeedAccumulator = BoulderBounceDefinitions.SpeedIndex(state.BounceCounter.RawValue);
            state.BounceCounter = state.BounceCounter.Decrement().Remaining;
            if (state.BounceCounter.IsNegative)
            {
                state.PreviousYPosition = slot.Position.Y.Whole;
                state.PreviousYSubposition = slot.Position.Y.Fraction;
                state.Function = BoulderAiFunction.Rolling;
            }
            return;
        }

        state.BounceSpeedAccumulator = unchecked((ushort)(
            state.BounceSpeedAccumulator + 256));
        AddBoulderHorizontalVelocity(slot, state);
        state.TravelSpeedAccumulator = AddAndCapUnsigned(
            state.TravelSpeedAccumulator,
            32,
            20480);
    }

    /// <summary>Tracks the floor with vertical compensation and advances horizontally until a wall hides/deletes the actor and spawns dust.</summary>
    private void RunBoulderRolling(RoomEnemySlot slot, BoulderEnemyState state, RoomLevelData level)
    {
        int verticalDisplacement = unchecked(
            EnemyMath.ReadQuadraticEnemySpeed(
                unchecked((byte)(SnesEightEight.WholeOf(state.TravelSpeedAccumulator))),
                negative: false) +
            SnesSixteenSixteen.FromWhole(state.VerticalCompensation));
        _terrain.MoveEnemyVertically(level, slot, verticalDisplacement);
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - state.VerticalCompensation)));

        int horizontalDisplacement = ReadBoulderHorizontalDisplacement(state);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            // Native ORA #$0300 both hides and deletes the record. The inert function is
            // still written before DetermineWhichEnemiesToProcess clears it next frame.
            slot.Properties = slot.Properties.With(
                EnemyProperties.Invisible | EnemyProperties.Deleted);
            state.Function = BoulderAiFunction.Inert;
            LastBoulderSoundEffect = EnemySoundEffectDefinitions.BoulderImpactSound;
            _projectiles.SpawnRoomGraphicsDustExplosion(
                slot.Position.Whole,
                BoulderDustAnimationIndex);
            LastBoulderSoundEffect = EnemySoundEffectDefinitions.BoulderBreakSound;
        }
        else
        {
            state.TravelSpeedAccumulator = AddAndCapUnsigned(
                state.TravelSpeedAccumulator,
                64,
                16384);
            if (slot.Position.Y.Whole == state.PreviousYPosition &&
                slot.Position.Y.Fraction == state.PreviousYSubposition)
            {
                state.VerticalCompensation = 0;
            }
        }

        state.PreviousYPosition = slot.Position.Y.Whole;
        state.PreviousYSubposition = slot.Position.Y.Fraction;
    }

    /// <summary>Adds the signed quadratic travel displacement to the actor's wrapped 16.16 X position.</summary>
    private static void AddBoulderHorizontalVelocity(RoomEnemySlot slot, BoulderEnemyState state)
    {
        int displacement = ReadBoulderHorizontalDisplacement(state);
        uint position = slot.Position.X.RawValue;
        position = unchecked(position + (uint)displacement);
        slot.Position = slot.Position.WithX(SnesFixedPosition.FromRaw(unchecked((uint)position)));
    }

    /// <summary>Reads the quadratic speed sample and signs it according to the spawn direction selector.</summary>
    private static int ReadBoulderHorizontalDisplacement(BoulderEnemyState state) =>
        EnemyMath.ReadQuadraticEnemySpeed(
            unchecked((byte)(SnesEightEight.WholeOf(state.TravelSpeedAccumulator))),
            negative: state.DirectionSelector != 0);

    /// <summary>Adds or subtracts a quadratic table sample from the actor's wrapped 16.16 Y position.</summary>
    private static void AddBoulderVerticalVelocity(RoomEnemySlot slot, ushort accumulator, bool negative)
    {
        int displacement = EnemyMath.ReadQuadraticEnemySpeed(
            SnesLittleEndian.HighByte(accumulator),
            negative);
        uint position = slot.Position.Y.RawValue;
        position = unchecked(position + (uint)displacement);
        slot.Position = slot.Position.WithY(SnesFixedPosition.FromRaw(unchecked((uint)position)));
    }

    /// <summary>Advances a native word accumulator and caps it using the cartridge's signed 16-bit comparison.</summary>
    private static ushort AddAndCapUnsigned(ushort value, ushort increment, ushort maximum)
    {
        ushort result = unchecked((ushort)(value + increment));
        return unchecked((short)(result - maximum)) < 0 ? result : maximum;
    }

    /// <summary>Rejects main AI before the slot's Boulder state has been initialized.</summary>
    internal static BoulderEnemyState RequireBoulderState(RoomEnemySlot slot) =>
        slot.SpeciesState<BoulderEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Boulder state.");

    /// <summary>Advances Boulder's typed animation.</summary>
    internal static void AdvanceBoulderTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireBoulderState(slot);
        AnimationProgramCursor<BoulderProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, BoulderAnimations.Programs, ref cursor, BoulderFrameDefinitions.Spritemap);
        state.Program = cursor;
    }

    /// <summary>Ports Boulder's initialization AI.</summary>
    internal static void RunBoulderInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeBoulder(slot);
    }

    /// <summary>Ports Boulder's main AI.</summary>
    internal void RunBoulderMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBoulderMain(slot, RequireBoulderState(slot), frame.Samus, frame.Level);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastBoulderSoundEffect = null;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastBoulderSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastBoulderSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
