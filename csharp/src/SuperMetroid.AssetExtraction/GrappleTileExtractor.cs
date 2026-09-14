using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts four endpoint tiles and twelve angle/animation segment tiles, omitting unused source-sheet padding.</summary>
public static class GrappleTileExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        using var planar = new MemoryStream();
        foreach (var transfer in GrappleTileDefinitions.Transfers)
            planar.Write(RomDataReader.ReadFixedBank(bus, transfer.SourceAddress, transfer.ByteCount));
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar.ToArray(), 4, GrappleTileDefinitions.Width / 8, out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] result = png.ToArray();
        var verified = GrappleTileAtlas.Load(new MemoryStream(result));
        foreach (var transfer in GrappleTileDefinitions.Transfers)
            if (!verified.Resolve(transfer.Asset).Span.SequenceEqual(RomDataReader.ReadFixedBank(bus, transfer.SourceAddress, transfer.ByteCount)))
                throw new InvalidDataException("Grapple PNG roundtrip changed native characters.");
        return result;
    }
}
