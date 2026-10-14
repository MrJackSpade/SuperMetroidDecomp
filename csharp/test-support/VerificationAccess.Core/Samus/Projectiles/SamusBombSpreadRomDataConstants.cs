
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values of <see cref="SamusBombSpreadRomData"/> that only verification reads.</summary>
internal static class SamusBombSpreadRomDataConstants
{
    /// <summary>Five 16-bit fuse words beginning at $90:D8CF.</summary>
    public const int FuseTimers = 0x90d8cf;
    /// <summary>Five native direction/magnitude X-velocity words beginning at $90:D8D9.</summary>
    public const int XVelocities = 0x90d8d9;
    /// <summary>Five whole-pixel initial Y-speed words beginning at $90:D8E3.</summary>
    public const int YSpeeds = 0x90d8e3;
    /// <summary>Five fractional initial Y-speed words beginning at $90:D8ED.</summary>
    public const int YSubspeeds = 0x90d8ed;

    /// <summary>Provides native bomb-spread fuse and velocity tables for Samus projectile comparisons.</summary>
    extension(SamusBombSpreadRomData)
    {
        /// <inheritdoc cref="SamusBombSpreadRomDataConstants.FuseTimers"/>
        internal static int FuseTimers => SamusBombSpreadRomDataConstants.FuseTimers;
        /// <inheritdoc cref="SamusBombSpreadRomDataConstants.XVelocities"/>
        internal static int XVelocities => SamusBombSpreadRomDataConstants.XVelocities;
        /// <inheritdoc cref="SamusBombSpreadRomDataConstants.YSpeeds"/>
        internal static int YSpeeds => SamusBombSpreadRomDataConstants.YSpeeds;
        /// <inheritdoc cref="SamusBombSpreadRomDataConstants.YSubspeeds"/>
        internal static int YSubspeeds => SamusBombSpreadRomDataConstants.YSubspeeds;
    }
}
