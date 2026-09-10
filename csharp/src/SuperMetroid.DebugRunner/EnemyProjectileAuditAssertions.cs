using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Domain assertions shared by focused enemy-family audits and exhaustive retail sweeps.
/// These helpers accept only a live projectile produced by real enemy AI; they do not
/// manufacture definitions or bypass bank-$86 initialization.
/// </summary>
internal static class EnemyProjectileAuditAssertions
{
    /// <summary>
    /// Arms one physical slot in the exact timer-zero family-$0500 state consumed by
    /// <c>EnemyBombCollHandler</c>. Focused audits supply only position and damage; keeping
    /// the shared collision radii, active sentinel, and explosion state here prevents each
    /// enemy-family test from inventing a subtly different synthetic bomb fixture.
    /// </summary>
    public static SamusBombProjectileSlot ArmExplodingNormalBomb(
        SamusBombProjectileSystem bombs,
        ushort x,
        ushort y,
        ushort damage = 100)
    {
        ArgumentNullException.ThrowIfNull(bombs);
        SamusBombProjectileSlot bomb = bombs.Slots[0];
        bomb.Type = SamusBombProjectileSystem.NormalBombType;
        bomb.Damage = damage;
        bomb.XPosition = x;
        bomb.YPosition = y;
        bomb.XRadius = 24;
        bomb.YRadius = 24;
        bomb.InstructionPointer = 0xa06b;
        bomb.InstructionTimer = 1;
        bomb.BombTimer = 0;
        return bomb;
    }

