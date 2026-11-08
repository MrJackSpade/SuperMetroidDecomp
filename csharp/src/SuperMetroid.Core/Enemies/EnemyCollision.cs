using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Samus-to-enemy interaction: touch, shot, bomb and power-bomb collision walks over the interactive
/// enemies, extended hitboxes, vulnerability and freezing, and the dispatch into the registered reactions.
/// </summary>
[RoomLifetime]
internal sealed class EnemyCollision
{
    /// <summary>Dispatches enemy callbacks selected by touch or projectile contact.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Queues the enemy sounds produced by hits, freezes, and deaths.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Owns ordinary death animations and drops after a lethal collision.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Publishes hit-driven room shake requests.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Supplies the current enemy frame's timing and state to collision callbacks.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Connects collision outcomes to enemy-owned projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies the active room's area and environment to collision logic.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Defines enemy iteration order for the collision passes.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Resizes a power bomb's sweep after each kill it makes.</summary>
    private readonly PowerBombRadiusAfterKill _powerBombRadius;

    /// <summary>Binds the enemy and room owners whose state or callbacks collisions can change.</summary>
    internal EnemyCollision(EnemyCollisionDependencies dependencies)
    {
        _powerBombRadius = dependencies.PowerBombRadius;
        _dispatch = dependencies.Dispatch;
        _audio = dependencies.Audio;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _earthquake = dependencies.Earthquake;
        _frameContext = dependencies.FrameContext;
        _projectiles = dependencies.Projectiles;
        _room = dependencies.Room;
        _slotTable = dependencies.SlotTable;
    }
    /// <summary>The touch reaction <paramref name="slot"/>'s definition names, or null when it names none.</summary>
    internal static EnemyReaction? TouchAi(RoomEnemySlot slot) => slot.Definition.Touch;

    /// <summary>The shot reaction <paramref name="slot"/>'s definition names, or null when it names none.</summary>
    internal static EnemyReaction? ShotAi(RoomEnemySlot slot) => slot.Definition.Shot;

    /// <summary>The Power Bomb reaction <paramref name="slot"/>'s definition names, or null when it names none.</summary>
    internal static EnemyReaction? PowerBombReaction(RoomEnemySlot slot) => slot.Definition.PowerBombReaction;

