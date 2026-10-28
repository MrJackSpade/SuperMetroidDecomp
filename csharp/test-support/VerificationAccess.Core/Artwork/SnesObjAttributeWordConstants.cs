namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Cartridge values and helpers of <see cref="SnesObjAttributeWord"/> that only verification reads.</summary>
internal static class SnesObjAttributeWordConstants
{
    /// <summary>Creates a word containing only a validated OBJ palette field.</summary>
    public static SnesObjAttributeWord FromPaletteBits(int paletteBits)
    {
        if ((paletteBits & ~SnesObjAttributeWord.PaletteFieldMask) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(paletteBits), paletteBits, "OBJ palette bits must fit the three-bit field.");
        }

        return new((ushort)paletteBits);
    }
}
