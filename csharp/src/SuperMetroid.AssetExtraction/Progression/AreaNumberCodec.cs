using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// The cartridge's numbering of the areas, which its room headers, save files and per-area tables use: the six Zebes
/// areas, then Ceres. Import and export decode and encode it here; the game names areas and never sees the number.
/// </summary>
public static class AreaNumberCodec
{
    /// <summary>Each area at the position of its number.</summary>
    private static readonly AreaId[] Numbered =
    [
        AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia, AreaId.Tourian, AreaId.Ceres,
    ];

    /// <summary>The number of areas the cartridge numbers.</summary>
    public static int Count => Numbered.Length;

    /// <summary>The area numbered <paramref name="number"/> in cartridge data.</summary>
    /// <param name="number">The area number read from cartridge or save data.</param>
    /// <param name="source">What held the number, reported when it numbers no area.</param>
    /// <exception cref="InvalidDataException"><paramref name="number"/> numbers no area.</exception>
    public static AreaId Decode(int number, string source) =>
        (uint)number < (uint)Numbered.Length
            ? Numbered[number]
            : throw new InvalidDataException($"{source} contains area number {number}, outside the {Numbered.Length} areas.");

    /// <summary>The area at <paramref name="number"/> of a per-area table the import or export walks.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> numbers no area.</exception>
    public static AreaId At(int number) =>
        (uint)number < (uint)Numbered.Length
            ? Numbered[number]
            : throw new ArgumentOutOfRangeException(nameof(number), number, "Numbers no area.");

    /// <summary>The number cartridge data gives <paramref name="area"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is not an area.</exception>
    public static int Encode(AreaId area)
    {
        int number = Array.IndexOf(Numbered, area);
        return number >= 0 ? number : throw new ArgumentOutOfRangeException(nameof(area), area, "Not an area.");
    }
}
