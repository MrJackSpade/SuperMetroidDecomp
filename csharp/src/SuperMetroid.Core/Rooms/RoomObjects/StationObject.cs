using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A map, energy, missile or save station (#1554). The station holds
/// its appearance, its access arm's phase and countdown, and its save handshake as domain values, and advances them on
/// each update: an access Samus triggered starts the arm (or asks to save), the arm extends, holds, activates the
/// station, holds and retracts by its authored timing (<see cref="StationAccessTimingDefinitions"/>), the accepted save
/// flashes its pod's lights for its authored loops, and the station's own animation shows its images
/// (<see cref="StationAnimationDefinitions"/>, #1994), the save station's only once.
/// </summary>
/// <param name="kind">Which station it is.</param>
/// <param name="area">The area whose map or save it serves.</param>
/// <param name="animation">The animation it shows first.</param>
/// <param name="completedAnimation">The animation it shows once used.</param>
/// <param name="savedAt">The load station a save station saves at, as its placement named it; null for any other station.</param>
internal sealed class StationObject(
    StationKind kind,
    AreaId area,
    StationAnimation animation,
    StationAnimation completedAnimation,
    LoadStation? savedAt) : RoomObjectBehaviour
{
    /// <summary>The load station a save station saves at, which its activation events carry; null for any other station.</summary>
    internal LoadStation? SavedAt { get; } = savedAt;

    /// <summary>Determines whether activation opens the map, refills a resource, or starts a save.</summary>
    public StationKind Kind { get; } = kind;
    /// <summary>Area whose map-acquired or save-station state is affected.</summary>
    public AreaId Area { get; } = area;
    /// <summary>Animation the station currently shows.</summary>
    internal StationAnimation ShownAnimation { get; private set; } = animation;
    /// <summary>Animation selected after the station has completed its first activation.</summary>
    internal StationAnimation CompletedAnimation { get; } = completedAnimation;
    /// <summary>Where the station's animation stands.</summary>
    private AnimationPlace _animationPlace = AnimationPlace.StartOfAnimation;
    /// <summary>Where the accepted save's flash stands.</summary>
    private AnimationPlace _flashPlace = AnimationPlace.StartOfAnimation;
    /// <summary>Updates remaining before the station's next image.</summary>
    public int AnimationTimer { get; set; } = 1;
    /// <summary>Keeps the save station's image still after its one initial draw.</summary>
    public bool InitialDrawCompleted { get; set; }
    /// <summary>Records an accepted access for the station's next update.</summary>
    public bool Triggered { get; set; }
    /// <summary>Current extension, hold, retraction, or map-unpause phase.</summary>
    public StationOperationPhase OperationPhase { get; set; }
    /// <summary>Updates remaining in the current access-arm phase.</summary>
    public int OperationTimer { get; set; }
    /// <summary>The access block where the activated access arm is drawn, or null when no access is active.</summary>
    public Position16<RoomBlocks>? AccessBlock { get; set; }
    /// <summary>The access Samus used, which selects the arm's draws.</summary>
    public StationAccessBehavior? AccessBehavior { get; set; }
    /// <summary>Tracks the save's confirmation and animation separately from arm motion.</summary>
    public SaveStationPhase SavePhase { get; set; }

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        StationObjects stations = context.RoomObjects.Stations;
        IRoomSamus samus = stations.RequireSamus($"{Kind} station");

        // The access's first draw installs its six-update countdown on the triggering update itself; the countdown only
        // runs on later updates.
        bool operationStartedThisUpdate = false;
        if (Triggered)
        {
            Triggered = false;
            bool canActivate = Kind switch
            {
                StationKind.Map => !stations.System.HasAreaMap(Area),
                StationKind.Energy => samus.Health != samus.MaxHealth,
                StationKind.Missile => samus.Missiles != samus.MaxMissiles,
                StationKind.Save => true,
                _ => throw new InvalidDataException($"Unknown station kind {Kind}."),
            };
            if (!canActivate && Kind is StationKind.Energy or StationKind.Missile)
            {
                // The full-resource branch releases Samus before the access ends. A refill between the touch and this
                // update must release her too.
                samus.Control.Unlock();
                AccessBlock = null;
                AccessBehavior = null;
            }
            if (canActivate && Kind == StationKind.Save)
            {
                SavePhase = SaveStationPhase.AwaitingConfirmation;
                Activate(place, stations, samus);
            }
            else if (canActivate)
            {
                // The arm draws its first extension image for six updates, holds fully inserted for 96, then activates
                // the station. The touch locked Samus; this update does not lock her again, so one an intervening
                // unpause freed stays free.
                OperationPhase = StationOperationPhase.Extending;
                OperationTimer = StationAccessTimingDefinitions.MovementUpdates;
                operationStartedThisUpdate = true;
                context.QueueSound(EnemySound.StationEngaged, SoundRequestPriority.Normal);
                DrawAccess(context, StationAccessPose.Retracted);
            }
        }

        if (Kind == StationKind.Save && StepSaveAnimation(place, context, stations))
            return;

        if (!operationStartedThisUpdate &&
            OperationPhase is not (StationOperationPhase.Idle or StationOperationPhase.AwaitingMapUnpause))
            StepAccess(place, context, stations, samus);

        // The save station draws once and then stays still until its floor trigger asks to save.
        if (Kind == StationKind.Save && InitialDrawCompleted)
            return;

        if (--AnimationTimer != 0)
            return;

        AnimationShown<StationAnimation, StationDraw> shown = StationAnimationDefinitions.Segments.PlayToNextFrame(
            StationAnimationDefinitions.Animations[ShownAnimation], ref _animationPlace, static _ => { });
        // A looping animation of images always shows its next image, held for any positive count of updates (#1627).
        if (shown is not AnimationShown<StationAnimation, StationDraw>.Frame { Duration: > 0 } frame)
            throw new InvalidDataException($"{Kind} station animation {ShownAnimation} showed {shown} instead of a timed image.");
        context.Draw(place, StationObjectDraws.Family, frame.Shown);
        AnimationTimer = frame.Duration;
        InitialDrawCompleted = true;
    }

    /// <summary>
    /// The accepted save's first image, drawn at <paramref name="block"/> on the update the save is accepted, which
    /// starts its loops.
    /// </summary>
    internal void StartSaveAnimation(IRoomObjectDrawing drawing, RoomLevelData level, Position16<RoomBlocks> block)
    {
        SavePhase = SaveStationPhase.Animating;
        _flashPlace = AnimationPlace.StartOfAnimation;
        if (!DrawSaveImage(drawing, level, block))
            throw new InvalidOperationException("The save station's flash ended before it showed an image.");
    }

    /// <summary>Advances the arm's countdown and, when it runs out, the arm's next phase.</summary>
    private void StepAccess(RoomObjectPlace place, RoomObjectStepContext context, StationObjects stations, IRoomSamus samus)
    {
        if (--OperationTimer != 0)
            return;
        switch (OperationPhase)
        {
            case StationOperationPhase.Extending:
                DrawAccess(context, StationAccessPose.Extended);
                OperationPhase = StationOperationPhase.Extended;
                OperationTimer = StationAccessTimingDefinitions.ExtendedHoldUpdates;
                break;
            case StationOperationPhase.Extended:
                Activate(place, stations, samus);
                // The recharges release Samus right after their message, before the arm retracts. The map leaves her
                // locked for leaving the map screen to release.
                if (Kind != StationKind.Map)
                    samus.Control.Unlock();
                OperationPhase = StationOperationPhase.PostActivationHold;
                OperationTimer = StationAccessTimingDefinitions.MovementUpdates;
                break;
            case StationOperationPhase.PostActivationHold:
                context.QueueSound(EnemySound.StationDisengaged, SoundRequestPriority.Normal);
                OperationPhase = StationOperationPhase.Retracting;
                OperationTimer = StationAccessTimingDefinitions.MovementUpdates;
                break;
            case StationOperationPhase.Retracting:
                DrawAccess(context, StationAccessPose.Retracted);
                OperationPhase = StationOperationPhase.FinalRetractionHold;
                OperationTimer = StationAccessTimingDefinitions.MovementUpdates;
                break;
            case StationOperationPhase.FinalRetractionHold:
                OperationPhase = Kind == StationKind.Map && samus.Control.InputLocked
                    ? StationOperationPhase.AwaitingMapUnpause
                    : StationOperationPhase.Idle;
                AccessBlock = null;
                AccessBehavior = null;
                // The access only ends here. Recharges released Samus at activation; leaving the map screen releases
                // her after the map.
                break;
            case StationOperationPhase.Idle:
            case StationOperationPhase.AwaitingMapUnpause:
            default:
                throw new InvalidDataException($"Unknown station operation phase {OperationPhase}.");
        }
    }

    /// <summary>Runs the accepted save's flash until the saved-game message can be published.</summary>
    private bool StepSaveAnimation(RoomObjectPlace place, RoomObjectStepContext context, StationObjects stations)
    {
        if (SavePhase is SaveStationPhase.Idle or SaveStationPhase.AwaitingConfirmation)
            return false;
        if (SavePhase == SaveStationPhase.AwaitingCompletionMessageClose)
            return true;
        if (SavePhase != SaveStationPhase.Animating)
            throw new InvalidDataException($"Save station has unknown phase {SavePhase}.");

        if (--AnimationTimer != 0)
            return true;

        // After the last of its loops the flash is over, and the save publishes the saved-game message instead of
        // lighting the pod again.
        if (!DrawSaveImage(context.RoomObjects.Drawing, context.Level, place.Block))
        {
            SavePhase = SaveStationPhase.AwaitingCompletionMessageClose;
            stations.Publish(new StationActivationEvent(
                StationKind.Save,
                GameplayMessageId.SaveCompleted,
                Area,
                SavedStation(place),
                place.Block));
            return true;
        }
        return true;
    }

    /// <summary>Draws the save flash's next image and holds it for its updates; false once the flash is over.</summary>
    /// <exception cref="InvalidOperationException">The flash showed neither an image nor its end.</exception>
    private bool DrawSaveImage(IRoomObjectDrawing drawing, RoomLevelData level, Position16<RoomBlocks> block)
    {
        Animation<StationAnimation> flash = StationAnimationDefinitions.Animations[StationAnimation.SaveFlash];
        switch (StationAnimationDefinitions.Segments.PlayToNextFrame(flash, ref _flashPlace, static _ => { }))
        {
            case AnimationShown<StationAnimation, StationDraw>.Frame frame:
                StationObjectDraws.Family.Draw(level, block, Ensure.NotNull(drawing), frame.Shown);
                AnimationTimer = frame.Duration;
                return true;
            case AnimationShown<StationAnimation, StationDraw>.Ended:
                return false;
            case var shown:
                throw new InvalidOperationException($"The save station's flash showed {shown}.");
        }
    }

    /// <summary>Applies the map or refill and publishes the station's message.</summary>
    private void Activate(RoomObjectPlace place, StationObjects stations, IRoomSamus samus)
    {
        GameplayMessageId message = Kind switch
        {
            StationKind.Map => GameplayMessageId.MapDataAccessCompleted,
            StationKind.Energy => GameplayMessageId.EnergyRechargeCompleted,
            StationKind.Missile => GameplayMessageId.MissileRechargeCompleted,
            StationKind.Save => GameplayMessageId.SaveConfirmation,
            _ => throw new InvalidDataException($"Unknown station kind {Kind}."),
        };

        switch (Kind)
        {
            case StationKind.Map:
                stations.System.SetAreaMapAcquired(Area);
                ShownAnimation = CompletedAnimation;
                break;
            case StationKind.Energy:
                samus.Health = samus.MaxHealth;
                ShownAnimation = CompletedAnimation;
                break;
            case StationKind.Missile:
                samus.Missiles = samus.MaxMissiles;
                ShownAnimation = CompletedAnimation;
                break;
            case StationKind.Save:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(StationKind)} {Kind}.");
        }

        _animationPlace = AnimationPlace.StartOfAnimation;
        AnimationTimer = 1;
        stations.Publish(new StationActivationEvent(
            Kind,
            message,
            Area,
            SavedAt,
            place.Block));
    }

    /// <summary>The load station a save station saves at, which its activation events carry.</summary>
    /// <exception cref="InvalidDataException">The save station was placed without its load station.</exception>
    private LoadStation SavedStation(RoomObjectPlace place) => SavedAt
        ?? throw new InvalidDataException($"Save station at block {place.Block} was placed without its load station.");

    /// <summary>Draws the access arm at the access Samus used, in the short or extended pose.</summary>
    private void DrawAccess(RoomObjectStepContext context, StationAccessPose pose)
    {
        StationAccessBehavior accessBehavior = AccessBehavior
            ?? throw new InvalidDataException("Station access has no behaviour owner.");
        StationAccessObjectDefinition definition = StationAccessObjects.Resolve(accessBehavior);
        StationObjectDraws.Family.Draw(context.Level, AccessBlock ?? throw new InvalidOperationException("A station drew its access arm with no access active."),
            context.RoomObjects.Drawing, pose == StationAccessPose.Extended ? definition.Extended : definition.Retracted);
    }
}
