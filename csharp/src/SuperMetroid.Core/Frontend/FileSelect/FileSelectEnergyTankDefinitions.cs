namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>File-select energy tank cells and row traversal.</summary>
internal static class FileSelectEnergyTankDefinitions
{
    /// <summary>The display divides current and maximum energy by 100.</summary>
    internal const int EnergyPerTank = 100;
    /// <summary>Tanks start four tile cells from the ENERGY anchor.</summary>
    internal const int ColumnOffset = 4;
    /// <summary>Tanks start one tilemap row below the ENERGY anchor.</summary>
    internal const int RowOffset = 1;
    /// <summary>Seven tanks are drawn on the lower row first.</summary>
    internal const int FirstRowCount = 7;
    /// <summary>Eight cells are permitted after wrapping (a harmless off-by-one in the game).</summary>
    internal const int WrappedRowCount = 8;
    /// <summary>
    /// After a row the drawing steps up one row and back seven columns, so a wrapped row starts where the
    /// first row started, and a third row one column right of that.
    /// </summary>
    internal const int RowRewindColumns = 7;
}
