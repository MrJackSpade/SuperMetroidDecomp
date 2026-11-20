using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's body, eye and tail: their setup, animations and cues.</summary>
[RoomLifetime]
internal sealed class DraygonParts
{
    /// <summary>The loaded room's bindings: its game-state services, random source and the boss state that outlives the room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Draygon's goop, eye drops, and other enemy projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes the room shaking caused by Draygon's attacks and death.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>Updates the body and eye shade bands used by Draygon's presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies frame timing for the body and linked-part AI behaviours.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Receives the sounds Draygon's animations and death request.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The goop and turret shots, whose goop behaviour Draygon's goop is bound to.</summary>
    private readonly DraygonShots _shots;
    /// <summary>The room's enemy places, among which each part finds the body listed before it.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>Builds the component from the encounter's collaborators.</summary>
    internal DraygonParts(DraygonPartsDependencies dependencies)
    {
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _effectActors = dependencies.EffectActors;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _audio = dependencies.Audio;
        _shots = dependencies.Shots;
        _places = dependencies.Places;
    }

    /// <summary>
    /// Sets Draygon's body up when the room spawns it: it begins the encounter with his eye, tail and arms, which his room
    /// lists right after his body (#2298), each playing the animation its setup starts.
    /// </summary>
    internal void InitializeDraygonBody(RoomEnemyPlace body)
    {
        // His opening's colours are the import's shade of them, which his sprites draw in (#1512).

        body.AnimationClock.StepOnNextUpdate();

        _frameContext.RequestFullSweep();
        RoomEnemyPlace[] parts = _places.ListedRunAfter(body, DraygonPartListingDefinitions.PartsAfterBody);
        var draygon = new DraygonEnemyState(body, eye: parts[0], tail: parts[1], arms: parts[2])
        {
            Phase = DraygonPhase.IntroInitialDelay,
        };
        // Draygon shows his body, eye and dying arms' frames on his body (#1354, #1466, #1515, #1540), nothing at first; the
        // room keeps showing it, hidden, once his body is deleted (#2194). The room's display reads only what he attaches;
        // his parts each hold the encounter from their own setup.
        _presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.Draygon, draygon));
        _presentation.BossShadings.Show(BossShadingLayer.Draygon, draygon.Colours);
        body.SetSpeciesState(draygon);
        // His setup shows only his opening's colours (#2547). His health band is the one his health starts in (#2304), so a
        // hurt the grapple's shock starts before any hit shows it, but his body layer shows it only from his first hurt
        // update or from the reaction that moves his health into another band.
        body.Shading = draygon.Colours.Sprites;
    }

    /// <summary>
    /// The part a part other than the body joins as it sets up, of the encounter his body, listed
    /// <paramref name="placesAfterBody"/> places before the part, began (#2298): the part holds it as its species state, and
    /// its animation, the one the encounter began it with, steps on its next update.
    /// </summary>
    private DraygonPart JoiningPart(RoomEnemyPlace place, int placesAfterBody, Func<DraygonEnemyState, DraygonPart> part)
    {
        DraygonPart joined = part(_places.ListedBefore(place, placesAfterBody).RequireSpeciesState<DraygonEnemyState>());
        place.SetSpeciesState(joined);
        place.AnimationClock.StepOnNextUpdate();
        return joined;
    }

    /// <summary>Sets Draygon's eye up, listed right after his body, inert until the body publishes facing AI.</summary>
    internal void InitializeDraygonEye(RoomEnemyPlace eye) => JoiningPart(eye, DraygonPartListingDefinitions.EyePlacesAfterBody, draygon => draygon.EyePart);

    /// <summary>Sets Draygon's tail up, listed after his eye, starting with its fake whip.</summary>
    internal void InitializeDraygonTail(RoomEnemyPlace tail) =>
        tail.Shading = JoiningPart(tail, DraygonPartListingDefinitions.TailPlacesAfterBody, draygon => draygon.TailPart).Draygon.Colours.Sprites;

    /// <summary>Sets Draygon's arms up, listed after his tail, in their idle loop.</summary>
    internal void InitializeDraygonArms(RoomEnemyPlace arms) =>
        arms.Shading = JoiningPart(arms, DraygonPartListingDefinitions.ArmsPlacesAfterBody, draygon => draygon.ArmsPart).Draygon.Colours.Sprites;

    /// <summary>Sets <paramref name="eye"/> tracking Samus facing <paramref name="facing"/> (#2165).</summary>
    private static void Track(DraygonEyeState eye, HorizontalDirection facing)
    {
        eye.Action = DraygonEyeAction.Tracking;
        eye.Facing = facing;
    }

    /// <summary>Runs the eye's step: inert, or tracking Samus the way the body faces.</summary>
    internal void RunDraygonEyeMain(DraygonEnemyState state, SamusState samus)
    {
        DraygonEyeState eye = state.EyeTracking;
        switch (eye.Action)
        {
            case DraygonEyeAction.Inert:
                return;
            case DraygonEyeAction.Tracking:
                TrackSamusWithDraygonEye(state, samus, eye.Facing);
                return;
            default:
                throw new InvalidOperationException($"Undefined Draygon eye action {eye.Action}.");
        }
    }

    /// <summary>
    /// Runs a Draygon part's animation where the shared enemy animation runs: the frame expires on the update
    /// its timer would, the clock notes each new frame, waits keep the frame, and a stop deletes the
    /// part.
    /// </summary>
    internal void AdvanceDraygonPartAnimation(DraygonPart running, SamusState samus)
    {
        RoomEnemyPlace part = running.Place;
        DraygonEnemyState state = running.Draygon;
        if (!part.AnimationClock.Expire())
            return;

        AnimationPlayback<DraygonSegment> playback = running.Playback;
        Animation<DraygonSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        AnimationShown<DraygonSegment, EnemyFrameId> shown = DraygonAnimations.Segments.PlayToNextFrame(ref animation, ref place,
            cue =>
            {
                ApplyDraygonCue(state, samus, cue);
                return null;
            });
        switch (shown)
        {
            case AnimationShown<DraygonSegment, EnemyFrameId>.Frame frame:
                running.Playback = new AnimationPlayback<DraygonSegment>(animation, place);
                part.Frame = frame.Shown;
                part.AnimationClock.Show(frame.Duration);
                return;
            case AnimationShown<DraygonSegment, EnemyFrameId>.Paused paused:
                running.Playback = new AnimationPlayback<DraygonSegment>(animation, place);
                part.AnimationClock.Pause(paused.Duration);
                return;
            case AnimationShown<DraygonSegment, EnemyFrameId>.Held:
                // A rest: the last frame stays and the clock stops (#2454).
                running.Playback = new AnimationPlayback<DraygonSegment>(animation, place);
                part.AnimationClock.Stop();
                return;
            case AnimationShown<DraygonSegment, EnemyFrameId>.Ended:
                // The end deletes the part; its animation stays where it stood and takes no further step (#2532).
                part.Presence = part.Presence.With(EnemyPresence.Deleted);
                part.AnimationClock.Stop();
                return;
            default:
                throw new InvalidOperationException($"Undefined animation outcome {shown}.");
        }
    }

    /// <summary>
    /// Carries out one cue of a Draygon animation. A cue that starts the parts' animations starts the running part's
    /// too, but the running part goes on with its own: once its play reaches its next frame, it stands where that play
    /// stopped.
    /// </summary>
    /// <remarks>Draygon's cues never continue the running part's play elsewhere.</remarks>
    private void ApplyDraygonCue(DraygonEnemyState state, SamusState samus, DraygonCue cue)
    {
        switch (cue)
        {
            case DraygonCue.StartPartAnimations start:
                state.BodyPart.SetAnimation(start.Body);
                state.EyePart.SetAnimation(start.Eye);
                state.TailPart.SetAnimation(start.Tail);
                state.ArmsPart.SetAnimation(start.Arms);
                state.Arms.AnimationClock.StepOnNextUpdate();
                state.Tail.AnimationClock.StepOnNextUpdate();
                state.Eye.AnimationClock.StepOnNextUpdate();
                state.Body.AnimationClock.StepOnNextUpdate();
                return;
            case DraygonCue.TrackWithEye track:
                Track(state.EyeTracking, track.Facing);
                return;
            case DraygonCue.ShiftBodyPicture shift:
                state.BodyPictureShift = shift.Shift;
                return;
            case DraygonCue.QueueSound sound:
                RequestDraygonSound(sound.Sound);
                return;
            case DraygonCue.SpawnDyingEffect dying:
                SpawnRandomDyingDraygonEffect(state, dying.Kind);
                return;
            case DraygonCue.ParalyseTailAndArms:
                state.TailPart.StartAnimation(DraygonSegment.Rest);
                state.ArmsPart.StartAnimation(DraygonSegment.Rest);
                return;
            case DraygonCue.SetBodyIntangible:
                state.Body.Contact = state.Body.Contact.With(EnemyContact.IgnoresSamus);
                return;
            case DraygonCue.SetBodyPhase bodyPhase:
                state.Phase = bodyPhase.Phase;
                return;
            case DraygonCue.TailWhipHit:
                ApplyDraygonTailWhipHit(state, samus);
                return;
            case DraygonCue.SpawnGoop goop:
                SpawnDraygonGoop(state, goop.Direction);
                return;
            default:
                throw new InvalidOperationException($"Undefined Draygon cue {cue}.");
        }
    }

    /// <summary>Queues a sound from one of Draygon's animations as the animation makes it, after the update's earlier requests.</summary>
    private void RequestDraygonSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues Draygon's music change as Draygon makes it.</summary>
    internal void RequestDraygonMusic(MusicRequest request) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, MusicQueueDelay.Shortest));

    /// <summary>Runs the eye direction partition.</summary>
    internal void TrackSamusWithDraygonEye(DraygonEnemyState state, SamusState samus, HorizontalDirection facing)
    {
        RoomEnemyPlace eye = state.Eye;
        // The eye breathes its bubble on its cadence whether or not its gaze turns,
        // as one of the room's dust and explosion effects.
        if (DraygonCadenceDefinitions.EyeBreathBubbleEvery128thUpdate.IsDue(eye.FrameCounter))
            _projectiles.SpawnDustOrExplosion(
                new(OffRoomPlaces.Place(eye.Position.X.Whole + (DraygonEyeEffects.HorizontalOffset.Toward(facing))), OffRoomPlaces.Place(eye.Position.Y.Whole + DraygonEyeEffects.VerticalOffset)),
                DraygonEyeEffects.DustVariant);
        Position16<RoomPixels> gaze = eye.Position.Whole +
            TowardFacing(new(DraygonEyeEffects.HorizontalOffset, DraygonEyeEffects.VerticalOffset), facing);
        TrigonometryAngle aim = EnemyMath.CalculateAimAngle(samus.Position.Whole - gaze);
        DraygonEyeState eyeState = state.EyeTracking;
        if (aim == eyeState.LookAngle)
            return;

        eyeState.LookAngle = aim;
        // The straight direction nearest the aim, up, right, down or left, chooses the look.
        CompassDirection direction = aim.NearestStraightDirection();
        DraygonSegment look = direction switch
        {
            CompassDirection.Up => facing == HorizontalDirection.Right
                ? DraygonSegment.EyeFacingRightLookingUp
                : DraygonSegment.EyeFacingLeftLookingUp,
            CompassDirection.Right => facing == HorizontalDirection.Right
                ? DraygonSegment.EyeFacingRightLookingRight
                : DraygonSegment.EyeFacingLeftLookingRight,
            CompassDirection.Down => facing == HorizontalDirection.Right
                ? DraygonSegment.EyeFacingRightLookingDown
                : DraygonSegment.EyeFacingLeftLookingDown,
            CompassDirection.Left => facing == HorizontalDirection.Right
                ? DraygonSegment.EyeFacingRightLookingLeft
                : DraygonSegment.EyeFacingLeftLookingLeft,
            CompassDirection.UpRight or CompassDirection.DownRight or CompassDirection.DownLeft or CompassDirection.UpLeft =>
                throw new InvalidOperationException($"Draygon's eye aim {aim} is nearest the diagonal {direction}."),
            _ => throw new InvalidOperationException($"Draygon's eye aim {aim} is nearest no straight direction."),
        };
        state.EyePart.StartAnimation(look);
    }

    /// <summary>Places a death-animation sprite at a room-RNG offset from the body.</summary>
    private void SpawnRandomDyingDraygonEffect(
        DraygonEnemyState state,
        RoomEffectActorKind kind)
    {
        int random = _room.Random.Generate();
        Offset16<RoomPixels> scatter = new(DraygonRandomDefinitions.DyingScatterX.Of(random) - DraygonArenaDefinitions.DyingScatterReach,
            DraygonRandomDefinitions.DyingScatterY.Of(random) - DraygonArenaDefinitions.DyingScatterReach);
        _effectActors.SpawnRoomEffectActor(
            state.Body.Position.Whole + scatter,
            kind,
            graphics: default);
    }

    /// <summary>Applies the tail whip's hit to grabbed Samus, including suit damage.</summary>
    private void ApplyDraygonTailWhipHit(DraygonEnemyState state, SamusState samus)
    {
        state.SwoopAcceleration = DraygonSwoopDefinitions.InitialAcceleration;
        SamusSuitDamage.Hurt(samus, state.Body.Definition.Damage);

        // The tail hit also shakes the foreground strongly for 32 updates, so the camera shows the hit as well as the
        // energy it costs.
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Strong, EarthquakeDirection.Vertical, EarthquakeReach.Foreground), DraygonArenaDefinitions.TailHitShakeUpdates);

        _effectActors.SpawnRoomEffectActor(
            new(samus.Position.X.Whole, samus.Position.Y.Whole + DraygonArenaDefinitions.TailHitDustBelowSamus),
            RoomEffectActorKind.BigDustCloud,
            graphics: default);
    }

    /// <summary>
    /// <paramref name="facingRight"/>, an offset given for a Draygon facing right, toward <paramref name="facing"/>: as it
    /// is facing right, mirrored facing left.
    /// </summary>
    internal static Offset16<RoomPixels> TowardFacing(Offset16<RoomPixels> facingRight, HorizontalDirection facing) =>
        facing == HorizontalDirection.Right ? facingRight : facingRight.MirroredX();

    /// <summary>Spawns a goop as either of the body animation's two goop cues does.</summary>
    internal void SpawnDraygonGoop(DraygonEnemyState state, HorizontalDirection direction)
    {
        EnemyProjectile? goop = _projectiles.AllocateEnemyProjectile();
        if (goop is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            goop,
            RoomEnemyProjectileKind.DraygonGoop,
            _shots.Goop,
            graphics: SpriteGraphics.SharedIn(DraygonColours.Goop));
        goop.StartAnimation(new DraygonProjectileAnimation(_shots.Goop, DraygonProjectileSegment.Goop));
        goop.Position = FixedPosition<RoomPixels>.FromWhole(state.Body.Position.Whole + (direction == HorizontalDirection.Right
            ? DraygonArenaDefinitions.GoopSpawnMovingRight : DraygonArenaDefinitions.GoopSpawnMovingLeft));

        int random = _room.Random.Generate();
        // The goop leaves within the quarter turn from a half turn when Draygon moves left, or from three quarters of a
        // turn when he moves right.
        TrigonometryAngle firstAngle = direction == HorizontalDirection.Right ? TrigonometryAngle.HalfTurn + TrigonometryAngle.QuarterTurn : TrigonometryAngle.HalfTurn;
        TrigonometryAngle angle = firstAngle + new TrigonometryAngle(DraygonRandomDefinitions.GoopAngleSpread.Of(random));
        SetDraygonProjectileVelocity(_shots.Goop, goop, angle, DraygonProjectileSpeeds.Goop);
    }

    /// <summary>Launches a Draygon projectile, which <paramref name="owner"/> is bound to, along <paramref name="angle"/> at <paramref name="speed"/>.</summary>
    internal static void SetDraygonProjectileVelocity(
        DraygonShots.DraygonShotBehavior owner,
        EnemyProjectile projectile,
        TrigonometryAngle angle,
        int speed) =>
        Ensure.NotNull(owner).Flights.Start(projectile, AngledFlightState.Launch(angle, speed));
}
