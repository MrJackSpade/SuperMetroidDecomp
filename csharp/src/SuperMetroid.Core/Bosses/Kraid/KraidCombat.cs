using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's hitboxes and touch, and what Samus's weapons do to his mouth and body.</summary>
[RoomLifetime]
internal sealed class KraidCombat
{
    /// <summary>Owns rocks, lint, and other fight projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues Kraid's encounter sound effects.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Spawns room sprite objects for Kraid's visual effects, and gives the place a dud flash took.</summary>
    private readonly RoomSpriteObjects _spriteObjects;
    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    private readonly KraidParts _parts;
    /// <summary>Checks mouth, body, and part collisions against Samus and shots.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Kraid's state and part lookups, part animations, phase timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidCombat(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, RoomSpriteObjects spriteObjects, KraidParts parts, EnemyCollision collision, KraidEncounterAccess access)
    {
        _projectiles = projectiles;
        _audio = audio;
        _spriteObjects = spriteObjects;
        _parts = parts;
        _collision = collision;
        _access = access;
    }

    /// <summary>Binds Kraid's body and mouth reactions to Samus bomb contacts.</summary>
    internal void AddKraidBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecial(c => c.Enemy.DefinitionId is EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidFoot,
            c =>
            {
                if (c.SelectedShotAi is not (EnemyReaction.Ignore or EnemyReaction.KraidArmShot) ||
                    c.Enemy.DefinitionId == EnemyDefinitionId.KraidFoot &&
                        c.SelectedShotAi != EnemyReaction.Ignore)
                {
                    throw new InvalidDataException(
                        $"Kraid part {c.Enemy.DefinitionId} selected " +
                        $"normal-bomb shot AI {c.SelectedShotAi?.ToString() ?? "(unnamed)"}.");
                }

                // The arm's shot reaction creates a room-graphics dust actor at the selected physical projectile
                // coordinates, then returns to the frame-hitbox walker that has already recorded the hit's direction.
                // Protected arm frames and every foot rectangle ignore the shot.
                if (c.SelectedShotAi == EnemyReaction.KraidArmShot)
                    SpawnKraidArmShotExplosion(c.Bomb.Type, c.Bomb.Position.Whole);
            });
    }

    /// <summary>Binds shot reactions for Kraid's mouth, body, arm, and nail surfaces.</summary>
    internal void AddKraidShotReactions(EnemySharedReactions table)
    {
        // Kraid's arm and foot are admitted by their species definitions' ignoring shot reaction, which still lets their hitboxes
        // decide; their authored hitboxes select the arm's private reaction.
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId is EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidFoot &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.Ignore)
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesFrameHitboxes ||
                    c.HitboxShotAi is not (EnemyReaction.Ignore or EnemyReaction.KraidArmShot) ||
                    c.Enemy.DefinitionId == EnemyDefinitionId.KraidFoot &&
                        c.HitboxShotAi != EnemyReaction.Ignore)
                {
                    throw new InvalidDataException(
                        $"Kraid part {c.Enemy.DefinitionId} selected " +
                        $"shot AI {c.HitboxShotAi?.ToString() ?? "(unnamed)"}.");
                }

                c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
                if (c.HitboxShotAi == EnemyReaction.KraidArmShot)
                    SpawnKraidArmShotExplosion(c.ProjectileType, c.Projectile.Position.Whole);
                return true;
            },
        });
    }

    /// <summary>Binds Samus contact handling for Kraid's body and parts.</summary>
    internal void AddKraidTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.KraidArm &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.KraidArmTouch,
            context =>
            {
                // The arm's push forces the bottom lint's firing phase, so it fires immediately after
                // the arm launches Samus up and right.
                // Writes only the whole pixels of the displacement, so preserve both
                // fractions exactly as they were before contact.
                _ = _access.RequireKraidState(context.Slot);
                context.Samus.Kinematics.ExtraDisplacement = context.Samus.Kinematics.ExtraDisplacement.WithWhole(new(4, -8));
                _access.Part(_parts.BottomLint).Phase = KraidPhase.LintFire;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
            });
    }

    /// <summary>
    /// Spawns the explosion of a shot that hits Kraid's arm, from the arm's hitbox
    /// reaction. Super Missiles select the big dust animation;
    /// beams, missiles, and normal bombs select the dud animation. The effect uses the
    /// shared room-graphics projectile allocator and queues the Samus-library explosion effect.
    /// </summary>
    private void SpawnKraidArmShotExplosion(
        SamusProjectileType projectileType,
        Position16<RoomPixels> position)
    {
        // Super Missiles, Power Bombs and beam explosions take the big explosion; the
        // tests one bit of their family's table row.
        MiscDustAnimation animationIndex = projectileType.Family is SamusProjectileFamily.SuperMissile or
                SamusProjectileFamily.PowerBomb or SamusProjectileFamily.BeamExplosion
            ? MiscDustAnimation.BigExplosion
            : MiscDustAnimation.DudShotTinyExplosion;
        _projectiles.SpawnRoomGraphicsDustExplosion(position, animationIndex);
        _audio.QueueEnemySound(SamusSound.DudShot, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Both passes return immediately once Kraid has begun sinking.
    /// </summary>
    private bool KraidSinkingSkipsProjectileCollision(RoomEnemySlot body) =>
        !KraidPhaseGroups.BeforeSinking.Contains(_access.Part(body).Phase);

    /// <summary>
    /// Resolves projectile hits on Kraid's mouth. Projectile
    /// removal is deferred to the projectile's frame behaviour.
    /// </summary>
    internal int ResolveKraidMouthProjectileHits(RoomEnemySlot body, KraidEnemyState state, IReadOnlyList<KraidBehavior.KraidPassProjectile> tested)
    {
        if (KraidSinkingSkipsProjectileCollision(body))
            return 0;

        int mouthHits = 0;
        KraidMouthHitbox? innerMouth = tested.Count == 0 ? null : KraidHeadAnimations.InnerMouthAt(state.HeadPlayback);
        if (innerMouth is { } inner)
        {
            KraidPassBounds bounds = PlaceKraidPassBounds(body, inner);
            foreach (KraidBehavior.KraidPassProjectile projectile in tested)
            {
                KraidBehavior.KraidCollisionShot shot = projectile.Read();
                SamusProjectileType type = shot.Type;
                if (!OverlapsKraidMouth(bounds, shot) ||
                    (type.Family == SamusProjectileFamily.Beam && !type.IsChargedBeam))
                    continue;

                if (type.Family == SamusProjectileFamily.Beam)
                    state.MouthFlags |= KraidMouthFlags.Hit;
                // A Super Missile's leftover changes the next shot's signed compare; rebuilding a clean rectangle here
                // doubles close-range damage.
                bounds.LeaveAfterMouthHit(KraidMouthHitBound.After(MouthFamily(shot.Type.Family), MouthHit(shot)));
                // Every beam takes Kraid's charged-beam response; the other families take their own.
                EnemyWeaponVulnerability vulnerability = type.Family switch
                {
                    SamusProjectileFamily.Beam => EnemyVulnerabilities.Of(body.Definition).ChargedBeam,
                    SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile or
                    SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb =>
                        EnemyCollision.ReadProjectileVulnerability(body, type),
                    SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion => EnemyWeaponVulnerability.NoDamage,
                    _ => throw new InvalidOperationException($"Undefined SamusProjectileFamily {type.Family}."),
                };
                int damage = vulnerability.DamageFrom(shot.Damage);
                if (damage != 0)
                {
                    body.Health = damage >= body.Health
                        ? 0
                        : body.Health - damage;
                    body.FlashTimer = body.HurtFlashFrames + 8;
                    body.AiStates = body.AiStates | EnemyAiStates.Hurt;
                    if (type.HasAnyBeam(SamusBeamFlags.Plasma))
                        body.InvincibilityTimer = 16;
                }
                else
                {
                    RoomSpriteObject dud = _spriteObjects.SpawnRoomSpriteObject(
                        shot.Position, RoomSpriteObjectKind.EnemyProjectileDud, graphics: default);
                    bounds.LeaveAfterDud(_spriteObjects.PlaceOf(dud), shot.Position);
                    _audio.QueueEnemySound(SamusSound.DudShot, priority: SoundRequestPriority.Low);
                }
                projectile.MarkCollision();
                mouthHits++;
            }
        }

        if (mouthHits != 0)
        {
            state.HurtFrame = 6;
            state.HurtFrameTimer = 2;
            if ((state.MouthFlags & KraidMouthFlags.ReopenArmed) != KraidMouthFlags.None)
                state.MouthFlags |= KraidMouthFlags.ReopenPending;
            if (body.Health < 1 &&
                KraidPhaseGroups.BeforeDeath.Contains(state.BodyState.Phase))
                BeginKraidDeath(state);
        }

        return mouthHits;
    }

    /// <summary>
    /// Resolves projectile hits on Kraid's body, the independent
    /// outer mouth/body pass.
    /// </summary>
    internal int ResolveKraidBodyProjectileHits(RoomEnemySlot body, KraidEnemyState state, IReadOnlyList<KraidBehavior.KraidPassProjectile> tested)
    {
        if (KraidSinkingSkipsProjectileCollision(body))
            return 0;

        state.MouthFlags &= ~KraidMouthFlags.Hit;
        int bodyHits = 0;
        if (tested.Count != 0)
        {
            KraidMouthHitbox outerMouth = KraidHeadAnimations.OuterMouthAt(state.HeadPlayback);
            KraidPassBounds bounds = PlaceKraidPassBounds(body, outerMouth);
            foreach (KraidBehavior.KraidPassProjectile projectile in tested)
            {
                KraidBehavior.KraidCollisionShot shot = projectile.Read();
                if (!OverlapsKraidBody(ref bounds, body, shot))
                    continue;
                SpawnKraidArmShotExplosion(shot.Type, shot.Position);
                bounds.LeaveAfterBodyHit(shot.Position);
                projectile.MarkCollision();
                if (shot.Type.IsChargedBeam)
                    state.MouthFlags |= KraidMouthFlags.Hit;
                bodyHits++;
            }
        }

        if (bodyHits != 0 && state.BodyState.Phase == KraidPhase.MainloopThinking)
        {
            state.BodyState.Phase = KraidPhase.InitializeEyeGlow;
            if ((state.MouthFlags & KraidMouthFlags.Hit) != KraidMouthFlags.None)
            {
                state.MouthFlags |= KraidMouthFlags.ReopenArmed;
                state.ArmMouthReopens();
            }
        }
        return bodyHits;
    }

    /// <summary>Places a mouth shape at Kraid's body position as the bounds a collision pass starts from.</summary>
    internal static KraidPassBounds PlaceKraidPassBounds(RoomEnemySlot body, KraidMouthHitbox shape) =>
        PlaceKraidPassBounds(body, KraidMouthHitboxes.ResolveCollision(shape));

    /// <summary>Places a mouth shape's left, top and bottom edges at Kraid's body position.</summary>
    internal static KraidPassBounds PlaceKraidPassBounds(RoomEnemySlot body, KraidMouthCollisionEdges shape) =>
        KraidPassBounds.Placed(
            body.Position.X.Whole + shape.Left,
            body.Position.Y.Whole + shape.Top,
            body.Position.Y.Whole + shape.Bottom);

    /// <summary>Whether a projectile overlaps the pass's bounds as the mouth pass tests them.</summary>
    internal static bool OverlapsKraidMouth(KraidPassBounds bounds, KraidBehavior.KraidCollisionShot shot) =>
        bounds.Overlaps(
            shot.Position.Y - shot.Radius.Y,
            shot.Position.Y + shot.Radius.Y,
            shot.Position.X + shot.Radius.X);

    /// <summary>
    /// Whether a projectile overlaps Kraid's body: inside the pass's bounds when it reaches above their bottom, and
    /// otherwise left of the body's contour at its height, which leaves the projectile's right edge as the bottom.
    /// </summary>
    internal static bool OverlapsKraidBody(ref KraidPassBounds bounds, RoomEnemySlot body, KraidBehavior.KraidCollisionShot shot)
    {
        if (bounds.IsAboveBottom(shot.Position.Y - shot.Radius.Y))
            return bounds.ReachesTopAndLeft(shot.Position.Y + shot.Radius.Y, shot.Position.X + shot.Radius.X);

        bounds.LeaveRightEdgeAsBottom(shot.Position.X + shot.Radius.X);
        int relativeY = CoordinateAxis.Difference(shot.Position.Y, body.Position.Y.Whole);
        return CoordinateAxis.AsSigned(body.Position.X.Whole + KraidBodyContour.LeftEdge(relativeY) - bounds.Bottom) < 0;
    }

    /// <summary>Switches the body from combat to its scripted death phases.</summary>
    private void BeginKraidDeath(KraidEnemyState state)
    {
        state.BodyState.Phase = KraidPhase.DeathInitialize;
        state.ClearMouthReactions();
        // The intangible property disables the ordinary touch list while the private death state owns body movement
        // and breakup effects. The fingernails are left out: they delete themselves once his health reads zero.
        foreach (RoomEnemySlot part in _parts.AllButNails)
            part.Contact = part.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Checks the body hitbox and applies contact damage to Samus.</summary>
    /// <remarks>These collision checks belong to actor AI rather than generic interactive hitboxes. The body's
    /// permanent intangible flag and lint's ignoring touch reaction do not suppress these private paths.</remarks>
    internal void ResolveKraidBodyContact(RoomEnemySlot body, KraidEnemyState state, SamusState samus)
    {
        if (!KraidPhaseGroups.BeforeDeath.Contains(state.BodyState.Phase))
            return;
        int relativeY = CoordinateAxis.Difference(samus.Position.Y.Whole, body.Position.Y.Whole);
        if (CoordinateAxis.AsSigned(body.Position.X.Whole + KraidBodyContour.LeftEdge(relativeY) -
            samus.Position.X.Whole - samus.Kinematics.Radius.X) >= 0)
            return;
        if (CoordinateAxis.Difference(samus.Position.X.Whole, KraidContactDefinitions.MinimumBodyEjectionX) >= 0)
            samus.Position -= new Offset16<RoomPixels>(KraidContactDefinitions.BodyEjection, 0);
        Position16<RoomPixels> ejected = samus.Position.Whole -
            new Offset16<RoomPixels>(0, KraidContactDefinitions.BodyEjection);
        samus.Position = samus.Position.WithWholeY((ejected - state.EjectionLimit).Y < 0
            ? state.EjectionLimit.Y : ejected.Y);
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(new(KraidContactDefinitions.ExtraX, KraidContactDefinitions.ExtraY));
        if (samus.InvincibilityTimer.IsZero)
            _collision.ResolveNormalEnemyTouch(body, samus);
    }

    /// <summary>The reaction of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindKraidFootHitboxReaction(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (!(enemy.DefinitionId == EnemyDefinitionId.KraidFoot &&
            KraidFootCollision.TryGetComponents(enemy.Frame, out var footComponents)))
            return null;

        // Kraid's foot art is replaceable, but each frame
        // still places two physical components against one shared hitbox list.
        foreach (KraidCollisionComponent component in footComponents)
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

    /// <summary>The reaction of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindKraidArmHitboxReaction(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (!(enemy.DefinitionId == EnemyDefinitionId.KraidArm &&
            KraidArmCollision.TryGetComponents(enemy.Frame, out var kraidComponents)))
            return null;

        // Kraid's arm art is editable, but the frame
        // also carries a separate, ordered physical component stream.
        // Read that stream from the immutable definition catalog.
        foreach (KraidCollisionComponent component in kraidComponents)
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

    /// <summary>The family of a projectile that hit the mouth, for the bound it leaves the mouth.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="family"/> is undefined.</exception>
    private static KraidMouthHitBound.Family MouthFamily(SamusProjectileFamily family) => family switch
    {
        SamusProjectileFamily.Beam => KraidMouthHitBound.Family.Beam,
        SamusProjectileFamily.Missile => KraidMouthHitBound.Family.Missile,
        SamusProjectileFamily.SuperMissile => KraidMouthHitBound.Family.SuperMissile,
        SamusProjectileFamily.PowerBomb => KraidMouthHitBound.Family.PowerBomb,
        SamusProjectileFamily.Bomb => KraidMouthHitBound.Family.Bomb,
        SamusProjectileFamily.BeamExplosion => KraidMouthHitBound.Family.BeamExplosion,
        SamusProjectileFamily.MissileExplosion => KraidMouthHitBound.Family.MissileExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "Undefined projectile family."),
    };

    /// <summary>How <paramref name="shot"/> was delivered, for the bound it leaves the mouth.</summary>
    private static KraidMouthHitBound.Hit MouthHit(KraidBehavior.KraidCollisionShot shot) =>
        shot.IsPlacedBomb ? KraidMouthHitBound.Hit.PlacedBomb
        : shot.Type.IsChargedBeam && shot.Type.Echo is not null ? KraidMouthHitBound.Hit.FiredChargedEcho
        : shot.Type.IsChargedBeam ? KraidMouthHitBound.Hit.FiredChargedBeam
        : shot.Type.Echo is not null ? KraidMouthHitBound.Hit.FiredEcho
        : KraidMouthHitBound.Hit.Fired;
}
