using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's hitboxes, touch, projectiles and hurt flash, and what Samus's weapons do to it.</summary>
[RoomLifetime]
internal sealed class CrocomireCombat
{
    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies frame timing and room effects to Crocomire AI.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Resolves Crocomire body and tongue contact with Samus.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Produces the distributed drops at skeleton collapse.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Crocomire's state lookups, body animation start, room objects, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireCombat(EnemyProjectileEngine projectiles, EnemyFrameContext frameContext, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, CrocomireEncounterAccess access)
    {
        _projectiles = projectiles;
        _frameContext = frameContext;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
        _access = access;
    }

    /// <summary>Shows Crocomire's white hurt flash on his body.</summary>
    internal void ApplyCrocomireHurtFlash(RoomEnemyPlace body, CrocomireEnemyState state)
    {
        bool white = body.HurtFlash.IsRunning && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(_frameContext.EnemyPassCount);
        // His body shows its fight colours or the white flash as the import's shades of them (#1512).
        state.Colours.ShowBody(white);
    }

    /// <summary>
    /// Runs the component-specific reaction a bomb's hitbox selects on <paramref name="state"/>, before the check for an
    /// ignoring reaction.
    /// </summary>
    /// <param name="c">The bomb contact.</param>
    /// <param name="state">The encounter the bombed part is part of.</param>
    internal void ReactToCrocomireBomb(EnemyBombContext c, CrocomireEnemyState state) =>
        // Crocomire's species definition names no shot reaction; every live component supplies its own. Normal bombs
        // therefore reach the same hit counter, dust, or mouth reaction his frame's hitbox selects.
        ResolveCrocomireHitboxShot(state, c.PlacedBomb.Type, c.Bomb.Position.Whole, c.SelectedShot, c.Camera);

    /// <summary>
    /// The mouth's shot (#1760): its spawner starts its animation, and a shot one runs its typed shot animation, whose
    /// drop is allocated with Crocomire's chances before the deletion frees the exploding actor.
    /// </summary>
    internal CrocomireProjectileBehavior MouthShot => field ??= new CrocomireProjectileBehavior(_projectiles, _deathsAndDrops, _access,
        CrocomireProjectileSegment.Shot);

    /// <summary>The bridge's fragments and the spike wall's pieces (#1760): their spawners start their animations.</summary>
    internal CrocomireProjectileBehavior Debris => field ??= new CrocomireProjectileBehavior(_projectiles, _deathsAndDrops, _access);

