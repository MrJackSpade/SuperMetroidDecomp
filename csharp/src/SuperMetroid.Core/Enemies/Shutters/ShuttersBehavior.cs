using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>The Shutters species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class ShuttersBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the species owner on the room's enemy audio queue.</summary>
    internal ShuttersBehavior(EnemyAudioQueue audio) => _audio = audio;

    /// <summary>
    /// Lays record <paramref name="record"/> of the escape's red shutter flash over every destroyable shutter among
    /// <paramref name="slots"/> (#1512): the first escape room's shutters draw in the colours the flash recolours, and
    /// each record replaces the one before it.
    /// </summary>
    internal static void ShowEscapeShutterFlash(IReadOnlyList<RoomEnemySlot> slots, PaletteFxFrame record)
    {
        Ensure.NotNull(slots);
        PictureShade flash = PaletteFxShades.Of(PaletteFxColours.TourianShutters, record);
        foreach (RoomEnemySlot slot in slots)
        {
            if (slot.DefinitionId != EnemyDefinitionId.ShutterDestroyable)
                continue;
            PictureShading shown = PictureShading.Standard;
            foreach (PictureShade shade in slot.Shading.Shades)
            {
                if (shade.Effect != flash.Effect)
                    shown = shown.With(shade);
            }
            foreach (ColourEffect effect in slot.Shading.Effects)
                shown = shown.With(effect);
            slot.Shading = shown.With(flash);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. ShuttersPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ShuttersEntity(this, slot, common);

    /// <summary>Each growing section advances sixteen pixels before its final placement.</summary>
    private const int GrowingShutterSectionLength = 16;
    /// <summary>Intermediate sections stop seven pixels inside their full-size endpoint.</summary>
    private const int GrowingShutterIntermediateInset = 7;

    /// <summary>Initializes the growing shutter.</summary>
    internal static void InitializeGrowingShutter(RoomEnemySlot slot)
    {
        GrowingShutterSpawnSettings settings = slot.SpawnSettings<GrowingShutterSpawnSettings>();
        var state = new GrowingShutterEnemyState(slot)
        {
            ActivationTimer = settings.ActivationWait,
            ActivationDistance = settings.ActivationDistance,
        };
        slot.SetSpeciesState(state);

        // The initializer selects its first function from the way it grows and what it waits for.
        bool growsUp = settings.Growth switch
        {
            GrowingShutterGrowth.Down => false,
            GrowingShutterGrowth.Up => true,
            _ => throw new InvalidOperationException($"Growing shutter growth {settings.Growth} is not a direction."),
        };
        state.Function = settings.Wait switch
        {
            GrowingShutterWait.Timer => growsUp
                ? GrowingShutterFunction.WaitToGrowUpForTimer
                : GrowingShutterFunction.WaitToGrowDownForTimer,
            GrowingShutterWait.SamusNear => growsUp
                ? GrowingShutterFunction.WaitToGrowUpForProximity
                : GrowingShutterFunction.WaitToGrowDownForProximity,
            _ => throw new InvalidOperationException($"Growing shutter wait {settings.Wait} is not a wait."),
        };

        state.PlaceSectionOrigins(growsUp);
        state.GrowthLevel = 0;

        state.GrowthSpeed = settings.GrowthSpeed;

        slot.CollidesByFrameHitboxes = false;
        slot.NewInstructionFrame = false;
        InstallGrowingShutterProgram(
            slot,
            state,
            ShutterProgram.GrowingTenPixels,
            yRadius: 8);
    }

    /// <summary>Runs the growing shutter's main AI.</summary>
    private void RunGrowingShutterMain(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Function)
        {
            case GrowingShutterFunction.WaitToGrowDownForTimer:
                if (AdvanceGrowingShutterTimer(state))
                    ActivateGrowingShutter(slot, state, direction: VerticalDirection.Down, camera);
                return;

            case GrowingShutterFunction.WaitToGrowUpForTimer:
                if (AdvanceGrowingShutterTimer(state))
                    ActivateGrowingShutter(slot, state, direction: VerticalDirection.Up, camera);
                return;

            case GrowingShutterFunction.WaitToGrowDownForProximity:
                if (samus is null)
                    throw new InvalidOperationException("Growing-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(slot, samus, state.ActivationDistance))
                    ActivateGrowingShutter(slot, state, direction: VerticalDirection.Down, camera);
                return;

            case GrowingShutterFunction.WaitToGrowUpForProximity:
                if (samus is null)
                    throw new InvalidOperationException("Growing-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(slot, samus, state.ActivationDistance))
                    ActivateGrowingShutter(slot, state, direction: VerticalDirection.Up, camera);
                return;

            case GrowingShutterFunction.GrowDown:
                GrowShutterDown(slot, state);
                return;

            case GrowingShutterFunction.GrowUp:
                if (samus is null)
                    throw new InvalidOperationException("Upward growing-shutter carry requires Samus state.");
                GrowShutterUp(slot, state, samus);
                return;

            default:
                throw new InvalidDataException(
                    $"Growing shutter function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Code tests the timer before decrementing it. Zero therefore activates on the
    /// current call, while a positive N waits N complete calls and activates on call N + 1.
    /// </summary>
    internal static bool AdvanceGrowingShutterTimer(GrowingShutterEnemyState state)
    {
        if (state.ActivationTimer == 0)
            return true;
        state.ActivationTimer = Math.Max(0, state.ActivationTimer - 1);
        return false;
    }

    /// <summary>Switches a timed or proximity shutter into growth and queues audible activation on screen.</summary>
    private void ActivateGrowingShutter(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        VerticalDirection direction,
        Position16<RoomPixels> camera)
    {
        state.Function = direction == VerticalDirection.Up
            ? GrowingShutterFunction.GrowUp
            : GrowingShutterFunction.GrowDown;
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>Runs the four growth stages.</summary>
    private static void GrowShutterDown(RoomEnemySlot slot, GrowingShutterEnemyState state)
    {
        if (state.GrowthLevel >= 4)
            return;

        slot.Position = slot.Position.AdvanceY(state.GrowthSpeed);
        Position16<RoomPixels> target = state.OriginForLevel() + new Offset16<RoomPixels>(0, GrowingShutterSectionLength);

        // The non-negative test returns at equality. A section advances only after its center has passed the
        // full sixteen-pixel threshold, after which the first three sections are inset seven.
        if ((target - slot.Position.Whole).Y >= 0)
            return;
        FinishGrowingShutterSection(slot, state, target, direction: VerticalDirection.Down);
    }

    /// <summary>Runs the four upward growth stages.</summary>
    private static void GrowShutterUp(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        SamusState samus)
    {
        if (state.GrowthLevel >= 4)
            return;

        state.PreviousPosition = slot.Position.Whole;
        slot.Position = slot.Position.AdvanceY(UpwardGrowthVelocity(state.GrowthSpeed));
        Position16<RoomPixels> target = state.OriginForLevel() - new Offset16<RoomPixels>(0, GrowingShutterSectionLength);
        if ((target - slot.Position.Whole).Y < 0)
        {
            CarrySamusWithGrowingShutter(slot, state, samus);
            return;
        }

        FinishGrowingShutterSection(slot, state, target, direction: VerticalDirection.Up);
        CarrySamusWithGrowingShutter(slot, state, samus);
    }

    /// <summary>
    /// The upward growth step: the exact signed 16.16 negation of the positive growth speed, borrowing one
    /// whole pixel from the negated whole pixels whenever the fraction is nonzero.
    /// </summary>
    internal static SignedSixteenSixteen UpwardGrowthVelocity(SignedSixteenSixteen growthSpeed) =>
        SignedSixteenSixteen.Zero - growthSpeed;

    /// <summary>Adds upward displacement only while Samus is riding the growing platform.</summary>
    private static void CarrySamusWithGrowingShutter(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        SamusState samus)
    {
        if (!PlatformsBehavior.IsSamusRidingPlatform(slot, samus))
            return;

        int wholeDelta = (slot.Position.Whole - state.PreviousPosition).Y;
        if (wholeDelta < 0)
        {
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, wholeDelta);
        }
    }

    /// <summary>Snaps a completed section, enlarges its collision radius, and selects the next artwork.</summary>
    internal static void FinishGrowingShutterSection(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        Position16<RoomPixels> target,
        VerticalDirection direction)
    {
        bool finalSection = state.GrowthLevel == 3;
        Position16<RoomPixels> placed = finalSection
            ? target
            : target + new Offset16<RoomPixels>(0, direction == VerticalDirection.Up
                ? GrowingShutterIntermediateInset
                : -GrowingShutterIntermediateInset);
        slot.Position = slot.Position.WithWholeY(placed.Y);
        state.GrowthLevel += 1;

        if (finalSection)
            return;

        ShutterProgram program = state.GrowthLevel switch
        {
            1 => ShutterProgram.GrowingTwentyPixels,
            2 => ShutterProgram.GrowingThirtyPixels,
            3 => ShutterProgram.FortyPixels,
            _ => throw new InvalidDataException(
                $"Growing shutter advanced to invalid level {state.GrowthLevel}."),
        };
        int radius = 8 + state.GrowthLevel * 8;
        InstallGrowingShutterProgram(slot, state, program, radius);
    }

    /// <summary>Restarts the next growth pose and updates the shutter's vertical collision radius.</summary>
    private static void InstallGrowingShutterProgram(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        ShutterProgram program,
        int yRadius)
    {
        state.Playback = new(ShutterAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        slot.Radius = slot.Radius.WithY(yRadius);
    }
    /// <summary>Samus's four-pixel correction when pushing against a horizontal shutter.</summary>
    private const int HorizontalShutterManualPushPixels = 4;

    /// <summary>Initializes a horizontal shutter.</summary>
    internal static void InitializeHorizontalShutter(RoomEnemySlot slot)
    {
        var state = new HorizontalShutterEnemyState();
        slot.SetSpeciesState(state);

        ShutterSpawnSettings settings = slot.SpawnSettings<ShutterSpawnSettings>();
        state.PrimaryDirection = settings.PrimaryDirection;
        state.ReactionDirection = Opposite(state.PrimaryDirection);

        EnemyLinearSpeedRow speeds = EnemyLinearSpeedRow.Moving(settings.Speed);
        state.RightVelocity = speeds.Positive;
        state.LeftVelocity = speeds.Negative;

        state.MovedLeftRestTime = settings.FirstRestTime;
        state.MovedRightRestTime = settings.SecondRestTime;

        state.TriggerMode = settings.TriggerMode;
        state.TravelDistance = settings.TravelDistance;
        state.ProximityDistance = settings.ProximityDistance;
        state.WaitTime = settings.WaitTime;
        state.FunctionTimer = WrappingCountdown.Start(settings.WaitTime);

        Position16<RoomPixels> spawn = slot.Position.Whole;
        var travel = new Offset16<RoomPixels>(state.TravelDistance, 0);
        state.LeftEndpoint = spawn;
        state.RightEndpoint = spawn + travel;
        if (state.PrimaryDirection == ShutterHeading.LeftOrUp)
        {
            state.RightEndpoint = spawn;
            state.LeftEndpoint = spawn - travel;
        }

        state.Function = HorizontalShutterFunction.Initial;
        state.MovingSamus = false;
        state.ShotActivated = false;
        slot.CollidesByFrameHitboxes = false;
        slot.NewInstructionFrame = false;
        InstallHorizontalShutterProgram(slot, state);
    }

    /// <summary>Runs the horizontal shutter's main AI.</summary>
    private static void RunHorizontalShutterMain(
        RoomEnemySlot slot,
        HorizontalShutterEnemyState state,
        SamusState? samus,
        ControllerButton controllerInput)
    {
        if (samus is null)
            throw new InvalidOperationException("Horizontal-shutter AI requires Samus state.");

        switch (state.Function)
        {
            case HorizontalShutterFunction.Initial:
                SelectInitialHorizontalShutterFunction(state);
                break;

            case HorizontalShutterFunction.WaitForTimer:
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsZero)
                {
                    state.FunctionTimer = WrappingCountdown.Start(state.WaitTime);
                    ActivateHorizontalShutter(state);
                }
                break;

            case HorizontalShutterFunction.WaitForHorizontalProximity:
                if (IsSamusWithinShutterHorizontalDistance(
                    slot,
                    samus,
                    state.ProximityDistance))
                {
                    ActivateHorizontalShutter(state);
                }
                break;

            case HorizontalShutterFunction.Activate:
                ActivateHorizontalShutter(state);
                break;

            case HorizontalShutterFunction.InitialNoOp:
            case HorizontalShutterFunction.PermanentNoOp:
                break;

            case HorizontalShutterFunction.MovingLeft:
                MoveHorizontalShutter(slot, state, samus, controllerInput, direction: HorizontalDirection.Left);
                break;

            case HorizontalShutterFunction.MovingRight:
                MoveHorizontalShutter(slot, state, samus, controllerInput, direction: HorizontalDirection.Right);
                break;

            case HorizontalShutterFunction.StoppedAfterMovingLeft:
                RunHorizontalShutterStopped(state, end: HorizontalDirection.Left);
                break;

            case HorizontalShutterFunction.StoppedAfterMovingRight:
                RunHorizontalShutterStopped(state, end: HorizontalDirection.Right);
                break;

            default:
                throw new InvalidDataException(
                    $"Horizontal shutter function {state.Function?.ToString() ?? "none"} is not translated.");
        }

        // An idle shutter reacts when one of Samus's four solid-enemy collisions
        // names this enemy and she has contact damage (e.g. screw attack).
        if (state.Function is not (HorizontalShutterFunction.MovingLeft or HorizontalShutterFunction.MovingRight) &&
            SamusContactsIdleShutter(slot, samus))
        {
            ReactHorizontalShutter(slot);
        }
    }

    /// <summary>The end opposite <paramref name="heading"/>.</summary>
    internal static ShutterHeading Opposite(ShutterHeading heading) => heading switch
    {
        ShutterHeading.LeftOrUp => ShutterHeading.RightOrDown,
        ShutterHeading.RightOrDown => ShutterHeading.LeftOrUp,
        _ => throw new InvalidOperationException($"Undefined {nameof(ShutterHeading)} {heading}."),
    };

    /// <summary>Selects the population's timer, proximity, activation, or idle initial function.</summary>
    internal static void SelectInitialHorizontalShutterFunction(HorizontalShutterEnemyState state)
    {
        state.Function = state.TriggerMode switch
        {
            ShutterTriggerMode.Timer => HorizontalShutterFunction.WaitForTimer,
            ShutterTriggerMode.HorizontalProximity => HorizontalShutterFunction.WaitForHorizontalProximity,
            ShutterTriggerMode.Immediate => HorizontalShutterFunction.Activate,
            ShutterTriggerMode.OneShotReaction or ShutterTriggerMode.RepeatableReaction => HorizontalShutterFunction.InitialNoOp,
            _ => throw new InvalidDataException($"Horizontal shutter trigger mode {state.TriggerMode} is invalid."),
        };
    }

    /// <summary>Starts movement in the population's primary direction.</summary>
    private static void ActivateHorizontalShutter(HorizontalShutterEnemyState state) =>
        state.Function = state.PrimaryDirection == ShutterHeading.LeftOrUp
            ? HorizontalShutterFunction.MovingLeft
            : HorizontalShutterFunction.MovingRight;

    /// <summary>
    /// Moves toward the left or right endpoint, carries Samus on contact (Samus on the leading side, pushing against the
    /// travel by four pixels when she holds the opposite direction), and enters rest or permanent stop at the endpoint.
    /// </summary>
    private static void MoveHorizontalShutter(
        RoomEnemySlot slot,
        HorizontalShutterEnemyState state,
        SamusState samus,
        ControllerButton controllerInput,
        HorizontalDirection direction)
    {
        bool samusLeftOfShutter = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0;
        state.MovingSamus = HorizontalShutterOverlapsSamus(slot, samus) && samusLeftOfShutter == (direction == HorizontalDirection.Left);
        SignedSixteenSixteen velocity = direction == HorizontalDirection.Left ? state.LeftVelocity : state.RightVelocity;
        slot.Position = slot.Position.AdvanceX(velocity);
        if (state.MovingSamus)
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(velocity);
            if (controllerInput.HasAny(direction == HorizontalDirection.Left ? ControllerButton.Right : ControllerButton.Left))
            {
                var push = new Offset16<RoomPixels>(unchecked(HorizontalShutterManualPushPixels), 0);
                samus.Kinematics.ExtraDisplacement = direction == HorizontalDirection.Left
                    ? samus.Kinematics.ExtraDisplacement - push
                    : samus.Kinematics.ExtraDisplacement + push;
                samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithFractionY(0);
            }
        }

        int beforeEndpoint = direction == HorizontalDirection.Left
            ? (state.LeftEndpoint - slot.Position.Whole).X
            : -(state.RightEndpoint - slot.Position.Whole).X;
        if (beforeEndpoint < 0)
            return;
        int restTime = direction == HorizontalDirection.Left ? state.MovedLeftRestTime : state.MovedRightRestTime;
        if (restTime == PermanentStopRestTime)
        {
            state.Function = HorizontalShutterFunction.PermanentNoOp;
            return;
        }
        state.FunctionTimer = WrappingCountdown.Start(restTime);
        state.Function = direction == HorizontalDirection.Left ? HorizontalShutterFunction.StoppedAfterMovingLeft : HorizontalShutterFunction.StoppedAfterMovingRight;
    }

    /// <summary>
    /// Counts down an endpoint rest before reversing, or before waiting for Samus's proximity when trigger mode one would
    /// reverse into the population's primary direction.
    /// </summary>
    private static void RunHorizontalShutterStopped(HorizontalShutterEnemyState state, HorizontalDirection end)
    {
        CountdownStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;
        bool reversesIntoPrimary = (state.PrimaryDirection == ShutterHeading.LeftOrUp) != (end == HorizontalDirection.Left);
        state.Function = state.TriggerMode == ShutterTriggerMode.HorizontalProximity && reversesIntoPrimary
            ? HorizontalShutterFunction.WaitForHorizontalProximity
            : end == HorizontalDirection.Left ? HorizontalShutterFunction.MovingRight : HorizontalShutterFunction.MovingLeft;
    }

    /// <summary>Tests shutter and Samus radius boxes before applying horizontal carry.</summary>
    private static bool HorizontalShutterOverlapsSamus(RoomEnemySlot slot, SamusState samus) =>
        EnemyCollision.RadiusBoxesOverlap(
            slot.Position.Whole,
            slot.Radius,
            samus.Position.Whole,
            samus.Kinematics.Radius);

    /// <summary>Runs the stationary/manual ejection portion of the shutter's touch AI.</summary>
    private static void TouchHorizontalShutter(
        RoomEnemySlot slot,
        SamusState samus,
        ControllerButton controllerInput)
    {
        HorizontalShutterEnemyState state = slot.RequireSpeciesState<HorizontalShutterEnemyState>();
        if (state.Function != HorizontalShutterFunction.PermanentNoOp)
            return;

        bool samusIsLeft = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0;
        if (samusIsLeft && controllerInput.HasAny(ControllerButton.Right))
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeX(-HorizontalShutterManualPushPixels).WithFractionY(0);
        }
        else if (!samusIsLeft && controllerInput.HasAny(ControllerButton.Left))
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeX(HorizontalShutterManualPushPixels).WithFractionY(0);
        }
    }

    /// <summary>Runs the movement-trigger tail shared by shot and power-bomb AI.</summary>
    private static void ReactHorizontalShutter(RoomEnemySlot slot)
    {
        HorizontalShutterEnemyState state = slot.RequireSpeciesState<HorizontalShutterEnemyState>();
        if (state.TriggerMode is not (ShutterTriggerMode.OneShotReaction or ShutterTriggerMode.RepeatableReaction))
            return;

        if (state.TriggerMode == ShutterTriggerMode.OneShotReaction)
        {
            if (state.ShotActivated)
                return;
            state.ShotActivated = true;
        }

        if (state.Function is HorizontalShutterFunction.MovingLeft or HorizontalShutterFunction.MovingRight)
            return;

        state.ReactionDirection = Opposite(state.ReactionDirection);
        state.Function = state.ReactionDirection == ShutterHeading.LeftOrUp
            ? HorizontalShutterFunction.MovingLeft
            : HorizontalShutterFunction.MovingRight;
    }

    /// <summary>Restarts the stationary shutter pose after movement or reaction.</summary>
    private static void InstallHorizontalShutterProgram(RoomEnemySlot slot, HorizontalShutterEnemyState state)
    {
        state.Playback = new(ShutterAnimations.StartingWith(ShutterProgram.Horizontal), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Rest length, in AI updates, that selects a permanent stop at an endpoint instead of a timed rest.</summary>
    private const int PermanentStopRestTime = 4080;

    /// <summary>The enemy-library sound queued when an on-screen shutter begins moving.</summary>
    private static readonly SoundEffectId ShutterActivationSound =
        EnemySoundEffectDefinitions.ShutterActivationSound;

    /// <summary>Frame camera used to admit activation sound only for on-screen shutters.</summary>
    private Position16<RoomPixels> _shutterCamera;

    /// <summary>Captures the current camera, which decides whether an activating shutter is heard.</summary>
    internal void BeginShutterFrame(Position16<RoomPixels> camera) => _shutterCamera = camera;

    /// <summary>Includes shootable, destroyable, and Kamer definitions in the shared vertical AI.</summary>
    internal static bool IsVerticalShutterDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.ShutterDestroyable or
        EnemyDefinitionId.Kamer;

    /// <summary>
    /// The shared idle-shutter contact test: Samus's solid-enemy contacts this update name this shutter, as
    /// <see cref="SolidContactSlotCombination"/> combines them, and her contact-damage index is nonzero.
    /// </summary>
    private static bool SamusContactsIdleShutter(RoomEnemySlot slot, SamusState samus) =>
        SolidContactSlotCombination.Combine(samus.Kinematics.SolidEnemyCollisions, EnemySlotOrder.Capacity) == slot.SlotIndex &&
        samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None;

    /// <summary>Tests the strict, wrapped absolute-X proximity threshold.</summary>
    private static bool IsSamusWithinShutterHorizontalDistance(
        RoomEnemySlot slot,
        SamusState samus,
        int maximumDistance)
    {
        int distance = CoordinateAxis.Wrap(samus.Position.X.Whole - slot.Position.X.Whole);
        if (CoordinateAxis.AsSigned(distance) < 0)
            distance = CoordinateAxis.Wrap(-distance);
        return distance < maximumDistance;
    }

    /// <summary>
    /// Tests whether the shutter is on screen for sound. Its bottom edge is 256 pixels, deliberately not the 224-line visible
    /// viewport, because the sound admission test uses a square screen window.
    /// </summary>
    private static bool IsShutterCenterOnScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        CoordinateAxis.Difference(slot.Position.X.Whole, camera.X) >= 0 &&
        CoordinateAxis.AsSigned(camera.X + 256 - slot.Position.X.Whole) >= 0 &&
        CoordinateAxis.Difference(slot.Position.Y.Whole, camera.Y) >= 0 &&
        CoordinateAxis.AsSigned(camera.Y + 256 - slot.Position.Y.Whole) >= 0;

    /// <summary>Publishes the activation sound only when the shutter center lies in the 256-pixel camera square.</summary>
    private void QueueShutterActivationSoundIfOnScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera)
    {
        if (IsShutterCenterOnScreen(slot, camera))
            _audio.QueueEnemySound(ShutterActivationSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Lets bombs trigger shootable vertical shutters without ordinary damage AI.</summary>
    internal void AddShuttersBombReactions(EnemySharedReactions table)
    {
        // The shutter's shot reaction never calls common shot AI. Bomb collision has already marked the physical bomb, then the
        // callback only advances the shutter state machine exactly as a beam, missile, touch, or power bomb would.
        table.AddBombSpecial(c => c.Enemy.DefinitionId is EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer &&
                                  c.SelectedShotAi == EnemyReaction.ShootableVerticalShutterShot,
            c => ReactVerticalShutter(c.Enemy, _shutterCamera));
    }

    /// <summary>Runs common bomb damage before reacting a destroyable vertical shutter.</summary>
    internal void AddDestroyableShutterBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.ShutterDestroyable &&
                                                       c.SelectedShotAi == EnemyReaction.DestroyableVerticalShutterShot)
        {
            AfterCommon = c => ReactDestroyableShutterAfterCommon(c.Enemy, c.StruckOccupant),
        });
    }

    /// <summary>Plays a shutter's cue-free animation on to its next frame after its frame expired.</summary>
    private static AnimationPlayback<ShutterProgram> PlayShutterAnimation(RoomEnemySlot slot, AnimationPlayback<ShutterProgram> playback)
    {
        EnemyProgramAnimation.Play(slot, ShutterAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"A shutter's cue-free animation raised {cue}."));
        return playback;
    }

    /// <summary>Advances ShutterGrowing's typed animation.</summary>
    internal static void AdvanceShutterGrowingTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        GrowingShutterEnemyState state = slot.RequireSpeciesState<GrowingShutterEnemyState>();
        state.Playback = PlayShutterAnimation(slot, state.Playback);
    }

    /// <summary>Advances ShutterShootable's typed animation.</summary>
    internal static void AdvanceShutterShootableTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        VerticalShutterEnemyState state = slot.RequireSpeciesState<VerticalShutterEnemyState>();
        state.Playback = PlayShutterAnimation(slot, state.Playback);
    }

    /// <summary>Advances ShutterHorizShootable's typed animation.</summary>
    internal static void AdvanceShutterHorizShootableTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        HorizontalShutterEnemyState state = slot.RequireSpeciesState<HorizontalShutterEnemyState>();
        state.Playback = PlayShutterAnimation(slot, state.Playback);
    }

    /// <summary>Runs ShutterGrowing's main AI.</summary>
    internal void RunShutterGrowingMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunGrowingShutterMain(
            slot,
            slot.RequireSpeciesState<GrowingShutterEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Runs ShutterShootable's main AI.</summary>
    internal void RunShutterShootableMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunVerticalShutterMain(
            slot,
            slot.RequireSpeciesState<VerticalShutterEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Runs ShutterHorizShootable's main AI.</summary>
    internal static void RunShutterHorizShootableMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunHorizontalShutterMain(
            slot,
            slot.RequireSpeciesState<HorizontalShutterEnemyState>(),
            frame.Samus,
            frame.ControllerInput);
    }

    /// <summary>Uses movement reactions instead of common power-bomb damage for shutter families.</summary>
    internal void AddShuttersPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => IsVerticalShutterDefinition(enemy.DefinitionId) &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.VerticalShutterPowerBomb)
        {
            ReplacesCommon = (enemy, _) => ReactVerticalShutter(enemy, _shutterCamera),
        });
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.ShutterHorizShootable &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.HorizontalShutterPowerBomb)
        {
            ReplacesCommon = (enemy, _) => ReactHorizontalShutter(enemy),
        });
    }

    /// <summary>Marks a shootable vertical shutter's projectile collision before activating it.</summary>
    internal void AddShuttersShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId is EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.ShootableVerticalShutterShot)
        {
            // The shot reaction goes directly to the shutter reaction instead of normal shot AI. The collision walker
            // has nevertheless already marked the colliding projectile, including Super Missile quake side effects.
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
                ReactVerticalShutter(c.Enemy, _shutterCamera);
                return true;
            },
        });
    }

    /// <summary>Runs common shot damage for destroyable shutters and movement reactions for horizontal ones.</summary>
    internal void AddDestroyableAndHorizontalShutterShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.ShutterDestroyable &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.DestroyableVerticalShutterShot)
        {
            AfterCommon = c => ReactDestroyableShutterAfterCommon(c.Enemy, c.StruckOccupant),
        });

        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.ShutterHorizShootable &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.HorizontalShutterShot)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = c => ReactHorizontalShutter(c.Enemy),
        });
    }

    /// <summary>Uses Samus contact to react vertical shutters or manually eject her from stopped horizontal ones.</summary>
    internal void AddShuttersTouch(EnemySharedReactions table)
    {
        // The touch reaction does not enter common contact damage. Body overlap is itself the trigger, and the reaction test decides
        // whether this population mode may start moving.
        table.AddTouch(slot => IsVerticalShutterDefinition(slot.DefinitionId) &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.VerticalShutterTouch,
            context => ReactVerticalShutter(context.Slot, _shutterCamera));

        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.ShutterHorizShootable &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.HorizontalShutterTouch,
            context =>
            {
                // The touch AI provides manual ejection only after a permanently stopped shutter.
                // The main AI's subsequent nonzero contact-damage-index check reuses the
                // shot reaction, allowing Screw/Speed contact to trigger modes three/four.
                TouchHorizontalShutter(context.Slot, context.Samus, context.ControllerInput);
                if (context.ContactDamageIndex != SamusContactDamageIndex.None)
                    ReactHorizontalShutter(context.Slot);
            });
    }
    /// <summary>Initializes a vertical shutter through either initializer and their shared tail.</summary>
    internal static void InitializeVerticalShutter(RoomEnemySlot slot)
    {
        if (!IsVerticalShutterDefinition(slot.DefinitionId))
        {
            throw new InvalidDataException(
                $"Definition {slot.DefinitionId} does not use vertical-shutter initialization.");
        }

        var state = new VerticalShutterEnemyState();
        slot.SetSpeciesState(state);

        ShutterSpawnSettings settings = slot.SpawnSettings<ShutterSpawnSettings>();
        state.PrimaryDirection = settings.PrimaryDirection;
        state.ReactionDirection = state.PrimaryDirection;

        EnemyLinearSpeedRow speeds = EnemyLinearSpeedRow.Moving(settings.Speed);
        state.DownVelocity = speeds.Positive;
        state.UpVelocity = speeds.Negative;

        state.MovedUpRestTime = settings.FirstRestTime;
        state.MovedDownRestTime = settings.SecondRestTime;

        state.TriggerMode = settings.TriggerMode;
        state.TravelDistance = settings.TravelDistance;
        state.ProximityDistance = settings.ProximityDistance;
        state.WaitTime = settings.WaitTime;
        state.FunctionTimer = WrappingCountdown.Start(settings.WaitTime);

        Position16<RoomPixels> spawn = slot.Position.Whole;
        var travel = new Offset16<RoomPixels>(0, state.TravelDistance);
        state.UpperEndpoint = spawn;
        state.LowerEndpoint = spawn + travel;
        if (state.PrimaryDirection == ShutterHeading.LeftOrUp)
        {
            state.LowerEndpoint = spawn;
            state.UpperEndpoint = spawn - travel;
        }

        state.Function = VerticalShutterFunction.Initial;
        state.MovingSamus = false;
        state.ShotActivated = false;
        slot.CollidesByFrameHitboxes = false;
        slot.NewInstructionFrame = false;
        InstallVerticalShutterProgram(
            slot,
            state,
            slot.DefinitionId == EnemyDefinitionId.Kamer ? ShutterProgram.KamerPlatform : ShutterProgram.FortyPixels);
    }

    /// <summary>Runs the vertical shutter's main AI.</summary>
    internal void RunVerticalShutterMain(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        // The initial function calls through its parameter table during this very AI
        // call; it does not spend a frame installing the selected wait function.
        // Keep Initial live until activation writes the moving function, as does.
        switch (state.Function == VerticalShutterFunction.Initial
            ? SelectInitialVerticalShutterFunction(state) : state.Function)
        {
            case VerticalShutterFunction.WaitForTimer:
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsZero)
                {
                    state.FunctionTimer = WrappingCountdown.Start(state.WaitTime);
                    ActivateVerticalShutter(slot, state, camera);
                }
                break;

            case VerticalShutterFunction.WaitForHorizontalProximity:
                if (samus is null)
                    throw new InvalidOperationException("Vertical-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(
                    slot,
                    samus,
                    state.ProximityDistance))
                {
                    ActivateVerticalShutter(slot, state, camera);
                }
                break;

            case VerticalShutterFunction.Activate:
                ActivateVerticalShutter(slot, state, camera);
                break;

            case VerticalShutterFunction.InitialNoOp:
            case VerticalShutterFunction.PermanentNoOp:
                break;

            case VerticalShutterFunction.MovingUp:
                if (samus is null)
                    throw new InvalidOperationException("Moving vertical shutter requires Samus state.");
                MoveVerticalShutter(slot, state, samus, direction: VerticalDirection.Up);
                break;

            case VerticalShutterFunction.MovingDown:
                if (samus is null)
                    throw new InvalidOperationException("Moving vertical shutter requires Samus state.");
                MoveVerticalShutter(slot, state, samus, direction: VerticalDirection.Down);
                break;

            case VerticalShutterFunction.StoppedAfterMovingUp:
                RunVerticalShutterStopped(slot, state, camera, end: VerticalDirection.Up);
                break;

            case VerticalShutterFunction.StoppedAfterMovingDown:
                RunVerticalShutterStopped(slot, state, camera, end: VerticalDirection.Down);
                break;

            case VerticalShutterFunction.Initial:
            default:
                throw new InvalidDataException(
                    $"Vertical shutter function {state.Function?.ToString() ?? "none"} is not translated.");
        }

        // An idle shutter reacts when one of Samus's four solid-enemy collisions
        // names this enemy and she has contact damage (e.g. screw attack).
        if (state.Function is not (VerticalShutterFunction.MovingUp or VerticalShutterFunction.MovingDown) &&
            SamusContactsIdleShutter(slot, samus ?? throw new InvalidOperationException(
                "Vertical-shutter AI requires Samus state for its contact reaction.")))
        {
            ReactVerticalShutter(slot, camera);
        }
    }

    /// <summary>Selects the vertical population's timer, proximity, activation, or idle initial function.</summary>
    internal static VerticalShutterFunction SelectInitialVerticalShutterFunction(VerticalShutterEnemyState state)
    {
        return state.TriggerMode switch
        {
            ShutterTriggerMode.Timer => VerticalShutterFunction.WaitForTimer,
            ShutterTriggerMode.HorizontalProximity => VerticalShutterFunction.WaitForHorizontalProximity,
            ShutterTriggerMode.Immediate => VerticalShutterFunction.Activate,
            ShutterTriggerMode.OneShotReaction or ShutterTriggerMode.RepeatableReaction => VerticalShutterFunction.InitialNoOp,
            _ => throw new InvalidDataException($"Vertical shutter trigger mode {state.TriggerMode} is invalid."),
        };
    }

    /// <summary>Starts movement in the primary vertical direction and queues on-screen audio.</summary>
    private void ActivateVerticalShutter(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera)
    {
        state.Function = state.PrimaryDirection == ShutterHeading.LeftOrUp
            ? VerticalShutterFunction.MovingUp
            : VerticalShutterFunction.MovingDown;
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>Moves toward the upper or lower endpoint, carrying Samus when riding the shutter.</summary>
    private static void MoveVerticalShutter(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState samus,
        VerticalDirection direction)
    {
        state.PreviousPosition = slot.Position.Whole;
        state.MovingSamus = PlatformsBehavior.IsSamusRidingPlatform(slot, samus);
        slot.Position = slot.Position.AdvanceY(direction == VerticalDirection.Up ? state.UpVelocity : state.DownVelocity);
        CarrySamusVertically(slot, state, samus);

        int beforeEndpoint = direction == VerticalDirection.Up
            ? (state.UpperEndpoint - slot.Position.Whole).Y
            : -(state.LowerEndpoint - slot.Position.Whole).Y;
        if (beforeEndpoint < 0)
            return;
        StopVerticalShutter(state, direction == VerticalDirection.Up);
    }

    /// <summary>Publishes the shutter's whole-pixel Y delta to Samus's extra displacement.</summary>
    private static void CarrySamusVertically(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState samus)
    {
        if (!state.MovingSamus)
            return;
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(
            (slot.Position.Whole - state.PreviousPosition).Y);
    }

    /// <summary>Enters a timed rest or permanent stop at the upper or lower endpoint.</summary>
    private static void StopVerticalShutter(VerticalShutterEnemyState state, bool atTop)
    {
        int restTime = atTop ? state.MovedUpRestTime : state.MovedDownRestTime;
        if (restTime == PermanentStopRestTime)
        {
            state.Function = VerticalShutterFunction.PermanentNoOp;
            return;
        }
        state.FunctionTimer = WrappingCountdown.Start(restTime);
        state.Function = atTop ? VerticalShutterFunction.StoppedAfterMovingUp : VerticalShutterFunction.StoppedAfterMovingDown;
    }

    /// <summary>
    /// Counts down an endpoint rest before reversing, or before waiting for renewed proximity when trigger mode one would
    /// reverse into the population's primary direction.
    /// </summary>
    private void RunVerticalShutterStopped(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera,
        VerticalDirection end)
    {
        CountdownStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;

        bool reversesIntoPrimary = (state.PrimaryDirection == ShutterHeading.LeftOrUp) != (end == VerticalDirection.Up);
        state.Function = state.TriggerMode == ShutterTriggerMode.HorizontalProximity && reversesIntoPrimary
            ? VerticalShutterFunction.WaitForHorizontalProximity
            : end == VerticalDirection.Up ? VerticalShutterFunction.MovingDown : VerticalShutterFunction.MovingUp;
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>
    /// Decides whether a reaction may start the shutter. Timer, proximity and immediate shutters merely make the acknowledgement sound;
    /// a one-shot reaction reacts once, while a repeatable reaction may be reactivated after every stop.
    /// </summary>
    private void ReactVerticalShutter(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        ReactVerticalShutter(slot, slot.RequireSpeciesState<VerticalShutterEnemyState>(), camera, healthAfterHit: null);

    /// <summary>
    /// Runs a destroyable shutter's reaction after the common damage of a shot or bomb. The hit that destroys it still sets
    /// it off, as a shutter at rest whose killed place lies at the room's top-left corner
    /// (<see cref="DestroyedShutterReaction"/>).
    /// </summary>
    private void ReactDestroyableShutterAfterCommon(RoomEnemySlot slot, EnemyOccupantStates struck) =>
        ReactVerticalShutter(slot, struck.Require<VerticalShutterEnemyState>(), _shutterCamera, healthAfterHit: slot.Health);

    /// <summary>
    /// Decides whether a reaction may start the shutter in <paramref name="slot"/>, whose state is
    /// <paramref name="state"/>. A destroyable shutter's reaction after a hit's damage carries the shutter's
    /// <paramref name="healthAfterHit"/>, null for any other reaction; one the hit destroyed reacts as one at rest.
    /// </summary>
    private void ReactVerticalShutter(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera,
        int? healthAfterHit)
    {
        if (state.TriggerMode is not (ShutterTriggerMode.OneShotReaction or ShutterTriggerMode.RepeatableReaction))
        {
            QueueShutterActivationSoundIfOnScreen(slot, camera);
            return;
        }

        if (state.TriggerMode == ShutterTriggerMode.OneShotReaction)
        {
            if (state.ShotActivated)
                return;
            state.ShotActivated = true;
        }

        bool moving = state.Function is VerticalShutterFunction.MovingUp or VerticalShutterFunction.MovingDown;
        if (healthAfterHit is { } health ? DestroyedShutterReaction.FindsMoving(moving, health) : moving)
        {
            return;
        }

        state.Function = state.ReactionDirection == ShutterHeading.LeftOrUp
            ? VerticalShutterFunction.MovingUp
            : VerticalShutterFunction.MovingDown;
        state.ReactionDirection = Opposite(state.ReactionDirection);
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>Restarts a vertical shutter's selected animation.</summary>
    private static void InstallVerticalShutterProgram(RoomEnemySlot slot, VerticalShutterEnemyState state, ShutterProgram program)
    {
        state.Playback = new(ShutterAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        BeginShutterFrame(frame.Camera);
    }
}
