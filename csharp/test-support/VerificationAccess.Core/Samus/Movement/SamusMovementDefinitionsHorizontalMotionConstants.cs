
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Cartridge values of <see cref="SamusMovementDefinitions.HorizontalMotion"/> that only verification reads.</summary>
internal static class SamusMovementDefinitionsHorizontalMotionConstants
{
    /// <summary><c>$91:B61F</c>, initial/next low byte for each Speed Booster stage.</summary>
    public const int SpeedBoostCounterLowBytes = 0x91b61f;

    /// <summary>Provides speed-boost counter low bytes used to check horizontal movement state.</summary>
    extension(SamusMovementDefinitions.HorizontalMotion)
    {
        /// <inheritdoc cref="SamusMovementDefinitionsHorizontalMotionConstants.SpeedBoostCounterLowBytes"/>
        internal static int SpeedBoostCounterLowBytes => SamusMovementDefinitionsHorizontalMotionConstants.SpeedBoostCounterLowBytes;
    }
}
