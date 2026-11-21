using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's hitboxes and touch, and what Samus's weapons do to his mouth and body.</summary>
[RoomLifetime]
internal sealed class KraidCombat
{
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues Kraid's encounter sound effects.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Spawns room effect actors for Kraid's visual effects, and gives the place a dud flash took.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    private readonly KraidParts _parts;
    /// <summary>Checks mouth, body, and part collisions against Samus and shots.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidCombat(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, RoomEffectActors effectActors, KraidParts parts, EnemyCollision collision)
    {
        _projectiles = projectiles;
        _audio = audio;
        _effectActors = effectActors;
        _parts = parts;
        _collision = collision;
    }

    /// <summary>Runs the arm's reaction to a bomb its hitboxes select, before the check for an ignoring reaction.</summary>
    internal void ReactToArmBomb(EnemyBombContext c)
    {
        if (c.SelectedShot != EnemyReaction.Ignore && c.SelectedShot != KraidReactions.ArmShot)
            throw new InvalidDataException($"Kraid's arm selected the bomb reaction {c.SelectedShot}.");

        // The arm's shot reaction raises dust where the shot struck; the hit's direction is already recorded.
        // Protected arm frames ignore the shot.
        if (c.SelectedShot == KraidReactions.ArmShot)
            SpawnKraidArmShotExplosion(c.PlacedBomb.Type, c.Bomb.Position.Whole);
    }

    /// <summary>Runs the foot's reaction to a bomb its hitboxes select: every foot rectangle ignores it.</summary>
    internal static void ReactToFootBomb(EnemyBombContext c)
    {
        if (c.SelectedShot != EnemyReaction.Ignore)
            throw new InvalidDataException($"Kraid's foot selected the bomb reaction {c.SelectedShot}.");
    }

