using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="ChargeFlareSprites"/> members production does not use.</summary>
internal static class ChargeFlareSpritesAccess
{
    /// <summary>Exposes <see cref="ChargeFlareSprites"/> state that only verification reads.</summary>
    extension(ChargeFlareSprites self)
    {
        /// <summary>Draws a valid native charge-flare selector without consulting ROM.</summary>
        public void Draw(ushort selector, OamBuffer oam, Position16<SpritePixels> origin)
        {
            if (selector >= ChargeFlareSpriteSelection.Selectors.Count)
                throw new InvalidDataException($"Unknown charge-flare sprite selector {selector:X4}.");
            self.sprites.Draw(ChargeFlareSpriteSelection.Selectors[selector], oam, origin);
        }
    }
}
