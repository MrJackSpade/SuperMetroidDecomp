using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The damage spike terrain deals Samus.
/// </summary>
/// <remarks>
/// Solid spike blocks are handled synchronously by the directional movement step.
/// Spike-air is deliberately different: the inside-block reaction samples Samus's bottom, center, and
/// top points during the input phase, before movement. Keeping both checks
/// here prevents their similar damage values from erasing that timing distinction.
/// </remarks>
public static class SamusTerrainHazardCollision
{
    /// <summary>
    /// Runs the damaging subset of Samus's inside-block detection and
    /// samples the room's heavy-spike gate (<see cref="RoomLevelData.HeavySpikesDamage"/>) her movement reads later.
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
        level.HeavySpikesDamage =
            area != AreaId.WreckedShip || areaBossDefeated;

        SamusInsideBlockSamplePoints.Visit(samus.Position.Y.Whole, samus.Kinematics.Radius.Y, (y, _) =>
            VisitInsidePoint(level, samus, Position16<RoomPixels>.Of(samus.Position.X.Whole, y)));
    }

    /// <summary>Applies the solid spike reactions after a solid spike block of <paramref name="level"/> is contacted.</summary>
    /// <param name="level">The room's level, whose heavy spikes may be harmless.</param>
    /// <param name="samus">Samus, whom a damaging spike hurts.</param>
    /// <param name="spike">The reaction the contacted spike block carries.</param>
    internal static void ApplySolidSpikeCollision(
        RoomLevelData level,
        SamusState samus,
        SpikeReaction spike)
    {
        if (!samus.InvincibilityTimer.IsZero)
            return;

        // Every other spike-block reaction is harmless.
        if (!SpikeHazardDefinitions.DamagingSpikeBlocks.TryGetValue(spike, out SpikeBlockBehavior behavior))
            return;
        // Heavy spikes are harmless in a level whose heavy spikes do no damage.
        if (behavior == SpikeBlockBehavior.Heavy && !level.HeavySpikesDamage)
            return;
        int damage = behavior switch
        {
            SpikeBlockBehavior.Heavy => SpikeHazardDefinitions.HeavySpikeDamage,
            SpikeBlockBehavior.Light =>
                SpikeHazardDefinitions.LightSpikeDamage,
            _ => throw new InvalidOperationException($"Undefined SpikeBlockBehavior {behavior}."),
        };

        PublishDamage(samus, damage);
    }

    /// <summary>Tests one input-phase body sample for damaging spike-air, restoring normal-air speed selection before the invincibility and contact-damage gates.</summary>
    private static void VisitInsidePoint(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomPixels> point)
    {
        RoomCollisionBlock block = level.GetCollisionBlockOrEdgeSolid(point.ToRoomBlock());
        if (block.Position is null || !level.TryReachBlockStoodFor(ref block))
            return;
        if (block.CollisionType != RoomCollisionType.SpikeAir ||
            block.Behavior != SpikeHazardDefinitions.DamagingSpikeAirBehavior)
        {
            return;
        }

        // The spike-air reaction is disabled during shinespark/Screw-Attack/contact-damage bodies and
        // while Samus is invincible. It also restores the ordinary air speeds before returning, whether or not damage
        // was admitted.
        samus.HorizontalSpeed.SelectAirSpeeds();
        if (samus.HorizontalSpeed.BodyAttack != SamusBodyAttack.None || !samus.InvincibilityTimer.IsZero)
            return;

        PublishDamage(samus, SpikeHazardDefinitions.LightSpikeDamage);
    }

    /// <summary>Applies spike damage and the invincibility, knockback duration, and facing-dependent direction side effects.</summary>
    private static void PublishDamage(
        SamusState samus,
        int wholeDamage)
    {
        samus.PeriodicDamage.Accumulate(EnergyAmount.FromEnergy(wholeDamage));
        samus.InvincibilityTimer = WrappingCountdown.Start(SpikeHazardDefinitions.InvincibilityFrames);
        samus.KnockbackHold = UpdateCountdown.For(SpikeHazardDefinitions.KnockbackFrames);

        // A right-facing pose is knocked left, and every other pose, front-facing ones included, is knocked right.
        samus.KnockbackXDirection = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Right
            ? KnockbackHorizontalDirection.Left
            : KnockbackHorizontalDirection.Right;
    }
}