    /// <summary>
    /// Isolates one naturally initialized projectile and proves the complete common
    /// $A0:9923 Samus-contact tail: literal damage, invincibility, pending knockback, cartridge
    /// touch-list installation, and property-$4000 persistence/deletion.
    /// </summary>
    public static void VerifyNaturalSamusContact(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        SamusState samus,
        SamusBombProjectileSystem sharedProjectiles,
        RoomLevelData level,
        RoomEnemyProjectileSlot target,
        ushort cameraX,
        ushort cameraY,
        byte frame = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(enemies);
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(target);
        if (!target.IsActive || !target.CanDamageSamus)
        {
            throw new InvalidDataException(
                "Enemy projectile contact assertion requires a live, damage-enabled actor.");
        }

        RoomEnemyProjectileKind kind = target.Kind;
        ushort damage = target.Damage;
        ushort invincibilityFrames = target.InvincibilityFrames;
        bool persists = target.PersistsOnSamusContact;
        int targetSlot = target.SlotIndex;

        // Preserve sibling actors exactly except for their collision-enable bit during this
        // one dispatcher pass. This makes exact damage deterministic even for shared-pool
        // families that launch several overlapping actors in a single frame.
        (RoomEnemyProjectileSlot Projectile, bool CanDamage)[] siblingCollision =
            enemies.EnemyProjectiles
                .Where(projectile => projectile.IsActive && projectile.SlotIndex != targetSlot)
                .Select(projectile => (projectile, projectile.CanDamageSamus))
                .ToArray();
        foreach ((RoomEnemyProjectileSlot projectile, _) in siblingCollision)
            projectile.CanDamageSamus = false;

        // $8170 is bank $86's canonical RTS pre-instruction. Timer two becomes one without
        // parsing a new animation command. Those two narrowly scoped writes prevent movement
        // or a list transition from racing the collision probe while leaving every authored
        // definition field under test untouched.
        target.PreInstruction = 0x8170;
        target.InstructionTimer = 2;

        // A producer scenario may previously have touched Samus through a body or scripted
        // route. Reinitialize a normal-pose target so this assertion tests the projectile,
        // not a stale bank-$90 special-movement request from an unrelated attack.
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.XPosition = target.XPosition;
        samus.YPosition = target.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
        samus.KnockbackActive = false;
        samus.KnockbackDirection = 0;
        samus.KnockbackXDirection = 0;
        samus.HorizontalSpeed.ContactDamageIndex = 0;

        enemies.StepEnemyProjectiles(
            level,
            samus,
            controllerInput: 0,
            cameraX: cameraX,
            cameraY: cameraY,
            nmiFrameCounter8: frame,
            samusBombs: sharedProjectiles);

        ushort expectedHealth = damage >= 999 ? (ushort)0 : unchecked((ushort)(999 - damage));
        if (samus.Health != expectedHealth ||
            samus.InvincibilityTimer != invincibilityFrames ||
            samus.KnockbackTimer != 5 ||
            samus.KnockbackActive || samus.KnockbackDirection != 0 ||
            samus.KnockbackXDirection != 1 || samus.Pose != SamusPoseIds.FacingRightNormalPose)
        {
            throw new InvalidDataException(
                $"Natural enemy projectile {kind} ($86:{(ushort)kind:X4}) contact produced " +
                $"health/invincibility/knockback {samus.Health}/" +
                $"{samus.InvincibilityTimer}/{samus.KnockbackTimer}/{samus.KnockbackActive}; " +
                $"expected {expectedHealth}/{invincibilityFrames}/5/false with a pending rightward hit " +
                $"and unchanged standing pose, from damage {damage}.");
        }

        RoomEnemyProjectileSlot physicalTarget = enemies.EnemyProjectiles[targetSlot];
        if (physicalTarget.IsActive != persists)
        {
            throw new InvalidDataException(
                $"Natural enemy projectile {kind} ($86:{(ushort)kind:X4}) persistence was " +
                $"{physicalTarget.IsActive}; cartridge property requires {persists}.");
        }

        if (persists)
        {
            ushort touchInstruction = ReadWord(
                bus,
                0x860000 | unchecked((ushort)((ushort)kind + 10)));
            if (touchInstruction != 0 &&
                (physicalTarget.InstructionPointer != touchInstruction ||
                 physicalTarget.InstructionTimer != 1))
            {
                throw new InvalidDataException(
                    $"Persistent enemy projectile {kind} ($86:{(ushort)kind:X4}) did not " +
                    $"install cartridge touch list $86:{touchInstruction:X4}; got " +
                    $"$86:{physicalTarget.InstructionPointer:X4}/" +
                    $"timer {physicalTarget.InstructionTimer}.");
            }
        }

        // Bank $A0 only publishes the timer/direction. Prove the later bank-$90
        // consumer admits that request and installs the actual hurt pose/handler.
        if (!SamusKnockbackMovement.TryStartPendingHitInterruption(
                bus, samus, 0, timeIsFrozen: false, level: level, nmiFrameCounter: frame) ||
            !samus.KnockbackActive || samus.KnockbackDirection != 2 ||
            samus.Pose != SamusPoseIds.KnockbackRightPose)
        {
            throw new InvalidDataException(
                $"Enemy projectile {kind} published a hit but its deferred knockback handoff failed.");
        }

        foreach ((RoomEnemyProjectileSlot projectile, bool canDamage) in siblingCollision)
        {
            if (projectile.IsActive)
                projectile.CanDamageSamus = canDamage;
        }
    }

