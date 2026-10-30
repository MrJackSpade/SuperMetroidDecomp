namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Draw_FileSelection_Energy ($81:A0A4-A111) tank cells and row traversal.</summary>
internal static class FileSelectEnergyTankDefinitions
{
    /// <summary>$81:A0B0/A0CD divides current and maximum energy by 100.</summary>
    internal const int EnergyPerTank = 100;
    /// <summary>$81:A0A4 advances four tile cells from the ENERGY anchor.</summary>
    internal const int ColumnOffset = 4;
    /// <summary>$81:A0E4 starts one tilemap row below the ENERGY anchor.</summary>
    internal const int RowOffset = 1;
    /// <summary>$81:A0DD draws seven tanks on the lower row first.</summary>
    internal const int FirstRowCount = 7;
    /// <summary>$81:A10C permits eight cells after wrapping (native harmless off-by-one).</summary>
    internal const int WrappedRowCount = 8;
    /// <summary>
    /// $81:A108 steps back $4E bytes after a row: up one row and back seven columns, so a wrapped row starts where the
    /// first row started, and a third row one column right of that.
    /// </summary>
    internal const int RowRewindColumns = 7;
}
