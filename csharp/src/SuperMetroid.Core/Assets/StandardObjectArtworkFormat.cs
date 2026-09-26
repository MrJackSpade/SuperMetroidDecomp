namespace SuperMetroid.Core.Assets;

/// <summary>Native standard OBJ upload shared by gameplay, the intro, and Ceres destruction.</summary>
public static class StandardObjectArtworkFormat
{
    public const int Version = 1;
    public const string FileName = "standard-object-characters.png";
    public const string ManifestFileName = "standard-object-artwork.json";
    /// <summary>Full $82:8318 DMA length; the intro uses only its first $2000 bytes.</summary>
    public const int TransferByteCount = 0x2e00;
    /// <summary>Encoded VRAM destination word for the standard OBJ transfer.</summary>
    public const ushort EncodedVramDestination = 0x6000;
}
