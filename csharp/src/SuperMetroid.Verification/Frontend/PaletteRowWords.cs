using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Frontend;

/// <summary>
/// Compares a cartridge attribute word with the palette row the port's definition holds in its place
/// (#1422). The word is decoded with the import pipeline's attribute codec and must select that row and nothing else,
/// which is everything the earlier whole-word comparison established.
/// </summary>
internal static class PaletteRowWords
{
    /// <summary>
    /// Asserts that <paramref name="cartridgeWord"/> selects palette row <paramref name="palette"/> with no character,
    /// priority or mirroring bits.
    /// </summary>
    /// <param name="cartridgeWord">The attribute or palette word the cartridge stores for the actor.</param>
    /// <param name="palette">The palette row the port's definition holds.</param>
    /// <param name="context">What the word belongs to, for the failure message.</param>
    internal static void AssertSelectsOnlyPaletteRow(ushort cartridgeWord, int palette, string context)
    {
        var attributes = new SnesObjAttributeWord(cartridgeWord);
        AssertEqual(palette, attributes.PaletteIndex, context + " palette row");
        AssertEqual(SnesObjAttributeWord.Create(0, attributes.PaletteIndex, 0), attributes, context + " selects only a palette row");
    }
}
