using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's body and tank: their setup, animations, movement, footsteps and the first phase.</summary>
[RoomLifetime]
internal sealed class MotherBrainBody
{
    /// <summary>The speed the hand's beam flies at toward Samus: twelve pixels an update.</summary>
    private static readonly SignedEightEight HandBeamSpeed = SignedEightEight.FromPixels(12);

    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Provides room services, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;
    /// <summary>The other actors in the room: the Baby, the Rinkas, the Pirate laser and the afterburn.</summary>
    private readonly IMotherBrainRoomActors _roomActors;

    /// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
    private readonly MotherBrainProjectiles _projectileAttacks;
    /// <summary>Mother Brain's state lookups, body and brain animation setters, timers and room-object requests.</summary>
    private readonly MotherBrainEncounterAccess _access;
    /// <summary>Holds the body her body frames show on.</summary>
    private readonly EnemyPresentation _presentation;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainBody(MotherBrainBodyDependencies dependencies)
    {
        _presentation = dependencies.Presentation;
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;
        _roomActors = dependencies.RoomActors;

        _projectileAttacks = dependencies.ProjectileAttacks;
        _access = dependencies.Access;
    }

    /// <summary>
    /// Runs a falling tube's animation where the shared enemy animation runs: its pose shows on the update the
    /// timer expires, raising the new-frame flag, and the script's sleep then keeps it.
    /// </summary>
    internal static void AdvanceMotherBrainTubeAnimation(RoomEnemySlot tube)
    {
        if (!tube.AnimationClock.Expire())
            return;
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        AnimationPlayback<MotherBrainTubePiece> playback = tubeState.Playback;
        EnemyAnimationPlayback.Play(tube, MotherBrainTubeAnimations.Segments, ref playback, MotherBrainVisualDefinitions.TubeFrames.Of,
            cue => throw new InvalidOperationException($"Falling-tube animations raise no event {cue}."));
        tubeState.Playback = playback;
    }

    /// <summary>Sets Mother Brain's body up when the room spawns it.</summary>
    internal void InitializeMotherBrainBody(RoomEnemySlot body)
    {
        body.AnimationClock.StepOnNextUpdate();
        body.Contact = body.Contact.With(EnemyContact.BlocksPlasmaBeam | EnemyContact.IgnoresSamus);
        body.Presence = body.Presence.With(EnemyPresence.Hidden);
        body.Shading = EnemyShades.HurtFlashLook;

        _motherBrainEncounter.MotherBrain = new MotherBrainEnemyState(body)
        {
            Form = MotherBrainForm.Glass,
            HitboxesEnabled = MotherBrainHitboxParts.Brain,
            BrainAction = MotherBrainBrainAction.SetupBrainToBeDrawn,
            Phase = MotherBrainBodyPhase.FirstPhase,
        };
        // Her body frames show on her body from here, which her room does not show over the pipe background until her
        // phase-two transition reveals it (#1356, #1515).
        _presentation.AttachBossBody(BossBodyParts.Hidden(BossBodyOwner.MotherBrain));
        // Her glass shards and tube projectiles show their own colours, MotherBrainColours.GlassAndTubes (#1544).
        // Her room's acid rises, which the room loader applies after enemy initialization.
        _frameResults.MotherBrainAcidChange = MotherBrainRoomFxDefinitions.AcidRises;
        SpawnMotherBrainInitialTurrets();
    }

    /// <summary>Updates the glass-bound first phase and advances its scripted reaction to Samus.</summary>
    internal void RunMotherBrainFirstPhase(MotherBrainEnemyState state, SamusState? samus)
    {
        // Before the glass room object finishes the nineteen missile increments and its 48-frame tail,
        // event two is clear and the first-phase function performs no phase mutation. Head-vs-Samus and
        // projectile interactions are separate collision passes and remain outside this
        // body-function dispatcher.
        bool glassDestroyed = _room.Services.Events.Has(StoryEvent.MotherBrainGlassDestroyed);
        if (glassDestroyed)
        {
            state.BrainMainShakeTimer = _earthquake.RemainingFrames;
            if (samus is not null && samus.Position.X.Whole < 236 && state.Head.Health == 0)
            {
                // This is the exact first-phase exit. The following
                // function remains represented by its own phase so the next frame
                // enters the real timed descent rather than jumping to standing form.
                state.DeleteTurretsAndRinkas = true;
                // Mother Brain and the Rinkas share this request. Publish it before later
                // enemy slots can keep drawing replaced tile art.
                _roomActors.TerminateRinkas();
                state.Form = MotherBrainForm.FakeDeath;
                _motherBrainEncounter.RequestMusic(MusicRequest.SelectTrack(MusicTrack.SecondSetSong), MusicQueueDelay.Shortest);

                // Sealing the wall creates both dust puffs before its
                // two identical fixed room objects. Requests remain ordered because the room-object
                // allocator searches the same descending pool for each call.
                _projectiles.SpawnRoomGraphicsDustExplosion(new(248, 72), MiscDustAnimation.SmallDustCloud);
                _projectiles.SpawnRoomGraphicsDustExplosion(new(248, 152), MiscDustAnimation.SmallDustCloud);
                _access.RequestMotherBrainObject(new(15, 4), MotherBrainRoomMutation.FillWall);
                _access.RequestMotherBrainObject(new(15, 9), MotherBrainRoomMutation.FillWall);
                state.Phase = MotherBrainBodyPhase.FakeDeathDescentInitialPause;
            }
        }

        // The first-phase function always falls through to the encounter's custom rectangle walker,
        // including the frame that arms fake death. This collision is not represented by
        // either enemy definition's ordinary radius and must stay in the body dispatcher.
        if (samus is not null)
            ResolveMotherBrainSamusCollision(state, samus);
    }

