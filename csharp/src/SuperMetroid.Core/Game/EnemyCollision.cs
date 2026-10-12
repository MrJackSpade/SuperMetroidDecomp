using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Core.Hardware.SnesAddressMath;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Samus-to-enemy interaction: touch, shot, bomb and power-bomb collision walks over the interactive
/// enemies, extended hitboxes, vulnerability and freezing, and the dispatch into the registered reactions.
/// </summary>
internal sealed class EnemyCollision
{
    private readonly IEnemyDispatch _dispatch;
    private readonly EnemyAudioQueue _audio;
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    private readonly RoomEarthquake _earthquake;
    private readonly EnemyFrameContext _frameContext;
    private readonly EnemyPresentation _presentation;
    private readonly EnemyProjectileEngine _projectiles;
    private readonly EnemyRoomBinding _room;
    private readonly EnemySlotTable _slotTable;

    internal EnemyCollision(EnemyCollisionDependencies dependencies)
    {
        _dispatch = dependencies.Dispatch;
        _audio = dependencies.Audio;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _earthquake = dependencies.Earthquake;
        _frameContext = dependencies.FrameContext;
        _presentation = dependencies.Presentation;
        _projectiles = dependencies.Projectiles;
        _room = dependencies.Room;
        _slotTable = dependencies.SlotTable;
    }
    /// <summary>The named header touch callback (+$30) of <paramref name="slot"/>'s definition, or null.</summary>
    internal static EnemyInteractionCallback? TouchAi(RoomEnemySlot slot) =>
        EnemyInteractionCallbacks.TryDecode(slot.Definition.Bank, slot.Definition.TouchAiPointer);

    /// <summary>The named header shot callback (+$32) of <paramref name="slot"/>'s definition, or null.</summary>
    internal static EnemyInteractionCallback? ShotAi(RoomEnemySlot slot) =>
        EnemyInteractionCallbacks.TryDecode(slot.Definition.Bank, slot.Definition.ShotAiPointer);

    /// <summary>The named header Power Bomb reaction (+$28) of <paramref name="slot"/>'s definition, or null.</summary>
    internal static EnemyInteractionCallback? PowerBombReaction(RoomEnemySlot slot) =>
        EnemyInteractionCallbacks.TryDecode(slot.Definition.Bank, slot.Definition.PowerBombReactionPointer);

    /// <summary>A raw callback word selected from <paramref name="slot"/>'s bank-local hitbox data, or null.</summary>
    internal static EnemyInteractionCallback? HitboxCallback(RoomEnemySlot slot, ushort pointer) =>
        EnemyInteractionCallbacks.TryDecode(slot.Definition.Bank, pointer);

