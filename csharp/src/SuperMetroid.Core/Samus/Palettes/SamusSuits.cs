using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Which suit Samus shows, and where a suit's entries lie in her per-suit colour lists.</summary>
public static class SamusSuits
{
    /// <summary>
    /// The suit whose colours Samus shows with <paramref name="equippedItems"/>. Gravity wins when both suits are
    /// equipped, matching the cartridge's branch order; this is an ordinary three-way choice, not another flag set.
    /// </summary>
    public static SamusSuit ShownSuit(this SamusEquipmentFlags equippedItems)
    {
        if (equippedItems.HasAny(SamusEquipmentFlags.GravitySuit))
            return SamusSuit.Gravity;
        return equippedItems.HasAny(SamusEquipmentFlags.VariaSuit) ? SamusSuit.Varia : SamusSuit.Power;
    }

    /// <summary>
    /// The place of <paramref name="suit"/>'s entries among the per-suit lists, in
    /// <see cref="SamusSuitDefinitions.ListOrder"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="suit"/> is not one of Samus's suits.</exception>
    public static int ListPlace(this SamusSuit suit)
    {
        int place = SamusSuitDefinitions.ListOrder.IndexOf(suit);
        return place >= 0 ? place : throw new ArgumentOutOfRangeException(nameof(suit), suit, "Samus has no such suit.");
    }

    /// <summary>The suit whose entries lie at <paramref name="place"/> among the per-suit lists.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="place"/> is past the last suit's entries.</exception>
    public static SamusSuit AtListPlace(int place) =>
        (uint)place < (uint)SamusSuitDefinitions.ListOrder.Length
            ? SamusSuitDefinitions.ListOrder[place]
            : throw new ArgumentOutOfRangeException(nameof(place), place, "No suit's entries lie there.");
}
