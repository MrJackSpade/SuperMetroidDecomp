using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable charge-flare compositions, independent of cadence, placement and projectile physics.</summary>
public sealed class ChargeFlareSpriteCatalog
{
    private readonly ProjectileSpriteCatalog sprites;
    private ChargeFlareSpriteCatalog(ProjectileSpriteCatalog sprites) => this.sprites = sprites;

    /// <summary>Compiles version-one charge/Hyper/grapple flare and directional spark artwork for all twenty-eight distinct bank-$93 compositions, independently of the fifty-four repeated animation selectors.</summary>
    /// <param name="json">Caller-owned JSON stream, left open, with every required pointer-named composition and ordered OAM parts using explicit palette selectors.</param>
    /// <returns>Installed visual compositions; charge cadence, screen placement, and selector advancement remain owned by the projectile or grapple system.</returns>
    /// <exception cref="InvalidDataException">The JSON is malformed or ambiguous, its required identity set or version is invalid, or its OAM fields/counts are unsupported.</exception>
    public static ChargeFlareSpriteCatalog Load(Stream json)
        => new(ProjectileSpriteCatalog.LoadFrames(json, ChargeFlareSpriteDefinitions.NativePointers.ToArray()));

    /// <summary>Draws a valid native charge-flare selector without consulting ROM.</summary>
    public void Draw(ushort selector, OamBuffer oam, ushort x, ushort y)
    {
        if (selector >= ChargeFlareSpriteDefinitions.Selectors.Length)
            throw new InvalidDataException($"Unknown charge-flare sprite selector {selector:X4}.");
        sprites.Draw(ChargeFlareSpriteDefinitions.Selectors[selector], oam, x, y);
    }
}