    /// <summary>Runs the common radius-based Samus/enemy touch pass for translated actors.</summary>
    /// <param name="samus">Live player state receiving the native touch callback.</param>
    /// <param name="controllerInput">Current held input used by touch handlers.</param>
    /// <param name="level">Room terrain required by collision-aware touch handlers.</param>
    /// <param name="onlyNativeEnemyIndex">EnemyMain's current physical index, or null for a standalone whole-list audit.</param>
    public bool ResolveOrdinarySamusContact(
        SamusState samus,
        ushort controllerInput,
        RoomLevelData? level = null,
        ushort? onlyNativeEnemyIndex = null)
    {
        Ensure.NotNull(samus);
        _room.EnsureLoaded();
        SamusContactDamageIndex contactDamageIndex = samus.HorizontalSpeed.ContactDamageIndex;
        if (contactDamageIndex == SamusContactDamageIndex.None && samus.InvincibilityTimer != 0)
            return false;

        foreach (ushort nativeIndex in _dispatch.InteractiveEnemyIndexes)
        {
            if (onlyNativeEnemyIndex.HasValue && nativeIndex != onlyNativeEnemyIndex.Value)
                continue;
            RoomEnemySlot slot = _slotTable.SlotFromNativeIndex(nativeIndex);
            // Native collision entry rejects a zero sprite map before resetting Samus's
            // invulnerability. Keep this per enemy: an empty interactive list or an actor
            // not yet publishing its map must not erase the timer. The reset still precedes
            // overlap testing, so a distant actor with a map can legitimately clear it.
            if (slot.SpritemapPointer == 0)
                continue;
            bool usesExtendedHitboxes = UsesExtendedSamusHitboxes(slot);
            // Multibox entry rejects the two shared no-op callback identities before
            // resetting invulnerability. Ordinary radius entry resets first instead.
            if (usesExtendedHitboxes && TouchAi(slot) is
                EnemyInteractionCallback.NoOp or EnemyInteractionCallback.NoOpShortReturn)
                continue;
            if (contactDamageIndex != SamusContactDamageIndex.None)
                samus.InvincibilityTimer = 0;

            // Native touch collision does not interpret property $0100 as a collision bit;
            // it suppresses rendering only. Hibashi relies on that distinction: its second
            // slot remains invisible while its instruction stream moves a live hitbox.
            // Both Ridley definitions use a hand-authored extended body plus a separately
            // solved tail tip. EnemyMain dispatches its body at this same pre-AI seam;
            // tail processing checks the tip later. Do not flatten either to the header's
            // dummy 8x8 radius here. An actor with no translated touch reaction is skipped.
            if (RidleyBehavior.IsRidleyDefinition(slot.EnemyDefinitionPointer) ||
                slot.Properties.HasAny(EnemyProperties.Deleted) ||
                _dispatch.Ai.FindTouch(slot) is not { } touch)
            {
                continue;
            }

            bool overlapsSamus;
            ushort hitboxTouchAiPointer = slot.Definition.TouchAiPointer;
            if (usesExtendedHitboxes)
            {
                overlapsSamus = TryFindExtendedHitboxCallback(
                    slot,
                    samus.Position.Whole,
                    samus.Kinematics.Radius,
                    selectShotCallback: false,
                    out hitboxTouchAiPointer);
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
            if (!usesExtendedHitboxes && slot.FrozenTimer != 0 &&
                slot.EnemyDefinitionPointer != EnemyDefinitionId.Respawn)
            {
                continue;
            }

            touch(new EnemyTouchContext(
                slot,
                samus,
                controllerInput,
                level,
                usesExtendedHitboxes,
                HitboxCallback(slot, hitboxTouchAiPointer),
                hitboxTouchAiPointer,
                contactDamageIndex));
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs common normal-enemy shot AI for ordinary radius-based translated actors. One
    /// projectile may resolve per enemy per pass, matching the native collision-handler exit.
    /// </summary>
    /// <param name="bus">Address space used by native-compatible projectile collision, reflection, and impact side effects.</param>
    /// <param name="projectiles">Live Samus beam and missile slots whose enemy collisions may consume, reflect, or transition their projectiles.</param>
    /// <param name="sharedProjectiles">Shared bomb-projectile owner passed to projectile impact handling.</param>
    /// <param name="samus">Live player state for enemy-specific shot reactions; may be null only for paths that do not require it, while player-dependent reactions such as Yard beam launch reject its absence.</param>
    /// <param name="onlyNativeEnemyIndex">Current EnemyMain slot, or null for a standalone whole-population audit.</param>
    public int ResolveOrdinaryProjectileHits(
        ISnesAddressSpace bus,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles,
        SamusState? samus = null,
        ushort? onlyNativeEnemyIndex = null)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);
        _room.EnsureLoaded();

        int hitCount = 0;
        foreach (ushort nativeIndex in _dispatch.InteractiveEnemyIndexes)
        {
            if (onlyNativeEnemyIndex.HasValue && nativeIndex != onlyNativeEnemyIndex.Value)
                continue;
            RoomEnemySlot enemy = _slotTable.SlotFromNativeIndex(nativeIndex);

            // The shot pass likewise admits visually invisible actors, but rejects the
            // engine's explicit empty spritemap sentinel $804D and property $0400. Keeping
            // these meanings separate mirrors the bank-$A0 list/collision machinery. An actor
            // with no translated shot reaction is skipped.
            if (enemy.EnemyDefinitionPointer == EnemyDefinitionId.RidleyCeres ||
                enemy.SpritemapPointer is 0 or 0x804d ||
                enemy.InvincibilityTimer != 0 ||
                enemy.Properties.HasAny(
                    EnemyProperties.Deleted |
                    EnemyProperties.IgnoreSamusCollision) ||
                _dispatch.Ai.FindShot(enemy) is not { } reaction)
            {
                continue;
            }

            // Powered definition $E8FF explicitly returns before common shot AI while
            // Phantoon's area-boss bit is clear. The projectile therefore does not begin an
            // impact, and the deactivated-looking body does not enter its recoil sequence.
            if (enemy.EnemyDefinitionPointer == EnemyDefinitionId.Robot &&
                !_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
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
                ushort projectileType = projectile.Type;
                ushort projectileDamage = projectile.Damage;
                ushort projectileDirection = projectile.Direction;
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
                    enemy.SpritemapPointer == CommonEnemyEmptyExtendedFrameDefinitions.Frame)
                    continue;
                if (usesExtendedHitboxes &&
                    IsCanonicalMultiboxNoOpAi(enemy.Definition.ShotAiPointer))
                {
                    // `EprojCollHandler_Multibox` compares the header callback before
                    // reading the extended map. Canonical RTS/RTL means no overlap, no
                    // Super-Missile quake, and no direction mark even when a visible
                    // authored rectangle would otherwise contain this projectile.
                    continue;
                }

                bool overlapsProjectile;
                ushort hitboxShotAiPointer = enemy.Definition.ShotAiPointer;
                if (usesExtendedHitboxes)
                {
                    overlapsProjectile = TryFindExtendedHitboxCallback(
                        enemy,
                        projectile.Position.Whole,
                        projectile.Radius,
                        selectShotCallback: true,
                        out hitboxShotAiPointer);
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

                bool selectedLiteralNoOpShotAi = IsLiteralNoOpEnemyAi(
                    enemy.Definition.Bank,
                    hitboxShotAiPointer);
                var shot = new EnemyShotContext(
                    enemy,
                    projectile,
                    projectiles,
                    bus,
                    samus,
                    projectileType,
                    projectileDamage,
                    projectileDirection,
                    family,
                    usesExtendedHitboxes,
                    HitboxCallback(enemy, hitboxShotAiPointer),
                    hitboxShotAiPointer);

                if (RunShotStage(reaction.BeforeSuperMissileQuake, shot))
                {
                    hitCount++;
                    break;
                }

                if (usesExtendedHitboxes && family == SamusProjectileFamily.SuperMissile)
                {
                    // The multibox prelude requests this earthquake before dispatching the
                    // hitbox callback, even when that callback is the no-op shell region.
                    _earthquake.EarthquakeTimer = SamusProjectileRomData.NonBeam.SuperMissileEarthquakeDuration;
                    _earthquake.EarthquakeType = SamusProjectileRomData.NonBeam.SuperMissileEnemyHitEarthquakeType;
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
                else if (!projectiles.EnemyImpacts.TryStartEnemyImpact(bus, sharedProjectiles, projectile.SlotIndex,
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
                    reaction.AfterCommon?.Invoke(shot, true);
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
                    ushort hurtTime = enemy.HurtAiTime == 0 ? (ushort)4 : enemy.HurtAiTime;
                    enemy.FlashTimer = unchecked((ushort)(hurtTime + 8));
                    enemy.AiHandlerBits = unchecked((ushort)(enemy.AiHandlerBits | 0x0002));
                    // Common shot AI queues the header cry before subtracting health,
                    // including lethal hits. Frozen targets suppress this cry; death
                    // explosion instructions retain their separate, later sound owner.
                    // QueueSfx stores the low byte, even for a constructed wide header.
                    if (enemy.FrozenTimer == 0 && enemy.Definition.HurtSoundEffect != 0)
                        _audio.QueueEnemySound(SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2, unchecked((byte)enemy.Definition.HurtSoundEffect)), 3);
                    // `$A0:A79E` gives every plasma-family hit sixteen invincibility frames.
                    // The packed beam type uses bit three for Plasma; other families do not
                    // set that low bit in their ordinary projectile words.
                    if ((projectileType & 0x0008) != 0)
                        enemy.InvincibilityTimer = EnemyShotTiming.PlasmaInvincibilityFrames;

                    bool lethal = damage >= enemy.Health;
                    bool freezeInsteadOfKilling = lethal &&
                        (projectileType & 0x0002) != 0 &&
                        (shotVulnerability.RawBeamEntry & 0xf0) != 0x80 &&
                        enemy.FrozenTimer == 0;
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
                            ? (ushort)0
                            : unchecked((ushort)(enemy.Health - damage));
                    }
                    if (enemy.Health == 0 && !reaction.ExemptFromGenericDeath)
                    {
                        reaction.BeforeGenericDeath?.Invoke(shot);
                        if (reaction.ReplacesGenericDeath?.Invoke(shot) != true)
                        {
                            _deathsAndDrops.StartGenericEnemyDeath(
                                enemy,
                                EnemyDeathsAndDrops.SelectNormalShotDeathAnimation(enemy, projectileType, forcePirateBigExplosion: false));
                        }
                    }
                }

                reaction.AfterCommon?.Invoke(shot, false);
                hitCount++;
                break;
            }
        }
        return hitCount;
    }

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
        ushort? onlyNativeEnemyIndex = null)
    {
        Ensure.NotNull(bombs);
        Ensure.NotNull(ordinaryProjectiles);
        _room.EnsureLoaded();

        int hitCount = 0;
        for (int ordinal = 0; ordinal < (onlyNativeEnemyIndex.HasValue ? 1 : _dispatch.InteractiveEnemyIndexes.Count); ordinal++)
        {
            ushort nativeIndex = onlyNativeEnemyIndex ?? _dispatch.InteractiveEnemyIndexes[ordinal];
            RoomEnemySlot enemy = _slotTable.SlotFromNativeIndex(nativeIndex);
            bool isMetroid = enemy.EnemyDefinitionPointer == EnemyDefinitionId.Metroid &&
                ShotAi(enemy) == EnemyInteractionCallback.MetroidShot;
            bool usesCommonShotAi = ShotAi(enemy) == EnemyInteractionCallback.NormalEnemyShot;
            bool usesLiteralNoOpShotAi = IsLiteralNoOpEnemyAi(
                enemy.Definition.Bank,
                enemy.Definition.ShotAiPointer);
            bool usesTranslatedPrivateShotAi = IsTranslatedPrivateNormalBombShotAi(
                enemy,
                enemy.Definition.ShotAiPointer);
            bool usesExtendedHitboxes =
                enemy.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap);
            bool headerSuppressesMultiboxShotCollision = usesExtendedHitboxes &&
                IsCanonicalMultiboxNoOpAi(enemy.Definition.ShotAiPointer);
            bool isCrocomire = enemy.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
                enemy.Definition.ShotAiPointer == 0 && usesExtendedHitboxes;
            if ((!isMetroid && !usesCommonShotAi && !usesLiteralNoOpShotAi &&
                    !usesTranslatedPrivateShotAi && !isCrocomire) ||
                enemy.SpritemapPointer == 0 ||
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
                SamusProjectileFamily family = bomb.PackedType.Family;

                // `$A0:A24E-$A265` admits an exploding family-$0500 normal bomb. The
                // ordinary-radius handler also retains the high-bit alternative used by
                // reflected bomb-like actors. `$A0:9D23`'s extended-spritemap handler is
                // narrower: it requires exactly family $0500 and a nonzero X word.
                if (bomb.Type == 0 ||
                    bomb.BombTimer != 0 ||
                    (usesExtendedHitboxes
                        ? family != SamusProjectileFamily.Bomb ||
                          bomb.Position.X.Whole == 0
                        : family != SamusProjectileFamily.Bomb &&
                          (bomb.Type & 0x8000) == 0))
                {
                    continue;
                }

                ushort selectedShotAiPointer = enemy.Definition.ShotAiPointer;
                bool overlaps = usesExtendedHitboxes
                    ? TryFindExtendedHitboxCallback(
                        enemy,
                        bomb.Position.Whole,
                        bomb.Radius,
                        selectShotCallback: true,
                        out selectedShotAiPointer)
                    : RadiusBoxesOverlap(
                        enemy.Position.Whole,
                        enemy.Radius,
                        bomb.Position.Whole,
                        bomb.Radius);
                EnemyInteractionCallback? selectedShotAi = HitboxCallback(enemy, selectedShotAiPointer);
                if (!overlaps)
                {
                    continue;
                }

                // Multibox actors dispatch the callback stored in the selected hitbox,
                // not necessarily the definition header. Do not turn a protected/private
                // component into common damage merely because the header uses `$802D`.
                bool selectedLiteralNoOp = IsLiteralNoOpEnemyAi(
                    enemy.Definition.Bank,
                    selectedShotAiPointer);
                bool selectedTranslatedPrivate = IsTranslatedPrivateNormalBombShotAi(
                    enemy,
                    selectedShotAiPointer);
                if (!isMetroid &&
                    selectedShotAi != EnemyInteractionCallback.NormalEnemyShot &&
                    !selectedLiteralNoOp &&
                    !selectedTranslatedPrivate)
                    continue;

                // EnemyBombCollHandler marks the physical bomb before dispatch. Unlike an
                // ordinary beam impact, the bomb is already running its explosion program;
                // common shot AI must not replace it with a bank-$93 impact animation.
                bomb.Direction = unchecked((ushort)(bomb.Direction | 0x0010));
                var bombContext = new EnemyBombContext(
                    enemy,
                    bomb,
                    ordinaryProjectiles,
                    samus,
                    selectedShotAi,
                    selectedShotAiPointer,
                    usesExtendedHitboxes);
                if (_dispatch.Ai.FindBombSpecial(bombContext) is { } special)
                {
                    special(bombContext);
                }
                else if (!selectedLiteralNoOp)
                {
                    if (_dispatch.Ai.FindBombSpecialAfterNoOp(bombContext) is { } afterNoOp)
                    {
                        afterNoOp(bombContext);
                    }
                    else
                    {
                        // A private callback that rejects the bomb returns before common damage; bank $A0 has
                        // already marked the bomb, so rejection belongs here rather than in collision admission.
                        EnemyBombReaction? reaction = _dispatch.Ai.FindBombCommon(bombContext);
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

    internal static bool IsLiteralNoOpEnemyAi(byte bank, ushort pointer) =>
        EnemyShotCallbackDefinitions.IsLiteralNoOp(bank, pointer);

    /// <summary>
    /// Tests the two engine-owned no-op callback addresses checked directly by
    /// <c>EprojCollHandler_Multibox</c> and <c>EnemyBombCollHandler_Multibox</c> before either
    /// routine reads an extended spritemap. A bank-local routine that merely contains the
    /// same RTS/RTL opcode is deliberately not equivalent: Kraid's `$A7:94B5` proves that
    /// such a header still scans components and may select `$A7:94B6` from an arm hitbox.
    /// </summary>
    internal static bool IsCanonicalMultiboxNoOpAi(ushort pointer) =>
        pointer is 0x804b or 0x804c;

    /// <summary>
    /// Reports the bank-$A2/$A3 private shot callbacks whose family-$0500 path is translated.
    /// Definition and callback are checked together because identical 16-bit addresses in
    /// different enemy banks are unrelated native routines, while an extended hitbox may
    /// deliberately override its definition header with a different callback.
    /// </summary>
    private static bool IsTranslatedPrivateNormalBombShotAi(
        RoomEnemySlot enemy,
        ushort callbackPointer)
    {
        EnemyInteractionCallback? callback = HitboxCallback(enemy, callbackPointer);
        return enemy.EnemyDefinitionPointer == EnemyDefinitionId.BabyTurtle && callback == EnemyInteractionCallback.BabyTurtleShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Owtch && callback == EnemyInteractionCallback.OwtchShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Rinka && callback == EnemyInteractionCallback.RinkaShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Oum &&
            callback == EnemyInteractionCallback.MaridiaLargeSnailShot ||
        enemy.EnemyDefinitionPointer is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 &&
            callback == EnemyInteractionCallback.GRipperRipper2Shot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Dragon && callback == EnemyInteractionCallback.DragonShot ||
        enemy.EnemyDefinitionPointer is
                EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer &&
            callback == EnemyInteractionCallback.ShootableVerticalShutterShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.ShutterDestroyable &&
            callback == EnemyInteractionCallback.DestroyableVerticalShutterShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Metaree && callback == EnemyInteractionCallback.MetareeShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea && callback == EnemyInteractionCallback.FirefleaShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Tripper && callback == EnemyInteractionCallback.TripperShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Mochtroid && callback == EnemyInteractionCallback.MochtroidShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Skree && callback == EnemyInteractionCallback.SkreeShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Yard && callback == EnemyInteractionCallback.YardShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Evir && callback == EnemyInteractionCallback.EvirShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw && callback == EnemyInteractionCallback.YappingMawShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Kago && callback == EnemyInteractionCallback.KagoShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite && callback == EnemyInteractionCallback.MagdolliteShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Beetom && callback == EnemyInteractionCallback.BeetomShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Powamp && callback == EnemyInteractionCallback.PowampShot ||
        WorkRobotBehavior.IsWorkRobotDefinition(enemy.EnemyDefinitionPointer) &&
            callback is EnemyInteractionCallback.WorkRobotShot or EnemyInteractionCallback.WorkRobotNoPowerShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Bull && callback == EnemyInteractionCallback.BullShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Spark && callback == EnemyInteractionCallback.SparkShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock &&
            callback == EnemyInteractionCallback.BlueBrinstarFaceBlockShot ||
        KiHunterBehavior.IsKiHunterBodyDefinition(enemy.EnemyDefinitionPointer) && callback == EnemyInteractionCallback.KiHunterShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid && callback == EnemyInteractionCallback.FakeKraidShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite && callback == EnemyInteractionCallback.ZebetiteShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.CorpseTorizo &&
            callback == EnemyInteractionCallback.DeadTorizoTouchAndShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSidehopper &&
            callback == EnemyInteractionCallback.DeadSidehopperShot ||
        DeadTourianBehavior.HasDeadTourianCorpseShotCallback(enemy) &&
            callbackPointer == enemy.Definition.ShotAiPointer ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody &&
            callback is EnemyInteractionCallback.DraygonShot or EnemyInteractionCallback.DudShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.SporeSpawn &&
            callback is EnemyInteractionCallback.SporeSpawnShot or EnemyInteractionCallback.DudShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody &&
            callback == EnemyInteractionCallback.PhantoonShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody &&
            callback == EnemyInteractionCallback.MotherBrainBodyShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead &&
            callback == EnemyInteractionCallback.MotherBrainHeadShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.BombTorizo &&
            callback is EnemyInteractionCallback.BombTorizoShot or EnemyInteractionCallback.TorizoStandUpSitDownShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo &&
            callback is EnemyInteractionCallback.GoldenTorizoShot or EnemyInteractionCallback.BombTorizoShot or
                EnemyInteractionCallback.TorizoStandUpSitDownShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool && callback == EnemyInteractionCallback.ShaktoolShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.KraidArm &&
            callback == EnemyInteractionCallback.KraidArmShot ||
        enemy.EnemyDefinitionPointer is EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.Ridley &&
            callback == EnemyInteractionCallback.RidleyShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid && callback == EnemyInteractionCallback.ShitroidShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon && callback == EnemyInteractionCallback.BotwoonShot ||
        enemy.EnemyDefinitionPointer is EnemyDefinitionId.Crocomire or EnemyDefinitionId.CrocomireTongue &&
            Enum.IsDefined((CrocomireHitboxShotCallback)callbackPointer) ||
        WallSpacePirateBehavior.IsOrdinarySpacePirateDefinition(enemy.EnemyDefinitionPointer) &&
            callback is EnemyInteractionCallback.SpacePirateShot or
                EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot or
                EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot;
    }

    /// <summary>
    /// Executes the family-$0500 branch of <c>NormalEnemyShotAiSkipDeathAnim</c> at
    /// $A0:A6DE, followed by <c>NormalEnemyShotAi</c>'s ordinary death tail.
    /// </summary>
    internal void ApplyCommonNormalBombDamage(
        RoomEnemySlot enemy,
        SamusBombProjectileSlot bomb,
        bool runGenericDeath = true)
    {
        ushort vulnerabilityPointer = enemy.Definition.VulnerabilityPointer != 0
            ? enemy.Definition.VulnerabilityPointer
            : EnemyVulnerabilityDefinitions.DefaultPointer;
        byte vulnerability = EnemyVulnerabilityDefinitions.Read(
            vulnerabilityPointer, EnemyVulnerabilityDefinitions.BombOffset);
        int damage = (bomb.Damage >> 1) * (vulnerability & 0x7f);

        // A zero multiplier still consumes the collision and leaves direction bit $10 on
        // the bomb. Native emits a dud sprite/sound here; those presentation queues are not
        // yet shared by the host bomb system, so enemy state correctly remains untouched.
        if (damage == 0)
            return;

        ushort hurtTime = enemy.HurtAiTime == 0 ? (ushort)4 : enemy.HurtAiTime;
        enemy.FlashTimer = unchecked((ushort)(hurtTime + 8));
        enemy.AiHandlerBits = unchecked((ushort)(enemy.AiHandlerBits | 0x0002));
        enemy.Health = damage >= enemy.Health
            ? (ushort)0
            : unchecked((ushort)(enemy.Health - damage));
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
        ISnesAddressSpace bus,
        Position16<RoomPixels> explosionCenter,
        byte explosionRadius,
        SamusState? samus = null)
    {
        Ensure.NotNull(bus);
        _room.EnsureLoaded();
        if (explosionRadius == 0)
            return 0;

        int horizontalRadius = explosionRadius;
        // `$A0:A31A-$A31D` retains carry from the first LSR through ADC. Expressing each
        // step makes odd-radius rounding agree with the 65C816 rather than using 0.75f.
        int carry = horizontalRadius & 1;
        int verticalRadius = ((horizontalRadius >> 1) + horizontalRadius + carry) >> 1;
        int reactionCount = 0;
        _deathsAndDrops._enemyDeathRespawnScratch = null;

        // The native pass walks all 32 physical slots from $07C0 down to zero, independent
        // of the ordinary active list. That matters while a power bomb reaches off-screen
        // actors and then sets their process-off-screen property below.
        for (int slotIndex = RoomEnemySystem.MaximumEnemyCount - 1; slotIndex >= 0; slotIndex--)
        {
            // A death inside the previous reaction overwrote `$12`, the horizontal radius
            // this walker never reloads: zero for an ordinary kill and $4000 for a
            // respawning one. The vertical radius in `$14` survives unchanged.
            if (_deathsAndDrops._enemyDeathRespawnScratch is { } deathScratch)
                horizontalRadius = deathScratch;
            _deathsAndDrops._enemyDeathRespawnScratch = null;
            RoomEnemySlot enemy = _slotTable._slots[slotIndex];
            if (enemy.EnemyDefinitionPointer is 0 or EnemyDefinitionId.Respawn ||
                enemy.InvincibilityTimer != 0 ||
                enemy.Properties.HasAny(EnemyProperties.Deleted))
            {
                continue;
            }

            // The radius actor is family `$0300` (power bomb), not the ordinary bomb
            // family `$0500`. Using `$0500` silently indexed byte 14 and made every enemy
            // whose bomb and power-bomb vulnerabilities differ behave incorrectly.
            byte vulnerability = ReadProjectileVulnerability(
                enemy,
                (ushort)SamusProjectileFamily.PowerBomb);
            if ((vulnerability & 0x7f) == 0)
                continue;

            int xDistance = Math.Abs(unchecked((short)(explosionCenter.X - enemy.Position.X.Whole)));
            int yDistance = Math.Abs(unchecked((short)(explosionCenter.Y - enemy.Position.Y.Whole)));
            if (xDistance >= horizontalRadius || yDistance >= verticalRadius)
                continue;

            ushort reactionPointerWord = enemy.Definition.PowerBombReactionPointer;
            EnemyPowerBombReaction? reaction = _dispatch.Ai.FindPowerBomb(enemy);
            if (reaction?.SkipsEnemy?.Invoke(enemy) == true)
                continue;
            if (reactionPointerWord != 0 && reaction is null)
            {
                throw new InvalidDataException(
                    $"Enemy ${(int)enemy.EnemyDefinitionPointer:X4} power-bomb reaction " +
                    $"${enemy.Definition.Bank:X2}:{reactionPointerWord:X4} is not translated.");
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
            if (vulnerability != 0xff)
            {
                int damage = 100 * (vulnerability & 0x7f);
                if (damage != 0)
                {
                    enemy.InvincibilityTimer = 48;
                    ushort hurtTime = enemy.HurtAiTime == 0 ? (ushort)4 : enemy.HurtAiTime;
                    enemy.FlashTimer = unchecked((ushort)(hurtTime + 8));
                    enemy.AiHandlerBits = unchecked((ushort)(enemy.AiHandlerBits | 0x0002));
                    enemy.Health = damage >= enemy.Health
                        ? (ushort)0
                        : unchecked((ushort)(enemy.Health - damage));
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

        _deathsAndDrops._enemyDeathRespawnScratch = null;
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
        ushort pointer = enemy.Definition.VulnerabilityPointer != 0
            ? enemy.Definition.VulnerabilityPointer
            : EnemyVulnerabilityDefinitions.DefaultPointer;
        SamusProjectileFamily family = projectileType.Family;
        if (family != SamusProjectileFamily.Beam)
        {
            int byteOffset = family switch
            {
                SamusProjectileFamily.Missile => EnemyVulnerabilityDefinitions.MissileOffset,
                SamusProjectileFamily.SuperMissile => EnemyVulnerabilityDefinitions.SuperMissileOffset,
                SamusProjectileFamily.Bomb => EnemyVulnerabilityDefinitions.BombOffset,
                SamusProjectileFamily.PowerBomb => EnemyVulnerabilityDefinitions.PowerBombOffset,
                _ => throw new InvalidDataException(
                    $"Projectile family ${(ushort)family:X3} has no translated vulnerability field."),
            };
            byte familyEntry = EnemyVulnerabilityDefinitions.Read(pointer, byteOffset);
            return new NormalShotVulnerability(
                Multiplier: familyEntry & 0x7f,
                FreezeImmediately: false,
                RawBeamEntry: 0);
        }

        byte beamEntry = EnemyVulnerabilityDefinitions.Read(
            pointer, projectileType.BeamCombination.TableIndex);
        if (beamEntry == 0xff)
        {
            return new NormalShotVulnerability(
                Multiplier: 0,
                FreezeImmediately: true,
                RawBeamEntry: beamEntry);
        }

        int multiplier = beamEntry & 0x7f;
        if (projectileType.IsChargedBeam)
        {
            // Charged-beam vulnerability is a dedicated byte, not another beam-combination
            // row. `$FF` and low-nibble zero both take the dud-shot branch; high bits other
            // than that sentinel do not contribute to the damage multiplier.
            byte chargedEntry = EnemyVulnerabilityDefinitions.Read(
                pointer, EnemyVulnerabilityDefinitions.ChargedBeamOffset);
            multiplier = chargedEntry == 0xff ? 0 : chargedEntry & 0x0f;
        }

        return new NormalShotVulnerability(
            multiplier,
            FreezeImmediately: false,
            RawBeamEntry: beamEntry);
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
        if (enemy.FrozenTimer == 0)
            _audio.QueueEnemySound(SoundEffectLibrary3Sounds.EnemyFreeze, maximumQueued: 3);
        enemy.FrozenTimer = samus?.LiquidPhysics.AreaIndex == AreaId.Norfair
            ? (ushort)300
            : (ushort)400;
        enemy.AiHandlerBits = unchecked((ushort)(enemy.AiHandlerBits | 0x0004));
        enemy.InvincibilityTimer = 10;
    }

    internal static byte ReadProjectileVulnerability(
        RoomEnemySlot enemy,
        SamusProjectileTypeWord projectileType)
    {
        ushort pointer = enemy.Definition.VulnerabilityPointer != 0
            ? enemy.Definition.VulnerabilityPointer
            : EnemyVulnerabilityDefinitions.DefaultPointer;
        SamusProjectileFamily family = projectileType.Family;
        int byteOffset = family switch
        {
            SamusProjectileFamily.Beam => projectileType.BeamCombination.TableIndex,
            SamusProjectileFamily.Missile => EnemyVulnerabilityDefinitions.MissileOffset,
            SamusProjectileFamily.SuperMissile => EnemyVulnerabilityDefinitions.SuperMissileOffset,
            SamusProjectileFamily.Bomb => EnemyVulnerabilityDefinitions.BombOffset,
            SamusProjectileFamily.PowerBomb => EnemyVulnerabilityDefinitions.PowerBombOffset,
            _ => throw new InvalidDataException(
                $"Projectile family ${(ushort)family:X3} has no translated vulnerability field."),
        };
        return EnemyVulnerabilityDefinitions.Read(pointer, byteOffset);
    }

    internal readonly record struct NormalShotVulnerability(
        int Multiplier,
        bool FreezeImmediately,
        byte RawBeamEntry);

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
        bool selectShotCallback,
        out ushort callback)
    {
        callback = 0;

        // Native multibox collision only accepts negative 16-bit spritemap pointers.
        // The common visually empty map `$804F` still owns a point hitbox, so it
        // must enter the same collision walker as other extended frames.
        if ((enemy.SpritemapPointer & 0x8000) == 0)
            return false;

        var target = new ExtendedHitboxTarget(
            Left: unchecked((ushort)(targetPosition.X - targetRadius.X)),
            Right: unchecked((ushort)(targetPosition.X + targetRadius.X)),
            Top: unchecked((ushort)(targetPosition.Y - targetRadius.Y)),
            Bottom: unchecked((ushort)(targetPosition.Y + targetRadius.Y)),
            SelectShotCallback: selectShotCallback);

        if (CommonEnemyEmptyExtendedFrameDefinitions.HasFrame(
                enemy.Definition.Bank, enemy.SpritemapPointer))
        {
            // The shared empty frame has a single zero-radius point at the
            // enemy origin. Run the same asymmetric native overlap test before
            // selecting its common touch/shot callback.
            if (!OverlapsExtendedHitbox(target, enemy.Position.X.Whole, enemy.Position.Y.Whole, enemy.Position.X.Whole, enemy.Position.Y.Whole))
                return false;
            callback = CommonEnemyEmptyExtendedFrameDefinitions.Callback(selectShotCallback);
            return true;
        }

        if (WalkingSpacePirateBehavior.IsWalkingSpacePirateDefinition(enemy.EnemyDefinitionPointer) ||
            WallSpacePirateBehavior.IsWallSpacePirateDefinition(enemy.EnemyDefinitionPointer) ||
            NinjaSpacePirateBehavior.IsNinjaSpacePirateDefinition(enemy.EnemyDefinitionPointer))
        {
            // All three ordinary Pirate families have engine-owned collision.
            // Editable component offsets in the installed visual asset cannot move
            // these rectangles or replace their native touch/shot callbacks.
            foreach (SpacePirateCollisionComponent component in
                     SpacePirateCollisionDefinitions.ComponentsAt(
                         enemy.SpritemapPointer))
            {
                Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
                foreach (SpacePirateCollisionHitbox hitbox in
                         SpacePirateCollisionDefinitions.HitboxesAt(
                             component.HitboxPointer))
                {
                    ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                    ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                    ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                    ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                    if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                        continue;
                    callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                    return true;
                }
            }
            return false;
        }

        if (DraygonBehavior.IsDraygonDefinition(enemy.EnemyDefinitionPointer) &&
            enemy.Definition.Bank == DraygonBg2FrameDefinitions.Bank)
        {
            // Both OAM and BG2 art are replaceable presentation. Their native
            // component offsets and hitbox pointers remain fixed gameplay data.
            foreach (DraygonCollisionComponent component in
                     DraygonCollisionDefinitions.ComponentsAt(
                         enemy.SpritemapPointer))
            {
                Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
                foreach (DraygonCollisionHitbox hitbox in
                         DraygonCollisionDefinitions.HitboxesAt(
                             component.HitboxPointer))
                {
                    ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                    ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                    ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                    ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                    if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                        continue;
                    callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                    return true;
                }
            }
            return false;
        }

        if (enemy.EnemyDefinitionPointer == EnemyDefinitionId.SporeSpawn &&
            enemy.Definition.Bank == DraygonBg2FrameDefinitions.Bank &&
            SporeSpawnCollisionDefinitions.IsFrame(enemy.SpritemapPointer))
        {
            // These twelve OAM frames are selected by Spore Spawn. Old visual
            // overrides may carry Draygon-prefixed keys, but every rectangle
            // and callback comes from the compiled gameplay catalog.
            foreach (SporeSpawnCollisionComponent component in
                     SporeSpawnCollisionDefinitions.ComponentsAt(
                         enemy.SpritemapPointer))
            {
                Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
                ushort componentX = componentOrigin.X;
                ushort componentY = componentOrigin.Y;
                foreach (SporeSpawnCollisionHitbox hitbox in
                         SporeSpawnCollisionDefinitions.HitboxesAt(
                             component.HitboxPointer))
                {
                    ushort left = unchecked((ushort)(componentX + hitbox.Left));
                    ushort top = unchecked((ushort)(componentY + hitbox.Top));
                    ushort right = unchecked((ushort)(componentX + hitbox.Right));
                    ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                    if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                        continue;
                    callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                    return true;
                }
            }
            return false;
        }

        if (PhantoonBehavior.IsPhantoonPartDefinition(enemy.EnemyDefinitionPointer))
        {
            // Phantoon's extended frame roots include hitbox-list pointers,
            // but its editable BG2 tilemap writes must never become collision
            // data. Preserve the native component and rectangle order from
            // the compiled bank-$A7 definitions, including point no-ops.
            foreach (PhantoonCollisionComponent component in
                     PhantoonCollisionDefinitions.ComponentsAt(enemy.SpritemapPointer))
            {
                Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
                foreach (PhantoonCollisionHitbox hitbox in
                         PhantoonCollisionDefinitions.HitboxesAt(component.HitboxPointer))
                {
                    ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                    ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                    ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                    ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                    if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                        continue;
                    callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                    return true;
                }
            }
            return false;
        }

        if (RidleyBehavior.IsRidleyDefinition(enemy.EnemyDefinitionPointer) &&
            enemy.Definition.Bank == RidleyCollisionDefinitions.Bank &&
            (RidleyCollisionDefinitions.HasFrame(enemy.SpritemapPointer) ||
             _room.Artwork is not null))
        {
            // The OAM component offsets in an installed composition are editable.
            // Contact and shot collision instead use the eleven fixed cartridge
            // body frames and their engine-owned hitbox lists.
            foreach (RidleyCollisionComponent component in
                     RidleyCollisionDefinitions.ComponentsAt(enemy.SpritemapPointer))
            {
                Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
                ushort componentX = componentOrigin.X;
                ushort componentY = componentOrigin.Y;
                foreach (RidleyCollisionHitbox hitbox in
                         RidleyCollisionDefinitions.HitboxesAt(component.HitboxPointer))
                {
                    ushort left = unchecked((ushort)(componentX + hitbox.Left));
                    ushort top = unchecked((ushort)(componentY + hitbox.Top));
                    ushort right = unchecked((ushort)(componentX + hitbox.Right));
                    ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                    if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                        continue;
                    callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                    return true;
                }
            }
            return false;
        }

        if (enemy.EnemyDefinitionPointer == EnemyDefinitionId.Steam &&
            enemy.Definition.Bank == CeresSteamCollisionDefinitions.Bank &&
            (CeresSteamCollisionDefinitions.HasFrame(enemy.SpritemapPointer) ||
             _room.Artwork is not null))
        {
            // The high byte of each steam frame's $1001 header is drawing
            // metadata, not an extra component. Both damaging rectangles and
            // callbacks remain fixed even if its installed OAM is edited.
            foreach (CeresSteamCollisionComponent component in
                     CeresSteamCollisionDefinitions.ComponentsAt(enemy.SpritemapPointer))
            {
                Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
                foreach (CeresSteamCollisionHitbox hitbox in
                         CeresSteamCollisionDefinitions.HitboxesAt(component.HitboxPointer))
                {
                    ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                    ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                    ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                    ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                    if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                        continue;
                    callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                    return true;
                }
            }
            return false;
        }

        if (enemy.EnemyDefinitionPointer == EnemyDefinitionId.Oum &&
            enemy.Definition.Bank == MaridiaLargeSnailCollisionDefinitions.Bank &&
            (MaridiaLargeSnailCollisionDefinitions.HasFrame(enemy.SpritemapPointer) ||
             _room.Artwork is not null))
        {
            // Oum's 30 native frames each have one (0,0) component, but the
            // ordered regions change with roll and attack direction. Keep
            // those rectangles and callback identities independent of OAM.
            ushort list = MaridiaLargeSnailCollisionDefinitions.HitboxListAt(
                enemy.SpritemapPointer);
            foreach (MaridiaLargeSnailCollisionHitbox hitbox in
                     MaridiaLargeSnailCollisionDefinitions.HitboxesAt(list))
            {
                ushort left = unchecked((ushort)(enemy.Position.X.Whole + hitbox.Left));
                ushort top = unchecked((ushort)(enemy.Position.Y.Whole + hitbox.Top));
                ushort right = unchecked((ushort)(enemy.Position.X.Whole + hitbox.Right));
                ushort bottom = unchecked((ushort)(enemy.Position.Y.Whole + hitbox.Bottom));
                if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                    continue;
                callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                return true;
            }
            return false;
        }

        if (enemy.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
            enemy.Definition.Bank == CrocomireBodyCollisionDefinitions.Bank &&
            CrocomireBodyCollisionDefinitions.HasFrame(enemy.SpritemapPointer))
        {
            // Physical body components and their callback lists are compiled
            // independently of Crocomire's replaceable OAM/BG2 presentation.
            // Preserve native component/hitbox order: the first overlap wins.
            foreach (CrocomireBodyCollisionComponent component in
                     CrocomireBodyCollisionDefinitions.ComponentsAt(enemy.SpritemapPointer))
            {
                Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
                foreach (CrocomireBodyCollisionHitbox hitbox in
                         CrocomireBodyCollisionDefinitions.HitboxesAt(component.HitboxPointer))
                {
                    ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                    ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                    ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                    ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                    if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                        continue;
                    callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                    return true;
                }
            }
            return false;
        }

        if (enemy.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue &&
            enemy.Definition.Bank == CrocomireTongueCollisionDefinitions.Bank &&
            (CrocomireTongueCollisionDefinitions.HasFrame(enemy.SpritemapPointer) ||
             _room.Artwork is not null))
        {
            // These nine selected frames have one component each, but both
            // referenced native hitbox lists are empty. Validate the exact
            // engine-owned frame identity and return no touch/shot callback.
            CrocomireTongueCollisionComponent component =
                CrocomireTongueCollisionDefinitions.ComponentAt(enemy.SpritemapPointer);
            _ = CrocomireTongueCollisionDefinitions.HitboxCountAt(
                component.HitboxPointer);
            return false;
        }

        if (enemy.EnemyDefinitionPointer is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo &&
            enemy.Definition.Bank == TorizoCollisionDefinitions.Bank)
        {
            foreach (GoldenTorizoCollisionComponent component in
                     TorizoCollisionDefinitions.ComponentsAt(enemy.SpritemapPointer))
            {
                Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
                ushort componentX = componentOrigin.X;
                ushort componentY = componentOrigin.Y;
                foreach (GoldenTorizoCollisionHitbox hitbox in
                         TorizoCollisionDefinitions.HitboxesAt(component.HitboxList))
                {
                    ushort left = unchecked((ushort)(componentX + hitbox.Left));
                    ushort top = unchecked((ushort)(componentY + hitbox.Top));
                    ushort right = unchecked((ushort)(componentX + hitbox.Right));
                    ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                    if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                        continue;
                    callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                    return true;
                }
            }
            return false;
        }

        if (enemy.EnemyDefinitionPointer == EnemyDefinitionId.KraidFoot &&
            enemy.Definition.Bank == KraidFootCollisionDefinitions.Bank &&
            KraidFootCollisionDefinitions.TryGetComponents(
                enemy.SpritemapPointer, out var footComponents))
        {
            // Kraid's foot art is replaceable, but each native extended frame
            // still places two physical components against one shared hitbox list.
            foreach (KraidFootCollisionComponent component in footComponents)
            {
                Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
                foreach (KraidFootCollisionHitbox hitbox in
                         KraidFootCollisionDefinitions.HitboxesAt(component.HitboxPointer))
                {
                    ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                    ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                    ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                    ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                    if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                        continue;
                    callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                    return true;
                }
            }
            return false;
        }

        if (enemy.EnemyDefinitionPointer == EnemyDefinitionId.KraidArm &&
            enemy.Definition.Bank == KraidArmCollisionDefinitions.Bank &&
            KraidArmCollisionDefinitions.TryGetComponents(
                enemy.SpritemapPointer, out var kraidComponents))
        {
            // Kraid's arm art is editable, but the native extended spritemap
            // also carries a separate, ordered physical component stream.
            // Read that stream from the immutable cartridge definition catalog.
            foreach (KraidArmCollisionComponent component in kraidComponents)
            {
                Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
                ushort componentX = componentOrigin.X;
                ushort componentY = componentOrigin.Y;
                foreach (KraidArmCollisionHitbox hitbox in
                         KraidArmCollisionDefinitions.HitboxesAt(component.HitboxPointer))
                {
                    ushort left = unchecked((ushort)(componentX + hitbox.Left));
                    ushort top = unchecked((ushort)(componentY + hitbox.Top));
                    ushort right = unchecked((ushort)(componentX + hitbox.Right));
                    ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                    if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                        continue;

                    callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                    return true;
                }
            }
            return false;
        }

        if (enemy.EnemyDefinitionPointer is EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainHead &&
            enemy.Definition.Bank == MotherBrainCollisionDefinitions.Bank)
        {
            // Body and head walk the same bank-$A9 roots; the head's dummy list keeps $A320.
            foreach (MotherBrainCollisionComponent component in
                     MotherBrainCollisionDefinitions.ComponentsAt(enemy.SpritemapPointer))
            {
                Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
                ushort componentX = componentOrigin.X;
                ushort componentY = componentOrigin.Y;
                foreach (MotherBrainCollisionHitbox hitbox in
                         MotherBrainCollisionDefinitions.HitboxesAt(component.HitboxPointer))
                {
                    ushort left = unchecked((ushort)(componentX + hitbox.Left));
                    ushort top = unchecked((ushort)(componentY + hitbox.Top));
                    ushort right = unchecked((ushort)(componentX + hitbox.Right));
                    ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                    if (!OverlapsExtendedHitbox(target, left, top, right, bottom))
                        continue;
                    callback = selectShotCallback ? hitbox.ShotAi : hitbox.TouchAi;
                    return true;
                }
            }
            return false;
        }

        throw new InvalidDataException(
            $"Enemy ${(int)enemy.EnemyDefinitionPointer:X4} extended collision frame " +
            $"${enemy.Definition.Bank:X2}:{enemy.SpritemapPointer:X4} has no compiled geometry.");
    }

    /// <summary>
    /// Shared literal `$A0:9A5A/$9B7F` rectangle tests. The shot and touch
    /// walkers have different strict/inclusive boundaries in the cartridge.
    /// </summary>
    private static bool OverlapsExtendedHitbox(
        ExtendedHitboxTarget target, ushort left, ushort top, ushort right, ushort bottom) =>
        target.SelectShotCallback
            ? !EnemyMath.IsNegative16(target.Right - left) &&
              EnemyMath.IsNegative16(target.Left - right) &&
              !EnemyMath.IsNegative16(target.Bottom - top) &&
              EnemyMath.IsNegative16(target.Top - bottom)
            : EnemyMath.IsNegative16(left - target.Right) &&
              !EnemyMath.IsNegative16(right - target.Left) &&
              EnemyMath.IsNegative16(top - target.Bottom) &&
              !EnemyMath.IsNegative16(bottom - target.Top);

    /// <summary>
    /// The colliding projectile's or Samus's edges as <c>$A0:9A5A/$9B7F</c> compare them, and
    /// whether the walker selects the shot (strict) or touch (inclusive) callback (#633).
    /// </summary>
    private readonly record struct ExtendedHitboxTarget(
        ushort Left, ushort Right, ushort Top, ushort Bottom, bool SelectShotCallback);

    /// <summary>
    /// Identifies actors whose ordinary Samus-contact collision actually enters
    /// <c>$A0:9A5A</c>'s extended-spritemap walker. This differs deliberately from the shot
    /// set: Phantoon uses authored component rectangles for touch, while its weapon collision
    /// is owned by the encounter-specific projectile resolver.
    /// </summary>
    internal static bool UsesExtendedSamusHitboxes(RoomEnemySlot enemy) =>
        enemy.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap) &&
        (WallSpacePirateBehavior.IsOrdinarySpacePirateDefinition(enemy.EnemyDefinitionPointer) ||
         enemy.EnemyDefinitionPointer is
            EnemyDefinitionId.Oum or
            EnemyDefinitionId.BombTorizo or
            EnemyDefinitionId.GoldenTorizo or
            EnemyDefinitionId.Crocomire or
            EnemyDefinitionId.CrocomireTongue or
            EnemyDefinitionId.SporeSpawn or
            EnemyDefinitionId.Steam or
            EnemyDefinitionId.KraidArm or
            EnemyDefinitionId.KraidFoot or
            EnemyDefinitionId.PhantoonBody or
            EnemyDefinitionId.DraygonBody);

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
        (WallSpacePirateBehavior.IsOrdinarySpacePirateDefinition(enemy.EnemyDefinitionPointer) ||
         enemy.EnemyDefinitionPointer is
            EnemyDefinitionId.Oum or
            EnemyDefinitionId.BombTorizo or
            EnemyDefinitionId.GoldenTorizo or
            EnemyDefinitionId.Crocomire or
            EnemyDefinitionId.CrocomireTongue or
            EnemyDefinitionId.SporeSpawn or
            EnemyDefinitionId.Steam or
            EnemyDefinitionId.Kraid or
            EnemyDefinitionId.KraidArm or
            EnemyDefinitionId.KraidFoot or
            EnemyDefinitionId.Ridley or
            EnemyDefinitionId.DraygonBody or
            EnemyDefinitionId.MotherBrainBody or
            EnemyDefinitionId.MotherBrainHead);

    /// <summary>
    /// Dispatches the common and Pirate-specific shot callbacks in hitbox records. Only the
    /// gold Ninja gives `$87C8/$883E` special meaning; every other Pirate deliberately falls
    /// through to normal shot AI even when it displays one of the shared Ninja maps.
    /// </summary>
    internal static PirateHitboxShotAction SelectPirateHitboxShotAction(
        RoomEnemySlot enemy,
        SamusProjectileSlot projectile,
        ushort hitboxShotAiPointer)
    {
        EnemyInteractionCallback? hitboxShotAi = HitboxCallback(enemy, hitboxShotAiPointer);
        // The empty extended map still has a real point hitbox. Its common callback
        // bypasses the Ninja-specific armor tests; it must not be rejected as unknown.
        if (hitboxShotAi is EnemyInteractionCallback.SpacePirateShot or EnemyInteractionCallback.NormalEnemyShot)
            return PirateHitboxShotAction.Normal;
        if (hitboxShotAi is not (
                EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot or EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot))
        {
            throw new InvalidDataException(
                $"Space Pirate hitbox shot AI $B2:{hitboxShotAiPointer:X4} is not translated.");
        }
        if (enemy.EnemyDefinitionPointer != EnemyDefinitionId.PirateGoldNinja)
            return PirateHitboxShotAction.Normal;

        SamusProjectileFamily family = projectile.PackedType.Family;
        if (hitboxShotAi == EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot)
        {
            if (family == SamusProjectileFamily.SuperMissile && projectile.Variable == 0)
                return PirateHitboxShotAction.Ignore;
            return (ushort)family < (ushort)SamusProjectileFamily.PowerBomb
                ? PirateHitboxShotAction.Reflect
                : PirateHitboxShotAction.Ignore;
        }

        if ((ushort)family >= (ushort)SamusProjectileFamily.PowerBomb)
            return PirateHitboxShotAction.Ignore;

        ushort vulnerabilityPointer = enemy.Definition.VulnerabilityPointer != 0
            ? enemy.Definition.VulnerabilityPointer
            : EnemyVulnerabilityDefinitions.DefaultPointer;
        int vulnerabilityOffset = family switch
        {
            SamusProjectileFamily.Beam => projectile.PackedType.BeamCombination.TableIndex,
            SamusProjectileFamily.Missile => EnemyVulnerabilityDefinitions.MissileOffset,
            SamusProjectileFamily.SuperMissile =>
                EnemyVulnerabilityDefinitions.SuperMissileOffset,
            _ => throw new InvalidDataException(
                $"Gold Ninja projectile family ${(ushort)family:X3} has no vulnerability field."),
        };
        int multiplier = EnemyVulnerabilityDefinitions.Read(
            vulnerabilityPointer, vulnerabilityOffset) & 0x0f;
        return multiplier is not (0 or 15)
            ? PirateHitboxShotAction.Normal
            : PirateHitboxShotAction.Reflect;
    }

    /// <summary>
    /// Dispatches a family-$0500 normal bomb through the same three bank-$B2 component
    /// callbacks as the ordinary projectile path. Non-gold Pirates always jump to normal
    /// Pirate shot handling. Gold Ninjas return from both `$87C8` and `$883E` because a
    /// normal bomb sorts after Power Bomb family `$0300`; consequently bombs neither damage
    /// nor reflect from either authored body region.
    /// </summary>
    internal static PirateHitboxShotAction SelectPirateNormalBombHitboxShotAction(
        RoomEnemySlot enemy,
        ushort hitboxShotAiPointer)
    {
        EnemyInteractionCallback? hitboxShotAi = HitboxCallback(enemy, hitboxShotAiPointer);
        if (hitboxShotAi is not (
                EnemyInteractionCallback.SpacePirateShot or
                EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot or
                EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot))
        {
            throw new InvalidDataException(
                $"Space Pirate normal-bomb hitbox AI $B2:{hitboxShotAiPointer:X4} is not translated.");
        }

        return enemy.EnemyDefinitionPointer == EnemyDefinitionId.PirateGoldNinja
            ? PirateHitboxShotAction.Ignore
            : PirateHitboxShotAction.Normal;
    }

    internal enum PirateHitboxShotAction : byte
    {
        Normal,
        Ignore,
        Reflect,
    }

    /// <summary>
    /// Ports $A0:A477-$A531 for one already-overlapping ordinary actor. A zero contact index
    /// damages Samus; Speed Booster, shinespark, Screw Attack, and pseudo-Screw instead read
    /// their dedicated bytes from the enemy's vulnerability record and damage the actor.
    /// </summary>
    internal void ResolveNormalEnemyTouch(
        RoomEnemySlot enemy,
        SamusState samus,
        bool skipDeathAnimation = false)
    {
        SamusContactDamageIndex contactDamageIndex = samus.HorizontalSpeed.ContactDamageIndex;
        if (contactDamageIndex == SamusContactDamageIndex.None)
        {
            ApplyNormalEnemyTouchDamage(
                samus,
                enemy.Definition.Damage,
                enemy.Position.X.Whole);
            return;
        }

        // The cartridge consumes charge before checking vulnerability. Even immune
        // contact removes Pseudo Screw's flare; genuine Screw Attack does not.
        if (contactDamageIndex == SamusContactDamageIndex.PseudoScrew)
        {
            SamusProjectileSystem projectiles = _frameContext._samusProjectilesForEnemyFrame
                ?? throw new InvalidOperationException("Pseudo Screw enemy contact requires the live projectile owner.");
            projectiles.Firing.ConsumePseudoScrewCharge(_presentation._cgram!, samus);
        }

        ushort baseDamage = contactDamageIndex switch
        {
            SamusContactDamageIndex.SpeedBooster => 500,
            SamusContactDamageIndex.Shinespark => 300,
            SamusContactDamageIndex.ScrewAttack => 2000,
            SamusContactDamageIndex.PseudoScrew => 200,
            _ => throw new InvalidOperationException(
                $"Undefined {nameof(SamusContactDamageIndex)} {(int)contactDamageIndex}."),
        };
        int vulnerabilityOffset = contactDamageIndex switch
        {
            SamusContactDamageIndex.SpeedBooster => EnemyVulnerabilityDefinitions.SpeedBoosterOffset,
            SamusContactDamageIndex.Shinespark => EnemyVulnerabilityDefinitions.ShinesparkOffset,
            SamusContactDamageIndex.ScrewAttack => EnemyVulnerabilityDefinitions.ScrewAttackOffset,
            SamusContactDamageIndex.PseudoScrew => EnemyVulnerabilityDefinitions.PseudoScrewOffset,
            _ => throw new InvalidOperationException(
                $"Undefined {nameof(SamusContactDamageIndex)} {(int)contactDamageIndex}."),
        };
        ushort vulnerabilityPointer = enemy.Definition.VulnerabilityPointer != 0
            ? enemy.Definition.VulnerabilityPointer
            : EnemyVulnerabilityDefinitions.DefaultPointer;
        byte vulnerability = EnemyVulnerabilityDefinitions.Read(
            vulnerabilityPointer, vulnerabilityOffset);
        int damage = (baseDamage >> 1) * (vulnerability & 0x7f);
        if (damage == 0)
            return;

        // Touch damage uses the raw hurt-AI duration (default four), unlike projectile and
        // power-bomb paths which add their own visible-flash tail. Samus's timers are also
        // explicitly cleared because this branch represents Samus attacking the enemy.
        enemy.FlashTimer = enemy.HurtAiTime == 0 ? (ushort)4 : enemy.HurtAiTime;
        enemy.AiHandlerBits = unchecked((ushort)(enemy.AiHandlerBits | 0x0002));
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
        enemy.Health = damage >= enemy.Health
            ? (ushort)0
            : unchecked((ushort)(enemy.Health - damage));
        _audio.QueueEnemySound(SoundEffectLibrary2Sounds.EnemyContactDamaged, maximumQueued: 1);
        if (enemy.Health != 0)
            return;

        // Rinka's bank-$A2 callback calls this common damage body explicitly, inspects the
        // resulting zero health, then chooses either generic respawn death or its private
        // hidden two-frame Mother Brain respawn. Do not clear the slot before that tail.
        if (skipDeathAnimation)
            return;

        _deathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: 1);
    }

    internal static bool RadiusBoxesOverlap(
        Position16<RoomPixels> firstPosition,
        Extent16<RoomPixels> firstRadius,
        Position16<RoomPixels> secondPosition,
        Extent16<RoomPixels> secondRadius) =>
        Math.Abs(unchecked((short)(firstPosition.X - secondPosition.X))) < firstRadius.X + secondRadius.X &&
        Math.Abs(unchecked((short)(firstPosition.Y - secondPosition.Y))) < firstRadius.Y + secondRadius.Y;

    internal static void ApplyNormalEnemyTouchDamage(
        SamusState samus,
        ushort damageBeforeSuit,
        ushort damageSourceX)
    {
        // Suit_Damage_Division quarters the source header's damage for Gravity or halves
        // it for Varia before normal touch publishes the invincibility/hurt request.
        ushort damage = SamusSuitDamage.Reduce(damageBeforeSuit, samus.EquippedItems);
        samus.Health = samus.Health <= damage
            ? (ushort)0
            : unchecked((ushort)(samus.Health - damage));
        samus.InvincibilityTimer = 0x0060;
        ushort direction = samus.Position.X.Whole >= damageSourceX ? (ushort)1 : (ushort)0;
        // $A0:A4A1 publishes a hit request; $90:DDE9 admits it only when no
        // knockback direction is installed. Damage can recur after invincibility
        // expires even if the old movement handler remains active (e.g. a boss grab).
        samus.KnockbackTimer = 5;
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
        ushort xDistance = EnemyMath.WrappedMagnitude(unchecked((ushort)(samus.Position.X.Whole - enemy.Position.X.Whole)));
        ushort horizontalGap = unchecked((ushort)(xDistance - samus.Kinematics.Radius.X));
        bool horizontalOverlap = xDistance < samus.Kinematics.Radius.X ||
            horizontalGap < enemy.Radius.X;
        if (!horizontalOverlap)
            return false;

        ushort biasedYDifference = unchecked((ushort)(samus.Position.Y.Whole + 3 - enemy.Position.Y.Whole));
        if (unchecked((short)biasedYDifference) >= 0)
            return false;
        ushort yDistance = unchecked((ushort)-biasedYDifference);
        ushort verticalGap = unchecked((ushort)(yDistance - samus.Kinematics.Radius.Y));
        return yDistance < samus.Kinematics.Radius.Y || verticalGap <= enemy.Radius.Y;
    }
}
