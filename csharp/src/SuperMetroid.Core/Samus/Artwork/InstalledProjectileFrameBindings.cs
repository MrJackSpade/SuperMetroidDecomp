using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The composition every timed projectile record draws, as installed (#1596). Durations, trail values, radii, damage and
/// program control flow are gameplay, held apart from this picture binding.
/// </summary>
public sealed class InstalledProjectileFrameBindings
{
    /// <summary>The composition each timed record draws, for every record.</summary>
    private readonly Dictionary<SamusProjectileFrame, ProjectileSpriteId> sprites;

    /// <summary>Takes the composition of every timed record, as the import pipeline validated it.</summary>
    /// <param name="sprites">The composition each record draws; it must bind every record and nothing else.</param>
    /// <exception cref="ArgumentException">A record is unbound, or a binding names no record.</exception>
    internal InstalledProjectileFrameBindings(Dictionary<SamusProjectileFrame, ProjectileSpriteId> sprites)
    {
        Ensure.NotNull(sprites);
        int records = 0;
        foreach ((SamusProjectileSegment segment, SamusProjectileRecord[] shown) in SamusProjectileRecordDefinitions.ByAnimation)
            for (int show = 0; show < shown.Length; show++, records++)
                if (!sprites.ContainsKey(new SamusProjectileFrame(segment, show)))
                    throw new ArgumentException($"Timed projectile record {segment} {show} has no installed composition.", nameof(sprites));
        if (sprites.Count != records)
            throw new ArgumentException($"{sprites.Count - records} installed compositions bind no timed projectile record.", nameof(sprites));
        this.sprites = sprites;
    }

    /// <summary>Returns the composition one timed projectile record draws.</summary>
    /// <param name="frame">The timed record.</param>
    internal ProjectileSpriteId Resolve(SamusProjectileFrame frame) => sprites[frame];
}
