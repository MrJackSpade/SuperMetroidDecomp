using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-faithful damage side effects owned by bank-$94 terrain collision.
/// </summary>
/// <remarks>
/// Solid spike blocks are handled synchronously by the directional movement dispatcher.
/// Spike-air is deliberately different: `$94:9B60` samples Samus's bottom, center, and
/// top points during frame-handler alpha, before beta movement. Keeping both entry points
/// here prevents their similar damage values from erasing that timing distinction.
/// </remarks>
public static class SamusTerrainHazardCollision
{
    /// <summary>
    /// Runs the damaging subset of <c>BlockInsideDetection</c> at <c>$94:9B60</c> and
    /// publishes the room-dependent spike-block BTS-zero gate used later during movement.
    /// </summary>
    public static void PrepareFrame(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        AreaId area,
        bool areaBossDefeated)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(samus);

        // `$94:8E83` makes Wrecked Ship's BTS-zero spike blocks harmless until Phantoon's
        // area-boss bit is set. Every other area admits that table entry unconditionally.
        samus.OrdinarySpikeBlockBtsZeroDamageEnabled =
            area != AreaId.WreckedShip || areaBossDefeated;

        SamusInsideBlockSamplePoints.Visit(samus.YPosition, samus.Kinematics.YRadius, (y, _) =>
            VisitInsidePoint(bus, level, samus, samus.XPosition, y));
    }

    /// <summary>Applies `$94:8E83/$8ECF/$8F0A` after a solid spike block is contacted.</summary>
    internal static void ApplySolidSpikeCollision(
        ISnesAddressSpace bus,
        SamusState samus,
        RoomCollisionBlock block)
    {
        if (samus.InvincibilityTimer != 0)
            return;

        // Every other spike-block BTS value selects a harmless reaction.
        var behavior = (SpikeBlockBehavior)block.Behavior;
        if (!Enum.IsDefined(behavior))
            return;
        ushort damage = behavior switch
        {
            SpikeBlockBehavior.Heavy => samus.OrdinarySpikeBlockBtsZeroDamageEnabled
                ? SamusTerrainHazardRomData.HeavySpikeDamage
                : (ushort)0,
            SpikeBlockBehavior.Light or SpikeBlockBehavior.AlternateLight =>
                SamusTerrainHazardRomData.LightSpikeDamage,
            _ => throw new InvalidOperationException($"Undefined SpikeBlockBehavior {behavior}."),
        };
        if (damage == 0)
            return;

        PublishDamage(bus, samus, damage);
    }

    private static void VisitInsidePoint(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        ushort x,
        ushort y)
    {
        RoomCollisionBlock block = level.GetCollisionBlockOrPrefilledSolid(x >> 4, y >> 4);
        if (block.Index < 0 || !SamusBlockCollision.TryResolveExtension(level, ref block))
            return;
        if (block.CollisionType != RoomCollisionType.SpikeAir ||
            block.Behavior != SamusTerrainHazardRomData.DamagingSpikeAirBehavior)
        {
            return;
        }

        // `$94:9866` is disabled during shinespark/Screw-Attack/contact-damage bodies and
        // while the common invincibility timer is nonzero. It also restores the normal-air
        // horizontal table before returning, independently of whether damage was admitted.
        samus.HorizontalSpeed.SelectNormalAirSpeedTable();
        if (samus.HorizontalSpeed.ContactDamageIndex != 0 || samus.InvincibilityTimer != 0)
            return;

        PublishDamage(bus, samus, SamusTerrainHazardRomData.LightSpikeDamage);
    }

    private static void PublishDamage(
        ISnesAddressSpace bus,
        SamusState samus,
        ushort wholeDamage)
    {
        samus.LiquidPhysics.AccumulatePeriodicDamage(subDamage: 0, wholeDamage);
        samus.InvincibilityTimer = SamusTerrainHazardRomData.InvincibilityFrames;
        samus.KnockbackTimer = SamusTerrainHazardRomData.KnockbackFrames;

        // The native expression `((pose_x_dir ^ $0C) & $08) != 0` maps right-facing
        // records to leftward knockback and both left/front-special records to the other
        // direction. A switch over the proven pose discriminator states that intent.
        samus.KnockbackXDirection = samus.ReadFacingDirection(bus) == SamusFacingDirection.Right
            ? (ushort)0
            : (ushort)1;
    }
}
