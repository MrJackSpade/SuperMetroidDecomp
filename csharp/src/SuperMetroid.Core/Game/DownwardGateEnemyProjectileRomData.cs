namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$86 identities used exclusively by downward gate projectiles.</summary>
internal static class DownwardGateEnemyProjectileRomData
{



    /// <summary>One block of accumulated 8.8 motion before advancing a sleeping list.</summary>
    public const ushort OneBlockDistance = 0x1000;
    public const int PixelsPerRoomBlock = 16;
    public const int ClosedPositionOffsetPixels = 64;
    public const int NativeBytesPerRoomBlock = 2;
}
