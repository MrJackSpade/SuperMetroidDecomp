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
        /// <summary>Ordinary projectile slots tested by Kraid's native reverse counter walk.</summary>
        private readonly SamusProjectileSystem _projectiles;
        /// <summary>Shared bomb slot appended after the ordinary projectile-slot range.</summary>
        private readonly SamusBombProjectileSystem _sharedProjectiles;

        /// <summary>Captures both projectile owners and validates the native counter against their bounded slot domain.</summary>
        /// <param name="projectiles">Ordinary Samus projectile slots and their active counter.</param>
        /// <param name="sharedProjectiles">Shared bomb storage used when the counter reaches the extra native slot.</param>
        public KraidShotSlots(SamusProjectileSystem projectiles, SamusBombProjectileSystem sharedProjectiles)
        {
            _projectiles = projectiles;
            _sharedProjectiles = sharedProjectiles;
            LastSlot = projectiles.ProjectileCounter;
            if (LastSlot > SamusProjectileSystem.SlotCount)
                throw new InvalidDataException($"Kraid projectile counter {LastSlot} exceeds the bounded native slot domain.");
        }

        /// <summary>Highest counter index visited, inclusive; this preserves the cartridge's count-as-index iteration.</summary>
        public int LastSlot { get; }

        /// <summary>Reads the projectile at a native counter index, mapping the extra index to the shared bomb slot.</summary>
        /// <param name="index">Counter index from zero through <see cref="LastSlot"/>.</param>
        /// <returns>A value snapshot of the selected projectile's collision geometry, type, and damage.</returns>
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

        /// <summary>Marks the selected projectile's direction word with the collision lifecycle state.</summary>
        /// <param name="index">Counter index identifying the ordinary shot or appended shared bomb.</param>
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
                    _ => 0,
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

    /// <summary>Builds the mutable collision rectangle from the active head tilemap hitbox and Kraid's world position.</summary>
    /// <param name="body">Kraid body slot providing the rectangle's world-space origin.</param>
    /// <param name="pointer">Head instruction pointer selecting the current mouth/body hitbox.</param>
    /// <returns>Scratch words matching the native collision routine's left, top, and bottom registers.</returns>
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

    /// <summary>Collision-relevant snapshot of one ordinary projectile or the shared bomb slot.</summary>
    /// <param name="X">Projectile center X coordinate in wrapped room pixels.</param>
    /// <param name="Y">Projectile center Y coordinate in wrapped room pixels.</param>
    /// <param name="XRadius">Horizontal collision radius used by Kraid's overlap comparisons.</param>
    /// <param name="YRadius">Vertical collision radius used by Kraid's overlap comparisons.</param>
    /// <param name="Type">Native projectile type word, used for family, charged-beam, and collision behavior.</param>
    /// <param name="Damage">Projectile damage value before the mouth handler's native halving and vulnerability multiplier.</param>
    private readonly record struct KraidCollisionShot(ushort X, ushort Y, ushort XRadius,
        ushort YRadius, ushort Type, ushort Damage);

    // These are the bounded DP $12/$14/$16 words local to a native collision pass.
    // Compare the wrapped subtraction's sign, not an unbounded host integer ordering.
    /// <summary>Scratch representation of the native wrapped collision boundaries; some passes reuse words as temporaries.</summary>
    private struct KraidCollisionScratch
    {
        /// <summary>Lower vertical boundary for mouth checks; body checks also reuse this word for the prior shot's right edge.</summary>
        public ushort Bottom;
        /// <summary>Upper vertical boundary for mouth and body rectangle overlap checks.</summary>
        public ushort Top;
        /// <summary>Left horizontal boundary for overlap checks against Kraid's head hitbox.</summary>
        public ushort Left;

        /// <summary>Tests the shot against the mouth's top, bottom, and left boundaries using native signed wrapped comparisons.</summary>
        /// <param name="shot">Projectile snapshot whose center and radii are checked.</param>
        /// <returns>True when the shot overlaps the mouth region.</returns>
        public readonly bool OverlapsMouth(KraidCollisionShot shot) =>
            unchecked((short)(shot.Y - shot.YRadius - 1 - Bottom)) < 0 &&
            unchecked((short)(shot.Y + shot.YRadius - Top)) >= 0 &&
            unchecked((short)(shot.X + shot.XRadius - Left)) >= 0;

        /// <summary>Tests the shot against the upper body rectangle or lower contour selected by its vertical position.</summary>
        /// <param name="body">Kraid body slot supplying the contour origin.</param>
        /// <param name="shot">Projectile snapshot whose center and radii are checked.</param>
        /// <returns>True when the shot overlaps the body region.</returns>
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

    /// <summary>Enters the death-initialization state, clears mouth flags, and disables ordinary Samus collision for the six native enemy slots.</summary>
    /// <param name="body">Kraid body slot whose AI state advances into death initialization.</param>
    /// <param name="state">Debugger-facing state whose mouth-hit bookkeeping is reset.</param>
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
