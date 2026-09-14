using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable charge-flare compositions, independent of cadence, placement and projectile physics.</summary>
public sealed class ChargeFlareSpriteCatalog
{
    private readonly ProjectileSpriteCatalog sprites;
    private ChargeFlareSpriteCatalog(ProjectileSpriteCatalog sprites) => this.sprites = sprites;

    public static ChargeFlareSpriteCatalog Load(Stream json)
        => new(ProjectileSpriteCatalog.LoadFrames(json, ChargeFlareSpriteDefinitions.NativePointers));

    /// <summary>Draws a valid native charge-flare selector without consulting ROM.</summary>
    public void Draw(ushort selector, OamBuffer oam, ushort x, ushort y)
    {
        if (selector >= ChargeFlareSpriteDefinitions.Selectors.Length)
            throw new InvalidDataException($"Unknown charge-flare sprite selector {selector:X4}.");
        sprites.Draw(ChargeFlareSpriteDefinitions.Selectors[selector], oam, x, y);
    }
}
