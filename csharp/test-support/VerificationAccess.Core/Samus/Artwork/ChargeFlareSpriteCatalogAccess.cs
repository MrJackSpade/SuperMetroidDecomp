using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="ChargeFlareSpriteCatalog"/> members production does not use.</summary>
internal static class ChargeFlareSpriteCatalogAccess
{
    /// <summary>Exposes <see cref="ChargeFlareSpriteCatalog"/> state that only verification reads.</summary>
    extension(ChargeFlareSpriteCatalog self)
    {
        /// <summary>Draws a valid native charge-flare selector without consulting ROM.</summary>
        public void Draw(ushort selector, OamBuffer oam, Position16<SpritePixels> origin)
        {
            if (selector >= ChargeFlareSpriteDefinitions.Selectors.Count)
                throw new InvalidDataException($"Unknown charge-flare sprite selector {selector:X4}.");
            self.sprites.Draw(ChargeFlareSpriteDefinitions.Selectors[selector], oam, origin);
        }
    }
}
