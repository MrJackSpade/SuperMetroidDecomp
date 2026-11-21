using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Cartridge values and helpers of <see cref="SamusSuitListLayout"/> that only verification reads.</summary>
internal static class SamusSuitListLayoutConstants
{
    /// <summary>The place of <paramref name="suit"/>'s entries among the per-suit lists, in <see cref="SamusSuitListLayout.Order"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="suit"/> is not one of Samus's suits.</exception>
    public static int Place(SamusSuit suit)
    {
        int place = SamusSuitListLayout.Order.IndexOf(suit);
        return place >= 0 ? place : throw new ArgumentOutOfRangeException(nameof(suit), suit, "Samus has no such suit.");
    }

    /// <summary>The suit whose entries are at <paramref name="place"/> among the per-suit lists, in <see cref="SamusSuitListLayout.Order"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No suit's entries are at that place.</exception>
    public static SamusSuit AtPlace(int place) => (uint)place < (uint)SamusSuitListLayout.Order.Length
        ? SamusSuitListLayout.Order[place]
        : throw new ArgumentOutOfRangeException(nameof(place), place, "No suit's entries are at that place.");
}
