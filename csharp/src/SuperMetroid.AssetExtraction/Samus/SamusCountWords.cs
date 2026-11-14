using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The codec between Samus's counts (<see cref="CountsRunBelowZero"/>) and the sixteen-bit words save slots and the
/// cartridge's work memory store them in: a count is its word read as a signed value. The legacy save-image codec and
/// the checks that compare with native state convert through it; nothing in the game knows the word form.
/// </summary>
[EncodingOwner("Samus's count words in saves and snapshots")]
public static class SamusCountWords
{
    /// <summary>A stored word, 0 through 65535, as a count, reading it as the signed value the cartridge compares.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="word"/> is not a word.</exception>
    public static int Decode(int word) => unchecked((short)Ensure.BetweenInclusive(0, ushort.MaxValue, word));

    /// <summary>A count as its stored word.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="value"/> is outside the range a count holds.</exception>
    public static ushort Encode(int value) => unchecked((ushort)CountsRunBelowZero.Require(value));
}
