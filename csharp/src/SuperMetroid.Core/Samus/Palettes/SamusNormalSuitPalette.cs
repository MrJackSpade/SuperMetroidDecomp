using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Shared normal-suit colour restore; every owner shows the same selected suit colours.</summary>
public static class SamusNormalSuitPalette
{
    /// <summary>
    /// Shows the normal colours of the suit Samus's equipment shows (#1512), without changing equipment or animation
    /// state.
    /// </summary>
    /// <param name="colours">Samus's colours, whose suit shade changes.</param>
    /// <param name="equippedItems">Samus's equipment; only the Gravity and Varia suits choose the suit shown.</param>
    public static void Load(SamusColours colours, SamusEquipmentFlags equippedItems) =>
        Ensure.NotNull(colours).ShowSuit(SamusShades.Suit(equippedItems.ShownSuit()));

    /// <summary>Shows the Power Suit's normal colours regardless of the equipped suits, without changing gameplay state.</summary>
    /// <param name="colours">Samus's colours, whose suit shade changes.</param>
    public static void LoadPower(SamusColours colours) => Ensure.NotNull(colours).ShowSuit(SamusShades.Suit(SamusSuit.Power));
}
