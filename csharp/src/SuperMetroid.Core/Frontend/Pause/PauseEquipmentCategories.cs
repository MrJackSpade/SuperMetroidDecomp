namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Native reserve, beam, suit/misc, and boot category definitions.</summary>
internal static class PauseEquipmentCategories
{
    /// <summary>$82:C04C Weapons mask table index of Spazer.</summary>
    public const int SpazerItem = 3;
    /// <summary>$82:C04C Weapons mask table index of Plasma; bottom of beam list.</summary>
    public const int PlasmaItem = 4;
    /// <summary>$82:AFDB ordinary Right entry starts at suit/misc table byte offset four.</summary>
    public const int BeamRightSuitItem = 2;
    /// <summary>The three inventory categories in native scan order; reserves use separate controls.</summary>
    public static IReadOnlyList<PauseEquipmentCategory> InventoryCategories { get; } =
        [PauseEquipmentCategory.Beams, PauseEquipmentCategory.Suits, PauseEquipmentCategory.Boots];

    /// <summary>Selects the native data contract for a named equipment category.</summary>
    /// <remarks>Each category is a distinct control, not a numeric sequence. Beams list five items with
    /// five-cell labels; suits and miscellaneous items six, and boots three, with nine-cell labels.
    /// Reserves use separate controls and list nothing.
    /// </remarks>
    public static PauseEquipmentCategoryDefinition Get(PauseEquipmentCategory category) => category switch
    {
        PauseEquipmentCategory.Reserves => new(PauseEquipmentCategory.Reserves, 0, 0),
        PauseEquipmentCategory.Beams => new(PauseEquipmentCategory.Beams, 5, 5),
        PauseEquipmentCategory.Suits => new(PauseEquipmentCategory.Suits, 6, 9),
        PauseEquipmentCategory.Boots => new(PauseEquipmentCategory.Boots, 3, 9),
        _ => throw new InvalidOperationException($"Undefined PauseEquipmentCategory {category}."),
    };
}
