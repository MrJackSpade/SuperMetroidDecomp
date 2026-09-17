namespace SuperMetroid.Core.Assets;

/// <summary>Editable 4-bpp font shared by the credits, result screens, and post-shot subtitle.</summary>
public sealed class EndingFontAtlas
{
    private readonly byte[] transfer;
    private EndingFontAtlas(byte[] transfer) => this.transfer = transfer;

    public ReadOnlyMemory<byte> Transfer => transfer;

    public static EndingFontAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png, EndingFontAtlasFormat.Width, EndingFontAtlasFormat.Height);
        byte[] planar = SnesPlanarTileEncoder.Encode(
            image.Pixels, image.Width, image.Height, EndingFontAtlasFormat.BitsPerPixel);
        if (planar.Length != EndingFontAtlasFormat.ByteCount)
            throw new InvalidDataException(
                $"Ending font compiled to {planar.Length} bytes; expected {EndingFontAtlasFormat.ByteCount}.");
        return new(planar);
    }

    /// <summary>Constructs the fallback cartridge-backed atlas used by focused legacy ending tests.</summary>
    internal static EndingFontAtlas FromPlanarBytes(ReadOnlySpan<byte> planar)
    {
        if (planar.Length != EndingFontAtlasFormat.ByteCount)
            throw new InvalidDataException(
                $"Ending font contains {planar.Length} planar bytes; expected {EndingFontAtlasFormat.ByteCount}.");
        return new(planar.ToArray());
    }
}

/// <summary>PNG and native transfer geometry for the 160-tile ending font resource.</summary>
public static class EndingFontAtlasFormat
{
    public const string FileName = "ending-font.png";
    public const int BitsPerPixel = 4;
    public const int ColorCount = 16;
    public const int TileCount = 160;
    public const int TilesPerRow = 16;
    public const int TileSize = 8;
    public const int Width = TilesPerRow * TileSize;
    public const int Height = TileCount / TilesPerRow * TileSize;
    public const int ByteCount = TileCount * BitsPerPixel * TileSize;
}
