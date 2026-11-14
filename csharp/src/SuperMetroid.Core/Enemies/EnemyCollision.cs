using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Samus-to-enemy interaction: touch, shot, bomb and power-bomb collision walks over the interactive
/// enemies, frame hitboxes, vulnerability and freezing, and the dispatch into the registered reactions.
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
    /// <summary>Supplies the current enemy frame's timing and state to collision reactions.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Connects collision outcomes to enemy-owned projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies the active room's area and environment to collision logic.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Defines enemy iteration order for the collision passes.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Resizes a power bomb's sweep after each kill it makes.</summary>
    private readonly PowerBombRadiusAfterKill _powerBombRadius;
    /// <summary>The room's sprite objects, which the burst at an enemy a shot or bomb hurts joins.</summary>
    private readonly RoomSpriteObjects _spriteObjects;

    /// <summary>Binds the enemy and room owners whose state or reactions collisions can change.</summary>
    internal EnemyCollision(EnemyCollisionDependencies dependencies)
    {
        _powerBombRadius = dependencies.PowerBombRadius;
        _spriteObjects = dependencies.SpriteObjects;
        _lifecycle = dependencies.Lifecycle;
        _audio = dependencies.Audio;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _earthquake = dependencies.Earthquake;
        _frameContext = dependencies.FrameContext;
        _projectiles = dependencies.Projectiles;
        _room = dependencies.Room;
        _slotTable = dependencies.SlotTable;
    }

    /// <summary>
    /// The touch reaction a touch selects when it meets <paramref name="slot"/> by its radius: the shared one its
    /// definition names, or else the species' own its hitboxes name (<see cref="IEnemyFrameHitboxes.SpeciesTouch"/>);
    /// null when neither names one.
    /// </summary>
    internal static EnemyReaction? TouchReactionOf(RoomEnemySlot slot) =>
        slot.Definition.Touch ?? (slot.Entity as IEnemyFrameHitboxes)?.SpeciesTouch;

    /// <summary>
    /// The shot reaction a shot or bomb selects when it meets <paramref name="slot"/> by its radius: the shared one its
    /// definition names, or else the species' own its hitboxes name (<see cref="IEnemyFrameHitboxes.SpeciesShot"/>);
    /// null when neither names one.
    /// </summary>
    internal static EnemyReaction? ShotReactionOf(RoomEnemySlot slot) =>
        slot.Definition.Shot ?? (slot.Entity as IEnemyFrameHitboxes)?.SpeciesShot;

    /// <summary>Runs the common radius-based Samus/enemy touch pass.</summary>
    /// <param name="samus">Live player state receiving the touch reaction.</param>
    /// <param name="controllerInput">Current held input used by touch responses.</param>
    /// <param name="level">Room terrain required by collision-aware touch responses.</param>
    /// <param name="onlyEnemy">The enemy the enemy pass is processing, or null for a standalone whole-list audit.</param>
    /// <param name="camera">The camera's position as the pass runs, which a reaction that starts a sound on screen reads; a standalone audit passes the room's top-left corner.</param>
    public bool ResolveOrdinarySamusContact(
        SamusState samus,
        ControllerButton controllerInput,
        RoomLevelData? level = null,
        RoomEnemySlot? onlyEnemy = null,
        Position16<RoomPixels> camera = default)
    {
        Ensure.NotNull(samus);
        _room.EnsureLoaded();
        SamusContactDamageIndex contactDamageIndex = samus.HorizontalSpeed.ContactDamageIndex;
        if (contactDamageIndex == SamusContactDamageIndex.None && !samus.InvincibilityTimer.IsZero)
            return false;

        foreach (RoomEnemySlot slot in _lifecycle.InteractiveEnemies)
        {
            if (onlyEnemy is not null && slot != onlyEnemy)
                continue;
            // Collision entry rejects an enemy showing no frame before resetting Samus's
            // invulnerability. Keep this per enemy: an empty interactive list or an actor
            // not yet showing a frame must not erase the timer. The reset still precedes
            // overlap testing, so a distant actor with a frame can legitimately clear it.
            if (slot.Frame is null)
                continue;
            bool usesFrameHitboxes = UsesFrameHitboxesForSamus(slot);
            // Frame-hitbox entry withholds the touch the definition excludes from its hitboxes before
            // resetting invulnerability. Ordinary radius entry resets first instead.
            if (usesFrameHitboxes && slot.Definition.TouchHitboxScan == EnemyHitboxScan.Withheld)
                continue;
            if (contactDamageIndex != SamusContactDamageIndex.None)
                samus.InvincibilityTimer = WrappingCountdown.Start(0);

            // Touch collision does not read the invisible property as a collision flag;
            // it suppresses rendering only. Hibashi relies on that distinction: its second
            // slot remains invisible while its animation moves a live hitbox.
            // Both Ridley definitions use a hand-authored extended body plus a separately
            // solved tail tip. The enemy pass dispatches its body at this same pre-AI seam;
            // tail processing checks the tip later. Do not flatten either to the definition's
            // dummy 8x8 radius here. An actor without a touch reaction is skipped.
            if (slot.Entity is IEnemyContactHooks { OwnsSamusTouch: true } ||
                slot.Presence.HasAny(EnemyPresence.Deleted) ||
                slot.Entity is not IEnemyTouchReaction { ReactsToTouch: true } touch)
            {
                continue;
            }

            bool overlapsSamus;
            EnemyReaction? hitboxTouch = TouchReactionOf(slot);
            if (usesFrameHitboxes)
            {
                overlapsSamus = TryFindFrameHitboxReaction(
                    slot,
                    samus.Position.Whole,
                    samus.Kinematics.Radius,
                    contactKind: HitboxContactKind.Touch,
                    out hitboxTouch);
            }
            else
            {
                overlapsSamus = RadiusBoxesOverlap(
                    slot.Position.Whole,
                    slot.Radius,
                    samus.Position.Whole,
                    samus.Kinematics.Radius);
            }

            if (!overlapsSamus)
                continue;

            // On the ordinary radius path a frozen enemy never runs its touch
            // AI, except a place awaiting its enemy's respawn. The extended-hitbox path has no such gate.
            if (!usesFrameHitboxes && slot.FrozenTimer != 0 &&
                !slot.AwaitsRespawn)
            {
                continue;
            }

            touch.ReactToTouch(new EnemyTouchContext(
                slot,
                samus,
                controllerInput,
                level,
                usesFrameHitboxes,
                hitboxTouch,
                contactDamageIndex,
                camera));
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs common normal-enemy shot AI for ordinary radius-based actors. One
    /// projectile may resolve per enemy per pass, matching the collision response's exit.
    /// </summary>
    /// <param name="projectiles">Live Samus beam and missile slots whose enemy collisions may consume, reflect, or transition their projectiles.</param>
    /// <param name="sharedProjectiles">Shared bomb-projectile owner passed to projectile impact handling.</param>
    /// <param name="roomArea">The area of the room the enemies are in, which decides how long a shot's freeze lasts.</param>
    /// <param name="samus">Live player state for enemy-specific shot reactions; may be null only for paths that do not require it, while player-dependent reactions such as Yard beam launch reject its absence.</param>
    /// <param name="onlyEnemy">The enemy the enemy pass is processing, or null for a standalone whole-population audit.</param>
    /// <param name="camera">The camera's position as the pass runs, which a reaction that starts a sound on screen reads; a standalone audit passes the room's top-left corner.</param>
    public int ResolveOrdinaryProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles,
        AreaId roomArea,
        SamusState? samus = null,
        RoomEnemySlot? onlyEnemy = null,
        Position16<RoomPixels> camera = default)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);
        _room.EnsureLoaded();

        int hitCount = 0;
        foreach (RoomEnemySlot enemy in _lifecycle.InteractiveEnemies)
        {
            if (onlyEnemy is not null && enemy != onlyEnemy)
                continue;

            // The shot pass likewise admits visually invisible actors, but rejects an enemy that shows no frame or
            // shows the empty frame while it collides by its radius, and the ignore-Samus-collision property. Keeping
            // these meanings separate mirrors the list/collision machinery. An actor
            // without a shot reaction is skipped.
            if (enemy.Entity is IEnemyShotAdmission admission && !admission.AdmitsShots() ||
                CommonEnemyEmptyFrames.ShotsPass(enemy) ||
                enemy.InvincibilityTimer != 0 ||
                enemy.Presence.HasAny(EnemyPresence.Deleted) ||
                enemy.Contact.HasAny(EnemyContact.IgnoresSamus) ||
                (enemy.Entity as IEnemyShotReaction)?.ShotReaction is not { } reaction)
            {
                continue;
            }


            if (reaction.SkipsEnemy?.Invoke(enemy) == true)
                continue;

            foreach (SamusProjectile projectile in projectiles.Slots)
            {
                // The slot still holds the live beam or missile when the enemy's collision
                // response selects vulnerability. Starting the impact below deliberately
                // turns the slot into an explosion, so retain the incoming type instead of
                // accidentally asking the vulnerability table about the visual effect.
                SamusProjectileType projectileType = projectile.Type;
                int projectileDamage = Ensure.AtLeastZero(projectile.Damage);
                SamusProjectileDirection projectileDirection = projectile.Direction;
                SamusProjectileFamily family = projectile.Type.Family;
                if (!projectile.HasEnemyCollisionPayload)
                    continue;

                // Bomb, power-bomb, and pseudo-screw actors share the projectile slot array,
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
                    hitCount++;
                    break;
                }

                if (usesFrameHitboxes && family == SamusProjectileFamily.SuperMissile)
                {
                    // The frame-hitbox prelude requests this earthquake before dispatching the
                    // hitbox reaction, even when that reaction is the ignoring shell region.
                    _earthquake.Start(SuperMissileDefinitions.EnemyHitEarthquake, SuperMissileDefinitions.EarthquakeDuration);
                }

                if (RunShotStage(reaction.AfterSuperMissileQuake, shot))
                {
                    hitCount++;
                    break;
                }

                if (selectedIgnore)
                {
                    projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(projectile, shot.MarksCollision);
                    hitCount++;
                    break;
                }

                if (RunShotStage(reaction.BeforeImpact, shot))
                {
                    hitCount++;
                    break;
                }

                if (reaction.AlwaysUsesCollisionPrelude || family is SamusProjectileFamily.Beam or
                    SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
                {
                    // The collision walker marks the projectile's direction, not a
                    // terrain impact. Projectile frame behaviours consume that marker
                    // on the next alpha pass; the damage below shows its own hit burst at the enemy.
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
                    hitCount++;
                    break;
                }

                NormalShotVulnerability shotVulnerability =
                    ReadNormalShotVulnerability(enemy, projectileType);

                if (shotVulnerability.FreezeImmediately)
                {
                    FreezeEnemyFromNormalShot(enemy, roomArea);
                    reaction.AfterCommon?.Invoke(shot with { Froze = true });
                    hitCount++;
                    break;
                }

                int damage = shotVulnerability.DamageFrom(projectileDamage);
                if (damage == 0)
                {
                    // A zero-damage hit enters the dud branch even for Plasma: immunity removes
                    // the projectile instead of letting its ordinary piercing rule win.
                    _projectiles.CreateEnemyProjectileDudShot(projectile);
                }
                if (damage != 0)
                {
                    enemy.FlashTimer = enemy.HurtFlashFrames + EnemyCollisionDefinitions.WeaponHurtFlashExtension;
                    enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
                    // Common shot AI queues the definition's cry before subtracting health,
                    // including lethal hits. Frozen targets suppress this cry; death
                    // explosion animations retain their separate, later sound owner. The
                    // same test, made here before a lethal Ice hit freezes the enemy, decides
                    // the hit burst below, which shows whether or not the enemy has a cry.
                    bool hurtUnfrozen = enemy.FrozenTimer == 0;
                    if (hurtUnfrozen && enemy.Definition.HurtSoundEffect is SoundEffectId cry)
                        _audio.QueueEnemySound(cry, SoundRequestPriority.Low);
                    // Every plasma-family hit gives sixteen invincibility frames.
                    // Only a beam combining Plasma gives this; other projectile
                    // families never count as Plasma.
                    if (projectileType.BeamCombination.HasPlasma)
                        enemy.InvincibilityTimer = EnemyShotTiming.PlasmaInvincibilityFrames;

                    bool lethal = damage >= enemy.Health;
                    bool freezeInsteadOfKilling = lethal &&
                        projectileType.BeamCombination.HasIce &&
                        !shotVulnerability.PreventsLethalFreeze &&
                        enemy.FrozenTimer == 0;
                    if (freezeInsteadOfKilling)
                    {
                        // A lethal Ice-bearing beam leaves health unchanged and freezes the
                        // actor. This is not the same path as an authored freezing entry above:
                        // hurt flash and hurt state were already set before the lethal test.
                        FreezeEnemyFromNormalShot(enemy, roomArea);
                    }
                    else
                    {
                        enemy.Health = lethal
                            ? 0
                            : enemy.Health - damage;
                    }
                    if (hurtUnfrozen && reaction.ShowsHitBurst(shot))
                        ShowHitBurst(enemy);
                    if (enemy.Health == 0 && !reaction.ExemptFromGenericDeath)
                    {
                        reaction.BeforeGenericDeath?.Invoke(shot);
                        if (reaction.ReplacesGenericDeath?.Invoke(shot) != true)
                        {
                            _deathsAndDrops.StartGenericEnemyDeath(
                                enemy,
                                EnemyDeathsAndDrops.SelectNormalShotDeathAnimation(enemy, projectileType, rule: ShotDeathAnimationRule.ByProjectile));
                        }
                    }
                }

                reaction.AfterCommon?.Invoke(shot);
                hitCount++;
                break;
            }
        }
        return hitCount;
    }

    /// <summary>Runs an optional enemy-specific shot stage and reports whether it consumed the hit.</summary>
    private static bool RunShotStage(EnemyShotStage? stage, in EnemyShotContext context) =>
        stage is not null && stage(in context);

    /// <summary>
    /// Runs the ordinary-radius half of the enemies' bomb collision.
    /// </summary>
    /// <remarks>
    /// Normal bombs are not part of the five-shot beam/missile walk above: the bomb pass waits for the shared bomb
    /// countdown to reach zero, marks the overlapping bomb as having hit, and dispatches the enemy's shot reaction with
    /// that bomb selected.
    ///
    /// This pass owns both the common reaction and the Metroid tail. Private per-species reactions remain deliberately
    /// outside the common branch; each joins it only with its own bomb-specific post-processing, rather than
    /// silently treating every reaction as the ordinary shot reaction.
    /// </remarks>
    /// <param name="bombs">The bomb slots whose exploding bombs may hit.</param>
    /// <param name="ordinaryProjectiles">Samus's beam and missile slots, which reactions read.</param>
    /// <param name="samus">Samus, when the pass has one.</param>
    /// <param name="onlyEnemy">The enemy the enemy pass is processing, or null for a standalone whole-population audit.</param>
    /// <param name="camera">The camera's position as the pass runs, which a reaction that starts a sound on screen reads; a standalone audit passes the room's top-left corner.</param>
    public int ResolveOrdinaryBombHits(
        SamusBombProjectileSystem bombs,
        SamusProjectileSystem ordinaryProjectiles,
        SamusState? samus = null,
        RoomEnemySlot? onlyEnemy = null,
        Position16<RoomPixels> camera = default)
    {
        Ensure.NotNull(bombs);
        Ensure.NotNull(ordinaryProjectiles);
        _room.EnsureLoaded();

        int hitCount = 0;
        for (int ordinal = 0; ordinal < (onlyEnemy is not null ? 1 : _lifecycle.InteractiveEnemies.Count); ordinal++)
        {
            RoomEnemySlot enemy = onlyEnemy ?? _lifecycle.InteractiveEnemies[ordinal];
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
                continue;
            }

            foreach (SamusBombProjectileSlot bomb in bombs.Slots)
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

                // Frame-hitbox actors dispatch the reaction stored in the selected hitbox,
                // not necessarily the definition's. Do not turn a protected/private
                // component into common damage merely because the definition names the ordinary shot.
                bool selectedIgnore = selectedShot == EnemyReaction.Ignore;
                bool selectedPrivateReaction = HasPrivateNormalBombShot(enemy, selectedShot);
                if (selectedShot != EnemyReaction.NormalEnemyShot &&
                    !selectedIgnore &&
                    !selectedPrivateReaction)
                    continue;

                // The bomb collision marks the physical bomb before dispatch. Unlike an
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
                            StartNormalBombDeathIfKilled(enemy);
                        reaction?.AfterCommon?.Invoke(bombContext);
                    }
                }

                hitCount++;
                break;
            }
        }

        return hitCount;
    }

    /// <summary>
    /// Reports the private shot reactions that have a normal-bomb path. The occupant decides, because a frame's
    /// hitbox may deliberately override its definition with a different reaction.
    /// </summary>
    private static bool HasPrivateNormalBombShot(
        RoomEnemySlot enemy,
        EnemyReaction? reaction) =>
        enemy.Entity is IEnemyPrivateShotReaction owner &&
        owner.OwnsPrivateShotReaction(reaction);

    /// <summary>
    /// Runs the common damage a normal bomb does through the shot reaction, without the hit burst or the death tail: the
    /// part of the reaction a species' own bomb handling calls before deciding the enemy's death itself.
    /// </summary>
    internal void ApplyNormalBombDamage(RoomEnemySlot enemy, SamusBombProjectileSlot bomb) => _ = HurtByNormalBomb(enemy, bomb);

    /// <summary>
    /// Damages <paramref name="enemy"/> by <paramref name="bomb"/> through its bomb vulnerability: a zero multiplier leaves
    /// the dud flash and sound instead; otherwise the enemy flashes, is hurt, cries unless frozen and loses the damage.
    /// </summary>
    /// <returns>Whether the bomb was a dud, hurt the enemy while it was frozen, or hurt it while it was not.</returns>
    private NormalBombHit HurtByNormalBomb(RoomEnemySlot enemy, SamusBombProjectileSlot bomb)
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
    private void StartNormalBombDeathIfKilled(RoomEnemySlot enemy)
    {
        if (enemy.Health != 0)
            return;
        _deathsAndDrops.StartGenericEnemyDeath(
            enemy,
            EnemyDeathsAndDrops.SelectNormalShotDeathAnimation(enemy, SamusBombProjectileSystem.NormalBombType));
    }

    /// <summary>
    /// Shows the enemy-hit burst at <paramref name="enemy"/>'s position: the ordinary shot reaction's sign that a shot or
    /// bomb hurt an enemy that was not frozen, shown after the damage and before the death. It is a room sprite object
    /// drawn with the room's graphics, so it takes one of the room's sprite-object places as it spawns.
    /// </summary>
    private void ShowHitBurst(RoomEnemySlot enemy) =>
        _ = _spriteObjects.SpawnRoomSpriteObject(enemy.Position.Whole, RoomSpriteObjectKind.EnemyHit, graphics: default);

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
    /// sample. The caller supplies the live power-bomb radius in whole pixels; the game
    /// uses it as the horizontal radius and derives a three-quarter-height vertical ellipse.
    /// </summary>
    /// <param name="explosionCenter">The explosion's centre.</param>
    /// <param name="explosionRadius">The explosion's horizontal radius in whole pixels; zero reaches nothing.</param>
    /// <param name="level">The room's terrain this update, which a species reaction that runs its main behaviour reads.</param>
    /// <param name="camera">The camera's position as the pass runs, which a species reaction that runs its main behaviour reads.</param>
    /// <param name="samus">Samus, when the update has one.</param>
    public int ResolveOrdinaryPowerBombHits(
        Position16<RoomPixels> explosionCenter,
        int explosionRadius,
        RoomLevelData level,
        Position16<RoomPixels> camera,
        SamusState? samus = null)
    {
        _room.EnsureLoaded();
        if (explosionRadius == 0)
            return 0;

        int horizontalRadius = explosionRadius;
        // The vertical radius keeps the remainder of the first halving through the addition. Expressing each
        // step makes odd-radius rounding agree with the game rather than using 0.75f.
        int halvingRemainder = horizontalRadius & 1;
        int verticalRadius = ((horizontalRadius >> 1) + horizontalRadius + halvingRemainder) >> 1;
        int reactionCount = 0;
        _powerBombRadius.Forget();

        // The blast reaches every slot in its own order, independent of the ordinary
        // active list. That matters while a power bomb reaches off-screen actors and then
        // sets their process-off-screen property below.
        foreach (RoomEnemySlot enemy in EnemySlotOrder.InBlastOrder(_slotTable.Slots))
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

            // Private reactions replace common power-bomb damage, but still receive the
            // process-off-screen bit after a qualifying ellipse overlap.
            if (reaction?.ReplacesCommon is { } privateReaction)
            {
                privateReaction(context);
                enemy.Processing = enemy.Processing.With(EnemyProcessing.OffScreen);
                reactionCount++;
                continue;
            }

            // The common reaction damages the enemy by a Power Bomb's damage; the process-off-screen property follows.
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
                    _deathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: EnemyDeathExplosion.Small);
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
        RoomEnemySlot enemy,
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

        // A charged shot takes the charged-beam multiplier instead; zero takes the dud-shot branch.
        int multiplier = projectileType.IsChargedBeam ? vulnerabilities.ChargedBeam.Multiplier : beam.Multiplier;
        return new NormalShotVulnerability(
            multiplier,
            FreezeImmediately: false,
            PreventsLethalFreeze: beam.PreventsLethalFreeze);
    }

    /// <summary>
    /// Freezes an enemy a shot froze: for <see cref="EnemyCollisionDefinitions.NorfairFreezeUpdates"/> in a Norfair room
    /// and <see cref="EnemyCollisionDefinitions.FreezeUpdates"/> in a room of any other area, as the original reads the
    /// current area. The protection after a freeze is shared by direct authored freezing and the lethal-Ice substitution.
    /// </summary>
    /// <param name="enemy">The enemy frozen.</param>
    /// <param name="roomArea">The area of the room the enemy is in.</param>
    internal void FreezeEnemyFromNormalShot(RoomEnemySlot enemy, AreaId roomArea)
    {
        // The direct-freeze vulnerability refreshes the timer even on an already frozen
        // actor, but only a newly frozen actor queues audio. Lethal-Ice substitution
        // reaches this helper only with an initially zero clock.
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
        RoomEnemySlot enemy,
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
        SamusProjectileFamily.Beam or SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion =>
            throw new InvalidDataException($"Projectile family {family} has no translated vulnerability field."),
        _ => throw new InvalidDataException($"Projectile family {family} has no translated vulnerability field."),
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
        RoomEnemySlot enemy,
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

        var target = new EnemyHitboxProbe(
            TopLeft: new(
                CoordinateAxis.Wrap(targetPosition.X - targetRadius.X),
                CoordinateAxis.Wrap(targetPosition.Y - targetRadius.Y)),
            BottomRight: new(
                CoordinateAxis.Wrap(targetPosition.X + targetRadius.X),
                CoordinateAxis.Wrap(targetPosition.Y + targetRadius.Y)),
            SelectsShot: contactKind == HitboxContactKind.Shot);

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

        // A species with engine-owned geometry answers for its own frames; null means none covers this frame.
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
    internal static bool UsesFrameHitboxesForSamus(RoomEnemySlot enemy) =>
        enemy.CollidesByFrameHitboxes &&
        (Capabilities(enemy) & EnemyCapabilities.SamusFrameHitboxes) != EnemyCapabilities.None;

    /// <summary>
    /// Identifies actors whose contact with Samus's shots is tested against their frames' hitboxes.
    /// <see cref="RoomEnemySlot.CollidesByFrameHitboxes"/> alone is not sufficient: Mother Brain and several
    /// encounter-owned actors collide by frame hitboxes elsewhere while their shots meet a definition's radius or
    /// encounter-specific geometry. Keeping this predicate beside the step gives data-backed probes the same
    /// boundary as gameplay.
    /// </summary>
    internal static bool UsesFrameHitboxesForProjectiles(RoomEnemySlot enemy) =>
        enemy.CollidesByFrameHitboxes &&
        (Capabilities(enemy) & EnemyCapabilities.ProjectileFrameHitboxes) != EnemyCapabilities.None;

    /// <summary>The bound occupant's collision capabilities; an empty slot has none.</summary>
    private static EnemyCapabilities Capabilities(RoomEnemySlot enemy) =>
        enemy.Entity?.Capabilities ?? EnemyCapabilities.None;

    /// <summary>
    /// Handles Samus touching one already-overlapping ordinary actor. An ordinary touch, with no
    /// attacking contact, damages Samus; Speed Booster, shinespark, Screw Attack, and pseudo-Screw instead read
    /// their own entries from the enemy's vulnerabilities and damage the actor.
    /// </summary>
    internal void ResolveNormalEnemyTouch(
        RoomEnemySlot enemy,
        SamusState samus,
        TouchDeathAnimation deathAnimation = TouchDeathAnimation.Play)
    {
        SamusContactDamageIndex contactDamageIndex = samus.HorizontalSpeed.ContactDamageIndex;
        int baseDamage;
        EnemyContactVulnerability contact = EnemyVulnerabilities.Of(enemy.Definition).Contact;
        EnemyWeaponVulnerability vulnerability;
        switch (contactDamageIndex)
        {
            case SamusContactDamageIndex.None:
                ApplyNormalEnemyTouchDamage(
                    samus,
                    enemy.Definition.Damage,
                    enemy.Position.X.Whole);
                return;
            case SamusContactDamageIndex.SpeedBooster:
                baseDamage = EnemyCollisionDefinitions.SpeedBoosterContactDamage;
                vulnerability = contact.SpeedBooster;
                break;
            case SamusContactDamageIndex.Shinespark:
                baseDamage = EnemyCollisionDefinitions.ShinesparkContactDamage;
                vulnerability = contact.Shinespark;
                break;
            case SamusContactDamageIndex.ScrewAttack:
                baseDamage = EnemyCollisionDefinitions.ScrewAttackContactDamage;
                vulnerability = contact.ScrewAttack;
                break;
            case SamusContactDamageIndex.PseudoScrew:
                // The game consumes charge before checking vulnerability. Even immune
                // contact removes Pseudo Screw's flare; genuine Screw Attack does not.
                SamusProjectileSystem projectiles = _frameContext.SamusProjectiles
                    ?? throw new InvalidOperationException("Pseudo Screw enemy contact requires the live projectile owner.");
                projectiles.Firing.ConsumePseudoScrewCharge(_frameContext.SamusColours
                    ?? throw new InvalidOperationException("Pseudo Screw enemy contact requires Samus's colours."), samus);
                baseDamage = EnemyCollisionDefinitions.PseudoScrewContactDamage;
                vulnerability = contact.PseudoScrew;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusContactDamageIndex)} {contactDamageIndex}.");
        }
        int damage = vulnerability.DamageFrom(baseDamage);
        if (damage == 0)
            return;

        // Touch damage shows the hurt flash alone, unlike projectile and
        // power-bomb paths which add their own visible-flash tail. Samus's timers are also
        // explicitly cleared because this branch represents Samus attacking the enemy.
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
        // hidden two-frame Mother Brain respawn. Do not clear the slot before that tail.
        if (deathAnimation == TouchDeathAnimation.Skip)
            return;

        _deathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: EnemyDeathExplosion.SamusContact);
    }

    /// <summary>Uses the strict radius test, excluding contact where either axis is only tangent.</summary>
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
    /// Exact asymmetric support test for an enemy touching Samus from below.
    /// Horizontal tangency is excluded; vertical tangency after the
    /// three-pixel bias is included. Both Kraid platforms and work robots call it.
    /// </summary>
    internal static bool IsEnemyTouchingSamusFromBelow(RoomEnemySlot enemy, SamusState samus)
    {
        int xDistance = CoordinateAxis.Distance(samus.Position.X.Whole, enemy.Position.X.Whole);
        int horizontalGap = CoordinateAxis.Wrap(xDistance - samus.Kinematics.Radius.X);
        bool horizontalOverlap = xDistance < samus.Kinematics.Radius.X ||
            horizontalGap < enemy.Radius.X;
        if (!horizontalOverlap)
            return false;

        int biasedYDifference = CoordinateAxis.Wrap(samus.Position.Y.Whole + 3 - enemy.Position.Y.Whole);
        if (CoordinateAxis.AsSigned(biasedYDifference) >= 0)
            return false;
        int yDistance = CoordinateAxis.Wrap(-biasedYDifference);
        int verticalGap = CoordinateAxis.Wrap(yDistance - samus.Kinematics.Radius.Y);
        return yDistance < samus.Kinematics.Radius.Y || verticalGap <= enemy.Radius.Y;
    }
}
