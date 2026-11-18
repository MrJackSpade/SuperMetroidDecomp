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
/// Samus-to-enemy interaction: touch, shot, bomb and power-bomb collision walks over the interactive
/// enemies, frame hitboxes, vulnerability and freezing, and the registered reactions they run.
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
    /// The touch reaction a touch selects when it meets <paramref name="place"/> by its radius: the shared one its
    /// definition names, or else the species' own its hitboxes name (<see cref="IEnemyFrameHitboxes.SpeciesTouch"/>);
    /// null when neither names one.
    /// </summary>
    internal static EnemyReaction? TouchReactionOf(RoomEnemyPlace place) =>
        place.Definition.Touch ?? (place.Entity as IEnemyFrameHitboxes)?.SpeciesTouch;

    /// <summary>
    /// The shot reaction a shot or bomb selects when it meets <paramref name="place"/> by its radius: the shared one its
    /// definition names, or else the species' own its hitboxes name (<see cref="IEnemyFrameHitboxes.SpeciesShot"/>);
    /// null when neither names one.
    /// </summary>
    internal static EnemyReaction? ShotReactionOf(RoomEnemyPlace place) =>
        place.Definition.Shot ?? (place.Entity as IEnemyFrameHitboxes)?.SpeciesShot;

    /// <summary>Runs the common radius-based touch between Samus and the enemy the enemy pass is processing.</summary>
    /// <param name="samus">Live player state receiving the touch reaction.</param>
    /// <param name="pseudoScrew">Samus's held charge, which her Pseudo Screw contact spends.</param>
    /// <param name="controllerInput">Current held input used by touch responses.</param>
    /// <param name="level">Room terrain required by collision-aware touch responses.</param>
    /// <param name="place">The enemy the enemy pass is processing; one that is not interactive touches nothing.</param>
    /// <param name="camera">The camera's position as the pass runs, which a reaction that starts a sound on screen reads.</param>
    /// <returns>Whether the enemy reacted to touching Samus.</returns>
    public bool ResolveOrdinarySamusContact(
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        GameplayAction controllerInput,
        RoomLevelData level,
        RoomEnemyPlace place,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(place);
        SamusBodyAttack bodyAttack = samus.HorizontalSpeed.BodyAttack;
        if (bodyAttack == SamusBodyAttack.None && !samus.InvincibilityTimer.IsZero)
            return false;
        if (!_lifecycle.InteractiveEnemies.Contains(place))
            return false;
        // Collision entry rejects an enemy showing no frame before resetting Samus's invulnerability: an actor not yet
        // showing a frame must not erase the timer. The reset still precedes overlap testing, so a distant actor with a
        // frame can legitimately clear it.
        if (place.Frame is null)
            return false;
        bool usesFrameHitboxes = UsesFrameHitboxesForSamus(place);
        // Frame-hitbox entry withholds the touch the definition excludes from its hitboxes before resetting
        // invulnerability. Ordinary radius entry resets first instead.
        if (usesFrameHitboxes && place.Definition.TouchHitboxScan == EnemyHitboxScan.Withheld)
            return false;
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
            return false;
        }

        bool overlapsSamus;
        EnemyReaction? hitboxTouch = TouchReactionOf(place);
        if (usesFrameHitboxes)
        {
            overlapsSamus = TryFindFrameHitboxReaction(
                place,
                samus.Position.Whole,
                samus.Kinematics.Radius,
                contactKind: HitboxContactKind.Touch,
                out hitboxTouch);
        }
        else
        {
            overlapsSamus = RadiusBoxesOverlap(
                place.Position.Whole,
                place.Radius,
                samus.Position.Whole,
                samus.Kinematics.Radius);
        }

        if (!overlapsSamus)
            return false;

        // On the ordinary radius path a frozen enemy never runs its touch AI, except a place awaiting its enemy's
        // respawn. The extended-hitbox path has no such gate.
        if (!usesFrameHitboxes && place.FrozenTimer != 0 &&
            !place.AwaitsRespawn)
        {
            return false;
        }

        touch.ReactToTouch(new EnemyTouchContext(
            place,
            samus,
            pseudoScrew,
            controllerInput,
            level,
            usesFrameHitboxes,
            hitboxTouch,
            camera));
        return true;
    }

    /// <summary>
    /// Runs common normal-enemy shot AI between Samus's shots and the enemy the enemy pass is processing. One
    /// projectile may resolve per enemy per pass, matching the collision response's exit.
    /// </summary>
    /// <param name="projectiles">Samus's live beams and missiles, whose enemy collisions may consume, reflect, or transition them.</param>
    /// <param name="sharedProjectiles">Shared bomb-projectile owner passed to projectile impact handling.</param>
    /// <param name="roomArea">The area of the room the enemies are in, which decides how long a shot's freeze lasts.</param>
    /// <param name="samus">Live player state for enemy-specific shot reactions.</param>
    /// <param name="enemy">The enemy the enemy pass is processing; one that is not interactive takes no shot.</param>
    /// <param name="camera">The camera's position as the pass runs, which a reaction that starts a sound on screen reads.</param>
    /// <returns>Whether a shot hit the enemy.</returns>
    public bool ResolveOrdinaryProjectileHit(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles,
        AreaId roomArea,
        SamusState samus,
        RoomEnemyPlace enemy,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);
        Ensure.NotNull(enemy);
        if (!_lifecycle.InteractiveEnemies.Contains(enemy))
            return false;

        // Shots likewise strike hidden enemies, but pass an enemy that shows no frame or shows the empty frame
        // while it collides by its radius, and one that ignores Samus. An enemy without a shot reaction is
        // skipped.
        if (enemy.Entity is IEnemyShotAdmission admission && !admission.AdmitsShots() ||
            CommonEnemyEmptyFrames.ShotsPass(enemy) ||
            enemy.InvincibilityTimer != 0 ||
            enemy.Presence.HasAny(EnemyPresence.Deleted) ||
            enemy.Contact.HasAny(EnemyContact.IgnoresSamus) ||
            (enemy.Entity as IEnemyShotReaction)?.ShotReaction is not { } reaction)
        {
            return false;
        }

        if (reaction.SkipsEnemy?.Invoke(enemy) == true)
            return false;

        foreach (SamusProjectile projectile in projectiles.Shots)
        {
            // The enemy's vulnerability answers for the beam or missile that hit it, before the impact below turns the
            // shot into its explosion.
            SamusProjectileType projectileType = projectile.Type;
            int projectileDamage = Ensure.AtLeastZero(projectile.Damage);
            SamusProjectileDirection projectileDirection = projectile.Direction;
            SamusProjectileFamily family = projectile.Type.Family;
            if (!projectile.HasEnemyCollisionPayload)
                continue;

            // Bomb, power-bomb, and pseudo-screw actors share the projectiles' places,
            // but ordinary enemy shot AI never treats them as beams or missiles.
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
                enemy.Definition.ShotHitboxScan == EnemyHitboxScan.Withheld)
            {
                // A definition that withholds shots from its hitboxes means no overlap, no
                // Super-Missile quake, and no direction mark even when a visible
                // authored rectangle would otherwise contain this projectile.
                continue;
            }

            bool overlapsProjectile;
            EnemyReaction? hitboxShot = ShotReactionOf(enemy);
            if (usesFrameHitboxes)
            {
                overlapsProjectile = TryFindFrameHitboxReaction(
                    enemy,
                    projectile.Position.Whole,
                    projectile.Radius,
                    contactKind: HitboxContactKind.Shot,
                    out hitboxShot);
            }
            else
            {
                overlapsProjectile = RadiusBoxesOverlap(
                    enemy.Position.Whole,
                    enemy.Radius,
                    projectile.Position.Whole,
                    projectile.Radius);
            }

            if (!overlapsProjectile)
                continue;

            bool selectedIgnore = hitboxShot == EnemyReaction.Ignore;
            var shot = new EnemyShotContext(
                enemy,
                projectile,
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
                return true;
            }

            if (usesFrameHitboxes && family == SamusProjectileFamily.SuperMissile)
            {
                // The frame-hitbox prelude requests this earthquake before running the
                // hitbox reaction, even when that reaction is the ignoring shell region.
                _earthquake.Start(SuperMissileDefinitions.EnemyHitEarthquake, SuperMissileDefinitions.EarthquakeDuration);
            }

            if (RunShotStage(reaction.AfterSuperMissileQuake, shot))
            {
                return true;
            }

            if (selectedIgnore)
            {
                projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(projectile, shot.MarksCollision);
                return true;
            }

            if (RunShotStage(reaction.BeforeImpact, shot))
            {
                return true;
            }

            if (reaction.AlwaysUsesCollisionPrelude || family is SamusProjectileFamily.Beam or
                SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
            {
                // The collision marks the projectile's direction, not a
                // terrain impact. Projectile frame behaviours consume that marker
                // on the projectiles' next update; the damage below shows its own hit burst at the enemy.
                // Super Missiles must retain their handler so it also clears their
                // collision links. Terrain conversion strands a live link that can
                // damage the enemy again after its flash timer expires.
                // Neither the beam nor the missile damage path rejects the removal marker: later
                // overlapping enemies in this pass still receive the live payload.
                projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(
                    projectile,
                    enemy.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                        !projectile.Type.BeamCombination.HasPlasma);
            }
            else if (!projectiles.EnemyImpacts.TryStartEnemyImpact(sharedProjectiles, projectile,
                enemy.Contact.HasAny(EnemyContact.BlocksPlasmaBeam)))
                continue;

            reaction.AfterImpact?.Invoke(shot);

            shot = shot with { HealthBefore = enemy.Health };
            if (RunShotStage(reaction.BeforeVulnerability, shot))
            {
                return true;
            }

            NormalShotHit hit = ApplyNormalShotDamage(enemy, projectile, projectileType, projectileDamage, roomArea);
            if (hit == NormalShotHit.Froze)
            {
                reaction.AfterCommon?.Invoke(shot with { Froze = true });
                return true;
            }

            if (hit is NormalShotHit.HurtFrozen or NormalShotHit.HurtUnfrozen)
            {
                if (hit == NormalShotHit.HurtUnfrozen && reaction.ShowsHitBurst(shot))
                    ShowHitBurst(enemy);
                if (enemy.Health == 0 && !reaction.ExemptFromGenericDeath)
                {
                    reaction.BeforeGenericDeath?.Invoke(shot);
                    if (reaction.ReplacesGenericDeath?.Invoke(shot) != true)
                    {
                        _deathsAndDrops.StartGenericEnemyDeath(
                            enemy,
                            EnemyDeathsAndDrops.SelectNormalShotDeathAnimation(enemy, projectileType, rule: ShotDeathAnimationRule.ByProjectile),
                            samus);
                    }
                }
            }

            reaction.AfterCommon?.Invoke(shot);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs common normal-shot damage (#2255), the shared step of every shot reaction that deals ordinary damage, the
    /// ordinary pass's and the bosses' own: the enemy's vulnerability to <paramref name="projectileType"/> freezes it
    /// outright, or makes the shot a dud when the damage is zero, or hurts it. A hurt enemy flashes, is hurt, cries unless
    /// frozen, takes Plasma's invincibility, and loses the damage, a lethal Ice-bearing beam freezing it instead where its
    /// vulnerability allows. The death and the hit burst that follow are the reaction's.
    /// </summary>
    /// <param name="enemy">The enemy the shot reached.</param>
    /// <param name="projectile">The shot, which a dud spends.</param>
    /// <param name="projectileType">The shot's type as it reached the enemy, before any impact rewrote it.</param>
    /// <param name="projectileDamage">The shot's damage, zero or more.</param>
    /// <param name="roomArea">The area of the room the enemy is in, which decides how long a freeze lasts.</param>
    /// <returns>Whether the shot froze the enemy outright, was a dud, or hurt it while it was frozen or not.</returns>
    internal NormalShotHit ApplyNormalShotDamage(
        RoomEnemyPlace enemy,
        SamusProjectile projectile,
        SamusProjectileType projectileType,
        int projectileDamage,
        AreaId roomArea)
    {
        Ensure.NotNull(enemy);
        Ensure.NotNull(projectile);
        NormalShotVulnerability shotVulnerability = ReadNormalShotVulnerability(enemy, projectileType);
        if (shotVulnerability.FreezeImmediately)
        {
            FreezeEnemyFromNormalShot(enemy, roomArea);
            return NormalShotHit.Froze;
        }

        int damage = shotVulnerability.DamageFrom(projectileDamage);
        if (damage == 0)
        {
            // A zero-damage hit is a dud even for Plasma: the immune enemy removes the shot, which does not pierce it.
            _projectiles.CreateEnemyProjectileDudShot(projectile);
            return NormalShotHit.Dud;
        }

        enemy.FlashTimer = enemy.HurtFlashFrames + EnemyCollisionDefinitions.WeaponHurtFlashExtension;
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
        // Common shot AI queues the definition's cry before subtracting health,
        // including lethal hits. Frozen targets suppress this cry; death
        // explosion animations retain their separate, later sound owner. The
        // same test, made here before a lethal Ice hit freezes the enemy, decides
        // the hit burst, which shows whether or not the enemy has a cry.
        bool hurtUnfrozen = enemy.FrozenTimer == 0;
        if (hurtUnfrozen && enemy.Definition.HurtSoundEffect is SoundEffectId cry)
            _audio.QueueEnemySound(cry, SoundRequestPriority.Low);
        // Every plasma-family hit gives sixteen invincibility frames.
        // Only a beam combining Plasma gives this; other projectile
        // families never count as Plasma.
        if (projectileType.BeamCombination.HasPlasma)
            enemy.InvincibilityTimer = EnemyCollisionDefinitions.PlasmaShotInvincibilityUpdates;

        bool lethal = damage >= enemy.Health;
        bool freezeInsteadOfKilling = lethal &&
            projectileType.BeamCombination.HasIce &&
            !shotVulnerability.PreventsLethalFreeze &&
            enemy.FrozenTimer == 0;
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
        return hurtUnfrozen ? NormalShotHit.HurtUnfrozen : NormalShotHit.HurtFrozen;
    }

    /// <summary>Runs an optional enemy-specific shot stage and reports whether it consumed the hit.</summary>
    private static bool RunShotStage(EnemyShotStage? stage, in EnemyShotContext context) =>
        stage is not null && stage(in context);

    /// <summary>
    /// Runs the ordinary-radius half of the enemies' bomb collision.
    /// </summary>
    /// <remarks>
    /// Normal bombs are not among the five beams and missiles tested above: the bomb pass waits for the shared bomb
    /// countdown to reach zero, marks the overlapping bomb as having hit, and runs the enemy's shot reaction with
    /// that bomb selected.
    ///
    /// This pass owns both the common reaction and the Metroid tail. Private per-species reactions remain deliberately
    /// outside the common branch; each joins it only with its own bomb-specific post-processing, rather than
    /// silently treating every reaction as the ordinary shot reaction.
    /// </remarks>
    /// <param name="bombs">Samus's bombs, whose exploding bombs may hit.</param>
    /// <param name="ordinaryProjectiles">Samus's beams and missiles, which reactions read.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="enemy">The enemy the enemy pass is processing.</param>
    /// <param name="camera">The camera's position as the pass runs, which a reaction that starts a sound on screen reads.</param>
    /// <returns>Whether a bomb hit the enemy.</returns>
    public bool ResolveOrdinaryBombHit(
        SamusBombProjectileSystem bombs,
        SamusProjectileSystem ordinaryProjectiles,
        SamusState samus,
        RoomEnemyPlace enemy,
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(bombs);
        Ensure.NotNull(ordinaryProjectiles);
        Ensure.NotNull(enemy);

        EnemyReaction? radiusShot = ShotReactionOf(enemy);
        bool usesCommonShot = radiusShot == EnemyReaction.NormalEnemyShot;
        bool usesIgnoringShot = radiusShot == EnemyReaction.Ignore;
        bool usesPrivateShot = HasPrivateNormalBombShot(enemy, radiusShot);
        bool usesFrameHitboxes = enemy.CollidesByFrameHitboxes;
        bool definitionWithholdsFrameHitboxShots = usesFrameHitboxes &&
            enemy.Definition.ShotHitboxScan == EnemyHitboxScan.Withheld;
        bool admittedByOccupant = enemy.Entity is IEnemyNormalBombAdmission admission &&
            admission.AdmitsNormalBombs(usesFrameHitboxes);
        if ((!usesCommonShot && !usesIgnoringShot &&
                !usesPrivateShot && !admittedByOccupant) ||
            enemy.Frame is null ||
            enemy.InvincibilityTimer != 0 ||
            enemy.Presence.HasAny(EnemyPresence.Deleted) ||
            definitionWithholdsFrameHitboxShots ||
            usesFrameHitboxes &&
                enemy.Contact.HasAny(EnemyContact.IgnoresSamus))
        {
            return false;
        }

        foreach (SamusBomb bomb in bombs.Bombs)
        {
            SamusProjectileFamily family = bomb.Type.Family;

            // The radius bomb pass admits an exploding normal bomb. The radius
            // pass also admits a spread bomb. The frame-hitbox pass is narrower,
            // and passes over a bomb on the room's leftmost column (LeftEdgeBombMissesFrameHitboxes).
            if (bomb.Type.IsNone ||
                !bomb.BombTimer.IsZero ||
                (usesFrameHitboxes
                    ? family != SamusProjectileFamily.Bomb ||
                      LeftEdgeBombMissesFrameHitboxes.Misses(bomb.Position.X.Whole)
                    : family != SamusProjectileFamily.Bomb &&
                      !bomb.IsBombSpread))
            {
                continue;
            }

            EnemyReaction? selectedShot = radiusShot;
            bool overlaps = usesFrameHitboxes
                ? TryFindFrameHitboxReaction(
                    enemy,
                    bomb.Position.Whole,
                    bomb.Radius,
                    contactKind: HitboxContactKind.Shot,
                    out selectedShot)
                : RadiusBoxesOverlap(
                    enemy.Position.Whole,
                    enemy.Radius,
                    bomb.Position.Whole,
                    bomb.Radius);
            if (!overlaps)
            {
                continue;
            }

            // Frame-hitbox actors run the reaction stored in the selected hitbox,
            // not necessarily the definition's. Do not turn a protected/private
            // component into common damage merely because the definition names the ordinary shot.
            bool selectedIgnore = selectedShot == EnemyReaction.Ignore;
            bool selectedPrivateReaction = HasPrivateNormalBombShot(enemy, selectedShot);
            if (selectedShot != EnemyReaction.NormalEnemyShot &&
                !selectedIgnore &&
                !selectedPrivateReaction)
                continue;

            // The bomb collision marks the bomb before its reaction runs. Unlike an
            // ordinary beam impact, the bomb is already running its explosion animation;
            // common shot AI must not replace it with an impact animation.
            bomb.IsSpent = true;
            var bombContext = new EnemyBombContext(
                enemy,
                bomb,
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
                EnemyBombReaction? reaction = bombReactions?.CommonBombDamageReaction(bombContext);
                if (reaction?.Rejects?.Invoke(bombContext) != true)
                {
                    bombContext = bombContext with { HealthBefore = enemy.Health };
                    reaction?.BeforeCommon?.Invoke(bombContext);
                    NormalBombHit hit = HurtByNormalBomb(enemy, bomb);
                    if (hit == NormalBombHit.HurtUnfrozen &&
                        (reaction?.ShowsHitBurst ?? EnemyBombReaction.AlwaysShowsHitBurst)(bombContext))
                    {
                        ShowHitBurst(enemy);
                    }
                    if (hit != NormalBombHit.Dud && (reaction?.RunsGenericDeath ?? true))
                        StartNormalBombDeathIfKilled(enemy, samus);
                    reaction?.AfterCommon?.Invoke(bombContext);
                }
            }

            return true;
        }
        return false;
    }

    /// <summary>
    /// Reports the private shot reactions that have a normal-bomb path. The occupant decides, because a frame's
    /// hitbox may deliberately override its definition with a different reaction.
    /// </summary>
    private static bool HasPrivateNormalBombShot(
        RoomEnemyPlace enemy,
        EnemyReaction? reaction) =>
        enemy.Entity is IEnemyPrivateShotReaction owner &&
        owner.OwnsPrivateShotReaction(reaction);

    /// <summary>
    /// Runs the common damage a normal bomb does through the shot reaction, without the hit burst or the death tail: the
    /// part of the reaction a species' own bomb handling calls before deciding the enemy's death itself.
    /// </summary>
    internal void ApplyNormalBombDamage(RoomEnemyPlace enemy, SamusBomb bomb) => _ = HurtByNormalBomb(enemy, bomb);

    /// <summary>
    /// Damages <paramref name="enemy"/> by <paramref name="bomb"/> through its bomb vulnerability: a zero multiplier leaves
    /// the dud flash and sound instead; otherwise the enemy flashes, is hurt, cries unless frozen and loses the damage.
    /// </summary>
    /// <returns>Whether the bomb was a dud, hurt the enemy while it was frozen, or hurt it while it was not.</returns>
    private NormalBombHit HurtByNormalBomb(RoomEnemyPlace enemy, SamusBomb bomb)
    {
        EnemyWeaponVulnerability vulnerability = EnemyVulnerabilities.Of(enemy.Definition).Ordnance.Bomb;
        int damage = vulnerability.DamageFrom(bomb.Damage);

        // A zero multiplier still consumes the collision and leaves the hit direction on the bomb: the bomb shows the
        // dud flash where it lies and the dud sound plays, and the enemy is untouched.
        if (damage == 0)
        {
            _projectiles.CreateBombDudShot(bomb);
            return NormalBombHit.Dud;
        }

        enemy.FlashTimer = enemy.HurtFlashFrames + EnemyCollisionDefinitions.WeaponHurtFlashExtension;
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
        // The common shot reaction a bomb runs queues the definition's cry before subtracting health, lethal hits
        // included, as it does for a beam or a missile; a frozen enemy makes no cry.
        bool hurtUnfrozen = enemy.FrozenTimer == 0;
        if (hurtUnfrozen && enemy.Definition.HurtSoundEffect is SoundEffectId cry)
            _audio.QueueEnemySound(cry, SoundRequestPriority.Low);
        enemy.Health = damage >= enemy.Health
            ? 0
            : enemy.Health - damage;
        return hurtUnfrozen ? NormalBombHit.HurtUnfrozen : NormalBombHit.HurtFrozen;
    }

    /// <summary>The ordinary shot reaction's death tail after a bomb's damage: a killed enemy starts its death.</summary>
    private void StartNormalBombDeathIfKilled(RoomEnemyPlace enemy, SamusState samus)
    {
        if (enemy.Health != 0)
            return;
        _deathsAndDrops.StartGenericEnemyDeath(
            enemy,
            EnemyDeathsAndDrops.SelectNormalShotDeathAnimation(enemy, SamusBombProjectileSystem.NormalBombType),
            samus);
    }

    /// <summary>
    /// Shows the enemy-hit burst at <paramref name="enemy"/>'s position: the ordinary shot reaction's sign that a shot or
    /// bomb hurt an enemy that was not frozen, shown after the damage and before the death. It is a room effect actor
    /// drawn with the room's graphics, so it takes one of the room's effect-actor places as it spawns.
    /// </summary>
    private void ShowHitBurst(RoomEnemyPlace enemy) =>
        _ = _effectActors.SpawnRoomEffectActor(enemy.Position.Whole, RoomEffectActorKind.HitBurst, graphics: default);

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
    public int ResolveOrdinaryPowerBombHits(
        Position16<RoomPixels> explosionCenter,
        int explosionRadius,
        RoomLevelData level,
        Position16<RoomPixels> camera,
        SamusState samus)
    {
        if (explosionRadius == 0)
            return 0;

        int horizontalRadius = explosionRadius;
        // The blast reaches three quarters as far vertically, rounded to the nearest pixel, a half pixel rounding down.
        int verticalRadius = (3 * horizontalRadius + 1) / 4;
        int reactionCount = 0;
        _powerBombRadius.Forget();

        // The blast reaches every enemy in its own order, whether or not it is updating; a power bomb reaches
        // off-screen enemies and keeps them updating off screen, below.
        foreach (RoomEnemyPlace enemy in EnemyPlaceOrder.InBlastOrder(_places.InUpdateOrder))
        {
            // A death inside the previous reaction resizes the horizontal reach for the rest of
            // the sweep; the vertical reach is unchanged.
            horizontalRadius = _powerBombRadius.HorizontalRadius(horizontalRadius);
            // A place with no enemy, a place awaiting its enemy's respawn among them, is passed.
            if (enemy.DefinitionId == EnemyDefinitionId.None ||
                enemy.InvincibilityTimer != 0 ||
                enemy.Presence.HasAny(EnemyPresence.Deleted))
            {
                continue;
            }

            // The radius actor takes the Power Bomb response, not the ordinary bomb's: every
            // enemy whose bomb and Power Bomb vulnerabilities differ depends on it.
            EnemyWeaponVulnerability vulnerability = EnemyVulnerabilities.Of(enemy.Definition).Ordnance.PowerBomb;
            if (vulnerability.Multiplier == 0)
                continue;

            // The blast lies in the room, but an enemy it reaches need not: a fly that has climbed through the ceiling or a
            // Bull braking past Samus can lie above or left of the room's origin, and the blast reaches one the short way,
            // as CoordinateAxis.Distance gives it.
            int xDistance = CoordinateAxis.Distance(explosionCenter.X, enemy.Position.X.Whole);
            int yDistance = CoordinateAxis.Distance(explosionCenter.Y, enemy.Position.Y.Whole);
            if (xDistance >= horizontalRadius || yDistance >= verticalRadius)
                continue;

            // A definition names only the ignoring reaction, which the entity answers, or none; an entity with a Power
            // Bomb reaction it has no response to throws here.
            EnemyPowerBombReaction? reaction = (enemy.Entity as IEnemyPowerBombReaction)?.PowerBombReaction;
            if (reaction?.SkipsEnemy?.Invoke(enemy) == true)
                continue;
            var context = new EnemyPowerBombContext(enemy, samus, level, camera);

            // A reaction of the enemy's own replaces the common Power Bomb damage, and the
            // enemy still goes on updating off screen once the blast's ellipse reaches it.
            if (reaction?.ReplacesCommon is { } privateReaction)
            {
                privateReaction(context);
                enemy.Processing = enemy.Processing.With(EnemyProcessing.OffScreen);
                reactionCount++;
                continue;
            }

            // The common reaction damages the enemy by a Power Bomb's damage, and then it keeps updating off screen.
            int damage = vulnerability.DamageFrom(SamusProjectileDamage.Read(SamusProjectileKind.PowerBomb));
            enemy.InvincibilityTimer = EnemyCollisionDefinitions.PowerBombInvincibilityUpdates;
            enemy.FlashTimer = enemy.HurtFlashFrames + EnemyCollisionDefinitions.WeaponHurtFlashExtension;
            enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
            enemy.Health = damage >= enemy.Health
                ? 0
                : enemy.Health - damage;
            if (enemy.Health == 0 && reaction?.ExemptFromGenericDeath != true)
            {
                reaction?.BeforeGenericDeath?.Invoke(context);
                if (reaction?.ReplacesGenericDeath is { } replaceDeath)
                    replaceDeath(context);
                else
                    _deathsAndDrops.StartGenericEnemyDeath(enemy, EnemyDeathExplosion.Small, samus);
            }

            reaction?.AfterCommon?.Invoke(context);

            enemy.Processing = enemy.Processing.With(EnemyProcessing.OffScreen);
            reactionCount++;
        }

        _powerBombRadius.Forget();
        return reactionCount;
    }

    /// <summary>
    /// Selects the exact multiplier the damage calculation uses for an ordinary shot: the beam combination's response, with a
    /// charged shot taking the charged-beam multiplier unless the beam freezes; missile, Super Missile, bomb and Power
    /// Bomb families take their own responses.
    /// </summary>
    internal static NormalShotVulnerability ReadNormalShotVulnerability(
        RoomEnemyPlace enemy,
        SamusProjectileType projectileType)
    {
        EnemyVulnerabilities vulnerabilities = EnemyVulnerabilities.Of(enemy.Definition);
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
                Multiplier: 0,
                FreezeImmediately: true,
                PreventsLethalFreeze: beam.PreventsLethalFreeze);
        }

        // A charged shot takes the charged-beam multiplier instead; zero makes the shot a dud.
        int multiplier = projectileType.IsChargedBeam ? vulnerabilities.ChargedBeam.Multiplier : beam.Multiplier;
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
    internal void FreezeEnemyFromNormalShot(RoomEnemyPlace enemy, AreaId roomArea)
    {
        // The direct-freeze vulnerability refreshes the timer even on an already frozen
        // actor, but only a newly frozen actor queues audio. Lethal-Ice substitution
        // reaches this step only with an initially zero clock.
        if (enemy.FrozenTimer == 0)
            _audio.QueueEnemySound(RoomSound.EnemyFrozen, priority: SoundRequestPriority.Low);
        enemy.FrozenTimer = roomArea == AreaId.Norfair
            ? EnemyCollisionDefinitions.NorfairFreezeUpdates
            : EnemyCollisionDefinitions.FreezeUpdates;
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Frozen;
        enemy.InvincibilityTimer = EnemyCollisionDefinitions.FreezeInvincibilityUpdates;
    }

    /// <summary>Selects the enemy's response to the projectile's beam combination or weapon family.</summary>
    internal static EnemyWeaponVulnerability ReadProjectileVulnerability(
        RoomEnemyPlace enemy,
        SamusProjectileType projectileType)
    {
        EnemyVulnerabilities vulnerabilities = EnemyVulnerabilities.Of(enemy.Definition);
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
        int Multiplier,
        bool FreezeImmediately,
        bool PreventsLethalFreeze)
    {
        /// <summary>
        /// The damage a shot of <paramref name="weaponDamage"/>, zero or more, deals by <see cref="Multiplier"/>, the rule
        /// every weapon response takes (<see cref="EnemyWeaponVulnerability.DamageFrom(int, int)"/>).
        /// </summary>
        internal int DamageFrom(int weaponDamage) => EnemyWeaponVulnerability.DamageFrom(weaponDamage, Multiplier);
    }

    /// <summary>
    /// Tests the hitboxes of the frame an enemy shows and returns the reaction belonging to the first overlapping
    /// hitbox. One frame may hold several hitboxes, each signed bounds about the enemy's origin with its own touch and
    /// shot reactions.
    /// </summary>
    internal static bool TryFindFrameHitboxReaction(
        RoomEnemyPlace enemy,
        Position16<RoomPixels> targetPosition,
        Extent16<RoomPixels> targetRadius,
        HitboxContactKind contactKind,
        out EnemyReaction? reaction)
    {
        reaction = null;

        // Frame-hitbox collision tests every frame an actor shows, and only an actor that has
        // shown none is skipped. The empty frame still has a point hitbox for an enemy that
        // collides by its frames' hitboxes, so it enters the same test as every other frame.
        if (enemy.Frame is null)
            return false;

        EnemyHitboxProbe target = EnemyHitboxProbe.Around(targetPosition, targetRadius, contactKind);

        if (CommonEnemyEmptyFrames.CollidesAtOrigin(enemy))
        {
            // The empty frame has a single zero-radius point at the
            // enemy origin. Run the same asymmetric overlap test before
            // selecting its common touch/shot reaction.
            if (!target.Overlaps(enemy.Position.X.Whole, enemy.Position.Y.Whole, enemy.Position.X.Whole, enemy.Position.Y.Whole))
                return false;
            reaction = contactKind == HitboxContactKind.Shot ? EnemyReaction.NormalEnemyShot : EnemyReaction.NormalEnemyTouch;
            return true;
        }

        // A species whose hitboxes the game defines answers for its own frames; null means none covers this frame.
        if (enemy.Entity is IEnemyFrameHitboxes owner && owner.FindHitboxReaction(target, out reaction) is { } owned)
            return owned;

        throw new InvalidDataException(
            $"Enemy {enemy.DefinitionId} extended collision frame " +
            $"{enemy.Frame} has no compiled geometry.");
    }

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
    /// encounter-owned actors collide by frame hitboxes elsewhere while their shots meet a definition's radius or
    /// encounter-specific geometry. Keeping this predicate beside the step gives data-backed probes the same
    /// boundary as gameplay.
    /// </summary>
    internal static bool UsesFrameHitboxesForProjectiles(RoomEnemyPlace enemy) =>
        enemy.CollidesByFrameHitboxes &&
        (Capabilities(enemy) & EnemyCapabilities.ProjectileFrameHitboxes) != EnemyCapabilities.None;

    /// <summary>The bound occupant's collision capabilities; an empty place has none.</summary>
    private static EnemyCapabilities Capabilities(RoomEnemyPlace enemy) =>
        enemy.Entity?.Capabilities ?? EnemyCapabilities.None;

    /// <summary>
    /// Handles the touch <paramref name="context"/> reached: the ordinary touch of its enemy by its Samus
    /// (<see cref="ResolveNormalEnemyTouch(RoomEnemyPlace, SamusState, PseudoScrewCharge, TouchDeathAnimation)"/>).
    /// </summary>
    internal void ResolveNormalEnemyTouch(
        EnemyTouchContext context,
        TouchDeathAnimation deathAnimation = TouchDeathAnimation.Play) =>
        ResolveNormalEnemyTouch(context.Place, context.Samus, context.PseudoScrew, deathAnimation);

    /// <summary>
    /// Handles Samus touching one already-overlapping ordinary actor. An ordinary touch, with no
    /// attacking contact, damages Samus; Speed Booster, shinespark, Screw Attack, and pseudo-Screw instead read
    /// their own entries from the enemy's vulnerabilities and damage the actor.
    /// </summary>
    /// <param name="enemy">The touched enemy.</param>
    /// <param name="samus">Samus, who touched it.</param>
    /// <param name="pseudoScrew">Samus's held charge, which her Pseudo Screw contact spends.</param>
    /// <param name="deathAnimation">Whether a lethal touch starts the enemy's death here.</param>
    internal void ResolveNormalEnemyTouch(
        RoomEnemyPlace enemy,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        TouchDeathAnimation deathAnimation = TouchDeathAnimation.Play)
    {
        SamusBodyAttack bodyAttack = samus.HorizontalSpeed.BodyAttack;
        int baseDamage;
        EnemyContactVulnerability contact = EnemyVulnerabilities.Of(enemy.Definition).Contact;
        EnemyWeaponVulnerability vulnerability;
        switch (bodyAttack)
        {
            case SamusBodyAttack.None:
                ApplyNormalEnemyTouchDamage(
                    samus,
                    enemy.Definition.Damage,
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
        // power-bomb paths which add their own visible-flash tail. Samus's timers are also
        // cleared, because Samus touching it this way is Samus attacking the enemy.
        enemy.FlashTimer = enemy.HurtFlashFrames;
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackTimer = 0;
        enemy.Health = damage >= enemy.Health
            ? 0
            : enemy.Health - damage;
        _audio.QueueEnemySound(EnemySound.ContactKill, priority: SoundRequestPriority.WhenIdle);
        if (enemy.Health != 0)
            return;

        // Rinka's touch reaction calls this common damage body explicitly, inspects the
        // resulting zero health, then chooses either generic respawn death or its private
        // hidden two-frame Mother Brain respawn. Do not clear the place before that tail.
        if (deathAnimation == TouchDeathAnimation.Skip)
            return;

        _deathsAndDrops.StartGenericEnemyDeath(enemy, EnemyDeathExplosion.SamusContact, samus);
    }

    /// <summary>Uses the strict radius test, excluding contact where either axis is only tangent.</summary>
    /// <remarks>
    /// Each axis's distance is a <see cref="CoordinateAxis.Distance"/>, the short way between the two. The test meets every
    /// enemy with Samus, her shots and her bombs, and either side can lie past the room's origin: a fly that has climbed
    /// through the ceiling or a Bull braking past Samus lies above or left of it, and a shot lives on up to 64 pixels past
    /// the view's left and top edges, a Wave shot passing through walls.
    /// </remarks>
    internal static bool RadiusBoxesOverlap(
        Position16<RoomPixels> firstPosition,
        Extent16<RoomPixels> firstRadius,
        Position16<RoomPixels> secondPosition,
        Extent16<RoomPixels> secondRadius) =>
        CoordinateAxis.Distance(firstPosition.X, secondPosition.X) < firstRadius.X + secondRadius.X &&
        CoordinateAxis.Distance(firstPosition.Y, secondPosition.Y) < firstRadius.Y + secondRadius.Y;

    /// <summary>Applies suit reduction and health loss, then publishes the hurt direction and invincibility window.</summary>
    internal static void ApplyNormalEnemyTouchDamage(
        SamusState samus,
        int damageBeforeSuit,
        int damageSourceX)
    {
        // The suit's damage division quarters the source definition's damage for Gravity or halves
        // it for Varia before normal touch publishes the invincibility/hurt request.
        SamusSuitDamage.Hurt(samus, damageBeforeSuit);
        samus.InvincibilityTimer = WrappingCountdown.Start(EnemyCollisionDefinitions.SamusTouchInvincibilityUpdates);
        KnockbackHorizontalDirection direction = samus.Position.X.Whole >= damageSourceX ? KnockbackHorizontalDirection.Right : KnockbackHorizontalDirection.Left;
        // The contact publishes a hit request; Samus admits it only when no
        // knockback direction is installed. Damage can recur after invincibility
        // expires even if the old movement owner remains active (e.g. a boss grab).
        samus.KnockbackTimer = EnemyCollisionDefinitions.SamusTouchKnockbackUpdates;
        samus.KnockbackXDirection = direction;
        // The enemy pass only publishes the request. The common post-animation
        // interruption owns pose/handler initialization later in this frame.
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
    /// of them is placed. The Kzan reckons her five pixels lower
    /// (<see cref="Kzan.KzanMotionDefinitions.SamusStandingBias"/>), every other enemy three.
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
