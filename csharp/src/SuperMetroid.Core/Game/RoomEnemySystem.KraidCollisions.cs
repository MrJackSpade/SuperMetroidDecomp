using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Kraid's body and mouth versus the five ordinary Samus-projectile slots. His visible body
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
        LastEnemyProjectileDudSoundEffect = 0x003d;
    }

    /// <summary>
    /// Resolves one enemy-main collision pass. The vulnerable inner mouth is tested first;
    /// remaining shots are absorbed by the outer mouth/body contour without damaging HP.
    /// </summary>
    public int ResolveKraidProjectileHits(
        ISnesAddressSpace bus,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(projectiles);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);
        if (_kraidState is null || _slots[0].EnemyDefinitionPointer != KraidDefinition)
            return 0;

        RoomEnemySlot body = _slots[0];
        KraidEnemyState state = _kraidState;
        if (body.Properties.HasAny(EnemyProperties.Deleted) ||
            unchecked((short)(body.VariableA + 0x3ac9)) >= 0)
        {
            return 0;
        }

        int hitCount = 0;
        foreach (SamusProjectileSlot shot in projectiles.Slots.Reverse())
        {
            if (!shot.IsActive)
                continue;

            ushort projectileType = shot.Type;
            ushort projectileDamage = shot.Damage;
            bool damagingFamily = (projectileType & 0x0f00) != 0 ||
                (projectileType & 0x0010) != 0;
            if (damagingFamily && state.InvulnerableMouthHitbox != ushort.MaxValue &&
                KraidMouthHitboxOverlapsShot(body, state.InvulnerableMouthHitbox, shot))
            {
                if (!projectiles.TryStartEnemyImpact(bus, sharedProjectiles, shot.SlotIndex))
                    continue;

                if ((projectileType & 0x0010) != 0)
                    state.MouthFlags |= 1;
                byte vulnerability = ReadProjectileVulnerability(bus, body, projectileType);
                int damage = (projectileDamage >> 1) * (vulnerability & 0x7f);
                if (damage != 0)
                {
                    body.Health = damage >= body.Health
                        ? (ushort)0
                        : unchecked((ushort)(body.Health - damage));
                    body.FlashTimer = 12;
                    body.AiHandlerBits = unchecked((ushort)(body.AiHandlerBits | 2));
                }
                state.HurtFrame = 6;
                state.HurtFrameTimer = 2;
                if ((state.MouthFlags & 2) != 0)
                    state.MouthFlags |= 4;
                if (body.Health == 0 && unchecked((short)(body.VariableA + 0x3ca0)) < 0)
                    BeginKraidDeath(body, state);
                hitCount++;
                continue;
            }

            if (!KraidOuterBodyOverlapsShot(body, state, shot) ||
                !projectiles.TryStartEnemyImpact(bus, sharedProjectiles, shot.SlotIndex))
            {
                continue;
            }
            if ((projectileType & 0x0010) != 0)
                state.MouthFlags |= 1;
            hitCount++;
        }

        if (hitCount != 0 && body.VariableA == (ushort)KraidAiFunction.MainloopThinking)
        {
            body.VariableA = (ushort)KraidAiFunction.InitializeEyeGlow;
            if ((state.MouthFlags & 1) != 0)
                state.MouthFlags |= 0x0302;
        }
        return hitCount;
    }

    private bool KraidMouthHitboxOverlapsShot(
        RoomEnemySlot body,
        ushort hitboxPointer,
        SamusProjectileSlot shot)
    {
        int address = 0xa70000 | hitboxPointer;
        short left = unchecked((short)ReadWord(_bus!, address));
        short top = unchecked((short)ReadWord(_bus!, address + 2));
        short bottom = unchecked((short)ReadWord(_bus!, address + 6));
        int leftBoundary = body.XPosition + left;
        int topBoundary = body.YPosition + top;
        int bottomBoundary = body.YPosition + bottom;
        return shot.YPosition - shot.YRadius - 1 < bottomBoundary &&
            shot.YPosition + shot.YRadius >= topBoundary &&
            shot.XPosition + shot.XRadius >= leftBoundary;
    }

    private bool KraidOuterBodyOverlapsShot(
        RoomEnemySlot body,
        KraidEnemyState state,
        SamusProjectileSlot shot)
    {
        if (state.VulnerableMouthHitbox != 0 &&
            KraidMouthHitboxOverlapsShot(body, state.VulnerableMouthHitbox, shot))
        {
            return true;
        }

        // `$B161` is a staircase contour: every record's first word is also the preceding
        // band's top boundary, followed by its left edge. The final `$8000` sentinel makes
        // the walk total for any signed Y offset without a host-side bounds guess.
        short relativeY = unchecked((short)(shot.YPosition - body.YPosition));
        int offset = 0;
        for (int record = 0; record < 7; record++, offset += 4)
        {
            short nextTop = unchecked((short)ReadWord(_bus!, 0xa7b165 + offset));
            if (relativeY < nextTop)
                continue;
            short left = unchecked((short)ReadWord(_bus!, 0xa7b163 + offset));
            return shot.XPosition + shot.XRadius > body.XPosition + left;
        }
        return false;
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
