using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Equipment-page base artwork and the compiled footprints owned by live menu state.</summary>
public static class PauseEquipmentBaseDefinitions
{
    public const int Version = 1;
    public const string FileName = "pause-equipment-base.json";
    public const int Columns = 32, Rows = 32, Cells = Columns * Rows;
    /// <summary>$B6:E800, native 32x32 equipment-page template.</summary>
    public const int Source = PauseMenuRomData.EquipmentTilemap;

    // These are visual ownership footprints resolved from the native destination tables,
    // not editable navigation or inventory rules. Rebinding base art must not erase them.
    private static readonly (int Cell, int Count)[] labelRegions =
    [
        // $82:C06C beam-label destinations. Native beam labels are five words, but the
        // Boots-to-Plasma simultaneous-input path copies nine and exposes four adjacent
        // words, so all nine are live state during content rebinding.
        (0x204, 9), (0x224, 9), (0x244, 9), (0x264, 9), (0x284, 9),
        // $82:C076 suit/misc destinations, nine words each.
        (0x135, 9), (0x155, 9), (0x1b5, 9), (0x1d5, 9), (0x1f5, 9), (0x215, 9),
        // $82:C082 boots destinations, nine words each.
        (0x275, 9), (0x295, 9), (0x2b5, 9),
        // $82:C068 reserve label destinations, then $82:8FCE's three digits.
        (0x144, 7), (0x164, 7), (PauseReserveUiDefinitions.DigitCell, PauseReserveUiDefinitions.SupplyDigitPlaces),
    ];

    public static bool IsLiveOwnedCell(int cell)
    {
        foreach (var region in labelRegions)
            if ((uint)(cell - region.Cell) < region.Count) return true;
        int wireframeRelative = cell - PauseWireframeDefinitions.DestinationByte / sizeof(ushort);
        return wireframeRelative >= 0 && wireframeRelative / (PauseWireframeDefinitions.DestinationStride / sizeof(ushort)) < PauseWireframeDefinitions.Rows &&
            wireframeRelative % (PauseWireframeDefinitions.DestinationStride / sizeof(ushort)) < PauseWireframeDefinitions.Columns;
    }

    public static bool IsArrowCell(int cell)
    {
        int vertical = cell - PauseReserveUiDefinitions.VerticalStartCell;
        if (vertical >= 0 && vertical % PauseReserveUiDefinitions.RowStrideCells == 0 &&
            vertical / PauseReserveUiDefinitions.RowStrideCells < PauseReserveUiDefinitions.VerticalCount) return true;
        return (uint)(cell - PauseReserveUiDefinitions.HorizontalStartCell) < PauseReserveUiDefinitions.HorizontalCount;
    }
}
