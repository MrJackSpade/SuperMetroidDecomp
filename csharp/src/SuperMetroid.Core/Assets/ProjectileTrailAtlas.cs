using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Twelve trail tiles in one indexed sheet, compiled into two noncontiguous VRAM transfers.</summary>
public sealed class ProjectileTrailAtlas : IVramAssetProvider
{
    /// <summary>Compiled planar bytes for the twelve authored trail tiles, stored without the unrelated VRAM gap between their destinations.</summary>
    private readonly byte[] tiles;

    /// <summary>Wraps the twelve encoded trail tiles used by the two noncontiguous VRAM ranges.</summary>
    /// <param name="tiles">Planar bytes ordered as eight ice/wave tiles followed by four missile tiles.</param>
    private ProjectileTrailAtlas(byte[] tiles) => this.tiles = tiles;
    /// <summary>First eight 4-bpp characters, $0100 bytes corresponding to $9A:D900 and standard OBJ tiles $38..$3F, for ice/wave trail frames.</summary>
    public ReadOnlyMemory<byte> IceAndWave => tiles.AsMemory(0, ProjectileTrailAtlasDefinitions.IceWaveByteCount);
    /// <summary>Last four 4-bpp characters, $0080 bytes corresponding to $9A:DB00 and standard OBJ tiles $48..$4B, for Missile and Super Missile trails.</summary>
    public ReadOnlyMemory<byte> Missile => tiles.AsMemory(ProjectileTrailAtlasDefinitions.IceWaveByteCount);
    /// <summary>Resolves one of the two trail-owned VRAM assets without uploading it or changing projectile/trail timing.</summary>
    /// <param name="asset">Ice/wave or missile trail tile asset identity.</param>
    /// <returns>The selected native planar character bytes.</returns>
    /// <exception cref="InvalidDataException">The requested identity is not owned by this trail atlas.</exception>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => asset switch
    {
        VramAssetId.ProjectileIceWaveTrailTiles => IceAndWave,
        VramAssetId.ProjectileMissileTrailTiles => Missile,
        _ => throw new InvalidDataException($"Trail atlas cannot resolve {asset}."),
    };
    /// <summary>Queues ice/wave then missile artwork at VRAM words $6380 and $6480, preserving the unrelated standard OBJ tiles $40..$47 between their ranges.</summary>
    /// <param name="queue">Destination asset-write queue; queued identities must resolve through the installed trail provider when executed.</param>
    public void QueueTo(VramWriteQueue queue)
    {
        queue.EnqueueAsset(VramAssetId.ProjectileIceWaveTrailTiles, checked((ushort)IceAndWave.Length),
            ProjectileTrailAtlasDefinitions.IceWaveDestinationWord);
        queue.EnqueueAsset(VramAssetId.ProjectileMissileTrailTiles, checked((ushort)Missile.Length),
            ProjectileTrailAtlasDefinitions.MissileDestinationWord);
    }
    /// <summary>Loads the 96-by-8 indexed PNG as twelve left-to-right 8-pixel tiles and encodes their palette indices into four-bit SNES planar character bytes.</summary>
    /// <param name="png">Caller-owned image stream; indices 0..15 are pens rather than final CGRAM colors.</param>
    /// <returns>Installed character art split into eight ice/wave tiles followed by four missile tiles, excluding the native inter-range gap.</returns>
    public static ProjectileTrailAtlas Load(Stream png)
    {
        var image = IndexedPng.Read(png, ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height);
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4));
    }
    /// <summary>Immediately uploads the two trail ranges in ice/wave-then-missile order at VRAM bytes $C700 and $C900, leaving the intervening $0100-byte region untouched.</summary>
    /// <param name="vram">Destination VRAM; this changes character pixels only, not sprite attributes, palettes, or trail lifetimes.</param>
    public void LoadTo(SnesVram vram)
    {
        vram.LoadBytes(ProjectileTrailAtlasDefinitions.IceWaveDestinationWord * 2, IceAndWave.Span);
        vram.LoadBytes(ProjectileTrailAtlasDefinitions.MissileDestinationWord * 2, Missile.Span);
    }
}
