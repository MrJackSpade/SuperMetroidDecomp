
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values of <see cref="SamusProjectileRomData.Trails"/> that only verification reads.</summary>
internal static class SamusProjectileRomDataTrailsConstants
{
    /// <summary>Number of trail pointer entries before the paired right table.</summary>
    public const int InstructionPointerCount = (SamusProjectileRomData.Trails.RightInstructionPointers - SamusProjectileRomData.Trails.LeftInstructionPointers) / sizeof(ushort);

    /// <summary>Provides the number of native trail instruction pointers for projectile trail decoding.</summary>
    extension(SamusProjectileRomData.Trails)
    {
        /// <inheritdoc cref="SamusProjectileRomDataTrailsConstants.InstructionPointerCount"/>
        internal static int InstructionPointerCount => SamusProjectileRomDataTrailsConstants.InstructionPointerCount;
    }
}
