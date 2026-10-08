using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Stable visual bindings for native equipment selectors; no editable navigation table.</summary>
public static class PauseSelectorDefinitions
{
    /// <summary>Supported revision of the equipment-selector anchor, composition, animation, and palette JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Asset filename for the editable pause-equipment selector presentation.</summary>
    public const string FileName = "pause-selectors.json";
    /// <summary>Presentation limits: sixteen required equipment anchors, at most 255 named visual frames and 255 cyclic phases, and initial/phase dwell times from 1 through 254 accepted update ticks.</summary>
    public const int AnchorCount = 16, MaximumFrames = 255, MaximumPhases = 255, MaximumDuration = 254;
    /// <summary>$82:C18E equipment-selector positions: tanks, weapons, suit/misc, boots.</summary>
    public const int PositionPointers = PauseMenuRomData.EquipmentSelectorPositionPointerTable;
    /// <summary>$82:C0EC points to selector timing entries, (duration, unused, sprite offset).</summary>
    public const int AnimationPointer = PauseMenuRomData.ItemSelectorAnimationPointer;
    /// <summary>$82:AB98 initializes the selector timer from the first byte at $82:C10C.</summary>
    public const int InitialTimer = PauseMenuRomData.ItemSelectorAnimationTimer;
    /// <summary>$82:C0DA identifies the low-byte category variable at WRAM $0755.</summary>
    public const int VariantPointer = PauseMenuRomData.ItemSelectorAnimationVariantPointer;
    /// <summary>$7E:0755 PauseMenu_EquipmentScreenCategoryIndex; the animation reads only its low byte.</summary>
    public const ushort CategoryVariable = 0x0755;
    /// <summary>$82:C1E8 points to the four category sprite bases.</summary>
    public const int BasePointer = PauseMenuRomData.EquipmentSelectorBaseTablePointer;
    /// <summary>$82:C100 is the native caller palette word, selecting OBJ palette three.</summary>
    public const int PaletteSource = PauseMenuRomData.SelectedItemSpritemapPointer;
    /// <summary>Bank $82 owns selector positions, timing and sprite-pointer data.</summary>
    public const int Bank = 0x820000;
    /// <summary>Enumerates every editable selector anchor in native category/item order: two reserve controls, five beams, six suit/miscellaneous items, and three boots; does not alter navigation or eligibility.</summary>
    /// <returns>Sixteen tuples containing the compiled category, item index, and stable asset anchor name.</returns>
    public static IEnumerable<(int Category, int Item, string Name)> Anchors()
    {
        for (int category = 0; category < 4; category++)
        {
            int count = category == PauseEquipmentCategories.Reserves ? 2 : PauseEquipmentCategories.Get(category).ItemCount;
            for (int item = 0; item < count; item++) yield return (category, item, Anchor(category, item));
        }
    }
    /// <summary>Names the selected native equipment control rather than indexing a name table.</summary>
    public static string Anchor(int category, int item) => (category, item) switch
    {
        (PauseEquipmentCategories.Reserves, 0) => "Reserve.Mode",
        (PauseEquipmentCategories.Reserves, 1) => "Reserve.Transfer",
        (PauseEquipmentCategories.Beams, 0) => "Beam.Charge",
        (PauseEquipmentCategories.Beams, 1) => "Beam.Ice",
        (PauseEquipmentCategories.Beams, 2) => "Beam.Wave",
        (PauseEquipmentCategories.Beams, 3) => "Beam.Spazer",
        (PauseEquipmentCategories.Beams, 4) => "Beam.Plasma",
        (PauseEquipmentCategories.Suits, 0) => "Equipment.Varia",
        (PauseEquipmentCategories.Suits, 1) => "Equipment.Gravity",
        (PauseEquipmentCategories.Suits, 2) => "Equipment.MorphBall",
        (PauseEquipmentCategories.Suits, 3) => "Equipment.Bombs",
        (PauseEquipmentCategories.Suits, 4) => "Equipment.SpringBall",
        (PauseEquipmentCategories.Suits, 5) => "Equipment.ScrewAttack",
        (PauseEquipmentCategories.Boots, 0) => "Boots.HiJump",
        (PauseEquipmentCategories.Boots, 1) => "Boots.SpaceJump",
        (PauseEquipmentCategories.Boots, 2) => "Boots.SpeedBooster",
        _ => throw new ArgumentOutOfRangeException(nameof(item), "Invalid equipment selector category/item."),
    };

    /// <summary>Projects the $82:C196..C1D5 selector lists from category columns and eight-pixel rows.</summary>
    /// <remarks>Suit/misc leaves two extra rows between Gravity and Morph Ball.
    /// Both native coordinates include the draw routine's one-pixel bias, removed here.</remarks>
    public static MapLabelPoint StockAnchor(int category, int item)
    {
        _ = Anchor(category, item);
        (int x, int y) = category switch
        {
            PauseEquipmentCategories.Reserves => (0x1b, 0x54),
            PauseEquipmentCategories.Beams => (0x30, 0x84),
            PauseEquipmentCategories.Suits => (0xcc, 0x4c + (item >= 2 ? 16 : 0)),
            PauseEquipmentCategories.Boots => (0xcc, 0x9c),
            _ => throw new ArgumentOutOfRangeException(nameof(category)),
        };
        return new(x - 1, y + item * 8 - 1);
    }
    /// <summary>$82:C202 bases: Reserve=$14, Beam=$15, Suit and Boots=$16. Retained as diagnostic identities.</summary>
    public static ushort NativeSpriteId(int category) => category switch
    { 0 => 0x14, 1 => 0x15, 2 or 3 => 0x16, _ => throw new ArgumentOutOfRangeException(nameof(category)) };
    /// <summary>Selects the visual animation role for an equipment category, sharing the Equipment role between suit/miscellaneous items and boots.</summary>
    /// <param name="category">Native equipment category: 0 reserves, 1 beams, 2 suit/miscellaneous items, or 3 boots.</param>
    /// <returns><c>Reserve</c>, <c>Beam</c>, or <c>Equipment</c>.</returns>
    public static string Group(int category) => category switch
    { 0 => "Reserve", 1 => "Beam", 2 or 3 => "Equipment", _ => throw new ArgumentOutOfRangeException(nameof(category)) };
}
