using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts native beam sheets per legal selection; shared stock sheets may be edited independently.</summary>
public static class BeamTileExtractor
{
    public static Dictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        var files = new Dictionary<string, byte[]>();
        for (int selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
        {
            ushort pointer = RomDataReader.ReadWordFixedBank(bus, SamusProjectileRomData.Beams.TilePointers + selection * 2);
            byte[] planar = RomDataReader.ReadFixedBank(bus,
                SamusProjectileRomData.Banks.CharacterData | pointer, BeamTileAtlasDefinitions.ByteCount);
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4,
                BeamTileAtlasDefinitions.Width / 8, out int width, out int height);
            using var stream = new MemoryStream();
            IndexedPng.Write(stream, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
            byte[] png = stream.ToArray();
            var verified = BeamTileAtlas.Load(new MemoryStream(png));
            if (!verified.Transfer.Span.SequenceEqual(planar))
                throw new InvalidDataException("Beam PNG roundtrip changed native tile bytes.");
            files.Add(BeamTileAtlasDefinitions.FileName(selection), png);
        }
        return files;
    }
}
