namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Retail addresses and reveal counts used to check the seven extracted area maps.</summary>
internal static class AreaMapAssetVerificationDefinitions
{
    /// <summary>Expected tilemap addresses in cartridge area-index order.</summary>
    public static ReadOnlySpan<int> TilemapAddresses =>
    [
        0xb59000,
        0xb58000,
        0xb5a000,
        0xb5b000,
        0xb5c000,
        0xb5d000,
        0xb5e000,
    ];

    /// <summary>Expected station-visible cell counts in cartridge area-index order.</summary>
    public static ReadOnlySpan<int> StationVisibleCellCounts => [183, 168, 230, 45, 225, 45, 15];

    /// <summary>Expected secret-only cell counts in cartridge area-index order.</summary>
    public static ReadOnlySpan<int> SecretOnlyCellCounts => [50, 86, 131, 24, 53, 34, 0];
}
