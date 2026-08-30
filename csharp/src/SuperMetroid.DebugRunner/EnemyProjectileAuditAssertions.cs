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
    /// Isolates one naturally initialized projectile and proves the complete common
    /// $A0:A306 Samus-contact tail: literal damage, invincibility, knockback, cartridge
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
        samus.Pose = SamusState.FacingRightNormalPose;
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
            !samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Natural enemy projectile {kind} ($86:{(ushort)kind:X4}) contact produced " +
                $"health/invincibility/knockback {samus.Health}/" +
                $"{samus.InvincibilityTimer}/{samus.KnockbackTimer}/{samus.KnockbackActive}; " +
                $"expected {expectedHealth}/{invincibilityFrames}/5/true from damage {damage}.");
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

        foreach ((RoomEnemyProjectileSlot projectile, bool canDamage) in siblingCollision)
        {
            if (projectile.IsActive)
                projectile.CanDamageSamus = canDamage;
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