    /// <summary>Runs the common radius-based Samus/enemy touch pass for translated actors.</summary>
    /// <param name="samus">Live player state receiving the touch reaction.</param>
    /// <param name="controllerInput">Current held input used by touch handlers.</param>
    /// <param name="level">Room terrain required by collision-aware touch handlers.</param>
    /// <param name="onlyEnemy">The enemy EnemyMain is processing, or null for a standalone whole-list audit.</param>
    public bool ResolveOrdinarySamusContact(
        SamusState samus,
        ControllerButton controllerInput,
        RoomLevelData? level = null,
        RoomEnemySlot? onlyEnemy = null)
    {
        Ensure.NotNull(samus);
        _room.EnsureLoaded();
        SamusContactDamageIndex contactDamageIndex = samus.HorizontalSpeed.ContactDamageIndex;
        if (contactDamageIndex == SamusContactDamageIndex.None && !samus.InvincibilityTimer.IsZero)
            return false;

        foreach (RoomEnemySlot slot in _dispatch.InteractiveEnemies)
        {
            if (onlyEnemy is not null && slot != onlyEnemy)
                continue;
            // Collision entry rejects a zero sprite map before resetting Samus's
            // invulnerability. Keep this per enemy: an empty interactive list or an actor
            // not yet publishing its map must not erase the timer. The reset still precedes
            // overlap testing, so a distant actor with a map can legitimately clear it.
            if (slot.Frame is null)
                continue;
            bool usesExtendedHitboxes = UsesExtendedSamusHitboxes(slot);
            // Multibox entry withholds the touch the header excludes from its hitboxes before
            // resetting invulnerability. Ordinary radius entry resets first instead.
            if (usesExtendedHitboxes && slot.Definition.TouchHitboxScan == EnemyHitboxScan.Withheld)
                continue;
            if (contactDamageIndex != SamusContactDamageIndex.None)
                samus.InvincibilityTimer = WrappingCountdown.Start(0);

            // Touch collision does not interpret property 0x0100 as a collision bit;
            // it suppresses rendering only. Hibashi relies on that distinction: its second
            // slot remains invisible while its instruction stream moves a live hitbox.
            // Both Ridley definitions use a hand-authored extended body plus a separately
            // solved tail tip. EnemyMain dispatches its body at this same pre-AI seam;
            // tail processing checks the tip later. Do not flatten either to the header's
            // dummy 8x8 radius here. An actor with no translated touch reaction is skipped.
            if (slot.Entity is IEnemyContactHooks { OwnsSamusTouch: true } ||
                slot.Properties.HasAny(EnemyProperties.Deleted) ||
                (slot.Entity as IEnemyTouchReaction)?.FindTouch() is not { } touch)
            {
                continue;
            }

            bool overlapsSamus;
            EnemyReaction? hitboxTouchAi = TouchAi(slot);
            if (usesExtendedHitboxes)
            {
                overlapsSamus = TryFindExtendedHitboxReaction(
                    slot,
                    samus.Position.Whole,
                    samus.Kinematics.Radius,
                    contactKind: HitboxContactKind.Touch,
                    out hitboxTouchAi);
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
            // AI, except the respawn placeholder. The extended-hitbox path has no such gate.
            if (!usesExtendedHitboxes && slot.FrozenTimer != 0 &&
                slot.DefinitionId != EnemyDefinitionId.Respawn)
            {
                continue;
            }

            touch(new EnemyTouchContext(
                slot,
                samus,
                controllerInput,
                level,
                usesExtendedHitboxes,
                hitboxTouchAi,
                contactDamageIndex));
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs common normal-enemy shot AI for ordinary radius-based translated actors. One
    /// projectile may resolve per enemy per pass, matching the collision-handler exit.
    /// </summary>
    /// <param name="projectiles">Live Samus beam and missile slots whose enemy collisions may consume, reflect, or transition their projectiles.</param>
    /// <param name="sharedProjectiles">Shared bomb-projectile owner passed to projectile impact handling.</param>
    /// <param name="samus">Live player state for enemy-specific shot reactions; may be null only for paths that do not require it, while player-dependent reactions such as Yard beam launch reject its absence.</param>
    /// <param name="onlyEnemy">The enemy EnemyMain is processing, or null for a standalone whole-population audit.</param>
    public int ResolveOrdinaryProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles,
        SamusState? samus = null,
        RoomEnemySlot? onlyEnemy = null)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);
        _room.EnsureLoaded();

        int hitCount = 0;
        foreach (RoomEnemySlot enemy in _dispatch.InteractiveEnemies)
        {
            if (onlyEnemy is not null && enemy != onlyEnemy)
                continue;

            // The shot pass likewise admits visually invisible actors, but rejects the
            // engine's explicit empty frame and property 0x0400. Keeping
            // these meanings separate mirrors the list/collision machinery. An actor
            // with no translated shot reaction is skipped.
            if (enemy.Entity is IEnemyShotAdmission admission && !admission.AdmitsShots() ||
                CommonEnemyEmptyFrames.DrawsNothing(enemy.Frame) ||
                enemy.InvincibilityTimer != 0 ||
                enemy.Properties.HasAny(
                    EnemyProperties.Deleted |
                    EnemyProperties.IgnoreSamusCollision) ||
                (enemy.Entity as IEnemyShotReaction)?.FindShot() is not { } reaction)
            {
                continue;
            }


            if (reaction.SkipsEnemy?.Invoke(enemy) == true)
                continue;

            foreach (SamusProjectileSlot projectile in projectiles.Slots)
            {
                // The slot still holds the live beam or missile when the enemy collision
                // handler selects vulnerability. Starting the impact below deliberately
                // turns the slot into an explosion, so retain the incoming type instead of
                // accidentally asking the vulnerability table about the visual effect.
                SamusProjectileType projectileType = projectile.Type;
                int projectileDamage = Ensure.AtLeastZero(projectile.Damage);
                SamusProjectileDirection projectileDirection = projectile.Direction;
                SamusProjectileFamily family = projectile.Type.Family;
                if (!projectile.HasEnemyCollisionPayload)
                    continue;

                // Bomb, power-bomb, and pseudo-screw actors share the projectile slot array,
                // but ordinary enemy shot AI never interprets them as beam/missile records.
                // Yard and Metroid are the translated exceptions: their private handlers treat
                // bomb-family impacts as physical kicks or escapes.
                if (!reaction.AdmitsBombFamilies &&
                    family is SamusProjectileFamily.PowerBomb or
                        SamusProjectileFamily.Bomb or
                        SamusProjectileFamily.BeamExplosion)
                {
                    continue;
                }

                bool usesExtendedHitboxes = UsesExtendedProjectileHitboxes(enemy);
                // The extended shot walker returns before testing any projectile while the enemy shows
                // the empty extended frame. Its point hitbox remains live for Samus and
                // bombs, whose extended passes have no such check.
                if (usesExtendedHitboxes &&
                    CommonEnemyEmptyFrames.IsFrame(enemy.Frame))
                    continue;
                if (usesExtendedHitboxes &&
                    enemy.Definition.ShotHitboxScan == EnemyHitboxScan.Withheld)
                {
                    // A header that withholds shots from its hitboxes means no overlap, no
                    // Super-Missile quake, and no direction mark even when a visible
                    // authored rectangle would otherwise contain this projectile.
                    continue;
                }

                bool overlapsProjectile;
                EnemyReaction? hitboxShotAi = ShotAi(enemy);
                if (usesExtendedHitboxes)
                {
                    overlapsProjectile = TryFindExtendedHitboxReaction(
                        enemy,
                        projectile.Position.Whole,
                        projectile.Radius,
                        contactKind: HitboxContactKind.Shot,
                        out hitboxShotAi);
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

                bool selectedIgnore = hitboxShotAi == EnemyReaction.Ignore;
                var shot = new EnemyShotContext(
                    enemy,
                    projectile,
                    projectiles,
                    samus,
                    projectileType,
                    projectileDamage,
                    projectileDirection,
                    family,
                    usesExtendedHitboxes,
                    hitboxShotAi)
                {
                    StruckOccupant = enemy.OccupantStates,
                };

                if (RunShotStage(reaction.BeforeSuperMissileQuake, shot))
                {
                    hitCount++;
                    break;
                }

                if (usesExtendedHitboxes && family == SamusProjectileFamily.SuperMissile)
                {
                    // The multibox prelude requests this earthquake before dispatching the
                    // hitbox reaction, even when that reaction is the ignoring shell region.
                    _earthquake.EarthquakeTimer = WrappingCountdown.Start(SuperMissileDefinitions.EarthquakeDuration);
                    _earthquake.EarthquakeType = SuperMissileDefinitions.EnemyHitEarthquakeType;
                }

                if (RunShotStage(reaction.AfterSuperMissileQuake, shot))
                {
                    hitCount++;
                    break;
                }

                if (selectedIgnore)
                {
                    projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(projectile.SlotIndex, shot.MarksCollision);
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
                    // The collision walker marks the direction word, not a
                    // terrain impact. Projectile pre-instructions consume that marker
                    // on the next alpha pass; normal shot AI owns its separate hit graphic.
                    // Super Missiles must retain their handler so it also clears their
                    // collision links. Terrain conversion strands a live link that can
                    // damage the enemy again after its flash timer expires.
                    // Neither the beam nor the missile damage path rejects the removal marker: later
                    // overlapping enemies in this pass still receive the live payload.
                    projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(
                        projectile.SlotIndex,
                        enemy.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) ||
                            !projectile.Type.BeamCombination.HasPlasma);
                }
                else if (!projectiles.EnemyImpacts.TryStartEnemyImpact(sharedProjectiles, projectile.SlotIndex,
                    enemy.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam)))
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
                    FreezeEnemyFromNormalShot(enemy, samus);
                    reaction.AfterCommon?.Invoke(shot with { Froze = true });
                    hitCount++;
                    break;
                }

                int damage = (projectileDamage >> 1) * shotVulnerability.Multiplier;
                if (damage == 0)
                {
                    // A zero-damage hit enters the dud branch even for Plasma: immunity removes
                    // the projectile instead of letting its ordinary piercing rule win.
                    _projectiles.CreateEnemyProjectileDudShot(projectile);
                }
                if (damage != 0)
                {
                    enemy.FlashTimer = enemy.HurtFlashFrames + 8;
                    enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
                    // Common shot AI queues the header cry before subtracting health,
                    // including lethal hits. Frozen targets suppress this cry; death
                    // explosion instructions retain their separate, later sound owner.
                    if (enemy.FrozenTimer == 0 && enemy.Definition.HurtSoundEffect is SoundEffectId cry)
                        _audio.QueueEnemySound(cry, SoundRequestPriority.Low);
                    // Every plasma-family hit gives sixteen invincibility frames.
                    // The packed beam type uses bit three for Plasma; other families do not
                    // set that low bit in their ordinary projectile words.
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
                        // hurt flash/bit two were already installed before the lethal test.
                        FreezeEnemyFromNormalShot(enemy, samus);
                    }
                    else
                    {
                        enemy.Health = lethal
                            ? 0
                            : enemy.Health - damage;
                    }
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
    /// Runs the ordinary-radius half of <c>EnemyBombCollHandler</c>.
    /// </summary>
    /// <remarks>
    /// Normal bombs occupy physical projectile slots five through nine. They are not part
    /// of the five-slot beam/missile walk above: the bomb pass waits for the shared bomb-variable
    /// word to reach zero, marks the overlapping bomb with direction bit 0x10, and dispatches
    /// the enemy's shot reaction with that physical slot selected. The original translation
    /// implemented only Metroid's private detach/recoil reaction, which meant ordinary bombs
    /// could never damage the many enemies that install common shot AI.
    ///
    /// This pass now owns both the common reaction and the already-translated Metroid tail.
    /// Private per-species reactions remain deliberately outside the common branch; they can
    /// be added here as their exact bomb-specific post-processing is translated rather than
    /// silently treating every reaction as the ordinary shot reaction.
    /// </remarks>
    public int ResolveOrdinaryBombHits(
        SamusBombProjectileSystem bombs,
        SamusProjectileSystem ordinaryProjectiles,
        SamusState? samus = null,
        RoomEnemySlot? onlyEnemy = null)
    {
        Ensure.NotNull(bombs);
        Ensure.NotNull(ordinaryProjectiles);
        _room.EnsureLoaded();

        int hitCount = 0;
        for (int ordinal = 0; ordinal < (onlyEnemy is not null ? 1 : _dispatch.InteractiveEnemies.Count); ordinal++)
        {
            RoomEnemySlot enemy = onlyEnemy ?? _dispatch.InteractiveEnemies[ordinal];
            bool usesCommonShotAi = ShotAi(enemy) == EnemyReaction.NormalEnemyShot;
            bool usesIgnoreShotAi = ShotAi(enemy) == EnemyReaction.Ignore;
            bool usesTranslatedPrivateShotAi = IsTranslatedPrivateNormalBombShotAi(enemy, ShotAi(enemy));
            bool usesExtendedHitboxes =
                enemy.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap);
            bool headerSuppressesMultiboxShotCollision = usesExtendedHitboxes &&
                enemy.Definition.ShotHitboxScan == EnemyHitboxScan.Withheld;
            bool admittedByOccupant = enemy.Entity is IEnemyNormalBombAdmission admission &&
                admission.AdmitsNormalBombs(usesExtendedHitboxes);
            if ((!usesCommonShotAi && !usesIgnoreShotAi &&
                    !usesTranslatedPrivateShotAi && !admittedByOccupant) ||
                enemy.Frame is null ||
                enemy.InvincibilityTimer != 0 ||
                enemy.Properties.HasAny(EnemyProperties.Deleted) ||
                headerSuppressesMultiboxShotCollision ||
                usesExtendedHitboxes &&
                    enemy.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
            {
                continue;
            }

            foreach (SamusBombProjectileSlot bomb in bombs.Slots)
            {
                SamusProjectileFamily family = bomb.Type.Family;

                // The ordinary-radius bomb pass admits an exploding normal bomb. The ordinary-radius
                // handler also admits a spread bomb. The extended-frame
                // handler is narrower: it requires a bomb and a nonzero X word.
                if (bomb.Type.IsNone ||
                    !bomb.BombTimer.IsZero ||
                    (usesExtendedHitboxes
                        ? family != SamusProjectileFamily.Bomb ||
                          bomb.Position.X.Whole == 0
                        : family != SamusProjectileFamily.Bomb &&
                          !bomb.IsBombSpread))
                {
                    continue;
                }

                EnemyReaction? selectedShotAi = ShotAi(enemy);
                bool overlaps = usesExtendedHitboxes
                    ? TryFindExtendedHitboxReaction(
                        enemy,
                        bomb.Position.Whole,
                        bomb.Radius,
                        contactKind: HitboxContactKind.Shot,
                        out selectedShotAi)
                    : RadiusBoxesOverlap(
                        enemy.Position.Whole,
                        enemy.Radius,
                        bomb.Position.Whole,
                        bomb.Radius);
                if (!overlaps)
                {
                    continue;
                }

                // Multibox actors dispatch the reaction stored in the selected hitbox,
                // not necessarily the definition header's. Do not turn a protected/private
                // component into common damage merely because the header names the ordinary shot.
                bool selectedIgnore = selectedShotAi == EnemyReaction.Ignore;
                bool selectedTranslatedPrivate = IsTranslatedPrivateNormalBombShotAi(enemy, selectedShotAi);
                if (selectedShotAi != EnemyReaction.NormalEnemyShot &&
                    !selectedIgnore &&
                    !selectedTranslatedPrivate)
                    continue;

                // EnemyBombCollHandler marks the physical bomb before dispatch. Unlike an
                // ordinary beam impact, the bomb is already running its explosion program;
                // common shot AI must not replace it with an impact animation.
                bomb.IsSpent = true;
                var bombContext = new EnemyBombContext(
                    enemy,
                    bomb,
                    ordinaryProjectiles,
                    samus,
                    selectedShotAi,
                    usesExtendedHitboxes)
                {
                    StruckOccupant = enemy.OccupantStates,
                };
                var bombReactions = enemy.Entity as IEnemyBombReaction;
                if (bombReactions?.FindBombSpecial(bombContext) is { } special)
                {
                    special(bombContext);
                }
                else if (!selectedIgnore)
                {
                    if (bombReactions?.FindBombSpecialAfterNoOp(bombContext) is { } afterNoOp)
                    {
                        afterNoOp(bombContext);
                    }
                    else
                    {
                        // A private reaction that rejects the bomb returns before common damage; the bomb pass has
                        // already marked the bomb, so rejection belongs here rather than in collision admission.
                        EnemyBombReaction? reaction = bombReactions?.FindBombCommon(bombContext);
                        if (reaction?.Rejects?.Invoke(bombContext) != true)
                        {
                            bombContext = bombContext with { HealthBefore = enemy.Health };
                            reaction?.BeforeCommon?.Invoke(bombContext);
                            ApplyCommonNormalBombDamage(enemy, bomb, runGenericDeath: reaction?.RunsGenericDeath ?? true);
                            reaction?.AfterCommon?.Invoke(bombContext);
                        }
                    }
                }

                hitCount++;
                break;
            }
        }

        return hitCount;
    }

    /// <summary>
    /// Reports the private shot reactions whose normal-bomb path is translated. The occupant decides, because an extended
    /// hitbox may deliberately override its definition header with a different reaction.
    /// </summary>
    private static bool IsTranslatedPrivateNormalBombShotAi(
        RoomEnemySlot enemy,
        EnemyReaction? reaction) =>
        enemy.Entity is IEnemyPrivateShotReaction owner &&
        owner.OwnsPrivateShotReaction(reaction);

    /// <summary>
    /// Executes the family-0x0500 branch of the shot AI that skips the death animation,
    /// followed by the ordinary shot AI's death tail.
    /// </summary>
    internal void ApplyCommonNormalBombDamage(
        RoomEnemySlot enemy,
        SamusBombProjectileSlot bomb,
        bool runGenericDeath = true)
    {
        EnemyWeaponVulnerability vulnerability = EnemyVulnerabilities.Of(enemy.Definition).Ordnance.Bomb;
        int damage = (bomb.Damage >> 1) * vulnerability.Multiplier;

        // A zero multiplier still consumes the collision and leaves the hit direction on
        // the bomb. Emits a dud sprite/sound here; those presentation queues are not
        // yet shared by the host bomb system, so enemy state correctly remains untouched.
        if (damage == 0)
            return;

        enemy.FlashTimer = enemy.HurtFlashFrames + 8;
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
        enemy.Health = damage >= enemy.Health
            ? 0
            : enemy.Health - damage;
        if (enemy.Health != 0 || !runGenericDeath)
            return;

        _deathsAndDrops.StartGenericEnemyDeath(
            enemy,
            EnemyDeathsAndDrops.SelectNormalShotDeathAnimation(enemy, SamusBombProjectileSystem.NormalBombType));
    }

    /// <summary>
    /// Runs the power-bomb interaction for one expansion
    /// sample. The caller supplies the high byte of the live power-bomb radius; the game
    /// uses it as the horizontal radius and derives a three-quarter-height vertical ellipse.
    /// </summary>
    public int ResolveOrdinaryPowerBombHits(
        Position16<RoomPixels> explosionCenter,
        int explosionRadius,
        SamusState? samus = null)
    {
        _room.EnsureLoaded();
        if (explosionRadius == 0)
            return 0;

        int horizontalRadius = explosionRadius;
        // The vertical radius keeps the carry from the first halving through the addition. Expressing each
        // step makes odd-radius rounding agree with the game rather than using 0.75f.
        int carry = horizontalRadius & 1;
        int verticalRadius = ((horizontalRadius >> 1) + horizontalRadius + carry) >> 1;
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
            if (enemy.DefinitionId is EnemyDefinitionId.None or EnemyDefinitionId.Respawn ||
                enemy.InvincibilityTimer != 0 ||
                enemy.Properties.HasAny(EnemyProperties.Deleted))
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

            EnemyPowerBombReaction? reaction = (enemy.Entity as IEnemyPowerBombReaction)?.FindPowerBomb();
            if (reaction?.SkipsEnemy?.Invoke(enemy) == true)
                continue;
            if (enemy.Definition.PowerBombReaction is { } named && reaction is null)
            {
                throw new InvalidDataException(
                    $"Enemy {enemy.DefinitionId} power-bomb reaction {named} is not translated.");
            }

            // Private callbacks replace common power-bomb damage, but still receive the
            // process-off-screen bit after a qualifying ellipse overlap.
            if (reaction?.ReplacesCommon is { } privateReaction)
            {
                privateReaction(enemy, samus);
                enemy.Properties = enemy.Properties.With(EnemyProperties.ProcessOffScreen);
                reactionCount++;
                continue;
            }

            // The common reaction damages the enemy; the process-off-screen property follows.
            int damage = 100 * vulnerability.Multiplier;
            enemy.InvincibilityTimer = 48;
            enemy.FlashTimer = enemy.HurtFlashFrames + 8;
            enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
            enemy.Health = damage >= enemy.Health
                ? 0
                : enemy.Health - damage;
            if (enemy.Health == 0 && reaction?.ExemptFromGenericDeath != true)
            {
                reaction?.BeforeGenericDeath?.Invoke(enemy, samus);
                if (reaction?.ReplacesGenericDeath is { } replaceDeath)
                    replaceDeath(enemy, samus);
                else
                    _deathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: EnemyDeathExplosion.Small);
            }

            reaction?.AfterCommon?.Invoke(enemy, samus);

            enemy.Properties = enemy.Properties.With(EnemyProperties.ProcessOffScreen);
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
    /// Installs the common frozen handler. Norfair (area two) uses 300 frames; every other
    /// retail area uses 400. The ten-frame invincibility word is shared by direct authored
    /// freezing and the lethal-Ice substitution path.
    /// </summary>
    internal void FreezeEnemyFromNormalShot(RoomEnemySlot enemy, SamusState? samus)
    {
        // The direct-freeze vulnerability refreshes the timer even on an already frozen
        // actor, but only a newly frozen actor queues audio. Lethal-Ice substitution
        // reaches this helper only with an initially zero clock.
        if (enemy.FrozenTimer == 0)
            _audio.QueueEnemySound(Library3Sound.EnemyFrozen, priority: SoundRequestPriority.Low);
        enemy.FrozenTimer = samus?.LiquidPhysics.AreaIndex == AreaId.Norfair
            ? 300
            : 400;
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Frozen;
        enemy.InvincibilityTimer = 10;
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
    /// Damage multiplier and freezing behavior selected from an ordinary shot's vulnerability byte. A lethal Ice-bearing
    /// beam freezes the enemy instead of killing it unless <see cref="PreventsLethalFreeze"/>, which the beam entry
    /// decides; non-beam families never prevent it.
    /// </summary>
    [IndependentFlag("independent facts of the vulnerability byte: immediate freeze, and lethal-freeze prevention")]
    internal readonly record struct NormalShotVulnerability(
        int Multiplier,
        bool FreezeImmediately,
        bool PreventsLethalFreeze);

    /// <summary>
    /// Walks the extended-frame structure and returns the
    /// reaction belonging to the first overlapping hitbox. One displayed enemy frame may
    /// contain several independently offset ordinary spritemaps; every component has a
    /// hitbox list whose records are signed bounds plus touch and shot reactions.
    /// </summary>
    internal bool TryFindExtendedHitboxReaction(
        RoomEnemySlot enemy,
        Position16<RoomPixels> targetPosition,
        Extent16<RoomPixels> targetRadius,
        HitboxContactKind contactKind,
        out EnemyReaction? reaction)
    {
        reaction = null;

        // Multibox collision walks every frame an actor shows, and only an actor that has
        // shown none is skipped. The common visually empty extended frame still owns a point
        // hitbox, so it enters the same collision walker as other extended frames.
        if (enemy.Frame is null)
            return false;

        var target = new EnemyHitboxProbe(
            TopLeft: new(
                CoordinateAxis.Wrap(targetPosition.X - targetRadius.X),
                CoordinateAxis.Wrap(targetPosition.Y - targetRadius.Y)),
            BottomRight: new(
                CoordinateAxis.Wrap(targetPosition.X + targetRadius.X),
                CoordinateAxis.Wrap(targetPosition.Y + targetRadius.Y)),
            SelectsShot: contactKind == HitboxContactKind.Shot,
            ArtworkInstalled: _room.Artwork is not null);

        if (CommonEnemyEmptyFrames.IsFrame(enemy.Frame))
        {
            // The shared empty frame has a single zero-radius point at the
            // enemy origin. Run the same asymmetric overlap test before
            // selecting its common touch/shot reaction.
            if (!target.Overlaps(enemy.Position.X.Whole, enemy.Position.Y.Whole, enemy.Position.X.Whole, enemy.Position.Y.Whole))
                return false;
            reaction = contactKind == HitboxContactKind.Shot ? EnemyReaction.NormalEnemyShot : EnemyReaction.NormalEnemyTouch;
            return true;
        }

        // A species with engine-owned geometry answers for its own frames; null means none covers this frame.
        if (enemy.Entity is IEnemyExtendedHitboxes owner && owner.FindHitboxReaction(target, out reaction) is { } owned)
            return owned;

        throw new InvalidDataException(
            $"Enemy {enemy.DefinitionId} extended collision frame " +
            $"{enemy.Frame} has no compiled geometry.");
    }

    /// <summary>
    /// Identifies actors whose ordinary Samus-contact collision actually enters
    /// the extended-frame touch walker. This differs deliberately from the shot
    /// set: Phantoon uses authored component rectangles for touch, while its weapon collision
    /// is owned by the encounter-specific projectile resolver.
    /// </summary>
    internal static bool UsesExtendedSamusHitboxes(RoomEnemySlot enemy) =>
        enemy.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap) &&
        (Capabilities(enemy) & EnemyCapabilities.ExtendedSamusHitboxes) != EnemyCapabilities.None;

    /// <summary>
    /// Identifies actors whose ordinary projectile collision actually enters
    /// the extended-frame shot walker. Extra-property bit 0x0004 alone is not
    /// sufficient: Mother Brain and several custom-drawn actors set it for scheduling or
    /// rendering while their shot paths use a header radius or encounter-specific geometry.
    /// Keeping this predicate beside the dispatcher gives data-backed probes the same boundary
    /// as gameplay and prevents them from interpreting unrelated component metadata as a
    /// hitbox pointer.
    /// </summary>
    internal static bool UsesExtendedProjectileHitboxes(RoomEnemySlot enemy) =>
        enemy.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap) &&
        (Capabilities(enemy) & EnemyCapabilities.ExtendedProjectileHitboxes) != EnemyCapabilities.None;

