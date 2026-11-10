using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The cartridge's pose numbering: pose <c>n</c> is the <c>n</c>th record of the 253-record pose tables, and the
/// three numbers past them read adjacent code. Only the import knows these numbers (#1595): it reads and lays out the
/// cartridge's pose tables by them and installs every table keyed by the pose's name, so gameplay never sees a number.
/// A record no gameplay path gives Samus has no pose, so its number names none. This is the one place a number becomes
/// its pose; <see cref="SamusPoseNumbers"/> turns a pose back into its number.
/// </summary>
[EncodingOwner("pose numbers of the cartridge's Samus pose tables")]
internal static class SamusPoseNumbering
{
    /// <summary>The number of records in the cartridge's pose tables, including the records of poses no path reaches.</summary>
    internal const int RecordCount = 253;

    /// <summary>Whether <paramref name="number"/> names a pose: a record some gameplay path gives Samus, or a number past the records.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is outside a byte.</exception>
    internal static bool NamesPose(int number) =>
        (uint)number < (uint)SamusPoseNumberDefinitions.ByNumber.Length
            ? SamusPoseNumberDefinitions.ByNumber[number] is not null
            : throw new ArgumentOutOfRangeException(nameof(number), number, "A pose number is one byte.");

    /// <summary>The pose a number names.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The number is outside a byte, or is a record no gameplay path gives Samus, which names no pose.
    /// </exception>
    internal static SamusPoseId FromNumber(int number) =>
        (uint)number < (uint)SamusPoseNumberDefinitions.ByNumber.Length
            ? SamusPoseNumberDefinitions.ByNumber[number] ?? throw new ArgumentOutOfRangeException(
                nameof(number), number, "The record of a pose no gameplay path reaches names no pose.")
            : throw new ArgumentOutOfRangeException(nameof(number), number, "A pose number is one byte.");
}
