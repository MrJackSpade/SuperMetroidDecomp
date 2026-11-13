using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable charge-flare compositions, independent of cadence, placement and projectile physics.</summary>
public sealed class ChargeFlareSprites
{
    /// <summary>Installed OAM parts keyed by the distinct flare compositions.</summary>
    internal readonly InstalledProjectileSprites sprites;
    /// <summary>Keeps the compiled compositions separate from the flare's cadence and placement.</summary>
    private ChargeFlareSprites(InstalledProjectileSprites sprites) => this.sprites = sprites;

    /// <summary>Compiles charge/Hyper/grapple flare and directional spark artwork for all twenty-eight distinct compositions.</summary>
    /// <param name="json">Caller-owned JSON stream, left open, with every required named composition and ordered OAM parts using explicit palette selectors.</param>
    /// <returns>Installed visual compositions; charge cadence, screen placement and picture choice remain owned by the projectile or grapple system.</returns>
    /// <exception cref="InvalidDataException">The JSON is malformed or ambiguous, its required identity set or version is invalid, or its OAM fields/counts are unsupported.</exception>
    public static ChargeFlareSprites Load(Stream json)
        => new(InstalledProjectileSprites.LoadFrames(json, ChargeFlareSpriteDefinitions.All));

    /// <summary>Flare composition <paramref name="composition"/> as a whole frame over the standard OBJ page.</summary>
    public SpriteFrame Frame(ProjectileSpriteId composition) => sprites.Frame(composition);
}
