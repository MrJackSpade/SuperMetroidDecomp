using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's hitboxes, touch, projectiles and hurt palette, and what Samus's weapons do to it.</summary>
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
    internal void ApplyCrocomireHurtFlash(RoomEnemySlot body)
    {
        bool white = body.FlashTimer != 0 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(_frameContext.RandomEnemyCounter);
        // His body shows its fight colours or the white flash as the import's shades of them (#1512).
        (_access.Crocomire ?? throw new InvalidOperationException("Crocomire's hurt flash runs once Crocomire is loaded."))
            .Colours.ShowBody(white);
    }

    /// <summary>Runs the component-specific reaction a bomb's hitbox selects, before the check for an ignoring reaction.</summary>
    internal void ReactToCrocomireBomb(EnemyBombContext c) =>
        // Crocomire's species definition names no shot reaction; every live component supplies its own. Normal bombs
        // therefore reach the same hit counter, dust, or mouth reaction selected by the map.
        ResolveCrocomireHitboxShot(c.Bomb.Type, c.Bomb.Position.Whole, c.SelectedShot);

    /// <summary>
    /// The mouth's shot (#1760): its spawner starts its animation, and a shot one runs its typed shot animation, whose
    /// drop is allocated with Crocomire's chances before the deletion frees the exploding actor.
    /// </summary>
    internal EnemyProjectileBehavior MouthShot => field ??= new CrocomireProjectileBehavior(this,
        () => new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.Shot));

    /// <summary>The bridge's fragments and the spike wall's pieces (#1760): their spawners start their animations.</summary>
    internal EnemyProjectileBehavior Debris => field ??= new CrocomireProjectileBehavior(this);

    /// <summary>One of Crocomire's projectiles: the shot's aim and flight, the spike wall's and the bridge's falls, and their cues.</summary>
    private sealed class CrocomireProjectileBehavior(CrocomireCombat owner, Func<EnemyProjectileAnimation>? shot = null)
        : EnemyProjectileBehavior(shot: shot), IEnemyProjectileCues<CrocomireProjectileCue>
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileFrameBehaviorContext context)
        {
            EnemyProjectileFrameBehavior motion = context.Projectile.FrameBehavior;
            if (motion == EnemyProjectileFrameBehavior.CrocomireShotAiming)
            {
                owner.StartCrocomireProjectileFlight(context.Projectile, context.Level);
            }
            else if (motion == EnemyProjectileFrameBehavior.CrocomireShotFlight)
            {
                RunCrocomireProjectileFlight(context.Projectile, context.Level);
            }
            else if (motion == EnemyProjectileFrameBehavior.CrocomireSpikeWallCollapse)
            {
                owner.RunCrocomireSpikeWallPiece(context.Projectile);
            }
            else if (motion == EnemyProjectileFrameBehavior.CrocomireBridgeFragmentFall)
            {
                RunCrocomireBridgeFragment(context.Projectile, context.Level);
            }
            else
            {
                base.Move(context);
            }
        }

        /// <summary>Carries out a cue of the shot's animation: the drop, allocated before the deletion frees the actor, or the deletion.</summary>
        public void RunCue(EnemyProjectileCueContext context, CrocomireProjectileCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case CrocomireProjectileCue.SpawnDrops:
                    owner._deathsAndDrops.SpawnEnemyDropFromDefinition(projectile.Position.Whole, EnemyDefinitionId.Crocomire);
                    return;
                case CrocomireProjectileCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(CrocomireProjectileCue)} {cue}.");
            }
        }
    }

    /// <summary>Routes power-bomb hits on each Crocomire component to their authored reactions.</summary>
    internal EnemyPowerBombReaction CrocomirePowerBombReaction => field ??= new()
    {
        ReplacesCommon = (enemy, _) => ResolveCrocomirePowerBombReaction(enemy),
    };

    /// <summary>Routes body, mouth, and auxiliary hitboxes to their distinct shot reactions.</summary>
    internal EnemyShotReaction CrocomireShotReaction => field ??= new()
    {
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (c.HitboxShot == EnemyReaction.NormalEnemyShot)
                return false;
            if (!c.UsesFrameHitboxes)
                throw new InvalidDataException("Crocomire's parts require collision by their frames' hitboxes.");
            c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
            ResolveCrocomireHitboxShot(c.Projectile.Type, c.Projectile.Position.Whole, c.HitboxShot);
            return true;
        },
    };

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

        _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
        // A claw's touch also pushes Samus back. The game latches the touch too, but only its unused claw attack
        // reads that latch, so it is not kept.
        if (context.HitboxTouch == CrocomireReactions.ClawTouch)
            context.Samus.Kinematics.ExtraDisplacement = context.Samus.Kinematics.ExtraDisplacement.WithWholeX(-4);
    }

    /// <summary>Runs the shot reaction a Crocomire hitbox selected after frame-hitbox collision.</summary>
    private void ResolveCrocomireHitboxShot(
        SamusProjectileType projectileType,
        Position16<RoomPixels> projectile,
        EnemyReaction? reaction)
    {
        CrocomireEnemyState state = _access.Crocomire ??
            throw new InvalidOperationException("Crocomire hitbox has no body owner.");
        RoomEnemySlot body = state.Body;

        // A shot on his raised or high mouth or his lower body is absorbed: it neither hurts him nor raises dust. The
        // game counted these and his mouth hits in a count nothing in the fight reads (#1620), so the count is gone.
        if (reaction == CrocomireReactions.HitCountShot)
            return;

        if ((reaction == CrocomireReactions.DustHitboxShot || reaction == CrocomireReactions.AlternateDustHitboxShot))
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
                projectile);
            return;
        }

        throw new InvalidOperationException($"Crocomire has no response to shot reaction {reaction?.ToString() ?? "(none)"}.");
    }

    /// <summary>
    /// The frames a mouth shot that starts a damage reaction adds to the hold of Crocomire's current animation frame,
    /// whatever his fight is doing: the game keeps one value for the projectile attack and one for every other fight
    /// phase, and both are this.
    /// </summary>
    internal const int MouthShotReactionHoldFrames = 8;

    /// <summary>Resolves Crocomire's mouth reaction.</summary>
    internal void ResolveCrocomireMouthShot(
        CrocomireEnemyState state,
        RoomEnemySlot body,
        SamusProjectileType projectileType,
        Position16<RoomPixels> projectile)
    {
        body.InvincibilityTimer = 0;

        // The off-screen branch bypasses projectile-family selection and still publishes
        // the pending-hit state. This wrapped signed comparison is the game's exact
        // `body left edge - (camera + 256)` test.
        bool bodyLeftEdgeIsOnScreen = CoordinateAxis.Difference(
            body.Position.X.Whole - body.Radius.X - 256, state.Camera.X) < 0;
        int stepCount = 0;
        SamusProjectileFamily family = projectileType.Family;
        if (bodyLeftEdgeIsOnScreen)
        {
            if (family == SamusProjectileFamily.Beam)
            {
                if (!projectileType.IsChargedBeam)
                {
                    body.AnimationClock.HoldFor(8);
                    SpawnCrocomireShotDust(projectileType, projectile);
                    return;
                }
                stepCount = 2;
            }
            else if (family == SamusProjectileFamily.Missile)
            {
                stepCount = 1;
            }
            else if (family == SamusProjectileFamily.SuperMissile)
            {
                stepCount = 3;
            }
        }

        if (!bodyLeftEdgeIsOnScreen || stepCount != 0)
        {
            state.StepCounter += stepCount;
            if (!state.DamageReactionPending)
            {
                body.AnimationClock.Extend(MouthShotReactionHoldFrames);
            }
            state.DamageReactionPending = true;
        }

        body.FlashTimer += 14;
        body.AiStates |= EnemyAiStates.Hurt;
    }

    /// <summary>Places a dust effect at the shot impact while preserving its component-relative offset.</summary>
    private void SpawnCrocomireShotDust(
        SamusProjectileType projectileType,
        Position16<RoomPixels> projectile)
    {
        MiscDustAnimation animationIndex = projectileType.Family != SamusProjectileFamily.SuperMissile
            ? MiscDustAnimation.DudShotTinyExplosion
            : MiscDustAnimation.BigExplosion;
        _projectiles.SpawnRoomGraphicsDustExplosion(projectile, animationIndex);
    }

    /// <summary>Runs Crocomire's power-bomb reaction without applying normal power-bomb HP damage.</summary>
    private void ResolveCrocomirePowerBombReaction(RoomEnemySlot body)
    {
        CrocomireEnemyState state = _access.RequireCrocomire(body);
        if (state.DeathPhase != CrocomireDeathPhase.Fighting)
            return;

        state.StepCounter = 3;
        if (state.Phase == CrocomirePhase.PowerBombCharge)
            return;

        body.FlashTimer += 4;
        body.AiStates |= EnemyAiStates.Hurt;
        state.Phase = CrocomirePhase.PowerBombCharge;

        CrocomireEncounterAccess.StartCrocomireBodyAnimation(state, SelectCrocomirePowerBombAnimation(state));
    }

    /// <summary>
    /// Selects the fully-open, part-open or closed reaction from the mouth of the frame shown, as the power-bomb reaction does
    /// by reading the frame's mouth. Before any frame is shown the body has no frame and reacts closed.
    /// </summary>
    private static CrocomireBodySegment SelectCrocomirePowerBombAnimation(CrocomireEnemyState state) =>
        state.BodyFrame is { } frame
            ? CrocomirePowerBombReactions.ForFrame(frame)
            : CrocomireBodySegment.PowerBombReactionMouthNotOpen;

    /// <summary>Runs the one-frame vector setup.</summary>
    internal void StartCrocomireProjectileFlight(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        // The setup performs one horizontal collision move and deliberately ignores whether it hit terrain.
        EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);

        // The last shot reads the gradient after the authored ones, which the installed service holds; do not clamp
        // it to the last authored gradient. The angle starts at up, not right: Y uses negative cosine.
        int shot = projectile.StateOf<CrocomireProjectileState>().Shot;
        int gradient = CrocomireVolley.IsLastShot(shot)
            ? _access.TenthShotGradient.Gradient
            : CrocomireVolley.AuthoredGradient(shot);
        projectile.Velocity = CalculateCrocomireProjectileVelocity(EnemyMath.CalculateBearing(new(-64, gradient)));
        projectile.FrameBehavior =
            EnemyProjectileFrameBehavior.CrocomireShotFlight;
    }

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

    /// <summary>Projects a projectile angle into signed 1/256-pixel X/Y speed: its sine and negative cosine scaled by the volley's multiplier.</summary>
    internal static EightEightVector<RoomPixels> CalculateCrocomireProjectileVelocity(Bearing bearing)
    {
        // A negative sine scales exactly as a positive one does. The
        // table is read with the bearing unconverted, so X is its sine and Y its negative cosine.
        TrigonometryAngle heading = bearing.ToMirroredTableAngle();
        return EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(
            Trigonometry.SignedSine(heading) * CrocomireProjectileDefinitions.VelocityMultiplier,
            Trigonometry.SignedSine(heading - TrigonometryAngle.QuarterTurn) * CrocomireProjectileDefinitions.VelocityMultiplier);
    }

    /// <summary>The gravity a falling spike-wall piece gains each update.</summary>
    private static readonly SignedSixteenSixteen SpikeWallPieceGravity = SignedSixteenSixteen.FromSixteenthsOfPixel(3);

    /// <summary>Runs the shard motion, exact to the 256th of a pixel, whose limits each piece takes from its place in the wall.</summary>
    internal void RunCrocomireSpikeWallPiece(EnemyProjectile projectile)
    {
        CrocomireSpikeWallPieceState piece = projectile.StateOf<CrocomireSpikeWallPieceState>();
        int maximumAcceleration = CrocomireSpikeMotion.MaximumAcceleration(piece.Piece);
        int acceleration = piece.Acceleration;
        if (acceleration != maximumAcceleration)
        {
            acceleration += CrocomireSpikeMotion.AccelerationDelta(piece.Piece);
            if (acceleration >= maximumAcceleration)
                acceleration = maximumAcceleration;
        }
        piece.Acceleration = acceleration;

        // The acceleration's whole part is added to the velocity in 256ths of a pixel: a fraction that passes a whole
        // pixel raises the whole velocity by one, and the whole pixels are then capped by this piece's maximum. The
        // piece starts at rest and its whole velocity stays at or below its cap of at most six pixels.
        SignedEightEight xVelocity = (projectile.Velocity.X + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(UnsignedEightEight.Whole(acceleration)))
            .WithWholePixelsAtMost(CrocomireSpikeMotion.MaximumVelocity(piece.Piece));
        projectile.Velocity = projectile.Velocity.WithX(xVelocity);

        // Move X in 256ths of a pixel, as the game's projectile mover does: the position's precision finer than a
        // 256th of a pixel stays exactly as it was.
        FixedCoordinate x = new FixedCoordinate(projectile.Position.X.Whole, projectile.Position.X.Fraction)
            .MovedBy(xVelocity);
        projectile.Position = projectile.Position.WithFractionX(x.Fraction).WithWholeX(x.Whole);

        // The fall is ordinary signed 1/65536-pixel motion, with three sixteenths of a pixel of gravity per update.
        piece.FallSpeed += SpikeWallPieceGravity;
        projectile.Position = projectile.Position.AdvanceY(piece.FallSpeed);

        if (projectile.Position.Y.Whole < 168)
            return;

        Position16<RoomPixels> impact = projectile.Position.Whole;
        bool crumbles = CrocomireSpikeWallCrumbleSounds.Crumbles(piece.Piece);
        projectile.Delete();
        if (crumbles)
            _access.RequestCrocomireSound(CrocomireSoundDefinitions.Crumble);
        _projectiles.SpawnRoomGraphicsDustExplosion(impact, MiscDustAnimation.BigDustCloud);
        _access.RequestCrocomireSound(CrocomireSoundDefinitions.BigExplosion);
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

    /// <summary>The reaction of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindCrocomireBodyHitboxReaction(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (!(enemy.DefinitionId == EnemyDefinitionId.Crocomire &&
            CrocomireBodyCollision.HasFrame(enemy.Frame)))
            return null;

        // Physical body components and their hitbox lists are compiled
        // independently of Crocomire's replaceable sprite/body-layer presentation.
        // Preserve component/hitbox order: the first overlap wins.
        foreach (EnemyCollisionComponent component in
                 CrocomireBodyCollision.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyCollisionHitbox hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        CoordinateAxis.Wrap(componentOrigin.X + hitbox.Left),
                        CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Top),
                        CoordinateAxis.Wrap(componentOrigin.X + hitbox.Right),
                        CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Bottom)))
                    continue;
                reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
                return true;
            }
        }
        return false;
    }

    /// <summary>The reaction of the first hitbox the probe overlaps on the tongue's frame, or null when the occupant is not the tongue.</summary>
    /// <exception cref="InvalidDataException">The tongue's frame has no compiled collision.</exception>
    internal static bool? FindCrocomireTongueHitboxReaction(RoomEnemySlot enemy, out EnemyReaction? reaction)
    {
        reaction = null;
        if (enemy.DefinitionId != EnemyDefinitionId.CrocomireTongue)
            return null;

        // These nine selected frames have one component each, but both
        // referenced hitbox lists are empty. Validate the exact
        // engine-owned frame identity and return no touch/shot reaction.
        CrocomireTongueCollisionComponent component =
            CrocomireTongueCollision.ComponentAt(enemy.Frame);
        _ = CrocomireTongueCollision.HitboxCountAt(
            component.HitboxList);
        return false;
    }
}
