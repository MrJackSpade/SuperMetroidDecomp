using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts trail OBJ appearance without exposing native timing or movement commands.</summary>
public static class ProjectileTrailExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var frames = new Dictionary<string, ProjectileTrailAppearance>();
        foreach (ushort frame in ProjectileTrailVisualDefinitions.Frames)
        {
            var a = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(bus, SamusProjectileRomData.Banks.Movement | (frame + 2)));
            frames.Add(ProjectileTrailVisualDefinitions.Name(frame), new()
            {
                TileColumn = a.TileNumber % ProjectileSpriteDefinitions.TileColumns,
                TileRow = a.TileNumber / ProjectileSpriteDefinitions.TileColumns,
                Palette = a.PaletteIndex, Priority = a.Priority, FlipX = a.FlipHorizontally, FlipY = a.FlipVertically,
            });
        }
        return ProjectileTrailCatalog.Write(new() { Version = ProjectileTrailVisualDefinitions.Version, Frames = frames });
    }
}
