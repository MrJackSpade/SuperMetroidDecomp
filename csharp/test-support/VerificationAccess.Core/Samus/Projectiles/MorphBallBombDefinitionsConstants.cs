
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values of <see cref="MorphBallBombDefinitions"/> that only verification reads.</summary>
internal static class MorphBallBombDefinitionsConstants
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
    extension(MorphBallBombDefinitions)
    {
        /// <inheritdoc cref="MorphBallBombDefinitionsConstants.FuseTimers"/>
        internal static int FuseTimers => MorphBallBombDefinitionsConstants.FuseTimers;
        /// <inheritdoc cref="MorphBallBombDefinitionsConstants.XVelocities"/>
        internal static int XVelocities => MorphBallBombDefinitionsConstants.XVelocities;
        /// <inheritdoc cref="MorphBallBombDefinitionsConstants.YSpeeds"/>
        internal static int YSpeeds => MorphBallBombDefinitionsConstants.YSpeeds;
        /// <inheritdoc cref="MorphBallBombDefinitionsConstants.YSubspeeds"/>
        internal static int YSubspeeds => MorphBallBombDefinitionsConstants.YSubspeeds;
    }
}
