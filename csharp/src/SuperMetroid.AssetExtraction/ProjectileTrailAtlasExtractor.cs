using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only trail-owned tiles, omitting the intervening missile/explosion graphics.</summary>
public static class ProjectileTrailAtlasExtractor
{
    /// <summary>Imports eight ice/wave trail characters followed by four missile-family characters, excluding the unrelated native tile gap.</summary>
    /// <param name="bus">Cartridge-import-capable address space supplying the two four-bit planar trail-character spans.</param>
    /// <returns>New 96x8 indexed PNG bytes preserving the twelve characters' pixel indices 0..15; diagnostic PNG colors do not select runtime palettes.</returns>
    /// <remarks>Recompiles the PNG and verifies both native character spans before returning; no filesystem output is written.</remarks>
    /// <exception cref="InvalidDataException">PNG validation fails or recompilation changes either native character span.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        byte[] first = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), ProjectileTrailAtlasDefinitions.IceWaveSource, ProjectileTrailAtlasDefinitions.IceWaveByteCount);
        byte[] second = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), ProjectileTrailAtlasDefinitions.MissileSource, ProjectileTrailAtlasDefinitions.MissileByteCount);
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
