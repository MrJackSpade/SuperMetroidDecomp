using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A map, energy or missile station (#1554, #2484), used through the access arm at the access block Samus runs into. The
/// station holds its animation and its arm's phase and use as domain values, and advances them on each update: an access
/// Samus triggered starts the arm, unless the station has nothing to give her; the arm extends, holds, activates the
/// station, holds and retracts by its authored timing (<see cref="StationAccessTimingDefinitions"/>), and the station's own
/// animation shows its images (<see cref="StationAnimationDefinitions"/>, #1994). What the station gives and when it turns
/// Samus away is its rule's (<see cref="StationAccessRule"/>). The station's animation and the arm's use each play on the
/// shared animation player (#2480).
/// </summary>
/// <param name="rule">How the station is used.</param>
/// <param name="area">The area whose map a map station serves.</param>
/// <param name="animation">The animation it shows first.</param>
internal sealed class AccessStationObject(StationAccessRule rule, AreaId area, StationAnimation animation) : RoomObjectBehaviour
{
    /// <summary>How the station is used: its access blocks, its animations, its reward and its message.</summary>
    internal StationAccessRule Rule { get; } = Ensure.NotNull(rule);

    /// <summary>Which station it is.</summary>
    public StationKind Kind => Rule.Kind;

    /// <summary>Area whose map a map station acquires.</summary>
    public AreaId Area { get; } = area;

    /// <summary>Animation the station currently shows.</summary>
    internal StationAnimation ShownAnimation { get; private set; } = animation;

    /// <summary>Animation shown once the station has given its reward: the map station's acquired one, or its ordinary one.</summary>
    internal StationAnimation CompletedAnimation => Rule.AcquiredAnimation ?? Rule.Animation;

    /// <summary>The station's animation playing: its first image shows on the station's first update.</summary>
    private readonly AnimationPlayer<StationAnimation, StationDraw, NoAnimationCue> _animation =
        AnimationPlayer<StationAnimation, StationDraw, NoAnimationCue>.Starting(
            StationAnimationDefinitions.Segments, StationAnimationDefinitions.Animations[animation]);

    /// <summary>The access arm's use playing, from the access's first draw.</summary>
    private readonly AnimationPlayer<StationAccessSegment, StationAccessPose, StationAccessCue> _arm =
        AnimationPlayer<StationAccessSegment, StationAccessPose, StationAccessCue>.Starting(
            StationAccessTimingDefinitions.Segments, StationAccessTimingDefinitions.Use);

    /// <summary>Records an accepted access for the station's next update.</summary>
    public bool Triggered { get; private set; }

    /// <summary>Whether the access arm is in use, idle, or waiting for the map screen to close.</summary>
    public StationOperationPhase OperationPhase { get; private set; }

    /// <summary>The access block where the activated access arm is drawn, or null when no access is active.</summary>
    public Position16<RoomBlocks>? AccessBlock { get; private set; }

    /// <summary>The access Samus used, which selects the arm's draws; null when no access is active.</summary>
    public StationAccess? Access { get; private set; }

    /// <summary>Samus's accepted <paramref name="access"/> at <paramref name="accessBlock"/>, which the station's next update takes.</summary>
    internal void Trigger(Position16<RoomBlocks> accessBlock, StationAccess access)
    {
        Triggered = true;
        AccessBlock = accessBlock;
        Access = access;
    }

    /// <summary>
    /// Leaving the map screen returns a map station waiting for it to idle. Unpause can arrive before the final access-art
    /// hold expires; that remaining animation keeps running.
    /// </summary>
    internal void ResumeAfterMapUnpause()
    {
        if (OperationPhase == StationOperationPhase.AwaitingMapUnpause)
            OperationPhase = StationOperationPhase.Idle;
    }

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        StationObjects stations = context.RoomObjects.Stations;
        IRoomSamus samus = stations.Samus;

        // The access's first draw starts its six-update countdown on the triggering update itself; the countdown only
        // runs on later updates.
        bool operationStartedThisUpdate = false;
        if (Triggered)
        {
            Triggered = false;
            if (Rule.Refill is { } refill && refill.IsFull(samus))
            {
                // The full-resource branch releases Samus before the access ends. A refill between the touch and this
                // update must release her too.
                samus.Control.Unlock();
                AccessBlock = null;
                Access = null;
            }
            else
            {
                // The arm draws its first extension image for six updates, holds fully inserted for 96, then activates
                // the station. The touch locked Samus; this update does not lock her again, so one an intervening
                // unpause freed stays free. A map station's access never reads the map: a station whose map is held
                // has no access blocks (#2508), and the arm's draws leave a used station's access cells solid.
                OperationPhase = StationOperationPhase.InUse;
                operationStartedThisUpdate = true;
                context.QueueSound(EnemySound.StationEngaged, SoundRequestPriority.Normal);
                _arm.Show(StationAccessTimingDefinitions.Use);
                DrawAccess(context, _arm.Showing);
            }
        }

        if (!operationStartedThisUpdate &&
            OperationPhase is not (StationOperationPhase.Idle or StationOperationPhase.AwaitingMapUnpause))
            StepAccess(context, stations, samus);

        // A looping animation of images always shows its next image once the shown one's updates run out (#1627).
        StationDraw? shown = _animation.StepToNewFrame();
        if (_animation.IsResting)
            throw new InvalidDataException($"{Kind} station animation {ShownAnimation} came to rest instead of showing its next image.");
        if (shown is { } image)
            context.Draw(place, StationObjectDraws.Family, image);
    }

    /// <summary>
    /// Plays the arm's use one update on: it draws the pose the update shows anew, carries out the moments it passes, and
    /// ends the access once it is over.
    /// </summary>
    private void StepAccess(RoomObjectStepContext context, StationObjects stations, IRoomSamus samus)
    {
        if (_arm.StepToNewFrame(cue => ReachAccessMoment(cue, context, stations, samus)) is { } pose)
        {
            DrawAccess(context, pose);
            return;
        }
        if (!_arm.HasStopped)
            return;
        OperationPhase = Rule.Refill is null && samus.Control.InputLocked
            ? StationOperationPhase.AwaitingMapUnpause
            : StationOperationPhase.Idle;
        AccessBlock = null;
        Access = null;
        // The access only ends here. Recharges released Samus at activation; leaving the map screen releases her after
        // the map.
    }

    /// <summary>Carries out the moment <paramref name="cue"/> of the arm's use.</summary>
    private void ReachAccessMoment(StationAccessCue cue, RoomObjectStepContext context, StationObjects stations, IRoomSamus samus)
    {
        switch (cue)
        {
            case StationAccessCue.Activates:
                Activate(stations, samus);
                // The recharges release Samus right after their message, before the arm retracts. The map leaves her
                // locked for leaving the map screen to release.
                if (Rule.Refill is not null)
                    samus.Control.Unlock();
                return;
            case StationAccessCue.Disengages:
                context.QueueSound(EnemySound.StationDisengaged, SoundRequestPriority.Normal);
                return;
            default:
                throw new InvalidDataException($"Unknown station access moment {cue}.");
        }
    }

    /// <summary>
    /// Gives the station's reward, the refill or the area's map, and publishes its message; the station shows its completed
    /// animation from this update's step.
    /// </summary>
    private void Activate(StationObjects stations, IRoomSamus samus)
    {
        if (Rule.Refill is { } refill)
            refill.Fill(samus);
        else
            stations.System.SetAreaMapAcquired(Area);
        ShownAnimation = CompletedAnimation;
        _animation.Start(StationAnimationDefinitions.Animations[ShownAnimation]);
        stations.Publish(new StationCompleted(Rule.CompletedMessage));
    }

    /// <summary>Draws the access arm at the access Samus used, in <paramref name="pose"/>.</summary>
    private void DrawAccess(RoomObjectStepContext context, StationAccessPose pose)
    {
        StationAccessObjectDefinition arm = StationAccessObjectDefinitions.Arms[Access
            ?? throw new InvalidOperationException("A station drew its access arm with no access active.")];
        StationObjectDraws.Family.Draw(context.Level, AccessBlock ?? throw new InvalidOperationException("A station drew its access arm with no access active."),
            context.RoomObjects.Drawing, pose == StationAccessPose.Extended ? arm.Extended : arm.Retracted);
    }
}
