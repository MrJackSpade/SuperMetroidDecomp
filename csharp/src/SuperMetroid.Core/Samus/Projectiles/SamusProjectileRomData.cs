namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Immutable cartridge tables and instruction pointers for Samus projectiles.</summary>
/// <remarks>
/// The native implementation spreads projectile metadata across banks $90, $91, $93,
/// $94, $9A, and $9B. Grouping by purpose keeps those cross-bank relationships explicit
/// while leaving live projectile slots, timers, and positions on the runtime system.
/// </remarks>
public static class SamusProjectileRomData
{
    /// <summary>$91:F8F3-$F903: tag on the prior Moonwalk muzzle direction, consumed by turning HUD dispatch.</summary>
    public const ushort MoonwalkPoseHandoffTag = 0x0100;

    /// <summary>$91:F5E3: normal-jump initializer tags the new pose's shot direction for the next HUD producer.</summary>
    public const ushort NormalJumpPoseHandoffTag = 0x8000;

    /// <summary>Native banks used to expand same-bank projectile pointers.</summary>
    public static class Banks
    {
        /// <summary>Bank containing projectile producers, motion tables, and trails.</summary>
        public const int Movement = 0x900000;
        /// <summary>Bank containing pose and projectile-driven suit-palette lists.</summary>
        public const int Pose = 0x910000;
        /// <summary>Bank containing projectile definitions and animation instructions.</summary>
        public const int Projectile = 0x930000;
        /// <summary>Bank containing projectile character graphics.</summary>
        public const int CharacterData = 0x9a0000;
        /// <summary>Bank containing Samus palettes and beam-trail offsets.</summary>
        public const int PaletteAndTrailData = 0x9b0000;
    }

    /// <summary>Pose-relative projectile and charge-flare origins.</summary>
    public static class Origins
    {
        /// <summary>Number of projectile directions represented by each origin table.</summary>
        public const int DirectionCount = 10;
    }

    /// <summary>Beam cooldown, audio, speed, acceleration, graphics, and palette tables.</summary>
    public static class Beams
    {
        /// <summary>Flare-counter value at which <c>$90:BAFC</c> starts SFX `$08`.</summary>
        public const ushort ChargeSoundStartCounter = 16;
        /// <summary>Flare-counter value at which release selects the charged beam family.</summary>
        public const ushort FullyChargedCounter = 60;
        /// <summary>Flare-counter clamp at which bank $90 begins testing special beam attacks.</summary>
        public const ushort SpecialAttackCounter = 120;
        /// <summary>Offset from uncharged delays to the charged-shot delay row.</summary>
        public const int ChargedRowOffset = 0x10;
        /// <summary>$90:B197 indexes $C2D1 by four times the low-nibble beam combination: two speed words per row.</summary>
        public const int InitialSpeedRowBytes = 2 * sizeof(ushort);
        /// <summary>Number of beam combinations present in each complete pointer table.</summary>
        public const int CombinationCount = 12;
    }

    /// <summary>Projectile-driven Samus and beam palette pointer tables.</summary>
    public static class Palettes
    {
        /// <summary>$90:ACCD writes the sixteen beam colors beginning at CGRAM index $E0.</summary>
        public const int BeamFirstColour = 0xe0;
    }

    /// <summary>Missile, Super Missile, bomb, and linked-projectile definitions.</summary>
    public static class NonBeam
    {
        /// <summary>$90:AC86, Samus_CanFireSuperMissile: reject at four live projectiles, reserving the fifth slot for the collision link.</summary>
        public const int SuperMissileAdmissionLimit = 4;
        /// <summary>$93:8125, KillProjectileInner: one-pixel diagonal quake affecting both BGs and enemies.</summary>
        public const ushort SuperMissileEarthquakeType = 20;
        /// <summary>$93:812B, KillProjectileInner: thirty room-shaking calls after Super Missile impact.</summary>
        public const ushort SuperMissileEarthquakeDuration = 30;
        /// <summary>
        /// $A0:9CB7 / $A0:A1E9, enemy/projectile collision: a Super Missile touching an enemy
        /// starts this quake for the same thirty calls. Crawlers fall only for the impact's
        /// type, so an enemy hit leaves them crawling.
        /// </summary>
        public const ushort SuperMissileEnemyHitEarthquakeType = 18;
    }

    /// <summary>Shared bank-$93 projectile instruction opcodes.</summary>
    public static class Instructions
    {
        /// <summary>Bytes in one ordinary bank-$93 timer/spritemap/radius/frame record.</summary>
        public const int AnimationRecordByteCount = 8;
    }
}
