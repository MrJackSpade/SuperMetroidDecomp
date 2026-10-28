using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts four endpoint tiles and twelve angle/animation segment tiles, omitting unused source-sheet padding.</summary>
public static class GrappleTileExtractor
{
    /// <summary>Compacts four endpoint characters and three four-character rope-orientation groups from bank $9A into one verified replacement atlas.</summary>
    /// <param name="bus">Import-capable cartridge source for the endpoint, horizontal, diagonal, and vertical transfers in <see cref="GrappleTileDefinitions.Transfers"/>.</param>
    /// <returns>A new 128-by-8 indexed PNG buffer containing sixteen 8-by-8 four-bit characters with a sixteen-color diagnostic palette.</returns>
    /// <remarks>Unused native sheet padding is omitted. The PNG is loaded back and every owned transfer is re-encoded and compared with its cartridge bytes; runtime palette colors and animation/angle selection are separate.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    /// <exception cref="InvalidDataException">The generated atlas cannot round-trip the native planar characters.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        using var planar = new MemoryStream();
        foreach (var transfer in GrappleTileDefinitions.Transfers)
            planar.Write(RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), transfer.SourceAddress, transfer.ByteCount));
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar.ToArray(), 4, GrappleTileDefinitions.Width / 8, out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] result = png.ToArray();
        var verified = GrappleTileAtlas.Load(new MemoryStream(result));
        foreach (var transfer in GrappleTileDefinitions.Transfers)
            if (!verified.Resolve(transfer.Asset).Span.SequenceEqual(RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), transfer.SourceAddress, transfer.ByteCount)))
                throw new InvalidDataException("Grapple PNG roundtrip changed native characters.");
        return result;
    }
}
