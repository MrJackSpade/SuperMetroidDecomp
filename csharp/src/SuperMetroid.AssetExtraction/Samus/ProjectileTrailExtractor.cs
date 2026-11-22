using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts trail OBJ appearance without exposing native timing or movement commands.</summary>
public static partial class ProjectileTrailExtractor
{
    /// <summary>Imports OBJ appearance words from the catalogued bank-$90 projectile-trail frame records.</summary>
    /// <param name="bus">Cartridge-import-capable address space supplying each native frame's visual operand.</param>
    /// <returns>New UTF-8 JSON bytes containing named tile-column and tile-row references, palette indices, priorities, and flips.</returns>
    /// <remarks>Frame duration, position, movement, and trail-spawning mechanics remain compiled rather than becoming editable visual fields.</remarks>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var frames = new Dictionary<string, ProjectileTrailAppearance>();
        for (int index = 0; index < ProjectileTrailVisualDefinitions.RecordCount; index++)
        {
            var a = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                SamusSourceDefinitions.MovementBank.Value | (ProjectileTrailSources.Records[index].Key + 2)));
            var appearance = new ProjectileTrailAppearance
            {
                TileColumn = a.TileNumber % ProjectileSpriteDefinitions.TileColumns,
                TileRow = a.TileNumber / ProjectileSpriteDefinitions.TileColumns,
                Palette = a.PaletteIndex, Depth = a.Depth, FlipX = a.FlipHorizontally, FlipY = a.FlipVertically,
            };
            // Every record of one frame shows that frame's one appearance (#2348), which the document holds once.
            string name = SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFrameKeys.Installed.Of(ProjectileTrailSources.Records[index].Value);
            if (!frames.TryAdd(name, appearance) && frames[name] != appearance)
                throw new InvalidDataException($"The trail records of frame {name} show different appearances.");
        }
        return InstalledProjectileTrails.Write(new() { Version = ProjectileTrailVisualDefinitions.Version, Frames = frames });
    }
}
