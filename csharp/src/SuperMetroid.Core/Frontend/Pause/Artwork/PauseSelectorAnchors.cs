using static SuperMetroid.Core.Frontend.Pause.Artwork.PauseSelectorDefinitions;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// Places the equipment screen's item selector (<see cref="PauseSelectorDefinitions"/>) over each category's items.
/// </summary>
public static class PauseSelectorAnchors
{
    /// <summary>The pause sheet's frame for the selector presentation's frame <paramref name="frame"/>.</summary>
    public static string SheetFrameName(string frame) => $"Selector.{frame}";
    /// <summary>Enumerates every editable selector anchor in native category/item order: two reserve controls, five beams, six suit/miscellaneous items, and three boots; does not alter navigation or eligibility.</summary>
    /// <returns>Sixteen tuples containing the compiled category, item index, and stable asset anchor name.</returns>
    public static IEnumerable<PauseSelectorAnchor> Anchors()
    {
        foreach (PauseEquipmentCategory category in Enum.GetValues<PauseEquipmentCategory>())
        {
            int count = category == PauseEquipmentCategory.Reserves ? ReserveControlCount : PauseEquipmentCategories.Get(category).ItemCount;
            for (int item = 0; item < count; item++) yield return new(category, item, Anchor(category, item));
        }
    }
    /// <summary>Names the selected native equipment control.</summary>
    public static string Anchor(PauseEquipmentCategory category, int item) =>
        Ensure.Listed(AnchorNames, new PauseEquipmentSlot(category, item));

    /// <summary>Projects the $82:C196..C1D5 selector lists from category columns and eight-pixel rows.</summary>
    /// <remarks>Suit/misc leaves two extra rows between Gravity and Morph Ball.
    /// Both native coordinates include the draw routine's one-pixel bias, removed here.</remarks>
    public static Position16<SpritePixels> StockAnchor(PauseEquipmentCategory category, int item)
    {
        _ = Anchor(category, item);
        Position16<ScreenPixels> origin = Ensure.Listed(CategoryOrigins, category);
        int gap = category == PauseEquipmentCategory.Suits && item >= SuitsLowerGroupFirstItem ? SuitsLowerGroupGap : 0;
        return new(checked((ushort)(origin.X - DrawBias)), checked((ushort)(origin.Y + gap + item * ItemRowStride - DrawBias)));
    }
    /// <summary>Selects the visual animation role for an equipment category, sharing the Equipment role between suit/miscellaneous items and boots.</summary>
    /// <param name="category">Native equipment category: 0 reserves, 1 beams, 2 suit/miscellaneous items, or 3 boots.</param>
    /// <returns><c>Reserve</c>, <c>Beam</c>, or <c>Equipment</c>.</returns>
    public static string Group(PauseEquipmentCategory category) => Ensure.Listed(Groups, category);
}
