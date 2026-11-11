using static SuperMetroid.AssetExtraction.Frontend.Pause.Artwork.PauseEquipmentBaseDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// Which cells of the equipment screen's base page (<see cref="PauseEquipmentBaseDefinitions"/>) the live labels, reserves
/// and wireframe own; the page's pictures are installed whole (#1615).
/// </summary>
public static class PauseEquipmentBaseTiles
{
    /// <summary>
    /// $82:C06C-$C087 destinations: beams occupy rows16-20, suits/misc rows9-10
    /// and13-16, boots rows19-21. All own nine cells on the cartridge's page, where a nine-cell write dispatched from
    /// the suit or boot column reaches along a beam row; the game shows each label's picture whole (#1515).
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
    /// <summary>Returns whether runtime inventory, reserve, or wireframe state owns a tilemap cell.</summary>
    /// <param name="cell">Zero-based cell index in the 32-by-32 equipment tilemap.</param>
    /// <returns><see langword="true"/> when the static base must leave the cell for live menu composition.</returns>
    public static bool IsLiveOwnedCell(int cell)
    {
        if (IsEquipmentLabelCell(cell)) return true;
        if (IsReserveCell(cell)) return true;
        int wireframeRelative = cell - PauseWireframeDefinitions.FirstPageTile;
        return wireframeRelative >= 0 && wireframeRelative / PauseWireframeDefinitions.PageColumns < PauseWireframeDefinitions.Rows &&
            wireframeRelative % PauseWireframeDefinitions.PageColumns < PauseWireframeDefinitions.Columns;
    }

    /// <summary>Returns whether reserve or wireframe composition owns a cell outside the equipment-label footprints.</summary>
    /// <param name="cell">Zero-based cell index in the equipment tilemap.</param>
    /// <returns><see langword="true"/> for a live-owned cell that is not an equipment label.</returns>
    public static bool IsNonInventoryLiveOwnedCell(int cell) =>
        IsLiveOwnedCell(cell) && !IsEquipmentLabelCell(cell);
}
