using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's body, eye and tail: their setup, programs and cues.</summary>
[RoomLifetime]
internal sealed class DraygonParts
{
    /// <summary>Provides the active room bus, random source, and persistent boss state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Draygon's goop, eye drops, and other enemy projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes the room shaking caused by Draygon's attacks and death.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomSpriteObjects _spriteObjects;
    /// <summary>Updates the body and eye palette bands used by Draygon's presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies frame timing for the body and linked-part AI callbacks.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Receives the sounds Draygon's programs and death request.</summary>
    private readonly EnemyAudioQueue _audio;


    /// <summary>Builds the component from the encounter's collaborators.</summary>
    internal DraygonParts(DraygonPartsDependencies dependencies)
    {
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _spriteObjects = dependencies.SpriteObjects;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _audio = dependencies.Audio;
    }

    /// <summary>The typed encounter extension while the retail Draygon population is loaded.</summary>
    public DraygonEnemyState? Draygon { get; internal set; }

    /// <summary>Sets Draygon's body up when the room spawns it.</summary>
    internal void InitializeDraygonBody(RoomEnemySlot body)
    {
        // His opening's colours are the import's shade of them, which his sprites draw in (#1512).

        // Draygon shows his body, eye and dying arms' frames on his body (#1354, #1466, #1515, #1540), nothing at first.
        _presentation.AttachBossBody(new BossBodyParts(BossBodyOwner.Draygon));

        body.FrameHold = WrappingCountdown.Start(1);

        _frameContext.RequestFullSweep();
        Draygon = new DraygonEnemyState(body)
        {
            MinimapDisabledAndBossTilesExplored = true,
            Function = DraygonAiFunction.IntroInitialDelay,
        };
        body.SetSpeciesState(Draygon);
        body.Shading = Draygon.Colours.Sprites;
        Draygon.StartAnimation(body, DraygonProgram.BodyFacingLeftIdle);
        Draygon.DisabledCannons.Add(DraygonWallCannon.UnusedBottom);
    }

    /// <summary>Runs the independent initializers.</summary>
    internal void InitializeDraygonPart(RoomEnemySlot part)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon part appeared before his body.");

