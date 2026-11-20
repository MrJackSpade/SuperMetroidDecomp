using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The composition every Samus projectile animation frame draws, as installed (#1596), or that it draws none (#2558): an
/// invisible frame shows no composition. Durations, trail values, radii, damage and animation control flow are gameplay,
/// held apart from this picture binding.
/// </summary>
public sealed class InstalledProjectileFrameBindings
{
    /// <summary>The composition each pictured frame draws.</summary>
    private readonly Dictionary<SamusProjectileFrameId, ProjectileSpriteId> sprites;

    /// <summary>The frames that draw no composition.</summary>
    private readonly IReadOnlySet<SamusProjectileFrameId> unpictured;

    /// <summary>Takes the composition of every pictured frame and the frames that draw none, as the import pipeline validated them.</summary>
    /// <param name="sprites">The composition each pictured frame draws.</param>
    /// <param name="unpictured">The frames that draw no composition; with <paramref name="sprites"/> they bind every frame once.</param>
    /// <exception cref="ArgumentException">A frame is unbound or bound twice, or a binding names no frame.</exception>
    internal InstalledProjectileFrameBindings(Dictionary<SamusProjectileFrameId, ProjectileSpriteId> sprites, IReadOnlySet<SamusProjectileFrameId> unpictured)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(unpictured);
        SamusProjectileFrameId[] frames = Enum.GetValues<SamusProjectileFrameId>();
        foreach (SamusProjectileFrameId frame in frames)
            if (sprites.ContainsKey(frame) == unpictured.Contains(frame))
                throw new ArgumentException($"Projectile frame {frame} needs exactly one of an installed composition or none.", nameof(sprites));
        if (sprites.Count + unpictured.Count != frames.Length)
            throw new ArgumentException($"{sprites.Count + unpictured.Count - frames.Length} installed bindings bind no projectile frame.", nameof(sprites));
        this.sprites = sprites;
        this.unpictured = unpictured;
    }

    /// <summary>Returns the composition one projectile animation frame draws, or none for a frame that draws nothing.</summary>
    /// <param name="frame">The frame.</param>
    internal ProjectileSpriteId? Resolve(SamusProjectileFrameId frame) => unpictured.Contains(frame) ? null : sprites[frame];
}
