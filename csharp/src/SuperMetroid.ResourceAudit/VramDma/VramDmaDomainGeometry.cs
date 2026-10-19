namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>Reviewed native geometry used by descriptor adapters, not gameplay configuration.</summary>
internal static class VramDmaDomainGeometry
{
    /// <summary>Enemy header tile-size bit 15 selects staging placement, not upload length.</summary>
    internal const int EnemyTileByteCountMask = 0x7fff;
    /// <summary>Highest lossless SNES CPU-bus address accepted by the queue.</summary>
    internal const int MaximumBusAddress = 0xffffff;
}
