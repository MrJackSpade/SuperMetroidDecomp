using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Shutters.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>The Shutters species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ShuttersBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. ShuttersPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ShuttersEntity(this, slot, common);

    /// <summary>Each growing section advances sixteen pixels before its final placement.</summary>
    private const short GrowingShutterSectionLength = 16;
    /// <summary>Intermediate sections stop seven pixels inside their full-size endpoint.</summary>
    private const short GrowingShutterIntermediateInset = 7;
    /// <summary>AI updates of endpoint rest per unit of a shutter's population rest parameter.</summary>
    private const int RestUpdatesPerParameterUnit = 16;

    /// <summary>Ports <c>GrowingShutter_Init</c> at $A2:E9DA.</summary>
    internal static void InitializeGrowingShutter(RoomEnemySlot slot)
    {
        var state = new GrowingShutterEnemyState(slot)
        {
            ActivationDistance = slot.Spawn.Population.Parameter1,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // The initializer consumes init0 and the low extra-property word as a two-bit table
        // selector before clearing the latter. Keep native dispatch ordering independent
        // of the origin direction encoded below.
        int initialFunctionIndex = slot.ExtraProperties.ToWord() * 2 + slot.CurrentInstruction;
        state.Function = GrowingShutters.InitialFunction(initialFunctionIndex);

        bool growsUp = slot.ExtraProperties.ToWord() != 0;
        state.PlaceSectionOrigins(growsUp);
        state.GrowthLevel = 0;

        state.GrowthSpeed = GrowingShutters.Speed(checked((ushort)slot.Spawn.Population.Parameter2));

        slot.ExtraProperties = default;
        InstallGrowingShutterProgram(
            slot,
            state,
            ShutterProgram.GrowingTenPixels,
            yRadius: 8);
    }

    /// <summary>Ports <c>GrowingShutter_Main</c> at $A2:EAB6.</summary>
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
    /// Native code tests the timer before decrementing it. Zero therefore activates on the
    /// current call, while a positive N waits N complete calls and activates on call N + 1.
    /// </summary>
    internal static bool AdvanceGrowingShutterTimer(GrowingShutterEnemyState state)
    {
        if (state.ActivationTimer.IsZero)
            return true;
        state.ActivationTimer = state.ActivationTimer.DecrementToZero();
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

    /// <summary>Ports the four growth stages at $A2:EB11-$EC12.</summary>
    private static void GrowShutterDown(RoomEnemySlot slot, GrowingShutterEnemyState state)
    {
        if (state.GrowthLevel >= 4)
            return;

        slot.Position = slot.Position.AdvanceY(state.GrowthSpeed);
        Position16<RoomPixels> target = state.OriginForLevel() + new Offset16<RoomPixels>(0, GrowingShutterSectionLength);

        // BPL returns at equality. A section advances only after its center has passed the
        // full sixteen-pixel threshold, after which the first three sections are inset seven.
        if ((target - slot.Position.Whole).Y >= 0)
            return;
        FinishGrowingShutterSection(slot, state, target, direction: VerticalDirection.Down);
    }

    /// <summary>Ports the four upward growth stages at $A2:EC13-$ED20.</summary>
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
    /// whole pixel from the negated whole word whenever the fraction is nonzero.
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
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, checked((short)wholeDelta));
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
                : (short)-GrowingShutterIntermediateInset);
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
        int radius = checked((ushort)(8 + state.GrowthLevel * 8));
        InstallGrowingShutterProgram(slot, state, program, radius);
    }

    /// <summary>Restarts the next growth pose and updates the shutter's vertical collision radius.</summary>
    private static void InstallGrowingShutterProgram(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        ShutterProgram program,
        int yRadius)
    {
        state.Program = AnimationProgramCursor<ShutterProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        slot.Radius = slot.Radius.WithY(checked((ushort)yRadius));
    }
    /// <summary>Samus's four-pixel correction when pushing against a horizontal shutter.</summary>
    private const short HorizontalShutterManualPushPixels = 4;

    /// <summary>Ports <c>HorizontalShutter_Init</c> at $A2:F111/$A2:F11E.</summary>
    internal static void InitializeHorizontalShutter(RoomEnemySlot slot)
    {
        var state = new HorizontalShutterEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        int packedSpeedAndDirection = checked((ushort)slot.CurrentInstruction);
        state.SpeedTableIndex = unchecked((byte)packedSpeedAndDirection);
        state.PrimaryDirection = unchecked((byte)(EightEight.WholeOf(packedSpeedAndDirection)));
        state.ReactionDirection = state.PrimaryDirection ^ 1;

        int speedRecordOffset = state.SpeedTableIndex * 8;
        state.RightVelocity = ReadShutterLinearSpeed(speedRecordOffset);
        state.LeftVelocity = ReadShutterLinearSpeed(speedRecordOffset + 4);

        PopulationParameterSettings restParameters = PopulationParameterSettings.Of(slot.ExtraProperties.ToWord());
        state.MovedLeftRestParameter = restParameters.First;
        state.MovedRightRestParameter = restParameters.Second;
        state.MovedLeftRestTime = state.MovedLeftRestParameter * RestUpdatesPerParameterUnit;
        state.MovedRightRestTime = state.MovedRightRestParameter * RestUpdatesPerParameterUnit;

        state.TriggerMode = DecodeTriggerMode(unchecked((byte)slot.Spawn.Population.Parameter1), "Horizontal");
        state.TravelDistance = slot.Spawn.Population.Parameter1Settings.Second;
        state.HorizontalProximityOrWaitTime = slot.Spawn.Population.Parameter2;
        state.FunctionTimer = WrappingCountdown.FromRaw(checked((ushort)slot.Spawn.Population.Parameter2));

        Position16<RoomPixels> spawn = slot.Position.Whole;
        var travel = new Offset16<RoomPixels>(checked((short)state.TravelDistance), 0);
        state.LeftEndpoint = spawn;
        state.RightEndpoint = spawn + travel;
        if (state.PrimaryDirection == 0)
        {
            state.RightEndpoint = spawn;
            state.LeftEndpoint = spawn - travel;
        }

        state.Function = HorizontalShutterFunction.Initial;
        state.MovingSamus = false;
        state.ShotActivated = false;
        slot.ExtraProperties = default;
        InstallHorizontalShutterProgram(slot, state);
    }

    /// <summary>Ports <c>HorizontalShutter_Main</c> at $A2:F1DE.</summary>
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
                    state.FunctionTimer = WrappingCountdown.Start(state.HorizontalProximityOrWaitTime);
                    ActivateHorizontalShutter(state);
                }
                break;

            case HorizontalShutterFunction.WaitForHorizontalProximity:
                if (IsSamusWithinShutterHorizontalDistance(
                    slot,
                    samus,
                    state.HorizontalProximityOrWaitTime))
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

        // $A2:F1E4-$A2:F211: an idle shutter reacts when Samus's four solid-enemy collision
        // words AND to this enemy's index and she has contact damage (e.g. screw attack).
        if (state.Function is not (HorizontalShutterFunction.MovingLeft or HorizontalShutterFunction.MovingRight) &&
            SamusContactsIdleShutter(slot, samus))
        {
            ReactHorizontalShutter(slot);
        }
    }

    /// <summary>Decodes a population's trigger-mode byte, the index into each shutter's five-entry initial-function table.</summary>
    /// <exception cref="InvalidDataException">The byte is past the table's five entries.</exception>
    internal static ShutterTriggerMode DecodeTriggerMode(int mode, string shutter) => mode switch
    {
        0 => ShutterTriggerMode.Timer,
        1 => ShutterTriggerMode.HorizontalProximity,
        2 => ShutterTriggerMode.Immediate,
        3 => ShutterTriggerMode.OneShotReaction,
        4 => ShutterTriggerMode.RepeatableReaction,
        _ => throw new InvalidDataException($"{shutter} shutter trigger mode {mode} exceeds its five-entry ROM table."),
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
        state.Function = state.PrimaryDirection == 0
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
        bool samusLeftOfShutter = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
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
        bool reversesIntoPrimary = (state.PrimaryDirection == 0) != (end == HorizontalDirection.Left);
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

    /// <summary>Ports the stationary/manual ejection portion of touch AI $A2:F3D8.</summary>
    private static void TouchHorizontalShutter(
        RoomEnemySlot slot,
        SamusState samus,
        ControllerButton controllerInput)
    {
        HorizontalShutterEnemyState state = slot.RequireSpeciesState<HorizontalShutterEnemyState>();
        if (state.Function != HorizontalShutterFunction.PermanentNoOp)
            return;

        bool samusIsLeft = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
        if (samusIsLeft && controllerInput.HasAny(ControllerButton.Right))
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeX(-HorizontalShutterManualPushPixels).WithFractionY(0);
        }
        else if (!samusIsLeft && controllerInput.HasAny(ControllerButton.Left))
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeX(HorizontalShutterManualPushPixels).WithFractionY(0);
        }
    }

    /// <summary>Ports the movement-trigger tail shared by shot and power-bomb AI $A2:F41A.</summary>
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

        state.ReactionDirection ^= 1;
        state.Function = state.ReactionDirection == 0
            ? HorizontalShutterFunction.MovingLeft
            : HorizontalShutterFunction.MovingRight;
    }

    /// <summary>Restarts the stationary shutter pose after movement or reaction.</summary>
    private static void InstallHorizontalShutterProgram(RoomEnemySlot slot, HorizontalShutterEnemyState state)
    {
        state.Program = AnimationProgramCursor<ShutterProgram>.Start(ShutterProgram.Horizontal);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Rest length, in AI updates, that selects a permanent stop at an endpoint instead of a timed rest.</summary>
    private const int PermanentStopRestTime = 4080;

    /// <summary>The library-two sound queued when an on-screen shutter begins moving.</summary>
    private static readonly SoundEffectId ShutterActivationSound =
        EnemySoundEffectDefinitions.ShutterActivationSound;

    /// <summary>Frame camera used to admit activation sound only for on-screen shutters.</summary>
    private Position16<RoomPixels> _shutterCamera;

    /// <summary>Last library-two shutter sound queued during the current enemy frame.</summary>
    public SoundEffectId? LastShutterSoundEffect { get; private set; }

    /// <summary>Captures the current camera and clears the prior frame's activation sound.</summary>
    internal void BeginShutterFrame(Position16<RoomPixels> camera)
    {
        _shutterCamera = camera;
        LastShutterSoundEffect = null;
    }

    /// <summary>Includes shootable, destroyable, and Kamer definitions in the shared vertical AI.</summary>
    internal static bool IsVerticalShutterDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.ShutterDestroyable or
        EnemyDefinitionId.Kamer;

    /// <summary>Reads one signed 16.16 velocity from the common linear-speed table at <paramref name="byteOffset"/>.</summary>
    private static SignedSixteenSixteen ReadShutterLinearSpeed(int byteOffset)
    {
        SignedSixteenSixteen speed = EnemyMath.ReadLinearEnemySpeed(checked((ushort)byteOffset));
        return speed;
    }

    /// <summary>
    /// The shared idle-shutter contact test: Samus's solid-enemy contacts this update name this shutter, as
    /// <see cref="SolidContactSlotCombination"/> combines them, and her contact-damage index is nonzero.
    /// </summary>
    private static bool SamusContactsIdleShutter(RoomEnemySlot slot, SamusState samus) =>
        SolidContactSlotCombination.Combine(samus.Kinematics.SolidEnemyCollisions, EnemySlotOrder.Capacity) == slot.SlotIndex &&
        samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None;

    /// <summary>Tests the native strict, wrapped absolute-X proximity threshold.</summary>
    private static bool IsSamusWithinShutterHorizontalDistance(
        RoomEnemySlot slot,
        SamusState samus,
        int maximumDistance)
    {
        int distance = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        if (unchecked((short)distance) < 0)
            distance = unchecked((ushort)-distance);
        return distance < maximumDistance;
    }

    /// <summary>
    /// Ports $A0:AD70. Its bottom edge is 256 pixels, deliberately not the 224-line visible
    /// viewport, because the original sound admission test uses a square screen window.
    /// </summary>
    private static bool IsShutterCenterOnScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        unchecked((short)(slot.Position.X.Whole - camera.X)) >= 0 &&
        unchecked((short)(camera.X + 256 - slot.Position.X.Whole)) >= 0 &&
        unchecked((short)(slot.Position.Y.Whole - camera.Y)) >= 0 &&
        unchecked((short)(camera.Y + 256 - slot.Position.Y.Whole)) >= 0;

    /// <summary>Publishes the activation sound only when the shutter center lies in the 256-pixel camera square.</summary>
    private void QueueShutterActivationSoundIfOnScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera)
    {
        if (IsShutterCenterOnScreen(slot, camera))
            LastShutterSoundEffect = ShutterActivationSound;
    }

    /// <summary>Lets bombs trigger shootable vertical shutters without ordinary damage AI.</summary>
    internal void AddShuttersBombReactions(EnemySharedReactions table)
    {
        // `$A2:F0A2` never calls common shot AI. Bomb collision has already marked the physical bomb, then the
        // callback only advances the shutter state machine exactly as a beam, missile, touch, or power bomb would.
        table.AddBombSpecial(c => c.Enemy.DefinitionId is EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer &&
                                  c.SelectedShotAi == EnemyInteractionCallback.ShootableVerticalShutterShot,
            c => ReactVerticalShutter(c.Enemy, _shutterCamera));
    }

    /// <summary>Runs common bomb damage before reacting a destroyable vertical shutter.</summary>
    internal void AddDestroyableShutterBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.ShutterDestroyable &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.DestroyableVerticalShutterShot)
        {
            AfterCommon = c => ReactVerticalShutter(c.Enemy, _shutterCamera),
        });
    }

    /// <summary>Advances ShutterGrowing's typed animation.</summary>
    internal static void AdvanceShutterGrowingTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        GrowingShutterEnemyState state = slot.RequireSpeciesState<GrowingShutterEnemyState>();
        AnimationProgramCursor<ShutterProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, ShutterAnimations.Programs, ref cursor, ShutterFrameDefinitions.ShownFrames.Of);
        state.Program = cursor;
    }

    /// <summary>Advances ShutterShootable's typed animation.</summary>
    internal static void AdvanceShutterShootableTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        VerticalShutterEnemyState state = slot.RequireSpeciesState<VerticalShutterEnemyState>();
        AnimationProgramCursor<ShutterProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, ShutterAnimations.Programs, ref cursor, ShutterFrameDefinitions.ShownFrames.Of);
        state.Program = cursor;
    }

    /// <summary>Advances ShutterHorizShootable's typed animation.</summary>
    internal static void AdvanceShutterHorizShootableTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        HorizontalShutterEnemyState state = slot.RequireSpeciesState<HorizontalShutterEnemyState>();
        AnimationProgramCursor<ShutterProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, ShutterAnimations.Programs, ref cursor, ShutterFrameDefinitions.ShownFrames.Of);
        state.Program = cursor;
    }

    /// <summary>Ports ShutterGrowing's main AI.</summary>
    internal void RunShutterGrowingMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunGrowingShutterMain(
            slot,
            slot.RequireSpeciesState<GrowingShutterEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Ports ShutterShootable's main AI.</summary>
    internal void RunShutterShootableMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunVerticalShutterMain(
            slot,
            slot.RequireSpeciesState<VerticalShutterEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Ports ShutterHorizShootable's main AI.</summary>
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
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.VerticalShutterPowerBomb)
        {
            ReplacesCommon = (enemy, _) => ReactVerticalShutter(enemy, _shutterCamera),
        });
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.ShutterHorizShootable &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.HorizontalShutterPowerBomb)
        {
            ReplacesCommon = (enemy, _) => ReactHorizontalShutter(enemy),
        });
    }

    /// <summary>Marks a shootable vertical shutter's projectile collision before activating it.</summary>
    internal void AddShuttersShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId is EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.ShootableVerticalShutterShot)
        {
            // $F0A2 goes directly to the shutter reaction instead of normal shot AI. The bank-$A0 collision walker
            // has nevertheless already marked the colliding projectile, including Super Missile quake side effects.
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                ReactVerticalShutter(c.Enemy, _shutterCamera);
                return true;
            },
        });
    }

    /// <summary>Runs common shot damage for destroyable shutters and movement reactions for horizontal ones.</summary>
    internal void AddDestroyableAndHorizontalShutterShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.ShutterDestroyable &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.DestroyableVerticalShutterShot)
        {
            AfterCommon = c => ReactVerticalShutter(c.Enemy, _shutterCamera),
        });

        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.ShutterHorizShootable &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.HorizontalShutterShot)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = c => ReactHorizontalShutter(c.Enemy),
        });
    }

    /// <summary>Uses Samus contact to react vertical shutters or manually eject her from stopped horizontal ones.</summary>
    internal void AddShuttersTouch(EnemySharedReactions table)
    {
        // $F09D does not enter common contact damage. Body overlap is itself the trigger, and $F0B6 decides
        // whether this population mode may start moving.
        table.AddTouch(slot => IsVerticalShutterDefinition(slot.DefinitionId) &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.VerticalShutterTouch,
            context => ReactVerticalShutter(context.Slot, _shutterCamera));

        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.ShutterHorizShootable &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.HorizontalShutterTouch,
            context =>
            {
                // $F3D8 provides manual ejection only after a permanently stopped shutter.
                // The main AI's subsequent nonzero contact-damage-index check reuses the
                // shot reaction, allowing Screw/Speed contact to trigger modes three/four.
                TouchHorizontalShutter(context.Slot, context.Samus, context.ControllerInput);
                if (context.ContactDamageIndex != SamusContactDamageIndex.None)
                    ReactHorizontalShutter(context.Slot);
            });
    }
    /// <summary>Ports $A2:EE05/$A2:EE12 and their shared tail at $A2:EE1F.</summary>
    internal static void InitializeVerticalShutter(RoomEnemySlot slot)
    {
        if (!IsVerticalShutterDefinition(slot.DefinitionId))
        {
            throw new InvalidDataException(
                $"Definition {slot.DefinitionId} does not use vertical-shutter initialization.");
        }

        var state = new VerticalShutterEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        int packedSpeedAndDirection = checked((ushort)slot.CurrentInstruction);
        state.SpeedTableIndex = unchecked((byte)packedSpeedAndDirection);
        state.PrimaryDirection = unchecked((byte)(EightEight.WholeOf(packedSpeedAndDirection)));
        state.ReactionDirection = state.PrimaryDirection;

        int speedRecordOffset = state.SpeedTableIndex * 8;
        state.DownVelocity = ReadShutterLinearSpeed(speedRecordOffset);
        state.UpVelocity = ReadShutterLinearSpeed(speedRecordOffset + 4);

        PopulationParameterSettings restParameters = PopulationParameterSettings.Of(slot.ExtraProperties.ToWord());
        state.MovedUpRestParameter = restParameters.First;
        state.MovedDownRestParameter = restParameters.Second;
        state.MovedUpRestTime = state.MovedUpRestParameter * RestUpdatesPerParameterUnit;
        state.MovedDownRestTime = state.MovedDownRestParameter * RestUpdatesPerParameterUnit;

        state.TriggerMode = DecodeTriggerMode(unchecked((byte)slot.Spawn.Population.Parameter1), "Vertical");
        state.TravelDistance = slot.Spawn.Population.Parameter1Settings.Second;
        state.HorizontalProximityOrWaitTime = slot.Spawn.Population.Parameter2;
        state.FunctionTimer = WrappingCountdown.FromRaw(checked((ushort)slot.Spawn.Population.Parameter2));

        Position16<RoomPixels> spawn = slot.Position.Whole;
        var travel = new Offset16<RoomPixels>(0, checked((short)state.TravelDistance));
        state.UpperEndpoint = spawn;
        state.LowerEndpoint = spawn + travel;
        if (state.PrimaryDirection == 0)
        {
            state.LowerEndpoint = spawn;
            state.UpperEndpoint = spawn - travel;
        }

        state.Function = VerticalShutterFunction.Initial;
        state.MovingSamus = false;
        state.ShotActivated = false;
        slot.ExtraProperties = default;
        InstallVerticalShutterProgram(
            slot,
            state,
            slot.DefinitionId == EnemyDefinitionId.Kamer ? ShutterProgram.KamerPlatform : ShutterProgram.FortyPixels);
    }

    /// <summary>Ports <c>VerticalShutter_Main</c> at $A2:EED1.</summary>
    internal void RunVerticalShutterMain(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        // The initial function JSRs through its parameter table during this very AI
        // call; it does not spend a frame installing the selected wait function.
        // Keep Initial live until activation writes the moving function, as native does.
        switch (state.Function == VerticalShutterFunction.Initial
            ? SelectInitialVerticalShutterFunction(state) : state.Function)
        {
            case VerticalShutterFunction.WaitForTimer:
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsZero)
                {
                    state.FunctionTimer = WrappingCountdown.Start(state.HorizontalProximityOrWaitTime);
                    ActivateVerticalShutter(slot, state, camera);
                }
                break;

            case VerticalShutterFunction.WaitForHorizontalProximity:
                if (samus is null)
                    throw new InvalidOperationException("Vertical-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(
                    slot,
                    samus,
                    state.HorizontalProximityOrWaitTime))
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

        // $A2:EED7-$A2:EF04: an idle shutter reacts when Samus's four solid-enemy collision
        // words AND to this enemy's index and she has contact damage (e.g. screw attack).
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
        state.Function = state.PrimaryDirection == 0
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

        bool reversesIntoPrimary = (state.PrimaryDirection == 0) != (end == VerticalDirection.Up);
        state.Function = state.TriggerMode == ShutterTriggerMode.HorizontalProximity && reversesIntoPrimary
            ? VerticalShutterFunction.WaitForHorizontalProximity
            : end == VerticalDirection.Up ? VerticalShutterFunction.MovingDown : VerticalShutterFunction.MovingUp;
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>
    /// Ports $A2:F0B6. Timer, proximity and immediate shutters merely make the acknowledgement sound;
    /// a one-shot reaction reacts once, while a repeatable reaction may be reactivated after every stop.
    /// </summary>
    private void ReactVerticalShutter(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera)
    {
        VerticalShutterEnemyState state = slot.RequireSpeciesState<VerticalShutterEnemyState>();
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

        if (state.Function is VerticalShutterFunction.MovingUp or VerticalShutterFunction.MovingDown)
            return;

        state.Function = state.ReactionDirection == 0
            ? VerticalShutterFunction.MovingUp
            : VerticalShutterFunction.MovingDown;
        state.ReactionDirection ^= 1;
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>Restarts a vertical shutter's selected animation.</summary>
    private static void InstallVerticalShutterProgram(RoomEnemySlot slot, VerticalShutterEnemyState state, ShutterProgram program)
    {
        state.Program = AnimationProgramCursor<ShutterProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        BeginShutterFrame(frame.Camera);
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastShutterSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max6);
    }
}