    /// <summary>One of Crocomire's projectiles: the shot's aim and flight, the spike wall's and the bridge's falls, and their cues.</summary>
    /// <param name="projectiles">The enemy projectiles, which raise the dust a landing spike-wall piece leaves.</param>
    /// <param name="deathsAndDrops">The drops a shot mouth shot leaves.</param>
    /// <param name="access">Crocomire's last shot's gradient and his sounds.</param>
    /// <param name="shot">Where the animation a destroying shot starts begins, or null for the shared delete animation.</param>
    internal sealed class CrocomireProjectileBehavior(
        EnemyProjectileEngine projectiles,
        EnemyDeathsAndDrops deathsAndDrops,
        CrocomireEncounterAccess access,
        CrocomireProjectileSegment? shot = null)
        : EnemyProjectileMotionBehavior<CrocomireProjectileMotion>(StartingMotions), IEnemyProjectileCues<CrocomireProjectileCue>
    {
        /// <summary>The gradient each mouth shot flies along, given as Crocomire spits it.</summary>
        internal EnemyProjectileStates<CrocomireProjectileState> Gradients { get; } = new();

        /// <summary>Which piece of the spike wall each falling piece is, given as the wall breaks.</summary>
        internal EnemyProjectileStates<CrocomireSpikeWallPieceState> SpikeWallPieces { get; } = new();

        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() =>
            shot is { } segment ? new CrocomireProjectileAnimation(this, segment) : base.ShotAnimation();

        /// <summary>The mouth shot aims, and the bridge fragments and spike-wall pieces fall, from their spawn.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, CrocomireProjectileMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.CrocomireProjectile] = CrocomireProjectileMotion.ShotAiming,
            [RoomEnemyProjectileKind.CrocomireBridgeCrumbling] = CrocomireProjectileMotion.BridgeFragmentFalling,
            [RoomEnemyProjectileKind.CrocomireSpikeWallPieces] = CrocomireProjectileMotion.SpikeWallCollapsing,
        };

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, CrocomireProjectileMotion motion)
        {
            switch (motion)
            {
                case CrocomireProjectileMotion.ShotAiming:
                    StartCrocomireProjectileFlight(context.Projectile, context.Level);
                    break;
                case CrocomireProjectileMotion.ShotFlying:
                    RunCrocomireProjectileFlight(context.Projectile, context.Level);
                    break;
                case CrocomireProjectileMotion.SpikeWallCollapsing:
                    RunCrocomireSpikeWallPiece(context.Projectile);
                    break;
                case CrocomireProjectileMotion.BridgeFragmentFalling:
                    RunCrocomireBridgeFragment(context.Projectile, context.Level);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(CrocomireProjectileMotion)} {motion}.");
            }
        }

        /// <summary>Carries out a cue of the shot's animation: the drop, allocated before the deletion frees the actor, or the deletion.</summary>
        public void RunCue(EnemyProjectileCueContext context, CrocomireProjectileCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case CrocomireProjectileCue.SpawnDrops:
                    deathsAndDrops.SpawnEnemyDropFromDefinition(projectile.Position.Whole, EnemyDefinitionId.Crocomire, context.Samus);
                    return;
                case CrocomireProjectileCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(CrocomireProjectileCue)} {cue}.");
            }
        }

        /// <summary>Runs the one-frame vector setup.</summary>
        private void StartCrocomireProjectileFlight(
            EnemyProjectile projectile,
            RoomLevelData level)
        {
            // The setup performs one horizontal collision move and deliberately ignores whether it hit terrain.
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);

            // The shot keeps the gradient its volley fires it along. The angle starts at up, not right: Y uses negative cosine.
            int gradient = Gradients.Of(projectile).Gradient;
            projectile.Velocity = CalculateCrocomireProjectileVelocity(EnemyMath.CalculateAimAngle(new(-CrocomireProjectileDefinitions.AimReachLeft, gradient)));
            projectile.StartMotion(this, CrocomireProjectileMotion.ShotFlying);
        }

        /// <summary>Runs the shard motion, exact to the 256th of a pixel, whose limits each piece takes from its place in the wall.</summary>
        private void RunCrocomireSpikeWallPiece(EnemyProjectile projectile)
        {
            CrocomireSpikeWallPieceState piece = SpikeWallPieces.Of(projectile);
            SignedSixteenSixteen maximumAcceleration = piece.Piece.MaximumAcceleration;
            SignedSixteenSixteen acceleration = piece.Acceleration;
            if (acceleration != maximumAcceleration)
            {
                acceleration += piece.Piece.AccelerationGrowth;
                if (acceleration >= maximumAcceleration)
                    acceleration = maximumAcceleration;
            }
            piece.Acceleration = acceleration;

            // The velocity gains the acceleration as Crocomire's falling pieces gain speed (CrocomireSpeedGain), up to this
            // piece's fastest: the piece starts at rest and its whole velocity stays at or below its cap of at most six pixels.
            SignedEightEight xVelocity = CrocomireSpeedGain.Gained(projectile.Velocity.X, acceleration,
                SignedEightEight.FromPixels(piece.Piece.FastestSpeed));
            projectile.Velocity = projectile.Velocity.WithX(xVelocity);

            // The piece moves across by its velocity, a 256th of a pixel at its finest: its place within a 256th of a
            // pixel stays exactly as it was.
            projectile.Position = projectile.Position.WithX(projectile.Position.X.MovedBy(xVelocity));

            // The piece falls by its speed, which gains the piece's gravity each update.
            piece.FallSpeed += CrocomireSpikeMotionDefinitions.PieceGravity;
            projectile.Position = projectile.Position.AdvanceY(piece.FallSpeed);

            if (projectile.Position.Y.Whole < CrocomireSpikeMotionDefinitions.LandingY)
                return;

            Position16<RoomPixels> impact = projectile.Position.Whole;
            projectile.Delete();
            if (piece.Piece.CrumbleSound is { } crumble)
                access.RequestCrocomireSound(crumble);
            projectiles.SpawnDustOrExplosion(impact, DustExplosionAnimation.BigDustCloud);
            access.RequestCrocomireSound(CrocomireSoundDefinitions.BigExplosion);
        }
    }

    /// <summary>Routes power-bomb hits on <paramref name="body"/> to his encounter's authored reaction.</summary>
    /// <param name="body">His body, whose encounter the reaction acts on.</param>
    internal static EnemyPowerBombReaction CrocomirePowerBombReactionOf(CrocomireBodyEntity body)
    {
        Ensure.NotNull(body);
        return new()
        {
            ReplacesCommon = c => ResolveCrocomirePowerBombReaction(c.Enemy, body.Encounter),
        };
    }

    /// <summary>Routes <paramref name="part"/>'s body, mouth, and auxiliary hitboxes to their distinct shot reactions on its encounter.</summary>
    /// <param name="part">His body or tongue, whose encounter the reactions act on.</param>
    internal EnemyShotReaction CrocomireShotReactionOf(CrocomireEntity part)
    {
        Ensure.NotNull(part);
        return new()
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (c.HitboxShot.IsOrdinary)
                    return false;
                if (!c.UsesFrameHitboxes)
                    throw new InvalidDataException("Crocomire's parts require collision by their frames' hitboxes.");
                c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
                ResolveCrocomireHitboxShot(part.Encounter, c.Shot.Type, c.Projectile.Position.Whole, c.HitboxShot, c.Camera);
                return true;
            },
        };
    }

    /// <summary>The touch reactions that push Samus back as well as hurting her, with the push: his claws'.</summary>
    private static readonly IReadOnlyDictionary<EnemyReaction, int> TouchPushes = new Dictionary<EnemyReaction, int>
    {
        [CrocomireReactions.ClawTouch] = CrocomireFightDefinitions.ClawPush,
    };

    /// <summary>The steps a weapon striking his mouth on screen pushes him back by; a weapon not listed pushes nothing.</summary>
    private static readonly IReadOnlyDictionary<SamusProjectileFamily, int> MouthPushSteps = new Dictionary<SamusProjectileFamily, int>
    {
        [SamusProjectileFamily.Beam] = CrocomireFightDefinitions.ChargedBeamSteps,
        [SamusProjectileFamily.Missile] = CrocomireFightDefinitions.MissileSteps,
        [SamusProjectileFamily.SuperMissile] = CrocomireFightDefinitions.SuperMissileSteps,
    };

    /// <summary>Runs Crocomire's component contact, including normal enemy touch damage.</summary>
    internal void ReactToCrocomireTouch(EnemyTouchContext context)
    {
        if (!context.UsesFrameHitboxes)
            throw new InvalidDataException("Crocomire's parts require collision by their frames' hitboxes.");
        if (context.HitboxTouch == EnemyReaction.Ignore)
            return; // The corpse's rectangles ignore Samus's touch.
        if ((context.HitboxTouch != CrocomireReactions.ClawTouch && context.HitboxTouch != EnemyReaction.OrdinaryTouch))
        {
            throw new InvalidDataException(
                $"Crocomire has no response to touch reaction {context.HitboxTouch}.");
        }

        _collision.ResolveOrdinaryTouch(context);
        // A claw's touch also pushes Samus back.
        if (TouchPushes.TryGetValue(context.HitboxTouch, out int push))
            context.Samus.Kinematics.ExtraDisplacement = context.Samus.Kinematics.ExtraDisplacement.WithWholeX(push);
    }

    /// <summary>
    /// Runs the shot reaction a Crocomire hitbox selected after frame-hitbox collision; <paramref name="camera"/> is the
    /// camera as the shot or bomb pass runs.
    /// </summary>
    private void ResolveCrocomireHitboxShot(
        CrocomireEnemyState state,
        SamusProjectileType projectileType,
        Position16<RoomPixels> projectile,
        EnemyReaction reaction,
        Position16<RoomPixels> camera)
    {
        if (!HitboxShotResponses.TryGetValue(reaction, out CrocomireHitboxShotResponse? response))
            throw new InvalidOperationException($"Crocomire has no response to shot reaction {reaction}.");
        response(state, projectileType, projectile, camera);
    }

    /// <summary>What a shot meeting one of his hitboxes' own reactions does to him.</summary>
    private delegate void CrocomireHitboxShotResponse(CrocomireEnemyState state, SamusProjectileType projectileType,
        Position16<RoomPixels> projectile, Position16<RoomPixels> camera);

    /// <summary>
    /// Each of his hitboxes' own shot reactions with what a shot meeting it does: a shot on his raised or high mouth or
    /// his lower body is absorbed, neither hurting him nor raising dust; a dust hitbox raises dust; his mouth runs its
    /// reaction.
    /// </summary>
    private IReadOnlyDictionary<EnemyReaction, CrocomireHitboxShotResponse> HitboxShotResponses => field ??=
        new Dictionary<EnemyReaction, CrocomireHitboxShotResponse>
        {
            [CrocomireReactions.AbsorbedShot] = (_, _, _, _) => { },
            [CrocomireReactions.DustShot] = (_, projectileType, projectile, _) => SpawnCrocomireShotDust(projectileType, projectile),
            [CrocomireReactions.MouthShot] = (state, projectileType, projectile, camera) =>
                ResolveCrocomireMouthShot(state, state.Body, projectileType, projectile, camera),
        };

    /// <summary>
    /// Resolves Crocomire's mouth reaction. Whether his body's left edge is on screen reads <paramref name="camera"/>, the
    /// camera as the shot or bomb pass runs, as the game reads the live camera there (#1875).
    /// </summary>
    private void ResolveCrocomireMouthShot(
        CrocomireEnemyState state,
        RoomEnemyPlace body,
        SamusProjectileType projectileType,
        Position16<RoomPixels> projectile,
        Position16<RoomPixels> camera)
    {
        body.Invincibility.End();

        // A hit while Crocomire's body is off screen skips choosing the projectile's family and still records the
        // pending hit. The body is on screen while its left edge lies left of the camera's right edge, its screen reach past
        // the camera.
        bool bodyLeftEdgeIsOnScreen = body.Position.X.Whole - body.Radius.X - CrocomireFightDefinitions.MouthShotScreenReach < camera.X;
        // The steps the hit pushes him back by, a hit off screen none, or null for a weapon on screen that pushes nothing.
        int? pushedSteps = 0;
        SamusProjectileFamily family = projectileType.Family;
        if (bodyLeftEdgeIsOnScreen)
        {
            if (family == SamusProjectileFamily.Beam && !projectileType.IsChargedBeam)
            {
                body.AnimationClock.HoldFor(CrocomireFightDefinitions.MouthShotReactionHoldFrames);
                SpawnCrocomireShotDust(projectileType, projectile);
                return;
            }
            pushedSteps = MouthPushSteps.TryGetValue(family, out int weaponSteps) ? weaponSteps : null;
        }

        if (pushedSteps is int steps)
        {
            state.StepCounter += steps;
            if (state.StepBack.Ask())
                body.AnimationClock.Extend(CrocomireFightDefinitions.MouthShotReactionHoldFrames);
        }

        body.HurtFlash.Lengthen(CrocomireFightDefinitions.MouthShotFlash);
        body.AiStates |= EnemyAiStates.Hurt;
    }

    /// <summary>Places a dust effect at the shot impact while preserving its component-relative offset.</summary>
    private void SpawnCrocomireShotDust(
        SamusProjectileType projectileType,
        Position16<RoomPixels> projectile)
    {
        DustExplosionAnimation dust = projectileType.Family != SamusProjectileFamily.SuperMissile
            ? DustExplosionAnimation.DudShotTinyExplosion
            : DustExplosionAnimation.BigExplosion;
        _projectiles.SpawnDustOrExplosion(projectile, dust);
    }

    /// <summary>Runs Crocomire's power-bomb reaction without applying normal power-bomb HP damage.</summary>
    private static void ResolveCrocomirePowerBombReaction(RoomEnemyPlace body, CrocomireEnemyState state)
    {
        if (state.DeathStage.Entry is not CrocomireDeathStage.Fighting)
            return;

        state.StepCounter = CrocomireFightDefinitions.PowerBombSteps;
        if (state.Phase == CrocomirePhase.PowerBombCharge)
            return;

        body.HurtFlash.Lengthen(CrocomireFightDefinitions.PowerBombFlash);
        body.AiStates |= EnemyAiStates.Hurt;
        state.Phase = CrocomirePhase.PowerBombCharge;

        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, SelectCrocomirePowerBombAnimation(state));
    }

    /// <summary>
    /// Selects the fully-open, part-open or closed reaction from the mouth of the frame shown, as the power-bomb reaction does
    /// by reading the frame's mouth. Before any frame is shown the body has no frame and reacts closed.
    /// </summary>
    private static CrocomireBodyAnimation SelectCrocomirePowerBombAnimation(CrocomireEnemyState state) =>
        state.BodyFrame is { } frame
            ? CrocomirePowerBombReactions.ForFrame(frame)
            : CrocomireBodyAnimation.PowerBombReactionMouthNotOpen;

    /// <summary>Flies a fight projectile, deleting it on the first horizontal/vertical wall hit.</summary>
    internal static void RunCrocomireProjectileFlight(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Delete();
        }
    }

    /// <summary>Projects a projectile's direction into its sideways and vertical speeds: its cosine and negative sine at the volley's speed.</summary>
    private static PixelVelocity<RoomPixels> CalculateCrocomireProjectileVelocity(TrigonometryAngle aim) => new(
        EnemyMath.SineVelocity(aim + TrigonometryAngle.QuarterTurn, CrocomireProjectileDefinitions.VolleySpeed),
        EnemyMath.SineVelocity(aim + TrigonometryAngle.HalfTurn, CrocomireProjectileDefinitions.VolleySpeed));

    /// <summary>Runs bridge-fragment vertical collision and gravity.</summary>
    internal static void RunCrocomireBridgeFragment(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Delete();
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + CrocomireBridgeFragmentDefinitions.Gravity);
    }

    /// <summary>
    /// The first hitbox the probe overlaps on the body's frame, whose reaction runs, or none; only his body's entity asks
    /// (#2179).
    /// </summary>
    /// <exception cref="InvalidDataException">The frame has no body hitboxes.</exception>
    internal static EnemyHitboxContact FindCrocomireBodyHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe)
    {
        if (!CrocomireBodyCollision.HasFrame(enemy.Frame))
            throw EnemyCollision.UncoveredFrame(enemy);

        // His body's hitboxes are the game's, whatever his installed pictures show; the first overlap wins, in the
        // order of his components and their hitboxes.
        foreach (EnemyCollisionComponent component in
                 CrocomireBodyCollision.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyCollisionHitbox hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        componentOrigin.X + hitbox.Left,
                        componentOrigin.Y + hitbox.Top,
                        componentOrigin.X + hitbox.Right,
                        componentOrigin.Y + hitbox.Bottom))
                    continue;
                return EnemyHitboxContact.Struck(probe.SelectsShot ? hitbox.Shot : hitbox.Touch);
            }
        }
        return EnemyHitboxContact.Missed;
    }

}
