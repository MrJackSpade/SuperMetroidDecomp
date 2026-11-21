using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Samus-to-enemy interaction: Samus's touch, shots, bombs and Power Bombs tested against each interactive enemy, its
/// frame hitboxes, vulnerability and freezing, and the reactions they run.
/// </summary>
[RoomLifetime]
internal sealed class EnemyCollision
{
    /// <summary>Lists the enemies admitted to interaction this frame, whose reactions touch and projectile contact select.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Queues the enemy sounds produced by hits, freezes, and deaths.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Owns ordinary death animations and drops after a lethal collision.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Publishes hit-driven room shake requests.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Connects collision outcomes to enemy-owned projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Defines enemy iteration order for the collision passes.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>Resizes a power bomb's sweep after each kill it makes.</summary>
    private readonly PowerBombRadiusAfterKill _powerBombRadius;
    /// <summary>The room's effect actors, which the burst at an enemy a shot or bomb hurts joins.</summary>
    private readonly RoomEffectActors _effectActors;

    /// <summary>Binds the enemy and room owners whose state or reactions collisions can change.</summary>
    internal EnemyCollision(EnemyCollisionDependencies dependencies)
    {
        _powerBombRadius = dependencies.PowerBombRadius;
        _effectActors = dependencies.EffectActors;
        _lifecycle = dependencies.Lifecycle;
        _audio = dependencies.Audio;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _earthquake = dependencies.Earthquake;
        _projectiles = dependencies.Projectiles;
        _places = dependencies.Places;
    }

    /// <summary>
    /// The touch reaction a touch runs when it meets <paramref name="occupant"/> by its radius: the one its definition names,
    /// shared or the species' own (#2558).
    /// </summary>
    internal static EnemyReaction TouchReactionOf(RoomEnemyOccupant occupant) => occupant.Definition.Touch;

    /// <summary>
    /// The shot reaction a shot or bomb runs when it meets <paramref name="occupant"/> by its radius: the one its definition
    /// names, shared or the species' own (#2558).
    /// </summary>
    internal static EnemyReaction ShotReactionOf(RoomEnemyOccupant occupant) => occupant.Definition.Shot;

    /// <summary>Runs the common radius-based touch between Samus and the enemy the enemy pass is processing.</summary>
    /// <param name="samus">Live player state receiving the touch reaction.</param>
    /// <param name="pseudoScrew">Samus's held charge, which her Pseudo Screw contact spends.</param>
    /// <param name="controllerInput">Current held input used by touch responses.</param>
    /// <param name="level">Room terrain required by collision-aware touch responses.</param>
    /// <param name="occupant">
    /// The enemy the enemy pass is processing, read through its place; one that is not interactive touches nothing.
    /// </param>
    /// <param name="camera">The camera's position as the pass runs, which a reaction that starts a sound on screen reads.</param>
    public void ResolveOrdinarySamusContact(
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        GameplayAction controllerInput,
        RoomLevelData level,
        RoomEnemyOccupant occupant,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(samus);
        RoomEnemyPlace place = Ensure.NotNull(occupant).Place;
        SamusBodyAttack bodyAttack = samus.HorizontalSpeed.BodyAttack;
        if (bodyAttack == SamusBodyAttack.None && !samus.InvincibilityTimer.IsZero)
            return;
        if (!_lifecycle.InteractiveEnemies.Contains(occupant))
            return;
        // Collision entry rejects an enemy showing no frame before resetting Samus's invulnerability: an actor not yet
        // showing a frame must not erase the timer. The reset still precedes overlap testing, so a distant actor with a
        // frame can legitimately clear it.
        if (place.Frame is null)
            return;
        bool usesFrameHitboxes = UsesFrameHitboxesForSamus(place);
        // Frame-hitbox entry withholds the touch the definition excludes from its hitboxes before resetting
        // invulnerability. Ordinary radius entry resets first instead.
        if (usesFrameHitboxes && occupant.Definition.TouchHitboxScan == EnemyHitboxScan.Withheld)
            return;
        if (bodyAttack != SamusBodyAttack.None)
            samus.InvincibilityTimer = WrappingCountdown.Start(0);

        // A hidden enemy still touches Samus; hiding only stops it being drawn. Hibashi relies on that: its hitbox part
        // stays unseen while its animation moves a live hitbox. Both Ridleys touch Samus with their own body hitboxes and,
        // later in the update, the tail's tip; neither touches by its definition's radius. An actor without a touch
        // reaction is skipped.
        if (place.Entity is IEnemyContactHooks { OwnsSamusTouch: true } ||
            place.Presence.HasAny(EnemyPresence.Deleted) ||
            place.Entity is not IEnemyTouchReaction { ReactsToTouch: true } touch)
        {
            return;
        }

        EnemyReaction hitboxTouch;
        if (usesFrameHitboxes)
        {
            if (FindFrameHitboxContact(
                    place,
                    samus.Position.Whole,
                    samus.Kinematics.Radius,
                    contactKind: HitboxContactKind.Touch) is not EnemyHitboxContact.StruckHitbox struck)
                return;
            hitboxTouch = struck.Reaction;
        }
        else
        {
            if (!RadiusBoxesOverlap(
                    place.Position.Whole,
                    place.Radius,
                    samus.Position.Whole,
                    samus.Kinematics.Radius))
                return;
            hitboxTouch = TouchReactionOf(occupant);
        }

        // On the ordinary radius path a frozen enemy never reacts to Samus's touch, except a place awaiting its enemy's
        // respawn. The extended-hitbox path has no such gate.
        if (!usesFrameHitboxes && place.Freeze.IsRunning &&
            !place.AwaitsRespawn)
        {
            return;
        }

        touch.ReactToTouch(new EnemyTouchContext(
            occupant,
            samus,
            pseudoScrew,
            controllerInput,
            level,
            usesFrameHitboxes,
            hitboxTouch,
            camera));
    }

