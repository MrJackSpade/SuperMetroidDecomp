using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Hardware;

/// <summary>
/// Converts raw cartridge palette words in verification fixtures into fifteen-bit colors
/// before passing them to typed presentation APIs.
/// </summary>
internal static class ColorWordsVerification
{
    /// <summary>Decodes cartridge color words.</summary>
    internal static Bgr555[] ToColors(ushort[] words) => Array.ConvertAll(words, Bgr555.FromWord);

    /// <summary>Decodes rows of cartridge color words.</summary>
    internal static Bgr555[][] ToColors(ushort[][] rows) => Array.ConvertAll(rows, ToColors);

    /// <summary>Decodes families of rows of cartridge color words.</summary>
    internal static Bgr555[][][] ToColors(ushort[][][] families) => Array.ConvertAll(families, ToColors);

    /// <summary>Decodes a pointer-keyed map of cartridge color words.</summary>
    internal static Dictionary<ushort, Bgr555> ToColors(Dictionary<ushort, ushort> words) =>
        words.ToDictionary(entry => entry.Key, entry => Bgr555.FromWord(entry.Value));
}
