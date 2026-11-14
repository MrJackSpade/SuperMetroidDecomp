using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Faithful damage side effects owned by terrain collision.
/// </summary>
/// <remarks>
/// Solid spike blocks are handled synchronously by the directional movement dispatcher.
/// Spike-air is deliberately different: the inside-block reaction samples Samus's bottom, center, and
/// top points during the input phase, before movement. Keeping both entry points
/// here prevents their similar damage values from erasing that timing distinction.
/// </remarks>
public static class SamusTerrainHazardCollision
{
    /// <summary>
    /// Runs the damaging subset of Samus's inside-block detection and
    /// publishes the room-dependent heavy-spike gate used later during movement.
    /// </summary>
    public static void PrepareFrame(
        RoomLevelData level,
        SamusState samus,
        AreaId area,
        bool areaBossDefeated)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);

        // The spike reaction makes Wrecked Ship's heavy spike blocks harmless until Phantoon is defeated.
        // Every other area admits that reaction unconditionally.
        samus.HeavySpikeDamageEnabled =
            area != AreaId.WreckedShip || areaBossDefeated;

        SamusInsideBlockSamplePoints.Visit(samus.Position.Y.Whole, samus.Kinematics.Radius.Y, (y, _) =>
            VisitInsidePoint(level, samus, Position16<RoomPixels>.Of(samus.Position.X.Whole, y)));
    }

    /// <summary>Applies the solid spike reactions after a solid spike block is contacted.</summary>
    internal static void ApplySolidSpikeCollision(
        SamusState samus,
        RoomCollisionBlock block)
    {
        if (!samus.InvincibilityTimer.IsZero)
            return;

        // Every other spike-block reaction, and a spike with no reaction, is harmless.
        if (block.Behavior.Spike is not { } spike ||
            !SpikeHazardDefinitions.DamagingSpikeBlocks.TryGetValue(spike, out SpikeBlockBehavior behavior))
            return;
        int damage = behavior switch
        {
            SpikeBlockBehavior.Heavy => samus.HeavySpikeDamageEnabled
                ? SpikeHazardDefinitions.HeavySpikeDamage
                : 0,
            SpikeBlockBehavior.Light or SpikeBlockBehavior.AlternateLight =>
                SpikeHazardDefinitions.LightSpikeDamage,
            _ => throw new InvalidOperationException($"Undefined SpikeBlockBehavior {behavior}."),
        };
        if (damage == 0)
            return;

        PublishDamage(samus, damage);
    }

    /// <summary>Tests one input-phase body sample for damaging spike-air, restoring normal-air speed selection before the invincibility and contact-damage gates.</summary>
    private static void VisitInsidePoint(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomPixels> point)
    {
        RoomCollisionBlock block = level.GetCollisionBlockOrEdgeSolid(point.ToRoomBlock());
        if (block.Position is null || !SamusBlockCollision.TryResolveExtension(level, ref block))
            return;
        if (block.CollisionType != RoomCollisionType.SpikeAir ||
            block.Behavior != SpikeHazardDefinitions.DamagingSpikeAirBehavior)
        {
            return;
        }

        // The spike-air reaction is disabled during shinespark/Screw-Attack/contact-damage bodies and
        // while the common invincibility timer is nonzero. It also restores the normal-air
        // horizontal table before returning, independently of whether damage was admitted.
        samus.HorizontalSpeed.SelectAirSpeeds();
        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None || !samus.InvincibilityTimer.IsZero)
            return;

        PublishDamage(samus, SpikeHazardDefinitions.LightSpikeDamage);
    }

    /// <summary>Applies spike damage and the invincibility, knockback duration, and facing-dependent direction side effects.</summary>
    private static void PublishDamage(
        SamusState samus,
        int wholeDamage)
    {
        samus.LiquidPhysics.AccumulatePeriodicDamage(subDamage: 0, wholeDamage);
        samus.InvincibilityTimer = WrappingCountdown.Start(SpikeHazardDefinitions.InvincibilityFrames);
        samus.KnockbackTimer = SpikeHazardDefinitions.KnockbackFrames;

        // The game's facing test maps right-facing records to leftward knockback and both
        // left/front-special records to the other direction. A switch over the proven pose discriminator states that intent.
        samus.KnockbackXDirection = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Right
            ? KnockbackHorizontalDirection.Left
            : KnockbackHorizontalDirection.Right;
    }
}