    /// <summary>The bound occupant's collision capabilities; an empty slot has none.</summary>
    private static EnemyCapabilities Capabilities(RoomEnemySlot enemy) =>
        enemy.Entity?.Capabilities ?? EnemyCapabilities.None;

    /// <summary>
    /// Handles Samus touching one already-overlapping ordinary actor. A zero contact index
    /// damages Samus; Speed Booster, shinespark, Screw Attack, and pseudo-Screw instead read
    /// their dedicated bytes from the enemy's vulnerability record and damage the actor.
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
                baseDamage = 500;
                vulnerability = contact.SpeedBooster;
                break;
            case SamusContactDamageIndex.Shinespark:
                baseDamage = 300;
                vulnerability = contact.Shinespark;
                break;
            case SamusContactDamageIndex.ScrewAttack:
                baseDamage = 2000;
                vulnerability = contact.ScrewAttack;
                break;
            case SamusContactDamageIndex.PseudoScrew:
                // The game consumes charge before checking vulnerability. Even immune
                // contact removes Pseudo Screw's flare; genuine Screw Attack does not.
                SamusProjectileSystem projectiles = _frameContext.SamusProjectiles
                    ?? throw new InvalidOperationException("Pseudo Screw enemy contact requires the live projectile owner.");
                projectiles.Firing.ConsumePseudoScrewCharge(_frameContext.SamusColours
                    ?? throw new InvalidOperationException("Pseudo Screw enemy contact requires Samus's colours."), samus);
                baseDamage = 200;
                vulnerability = contact.PseudoScrew;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusContactDamageIndex)} {contactDamageIndex}.");
        }
        int damage = (baseDamage >> 1) * vulnerability.Multiplier;
        if (damage == 0)
            return;

        // Touch damage uses the raw hurt-AI duration (default four), unlike projectile and
        // power-bomb paths which add their own visible-flash tail. Samus's timers are also
        // explicitly cleared because this branch represents Samus attacking the enemy.
        enemy.FlashTimer = enemy.HurtFlashFrames;
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackTimer = 0;
        enemy.Health = damage >= enemy.Health
            ? 0
            : enemy.Health - damage;
        _audio.QueueEnemySound(Library2Sound.ContactKill, priority: SoundRequestPriority.WhenIdle);
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
        // The suit's damage division quarters the source header's damage for Gravity or halves
        // it for Varia before normal touch publishes the invincibility/hurt request.
        int damage = SamusSuitDamage.Reduce(damageBeforeSuit, samus.EquippedItems);
        samus.Health = samus.Health <= damage
            ? 0
            : samus.Health - damage;
        samus.InvincibilityTimer = WrappingCountdown.Start(96);
        KnockbackHorizontalDirection direction = samus.Position.X.Whole >= damageSourceX ? KnockbackHorizontalDirection.Right : KnockbackHorizontalDirection.Left;
        // The contact publishes a hit request; Samus admits it only when no
        // knockback direction is installed. Damage can recur after invincibility
        // expires even if the old movement handler remains active (e.g. a boss grab).
        samus.KnockbackTimer = 5;
        samus.KnockbackXDirection = direction;
        // EnemyMain only publishes the request. The common post-animation
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
