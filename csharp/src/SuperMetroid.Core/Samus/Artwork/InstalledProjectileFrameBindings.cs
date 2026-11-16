using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The composition every Samus projectile animation frame draws, as installed (#1596). Durations, trail values, radii, damage and
/// animation control flow are gameplay, held apart from this picture binding.
/// </summary>
public sealed class InstalledProjectileFrameBindings
{
    /// <summary>The composition each frame draws, for every frame.</summary>
    private readonly Dictionary<SamusProjectileFrameId, ProjectileSpriteId> sprites;

    /// <summary>Takes the composition of every frame, as the import pipeline validated it.</summary>
    /// <param name="sprites">The composition each frame draws; it must bind every frame and nothing else.</param>
    /// <exception cref="ArgumentException">A frame is unbound, or a binding names no frame.</exception>
    internal InstalledProjectileFrameBindings(Dictionary<SamusProjectileFrameId, ProjectileSpriteId> sprites)
    {
        Ensure.NotNull(sprites);
        SamusProjectileFrameId[] frames = Enum.GetValues<SamusProjectileFrameId>();
        foreach (SamusProjectileFrameId frame in frames)
            if (!sprites.ContainsKey(frame))
                throw new ArgumentException($"Projectile frame {frame} has no installed composition.", nameof(sprites));
        if (sprites.Count != frames.Length)
            throw new ArgumentException($"{sprites.Count - frames.Length} installed compositions bind no projectile frame.", nameof(sprites));
        this.sprites = sprites;
    }

    /// <summary>Returns the composition one projectile animation frame draws.</summary>
    /// <param name="frame">The frame.</param>
    internal ProjectileSpriteId Resolve(SamusProjectileFrameId frame) => sprites[frame];
}
