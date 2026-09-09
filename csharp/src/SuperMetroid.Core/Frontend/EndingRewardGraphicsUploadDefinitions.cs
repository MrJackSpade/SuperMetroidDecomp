namespace SuperMetroid.Core.Frontend;

internal static class EndingRewardGraphicsUploadDefinitions
{
    /// <summary>$8B:F6B8: Func216 source-address table, relative to WRAM bank $7F.</summary>
    public const int SourceTable = 0x8bf6b8;
    /// <summary>$8B:F6D8: Func216 destination VRAM word-address table.</summary>
    public const int DestinationTable = 0x8bf6d8;
    /// <summary>$7F:4000: decompressed post-credits icon staging buffer.</summary>
    public const int SourceBase = 0x4000;
    /// <summary>Func216 transfers $800 bytes on each of sixteen calls.</summary>
    public const int ChunkBytes = 0x800;
    /// <summary>Full $7F:4000–BFFF staging range consumed by Func216.</summary>
    public const int SourceBytes = 0x8000;
}
