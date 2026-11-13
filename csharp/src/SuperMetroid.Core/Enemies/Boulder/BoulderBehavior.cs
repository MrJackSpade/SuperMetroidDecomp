using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Boulder.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>The Boulder species owner: its per-room state, behaviour and the interactions it registers.</summary>
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

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects terrain collision and the shared projectile engine for Boulder AI.</summary>
    internal BoulderBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>Room-graphics dust animation selected by terminal impacts.</summary>
    private const MiscDustAnimation BoulderDustAnimationIndex = MiscDustAnimation.RockParticles;

    /// <summary>The enemy-library sound queued when a falling or rolling boulder hits terrain.</summary>
    private static readonly SoundEffectId BoulderImpactSound =
        EnemySoundEffectDefinitions.BoulderImpactSound;

    /// <summary>The enemy-library sound that replaces the impact sound when the boulder breaks into dust.</summary>
    private static readonly SoundEffectId BoulderBreakSound =
        EnemySoundEffectDefinitions.BoulderBreakSound;

    /// <summary>Bounce-clock place the first rebound starts from: quadratic-speed record 32.</summary>
    private static readonly EnemyQuadraticSpeedIndex FirstReboundBounceSpeed = EnemyQuadraticSpeedIndex.FromRows(32);

    /// <summary>Change in the bounce clock per falling or rebounding update: one quadratic-speed record.</summary>
    private static readonly EnemyQuadraticSpeedIndex BounceSpeedStep = EnemyQuadraticSpeedIndex.FromRows(1);

    /// <summary>Largest clock the initial fall, and the travel clock while bouncing, reach: quadratic-speed record 80.</summary>
    private static readonly EnemyQuadraticSpeedIndex MaximumBouncingSpeed = EnemyQuadraticSpeedIndex.FromRows(80);

    /// <summary>Change in the travel clock per bouncing update, an eighth of a record.</summary>
    private static readonly EnemyQuadraticSpeedIndex BouncingTravelSpeedStep = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(32);

    /// <summary>Change in the travel clock per rolling update, a quarter of a record.</summary>
    private static readonly EnemyQuadraticSpeedIndex RollingTravelSpeedStep = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(64);

    /// <summary>Largest travel clock while rolling: quadratic-speed record 64.</summary>
    private static readonly EnemyQuadraticSpeedIndex MaximumRollingTravelSpeed = EnemyQuadraticSpeedIndex.FromRows(64);

    /// <summary>Reads both spawn parameters as direction, trigger windows, vertical compensation, and initial height; a zero offset starts rolling without bounces.</summary>
    internal static void InitializeBoulder(RoomEnemySlot slot)
    {
        var state = new BoulderEnemyState();
        slot.SetSpeciesState(state);

        state.BounceSpeedAccumulator = EnemyQuadraticSpeedIndex.Start;
        state.TravelSpeedAccumulator = EnemyQuadraticSpeedIndex.Start;
        state.BounceCounter = 2;
        state.Function = BoulderAiFunction.WaitingForSamus;
        state.InitialPosition = slot.Position.Whole;
        state.PreviousY = slot.Position.Y;

        BoulderSpawnSettings settings = slot.SpawnSettings<BoulderSpawnSettings>();
        int initialYOffset = settings.Drop;
        if (initialYOffset == 0)
        {
            // A zero drop is not “no offset.” The game substitutes one pixel and marks the
            // boulder so the trigger jumps directly to rolling instead of the bounce chain.
            initialYOffset = 1;
            state.StartsRollingWithoutBounce = true;
        }
        slot.Position = slot.Position.WithWrappedWholeY(state.InitialPosition.Y - initialYOffset);

        int horizontalReach = -settings.TriggerReach;

        // The population entry's trigger height is the strict Samus-Y threshold.
        int verticalReach = settings.TriggerHeight;
        state.Roll = settings.Roll;
        switch (settings.Roll)
        {
            case BoulderRoll.Right:
                horizontalReach = -horizontalReach;
                state.Playback = new(BoulderAnimations.StartingWith(BoulderProgram.Right), AnimationPlace.StartOfAnimation);
                break;
            case BoulderRoll.Left:
            case BoulderRoll.LeftBreakingOnImpact:
                state.Playback = new(BoulderAnimations.StartingWith(BoulderProgram.Left), AnimationPlace.StartOfAnimation);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BoulderRoll)} {settings.Roll}.");
        }
        state.TriggerReach = Offset16<RoomPixels>.Of(horizontalReach, verticalReach);

        state.VerticalCompensation = new Offset16<RoomPixels>(0, settings.Landing switch
        {
            BoulderLanding.Standard => 2,
            BoulderLanding.Deep => 5,
            _ => throw new InvalidOperationException($"Undefined {nameof(BoulderLanding)} {settings.Landing}."),
        });
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
        int deltaY = CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole);
        if (deltaY < 0 || CoordinateAxis.Difference(deltaY, state.TriggerReach.Y) >= 0)
            return;

        int deltaX = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole);
        bool enteredHorizontalWindow = state.Roll != BoulderRoll.Right
            ? deltaX < 0 && CoordinateAxis.Difference(deltaX, state.TriggerReach.X) >= 0
            : deltaX >= 0 && CoordinateAxis.Difference(deltaX, state.TriggerReach.X) < 0;
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
        if (CoordinateAxis.Difference(slot.Position.Y.Whole, state.InitialPosition.Y) < 0)
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

        // The game subtracts one record and branches on the sign of the result, storing zero in place of a
        // result below zero, so the clock never keeps a value below zero past this update.
        EnemyQuadraticSpeedIndex slowed = state.BounceSpeedAccumulator - BounceSpeedStep;
        if (slowed.IsNegative)
        {
            state.BounceSpeedAccumulator = EnemyQuadraticSpeedIndex.Start;
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
        SignedSixteenSixteen verticalDisplacement = state.BounceSpeedAccumulator.Row.Positive;
        if (_terrain.MoveEnemyVertically(level, slot, verticalDisplacement))
        {
            _audio.QueueEnemySound(BoulderImpactSound, priority: SoundRequestPriority.Normal);
            if (state.Roll == BoulderRoll.LeftBreakingOnImpact)
            {
                slot.Presence = slot.Presence.With(EnemyPresence.Deleted);
                _projectiles.SpawnRoomGraphicsDustExplosion(
                    slot.Position.Whole,
                    BoulderDustAnimationIndex);
                _audio.QueueEnemySound(BoulderBreakSound, priority: SoundRequestPriority.Normal);
                return;
            }

            state.Function = BoulderAiFunction.Rebound;

            // The third impact still installs the leading zero sample before the counter
            // goes below zero. Preserve that write even though this impact enters rolling.
            state.BounceSpeedAccumulator = BoulderBounces.SpeedIndex(state.BounceCounter);
            if (--state.BounceCounter < 0)
            {
                state.PreviousY = slot.Position.Y;
                state.Function = BoulderAiFunction.Rolling;
            }
            return;
        }

        // The falling clock is not capped: its next read selects the following quadratic record, and the table
        // rejects record 95 before the clock could approach the limit of its range.
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
        SignedSixteenSixteen verticalDisplacement =
            state.TravelSpeedAccumulator.Row.Positive +
            SignedSixteenSixteen.FromWholePixels(state.VerticalCompensation.Y);
        _terrain.MoveEnemyVertically(level, slot, verticalDisplacement);
        slot.Position = slot.Position.WithWhole(slot.Position.Whole - state.VerticalCompensation);

        SignedSixteenSixteen horizontalDisplacement = ReadBoulderHorizontalDisplacement(state);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            // Setting the hidden and deleted properties both hides and deletes the boulder. The inert function is
            // still written before the next update's processing pass clears it.
            slot.Presence = slot.Presence.With(EnemyPresence.Hidden | EnemyPresence.Deleted);
            state.Function = BoulderAiFunction.Inert;
            _audio.QueueEnemySound(BoulderImpactSound, priority: SoundRequestPriority.Normal);
            _projectiles.SpawnRoomGraphicsDustExplosion(
                slot.Position.Whole,
                BoulderDustAnimationIndex);
            _audio.QueueEnemySound(BoulderBreakSound, priority: SoundRequestPriority.Normal);
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

    /// <summary>Moves the actor horizontally by its signed quadratic travel speed, wrapping within the coordinate range.</summary>
    private static void AddBoulderHorizontalVelocity(RoomEnemySlot slot, BoulderEnemyState state) =>
        slot.Position = slot.Position.WithX(slot.Position.X.MovedBy(ReadBoulderHorizontalDisplacement(state)));

    /// <summary>Reads the quadratic speed sample and signs it according to the spawn direction selector.</summary>
    private static SignedSixteenSixteen ReadBoulderHorizontalDisplacement(BoulderEnemyState state) =>
        state.TravelSpeedAccumulator.Row.Toward(state.Roll != BoulderRoll.Right ? EnemySpeedSign.Negative : EnemySpeedSign.Positive);

    /// <summary>Moves the actor vertically by a quadratic table speed toward <paramref name="direction"/>, wrapping within the coordinate range.</summary>
    private static void AddBoulderVerticalVelocity(RoomEnemySlot slot, EnemyQuadraticSpeedIndex accumulator, VerticalDirection direction)
    {
        SignedSixteenSixteen displacement = accumulator.Row.Toward(direction == VerticalDirection.Up ? EnemySpeedSign.Negative : EnemySpeedSign.Positive);
        slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(displacement));
    }

    /// <summary>
    /// Advances an acceleration clock and caps it. The game compares the sum with the cap by the sign of their
    /// difference; both stay within 0 through 24576 here, so that is the ordinary comparison.
    /// </summary>
    private static EnemyQuadraticSpeedIndex AddAndCap(
        EnemyQuadraticSpeedIndex value, EnemyQuadraticSpeedIndex increment, EnemyQuadraticSpeedIndex maximum) =>
        EnemyQuadraticSpeedIndex.Min(value + increment, maximum);

    /// <summary>Advances Boulder's typed animation.</summary>
    internal static void AdvanceBoulderTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, BoulderAnimations.Segments, BoulderFrameDefinitions.ShownFrames.Of,
            static (BoulderEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Boulder animation raised {cue}."));

    /// <summary>Runs Boulder's main AI.</summary>
    internal void RunBoulderMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBoulderMain(slot, slot.RequireSpeciesState<BoulderEnemyState>(), frame.Samus, frame.Level);
    }
}
