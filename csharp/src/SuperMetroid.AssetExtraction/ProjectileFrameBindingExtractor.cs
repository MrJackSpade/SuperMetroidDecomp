using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports visual spritemap operands from all authored timed projectile records.</summary>
public static class ProjectileFrameBindingExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new Dictionary<string, string>();
        var legalSprites = ProjectileSpriteDefinitions.NativePointers.ToArray().ToHashSet();
        foreach (ushort pointer in SamusProjectileRadiusDefinitions.TimedRecordPointers)
        {
            ushort sprite = RomDataReader.ReadWordFixedBank(bus,
                SamusProjectileRomData.Banks.Projectile | unchecked((ushort)(pointer + 2)));
            if (!legalSprites.Contains(sprite))
                throw new InvalidDataException(
                    $"Timed projectile $93:{pointer:X4} selects uncatalogued sprite ${sprite:X4}.");
            frames.Add(ProjectileFrameBindingFormat.FrameName(pointer),
                ProjectileSpriteDefinitions.Name(sprite));
        }
        return ProjectileFrameBindingCatalog.Write(new ProjectileFrameBindingDocument
        {
            Version = ProjectileFrameBindingFormat.Version,
            Frames = frames,
        });
    }
}