    /// <summary>
    /// Lets Samus's shots strike the enemy the enemy pass is processing, as they strike any enemy. One
    /// projectile may resolve per enemy per pass, matching the collision response's exit.
    /// </summary>
    /// <param name="projectiles">Samus's live beams and missiles, whose enemy collisions may consume, reflect, or transition them.</param>
    /// <param name="samusBombs">Shared bomb-projectile owner passed to projectile impact handling.</param>
    /// <param name="roomArea">The area of the room the enemies are in, which decides how long a shot's freeze lasts.</param>
    /// <param name="samus">Live player state for enemy-specific shot reactions.</param>
    /// <param name="occupant">
    /// The enemy the enemy pass is processing, read through its place; one that is not interactive takes no shot.
    /// </param>
    /// <param name="camera">The camera's position as the pass runs, which a reaction that starts a sound on screen reads.</param>
    public void ResolveOrdinaryProjectileHit(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem samusBombs,
        AreaId roomArea,
        SamusState samus,
        RoomEnemyOccupant occupant,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(samusBombs);
        RoomEnemyPlace enemy = Ensure.NotNull(occupant).Place;
        if (!_lifecycle.InteractiveEnemies.Contains(occupant))
            return;

        // Shots likewise strike hidden enemies, but pass an enemy that shows no frame or shows the empty frame
        // while it collides by its radius, and one that ignores Samus. An enemy without a shot reaction is
        // skipped.
        if (enemy.Entity is IEnemyShotAdmission admission && !admission.AdmitsShots() ||
            CommonEnemyEmptyFrames.ShotsPass(enemy) ||
            enemy.Invincibility.IsRunning ||
            enemy.Presence.HasAny(EnemyPresence.Deleted) ||
            enemy.Contact.HasAny(EnemyContact.IgnoresSamus) ||
            enemy.Entity is not IEnemyShotReaction { ShotReaction: var reaction })
        {
            return;
        }

        if (reaction.SkipsEnemy?.Invoke(enemy) == true)
            return;

        foreach (SamusProjectile projectile in projectiles.Shots)
        {
            // The enemy's vulnerability answers for the beam or missile that hit it, before the impact below turns the
            // shot into its explosion.
            if (projectile.Occupant is not { HasEnemyCollisionPayload: true } held)
                continue;
            SamusProjectileType projectileType = held.Type;
            int projectileDamage = held.Damage;
            SamusProjectileDirection projectileDirection = projectile.Direction;
            SamusProjectileFamily family = held.Type.Family;

            // Bomb, power-bomb, and pseudo-screw actors share the projectiles' places,
            // but no ordinary enemy reacts to them as it reacts to beams or missiles.
            // Yard and Metroid are the exceptions: their own responses treat
            // bomb-family impacts as physical kicks or escapes.
            if (!reaction.AdmitsBombFamilies &&
                family is SamusProjectileFamily.PowerBomb or
                    SamusProjectileFamily.Bomb or
                    SamusProjectileFamily.BeamExplosion)
            {
                continue;
            }

            bool usesFrameHitboxes = UsesFrameHitboxesForProjectiles(enemy);
            // The frame-hitbox shot test returns before testing any projectile while the enemy shows
            // the empty frame. Its point hitbox remains live for Samus and
            // bombs, whose frame-hitbox passes have no such check.
            if (usesFrameHitboxes &&
                CommonEnemyEmptyFrames.CollidesAtOrigin(enemy))
                continue;
            if (usesFrameHitboxes &&
                occupant.Definition.ShotHitboxScan == EnemyHitboxScan.Withheld)
            {
                // A definition that withholds shots from its hitboxes means no overlap, no
                // Super-Missile quake, and no direction mark even when a visible
                // authored rectangle would otherwise contain this projectile.
                continue;
            }

            EnemyReaction hitboxShot;
            if (usesFrameHitboxes)
            {
                if (FindFrameHitboxContact(
                        enemy,
                        projectile.Position.Whole,
                        projectile.Radius,
                        contactKind: HitboxContactKind.Shot) is not EnemyHitboxContact.StruckHitbox struck)
                    continue;
                hitboxShot = struck.Reaction;
            }
            else
            {
                if (!RadiusBoxesOverlap(
                        enemy.Position.Whole,
                        enemy.Radius,
                        projectile.Position.Whole,
                        projectile.Radius))
                    continue;
                hitboxShot = ShotReactionOf(occupant);
            }

            bool selectedIgnore = hitboxShot == EnemyReaction.Ignore;
            var shot = new EnemyShotContext(
                occupant,
                held,
                projectiles,
                samus,
                projectileType,
                projectileDamage,
                projectileDirection,
                family,
                usesFrameHitboxes,
                hitboxShot,
                camera)
            {
                StruckOccupant = enemy.OccupantStates,
            };

            if (RunShotStage(reaction.BeforeSuperMissileQuake, shot))
            {
                return;
            }

            if (usesFrameHitboxes && family == SamusProjectileFamily.SuperMissile)
            {
                // A Super Missile meeting a frame hitbox shakes the room before the hitbox's reaction runs, even when
                // that reaction is the ignoring shell region.
                _earthquake.Start(SuperMissileDefinitions.EnemyHitEarthquake, SuperMissileDefinitions.EarthquakeDuration);
            }

            if (RunShotStage(reaction.AfterSuperMissileQuake, shot))
            {
                return;
            }

            if (selectedIgnore)
            {
                projectiles.EnemyImpacts.TouchEnemyWithoutImpact(held, shot.MarksCollision);
                return;
            }

            if (RunShotStage(reaction.BeforeImpact, shot))
            {
                return;
            }

            if (reaction.EndsWithoutImpact || family is SamusProjectileFamily.Beam or
                SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
            {
                // The collision marks the projectile's direction rather than striking terrain; the projectile acts on
                // the mark on its next update, and the damage below shows its own hit burst at the enemy. A Super
                // Missile keeps moving as itself so that it also clears its collision links, which would otherwise stay
                // live and damage the enemy again once its flash timer runs out. Neither the beam's nor the missile's
                // damage refuses the mark, so later overlapping enemies this pass still take the live payload.
                projectiles.EnemyImpacts.TouchEnemyWithoutImpact(
                    held,
                    enemy.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                        !held.Type.BeamCombination.HasPlasma);
            }
            else if (!projectiles.EnemyImpacts.TryStartEnemyImpact(samusBombs, held,
                enemy.Contact.HasAny(EnemyContact.BlocksPlasmaBeam)))
                continue;

            reaction.AfterImpact?.Invoke(shot);

            var hitShot = new EnemyShotHitContext(shot, enemy.Health);
            if (RunShotStage(reaction.BeforeVulnerability, hitShot))
            {
                return;
            }

            NormalShotHit hit = ApplyNormalShotDamage(occupant, held, projectileType, projectileDamage, roomArea).Hit;
            if (hit == NormalShotHit.Froze)
            {
                reaction.AfterCommon?.Invoke(hitShot with { Froze = true });
                return;
            }

            if (hit is NormalShotHit.HurtFrozen or NormalShotHit.HurtUnfrozen)
            {
                if (hit == NormalShotHit.HurtUnfrozen && reaction.ShowsHitBurst(hitShot))
                    ShowHitBurst(enemy);
                if (enemy.Health == 0 && reaction.Lethal is not EnemyShotLethal.SpeciesOwnedDeath)
                {
                    if (reaction.Lethal is EnemyShotLethal.AfterStage before)
                        before.Stage(hitShot);
                    if (!(reaction.Lethal is EnemyShotLethal.Replaced replaced && replaced.Death(hitShot)))
                    {
                        _deathsAndDrops.StartGenericEnemyDeath(
                            occupant,
                            EnemyDeathsAndDrops.ShotKillExplosion(occupant, projectileType, rule: ShotDeathAnimationRule.ByProjectile),
                            samus);
                    }
                }
            }

            reaction.AfterCommon?.Invoke(hitShot);
            return;
        }
    }

    /// <summary>
    /// Runs common normal-shot damage (#2255), the shared step of every shot reaction that deals ordinary damage, the
    /// ordinary pass's and the bosses' own: the enemy's vulnerability to <paramref name="projectileType"/> freezes it
    /// outright, or makes the shot a dud when the damage is zero, or hurts it. A hurt enemy flashes, is hurt, cries unless
    /// frozen, takes Plasma's invincibility, and loses the damage, a lethal Ice-bearing beam freezing it instead where its
    /// vulnerability allows. The death and the hit burst that follow are the reaction's.
    /// </summary>
    /// <param name="occupant">The enemy the shot reached.</param>
    /// <param name="projectile">The shot, which a dud spends.</param>
    /// <param name="projectileType">The shot's type as it reached the enemy, before any impact rewrote it.</param>
    /// <param name="projectileDamage">The shot's damage, zero or more.</param>
    /// <param name="roomArea">The area of the room the enemy is in, which decides how long a freeze lasts.</param>
    /// <returns>Whether the shot froze the enemy outright, was a dud, or hurt it while it was frozen or not, and the health it lost.</returns>
    internal NormalShotOutcome ApplyNormalShotDamage(
        RoomEnemyOccupant occupant,
        SamusProjectileOccupant projectile,
        SamusProjectileType projectileType,
        int projectileDamage,
        AreaId roomArea)
    {
        RoomEnemyPlace enemy = Ensure.NotNull(occupant).Place;
        Ensure.NotNull(projectile);
        int healthBefore = enemy.Health;
        NormalShotVulnerability shotVulnerability = ReadNormalShotVulnerability(occupant, projectileType);
        if (shotVulnerability.FreezeImmediately)
        {
            FreezeEnemyFromNormalShot(enemy, roomArea);
            return new(NormalShotHit.Froze, healthBefore - enemy.Health);
        }

        int damage = shotVulnerability.DamageFrom(projectileDamage);
        if (damage == 0)
        {
            // A zero-damage hit is a dud even for Plasma: the immune enemy removes the shot, which does not pierce it.
            _projectiles.CreateEnemyProjectileDudShot(projectile);
            return new(NormalShotHit.Dud, healthBefore - enemy.Health);
        }

        enemy.HurtFlash.Restart(enemy.HurtFlashFrames + EnemyCollisionDefinitions.WeaponHurtFlashExtension);
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
        // The common shot reaction queues the definition's cry before subtracting health,
        // including lethal hits. Frozen targets suppress this cry; death
        // explosion animations retain their separate, later sound owner. The
        // same test, made here before a lethal Ice hit freezes the enemy, decides
        // the hit burst, which shows whether or not the enemy has a cry.
        bool hurtUnfrozen = enemy.Freeze.IsOver;
        if (hurtUnfrozen && occupant.Definition.HurtSoundEffect is SoundEffectId cry)
            _audio.QueueEnemySound(cry, SoundRequestPriority.Low);
        // Every plasma-family hit gives its invincibility frames (EnemyCollisionDefinitions.PlasmaShotInvincibilityUpdates).
        // Only a beam combining Plasma gives this; other projectile
        // families never count as Plasma.
        if (projectileType.BeamCombination.HasPlasma)
            enemy.Invincibility.Restart(EnemyCollisionDefinitions.PlasmaShotInvincibilityUpdates);

        bool lethal = damage >= enemy.Health;
        bool freezeInsteadOfKilling = lethal &&
            projectileType.BeamCombination.HasIce &&
            !shotVulnerability.PreventsLethalFreeze &&
            enemy.Freeze.IsOver;
        if (freezeInsteadOfKilling)
        {
            // A lethal Ice-bearing beam leaves health unchanged and freezes the
            // actor. This is not the same path as an authored freezing entry:
            // hurt flash and hurt state were already set before the lethal test.
            FreezeEnemyFromNormalShot(enemy, roomArea);
        }
        else
        {
            enemy.Health = lethal
                ? 0
                : enemy.Health - damage;
        }
        return new(hurtUnfrozen ? NormalShotHit.HurtUnfrozen : NormalShotHit.HurtFrozen, healthBefore - enemy.Health);
    }

    /// <summary>Runs an optional enemy-specific shot stage and reports whether it consumed the hit.</summary>
    private static bool RunShotStage(EnemyShotStage? stage, in EnemyShotContext context) =>
        stage is not null && stage(in context);

    /// <summary>
    /// Runs the ordinary-radius half of the enemies' bomb collision.
    /// </summary>
    /// <remarks>
    /// Normal bombs are tested apart from Samus's shots: the bomb pass waits for the shared bomb countdown to reach
    /// zero, marks the overlapping bomb as having hit, and runs the enemy's shot reaction with that bomb selected.
    ///
    /// The common reaction and the Metroids' own reaction both meet bombs here; every other species' own reaction meets
    /// them only with its own bomb handling.
    /// </remarks>
    /// <param name="bombs">Samus's bombs, whose exploding bombs may hit.</param>
    /// <param name="ordinaryProjectiles">Samus's beams and missiles, which reactions read.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="occupant">The enemy the enemy pass is processing, read through its place.</param>
    /// <param name="camera">The camera's position as the pass runs, which a reaction that starts a sound on screen reads.</param>
    public void ResolveOrdinaryBombHit(
        SamusBombProjectileSystem bombs,
        SamusProjectileSystem ordinaryProjectiles,
        SamusState samus,
        RoomEnemyOccupant occupant,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(bombs);
        Ensure.NotNull(ordinaryProjectiles);
        RoomEnemyPlace enemy = Ensure.NotNull(occupant).Place;

        EnemyReaction radiusShot = ShotReactionOf(occupant);
        bool usesCommonShot = radiusShot == EnemyReaction.OrdinaryShot;
        bool usesIgnoringShot = radiusShot == EnemyReaction.Ignore;
        bool usesPrivateShot = HasPrivateNormalBombShot(enemy, radiusShot);
        bool usesFrameHitboxes = enemy.CollidesByFrameHitboxes;
        bool definitionWithholdsFrameHitboxShots = usesFrameHitboxes &&
            occupant.Definition.ShotHitboxScan == EnemyHitboxScan.Withheld;
        bool admittedByOccupant = enemy.Entity is IEnemyNormalBombAdmission admission &&
            admission.AdmitsNormalBombs(usesFrameHitboxes);
        if ((!usesCommonShot && !usesIgnoringShot &&
                !usesPrivateShot && !admittedByOccupant) ||
            enemy.Frame is null ||
            enemy.Invincibility.IsRunning ||
            enemy.Presence.HasAny(EnemyPresence.Deleted) ||
            definitionWithholdsFrameHitboxShots ||
            usesFrameHitboxes &&
                enemy.Contact.HasAny(EnemyContact.IgnoresSamus))
        {
            return;
        }

        foreach (SamusBomb bomb in bombs.Bombs)
        {
            // A place no bomb holds has none to explode.
            if (bomb.Occupant is not { } placed)
                continue;
            SamusProjectileFamily family = placed.Type.Family;

            // The radius bomb pass admits an exploding normal bomb. The radius
            // pass also admits a spread bomb. The frame-hitbox pass is narrower,
            // and passes over a bomb on the room's leftmost column (LeftEdgeBombMissesFrameHitboxes).
            if (!bomb.HasBurnedOut ||
                (usesFrameHitboxes
                    ? family != SamusProjectileFamily.Bomb ||
                      LeftEdgeBombMissesFrameHitboxes.Misses(bomb.Position.X.Whole)
                    : family != SamusProjectileFamily.Bomb &&
                      !bomb.IsBombSpread))
            {
                continue;
            }

            EnemyReaction selectedShot;
            if (usesFrameHitboxes)
            {
                if (FindFrameHitboxContact(
                        enemy,
                        bomb.Position.Whole,
                        bomb.Radius,
                        contactKind: HitboxContactKind.Shot) is not EnemyHitboxContact.StruckHitbox struck)
                    continue;
                selectedShot = struck.Reaction;
            }
            else
            {
                if (!RadiusBoxesOverlap(
                        enemy.Position.Whole,
                        enemy.Radius,
                        bomb.Position.Whole,
                        bomb.Radius))
                    continue;
                selectedShot = radiusShot;
            }

            // A frame-hitbox enemy runs the reaction its selected hitbox names, which may differ from its definition's.
            bool selectedIgnore = selectedShot == EnemyReaction.Ignore;
            bool selectedPrivateReaction = HasPrivateNormalBombShot(enemy, selectedShot);
            if (selectedShot != EnemyReaction.OrdinaryShot &&
                !selectedIgnore &&
                !selectedPrivateReaction)
                continue;

            // The bomb collision marks the bomb before its reaction runs. Unlike an
            // ordinary beam impact, the bomb is already running its explosion animation, which the
            // common shot reaction leaves in place of an impact animation.
            placed.Spend();
            var bombContext = new EnemyBombContext(
                occupant,
                placed,
                ordinaryProjectiles,
                samus,
                selectedShot,
                usesFrameHitboxes,
                camera)
            {
                StruckOccupant = enemy.OccupantStates,
            };
            // The enemy's own reaction comes first, even for an ignoring selection; an ignoring selection ends the
            // contact there, and otherwise the enemy may replace common damage or run its stages around it.
            var bombReactions = enemy.Entity as IEnemyBombReaction;
            if (bombReactions?.TryReactToBomb(bombContext) != true && !selectedIgnore &&
                bombReactions?.TryReplaceCommonBombDamage(bombContext) != true)
            {
                // A private reaction that rejects the bomb returns before common damage; the bomb pass has
                // already marked the bomb, so rejection belongs here rather than in collision admission.
                // An enemy with no bomb reaction of its own takes the plain common damage.
                EnemyBombReaction reaction = bombReactions is { } own
                    ? own.CommonBombDamageReaction(bombContext)
                    : EnemyBombReaction.PlainCommon;
                if (reaction.Rejects?.Invoke(bombContext) != true)
                {
                    var hitBomb = new EnemyBombHitContext(bombContext, enemy.Health);
                    reaction.BeforeCommon?.Invoke(hitBomb);
                    NormalBombHit hit = HurtByNormalBomb(occupant, placed);
                    if (hit == NormalBombHit.HurtUnfrozen && reaction.ShowsHitBurst(hitBomb))
                    {
                        ShowHitBurst(enemy);
                    }
                    if (hit != NormalBombHit.Dud && reaction.RunsGenericDeath)
                        StartNormalBombDeathIfKilled(occupant, samus);
                    reaction.AfterCommon?.Invoke(hitBomb);
                }
            }

            return;
        }
    }

    /// <summary>
    /// Reports the private shot reactions that have a normal-bomb path. The occupant decides, because a frame's
    /// hitbox may deliberately override its definition with a different reaction.
    /// </summary>
    private static bool HasPrivateNormalBombShot(
        RoomEnemyPlace enemy,
        EnemyReaction reaction) =>
        enemy.Entity is IEnemyPrivateShotReaction owner &&
        owner.OwnsPrivateShotReaction(reaction);

    /// <summary>
    /// Runs the common damage a normal bomb does through the shot reaction, without the hit burst or the death that
    /// follows: the part of the reaction a species' own bomb handling runs before deciding the enemy's death itself.
    /// </summary>
    internal void ApplyNormalBombDamage(RoomEnemyOccupant enemy, SamusBombOccupant placed) => _ = HurtByNormalBomb(enemy, placed);

    /// <summary>
    /// Damages <paramref name="enemy"/> by the bomb <paramref name="placed"/> through its bomb vulnerability: a zero
    /// multiplier leaves the dud flash and sound instead; otherwise the enemy flashes, is hurt, cries unless frozen and
    /// loses the damage.
    /// </summary>
    /// <returns>Whether the bomb was a dud, hurt the enemy while it was frozen, or hurt it while it was not.</returns>
    private NormalBombHit HurtByNormalBomb(RoomEnemyOccupant occupant, SamusBombOccupant placed)
    {
        RoomEnemyPlace enemy = occupant.Place;
        SamusBomb bomb = placed.Bomb;
        EnemyWeaponVulnerability vulnerability = EnemyVulnerabilities.Of(occupant.Definition).Ordnance.Bomb;
        int damage = vulnerability.DamageFrom(placed.Damage);

        // A zero multiplier still consumes the collision and leaves the hit direction on the bomb: the bomb shows the
        // dud flash where it lies and the dud sound plays, and the enemy is untouched.
        if (damage == 0)
        {
            _projectiles.CreateBombDudShot(bomb);
            return NormalBombHit.Dud;
        }

        enemy.HurtFlash.Restart(enemy.HurtFlashFrames + EnemyCollisionDefinitions.WeaponHurtFlashExtension);
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
        // The common shot reaction a bomb runs queues the definition's cry before subtracting health, lethal hits
        // included, as it does for a beam or a missile; a frozen enemy makes no cry.
        bool hurtUnfrozen = enemy.Freeze.IsOver;
        if (hurtUnfrozen && occupant.Definition.HurtSoundEffect is SoundEffectId cry)
            _audio.QueueEnemySound(cry, SoundRequestPriority.Low);
        enemy.Health = damage >= enemy.Health
            ? 0
            : enemy.Health - damage;
        return hurtUnfrozen ? NormalBombHit.HurtUnfrozen : NormalBombHit.HurtFrozen;
    }

    /// <summary>Starts the death of an enemy a bomb's damage killed.</summary>
    private void StartNormalBombDeathIfKilled(RoomEnemyOccupant killed, SamusState samus)
    {
        RoomEnemyPlace enemy = killed.Place;
        if (enemy.Health != 0)
            return;
        _deathsAndDrops.StartGenericEnemyDeath(
            killed,
            EnemyDeathsAndDrops.ShotKillExplosion(killed, SamusBombProjectileSystem.NormalBombType),
            samus);
    }

    /// <summary>
    /// Shows the enemy-hit burst at <paramref name="enemy"/>'s position: the ordinary shot reaction's sign that a shot or
    /// bomb hurt an enemy that was not frozen, shown after the damage and before the death. It is a room effect actor
    /// drawn with the room's graphics, so it takes one of the room's effect-actor places as it spawns.
    /// </summary>
    private void ShowHitBurst(RoomEnemyPlace enemy) =>
        _ = _effectActors.SpawnRoomEffectActor(enemy.Position.Whole, RoomEffectActorKind.HitBurst, graphics: SpriteGraphics.SharedStandard);

    /// <summary>What a normal bomb's common damage did to the enemy it reached.</summary>
    private enum NormalBombHit
    {
        /// <summary>The enemy's bomb vulnerability is zero: the bomb shows the dud flash and the enemy is untouched.</summary>
        Dud,
        /// <summary>The bomb hurt an enemy that was frozen, which makes no cry and shows no hit burst.</summary>
        HurtFrozen,
        /// <summary>The bomb hurt an enemy that was not frozen.</summary>
        HurtUnfrozen,
    }

    /// <summary>
    /// Runs the power-bomb interaction for one expansion
    /// sample. The power bomb's live radius comes in whole pixels; the game
    /// uses it as the horizontal radius and derives a three-quarter-height vertical ellipse.
    /// </summary>
    /// <param name="explosionCenter">The explosion's centre.</param>
    /// <param name="explosionRadius">The explosion's horizontal radius in whole pixels; zero reaches nothing.</param>
    /// <param name="level">The room's terrain this update, which a species reaction that runs its main behaviour reads.</param>
    /// <param name="camera">The camera's position as the pass runs, which a species reaction that runs its main behaviour reads.</param>
    /// <param name="samus">Samus.</param>
    public void ResolveOrdinaryPowerBombHits(
        Position16<RoomPixels> explosionCenter,
        int explosionRadius,
        RoomLevelData level,
        Position16<RoomPixels> camera,
        SamusState samus)
    {
        // The radius is a real size: one that has not grown from zero reaches nothing, as the game's zero test skips it.
        if (explosionRadius == 0)
            return;

        int horizontalRadius = explosionRadius;
        // The blast reaches three quarters as far vertically, rounded to the nearest pixel, a half pixel rounding down.
        int verticalRadius = (3 * horizontalRadius + 1) / 4;
        _powerBombRadius.Forget();

        // The blast reaches every enemy in its own order, whether or not it is updating; a power bomb reaches
        // off-screen enemies and keeps them updating off screen, below.
        foreach (RoomEnemyPlace enemy in EnemyPlaceOrder.InBlastOrder(_places.InUpdateOrder))
        {
            // A death inside the previous reaction resizes the horizontal reach for the rest of
            // the sweep; the vertical reach is unchanged.
            horizontalRadius = _powerBombRadius.HorizontalRadius(horizontalRadius);
            // A place with no enemy, a place awaiting its enemy's respawn among them, is passed.
            if (enemy.Occupant is not { } occupant ||
                enemy.Invincibility.IsRunning ||
                enemy.Presence.HasAny(EnemyPresence.Deleted))
            {
                continue;
            }

            // The radius actor takes the Power Bomb response, not the ordinary bomb's: every
            // enemy whose bomb and Power Bomb vulnerabilities differ depends on it.
            EnemyWeaponVulnerability vulnerability = EnemyVulnerabilities.Of(occupant.Definition).Ordnance.PowerBomb;
            if (vulnerability.Multiplier.IsNone)
                continue;

            // The blast lies in the room, but an enemy it reaches need not: a fly that has climbed through the ceiling or a
            // Bull braking past Samus can lie above or left of the room's origin, and the blast reaches one the short way,
            // as DistanceAcrossRoomEdge.Between gives it.
            int xDistance = DistanceAcrossRoomEdge.Between(explosionCenter.X, enemy.Position.X.Whole);
            int yDistance = DistanceAcrossRoomEdge.Between(explosionCenter.Y, enemy.Position.Y.Whole);
            if (xDistance >= horizontalRadius || yDistance >= verticalRadius)
                continue;

            // A definition names the ignoring reaction or the common one, which the entity answers; an entity without a
            // Power Bomb reaction takes the common damage.
            EnemyPowerBombReaction reaction = enemy.Entity is IEnemyPowerBombReaction owner
                ? owner.PowerBombReaction
                : EnemyCommonParticipation.CommonPowerBomb;
            if (reaction.SkipsEnemy?.Invoke(enemy) == true)
                continue;
            var context = new EnemyPowerBombContext(occupant, samus, level, camera);

            // A reaction of the enemy's own replaces the common Power Bomb damage, and the
            // enemy still goes on updating off screen once the blast's ellipse reaches it.
            if (reaction.ReplacesCommon is { } privateReaction)
            {
                privateReaction(context);
                enemy.Processing = enemy.Processing.With(EnemyProcessing.OffScreen);
                continue;
            }

            // The common reaction damages the enemy by a Power Bomb's damage, and then it keeps updating off screen.
            int damage = vulnerability.DamageFrom(SamusProjectileDamage.Read(SamusProjectileKind.PowerBomb));
            enemy.Invincibility.Restart(EnemyCollisionDefinitions.PowerBombInvincibilityUpdates);
            enemy.HurtFlash.Restart(enemy.HurtFlashFrames + EnemyCollisionDefinitions.WeaponHurtFlashExtension);
            enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
            enemy.Health = damage >= enemy.Health
                ? 0
                : enemy.Health - damage;
            if (enemy.Health == 0 && reaction.Lethal is not EnemyPowerBombLethal.SpeciesOwnedDeath)
            {
                if (reaction.Lethal is EnemyPowerBombLethal.AfterStage before)
                    before.Stage(context);
                if (reaction.Lethal is EnemyPowerBombLethal.Replaced replaced)
                    replaced.Death(context);
                else
                    _deathsAndDrops.StartGenericEnemyDeath(occupant, EnemyDeathExplosion.Small, samus);
            }

            reaction.AfterCommon?.Invoke(context);

            enemy.Processing = enemy.Processing.With(EnemyProcessing.OffScreen);
        }

        _powerBombRadius.Forget();
    }

    /// <summary>
    /// Selects the exact multiplier the damage calculation uses for an ordinary shot: the beam combination's response, with a
    /// charged shot taking the charged-beam multiplier unless the beam freezes; missile, Super Missile, bomb and Power
    /// Bomb families take their own responses.
    /// </summary>
    private static NormalShotVulnerability ReadNormalShotVulnerability(
        RoomEnemyOccupant enemy,
        SamusProjectileType projectileType)
    {
        EnemyVulnerabilities vulnerabilities = EnemyVulnerabilities.Of(Ensure.NotNull(enemy).Definition);
        SamusProjectileFamily family = projectileType.Family;
        if (family != SamusProjectileFamily.Beam)
        {
            return new NormalShotVulnerability(
                Multiplier: FamilyVulnerability(vulnerabilities, family).Multiplier,
                FreezeImmediately: false,
                PreventsLethalFreeze: false);
        }

        EnemyWeaponVulnerability beam = vulnerabilities.ForBeam(projectileType.BeamCombination);
        if (beam.FreezesWithoutDamage)
        {
            return new NormalShotVulnerability(
                Multiplier: WeaponDamageFactor.None,
                FreezeImmediately: true,
                PreventsLethalFreeze: beam.PreventsLethalFreeze);
        }

        // A charged shot takes the charged-beam multiplier instead; zero makes the shot a dud.
        WeaponDamageFactor multiplier = projectileType.IsChargedBeam ? vulnerabilities.ChargedBeam.Multiplier : beam.Multiplier;
        return new NormalShotVulnerability(
            multiplier,
            FreezeImmediately: false,
            PreventsLethalFreeze: beam.PreventsLethalFreeze);
    }

    /// <summary>
    /// Freezes an enemy a shot froze: for <see cref="EnemyCollisionDefinitions.NorfairFreezeUpdates"/> in a Norfair room
    /// and <see cref="EnemyCollisionDefinitions.FreezeUpdates"/> in a room of any other area. The protection after a freeze
    /// is shared by direct authored freezing and the lethal-Ice substitution.
    /// </summary>
    /// <param name="enemy">The enemy frozen.</param>
    /// <param name="roomArea">The area of the room the enemy is in.</param>
    private void FreezeEnemyFromNormalShot(RoomEnemyPlace enemy, AreaId roomArea)
    {
        // The direct-freeze vulnerability refreshes the timer even on an already frozen
        // actor, but only a newly frozen actor queues audio. Lethal-Ice substitution
        // reaches this step only with an initially zero clock.
        if (enemy.Freeze.IsOver)
            _audio.QueueEnemySound(RoomSound.EnemyFrozen, priority: SoundRequestPriority.Low);
        enemy.Freeze.Restart(EnemyCollisionDefinitions.FreezeUpdatesByArea.TryGetValue(roomArea, out int areaFreeze)
            ? areaFreeze
            : EnemyCollisionDefinitions.FreezeUpdates);
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Frozen;
        enemy.Invincibility.Restart(EnemyCollisionDefinitions.FreezeInvincibilityUpdates);
    }

    /// <summary>Selects the enemy's response to the projectile's beam combination or weapon family.</summary>
    internal static EnemyWeaponVulnerability ReadProjectileVulnerability(
        RoomEnemyOccupant enemy,
        SamusProjectileType projectileType)
    {
        EnemyVulnerabilities vulnerabilities = EnemyVulnerabilities.Of(Ensure.NotNull(enemy).Definition);
        SamusProjectileFamily family = projectileType.Family;
        return family == SamusProjectileFamily.Beam
            ? vulnerabilities.ForBeam(projectileType.BeamCombination)
            : FamilyVulnerability(vulnerabilities, family);
    }

    /// <summary>The response a missile, Super Missile, bomb or Power Bomb family takes.</summary>
    /// <exception cref="InvalidDataException">The family has no response of its own.</exception>
    private static EnemyWeaponVulnerability FamilyVulnerability(EnemyVulnerabilities vulnerabilities, SamusProjectileFamily family) => family switch
    {
        SamusProjectileFamily.Missile => vulnerabilities.Ordnance.Missile,
        SamusProjectileFamily.SuperMissile => vulnerabilities.Ordnance.SuperMissile,
        SamusProjectileFamily.Bomb => vulnerabilities.Ordnance.Bomb,
        SamusProjectileFamily.PowerBomb => vulnerabilities.Ordnance.PowerBomb,
        SamusProjectileFamily.Beam or SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion or _ =>
            throw new InvalidDataException($"Projectile family {family} has no response of its own."),
    };

    /// <summary>
    /// Damage multiplier and freezing behavior selected from an ordinary shot's vulnerability. A lethal Ice-bearing
    /// beam freezes the enemy instead of killing it unless <see cref="PreventsLethalFreeze"/>, which the beam entry
    /// decides; non-beam families never prevent it.
    /// </summary>
    [IndependentFlag("independent facts of an ordinary shot's vulnerability: immediate freeze, and lethal-freeze prevention")]
    internal readonly record struct NormalShotVulnerability(
        WeaponDamageFactor Multiplier,
        bool FreezeImmediately,
        bool PreventsLethalFreeze)
    {
        /// <summary>
        /// The damage a shot of <paramref name="weaponDamage"/>, zero or more, deals by <see cref="Multiplier"/>, the rule
        /// every weapon response takes (<see cref="WeaponDamageFactor.DamageFrom"/>).
        /// </summary>
        internal int DamageFrom(int weaponDamage) => Multiplier.DamageFrom(weaponDamage);
    }

    /// <summary>
    /// Tests the hitboxes of the frame an enemy shows and returns the first overlapping hitbox, whose reaction runs, or
    /// none. One frame may hold several hitboxes, each signed bounds about the enemy's origin with its own touch and shot
    /// reactions.
    /// </summary>
    /// <exception cref="InvalidDataException">No geometry covers the frame the enemy shows.</exception>
    internal static EnemyHitboxContact FindFrameHitboxContact(
        RoomEnemyPlace enemy,
        Position16<RoomPixels> targetPosition,
        Extent16<RoomPixels> targetRadius,
        HitboxContactKind contactKind)
    {
        // Frame-hitbox collision tests every frame an actor shows, and only an actor that has
        // shown none is skipped. The empty frame still has a point hitbox for an enemy that
        // collides by its frames' hitboxes, so it enters the same test as every other frame.
        if (enemy.Frame is null)
            return EnemyHitboxContact.Missed;

        EnemyHitboxProbe target = EnemyHitboxProbe.Around(targetPosition, targetRadius, contactKind);

        if (CommonEnemyEmptyFrames.CollidesAtOrigin(enemy))
        {
            // The empty frame has a single zero-radius point at the
            // enemy origin. Run the same asymmetric overlap test before
            // selecting its common touch/shot reaction.
            if (!target.Overlaps(enemy.Position.X.Whole, enemy.Position.Y.Whole, enemy.Position.X.Whole, enemy.Position.Y.Whole))
                return EnemyHitboxContact.Missed;
            return EnemyHitboxContact.Struck(contactKind == HitboxContactKind.Shot ? EnemyReaction.OrdinaryShot : EnemyReaction.OrdinaryTouch);
        }

        // A species whose hitboxes the game defines answers for its own frames.
        if (enemy.Entity is IEnemyFrameHitboxes owner)
            return owner.FindHitboxReaction(target);

        throw UncoveredFrame(enemy);
    }

    /// <summary>The failure of a frame-hitbox test on a frame no collision geometry covers.</summary>
    /// <param name="enemy">The enemy whose shown frame has no geometry.</param>
    internal static InvalidDataException UncoveredFrame(RoomEnemyPlace enemy) =>
        new($"Enemy {Ensure.NotNull(enemy).Occupant?.DefinitionId} extended collision frame {enemy.Frame} has no collision geometry.");

    /// <summary>
    /// Identifies actors whose contact with Samus is tested against their frames' hitboxes. This differs deliberately
    /// from the shot set: Phantoon's frames' hitboxes meet Samus, while his own projectile resolver owns how shots meet
    /// him.
    /// </summary>
    internal static bool UsesFrameHitboxesForSamus(RoomEnemyPlace enemy) =>
        enemy.CollidesByFrameHitboxes &&
        (Capabilities(enemy) & EnemyCapabilities.SamusFrameHitboxes) != EnemyCapabilities.None;

    /// <summary>
    /// Identifies actors whose contact with Samus's shots is tested against their frames' hitboxes.
    /// <see cref="RoomEnemyPlace.CollidesByFrameHitboxes"/> alone is not sufficient: Mother Brain and several
    /// encounter-owned actors collide by frame hitboxes elsewhere while their shots meet a definition's radius or the
    /// encounter's own shapes.
    /// </summary>
    internal static bool UsesFrameHitboxesForProjectiles(RoomEnemyPlace enemy) =>
        enemy.CollidesByFrameHitboxes &&
        (Capabilities(enemy) & EnemyCapabilities.ProjectileFrameHitboxes) != EnemyCapabilities.None;

    /// <summary>The bound occupant's collision capabilities; an empty place has none.</summary>
    private static EnemyCapabilities Capabilities(RoomEnemyPlace enemy) =>
        enemy.Entity?.Capabilities ?? EnemyCapabilities.None;

    /// <summary>
    /// Handles the touch <paramref name="context"/> reached: the ordinary touch of its enemy by its Samus
    /// (<see cref="ResolveOrdinaryTouch(RoomEnemyOccupant, SamusState, PseudoScrewCharge, TouchDeathAnimation)"/>).
    /// </summary>
    internal void ResolveOrdinaryTouch(
        EnemyTouchContext context,
        TouchDeathAnimation deathAnimation = TouchDeathAnimation.Play) =>
        ResolveOrdinaryTouch(context.Occupant, context.Samus, context.PseudoScrew, deathAnimation);

    /// <summary>
    /// Handles Samus touching one already-overlapping ordinary actor. An ordinary touch, with no
    /// attacking contact, damages Samus; Speed Booster, shinespark, Screw Attack, and pseudo-Screw instead read
    /// their own entries from the enemy's vulnerabilities and damage the actor.
    /// </summary>
    /// <param name="touched">The touched enemy, read through its place.</param>
    /// <param name="samus">Samus, who touched it.</param>
    /// <param name="pseudoScrew">Samus's held charge, which her Pseudo Screw contact spends.</param>
    /// <param name="deathAnimation">Whether a lethal touch starts the enemy's death here.</param>
    internal void ResolveOrdinaryTouch(
        RoomEnemyOccupant touched,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        TouchDeathAnimation deathAnimation = TouchDeathAnimation.Play)
    {
        RoomEnemyPlace enemy = Ensure.NotNull(touched).Place;
        SamusBodyAttack bodyAttack = samus.HorizontalSpeed.BodyAttack;
        int baseDamage;
        EnemyContactVulnerability contact = EnemyVulnerabilities.Of(touched.Definition).Contact;
        EnemyWeaponVulnerability vulnerability;
        switch (bodyAttack)
        {
            case SamusBodyAttack.None:
                ApplyOrdinaryTouchDamage(
                    samus,
                    touched.Definition.Damage,
                    enemy.Position.X.Whole);
                return;
            case SamusBodyAttack.SpeedBooster:
                baseDamage = EnemyCollisionDefinitions.SpeedBoosterContactDamage;
                vulnerability = contact.SpeedBooster;
                break;
            case SamusBodyAttack.Shinespark:
                baseDamage = EnemyCollisionDefinitions.ShinesparkContactDamage;
                vulnerability = contact.Shinespark;
                break;
            case SamusBodyAttack.ScrewAttack:
                baseDamage = EnemyCollisionDefinitions.ScrewAttackContactDamage;
                vulnerability = contact.ScrewAttack;
                break;
            case SamusBodyAttack.PseudoScrew:
                // The game consumes charge before checking vulnerability. Even immune
                // contact removes Pseudo Screw's flare; genuine Screw Attack does not.
                pseudoScrew.Spend(samus);
                baseDamage = EnemyCollisionDefinitions.PseudoScrewContactDamage;
                vulnerability = contact.PseudoScrew;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusBodyAttack)} {bodyAttack}.");
        }
        int damage = vulnerability.DamageFrom(baseDamage);
        if (damage == 0)
            return;