    /// <summary>
    /// Isolates one naturally initialized, flag-zero enemy projectile and proves the common
    /// <c>$A0:996C-$9A30</c> Samus-shot dispatcher. The assertion deliberately creates only
    /// the incoming power-beam test shot: the target's kind, radii, properties, animation,
    /// and producer-owned state must all have come from the retail encounter under audit.
    /// </summary>
    /// <returns>The cartridge shot-response instruction read from definition word +12.</returns>
    public static ushort VerifyNaturalDestructibleSamusShot(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        SamusProjectileSystem samusProjectiles,
        SamusBombProjectileSystem sharedProjectiles,
        RoomEnemyProjectileSlot target)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(enemies);
        ArgumentNullException.ThrowIfNull(samusProjectiles);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);
        ArgumentNullException.ThrowIfNull(target);
        if (!target.IsActive || !target.BlocksSamusProjectiles || target.CollisionOption != 0)
        {
            throw new InvalidDataException(
                "Enemy projectile shot assertion requires a live, blocking flag-zero actor.");
        }

        RoomEnemyProjectileKind kind = target.Kind;
        int targetSlot = target.SlotIndex;
        ushort expectedShotInstruction = ReadWord(
            bus,
            0x860000 | unchecked((ushort)((ushort)kind + 12)));
        if (expectedShotInstruction == 0)
        {
            throw new InvalidDataException(
                $"Destructible enemy projectile {kind} ($86:{(ushort)kind:X4}) has no " +
                "cartridge shot-response instruction.");
        }

        // The native collision pass scans every blocking bank-$86 slot and every live Samus
        // shot. Preserve sibling state while masking only their blocking bit, and clear the
        // ordinary five-shot pool so exactly one physical collision is attributable here.
        (RoomEnemyProjectileSlot Projectile, bool Blocks)[] siblingCollision =
            enemies.EnemyProjectiles
                .Where(projectile => projectile.IsActive && projectile.SlotIndex != targetSlot)
                .Select(projectile => (projectile, projectile.BlocksSamusProjectiles))
                .ToArray();
        foreach ((RoomEnemyProjectileSlot projectile, _) in siblingCollision)
            projectile.BlocksSamusProjectiles = false;
        foreach (SamusProjectileSlot projectile in samusProjectiles.Slots.Take(5))
            projectile.ClearFields();

        // Type $0001 is the ordinary power beam. Its non-bit-3 form also proves that the
        // Samus projectile receives direction lifecycle state, not the unrelated ordinary
        // enemy-impact animation, before the enemy projectile installs its shot list.
        SamusProjectileSlot shot = samusProjectiles.Slots[0];
        shot.Type = 0x0001;
        shot.Damage = 20;
        shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.XPosition = target.XPosition;
        shot.YPosition = target.YPosition;
        shot.XRadius = 4;
        shot.YRadius = 4;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 1;

        ushort? dudBefore = enemies.LastEnemyProjectileDudSoundEffect;
        int hitCount;
        try
        {
            hitCount = enemies.ResolveEnemyProjectileSamusProjectileHits(
                bus,
                samusProjectiles,
                sharedProjectiles);
        }
        finally
        {
            foreach ((RoomEnemyProjectileSlot projectile, bool blocks) in siblingCollision)
            {
                if (projectile.IsActive)
                    projectile.BlocksSamusProjectiles = blocks;
            }
        }

        RoomEnemyProjectileSlot physicalTarget = enemies.EnemyProjectiles[targetSlot];
        if (hitCount != 1 || !physicalTarget.IsActive ||
            physicalTarget.CollidedProjectileType != 0x0001 ||
            physicalTarget.InstructionPointer != expectedShotInstruction ||
            physicalTarget.InstructionTimer != 1 || physicalTarget.PreInstruction != 0x84fb ||
            physicalTarget.BlocksSamusProjectiles ||
            physicalTarget.PersistsOnSamusContact || !physicalTarget.CanDamageSamus ||
            enemies.LastEnemyProjectileDudSoundEffect != dudBefore)
        {
            throw new InvalidDataException(
                $"Natural enemy projectile {kind} ($86:{(ushort)kind:X4}) shot dispatch " +
                $"produced hits/live/type/list/timer/pre={hitCount}/{physicalTarget.IsActive}/" +
                $"${physicalTarget.CollidedProjectileType:X4}/" +
                $"$86:{physicalTarget.InstructionPointer:X4}/{physicalTarget.InstructionTimer}/" +
                $"$86:{physicalTarget.PreInstruction:X4}, properties damage/persist/block=" +
                $"{physicalTarget.CanDamageSamus}/{physicalTarget.PersistsOnSamusContact}/" +
                $"{physicalTarget.BlocksSamusProjectiles}; expected one live $0001 hit, " +
                $"$86:{expectedShotInstruction:X4}/1/$84FB, true/false/false.");
        }

        if (shot.Type != 0x0001 || !shot.PackedDirection.HasLowByteLifecycleState ||
            shot.InstructionPointer != 0x9000)
        {
            throw new InvalidDataException(
                $"Natural enemy projectile {kind} rewrote its Samus shot instead of " +
                $"marking lifecycle state: type/direction/list=" +
                $"${shot.Type:X4}/${shot.Direction:X4}/${shot.InstructionPointer:X4}.");
        }

        return expectedShotInstruction;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
