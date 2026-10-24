
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where the cartridge keeps Samus's non-beam projectile tables, and their record widths, for verification.</summary>
internal static class SamusProjectileNonBeamTables
{
    /// <summary>Bytes in one direction's acceleration/subacceleration record.</summary>
    public const int AccelerationRecordByteCount = 4;

    /// <summary>$93:83F1: bank-$93 data pointers for missiles, bombs, and special projectiles.</summary>
    public const int DataPointers = 0x9383f1;
    /// <summary>$93:842B: bank-$93 definitions for Super Missile link projectiles.</summary>
    public const int SuperMissileLinkDataPointers = 0x93842b;
}
