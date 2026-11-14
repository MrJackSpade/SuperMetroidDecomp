using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>The Shutters species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class ShuttersBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>The room's enemy places, among which the idle contact test finds the enemies Samus met.</summary>
    private readonly EnemySlotTable _places;

    /// <summary>Builds the species owner on the room's enemy audio queue and enemy places.</summary>
    internal ShuttersBehavior(EnemyAudioQueue audio, EnemySlotTable places)
    {
        _audio = audio;
        _places = Ensure.NotNull(places);
    }

    /// <summary>
    /// Shows <paramref name="frame"/> of the escape's red shutter flash on every destroyable shutter among
    /// <paramref name="slots"/> (#1512): the first escape room's shutters draw in the colours the flash recolours, and
    /// each frame of the flash replaces the one before it.
    /// </summary>
    internal static void ShowEscapeShutterFlash(IReadOnlyList<RoomEnemySlot> slots, ColourEffectFrame frame)
    {
        Ensure.NotNull(slots);
        PictureShade flash = ColourEffectShades.Of(ColourEffectColours.TourianShutters, frame);
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

        // The initializer selects its first action from the way it grows and what it waits for.
        bool growsUp = settings.Growth switch
        {
            GrowingShutterGrowth.Down => false,
            GrowingShutterGrowth.Up => true,
            _ => throw new InvalidOperationException($"Growing shutter growth {settings.Growth} is not a direction."),
        };
        state.Action = settings.Wait switch
        {
            GrowingShutterWait.Timer => growsUp
                ? GrowingShutterAction.WaitToGrowUpForTimer
                : GrowingShutterAction.WaitToGrowDownForTimer,
            GrowingShutterWait.SamusNear => growsUp
                ? GrowingShutterAction.WaitToGrowUpForProximity
                : GrowingShutterAction.WaitToGrowDownForProximity,
            _ => throw new InvalidOperationException($"Growing shutter wait {settings.Wait} is not a wait."),
        };

        state.PlaceSectionOrigins(growsUp);
        state.GrowthLevel = 0;

        state.GrowthSpeed = settings.GrowthSpeed;

        slot.CollidesByFrameHitboxes = false;
        slot.AnimationClock.ShowsNewFrame = false;
        StartGrowingShutterAnimation(
            slot,
            state,
            ShutterSegment.GrowingTenPixels,
            yRadius: 8);
    }

    /// <summary>Runs the growing shutter's main AI.</summary>
    private void RunGrowingShutterMain(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case GrowingShutterAction.WaitToGrowDownForTimer:
                if (AdvanceGrowingShutterTimer(state))
                    ActivateGrowingShutter(slot, state, direction: VerticalDirection.Down, camera);
                return;

            case GrowingShutterAction.WaitToGrowUpForTimer:
                if (AdvanceGrowingShutterTimer(state))
                    ActivateGrowingShutter(slot, state, direction: VerticalDirection.Up, camera);
                return;

            case GrowingShutterAction.WaitToGrowDownForProximity:
                if (samus is null)
                    throw new InvalidOperationException("Growing-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(slot, samus, state.ActivationDistance))
                    ActivateGrowingShutter(slot, state, direction: VerticalDirection.Down, camera);
                return;

            case GrowingShutterAction.WaitToGrowUpForProximity:
                if (samus is null)
                    throw new InvalidOperationException("Growing-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(slot, samus, state.ActivationDistance))
                    ActivateGrowingShutter(slot, state, direction: VerticalDirection.Up, camera);
                return;

            case GrowingShutterAction.GrowDown:
                GrowShutterDown(slot, state);
                return;

            case GrowingShutterAction.GrowUp:
                if (samus is null)
                    throw new InvalidOperationException("Upward growing-shutter carry requires Samus state.");
                GrowShutterUp(slot, state, samus);
                return;

            default:
                throw new InvalidDataException(
                    $"Growing shutter has no behaviour for action {state.Action?.ToString() ?? "none"}.");
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
        state.Action = direction == VerticalDirection.Up
            ? GrowingShutterAction.GrowUp
            : GrowingShutterAction.GrowDown;
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
    /// The upward growth step: the exact signed 1/65536-pixel negation of the positive growth speed, borrowing one
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

        ShutterSegment segment = state.GrowthLevel switch
        {
            1 => ShutterSegment.GrowingTwentyPixels,
            2 => ShutterSegment.GrowingThirtyPixels,
            3 => ShutterSegment.FortyPixels,
            _ => throw new InvalidDataException(
                $"Growing shutter advanced to invalid level {state.GrowthLevel}."),
        };
        int radius = 8 + state.GrowthLevel * 8;
        StartGrowingShutterAnimation(slot, state, segment, radius);
    }

    /// <summary>Restarts the next growth pose and updates the shutter's vertical collision radius.</summary>
    private static void StartGrowingShutterAnimation(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        ShutterSegment segment,
        int yRadius)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, ShutterAnimations.StartingWith(segment));
        slot.Radius = slot.Radius.WithY(yRadius);
    }
    /// <summary>The end opposite <paramref name="heading"/>.</summary>
    internal static ShutterHeading Opposite(ShutterHeading heading) => heading switch
    {
        ShutterHeading.LeftOrUp => ShutterHeading.RightOrDown,
        ShutterHeading.RightOrDown => ShutterHeading.LeftOrUp,
        _ => throw new InvalidOperationException($"Undefined {nameof(ShutterHeading)} {heading}."),
    };

    /// <summary>The enemy-library sound queued when an on-screen shutter begins moving.</summary>
    private static readonly SoundEffectId ShutterActivationSound =
        EnemySoundEffectDefinitions.ShutterActivationSound;

    /// <summary>Includes shootable, destroyable, and Kamer definitions in the shared vertical AI.</summary>
    internal static bool IsVerticalShutterDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.ShutterDestroyable or
        EnemyDefinitionId.Kamer;

    /// <summary>
    /// The shared idle-shutter contact test: Samus's solid-enemy contacts this update name this shutter, as
    /// <see cref="SolidContactSlotCombination"/> combines the room's places of the enemies she met, and her
    /// contact-damage index is nonzero.
    /// </summary>
    private bool SamusContactsIdleShutter(RoomEnemySlot slot, SamusState samus) =>
        ReferenceEquals(SolidContactSlotCombination.NamedPlace<ISolidEnemy>(samus.Kinematics.SolidEnemyContacts, _places.Slots), slot) &&
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
    internal void ReactToShootableShutterBomb(EnemyBombContext c) =>
        // The shutter's shot reaction never calls common shot AI. Bomb collision has already marked the physical bomb, then the
        // reaction only advances the shutter state machine exactly as a beam, missile, touch, or power bomb would.
        ReactVerticalShutter(c.Enemy, c.Camera);

    /// <summary>
    /// Runs common bomb damage before reacting a destroyable vertical shutter, as its shot reaction, which a bomb runs,
    /// does.
    /// </summary>
    internal EnemyBombReaction DestroyableShutterBombReaction => field ??= new()
    {
        AfterCommon = c => ReactDestroyableShutterAfterCommon(c.Enemy, c.StruckOccupant, c.Camera),
    };

    /// <summary>Plays a shutter's cue-free animation on to its next frame after its frame expired.</summary>
    private static AnimationPlayback<ShutterSegment> PlayShutterAnimation(RoomEnemySlot slot, AnimationPlayback<ShutterSegment> playback)
    {
        EnemyAnimationPlayback.Play(slot, ShutterAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"A shutter's cue-free animation raised {cue}."));
        return playback;
    }

    /// <summary>Advances ShutterGrowing's typed animation.</summary>
    internal static void AdvanceShutterGrowingTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        GrowingShutterEnemyState state = slot.RequireSpeciesState<GrowingShutterEnemyState>();
        state.Playback = PlayShutterAnimation(slot, state.Playback);
    }

    /// <summary>Advances ShutterShootable's typed animation.</summary>
    internal static void AdvanceShutterShootableTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        VerticalShutterEnemyState state = slot.RequireSpeciesState<VerticalShutterEnemyState>();
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

    /// <summary>Uses movement reactions instead of common power-bomb damage for shutter families.</summary>
    internal EnemyPowerBombReaction VerticalShutterPowerBombReaction => field ??= new()
    {
        ReplacesCommon = c => ReactVerticalShutter(c.Enemy, c.Camera),
    };

    /// <summary>Marks a shootable vertical shutter's projectile collision before activating it.</summary>
    internal EnemyShotReaction ShootableShutterShotReaction => field ??= new()
    {
        // The shot reaction goes directly to the shutter reaction instead of normal shot AI. The collision walker
        // has nevertheless already marked the colliding projectile, including Super Missile quake side effects.
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
            ReactVerticalShutter(c.Enemy, c.Camera);
            return true;
        },
    };

    /// <summary>Runs common shot damage for destroyable shutters.</summary>
    internal EnemyShotReaction DestroyableShutterShotReaction => field ??= new()
    {
        AfterCommon = c => ReactDestroyableShutterAfterCommon(c.Enemy, c.StruckOccupant, c.Camera),
    };

    /// <summary>Uses Samus contact to react vertical shutters.</summary>
    internal void ReactToVerticalShutterTouch(EnemyTouchContext context) =>
        // The touch reaction does not enter common contact damage. Body overlap is itself the trigger, and the reaction test decides
        // whether this population mode may start moving.
        ReactVerticalShutter(context.Slot, context.Camera);
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

        state.MovedUpRest = settings.RestAfterMovingUp;
        state.MovedDownRest = settings.RestAfterMovingDown;

        state.TriggerMode = settings.TriggerMode;
        state.TravelDistance = settings.TravelDistance;
        state.ProximityDistance = settings.ProximityDistance;
        state.WaitTime = settings.WaitTime;
        state.ActionTimer = WrappingCountdown.Start(settings.WaitTime);

        Position16<RoomPixels> spawn = slot.Position.Whole;
        var travel = new Offset16<RoomPixels>(0, state.TravelDistance);
        state.UpperEndpoint = spawn;
        state.LowerEndpoint = spawn + travel;
        if (state.PrimaryDirection == ShutterHeading.LeftOrUp)
        {
            state.LowerEndpoint = spawn;
            state.UpperEndpoint = spawn - travel;
        }

        state.Action = VerticalShutterAction.Initial;
        state.MovingSamus = false;
        state.ShotActivated = false;
        slot.CollidesByFrameHitboxes = false;
        slot.AnimationClock.ShowsNewFrame = false;
        StartVerticalShutterAnimation(
            slot,
            state,
            slot.DefinitionId == EnemyDefinitionId.Kamer ? ShutterSegment.KamerPlatform : ShutterSegment.FortyPixels);
    }

    /// <summary>Runs the vertical shutter's main AI.</summary>
    internal void RunVerticalShutterMain(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        // The initial action calls through its parameter table during this very AI
        // call; it does not spend a frame setting the selected wait action.
        // Keep Initial live until activation writes the moving action, as the game does.
        switch (state.Action == VerticalShutterAction.Initial
            ? SelectInitialVerticalShutterAction(state) : state.Action)
        {
            case VerticalShutterAction.WaitForTimer:
                state.ActionTimer = state.ActionTimer.Decrement().Remaining;
                if (state.ActionTimer.IsZero)
                {
                    state.ActionTimer = WrappingCountdown.Start(state.WaitTime);
                    ActivateVerticalShutter(slot, state, camera);
                }
                break;

            case VerticalShutterAction.WaitForHorizontalProximity:
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

            case VerticalShutterAction.Activate:
                ActivateVerticalShutter(slot, state, camera);
                break;

            case VerticalShutterAction.Still:
                break;

            case VerticalShutterAction.MovingUp:
                if (samus is null)
                    throw new InvalidOperationException("Moving vertical shutter requires Samus state.");
                MoveVerticalShutter(slot, state, samus, direction: VerticalDirection.Up);
                break;

            case VerticalShutterAction.MovingDown:
                if (samus is null)
                    throw new InvalidOperationException("Moving vertical shutter requires Samus state.");
                MoveVerticalShutter(slot, state, samus, direction: VerticalDirection.Down);
                break;

            case VerticalShutterAction.StoppedAfterMovingUp:
                RunVerticalShutterStopped(slot, state, camera, end: VerticalDirection.Up);
                break;

            case VerticalShutterAction.StoppedAfterMovingDown:
                RunVerticalShutterStopped(slot, state, camera, end: VerticalDirection.Down);
                break;

            case VerticalShutterAction.Initial:
            default:
                throw new InvalidDataException(
                    $"Vertical shutter has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }

        // An idle shutter reacts when one of Samus's four solid-enemy collisions
        // names this enemy and she has contact damage (e.g. screw attack).
        if (state.Action is not (VerticalShutterAction.MovingUp or VerticalShutterAction.MovingDown) &&
            SamusContactsIdleShutter(slot, samus ?? throw new InvalidOperationException(
                "Vertical-shutter AI requires Samus state for its contact reaction.")))
        {
            ReactVerticalShutter(slot, camera);
        }
    }

    /// <summary>Selects the vertical population's timer, proximity, activation, or idle initial action.</summary>
    internal static VerticalShutterAction SelectInitialVerticalShutterAction(VerticalShutterEnemyState state)
    {
        return state.TriggerMode switch
        {
            ShutterTriggerMode.Timer => VerticalShutterAction.WaitForTimer,
            ShutterTriggerMode.HorizontalProximity => VerticalShutterAction.WaitForHorizontalProximity,
            ShutterTriggerMode.Immediate => VerticalShutterAction.Activate,
            ShutterTriggerMode.OneShotReaction or ShutterTriggerMode.RepeatableReaction => VerticalShutterAction.Still,
            _ => throw new InvalidDataException($"Vertical shutter trigger mode {state.TriggerMode} is invalid."),
        };
    }

    /// <summary>Starts movement in the primary vertical direction and queues on-screen audio.</summary>
    private void ActivateVerticalShutter(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera)
    {
        state.Action = state.PrimaryDirection == ShutterHeading.LeftOrUp
            ? VerticalShutterAction.MovingUp
            : VerticalShutterAction.MovingDown;
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
        ShutterRest rest = atTop ? state.MovedUpRest : state.MovedDownRest;
        if (rest.IsPermanent)
        {
            state.Action = VerticalShutterAction.Still;
            return;
        }
        state.ActionTimer = WrappingCountdown.Start(rest.Updates);
        state.Action = atTop ? VerticalShutterAction.StoppedAfterMovingUp : VerticalShutterAction.StoppedAfterMovingDown;
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
        CountdownStep timer = state.ActionTimer.Decrement();
        state.ActionTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;

        bool reversesIntoPrimary = (state.PrimaryDirection == ShutterHeading.LeftOrUp) != (end == VerticalDirection.Up);
        state.Action = state.TriggerMode == ShutterTriggerMode.HorizontalProximity && reversesIntoPrimary
            ? VerticalShutterAction.WaitForHorizontalProximity
            : end == VerticalDirection.Up ? VerticalShutterAction.MovingDown : VerticalShutterAction.MovingUp;
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
    private void ReactDestroyableShutterAfterCommon(RoomEnemySlot slot, EnemyOccupantStates struck, Position16<RoomPixels> camera) =>
        ReactVerticalShutter(slot, struck.Require<VerticalShutterEnemyState>(), camera, healthAfterHit: slot.Health);

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

        bool moving = state.Action is VerticalShutterAction.MovingUp or VerticalShutterAction.MovingDown;
        if (healthAfterHit is { } health ? DestroyedShutterReaction.FindsMoving(moving, health) : moving)
        {
            return;
        }

        state.Action = state.ReactionDirection == ShutterHeading.LeftOrUp
            ? VerticalShutterAction.MovingUp
            : VerticalShutterAction.MovingDown;
        state.ReactionDirection = Opposite(state.ReactionDirection);
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>Restarts a vertical shutter's selected animation.</summary>
    private static void StartVerticalShutterAnimation(RoomEnemySlot slot, VerticalShutterEnemyState state, ShutterSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, ShutterAnimations.StartingWith(segment));
    }
}
