using SuperMetroid.Core.Bosses.Draygon.Artwork;
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
    }

    /// <summary>The typed encounter extension while the retail Draygon population is loaded.</summary>
    public DraygonEnemyState? Draygon { get; internal set; }

    /// <summary>Sets Draygon's body up when the room spawns it.</summary>
    internal void InitializeDraygonBody(RoomEnemyPlace body)
    {
        // His opening's colours are the import's shade of them, which his sprites draw in (#1512).

        // Draygon shows his body, eye and dying arms' frames on his body (#1354, #1466, #1515, #1540), nothing at first.
        _presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.Draygon));

        body.AnimationClock.StepOnNextUpdate();

        _frameContext.RequestFullSweep();
        Draygon = new DraygonEnemyState(body)
        {
            Phase = DraygonPhase.IntroInitialDelay,
        };
        body.SetSpeciesState(Draygon);
        body.Shading = Draygon.Colours.Sprites;
        Draygon.StartAnimation(body, DraygonSegment.BodyFacingLeftIdle);
    }

    /// <summary>
    /// The encounter a part other than the body joins as it sets up, which his body has set up first; the part's animation
    /// steps on its next update.
    /// </summary>
    /// <exception cref="InvalidDataException">The part sets up before his body.</exception>
    private DraygonEnemyState JoiningPart(RoomEnemyPlace part)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon part appeared before his body.");
        part.AnimationClock.StepOnNextUpdate();
        return state;
    }

    /// <summary>Sets Draygon's eye up, inert until the body publishes facing AI.</summary>
    internal void InitializeDraygonEye(RoomEnemyPlace eye)
    {
        DraygonEnemyState state = JoiningPart(eye);
        var eyeState = new DraygonEyeState(DraygonEyeAction.Inert);
        eye.SetSpeciesState(eyeState);
        state.Eye = eye;
        state.StartAnimation(eye, DraygonSegment.EyeFacingLeftIdle);
    }

    /// <summary>Sets Draygon's tail up, starting with its fake whip.</summary>
    internal void InitializeDraygonTail(RoomEnemyPlace tail)
    {
        DraygonEnemyState state = JoiningPart(tail);
        state.Tail = tail;
        tail.Shading = state.Colours.Sprites;
        state.StartAnimation(tail, DraygonSegment.TailFacingLeftInitialFakeWhip);
    }

    /// <summary>Sets Draygon's arms up in their idle loop.</summary>
    internal void InitializeDraygonArms(RoomEnemyPlace arms)
    {
        DraygonEnemyState state = JoiningPart(arms);
        // Body init briefly writes a body phase before this part exists. Retail part init
        // clears that lost write and installs the ordinary idle loop.
        arms.Shading = state.Colours.Sprites;
        state.Arms = arms;
        state.StartAnimation(arms, DraygonSegment.ArmsFacingLeftIdle);
    }

    /// <summary>Sets <paramref name="eye"/> tracking Samus facing <paramref name="facing"/> (#2165).</summary>
    private static void Track(DraygonEyeState eye, HorizontalDirection facing)
    {
        eye.Action = DraygonEyeAction.Tracking;
        eye.Facing = facing;
    }

    /// <summary>Runs the eye's step: inert, or tracking Samus the way the body faces.</summary>
    internal void RunDraygonEyeMain(RoomEnemyPlace part, SamusState samus)
    {
        DraygonEyeState eye = part.RequireSpeciesState<DraygonEyeState>();
        switch (eye.Action)
        {
            case DraygonEyeAction.Inert:
                return;
            case DraygonEyeAction.Tracking:
                TrackSamusWithDraygonEye(part, samus, eye.Facing);
                return;
            default:
                throw new InvalidOperationException($"Undefined Draygon eye action {eye.Action}.");
        }
    }

    /// <summary>Restarts a linked Draygon part's animation on its next update, as starting an animation does.</summary>
    internal void StartDraygonAnimation(RoomEnemyPlace part, DraygonSegment segment)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon animation started without encounter state.");
        state.StartAnimation(part, segment);
        part.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>
    /// Runs a Draygon part's animation where the shared enemy animation runs: the frame expires on the update
    /// its timer would, a new frame raises the new-frame flag, waits keep the frame, and a stop deletes the
    /// part.
    /// </summary>
    internal void AdvanceDraygonPartAnimation(RoomEnemyPlace part, SamusState samus)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon part ran without encounter state.");
        if (!part.AnimationClock.Expire())
            return;

        AnimationPlayback<DraygonSegment> playback = state.Playback(part);
        Animation<DraygonSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        AnimationShown<DraygonSegment, EnemyFrameId> shown = DraygonAnimations.Segments.PlayToNextFrame(ref animation, ref place,
            cue =>
            {
                ApplyDraygonCue(state, part, samus, cue);
                return null;
            });
        switch (shown)
        {
            case AnimationShown<DraygonSegment, EnemyFrameId>.Frame frame:
                state.SetPlayback(part, new AnimationPlayback<DraygonSegment>(animation, place));
                part.Frame = frame.Shown;
                part.AnimationClock.Show(frame.Duration);
                return;
            case AnimationShown<DraygonSegment, EnemyFrameId>.Paused paused:
                state.SetPlayback(part, new AnimationPlayback<DraygonSegment>(animation, place));
                part.AnimationClock.Pause(paused.Duration);
                return;
            case AnimationShown<DraygonSegment, EnemyFrameId>.Held:
                // A rest: the timer wraps from zero and the last frame stays.
                state.SetPlayback(part, new AnimationPlayback<DraygonSegment>(animation, place));
                return;
            case AnimationShown<DraygonSegment, EnemyFrameId>.Ended:
                // The end deletes the part and leaves its animation's place unwritten.
                part.Presence = part.Presence.With(EnemyPresence.Deleted);
                return;
            default:
                throw new InvalidOperationException($"Undefined animation outcome {shown}.");
        }
    }

    /// <summary>
    /// Carries out one cue of a Draygon animation run by <paramref name="part"/>. Animation starts write the parts'
    /// stored playbacks, so the running part's own write is replaced when its update ends, as the animation runner's
    /// final store of the running part's position replaces it.
    /// </summary>
    /// <remarks>Draygon's cues never continue the running part's play elsewhere.</remarks>
    private void ApplyDraygonCue(DraygonEnemyState state, RoomEnemyPlace part, SamusState samus, DraygonCue cue)
    {
        switch (cue)
        {
            case DraygonCue.StartPartAnimations start:
                state.StartAnimation(state.Body, start.Body);
                state.StartAnimation(RequirePart(state.Eye), start.Eye);
                state.StartAnimation(RequirePart(state.Tail), start.Tail);
                state.StartAnimation(RequirePart(state.Arms), start.Arms);
                state.Arms.AnimationClock.StepOnNextUpdate();
                state.Tail.AnimationClock.StepOnNextUpdate();
                state.Eye.AnimationClock.StepOnNextUpdate();
                state.Body.AnimationClock.StepOnNextUpdate();
                return;
            case DraygonCue.TrackWithEye track:
                Track(RequirePart(state.Eye).RequireSpeciesState<DraygonEyeState>(), track.Facing);
                return;
            case DraygonCue.TrackWithOwnEye track:
                Track(part.RequireSpeciesState<DraygonEyeState>(), track.Facing);
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
                StartDraygonAnimation(RequirePart(state.Tail), DraygonSegment.Rest);
                StartDraygonAnimation(RequirePart(state.Arms), DraygonSegment.Rest);
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
                SpawnDraygonGoop(state, goop.Direction == HorizontalDirection.Right);
                return;
            default:
                throw new InvalidOperationException($"Undefined Draygon cue {cue}.");
        }
    }

    /// <summary>A linked Draygon part, failing if the part never loaded.</summary>
    internal static RoomEnemyPlace RequirePart(RoomEnemyPlace? part) =>
        part ?? throw new InvalidDataException("A Draygon animation ran before all four parts loaded.");

    /// <summary>Queues a sound from one of Draygon's animations as the animation makes it, after the update's earlier requests.</summary>
    private void RequestDraygonSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues Draygon's music change as Draygon makes it.</summary>
    internal void RequestDraygonMusic(MusicRequest request) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, MusicQueueDelay.Shortest));

    /// <summary>Runs the eye direction partition.</summary>
    internal void TrackSamusWithDraygonEye(RoomEnemyPlace eye, SamusState samus, HorizontalDirection facing)
    {
        // Emission precedes the angle-change early return, so a stationary
        // target must not suppress the periodic particle. It shares the real pool.
        if (DraygonCadenceDefinitions.EyeBreathBubbleEvery128thUpdate.IsDue(eye.FrameCounter))
            _projectiles.SpawnDustOrExplosion(
                new(CoordinateAxis.Wrap(eye.Position.X.Whole + (facing == HorizontalDirection.Right
                    ? DraygonEyeEffects.HorizontalOffset : -DraygonEyeEffects.HorizontalOffset)), CoordinateAxis.Wrap(eye.Position.Y.Whole + DraygonEyeEffects.VerticalOffset)),
                DraygonEyeEffects.DustVariant);
        Position16<RoomPixels> gaze = eye.Position.Whole +
            new Offset16<RoomPixels>(facing == HorizontalDirection.Right ? 24 : -24, -32);
        Bearing bearing = EnemyMath.CalculateBearing(samus.Position.Whole - gaze);
        DraygonEyeState eyeState = eye.RequireSpeciesState<DraygonEyeState>();
        if (bearing == eyeState.LookAngle)
            return;

        eyeState.LookAngle = bearing;
        // The straight direction nearest the bearing, up, right, down or left, chooses the look.
        CompassDirection direction = bearing.NearestStraightDirection();
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
                throw new InvalidOperationException($"Draygon's eye bearing {bearing} is nearest the diagonal {direction}."),
            _ => throw new InvalidOperationException($"Draygon's eye bearing {bearing} is nearest no straight direction."),
        };
        StartDraygonAnimation(eye, look);
    }

    /// <summary>Places a death-animation sprite at a room-RNG offset from the body.</summary>
    private void SpawnRandomDyingDraygonEffect(
        DraygonEnemyState state,
        RoomEffectActorKind kind)
    {
        int random = _room.Random.Generate();
        Offset16<RoomPixels> scatter = new(DraygonRandomDefinitions.DyingScatterX.Of(random) - 64, DraygonRandomDefinitions.DyingScatterY.Of(random) - 64);
        _effectActors.SpawnRoomEffectActor(
            state.Body.Position.Whole + scatter,
            kind,
            graphics: default);
    }

    /// <summary>Applies the tail whip's hit to grabbed Samus, including suit damage.</summary>
    private void ApplyDraygonTailWhipHit(DraygonEnemyState state, SamusState samus)
    {
        state.SwoopAcceleration = DraygonBehavior.DraygonInitialSwoopAcceleration;
        SamusSuitDamage.Hurt(samus, state.Body.Definition.Damage);

        // The tail hit also shakes the foreground strongly for 32 updates, so the camera shows the hit as well as the
        // energy it costs.
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Strong, EarthquakeDirection.Vertical, EarthquakeReach.Foreground), 32);

        _effectActors.SpawnRoomEffectActor(
            new(samus.Position.X.Whole, CoordinateAxis.Wrap(samus.Position.Y.Whole + 16)),
            RoomEffectActorKind.BigDustCloud,
            graphics: default);
    }

    /// <summary>Spawns a goop as either of the body animation's two goop cues does.</summary>
    internal void SpawnDraygonGoop(DraygonEnemyState state, bool movingRight)
    {
        EnemyProjectile? goop = _projectiles.AllocateEnemyProjectile();
        if (goop is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            goop,
            RoomEnemyProjectileKind.DraygonGoop,
            _shots.Goop,
            graphics: SpriteGraphics.SharedIn(DraygonColours.Goop));
        goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileSegment.Goop));
        goop.Position = FixedPosition<RoomPixels>.FromWhole(new(CoordinateAxis.Wrap(state.Body.Position.X.Whole + (movingRight ? 24 : -28)), CoordinateAxis.Wrap(state.Body.Position.Y.Whole - 16)));

        int random = _room.Random.Generate();
        // The goop leaves within the quarter turn from a half turn when Draygon moves left, or from three quarters of a
        // turn when he moves right.
        TrigonometryAngle firstAngle = movingRight ? TrigonometryAngle.HalfTurn + TrigonometryAngle.QuarterTurn : TrigonometryAngle.HalfTurn;
        TrigonometryAngle angle = firstAngle + new TrigonometryAngle(DraygonRandomDefinitions.GoopAngleSpread.Of(random));
        SetDraygonProjectileVelocity(goop, angle, DraygonProjectileSpeeds.Goop);
    }

    /// <summary>Launches a Draygon projectile along <paramref name="angle"/> at <paramref name="speed"/>.</summary>
    internal static void SetDraygonProjectileVelocity(
        EnemyProjectile projectile,
        TrigonometryAngle angle,
        int speed) =>
        projectile.State = AngledFlightState.Launch(angle, speed);
}
