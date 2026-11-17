using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's body and tank: their setup, animations, movement, footsteps and the first phase.</summary>
[RoomLifetime]
internal sealed class MotherBrainBody
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
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
    /// <summary>The room's enemy places, which list her head right after her body (#2298).</summary>
    private readonly EnemyPlaces _places;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainBody(MotherBrainBodyDependencies dependencies)
    {
        _presentation = dependencies.Presentation;
        _places = dependencies.Places;
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
    /// Runs a falling tube's animation where the shared enemy animation runs: its still picture shows on the update the
    /// timer expires, raising the new-frame flag, and the animation then holds it.
    /// </summary>
    internal static void AdvanceMotherBrainTubeAnimation(RoomEnemyPlace tube)
    {
        if (!tube.AnimationClock.Expire())
            return;
        MotherBrainFallingTubeState tubeState = tube.RequireSpeciesState<MotherBrainFallingTubeState>();
        AnimationPlayback<MotherBrainTubePiece> playback = tubeState.Playback;
        EnemyAnimationPlayback.Play(tube, MotherBrainTubeAnimations.Segments, ref playback,
            cue => throw new InvalidOperationException($"Falling-tube animations raise no event {cue}."));
        tubeState.Playback = playback;
    }

    /// <summary>Sets Mother Brain's body up when the room spawns it.</summary>
    internal void InitializeMotherBrainBody(RoomEnemyPlace body)
    {
        body.AnimationClock.StepOnNextUpdate();
        body.Contact = body.Contact.With(EnemyContact.BlocksPlasmaBeam | EnemyContact.IgnoresSamus);
        body.Presence = body.Presence.With(EnemyPresence.Hidden);
        body.Shading = EnemyShades.HurtFlashLook;

        // The encounter's setup creates the walk credit and the burst cycle the encounter and its later sequence share.
        // The body begins the encounter, which it holds from here and her head joins (#2218).
        MotherBrainEnemyState state = _motherBrainEncounter.Begin(new MotherBrainEnemyState(body, _places.ListedRunAfter(body, count: 1)[0],
            new MotherBrainWalkCounter(), new MotherBrainBurstCycle())
        {
            Form = MotherBrainForm.Glass,
            HitboxesEnabled = MotherBrainHitboxParts.Brain,
            BrainAction = MotherBrainBrainAction.SetupBrainToBeDrawn,
        });
        body.SetSpeciesState(state);
        // Her body frames show on her body from here, which her room does not show over the pipe background until her
        // phase-two transition reveals it (#1356, #1515).
        _presentation.AttachBossBody(BossBodyParts.Hidden(BossBodyOwner.MotherBrain, state));
        // Her glass shards and tube projectiles show their own colours, MotherBrainColours.GlassAndTubes (#1544).
        // Her room's acid rises, which the room loader applies after enemy initialization.
        _frameResults.MotherBrainAcidChange = MotherBrainRoomFxDefinitions.AcidRises;
        SpawnMotherBrainInitialTurrets(state);
    }

    /// <summary>Updates the glass-bound first phase and advances its scripted reaction to Samus.</summary>
    internal void RunMotherBrainFirstPhase(MotherBrainEnemyState state, SamusState samus)
    {
        // Until the glass has taken its nineteen missile hits and finished shattering 48 frames later, the first phase
        // changes nothing. Head-vs-Samus and
        // projectile interactions are separate collision passes and remain outside this
        // body phase.
        bool glassDestroyed = _room.Services.Events.Has(StoryEvent.MotherBrainGlassDestroyed);
        if (glassDestroyed)
        {
            state.BrainMainShakeTimer = _earthquake.RemainingFrames;
            if (samus.Position.X.Whole < MotherBrainFightDefinitions.FirstPhaseExitSamusX && state.Head.Health == 0)
            {
                // This is the exact first-phase exit. The following
                // step remains a phase of its own so the next frame
                // enters the real timed descent rather than jumping to standing form.
                state.DeleteTurretsAndRinkas = true;
                // Mother Brain and the Rinkas share this request. Publish it before later
                // enemy places can keep drawing replaced pictures.
                _roomActors.TerminateRinkas();
                state.Form = MotherBrainForm.FakeDeath;
                _motherBrainEncounter.RequestMusic(MusicRequest.SelectTrack(MusicTrack.SilentSong), MusicQueueDelay.Shortest);

                // Sealing the wall creates both dust puffs before its
                // two identical fixed room objects. Requests remain ordered because the room-object
                // allocator searches the same descending pool for each request.
                foreach (Position16<RoomPixels> dust in MotherBrainFightDefinitions.SealedWallDust)
                    _projectiles.SpawnDustOrExplosion(dust, MiscDustAnimation.SmallDustCloud);
                foreach (RoomObjectSpawnRequest<MotherBrainRoomMutation> wall in MotherBrainFightDefinitions.SealedWall)
                    _access.RequestMotherBrainObject(wall.Block, wall.RoomObject);
                state.DescentPhase = MotherBrainDescentPhase.InitialPause;
            }
        }

        // The first phase always tests Samus against the encounter's own rectangles, including on the frame that
        // arms fake death. This collision is not represented by
        // either enemy definition's ordinary radius and must stay in the body's own phases.
        ResolveMotherBrainSamusCollision(state, samus);
    }

    /// <summary>
    /// Plays the body's animation where the shared enemy animation runs: the frame expires on the update its timer would,
    /// and a newly shown frame raises the new-frame flag. A held animation rests: the timer wraps from zero and the last
    /// frame stays. Before its first animation the body shows her brain's hit area for good (#2220).
    /// </summary>
    internal void AdvanceMotherBrainBodyAnimation(MotherBrainEnemyState state, SamusState samus)
    {
        RoomEnemyPlace body = state.Body;
        if (!body.AnimationClock.Expire())
            return;
        if (state.BodyPlayback is not { } playback)
        {
            MotherBrainHead.ShowBrainHitArea(body);
            return;
        }
        EnemyAnimationPlayback.Play(body, MotherBrainAnimations.BodySegments, ref playback,
            cue =>
            {
                ApplyMotherBrainBodyCue(state, samus, cue);
                return null;
            });
        state.BodyPlayback = playback;
    }

    /// <summary>Carries out one cue of a body animation.</summary>
    private void ApplyMotherBrainBodyCue(MotherBrainEnemyState state, SamusState samus, MotherBrainBodyCue cue)
    {
        switch (cue)
        {
            // Pose is shared encounter state, not a property of the displayed frame. Body AI
            // waits on these exact values while the animation continues independently.
            case MotherBrainBodyCue.SetPose setPose:
                state.Pose = setPose.Pose;
                return;
            // The walking steps' visible motion is occasionally opposite their signed Y
            // addition. The animations keep the additions the walking steps perform; the posture
            // changes move her vertically only.
            case MotherBrainBodyCue.Move move:
                MoveMotherBrainBody(state, move.Offset);
                return;
            case MotherBrainBodyCue.StepAndMove stepAndMove:
                RunMotherBrainFootstep(state);
                MoveMotherBrainBody(state, stepAndMove.Offset);
                return;
            case MotherBrainBodyCue.SpawnHandDust spawnHandDust:
                _projectiles.SpawnDustOrExplosion(
                    new(CoordinateAxis.Wrap(state.Body.Position.X.Whole + spawnHandDust.Offset.X), CoordinateAxis.Wrap(state.Body.Position.Y.Whole + spawnHandDust.Offset.Y)),
                    spawnHandDust.Dust);
                return;
            case MotherBrainBodyCue.Signal { Kind: MotherBrainBodyCueKind.FireHandBeam }:
            {
                _motherBrainEncounter.RequestBodySound(MotherBrainSoundDefinitions.HandBeam);
                SpawnMotherBrainHandBeamCharging(state, samus);
                return;
            }
            case MotherBrainBodyCue.Signal { Kind: MotherBrainBodyCueKind.AdvanceHandBeamPhase }:
                // The hand beam's hold has elapsed: firing finishes. Only the firing phase plays this animation.
                state.HandBeamPhase = state.HandBeamPhase == MotherBrainHandBeamPhase.Firing
                    ? MotherBrainHandBeamPhase.Finish
                    : throw new InvalidOperationException($"Mother Brain's hand beam finished firing in phase {state.HandBeamPhase}.");
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

        RoomEnemyPlace head = state.Head;
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

            // Clamps shallow contact up to the push's least distance. The resulting displacement is
            // consumed by Samus movement later in the frame, so overwrite rather than add.
            samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWholePixels(new(Math.Max(xOverlap, MotherBrainFightDefinitions.BodyPush.X), MotherBrainFightDefinitions.BodyPush.Y));
            samus.InvincibilityTimer = WrappingCountdown.Start(EnemyCollisionDefinitions.SamusTouchInvincibilityUpdates);
            samus.KnockbackTimer = EnemyCollisionDefinitions.SamusTouchKnockbackUpdates;
            samus.KnockbackXDirection = KnockbackHorizontalDirection.Right;
            if (CoordinateAxis.Difference(samus.Position.Y.Whole, MotherBrainFightDefinitions.BodyPushDownAboveY) < 0)
                samus.Kinematics.YDirection = SamusVerticalDirection.Down;

            // Only contact to the right of the hurting touch's edge hurts Samus. Contact
            // elsewhere still installs the displacement and timers above without damage.
            if (CoordinateAxis.AsSigned(state.Body.Position.X.Whole + MotherBrainFightDefinitions.HurtingTouchRightOfBody - samus.Position.X.Whole) < 0)
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
    /// Runs the hand beam's charging initializer. The body—not the articulated
    /// head—is the source: its frame puts the hand at its hand offset.
    /// </summary>
    internal void SpawnMotherBrainHandBeamCharging(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        EnemyProjectile? beam = _projectiles.AllocateEnemyProjectile();
        if (beam is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            beam,
            RoomEnemyProjectileKind.MotherBrainHandBeamCharging,
            _projectileAttacks.Projectile,
            graphics: SpriteGraphics.SharedIn(EnemyShades.HurtFlashLook));
        beam.Velocity = EightEightVector<RoomPixels>.Zero;
        beam.Position = beam.Position.WithFractionX(0).WithFractionY(0);
        beam.Graphics = SpriteGraphics.SharedIn(state.Colours.OwnShading);
        beam.Position = beam.Position.WithWhole(state.Body.Position.Whole + MotherBrainFightDefinitions.HandOffset);

        state.HandBeamNextPosition = FixedPosition<RoomPixels>.FromWhole(beam.Position.Whole);

        TrigonometryAngle angle = EnemyMath.CalculateBearing(samus.Position.Whole - beam.Position.Whole)
            .ToTrigonometryAngle().TurnedBy(TrigonometryAngle.QuarterTurn);
        state.HandBeamNextAngle = angle;
        state.HandBeamNextVelocity = new EightEightVector<RoomPixels>(
            EnemyMath.SineSpeed(MotherBrainHandBeamDefinitions.Speed, angle),
            EnemyMath.SineSpeed(MotherBrainHandBeamDefinitions.Speed, angle.TurnedBy(TrigonometryAngle.QuarterTurn)));

        beam.State = new MotherBrainProjectileState(state);
        beam.StartAnimation(new MotherBrainHandBeamAnimation());
    }

    /// <summary>
    /// Body movement: moves her, and her body's pictures with her (#1949, #2031), keeping the large body art rigidly
    /// attached to its small hitbox record.
    /// </summary>
    private static void MoveMotherBrainBody(MotherBrainEnemyState state, Offset16<RoomPixels> displacement) =>
        state.Body.Position += displacement;

    /// <summary>Starts the shake each planted walking frame produces.</summary>
    private void RunMotherBrainFootstep(MotherBrainEnemyState state)
    {
        _earthquake.Start(MotherBrainFightDefinitions.FootstepShake, MotherBrainFightDefinitions.FootstepShakeUpdates);

        // Form three additionally makes the footstep sound. Phase two normally never
        // reaches that branch, but retaining it here makes the shared gait cue complete.
        if (state.Form == MotherBrainForm.DrainedByBabyMetroid)
            _motherBrainEncounter.RequestCueSound(RoomSound.MotherBrainFootstep);
    }

    /// <summary>
    /// Makes the twelve turret spawns
    /// the body's initializer requests. Allocation scans the pool from its highest place down, so the first turret takes
    /// the highest place and each later one the next place below.
    /// </summary>
    private void SpawnMotherBrainInitialTurrets(MotherBrainEnemyState state)
    {
        foreach (MotherBrainTurretDefinition turret in MotherBrainTurretDefinitions.Turrets)
            SpawnMotherBrainTurret(state, turret);
    }

    /// <summary>Spawns one of Mother Brain's turrets, <paramref name="definition"/>.</summary>
    internal void SpawnMotherBrainTurret(MotherBrainEnemyState state, MotherBrainTurretDefinition definition)
    {
        EnemyProjectile? turret = _projectiles.AllocateEnemyProjectile();
        if (turret is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            turret,
            RoomEnemyProjectileKind.MotherBrainRoomTurret,
            _projectileAttacks.Projectile,
            MotherBrainTurretDefinitions.Graphics);

        turret.Position = turret.Position.WithWhole(definition.Position);
        turret.State = new MotherBrainTurretState(state, definition);
        turret.StartAnimation(new MotherBrainTurretAnimation(MotherBrainTurretPose.Turret(definition.InitialDirection)));

        // The initializer draws the RNG twice, once for each of the turret's waits, and clamps each wait's remainder modulo
        // 256 upward.
        _projectileAttacks.Projectile.ResetMotherBrainTurretRotationTimer(turret);
        _projectileAttacks.Projectile.ResetMotherBrainTurretCooldown(turret);
    }
}
