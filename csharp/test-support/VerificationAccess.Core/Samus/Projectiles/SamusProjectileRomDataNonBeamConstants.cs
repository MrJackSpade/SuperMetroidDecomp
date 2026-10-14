
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values of <see cref="SamusProjectileRomData.NonBeam"/> that only verification reads.</summary>
internal static class SamusProjectileRomDataNonBeamConstants
{
    /// <summary>Bytes in one direction's acceleration/subacceleration record.</summary>
    public const int AccelerationRecordByteCount = 4;

    /// <summary>Provides the acceleration record width for non-beam projectile data checks.</summary>
    extension(SamusProjectileRomData.NonBeam)
    {
        /// <inheritdoc cref="SamusProjectileRomDataNonBeamConstants.AccelerationRecordByteCount"/>
        internal static int AccelerationRecordByteCount => SamusProjectileRomDataNonBeamConstants.AccelerationRecordByteCount;
    }
}
