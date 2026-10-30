using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts native beam sheets per legal selection; shared stock sheets may be edited independently.</summary>
public static class BeamTileExtractor
{
    /// <summary>Decodes each legal native beam tile sheet to indexed PNG and verifies that loading the PNG reproduces the original planar transfer bytes.</summary>
    /// <param name="bus">Supported-cartridge address space containing the beam tile pointer table and four-bit planar graphics.</param>
    /// <returns>PNG bytes keyed by the editable filename for each compiled beam artwork selection.</returns>
    public static Dictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        var files = new Dictionary<string, byte[]>();
        for (int index = 0; index < BeamTileAtlasDefinitions.ArtworkCount; index++)
        {
            SamusBeamCombination selection = BeamTileAtlasDefinitions.SelectionAt(index);
            int pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusSourceDefinitions.TilePointers.Value + selection.TableIndex * 2);
            byte[] planar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                SamusSourceDefinitions.CharacterDataBank.Value | pointer, BeamTileAtlasDefinitions.ByteCount);
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
                BeamTileAtlasDefinitions.Width / 8, out Extent16<ImagePixels> size);
            using var stream = new MemoryStream();
            IndexedPng.Write(stream, size, pixels, SnesGraphics.DiagnosticPalette(16));
            byte[] png = stream.ToArray();
            var verified = BeamTileAtlas.Load(new MemoryStream(png), selection);
            if (!verified.Characters.Span.SequenceEqual(planar))
                throw new InvalidDataException("Beam PNG roundtrip changed native tile bytes.");
            files.Add(BeamTileAtlasDefinitions.FileName(selection), png);
        }
        return files;
    }
}
