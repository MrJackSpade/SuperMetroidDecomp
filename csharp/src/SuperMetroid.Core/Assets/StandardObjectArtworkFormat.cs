namespace SuperMetroid.Core.Assets;

/// <summary>Native standard OBJ upload shared by gameplay, the intro, and Ceres destruction.</summary>
public static class StandardObjectArtworkFormat
{
    /// <summary>Supported stock manifest schema version; loading also verifies the pinned cartridge identity and stock PNG hash.</summary>
    public const int Version = 1;
    /// <summary>Stock and override indexed PNG filename: 256x96 pixels containing 368 row-major 8x8 4-bpp characters, with the final 16 cells zero-filled.</summary>
    public const string FileName = "standard-object-characters.png";
    /// <summary>Stock JSON manifest filename recording the schema version, source-cartridge SHA-256, and stock PNG SHA-256; overrides replace only the PNG.</summary>
    public const string ManifestFileName = "standard-object-artwork.json";
    /// <summary>Selected $2E00-byte standard OBJ artwork span from $9A:D200; the native $82:8318 transfer spans $4000 bytes, while the intro uses the first $2000.</summary>
    public const int TransferByteCount = 0x2e00;
    /// <summary>PPU word-address destination $6000 (VRAM byte offset $C000) for the standard OBJ transfer, with normal word increment.</summary>
    public const ushort EncodedVramDestination = 0x6000;
}