    /// <summary>
    /// Plays the body's animation, or the head slot's dummy, where the shared enemy animation runs: the frame
    /// expires on the update its timer would, and a newly shown frame raises the new-frame flag. A held animation is the
    /// script's sleep: the timer wraps from zero and the last frame stays.
    /// </summary>
    internal void AdvanceMotherBrainSlotAnimation(RoomEnemySlot slot, SamusState? samus)
    {
        MotherBrainEnemyState state = _access.RequireCompleteMotherBrainState(slot);
        bool body = ReferenceEquals(slot, state.Body);
        if (!slot.AnimationClock.Expire())
            return;
        AnimationPlayback<MotherBrainBodySegment> playback = body ? state.BodyPlayback : state.HeadSlotPlayback;
        EnemyAnimationPlayback.Play(slot, MotherBrainAnimations.BodySegments, ref playback, MotherBrainBodyVisualDefinitions.ShownFrames.Of,
            cue =>
            {
                ApplyMotherBrainBodyCue(state, samus, cue);
                return null;
            });
        if (body)
            state.BodyPlayback = playback;
        else
            state.HeadSlotPlayback = playback;
    }

    /// <summary>Carries out one cue of a body animation.</summary>
    private void ApplyMotherBrainBodyCue(MotherBrainEnemyState state, SamusState? samus, MotherBrainBodyCue cue)
    {
        switch (cue)
        {
            // Pose is shared encounter state, not a property of the displayed map. Body AI
            // waits on these exact values while the animation continues independently.
            case MotherBrainBodyCue.SetPose setPose:
                state.Pose = setPose.Pose;
                return;
            // The walking steps' visible motion is occasionally opposite their signed Y
            // addition. The animations keep the additions the walking steps perform.
            case MotherBrainBodyCue.Move move:
                MoveMotherBrainBody(move.Offset);
                return;
            case MotherBrainBodyCue.StepAndMove stepAndMove:
                RunMotherBrainFootstep();
                MoveMotherBrainBody(stepAndMove.Offset);
                return;
            // The six posture-transition steps each move the body
            // vertically, counter-scrolls the background by the opposite amount, and derives the background's X from
            // the body's new origin plus its authored horizontal bias.
            case MotherBrainBodyCue.MoveWithScrollBias moveWithScrollBias:
                MoveMotherBrainBodyWithScrollBias(moveWithScrollBias.Y, moveWithScrollBias.ScrollBias);
                return;
            case MotherBrainBodyCue.SpawnHandDust spawnHandDust:
                _projectiles.SpawnRoomGraphicsDustExplosion(
                    new(CoordinateAxis.Wrap(state.Body.Position.X.Whole + spawnHandDust.Offset.X), CoordinateAxis.Wrap(state.Body.Position.Y.Whole + spawnHandDust.Offset.Y)),
                    spawnHandDust.Dust);
                return;
            case MotherBrainBodyCue.Signal { Kind: MotherBrainBodyCueKind.FireHandBeam }:
            {
                SamusState target = samus ?? throw new InvalidOperationException(
                    "Mother Brain hand-beam aiming requires the active Samus actor.");
                _motherBrainEncounter.RequestBodySound(MotherBrainSoundDefinitions.HandBeam);
                SpawnMotherBrainHandBeamCharging(state, target);
                return;
            }
            case MotherBrainBodyCue.Signal { Kind: MotherBrainBodyCueKind.AdvanceHandBeamPhase }:
                state.HandBeamPhase = MotherBrainHandBeamPhases.Next(state.HandBeamPhase);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MotherBrainBodyCueKind)} {cue}.");
        }
    }

    /// <summary>
    /// Resolves Mother Brain's contact with Samus. Mother Brain owns a flag-selected set of asymmetric
    /// rectangles whose side extents are measured relative to Samus, rather than an
    /// ordinary enemy radius. Phase one enables only the brain list; later phases also
    /// admit the body and three independently positioned neck segments.
    /// </summary>
    internal static bool ResolveMotherBrainSamusCollision(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        MotherBrainHitboxParts enabled = state.HitboxesEnabled;
        if ((enabled & MotherBrainHitboxParts.Body) != MotherBrainHitboxParts.None && ResolveMotherBrainSamusCollisionPart(
                state,
                samus,
                MotherBrainContactPart.Body,
                state.Body.Position.Whole))
        {
            return true;
        }

        RoomEnemySlot head = state.Head;
        if ((enabled & MotherBrainHitboxParts.Brain) != MotherBrainHitboxParts.None && ResolveMotherBrainSamusCollisionPart(
                state,
                samus,
                MotherBrainContactPart.Brain,
                head.Position.Whole))
        {
            return true;
        }

        if ((enabled & MotherBrainHitboxParts.Neck) != MotherBrainHitboxParts.None)
        {
            // Tests only middle joints one, two, and three. Joint zero is buried in
            // the body and joint four is already covered by the separate brain rectangles.
            Position16<RoomPixels>[] collisionSegments = state.NeckJoints[MotherBrainNeckJointDefinitions.CollisionJoints];
            foreach (Position16<RoomPixels> segment in collisionSegments)
            {
                if (ResolveMotherBrainSamusCollisionPart(
                        state,
                        samus,
                        MotherBrainContactPart.Neck,
                        segment))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>Checks a single head or body contact region and applies the phase-specific collision response.</summary>
    internal static bool ResolveMotherBrainSamusCollisionPart(
        MotherBrainEnemyState state,
        SamusState samus,
        MotherBrainContactPart contactPart,
        Position16<RoomPixels> origin)
    {
        Offset16<RoomPixels> fromOrigin = samus.Position.Whole - origin;
        foreach (EnemyHitboxRectangle hitbox in
            MotherBrainContacts.Get(contactPart))
        {
            bool belowOrigin = fromOrigin.Y >= 0;
            int yDistance = Math.Abs(fromOrigin.Y);
            int yExtent = belowOrigin ? hitbox.Bottom : hitbox.Top;
            int yOverlap = samus.Kinematics.Radius.Y + Math.Abs(yExtent) - yDistance;
            if (yOverlap < 0)
                continue;

            bool rightOfOrigin = fromOrigin.X >= 0;
            int xDistance = Math.Abs(fromOrigin.X);
            int xExtent = rightOfOrigin ? hitbox.Right : hitbox.Left;
            int xOverlap = samus.Kinematics.Radius.X + Math.Abs(xExtent) - xDistance;
            if (xOverlap < 0)
                continue;

            // Clamps shallow contact UP to four pixels. The resulting displacement is
            // consumed by Samus movement later in the frame, so overwrite rather than add.
            samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWholePixels(new(Math.Max(xOverlap, 4), 4));
            samus.InvincibilityTimer = WrappingCountdown.Start(96);
            samus.KnockbackTimer = 5;
            samus.KnockbackXDirection = KnockbackHorizontalDirection.Right;
            if (CoordinateAxis.Difference(samus.Position.Y.Whole, 192) < 0)
                samus.Kinematics.YDirection = SamusVerticalDirection.Down;

            // Only contact to the right of body X+24 hurts Samus. Contact
            // elsewhere still installs the displacement and timers above without damage.
            if (CoordinateAxis.AsSigned(state.Body.Position.X.Whole + 24 - samus.Position.X.Whole) < 0)
            {
                // The hurting touch is an ordinary enemy touch: suit-divided damage, the hit's invincibility, and
                // knockback away from her body.
                EnemyCollision.ApplyNormalEnemyTouchDamage(samus, state.Body.Definition.Damage, state.Body.Position.X.Whole);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs the hand beam's charging initializer. The body record—not the articulated
    /// head—is the source: its extended map puts the hand at (+64,-48).
    /// </summary>
    internal void SpawnMotherBrainHandBeamCharging(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        EnemyProjectile? beam = _projectiles.AllocateEnemyProjectile();
        if (beam is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            beam,
            RoomEnemyProjectileKind.MotherBrainHandBeamCharging,
            graphics: SpriteGraphics.SharedIn(EnemyShades.HurtFlashLook));
        beam.Velocity = EightEightVector<RoomPixels>.Zero;
        beam.Position = beam.Position.WithFractionX(0).WithFractionY(0);
        beam.Graphics = SpriteGraphics.SharedIn(state.Colours.OwnShading);
        beam.Position = beam.Position.WithWhole(new(CoordinateAxis.Wrap(state.Body.Position.X.Whole + 64), CoordinateAxis.Wrap(state.Body.Position.Y.Whole - 48)));

        state.HandBeamNextPosition = FixedPosition<RoomPixels>.FromWhole(beam.Position.Whole);

        TrigonometryAngle angle = EnemyMath.CalculateBearing(samus.Position.Whole - beam.Position.Whole)
            .ToTableAngle().TurnedBy(TrigonometryAngle.QuarterTurn);
        state.HandBeamNextAngle = angle;
        state.HandBeamNextVelocity = new EightEightVector<RoomPixels>(
            EnemyMath.MultiplySineSample(HandBeamSpeed, angle),
            EnemyMath.MultiplySineSample(HandBeamSpeed, angle.TurnedBy(TrigonometryAngle.QuarterTurn)));

        beam.StartAnimation(new MotherBrainHandBeamAnimation());
    }

    /// <summary>
    /// Body and background movement. The signed Y operand is added
    /// to the actor and subtracted from the background, keeping the large background body art
    /// rigidly attached to its small sprite hitbox record. X movement happens before this call,
    /// so the derived scroll must sample the already-updated body X.
    /// </summary>
    private void MoveMotherBrainBody(Offset16<RoomPixels> displacement)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain body bytecode ran without its multipart encounter state.");
        state.Body.Position += displacement;
        PlaceMotherBrainBodyLayer(state, new(
            CoordinateAxis.Wrap(34 - state.Body.Position.X.Whole),
            CoordinateAxis.Wrap(state.BodyLayerPlacement.Y - displacement.Y)));
    }

    /// <summary>
    /// Posture-transition variant. Its horizontal argument is a scroll bias, not
    /// actor displacement; only Y changes the physical body coordinate.
    /// </summary>
    private void MoveMotherBrainBodyWithScrollBias(
        int yDisplacement,
        int horizontalScrollBias)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain posture bytecode ran without its multipart encounter state.");
        state.Body.Position = state.Body.Position.WithWrappedWholeY(state.Body.Position.Y.Whole + yDisplacement);
        PlaceMotherBrainBodyLayer(state, new(
            CoordinateAxis.Wrap(34 + horizontalScrollBias - state.Body.Position.X.Whole),
            CoordinateAxis.Wrap(state.BodyLayerPlacement.Y - yDisplacement)));
    }

    /// <summary>Shared four-frame earthquake produced by each planted walking frame.</summary>
    private void RunMotherBrainFootstep()
    {
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Vertical, EarthquakeReach.Foreground), 4);

        // Form three additionally makes the footstep sound. Phase two normally never
        // reaches that branch, but retaining it here makes the shared gait cue complete.
        if (_motherBrainEncounter.MotherBrain?.Form == MotherBrainForm.DrainedByBabyMetroid)
            _motherBrainEncounter.RequestCueSound(RoomSound.MotherBrainFootstep);
    }

    /// <summary>Places her body layer for the current motion; the displayed frame publishes it with her body (#1356).</summary>
    internal static void PlaceMotherBrainBodyLayer(
        MotherBrainEnemyState state,
        Position16<BackgroundPixels> placement)
    {
        state.BodyLayerPlacement = placement;
        state.PlacesBodyLayer = true;
    }

    /// <summary>
    /// Makes the twelve turret spawns
    /// the body's initializer requests. Allocation scans the pool from its highest place down, so request
    /// zero lands in place 17 and request eleven lands in place 6.
    /// </summary>
    private void SpawnMotherBrainInitialTurrets()
    {
        for (int parameter = 0; parameter < 12; parameter++)
            SpawnMotherBrainTurret(parameter);
    }

    /// <summary>Spawns one of Mother Brain's turrets.</summary>
    internal void SpawnMotherBrainTurret(int parameter)
    {
        if (parameter is < 0 or >= 12)
            throw new ArgumentOutOfRangeException(nameof(parameter));

        EnemyProjectile? turret = _projectiles.AllocateEnemyProjectile();
        if (turret is null)
            return;

        MotherBrainProjectileStarts.Initialize(
            turret,
            RoomEnemyProjectileKind.MotherBrainRoomTurret,
            MotherBrainTurretDefinitions.Graphics);

        MotherBrainTurretDefinition definition =
            MotherBrainTurrets.ForTurret(parameter);
        MotherBrainTurretDirection direction = definition.InitialDirection;
        turret.Position = turret.Position.WithWhole(definition.Position);
        turret.State = new MotherBrainTurretState(parameter, MotherBrainTurretDefinitions.Ring.PositionOf(direction));
        turret.StartAnimation(new MotherBrainTurretAnimation(MotherBrainTurretSegment.TurretPose, direction));

        // The initializer draws the RNG twice, once for each of the turret's waits, and clamps each wait's remainder modulo
        // 256 upward.
        _projectileAttacks.ResetMotherBrainTurretRotationTimer(turret);
        _projectileAttacks.ResetMotherBrainTurretCooldown(turret);
    }
}
