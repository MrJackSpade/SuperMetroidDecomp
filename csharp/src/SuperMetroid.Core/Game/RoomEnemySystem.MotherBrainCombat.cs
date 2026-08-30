namespace SuperMetroid.Core.Game;

/// <summary>Mother Brain's encounter-specific projectile callbacks from bank <c>$A9</c>.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Ports the first-form branch of head shot AI <c>$A9:B507</c>. The glass encounter
    /// accepts only missile families; beams pass through without being converted into an
    /// impact, while every accepted missile increments the room PLM argument before common
    /// vulnerability damage is applied without the ordinary enemy-deletion tail.
    /// </summary>
    private bool ResolveMotherBrainHeadShot(
        Hardware.ISnesAddressSpace bus,
        RoomEnemySlot head,
        SamusProjectileSlot projectile,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles,
        ushort projectileType,
        ushort projectileDamage)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidOperationException(
            "Mother Brain head shot AI ran without its multipart encounter state.");
        if (!ReferenceEquals(state.Head, head))
        {
            throw new InvalidOperationException(
                "Mother Brain head shot AI ran for a slot not linked to the loaded body.");
        }
        if (state.Form != 0)
        {
            throw new NotSupportedException(
                $"Mother Brain form ${state.Form:X4} shot behavior is not translated.");
        }

        SamusProjectileFamily family = (SamusProjectileFamily)(projectileType & 0x0f00);
        if (family is not (SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile))
            return true;

        if (!projectiles.TryStartEnemyImpact(bus, sharedProjectiles, projectile.SlotIndex))
            return false;

        (_incrementMotherBrainGlassRoomArgument ?? throw new InvalidOperationException(
            "Mother Brain head damage has no loaded glass-PLM argument writer."))();

        // `$B52D-$B539` deliberately alternates odd/even flash parity when a second hit
        // arrives during the existing flash. The common no-death damage helper then owns
        // health, hurt-AI, and the final ROM-authored hurt duration.
        head.FlashTimer = head.FlashTimer != 0 && (head.FlashTimer & 1) != 0
            ? (ushort)14
            : (ushort)13;

        byte vulnerability = ReadProjectileVulnerability(bus, head, projectileType);
        if (vulnerability == 0xff)
        {
            head.FrozenTimer = 400;
            head.AiHandlerBits = unchecked((ushort)(head.AiHandlerBits | 0x0004));
            head.InvincibilityTimer = 10;
            return true;
        }

        int damage = (projectileDamage >> 1) * (vulnerability & 0x7f);
        if (damage == 0)
        {
            CreateEnemyProjectileDudShot(projectile);
            return true;
        }

        ushort hurtTime = head.HurtAiTime == 0 ? (ushort)4 : head.HurtAiTime;
        head.FlashTimer = unchecked((ushort)(hurtTime + 8));
        head.AiHandlerBits = unchecked((ushort)(head.AiHandlerBits | 0x0002));
        head.Health = damage >= head.Health
            ? (ushort)0
            : unchecked((ushort)(head.Health - damage));
        return true;
    }
}
