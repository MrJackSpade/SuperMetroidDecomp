using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The cartridge's pose numbering: pose <c>n</c> is the <c>n</c>th record of the 253-record pose tables. Only the import
/// knows these numbers (#1595): it reads and lays out the cartridge's pose tables by them and installs every table keyed
/// by the pose's name, so gameplay never sees a number. A record no gameplay path gives Samus has no pose, so its number
/// names none, and neither does a number past the records, which reads adjacent code no path gives Samus (#2457). This
/// is the one place a number becomes its pose; <see cref="SamusPoseNumbers"/> turns a pose back into its number.
/// </summary>
[EncodingOwner("pose numbers of the cartridge's Samus pose tables")]
internal static class SamusPoseNumbering
{
    /// <summary>The number of records in the cartridge's pose tables, including the records of poses no path reaches.</summary>
    internal const int RecordCount = 253;

    /// <summary>Whether <paramref name="number"/> names a pose: a record some gameplay path gives Samus.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is outside a byte.</exception>
    internal static bool NamesPose(int number) =>
        (uint)number <= byte.MaxValue
            ? number < SamusPoseNumberDefinitions.ByNumber.Length && SamusPoseNumberDefinitions.ByNumber[number] is not null
            : throw new ArgumentOutOfRangeException(nameof(number), number, "A pose number is one byte.");

    /// <summary>The pose a number names.</summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The number is outside a byte, or is a record no gameplay path gives Samus or a number past the records, which name
    /// no pose.
    /// </exception>
    internal static SamusPoseId FromNumber(int number) =>
        (uint)number <= byte.MaxValue
            ? number < SamusPoseNumberDefinitions.ByNumber.Length && SamusPoseNumberDefinitions.ByNumber[number] is SamusPoseId pose
                ? pose
                : throw new ArgumentOutOfRangeException(nameof(number), number, "A number no gameplay path gives Samus names no pose.")
            : throw new ArgumentOutOfRangeException(nameof(number), number, "A pose number is one byte.");

    /// <summary>
    /// The animation the record numbered <paramref name="number"/> plays when no pose names the record but a read past an
    /// earlier animation reaches the record's animation (#2048); null for any other number.
    /// </summary>
    internal static SamusAnimationName? AnimationWithoutPose(int number) =>
        SamusPoseNumberDefinitions.AnimationsOfRecordsWithoutPose.TryGetValue(number, out SamusAnimationName animation) ? animation : null;
}
