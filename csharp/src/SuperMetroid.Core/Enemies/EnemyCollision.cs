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
    /// <summary>Dispatches bank-local enemy callbacks selected by touch or projectile contact.</summary>
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
    /// <summary>Defines native enemy iteration order for the collision passes.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Binds the enemy and room owners whose state or callbacks collisions can change.</summary>
    internal EnemyCollision(EnemyCollisionDependencies dependencies)
    {
        _dispatch = dependencies.Dispatch;
        _audio = dependencies.Audio;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _earthquake = dependencies.Earthquake;
        _frameContext = dependencies.FrameContext;
        _projectiles = dependencies.Projectiles;
        _room = dependencies.Room;
        _slotTable = dependencies.SlotTable;
    }
    /// <summary>The named header touch callback (+$30) of <paramref name="slot"/>'s definition, or null.</summary>
    internal static EnemyInteractionCallback? TouchAi(RoomEnemySlot slot) => slot.Definition.Touch;

    /// <summary>The named header shot callback (+$32) of <paramref name="slot"/>'s definition, or null.</summary>
    internal static EnemyInteractionCallback? ShotAi(RoomEnemySlot slot) => slot.Definition.Shot;

    /// <summary>The named header Power Bomb reaction (+$28) of <paramref name="slot"/>'s definition, or null.</summary>
    internal static EnemyInteractionCallback? PowerBombReaction(RoomEnemySlot slot) => slot.Definition.PowerBombReaction;

    /// <summary>Runs the common radius-based Samus/enemy touch pass for translated actors.</summary>
    /// <param name="samus">Live player state receiving the native touch callback.</param>
    /// <param name="controllerInput">Current held input used by touch handlers.</param>
    /// <param name="level">Room terrain required by collision-aware touch handlers.</param>
    /// <param name="onlyEnemyIndex">EnemyMain's current physical index, or null for a standalone whole-list audit.</param>
    public bool ResolveOrdinarySamusContact(
        SamusState samus,
        ControllerButton controllerInput,
        RoomLevelData? level = null,
        int? onlyEnemyIndex = null)
    {
        Ensure.NotNull(samus);
        _room.EnsureLoaded();
        SamusContactDamageIndex contactDamageIndex = samus.HorizontalSpeed.ContactDamageIndex;
        if (contactDamageIndex == SamusContactDamageIndex.None && !samus.InvincibilityTimer.IsZero)
            return false;

        foreach (int enemyIndex in _dispatch.InteractiveEnemyIndexes)
        {
            if (onlyEnemyIndex.HasValue && enemyIndex != onlyEnemyIndex.Value)
                continue;
            RoomEnemySlot slot = _slotTable.SlotAtEnemyIndex(enemyIndex);
            // Native collision entry rejects a zero sprite map before resetting Samus's
            // invulnerability. Keep this per enemy: an empty interactive list or an actor
            // not yet publishing its map must not erase the timer. The reset still precedes
            // overlap testing, so a distant actor with a map can legitimately clear it.
            if (slot.Frame is null)
                continue;
            bool usesExtendedHitboxes = UsesExtendedSamusHitboxes(slot);
            // Multibox entry rejects the two shared no-op callback identities before
            // resetting invulnerability. Ordinary radius entry resets first instead.
            if (usesExtendedHitboxes && TouchAi(slot) is
                EnemyInteractionCallback.NoOp or EnemyInteractionCallback.NoOpShortReturn)
                continue;
            if (contactDamageIndex != SamusContactDamageIndex.None)
                samus.InvincibilityTimer = WrappingCountdown.Start(0);

            // Native touch collision does not interpret property $0100 as a collision bit;
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
            EnemyInteractionCallback? hitboxTouchAi = TouchAi(slot);
            if (usesExtendedHitboxes)
            {
                overlapsSamus = TryFindExtendedHitboxCallback(
                    slot,
                    samus.Position.Whole,
                    samus.Kinematics.Radius,
                    callbackKind: HitboxCallbackKind.Touch,
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

            // $A0:A10E-A11F: on the ordinary radius path a frozen enemy never runs its touch
            // AI, except the respawn placeholder. The extended-hitbox path has no such gate.
            if (!usesExtendedHitboxes && !slot.FrozenTimer.IsZero &&
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
    /// projectile may resolve per enemy per pass, matching the native collision-handler exit.
    /// </summary>
    /// <param name="projectiles">Live Samus beam and missile slots whose enemy collisions may consume, reflect, or transition their projectiles.</param>
    /// <param name="sharedProjectiles">Shared bomb-projectile owner passed to projectile impact handling.</param>
    /// <param name="samus">Live player state for enemy-specific shot reactions; may be null only for paths that do not require it, while player-dependent reactions such as Yard beam launch reject its absence.</param>
    /// <param name="onlyEnemyIndex">Current EnemyMain slot, or null for a standalone whole-population audit.</param>
    public int ResolveOrdinaryProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles,
        SamusState? samus = null,
        int? onlyEnemyIndex = null)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);
        _room.EnsureLoaded();

        int hitCount = 0;
        foreach (int enemyIndex in _dispatch.InteractiveEnemyIndexes)
        {
            if (onlyEnemyIndex.HasValue && enemyIndex != onlyEnemyIndex.Value)
                continue;
            RoomEnemySlot enemy = _slotTable.SlotAtEnemyIndex(enemyIndex);

            // The shot pass likewise admits visually invisible actors, but rejects the
            // engine's explicit empty spritemap sentinel $804D and property $0400. Keeping
            // these meanings separate mirrors the bank-$A0 list/collision machinery. An actor
            // with no translated shot reaction is skipped.
            if (enemy.Entity is IEnemyShotAdmission admission && !admission.AdmitsShots() ||
                CommonEnemyEmptyFrames.DrawsNothing(enemy.Frame) ||
                !enemy.InvincibilityTimer.IsZero ||
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
                // `$0C18,x` is still the live beam/missile type when the enemy collision
                // handler selects vulnerability. Starting the impact below deliberately
                // rewrites that slot to explosion family `$0700`, so retain the incoming
                // word instead of accidentally asking the vulnerability table about the
                // newly-created visual effect.
                SamusProjectileTypeWord projectileType = projectile.PackedType;
                int projectileDamage = checked((ushort)projectile.Damage);
                SamusProjectileDirectionWord projectileDirection = projectile.PackedDirection;
                SamusProjectileFamily family = projectile.PackedType.Family;
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
                // `$A0:9BA6` returns before testing any projectile while the enemy shows
                // the empty extended frame. Its point hitbox remains live for Samus and
                // bombs, whose extended passes have no such check.
                if (usesExtendedHitboxes &&
                    CommonEnemyEmptyFrames.IsFrame(enemy.Frame))
                    continue;
                if (usesExtendedHitboxes &&
                    IsCanonicalMultiboxNoOpAi(ShotAi(enemy)))
                {
                    // `EprojCollHandler_Multibox` compares the header callback before
                    // reading the extended map. Canonical RTS/RTL means no overlap, no
                    // Super-Missile quake, and no direction mark even when a visible
                    // authored rectangle would otherwise contain this projectile.
                    continue;
                }

                bool overlapsProjectile;
                EnemyInteractionCallback? hitboxShotAi = ShotAi(enemy);
                if (usesExtendedHitboxes)
                {
                    overlapsProjectile = TryFindExtendedHitboxCallback(
                        enemy,
                        projectile.Position.Whole,
                        projectile.Radius,
                        callbackKind: HitboxCallbackKind.Shot,
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

                bool selectedLiteralNoOpShotAi = IsLiteralNoOpShotAi(enemy.Definition, hitboxShotAi);
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
                    hitboxShotAi);

                if (RunShotStage(reaction.BeforeSuperMissileQuake, shot))
                {
                    hitCount++;
                    break;
                }

                if (usesExtendedHitboxes && family == SamusProjectileFamily.SuperMissile)
                {
                    // The multibox prelude requests this earthquake before dispatching the
                    // hitbox callback, even when that callback is the no-op shell region.
                    _earthquake.EarthquakeTimer = WrappingCountdown.Start(SuperMissileDefinitions.EarthquakeDuration);
                    _earthquake.EarthquakeType = SuperMissileDefinitions.EnemyHitEarthquakeType;
                }

                if (RunShotStage(reaction.AfterSuperMissileQuake, shot))
                {
                    hitCount++;
                    break;
                }

                if (selectedLiteralNoOpShotAi)
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
                    // The bank-$A0 collision walker marks the direction word, not a
                    // terrain impact. Projectile pre-instructions consume that marker
                    // on the next alpha pass; normal shot AI owns its separate hit graphic.
                    // Super Missiles must retain their handler so it also clears their
                    // collision links. Terrain conversion strands a live link that can
                    // damage the enemy again after its flash timer expires.
                    // $A0:A184 and $A0:9C39 do not reject the removal marker: later
                    // overlapping enemies in this pass still receive the live payload.
                    projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(
                        projectile.SlotIndex,
                        enemy.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) ||
                            !projectile.PackedType.BeamCombination.HasPlasma);
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
                    // $A0:A75B enters the dud branch even for Plasma: immunity removes
                    // the projectile instead of letting its ordinary piercing rule win.
                    _projectiles.CreateEnemyProjectileDudShot(projectile);
                }
                if (damage != 0)
                {
                    enemy.FlashTimer = WrappingCountdown.Start(enemy.HurtFlashFrames + 8);
                    enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
                    // Common shot AI queues the header cry before subtracting health,
                    // including lethal hits. Frozen targets suppress this cry; death
                    // explosion instructions retain their separate, later sound owner.
                    if (enemy.FrozenTimer.IsZero && enemy.Definition.HurtSoundEffect is SoundEffectId cry)
                        _audio.QueueEnemySound(cry, SoundQueueLimit.Max3);
                    // `$A0:A79E` gives every plasma-family hit sixteen invincibility frames.
                    // The packed beam type uses bit three for Plasma; other families do not
                    // set that low bit in their ordinary projectile words.
                    if (projectileType.BeamCombination.HasPlasma)
                        enemy.InvincibilityTimer = WrappingCountdown.Start(EnemyShotTiming.PlasmaInvincibilityFrames);

                    bool lethal = damage >= enemy.Health;
                    bool freezeInsteadOfKilling = lethal &&
                        projectileType.BeamCombination.HasIce &&
                        !shotVulnerability.PreventsLethalFreeze &&
                        enemy.FrozenTimer.IsZero;
                    if (freezeInsteadOfKilling)
                    {
                        // A lethal Ice-bearing beam leaves health unchanged and freezes the
                        // actor. This is not the same path as an authored `$FF` entry above:
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
    /// Ports the ordinary-radius half of <c>EnemyBombCollHandler</c> at $A0:A236.
    /// </summary>
    /// <remarks>
    /// Normal bombs occupy physical projectile slots five through nine. They are not part
    /// of the five-slot beam/missile walk above: bank $A0 waits for the shared bomb-variable
    /// word to reach zero, marks the overlapping bomb with direction bit $10, and dispatches
    /// the enemy's shot callback with that physical slot selected. The original translation
    /// implemented only Metroid's private detach/recoil callback, which meant ordinary bombs
    /// could never damage the many enemies that install common shot AI.
    ///
    /// This pass now owns both the common callback and the already-translated Metroid tail.
    /// Private per-species callbacks remain deliberately outside the common branch; they can
    /// be added here as their exact bomb-specific post-processing is translated rather than
    /// silently treating every callback as <c>$A0:802D</c>.
    /// </remarks>
    public int ResolveOrdinaryBombHits(
        SamusBombProjectileSystem bombs,
        SamusProjectileSystem ordinaryProjectiles,
        SamusState? samus = null,
        int? onlyEnemyIndex = null)
    {
        Ensure.NotNull(bombs);
        Ensure.NotNull(ordinaryProjectiles);
        _room.EnsureLoaded();

        int hitCount = 0;
        for (int ordinal = 0; ordinal < (onlyEnemyIndex.HasValue ? 1 : _dispatch.InteractiveEnemyIndexes.Count); ordinal++)
        {
            int enemyIndex = onlyEnemyIndex ?? _dispatch.InteractiveEnemyIndexes[ordinal];
            RoomEnemySlot enemy = _slotTable.SlotAtEnemyIndex(enemyIndex);
            bool usesCommonShotAi = ShotAi(enemy) == EnemyInteractionCallback.NormalEnemyShot;
            bool usesLiteralNoOpShotAi = IsLiteralNoOpShotAi(enemy.Definition, ShotAi(enemy));
            bool usesTranslatedPrivateShotAi = IsTranslatedPrivateNormalBombShotAi(enemy, ShotAi(enemy));
            bool usesExtendedHitboxes =
                enemy.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap);
            bool headerSuppressesMultiboxShotCollision = usesExtendedHitboxes &&
                IsCanonicalMultiboxNoOpAi(ShotAi(enemy));
            bool admittedByOccupant = enemy.Entity is IEnemyNormalBombAdmission admission &&
                admission.AdmitsNormalBombs(usesExtendedHitboxes);
            if ((!usesCommonShotAi && !usesLiteralNoOpShotAi &&
                    !usesTranslatedPrivateShotAi && !admittedByOccupant) ||
                enemy.Frame is null ||
                !enemy.InvincibilityTimer.IsZero ||
                enemy.Properties.HasAny(EnemyProperties.Deleted) ||
                headerSuppressesMultiboxShotCollision ||
                usesExtendedHitboxes &&
                    enemy.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
            {
                continue;
            }

            foreach (SamusBombProjectileSlot bomb in bombs.Slots)
            {
                SamusProjectileFamily family = bomb.PackedType.Family;

                // `$A0:A24E-$A265` admits an exploding family-$0500 normal bomb. The
                // ordinary-radius handler also retains the high-bit alternative used by
                // reflected bomb-like actors. `$A0:9D23`'s extended-spritemap handler is
                // narrower: it requires exactly family $0500 and a nonzero X word.
                if (bomb.Type == 0 ||
                    !bomb.BombTimer.IsZero ||
                    (usesExtendedHitboxes
                        ? family != SamusProjectileFamily.Bomb ||
                          bomb.Position.X.Whole == 0
                        : family != SamusProjectileFamily.Bomb &&
                          (bomb.Type & SamusProjectileTypeWord.LiveMarker) == 0))
                {
                    continue;
                }

                EnemyInteractionCallback? selectedShotAi = ShotAi(enemy);
                bool overlaps = usesExtendedHitboxes
                    ? TryFindExtendedHitboxCallback(
                        enemy,
                        bomb.Position.Whole,
                        bomb.Radius,
                        callbackKind: HitboxCallbackKind.Shot,
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

                // Multibox actors dispatch the callback stored in the selected hitbox,
                // not necessarily the definition header. Do not turn a protected/private
                // component into common damage merely because the header uses `$802D`.
                bool selectedLiteralNoOp = IsLiteralNoOpShotAi(enemy.Definition, selectedShotAi);
                bool selectedTranslatedPrivate = IsTranslatedPrivateNormalBombShotAi(enemy, selectedShotAi);
                if (selectedShotAi != EnemyInteractionCallback.NormalEnemyShot &&
                    !selectedLiteralNoOp &&
                    !selectedTranslatedPrivate)
                    continue;

                // EnemyBombCollHandler marks the physical bomb before dispatch. Unlike an
                // ordinary beam impact, the bomb is already running its explosion program;
                // common shot AI must not replace it with a bank-$93 impact animation.
                bomb.Direction = unchecked((ushort)(bomb.Direction | SamusProjectileDirectionMasks.Spent));
                var bombContext = new EnemyBombContext(
                    enemy,
                    bomb,
                    ordinaryProjectiles,
                    samus,
                    selectedShotAi,
                    usesExtendedHitboxes);
                var bombReactions = enemy.Entity as IEnemyBombReaction;
                if (bombReactions?.FindBombSpecial(bombContext) is { } special)
                {
                    special(bombContext);
                }
                else if (!selectedLiteralNoOp)
                {
                    if (bombReactions?.FindBombSpecialAfterNoOp(bombContext) is { } afterNoOp)
                    {
                        afterNoOp(bombContext);
                    }
                    else
                    {
                        // A private callback that rejects the bomb returns before common damage; bank $A0 has
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

    /// <summary>Checks whether an enemy callback, run in <paramref name="definition"/>'s code, returns at once.</summary>
    internal static bool IsLiteralNoOpShotAi(RoomEnemyDefinition definition, EnemyInteractionCallback? callback) =>
        callback is EnemyInteractionCallback.CeresDoorNoOpShot or EnemyInteractionCallback.KraidNoOpShot or EnemyInteractionCallback.ChozoNoOpShot ||
        callback == EnemyInteractionCallback.NoOp && definition.NoOpCallbackBody == EnemyCallbackBody.ImmediateReturn;

    /// <summary>
    /// Tests the two engine-owned no-op callback addresses checked directly by
    /// <c>EprojCollHandler_Multibox</c> and <c>EnemyBombCollHandler_Multibox</c> before either
    /// routine reads an extended spritemap. A bank-local routine that merely contains the
    /// same RTS/RTL opcode is deliberately not equivalent: Kraid's `$A7:94B5` proves that
    /// such a header still scans components and may select `$A7:94B6` from an arm hitbox.
    /// </summary>
    internal static bool IsCanonicalMultiboxNoOpAi(EnemyInteractionCallback? callback) =>
        callback is EnemyInteractionCallback.NoOp or EnemyInteractionCallback.NoOpShortReturn;

    /// <summary>
    /// Reports the bank-$A2/$A3 private shot callbacks whose family-$0500 path is translated.
    /// Definition and callback are checked together because identical 16-bit addresses in
    /// different enemy banks are unrelated native routines, while an extended hitbox may
    /// deliberately override its definition header with a different callback.
    /// </summary>
    private static bool IsTranslatedPrivateNormalBombShotAi(
        RoomEnemySlot enemy,
        EnemyInteractionCallback? callback) =>
        enemy.Entity is IEnemyPrivateShotCallback owner &&
        owner.OwnsPrivateShotCallback(callback);

    /// <summary>
    /// Executes the family-$0500 branch of <c>NormalEnemyShotAiSkipDeathAnim</c> at
    /// $A0:A6DE, followed by <c>NormalEnemyShotAi</c>'s ordinary death tail.
    /// </summary>
    internal void ApplyCommonNormalBombDamage(
        RoomEnemySlot enemy,
        SamusBombProjectileSlot bomb,
        bool runGenericDeath = true)
    {
        EnemyVulnerabilityTable table = enemy.Definition.Vulnerability;
        int vulnerability = EnemyVulnerabilityEntries.Read(
            table, EnemyVulnerabilityDefinitions.BombOffset);
        int damage = (bomb.Damage >> 1) * EnemyVulnerabilityEntries.Multiplier(vulnerability);

        // A zero multiplier still consumes the collision and leaves direction bit $10 on
        // the bomb. Native emits a dud sprite/sound here; those presentation queues are not
        // yet shared by the host bomb system, so enemy state correctly remains untouched.
        if (damage == 0)
            return;

        enemy.FlashTimer = WrappingCountdown.Start(enemy.HurtFlashFrames + 8);
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
    /// Ports <c>Process_Enemy_PowerBomb_Interaction</c> at $A0:A306 for one expansion
    /// sample. The caller supplies the high byte of the live power-bomb radius; native code
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
        // `$A0:A31A-$A31D` retains carry from the first LSR through ADC. Expressing each
        // step makes odd-radius rounding agree with the 65C816 rather than using 0.75f.
        int carry = horizontalRadius & 1;
        int verticalRadius = ((horizontalRadius >> 1) + horizontalRadius + carry) >> 1;
        int reactionCount = 0;
        _deathsAndDrops.ClearRespawnScratch();

        // The native pass walks all 32 physical slots from $07C0 down to zero, independent
        // of the ordinary active list. That matters while a power bomb reaches off-screen
        // actors and then sets their process-off-screen property below.
        for (int slotIndex = EnemySlotTable.MaximumEnemyCount - 1; slotIndex >= 0; slotIndex--)
        {
            // A death inside the previous reaction overwrote `$12`, the horizontal radius
            // this walker never reloads: zero for an ordinary kill and $4000 for a
            // respawning one. The vertical radius in `$14` survives unchanged.
            if (_deathsAndDrops.TakeRespawnScratch() is { } deathScratch)
                horizontalRadius = deathScratch;
            RoomEnemySlot enemy = _slotTable.Slots[slotIndex];
            if (enemy.DefinitionId is EnemyDefinitionId.None or EnemyDefinitionId.Respawn ||
                !enemy.InvincibilityTimer.IsZero ||
                enemy.Properties.HasAny(EnemyProperties.Deleted))
            {
                continue;
            }

            // The radius actor is family `$0300` (power bomb), not the ordinary bomb
            // family `$0500`. Using `$0500` silently indexed byte 14 and made every enemy
            // whose bomb and power-bomb vulnerabilities differ behave incorrectly.
            int vulnerability = EnemyVulnerabilityEntries.Read(
                enemy.Definition.Vulnerability,
                FamilyVulnerabilityOffset(SamusProjectileFamily.PowerBomb));
            if (EnemyVulnerabilityEntries.Multiplier(vulnerability) == 0)
                continue;

            int xDistance = Math.Abs(unchecked((short)(explosionCenter.X - enemy.Position.X.Whole)));
            int yDistance = Math.Abs(unchecked((short)(explosionCenter.Y - enemy.Position.Y.Whole)));
            if (xDistance >= horizontalRadius || yDistance >= verticalRadius)
                continue;

            EnemyPowerBombReaction? reaction = (enemy.Entity as IEnemyPowerBombReaction)?.FindPowerBomb();
            if (reaction?.SkipsEnemy?.Invoke(enemy) == true)
                continue;
            if (enemy.Definition.PowerBombReactionBody != EnemyCallbackBody.None && reaction is null)
            {
                throw new InvalidDataException(
                    $"Enemy {enemy.DefinitionId} power-bomb reaction " +
                    $"{enemy.Definition.PowerBombReaction?.ToString() ?? "(unnamed)"} is not translated.");
            }

            // Private callbacks replace common power-bomb damage, but still receive the native
            // process-off-screen bit after a qualifying ellipse overlap.
            if (reaction?.ReplacesCommon is { } privateReaction)
            {
                privateReaction(enemy, samus);
                enemy.Properties = enemy.Properties.With(EnemyProperties.ProcessOffScreen);
                reactionCount++;
                continue;
            }

            // `$FF` reaches the reaction dispatcher because the outer admission check masks
            // bit seven, then common AI explicitly returns without damage. It still receives
            // property $0800 afterward, an observable quirk preserved below.
            if (!EnemyVulnerabilityEntries.IsSentinel(vulnerability))
            {
                int damage = 100 * EnemyVulnerabilityEntries.Multiplier(vulnerability);
                if (damage != 0)
                {
                    enemy.InvincibilityTimer = WrappingCountdown.Start(48);
                    enemy.FlashTimer = WrappingCountdown.Start(enemy.HurtFlashFrames + 8);
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
                            _deathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: 0);
                    }
                }
            }

            reaction?.AfterCommon?.Invoke(enemy, samus);

            enemy.Properties = enemy.Properties.With(EnemyProperties.ProcessOffScreen);
            reactionCount++;
        }

        _deathsAndDrops.ClearRespawnScratch();
        return reactionCount;
    }

    /// <summary>
    /// Selects the exact multiplier used by `$A0:A6DE` for an ordinary shot. Beam entries
    /// occupy bytes 0..15; charged shots replace their multiplier with byte 19's low nibble.
    /// Missile, Super Missile, bomb, and Power Bomb families use bytes 12..15 directly.
    /// </summary>
    internal static NormalShotVulnerability ReadNormalShotVulnerability(
        RoomEnemySlot enemy,
        SamusProjectileTypeWord projectileType)
    {
        EnemyVulnerabilityTable table = enemy.Definition.Vulnerability;
        SamusProjectileFamily family = projectileType.Family;
        if (family != SamusProjectileFamily.Beam)
        {
            int byteOffset = FamilyVulnerabilityOffset(family);
            int familyEntry = EnemyVulnerabilityEntries.Read(table, byteOffset);
            return new NormalShotVulnerability(
                Multiplier: EnemyVulnerabilityEntries.Multiplier(familyEntry),
                FreezeImmediately: false,
                PreventsLethalFreeze: false);
        }

        int beamEntry = EnemyVulnerabilityEntries.Read(
            table, projectileType.BeamCombination.TableIndex);
        if (EnemyVulnerabilityEntries.IsSentinel(beamEntry))
        {
            return new NormalShotVulnerability(
                Multiplier: 0,
                FreezeImmediately: true,
                PreventsLethalFreeze: EnemyVulnerabilityEntries.PreventsLethalFreeze(beamEntry));
        }

        int multiplier = EnemyVulnerabilityEntries.Multiplier(beamEntry);
        if (projectileType.IsChargedBeam)
        {
            // Charged-beam vulnerability is a dedicated byte, not another beam-combination
            // row. `$FF` and low-nibble zero both take the dud-shot branch; high bits other
            // than that sentinel do not contribute to the damage multiplier.
            int chargedEntry = EnemyVulnerabilityEntries.Read(
                table, EnemyVulnerabilityDefinitions.ChargedBeamOffset);
            multiplier = EnemyVulnerabilityEntries.IsSentinel(chargedEntry) ? 0 : EnemyVulnerabilityEntries.LowNibbleMultiplier(chargedEntry);
        }

        return new NormalShotVulnerability(
            multiplier,
            FreezeImmediately: false,
            PreventsLethalFreeze: EnemyVulnerabilityEntries.PreventsLethalFreeze(beamEntry));
    }

    /// <summary>
    /// Installs the common frozen handler. Norfair (area two) uses 300 frames; every other
    /// retail area uses 400. The ten-frame invincibility word is shared by direct `$FF`
    /// freezing and the lethal-Ice substitution path.
    /// </summary>
    internal void FreezeEnemyFromNormalShot(RoomEnemySlot enemy, SamusState? samus)
    {
        // The direct-freeze vulnerability refreshes the timer even on an already frozen
        // actor, but only a newly frozen actor queues audio. Lethal-Ice substitution
        // reaches this helper only with an initially zero clock.
        if (enemy.FrozenTimer.IsZero)
            _audio.QueueEnemySound(Library3Sound.EnemyFrozen, maximumQueued: SoundQueueLimit.Max3);
        enemy.FrozenTimer = WrappingCountdown.FromRaw(samus?.LiquidPhysics.AreaIndex == AreaId.Norfair
            ? (ushort)300
            : (ushort)400);
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Frozen;
        enemy.InvincibilityTimer = WrappingCountdown.Start(10);
    }

    /// <summary>Selects the enemy's beam-combination or weapon-family vulnerability byte.</summary>
    internal static byte ReadProjectileVulnerability(
        RoomEnemySlot enemy,
        SamusProjectileTypeWord projectileType)
    {
        EnemyVulnerabilityTable table = enemy.Definition.Vulnerability;
        SamusProjectileFamily family = projectileType.Family;
        int byteOffset = family == SamusProjectileFamily.Beam
            ? projectileType.BeamCombination.TableIndex
            : FamilyVulnerabilityOffset(family);
        return EnemyVulnerabilityEntries.Read(table, byteOffset);
    }

    /// <summary>The vulnerability byte a missile, Super Missile, bomb or Power Bomb family reads.</summary>
    /// <exception cref="InvalidDataException">The family has no vulnerability byte of its own.</exception>
    private static int FamilyVulnerabilityOffset(SamusProjectileFamily family) => family switch
    {
        SamusProjectileFamily.Missile => EnemyVulnerabilityDefinitions.MissileOffset,
        SamusProjectileFamily.SuperMissile => EnemyVulnerabilityDefinitions.SuperMissileOffset,
        SamusProjectileFamily.Bomb => EnemyVulnerabilityDefinitions.BombOffset,
        SamusProjectileFamily.PowerBomb => EnemyVulnerabilityDefinitions.PowerBombOffset,
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
    /// Walks `$A0:9A5A/$A0:9B7F`'s bank-local extended-spritemap structure and returns the
    /// callback belonging to the first overlapping hitbox. One displayed enemy frame may
    /// contain several independently offset ordinary spritemaps; every component points at
    /// a hitbox list whose records are signed bounds plus touch/shot function pointers.
    /// </summary>
    internal bool TryFindExtendedHitboxCallback(
        RoomEnemySlot enemy,
        Position16<RoomPixels> targetPosition,
        Extent16<RoomPixels> targetRadius,
        HitboxCallbackKind callbackKind,
        out EnemyInteractionCallback? callback)
    {
        callback = null;

        // Native multibox collision walks every frame an actor shows, and only an actor that has
        // shown none is skipped. The common visually empty extended frame still owns a point
        // hitbox, so it enters the same collision walker as other extended frames.
        if (enemy.Frame is null)
            return false;

        var target = new EnemyHitboxProbe(
            TopLeft: new(
                unchecked((ushort)(targetPosition.X - targetRadius.X)),
                unchecked((ushort)(targetPosition.Y - targetRadius.Y))),
            BottomRight: new(
                unchecked((ushort)(targetPosition.X + targetRadius.X)),
                unchecked((ushort)(targetPosition.Y + targetRadius.Y))),
            SelectShotCallback: callbackKind == HitboxCallbackKind.Shot,
            ArtworkInstalled: _room.Artwork is not null);

        if (CommonEnemyEmptyFrames.IsFrame(enemy.Frame))
        {
            // The shared empty frame has a single zero-radius point at the
            // enemy origin. Run the same asymmetric native overlap test before
            // selecting its common touch/shot callback.
            if (!target.Overlaps(enemy.Position.X.Whole, enemy.Position.Y.Whole, enemy.Position.X.Whole, enemy.Position.Y.Whole))
                return false;
            callback = callbackKind == HitboxCallbackKind.Shot ? EnemyInteractionCallback.NormalEnemyShot : EnemyInteractionCallback.NormalEnemyTouch;
            return true;
        }

        // A species with engine-owned geometry answers for its own frames; null means none covers this frame.
        if (enemy.Entity is IEnemyExtendedHitboxes owner && owner.FindHitboxCallback(target, out callback) is { } owned)
            return owned;

        throw new InvalidDataException(
            $"Enemy {enemy.DefinitionId} extended collision frame " +
            $"{enemy.Frame} has no compiled geometry.");
    }

    /// <summary>
    /// Identifies actors whose ordinary Samus-contact collision actually enters
    /// <c>$A0:9A5A</c>'s extended-spritemap walker. This differs deliberately from the shot
    /// set: Phantoon uses authored component rectangles for touch, while its weapon collision
    /// is owned by the encounter-specific projectile resolver.
    /// </summary>
    internal static bool UsesExtendedSamusHitboxes(RoomEnemySlot enemy) =>
        enemy.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap) &&
        (Capabilities(enemy) & EnemyCapabilities.ExtendedSamusHitboxes) != EnemyCapabilities.None;

    /// <summary>
    /// Identifies actors whose ordinary projectile collision actually enters
    /// <c>$A0:9B7F</c>'s extended-spritemap walker. Extra-property bit $0004 alone is not
    /// sufficient: Mother Brain and several custom-drawn actors set it for scheduling or
    /// rendering while their shot paths use a header radius or encounter-specific geometry.
    /// Keeping this predicate beside the dispatcher gives ROM-backed probes the same boundary
    /// as gameplay and prevents them from interpreting unrelated component metadata as a
    /// bank-local hitbox pointer.
    /// </summary>
    internal static bool UsesExtendedProjectileHitboxes(RoomEnemySlot enemy) =>
        enemy.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap) &&
        (Capabilities(enemy) & EnemyCapabilities.ExtendedProjectileHitboxes) != EnemyCapabilities.None;

    /// <summary>The bound occupant's collision capabilities; an empty slot has none.</summary>
    private static EnemyCapabilities Capabilities(RoomEnemySlot enemy) =>
        enemy.Entity?.Capabilities ?? EnemyCapabilities.None;

    /// <summary>
    /// Ports $A0:A477-$A531 for one already-overlapping ordinary actor. A zero contact index
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
        int vulnerabilityOffset;
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
                vulnerabilityOffset = EnemyVulnerabilityDefinitions.SpeedBoosterOffset;
                break;
            case SamusContactDamageIndex.Shinespark:
                baseDamage = 300;
                vulnerabilityOffset = EnemyVulnerabilityDefinitions.ShinesparkOffset;
                break;
            case SamusContactDamageIndex.ScrewAttack:
                baseDamage = 2000;
                vulnerabilityOffset = EnemyVulnerabilityDefinitions.ScrewAttackOffset;
                break;
            case SamusContactDamageIndex.PseudoScrew:
                // The cartridge consumes charge before checking vulnerability. Even immune
                // contact removes Pseudo Screw's flare; genuine Screw Attack does not.
                SamusProjectileSystem projectiles = _frameContext.SamusProjectiles
                    ?? throw new InvalidOperationException("Pseudo Screw enemy contact requires the live projectile owner.");
                projectiles.Firing.ConsumePseudoScrewCharge(_frameContext.SamusColours
                    ?? throw new InvalidOperationException("Pseudo Screw enemy contact requires Samus's colours."), samus);
                baseDamage = 200;
                vulnerabilityOffset = EnemyVulnerabilityDefinitions.PseudoScrewOffset;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusContactDamageIndex)} {contactDamageIndex}.");
        }
        EnemyVulnerabilityTable table = enemy.Definition.Vulnerability;
        int vulnerability = EnemyVulnerabilityEntries.Read(
            table, vulnerabilityOffset);
        int damage = (baseDamage >> 1) * EnemyVulnerabilityEntries.Multiplier(vulnerability);
        if (damage == 0)
            return;

        // Touch damage uses the raw hurt-AI duration (default four), unlike projectile and
        // power-bomb paths which add their own visible-flash tail. Samus's timers are also
        // explicitly cleared because this branch represents Samus attacking the enemy.
        enemy.FlashTimer = WrappingCountdown.Start(enemy.HurtFlashFrames);
        enemy.AiStates = enemy.AiStates | EnemyAiStates.Hurt;
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackTimer = WrappingCountdown.Start(0);
        enemy.Health = damage >= enemy.Health
            ? 0
            : enemy.Health - damage;
        _audio.QueueEnemySound(Library2Sound.ContactKill, maximumQueued: SoundQueueLimit.Max1);
        if (enemy.Health != 0)
            return;

        // Rinka's bank-$A2 callback calls this common damage body explicitly, inspects the
        // resulting zero health, then chooses either generic respawn death or its private
        // hidden two-frame Mother Brain respawn. Do not clear the slot before that tail.
        if (deathAnimation == TouchDeathAnimation.Skip)
            return;

        _deathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: 1);
    }

    /// <summary>Uses the native strict radius test, excluding contact where either axis is only tangent.</summary>
    internal static bool RadiusBoxesOverlap(
        Position16<RoomPixels> firstPosition,
        Extent16<RoomPixels> firstRadius,
        Position16<RoomPixels> secondPosition,
        Extent16<RoomPixels> secondRadius) =>
        Math.Abs(unchecked((short)(firstPosition.X - secondPosition.X))) < firstRadius.X + secondRadius.X &&
        Math.Abs(unchecked((short)(firstPosition.Y - secondPosition.Y))) < firstRadius.Y + secondRadius.Y;

    /// <summary>Applies suit reduction and health loss, then publishes the native hurt direction and invincibility window.</summary>
    internal static void ApplyNormalEnemyTouchDamage(
        SamusState samus,
        int damageBeforeSuit,
        int damageSourceX)
    {
        // Suit_Damage_Division quarters the source header's damage for Gravity or halves
        // it for Varia before normal touch publishes the invincibility/hurt request.
        int damage = SamusSuitDamage.Reduce(checked((ushort)damageBeforeSuit), samus.EquippedItems);
        samus.Health = samus.Health <= damage
            ? 0
            : samus.Health - damage;
        samus.InvincibilityTimer = WrappingCountdown.Start(96);
        KnockbackHorizontalDirection direction = samus.Position.X.Whole >= damageSourceX ? KnockbackHorizontalDirection.Right : KnockbackHorizontalDirection.Left;
        // $A0:A4A1 publishes a hit request; $90:DDE9 admits it only when no
        // knockback direction is installed. Damage can recur after invincibility
        // expires even if the old movement handler remains active (e.g. a boss grab).
        samus.KnockbackTimer = WrappingCountdown.Start(5);
        samus.KnockbackXDirection = direction;
        // EnemyMain only publishes the request. The common post-animation
        // interruption owns pose/handler initialization later in this frame.
    }

    /// <summary>
    /// Exact asymmetric support test from CheckIfEnemyIsTouchingSamusFromBelow,
    /// $A0:ABE7. Horizontal tangency is excluded; vertical tangency after the
    /// three-pixel bias is included. Both Kraid platforms and work robots call it.
    /// </summary>
    internal static bool IsEnemyTouchingSamusFromBelow(RoomEnemySlot enemy, SamusState samus)
    {
        int xDistance = EnemyMath.WrappedMagnitude(unchecked((ushort)(samus.Position.X.Whole - enemy.Position.X.Whole)));
        int horizontalGap = unchecked((ushort)(xDistance - samus.Kinematics.Radius.X));
        bool horizontalOverlap = xDistance < samus.Kinematics.Radius.X ||
            horizontalGap < enemy.Radius.X;
        if (!horizontalOverlap)
            return false;

        int biasedYDifference = unchecked((ushort)(samus.Position.Y.Whole + 3 - enemy.Position.Y.Whole));
        if (unchecked((short)biasedYDifference) >= 0)
            return false;
        int yDistance = unchecked((ushort)-biasedYDifference);
        int verticalGap = unchecked((ushort)(yDistance - samus.Kinematics.Radius.Y));
        return yDistance < samus.Kinematics.Radius.Y || verticalGap <= enemy.Radius.Y;
    }
}