    /// <summary>The arm's shot reaction, which its hitboxes decide behind its definition's ignoring one.</summary>
    internal EnemyShotReaction ArmShotReaction => field ??= new()
    {
        // Kraid's arm is admitted by its definition's ignoring shot reaction, which still lets its hitboxes decide; its
        // authored hitboxes select the arm's private reaction.
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (!c.UsesFrameHitboxes || (c.HitboxShot != EnemyReaction.Ignore && c.HitboxShot != KraidReactions.ArmShot))
                throw new InvalidDataException($"Kraid's arm selected the shot reaction {c.HitboxShot}.");

            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            if (c.HitboxShot == KraidReactions.ArmShot)
                SpawnKraidArmShotExplosion(c.ProjectileType, c.Projectile.Position.Whole);
            return true;
        },
    };

    /// <summary>The foot's shot reaction, which its hitboxes decide behind its definition's ignoring one: every rectangle ignores the shot.</summary>
    internal static EnemyShotReaction FootShotReaction { get; } = new()
    {
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (!c.UsesFrameHitboxes || c.HitboxShot != EnemyReaction.Ignore)
                throw new InvalidDataException($"Kraid's foot selected the shot reaction {c.HitboxShot}.");

            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            return true;
        },
    };

    /// <summary>Runs the arm's push against Samus's touch.</summary>
    /// <param name="context">The touch.</param>
    /// <param name="state">The encounter the arm is part of.</param>
    internal void ReactToArmTouch(EnemyTouchContext context, KraidEnemyState state)
    {
        // The arm's push forces the bottom lint's firing phase, so it fires immediately after
        // the arm launches Samus up and right.
        // Writes only the whole pixels of the displacement, so preserve both
        // fractions exactly as they were before contact.
        context.Samus.Kinematics.ExtraDisplacement = context.Samus.Kinematics.ExtraDisplacement.WithWhole(KraidContactDefinitions.PushBack);
        state.BottomLintState.Phase = KraidLintPhase.Fire;
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
    }

    /// <summary>
    /// Spawns the explosion of a shot that hits Kraid's arm, from the arm's hitbox reaction, in a free enemy-projectile
    /// place of the room, and plays the dud shot's sound.
    /// </summary>
    private void SpawnKraidArmShotExplosion(
        SamusProjectileType projectileType,
        Position16<RoomPixels> position)
    {
        // Super Missiles, Power Bombs and beam explosions take the big explosion, and every
        // other projectile the small one.
        DustExplosionAnimation explosion = projectileType.Family is SamusProjectileFamily.SuperMissile or
                SamusProjectileFamily.PowerBomb or SamusProjectileFamily.BeamExplosion
            ? DustExplosionAnimation.BigExplosion
            : DustExplosionAnimation.DudShotTinyExplosion;
        _projectiles.SpawnDustOrExplosion(position, explosion);
        _audio.QueueEnemySound(SamusSound.DudShot, priority: SoundRequestPriority.Normal);
    }



    /// <summary>
    /// Resolves projectile hits on Kraid's mouth. Projectile
    /// removal is deferred to the projectile's own update.
    /// </summary>
    internal void ResolveKraidMouthProjectileHits(RoomEnemyOccupant kraid, KraidEnemyState state, IReadOnlyList<KraidBehavior.KraidPassProjectile> tested)
    {
        RoomEnemyPlace body = Ensure.NotNull(kraid).Place;
        if (!state.BodyState.ShotsCollide)
            return;

        int mouthHits = 0;
        KraidMouthHitbox? innerMouth = tested.Count == 0 ? null : KraidHeadAnimations.InnerMouthAt(state.HeadPlayback);
        if (innerMouth is { } inner)
        {
            KraidPassBounds bounds = PlaceKraidPassBounds(body, inner);
            foreach (KraidBehavior.KraidPassProjectile projectile in tested)
            {
                if (projectile.Held is not { } held)
                    continue;
                KraidBehavior.KraidCollisionShot shot = held.Shot;
                SamusProjectileType type = shot.Type;
                if (!OverlapsKraidMouth(bounds, shot) ||
                    (type.Family == SamusProjectileFamily.Beam && !type.IsChargedBeam))
                    continue;

                // A charged beam into his mouth marks a charged hit as the game does, but his body's pass, which alone
                // reads the mark, forgets it before it tests, so only the body pass's own charged hits arm his mouth's
                // reopens (#2562).
                // Every beam takes Kraid's charged-beam response; the other families take their own.
                EnemyWeaponVulnerability vulnerability = type.Family switch
                {
                    SamusProjectileFamily.Beam => EnemyVulnerabilities.Of(kraid.Definition).ChargedBeam,
                    SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile or
                    SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb =>
                        EnemyCollision.ReadProjectileVulnerability(kraid, type),
                    SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion => EnemyWeaponVulnerability.NoDamage,
                    _ => throw new InvalidOperationException($"Undefined SamusProjectileFamily {type.Family}."),
                };
                int damage = vulnerability.DamageFrom(shot.Damage);
                if (damage != 0)
                {
                    // A damaging hit lets only the later shots from the row its kind names reach the mouth, which decides
                    // whether the next shot's test falls inside; a dud leaves its own bounds below.
                    bounds.LeaveAfterMouthHit(KraidMouthHitReach.HighestReachingRow(DamagingMouthHit(shot)));
                    body.Health = damage >= body.Health
                        ? 0
                        : body.Health - damage;
                    body.HurtFlash.Restart(body.HurtFlashFrames + EnemyCollisionDefinitions.WeaponHurtFlashExtension);
                    body.AiStates = body.AiStates | EnemyAiStates.Hurt;
                    if (type.HasAnyBeam(SamusBeamFlags.Plasma))
                        body.Invincibility.Restart(EnemyCollisionDefinitions.PlasmaShotInvincibilityUpdates);
                }
                else
                {
                    RoomEffectActor dud = _effectActors.SpawnRoomEffectActor(
                        shot.Position, RoomEffectActorKind.DudShot, graphics: SpriteGraphics.SharedStandard);
                    bounds.LeaveAfterDud(_effectActors.PlaceOf(dud), shot.Position);
                    _audio.QueueEnemySound(SamusSound.DudShot, priority: SoundRequestPriority.Low);
                }
                held.MarkCollision();
                mouthHits++;
            }
        }

        if (mouthHits != 0)
        {
            state.StartHurtFlash();
            state.MarkMouthReopen();
            if (body.Health < 1 && state.BodyState.Fights)
                BeginKraidDeath(state);
        }
    }

    /// <summary>
    /// Resolves projectile hits on Kraid's body, the independent
    /// outer mouth/body pass.
    /// </summary>
    internal void ResolveKraidBodyProjectileHits(RoomEnemyPlace body, KraidEnemyState state, IReadOnlyList<KraidBehavior.KraidPassProjectile> tested)
    {
        if (!state.BodyState.ShotsCollide)
            return;

        bool chargedHit = false;
        int bodyHits = 0;
        if (tested.Count != 0)
        {
            KraidMouthHitbox outerMouth = KraidHeadAnimations.OuterMouthAt(state.HeadPlayback);
            KraidPassBounds bounds = PlaceKraidPassBounds(body, outerMouth);
            foreach (KraidBehavior.KraidPassProjectile projectile in tested)
            {
                if (projectile.Held is not { } held)
                    continue;
                KraidBehavior.KraidCollisionShot shot = held.Shot;
                if (!OverlapsKraidBody(ref bounds, body, shot))
                    continue;
                SpawnKraidArmShotExplosion(shot.Type, shot.Position);
                bounds.LeaveAfterBodyHit(shot.Position);
                held.MarkCollision();
                if (shot.Type.IsChargedBeam)
                    chargedHit = true;
                bodyHits++;
            }
        }

        if (bodyHits != 0 && state.BodyState.Stage == KraidBodyStage.Fighting &&
            state.BodyState.FightPhase == KraidFightPhase.FirstPhaseThinking)
        {
            state.BodyState.FightPhase = KraidFightPhase.InitializeEyeGlow;
            if (chargedHit)
            {
                state.ArmMouthReopens();
            }
        }
    }

    /// <summary>Places a mouth shape at Kraid's body position as the bounds a collision pass starts from.</summary>
    private static KraidPassBounds PlaceKraidPassBounds(RoomEnemyPlace body, KraidMouthHitbox shape) =>
        PlaceKraidPassBounds(body, KraidMouthHitboxes.ResolveCollision(shape));

    /// <summary>Places a mouth shape's left, top and bottom edges at Kraid's body position.</summary>
    private static KraidPassBounds PlaceKraidPassBounds(RoomEnemyPlace body, KraidMouthCollisionEdges shape) =>
        KraidPassBounds.Placed(
            body.Position.X.Whole + shape.Left,
            body.Position.Y.Whole + shape.Top,
            body.Position.Y.Whole + shape.Bottom);

    /// <summary>Whether a projectile overlaps the pass's bounds as the mouth pass tests them.</summary>
    private static bool OverlapsKraidMouth(KraidPassBounds bounds, KraidBehavior.KraidCollisionShot shot) =>
        bounds.Overlaps(
            shot.Position.Y - shot.Radius.Y,
            shot.Position.Y + shot.Radius.Y,
            shot.Position.X + shot.Radius.X);

    /// <summary>
    /// Whether a projectile overlaps Kraid's body: inside the pass's bounds when it reaches above their bottom, and
    /// otherwise left of the body's contour at its height, which leaves the projectile's right edge as the bottom.
    /// </summary>
    private static bool OverlapsKraidBody(ref KraidPassBounds bounds, RoomEnemyPlace body, KraidBehavior.KraidCollisionShot shot)
    {
        if (bounds.IsAboveBottom(shot.Position.Y - shot.Radius.Y))
            return bounds.ReachesTopAndLeft(shot.Position.Y + shot.Radius.Y, shot.Position.X + shot.Radius.X);

        bounds.LeaveRightEdgeAsBottom(shot.Position.X + shot.Radius.X);
        int relativeY = shot.Position.Y - body.Position.Y.Whole;
        return body.Position.X.Whole + KraidBodyContour.LeftEdge(relativeY) < bounds.Bottom;
    }

    /// <summary>Switches the body from combat to its scripted death phases.</summary>
    private void BeginKraidDeath(KraidEnemyState state)
    {
        state.BodyState.DeathPhase = KraidDeathPhase.Initialize;
        state.ClearMouthReactions();
        // His parts stop touching Samus while his death moves his body and breaks him up. The fingernails are left out: they delete themselves once his health reads zero.
        foreach (RoomEnemyPlace part in _parts.AllButNails)
            part.Contact = part.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Checks the body hitbox and applies contact damage to Samus.</summary>
    /// <remarks>These collision checks belong to actor AI rather than generic interactive hitboxes. The body's
    /// permanent intangible flag and lint's ignoring touch reaction do not suppress these private paths.</remarks>
    internal void ResolveKraidBodyContact(RoomEnemyOccupant kraid, KraidEnemyState state, SamusState samus, PseudoScrewCharge pseudoScrew)
    {
        RoomEnemyPlace body = kraid.Place;
        if (!state.BodyState.Fights)
            return;
        int relativeY = samus.Position.Y.Whole - body.Position.Y.Whole;
        if (body.Position.X.Whole + KraidBodyContour.LeftEdge(relativeY) >=
            samus.Position.X.Whole + samus.Kinematics.Radius.X)
            return;
        if (samus.Position.X.Whole >= KraidContactDefinitions.MinimumBodyEjectionX)
            samus.Position -= new Offset16<RoomPixels>(KraidContactDefinitions.BodyEjection, 0);
        Position16<RoomPixels> ejected = samus.Position.Whole -
            new Offset16<RoomPixels>(0, KraidContactDefinitions.BodyEjection);
        samus.Position = samus.Position.WithWholeY(ejected.Y < state.EjectionLimitY
            ? state.EjectionLimitY : ejected.Y);
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(KraidContactDefinitions.PushBack);
        if (samus.InvincibilityTimer.IsZero)
            _collision.ResolveOrdinaryTouch(kraid, samus, pseudoScrew);
    }

    /// <summary>
    /// The first hitbox the probe overlaps on his foot's pose, whose reaction runs, or none; the foot's own entity asks with
    /// its own pose (#2301).
    /// </summary>
    /// <exception cref="InvalidDataException">The pose has no hitboxes.</exception>
    internal static EnemyHitboxContact FindKraidFootHitboxReaction(RoomEnemyPlace enemy, KraidPartPose? pose, EnemyHitboxProbe probe)
    {
        if (!KraidFootCollision.TryGetComponents(pose, out var footComponents))
            throw EnemyCollision.UncoveredFrame(enemy);

        // Kraid's foot art is replaceable, but each pose still places two physical components against one shared hitbox
        // list, moved with the pose's step.
        return FindComponentReaction(enemy, footComponents, probe);
    }

    /// <summary>
    /// The reaction of the first hitbox of <paramref name="components"/>, placed from <paramref name="enemy"/>'s position,
    /// that <paramref name="probe"/> overlaps, in the components' and their hitboxes' order (#2557): struck with that
    /// reaction, or missed.
    /// The arm and the foot are searched alike.
    /// </summary>
    private static EnemyHitboxContact FindComponentReaction(
        RoomEnemyPlace enemy, IEnumerable<KraidCollisionComponent> components, EnemyHitboxProbe probe)
    {
        foreach (KraidCollisionComponent component in components)
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
                return EnemyHitboxContact.Struck(probe.Contact == HitboxContactKind.Shot ? hitbox.Shot : hitbox.Touch);
            }
        }
        return EnemyHitboxContact.Missed;
    }

    /// <summary>
    /// The first hitbox the probe overlaps on his arm's frame, whose reaction runs, or none; the arm's own entity asks
    /// (#2301).
    /// </summary>
    /// <exception cref="InvalidDataException">The frame has no hitboxes.</exception>
    internal static EnemyHitboxContact FindKraidArmHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe)
    {
        if (!KraidArmCollision.TryGetComponents(enemy.Frame, out var kraidComponents))
            throw EnemyCollision.UncoveredFrame(enemy);

        // Kraid's arm art is editable, but the frame
        // also carries a separate, ordered list of physical components.
        // Read those components from the immutable definition catalog.
        return FindComponentReaction(enemy, kraidComponents, probe);
    }

    /// <summary>
    /// The kind of hit <paramref name="shot"/> made when it damaged the mouth, for the row it lets later shots in from. An
    /// uncharged beam or an echo never hits the mouth, an explosion never damages it, Samus fires no Power Bomb, and only
    /// her first bomb's place holds a placed bomb.
    /// </summary>
    /// <exception cref="InvalidOperationException">No projectile of the shot's family and delivery damages the mouth.</exception>
    private static KraidMouthHitReach.DamagingHit DamagingMouthHit(KraidBehavior.KraidCollisionShot shot) =>
        shot.Type.Family switch
        {
            SamusProjectileFamily.Beam => !shot.IsPlacedBomb && shot.Type.IsChargedBeam && shot.Type.Echo is null
                ? KraidMouthHitReach.DamagingHit.ChargedBeam : throw CannotDamageMouth(shot),
            SamusProjectileFamily.Missile => !shot.IsPlacedBomb ? KraidMouthHitReach.DamagingHit.Missile : throw CannotDamageMouth(shot),
            SamusProjectileFamily.SuperMissile => !shot.IsPlacedBomb ? KraidMouthHitReach.DamagingHit.SuperMissile : throw CannotDamageMouth(shot),
            SamusProjectileFamily.Bomb => shot.IsPlacedBomb ? KraidMouthHitReach.DamagingHit.PlacedBomb : KraidMouthHitReach.DamagingHit.SpreadBomb,
            SamusProjectileFamily.PowerBomb => shot.IsPlacedBomb ? KraidMouthHitReach.DamagingHit.PlacedPowerBomb : throw CannotDamageMouth(shot),
            SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion => throw CannotDamageMouth(shot),
            _ => throw new InvalidOperationException($"Undefined SamusProjectileFamily {shot.Type.Family}."),
        };

    /// <summary>The failure of a projectile that cannot damage Kraid's mouth reaching the mouth's damage.</summary>
    private static InvalidOperationException CannotDamageMouth(KraidBehavior.KraidCollisionShot shot) =>
        new($"A {shot.Type} projectile ({(shot.IsPlacedBomb ? "placed" : "fired")}) cannot damage Kraid's mouth.");
}
