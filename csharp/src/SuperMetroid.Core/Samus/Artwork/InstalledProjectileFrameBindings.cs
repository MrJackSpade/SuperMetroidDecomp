using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Visual composition selection for every timed projectile record. Durations,
/// trail values, radii, damage, and instruction control flow are compiled separately.
/// </summary>
public sealed class InstalledProjectileFrameBindings
{
    /// <summary>Only frame bindings that differ from the records' stock compositions, by record.</summary>
    internal readonly Dictionary<SamusProjectileFrame, ProjectileSpriteId> sprites;

    /// <summary>Retains the override lookup the import pipeline validated, detached from editable JSON.</summary>
    internal InstalledProjectileFrameBindings(Dictionary<SamusProjectileFrame, ProjectileSpriteId> sprites) => this.sprites = sprites;

    /// <summary>Returns the composition one timed projectile record draws: an installed override or the record's stock composition.</summary>
    /// <param name="frame">The timed record.</param>
    internal ProjectileSpriteId Resolve(SamusProjectileFrame frame) =>
        sprites.TryGetValue(frame, out ProjectileSpriteId sprite) ? sprite : SamusProjectileRecordDefinitions.Get(frame).Sprite;
}
