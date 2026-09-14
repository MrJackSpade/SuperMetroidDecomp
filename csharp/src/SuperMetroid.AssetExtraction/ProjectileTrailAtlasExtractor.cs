using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only trail-owned tiles, omitting the intervening missile/explosion graphics.</summary>
public static class ProjectileTrailAtlasExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        byte[] first = RomDataReader.ReadFixedBank(bus, ProjectileTrailAtlasDefinitions.IceWaveSource, ProjectileTrailAtlasDefinitions.IceWaveByteCount);
        byte[] second = RomDataReader.ReadFixedBank(bus, ProjectileTrailAtlasDefinitions.MissileSource, ProjectileTrailAtlasDefinitions.MissileByteCount);
        byte[] planar = first.Concat(second).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, ProjectileTrailAtlasDefinitions.Width / 8, out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] bytes = png.ToArray();
        var verified = ProjectileTrailAtlas.Load(new MemoryStream(bytes));
        if (!verified.IceAndWave.Span.SequenceEqual(first) || !verified.Missile.Span.SequenceEqual(second))
            throw new InvalidDataException("Trail PNG roundtrip changed native characters.");
        return bytes;
    }
}
