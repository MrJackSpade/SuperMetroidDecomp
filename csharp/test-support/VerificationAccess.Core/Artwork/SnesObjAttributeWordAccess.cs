namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Verification access to <see cref="SnesObjAttributeWord"/> members production does not use.</summary>
internal static class SnesObjAttributeWordAccess
{
    /// <summary>Exposes <see cref="SnesObjAttributeWord"/> state that only verification reads.</summary>
    extension(SnesObjAttributeWord self)
    {
        /// <summary>Replaces only the OBJ palette field, preserving tile, priority, and flips.</summary>
        public SnesObjAttributeWord WithPaletteBits(ushort paletteBits)
        {
            SnesObjAttributeWord palette = SnesObjAttributeWordConstants.FromPaletteBits(paletteBits);
            return new(unchecked((ushort)((self.Raw & ~SnesObjAttributeWord.PaletteFieldMask) | palette.Raw)));
        }

        /// <summary>Replaces the three-bit palette index.</summary>
        public SnesObjAttributeWord WithPaletteIndex(int paletteIndex)
        {
            if ((uint)paletteIndex > SnesObjAttributeWord.ThreeBitMask)
                throw new ArgumentOutOfRangeException(nameof(paletteIndex));
            return self.WithPaletteBits(unchecked((ushort)(paletteIndex << SnesObjAttributeWord.PaletteShift)));
        }
    }
}
