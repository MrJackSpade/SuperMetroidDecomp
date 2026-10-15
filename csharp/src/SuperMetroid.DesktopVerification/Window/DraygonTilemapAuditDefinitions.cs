namespace SuperMetroid.DesktopVerification.Window;

/// <summary>Native sentinels and WRAM destination used by Draygon's extended BG2 tilemap commands.</summary>
internal static class DraygonTilemapAuditDefinitions
{
    /// <summary>$A0:96CA ProcessExtendedTilemap writes into tilemap_stuff at $7E:2000.</summary>
    internal const int WorkingRamBase = 0x2000;
    /// <summary>Native extended component header selecting a BG tilemap command stream.</summary>
    internal const int ExtendedTilemapMarker = 0xfffe;
    /// <summary>Native destination sentinel terminating ProcessExtendedTilemap.</summary>
    internal const int StreamEnd = 0xffff;
}
