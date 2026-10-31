using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Yard;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The codec between a Yard's <see cref="YardDirection"/> and the direction number the cartridge stores and indexes its
/// eight direction records by (#1472), for the checks that compare with those records.
/// </summary>
[EncodingOwner("Yard direction number")]
internal static class YardDirectionCodec
{
    /// <summary>The directions in the cartridge's numbering.</summary>
    private static readonly YardDirection[] Numbered =
    [
        YardDirection.UpsideRightMovingUp, YardDirection.UpsideRightMovingDown,
        YardDirection.UpsideLeftMovingUp, YardDirection.UpsideLeftMovingDown,
        YardDirection.UpsideDownMovingLeft, YardDirection.UpsideDownMovingRight,
        YardDirection.UpsideUpMovingLeft, YardDirection.UpsideUpMovingRight,
    ];

    /// <summary>The number of directions.</summary>
    public static int Count => Numbered.Length;

    /// <summary>The direction numbered <paramref name="number"/>, 0 through 7.</summary>
    public static YardDirection At(int number) => Numbered[Ensure.BetweenInclusive(0, Numbered.Length - 1, number)];

    /// <summary>The cartridge's number for <paramref name="direction"/>.</summary>
    public static int NumberOf(YardDirection direction) =>
        Array.IndexOf(Numbered, direction) is var number and >= 0
            ? number
            : throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined Yard direction.");
}
