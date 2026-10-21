
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values of <see cref="SamusProjectileRomData.Trails"/> that only verification reads.</summary>
internal static class SamusProjectileRomDataTrailsConstants
{
    /// <summary>$90:B5BB, the left trail program table.</summary>
    public const int LeftInstructionPointers = 0x90b5bb;

    /// <summary>$90:B609, the right trail program table.</summary>
    public const int RightInstructionPointers = 0x90b609;

    /// <summary>Number of trail pointer entries before the paired right table.</summary>
    public const int InstructionPointerCount = (RightInstructionPointers - LeftInstructionPointers) / sizeof(ushort);

    /// <summary>Provides the number of native trail instruction pointers for projectile trail decoding.</summary>
    extension(SamusProjectileRomData.Trails)
    {
        /// <inheritdoc cref="SamusProjectileRomDataTrailsConstants.InstructionPointerCount"/>
        internal static int InstructionPointerCount => SamusProjectileRomDataTrailsConstants.InstructionPointerCount;
        /// <inheritdoc cref="SamusProjectileRomDataTrailsConstants.LeftInstructionPointers"/>
        internal static int LeftInstructionPointers => SamusProjectileRomDataTrailsConstants.LeftInstructionPointers;
        /// <inheritdoc cref="SamusProjectileRomDataTrailsConstants.RightInstructionPointers"/>
        internal static int RightInstructionPointers => SamusProjectileRomDataTrailsConstants.RightInstructionPointers;
    }
}
