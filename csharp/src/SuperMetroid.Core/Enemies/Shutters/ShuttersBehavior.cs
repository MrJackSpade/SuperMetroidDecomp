using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>The Shutters species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ShuttersBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>The room's enemy places, among which the idle contact test finds the enemies Samus met.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>Builds the species owner on the room's enemy audio queue and enemy places.</summary>
    internal ShuttersBehavior(EnemyAudioQueue audio, EnemyPlaces places)
    {
        _audio = audio;
        _places = Ensure.NotNull(places);
    }

    /// <summary>
    /// Shows <paramref name="frame"/> of the escape's red shutter flash on every destroyable shutter among
    /// <paramref name="places"/> (#1512): the first escape room's shutters draw in the colours the flash recolours, and
    /// each frame of the flash replaces the one before it.
    /// </summary>
    internal static void ShowEscapeShutterFlash(IReadOnlyList<RoomEnemyPlace> places, ColourEffectFrame frame)
    {
        Ensure.NotNull(places);
        PictureShade flash = ColourEffectShades.Of(ColourEffectColours.TourianShutters, frame);
        foreach (RoomEnemyPlace place in places)
        {
            if (place.DefinitionId != EnemyDefinitionId.ShutterDestroyable)
                continue;
            PictureShading shown = PictureShading.Standard;
            foreach (PictureShade shade in place.Shading.Shades)
            {
                if (shade.Effect != flash.Effect)
                    shown = shown.With(shade);
            }
            foreach (ColourEffect effect in place.Shading.Effects)
                shown = shown.With(effect);
            place.Shading = shown.With(flash);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Kamer, EnemyDefinitionId.ShutterDestroyable, EnemyDefinitionId.ShutterGrowing, EnemyDefinitionId.ShutterShootable];

    /// <summary>The growing shutter, a shootable vertical shutter or the Kamer that reuses it, or a destroyable vertical shutter (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Shutters definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.ShutterGrowing)
            return new GrowingShutterEntity(this, place, common);
        if (definition is EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer)
            return new ShootableShutterEntity(this, place, common);
        if (definition == EnemyDefinitionId.ShutterDestroyable)
            return new DestroyableShutterEntity(this, place, common);
        throw new InvalidOperationException($"The Shutters species creates no {place.DefinitionId}.");
    }

    /// <summary>Initializes the growing shutter.</summary>
    internal static void InitializeGrowingShutter(RoomEnemyPlace place)
    {
        GrowingShutterSpawnSettings settings = place.SpawnSettings<GrowingShutterSpawnSettings>();

        // The initializer selects its first action from the way it grows and what it waits for.
        bool growsUp = settings.Growth switch
        {
            GrowingShutterGrowth.Down => false,
            GrowingShutterGrowth.Up => true,
            _ => throw new InvalidOperationException($"Growing shutter growth {settings.Growth} is not a direction."),
        };
        GrowingShutterAction action = settings.Wait switch
        {
            GrowingShutterWait.Timer => growsUp
                ? GrowingShutterAction.WaitToGrowUpForTimer
                : GrowingShutterAction.WaitToGrowDownForTimer,
            GrowingShutterWait.SamusNear => growsUp
                ? GrowingShutterAction.WaitToGrowUpForProximity
                : GrowingShutterAction.WaitToGrowDownForProximity,
            _ => throw new InvalidOperationException($"Growing shutter wait {settings.Wait} is not a wait."),
        };
        var state = new GrowingShutterEnemyState(action)
        {
            ActivationTimer = settings.ActivationWait,
            ActivationDistance = settings.ActivationDistance,
        };
        place.SetSpeciesState(state);

        state.PlaceSectionOrigins(place.Position.Y.Whole, growsUp);
        state.GrowthLevel = 0;

        state.GrowthSpeed = settings.GrowthSpeed;

        place.CollidesByFrameHitboxes = false;
        place.AnimationClock.ShowsNewFrame = false;
        StartGrowingShutterAnimation(
            place,
            state,
            ShutterSegment.GrowingTenPixels,
            yRadius: 8);
    }

    /// <summary>Runs the growing shutter's main AI.</summary>
    private void RunGrowingShutterMain(
        RoomEnemyPlace place,
        GrowingShutterEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case GrowingShutterAction.WaitToGrowDownForTimer:
                if (AdvanceGrowingShutterTimer(state))
                    ActivateGrowingShutter(place, state, direction: VerticalDirection.Down, camera);
                return;

            case GrowingShutterAction.WaitToGrowUpForTimer:
                if (AdvanceGrowingShutterTimer(state))
                    ActivateGrowingShutter(place, state, direction: VerticalDirection.Up, camera);
                return;

            case GrowingShutterAction.WaitToGrowDownForProximity:
                if (IsSamusWithinShutterHorizontalDistance(place, samus, state.ActivationDistance))
                    ActivateGrowingShutter(place, state, direction: VerticalDirection.Down, camera);
                return;

            case GrowingShutterAction.WaitToGrowUpForProximity:
                if (IsSamusWithinShutterHorizontalDistance(place, samus, state.ActivationDistance))
                    ActivateGrowingShutter(place, state, direction: VerticalDirection.Up, camera);
                return;

            case GrowingShutterAction.GrowDown:
                GrowShutterDown(place, state);
                return;

            case GrowingShutterAction.GrowUp:
                GrowShutterUp(place, state, samus);
                return;

            default:
                throw new InvalidDataException(
                    $"Growing shutter has no behaviour for action {state.Action}.");
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
        RoomEnemyPlace place,
        GrowingShutterEnemyState state,
        VerticalDirection direction,
        Position16<RoomPixels> camera)
    {
        state.Action = direction == VerticalDirection.Up
            ? GrowingShutterAction.GrowUp
            : GrowingShutterAction.GrowDown;
        QueueShutterActivationSoundIfOnScreen(place, camera);
    }

    /// <summary>Runs the four growth stages.</summary>
    private static void GrowShutterDown(RoomEnemyPlace place, GrowingShutterEnemyState state)
    {
        if (state.GrowthLevel >= 4)
            return;

        place.Position = place.Position.AdvanceY(state.GrowthSpeed);
        int targetY = CoordinateAxis.Wrap(state.OriginYForLevel() + GrowingShutterDefinitions.SectionLength);

        // The non-negative test returns at equality. A section advances only after its center has passed the
        // full sixteen-pixel threshold, after which the first three sections are inset seven.
        if (CoordinateAxis.Difference(targetY, place.Position.Y.Whole) >= 0)
            return;
        FinishGrowingShutterSection(place, state, targetY, direction: VerticalDirection.Down);
    }

    /// <summary>Runs the four upward growth stages.</summary>
    private static void GrowShutterUp(
        RoomEnemyPlace place,
        GrowingShutterEnemyState state,
        SamusState samus)
    {
        if (state.GrowthLevel >= 4)
            return;

        state.PreviousPosition = place.Position.Whole;
        place.Position = place.Position.AdvanceY(UpwardGrowthVelocity(state.GrowthSpeed));
        int targetY = CoordinateAxis.Wrap(state.OriginYForLevel() - GrowingShutterDefinitions.SectionLength);
        if (CoordinateAxis.Precedes(targetY, place.Position.Y.Whole))
        {
            CarrySamusWithGrowingShutter(place, state, samus);
            return;
        }

        FinishGrowingShutterSection(place, state, targetY, direction: VerticalDirection.Up);
        CarrySamusWithGrowingShutter(place, state, samus);
    }

    /// <summary>
    /// The upward growth step: the exact signed 1/65536-pixel negation of the positive growth speed, one whole pixel
    /// further up than the negated whole pixels whenever the fraction is nonzero.
    /// </summary>
    internal static SignedSixteenSixteen UpwardGrowthVelocity(SignedSixteenSixteen growthSpeed) =>
        SignedSixteenSixteen.Zero - growthSpeed;

    /// <summary>Adds upward displacement only while Samus is riding the growing platform.</summary>
    private static void CarrySamusWithGrowingShutter(
        RoomEnemyPlace place,
        GrowingShutterEnemyState state,
        SamusState samus)
    {
        if (!EnemyCollision.IsSamusStandingOn(place, samus))
            return;

        int wholeDelta = (place.Position.Whole - state.PreviousPosition).Y;
        if (wholeDelta < 0)
        {
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, wholeDelta);
        }
    }

    /// <summary>
    /// Snaps a completed section to its target row <paramref name="targetY"/>, enlarges its collision radius, and selects
    /// the next artwork.
    /// </summary>
    internal static void FinishGrowingShutterSection(
        RoomEnemyPlace place,
        GrowingShutterEnemyState state,
        int targetY,
        VerticalDirection direction)
    {
        bool finalSection = state.GrowthLevel == 3;
        int placedY = finalSection
            ? targetY
            : CoordinateAxis.Wrap(targetY + (direction == VerticalDirection.Up
                ? GrowingShutterDefinitions.IntermediateInset
                : -GrowingShutterDefinitions.IntermediateInset));
        place.Position = place.Position.WithWholeY(placedY);
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
        StartGrowingShutterAnimation(place, state, segment, radius);
    }

    /// <summary>Restarts the next growth pose and updates the shutter's vertical collision radius.</summary>
    private static void StartGrowingShutterAnimation(
        RoomEnemyPlace place,
        GrowingShutterEnemyState state,
        ShutterSegment segment,
        int yRadius)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, ShutterAnimations.StartingWith(segment));
        place.Radius = place.Radius.WithY(yRadius);
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
    /// <see cref="SolidContactPlaceCombination"/> combines the room's places of the enemies she met, and her
    /// body is an attack.
    /// </summary>
    private bool SamusContactsIdleShutter(RoomEnemyPlace place, SamusState samus) =>
        ReferenceEquals(SolidContactPlaceCombination.NamedPlace<ISolidEnemy>(samus.Kinematics.SolidEnemyContacts, _places.InUpdateOrder), place) &&
        samus.HorizontalSpeed.BodyAttack != SamusBodyAttack.None;

    /// <summary>Whether Samus lies strictly within <paramref name="maximumDistance"/> of the shutter horizontally.</summary>
    private static bool IsSamusWithinShutterHorizontalDistance(
        RoomEnemyPlace place,
        SamusState samus,
        int maximumDistance) =>
        CoordinateAxis.Distance(samus.Position.X.Whole, place.Position.X.Whole) < maximumDistance;

    /// <summary>
    /// Publishes the activation sound only when the shutter's centre lies in the enemies' square view, which reaches
    /// below the screen's visible lines.
    /// </summary>
    private void QueueShutterActivationSoundIfOnScreen(
        RoomEnemyPlace place,
        Position16<RoomPixels> camera)
    {
        if (EnemyView.Contains(place.Position.Whole, camera))
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
    private static AnimationPlayback<ShutterSegment> PlayShutterAnimation(RoomEnemyPlace place, AnimationPlayback<ShutterSegment> playback)
    {
        EnemyAnimationPlayback.Play(place, ShutterAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"A shutter's cue-free animation raised {cue}."));
        return playback;
    }

    /// <summary>Advances ShutterGrowing's typed animation.</summary>
    internal static void AdvanceShutterGrowingTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        GrowingShutterEnemyState state = place.RequireSpeciesState<GrowingShutterEnemyState>();
        state.Playback = PlayShutterAnimation(place, state.Playback);
    }

    /// <summary>Advances ShutterShootable's typed animation.</summary>
    internal static void AdvanceShutterShootableTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        VerticalShutterEnemyState state = place.RequireSpeciesState<VerticalShutterEnemyState>();
        state.Playback = PlayShutterAnimation(place, state.Playback);
    }

    /// <summary>Runs ShutterGrowing's main AI.</summary>
    internal void RunShutterGrowingMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunGrowingShutterMain(
            place,
            place.RequireSpeciesState<GrowingShutterEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Runs ShutterShootable's main AI.</summary>
    internal void RunShutterShootableMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunVerticalShutterMain(
            place,
            place.RequireSpeciesState<VerticalShutterEnemyState>(),
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
        ReactVerticalShutter(context.Place, context.Camera);
    /// <summary>Initializes a vertical shutter through either initializer and their shared tail.</summary>
    internal static void InitializeVerticalShutter(RoomEnemyPlace place)
    {
        if (!IsVerticalShutterDefinition(place.DefinitionId))
        {
            throw new InvalidDataException(
                $"Definition {place.DefinitionId} does not use vertical-shutter initialization.");
        }

        ShutterSpawnSettings settings = place.SpawnSettings<ShutterSpawnSettings>();
        var state = new VerticalShutterEnemyState(InitialAction(settings.TriggerMode));
        place.SetSpeciesState(state);

        state.PrimaryDirection = settings.PrimaryDirection;
        state.ReactionDirection = state.PrimaryDirection;

        EnemyLinearSpeed speeds = new(settings.Speed);
        state.DownVelocity = speeds.Positive;
        state.UpVelocity = speeds.Negative;

        state.MovedUpRest = settings.RestAfterMovingUp;
        state.MovedDownRest = settings.RestAfterMovingDown;

        state.TriggerMode = settings.TriggerMode;
        state.TravelDistance = settings.TravelDistance;
        state.ProximityDistance = settings.ProximityDistance;
        state.WaitTime = settings.WaitTime;
        state.ActionTimer = WrappingCountdown.Start(settings.WaitTime);

        Position16<RoomPixels> spawn = place.Position.Whole;
        var travel = new Offset16<RoomPixels>(0, state.TravelDistance);
        state.UpperEndpoint = spawn;
        state.LowerEndpoint = spawn + travel;
        if (state.PrimaryDirection == ShutterHeading.LeftOrUp)
        {
            state.LowerEndpoint = spawn;
            state.UpperEndpoint = spawn - travel;
        }

        state.MovingSamus = false;
        state.ShotActivated = false;
        place.CollidesByFrameHitboxes = false;
        place.AnimationClock.ShowsNewFrame = false;
        StartVerticalShutterAnimation(
            place,
            state,
            place.DefinitionId == EnemyDefinitionId.Kamer ? ShutterSegment.KamerPlatform : ShutterSegment.FortyPixels);
    }

    /// <summary>Runs the vertical shutter's main AI.</summary>
    internal void RunVerticalShutterMain(
        RoomEnemyPlace place,
        VerticalShutterEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case VerticalShutterAction.WaitForTimer:
                state.ActionTimer = state.ActionTimer.Decrement().Remaining;
                if (state.ActionTimer.IsZero)
                {
                    state.ActionTimer = WrappingCountdown.Start(state.WaitTime);
                    ActivateVerticalShutter(place, state, camera);
                }
                break;

            case VerticalShutterAction.WaitForHorizontalProximity:
                if (IsSamusWithinShutterHorizontalDistance(
                    place,
                    samus,
                    state.ProximityDistance))
                {
                    ActivateVerticalShutter(place, state, camera);
                }
                break;

            case VerticalShutterAction.Activate:
                ActivateVerticalShutter(place, state, camera);
                break;

            case VerticalShutterAction.Still:
                break;

            case VerticalShutterAction.MovingUp:
                MoveVerticalShutter(place, state, samus, direction: VerticalDirection.Up);
                break;

            case VerticalShutterAction.MovingDown:
                MoveVerticalShutter(place, state, samus, direction: VerticalDirection.Down);
                break;

            case VerticalShutterAction.StoppedAfterMovingUp:
                RunVerticalShutterStopped(place, state, camera, end: VerticalDirection.Up);
                break;

            case VerticalShutterAction.StoppedAfterMovingDown:
                RunVerticalShutterStopped(place, state, camera, end: VerticalDirection.Down);
                break;

            default:
                throw new InvalidDataException(
                    $"Vertical shutter has no behaviour for action {state.Action}.");
        }

        // An idle shutter reacts when one of Samus's four solid-enemy collisions
        // names this enemy and she has contact damage (e.g. screw attack).
        if (state.Action is not (VerticalShutterAction.MovingUp or VerticalShutterAction.MovingDown) &&
            SamusContactsIdleShutter(place, samus))
        {
            ReactVerticalShutter(place, camera);
        }
    }

    /// <summary>
    /// The action a vertical shutter whose trigger is <paramref name="triggerMode"/> starts in: waiting for its timer or for
    /// Samus, activating on its first update, or standing still until a reaction. The trigger never changes, so
    /// initialization installs it once and the first update already runs it.
    /// </summary>
    internal static VerticalShutterAction InitialAction(ShutterTriggerMode triggerMode) => triggerMode switch
    {
        ShutterTriggerMode.Timer => VerticalShutterAction.WaitForTimer,
        ShutterTriggerMode.HorizontalProximity => VerticalShutterAction.WaitForHorizontalProximity,
        ShutterTriggerMode.Immediate => VerticalShutterAction.Activate,
        ShutterTriggerMode.OneShotReaction or ShutterTriggerMode.RepeatableReaction => VerticalShutterAction.Still,
        _ => throw new InvalidDataException($"Vertical shutter trigger mode {triggerMode} is invalid."),
    };

    /// <summary>Starts movement in the primary vertical direction and queues on-screen audio.</summary>
    private void ActivateVerticalShutter(
        RoomEnemyPlace place,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera)
    {
        state.Action = state.PrimaryDirection == ShutterHeading.LeftOrUp
            ? VerticalShutterAction.MovingUp
            : VerticalShutterAction.MovingDown;
        QueueShutterActivationSoundIfOnScreen(place, camera);
    }

    /// <summary>Moves toward the upper or lower endpoint, carrying Samus when riding the shutter.</summary>
    private static void MoveVerticalShutter(
        RoomEnemyPlace place,
        VerticalShutterEnemyState state,
        SamusState samus,
        VerticalDirection direction)
    {
        state.PreviousPosition = place.Position.Whole;
        state.MovingSamus = EnemyCollision.IsSamusStandingOn(place, samus);
        place.Position = place.Position.AdvanceY(direction == VerticalDirection.Up ? state.UpVelocity : state.DownVelocity);
        CarrySamusVertically(place, state, samus);

        int beforeEndpoint = direction == VerticalDirection.Up
            ? (state.UpperEndpoint - place.Position.Whole).Y
            : -(state.LowerEndpoint - place.Position.Whole).Y;
        if (beforeEndpoint < 0)
            return;
        StopVerticalShutter(state, direction == VerticalDirection.Up);
    }

    /// <summary>Publishes the shutter's whole-pixel Y delta to Samus's extra displacement.</summary>
    private static void CarrySamusVertically(
        RoomEnemyPlace place,
        VerticalShutterEnemyState state,
        SamusState samus)
    {
        if (!state.MovingSamus)
            return;
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(
            (place.Position.Whole - state.PreviousPosition).Y);
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
    /// Counts down an endpoint rest before reversing, or before waiting for renewed proximity when a proximity shutter would
    /// reverse into the population's primary direction.
    /// </summary>
    private void RunVerticalShutterStopped(
        RoomEnemyPlace place,
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
        QueueShutterActivationSoundIfOnScreen(place, camera);
    }

    /// <summary>
    /// Decides whether a reaction may start the shutter. Timer, proximity and immediate shutters merely make the acknowledgement sound;
    /// a one-shot reaction reacts once, while a repeatable reaction may be reactivated after every stop.
    /// </summary>
    private void ReactVerticalShutter(
        RoomEnemyPlace place,
        Position16<RoomPixels> camera) =>
        ReactVerticalShutter(place, place.RequireSpeciesState<VerticalShutterEnemyState>(), camera);

    /// <summary>
    /// Runs a destroyable shutter's reaction after the common damage of a shot or bomb. The hit that destroys it still sets
    /// it off, as a shutter at rest whose killed place lies at the room's top-left corner: the reaction reads the shutter's
    /// place, which the kill cleared (<see cref="RoomEnemyPlace.FoundState{T}"/>).
    /// </summary>
    private void ReactDestroyableShutterAfterCommon(RoomEnemyPlace place, EnemyOccupantStates struck, Position16<RoomPixels> camera) =>
        ReactVerticalShutter(place, struck.Require<VerticalShutterEnemyState>(), camera);

    /// <summary>
    /// Decides whether a reaction may start the shutter in <paramref name="place"/>, whose state is
    /// <paramref name="state"/>. Whether it is already moving is read in its place, so one a hit destroyed reacts as one
    /// at rest.
    /// </summary>
    private void ReactVerticalShutter(
        RoomEnemyPlace place,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera)
    {
        if (state.TriggerMode is not (ShutterTriggerMode.OneShotReaction or ShutterTriggerMode.RepeatableReaction))
        {
            QueueShutterActivationSoundIfOnScreen(place, camera);
            return;
        }

        if (state.TriggerMode == ShutterTriggerMode.OneShotReaction)
        {
            if (state.ShotActivated)
                return;
            state.ShotActivated = true;
        }

        if (place.FoundState<VerticalShutterEnemyState>() is { Action: VerticalShutterAction.MovingUp or VerticalShutterAction.MovingDown })
            return;

        state.Action = state.ReactionDirection == ShutterHeading.LeftOrUp
            ? VerticalShutterAction.MovingUp
            : VerticalShutterAction.MovingDown;
        state.ReactionDirection = Opposite(state.ReactionDirection);
        QueueShutterActivationSoundIfOnScreen(place, camera);
    }

    /// <summary>Restarts a vertical shutter's selected animation.</summary>
    private static void StartVerticalShutterAnimation(RoomEnemyPlace place, VerticalShutterEnemyState state, ShutterSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, ShutterAnimations.StartingWith(segment));
    }
}
