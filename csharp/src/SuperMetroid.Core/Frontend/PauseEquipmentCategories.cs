namespace SuperMetroid.Core.Frontend;

/// <summary>Native reserve, beam, suit/misc, and boot category definitions.</summary>
internal static class PauseEquipmentCategories
{
    /// <summary>$82:C04C Weapons mask table index of Spazer.</summary>
    public const int SpazerItem = 3;
    /// <summary>$82:C04C Weapons mask table index of Plasma; bottom of beam list.</summary>
    public const int PlasmaItem = 4;
    /// <summary>$82:AFDB ordinary Right entry starts at suit/misc table byte offset four.</summary>
    public const int BeamRightSuitItem = 2;
    /// <summary>$82:B568 target offsets are relative to EquipmentScreenBG1Tilemap at $7E:3800.</summary>
    public const int TilemapWramBase = 0x3800;
    /// <summary>The three inventory categories in native scan order; reserves use separate controls.</summary>
    public static IReadOnlyList<PauseEquipmentCategory> InventoryCategories { get; } =
        [PauseEquipmentCategory.Beams, PauseEquipmentCategory.Suits, PauseEquipmentCategory.Boots];

    /// <summary>Selects the native data contract for a named equipment category.</summary>
    /// <remarks>The category dispatcher at $82:AC58 selects distinct controls, not
    /// a numeric sequence. Pointer fields correspond to $82:C02C/C034/C044;
    /// item counts follow $82:ABCC/ABEB/AC05 and copy lengths $82:AFCE/B0C8/B156.
    /// Reserves use separate controls and retain the managed zero-data contract.
    /// </remarks>
    public static PauseEquipmentCategoryDefinition Get(PauseEquipmentCategory category) => category switch
    {
        PauseEquipmentCategory.Reserves => new(PauseEquipmentCategory.Reserves, 0, 0, 0, 0),
        PauseEquipmentCategory.Beams => new(PauseEquipmentCategory.Beams, PauseMenuRomData.BeamOffsetTable, PauseMenuRomData.BeamTilemapPointerTable, 5, 5),
        PauseEquipmentCategory.Suits => new(PauseEquipmentCategory.Suits, PauseMenuRomData.SuitOffsetTable, PauseMenuRomData.SuitTilemapPointerTable, 6, 9),
        PauseEquipmentCategory.Boots => new(PauseEquipmentCategory.Boots, PauseMenuRomData.BootOffsetTable, PauseMenuRomData.BootTilemapPointerTable, 3, 9),
        _ => throw new InvalidOperationException($"Undefined PauseEquipmentCategory {category}."),
    };
}
