using static SuperMetroid.Core.Frontend.Pause.Artwork.PauseSelectorDefinitions;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// Places the equipment screen's item selector (<see cref="PauseSelectorDefinitions"/>) over each category's items.
/// </summary>
public static class PauseSelectorAnchors
{
    /// <summary>The pause sheet's frame for the selector presentation's frame <paramref name="frame"/>.</summary>
    public static string SheetFrameName(string frame) => $"Selector.{frame}";
    /// <summary>Enumerates every editable selector anchor in category/item order: two reserve controls, five beams, six suit/miscellaneous items, and three boots; does not alter navigation or eligibility.</summary>
    /// <returns>Sixteen anchors containing the compiled category and item index; <see cref="Anchor"/> names each.</returns>
    internal static IEnumerable<PauseEquipmentSlot> Anchors()
    {
        foreach (PauseEquipmentCategory category in Enum.GetValues<PauseEquipmentCategory>())
        {
            int count = category == PauseEquipmentCategory.Reserves ? ReserveControlCount : PauseEquipmentCategories.Get(category).ItemCount;
            for (int item = 0; item < count; item++) yield return new(category, item);
        }
    }
    /// <summary>Names the selected equipment control.</summary>
    public static string Anchor(PauseEquipmentCategory category, int item) =>
        Ensure.Listed(AnchorNames, new PauseEquipmentSlot(category, item));

    /// <summary>Projects the selector lists from category columns and eight-pixel rows.</summary>
    /// <remarks>Suit/misc leaves two extra rows between Gravity and Morph Ball.
    /// Both stock coordinates include the draw routine's one-pixel bias, removed here.</remarks>
    public static Position16<SpritePixels> StockAnchor(PauseEquipmentCategory category, int item)
    {
        _ = Anchor(category, item);
        Position16<ScreenPixels> origin = Ensure.Listed(CategoryOrigins, category);
        int gap = category == PauseEquipmentCategory.Suits && item >= SuitsLowerGroupFirstItem ? SuitsLowerGroupGap : 0;
        return new(origin.X - DrawBias, origin.Y + gap + item * ItemRowStride - DrawBias);
    }
}
