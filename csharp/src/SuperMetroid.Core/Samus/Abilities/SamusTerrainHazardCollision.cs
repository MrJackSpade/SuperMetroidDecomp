using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

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
        RoomLevelData level,
        SamusState samus,
        AreaId area,
        bool areaBossDefeated)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);

        // `$94:8E83` makes Wrecked Ship's BTS-zero spike blocks harmless until Phantoon's
        // area-boss bit is set. Every other area admits that table entry unconditionally.
        samus.OrdinarySpikeBlockBtsZeroDamageEnabled =
            area != AreaId.WreckedShip || areaBossDefeated;

        SamusInsideBlockSamplePoints.Visit(samus.Position.Y.Whole, samus.Kinematics.Radius.Y, (y, _) =>
            VisitInsidePoint(level, samus, new(samus.Position.X.Whole, y)));
    }

    /// <summary>Applies `$94:8E83/$8ECF/$8F0A` after a solid spike block is contacted.</summary>
    internal static void ApplySolidSpikeCollision(
        SamusState samus,
        RoomCollisionBlock block)
    {
        if (!samus.InvincibilityTimer.IsZero)
            return;

        // Every other spike-block BTS value selects a harmless reaction.
        var behavior = (SpikeBlockBehavior)block.Bts.Value;
        if (!Enum.IsDefined(behavior))
            return;
        ushort damage = behavior switch
        {
            SpikeBlockBehavior.Heavy => samus.OrdinarySpikeBlockBtsZeroDamageEnabled
                ? SpikeHazardDefinitions.HeavySpikeDamage
                : (ushort)0,
            SpikeBlockBehavior.Light or SpikeBlockBehavior.AlternateLight =>
                SpikeHazardDefinitions.LightSpikeDamage,
            _ => throw new InvalidOperationException($"Undefined SpikeBlockBehavior {behavior}."),
        };
        if (damage == 0)
            return;

        PublishDamage(samus, damage);
    }

    /// <summary>Tests one alpha-phase body sample for damaging spike-air, restoring normal-air speed selection before the invincibility and contact-damage gates.</summary>
    private static void VisitInsidePoint(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomPixels> point)
    {
        RoomCollisionBlock block = level.GetCollisionBlockOrPrefilledSolid(new(unchecked((ushort)(point.X >> 4)), unchecked((ushort)(point.Y >> 4))));
        if (block.Index < 0 || !SamusBlockCollision.TryResolveExtension(level, ref block))
            return;
        if (block.CollisionType != RoomCollisionType.SpikeAir ||
            block.Bts.Value != SpikeHazardDefinitions.DamagingSpikeAirBehavior)
        {
            return;
        }

        // `$94:9866` is disabled during shinespark/Screw-Attack/contact-damage bodies and
        // while the common invincibility timer is nonzero. It also restores the normal-air
        // horizontal table before returning, independently of whether damage was admitted.
        samus.HorizontalSpeed.SelectNormalAirSpeedTable();
        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None || !samus.InvincibilityTimer.IsZero)
            return;

        PublishDamage(samus, SpikeHazardDefinitions.LightSpikeDamage);
    }

    /// <summary>Applies spike damage and the native invincibility, knockback duration, and facing-dependent direction side effects.</summary>
    private static void PublishDamage(
        SamusState samus,
        ushort wholeDamage)
    {
        samus.LiquidPhysics.AccumulatePeriodicDamage(subDamage: 0, wholeDamage);
        samus.InvincibilityTimer = WrappingCountdown.Start(SpikeHazardDefinitions.InvincibilityFrames);
        samus.KnockbackTimer = WrappingCountdown.Start(SpikeHazardDefinitions.KnockbackFrames);

        // The native expression `((pose_x_dir ^ $0C) & $08) != 0` maps right-facing
        // records to leftward knockback and both left/front-special records to the other
        // direction. A switch over the proven pose discriminator states that intent.
        samus.KnockbackXDirection = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Right
            ? KnockbackHorizontalDirection.Left
            : KnockbackHorizontalDirection.Right;
    }
}
