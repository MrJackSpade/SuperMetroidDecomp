namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Cartridge values and helpers of <see cref="PauseTileAtlasFormat"/> that only verification reads.</summary>
internal static class PauseTileAtlasFormatConstants
{
    /// <summary>Byte $2000 immediately follows the shared map characters in pause VRAM.</summary>
    public const int DestinationByte = MapTileAtlasFormat.ByteCount;
}
