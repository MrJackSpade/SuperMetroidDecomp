using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Hardware;

/// <summary>
/// Converts raw cartridge palette words in verification fixtures into fifteen-bit colors
/// before passing them to typed presentation APIs.
/// </summary>
internal static class ColorWordsVerification
{
    /// <summary>Decodes cartridge color words.</summary>
    internal static RgbColor[] ToColors(ushort[] words) => Array.ConvertAll(words, RgbColor.FromWord);

    /// <summary>Decodes rows of cartridge color words.</summary>
    internal static RgbColor[][] ToColors(ushort[][] rows) => Array.ConvertAll(rows, ToColors);

    /// <summary>Decodes families of rows of cartridge color words.</summary>
    internal static RgbColor[][][] ToColors(ushort[][][] families) => Array.ConvertAll(families, ToColors);

    /// <summary>Decodes a pointer-keyed map of cartridge color words.</summary>
    internal static Dictionary<ushort, RgbColor> ToColors(Dictionary<ushort, ushort> words) =>
        words.ToDictionary(entry => entry.Key, entry => RgbColor.FromWord(entry.Value));
}
