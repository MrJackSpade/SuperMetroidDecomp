using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The composition every Samus projectile animation frame draws, as installed (#1596). Durations, trail values, radii, damage and
/// animation control flow are gameplay, held apart from this picture binding.
/// </summary>
public sealed class InstalledProjectileFrameBindings
{
    /// <summary>The composition each frame draws, for every frame.</summary>
    private readonly Dictionary<SamusProjectileFrame, ProjectileSpriteId> sprites;

    /// <summary>Takes the composition of every frame, as the import pipeline validated it.</summary>
    /// <param name="sprites">The composition each frame draws; it must bind every frame and nothing else.</param>
    /// <exception cref="ArgumentException">A frame is unbound, or a binding names no frame.</exception>
    internal InstalledProjectileFrameBindings(Dictionary<SamusProjectileFrame, ProjectileSpriteId> sprites)
    {
        Ensure.NotNull(sprites);
        int frames = 0;
        foreach ((SamusProjectileSegment segment, SamusProjectileFrameGameplay[] shown) in SamusProjectileFrameDefinitions.ByAnimation)
            for (int show = 0; show < shown.Length; show++, frames++)
                if (!sprites.ContainsKey(new SamusProjectileFrame(segment, show)))
                    throw new ArgumentException($"Projectile frame {segment} {show} has no installed composition.", nameof(sprites));
        if (sprites.Count != frames)
            throw new ArgumentException($"{sprites.Count - frames} installed compositions bind no projectile frame.", nameof(sprites));
        this.sprites = sprites;
    }

    /// <summary>Returns the composition one projectile animation frame draws.</summary>
    /// <param name="frame">The frame.</param>
    internal ProjectileSpriteId Resolve(SamusProjectileFrame frame) => sprites[frame];
}
