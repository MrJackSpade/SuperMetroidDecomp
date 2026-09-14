using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Twelve trail tiles in one indexed sheet, compiled into two noncontiguous VRAM transfers.</summary>
public sealed class ProjectileTrailAtlas : IVramAssetProvider
{
    private readonly byte[] tiles;
    private ProjectileTrailAtlas(byte[] tiles) => this.tiles = tiles;
    public ReadOnlyMemory<byte> IceAndWave => tiles.AsMemory(0, ProjectileTrailAtlasDefinitions.IceWaveByteCount);
    public ReadOnlyMemory<byte> Missile => tiles.AsMemory(ProjectileTrailAtlasDefinitions.IceWaveByteCount);
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => asset switch
    {
        VramAssetId.ProjectileIceWaveTrailTiles => IceAndWave,
        VramAssetId.ProjectileMissileTrailTiles => Missile,
        _ => throw new InvalidDataException($"Trail atlas cannot resolve {asset}."),
    };
    public void QueueTo(VramWriteQueue queue)
    {
        queue.EnqueueAsset(VramAssetId.ProjectileIceWaveTrailTiles, checked((ushort)IceAndWave.Length),
            ProjectileTrailAtlasDefinitions.IceWaveDestinationWord);
        queue.EnqueueAsset(VramAssetId.ProjectileMissileTrailTiles, checked((ushort)Missile.Length),
            ProjectileTrailAtlasDefinitions.MissileDestinationWord);
    }
    public static ProjectileTrailAtlas Load(Stream png)
    {
        var image = IndexedPng.Read(png, ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height);
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4));
    }
    public void LoadTo(SnesVram vram)
    {
        vram.LoadBytes(ProjectileTrailAtlasDefinitions.IceWaveDestinationWord * 2, IceAndWave.Span);
        vram.LoadBytes(ProjectileTrailAtlasDefinitions.MissileDestinationWord * 2, Missile.Span);
    }
}
