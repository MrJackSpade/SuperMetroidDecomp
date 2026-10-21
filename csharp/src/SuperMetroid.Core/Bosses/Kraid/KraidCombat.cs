using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.SpriteObjects;
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
    /// <summary>Spawns room sprite objects for Kraid's visual effects.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Provides the native body and seven part slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Checks mouth, body, and part collisions against Samus and shots.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidCombat(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, RoomSpriteObjectPool spriteObjects, EnemySlotTable slotTable, EnemyCollision collision, KraidEncounterAccess access)
    {
        _projectiles = projectiles;
        _audio = audio;
        _spriteObjects = spriteObjects;
        _slotTable = slotTable;
        _collision = collision;
        _access = access;
    }

    /// <summary>Binds Kraid's body and mouth reactions to Samus bomb contacts.</summary>
    internal void RegisterKraidBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecial(c => c.Enemy.DefinitionId is EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidFoot,
            c =>
            {
                if (c.SelectedShotAi is not (EnemyInteractionCallback.KraidNoOpShot or EnemyInteractionCallback.KraidArmShot) ||
                    c.Enemy.DefinitionId == EnemyDefinitionId.KraidFoot &&
                        c.SelectedShotAi != EnemyInteractionCallback.KraidNoOpShot)
                {
                    throw new InvalidDataException(
                        $"Kraid part ${(int)c.Enemy.DefinitionId:X4} selected " +
                        $"normal-bomb shot AI {c.SelectedShotAi?.ToString() ?? "(unnamed)"}.");
                }

                // `$A7:94B6` creates a room-graphics dust actor at the selected physical projectile coordinates,
                // then returns to the multibox walker that has already installed direction bit `$10`. `$94B5` is the
                // adjacent literal RTL used by protected arm frames and every foot rectangle.
                if (c.SelectedShotAi == EnemyInteractionCallback.KraidArmShot)
                    SpawnKraidArmShotExplosion(c.Bomb.PackedType.Raw, c.Bomb.Position.Whole);
            });
    }

    /// <summary>Binds shot callbacks for Kraid's mouth, body, arm, and nail surfaces.</summary>
    internal void RegisterKraidShotReactions(EnemySharedReactions table)
    {
        // Kraid's arm and foot are admitted by their literal no-op header callback, but their authored hitboxes
        // select the arm's private callback.
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId is EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidFoot &&
                                                     EnemyCollision.IsLiteralNoOpShotAi(enemy.Definition, EnemyCollision.ShotAi(enemy)))
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes ||
                    c.HitboxShotAi is not (EnemyInteractionCallback.KraidNoOpShot or EnemyInteractionCallback.KraidArmShot) ||
                    c.Enemy.DefinitionId == EnemyDefinitionId.KraidFoot &&
                        c.HitboxShotAi != EnemyInteractionCallback.KraidNoOpShot)
                {
                    throw new InvalidDataException(
                        $"Kraid part ${(int)c.Enemy.DefinitionId:X4} selected " +
                        $"shot AI {c.HitboxShotAi?.ToString() ?? "(unnamed)"}.");
                }

                c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                if (c.HitboxShotAi == EnemyInteractionCallback.KraidArmShot)
                    SpawnKraidArmShotExplosion(c.ProjectileType.Raw, c.Projectile.Position.Whole);
                return true;
            },
        });
    }

    /// <summary>Binds Samus contact handling for Kraid's body and parts.</summary>
    internal void RegisterKraidTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.KraidArm &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.KraidArmTouch,
            context =>
            {
                // `$A7:9490-$94A4` deliberately addresses physical enemy slot four,
                // rather than searching for a particular lint definition. The encounter's
                // eight-record layout makes that the bottom lint; forcing `$B89B` here
                // causes it to fire immediately after the arm launches Samus up and right.
                // Native writes only the whole displacement words, so preserve both
                // fractional words exactly as they were before contact.
                _ = _access.RequireKraidState(context.Slot);
                context.Samus.Kinematics.ExtraDisplacement = context.Samus.Kinematics.ExtraDisplacement.WithWhole(new(4, -8));
                _access.Part(_slotTable.Slots[4]).Function = KraidAiFunction.LintFire;
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
            });
    }

    /// <summary>
    /// Ports Kraid arm helper <c>SpawnExplosionProjectile</c> at `$A7:B0CB`, called by the
    /// arm hitbox callback at `$A7:94B6`. Super Missiles select native dust animation `$1D`;
    /// beams, missiles, and family-$0500 normal bombs select `$06`. The effect uses the
    /// shared room-graphics projectile allocator and queues sound-library-one effect `$3D`.
    /// </summary>
    private void SpawnKraidArmShotExplosion(
        ushort projectileType,
        Position16<RoomPixels> position)
    {
        // Family bit 1 ($0200) is set for super missiles and power bombs.
        MiscDustAnimation animationIndex = (projectileType & 512) != 0
            ? MiscDustAnimation.BigExplosion
            : MiscDustAnimation.DudShotTinyExplosion;
        _projectiles.SpawnRoomGraphicsDustExplosion(position, animationIndex);
        _audio.QueueEnemySound(SoundEffectLibrary1Sounds.DudShot, maximumQueued: 6);
    }

    /// <summary>
    /// Both passes return immediately once Kraid has begun sinking ($A7:AFAD/$B182).
    /// </summary>
    private bool KraidSinkingSkipsProjectileCollision(RoomEnemySlot body) =>
        _access.Part(body).Function >= KraidAiFunction.DeathSink;

    /// <summary>
    /// Ports <c>KraidsMouth_vs_Projectile_CollisionHandling</c> ($A7:AFAA). Projectile
    /// removal is deferred to the projectile pre-instruction.
    /// </summary>
    internal int ResolveKraidMouthProjectileHits(RoomEnemySlot body, KraidEnemyState state, KraidBehavior.KraidShotSlots shots)
    {
        if (KraidSinkingSkipsProjectileCollision(body))
            return 0;

        int lastSlot = shots.LastSlot;
        int mouthHits = 0;
        KraidMouthHitbox? innerMouth = lastSlot == 0 ? null : KraidHeadAnimations.InnerMouthAt(state.HeadProgram);
        if (innerMouth is { } inner)
        {
            KraidBehavior.KraidCollisionScratch scratch = LoadKraidCollisionScratch(body, inner);
            for (int index = lastSlot; index >= 0; index--)
            {
                KraidBehavior.KraidCollisionShot shot = shots.Read(index);
                SamusProjectileTypeWord type = shot.Type;
                if (!scratch.OverlapsMouth(shot) ||
                    (type.Family == SamusProjectileFamily.Beam && !type.IsChargedBeam))
                    continue;

                if (type.Family == SamusProjectileFamily.Beam)
                    state.MouthFlags |= KraidMouthFlags.Hit;
                // The common shot callback reuses DP $12/$14 for type/vulnerability.
                // In particular, $8200 left by a Super Missile changes the next shot's
                // signed CMP; rebuilding a clean rectangle here doubles close-range damage.
                scratch.Bottom = shot.Type.Raw;
                scratch.Top = KraidContactDefinitions.ShotVulnerabilityResidue;
                // A vulnerability entry's multiplier is its low nibble for charged beams and its low seven bits otherwise.
                int multiplier = type.Family switch
                {
                    SamusProjectileFamily.Beam => EnemyVulnerabilityDefinitions.Read(
                        body.Definition.Vulnerability, EnemyVulnerabilityDefinitions.ChargedBeamOffset) & 15,
                    SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile or
                    SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb =>
                        EnemyCollision.ReadProjectileVulnerability(body, type) & 127,
                    SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion => 0,
                    _ => throw new InvalidOperationException($"Undefined SamusProjectileFamily {type.Family}."),
                };
                int damage = unchecked((ushort)((shot.Damage >> 1) * multiplier));
                if (damage != 0)
                {
                    body.Health = damage >= body.Health
                        ? 0
                        : body.Health - damage;
                    body.FlashTimer = WrappingCountdown.Start(body.HurtFlashFrames + 8);
                    body.AiStates = body.AiStates | EnemyAiStates.Hurt;
                    if (type.HasAnyBeam(SamusBeamFlags.Plasma))
                        body.InvincibilityTimer = WrappingCountdown.Start(16);
                }
                else
                {
                    RoomSpriteObjectSlot? dud = _spriteObjects.SpawnRoomSpriteObject(
                        shot.Position, RoomSpriteObjectKind.EnemyProjectileDud, graphicsIndex: 0);
                    // Create_Sprite_Object returns its native word-array index, twice the slot, in $12 only on success.
                    scratch.Bottom = dud is { } created ? checked((ushort)(created.SlotIndex * sizeof(ushort))) : shot.Position.X;
                    scratch.Top = shot.Position.Y;
                    scratch.Left = (ushort)RoomSpriteObjectKind.EnemyProjectileDud;
                    _audio.QueueEnemySound(SoundEffectLibrary1Sounds.DudShot, maximumQueued: 3);
                }
                shots.MarkCollision(index);
                mouthHits++;
            }
        }

        if (mouthHits != 0)
        {
            state.HurtFrame = 6;
            state.HurtFrameTimer = WrappingCountdown.Start(2);
            if ((state.MouthFlags & KraidMouthFlags.ReopenArmed) != 0)
                state.MouthFlags |= KraidMouthFlags.ReopenPending;
            if (body.Health < 1 &&
                state.Parts[body.SlotIndex].Function < KraidAiFunction.DeathInitialize)
                BeginKraidDeath(body, state);
        }

        return mouthHits;
    }

    /// <summary>
    /// Ports <c>KraidBody_vs_Projectile_CollisionHandling</c> ($A7:B181), the independent
    /// outer mouth/body pass.
    /// </summary>
    internal int ResolveKraidBodyProjectileHits(RoomEnemySlot body, KraidEnemyState state, KraidBehavior.KraidShotSlots shots)
    {
        if (KraidSinkingSkipsProjectileCollision(body))
            return 0;

        state.MouthFlags &= ~KraidMouthFlags.Hit;
        int lastSlot = shots.LastSlot;
        int bodyHits = 0;
        if (lastSlot != 0)
        {
            KraidMouthHitbox outerMouth = KraidHeadAnimations.OuterMouthAt(state.HeadProgram);
            KraidBehavior.KraidCollisionScratch scratch = LoadKraidCollisionScratch(body, outerMouth);
            for (int index = lastSlot; index >= 0; index--)
            {
                KraidBehavior.KraidCollisionShot shot = shots.Read(index);
                if (!scratch.OverlapsBody(body, shot))
                    continue;
                SpawnKraidArmShotExplosion(shot.Type.Raw, shot.Position);
                scratch.Bottom = shot.Position.X;
                scratch.Top = shot.Position.Y;
                shots.MarkCollision(index);
                if (shot.Type.IsChargedBeam)
                    state.MouthFlags |= KraidMouthFlags.Hit;
                bodyHits++;
            }
        }

        if (bodyHits != 0 && state.Parts[body.SlotIndex].Function == KraidAiFunction.MainloopThinking)
        {
            state.Parts[body.SlotIndex].Function = KraidAiFunction.InitializeEyeGlow;
            if ((state.MouthFlags & KraidMouthFlags.Hit) != 0)
            {
                state.MouthFlags |= KraidMouthFlags.ReopenArmed;
                state.ArmMouthReopens();
            }
        }
        return bodyHits;
    }

    /// <summary>Converts a mouth shape into wrapped world-space collision bounds.</summary>
    internal static KraidBehavior.KraidCollisionScratch LoadKraidCollisionScratch(RoomEnemySlot body, KraidMouthHitbox shape) =>
        LoadKraidCollisionScratch(body, KraidMouthHitboxDefinitions.ResolveCollision(shape));

    /// <summary>Places a mouth shape's left, top and bottom edges at Kraid's body position, wrapping as the cartridge does.</summary>
    internal static KraidBehavior.KraidCollisionScratch LoadKraidCollisionScratch(
        RoomEnemySlot body, KraidMouthCollisionEdges shape)
    {
        (short left, short top, short bottom) = shape;
        return new()
        {
            Left = unchecked((ushort)(body.Position.X.Whole + left)),
            Top = unchecked((ushort)(body.Position.Y.Whole + top)),
            Bottom = unchecked((ushort)(body.Position.Y.Whole + bottom)),
        };
    }

    /// <summary>Switches the body from combat to its scripted death function.</summary>
    private void BeginKraidDeath(RoomEnemySlot body, KraidEnemyState state)
    {
        state.Parts[body.SlotIndex].Function = KraidAiFunction.DeathInitialize;
        state.ClearMouthReactions();
        for (int slot = 0; slot < 6; slot++)
        {
            // Native property `$0400` disables the ordinary touch list while the private
            // death state owns body movement and breakup effects.
            // Nails are separate projectile-like actors and delete themselves from HP zero.
            // Keeping this six-slot bound preserves that exact division.
            _slotTable.Slots[slot].Properties = _slotTable.Slots[slot].Properties.With(
                EnemyProperties.IgnoreSamusCollision);
        }
    }

    /// <summary>Checks the body hitbox and applies contact damage to Samus.</summary>
    /// <remarks>These collision checks belong to actor AI rather than generic interactive hitboxes. The body's
    /// permanent intangible flag and lint's no-op touch callback do not suppress these private native paths.</remarks>
    internal void ResolveKraidBodyContact(RoomEnemySlot body, KraidEnemyState state, SamusState samus)
    {
        if (state.Parts[body.SlotIndex].Function >= KraidAiFunction.DeathInitialize)
            return;
        short relativeY = unchecked((short)(samus.Position.Y.Whole - body.Position.Y.Whole));
        if (unchecked((short)(body.Position.X.Whole + KraidBodyContour.LeftEdge(relativeY) -
            samus.Position.X.Whole - samus.Kinematics.Radius.X)) >= 0)
            return;
        if (unchecked((short)(samus.Position.X.Whole - KraidContactDefinitions.MinimumBodyEjectionX)) >= 0)
            samus.Position -= new Offset16<RoomPixels>(unchecked((short)KraidContactDefinitions.BodyEjection), 0);
        Position16<RoomPixels> ejected = samus.Position.Whole -
            new Offset16<RoomPixels>(0, unchecked((short)KraidContactDefinitions.BodyEjection));
        samus.Position = samus.Position.WithWholeY((ejected - state.EjectionLimit).Y < 0
            ? state.EjectionLimit.Y : ejected.Y);
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(new(KraidContactDefinitions.ExtraX, KraidContactDefinitions.ExtraY));
        if (samus.InvincibilityTimer.IsZero)
            _collision.ResolveNormalEnemyTouch(body, samus);
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindKraidFootHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(enemy.DefinitionId == EnemyDefinitionId.KraidFoot &&
            KraidFootCollisionDefinitions.TryGetComponents(enemy.Frame, out var footComponents)))
            return null;

        // Kraid's foot art is replaceable, but each native extended frame
        // still places two physical components against one shared hitbox list.
        foreach (KraidCollisionComponent component in footComponents)
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (KraidCollisionHitbox hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback
                    ? KraidHitboxCallbackCodec.ToCallback(hitbox.Shot)
                    : KraidHitboxCallbackCodec.ToCallback(hitbox.Touch);
                return true;
            }
        }
        return false;
    
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindKraidArmHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(enemy.DefinitionId == EnemyDefinitionId.KraidArm &&
            KraidArmCollisionDefinitions.TryGetComponents(enemy.Frame, out var kraidComponents)))
            return null;

        // Kraid's arm art is editable, but the native extended spritemap
        // also carries a separate, ordered physical component stream.
        // Read that stream from the immutable cartridge definition catalog.
        foreach (KraidCollisionComponent component in kraidComponents)
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            ushort componentX = componentOrigin.X;
            ushort componentY = componentOrigin.Y;
            foreach (KraidCollisionHitbox hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentX + hitbox.Left));
                ushort top = unchecked((ushort)(componentY + hitbox.Top));
                ushort right = unchecked((ushort)(componentX + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;

                callback = probe.SelectShotCallback
                    ? KraidHitboxCallbackCodec.ToCallback(hitbox.Shot)
                    : KraidHitboxCallbackCodec.ToCallback(hitbox.Touch);
                return true;
            }
        }
        return false;
    
    }
}
