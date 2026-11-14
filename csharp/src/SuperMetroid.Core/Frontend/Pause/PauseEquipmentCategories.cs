namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Reserve, beam, suit/misc, and boot category definitions.</summary>
internal static class PauseEquipmentCategories
{
    /// <summary>Plasma's place in the pause screen's beam list, at its bottom.</summary>
    public const int PlasmaItem = 4;
    /// <summary>Moving right from the beams without Up held starts the suit and miscellaneous scan at its third item.</summary>
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
