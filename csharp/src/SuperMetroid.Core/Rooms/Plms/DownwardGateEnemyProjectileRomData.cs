namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Named bank-$86 identities used exclusively by downward gate projectiles.</summary>
internal static class DownwardGateEnemyProjectileRomData
{



    /// <summary>One block of accumulated 8.8 motion before advancing a sleeping list.</summary>
    public const ushort OneBlockDistance = 0x1000;
    /// <summary>Converts the gate PLM's room block coordinates into projectile pixel coordinates.</summary>
    public const int PixelsPerRoomBlock = 16;
    /// <summary>Positions the closed gate projectile four blocks below its PLM origin.</summary>
    public const int ClosedPositionOffsetPixels = 64;
    /// <summary>Converts a PLM block index into the byte offset kept in the projectile's native association field.</summary>
    public const int NativeBytesPerRoomBlock = 2;
}
