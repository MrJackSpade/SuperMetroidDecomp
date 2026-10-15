
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Cartridge values of <see cref="SamusMovementRomData.HorizontalMotion"/> that only verification reads.</summary>
internal static class SamusMovementRomDataHorizontalMotionConstants
{
    /// <summary><c>$91:B61F</c>, initial/next low byte for each Speed Booster stage.</summary>
    public const int SpeedBoostCounterLowBytes = 0x91b61f;

    /// <summary>Provides speed-boost counter low bytes used to check horizontal movement state.</summary>
    extension(SamusMovementRomData.HorizontalMotion)
    {
        /// <inheritdoc cref="SamusMovementRomDataHorizontalMotionConstants.SpeedBoostCounterLowBytes"/>
        internal static int SpeedBoostCounterLowBytes => SamusMovementRomDataHorizontalMotionConstants.SpeedBoostCounterLowBytes;
    }
}
