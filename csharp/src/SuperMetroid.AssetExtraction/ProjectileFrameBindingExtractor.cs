using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports visual spritemap operands from all authored timed projectile records.</summary>
public static class ProjectileFrameBindingExtractor
{
    /// <summary>Imports the spritemap selection for every compiled timed Samus-projectile record in bank $93.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the timed records' spritemap operands.</param>
    /// <returns>New UTF-8 JSON bytes mapping each stable record name to a catalogued projectile-sprite name.</returns>
    /// <remarks>Exports visual bindings only; record durations, collision radii, and instruction control flow remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A timed record selects a spritemap outside the compiled visual catalog or binding validation fails.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new Dictionary<string, string>();
        var legalSprites = ProjectileSpriteDefinitions.NativePointers.ToArray().ToHashSet();
        foreach (ushort pointer in SamusProjectileRadiusDefinitions.TimedRecordPointers)
        {
            ushort sprite = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
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
