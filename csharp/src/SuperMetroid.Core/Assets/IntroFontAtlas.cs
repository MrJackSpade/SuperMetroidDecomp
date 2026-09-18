namespace SuperMetroid.Core.Assets;

/// <summary>Editable 2-bpp font used by the English opening narration.</summary>
public sealed class IntroFontAtlas
{
    private readonly byte[] transfer;

    private IntroFontAtlas(byte[] transfer) => this.transfer = transfer;

    public ReadOnlyMemory<byte> Transfer => transfer;

    public static IntroFontAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png, IntroFontAtlasFormat.Width, IntroFontAtlasFormat.Height);
        byte[] planar = SnesPlanarTileEncoder.Encode(
            image.Pixels, image.Width, image.Height, IntroFontAtlasFormat.BitsPerPixel);
        if (planar.Length != IntroFontAtlasFormat.ByteCount)
        {
            throw new InvalidDataException(
                $"Opening font compiled to {planar.Length} bytes; expected " +
                $"{IntroFontAtlasFormat.ByteCount}.");
        }
        return new(planar);
    }

    /// <summary>Constructs the cartridge-backed fallback used by focused legacy tests.</summary>
    internal static IntroFontAtlas FromPlanarBytes(ReadOnlySpan<byte> planar)
    {
        if (planar.Length != IntroFontAtlasFormat.ByteCount)
        {
            throw new InvalidDataException(
                $"Opening font contains {planar.Length} bytes; expected " +
                $"{IntroFontAtlasFormat.ByteCount}.");
        }
        return new(planar.ToArray());
    }
}

/// <summary>PNG and native transfer geometry for the 144-tile opening font.</summary>
public static class IntroFontAtlasFormat
{
    public const string FileName = "intro-font.png";
    public const int BitsPerPixel = 2;
    public const int ColorCount = 4;
    public const int TileCount = 144;
    public const int TilesPerRow = 16;
    public const int TileSize = 8;
    public const int Width = TilesPerRow * TileSize;
    public const int Height = TileCount / TilesPerRow * TileSize;
    public const int ByteCount = TileCount * BitsPerPixel * TileSize;
}
