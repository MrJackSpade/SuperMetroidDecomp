namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Reserve, beam, suit/misc, and boot category definitions.</summary>
internal static class PauseEquipmentCategories
{
    /// <summary>Weapons mask table index of Plasma; bottom of beam list.</summary>
    public const int PlasmaItem = 4;
    /// <summary>The ordinary Right entry starts at suit/misc table byte offset four.</summary>
    public const int BeamRightSuitItem = 2;
    /// <summary>The three inventory categories in scan order; reserves use separate controls.</summary>
    public static IReadOnlyList<PauseEquipmentCategory> InventoryCategories { get; } =
        [PauseEquipmentCategory.Beams, PauseEquipmentCategory.Suits, PauseEquipmentCategory.Boots];

    /// <summary>Selects the data contract for a named equipment category.</summary>
    /// <remarks>Each category is a distinct control, not a numeric sequence. Beams list five items, suits and
    /// miscellaneous items six, and boots three. Reserves use separate controls and list nothing.
    /// </remarks>
    public static PauseEquipmentCategoryDefinition Get(PauseEquipmentCategory category) => category switch
    {
        PauseEquipmentCategory.Reserves => new(PauseEquipmentCategory.Reserves, 0),
        PauseEquipmentCategory.Beams => new(PauseEquipmentCategory.Beams, 5),
        PauseEquipmentCategory.Suits => new(PauseEquipmentCategory.Suits, 6),
        PauseEquipmentCategory.Boots => new(PauseEquipmentCategory.Boots, 3),
        _ => throw new InvalidOperationException($"Undefined PauseEquipmentCategory {category}."),
    };
}
