using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable four-bit OBJ characters for Samus's five death-explosion transfers.</summary>
public sealed class SamusDeathTileAtlas : IInstalledArtworkTransferSource
{
    private readonly byte[] planar;

    private SamusDeathTileAtlas(byte[] planar)
    {
        if (planar.Length != SamusDeathTileAtlasFormat.TotalByteCount)
            throw new InvalidDataException("Samus death tile atlas has the wrong transfer size.");
        this.planar = planar;
    }

    /// <summary>SHA-256 of all five selected death-explosion character uploads.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusDeathTileAtlas),
        content => content.Append("death characters", this.planar));

    public static SamusDeathTileAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(png,
            SamusDeathTileAtlasFormat.Width, SamusDeathTileAtlasFormat.Height);
        return new SamusDeathTileAtlas(SnesPlanarTileEncoder.Encode(image.Pixels,
            image.Width, image.Height, SamusDeathTileAtlasFormat.BitsPerPixel));
    }

    /// <summary>Finds one native $400-byte transfer without changing its queued destination.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        var segments = SamusSpecialSequenceRomData.Death.TileSegments;
        for (int index = 0; index < segments.Length; index++)
        {
            if (segments[index].SourceAddress != sourceAddress)
                continue;
            if (byteCount != SamusSpecialSequenceRomData.Death.TileSegmentByteCount)
                throw new InvalidDataException(
                    $"Samus death segment ${sourceAddress:X6} requires " +
                    $"{SamusSpecialSequenceRomData.Death.TileSegmentByteCount} bytes, not {byteCount}.");
            data = planar.AsMemory(index * byteCount, byteCount);
            return true;
        }
        data = default;
        return false;
    }
}

/// <summary>Five $400-byte 4-bpp segments, each occupying four 64-pixel-wide tile rows.</summary>
public static class SamusDeathTileAtlasFormat
{
    public const string ArtworkFileName = "samus-death-explosion.png";
    public const string ManifestFileName = "samus-death-explosion-manifest.json";
    public const int BitsPerPixel = 4;
    public const int Width = 64;
    public const int SegmentCount = 5;
    public const int TotalByteCount = SegmentCount * SamusSpecialSequenceRomData.Death.TileSegmentByteCount;
    public const int Height = TotalByteCount / 32 / (Width / 8) * 8;
}
