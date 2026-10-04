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

    /// <summary>
    /// $82:C06C-$C087 destinations: beams occupy rows16-20, suits/misc rows9-10
    /// and13-16, boots rows19-21. All own nine cells, including the Plasma overrun
    /// into adjacent words used by the native simultaneous-input VAR behavior.
    /// </summary>
    public static bool IsEquipmentLabelCell(int cell)
    {
        if ((uint)cell >= Cells) return false;
        int row = cell / Columns;
        int column = cell % Columns;
        if (row is >= 16 and <= 20 && column is >= 4 and < 13) return true;
        return column is >= 21 and < 30 &&
            (row is 9 or 10 or >= 13 and <= 16 or >= 19 and <= 21);
    }

    /// <summary>$82:C068/$C06A reserve labels occupy seven columns on rows10/11; supply digits use their own anchor.</summary>
    private static bool IsReserveCell(int cell)
    {
        if ((uint)(cell - PauseReserveUiDefinitions.DigitCell) < PauseReserveUiDefinitions.SupplyDigitPlaces)
            return true;
        if ((uint)cell >= Cells) return false;
        return cell / Columns is 10 or 11 && cell % Columns is >= 4 and < 11;
    }
    public static bool IsLiveOwnedCell(int cell)
    {
        if (IsEquipmentLabelCell(cell)) return true;
        if (IsReserveCell(cell)) return true;
        int wireframeRelative = cell - PauseWireframeDefinitions.DestinationByte / sizeof(ushort);
        return wireframeRelative >= 0 && wireframeRelative / (PauseWireframeDefinitions.DestinationStride / sizeof(ushort)) < PauseWireframeDefinitions.Rows &&
            wireframeRelative % (PauseWireframeDefinitions.DestinationStride / sizeof(ushort)) < PauseWireframeDefinitions.Columns;
    }

    public static bool IsNonInventoryLiveOwnedCell(int cell) =>
        IsLiveOwnedCell(cell) && !IsEquipmentLabelCell(cell);

    public static bool IsArrowCell(int cell)
    {
        int vertical = cell - PauseReserveUiDefinitions.VerticalStartCell;
        if (vertical >= 0 && vertical % PauseReserveUiDefinitions.RowStrideCells == 0 &&
            vertical / PauseReserveUiDefinitions.RowStrideCells < PauseReserveUiDefinitions.VerticalCount) return true;
        return (uint)(cell - PauseReserveUiDefinitions.HorizontalStartCell) < PauseReserveUiDefinitions.HorizontalCount;
    }
}
