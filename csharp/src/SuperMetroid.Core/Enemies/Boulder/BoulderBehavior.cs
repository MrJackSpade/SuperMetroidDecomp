using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Boulder.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>The Boulder species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
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

    /// <summary>The library-two sound queued when a falling or rolling boulder hits terrain.</summary>
    private static readonly SoundEffectId BoulderImpactSound =
        EnemySoundEffectDefinitions.BoulderImpactSound;

    /// <summary>The library-two sound that replaces the impact sound when the boulder breaks into dust.</summary>
    private static readonly SoundEffectId BoulderBreakSound =
        EnemySoundEffectDefinitions.BoulderBreakSound;

    /// <summary>Bounce-clock value the first rebound starts from: quadratic-speed record 32.</summary>
    private const int FirstReboundBounceSpeed = 8192;

    /// <summary>Change in the bounce clock per falling or rebounding update: one quadratic-speed record.</summary>
    private const int BounceSpeedStep = 256;

    /// <summary>Largest clock the initial fall, and the travel clock while bouncing, reach: quadratic-speed record 80.</summary>
    private const int MaximumBouncingSpeed = 20480;

    /// <summary>Change in the travel clock per bouncing update, an eighth of a record.</summary>
    private const int BouncingTravelSpeedStep = 32;

    /// <summary>Change in the travel clock per rolling update, a quarter of a record.</summary>
    private const int RollingTravelSpeedStep = 64;

    /// <summary>Largest travel clock while rolling: quadratic-speed record 64.</summary>
    private const int MaximumRollingTravelSpeed = 16384;

    /// <summary>Most recent library-two sound emitted by a landing or terminal impact.</summary>
    public SoundEffectId? LastBoulderSoundEffect { get; private set; }

    /// <summary>Interprets both spawn parameters as direction, trigger windows, vertical compensation, and initial height; a zero offset starts rolling without bounces.</summary>
    internal static void InitializeBoulder(RoomEnemySlot slot)
    {
        var state = new BoulderEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        state.BounceSpeedAccumulator = 0;
        state.TravelSpeedAccumulator = 0;
        state.BounceCounter = WrappingCountdown.Start(2);
        state.Function = BoulderAiFunction.WaitingForSamus;
        state.InitialPosition = slot.Position.Whole;
        state.PreviousY = slot.Position.Y;

        int initialYOffset = SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter2);
        if (initialYOffset == 0)
        {
            // A zero high byte is not “no offset.” Native substitutes one pixel and sets
            // var07 so the trigger jumps directly to rolling instead of the bounce chain.
            initialYOffset = 1;
            state.StartsRollingWithoutBounce = true;
        }
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(state.InitialPosition.Y - initialYOffset)));

        int horizontalReach = -SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter2);

        // The population record's initialization parameter was copied to current_instruction
        // before init AI ran. Boulder consumes only its low byte as the strict Samus-Y
        // trigger threshold, then starts the real rolling animation in its place.
        int verticalReach = unchecked((byte)slot.CurrentInstruction);
        state.DirectionSelector = SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter1);
        if (state.DirectionSelector == 0)
        {
            horizontalReach = -horizontalReach;
            state.Program = AnimationProgramCursor<BoulderProgram>.Start(BoulderProgram.Right);
        }
        else
        {
            state.Program = AnimationProgramCursor<BoulderProgram>.Start(BoulderProgram.Left);
        }
        state.TriggerReach = Offset16<RoomPixels>.Of(horizontalReach, verticalReach);

        state.VerticalCompensation = new Offset16<RoomPixels>(0, (byte)slot.Spawn.Population.Parameter1 == 0 ? (short)2 : (short)5);
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
                    $"Boulder function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Starts the bounce chain or direct roll only when Samus enters the direction-specific X window below the boulder.</summary>
    private static void RunBoulderWaiting(RoomEnemySlot slot, BoulderEnemyState state, SamusState samus)
    {
        int deltaY = unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole));
        if (deltaY < 0 || unchecked((short)(deltaY - state.TriggerReach.Y)) >= 0)
            return;

        int deltaX = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole));
        bool enteredHorizontalWindow = state.DirectionSelector != 0
            ? deltaX < 0 && unchecked((short)(deltaX - state.TriggerReach.X)) >= 0
            : deltaX >= 0 && unchecked((short)(deltaX - state.TriggerReach.X)) < 0;
        if (!enteredHorizontalWindow)
            return;

        state.Function = state.StartsRollingWithoutBounce
            ? BoulderAiFunction.Rolling
            : BoulderAiFunction.InitialFall;
    }

    /// <summary>Accelerates down toward the recorded spawn height, then clamps and starts the first rebound.</summary>
    private static void RunBoulderInitialFall(RoomEnemySlot slot, BoulderEnemyState state)
    {
        AddBoulderVerticalVelocity(slot, state.BounceSpeedAccumulator, direction: VerticalDirection.Down);
        if (unchecked((short)(slot.Position.Y.Whole - state.InitialPosition.Y)) < 0)
        {
            state.BounceSpeedAccumulator = AddAndCap(
                state.BounceSpeedAccumulator,
                BounceSpeedStep,
                MaximumBouncingSpeed);
            return;
        }

        slot.Position = slot.Position.WithWholeY(state.InitialPosition.Y);
        state.Function = BoulderAiFunction.Rebound;
        state.BounceSpeedAccumulator = FirstReboundBounceSpeed;
    }

    /// <summary>Applies upward quadratic motion while reducing bounce speed, adding sideways travel, then returns to falling.</summary>
    private static void RunBoulderRebound(RoomEnemySlot slot, BoulderEnemyState state)
    {
        AddBoulderVerticalVelocity(slot, state.BounceSpeedAccumulator, direction: VerticalDirection.Up);

        // The cartridge subtracts one record and branches on the sign of the result, storing zero in place of a
        // result below zero, so the clock never keeps a value below zero past this update.
        int slowed = state.BounceSpeedAccumulator - BounceSpeedStep;
        if (slowed < 0)
        {
            state.BounceSpeedAccumulator = 0;
            state.Function = BoulderAiFunction.Falling;
            return;
        }
        state.BounceSpeedAccumulator = slowed;

        AddBoulderHorizontalVelocity(slot, state);
        state.TravelSpeedAccumulator = AddAndCap(
            state.TravelSpeedAccumulator,
            BouncingTravelSpeedStep,
            MaximumBouncingSpeed);
    }

    /// <summary>Accelerates through terrain contact; ordinary impacts consume bounce indexes while selector two breaks immediately.</summary>
    internal void RunBoulderFalling(RoomEnemySlot slot, BoulderEnemyState state, RoomLevelData level)
    {
        int verticalDisplacement = EnemyMath.ReadQuadraticEnemySpeed(
            unchecked((byte)(SnesEightEight.WholeOf(state.BounceSpeedAccumulator))),
            negative: false);
        if (_terrain.MoveEnemyVertically(level, slot, verticalDisplacement))
        {
            LastBoulderSoundEffect = BoulderImpactSound;
            if (state.DirectionSelector == 2)
            {
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                _projectiles.SpawnRoomGraphicsDustExplosion(
                    slot.Position.Whole,
                    BoulderDustAnimationIndex);
                LastBoulderSoundEffect = BoulderBreakSound;
                return;
            }

            state.Function = BoulderAiFunction.Rebound;

            // The third impact still installs the leading zero sample before the counter
            // underflows. Preserve that write even though this impact enters rolling.
            state.BounceSpeedAccumulator = BoulderBounceDefinitions.SpeedIndex(state.BounceCounter.RawValue);
            state.BounceCounter = state.BounceCounter.Decrement().Remaining;
            if (state.BounceCounter.IsNegative)
            {
                state.PreviousY = slot.Position.Y;
                state.Function = BoulderAiFunction.Rolling;
            }
            return;
        }

        // The falling clock is not capped: its next read selects the following quadratic record, and the table
        // rejects record 95 before the clock could approach the word limit.
        state.BounceSpeedAccumulator += BounceSpeedStep;
        AddBoulderHorizontalVelocity(slot, state);
        state.TravelSpeedAccumulator = AddAndCap(
            state.TravelSpeedAccumulator,
            BouncingTravelSpeedStep,
            MaximumBouncingSpeed);
    }

    /// <summary>Tracks the floor with vertical compensation and advances horizontally until a wall hides/deletes the actor and spawns dust.</summary>
    private void RunBoulderRolling(RoomEnemySlot slot, BoulderEnemyState state, RoomLevelData level)
    {
        int verticalDisplacement = unchecked(
            EnemyMath.ReadQuadraticEnemySpeed(
                unchecked((byte)(SnesEightEight.WholeOf(state.TravelSpeedAccumulator))),
                negative: false) +
            SnesSixteenSixteen.FromWhole(state.VerticalCompensation.Y));
        _terrain.MoveEnemyVertically(level, slot, verticalDisplacement);
        slot.Position = slot.Position.WithWhole(slot.Position.Whole - state.VerticalCompensation);

        int horizontalDisplacement = ReadBoulderHorizontalDisplacement(state);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            // Native ORA #$0300 both hides and deletes the record. The inert function is
            // still written before DetermineWhichEnemiesToProcess clears it next frame.
            slot.Properties = slot.Properties.With(
                EnemyProperties.Invisible | EnemyProperties.Deleted);
            state.Function = BoulderAiFunction.Inert;
            LastBoulderSoundEffect = BoulderImpactSound;
            _projectiles.SpawnRoomGraphicsDustExplosion(
                slot.Position.Whole,
                BoulderDustAnimationIndex);
            LastBoulderSoundEffect = BoulderBreakSound;
        }
        else
        {
            state.TravelSpeedAccumulator = AddAndCap(
                state.TravelSpeedAccumulator,
                RollingTravelSpeedStep,
                MaximumRollingTravelSpeed);
            if (slot.Position.Y == state.PreviousY)
                state.VerticalCompensation = Offset16<RoomPixels>.Zero;
        }

        state.PreviousY = slot.Position.Y;
    }

    /// <summary>Adds the signed quadratic travel displacement to the actor's wrapped 16.16 X position.</summary>
    private static void AddBoulderHorizontalVelocity(RoomEnemySlot slot, BoulderEnemyState state)
    {
        int displacement = ReadBoulderHorizontalDisplacement(state);
        uint position = slot.Position.X.RawValue;
        position = unchecked(position + (uint)displacement);
        slot.Position = slot.Position.WithX(SnesFixedPosition.FromRaw(unchecked(position)));
    }

    /// <summary>Reads the quadratic speed sample and signs it according to the spawn direction selector.</summary>
    private static int ReadBoulderHorizontalDisplacement(BoulderEnemyState state) =>
        EnemyMath.ReadQuadraticEnemySpeed(
            unchecked((byte)(SnesEightEight.WholeOf(state.TravelSpeedAccumulator))),
            negative: state.DirectionSelector != 0);

    /// <summary>Adds or subtracts a quadratic table sample from the actor's wrapped 16.16 Y position.</summary>
    private static void AddBoulderVerticalVelocity(RoomEnemySlot slot, int accumulator, VerticalDirection direction)
    {
        int displacement = EnemyMath.ReadQuadraticEnemySpeed(
            unchecked((byte)(SnesEightEight.WholeOf(accumulator))),
            direction == VerticalDirection.Up);
        uint position = slot.Position.Y.RawValue;
        position = unchecked(position + (uint)displacement);
        slot.Position = slot.Position.WithY(SnesFixedPosition.FromRaw(unchecked(position)));
    }

    /// <summary>
    /// Advances an acceleration clock and caps it. The cartridge compares the sum with the cap by the sign of their
    /// word difference; both stay within 0 through 24576 here, so that is the ordinary comparison.
    /// </summary>
    private static int AddAndCap(int value, int increment, int maximum) => Math.Min(value + increment, maximum);

    /// <summary>Advances Boulder's typed animation.</summary>
    internal static void AdvanceBoulderTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, BoulderAnimations.Programs, BoulderFrameDefinitions.ShownFrames.Of,
            static (BoulderEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports Boulder's main AI.</summary>
    internal void RunBoulderMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBoulderMain(slot, slot.RequireSpeciesState<BoulderEnemyState>(), frame.Samus, frame.Level);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastBoulderSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastBoulderSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max6);
    }
}
