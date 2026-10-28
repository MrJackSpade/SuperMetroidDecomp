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
            int count = category == PauseEquipmentCategory.Reserves ? 2 : PauseEquipmentCategories.Get(category).ItemCount;
            for (int item = 0; item < count; item++) yield return new(category, item, Anchor(category, item));
        }
    }
    /// <summary>Names the selected native equipment control rather than indexing a name table.</summary>
    public static string Anchor(PauseEquipmentCategory category, int item) => new PauseEquipmentSlot(category, item) switch
    {
        (PauseEquipmentCategory.Reserves, 0) => "Reserve.Mode",
        (PauseEquipmentCategory.Reserves, 1) => "Reserve.Transfer",
        (PauseEquipmentCategory.Beams, 0) => "Beam.Charge",
        (PauseEquipmentCategory.Beams, 1) => "Beam.Ice",
        (PauseEquipmentCategory.Beams, 2) => "Beam.Wave",
        (PauseEquipmentCategory.Beams, 3) => "Beam.Spazer",
        (PauseEquipmentCategory.Beams, 4) => "Beam.Plasma",
        (PauseEquipmentCategory.Suits, 0) => "Equipment.Varia",
        (PauseEquipmentCategory.Suits, 1) => "Equipment.Gravity",
        (PauseEquipmentCategory.Suits, 2) => "Equipment.MorphBall",
        (PauseEquipmentCategory.Suits, 3) => "Equipment.Bombs",
        (PauseEquipmentCategory.Suits, 4) => "Equipment.SpringBall",
        (PauseEquipmentCategory.Suits, 5) => "Equipment.ScrewAttack",
        (PauseEquipmentCategory.Boots, 0) => "Boots.HiJump",
        (PauseEquipmentCategory.Boots, 1) => "Boots.SpaceJump",
        (PauseEquipmentCategory.Boots, 2) => "Boots.SpeedBooster",
        _ => throw new ArgumentOutOfRangeException(nameof(item), "Invalid equipment selector category/item."),
    };

    /// <summary>Projects the $82:C196..C1D5 selector lists from category columns and eight-pixel rows.</summary>
    /// <remarks>Suit/misc leaves two extra rows between Gravity and Morph Ball.
    /// Both native coordinates include the draw routine's one-pixel bias, removed here.</remarks>
    public static Position16<SpritePixels> StockAnchor(PauseEquipmentCategory category, int item)
    {
        _ = Anchor(category, item);
        Position16<ScreenPixels> anchor = category switch
        {
            PauseEquipmentCategory.Reserves => new Position16<ScreenPixels>(27, 84),
            PauseEquipmentCategory.Beams => new Position16<ScreenPixels>(48, 132),
            PauseEquipmentCategory.Suits => new Position16<ScreenPixels>(204, checked((ushort)(76 + (item >= 2 ? 16 : 0)))),
            PauseEquipmentCategory.Boots => new Position16<ScreenPixels>(204, 156),
            _ => throw new ArgumentOutOfRangeException(nameof(category)),
        };
        return new(checked((ushort)(anchor.X - 1)), checked((ushort)(anchor.Y + item * 8 - 1)));
    }
    /// <summary>Selects the visual animation role for an equipment category, sharing the Equipment role between suit/miscellaneous items and boots.</summary>
    /// <param name="category">Native equipment category: 0 reserves, 1 beams, 2 suit/miscellaneous items, or 3 boots.</param>
    /// <returns><c>Reserve</c>, <c>Beam</c>, or <c>Equipment</c>.</returns>
    public static string Group(PauseEquipmentCategory category) => category switch
    { PauseEquipmentCategory.Reserves => "Reserve", PauseEquipmentCategory.Beams => "Beam",
        PauseEquipmentCategory.Suits or PauseEquipmentCategory.Boots => "Equipment",
        _ => throw new InvalidOperationException($"Undefined PauseEquipmentCategory {category}.") };
}
