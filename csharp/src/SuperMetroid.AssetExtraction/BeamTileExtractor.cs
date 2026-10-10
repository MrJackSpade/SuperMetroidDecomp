using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

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
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Beams.TilePointers + selection.TableIndex * 2);
            byte[] planar = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                SamusProjectileRomData.Banks.CharacterData | pointer, BeamTileAtlasDefinitions.ByteCount);
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
                BeamTileAtlasDefinitions.Width / 8, out int width, out int height);
            using var stream = new MemoryStream();
            IndexedPng.Write(stream, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
            byte[] png = stream.ToArray();
            var verified = BeamTileAtlas.Load(new MemoryStream(png), selection);
            if (!verified.Transfer.Span.SequenceEqual(planar))
                throw new InvalidDataException("Beam PNG roundtrip changed native tile bytes.");
            files.Add(BeamTileAtlasDefinitions.FileName(selection), png);
        }
        return files;
    }
}
