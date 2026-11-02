using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The import's check of the colour words it reads from the cartridge and from installed colour documents.</summary>
internal static class RgbColorWords
{
    /// <summary>Recognizing a stored colour word.</summary>
    extension(RgbColor)
    {
        /// <summary>
        /// Whether <paramref name="word"/> is a stored colour: the word is unchanged when the bit outside the colour, which
        /// <see cref="RgbColor.FromWordIgnoringTopBit"/> drops, is dropped.
        /// </summary>
        internal static bool IsColorWord(ushort word) => RgbColor.FromWordIgnoringTopBit(word).ToWord() == word;
    }
}
