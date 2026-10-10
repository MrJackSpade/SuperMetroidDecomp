using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's body and mouth versus the native counter-indexed projectile slots. His visible body
/// is BG2, so the generic radius/extended-spritemap walker cannot represent these hitboxes;
/// bank `$A7:AFAA-$B267` explicitly consumes pointers from the active head tilemap entry.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Ports Kraid arm helper <c>SpawnExplosionProjectile</c> at `$A7:B0CB`, called by the
    /// arm hitbox callback at `$A7:94B6`. Super Missiles select native dust animation `$1D`;
    /// beams, missiles, and family-$0500 normal bombs select `$06`. The effect uses the
    /// shared room-graphics projectile allocator and queues sound-library-one effect `$3D`.
    /// </summary>
    private void SpawnKraidArmShotExplosion(
        ushort projectileType,
        ushort xPosition,
        ushort yPosition)
    {
        ushort animationIndex = (projectileType & 0x0200) != 0
            ? (ushort)0x001d
            : (ushort)0x0006;
        SpawnRoomGraphicsDustExplosion(xPosition, yPosition, animationIndex);
        QueueEnemySound(SoundEffectLibrary1Sounds.DudShot, maximumQueued: 6);
    }

    /// <summary>
    /// The counter-indexed shot view both passes use. The cartridge starts at the count
    /// itself, not count - 1, and does not inspect active-slot sentinels; count five
    /// therefore includes the first physical bomb.
    /// </summary>
    private readonly struct KraidShotSlots
    {
        private readonly SamusProjectileSystem _projectiles;
        private readonly SamusBombProjectileSystem _sharedProjectiles;

        public KraidShotSlots(SamusProjectileSystem projectiles, SamusBombProjectileSystem sharedProjectiles)
        {
            _projectiles = projectiles;
            _sharedProjectiles = sharedProjectiles;
            LastSlot = projectiles.ProjectileCounter;
            if (LastSlot > SamusProjectileSystem.SlotCount)
                throw new InvalidDataException($"Kraid projectile counter {LastSlot} exceeds the bounded native slot domain.");
        }

        public int LastSlot { get; }

        public KraidCollisionShot Read(int index)
        {
            if (index < SamusProjectileSystem.SlotCount)
            {
                SamusProjectileSlot shot = _projectiles.Slots[index];
                return new(shot.XPosition, shot.YPosition, shot.XRadius, shot.YRadius, shot.Type, shot.Damage);
            }
            SamusBombProjectileSlot bomb = _sharedProjectiles.Slots[0];
            return new(bomb.XPosition, bomb.YPosition, bomb.XRadius, bomb.YRadius, bomb.Type, bomb.Damage);
        }

        public void MarkCollision(int index)
        {
            if (index < SamusProjectileSystem.SlotCount)
            {
                SamusProjectileSlot shot = _projectiles.Slots[index];
                shot.Direction = shot.PackedDirection.WithCollisionLifecycleState();
            }
            else
            {
                SamusBombProjectileSlot bomb = _sharedProjectiles.Slots[0];
                bomb.Direction = new SamusProjectileDirectionWord(bomb.Direction).WithCollisionLifecycleState();
            }
        }
    }

    /// <summary>
    /// Both passes return immediately once Kraid has begun sinking ($A7:AFAD/$B182).
    /// </summary>
    private static bool KraidSinkingSkipsProjectileCollision(RoomEnemySlot body) =>
        unchecked((short)(body.VariableA - (ushort)KraidAiFunction.DeathSink)) >= 0;

    /// <summary>
    /// Ports <c>KraidsMouth_vs_Projectile_CollisionHandling</c> ($A7:AFAA). Projectile
    /// removal is deferred to the projectile pre-instruction.
    /// </summary>
    private int ResolveKraidMouthProjectileHits(RoomEnemySlot body, KraidEnemyState state, KraidShotSlots shots)
    {
        if (KraidSinkingSkipsProjectileCollision(body))
            return 0;

        int lastSlot = shots.LastSlot;
        int mouthHits = 0;
        ushort innerMouth = lastSlot == 0 ? ushort.MaxValue :
            KraidHeadInstructionDefinitions.ReadCollisionHitbox(_bus!, body.VariableB, innerMouth: true);
        if (lastSlot != 0 && innerMouth != ushort.MaxValue)
        {
            KraidCollisionScratch scratch = LoadKraidCollisionScratch(body, innerMouth);
            for (int index = lastSlot; index >= 0; index--)
            {
                KraidCollisionShot shot = shots.Read(index);
                SamusProjectileTypeWord type = new(shot.Type);
                if (!scratch.OverlapsMouth(shot) ||
                    (type.Family == SamusProjectileFamily.Beam && !type.IsChargedBeam))
                    continue;

                if (type.Family == SamusProjectileFamily.Beam)
                    state.MouthFlags |= 1;
                // The common shot callback reuses DP $12/$14 for type/vulnerability.
                // In particular, $8200 left by a Super Missile changes the next shot's
                // signed CMP; rebuilding a clean rectangle here doubles close-range damage.
                scratch.Bottom = shot.Type;
                scratch.Top = body.Definition.VulnerabilityPointer != 0
                    ? body.Definition.VulnerabilityPointer : EnemyVulnerabilityDefinitions.DefaultPointer;
                int multiplier = type.Family switch
                {
                    SamusProjectileFamily.Beam => EnemyVulnerabilityDefinitions.Read(
                        scratch.Top, EnemyVulnerabilityDefinitions.ChargedBeamOffset) & 0x0f,
                    SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile or
                    SamusProjectileFamily.Bomb or SamusProjectileFamily.PowerBomb =>
                        ReadProjectileVulnerability(body, type) & 0x7f,
                    SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion => 0,
                    _ => throw new InvalidOperationException($"Undefined SamusProjectileFamily {type.Family}."),
                };
                int damage = unchecked((ushort)((shot.Damage >> 1) * multiplier));
                if (damage != 0)
                {
                    body.Health = damage >= body.Health
                        ? (ushort)0
                        : unchecked((ushort)(body.Health - damage));
                    body.FlashTimer = unchecked((ushort)((body.HurtAiTime == 0 ? 4 : body.HurtAiTime) + 8));
                    body.AiHandlerBits = unchecked((ushort)(body.AiHandlerBits | 2));
                    if ((shot.Type & 8) != 0)
                        body.InvincibilityTimer = 16;
                }
                else
                {
                    RoomSpriteObjectSlot? dud = SpawnRoomSpriteObject(
                        shot.X, shot.Y, RoomSpriteObjectKind.EnemyProjectileDud, graphicsIndex: 0);
                    // Create_Sprite_Object returns its native index in $12 only on success.
                    scratch.Bottom = dud?.NativeIndex ?? shot.X;
                    scratch.Top = shot.Y;
                    scratch.Left = (ushort)RoomSpriteObjectKind.EnemyProjectileDud;
                    QueueEnemySound(SoundEffectLibrary1Sounds.DudShot, maximumQueued: 3);
                }
                shots.MarkCollision(index);
                mouthHits++;
            }
        }

        if (mouthHits != 0)
        {
            state.HurtFrame = 6;
            state.HurtFrameTimer = 2;
            if ((state.MouthFlags & 2) != 0)
                state.MouthFlags |= 4;
            if (unchecked((short)(body.Health - 1)) < 0 &&
                unchecked((short)(body.VariableA - (ushort)KraidAiFunction.DeathInitialize)) < 0)
                BeginKraidDeath(body, state);
        }

        return mouthHits;
    }

    /// <summary>
    /// Ports <c>KraidBody_vs_Projectile_CollisionHandling</c> ($A7:B181), the independent
    /// outer mouth/body pass.
    /// </summary>
    private int ResolveKraidBodyProjectileHits(RoomEnemySlot body, KraidEnemyState state, KraidShotSlots shots)
    {
        if (KraidSinkingSkipsProjectileCollision(body))
            return 0;

        state.MouthFlags &= 0xfffe;
        int lastSlot = shots.LastSlot;
        int bodyHits = 0;
        if (lastSlot != 0)
        {
            ushort outerMouth = KraidHeadInstructionDefinitions.ReadCollisionHitbox(_bus!, body.VariableB, innerMouth: false);
            KraidCollisionScratch scratch = LoadKraidCollisionScratch(body, outerMouth);
            for (int index = lastSlot; index >= 0; index--)
            {
                KraidCollisionShot shot = shots.Read(index);
                if (!scratch.OverlapsBody(body, shot))
                    continue;
                SpawnKraidArmShotExplosion(shot.Type, shot.X, shot.Y);
                scratch.Bottom = shot.X;
                scratch.Top = shot.Y;
                shots.MarkCollision(index);
                if ((shot.Type & 0x0010) != 0)
                    state.MouthFlags |= 1;
                bodyHits++;
            }
        }

        if (bodyHits != 0 && body.VariableA == (ushort)KraidAiFunction.MainloopThinking)
        {
            body.VariableA = (ushort)KraidAiFunction.InitializeEyeGlow;
            if ((state.MouthFlags & 1) != 0)
                state.MouthFlags |= 0x0302;
        }
        return bodyHits;
    }

    private KraidCollisionScratch LoadKraidCollisionScratch(RoomEnemySlot body, ushort pointer)
    {
        (short left, short top, short bottom) = KraidMouthHitboxes.ResolveCollision(_bus!, pointer);
        return new()
        {
            Left = unchecked((ushort)(body.XPosition + left)),
            Top = unchecked((ushort)(body.YPosition + top)),
            Bottom = unchecked((ushort)(body.YPosition + bottom)),
        };
    }

    private readonly record struct KraidCollisionShot(ushort X, ushort Y, ushort XRadius,
        ushort YRadius, ushort Type, ushort Damage);

    // These are the bounded DP $12/$14/$16 words local to a native collision pass.
    // Compare the wrapped subtraction's sign, not an unbounded host integer ordering.
    private struct KraidCollisionScratch
    {
        public ushort Bottom;
        public ushort Top;
        public ushort Left;

        public readonly bool OverlapsMouth(KraidCollisionShot shot) =>
            unchecked((short)(shot.Y - shot.YRadius - 1 - Bottom)) < 0 &&
            unchecked((short)(shot.Y + shot.YRadius - Top)) >= 0 &&
            unchecked((short)(shot.X + shot.XRadius - Left)) >= 0;

        public bool OverlapsBody(RoomEnemySlot body, KraidCollisionShot shot)
        {
            if (unchecked((short)(shot.Y - shot.YRadius - 1 - Bottom)) < 0)
                return unchecked((short)(shot.Y + shot.YRadius - Top)) >= 0 &&
                    unchecked((short)(shot.X + shot.XRadius - Left)) >= 0;

            Bottom = unchecked((ushort)(shot.X + shot.XRadius));
            short relativeY = unchecked((short)(shot.Y - body.YPosition));
            return unchecked((short)(body.XPosition + KraidBodyContour.LeftEdge(relativeY) - Bottom)) < 0;
        }
    }

    private void BeginKraidDeath(RoomEnemySlot body, KraidEnemyState state)
    {
        body.VariableA = (ushort)KraidAiFunction.DeathInitialize;
        state.MouthFlags = 0;
        for (int slot = 0; slot < 6; slot++)
        {
            // Native property `$0400` disables the ordinary touch list while the private
            // death state owns body movement and breakup effects.
            // Nails are separate projectile-like actors and delete themselves from HP zero.
            // Keeping this six-slot bound preserves that exact division.
            _slots[slot].Properties = _slots[slot].Properties.With(
                EnemyProperties.IgnoreSamusCollision);
        }
    }
}