        // Touch damage shows the hurt flash alone, unlike projectile and
        // power-bomb damage, which add their own visible flash. Samus's timers are also
        // cleared, because Samus touching it this way is Samus attacking the enemy.
        enemy.HurtFlash.Restart(enemy.HurtFlashFrames);
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
        samus.ClearHurtTimers();
        enemy.Health = damage >= enemy.Health
            ? 0
            : enemy.Health - damage;
        _audio.QueueEnemySound(EnemySound.ContactKill, priority: SoundRequestPriority.WhenIdle);
        if (enemy.Health != 0)
            return;

        // A Rinka killed by touch chooses its own death, the generic respawning death or, one of Mother Brain's, its
        // hidden respawn two updates on, so its place is left as it is here.
        if (deathAnimation == TouchDeathAnimation.Skip)
            return;

        _deathsAndDrops.StartGenericEnemyDeath(touched, EnemyDeathExplosion.SamusContact, samus);
    }

    /// <summary>Uses the strict radius test, excluding contact where either axis is only tangent.</summary>
    /// <remarks>
    /// Each axis's distance is a <see cref="DistanceAcrossRoomEdge.Between"/>, the short way between the two. The test meets every
    /// enemy with Samus, her shots and her bombs, and either side can lie past the room's origin: a fly that has climbed
    /// through the ceiling or a Bull braking past Samus lies above or left of it, and a shot lives on up to 64 pixels past
    /// the view's left and top edges, a Wave shot passing through walls.
    /// </remarks>
    internal static bool RadiusBoxesOverlap(
        Position16<RoomPixels> firstPosition,
        Extent16<RoomPixels> firstRadius,
        Position16<RoomPixels> secondPosition,
        Extent16<RoomPixels> secondRadius) =>
        DistanceAcrossRoomEdge.Between(firstPosition.X, secondPosition.X) < firstRadius.X + secondRadius.X &&
        DistanceAcrossRoomEdge.Between(firstPosition.Y, secondPosition.Y) < firstRadius.Y + secondRadius.Y;

    /// <summary>Applies suit reduction and health loss, then publishes the hurt direction and invincibility window.</summary>
    internal static void ApplyOrdinaryTouchDamage(
        SamusState samus,
        int damageBeforeSuit,
        int damageSourceX)
    {
        // The suit's damage division quarters the source definition's damage for Gravity or halves
        // it for Varia before normal touch publishes the invincibility/hurt request.
        SamusSuitDamage.Hurt(samus, damageBeforeSuit);
        KnockbackHorizontalDirection direction = samus.Position.X.Whole >= damageSourceX ? KnockbackHorizontalDirection.Right : KnockbackHorizontalDirection.Left;
        // The contact publishes a hit request; Samus admits it only when no
        // knockback direction is installed. Damage can recur after invincibility
        // expires even if the old movement owner remains active (e.g. a boss grab).
        samus.RequestKnockback(EnemyCollisionDefinitions.SamusTouchInvincibilityUpdates,
            EnemyCollisionDefinitions.SamusTouchKnockbackUpdates, direction);
        // The enemy pass only publishes the request; Samus's knockback pose starts later in the update.
    }

    /// <summary>
    /// Whether Samus stands on <paramref name="enemy"/> (#2155): the one test every enemy that carries or supports her
    /// makes, Kraid's lints, the platforms, the Mama Turtle and her babies, the shutters, Oum and the Work Robots, with
    /// her centre reckoned <see cref="EnemyCollisionDefinitions.SamusStandingBias"/> pixels lower.
    /// </summary>
    internal static bool IsSamusStandingOn(RoomEnemyPlace enemy, SamusState samus) =>
        IsSamusStandingOn(enemy, samus, EnemyCollisionDefinitions.SamusStandingBias);

    /// <summary>
    /// Whether Samus stands on <paramref name="enemy"/> with her centre reckoned <paramref name="bias"/> pixels lower
    /// (#2155, #2342): that centre lies above the enemy's centre, and their bodies overlap: horizontally strictly, so bodies
    /// that only meet side by side do not count, and vertically including the pixel where her feet meet the enemy's top.
    /// Every enemy that asks stays in the room, and Samus lies above its top only on a departing up-elevator, where none
    /// of them is placed. The Kzan reckons her lower by its own bias
    /// (<see cref="Kzan.KzanMotionDefinitions.SamusStandingBias"/>), every other enemy by the common one.
    /// </summary>
    internal static bool IsSamusStandingOn(RoomEnemyPlace enemy, SamusState samus, int bias)
    {
        if (Math.Abs(samus.Position.X.Whole - enemy.Position.X.Whole) >=
            samus.Kinematics.Radius.X + enemy.Radius.X)
            return false;

        int below = samus.Position.Y.Whole + bias - enemy.Position.Y.Whole;
        return below < 0 && -below <= samus.Kinematics.Radius.Y + enemy.Radius.Y;
    }
}
