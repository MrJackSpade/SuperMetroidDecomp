
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values of <see cref="SamusProjectileRomData.Beams"/> that only verification reads.</summary>
internal static class SamusProjectileRomDataBeamsConstants
{
    /// <summary>Charged firing sound IDs indexed by beam combination.</summary>
    public const int ChargedSounds = 0x90c2a7;
    /// <summary>Damage word followed by one instruction pointer per direction.</summary>
    public const int DataRecordByteCount = sizeof(ushort) + SamusProjectileRomData.Origins.DirectionCount * sizeof(ushort);
    /// <summary>Duration, spritemap, radii, and animation word in one instruction record.</summary>
    public const int InstructionRecordByteCount = 8;
    /// <summary>Uncharged firing sound IDs indexed by beam combination.</summary>
    public const int UnchargedSounds = 0x90c28f;

    /// <summary>Provides beam sound tables and data and instruction widths for Samus projectile decoding.</summary>
    extension(SamusProjectileRomData.Beams)
    {
        /// <inheritdoc cref="SamusProjectileRomDataBeamsConstants.ChargedSounds"/>
        internal static int ChargedSounds => SamusProjectileRomDataBeamsConstants.ChargedSounds;
        /// <inheritdoc cref="SamusProjectileRomDataBeamsConstants.DataRecordByteCount"/>
        internal static int DataRecordByteCount => SamusProjectileRomDataBeamsConstants.DataRecordByteCount;
        /// <inheritdoc cref="SamusProjectileRomDataBeamsConstants.InstructionRecordByteCount"/>
        internal static int InstructionRecordByteCount => SamusProjectileRomDataBeamsConstants.InstructionRecordByteCount;
        /// <inheritdoc cref="SamusProjectileRomDataBeamsConstants.UnchargedSounds"/>
        internal static int UnchargedSounds => SamusProjectileRomDataBeamsConstants.UnchargedSounds;
    }
}
