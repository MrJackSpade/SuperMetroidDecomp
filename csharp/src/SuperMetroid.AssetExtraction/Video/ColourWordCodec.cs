using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// The cartridge's colour word (#1512, #1544): the import's codec between a stored 15-bit colour and a plain colour. Red
/// takes bits 0-4, green bits 5-9 and blue bits 10-14, each one of a colour's thirty-two levels
/// (<see cref="FadeLevelColour"/>); bit 15 is not a colour bit. The cartridge's colour tables, colour memory and the
/// installed colour documents store colours in this form, so only the import and the checks that read those stores
/// decode or encode it. Core holds plain colours and never sees the word.
/// </summary>
[EncodingOwner("RGB5 colour word of the cartridge and the installed colour documents")]
internal static class ColourWordCodec
{
    /// <summary>Bytes in one stored colour word.</summary>
    internal const int ByteCount = sizeof(ushort);

    /// <summary>Mask for the fifteen colour bits, excluding the unused high bit.</summary>
    private const int ColorBits = (1 << 15) - 1;

    /// <summary>Bits each channel takes in the word.</summary>
    private const int ChannelBits = 5;

    /// <summary>Mask for one channel's bits once shifted to the bottom of the word.</summary>
    private const int ChannelMask = (1 << ChannelBits) - 1;

    /// <summary>Where the green channel starts in the word.</summary>
    private const int GreenShift = ChannelBits;

    /// <summary>Where the blue channel starts in the word.</summary>
    private const int BlueShift = 2 * ChannelBits;

    /// <summary>Decoding and recognizing a stored colour word.</summary>
    extension(RgbColor)
    {
        /// <summary>
        /// Decodes a stored 15-bit colour word. A set bit 15 is rejected: it is not part of any colour, and only
        /// <c>FromWordIgnoringTopBit</c> defines a result for it.
        /// </summary>
        internal static RgbColor FromWord(ushort word)
        {
            if ((word & ~ColorBits) != 0)
                throw new ArgumentOutOfRangeException(nameof(word), word, $"Colour word ${word:X4} sets bit 15, which is not a colour bit.");
            return Decode(word);
        }

        /// <summary>
        /// The colour a packed word shows when its top bit, which is not a colour bit, may be set: the top bit is
        /// dropped. A colour step that carries out of the blue channel, and a stored word loaded as a palette, show this
        /// colour, as the original palette hardware kept only fifteen bits.
        /// </summary>
        internal static RgbColor FromWordIgnoringTopBit(int word) => Decode(word & ColorBits);

        /// <summary>
        /// Whether <paramref name="word"/> is a stored colour: the word is unchanged when the bit outside the colour,
        /// which <c>FromWordIgnoringTopBit</c> drops, is dropped.
        /// </summary>
        internal static bool IsColorWord(int word) => RgbColor.FromWordIgnoringTopBit(checked((ushort)word)).ToWord() == word;
    }

    /// <summary>Encoding a colour as its stored word.</summary>
    extension(RgbColor colour)
    {
        /// <summary>
        /// The packed 15-bit word, for the cartridge's, colour memory's and installed documents' stores: the colour's own
        /// levels, as every colour the game shows, its fades' steps among them, lies on the fade scale.
        /// </summary>
        /// <exception cref="ArgumentException">The colour is off the fade scale (<see cref="FadeLevelColour.Of"/>).</exception>
        internal ushort ToWord()
        {
            FadeLevelColour levels = colour.FadeLevels;
            return (ushort)(levels.Red | (levels.Green << GreenShift) | (levels.Blue << BlueShift));
        }
    }

    /// <summary>Splits a fifteen-bit word into its three channels' levels and shows them.</summary>
    private static RgbColor Decode(int word) =>
        new FadeLevelColour(word & ChannelMask, (word >> GreenShift) & ChannelMask, (word >> BlueShift) & ChannelMask).ToColour();
}
