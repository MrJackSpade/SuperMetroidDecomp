using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable charge-flare compositions, independent of cadence, placement and projectile physics.</summary>
public sealed class ChargeFlareSprites
{
    /// <summary>Installed OAM parts keyed by the distinct flare compositions.</summary>
    internal readonly InstalledProjectileSprites sprites;
    /// <summary>Keeps the compiled compositions separate from native selector cadence and placement.</summary>
    private ChargeFlareSprites(InstalledProjectileSprites sprites) => this.sprites = sprites;

    /// <summary>Compiles charge/Hyper/grapple flare and directional spark artwork for all twenty-eight distinct compositions, independently of the fifty-four repeated animation selectors.</summary>
    /// <param name="json">Caller-owned JSON stream, left open, with every required named composition and ordered OAM parts using explicit palette selectors.</param>
    /// <returns>Installed visual compositions; charge cadence, screen placement, and selector advancement remain owned by the projectile or grapple system.</returns>
    /// <exception cref="InvalidDataException">The JSON is malformed or ambiguous, its required identity set or version is invalid, or its OAM fields/counts are unsupported.</exception>
    public static ChargeFlareSprites Load(Stream json)
        => new(InstalledProjectileSprites.LoadFrames(json, ChargeFlareSpriteDefinitions.All));

    /// <summary>True when <paramref name="selector"/> indexes the native $93:A1A1 flare table.</summary>
    public static bool Contains(int selector) => selector < ChargeFlareSpriteSelection.Selectors.Count;

    /// <summary>The flare composition a valid native selector names, as a whole frame over the standard OBJ page.</summary>
    public SpriteFrame Frame(int selector)
    {
        if (selector >= ChargeFlareSpriteSelection.Selectors.Count)
            throw new InvalidDataException($"Unknown charge-flare sprite selector {selector:X4}.");
        return sprites.Frame(ChargeFlareSpriteSelection.Selectors[selector]);
    }
}
