using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="ChargeFlareSprites"/> members production does not use.</summary>
internal static class ChargeFlareSpritesAccess
{
    /// <summary>Exposes <see cref="ChargeFlareSprites"/> state that only verification reads.</summary>
    extension(ChargeFlareSprites self)
    {
        /// <summary>Draws the composition an entry of the original selector table stands for, without consulting ROM.</summary>
        public void Draw(ushort selector, OamBuffer oam, Position16<SpritePixels> origin)
        {
            if (selector >= ChargeFlareSpriteNative.NativeSelectorCount)
                throw new InvalidDataException($"Unknown charge-flare sprite selector {selector:X4}.");
            self.sprites.Draw(ChargeFlareSpriteDefinitions.NativeSelector(selector), oam, origin);
        }
    }
}
