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

    /// <summary>Runs Crocomire's eight-color body hurt flash.</summary>
    internal void ApplyCrocomireHurtFlash(RoomEnemyPlace body, CrocomireEnemyState state)
    {
        bool white = body.FlashTimer != 0 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(_frameContext.EnemyPassCount);
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
        // therefore reach the same hit counter, dust, or mouth reaction selected by the map.
        ResolveCrocomireHitboxShot(state, c.Bomb.Type, c.Bomb.Position.Whole, c.SelectedShot, c.Camera);

    /// <summary>
    /// The mouth's shot (#1760): its spawner starts its animation, and a shot one runs its typed shot animation, whose
    /// drop is allocated with Crocomire's chances before the deletion frees the exploding actor.
    /// </summary>
    internal EnemyProjectileBehavior MouthShot => field ??= new CrocomireProjectileBehavior(_projectiles, _deathsAndDrops, _access,
        () => new CrocomireProjectileAnimation(CrocomireProjectileSegment.Shot));

    /// <summary>The bridge's fragments and the spike wall's pieces (#1760): their spawners start their animations.</summary>
    internal EnemyProjectileBehavior Debris => field ??= new CrocomireProjectileBehavior(_projectiles, _deathsAndDrops, _access);

    /// <summary>One of Crocomire's projectiles: the shot's aim and flight, the spike wall's and the bridge's falls, and their cues.</summary>
    /// <param name="projectiles">The enemy projectiles, which raise the dust a landing spike-wall piece leaves.</param>
    /// <param name="deathsAndDrops">The drops a shot mouth shot leaves.</param>
    /// <param name="access">Crocomire's last shot's gradient and his sounds.</param>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    private sealed class CrocomireProjectileBehavior(
        EnemyProjectileEngine projectiles,
        EnemyDeathsAndDrops deathsAndDrops,
        CrocomireEncounterAccess access,
        Func<EnemyProjectileAnimation>? shot = null)
        : EnemyProjectileMotionBehavior<CrocomireProjectileMotion>(StartingMotions, shot), IEnemyProjectileCues<CrocomireProjectileCue>
    {
        /// <summary>The mouth shot aims, and the bridge fragments and spike-wall pieces fall, from their spawn.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, CrocomireProjectileMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.CrocomireProjectile] = CrocomireProjectileMotion.ShotAiming,
            [RoomEnemyProjectileKind.CrocomireBridgeCrumbling] = CrocomireProjectileMotion.BridgeFragmentFalling,
            [RoomEnemyProjectileKind.CrocomireSpikeWallPieces] = CrocomireProjectileMotion.SpikeWallCollapsing,
        };

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, CrocomireProjectileMotion motion)
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
        private static void StartCrocomireProjectileFlight(
            EnemyProjectile projectile,
            RoomLevelData level)
        {
            // The setup performs one horizontal collision move and deliberately ignores whether it hit terrain.
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);

            // The shot keeps the gradient its volley fires it along. The angle starts at up, not right: Y uses negative cosine.
            int gradient = projectile.StateOf<CrocomireProjectileState>().Gradient;
            projectile.Velocity = CalculateCrocomireProjectileVelocity(EnemyMath.CalculateBearing(new(-CrocomireProjectileDefinitions.AimReachLeft, gradient)));
            projectile.StartMotion(CrocomireProjectileMotion.ShotFlying);
        }

        /// <summary>Runs the shard motion, exact to the 256th of a pixel, whose limits each piece takes from its place in the wall.</summary>
        private void RunCrocomireSpikeWallPiece(EnemyProjectile projectile)
        {
            CrocomireSpikeWallPieceState piece = projectile.StateOf<CrocomireSpikeWallPieceState>();
            SignedSixteenSixteen maximumAcceleration = CrocomireSpikeMotion.MaximumAcceleration(piece.Piece);
            SignedSixteenSixteen acceleration = piece.Acceleration;
            if (acceleration != maximumAcceleration)
            {
                acceleration += CrocomireSpikeMotion.AccelerationDelta(piece.Piece);
                if (acceleration >= maximumAcceleration)
                    acceleration = maximumAcceleration;
            }
            piece.Acceleration = acceleration;

            // The velocity gains the acceleration as Crocomire's falling pieces gain speed (CrocomireSpeedGain), up to this
            // piece's fastest: the piece starts at rest and its whole velocity stays at or below its cap of at most six pixels.
            SignedEightEight xVelocity = CrocomireSpeedGain.Gained(projectile.Velocity.X, acceleration,
                SignedEightEight.FromPixels(CrocomireSpikeMotion.MaximumVelocity(piece.Piece)));
            projectile.Velocity = projectile.Velocity.WithX(xVelocity);

            // The piece moves across by its velocity, a 256th of a pixel at its finest: its place within a 256th of a
            // pixel stays exactly as it was.
            FixedCoordinate x = new FixedCoordinate(projectile.Position.X.Whole, projectile.Position.X.Fraction)
                .MovedBy(xVelocity);
            projectile.Position = projectile.Position.WithFractionX(x.Fraction).WithWholeX(x.Whole);

            // The piece falls by its speed, which gains three sixteenths of a pixel of gravity per update.
            piece.FallSpeed += CrocomireSpikeMotionDefinitions.PieceGravity;
            projectile.Position = projectile.Position.AdvanceY(piece.FallSpeed);

            if (projectile.Position.Y.Whole < CrocomireSpikeMotionDefinitions.LandingY)
                return;

            Position16<RoomPixels> impact = projectile.Position.Whole;
            bool crumbles = CrocomireSpikeWallCrumbleSounds.Crumbles(piece.Piece);
            projectile.Delete();
            if (crumbles)
                access.RequestCrocomireSound(CrocomireSoundDefinitions.Crumble);
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
                if (c.HitboxShot == EnemyReaction.NormalEnemyShot)
                    return false;
                if (!c.UsesFrameHitboxes)
                    throw new InvalidDataException("Crocomire's parts require collision by their frames' hitboxes.");
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
                ResolveCrocomireHitboxShot(part.Encounter, c.Projectile.Type, c.Projectile.Position.Whole, c.HitboxShot, c.Camera);
                return true;
            },
        };
    }

    /// <summary>Runs Crocomire's component contact, including normal enemy touch damage.</summary>
    internal void ReactToCrocomireTouch(EnemyTouchContext context)
    {
        if (!context.UsesFrameHitboxes)
            throw new InvalidDataException("Crocomire's parts require collision by their frames' hitboxes.");
        if (context.HitboxTouch == EnemyReaction.Ignore)
            return; // The corpse's rectangles ignore Samus's touch.
        if ((context.HitboxTouch != CrocomireReactions.ClawTouch && context.HitboxTouch != EnemyReaction.NormalEnemyTouch))
        {
            throw new InvalidDataException(
                $"Crocomire has no response to touch reaction {context.HitboxTouch?.ToString() ?? "(none)"}.");
        }

        _collision.ResolveNormalEnemyTouch(context);
        // A claw's touch also pushes Samus back.
        if (context.HitboxTouch == CrocomireReactions.ClawTouch)
            context.Samus.Kinematics.ExtraDisplacement = context.Samus.Kinematics.ExtraDisplacement.WithWholeX(CrocomireFightDefinitions.ClawPush);
    }

    /// <summary>
    /// Runs the shot reaction a Crocomire hitbox selected after frame-hitbox collision; <paramref name="camera"/> is the
    /// camera as the shot or bomb pass runs.
    /// </summary>
    private void ResolveCrocomireHitboxShot(
        CrocomireEnemyState state,
        SamusProjectileType projectileType,
        Position16<RoomPixels> projectile,
        EnemyReaction? reaction,
        Position16<RoomPixels> camera)
    {
        RoomEnemyPlace body = state.Body;

        // A shot on his raised or high mouth or his lower body is absorbed: it neither hurts him nor raises dust.
        if (reaction == CrocomireReactions.AbsorbedShot)
            return;

        if (reaction == CrocomireReactions.DustShot)
        {
            SpawnCrocomireShotDust(projectileType, projectile);
            return;
        }

        if (reaction == CrocomireReactions.MouthShot)
        {
            ResolveCrocomireMouthShot(
                state,
                body,
                projectileType,
                projectile,
                camera);
            return;
        }

        throw new InvalidOperationException($"Crocomire has no response to shot reaction {reaction?.ToString() ?? "(none)"}.");
    }

    /// <summary>
    /// Resolves Crocomire's mouth reaction. Whether his body's left edge is on screen reads <paramref name="camera"/>, the
    /// camera as the shot or bomb pass runs, as the game reads the live camera there (#1875).
    /// </summary>
    internal void ResolveCrocomireMouthShot(
        CrocomireEnemyState state,
        RoomEnemyPlace body,
        SamusProjectileType projectileType,
        Position16<RoomPixels> projectile,
        Position16<RoomPixels> camera)
    {
        body.InvincibilityTimer = 0;

        // A hit while Crocomire's body is off screen skips choosing the projectile's family and still records the
        // pending hit. The body is on screen while its left edge lies left of the camera's right edge, 256 pixels past
        // the camera.
        bool bodyLeftEdgeIsOnScreen = body.Position.X.Whole - body.Radius.X - CrocomireFightDefinitions.MouthShotScreenReach < camera.X;
        int stepCount = 0;
        SamusProjectileFamily family = projectileType.Family;
        if (bodyLeftEdgeIsOnScreen)
        {
            if (family == SamusProjectileFamily.Beam)
            {
                if (!projectileType.IsChargedBeam)
                {
                    body.AnimationClock.HoldFor(CrocomireFightDefinitions.MouthShotReactionHoldFrames);
                    SpawnCrocomireShotDust(projectileType, projectile);
                    return;
                }
                stepCount = CrocomireFightDefinitions.ChargedBeamSteps;
            }
            else if (family == SamusProjectileFamily.Missile)
            {
                stepCount = CrocomireFightDefinitions.MissileSteps;
            }
            else if (family == SamusProjectileFamily.SuperMissile)
            {
                stepCount = CrocomireFightDefinitions.SuperMissileSteps;
            }
        }

        if (!bodyLeftEdgeIsOnScreen || stepCount != 0)
        {
            state.StepCounter += stepCount;
            if (!state.DamageReactionPending)
            {
                body.AnimationClock.Extend(CrocomireFightDefinitions.MouthShotReactionHoldFrames);
            }
            state.DamageReactionPending = true;
        }

        body.FlashTimer += CrocomireFightDefinitions.MouthShotFlash;
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

        body.FlashTimer += CrocomireFightDefinitions.PowerBombFlash;
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

    /// <summary>Projects a projectile angle into its sideways and vertical speeds: its sine and negative cosine scaled by the volley's multiplier.</summary>
    internal static EightEightVector<RoomPixels> CalculateCrocomireProjectileVelocity(Bearing bearing)
    {
        // A negative sine scales exactly as a positive one does. The bearing is used as a heading without
        // conversion, so X is its sine and Y its negative cosine.
        TrigonometryAngle heading = bearing.ToMirroredTrigonometryAngle();
        return EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(
            Trigonometry.SignedSine(heading) * CrocomireProjectileDefinitions.VelocityMultiplier,
            Trigonometry.SignedSine(heading - TrigonometryAngle.QuarterTurn) * CrocomireProjectileDefinitions.VelocityMultiplier);
    }

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
    /// The reaction of the first hitbox the probe overlaps on the body's frame, or null when the frame has no body hitboxes;
    /// only his body's entity asks (#2179).
    /// </summary>
    internal static bool? FindCrocomireBodyHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (!CrocomireBodyCollision.HasFrame(enemy.Frame))
            return null;

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
                reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
                return true;
            }
        }
        return false;
    }

}
