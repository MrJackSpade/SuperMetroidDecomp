using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A map, energy, missile or save station (#1554), which replaces the stations' family state and step. The station holds
/// its appearance, its access arm's phase and countdown, and its save handshake as domain values, and advances them on
/// each update: an access Samus triggered starts the arm (or asks to save), the arm extends, holds, activates the
/// station, holds and retracts by its authored timing (<see cref="StationAccessTimingDefinitions"/>), the accepted save
/// alternates its two images for its authored loops (<see cref="SaveStationAnimationDefinitions"/>), and the station's
/// own animation shows its images (<see cref="StationAnimationDefinitions.Frames"/>), the save station's only
/// once.
/// </summary>
/// <param name="kind">Which station it is.</param>
/// <param name="area">The area whose map or save it serves.</param>
/// <param name="animationList">The animation it shows first.</param>
/// <param name="completedAnimationList">The animation it shows once used.</param>
/// <param name="animationFrameCount">The images in its ordinary animation's loop.</param>
internal sealed class StationObject(
    StationKind kind,
    AreaId area,
    StationAnimationList animationList,
    StationAnimationList completedAnimationList,
    int animationFrameCount) : RoomObjectBehaviour
{
    /// <summary>Determines whether activation opens the map, refills a resource, or starts a save.</summary>
    public StationKind Kind { get; } = kind;
    /// <summary>Area whose map-acquired or save-station state is affected.</summary>
    public AreaId AreaIndex { get; } = area;
    /// <summary>Animation the station currently shows.</summary>
    public StationAnimationList AnimationList { get; set; } = animationList;
    /// <summary>Animation selected after the station has completed its first activation.</summary>
    public StationAnimationList CompletedAnimationList { get; } = completedAnimationList;
    /// <summary>Number of images in the current ordinary animation loop.</summary>
    public int AnimationFrameCount { get; } = animationFrameCount;
    /// <summary>Next image of the station's animation.</summary>
    public int AnimationFrame { get; set; }
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
    /// <summary>Remaining two-image save animation loops before the completion message.</summary>
    public int SaveAnimationLoopsRemaining { get; set; }

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
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
                StationKind.Map => !stations.RequireSystem("Map station").HasAreaMap(AreaIndex),
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

        StationAnimationDefinitions.Frame frame = StationAnimations.Resolve(AnimationList, AnimationFrame);
        // A frame's hold is any positive count of updates (#1627).
        if (frame.Duration <= 0)
        {
            throw new InvalidDataException(
                $"{Kind} station animation {AnimationList} frame {AnimationFrame} does not begin with a timed draw.");
        }
        context.Draw(place, frame.Draw);
        AnimationTimer = frame.Duration;
        InitialDrawCompleted = true;
        AnimationFrame = (AnimationFrame + 1) % AnimationFrameCount;
    }

    /// <summary>
    /// The accepted save's first image, drawn at <paramref name="block"/> on the update the save is accepted, which
    /// starts its loops.
    /// </summary>
    internal void StartSaveAnimation(IRoomObjectDrawing drawing, RoomLevelData level, Position16<RoomBlocks> block)
    {
        SavePhase = SaveStationPhase.Animating;
        AnimationFrame = 0;
        SaveAnimationLoopsRemaining = SaveStationAnimationDefinitions.SaveAnimationLoops;
        DrawSaveImage(drawing, level, block);
    }

    /// <summary>Advances the arm's countdown and, when it runs out, the arm's next phase.</summary>
    private void StepAccess(RoomObjectSlot place, RoomObjectStepContext context, StationObjects stations, IRoomSamus samus)
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
                OperationPhase = Kind == StationKind.Map && samus.Control.AlphaLocked
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

    /// <summary>Runs the accepted save's two-image loop until the saved-game message can be published.</summary>
    private bool StepSaveAnimation(RoomObjectSlot place, RoomObjectStepContext context, StationObjects stations)
    {
        if (SavePhase is SaveStationPhase.Idle or SaveStationPhase.AwaitingConfirmation)
            return false;
        if (SavePhase == SaveStationPhase.AwaitingCompletionMessageClose)
            return true;
        if (SavePhase != SaveStationPhase.Animating)
            throw new InvalidDataException($"Save station has unknown phase {SavePhase}.");

        if (--AnimationTimer != 0)
            return true;

        // The loop is counted as its second image ends. After the last of its loops the save publishes the saved-game
        // message instead of returning to the first image.
        if (AnimationFrame == 0 && SaveAnimationLoopsRemaining == 0)
        {
            SavePhase = SaveStationPhase.AwaitingCompletionMessageClose;
            stations.Publish(new StationActivationEvent(
                StationKind.Save,
                GameplayMessageIds.SaveCompleted,
                AreaIndex,
                SavedStation(place),
                place.Block));
            return true;
        }
        DrawSaveImage(context.RoomObjects.Drawing, context.Level, place.Block);
        return true;
    }

    /// <summary>Draws the next of the save's two four-update images.</summary>
    private void DrawSaveImage(IRoomObjectDrawing drawing, RoomLevelData level, Position16<RoomBlocks> block)
    {
        StationAnimationList list = AnimationFrame == 0
            ? StationAnimationList.SaveFirstFrame
            : StationAnimationList.SaveSecondFrame;
        StationAnimationDefinitions.Frame frame = StationAnimations.Resolve(list, 0);
        Ensure.NotNull(drawing).Draw(level, block, frame.Draw);
        AnimationTimer = frame.Duration;
        AnimationFrame ^= 1;
        if (AnimationFrame == 0)
            SaveAnimationLoopsRemaining--;
    }

    /// <summary>Applies the map or refill and publishes the station's message.</summary>
    private void Activate(RoomObjectSlot place, StationObjects stations, IRoomSamus samus)
    {
        GameplayMessageId message = Kind switch
        {
            StationKind.Map => GameplayMessageIds.MapDataAccessCompleted,
            StationKind.Energy => GameplayMessageIds.EnergyRechargeCompleted,
            StationKind.Missile => GameplayMessageIds.MissileRechargeCompleted,
            StationKind.Save => GameplayMessageIds.SaveConfirmation,
            _ => throw new InvalidDataException($"Unknown station kind {Kind}."),
        };

        switch (Kind)
        {
            case StationKind.Map:
                stations.RequireSystem("Map station").SetAreaMapAcquired(AreaIndex);
                AnimationList = CompletedAnimationList;
                break;
            case StationKind.Energy:
                samus.Health = samus.MaxHealth;
                AnimationList = CompletedAnimationList;
                break;
            case StationKind.Missile:
                samus.Missiles = samus.MaxMissiles;
                AnimationList = CompletedAnimationList;
                break;
            case StationKind.Save:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(StationKind)} {Kind}.");
        }

        AnimationFrame = 0;
        AnimationTimer = 1;
        stations.Publish(new StationActivationEvent(
            Kind,
            message,
            AreaIndex,
            (place.Argument as SaveStationArgument)?.Station,
            place.Block));
    }

    /// <summary>The load station a save station saves at, which its activation events carry.</summary>
    private static LoadStation SavedStation(RoomObjectSlot place) => (place.Argument as SaveStationArgument
        ?? throw new InvalidDataException($"Save station at block {place.Block} was placed without its load station.")).Station;

    /// <summary>Draws the access arm at the access Samus used, in the short or extended pose.</summary>
    private void DrawAccess(RoomObjectStepContext context, StationAccessPose pose)
    {
        StationAccessBehavior accessBehavior = AccessBehavior
            ?? throw new InvalidDataException("Station access has no behaviour owner.");
        StationAccessObjectDefinition definition = StationAccessObjects.Resolve(accessBehavior);
        context.RoomObjects.Drawing.Draw(context.Level, AccessBlock ?? throw new InvalidOperationException("A station drew its access arm with no access active."),
            RoomObjectDraw.Of(pose == StationAccessPose.Extended ? definition.Extended : definition.Retracted));
    }
}
