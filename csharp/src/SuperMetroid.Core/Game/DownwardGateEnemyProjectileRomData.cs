namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$86 identities used exclusively by downward gate projectiles.</summary>
internal static class DownwardGateEnemyProjectileRomData
{
    /// <summary>Initializer-selected no-op pre-instruction at <c>$86:E604</c>.</summary>
    public const ushort InertPreInstruction = 0xe604;

    /// <summary>Signed 8.8 vertical gate movement pre-instruction at <c>$86:E605</c>.</summary>
    public const ushort MovementPreInstruction = 0xe605;

    /// <summary>Instruction <c>$86:E533</c>, which consumes one signed Y-velocity word.</summary>
    public const ushort SetYVelocityInstruction = 0xe533;
    public const ushort ClosedSleepInstruction = 0xe566;

    /// <summary>One block of accumulated 8.8 motion before advancing a sleeping list.</summary>
    public const ushort OneBlockDistance = 0x1000;
    public const int PixelsPerRoomBlock = 16;
    public const int ClosedPositionOffsetPixels = 64;
    public const int NativeBytesPerRoomBlock = 2;
}
