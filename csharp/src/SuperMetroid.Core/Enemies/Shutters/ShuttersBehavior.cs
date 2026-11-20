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
internal sealed class ShuttersBehavior : IEnemySpecies, IEnemyEntityFactory, IEscapeShutterFlash
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
    /// The destroyable shutters the room has set up, in the order it set them up (#2380): each shows the escape's red
    /// shutter flash while it holds the place it was set up in.
    /// </summary>
    private readonly List<DestroyableShutterEntity> _destroyableShutters = [];

    /// <summary>Keeps <paramref name="shutter"/>, a destroyable shutter the room has just set up, to show the escape's shutter flash.</summary>
    internal void KeepDestroyableShutter(DestroyableShutterEntity shutter) => _destroyableShutters.Add(Ensure.NotNull(shutter));

    /// <summary>
    /// Shows <paramref name="frame"/> of the escape's red shutter flash on every destroyable shutter the room set up that
    /// still holds its place (#1512, #2380): the first escape room's shutters draw in the colours the flash recolours, and
    /// each frame of the flash replaces the one before it. A destroyed shutter, and any enemy that took its place since,
    /// shows no flash.
    /// </summary>
    public void ShowEscapeShutterFlash(ColourEffectFrame frame)
    {
        PictureShade flash = ColourEffectShades.Of(ColourEffectColours.TourianShutters, frame);
        foreach (DestroyableShutterEntity shutter in _destroyableShutters)
            shutter.ShowEscapeShutterFlash(flash);
    }

    /// <summary>
    /// Shows <paramref name="flash"/>, a frame of the escape's red shutter flash, on the shutter in <paramref name="place"/>
    /// in place of the frame it showed before, keeping its other shades and colour effects.
    /// </summary>
    internal static void LayEscapeShutterFlash(RoomEnemyPlace place, PictureShade flash)
    {
        Ensure.NotNull(place);
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

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Kamer, EnemyDefinitionId.ShutterDestroyable, EnemyDefinitionId.ShutterGrowing, EnemyDefinitionId.ShutterShootable];

    /// <summary>The growing shutter, a shootable vertical shutter, the Kamer that reuses it, or a destroyable vertical shutter (#2146, #2179).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Shutters definition.</exception>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(occupant).DefinitionId;
        if (definition == EnemyDefinitionId.ShutterGrowing)
            return EnemyOccupant.Of(new GrowingShutterEntity(this, occupant, place, common));
        if (definition == EnemyDefinitionId.ShutterShootable)
            return EnemyOccupant.Of(new ShootableShutterEntity(this, occupant, place, common));
        if (definition == EnemyDefinitionId.Kamer)
            return EnemyOccupant.Of(new KamerEntity(this, occupant, place, common));
        if (definition == EnemyDefinitionId.ShutterDestroyable)
            return EnemyOccupant.Of(new DestroyableShutterEntity(this, occupant, place, common));
        throw new InvalidOperationException($"The Shutters species creates no {occupant.DefinitionId}.");
    }

    /// <summary>Initializes the growing shutter.</summary>
    internal static void InitializeGrowingShutter(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        GrowingShutterSpawnSettings settings = occupant.SpawnSettings<GrowingShutterSpawnSettings>();

        // The setup selects its first action from what it waits for; the way it grows is the shutter's own.
        GrowingShutterAction action = settings.Wait switch
        {
            GrowingShutterWait.Timer => GrowingShutterAction.WaitForTimer,
            GrowingShutterWait.SamusNear => GrowingShutterAction.WaitForProximity,
            _ => throw new InvalidOperationException($"Growing shutter wait {settings.Wait} is not a wait."),
        };
        // Its sections' origins follow from the spawn row, each eight pixels further along the growth.
        var state = new GrowingShutterEnemyState(action, place.Position.Y.Whole, settings.Growth)
        {
            ActivationTimer = UpdateCountdown.For(settings.ActivationWait + EnemyWaitDefinitions.RunOutUpdate),
        };
        place.SetSpeciesState(state);

        // The shutter's new clock shows no new frame until its animation's first step shows one.
        place.CollidesByFrameHitboxes = false;
        StartGrowingShutterAnimation(
            place,
            state,
            ShutterSegment.QuarterGrown,
            GrowthRadius(state.GrowthLevel));
    }

    /// <summary>Runs the growing shutter's main AI.</summary>
    private void RunGrowingShutterMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        GrowingShutterEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case GrowingShutterAction.WaitForTimer:
                if (AdvanceGrowingShutterTimer(state))
                    ActivateGrowingShutter(place, state, camera);
                return;

            case GrowingShutterAction.WaitForProximity:
                if (IsSamusWithinShutterHorizontalDistance(place, samus, occupant.SpawnSettings<GrowingShutterSpawnSettings>().ActivationDistance))
                    ActivateGrowingShutter(place, state, camera);
                return;

            case GrowingShutterAction.Grow when state.Growth == VerticalDirection.Up:
                GrowShutterUp(occupant, place, state, samus);
                return;

            case GrowingShutterAction.Grow:
                GrowShutterDown(occupant, place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Growing shutter has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Steps the timer-mode wait one update and reports whether it ran out on this update (#2281).</summary>
    private static bool AdvanceGrowingShutterTimer(GrowingShutterEnemyState state) => state.ActivationTimer.StepEnds();

    /// <summary>Switches a timed or proximity shutter into growth and queues audible activation on screen.</summary>
    private void ActivateGrowingShutter(
        RoomEnemyPlace place,
        GrowingShutterEnemyState state,
        Position16<RoomPixels> camera)
    {
        state.Action = GrowingShutterAction.Grow;
        QueueShutterActivationSoundIfOnScreen(place, camera);
    }

    /// <summary>Runs the four growth stages.</summary>
    private static void GrowShutterDown(RoomEnemyOccupant occupant, RoomEnemyPlace place, GrowingShutterEnemyState state)
    {
        if (state.IsFullyGrown)
            return;

        place.Position = place.Position.AdvanceY(occupant.SpawnSettings<GrowingShutterSpawnSettings>().GrowthSpeed);
        int targetY = state.OriginYForLevel() + GrowingShutterDefinitions.SectionLength;

        // A section finishes only once the shutter has moved past its sixteen pixels, not on reaching them; each of the
        // first three is then placed seven pixels short of its full-size end.
        if (targetY >= place.Position.Y.Whole)
            return;
        FinishGrowingShutterSection(place, state, targetY, direction: VerticalDirection.Down);
    }

    /// <summary>Runs the four upward growth stages.</summary>
    private static void GrowShutterUp(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        GrowingShutterEnemyState state,
        SamusState samus)
    {
        if (state.IsFullyGrown)
            return;

        int previousY = place.Position.Y.Whole;
        place.Position = place.Position.AdvanceY(UpwardGrowthVelocity(occupant.SpawnSettings<GrowingShutterSpawnSettings>().GrowthSpeed));
        int targetY = state.OriginYForLevel() - GrowingShutterDefinitions.SectionLength;
        if (targetY < place.Position.Y.Whole)
        {
            CarrySamusWithGrowingShutter(place, samus, previousY);
            return;
        }

        FinishGrowingShutterSection(place, state, targetY, direction: VerticalDirection.Up);
        CarrySamusWithGrowingShutter(place, samus, previousY);
    }

    /// <summary>
    /// The upward growth step: the growth speed, fraction included, reversed to move the shutter up.
    /// </summary>
    private static SignedSixteenSixteen UpwardGrowthVelocity(SignedSixteenSixteen growthSpeed) =>
        SignedSixteenSixteen.Zero - growthSpeed;

    /// <summary>
    /// Adds upward displacement only while Samus is riding the growing platform: the whole pixels it rose this update from
    /// <paramref name="previousY"/>, its row before the move (#2178), as it only ever moves vertically.
    /// </summary>
    private static void CarrySamusWithGrowingShutter(
        RoomEnemyPlace place,
        SamusState samus,
        int previousY)
    {
        if (!EnemyCollision.IsSamusStandingOn(place, samus))
            return;

        int wholeDelta = place.Position.Y.Whole - previousY;
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
        bool finalSection = state.GrowthLevel == GrowingShutterEnemyState.SectionCount - 1;
        int placedY = finalSection
            ? targetY
            : targetY + (GrowingShutterDefinitions.IntermediateInset.Toward(direction.Reversed()));
        place.Position = place.Position.WithWholeY(placedY);
        state.GrowthLevel += 1;

        if (finalSection)
            return;

        ShutterSegment segment = state.GrowthLevel switch
        {
            1 => ShutterSegment.HalfGrown,
            2 => ShutterSegment.ThreeQuartersGrown,
            3 => ShutterSegment.FullyGrown,
            _ => throw new InvalidDataException(
                $"Growing shutter advanced to invalid level {state.GrowthLevel}."),
        };
        StartGrowingShutterAnimation(place, state, segment, GrowthRadius(state.GrowthLevel));
    }

    /// <summary>The vertical radius of a shutter grown <paramref name="level"/> sections past its first.</summary>
    private static int GrowthRadius(int level) => GrowingShutterDefinitions.SectionRadius * (level + 1);

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
    /// <summary>The enemy-library sound queued when an on-screen shutter begins moving.</summary>
    private static readonly SoundEffectId ShutterActivationSound =
        EnemySoundEffectDefinitions.ShutterActivationSound;

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
        Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) < maximumDistance;

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

    /// <summary>Lets bombs trigger shootable vertical shutters without damaging them.</summary>
    /// <param name="occupant">The shutter the bomb struck.</param>
    /// <param name="c">The bomb contact.</param>
    internal void ReactToShootableShutterBomb(RoomEnemyOccupant occupant, EnemyBombContext c) =>
        // A bomb deals the shutter no damage: its collision already marked, it starts the shutter moving as a beam, a
        // missile, Samus's touch or a power bomb does.
        ReactVerticalShutter(occupant, c.Enemy, c.Camera);

    /// <summary>
    /// Runs common bomb damage before reacting a destroyable vertical shutter, as its shot reaction, which a bomb runs,
    /// does.
    /// </summary>
    /// <param name="shutter">The shutter the reaction is built for, whose placement names how it is set off.</param>
    internal EnemyBombReaction DestroyableShutterBombReaction(RoomEnemyOccupant shutter) => new()
    {
        AfterCommon = c => ReactDestroyableShutterAfterCommon(shutter, c.Enemy, c.StruckOccupant, c.Camera),
    };

    /// <summary>Plays a shutter's cue-free animation on to its next frame after its frame expired.</summary>
    private static AnimationPlayback<ShutterSegment> PlayShutterAnimation(RoomEnemyPlace place, AnimationPlayback<ShutterSegment> playback)
    {
        EnemyAnimationPlayback.Play(place, ShutterAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"A shutter's cue-free animation raised {cue}."));
        return playback;
    }

    /// <summary>Advances ShutterGrowing's typed animation.</summary>
    internal static void AdvanceShutterGrowingTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        GrowingShutterEnemyState state = place.RequireSpeciesState<GrowingShutterEnemyState>();
        state.Playback = PlayShutterAnimation(place, state.Playback);
    }

    /// <summary>Advances a vertical shutter's typed animation: a shootable or destroyable shutter's, or the Kamer's.</summary>
    internal static void AdvanceVerticalShutterTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        VerticalShutterEnemyState state = place.RequireSpeciesState<VerticalShutterEnemyState>();
        state.Playback = PlayShutterAnimation(place, state.Playback);
    }

    /// <summary>Runs ShutterGrowing's main AI.</summary>
    internal void RunShutterGrowingMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunGrowingShutterMain(
            occupant, place,
            place.RequireSpeciesState<GrowingShutterEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Runs a vertical shutter's main AI: a shootable or destroyable shutter's, or the Kamer's.</summary>
    internal void RunVerticalShutterMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunVerticalShutterMain(
            occupant, place,
            place.RequireSpeciesState<VerticalShutterEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Uses movement reactions instead of common power-bomb damage for shutter families.</summary>
    /// <param name="shutter">The shutter the reaction is built for, whose placement names how it is set off.</param>
    internal EnemyPowerBombReaction VerticalShutterPowerBombReaction(RoomEnemyOccupant shutter) => new()
    {
        ReplacesCommon = c => ReactVerticalShutter(shutter, c.Enemy, c.Camera),
    };

    /// <summary>Marks a shootable vertical shutter's projectile collision before activating it.</summary>
    /// <param name="shutter">The shutter the reaction is built for, whose placement names how it is set off.</param>
    internal EnemyShotReaction ShootableShutterShotReaction(RoomEnemyOccupant shutter) => new()
    {
        // A shot deals the shutter no damage and starts it moving; the shot's collision is still marked, a Super
        // Missile's quake included.
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            ReactVerticalShutter(shutter, c.Enemy, c.Camera);
            return true;
        },
    };

    /// <summary>Runs common shot damage for destroyable shutters.</summary>
    /// <param name="shutter">The shutter the reaction is built for, whose placement names how it is set off.</param>
    internal EnemyShotReaction DestroyableShutterShotReaction(RoomEnemyOccupant shutter) => new()
    {
        AfterCommon = c => ReactDestroyableShutterAfterCommon(shutter, c.Enemy, c.StruckOccupant, c.Camera),
    };

    /// <summary>Uses Samus contact to react vertical shutters.</summary>
    internal void ReactToVerticalShutterTouch(RoomEnemyOccupant occupant, EnemyTouchContext context) =>
        // Samus's touch deals no contact damage: touching the shutter is itself the trigger, and the shutter's trigger mode
        // decides whether it starts moving.
        ReactVerticalShutter(occupant, context.Place, context.Camera);
    /// <summary>Initializes a vertical shutter, which shows <paramref name="pose"/>: its entity names it (#2179).</summary>
    internal static void InitializeVerticalShutter(RoomEnemyOccupant occupant, RoomEnemyPlace place, ShutterSegment pose)
    {
        ShutterSpawnSettings settings = occupant.SpawnSettings<ShutterSpawnSettings>();
        var state = new VerticalShutterEnemyState(InitialAction(settings.TriggerMode));
        place.SetSpeciesState(state);

        state.ReactionDirection = settings.PrimaryDirection;
        state.ActionTimer = WrappingCountdown.Start(settings.WaitTime);

        // Every placement's travel keeps both endpoints inside the room.
        int spawnY = place.Position.Y.Whole;
        state.UpperEndpointY = spawnY;
        state.LowerEndpointY = spawnY + settings.TravelDistance;
        if (settings.PrimaryDirection == VerticalDirection.Up)
        {
            state.LowerEndpointY = spawnY;
            state.UpperEndpointY = spawnY - settings.TravelDistance;
        }

        // The shutter's new clock shows no new frame until its animation's first step shows one.
        place.CollidesByFrameHitboxes = false;
        StartVerticalShutterAnimation(place, state, pose);
    }

    /// <summary>Runs the vertical shutter's main AI.</summary>
    private void RunVerticalShutterMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
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
                    state.ActionTimer = WrappingCountdown.Start(occupant.SpawnSettings<ShutterSpawnSettings>().WaitTime);
                    ActivateVerticalShutter(occupant, place, state, camera);
                }
                break;

            case VerticalShutterAction.WaitForHorizontalProximity:
                if (IsSamusWithinShutterHorizontalDistance(
                    place,
                    samus,
                    occupant.SpawnSettings<ShutterSpawnSettings>().ProximityDistance))
                {
                    ActivateVerticalShutter(occupant, place, state, camera);
                }
                break;

            case VerticalShutterAction.Activate:
                ActivateVerticalShutter(occupant, place, state, camera);
                break;

            case VerticalShutterAction.Still:
                break;

            case VerticalShutterAction.MovingUp:
                MoveVerticalShutter(occupant, place, state, samus, direction: VerticalDirection.Up);
                break;

            case VerticalShutterAction.MovingDown:
                MoveVerticalShutter(occupant, place, state, samus, direction: VerticalDirection.Down);
                break;

            case VerticalShutterAction.StoppedAfterMovingUp:
                RunVerticalShutterStopped(occupant, place, state, camera, end: VerticalDirection.Up);
                break;

            case VerticalShutterAction.StoppedAfterMovingDown:
                RunVerticalShutterStopped(occupant, place, state, camera, end: VerticalDirection.Down);
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
            ReactVerticalShutter(occupant, place, camera);
        }
    }

    /// <summary>
    /// The action a vertical shutter whose trigger is <paramref name="triggerMode"/> starts in: waiting for its timer or for
    /// Samus, activating on its first update, or standing still until a reaction. The trigger never changes, so
    /// initialization installs it once and the first update already runs it.
    /// </summary>
    private static VerticalShutterAction InitialAction(ShutterTriggerMode triggerMode) => triggerMode switch
    {
        ShutterTriggerMode.Timer => VerticalShutterAction.WaitForTimer,
        ShutterTriggerMode.HorizontalProximity => VerticalShutterAction.WaitForHorizontalProximity,
        ShutterTriggerMode.Immediate => VerticalShutterAction.Activate,
        ShutterTriggerMode.OneShotReaction or ShutterTriggerMode.RepeatableReaction => VerticalShutterAction.Still,
        _ => throw new InvalidDataException($"Vertical shutter trigger mode {triggerMode} is invalid."),
    };

    /// <summary>Starts movement in the primary vertical direction and queues on-screen audio.</summary>
    private void ActivateVerticalShutter(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera)
    {
        state.Action = MovingToward(occupant.SpawnSettings<ShutterSpawnSettings>().PrimaryDirection);
        QueueShutterActivationSoundIfOnScreen(place, camera);
    }

    /// <summary>The action that moves a vertical shutter toward <paramref name="direction"/>.</summary>
    private static VerticalShutterAction MovingToward(VerticalDirection direction) => direction switch
    {
        VerticalDirection.Up => VerticalShutterAction.MovingUp,
        VerticalDirection.Down => VerticalShutterAction.MovingDown,
        _ => throw new InvalidOperationException($"Vertical shutter direction {direction} is not up or down."),
    };

    /// <summary>
    /// The velocity of the vertical shutter in <paramref name="place"/> moving toward <paramref name="direction"/>: its
    /// spawn speed, reversed upward, fraction included (#2278).
    /// </summary>
    internal static SignedSixteenSixteen VerticalShutterVelocity(RoomEnemyOccupant occupant, VerticalDirection direction)
    {
        SignedSixteenSixteen speed = occupant.SpawnSettings<ShutterSpawnSettings>().Speed;
        return speed.Toward(direction);
    }

    /// <summary>Moves toward the upper or lower endpoint, carrying Samus when riding the shutter.</summary>
    private static void MoveVerticalShutter(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        VerticalShutterEnemyState state,
        SamusState samus,
        VerticalDirection direction)
    {
        // The riding test reads where the shutter stood before it moves; a rider is carried by the move's whole pixels.
        int previousY = place.Position.Y.Whole;
        bool carriesSamus = EnemyCollision.IsSamusStandingOn(place, samus);
        place.Position = place.Position.AdvanceY(VerticalShutterVelocity(occupant, direction));
        if (carriesSamus)
            CarrySamusVertically(place, samus, previousY);

        int beforeEndpoint = direction == VerticalDirection.Up
            ? state.UpperEndpointY - place.Position.Y.Whole
            : place.Position.Y.Whole - state.LowerEndpointY;
        if (beforeEndpoint < 0)
            return;
        StopVerticalShutter(occupant, state, direction);
    }

    /// <summary>
    /// Publishes the shutter's whole-pixel Y delta from <paramref name="previousY"/>, its row before this update's move
    /// (#2178), to the riding Samus's extra displacement: a vertical shutter only moves vertically.
    /// </summary>
    private static void CarrySamusVertically(
        RoomEnemyPlace place,
        SamusState samus,
        int previousY) =>
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(
            place.Position.Y.Whole - previousY);

    /// <summary>Enters a timed rest or permanent stop at the endpoint the shutter reached moving toward <paramref name="moved"/>.</summary>
    private static void StopVerticalShutter(RoomEnemyOccupant occupant, VerticalShutterEnemyState state, VerticalDirection moved)
    {
        ShutterSpawnSettings settings = occupant.SpawnSettings<ShutterSpawnSettings>();
        ShutterRest rest = moved == VerticalDirection.Up ? settings.RestAfterMovingUp : settings.RestAfterMovingDown;
        if (rest.IsPermanent)
        {
            state.Action = VerticalShutterAction.Still;
            return;
        }
        state.RestTimer = UpdateCountdown.For(rest.Updates + EnemyWaitDefinitions.RunOutUpdate);
        state.Action = moved == VerticalDirection.Up ? VerticalShutterAction.StoppedAfterMovingUp : VerticalShutterAction.StoppedAfterMovingDown;
    }

    /// <summary>
    /// Counts down an endpoint rest before reversing, or before waiting for renewed proximity when a proximity shutter would
    /// reverse into the population's primary direction.
    /// </summary>
    private void RunVerticalShutterStopped(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera,
        VerticalDirection end)
    {
        if (!state.RestTimer.Step())
            return;

        ShutterSpawnSettings settings = occupant.SpawnSettings<ShutterSpawnSettings>();
        bool reversesIntoPrimary = settings.PrimaryDirection == end.Reversed();
        state.Action = settings.TriggerMode == ShutterTriggerMode.HorizontalProximity && reversesIntoPrimary
            ? VerticalShutterAction.WaitForHorizontalProximity
            : end == VerticalDirection.Up ? VerticalShutterAction.MovingDown : VerticalShutterAction.MovingUp;
        QueueShutterActivationSoundIfOnScreen(place, camera);
    }

    /// <summary>
    /// Decides whether a reaction may start the shutter. Timer, proximity and immediate shutters merely make the acknowledgement sound;
    /// a one-shot reaction reacts once, while a repeatable reaction may be reactivated after every stop.
    /// </summary>
    private void ReactVerticalShutter(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        Position16<RoomPixels> camera) =>
        ReactVerticalShutter(occupant, place, place.RequireSpeciesState<VerticalShutterEnemyState>(), camera);

    /// <summary>
    /// Runs a destroyable shutter's reaction after the common damage of a shot or bomb. The hit that destroys it still sets
    /// it off, as a shutter at rest whose killed place lies at the room's top-left corner: the reaction reads the shutter's
    /// place, which the kill cleared (<see cref="RoomEnemyPlace.FoundState{T}"/>).
    /// </summary>
    private void ReactDestroyableShutterAfterCommon(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyOccupantStates struck, Position16<RoomPixels> camera) =>
        ReactVerticalShutter(occupant, place, struck.Require<VerticalShutterEnemyState>(), camera);

    /// <summary>
    /// Decides whether a reaction may start the shutter in <paramref name="place"/>, whose state is
    /// <paramref name="state"/>. Whether it is already moving is read in its place, so one a hit destroyed reacts as one
    /// at rest.
    /// </summary>
    private void ReactVerticalShutter(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera)
    {
        ShutterTriggerMode trigger = occupant.SpawnSettings<ShutterSpawnSettings>().TriggerMode;
        if (trigger is not (ShutterTriggerMode.OneShotReaction or ShutterTriggerMode.RepeatableReaction))
        {
            QueueShutterActivationSoundIfOnScreen(place, camera);
            return;
        }

        if (trigger == ShutterTriggerMode.OneShotReaction)
        {
            if (state.ShotActivated)
                return;
            state.ShotActivated = true;
        }

        if (place.FoundState<VerticalShutterEnemyState>() is { Action: VerticalShutterAction.MovingUp or VerticalShutterAction.MovingDown })
            return;

        state.Action = MovingToward(state.ReactionDirection);
        state.ReactionDirection = state.ReactionDirection.Reversed();
        QueueShutterActivationSoundIfOnScreen(place, camera);
    }

    /// <summary>Restarts a vertical shutter's selected animation.</summary>
    private static void StartVerticalShutterAnimation(RoomEnemyPlace place, VerticalShutterEnemyState state, ShutterSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, ShutterAnimations.StartingWith(segment));
    }
}
