namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed native geometry used by descriptor adapters, not gameplay configuration.</summary>
internal static class VramDmaDomainGeometry
{
    /// <summary>Enemy header tile-size bit 15 selects staging placement, not upload length.</summary>
    internal const int EnemyTileByteCountMask = 0x7fff;
    /// <summary>Native room-scroll cells represent 256-pixel screens.</summary>
    internal const int ScreenPixelShift = 8;
    /// <summary>One aligned eight-pixel tile row selected by the sky camera mask.</summary>
    internal const int TileRowPixels = 8;
    /// <summary>Low byte of a native world coordinate selects its position within a screen.</summary>
    internal const int WithinScreenMask = 0xff;
    /// <summary>Nonblue scroll cells permit the additional $1F-pixel lower camera alignment.</summary>
    internal const int NonBlueLowerAlignment = 0x1f;
    /// <summary>Highest lossless SNES CPU-bus address accepted by the queue.</summary>
    internal const int MaximumBusAddress = 0xffffff;
}