        part.FrameHold = WrappingCountdown.Start(1);
        switch (DraygonPartDefinitions.Parts[part.DefinitionId])
        {
            case DraygonPart.Eye:
                // Inert until the body publishes facing AI.
                var eyeState = new DraygonEyeState { Function = DraygonEyeFunction.Inert };
                part.SetSpeciesState(eyeState);
                state.Eye = part;
                state.StartAnimation(part, DraygonProgram.EyeFacingLeftIdle);
                return;

            case DraygonPart.Tail:
                state.Tail = part;
                part.Shading = state.Colours.Sprites;
                state.StartAnimation(part, DraygonProgram.TailFacingLeftInitialFakeWhip);
                return;

            case DraygonPart.Arms:
                // Body init briefly writes a body function before this record exists. Retail part init
                // clears that lost write and installs the ordinary idle loop.
                part.Shading = state.Colours.Sprites;
                state.Arms = part;
                state.StartAnimation(part, DraygonProgram.ArmsFacingLeftIdle);
                return;

            case DraygonPart.Body:
            default:
                throw new InvalidDataException(
                    $"Enemy {part.DefinitionId} is not a Draygon part.");
        }
    }

    /// <summary>Runs the no-op tail/arms main AI and the active eye dispatcher.</summary>
    internal void RunDraygonPartMain(RoomEnemySlot part, SamusState? samus)
    {
        if (part.DefinitionId is EnemyDefinitionId.DraygonTail or EnemyDefinitionId.DraygonArms)
            return;
        if (part.DefinitionId != EnemyDefinitionId.DraygonEye)
            throw new InvalidDataException("Draygon part dispatcher received another family.");

        DraygonEyeFunction function = part.RequireSpeciesState<DraygonEyeState>().Function
            ?? throw new InvalidDataException("Draygon's eye has no function.");
        switch (function)
        {
            case DraygonEyeFunction.Inert:
                return;
            case DraygonEyeFunction.FacingLeft:
                TrackSamusWithDraygonEye(part, samus, facing: HorizontalDirection.Left);
                return;
            case DraygonEyeFunction.FacingRight:
                TrackSamusWithDraygonEye(part, samus, facing: HorizontalDirection.Right);
                return;
            default:
                throw new InvalidOperationException($"Undefined Draygon eye function {function}.");
        }
    }

    /// <summary>Restarts a linked Draygon part's program on its next update, as a list install does.</summary>
    internal void StartDraygonProgram(RoomEnemySlot part, DraygonProgram program)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon program started without encounter state.");
        state.StartAnimation(part, program);
        part.FrameHold = WrappingCountdown.Start(1);
    }

    /// <summary>
    /// Runs a Draygon part's program where the shared enemy animation runs: the frame expires on the update
    /// its timer would, a new frame raises the new-frame flag, waits keep the frame, and a stop deletes the
    /// part. The part's loop counter is its frame timer.
    /// </summary>
    internal void AdvanceDraygonPartAnimation(RoomEnemySlot part, SamusState? samus)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon part ran without encounter state.");
        CountdownStep frameHold = part.FrameHold.Decrement();
        part.FrameHold = frameHold.Remaining;
        if (!frameHold.IsZero)
        {
            part.NewInstructionFrame = false;
            return;
        }

        AnimationPlayback<DraygonProgram> playback = state.Playback(part);
        Animation<DraygonProgram> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        WrappingCountdown counter = part.Timer;
        AnimationShown<DraygonProgram, DraygonFrame> shown = DraygonAnimations.Segments.PlayToNextFrame(ref animation, ref place, ref counter,
            cue =>
            {
                ApplyDraygonCue(state, part, samus, cue);
                return null;
            });
        part.Timer = counter;
        switch (shown)
        {
            case AnimationShown<DraygonProgram, DraygonFrame>.Frame frame:
                state.SetPlayback(part, new AnimationPlayback<DraygonProgram>(animation, place));
                part.FrameHold = WrappingCountdown.FromRaw(frame.Duration);
                part.Frame = DraygonFrameDefinitions.ShownFrames.Of(frame.Shown);
                part.NewInstructionFrame = true;
                return;
            case AnimationShown<DraygonProgram, DraygonFrame>.Paused paused:
                state.SetPlayback(part, new AnimationPlayback<DraygonProgram>(animation, place));
                part.FrameHold = WrappingCountdown.FromRaw(paused.Duration);
                return;
            case AnimationShown<DraygonProgram, DraygonFrame>.Held:
                // The script's sleep: the timer wraps from zero and the last frame stays.
                state.SetPlayback(part, new AnimationPlayback<DraygonProgram>(animation, place));
                return;
            case AnimationShown<DraygonProgram, DraygonFrame>.Ended:
                // The end deletes the part and leaves its animation's place unwritten.
                part.Presence = part.Presence.With(EnemyPresence.Deleted);
                return;
            default:
                throw new InvalidOperationException($"Undefined animation outcome {shown}.");
        }
    }

    /// <summary>
    /// Carries out one cue of a Draygon program run by <paramref name="part"/>. Animation starts write the parts'
    /// stored playbacks, so the running part's own write is replaced when its update ends, as the program runner's
    /// final store of the running part's position replaces it.
    /// </summary>
    /// <remarks>Draygon's cues never continue the running part's play elsewhere.</remarks>
    private void ApplyDraygonCue(DraygonEnemyState state, RoomEnemySlot part, SamusState? samus, DraygonCue cue)
    {
        switch (cue)
        {
            case DraygonCue.StartPartPrograms start:
                state.StartAnimation(state.Body, start.Body);
                state.StartAnimation(RequirePart(state.Eye), start.Eye);
                state.StartAnimation(RequirePart(state.Tail), start.Tail);
                state.StartAnimation(RequirePart(state.Arms), start.Arms);
                state.Arms.FrameHold = WrappingCountdown.Start(1);
                state.Tail.FrameHold = WrappingCountdown.Start(1);
                state.Eye.FrameHold = WrappingCountdown.Start(1);
                state.Body.FrameHold = WrappingCountdown.Start(1);
                return;
            case DraygonCue.SetEyeFunction eyeFunction:
                (RequirePart(state.Eye)).RequireSpeciesState<DraygonEyeState>().Function = eyeFunction.Function;
                return;
            case DraygonCue.SetOwnEyeFunction ownFunction:
                part.RequireSpeciesState<DraygonEyeState>().Function = ownFunction.Function;
                return;
            case DraygonCue.DisplaceBodyGraphics displace:
                state.BodyGraphicsDisplacement = displace.Offset;
                return;
            case DraygonCue.QueueSound sound:
                RequestDraygonSound(sound.Sound);
                return;
            case DraygonCue.SpawnDyingObject dying:
                SpawnRandomDyingDraygonObject(state, dying.Kind);
                return;
            case DraygonCue.ParalyseTailAndArms:
                StartDraygonProgram(RequirePart(state.Tail), DraygonProgram.Sleep);
                StartDraygonProgram(RequirePart(state.Arms), DraygonProgram.Sleep);
                return;
            case DraygonCue.SetBodyIntangible:
                state.Body.Contact = state.Body.Contact.With(EnemyContact.IgnoresSamus);
                return;
            case DraygonCue.SetBodyFunction bodyFunction:
                state.Function = bodyFunction.Function;
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

    /// <summary>A linked Draygon part, failing if its record never loaded.</summary>
    internal static RoomEnemySlot RequirePart(RoomEnemySlot? part) =>
        part ?? throw new InvalidDataException("A Draygon program ran before all four parts loaded.");

    /// <summary>Queues a sound from one of Draygon's programs as the program makes it, after the update's earlier requests.</summary>
    private void RequestDraygonSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues Draygon's music change as Draygon makes it.</summary>
    internal void RequestDraygonMusic(MusicRequest request) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, MusicQueueDelay.Shortest));

    /// <summary>Runs the eye direction partition.</summary>
    internal void TrackSamusWithDraygonEye(RoomEnemySlot eye, SamusState? samus, HorizontalDirection facing)
    {
        // Emission precedes the angle-change early return, so a stationary
        // target must not suppress the periodic particle. It shares the real pool.
        if (DraygonCadenceDefinitions.EyeBreathBubbleEvery128thUpdate.IsDue(eye.FrameCounter))
            _projectiles.SpawnRoomGraphicsDustExplosion(
                new(CoordinateAxis.Wrap(eye.Position.X.Whole + (facing == HorizontalDirection.Right
                    ? DraygonEyeEffects.HorizontalOffset : -DraygonEyeEffects.HorizontalOffset)), CoordinateAxis.Wrap(eye.Position.Y.Whole + DraygonEyeEffects.VerticalOffset)),
                DraygonEyeEffects.DustVariant);
        if (samus is null)
            return;

        Position16<RoomPixels> gaze = eye.Position.Whole +
            new Offset16<RoomPixels>(facing == HorizontalDirection.Right ? 24 : -24, -32);
        Bearing bearing = EnemyMath.CalculateBearing(samus.Position.Whole - gaze);
        DraygonEyeState eyeState = eye.RequireSpeciesState<DraygonEyeState>();
        if (bearing == eyeState.LookAngle)
            return;

        eyeState.LookAngle = bearing;
        // The quarter of the turn centred on up, right, down or left that the bearing lies in chooses the look.
        int quarter = bearing.TurnedBySteps(RotationAngle.StepsPerTurn / 8).Sector(4);
        DraygonProgram look = quarter switch
        {
            0 => facing == HorizontalDirection.Right
                ? DraygonProgram.EyeFacingRightLookingUp
                : DraygonProgram.EyeFacingLeftLookingUp,
            1 => facing == HorizontalDirection.Right
                ? DraygonProgram.EyeFacingRightLookingRight
                : DraygonProgram.EyeFacingLeftLookingRight,
            2 => facing == HorizontalDirection.Right
                ? DraygonProgram.EyeFacingRightLookingDown
                : DraygonProgram.EyeFacingLeftLookingDown,
            3 => facing == HorizontalDirection.Right
                ? DraygonProgram.EyeFacingRightLookingLeft
                : DraygonProgram.EyeFacingLeftLookingLeft,
            _ => throw new InvalidOperationException($"Draygon's eye bearing {bearing} lies in no quarter of the turn."),
        };
        StartDraygonProgram(eye, look);
        eye.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Places a death-animation sprite at a room-RNG offset from the body.</summary>
    private void SpawnRandomDyingDraygonObject(
        DraygonEnemyState state,
        RoomSpriteObjectKind kind)
    {
        int random = _room.Random.Generate();
        Offset16<RoomPixels> scatter = new(DraygonRandomDefinitions.DyingScatterX.Of(random) - 64, DraygonRandomDefinitions.DyingScatterY.Of(random) - 64);
        _spriteObjects.SpawnRoomSpriteObject(
            state.Body.Position.Whole + scatter,
            kind,
            graphics: default);
    }

    /// <summary>Applies the tail whip's hit to grabbed Samus, including suit damage.</summary>
    private void ApplyDraygonTailWhipHit(DraygonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon's tail-hit instruction requires Samus state.");

        state.SwoopAcceleration = DraygonBehavior.DraygonInitialSwoopAcceleration;
        int rawDamage = state.Body.Definition.Damage;
        int damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? rawDamage >> 2
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? rawDamage >> 1
                : rawDamage;
        samus.Health = samus.Health <= damage
            ? 0
            : samus.Health - damage;

        // The tail hit also publishes the same 32-frame, type-seven quake used by the
        // game's grabbed-tail impact. Keeping it on the shared room-enemy surface lets
        // the ordinary camera renderer observe the hit instead of reducing it to HP loss.
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Strong, EarthquakeDirection.Vertical, EarthquakeReach.Foreground), 32);

        state.TailWhipHits++;
        _spriteObjects.SpawnRoomSpriteObject(
            new(samus.Position.X.Whole, CoordinateAxis.Wrap(samus.Position.Y.Whole + 16)),
            RoomSpriteObjectKind.DustCloud,
            graphics: default);
    }

    /// <summary>Spawns a goop as either of the body program's two goop cues does.</summary>
    internal void SpawnDraygonGoop(DraygonEnemyState state, bool movingRight)
    {
        EnemyProjectile? goop = _projectiles.AllocateEnemyProjectile();
        if (goop is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            goop,
            RoomEnemyProjectileKind.DraygonGoop,
            graphics: SpriteGraphics.SharedIn(DraygonColours.Goop));
        goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileProgram.Goop));
        goop.Position = FixedPosition<RoomPixels>.FromWhole(new(CoordinateAxis.Wrap(state.Body.Position.X.Whole + (movingRight ? 24 : -28)), CoordinateAxis.Wrap(state.Body.Position.Y.Whole - 16)));

        int random = _room.Random.Generate();
        TrigonometryAngle angle = new(DraygonRandomDefinitions.GoopAngleSpread.Of(random) + (movingRight ? 192 : 128));
        SetDraygonProjectileVelocity(goop, angle, DraygonProjectileSpeeds.Goop);
        state.GoopProjectilesSpawned++;
    }

    /// <summary>Launches a Draygon projectile along <paramref name="angle"/> at <paramref name="speed"/>.</summary>
    internal static void SetDraygonProjectileVelocity(
        EnemyProjectile projectile,
        TrigonometryAngle angle,
        int speed) =>
        projectile.State = AngledFlightState.Launch(angle, speed);
}
