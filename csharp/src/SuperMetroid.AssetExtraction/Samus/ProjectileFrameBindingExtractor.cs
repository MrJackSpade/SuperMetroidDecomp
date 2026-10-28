using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Exports visual spritemap operands from all authored timed projectile records.</summary>
public static class ProjectileFrameBindingExtractor
{
    /// <summary>Imports the spritemap selection for every compiled timed Samus-projectile record in bank $93.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the timed records' spritemap operands.</param>
    /// <returns>New UTF-8 JSON bytes mapping each record's name to the name of the composition it draws.</returns>
    /// <remarks>Exports visual bindings only; record durations, collision radii, and instruction control flow remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A timed record selects a spritemap outside the compiled visual catalog or binding validation fails.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        ushort[] recordSprites = ProjectileSpriteLocations.RecordSprites(bus);
        Dictionary<ushort, ProjectileSpriteId> byLocation = ProjectileSpriteLocations.Projectiles()
            .ToDictionary(pair => pair.Value, pair => pair.Key);
        var frames = new Dictionary<string, string>();
        for (int record = 0; record < recordSprites.Length; record++)
            frames.Add(ProjectileFrameBindingFormat.FrameName(record), byLocation.TryGetValue(recordSprites[record], out ProjectileSpriteId sprite)
                ? sprite.Name
                : throw new InvalidDataException($"Timed projectile record {record} selects ${recordSprites[record]:X4}, which is no projectile composition."));
        return ProjectileFrameBindingCatalog.Write(new ProjectileFrameBindingDocument
        {
            Version = ProjectileFrameBindingFormat.Version,
            Frames = frames,
        });
    }
}
